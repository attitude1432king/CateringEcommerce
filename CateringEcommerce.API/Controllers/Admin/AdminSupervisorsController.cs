using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.Domain.Enums.Admin;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace CateringEcommerce.API.Controllers.Admin
{
    [Route("api/admin/supervisors")]
    [ApiController]
    [AdminAuthorize]
    public class AdminSupervisorsController : ControllerBase
    {
        private readonly IAdminSupervisorRepository _supervisorRepository;
        private readonly IAdminAuthRepository _adminAuthRepository;
        private readonly ILogger<AdminSupervisorsController> _logger;

        public AdminSupervisorsController(
            IAdminSupervisorRepository supervisorRepository,
            IAdminAuthRepository adminAuthRepository,
            ILogger<AdminSupervisorsController> logger)
        {
            _supervisorRepository = supervisorRepository ?? throw new ArgumentNullException(nameof(supervisorRepository));
            _adminAuthRepository = adminAuthRepository ?? throw new ArgumentNullException(nameof(adminAuthRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("registrations")]
        public IActionResult GetRegistrationRequests([FromQuery] AdminSupervisorRegistrationListRequest request)
        {
            try
            {
                var result = _supervisorRepository.GetRegistrationRequests(request);
                return ApiResponseHelper.Success(result, "Registration requests retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get supervisor registration requests");
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetSupervisorDetails(long id)
        {
            try
            {
                var result = _supervisorRepository.GetSupervisorDetails(id);
                if (result == null)
                    return NotFound(ApiResponseHelper.Failure("Supervisor not found."));
                return ApiResponseHelper.Success(result, "Supervisor details retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get supervisor details for ID: {SupervisorId}", id);
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPut("{id}/status")]
        public IActionResult UpdateSupervisorStatus(long id, [FromBody] AdminSupervisorStatusUpdate request)
        {
            try
            {
                var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                if (adminIdClaim == null || !long.TryParse(adminIdClaim.Value, out long adminId))
                {
                    return ApiResponseHelper.Failure("Invalid admin session.");
                }

                if (!Enum.IsDefined(typeof(SupervisorApprovalStatus), request.Status))
                {
                    return ApiResponseHelper.Failure("Invalid status value.");
                }

                request.SupervisorId = id;
                request.UpdatedBy = adminId;

                bool success = _supervisorRepository.UpdateSupervisorStatus(request);

                if (!success)
                    return ApiResponseHelper.Failure("Failed to update supervisor status.");

                string statusName = ((SupervisorApprovalStatus)request.Status).ToString();

                _adminAuthRepository.LogAdminActivity(adminId, "UPDATE_SUPERVISOR_STATUS", $"Updated supervisor {id} status to {statusName}");

                return ApiResponseHelper.Success(null, "Supervisor status updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update supervisor status for ID: {SupervisorId}", id);
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("active")]
        public IActionResult GetActiveSupervisors([FromQuery] AdminActiveSupervisorListRequest request)
        {
            try
            {
                var result = _supervisorRepository.GetActiveSupervisors(request);
                return ApiResponseHelper.Success(result, "Active supervisors retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get active supervisors");
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPut("{id}/block")]
        public IActionResult BlockSupervisor(long id, [FromBody] BlockSupervisorRequest request)
        {
            try
            {
                var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                if (adminIdClaim == null || !long.TryParse(adminIdClaim.Value, out long adminId))
                {
                    return ApiResponseHelper.Failure("Invalid admin session.");
                }

                bool success = _supervisorRepository.BlockSupervisor(id, adminId, request?.Reason);

                if (!success)
                    return ApiResponseHelper.Failure("Failed to block supervisor. Supervisor may not be active.");

                _adminAuthRepository.LogAdminActivity(adminId, "BLOCK_SUPERVISOR", $"Blocked supervisor {id}");

                return ApiResponseHelper.Success(null, "Supervisor blocked successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to block supervisor ID: {SupervisorId}", id);
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPut("{id}/unblock")]
        public IActionResult UnblockSupervisor(long id)
        {
            try
            {
                var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                if (adminIdClaim == null || !long.TryParse(adminIdClaim.Value, out long adminId))
                {
                    return ApiResponseHelper.Failure("Invalid admin session.");
                }

                bool success = _supervisorRepository.UnblockSupervisor(id, adminId);

                if (!success)
                    return ApiResponseHelper.Failure("Failed to unblock supervisor. Supervisor may not be blocked.");

                _adminAuthRepository.LogAdminActivity(adminId, "UNBLOCK_SUPERVISOR", $"Unblocked supervisor {id}");

                return ApiResponseHelper.Success(null, "Supervisor unblocked successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to unblock supervisor ID: {SupervisorId}", id);
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteSupervisor(long id)
        {
            try
            {
                var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                if (adminIdClaim == null || !long.TryParse(adminIdClaim.Value, out long adminId))
                {
                    return ApiResponseHelper.Failure("Invalid admin session.");
                }

                bool success = _supervisorRepository.DeleteSupervisor(id, adminId);

                if (!success)
                    return ApiResponseHelper.Failure("Failed to delete supervisor.");

                _adminAuthRepository.LogAdminActivity(adminId, "DELETE_SUPERVISOR", $"Deleted supervisor {id}");

                return ApiResponseHelper.Success(null, "Supervisor deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete supervisor ID: {SupervisorId}", id);
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost("{id}/restore")]
        public IActionResult RestoreSupervisor(long id)
        {
            try
            {
                var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                if (adminIdClaim == null || !long.TryParse(adminIdClaim.Value, out long adminId))
                {
                    return ApiResponseHelper.Failure("Invalid admin session.");
                }

                bool success = _supervisorRepository.RestoreSupervisor(id, adminId);

                if (!success)
                    return ApiResponseHelper.Failure("Failed to restore supervisor. It may not be deleted.");

                _adminAuthRepository.LogAdminActivity(adminId, "RESTORE_SUPERVISOR", $"Restored supervisor {id}");

                return ApiResponseHelper.Success(null, "Supervisor restored successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restore supervisor ID: {SupervisorId}", id);
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("export")]
        public IActionResult ExportSupervisors([FromQuery] AdminActiveSupervisorListRequest request)
        {
            try
            {
                var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                if (adminIdClaim == null || !long.TryParse(adminIdClaim.Value, out long adminId))
                {
                    return ApiResponseHelper.Failure("Invalid admin session.");
                }

                var supervisors = _supervisorRepository.GetSupervisorsForExport(request);

                var csv = new StringBuilder();
                csv.AppendLine("ID,Full Name,Email,Phone,City,State,Type,Rating,Events Supervised,Status,Created Date");

                foreach (var s in supervisors)
                {
                    csv.AppendLine($"{s.SupervisorId},{EscapeCsv(s.FullName)},{EscapeCsv(s.Email)},{EscapeCsv(s.Phone)},{EscapeCsv(s.City)},{EscapeCsv(s.State)},{s.SupervisorType},{s.AverageRating?.ToString("F1") ?? "N/A"},{s.TotalEventsSupervised},{s.CurrentStatus},{s.CreatedDate:yyyy-MM-dd}");
                }

                _adminAuthRepository.LogAdminActivity(adminId, "EXPORT_SUPERVISORS", $"Exported {supervisors.Count} supervisors");

                var bytes = Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", $"supervisors_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export supervisors");
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

    public class BlockSupervisorRequest
    {
        public string? Reason { get; set; }
    }
}
