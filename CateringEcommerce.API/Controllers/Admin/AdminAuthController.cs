using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Cryptography;
using System.Text;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.BAL.Helpers;

namespace CateringEcommerce.API.Controllers.Admin
{
    [Route("api/admin/auth")]
    [ApiController]
    public class AdminAuthController : ControllerBase
    {
        private readonly IAdminAuthRepository _adminAuthRepository;
        private readonly IRBACRepository _rbacRepository;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AdminAuthController> _logger;
        private const int MAX_FAILED_ATTEMPTS = 5;
        private const int LOCK_DURATION_MINUTES = 30;

        public AdminAuthController(
            IAdminAuthRepository adminAuthRepository,
            IRBACRepository rbacRepository,
            ITokenService tokenService,
            ILogger<AdminAuthController> logger)
        {
            _adminAuthRepository = adminAuthRepository ?? throw new ArgumentNullException(nameof(adminAuthRepository));
            _rbacRepository = rbacRepository ?? throw new ArgumentNullException(nameof(rbacRepository));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Admin Login (Rate Limited: 3 attempts per 15 minutes)
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("admin_login")]
        public IActionResult Login([FromBody] AdminLoginRequest request)
        {
            try
            {
                // Validate model state (DataAnnotations)
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponseHelper.Failure("Invalid request data."));

                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                    return ApiResponseHelper.Failure("Username and password are required.");

                // Check if account is locked
                if (_adminAuthRepository.IsAccountLocked(request.Username))
                {
                    return ApiResponseHelper.Failure("Account is locked due to multiple failed login attempts. Please try again later.");
                }

                // Try BCrypt first, then fall back to SHA256 for backward compatibility
                var admin = _adminAuthRepository.GetAdminByUsername(request.Username);

                if (admin == null)
                {
                    // Increment failed login attempts even if user doesn't exist (prevent enumeration)
                    _adminAuthRepository.IncrementFailedLoginAttempts(request.Username);
                    return ApiResponseHelper.Failure("Invalid username or password.");
                }

                // Verify password - support both BCrypt (new) and SHA256 (legacy)
                bool isPasswordValid = HashHelper.VerifyPassword(request.Password, admin.PasswordHash);

                if (!isPasswordValid)
                {
                    // Increment failed login attempts
                    _adminAuthRepository.IncrementFailedLoginAttempts(request.Username);

                    // Check if we should lock the account after this failed attempt
                    var updatedAdmin = _adminAuthRepository.GetAdminByUsername(request.Username);
                    if (updatedAdmin != null && updatedAdmin.FailedLoginAttempts >= MAX_FAILED_ATTEMPTS)
                    {
                        _adminAuthRepository.LockAccount(request.Username, DateTime.UtcNow.AddMinutes(LOCK_DURATION_MINUTES));
                        return ApiResponseHelper.Failure("Account locked due to multiple failed attempts. Please try again in 30 minutes.");
                    }

                    return ApiResponseHelper.Failure("Invalid username or password.");
                }

                // Check if account should be locked (in case it wasn't checked before)
                if (admin.FailedLoginAttempts >= MAX_FAILED_ATTEMPTS)
                {
                    _adminAuthRepository.LockAccount(request.Username, DateTime.UtcNow.AddMinutes(LOCK_DURATION_MINUTES));
                    return ApiResponseHelper.Failure("Account locked due to multiple failed attempts. Please try again in 30 minutes.");
                }

                // Reset failed attempts on successful login
                _adminAuthRepository.ResetFailedLoginAttempts(admin.AdminId);

                // Update last login
                _adminAuthRepository.UpdateLastLogin(admin.AdminId);

                // Log activity
                _adminAuthRepository.LogAdminActivity(admin.AdminId, "LOGIN", "Admin logged in successfully");

                // Generate JWT token
                var additionalClaims = new Dictionary<string, string>
                {
                    { "AdminId", admin.AdminId.ToString() },
                    { "Email", admin.Email }
                };
                string token = _tokenService.GenerateToken(
                    admin.AdminId.ToString(),
                    admin.Role,
                    additionalClaims
                );

               //SECURITY FIX: Set token as httpOnly cookie instead of returning in response
               var cookieOptions = new CookieOptions
               {
                   HttpOnly = true,  // Prevents JavaScript access (XSS protection)
                   Secure = true,    // HTTPS only
                   SameSite = SameSiteMode.None,  // Allow cross-origin (different ports)
                   Expires = DateTimeOffset.UtcNow.AddDays(7),  // 7-day expiry
                   Path = "/",
                   IsEssential = true
               };


                Response.Cookies.Append("adminToken", token, cookieOptions);

                // Check if the account has a temporary password that must be changed
                bool requirePasswordChange = _adminAuthRepository.IsTemporaryPassword(admin.AdminId);

                // SECURITY: Do NOT include token in response body
                var response = new
                {
                    adminId = admin.AdminId,
                    username = admin.Username,
                    email = admin.Email,
                    fullName = admin.FullName,
                    role = admin.Role,
                    lastLogin = admin.LastLogin ?? DateTime.UtcNow,
                    profilePhoto = admin.ProfilePhoto,
                    requirePasswordChange   // Frontend must show force-change modal when true
                };

                string loginMessage = requirePasswordChange
                    ? "Login successful. Please change your temporary password to continue."
                    : "Login successful.";

                return ApiResponseHelper.Success(response, loginMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Admin Auth operation failed: {ex.Message}");
                return StatusCode(500, ApiResponseHelper.Failure("An internal error occurred. Please try again later."));
            }
        }

