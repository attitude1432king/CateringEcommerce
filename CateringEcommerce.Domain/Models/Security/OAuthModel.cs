using System;
using System.Collections.Generic;

namespace CateringEcommerce.Domain.Models.Security
{
    /// <summary>
    /// OAuth Provider Configuration
    /// </summary>
    public class OAuthProviderModel
    {
        public long ProviderId { get; set; }
        public string ProviderName { get; set; } // GOOGLE, FACEBOOK, APPLE
        public string ClientId { get; set; }
        public string ClientSecret { get; set; } // Encrypted
        public string RedirectUri { get; set; }
        public string AuthorizationEndpoint { get; set; }
        public string TokenEndpoint { get; set; }
        public string UserInfoEndpoint { get; set; }
        public string Scope { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    /// <summary>
    /// User OAuth Connection
    /// </summary>
    public class UserOAuthModel
    {
        public long OAuthId { get; set; }
        public long UserId { get; set; }
        public long ProviderId { get; set; }

        // OAuth Data
        public string ProviderUserId { get; set; }
        public string ProviderEmail { get; set; }
        public string ProviderName { get; set; }
        public string ProviderPicture { get; set; }

        // Tokens (Encrypted)
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? TokenExpiresAt { get; set; }

        // Linking
        public bool IsPrimary { get; set; }
        public DateTime LinkedDate { get; set; }
        public DateTime? LastLogin { get; set; }

        // Audit
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation
        public string ProviderDisplayName { get; set; }
    }

    /// <summary>
    /// OAuth Authorization Request
    /// </summary>
    public class OAuthAuthorizationRequest
    {
        public string Provider { get; set; } // google, facebook
        public string RedirectUrl { get; set; } // Where to redirect after success
        public Dictionary<string, string> AdditionalData { get; set; }
    }

    /// <summary>
    /// OAuth Authorization Response (URL to redirect user to)
    /// </summary>
    public class OAuthAuthorizationResponse
    {
        public string AuthorizationUrl { get; set; }
        public string State { get; set; } // CSRF token
    }

    /// <summary>
    /// OAuth Callback Request (from provider)
    /// </summary>
    public class OAuthCallbackRequest
    {
        public string Code { get; set; } // Authorization code from provider
        public string State { get; set; } // CSRF token to verify
        public string Error { get; set; } // Error from provider
        public string ErrorDescription { get; set; }
    }

    /// <summary>
    /// OAuth Token Response (from provider)
    /// </summary>
    public class OAuthTokenResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; }
        public string Scope { get; set; }
    }

    /// <summary>
    /// OAuth User Info (from provider)
    /// </summary>
    public class OAuthUserInfo
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public bool EmailVerified { get; set; }
        public string Name { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string Picture { get; set; }
        public string Locale { get; set; }
    }

    /// <summary>
    /// OAuth Login Result
    /// </summary>
    public class OAuthLoginResult
    {
        public bool Success { get; set; }
        public bool IsNewUser { get; set; }
        public long UserId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Token { get; set; } // JWT token for authenticated session
        public string RefreshToken { get; set; }
        public string Message { get; set; }
        public bool RequiresAdditionalInfo { get; set; } // If profile is incomplete
    }

    /// <summary>
    /// Link OAuth Account Request
    /// </summary>
    public class LinkOAuthAccountDto
    {
        public long UserId { get; set; }
        public string Provider { get; set; }
        public string AuthorizationCode { get; set; }
    }

    /// <summary>
    /// Unlink OAuth Account Request
    /// </summary>
    public class UnlinkOAuthAccountDto
    {
        public long UserId { get; set; }
        public long OAuthId { get; set; }
        public string Password { get; set; } // Require password confirmation
    }

    /// <summary>
    /// OAuth State Token (CSRF Protection)
    /// </summary>
    public class OAuthStateModel
    {
        public long StateId { get; set; }
        public string StateToken { get; set; }
        public string ProviderName { get; set; }
        public string RedirectUrl { get; set; }
        public string AdditionalData { get; set; } // JSON
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool Used { get; set; }
        public DateTime? UsedDate { get; set; }
    }

    /// <summary>
    /// Connected OAuth Accounts (for user profile)
    /// </summary>
    public class ConnectedOAuthAccountDto
    {
        public long OAuthId { get; set; }
        public string Provider { get; set; }
        public string ProviderEmail { get; set; }
        public string ProviderName { get; set; }
        public string ProviderPicture { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime LinkedDate { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool CanUnlink { get; set; } // False if it's the only login method
    }
}
