namespace CateringEcommerce.Domain.Interfaces
{
    /// <summary>
    /// Token generation and validation service for JWT authentication
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generate JWT token for user/admin/owner/supervisor
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="userType">Type of user (ADMIN, USER, PARTNER, SUPERVISOR)</param>
        /// <param name="additionalClaims">Optional additional claims to include in token</param>
        /// <returns>JWT token string</returns>
        string GenerateToken(string userId, string userType, Dictionary<string, string>? additionalClaims = null);

        /// <summary>
        /// Validate JWT token signature and expiration
        /// </summary>
        /// <param name="token">JWT token to validate</param>
        /// <returns>True if token is valid, false otherwise</returns>
        bool ValidateToken(string token);

        /// <summary>
        /// Extract claims from JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Dictionary of claims, or null if token is invalid</returns>
        Dictionary<string, string>? GetTokenClaims(string token);

        /// <summary>
        /// Generate refresh token for token renewal
        /// </summary>
        /// <returns>Refresh token string</returns>
        string GenerateRefreshToken();
    }
}