        /// <summary>
        /// Get current admin details
        /// </summary>
        [HttpGet("me")]
        [AdminAuthorize]
        public IActionResult GetCurrentAdmin()
        {
            try
            {
                var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                if (adminIdClaim == null || !long.TryParse(adminIdClaim.Value, out long adminId))
                {
                    return ApiResponseHelper.Failure("Invalid admin session.");
                }

                var admin = _adminAuthRepository.GetAdminById(adminId);

                if (admin == null)
                    return ApiResponseHelper.Failure("Admin not found.");

                return ApiResponseHelper.Success(admin, "Admin details retrieved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Admin Auth operation failed: {ex.Message}");
                return StatusCode(500, ApiResponseHelper.Failure("An internal error occurred. Please try again later."));
            }
        }

        /// <summary>
        /// Get current admin permissions and roles from database
        /// </summary>
        [HttpGet("permissions")]
        [AdminAuthorize]
        public async Task<IActionResult> GetPermissions()
        {
            try
            {
                var adminIdClaim = User.Claims.LastOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                if (adminIdClaim == null || !long.TryParse(adminIdClaim.Value, out long adminId))
                {
                    return ApiResponseHelper.Failure("Invalid admin session.");
                }

                // Get permissions from database using RBAC repository
                var permissionContext = await _rbacRepository.GetAdminPermissionContextAsync(adminId);

                // Format response for frontend
                var permissionData = new
                {
                    roles = permissionContext.Roles,
                    permissions = permissionContext.Permissions
                };

                return ApiResponseHelper.Success(permissionData, "Permissions retrieved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Admin Auth operation failed: {ex.Message}");
                return StatusCode(500, ApiResponseHelper.Failure("An internal error occurred. Please try again later."));
            }
        }

        /// <summary>
        /// Change temporary password — must be called by a logged-in admin whose
        /// c_is_temporary_password = 1. Validates current password, enforces strength
        /// requirements, then clears the temporary flag.
        /// </summary>
        [HttpPost("change-temporary-password")]
        [AdminAuthorize]
        public IActionResult ChangeTempPassword([FromBody] ChangeTempPasswordDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponseHelper.Failure("Invalid request data."));

                if (dto.NewPassword != dto.ConfirmPassword)
                    return BadRequest(ApiResponseHelper.Failure("New password and confirmation do not match."));

                if (!IsStrongPassword(dto.NewPassword))
                    return BadRequest(ApiResponseHelper.Failure(
                        "Password must be at least 10 characters and include uppercase, lowercase, a digit, and a special character."));

                var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                if (adminIdClaim == null || !long.TryParse(adminIdClaim.Value, out long adminId))
                    return ApiResponseHelper.Failure("Invalid admin session.");

                var admin = _adminAuthRepository.GetAdminById(adminId);
                if (admin == null)
                    return ApiResponseHelper.Failure("Admin not found.");

                if (!HashHelper.VerifyPassword(dto.CurrentPassword, admin.PasswordHash))
                    return BadRequest(ApiResponseHelper.Failure("Current password is incorrect."));

                if (dto.CurrentPassword == dto.NewPassword)
                    return BadRequest(ApiResponseHelper.Failure("New password must be different from the current password."));

                var newHash = HashHelper.HashPassword(dto.NewPassword);
                bool updated = _adminAuthRepository.ChangeTempPassword(adminId, newHash);

                if (!updated)
                    return StatusCode(500, ApiResponseHelper.Failure("Failed to update password. Please try again."));

                _adminAuthRepository.LogAdminActivity(adminId, "CHANGE_TEMP_PASSWORD", "Admin changed temporary password");

                return ApiResponseHelper.Success(null, "Password changed successfully. Welcome!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] ChangeTempPassword failed: {ex.Message}");
                return StatusCode(500, ApiResponseHelper.Failure("An internal error occurred. Please try again later."));
            }
        }

        private static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 10)
                return false;

            bool hasUpper   = password.Any(char.IsUpper);
            bool hasLower   = password.Any(char.IsLower);
            bool hasDigit   = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

        /// <summary>
        /// Admin Logout - Clears authentication cookie
        /// </summary>
        [HttpPost("logout")]
        [AdminAuthorize]
        public IActionResult Logout()
        {
            try
            {
                var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                if (adminIdClaim != null && long.TryParse(adminIdClaim.Value, out long adminId))
                {
                    _adminAuthRepository.LogAdminActivity(adminId, "LOGOUT", "Admin logged out");
                }

                // SECURITY FIX: Clear httpOnly cookie
                Response.Cookies.Delete("adminToken", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/"
                });

                return ApiResponseHelper.Success(null, "Logout successful.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Admin Auth operation failed: {ex.Message}");
                return StatusCode(500, ApiResponseHelper.Failure("An internal error occurred. Please try again later."));
            }
        }
    }
}
