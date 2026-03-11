using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace CateringEcommerce.API.Controllers.Admin
{
    [Route("api/admin/users")]
    [ApiController]
    [AdminAuthorize]
    public class AdminUsersController : ControllerBase
    {
        private readonly IAdminUserRepository _userRepository;
        private readonly IAdminAuthRepository _adminAuthRepository;
        private readonly ILogger<AdminUsersController> _logger;

        public AdminUsersController(
            IAdminUserRepository userRepository,
            IAdminAuthRepository adminAuthRepository,
            ILogger<AdminUsersController> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _adminAuthRepository = adminAuthRepository ?? throw new ArgumentNullException(nameof(adminAuthRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all users with filtering and pagination
        /// </summary>
        [HttpGet]
        public IActionResult GetAllUsers([FromQuery] AdminUserListRequest request)
        {
            try
            {
                var result = _userRepository.GetAllUsers(request);
                return ApiResponseHelper.Success(result, "Users retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve users");
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get user details by ID
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetUserById(long id)
        {
            try
            {
                var user = _userRepository.GetUserById(id);

                if (user == null)
                    return ApiResponseHelper.Failure("User not found.");

                return ApiResponseHelper.Success(user, "User details retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve user {UserId}", id);
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Block or unblock user
        /// </summary>
        [HttpPut("{id}/status")]
        public IActionResult UpdateUserStatus(long id, [FromBody] AdminUserStatusUpdate request)
        {
            try
            {
                var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                if (adminIdClaim == null || !long.TryParse(adminIdClaim.Value, out long adminId))
                {
                    return ApiResponseHelper.Failure("Invalid admin session.");
                }

                request.UserId = id;
                request.UpdatedBy = adminId;

                bool success = _userRepository.UpdateUserStatus(request);

                if (!success)
                    return ApiResponseHelper.Failure("Failed to update user status.");

                _adminAuthRepository.LogAdminActivity(adminId, "UPDATE_USER_STATUS", $"Updated user {id} - Blocked: {request.IsBlocked}");

                return ApiResponseHelper.Success(null, "User status updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user status for {UserId}", id);
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Soft delete a user
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(long id)
        {
            try
            {
                var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                if (adminIdClaim == null || !long.TryParse(adminIdClaim.Value, out long adminId))
                {
                    return ApiResponseHelper.Failure("Invalid admin session.");
                }

                bool success = _userRepository.SoftDeleteUser(id, adminId);

                if (!success)
                    return ApiResponseHelper.Failure("Failed to delete user. User may already be deleted.");

                _adminAuthRepository.LogAdminActivity(adminId, "DELETE_USER", $"Soft deleted user {id}");

                return ApiResponseHelper.Success(null, "User deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user {UserId}", id);
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Restore a soft-deleted user
        /// </summary>
        [HttpPost("{id}/restore")]
        public IActionResult RestoreUser(long id)
        {
            try
            {
                var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                if (adminIdClaim == null || !long.TryParse(adminIdClaim.Value, out long adminId))
                {
                    return ApiResponseHelper.Failure("Invalid admin session.");
                }

                bool success = _userRepository.RestoreUser(id, adminId);

                if (!success)
                    return ApiResponseHelper.Failure("Failed to restore user. User may not be deleted.");

                _adminAuthRepository.LogAdminActivity(adminId, "RESTORE_USER", $"Restored user {id}");

                return ApiResponseHelper.Success(null, "User restored successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restore user {UserId}", id);
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Export user list as CSV
        /// </summary>
        [HttpGet("export")]
        public IActionResult ExportUsers([FromQuery] AdminUserListRequest request)
        {
            try
            {
                var users = _userRepository.GetUsersForExport(request);

                var csv = new StringBuilder();
                csv.AppendLine("User ID,Full Name,Phone,Email,City,State,Status,Blocked,Total Orders,Total Spent,Registered Date,Last Login");

                foreach (var user in users)
                {
                    csv.AppendLine($"{user.UserId}," +
                        $"\"{EscapeCsv(user.FullName)}\"," +
                        $"\"{EscapeCsv(user.Phone)}\"," +
                        $"\"{EscapeCsv(user.Email ?? "")}\"," +
                        $"\"{EscapeCsv(user.CityName ?? "")}\"," +
                        $"\"{EscapeCsv(user.StateName ?? "")}\"," +
                        $"{(user.IsActive ? "Active" : "Inactive")}," +
                        $"{(user.IsBlocked ? "Yes" : "No")}," +
                        $"{user.TotalOrders}," +
                        $"{user.TotalSpent:F2}," +
                        $"{user.CreatedDate:yyyy-MM-dd}," +
                        $"{(user.LastLogin.HasValue ? user.LastLogin.Value.ToString("yyyy-MM-dd") : "Never")}");
                }

                var bytes = Encoding.UTF8.GetBytes(csv.ToString());
                var fileName = $"users_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export users");
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        private static string EscapeCsv(string value)
        {
            return value.Replace("\"", "\"\"");
        }
    }
}
