using CateringEcommerce.API.Helpers;
using CateringEcommerce.Domain.Interfaces.Supervisor;
using CateringEcommerce.Domain.Models.Supervisor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CateringEcommerce.API.Controllers.Supervisor
{
    /// <summary>
    /// Supervisor Management Controller
    /// Admin: CRUD, authority management, status management, analytics
    /// Supervisor: Self-service portal (dashboard, profile, availability)
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/Supervisor/[controller]")]
    public class SupervisorManagementController : ControllerBase
    {
        private readonly ILogger<SupervisorManagementController> _logger;
        private readonly ISupervisorRepository _supervisorRepo;

        public SupervisorManagementController(
            ILogger<SupervisorManagementController> logger,
            ISupervisorRepository supervisorRepo)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _supervisorRepo = supervisorRepo ?? throw new ArgumentNullException(nameof(supervisorRepo));
        }

        #region Helper Methods

        private long GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("UserId")?.Value;

            if (long.TryParse(userIdClaim, out long userId))
            {
                return userId;
            }
            return 0;
        }

        #endregion

        // =============================================
        // SUPERVISOR SELF-SERVICE PORTAL
        // =============================================

        /// <summary>
        /// GET: api/Supervisor/SupervisorManagement/dashboard
        /// Get supervisor's own dashboard (stats, assignments, earnings)
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetMyDashboard()
        {
            try
            {
                long supervisorId = GetUserId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                _logger.LogInformation("Supervisor {SupervisorId} fetching dashboard", supervisorId);

                var dashboard = await _supervisorRepo.GetSupervisorDashboardAsync(supervisorId);

                if (dashboard == null)
                {
                    return ApiResponseHelper.Failure("Dashboard data not found.");
                }

                return ApiResponseHelper.Success(dashboard, "Dashboard loaded successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching supervisor dashboard");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while loading dashboard."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/SupervisorManagement/profile
        /// Get supervisor's own profile
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                long supervisorId = GetUserId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                var supervisor = await _supervisorRepo.GetSupervisorByIdAsync(supervisorId);

                if (supervisor == null)
                {
                    return ApiResponseHelper.Failure("Supervisor profile not found.");
                }

                return ApiResponseHelper.Success(supervisor, "Profile retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching supervisor profile");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching profile."));
            }
        }

        /// <summary>
        /// PUT: api/Supervisor/SupervisorManagement/profile
        /// Update supervisor's own profile
        /// </summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateSupervisorDto updates)
        {
            try
            {
                long supervisorId = GetUserId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                _logger.LogInformation("Supervisor {SupervisorId} updating profile", supervisorId);

                var success = await _supervisorRepo.UpdateSupervisorAsync(supervisorId, updates);

                if (success)
                {
                    return ApiResponseHelper.Success(null, "Profile updated successfully.");
                }

                return ApiResponseHelper.Failure("Failed to update profile.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supervisor profile");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while updating profile."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/SupervisorManagement/authority
        /// Check supervisor's own authority level and permissions
        /// </summary>
        [HttpGet("authority")]
        public async Task<IActionResult> CheckMyAuthority([FromQuery] string actionType = "GENERAL")
        {
            try
            {
                long supervisorId = GetUserId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                var authorityCheck = await _supervisorRepo.CheckSupervisorAuthorityAsync(supervisorId, actionType);

                return ApiResponseHelper.Success(authorityCheck, "Authority check completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking supervisor authority");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while checking authority."));
            }
        }

        // =============================================
        // SUPERVISOR AVAILABILITY & SCHEDULING
        // =============================================

        /// <summary>
        /// PUT: api/Supervisor/SupervisorManagement/availability
        /// Update supervisor's availability schedule
        /// </summary>
        [HttpPut("availability")]
        public async Task<IActionResult> UpdateAvailability([FromBody] List<AvailabilitySlot> availability)
        {
            try
            {
                long supervisorId = GetUserId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                if (availability == null || availability.Count == 0)
                {
                    return ApiResponseHelper.Failure("At least one availability slot is required.");
                }

                _logger.LogInformation("Supervisor {SupervisorId} updating availability with {Count} slots", supervisorId, availability.Count);

                var success = await _supervisorRepo.UpdateAvailabilityAsync(supervisorId, availability);

                if (success)
                {
                    return ApiResponseHelper.Success(null, "Availability updated successfully.");
                }

                return ApiResponseHelper.Failure("Failed to update availability.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating availability");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while updating availability."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/SupervisorManagement/availability
        /// Get supervisor's availability for a specific date
        /// </summary>
        [HttpGet("availability")]
        public async Task<IActionResult> GetMyAvailability([FromQuery] DateTime? date = null)
        {
            try
            {
                long supervisorId = GetUserId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                var targetDate = date ?? DateTime.UtcNow;
                var slots = await _supervisorRepo.GetAvailabilityAsync(supervisorId, targetDate);

                return ApiResponseHelper.Success(slots, "Availability retrieved.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching availability");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching availability."));
            }
        }

        // =============================================
        // ADMIN - SUPERVISOR CRUD
        // =============================================

        /// <summary>
        /// GET: api/Supervisor/SupervisorManagement/admin/all
        /// Get all supervisors with optional type/status filter
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAllSupervisors([FromQuery] SupervisorType? type = null, [FromQuery] string status = null)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                _logger.LogInformation("Admin {AdminId} fetching supervisors. Type: {Type}, Status: {Status}", adminId, type?.ToString() ?? "ALL", status ?? "ALL");

                var supervisors = await _supervisorRepo.GetAllSupervisorsAsync(type, status);

                return ApiResponseHelper.Success(supervisors, $"Found {supervisors.Count} supervisor(s).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all supervisors");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching supervisors."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/SupervisorManagement/admin/{supervisorId}
        /// Get supervisor details by ID
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/{supervisorId}")]
        public async Task<IActionResult> GetSupervisorById(long supervisorId)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                var supervisor = await _supervisorRepo.GetSupervisorByIdAsync(supervisorId);

                if (supervisor == null)
                {
                    return ApiResponseHelper.Failure("Supervisor not found.");
                }

                return ApiResponseHelper.Success(supervisor, "Supervisor details retrieved.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching supervisor {SupervisorId}", supervisorId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching supervisor details."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/SupervisorManagement/admin/dashboard/{supervisorId}
        /// Get dashboard data for a specific supervisor (admin view)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/dashboard/{supervisorId}")]
        public async Task<IActionResult> GetSupervisorDashboard(long supervisorId)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                var dashboard = await _supervisorRepo.GetSupervisorDashboardAsync(supervisorId);

                if (dashboard == null)
                {
                    return ApiResponseHelper.Failure("Supervisor dashboard not found.");
                }

                return ApiResponseHelper.Success(dashboard, "Supervisor dashboard retrieved.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard for supervisor {SupervisorId}", supervisorId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching supervisor dashboard."));
            }
        }

        /// <summary>
        /// PUT: api/Supervisor/SupervisorManagement/admin/{supervisorId}
        /// Admin updates supervisor details
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("admin/{supervisorId}")]
        public async Task<IActionResult> UpdateSupervisor(long supervisorId, [FromBody] UpdateSupervisorDto updates)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                _logger.LogInformation("Admin {AdminId} updating supervisor {SupervisorId}", adminId, supervisorId);

                var success = await _supervisorRepo.UpdateSupervisorAsync(supervisorId, updates);

                if (success)
                {
                    return ApiResponseHelper.Success(null, "Supervisor updated successfully.");
                }

                return ApiResponseHelper.Failure("Failed to update supervisor.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supervisor {SupervisorId}", supervisorId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while updating supervisor."));
            }
        }

        /// <summary>
        /// DELETE: api/Supervisor/SupervisorManagement/admin/{supervisorId}
        /// Admin soft-deletes a supervisor
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("admin/{supervisorId}")]
        public async Task<IActionResult> DeleteSupervisor(long supervisorId)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                _logger.LogInformation("Admin {AdminId} deleting supervisor {SupervisorId}", adminId, supervisorId);

                var success = await _supervisorRepo.DeleteSupervisorAsync(supervisorId);

                if (success)
                {
                    _logger.LogInformation("Supervisor {SupervisorId} deleted by admin {AdminId}", supervisorId, adminId);
                    return ApiResponseHelper.Success(null, "Supervisor deleted successfully.");
                }

                return ApiResponseHelper.Failure("Failed to delete supervisor.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting supervisor {SupervisorId}", supervisorId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while deleting supervisor."));
            }
        }

        // =============================================
        // ADMIN - AUTHORITY MANAGEMENT
        // =============================================

        /// <summary>
        /// GET: api/Supervisor/SupervisorManagement/admin/check-authority/{supervisorId}
        /// Check a supervisor's authority for a specific action
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/check-authority/{supervisorId}")]
        public async Task<IActionResult> CheckSupervisorAuthority(long supervisorId, [FromQuery] string actionType)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                if (string.IsNullOrWhiteSpace(actionType))
                {
                    return ApiResponseHelper.Failure("Action type is required.");
                }

                var result = await _supervisorRepo.CheckSupervisorAuthorityAsync(supervisorId, actionType);

                return ApiResponseHelper.Success(result, "Authority check completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking authority for supervisor {SupervisorId}", supervisorId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while checking authority."));
            }
        }

        /// <summary>
        /// PUT: api/Supervisor/SupervisorManagement/admin/authority
        /// Update supervisor's authority level
        /// Only upgrades: BASIC → INTERMEDIATE → ADVANCED → FULL
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("admin/authority")]
        public async Task<IActionResult> UpdateAuthorityLevel([FromBody] UpdateAuthorityRequest request)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                if (string.IsNullOrWhiteSpace(request.Reason))
                {
                    return ApiResponseHelper.Failure("Reason for authority change is required.");
                }

                _logger.LogInformation("Admin {AdminId} updating authority for supervisor {SupervisorId} to {Level}. Reason: {Reason}",
                    adminId, request.SupervisorId, request.NewLevel, request.Reason);

                var success = await _supervisorRepo.UpdateAuthorityLevelAsync(
                    request.SupervisorId, request.NewLevel, adminId, request.Reason);

                if (success)
                {
                    _logger.LogInformation("Authority updated for supervisor {SupervisorId} to {Level}", request.SupervisorId, request.NewLevel);
                    return ApiResponseHelper.Success(null, $"Authority level updated to {request.NewLevel}.");
                }

                return ApiResponseHelper.Failure("Failed to update authority level. Only upgrades are allowed.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Update authority failed: {Message}", ex.Message);
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating authority for supervisor {SupervisorId}", request.SupervisorId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while updating authority level."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/SupervisorManagement/admin/grant-permission
        /// Grant a specific permission to supervisor
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/grant-permission")]
        public async Task<IActionResult> GrantPermission([FromBody] PermissionRequest request)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                _logger.LogInformation("Admin {AdminId} granting permission {Permission} to supervisor {SupervisorId}",
                    adminId, request.PermissionType, request.SupervisorId);

                var success = await _supervisorRepo.GrantPermissionAsync(request.SupervisorId, request.PermissionType, adminId);

                if (success)
                {
                    return ApiResponseHelper.Success(null, $"Permission '{request.PermissionType}' granted successfully.");
                }

                return ApiResponseHelper.Failure("Failed to grant permission.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error granting permission to supervisor {SupervisorId}", request.SupervisorId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while granting permission."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/SupervisorManagement/admin/revoke-permission
        /// Revoke a specific permission from supervisor
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/revoke-permission")]
        public async Task<IActionResult> RevokePermission([FromBody] PermissionRequest request)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                _logger.LogInformation("Admin {AdminId} revoking permission {Permission} from supervisor {SupervisorId}",
                    adminId, request.PermissionType, request.SupervisorId);

                var success = await _supervisorRepo.RevokePermissionAsync(request.SupervisorId, request.PermissionType, adminId);

                if (success)
                {
                    return ApiResponseHelper.Success(null, $"Permission '{request.PermissionType}' revoked successfully.");
                }

                return ApiResponseHelper.Failure("Failed to revoke permission.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking permission from supervisor {SupervisorId}", request.SupervisorId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while revoking permission."));
            }
        }

        // =============================================
        // ADMIN - STATUS MANAGEMENT
        // =============================================

        /// <summary>
        /// POST: api/Supervisor/SupervisorManagement/admin/activate
        /// Activate a supervisor
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/activate")]
        public async Task<IActionResult> ActivateSupervisor([FromBody] SupervisorStatusRequest request)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                _logger.LogInformation("Admin {AdminId} activating supervisor {SupervisorId}", adminId, request.SupervisorId);

                var success = await _supervisorRepo.ActivateSupervisorAsync(request.SupervisorId, adminId);

                if (success)
                {
                    return ApiResponseHelper.Success(null, "Supervisor activated successfully.");
                }

                return ApiResponseHelper.Failure("Failed to activate supervisor. Ensure all requirements are met.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Activation failed: {Message}", ex.Message);
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating supervisor {SupervisorId}", request.SupervisorId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while activating supervisor."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/SupervisorManagement/admin/suspend
        /// Suspend a supervisor (temporary)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/suspend")]
        public async Task<IActionResult> SuspendSupervisor([FromBody] SupervisorStatusRequest request)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                if (string.IsNullOrWhiteSpace(request.Reason))
                {
                    return ApiResponseHelper.Failure("Suspension reason is required.");
                }

                _logger.LogInformation("Admin {AdminId} suspending supervisor {SupervisorId}. Reason: {Reason}",
                    adminId, request.SupervisorId, request.Reason);

                var success = await _supervisorRepo.SuspendSupervisorAsync(request.SupervisorId, adminId, request.Reason);

                if (success)
                {
                    _logger.LogInformation("Supervisor {SupervisorId} suspended by admin {AdminId}", request.SupervisorId, adminId);
                    return ApiResponseHelper.Success(null, "Supervisor suspended successfully.");
                }

                return ApiResponseHelper.Failure("Failed to suspend supervisor.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suspending supervisor {SupervisorId}", request.SupervisorId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while suspending supervisor."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/SupervisorManagement/admin/terminate
        /// Terminate a supervisor (permanent)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/terminate")]
        public async Task<IActionResult> TerminateSupervisor([FromBody] SupervisorStatusRequest request)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                if (string.IsNullOrWhiteSpace(request.Reason))
                {
                    return ApiResponseHelper.Failure("Termination reason is required.");
                }

                _logger.LogInformation("Admin {AdminId} terminating supervisor {SupervisorId}. Reason: {Reason}",
                    adminId, request.SupervisorId, request.Reason);

                var success = await _supervisorRepo.TerminateSupervisorAsync(request.SupervisorId, adminId, request.Reason);

                if (success)
                {
                    _logger.LogInformation("Supervisor {SupervisorId} terminated by admin {AdminId}", request.SupervisorId, adminId);
                    return ApiResponseHelper.Success(null, "Supervisor terminated successfully.");
                }

                return ApiResponseHelper.Failure("Failed to terminate supervisor.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error terminating supervisor {SupervisorId}", request.SupervisorId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while terminating supervisor."));
            }
        }

        /// <summary>
        /// PUT: api/Supervisor/SupervisorManagement/admin/update-status
        /// Generic status update for supervisor
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("admin/update-status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateSupervisorStatusRequest request)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                _logger.LogInformation("Admin {AdminId} updating supervisor {SupervisorId} status to {Status}",
                    adminId, request.SupervisorId, request.NewStatus);

                var success = await _supervisorRepo.UpdateStatusAsync(
                    request.SupervisorId, request.NewStatus, adminId, request.Notes);

                if (success)
                {
                    return ApiResponseHelper.Success(null, $"Supervisor status updated to {request.NewStatus}.");
                }

                return ApiResponseHelper.Failure("Failed to update supervisor status.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Update status failed: {Message}", ex.Message);
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for supervisor {SupervisorId}", request.SupervisorId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while updating status."));
            }
        }

        // =============================================
        // ADMIN - SEARCH & ANALYTICS
        // =============================================

        /// <summary>
        /// POST: api/Supervisor/SupervisorManagement/admin/search
        /// Search supervisors with filters
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/search")]
        public async Task<IActionResult> SearchSupervisors([FromBody] SupervisorSearchDto filters)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                var supervisors = await _supervisorRepo.SearchSupervisorsAsync(filters);

                return ApiResponseHelper.Success(supervisors, $"Found {supervisors.Count} supervisor(s).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching supervisors");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while searching supervisors."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/SupervisorManagement/admin/by-zone/{zoneId}
        /// Get supervisors by zone
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/by-zone/{zoneId}")]
        public async Task<IActionResult> GetSupervisorsByZone(long zoneId)
        {
            try
            {
                var supervisors = await _supervisorRepo.GetSupervisorsByZoneAsync(zoneId);

                return ApiResponseHelper.Success(supervisors, $"Found {supervisors.Count} supervisor(s) in zone.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching supervisors for zone {ZoneId}", zoneId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching supervisors by zone."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/SupervisorManagement/admin/by-authority/{level}
        /// Get supervisors by authority level
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/by-authority/{level}")]
        public async Task<IActionResult> GetSupervisorsByAuthority(AuthorityLevel level)
        {
            try
            {
                var supervisors = await _supervisorRepo.GetSupervisorsByAuthorityAsync(level);

                return ApiResponseHelper.Success(supervisors, $"Found {supervisors.Count} supervisor(s) with {level} authority.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching supervisors by authority {Level}", level);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching supervisors by authority."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/SupervisorManagement/admin/available
        /// Find available supervisors for a specific event date/type
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/available")]
        public async Task<IActionResult> GetAvailableSupervisors(
            [FromQuery] DateTime eventDate,
            [FromQuery] string eventType,
            [FromQuery] long? zoneId = null)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                _logger.LogInformation("Admin {AdminId} finding available supervisors for {EventType} on {Date}",
                    adminId, eventType, eventDate.ToString("yyyy-MM-dd"));

                var supervisors = await _supervisorRepo.GetAvailableSupervisorsAsync(eventDate, eventType, zoneId);

                return ApiResponseHelper.Success(supervisors, $"Found {supervisors.Count} available supervisor(s).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding available supervisors");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while finding available supervisors."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/SupervisorManagement/admin/statistics
        /// Get overall supervisor statistics
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/statistics")]
        public async Task<IActionResult> GetSupervisorStatistics([FromQuery] SupervisorType? type = null)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                var statistics = await _supervisorRepo.GetSupervisorStatisticsAsync(type);

                return ApiResponseHelper.Success(statistics, "Supervisor statistics retrieved.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching supervisor statistics");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching statistics."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/SupervisorManagement/admin/performance
        /// Get supervisor performance report for a date range
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/performance")]
        public async Task<IActionResult> GetPerformanceReport([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                if (fromDate > toDate)
                {
                    return ApiResponseHelper.Failure("From date must be before to date.");
                }

                _logger.LogInformation("Admin {AdminId} fetching performance report from {From} to {To}",
                    adminId, fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd"));

                var report = await _supervisorRepo.GetSupervisorPerformanceReportAsync(fromDate, toDate);

                return ApiResponseHelper.Success(report, $"Performance report for {report.Count} supervisor(s).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching performance report");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching performance report."));
            }
        }
    }

    #region Management Request Models

    public class UpdateAuthorityRequest
    {
        public long SupervisorId { get; set; }
        public AuthorityLevel NewLevel { get; set; }
        public string Reason { get; set; }
    }

    public class PermissionRequest
    {
        public long SupervisorId { get; set; }
        public string PermissionType { get; set; }
    }

    public class SupervisorStatusRequest
    {
        public long SupervisorId { get; set; }
        public string Reason { get; set; }
    }

    public class UpdateSupervisorStatusRequest
    {
        public long SupervisorId { get; set; }
        public string NewStatus { get; set; }
        public string Notes { get; set; }
    }

    #endregion
}
