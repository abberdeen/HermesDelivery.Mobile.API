using ERP.API.Identity;
using HermesDelivery.Mobile.API.Models.DTO.OAuth;
using HermesDelivery.Mobile.API.Services.OAuth;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;

namespace ERP.API.Controllers
{
    public class AuthController : ApiController
    {
        private readonly AccountService _userService;
        private readonly RefreshTokenService _refreshTokenService;
        private readonly JWTokenService _jwTokenService;

        public AuthController()
        {
            _userService = new AccountService();
            _refreshTokenService = new RefreshTokenService();
            _jwTokenService = new JWTokenService();
        }

        public static bool VerifyHashedPassword(string hashedPassword, string password)
        {
            byte[] buffer4;
            if (hashedPassword == null)
            {
                return false;
            }
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            byte[] src = Convert.FromBase64String(hashedPassword);
            if ((src.Length != 0x31) || (src[0] != 0))
            {
                return false;
            }
            byte[] dst = new byte[0x10];
            Buffer.BlockCopy(src, 1, dst, 0, 0x10);
            byte[] buffer3 = new byte[0x20];
            Buffer.BlockCopy(src, 0x11, buffer3, 0, 0x20);
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, dst, 0x3e8))
            {
                buffer4 = bytes.GetBytes(0x20);
            }
            return ByteArraysEqual(buffer3, buffer4);
        }

        public static bool ByteArraysEqual(byte[] b1, byte[] b2)
        {
            if (b1 == b2) return true;
            if (b1 == null || b2 == null) return false;
            if (b1.Length != b2.Length) return false;
            for (int i = 0; i < b1.Length; i++)
            {
                if (b1[i] != b2[i]) return false;
            }
            return true;
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("Login")]
        [System.Web.Http.AllowAnonymous]
        public async Task<Object> Login([FromBody]AuthLoginDTO model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userService.GetUserByNameAsync(model.UserName);

                if (user == null)
                {
                    return NotFound();
                }

                if (!VerifyHashedPassword(user.PasswordHash, model.Password))
                {
                    return BadRequest("Incorrect login or password");
                }

                var newRefreshToken = GenerateTokenByRandomNumber();

                var refreshTokenDto = new RefreshTokenDTO
                {
                    IsActive = true,
                    Token = newRefreshToken,
                    Expires = DateTime.Now.AddDays(1),
                    RemoteIp = GetRemoteIp()
                };

                await _refreshTokenService.SetAsync(refreshTokenDto, user.Id);

                var jwToken = await _jwTokenService.GetTokenAsync(user.Id);

                var newJWToken = await GenerateJWTokenAsync(user.Id);

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

        public static string GenerateTokenByRandomNumber(int size = 32)
        {
            var randomNumber = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("RefreshToken")]
        public async Task<Object> RefreshToken([FromBody]string jwToken, string refreshToken)
        {
            var userPrincipal = GetPrincipalFromToken(jwToken);

            var tokenActive = await IsTokenExistsAsync(jwToken);

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

                    var refreshTokenDto = new RefreshTokenDTO
                    {
                        IsActive = true,
                        Token = newRefreshToken,
                        Expires = DateTime.Now.AddDays(5),
                        RemoteIp = GetRemoteIp()
                    };

                    await _refreshTokenService.SetAsync(refreshTokenDto, userId);

                    // JWToken
                    var newJWToken = await GenerateJWTokenAsync(userId);

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

        private async Task<string> GenerateJWTokenAsync(string userId, bool remember = false)
        {
            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return null;
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("id", user.Id.ToString()),
            };

            //
            claims.Add(new Claim("name", user.UserName));

            //
            var roles = await _userService.GetUserRolesByIdAsync(user.Id);

            claims.AddRange(roles.Select(p => new Claim(type: ClaimTypes.Role, p)));

            JWTSettingsDTO jwtSettings = GetSettings();

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));
            SigningCredentials _signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var accessExpiration = remember ? jwtSettings.RememberMeExpiration : jwtSettings.AccessExpiration;

            var jwtToken = new JwtSecurityToken(
                jwtSettings.Issuer,
                jwtSettings.Audience,
                claims,
                expires: DateTime.UtcNow.AddMinutes(accessExpiration),
                signingCredentials: _signingCredentials
            );
            JwtSecurityTokenHandler _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            return _jwtSecurityTokenHandler.WriteToken(jwtToken);
        }

        public async Task<bool> IsTokenExistsAsync(string token)
        {
            var userId = await _jwTokenService.GetUserIdAsync(token);
            return userId != null;
        }

        public ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            JwtSecurityTokenHandler _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

            var tokenValidationParameters = GetTokenValidationParameters(GetSettings());
            tokenValidationParameters.ValidateLifetime = false;

            var principal = _jwtSecurityTokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (!(securityToken is JwtSecurityToken jwtSecurityToken) || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");
            return principal;
        }

        public static TokenValidationParameters GetTokenValidationParameters(JWTSettingsDTO jwtSettings)
        {
            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            };
        }

        private static JWTSettingsDTO GetSettings()
        {
            return new JWTSettingsDTO
            {
                Secret = ConfigurationManager.AppSettings.Get("secret"),
                Issuer = ConfigurationManager.AppSettings.Get("issuer"),
                Audience = "https://api.kenguru.tj",
                AccessExpiration = 2260,
                RefreshExpiration = 2260,
                RememberMeExpiration = 2260
            };
        }

        private static string GetRemoteIp()
        {
            string ip = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(ip))
            {
                ip = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
            return ip;
        }
    }
}