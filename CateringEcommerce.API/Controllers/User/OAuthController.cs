using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Security;
using CateringEcommerce.Domain.Models.Security;
using CateringEcommerce.API.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CateringEcommerce.API.Controllers.User
{
    [Route("api/oauth")]
    [ApiController]
    [UserAuthorize]
    public class OAuthController : ControllerBase
    {
        private readonly IOAuthRepository _oauthRepo;
        private readonly ITokenService _tokenService;

        public OAuthController(IOAuthRepository oauthRepo, ITokenService tokenService)
        {
            _oauthRepo = oauthRepo ?? throw new ArgumentNullException(nameof(oauthRepo));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        /// <summary>
        /// Initiate OAuth login flow - redirects user to OAuth provider
        /// </summary>
        /// <param name="provider">Provider name (google, facebook)</param>
        /// <returns>Authorization URL to redirect user to</returns>
        [HttpGet("{provider}/login")]
        public async Task<IActionResult> InitiateOAuthLogin(string provider)
        {
            try
            {
                // Get client IP and User Agent for security
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

                var request = new OAuthAuthorizationRequest
                {
                    Provider = provider,
                    RedirectUrl = $"{Request.Scheme}://{Request.Host}/oauth-callback", // Frontend callback URL
                    AdditionalData = new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "ip_address", ipAddress },
                        { "user_agent", userAgent }
                    }
                };

                var response = await _oauthRepo.GenerateAuthorizationUrlAsync(request);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        authorizationUrl = response.AuthorizationUrl,
                        state = response.State
                    },
                    message = $"Redirect user to {provider} for authentication"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "OAuth initiation failed", error = ex.Message });
            }
        }

        /// <summary>
        /// Handle OAuth callback from provider
        /// </summary>
        /// <param name="provider">Provider name (google, facebook)</param>
        /// <param name="code">Authorization code from provider</param>
        /// <param name="state">State token for CSRF protection</param>
        /// <returns>JWT token and user info</returns>
        [HttpGet("{provider}/callback")]
        public async Task<IActionResult> HandleOAuthCallback(string provider, [FromQuery] string code, [FromQuery] string state)
        {
            try
            {
                if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
                {
                    return BadRequest(new { success = false, message = "Missing code or state parameter" });
                }

                // Validate state token (CSRF protection)
                var stateModel = await _oauthRepo.ValidateStateTokenAsync(state);
                if (stateModel == null)
                {
                    return BadRequest(new { success = false, message = "Invalid or expired state token" });
                }

                // Mark state token as used
                await _oauthRepo.MarkStateTokenUsedAsync(state);

                // Exchange authorization code for access token
                var tokenResponse = await _oauthRepo.ExchangeCodeForTokenAsync(provider, code);

                // Get user info from OAuth provider
                var userInfo = await _oauthRepo.GetUserInfoAsync(provider, tokenResponse.AccessToken);

                // Check if OAuth account is already linked
                var existingConnection = await _oauthRepo.GetOAuthConnectionAsync(provider, userInfo.Id);

                long userId;
                bool isNewUser = false;

                if (existingConnection != null)
                {
                    // User already has this OAuth account linked
                    userId = existingConnection.UserId;

                    // Update tokens and last login
                    await _oauthRepo.UpdateOAuthTokensAsync(existingConnection.OAuthId, tokenResponse);
                    await _oauthRepo.UpdateLastLoginAsync(existingConnection.OAuthId);
                }
                else
                {
                    // Check if user exists by email
                    var existingUserId = await _oauthRepo.FindUserByEmailAsync(userInfo.Email);

                    if (existingUserId.HasValue)
                    {
                        // User exists - link OAuth account
                        userId = existingUserId.Value;
                        await _oauthRepo.LinkOAuthAccountAsync(userId, provider, userInfo, tokenResponse);
                    }
                    else
                    {
                        // New user - create account
                        userId = await _oauthRepo.CreateUserFromOAuthAsync(userInfo, provider);
                        await _oauthRepo.LinkOAuthAccountAsync(userId, provider, userInfo, tokenResponse);
                        isNewUser = true;
                    }
                }

                // Generate JWT token
                var additionalClaims = new Dictionary<string, string>
                {
                    { "Email", userInfo.Email },
                    { "Name", userInfo.Name },
                    { "Provider", provider }
                };
                var jwtToken = _tokenService.GenerateToken(userId.ToString(), "USER", additionalClaims);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        token = jwtToken,
                        userId = userId,
                        email = userInfo.Email,
                        name = userInfo.Name,
                        picture = userInfo.Picture,
                        provider = provider,
                        isNewUser = isNewUser
                    },
                    message = isNewUser ? "Account created and logged in successfully" : "Logged in successfully"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to communicate with OAuth provider", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "OAuth callback failed", error = ex.Message });
            }
        }

        /// <summary>
        /// Link an OAuth account to existing user
        /// </summary>
        /// <param name="request">Link account request</param>
        /// <returns>Success status</returns>
        [HttpPost("link-account")]
        public async Task<IActionResult> LinkOAuthAccount([FromBody] LinkOAuthAccountRequest request)
        {
            try
            {
                // Get user ID from JWT token
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var userId = long.Parse(userIdClaim);

                // Validate state token
                var stateModel = await _oauthRepo.ValidateStateTokenAsync(request.State);
                if (stateModel == null)
                {
                    return BadRequest(new { success = false, message = "Invalid or expired state token" });
                }

                await _oauthRepo.MarkStateTokenUsedAsync(request.State);

                // Exchange code for token
                var tokenResponse = await _oauthRepo.ExchangeCodeForTokenAsync(request.Provider, request.Code);

                // Get user info
                var userInfo = await _oauthRepo.GetUserInfoAsync(request.Provider, tokenResponse.AccessToken);

                // Link OAuth account
                await _oauthRepo.LinkOAuthAccountAsync(userId, request.Provider, userInfo, tokenResponse);

                return Ok(new
                {
                    success = true,
                    message = $"{request.Provider} account linked successfully"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to link account", error = ex.Message });
            }
        }

        /// <summary>
        /// Unlink an OAuth account from user
        /// </summary>
        /// <param name="oauthId">OAuth connection ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("unlink-account/{oauthId}")]
        public async Task<IActionResult> UnlinkOAuthAccount(long oauthId)
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var userId = long.Parse(userIdClaim);

                var success = await _oauthRepo.UnlinkOAuthAccountAsync(userId, oauthId);

                if (!success)
                {
                    return BadRequest(new { success = false, message = "Failed to unlink account" });
                }

                return Ok(new
                {
                    success = true,
                    message = "OAuth account unlinked successfully"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to unlink account", error = ex.Message });
            }
        }

        /// <summary>
        /// Get user's connected OAuth accounts
        /// </summary>
        /// <returns>List of connected accounts</returns>
        [HttpGet("connected-accounts")]
        public async Task<IActionResult> GetConnectedAccounts()
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var userId = long.Parse(userIdClaim);

                var connections = await _oauthRepo.GetUserOAuthConnectionsAsync(userId);

                return Ok(new
                {
                    success = true,
                    data = connections,
                    count = connections.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to get connected accounts", error = ex.Message });
            }
        }

        /// <summary>
        /// Set primary OAuth connection
        /// </summary>
        /// <param name="oauthId">OAuth connection ID to set as primary</param>
        /// <returns>Success status</returns>
        [HttpPut("set-primary/{oauthId}")]
        public async Task<IActionResult> SetPrimaryConnection(long oauthId)
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var userId = long.Parse(userIdClaim);

                var success = await _oauthRepo.SetPrimaryOAuthConnectionAsync(userId, oauthId);

                if (!success)
                {
                    return BadRequest(new { success = false, message = "Failed to set primary connection" });
                }

                return Ok(new
                {
                    success = true,
                    message = "Primary connection set successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to set primary connection", error = ex.Message });
            }
        }

        /// <summary>
        /// Get active OAuth providers
        /// </summary>
        /// <returns>List of active providers</returns>
        [HttpGet("providers")]
        public async Task<IActionResult> GetActiveProviders()
        {
            try
            {
                var providers = await _oauthRepo.GetActiveProvidersAsync();

                // Don't expose client secrets
                var safeProviders = providers.Select(p => new
                {
                    p.ProviderName,
                    p.IsActive
                }).ToList();

                return Ok(new
                {
                    success = true,
                    data = safeProviders
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to get providers", error = ex.Message });
            }
        }
    }

    // Request models
    public class LinkOAuthAccountRequest
    {
        public string Provider { get; set; }
        public string Code { get; set; }
        public string State { get; set; }
    }
}
