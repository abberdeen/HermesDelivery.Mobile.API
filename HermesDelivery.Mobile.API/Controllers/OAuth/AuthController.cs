using HermesDMobAPI.Infrastructure.Extensions;
using HermesDMobAPI.Models.DTO.OAuth;
using HermesDMobAPI.Services.Account;
using HermesDMobAPI.Services.OAuth;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace HermesDMobAPI.Controllers.OAuth
{
    public class AuthController : ApiController
    {
        private readonly UserService _userService;
        private readonly RefreshTokenService _refreshTokenService;
        private readonly JwTokenService _jwTokenService;
        private readonly AuthService _authService;

        public AuthController()
        {
            _userService = new UserService();
            _refreshTokenService = new RefreshTokenService();
            _jwTokenService = new JwTokenService();
            _authService = new AuthService();
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("Login")]
        [System.Web.Http.AllowAnonymous]
        public async Task<Object> Login([FromBody]AuthLoginDto model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userService.GetUserByNameAsync(model.UserName);

                if (user == null)
                {
                    return NotFound();
                }
                
                var hasher = new Microsoft.AspNet.Identity.PasswordHasher();

                if (hasher.VerifyHashedPassword(user.PasswordHash, model.Password) != PasswordVerificationResult.Success)
                {
                    return BadRequest("Incorrect login or password");
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

                var memCacher = new MemoryCacher();
                if (jwToken != null)
                {
                    if (memCacher.GetValue(jwToken) != null)
                    {
                        memCacher.Delete(jwToken);
                    }
                }
                memCacher.Add(newJWToken, user.Id, DateTimeOffset.UtcNow.AddHours(12));

                await _jwTokenService.SetAsync(user.Id, newJWToken);

                return new { JWT = newJWToken, RefreshToken = newRefreshToken };
            }
            return NotFound();
        }

        //[System.Web.Http.HttpGet]
        //[System.Web.Http.Route("LogOut")]
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

        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("RefreshToken")]
        public async Task<Object> RefreshToken([FromBody]string jwToken, string refreshToken)
        {
            var userPrincipal = _authService.GetPrincipalFromToken(jwToken);

            var tokenActive = await _authService.IsTokenExistsAsync(jwToken);

            // invalid token/signing key was passed and we can't extract user claims
            if (userPrincipal != null && tokenActive)
            {
                var userId = userPrincipal.Claims.First(c => c.Type == "id").Value;

                var user = await _refreshTokenService.GetByUserByIdAsync(userId);

                if (user != null && user.RefreshTokenIsActive == true && user.RefreshTokenIp == GetRemoteIp() && user.RefreshToken == refreshToken)
                {
                    string _userId = userPrincipal.Claims.FirstOrDefault(c => c.Type == "id")?.Value.ToString();

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

                    var memCacher = new MemoryCacher();
                    if (memCacher.GetValue(jwToken) == null)
                    {
                        memCacher.Add(newJWToken, user.Id, DateTimeOffset.UtcNow.AddHours(12));
                    }

                    return new { JWT = newJWToken, RefreshToken = newRefreshToken };
                }
                return new HttpNotFoundResult();
            }
            return new HttpNotFoundResult();
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