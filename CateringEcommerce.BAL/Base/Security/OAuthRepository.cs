using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using CateringEcommerce.Domain.Interfaces.Security;
using CateringEcommerce.Domain.Models.Security;
using Microsoft.Extensions.Configuration;
using CateringEcommerce.BAL.Configuration;

namespace CateringEcommerce.BAL.Base.Security
{
    /// <summary>
    /// OAuth Authentication Repository
    /// Handles OAuth integration with Google, Facebook, etc.
    /// </summary>
    public class OAuthRepository : IOAuthRepository
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public OAuthRepository(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _httpClient = httpClientFactory.CreateClient();
        }

        // =============================================
        // PROVIDER CONFIGURATION
        // =============================================

        public async Task<OAuthProviderModel> GetProviderAsync(string providerName)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = $@"
                SELECT c_provider_id AS ProviderId, c_provider_name AS ProviderName,
                       c_client_id AS ClientId, c_client_secret AS ClientSecret,
                       c_redirect_uri AS RedirectUri, c_authorization_endpoint AS AuthorizationEndpoint,
                       c_token_endpoint AS TokenEndpoint, c_user_info_endpoint AS UserInfoEndpoint,
                       c_scope AS Scope, c_is_active AS IsActive,
                       c_createddate AS CreatedDate, c_modifieddate AS ModifiedDate
                FROM {Table.SysOAuthProvider}
                WHERE c_provider_name = @ProviderName AND c_is_active = 1";

