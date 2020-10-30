using AutoMapper;
using CourierAPI.Infrastructure;
using CourierAPI.Infrastructure.Extensions;
using CourierAPI.Models.DTO.OAuth;
using CourierAPI.Services.Account;
using CourierAPI.Services.OAuth;
using Microsoft.AspNet.Identity;
using Serilog;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace CourierAPI.Controllers.OAuth
{
    public class AuthController : ApiControllerExtension
    {
        private readonly ILogger _logger;
        private IMapper _mapper;
        private readonly UserService _userService;
        private readonly RefreshTokenService _refreshTokenService;
        private readonly JwTokenService _jwTokenService;
        private readonly AuthService _authService;

        public AuthController(
            ILogger logger,
            IMapper mapper,
            UserService userService,
            RefreshTokenService refreshTokenService,
            JwTokenService jwTokenService,
            AuthService authService)
        {
            _logger = logger;
            _mapper = mapper;
            _userService = userService;
            _refreshTokenService = refreshTokenService;
            _jwTokenService = jwTokenService;
            _authService = authService;
        }

        [HttpPost]
        [Route("Auth")]
        [AllowAnonymous]
        [ResponseType(typeof(LoginResponseDto))]
        public async Task<IHttpActionResult> Login([FromBody]LoginRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                return Response(AppMessage.InvalidModel);
            }

            var user = await _userService.GetUserByNameAsync(model.Username);

            if (user == null)
            {
                return Response(AppMessage.InvalidLoginOrPassword);
            }

            var hasher = new PasswordHasher();

            if (hasher.VerifyHashedPassword(user.PasswordHash, model.Password) != PasswordVerificationResult.Success)
            {
                return Response(AppMessage.InvalidLoginOrPassword);
            }

            var newRefreshToken = GenerateTokenByRandomNumber();

            var refreshTokenDto = new RefreshTokenDto
            {
                IsActive = true,
                Token = newRefreshToken,
                Expires = DateTime.Now.AddDays(1),
                RemoteIp = GetRemoteIp()
            };

            await _refreshTokenService.SetAsync(refreshTokenDto, user.Id);

            var jwToken = await _jwTokenService.GetTokenAsync(user.Id);

            var newJWToken = await _authService.GenerateJWTokenAsync(user.Id);

            var memCacher = new CustomMemoryCacher();
            if (jwToken != null)
            {
                if (memCacher.GetValue(jwToken) != null)
                {
                    memCacher.Delete(jwToken);
                }
            }
            memCacher.Add(newJWToken, user.Id, DateTimeOffset.UtcNow.AddHours(12));

            await _jwTokenService.SetAsync(user.Id, newJWToken);

            _logger.Information($"User {model.Username} logged in.");

            var response = new LoginResponseDto()
            {
                AccessToken = newJWToken,
                RefreshToken = newRefreshToken
            };

            return Ok(response);
        }

        //[HttpGet]
        //[Route("LogOut")]
        //public async Task<string> LogOut()
        //{
        //    string token = "";
        //    if (HttpContext.Current.Request.Headers.Get("Authorization") != null)
        //    {
        //        token = Convert.ToString(HttpContext.Current.Request.Headers.Get("Authorization"));
        //    }

        //    var activeUserToken = await _jwTokenService.GetByTokenAsync(token);

        //    var refreshToken = await _refreshTokenService.GetByUserIdAsync(activeUserToken.UserId);
        //    if (refreshToken != null)
        //        await _refreshTokenService.DeleteAsync(refreshToken);

        //    await _jwTokenService.DeleteAsync(activeUserToken);

        //    var memCacher = new MemoryCacher();
        //    if (memCacher.GetValue(token) != null)
        //    {
        //        memCacher.Delete(token);
        //    }

        //    return "Logged out";
        //}

        [AllowAnonymous]
        [HttpPost]
        [Route("RefreshToken")]
        [ResponseType(typeof(LoginResponseDto))]
        public async Task<IHttpActionResult> RefreshToken([FromBody]string jwToken, string refreshToken)
        {
            var userPrincipal = _authService.GetPrincipalFromToken(jwToken);

            var tokenActive = await _authService.IsTokenExistsAsync(jwToken);

            // invalid token/signing key was passed and we can't extract user claims
            if (userPrincipal == null || !tokenActive)
            {
                return NotFound();
            }

            var userId = userPrincipal.Claims.First(c => c.Type == "id").Value;

            var user = await _refreshTokenService.GetByUserByIdAsync(userId);

            if (user == null ||
                user.RefreshTokenIsActive != true ||
                user.RefreshTokenIp != GetRemoteIp() ||
                user.RefreshToken != refreshToken)
            {
                return NotFound();
            }

            var _userId = userPrincipal.Claims.FirstOrDefault(c => c.Type == "id")?.Value.ToString();

            await _refreshTokenService.ClearAsync(userId);

            // RefreshToken
            var newRefreshToken = GenerateTokenByRandomNumber();

            var refreshTokenDto = new RefreshTokenDto
            {
                IsActive = true,
                Token = newRefreshToken,
                Expires = DateTime.Now.AddDays(5),
                RemoteIp = GetRemoteIp()
            };

            await _refreshTokenService.SetAsync(refreshTokenDto, userId);

            // JWToken
            var newJWToken = await _authService.GenerateJWTokenAsync(userId);

            await _jwTokenService.SetAsync(userId, newJWToken);

            var memCacher = new CustomMemoryCacher();
            if (memCacher.GetValue(jwToken) == null)
            {
                memCacher.Add(newJWToken, user.Id, DateTimeOffset.UtcNow.AddHours(12));
            }

            var response = new LoginResponseDto()
            {
                AccessToken = newJWToken,
                RefreshToken = newRefreshToken
            };

            return Ok(response);
        }

        #region "Helpers"

        private static string GetRemoteIp()
        {
            string ip = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(ip))
            {
                ip = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
            return ip;
        }

        private static string GenerateTokenByRandomNumber(int size = 32)
        {
            var randomNumber = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        #endregion "Helpers"
    }
}