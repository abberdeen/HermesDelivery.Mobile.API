using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using HermesDelivery.Mobile.API.Models.DTO.OAuth;
using HermesDelivery.Mobile.API.Services.OAuth;
using Microsoft.Owin.Security;

namespace ERP.API.Controllers
{
    [System.Web.Http.RoutePrefix("api/v1/Auth")]
    public class AuthController : Controller
    {
        private readonly AccountService _userService;
        private readonly RefreshTokenService _refreshTokenService;
        private readonly JWTokenService _jwTokenService;
        private readonly IUserRoleService _userRoleService; 

        public AuthController( )
        {
            _userService = new AccountService();
            _refreshTokenService = new RefreshTokenService();
            _jwTokenService = new JWTokenService();
            _userRoleService = userRoleService; 
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("Login")]
        [System.Web.Http.AllowAnonymous]
        public async Task<ActionResult> Login([FromBody]AuthLoginDTO model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userService.GetUserAsync(model.UserName);

                if (user == null)
                {
                    return HttpNotFound();
                }

                if (Encrypt(model.Password) != user.PasswordHash) return BaseDataResponse<AuthenticateResult>.Fail(null, new ErrorModel(ErrorCode.InvalidLoginPass));

                if (user.LockDate != null) return BaseDataResponse<AuthenticateResult>.Fail(null, new ErrorModel(ErrorCode.AccessToTheSystemIsSuspended));

                var t = await _refreshTokenService.GetByUserIdAsync(user.Id);
                if (t != null)
                    await _refreshTokenService.DeleteAsync(t);

                var token = await GetJwtTokenForUserAsync(user);

                var refreshToken = GenerateTokenByRandomNumber();

                RefreshToken rt = new RefreshToken();
                rt.CreatedAt = DateTime.Now;
                rt.IsActive = true;
                rt.UserId = user.Id;
                rt.Token = refreshToken;
                rt.Expires = DateTime.Now.AddDays(5);
                rt.RemoteIpAddress = HttpContext.Current.Request.UserHostAddress;

                await _refreshTokenService.CreateAsync(rt);

                var userToken = await _jwTokenService.GetByUserIdAsync(user.Id);

                if (userToken == null)
                {
                    userToken = new ActiveUserToken()
                    {
                        UserId = user.Id,
                        Token = token
                    };
                    await _jwTokenService.CreateAsync(userToken);

                    //cache
                    var memCacher = new MemoryCacher();
                    if (memCacher.GetValue(userToken.Token) == null)
                    {
                        memCacher.Add(token, user.Id, DateTimeOffset.UtcNow.AddHours(10));
                    }
                }
                else
                {
                    var memCacher = new MemoryCacher();
                    if (memCacher.GetValue(userToken.Token) != null)
                    {
                        memCacher.Delete(userToken.Token);
                    }
                    memCacher.Add(token, user.Id, DateTimeOffset.UtcNow.AddHours(10));
                    userToken.Token = token;
                    await _jwTokenService.UpdateAsync(userToken);
                }

                return BaseDataResponse<AuthenticateResult>.Success(new AuthenticateResult(token, refreshToken));
            }
            return Response(BaseDataResponse<AuthenticateResult>.Fail(null, new ErrorModel(ErrorCode.ModelStateIsNotValid)));
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
        public async Task<ActionResult> RefreshToken(string refreshToken)
        {
            var userPrincipal = GetPrincipalFromToken(refreshToken);
            var tokenActive = await TokenActiveAsync(refreshToken);
            // invalid token/signing key was passed and we can't extract user claims
            if (userPrincipal != null && tokenActive)
            {
                var userId = userPrincipal.Claims.First(c => c.Type == "id").Value;

                //var ipAddress = _accessor.HttpContext.Connection.RemoteIpAddress?.ToString();
                var ipAddress = HttpContext.Current.Request.UserHostAddress;

                var user = await _refreshTokenService.GetByUserIdAsync(userId);

                if (user != null && user.IsActive && user.RemoteIpAddress == ipAddress && user.Token == model.RefreshToken)
                {
                    string _userId = userPrincipal.Claims.FirstOrDefault(c => c.Type == "id")?.Value.ToString();

                    var user = await _userService.GetUserByIdAsync(_userId);
                    var jwtToken = await GetJwtTokenForUserAsync(user);
                    var existingRt = await _refreshTokenService.GetByUserIdAsync(userId);

                    await _refreshTokenService.DeleteAsync(existingRt);

                    var refreshToken = GenerateTokenByRandomNumber();

                    RefreshToken rt = new RefreshToken();
                    rt.CreatedAt = DateTime.Now;
                    rt.IsActive = true;
                    rt.UserId = user.Id;
                    rt.Token = refreshToken;
                    rt.Expires = DateTime.Now.AddDays(5);
                    rt.RemoteIpAddress = HttpContext.Current.Request.UserHostAddress;

                    await _refreshTokenService.CreateAsync(rt);

                    var userToken = await _jwTokenService.GetByUserIdAsync(user.Id);

                    if (userToken == null)
                    {
                        userToken = new ActiveUserToken()
                        {
                            UserId = user.Id,
                            Token = jwtToken
                        };
                        await _jwTokenService.CreateAsync(userToken);

                        var memCacher = new MemoryCacher();
                        memCacher.Add(jwtToken, user.Id, DateTimeOffset.UtcNow.AddHours(10));
                    }
                    else
                    {
                        //if (HttpRuntime.Cache.Get())
                        //{
                        //    activeTokens.Remove(userToken.Token);
                        //    _cache.Set(ActiveTokenCacheName, activeTokens);
                        //}
                        var memCacher = new MemoryCacher();
                        if (memCacher.GetValue(model.Token) == null)
                        {
                            memCacher.Add(jwtToken, user.Id, DateTimeOffset.UtcNow.AddHours(10));
                        }

                        userToken.Token = jwtToken;
                        await _jwTokenService.UpdateAsync(userToken);
                    }
                    return BaseDataResponse<AuthenticateResult>.Success(new AuthenticateResult(jwtToken, refreshToken));
                }
                return BaseDataResponse<AuthenticateResult>.Fail(null, new ErrorModel(ErrorCode.InvalidLoginPass));
            }
            return Response(BaseDataResponse<AuthenticateResult>.Fail(null));
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
            var roles = await _userRoleService.GetByUserIdAsync(user.Id);

            claims.AddRange(roles.Select(p => new Claim(type: ClaimTypes.Role, p)));

            JWTSettings jwtSettings = GetSettings();

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
            var userId= await _jwTokenService.GetUserIdAsync(token);
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

        public static TokenValidationParameters GetTokenValidationParameters(JWTSettings jwtSettings)
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

        private static JWTSettings GetSettings()
        {
            return new JWTSettings
            {
                Secret = "98RzBdS6S8E3kP4AdmdKfPzahJNqXMLn4t2VLwrRXDesepjeUg5t8ddrHcWemGswUU8TsF9dRqLm2YzCbaVXUWKKUgSXSFtmW6wad6vJCYVG4dQWfLvKCy9tse3AWVtMeyRqSta5Vy7XaAmDdhqzmna6ZeV68886RKzLA25egGJ3Fy7nQe68Aw5WpLK3HEEkG67YTH8daJkpHR5BhJKXa2zLX5ZvWtgAuVYSQZxykbp64bgAQ4AZFadFMCcafhTZ",
                Issuer = "https://kenguru.tj",
                Audience = "https://api.kenguru.tj",
                AccessExpiration = 2260,
                RefreshExpiration = 2260,
                RememberMeExpiration = 2260
            };
        }

    }
}