using CateringEcommerce.Domain.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CateringEcommerce.BAL.Configuration
{
    /// <summary>
    /// JWT Token generation and validation service
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly ISystemSettingsProvider _settings;

        public TokenService(ISystemSettingsProvider settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        /// Generate JWT token with flexible claims
        /// </summary>
        public string GenerateToken(string userId, string role, Dictionary<string, string>? additionalClaims = null)
        {
            var jwtKey = _settings.GetString("JWT.KEY");
            var jwtIssuer = _settings.GetString("JWT.ISSUER");
            var jwtAudience = _settings.GetString("JWT.AUDIENCE");
            var expireMinutes = _settings.GetInt("JWT.EXPIRE_MINUTES", 1440);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add additional claims if provided
            if (additionalClaims != null)
            {
                foreach (var claim in additionalClaims)
                {
                    claims.Add(new Claim(claim.Key, claim.Value));
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Validate JWT token
        /// </summary>
        public bool ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_settings.GetString("JWT.KEY"));

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _settings.GetString("JWT.ISSUER"),
                    ValidateAudience = true,
                    ValidAudience = _settings.GetString("JWT.AUDIENCE"),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Extract claims from JWT token
        /// </summary>
        public Dictionary<string, string>? GetTokenClaims(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                return jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Generate cryptographically secure refresh token
        /// </summary>
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
