using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.Admin;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.AspNetCore.Mvc;

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
                _logger.LogError(ex, "Failed to process user request");
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
                _logger.LogError(ex, "Failed to process user request");
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

                // Log activity
                _adminAuthRepository.LogAdminActivity(adminId, "UPDATE_USER_STATUS", $"Updated user {id} - Blocked: {request.IsBlocked}");

                return ApiResponseHelper.Success(null, "User status updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process user request");
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }
    }
}