            return await connection.QueryFirstOrDefaultAsync<OAuthProviderModel>(sql, new { ProviderName = providerName.ToUpper() });
        }

        public async Task<List<OAuthProviderModel>> GetActiveProvidersAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = $@"
                SELECT c_provider_id AS ProviderId, c_provider_name AS ProviderName,
                       c_client_id AS ClientId, c_redirect_uri AS RedirectUri,
                       c_authorization_endpoint AS AuthorizationEndpoint,
                       c_token_endpoint AS TokenEndpoint, c_user_info_endpoint AS UserInfoEndpoint,
                       c_scope AS Scope, c_is_active AS IsActive,
                       c_createddate AS CreatedDate, c_modifieddate AS ModifiedDate
                FROM {Table.SysOAuthProvider}
                WHERE c_is_active = 1
                ORDER BY c_provider_name";

            var result = await connection.QueryAsync<OAuthProviderModel>(sql);
            return result.ToList();
        }

        public async Task<bool> UpdateProviderConfigAsync(OAuthProviderModel provider)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = $@"
                UPDATE {Table.SysOAuthProvider}
                SET c_client_id = @ClientId,
                    c_client_secret = @ClientSecret,
                    c_redirect_uri = @RedirectUri,
                    c_authorization_endpoint = @AuthorizationEndpoint,
                    c_token_endpoint = @TokenEndpoint,
                    c_user_info_endpoint = @UserInfoEndpoint,
                    c_scope = @Scope,
                    c_is_active = @IsActive,
                    c_modifieddate = GETDATE()
                WHERE c_provider_id = @ProviderId";

            var rowsAffected = await connection.ExecuteAsync(sql, provider);
            return rowsAffected > 0;
        }

        // =============================================
        // AUTHORIZATION FLOW
        // =============================================

        public async Task<OAuthAuthorizationResponse> GenerateAuthorizationUrlAsync(OAuthAuthorizationRequest request)
        {
            var provider = await GetProviderAsync(request.Provider);
            if (provider == null)
                throw new InvalidOperationException($"OAuth provider '{request.Provider}' not found or inactive");

            // Create state token for CSRF protection
            var stateToken = await CreateStateTokenAsync(
                provider.ProviderName,
                request.RedirectUrl,
                JsonSerializer.Serialize(request.AdditionalData),
                null, // IP address set by controller
                null  // User agent set by controller
            );

            // Build authorization URL
            var queryParams = new Dictionary<string, string>
            {
                { "client_id", provider.ClientId },
                { "redirect_uri", provider.RedirectUri },
                { "response_type", "code" },
                { "scope", provider.Scope },
                { "state", stateToken },
                { "access_type", "offline" }, // For refresh token
                { "prompt", "consent" }
            };

            var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
            var authorizationUrl = $"{provider.AuthorizationEndpoint}?{queryString}";

            return new OAuthAuthorizationResponse
            {
                AuthorizationUrl = authorizationUrl,
                State = stateToken
            };
        }

        public async Task<string> CreateStateTokenAsync(string providerName, string redirectUrl, string additionalData,
            string ipAddress, string userAgent)
        {
            using var connection = new SqlConnection(_connectionString);

            // Generate cryptographically secure random state token
            var stateToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");

            var sql = $@"
                INSERT INTO {Table.SysOAuthState}
                (c_state_token, c_provider_name, c_redirect_url, c_additional_data,
                 c_ip_address, c_user_agent, c_createddate, c_expires_at, c_used, c_used_date)
                VALUES
                (@StateToken, @ProviderName, @RedirectUrl, @AdditionalData,
                 @IpAddress, @UserAgent, GETDATE(), DATEADD(MINUTE, 10, GETDATE()), 0, NULL)";

            await connection.ExecuteAsync(sql, new
            {
                StateToken = stateToken,
                ProviderName = providerName,
                RedirectUrl = redirectUrl,
                AdditionalData = additionalData,
                IpAddress = ipAddress,
                UserAgent = userAgent
            });

            return stateToken;
        }

        public async Task<OAuthStateModel> ValidateStateTokenAsync(string stateToken)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = $@"
                SELECT c_state_id AS StateId, c_state_token AS StateToken,
                       c_provider_name AS ProviderName, c_redirect_url AS RedirectUrl,
                       c_additional_data AS AdditionalData, c_ip_address AS IpAddress,
                       c_user_agent AS UserAgent, c_createddate AS CreatedDate,
                       c_expires_at AS ExpiresAt, c_used AS Used, c_used_date AS UsedDate
                FROM {Table.SysOAuthState}
                WHERE c_state_token = @StateToken
                  AND c_used = 0
                  AND c_expires_at > GETDATE()";

            return await connection.QueryFirstOrDefaultAsync<OAuthStateModel>(sql, new { StateToken = stateToken });
        }

        public async Task<bool> MarkStateTokenUsedAsync(string stateToken)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = $@"
                UPDATE {Table.SysOAuthState}
                SET c_used = 1, c_used_date = GETDATE()
                WHERE c_state_token = @StateToken";

            var rowsAffected = await connection.ExecuteAsync(sql, new { StateToken = stateToken });
            return rowsAffected > 0;
        }

        // =============================================
        // TOKEN EXCHANGE
        // =============================================

        public async Task<OAuthTokenResponse> ExchangeCodeForTokenAsync(string providerName, string code)
        {
            var provider = await GetProviderAsync(providerName);
            if (provider == null)
                throw new InvalidOperationException($"OAuth provider '{providerName}' not found or inactive");

            var requestBody = new Dictionary<string, string>
            {
                { "code", code },
                { "client_id", provider.ClientId },
                { "client_secret", provider.ClientSecret },
                { "redirect_uri", provider.RedirectUri },
                { "grant_type", "authorization_code" }
            };

            var content = new FormUrlEncodedContent(requestBody);
            var response = await _httpClient.PostAsync(provider.TokenEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Token exchange failed: {response.StatusCode} - {errorContent}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<OAuthTokenResponse>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                PropertyNameCaseInsensitive = true
            });

            return tokenResponse;
        }

        public async Task<OAuthUserInfo> GetUserInfoAsync(string providerName, string accessToken)
        {
            var provider = await GetProviderAsync(providerName);
            if (provider == null)
                throw new InvalidOperationException($"OAuth provider '{providerName}' not found or inactive");

            var request = new HttpRequestMessage(HttpMethod.Get, provider.UserInfoEndpoint);
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"User info fetch failed: {response.StatusCode} - {errorContent}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<OAuthUserInfo>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                PropertyNameCaseInsensitive = true
            });

            return userInfo;
        }

        public async Task<OAuthTokenResponse> RefreshAccessTokenAsync(string providerName, string refreshToken)
        {
            var provider = await GetProviderAsync(providerName);
            if (provider == null)
                throw new InvalidOperationException($"OAuth provider '{providerName}' not found or inactive");

            var requestBody = new Dictionary<string, string>
            {
                { "refresh_token", refreshToken },
                { "client_id", provider.ClientId },
                { "client_secret", provider.ClientSecret },
                { "grant_type", "refresh_token" }
            };

            var content = new FormUrlEncodedContent(requestBody);
            var response = await _httpClient.PostAsync(provider.TokenEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Token refresh failed: {response.StatusCode} - {errorContent}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<OAuthTokenResponse>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                PropertyNameCaseInsensitive = true
            });

            return tokenResponse;
        }

        // =============================================
        // USER ACCOUNT LINKING
        // =============================================

        public async Task<UserOAuthModel> GetOAuthConnectionAsync(string providerName, string providerUserId)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = $@"
                SELECT o.c_oauth_id AS OAuthId, o.c_userid AS UserId, o.c_provider_id AS ProviderId,
                       o.c_provider_user_id AS ProviderUserId, o.c_provider_email AS ProviderEmail,
                       o.c_provider_name AS ProviderName, o.c_provider_picture AS ProviderPicture,
                       o.c_access_token AS AccessToken, o.c_refresh_token AS RefreshToken,
                       o.c_token_expires_at AS TokenExpiresAt, o.c_is_primary AS IsPrimary,
                       o.c_linked_date AS LinkedDate, o.c_last_login AS LastLogin,
                       o.c_createddate AS CreatedDate, o.c_modifieddate AS ModifiedDate,
                       p.c_provider_name as ProviderDisplayName
                FROM {Table.SysUserOAuth} o
                INNER JOIN {Table.SysOAuthProvider} p ON o.c_provider_id = p.c_provider_id
                WHERE p.c_provider_name = @ProviderName
                  AND o.c_provider_user_id = @ProviderUserId";

            return await connection.QueryFirstOrDefaultAsync<UserOAuthModel>(sql, new
            {
                ProviderName = providerName.ToUpper(),
                ProviderUserId = providerUserId
            });
        }

        public async Task<long?> FindUserByEmailAsync(string email)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = $"SELECT c_userid FROM {Table.SysUser} WHERE c_email = @Email AND c_isactive = 1";
            return await connection.QueryFirstOrDefaultAsync<long?>(sql, new { Email = email });
        }

        public async Task<long> CreateUserFromOAuthAsync(OAuthUserInfo userInfo, string providerName)
        {
            using var connection = new SqlConnection(_connectionString);

            // Generate a random password (user won't use it, but database might require it)
            var randomPassword = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

            var sql = $@"
                INSERT INTO {Table.SysUser} (c_email, c_password_hash, c_name, c_mobile,
                                       c_isemailverified, c_isactive, c_createddate, c_modifieddate, c_picture)
                OUTPUT INSERTED.c_userid
                VALUES (@Email, @PasswordHash, @Name, '',
                        1, 1, GETDATE(), GETDATE(), @Picture)";

            var userId = await connection.QuerySingleAsync<long>(sql, new
            {
                Email = userInfo.Email,
                PasswordHash = randomPassword, // Placeholder
                Name = userInfo.Name ?? (userInfo.GivenName + " " + userInfo.FamilyName).Trim(),
                Picture = userInfo.Picture
            });

            return userId;
        }

        public async Task<bool> LinkOAuthAccountAsync(long userId, string providerName, OAuthUserInfo userInfo,
            OAuthTokenResponse tokens)
        {
            var provider = await GetProviderAsync(providerName);
            if (provider == null)
                throw new InvalidOperationException($"OAuth provider '{providerName}' not found");

            using var connection = new SqlConnection(_connectionString);

            // Check if this OAuth account is already linked to another user
            var existingConnection = await GetOAuthConnectionAsync(providerName, userInfo.Id);
            if (existingConnection != null && existingConnection.UserId != userId)
                throw new InvalidOperationException("This OAuth account is already linked to another user");

            // Check if user already has this provider linked
            var sql = $@"
                SELECT COUNT(*)
                FROM {Table.SysUserOAuth} o
                INNER JOIN {Table.SysOAuthProvider} p ON o.c_provider_id = p.c_provider_id
                WHERE o.c_userid = @UserId AND p.c_provider_name = @ProviderName";

            var hasProvider = await connection.QuerySingleAsync<int>(sql, new
            {
                UserId = userId,
                ProviderName = providerName.ToUpper()
            }) > 0;

            if (hasProvider)
                throw new InvalidOperationException($"User already has {providerName} linked");

            // Store tokens (in production, encrypt these!)
            var insertSql = $@"
                INSERT INTO {Table.SysUserOAuth}
                (c_userid, c_provider_id, c_provider_user_id, c_provider_email, c_provider_name, c_provider_picture,
                 c_access_token, c_refresh_token, c_token_expires_at, c_is_primary, c_linked_date, c_last_login,
                 c_createddate, c_modifieddate)
                VALUES
                (@UserId, @ProviderId, @ProviderUserId, @ProviderEmail, @ProviderName, @ProviderPicture,
                 @AccessToken, @RefreshToken, @TokenExpiresAt, @IsPrimary, GETDATE(), GETDATE(),
                 GETDATE(), GETDATE())";

            var rowsAffected = await connection.ExecuteAsync(insertSql, new
            {
                UserId = userId,
                ProviderId = provider.ProviderId,
                ProviderUserId = userInfo.Id,
                ProviderEmail = userInfo.Email,
                ProviderName = userInfo.Name,
                ProviderPicture = userInfo.Picture,
                AccessToken = tokens.AccessToken, // TODO: Encrypt in production
                RefreshToken = tokens.RefreshToken,
                TokenExpiresAt = tokens.ExpiresIn > 0 ? DateTime.UtcNow.AddSeconds(tokens.ExpiresIn) : (DateTime?)null,
                IsPrimary = false
            });

            return rowsAffected > 0;
        }

        public async Task<bool> UnlinkOAuthAccountAsync(long userId, long oauthId)
        {
            // Check if user can unlink (must have password or another OAuth)
            var canUnlink = await CanUnlinkOAuthAccountAsync(userId, oauthId);
            if (!canUnlink)
                throw new InvalidOperationException("Cannot unlink - this is your only login method. Set a password first.");

            using var connection = new SqlConnection(_connectionString);
            var sql = $"DELETE FROM {Table.SysUserOAuth} WHERE c_oauth_id = @OAuthId AND c_userid = @UserId";
            var rowsAffected = await connection.ExecuteAsync(sql, new { OAuthId = oauthId, UserId = userId });
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateOAuthTokensAsync(long oauthId, OAuthTokenResponse tokens)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = $@"
                UPDATE {Table.SysUserOAuth}
                SET c_access_token = @AccessToken,
                    c_refresh_token = COALESCE(@RefreshToken, c_refresh_token),
                    c_token_expires_at = @TokenExpiresAt,
                    c_modifieddate = GETDATE()
                WHERE c_oauth_id = @OAuthId";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                OAuthId = oauthId,
                AccessToken = tokens.AccessToken, // TODO: Encrypt in production
                RefreshToken = tokens.RefreshToken,
                TokenExpiresAt = tokens.ExpiresIn > 0 ? DateTime.UtcNow.AddSeconds(tokens.ExpiresIn) : (DateTime?)null
            });

            return rowsAffected > 0;
        }

        public async Task<bool> UpdateLastLoginAsync(long oauthId)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = $"UPDATE {Table.SysUserOAuth} SET c_last_login = GETDATE(), c_modifieddate = GETDATE() WHERE c_oauth_id = @OAuthId";
            var rowsAffected = await connection.ExecuteAsync(sql, new { OAuthId = oauthId });
            return rowsAffected > 0;
        }

        // =============================================
        // USER OAUTH CONNECTIONS
        // =============================================

        public async Task<List<ConnectedOAuthAccountDto>> GetUserOAuthConnectionsAsync(long userId)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = $@"
                SELECT o.c_oauth_id AS OAuthId, p.c_provider_name AS Provider, o.c_provider_email AS ProviderEmail,
                       o.c_provider_name AS ProviderName, o.c_provider_picture AS ProviderPicture,
                       o.c_is_primary AS IsPrimary, o.c_linked_date AS LinkedDate, o.c_last_login AS LastLogin
                FROM {Table.SysUserOAuth} o
                INNER JOIN {Table.SysOAuthProvider} p ON o.c_provider_id = p.c_provider_id
                WHERE o.c_userid = @UserId
                ORDER BY o.c_is_primary DESC, o.c_linked_date DESC";

            var connections = await connection.QueryAsync<ConnectedOAuthAccountDto>(sql, new { UserId = userId });

            // Determine if each can be unlinked
            foreach (var conn in connections)
            {
                conn.CanUnlink = await CanUnlinkOAuthAccountAsync(userId, conn.OAuthId);
            }

            return connections.ToList();
        }

        public async Task<bool> HasOAuthConnectionsAsync(long userId)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = $"SELECT COUNT(*) FROM {Table.SysUserOAuth} WHERE c_userid = @UserId";
            var count = await connection.QuerySingleAsync<int>(sql, new { UserId = userId });
            return count > 0;
        }

        public async Task<bool> CanUnlinkOAuthAccountAsync(long userId, long oauthId)
        {
            using var connection = new SqlConnection(_connectionString);

            // Check if user has a password
            var hasPasswordSql = $@"
                SELECT CASE WHEN c_password_hash IS NOT NULL AND c_password_hash <> '' THEN 1 ELSE 0 END
                FROM {Table.SysUser} WHERE c_userid = @UserId";
            var hasPassword = await connection.QuerySingleAsync<bool>(hasPasswordSql, new { UserId = userId });

            if (hasPassword)
                return true; // Can unlink if user has password

            // Check if user has other OAuth connections
            var otherConnectionsSql = $"SELECT COUNT(*) FROM {Table.SysUserOAuth} WHERE c_userid = @UserId AND c_oauth_id <> @OAuthId";
            var otherConnectionsCount = await connection.QuerySingleAsync<int>(otherConnectionsSql, new { UserId = userId, OAuthId = oauthId });

            return otherConnectionsCount > 0; // Can unlink if user has other OAuth connections
        }

        public async Task<UserOAuthModel> GetPrimaryOAuthConnectionAsync(long userId)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = $@"
                SELECT TOP 1 o.c_oauth_id AS OAuthId, o.c_userid AS UserId, o.c_provider_id AS ProviderId,
                       o.c_provider_user_id AS ProviderUserId, o.c_provider_email AS ProviderEmail,
                       o.c_provider_name AS ProviderName, o.c_provider_picture AS ProviderPicture,
                       o.c_access_token AS AccessToken, o.c_refresh_token AS RefreshToken,
                       o.c_token_expires_at AS TokenExpiresAt, o.c_is_primary AS IsPrimary,
                       o.c_linked_date AS LinkedDate, o.c_last_login AS LastLogin,
                       o.c_createddate AS CreatedDate, o.c_modifieddate AS ModifiedDate,
                       p.c_provider_name AS ProviderDisplayName
                FROM {Table.SysUserOAuth} o
                INNER JOIN {Table.SysOAuthProvider} p ON o.c_provider_id = p.c_provider_id
                WHERE o.c_userid = @UserId
                ORDER BY o.c_is_primary DESC, o.c_linked_date ASC";

            return await connection.QueryFirstOrDefaultAsync<UserOAuthModel>(sql, new { UserId = userId });
        }

        public async Task<bool> SetPrimaryOAuthConnectionAsync(long userId, long oauthId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Remove primary from all connections
                var clearPrimarySql = $"UPDATE {Table.SysUserOAuth} SET c_is_primary = 0 WHERE c_userid = @UserId";
                await connection.ExecuteAsync(clearPrimarySql, new { UserId = userId }, transaction);

                // Set new primary
                var setPrimarySql = $"UPDATE {Table.SysUserOAuth} SET c_is_primary = 1 WHERE c_oauth_id = @OAuthId AND c_userid = @UserId";
                var rowsAffected = await connection.ExecuteAsync(setPrimarySql, new { OAuthId = oauthId, UserId = userId }, transaction);

                transaction.Commit();
                return rowsAffected > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // =============================================
        // CLEANUP
        // =============================================

        public async Task<(int DeletedStates, int DeletedConnections)> CleanupExpiredDataAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            // Delete expired state tokens (older than 1 day)
            var deleteStatesSql = $"DELETE FROM {Table.SysOAuthState} WHERE c_expires_at < DATEADD(DAY, -1, GETDATE())";
            var deletedStates = await connection.ExecuteAsync(deleteStatesSql);

            // Delete OAuth connections for deleted users (if any)
            var deleteConnectionsSql = $@"
                DELETE FROM {Table.SysUserOAuth}
                WHERE c_userid NOT IN (SELECT c_userid FROM {Table.SysUser} WHERE c_isactive = 1)";
            var deletedConnections = await connection.ExecuteAsync(deleteConnectionsSql);

            return (deletedStates, deletedConnections);
        }
    }
}
