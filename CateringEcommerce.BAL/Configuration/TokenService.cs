using CateringEcommerce.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _config;
        private readonly string _jwtKey;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        private readonly int _expireMinutes;

        public TokenService(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            var jwtSettings = _config.GetSection("Jwt");
            _jwtKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not configured");
            _jwtIssuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
            _jwtAudience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
            _expireMinutes = Convert.ToInt32(jwtSettings["ExpireMinutes"] ?? "1440"); // Default 24 hours
        }

        /// <summary>
        /// Generate JWT token with flexible claims
        /// </summary>
        public string GenerateToken(string userId, string userType, Dictionary<string, string>? additionalClaims = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim("UserType", userType),
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

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_expireMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Legacy method for backward compatibility
        /// </summary>
        public string GenerateToken(string username, string role, string PKID, string phoneNumber)
        {
            var additionalClaims = new Dictionary<string, string>
            {
                { JwtRegisteredClaimNames.Sub, username },
                { ClaimTypes.Role, role },
                { ClaimTypes.MobilePhone, phoneNumber }
            };

            return GenerateToken(PKID, role, additionalClaims);
        }

        /// <summary>
        /// Validate JWT token
        /// </summary>
        public bool ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtKey);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtAudience,
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
