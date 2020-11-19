using AutoMapper;
using CourierAPI.Infrastructure;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Infrastructure.Extensions;
using CourierAPI.Services.OAuth;
using Microsoft.AspNet.Identity;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using CourierAPI.DTO;
using CourierAPI.DTO.OAuth;
using WebGrease.Css.Extensions;

namespace CourierAPI.Controllers.OAuth
{
    /// <summary>
    /// Авторизация.
    /// </summary>
    public class AuthController : ApiControllerExtension
    {
        private readonly ILogger _logger;
        private IMapper _mapper;
        private readonly RefreshTokenService _refreshTokenService;
        private readonly JwTokenService _jwTokenService;
        private readonly AuthService _authService;

        public AuthController(
            ILogger logger,
            IMapper mapper,
            RefreshTokenService refreshTokenService,
            JwTokenService jwTokenService,
            AuthService authService)
        {
            _logger = logger;
            _mapper = mapper;
            _refreshTokenService = refreshTokenService;
            _jwTokenService = jwTokenService;
            _authService = authService;
        }
          
        /// <summary>
        /// Вход в систему. Получить OAuth токен.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        //[Route("Auth/Login")]
        [Route("Auth")]
        [AllowAnonymous]
        [ResponseType(typeof(LoginResponseDto))]
        [HttpPost]
        public async Task<IHttpActionResult> Login([FromBody]LoginRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                return Response(AppMessage.InvalidModel);
            }

            var courier = await _authService.GetCourierByPhoneAsync(model.Username);

            //
            if (courier == null)
            {
                return Response(AppMessage.InvalidLoginOrPassword);
            }

            
            //
            if (string.IsNullOrEmpty(courier.PasswordHash) || string.IsNullOrEmpty(model.Password.Trim()))
            {
                throw new Exception("Courier password is empty");
            }

            //
            var hasher = new PasswordHasher();
            
            if (hasher.VerifyHashedPassword(courier.PasswordHash, model.Password.Trim()) != PasswordVerificationResult.Success)
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

            await _refreshTokenService.SetAsync(refreshTokenDto, courier.Id);

            var jwToken = await _jwTokenService.GetTokenAsync(courier.Id);

            var newJWToken = await _authService.GenerateJWTokenAsync(courier.Id);

            var memCacher = new CustomMemoryCacher();
            if (jwToken != null)
            {
                if (memCacher.GetValue(jwToken) != null)
                {
                    memCacher.Delete(jwToken);
                }
            }
            memCacher.Add(newJWToken, courier.Id, DateTimeOffset.UtcNow.AddHours(12));

            await _jwTokenService.SetAsync(courier.Id, newJWToken);

            _logger.Information($"Courier {model.Username} logged in.");

            var response = new LoginResponseDto()
            {
                AccessToken = newJWToken,
                RefreshToken = newRefreshToken
            };

            return Ok(response);
        }

        /// <summary>
        /// Получить RefreshToken.
        /// </summary>
        /// <param name="jwToken"></param>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("Auth/RefreshToken")]
        [ResponseType(typeof(LoginResponseDto))]
        public async Task<IHttpActionResult> RefreshToken([FromBody]string jwToken, string refreshToken)
        {
            var courierPrincipal = _authService.GetPrincipalFromToken(jwToken);

            var tokenActive = await _authService.IsTokenExistsAsync(jwToken);

            // invalid token/signing key was passed and we can't extract courier claims
            if (courierPrincipal == null || !tokenActive)
            {
                return NotFound();
            }

            var id = courierPrincipal.Claims.First(c => c.Type == "id").Value;

            var courierId = Convert.ToInt32(id);

            var courier = await _refreshTokenService.GetByCourierAuthDataByIdAsync(courierId);

            if (courier == null ||
                courier.RefreshTokenIsActive != true ||
                courier.RefreshTokenIp != GetRemoteIp() ||
                courier.RefreshToken != refreshToken)
            {
                return NotFound();
            }

            await _refreshTokenService.ClearAsync(courierId);

            // RefreshToken
            var newRefreshToken = GenerateTokenByRandomNumber();

            var refreshTokenDto = new RefreshTokenDto
            {
                IsActive = true,
                Token = newRefreshToken,
                Expires = DateTime.Now.AddDays(5),
                RemoteIp = GetRemoteIp()
            };

            await _refreshTokenService.SetAsync(refreshTokenDto, courierId);

            // JWToken
            var newJWToken = await _authService.GenerateJWTokenAsync(courierId);

            await _jwTokenService.SetAsync(courierId, newJWToken);

            var memCacher = new CustomMemoryCacher();
            if (memCacher.GetValue(jwToken) == null)
            {
                memCacher.Add(newJWToken, courier.Id, DateTimeOffset.UtcNow.AddHours(12));
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