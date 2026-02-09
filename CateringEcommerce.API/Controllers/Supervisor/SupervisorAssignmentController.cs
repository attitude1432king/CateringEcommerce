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
    /// Supervisor Assignment Controller
    /// Handles event assignment management: creation, acceptance, check-in, completion, payment release
    /// Split into Supervisor actions and Admin actions
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/Supervisor/[controller]")]
    public class SupervisorAssignmentController : ControllerBase
    {
        private readonly ILogger<SupervisorAssignmentController> _logger;
        private readonly ISupervisorAssignmentRepository _assignmentRepo;

        public SupervisorAssignmentController(
            ILogger<SupervisorAssignmentController> logger,
            ISupervisorAssignmentRepository assignmentRepo)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _assignmentRepo = assignmentRepo ?? throw new ArgumentNullException(nameof(assignmentRepo));
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
        // SUPERVISOR SELF-SERVICE ENDPOINTS
        // =============================================

        /// <summary>
        /// GET: api/Supervisor/SupervisorAssignment/my-assignments
        /// Get all assignments for the logged-in supervisor
        /// </summary>
        [HttpGet("my-assignments")]
        public async Task<IActionResult> GetMyAssignments([FromQuery] string status = null)
        {
            try
            {
                long supervisorId = GetUserId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                _logger.LogInformation("Supervisor {SupervisorId} fetching assignments with status filter: {Status}", supervisorId, status ?? "ALL");

                var assignments = await _assignmentRepo.GetAssignmentsBySupervisorAsync(supervisorId, status);

                return ApiResponseHelper.Success(assignments, "Assignments retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching assignments for supervisor");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching assignments."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/SupervisorAssignment/{assignmentId}
        /// Get single assignment details
        /// </summary>
        [HttpGet("{assignmentId}")]
        public async Task<IActionResult> GetAssignment(long assignmentId)
        {
            try
            {
                long userId = GetUserId();
                if (userId <= 0)
                {
                    return ApiResponseHelper.Failure("User not authenticated.");
                }

                _logger.LogInformation("User {UserId} fetching assignment {AssignmentId}", userId, assignmentId);

                var assignment = await _assignmentRepo.GetAssignmentByIdAsync(assignmentId);

                if (assignment == null)
                {
                    return ApiResponseHelper.Failure("Assignment not found.");
                }

                return ApiResponseHelper.Success(assignment, "Assignment retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching assignment {AssignmentId}", assignmentId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching assignment details."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/SupervisorAssignment/accept
        /// Supervisor accepts an assignment
        /// </summary>
        [HttpPost("accept")]
        public async Task<IActionResult> AcceptAssignment([FromBody] AcceptAssignmentRequest request)
        {
            try
            {
                long supervisorId = GetUserId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                _logger.LogInformation("Supervisor {SupervisorId} accepting assignment {AssignmentId}", supervisorId, request.AssignmentId);

                var success = await _assignmentRepo.AcceptAssignmentAsync(request.AssignmentId, supervisorId);

                if (success)
                {
                    _logger.LogInformation("Assignment {AssignmentId} accepted by supervisor {SupervisorId}", request.AssignmentId, supervisorId);
                    return ApiResponseHelper.Success(null, "Assignment accepted successfully.");
                }

                return ApiResponseHelper.Failure("Failed to accept assignment. It may have already been accepted or cancelled.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Accept assignment failed: {Message}", ex.Message);
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting assignment {AssignmentId}", request.AssignmentId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while accepting assignment."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/SupervisorAssignment/reject
        /// Supervisor rejects an assignment with reason
        /// </summary>
        [HttpPost("reject")]
        public async Task<IActionResult> RejectAssignment([FromBody] RejectAssignmentRequest request)
        {
            try
            {
                long supervisorId = GetUserId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                if (string.IsNullOrWhiteSpace(request.Reason))
                {
                    return ApiResponseHelper.Failure("Rejection reason is required.");
                }

                _logger.LogInformation("Supervisor {SupervisorId} rejecting assignment {AssignmentId}. Reason: {Reason}", supervisorId, request.AssignmentId, request.Reason);

                var success = await _assignmentRepo.RejectAssignmentAsync(request.AssignmentId, supervisorId, request.Reason);

                if (success)
                {
                    _logger.LogInformation("Assignment {AssignmentId} rejected by supervisor {SupervisorId}", request.AssignmentId, supervisorId);
                    return ApiResponseHelper.Success(null, "Assignment rejected successfully.");
                }

                return ApiResponseHelper.Failure("Failed to reject assignment.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Reject assignment failed: {Message}", ex.Message);
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting assignment {AssignmentId}", request.AssignmentId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while rejecting assignment."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/SupervisorAssignment/checkin
        /// Supervisor checks in at event location with GPS + photo
        /// </summary>
        [HttpPost("checkin")]
        public async Task<IActionResult> CheckIn([FromBody] CheckInDto request)
        {
            try
            {
                long supervisorId = GetUserId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                request.SupervisorId = supervisorId;
                request.CheckInTime = DateTime.UtcNow;

                if (string.IsNullOrWhiteSpace(request.GPSLocation))
                {
                    return ApiResponseHelper.Failure("GPS location is required for check-in.");
                }

                _logger.LogInformation("Supervisor {SupervisorId} checking in for assignment {AssignmentId} at {Location}", supervisorId, request.AssignmentId, request.GPSLocation);

                var success = await _assignmentRepo.CheckInAsync(request);

                if (success)
                {
                    _logger.LogInformation("Supervisor {SupervisorId} checked in successfully for assignment {AssignmentId}", supervisorId, request.AssignmentId);
                    return ApiResponseHelper.Success(null, "Check-in successful. You can now proceed with pre-event verification.");
                }

                return ApiResponseHelper.Failure("Failed to check in. Ensure the assignment is accepted and not already checked in.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Check-in failed: {Message}", ex.Message);
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during check-in for assignment {AssignmentId}", request.AssignmentId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred during check-in."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/SupervisorAssignment/request-payment
        /// Supervisor requests payment release after event completion
        /// REGISTERED supervisors: Request only (requires admin approval)
        /// CAREER supervisors (FULL authority): Direct release
        /// </summary>
        [HttpPost("request-payment")]
        public async Task<IActionResult> RequestPaymentRelease([FromBody] PaymentReleaseRequest request)
        {
            try
            {
                long supervisorId = GetUserId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                if (request.Amount <= 0)
                {
                    return ApiResponseHelper.Failure("Payment amount must be greater than zero.");
                }

                _logger.LogInformation("Supervisor {SupervisorId} requesting payment release for assignment {AssignmentId}, amount: {Amount}", supervisorId, request.AssignmentId, request.Amount);

                var response = await _assignmentRepo.RequestPaymentReleaseAsync(request.AssignmentId, supervisorId, request.Amount);

                if (response.Success)
                {
                    if (response.DirectRelease)
                    {
                        _logger.LogInformation("Payment directly released for assignment {AssignmentId} by CAREER supervisor {SupervisorId}", request.AssignmentId, supervisorId);
                        return ApiResponseHelper.Success(response, "Payment released successfully.");
                    }
                    else
                    {
                        _logger.LogInformation("Payment release requested for assignment {AssignmentId} by supervisor {SupervisorId}. Awaiting admin approval.", request.AssignmentId, supervisorId);
                        return ApiResponseHelper.Success(response, "Payment release requested. Awaiting admin approval.");
                    }
                }

                return ApiResponseHelper.Failure(response.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Payment release request failed: {Message}", ex.Message);
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting payment release for assignment {AssignmentId}", request.AssignmentId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while requesting payment release."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/SupervisorAssignment/complete
        /// Supervisor marks assignment as completed
        /// </summary>
        [HttpPost("complete")]
        public async Task<IActionResult> CompleteAssignment([FromBody] CompleteAssignmentRequest request)
        {
            try
            {
                long supervisorId = GetUserId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                _logger.LogInformation("Supervisor {SupervisorId} completing assignment {AssignmentId}", supervisorId, request.AssignmentId);

                var success = await _assignmentRepo.CompleteAssignmentAsync(request.AssignmentId, supervisorId);

                if (success)
                {
                    _logger.LogInformation("Assignment {AssignmentId} completed by supervisor {SupervisorId}", request.AssignmentId, supervisorId);
                    return ApiResponseHelper.Success(null, "Assignment completed successfully. You can now submit the post-event report.");
                }

                return ApiResponseHelper.Failure("Failed to complete assignment. Ensure all event phases are done.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Complete assignment failed: {Message}", ex.Message);
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing assignment {AssignmentId}", request.AssignmentId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while completing assignment."));
            }
        }

        // =============================================
        // ADMIN ENDPOINTS - Assignment Management
        // =============================================

        /// <summary>
        /// POST: api/Supervisor/SupervisorAssignment/admin/find-eligible
        /// Find eligible supervisors for an event based on rules
        /// </summary>
        [HttpPost("admin/find-eligible")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> FindEligibleSupervisors([FromBody] FindEligibleSupervisorsDto criteria)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                _logger.LogInformation("Admin {AdminId} finding eligible supervisors for order {OrderId}, event type: {EventType}", adminId, criteria.OrderId, criteria.EventType);

                var eligible = await _assignmentRepo.FindEligibleSupervisorsAsync(criteria);

                return ApiResponseHelper.Success(eligible, $"Found {eligible.Count} eligible supervisor(s).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding eligible supervisors for order {OrderId}", criteria.OrderId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while finding eligible supervisors."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/SupervisorAssignment/admin/assign
        /// Admin assigns a supervisor to an event
        /// </summary>
        [HttpPost("admin/assign")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignSupervisor([FromBody] AssignSupervisorDto assignment)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                assignment.AssignedBy = adminId;

                _logger.LogInformation("Admin {AdminId} assigning supervisor {SupervisorId} to order {OrderId}", adminId, assignment.SupervisorId, assignment.OrderId);

                var assignmentId = await _assignmentRepo.AssignSupervisorToEventAsync(assignment);

                if (assignmentId > 0)
                {
                    _logger.LogInformation("Supervisor {SupervisorId} assigned to order {OrderId}. Assignment ID: {AssignmentId}", assignment.SupervisorId, assignment.OrderId, assignmentId);
                    return ApiResponseHelper.Success(new { assignmentId }, "Supervisor assigned successfully.");
                }

                return ApiResponseHelper.Failure("Failed to assign supervisor.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Assign supervisor failed: {Message}", ex.Message);
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning supervisor to order {OrderId}", assignment.OrderId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while assigning supervisor."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/SupervisorAssignment/admin/bulk-assign
        /// Admin bulk assigns multiple supervisors to an event
        /// </summary>
        [HttpPost("admin/bulk-assign")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkAssignSupervisors([FromBody] BulkAssignRequest request)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                if (request.SupervisorIds == null || request.SupervisorIds.Count == 0)
                {
                    return ApiResponseHelper.Failure("At least one supervisor must be selected.");
                }

                _logger.LogInformation("Admin {AdminId} bulk assigning {Count} supervisors to order {OrderId}", adminId, request.SupervisorIds.Count, request.OrderId);

                var assignmentIds = await _assignmentRepo.BulkAssignSupervisorsAsync(request.OrderId, request.SupervisorIds, adminId);

                _logger.LogInformation("Bulk assignment completed. {Count} assignments created for order {OrderId}", assignmentIds.Count, request.OrderId);

                return ApiResponseHelper.Success(new { assignmentIds, count = assignmentIds.Count }, $"{assignmentIds.Count} supervisor(s) assigned successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk assigning supervisors to order {OrderId}", request.OrderId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred during bulk assignment."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/SupervisorAssignment/admin/by-order/{orderId}
        /// Get all assignments for a specific order
        /// </summary>
        [HttpGet("admin/by-order/{orderId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAssignmentsByOrder(long orderId)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                _logger.LogInformation("Admin {AdminId} fetching assignments for order {OrderId}", adminId, orderId);

                var assignments = await _assignmentRepo.GetAssignmentsByOrderAsync(orderId);

                return ApiResponseHelper.Success(assignments, "Assignments retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching assignments for order {OrderId}", orderId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching assignments."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/SupervisorAssignment/admin/all
        /// Get all assignments with optional date filter
        /// </summary>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAssignments([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                _logger.LogInformation("Admin {AdminId} fetching all assignments from {From} to {To}", adminId, fromDate?.ToString("yyyy-MM-dd") ?? "start", toDate?.ToString("yyyy-MM-dd") ?? "end");

                var assignments = await _assignmentRepo.GetAllAssignmentsAsync(fromDate, toDate);

                return ApiResponseHelper.Success(assignments, "Assignments retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all assignments");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching assignments."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/SupervisorAssignment/admin/search
        /// Search assignments with filters
        /// </summary>
        [HttpPost("admin/search")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SearchAssignments([FromBody] AssignmentSearchDto filters)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                _logger.LogInformation("Admin {AdminId} searching assignments with filters", adminId);

                var assignments = await _assignmentRepo.SearchAssignmentsAsync(filters);

                return ApiResponseHelper.Success(assignments, $"Found {assignments.Count} assignment(s).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching assignments");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while searching assignments."));
            }
        }

        /// <summary>
        /// PUT: api/Supervisor/SupervisorAssignment/admin/update-status
        /// Admin updates assignment status
        /// </summary>
        [HttpPut("admin/update-status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAssignmentStatus([FromBody] UpdateStatusRequest request)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                _logger.LogInformation("Admin {AdminId} updating assignment {AssignmentId} status to {Status}", adminId, request.AssignmentId, request.NewStatus);

                var success = await _assignmentRepo.UpdateAssignmentStatusAsync(request.AssignmentId, request.NewStatus, adminId, request.Notes);

                if (success)
                {
                    return ApiResponseHelper.Success(null, "Assignment status updated successfully.");
                }

                return ApiResponseHelper.Failure("Failed to update assignment status.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Update assignment status failed: {Message}", ex.Message);
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating assignment status for {AssignmentId}", request.AssignmentId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while updating assignment status."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/SupervisorAssignment/admin/cancel
        /// Admin cancels an assignment
        /// </summary>
        [HttpPost("admin/cancel")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CancelAssignment([FromBody] CancelAssignmentRequest request)
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
                    return ApiResponseHelper.Failure("Cancellation reason is required.");
                }

                _logger.LogInformation("Admin {AdminId} cancelling assignment {AssignmentId}. Reason: {Reason}", adminId, request.AssignmentId, request.Reason);

                var success = await _assignmentRepo.CancelAssignmentAsync(request.AssignmentId, adminId, request.Reason);

                if (success)
                {
                    _logger.LogInformation("Assignment {AssignmentId} cancelled by admin {AdminId}", request.AssignmentId, adminId);
                    return ApiResponseHelper.Success(null, "Assignment cancelled successfully.");
                }

                return ApiResponseHelper.Failure("Failed to cancel assignment.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cancel assignment failed: {Message}", ex.Message);
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling assignment {AssignmentId}", request.AssignmentId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while cancelling assignment."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/SupervisorAssignment/admin/approve-payment
        /// Admin approves payment release for REGISTERED supervisors
        /// </summary>
        [HttpPost("admin/approve-payment")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApprovePaymentRelease([FromBody] ApprovePaymentRequest request)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                _logger.LogInformation("Admin {AdminId} approving payment release for assignment {AssignmentId}", adminId, request.AssignmentId);

                var success = await _assignmentRepo.ApprovePaymentReleaseAsync(request.AssignmentId, adminId, request.Notes);

                if (success)
                {
                    _logger.LogInformation("Payment release approved for assignment {AssignmentId} by admin {AdminId}", request.AssignmentId, adminId);
                    return ApiResponseHelper.Success(null, "Payment release approved successfully.");
                }

                return ApiResponseHelper.Failure("Failed to approve payment release.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Approve payment release failed: {Message}", ex.Message);
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving payment release for assignment {AssignmentId}", request.AssignmentId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while approving payment release."));
            }
        }

        // =============================================
        // ANALYTICS & REPORTING ENDPOINTS (Admin)
        // =============================================

        /// <summary>
        /// GET: api/Supervisor/SupervisorAssignment/admin/upcoming
        /// Get upcoming assignments within specified days
        /// </summary>
        [HttpGet("admin/upcoming")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUpcomingAssignments([FromQuery] int daysAhead = 7)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                var assignments = await _assignmentRepo.GetUpcomingAssignmentsAsync(daysAhead);

                return ApiResponseHelper.Success(assignments, $"Found {assignments.Count} upcoming assignment(s) in next {daysAhead} days.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching upcoming assignments");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching upcoming assignments."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/SupervisorAssignment/admin/overdue
        /// Get overdue assignments
        /// </summary>
        [HttpGet("admin/overdue")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOverdueAssignments()
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                var assignments = await _assignmentRepo.GetOverdueAssignmentsAsync();

                return ApiResponseHelper.Success(assignments, $"Found {assignments.Count} overdue assignment(s).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching overdue assignments");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching overdue assignments."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/SupervisorAssignment/admin/statistics
        /// Get assignment statistics for a date range
        /// </summary>
        [HttpGet("admin/statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAssignmentStatistics([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
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

                _logger.LogInformation("Admin {AdminId} fetching assignment statistics from {From} to {To}", adminId, fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd"));

                var statistics = await _assignmentRepo.GetAssignmentStatisticsAsync(fromDate, toDate);

                return ApiResponseHelper.Success(statistics, "Statistics retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching assignment statistics");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching statistics."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/SupervisorAssignment/admin/workload
        /// Get supervisor workload distribution for a date range
        /// </summary>
        [HttpGet("admin/workload")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSupervisorWorkload([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
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

                _logger.LogInformation("Admin {AdminId} fetching supervisor workload from {From} to {To}", adminId, fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd"));

                var workload = await _assignmentRepo.GetSupervisorWorkloadAsync(fromDate, toDate);

                return ApiResponseHelper.Success(workload, "Workload data retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching supervisor workload");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching workload data."));
            }
        }
    }

    #region Request Models

    public class AcceptAssignmentRequest
    {
        public long AssignmentId { get; set; }
    }

    public class RejectAssignmentRequest
    {
        public long AssignmentId { get; set; }
        public string Reason { get; set; }
    }

    public class PaymentReleaseRequest
    {
        public long AssignmentId { get; set; }
        public decimal Amount { get; set; }
    }

    public class CompleteAssignmentRequest
    {
        public long AssignmentId { get; set; }
    }

    public class BulkAssignRequest
    {
        public long OrderId { get; set; }
        public List<long> SupervisorIds { get; set; }
    }

    public class UpdateStatusRequest
    {
        public long AssignmentId { get; set; }
        public string NewStatus { get; set; }
        public string Notes { get; set; }
    }

    public class CancelAssignmentRequest
    {
        public long AssignmentId { get; set; }
        public string Reason { get; set; }
    }

    public class ApprovePaymentRequest
    {
        public long AssignmentId { get; set; }
        public string Notes { get; set; }
    }

    #endregion
}
