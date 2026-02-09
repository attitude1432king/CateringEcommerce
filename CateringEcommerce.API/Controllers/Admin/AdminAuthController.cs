using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using CateringEcommerce.Domain.Interfaces.Admin;

namespace CateringEcommerce.API.Controllers.Admin
{
    [Route("api/admin/auth")]
    [ApiController]
    public class AdminAuthController : ControllerBase
    {
        private readonly IDatabaseHelper _dbHelper;
        private readonly IAdminAuthRepository _adminAuthRepository;
        private readonly IRBACRepository _rbacRepository;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AdminAuthController> _logger;
        private const int MAX_FAILED_ATTEMPTS = 5;
        private const int LOCK_DURATION_MINUTES = 30;

        public AdminAuthController(
            IDatabaseHelper dbHelper,
            IAdminAuthRepository adminAuthRepository,
            IRBACRepository rbacRepository,
            ITokenService tokenService,
            ILogger<AdminAuthController> logger)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
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
                bool isPasswordValid = VerifyPassword(request.Password, admin.PasswordHash);

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
                    admin.Username,
                    admin.Role,
                    additionalClaims
                );
                    
                // SECURITY FIX: Set token as httpOnly cookie instead of returning in response
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,  // Prevents JavaScript access (XSS protection)
                    Secure = true,    // HTTPS only
                    SameSite = SameSiteMode.Strict,  // CSRF protection
                    Expires = DateTimeOffset.UtcNow.AddDays(7),  // 7-day expiry
                    Path = "/",
                    IsEssential = true
                };

                Response.Cookies.Append("adminToken", token, cookieOptions);

                // SECURITY: Do NOT include token in response body
                var response = new AdminLoginResponse
                {
                    AdminId = admin.AdminId,
                    Username = admin.Username,
                    Email = admin.Email,
                    FullName = admin.FullName,
                    Role = admin.Role,
                    // Token removed from response (now in httpOnly cookie)
                    LastLogin = admin.LastLogin ?? DateTime.UtcNow,
                    ProfilePhoto = admin.ProfilePhoto
                };

                return ApiResponseHelper.Success(response, "Login successful.");
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
                    permissions = permissionContext.IsSuperAdmin
                        ? new List<string> { "*" }  // Super admin gets wildcard permission
                        : permissionContext.Permissions
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
                    SameSite = SameSiteMode.Strict,
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

        /// <summary>
        /// Hash password using BCrypt (secure hashing with salt)
        /// </summary>
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        /// <summary>
        /// Verify password against stored hash - supports both BCrypt (new) and SHA256 (legacy)
        /// </summary>
        private bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
                return false;

            try
            {
                // Try BCrypt verification first (new secure method)
                if (storedHash.StartsWith("$2") || storedHash.StartsWith("$2a") || storedHash.StartsWith("$2b") || storedHash.StartsWith("$2y"))
                {
                    return BCrypt.Net.BCrypt.Verify(password, storedHash);
                }

                // Fall back to SHA256 for legacy support (will be phased out)
                // This allows gradual migration from SHA256 to BCrypt
                using (var sha256 = SHA256.Create())
                {
                    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    var sha256Hash = Convert.ToBase64String(hashedBytes);
                    return sha256Hash == storedHash;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Password verification failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Legacy SHA256 hash method - kept for migration purposes only
        /// DO NOT USE for new passwords - use HashPassword() instead
        /// </summary>
        [Obsolete("This method is deprecated. Use HashPassword() with BCrypt instead.")]
        private string HashPasswordLegacy(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
