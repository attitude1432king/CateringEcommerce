using CateringEcommerce.API.Helpers;
using CateringEcommerce.Domain.Interfaces.Supervisor;
using CateringEcommerce.Domain.Models.Supervisor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CateringEcommerce.API.Controllers.Supervisor
{
    /// <summary>
    /// Supervisor Payment Controller
    /// Handles earnings tracking, payment history, and admin payment approval queues
    /// Supervisors REQUEST payment release → Admins APPROVE/REJECT
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/Supervisor/[controller]")]
    public class SupervisorPaymentController : ControllerBase
    {
        private readonly ILogger<SupervisorPaymentController> _logger;
        private readonly ISupervisorAssignmentRepository _assignmentRepo;
        private readonly ISupervisorRepository _supervisorRepo;

        public SupervisorPaymentController(
            ILogger<SupervisorPaymentController> logger,
            ISupervisorAssignmentRepository assignmentRepo,
            ISupervisorRepository supervisorRepo)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _assignmentRepo = assignmentRepo ?? throw new ArgumentNullException(nameof(assignmentRepo));
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
        // SUPERVISOR EARNINGS ENDPOINTS
        // =============================================

        /// <summary>
        /// GET: api/Supervisor/SupervisorPayment/earnings
        /// Get earnings summary for the logged-in supervisor
        /// Returns: total earnings, pending payments, released payments
        /// </summary>
        [HttpGet("earnings")]
        public async Task<IActionResult> GetEarningsSummary()
        {
            try
            {
                long supervisorId = GetUserId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                _logger.LogInformation("Supervisor {SupervisorId} fetching earnings summary", supervisorId);

                // Get all completed assignments for this supervisor
                var allAssignments = await _assignmentRepo.GetAssignmentsBySupervisorAsync(supervisorId);

                var completedAssignments = allAssignments
                    .Where(a => a.AssignmentStatus == "COMPLETED")
                    .ToList();

                var totalEarnings = completedAssignments
                    .Where(a => a.PaymentReleaseApproved)
                    .Sum(a => a.SupervisorFee);

                var pendingPayments = completedAssignments
                    .Where(a => a.PaymentReleaseRequested && !a.PaymentReleaseApproved)
                    .Sum(a => a.SupervisorFee);

                var notRequestedPayments = completedAssignments
                    .Where(a => !a.PaymentReleaseRequested)
                    .Sum(a => a.SupervisorFee);

                var earnings = new
                {
                    totalEarnings,
                    releasedPayments = totalEarnings,
                    pendingPayments,
                    notRequestedPayments,
                    totalCompleted = completedAssignments.Count,
                    totalReleased = completedAssignments.Count(a => a.PaymentReleaseApproved),
                    totalPending = completedAssignments.Count(a => a.PaymentReleaseRequested && !a.PaymentReleaseApproved),
                    totalNotRequested = completedAssignments.Count(a => !a.PaymentReleaseRequested)
                };

                return ApiResponseHelper.Success(earnings, "Earnings summary retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching earnings summary");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching earnings summary."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/SupervisorPayment/history
        /// Get full payment history for the logged-in supervisor
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetPaymentHistory([FromQuery] string status = null)
        {
            try
            {
                long supervisorId = GetUserId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                _logger.LogInformation("Supervisor {SupervisorId} fetching payment history, filter: {Status}", supervisorId, status ?? "ALL");

                var allAssignments = await _assignmentRepo.GetAssignmentsBySupervisorAsync(supervisorId, "COMPLETED");

                var paymentHistory = allAssignments.Select(a => new
                {
                    a.AssignmentId,
                    a.AssignmentNumber,
                    a.OrderId,
                    a.EventDate,
                    a.EventLocation,
                    a.EventType,
                    a.SupervisorFee,
                    a.PaymentReleaseRequested,
                    a.PaymentReleaseRequestDate,
                    a.PaymentReleaseApproved,
                    a.PaymentReleaseApprovalDate,
                    paymentStatus = a.PaymentReleaseApproved ? "RELEASED"
                                  : a.PaymentReleaseRequested ? "PENDING"
                                  : "NOT_REQUESTED"
                }).ToList();

                // Apply optional status filter
                if (!string.IsNullOrWhiteSpace(status))
                {
                    paymentHistory = paymentHistory
                        .Where(p => p.paymentStatus.Equals(status, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                return ApiResponseHelper.Success(paymentHistory, $"Found {paymentHistory.Count} payment record(s).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching payment history");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching payment history."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/SupervisorPayment/request
        /// Supervisor requests payment release for a completed assignment
        /// REGISTERED supervisors: Request only (admin approval required)
        /// CAREER supervisors (FULL authority): Direct release
        /// </summary>
        [HttpPost("request")]
        public async Task<IActionResult> RequestPayment([FromBody] SupervisorPaymentRequest request)
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

                _logger.LogInformation("Supervisor {SupervisorId} requesting payment for assignment {AssignmentId}, amount: {Amount}",
                    supervisorId, request.AssignmentId, request.Amount);

                // Verify the assignment belongs to this supervisor and is completed
                var assignment = await _assignmentRepo.GetAssignmentByIdAsync(request.AssignmentId);
                if (assignment == null)
                {
                    return ApiResponseHelper.Failure("Assignment not found.");
                }

                if (assignment.SupervisorId != supervisorId)
                {
                    return ApiResponseHelper.Failure("Access denied. This assignment does not belong to you.");
                }

                if (assignment.AssignmentStatus != "COMPLETED")
                {
                    return ApiResponseHelper.Failure("Payment can only be requested for completed assignments.");
                }

                if (assignment.PaymentReleaseRequested)
                {
                    return ApiResponseHelper.Failure("Payment has already been requested for this assignment.");
                }

                var response = await _assignmentRepo.RequestPaymentReleaseAsync(request.AssignmentId, supervisorId, request.Amount);

                if (response.Success)
                {
                    if (response.DirectRelease)
                    {
                        _logger.LogInformation("Payment directly released for assignment {AssignmentId}", request.AssignmentId);
                        return ApiResponseHelper.Success(response, "Payment released successfully.");
                    }
                    else
                    {
                        _logger.LogInformation("Payment request submitted for assignment {AssignmentId}. Awaiting admin approval.", request.AssignmentId);
                        return ApiResponseHelper.Success(response, "Payment release requested. You will be notified once admin approves.");
                    }
                }

                return ApiResponseHelper.Failure(response.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Payment request failed: {Message}", ex.Message);
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting payment for assignment {AssignmentId}", request.AssignmentId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while requesting payment."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/SupervisorPayment/status/{assignmentId}
        /// Get payment status for a specific assignment
        /// </summary>
        [HttpGet("status/{assignmentId}")]
        public async Task<IActionResult> GetPaymentStatus(long assignmentId)
        {
            try
            {
                long supervisorId = GetUserId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                _logger.LogInformation("Supervisor {SupervisorId} checking payment status for assignment {AssignmentId}", supervisorId, assignmentId);

                var assignment = await _assignmentRepo.GetAssignmentByIdAsync(assignmentId);

                if (assignment == null)
                {
                    return ApiResponseHelper.Failure("Assignment not found.");
                }

                if (assignment.SupervisorId != supervisorId)
                {
                    return ApiResponseHelper.Failure("Access denied.");
                }

                var paymentStatus = new
                {
                    assignment.AssignmentId,
                    assignment.AssignmentNumber,
                    assignment.SupervisorFee,
                    assignment.AssignmentStatus,
                    assignment.PaymentReleaseRequested,
                    assignment.PaymentReleaseRequestDate,
                    assignment.PaymentReleaseApproved,
                    assignment.PaymentReleaseApprovalDate,
                    status = assignment.PaymentReleaseApproved ? "RELEASED"
                           : assignment.PaymentReleaseRequested ? "PENDING_APPROVAL"
                           : assignment.AssignmentStatus == "COMPLETED" ? "READY_TO_REQUEST"
                           : "NOT_ELIGIBLE"
                };

                return ApiResponseHelper.Success(paymentStatus, "Payment status retrieved.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching payment status for assignment {AssignmentId}", assignmentId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching payment status."));
            }
        }

        // =============================================
        // ADMIN PAYMENT APPROVAL QUEUE
        // =============================================

        /// <summary>
        /// GET: api/Supervisor/SupervisorPayment/admin/pending-approvals
        /// Get all pending payment release requests for admin approval
        /// </summary>
        [HttpGet("admin/pending-approvals")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingPaymentApprovals()
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                _logger.LogInformation("Admin {AdminId} fetching pending payment approvals", adminId);

                // Search for assignments that have payment requested but not approved
                var filters = new AssignmentSearchDto
                {
                    PaymentReleased = false
                };

                var allAssignments = await _assignmentRepo.SearchAssignmentsAsync(filters);

                var pendingApprovals = allAssignments
                    .Where(a => a.PaymentReleaseRequested && !a.PaymentReleaseApproved)
                    .Select(a => new
                    {
                        a.AssignmentId,
                        a.AssignmentNumber,
                        a.SupervisorId,
                        a.OrderId,
                        a.EventDate,
                        a.EventLocation,
                        a.EventType,
                        a.SupervisorFee,
                        a.PaymentReleaseRequestDate,
                        a.AssignmentStatus,
                        a.SupervisorRating,
                        a.IssuesReported
                    })
                    .OrderBy(a => a.PaymentReleaseRequestDate)
                    .ToList();

                return ApiResponseHelper.Success(pendingApprovals, $"Found {pendingApprovals.Count} pending payment approval(s).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending payment approvals");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching pending approvals."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/SupervisorPayment/admin/approve
        /// Admin approves payment release for a supervisor
        /// </summary>
        [HttpPost("admin/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApprovePayment([FromBody] AdminPaymentAction request)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                _logger.LogInformation("Admin {AdminId} approving payment for assignment {AssignmentId}", adminId, request.AssignmentId);

                var success = await _assignmentRepo.ApprovePaymentReleaseAsync(request.AssignmentId, adminId, request.Notes);

                if (success)
                {
                    _logger.LogInformation("Payment approved for assignment {AssignmentId} by admin {AdminId}", request.AssignmentId, adminId);
                    return ApiResponseHelper.Success(null, "Payment release approved successfully.");
                }

                return ApiResponseHelper.Failure("Failed to approve payment. The request may have already been processed.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Approve payment failed: {Message}", ex.Message);
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving payment for assignment {AssignmentId}", request.AssignmentId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while approving payment."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/SupervisorPayment/admin/reject
        /// Admin rejects payment release request
        /// </summary>
        [HttpPost("admin/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectPayment([FromBody] AdminPaymentAction request)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                if (string.IsNullOrWhiteSpace(request.Notes))
                {
                    return ApiResponseHelper.Failure("Rejection reason is required.");
                }

                _logger.LogInformation("Admin {AdminId} rejecting payment for assignment {AssignmentId}. Reason: {Reason}", adminId, request.AssignmentId, request.Notes);

                // Update assignment status to reflect payment rejection
                var success = await _assignmentRepo.UpdateAssignmentStatusAsync(
                    request.AssignmentId, "PAYMENT_REJECTED", adminId, request.Notes);

                if (success)
                {
                    _logger.LogInformation("Payment rejected for assignment {AssignmentId} by admin {AdminId}", request.AssignmentId, adminId);
                    return ApiResponseHelper.Success(null, "Payment release rejected. Supervisor will be notified.");
                }

                return ApiResponseHelper.Failure("Failed to reject payment.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting payment for assignment {AssignmentId}", request.AssignmentId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while rejecting payment."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/SupervisorPayment/admin/payment-summary
        /// Admin view: overall payment statistics
        /// </summary>
        [HttpGet("admin/payment-summary")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPaymentSummary([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                var from = fromDate ?? DateTime.UtcNow.AddMonths(-1);
                var to = toDate ?? DateTime.UtcNow;

                _logger.LogInformation("Admin {AdminId} fetching payment summary from {From} to {To}", adminId, from.ToString("yyyy-MM-dd"), to.ToString("yyyy-MM-dd"));

                var statistics = await _assignmentRepo.GetAssignmentStatisticsAsync(from, to);

                var allAssignments = await _assignmentRepo.GetAllAssignmentsAsync(from, to);

                var paymentSummary = new
                {
                    totalSupervisorFees = statistics.TotalSupervisorFees,
                    totalAssignments = statistics.TotalAssignments,
                    completedAssignments = statistics.CompletedAssignments,
                    totalReleased = allAssignments.Count(a => a.PaymentReleaseApproved),
                    totalPending = allAssignments.Count(a => a.PaymentReleaseRequested && !a.PaymentReleaseApproved),
                    totalNotRequested = allAssignments.Count(a => a.AssignmentStatus == "COMPLETED" && !a.PaymentReleaseRequested),
                    releasedAmount = allAssignments.Where(a => a.PaymentReleaseApproved).Sum(a => a.SupervisorFee),
                    pendingAmount = allAssignments.Where(a => a.PaymentReleaseRequested && !a.PaymentReleaseApproved).Sum(a => a.SupervisorFee),
                    fromDate = from,
                    toDate = to
                };

                return ApiResponseHelper.Success(paymentSummary, "Payment summary retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching payment summary");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching payment summary."));
            }
        }
    }

    #region Payment Request Models

    public class SupervisorPaymentRequest
    {
        public long AssignmentId { get; set; }
        public decimal Amount { get; set; }
    }

    public class AdminPaymentAction
    {
        public long AssignmentId { get; set; }
        public string Notes { get; set; }
    }

    #endregion
}
