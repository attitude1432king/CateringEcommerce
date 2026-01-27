using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Common.Admin;
using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace CateringEcommerce.API.Controllers.Admin
{
    [Route("api/admin/auth")]
    [ApiController]
    public class AdminAuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly string _connStr;
        private readonly TokenService _tokenService;
        private const int MAX_FAILED_ATTEMPTS = 5;
        private const int LOCK_DURATION_MINUTES = 30;

        public AdminAuthController(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _connStr = _config.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
            _tokenService = new TokenService(config);
        }

        /// <summary>
        /// Admin Login
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] AdminLoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                    return ApiResponseHelper.Failure("Username and password are required.");

                var repository = new AdminAuthRepository(_connStr);

                // Check if account is locked
                if (repository.IsAccountLocked(request.Username))
                {
                    return ApiResponseHelper.Failure("Account is locked due to multiple failed login attempts. Please try again later.");
                }

                // Hash the password
                string passwordHash = HashPassword(request.Password);

                // Authenticate admin
                var admin = repository.AuthenticateAdmin(request.Username, passwordHash);

                if (admin == null)
                {
                    // Increment failed login attempts
                    repository.IncrementFailedLoginAttempts(request.Username);

                    // Check if we should lock the account
                    var adminData = repository.GetAdminById(0); // This needs improvement - we should get by username
                    // For now, just return error
                    return ApiResponseHelper.Failure("Invalid username or password.");
                }

                // Check if max attempts exceeded and lock account
                if (admin.FailedLoginAttempts >= MAX_FAILED_ATTEMPTS)
                {
                    repository.LockAccount(request.Username, DateTime.UtcNow.AddMinutes(LOCK_DURATION_MINUTES));
                    return ApiResponseHelper.Failure("Account locked due to multiple failed attempts. Please try again in 30 minutes.");
                }

                // Reset failed attempts on successful login
                repository.ResetFailedLoginAttempts(admin.AdminId);

                // Update last login
                repository.UpdateLastLogin(admin.AdminId);

                // Log activity
                repository.LogAdminActivity(admin.AdminId, "LOGIN", "Admin logged in successfully");

                // Generate JWT token
                string token = _tokenService.GenerateToken(
                    admin.Username,
                    admin.Role,
                    admin.AdminId.ToString(),
                    admin.Email
                );

                var response = new AdminLoginResponse
                {
                    AdminId = admin.AdminId,
                    Username = admin.Username,
                    Email = admin.Email,
                    FullName = admin.FullName,
                    Role = admin.Role,
                    Token = token,
                    LastLogin = admin.LastLogin ?? DateTime.UtcNow,
                    ProfilePhoto = admin.ProfilePhoto
                };

                return ApiResponseHelper.Success(response, "Login successful.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
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

                var repository = new AdminAuthRepository(_connStr);
                var admin = repository.GetAdminById(adminId);

                if (admin == null)
                    return ApiResponseHelper.Failure("Admin not found.");

                return ApiResponseHelper.Success(admin, "Admin details retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Admin Logout (optional - mainly for logging purposes)
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
                    var repository = new AdminAuthRepository(_connStr);
                    repository.LogAdminActivity(adminId, "LOGOUT", "Admin logged out");
                }

                return ApiResponseHelper.Success(null, "Logout successful.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
