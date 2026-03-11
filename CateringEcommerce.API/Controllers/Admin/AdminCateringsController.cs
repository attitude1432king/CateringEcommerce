using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.Domain.Enums.Admin;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace CateringEcommerce.API.Controllers.Admin
{
    [Route("api/admin/caterings")]
    [ApiController]
    [AdminAuthorize]
    public class AdminCateringsController : ControllerBase
    {
        private readonly IAdminCateringRepository _cateringRepository;
        private readonly IAdminAuthRepository _adminAuthRepository;
        private readonly ILogger<AdminCateringsController> _logger;

        public AdminCateringsController(
            IAdminCateringRepository cateringRepository,
            IAdminAuthRepository adminAuthRepository,
            ILogger<AdminCateringsController> logger)
        {
            _cateringRepository = cateringRepository ?? throw new ArgumentNullException(nameof(cateringRepository));
            _adminAuthRepository = adminAuthRepository ?? throw new ArgumentNullException(nameof(adminAuthRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all caterings with filtering and pagination
        /// </summary>
        [HttpGet]
        public IActionResult GetAllCaterings([FromQuery] AdminCateringListRequest request)
        {
            try
            {
                var result = _cateringRepository.GetAllCaterings(request);
                return ApiResponseHelper.Success(result, "Caterings retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all caterings");
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
                var catering = _cateringRepository.GetCateringById(id);

                if (catering == null)
                    return ApiResponseHelper.Failure("Catering not found.");

                return ApiResponseHelper.Success(catering, "Catering details retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get catering by ID: {CateringId}", id);
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update catering status (Approve/Reject/Block/UnderReview)
        /// Status enum: 1=Pending, 2=Approved, 3=Rejected, 4=UnderReview, 5=InfoRequested
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

                bool success = _cateringRepository.UpdateCateringStatus(request);

                if (!success)
                    return ApiResponseHelper.Failure("Failed to update catering status.");

                string statusName = Enum.IsDefined(typeof(ApprovalStatus), request.Status)
                    ? ((ApprovalStatus)request.Status).ToString()
                    : request.Status.ToString();

                _adminAuthRepository.LogAdminActivity(adminId, "UPDATE_CATERING_STATUS", $"Updated catering {id} status to {statusName}");

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

                bool success = _cateringRepository.DeleteCatering(id, adminId);

                if (!success)
                    return ApiResponseHelper.Failure("Failed to delete catering.");

                _adminAuthRepository.LogAdminActivity(adminId, "DELETE_CATERING", $"Deleted catering {id}");

                return ApiResponseHelper.Success(null, "Catering deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Restore a soft-deleted catering
        /// </summary>
        [HttpPost("{id}/restore")]
        public IActionResult RestoreCatering(long id)
        {
            try
            {
                var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                if (adminIdClaim == null || !long.TryParse(adminIdClaim.Value, out long adminId))
                {
                    return ApiResponseHelper.Failure("Invalid admin session.");
                }

                bool success = _cateringRepository.RestoreCatering(id, adminId);

                if (!success)
                    return ApiResponseHelper.Failure("Failed to restore catering. It may not be deleted.");

                _adminAuthRepository.LogAdminActivity(adminId, "RESTORE_CATERING", $"Restored catering {id}");

                return ApiResponseHelper.Success(null, "Catering restored successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Export caterings list as CSV
        /// </summary>
        [HttpGet("export")]
        public IActionResult ExportCaterings([FromQuery] AdminCateringListRequest request)
        {
            try
            {
                var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                if (adminIdClaim == null || !long.TryParse(adminIdClaim.Value, out long adminId))
                {
                    return ApiResponseHelper.Failure("Invalid admin session.");
                }

                var caterings = _cateringRepository.GetCateringsForExport(request);

                var csv = new StringBuilder();
                csv.AppendLine("ID,Business Name,Owner Name,Phone,Email,City,State,Status,Verified,Active,Blocked,Rating,Reviews,Orders,Earnings,Created Date");

                foreach (var c in caterings)
                {
                    string statusName = Enum.IsDefined(typeof(ApprovalStatus), c.Status)
                        ? ((ApprovalStatus)c.Status).ToString()
                        : c.Status.ToString();

                    csv.AppendLine($"{c.CateringId},{EscapeCsv(c.BusinessName)},{EscapeCsv(c.OwnerName)},{EscapeCsv(c.Phone)},{EscapeCsv(c.Email)},{EscapeCsv(c.City)},{EscapeCsv(c.State)},{statusName},{(c.IsVerified ? "Yes" : "No")},{(c.IsActive ? "Yes" : "No")},{(c.IsBlocked ? "Yes" : "No")},{c.Rating?.ToString("F1") ?? "N/A"},{c.TotalReviews},{c.TotalOrders},{c.TotalEarnings:F2},{c.CreatedDate:yyyy-MM-dd}");
                }

                _adminAuthRepository.LogAdminActivity(adminId, "EXPORT_CATERINGS", $"Exported {caterings.Count} caterings");

                var bytes = Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", $"caterings_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export caterings");
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }
            return value;
        }
    }
}
