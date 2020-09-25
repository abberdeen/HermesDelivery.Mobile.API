using HermesDMobAPI.Models.DTO.OAuth;
using HermesDMobAPI.Services.Account;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace HermesDMobAPI.Services.OAuth
{
    public class AuthService
    {
        private UserService _userService;
        private JwTokenService _jwTokenService;

        public AuthService()
        {
            _userService = new UserService();
            _jwTokenService = new JwTokenService();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="remember"></param>
        /// <returns></returns>
        public async Task<string> GenerateJWTokenAsync(string userId, bool remember = false)
        {
            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return null;
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id), 
                new Claim("id", user.Id),
            };

            //
            var roles = await _userService.GetUserRolesByIdAsync(userId);

            claims.AddRange(roles.Select(p => new Claim(type: ClaimTypes.Role, p)));

            JwtSettingsDto jwtSettings = GetSettings();

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

        /// <summary>
        ///
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<bool> IsTokenExistsAsync(string token)
        {
            var userId = await _jwTokenService.GetUserIdAsync(token);
            return userId != null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
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

        private static TokenValidationParameters GetTokenValidationParameters(JwtSettingsDto jwtSettings)
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

        private static JwtSettingsDto GetSettings()
        {
            return new JwtSettingsDto
            {
                Secret = ConfigurationManager.AppSettings.Get("secret"),
                Issuer = ConfigurationManager.AppSettings.Get("issuer"),
                Audience = ConfigurationManager.AppSettings.Get("issuer"),
                AccessExpiration = 2260,
                RefreshExpiration = 2260,
                RememberMeExpiration = 2260
            };
        }
    }
}