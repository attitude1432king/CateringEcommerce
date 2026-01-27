using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Common.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.AspNetCore.Mvc;

namespace CateringEcommerce.API.Controllers.Admin
{
    [Route("api/admin/caterings")]
    [ApiController]
    [AdminAuthorize]
    public class AdminCateringsController : ControllerBase
    {
        private readonly string _connStr;

        public AdminCateringsController(IConfiguration config)
        {
            _connStr = config.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
        }

        /// <summary>
        /// Get all caterings with filtering and pagination
        /// </summary>
        [HttpGet]
        public IActionResult GetAllCaterings([FromQuery] AdminCateringListRequest request)
        {
            try
            {
                var repository = new AdminCateringRepository(_connStr);
                var result = repository.GetAllCaterings(request);
                return ApiResponseHelper.Success(result, "Caterings retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get catering details by ID
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetCateringById(long id)
        {
            try
            {
                var repository = new AdminCateringRepository(_connStr);
                var catering = repository.GetCateringById(id);

                if (catering == null)
                    return ApiResponseHelper.Failure("Catering not found.");

                return ApiResponseHelper.Success(catering, "Catering details retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update catering status (Approve/Reject/Block/Activate)
        /// </summary>
        [HttpPut("{id}/status")]
        public IActionResult UpdateCateringStatus(long id, [FromBody] AdminCateringStatusUpdate request)
        {
            try
            {
                var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                if (adminIdClaim == null || !long.TryParse(adminIdClaim.Value, out long adminId))
                {
                    return ApiResponseHelper.Failure("Invalid admin session.");
                }

                request.CateringId = id;
                request.UpdatedBy = adminId;

                var repository = new AdminCateringRepository(_connStr);
                bool success = repository.UpdateCateringStatus(request);

                if (!success)
                    return ApiResponseHelper.Failure("Failed to update catering status.");

                // Log activity
                var authRepo = new AdminAuthRepository(_connStr);
                authRepo.LogAdminActivity(adminId, "UPDATE_CATERING_STATUS", $"Updated catering {id} status to {request.Status}");

                return ApiResponseHelper.Success(null, "Catering status updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete catering (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult DeleteCatering(long id)
        {
            try
            {
                var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                if (adminIdClaim == null || !long.TryParse(adminIdClaim.Value, out long adminId))
                {
                    return ApiResponseHelper.Failure("Invalid admin session.");
                }

                var repository = new AdminCateringRepository(_connStr);
                bool success = repository.DeleteCatering(id, adminId);

                if (!success)
                    return ApiResponseHelper.Failure("Failed to delete catering.");

                // Log activity
                var authRepo = new AdminAuthRepository(_connStr);
                authRepo.LogAdminActivity(adminId, "DELETE_CATERING", $"Deleted catering {id}");

                return ApiResponseHelper.Success(null, "Catering deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }
    }
}
