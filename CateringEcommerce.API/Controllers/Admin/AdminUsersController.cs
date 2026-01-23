using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Common.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.AspNetCore.Mvc;

namespace CateringEcommerce.API.Controllers.Admin
{
    [Route("api/admin/users")]
    [ApiController]
    [AdminAuthorize]
    public class AdminUsersController : ControllerBase
    {
        private readonly string _connStr;

        public AdminUsersController(IConfiguration config)
        {
            _connStr = config.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
        }

        /// <summary>
        /// Get all users with filtering and pagination
        /// </summary>
        [HttpGet]
        public IActionResult GetAllUsers([FromQuery] AdminUserListRequest request)
        {
            try
            {
                var repository = new AdminUserRepository(_connStr);
                var result = repository.GetAllUsers(request);
                return ApiResponseHelper.Success(result, "Users retrieved successfully.");
            }
            catch (Exception ex)
            {
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
                var repository = new AdminUserRepository(_connStr);
                var user = repository.GetUserById(id);

                if (user == null)
                    return ApiResponseHelper.Failure("User not found.");

                return ApiResponseHelper.Success(user, "User details retrieved successfully.");
            }
            catch (Exception ex)
            {
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

                var repository = new AdminUserRepository(_connStr);
                bool success = repository.UpdateUserStatus(request);

                if (!success)
                    return ApiResponseHelper.Failure("Failed to update user status.");

                // Log activity
                var authRepo = new AdminAuthRepository(_connStr);
                authRepo.LogAdminActivity(adminId, "UPDATE_USER_STATUS", $"Updated user {id} - Blocked: {request.IsBlocked}");

                return ApiResponseHelper.Success(null, "User status updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }
    }
}
