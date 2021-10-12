using CourierAPI.Infrastructure.Database;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CourierAPI.DTO.OAuth;

namespace CourierAPI.Services.OAuth
{
    public class AuthService
    {
        private AppDbContext _dbContext;
        private readonly ILogger _logger;
        private JwTokenService _jwTokenService;

        public AuthService(ILogger logger, JwTokenService jwTokenService)
        {
            _dbContext = new AppDbContext();
            _logger = logger;
            _jwTokenService = jwTokenService;
        }

        public async Task<Courier> GetCourierByPhoneAsync(string phoneNumber)
        {
            return await _dbContext.Couriers.Where(e => e.Phone == phoneNumber).FirstOrDefaultAsync();
        }

        public async Task<Courier> GetCourierByIdAsync(int courierId)
        {
            return await _dbContext.Couriers.Where(e => e.Id == courierId).FirstOrDefaultAsync();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="courierId"></param>
        /// <param name="remember"></param>
        /// <returns></returns>
        public async Task<string> GenerateJWTokenAsync(int courierId, bool remember = false)
        {
            var courier = await this.GetCourierByIdAsync(courierId);

            if (courier == null)
            {
                return null;
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, courier.Phone),
                new Claim(ClaimTypes.NameIdentifier, courier.Id.ToString()),
                new Claim("id", courier.Id.ToString()),
            };

            //
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
            var courierId = await _jwTokenService.GetCourierIdAsync(token);
            return courierId != null;
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