using HermesDMobAPI.Infrastructure.Extensions;
using HermesDMobAPI.Models.DTO.OAuth;
using HermesDMobAPI.Services.Account;
using HermesDMobAPI.Services.OAuth;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using HermesDMobAPI.Infrastructure;
using HermesDMobAPI.Models.DTO;
using Microsoft.AspNet.Identity;
using Serilog;

namespace HermesDMobAPI.Controllers.OAuth
{
    public class AuthController : ApiControllerExtension
    {
        private ILogger _log;
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
            _log = logger;
            _mapper = mapper;
            _userService = userService;
            _refreshTokenService = refreshTokenService;
            _jwTokenService = jwTokenService;
            _authService = authService;
        }

        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
        public async Task<object> Login([FromBody]AuthLoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(AppMessage.InvalidModel);
            }

            var user = await _userService.GetUserByNameAsync(model.UserName);

            if (user == null)
            {
                return NotFound(AppMessage.InvalidLoginOrPassword);
            }

            var hasher = new PasswordHasher();

            if (hasher.VerifyHashedPassword(user.PasswordHash, model.Password) != PasswordVerificationResult.Success)
            {
                return NotFound(AppMessage.InvalidLoginOrPassword);
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

            _log.Information($"User {model.UserName} logged in.");

            return new
            {
                JWT = newJWToken,
                RefreshToken = newRefreshToken
            };
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
        public async Task<object> RefreshToken([FromBody]string jwToken, string refreshToken)
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

            return new { JWT = newJWToken, RefreshToken = newRefreshToken };


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