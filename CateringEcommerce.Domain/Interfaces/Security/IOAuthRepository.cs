using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Models.Security;

namespace CateringEcommerce.Domain.Interfaces.Security
{
    /// <summary>
    /// OAuth Authentication Repository
    /// Handles OAuth integration with Google, Facebook, etc.
    /// </summary>
    public interface IOAuthRepository
    {
        // =============================================
        // PROVIDER CONFIGURATION
        // =============================================

        /// <summary>
        /// Get OAuth provider configuration
        /// </summary>
        Task<OAuthProviderModel> GetProviderAsync(string providerName);

        /// <summary>
        /// Get all active OAuth providers
        /// </summary>
        Task<List<OAuthProviderModel>> GetActiveProvidersAsync();

        // =============================================
        // AUTHORIZATION FLOW
        // =============================================

        /// <summary>
        /// Generate authorization URL for OAuth provider
        /// </summary>
        Task<OAuthAuthorizationResponse> GenerateAuthorizationUrlAsync(OAuthAuthorizationRequest request);

        /// <summary>
        /// Create and store OAuth state token (CSRF protection)
        /// </summary>
        Task<string> CreateStateTokenAsync(string providerName, string redirectUrl, string additionalData,
            string ipAddress, string userAgent);

        /// <summary>
        /// Validate OAuth state token
        /// </summary>
        Task<OAuthStateModel> ValidateStateTokenAsync(string stateToken);

        /// <summary>
        /// Mark state token as used
        /// </summary>
        Task<bool> MarkStateTokenUsedAsync(string stateToken);

        // =============================================
        // TOKEN EXCHANGE
        // =============================================

        /// <summary>
        /// Exchange authorization code for access token
        /// </summary>
        Task<OAuthTokenResponse> ExchangeCodeForTokenAsync(string providerName, string code);

        /// <summary>
        /// Get user info from OAuth provider
        /// </summary>
        Task<OAuthUserInfo> GetUserInfoAsync(string providerName, string accessToken);

        /// <summary>
        /// Refresh access token
        /// </summary>
        Task<OAuthTokenResponse> RefreshAccessTokenAsync(string providerName, string refreshToken);

        // =============================================
        // USER ACCOUNT LINKING
        // =============================================

        /// <summary>
        /// Find user by OAuth provider user ID
        /// </summary>
        Task<UserOAuthModel> GetOAuthConnectionAsync(string providerName, string providerUserId);

        /// <summary>
        /// Find existing user by email from OAuth
        /// </summary>
        Task<long?> FindUserByEmailAsync(string email);

        /// <summary>
        /// Create new user from OAuth data
        /// </summary>
        Task<long> CreateUserFromOAuthAsync(OAuthUserInfo userInfo, string providerName);

        /// <summary>
        /// Link OAuth account to existing user
        /// </summary>
        Task<bool> LinkOAuthAccountAsync(long userId, string providerName, OAuthUserInfo userInfo,
            OAuthTokenResponse tokens);

        /// <summary>
        /// Unlink OAuth account from user
        /// </summary>
        Task<bool> UnlinkOAuthAccountAsync(long userId, long oauthId);

        /// <summary>
        /// Update OAuth tokens
        /// </summary>
        Task<bool> UpdateOAuthTokensAsync(long oauthId, OAuthTokenResponse tokens);

        /// <summary>
        /// Update last login time
        /// </summary>
        Task<bool> UpdateLastLoginAsync(long oauthId);

        // =============================================
        // USER OAUTH CONNECTIONS
        // =============================================

        /// <summary>
        /// Get all OAuth connections for user
        /// </summary>
        Task<List<ConnectedOAuthAccountDto>> GetUserOAuthConnectionsAsync(long userId);

        /// <summary>
        /// Check if user has any OAuth connections
        /// </summary>
        Task<bool> HasOAuthConnectionsAsync(long userId);

        /// <summary>
        /// Check if user can unlink OAuth account (must have password or another OAuth)
        /// </summary>
        Task<bool> CanUnlinkOAuthAccountAsync(long userId, long oauthId);

        /// <summary>
        /// Get primary OAuth connection (if any)
        /// </summary>
        Task<UserOAuthModel> GetPrimaryOAuthConnectionAsync(long userId);

        /// <summary>
        /// Set OAuth connection as primary
        /// </summary>
        Task<bool> SetPrimaryOAuthConnectionAsync(long userId, long oauthId);

        // =============================================
        // CLEANUP
        // =============================================

        /// <summary>
        /// Clean up expired state tokens and inactive connections
        /// </summary>
        Task<(int DeletedStates, int DeletedConnections)> CleanupExpiredDataAsync();
    }
}
