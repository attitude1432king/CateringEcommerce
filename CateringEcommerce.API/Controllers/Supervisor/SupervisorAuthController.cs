using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Supervisor;
using CateringEcommerce.Domain.Models.Supervisor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace CateringEcommerce.API.Controllers.Supervisor
{
    [Route("api/Supervisor/auth")]
    [ApiController]
    public class SupervisorAuthController : ControllerBase
    {
        private readonly ISupervisorRepository _supervisorRepository;
        private readonly ITokenService _tokenService;
        private readonly ILogger<SupervisorAuthController> _logger;

        private static readonly CookieOptions SupervisorCookieOptions = new()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            Path = "/",
            IsEssential = true
        };

        private static readonly CookieOptions ClearCookieOptions = new()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/"
        };

        public SupervisorAuthController(
            ISupervisorRepository supervisorRepository,
            ITokenService tokenService,
            ILogger<SupervisorAuthController> logger)
        {
            _supervisorRepository = supervisorRepository ?? throw new ArgumentNullException(nameof(supervisorRepository));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Supervisor Login — issues supervisorToken as httpOnly cookie.
        /// Token is NEVER returned in the response body.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login([FromBody] SupervisorLoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponseHelper.Failure("Invalid request data."));

                if (string.IsNullOrWhiteSpace(request.Identifier) || string.IsNullOrWhiteSpace(request.Password))
                    return BadRequest(ApiResponseHelper.Failure("Email/phone and password are required."));

                var loginInfo = await _supervisorRepository.GetSupervisorForLoginAsync(request.Identifier);

                if (loginInfo == null)
                    return ApiResponseHelper.Failure("Invalid credentials. Please try again.");

                // Only ACTIVE supervisors may log in
                if (!loginInfo.CurrentStatus.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase))
                    return ApiResponseHelper.Failure("Your account is not active. Please contact support.");

                if (!HashHelper.VerifyPassword(request.Password, loginInfo.PasswordHash))
                    return ApiResponseHelper.Failure("Invalid credentials. Please try again.");

                // Generate JWT — role "Supervisor" for authorization filters
                var additionalClaims = new Dictionary<string, string>
                {
                    { "SupervisorId",    loginInfo.SupervisorId.ToString() },
                    { "SupervisorType",  loginInfo.SupervisorType },
                    { "AuthorityLevel",  loginInfo.AuthorityLevel }
                };

                string token = _tokenService.GenerateToken(
                    loginInfo.SupervisorId.ToString(),
                    "Supervisor",
                    additionalClaims);

                // SECURITY: set as httpOnly cookie — never expose in response body
                Response.Cookies.Append("supervisorToken", token, SupervisorCookieOptions);

                await _supervisorRepository.UpdateLastLoginAsync(loginInfo.SupervisorId);

                _logger.LogInformation("Supervisor {Id} logged in successfully.", loginInfo.SupervisorId);

                return ApiResponseHelper.Success(new
                {
                    supervisorId    = loginInfo.SupervisorId,
                    fullName        = loginInfo.FullName,
                    email           = loginInfo.Email,
                    phone           = loginInfo.Phone,
                    supervisorType  = loginInfo.SupervisorType,
                    authorityLevel  = loginInfo.AuthorityLevel
                }, "Login successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Supervisor login failed.");
                return StatusCode(500, ApiResponseHelper.Failure("An internal error occurred. Please try again later."));
            }
        }

        /// <summary>
        /// Supervisor Logout — clears the supervisorToken httpOnly cookie.
        /// </summary>
        [HttpPost("logout")]
        [SupervisorAuthorize]
        public IActionResult Logout()
        {
            try
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (long.TryParse(idClaim, out long supervisorId))
                    _logger.LogInformation("Supervisor {Id} logged out.", supervisorId);

                Response.Cookies.Delete("supervisorToken", ClearCookieOptions);
                return ApiResponseHelper.Success(null, "Logout successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Supervisor logout failed.");
                return StatusCode(500, ApiResponseHelper.Failure("An internal error occurred."));
            }
        }

        /// <summary>
        /// Returns the authenticated supervisor's profile — used by frontend on mount to validate session.
        /// </summary>
        [HttpGet("me")]
        [SupervisorAuthorize]
        public async Task<IActionResult> Me()
        {
            try
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!long.TryParse(idClaim, out long supervisorId))
                    return ApiResponseHelper.Failure("Invalid supervisor session.");

                var supervisor = await _supervisorRepository.GetSupervisorByIdAsync(supervisorId);
                if (supervisor == null)
                    return ApiResponseHelper.Failure("Supervisor not found.");

                return ApiResponseHelper.Success(supervisor, "Session valid.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Supervisor /me failed.");
                return StatusCode(500, ApiResponseHelper.Failure("An internal error occurred."));
            }
        }
    }
}
