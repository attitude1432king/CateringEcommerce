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
    /// Event Supervision Controller
    /// Handles Pre/During/Post event workflows for supervisors
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/Supervisor/[controller]")]
    public class EventSupervisionController : ControllerBase
    {
        private readonly ILogger<EventSupervisionController> _logger;
        private readonly IEventSupervisionRepository _eventSupervisionRepo;

        public EventSupervisionController(
            ILogger<EventSupervisionController> logger,
            IEventSupervisionRepository eventSupervisionRepo)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventSupervisionRepo = eventSupervisionRepo ?? throw new ArgumentNullException(nameof(eventSupervisionRepo));
        }

        #region Helper Methods

        private long GetSupervisorId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("UserId")?.Value;

            if (long.TryParse(userIdClaim, out long supervisorId))
            {
                return supervisorId;
            }
            return 0;
        }

        #endregion

        // =============================================
        // PRE-EVENT VERIFICATION ENDPOINTS
        // =============================================

        /// <summary>
        /// POST: api/Supervisor/EventSupervision/pre-event/submit
        /// Submit complete pre-event verification checklist
        /// </summary>
        [HttpPost("pre-event/submit")]
        public async Task<IActionResult> SubmitPreEventVerification([FromBody] SubmitPreEventVerificationDto request)
        {
            try
            {
                long supervisorId = GetSupervisorId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                // Ensure supervisor ID matches
                request.SupervisorId = supervisorId;

                _logger.LogInformation($"Supervisor {supervisorId} submitting pre-event verification for assignment {request.AssignmentId}");

                var success = await _eventSupervisionRepo.SubmitPreEventVerificationAsync(request);

                if (success)
                {
                    _logger.LogInformation($"Pre-event verification submitted successfully for assignment {request.AssignmentId}");
                    return ApiResponseHelper.Success(null, "Pre-event verification submitted successfully.");
                }

                return ApiResponseHelper.Failure("Failed to submit pre-event verification.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Pre-event verification failed: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting pre-event verification");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while submitting pre-event verification."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/EventSupervision/pre-event/{assignmentId}
        /// Get pre-event verification details
        /// </summary>
        [HttpGet("pre-event/{assignmentId}")]
        public async Task<IActionResult> GetPreEventVerification(long assignmentId)
        {
            try
            {
                long supervisorId = GetSupervisorId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                _logger.LogInformation($"Supervisor {supervisorId} fetching pre-event verification for assignment {assignmentId}");

                var verification = await _eventSupervisionRepo.GetPreEventVerificationAsync(assignmentId);

                if (verification == null)
                {
                    return ApiResponseHelper.Failure("Pre-event verification not found.");
                }

                // Verify supervisor owns this assignment
                if (verification.SupervisorId != supervisorId)
                {
                    _logger.LogWarning($"Supervisor {supervisorId} attempted to access verification for assignment belonging to supervisor {verification.SupervisorId}");
                    return ApiResponseHelper.Failure("Access denied.");
                }

                return ApiResponseHelper.Success(verification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching pre-event verification for assignment {assignmentId}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching verification details."));
            }
        }

        /// <summary>
        /// PUT: api/Supervisor/EventSupervision/pre-event/{checklistId}
        /// Update pre-event checklist before final submission
        /// </summary>
        [HttpPut("pre-event/{checklistId}")]
        public async Task<IActionResult> UpdatePreEventChecklist(long checklistId, [FromBody] PreEventVerificationModel updates)
        {
            try
            {
                long supervisorId = GetSupervisorId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                _logger.LogInformation($"Supervisor {supervisorId} updating pre-event checklist {checklistId}");

                var success = await _eventSupervisionRepo.UpdatePreEventChecklistAsync(checklistId, updates);

                if (success)
                {
                    return ApiResponseHelper.Success(null, "Pre-event checklist updated successfully.");
                }

                return ApiResponseHelper.Failure("Failed to update pre-event checklist.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating pre-event checklist {checklistId}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while updating checklist."));
            }
        }

        // =============================================
        // DURING-EVENT MONITORING ENDPOINTS
        // =============================================

        /// <summary>
        /// POST: api/Supervisor/EventSupervision/during/food-serving
        /// Monitor food serving quality during event
        /// </summary>
        [HttpPost("during/food-serving")]
        public async Task<IActionResult> RecordFoodServingMonitor([FromBody] FoodServingMonitorDto request)
        {
            try
            {
                long supervisorId = GetSupervisorId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                request.SupervisorId = supervisorId;

                _logger.LogInformation($"Supervisor {supervisorId} recording food serving monitor for assignment {request.AssignmentId}");

                var success = await _eventSupervisionRepo.RecordFoodServingMonitorAsync(request);

                if (success)
                {
                    return ApiResponseHelper.Success(null, "Food serving quality recorded successfully.");
                }

                return ApiResponseHelper.Failure("Failed to record food serving quality.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording food serving monitor");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while recording food serving quality."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/EventSupervision/during/update-guest-count
        /// Update guest count in real-time during event
        /// </summary>
        [HttpPost("during/update-guest-count")]
        public async Task<IActionResult> UpdateGuestCount([FromBody] UpdateGuestCountDto request)
        {
            try
            {
                long supervisorId = GetSupervisorId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                request.SupervisorId = supervisorId;
                request.Timestamp = DateTime.UtcNow;

                _logger.LogInformation($"Supervisor {supervisorId} updating guest count to {request.ActualGuestCount} for assignment {request.AssignmentId}");

                var success = await _eventSupervisionRepo.UpdateGuestCountAsync(request);

                if (success)
                {
                    return ApiResponseHelper.Success(null, "Guest count updated successfully.");
                }

                return ApiResponseHelper.Failure("Failed to update guest count.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating guest count");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while updating guest count."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/EventSupervision/during/request-extra-quantity
        /// Request extra quantity with client approval required
        /// </summary>
        [HttpPost("during/request-extra-quantity")]
        public async Task<IActionResult> RequestExtraQuantity([FromBody] RequestExtraQuantityDto request)
        {
            try
            {
                long supervisorId = GetSupervisorId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                request.SupervisorId = supervisorId;

                _logger.LogInformation($"Supervisor {supervisorId} requesting extra quantity for assignment {request.AssignmentId}: {request.ExtraQuantity} x {request.ItemName}");

                var response = await _eventSupervisionRepo.RequestExtraQuantityAsync(request);

                if (response.Success)
                {
                    _logger.LogInformation($"Extra quantity request created. Tracking ID: {response.TrackingId}, Approval Method: {response.ApprovalMethod}");

                    if (response.ApprovalMethod == ClientApprovalMethod.OTP)
                    {
                        _logger.LogInformation($"OTP sent to client. Code: {response.OTPCode}, Expires: {response.OTPExpiresAt}");
                    }

                    return ApiResponseHelper.Success(response, response.Message);
                }

                return ApiResponseHelper.Failure(response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting extra quantity");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while requesting extra quantity."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/EventSupervision/during/verify-otp
        /// Verify client OTP for extra quantity approval
        /// </summary>
        [HttpPost("during/verify-otp")]
        public async Task<IActionResult> VerifyClientOTP([FromBody] VerifyClientOTPDto request)
        {
            try
            {
                _logger.LogInformation($"Verifying client OTP for assignment {request.AssignmentId}");

                // Get client IP from request
                request.ClientIPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

                var response = await _eventSupervisionRepo.VerifyClientOTPAsync(request);

                if (response.Success)
                {
                    if (response.OTPVerified)
                    {
                        _logger.LogInformation($"OTP verified successfully for assignment {request.AssignmentId}. Approval status: {response.ApprovalStatus}");
                        return ApiResponseHelper.Success(response, "OTP verified successfully. Extra quantity request approved.");
                    }
                    else
                    {
                        _logger.LogWarning($"OTP verification failed for assignment {request.AssignmentId}. Remaining attempts: {response.RemainingAttempts}");
                        return ApiResponseHelper.Failure(response.Message, "warning", response);
                    }
                }

                return ApiResponseHelper.Failure(response.Message, "error", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying client OTP");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while verifying OTP."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/EventSupervision/during/resend-otp
        /// Resend OTP to client if expired or lost
        /// </summary>
        [HttpPost("during/resend-otp")]
        public async Task<IActionResult> ResendClientOTP([FromBody] ResendOTPRequest request)
        {
            try
            {
                long supervisorId = GetSupervisorId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                _logger.LogInformation($"Supervisor {supervisorId} requesting OTP resend for assignment {request.AssignmentId}");

                var newOTPCode = await _eventSupervisionRepo.ResendClientOTPAsync(request.AssignmentId, request.Purpose);

                if (!string.IsNullOrEmpty(newOTPCode))
                {
                    _logger.LogInformation($"New OTP sent successfully for assignment {request.AssignmentId}");
                    return ApiResponseHelper.Success(null, "OTP resent successfully. Please check your SMS.");
                }

                return ApiResponseHelper.Failure("Failed to resend OTP.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending OTP");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while resending OTP."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/EventSupervision/during/tracking/{assignmentId}
        /// Get all during-event tracking logs
        /// </summary>
        [HttpGet("during/tracking/{assignmentId}")]
        public async Task<IActionResult> GetDuringEventTracking(long assignmentId)
        {
            try
            {
                long supervisorId = GetSupervisorId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                _logger.LogInformation($"Supervisor {supervisorId} fetching during-event tracking for assignment {assignmentId}");

                var tracking = await _eventSupervisionRepo.GetDuringEventTrackingAsync(assignmentId);

                return ApiResponseHelper.Success(tracking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching during-event tracking for assignment {assignmentId}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching tracking logs."));
            }
        }

        // =============================================
        // POST-EVENT COMPLETION ENDPOINTS
        // =============================================

        /// <summary>
        /// POST: api/Supervisor/EventSupervision/post-event/submit
        /// Submit comprehensive post-event completion report
        /// </summary>
        [HttpPost("post-event/submit")]
        public async Task<IActionResult> SubmitPostEventReport([FromBody] SubmitPostEventReportDto request)
        {
            try
            {
                long supervisorId = GetSupervisorId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                request.SupervisorId = supervisorId;

                _logger.LogInformation($"Supervisor {supervisorId} submitting post-event report for assignment {request.AssignmentId}");

                var success = await _eventSupervisionRepo.SubmitPostEventReportAsync(request);

                if (success)
                {
                    _logger.LogInformation($"Post-event report submitted successfully for assignment {request.AssignmentId}");
                    return ApiResponseHelper.Success(null, "Post-event report submitted successfully. Awaiting admin verification.");
                }

                return ApiResponseHelper.Failure("Failed to submit post-event report.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Post-event report submission failed: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting post-event report");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while submitting post-event report."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/EventSupervision/post-event/{assignmentId}
        /// Get post-event report details
        /// </summary>
        [HttpGet("post-event/{assignmentId}")]
        public async Task<IActionResult> GetPostEventReport(long assignmentId)
        {
            try
            {
                long supervisorId = GetSupervisorId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                _logger.LogInformation($"Supervisor {supervisorId} fetching post-event report for assignment {assignmentId}");

                var report = await _eventSupervisionRepo.GetPostEventReportAsync(assignmentId);

                if (report == null)
                {
                    return ApiResponseHelper.Failure("Post-event report not found.");
                }

                // Verify supervisor owns this assignment
                if (report.SupervisorId != supervisorId)
                {
                    _logger.LogWarning($"Supervisor {supervisorId} attempted to access report for assignment belonging to supervisor {report.SupervisorId}");
                    return ApiResponseHelper.Failure("Access denied.");
                }

                return ApiResponseHelper.Success(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching post-event report for assignment {assignmentId}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching post-event report."));
            }
        }

        /// <summary>
        /// PUT: api/Supervisor/EventSupervision/post-event/{reportId}
        /// Update post-event report before admin verification
        /// </summary>
        [HttpPut("post-event/{reportId}")]
        public async Task<IActionResult> UpdatePostEventReport(long reportId, [FromBody] PostEventReportModel updates)
        {
            try
            {
                long supervisorId = GetSupervisorId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                _logger.LogInformation($"Supervisor {supervisorId} updating post-event report {reportId}");

                var success = await _eventSupervisionRepo.UpdatePostEventReportAsync(reportId, updates);

                if (success)
                {
                    return ApiResponseHelper.Success(null, "Post-event report updated successfully.");
                }

                return ApiResponseHelper.Failure("Failed to update post-event report.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating post-event report {reportId}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while updating report."));
            }
        }

        // =============================================
        // ADMIN ENDPOINTS
        // =============================================

        /// <summary>
        /// POST: api/Supervisor/EventSupervision/admin/verify-report/{reportId}
        /// Admin verification of post-event report
        /// </summary>
        [HttpPost("admin/verify-report/{reportId}")]
        [Authorize(Roles = "Admin")] // Assuming admin role-based authorization
        public async Task<IActionResult> VerifyPostEventReport(long reportId, [FromBody] VerifyReportRequest request)
        {
            try
            {
                long adminId = GetSupervisorId(); // Gets admin user ID from token
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                _logger.LogInformation($"Admin {adminId} verifying post-event report {reportId}");

                var success = await _eventSupervisionRepo.VerifyPostEventReportAsync(reportId, adminId, request.VerificationNotes);

                if (success)
                {
                    _logger.LogInformation($"Post-event report {reportId} verified successfully by admin {adminId}");
                    return ApiResponseHelper.Success(null, "Post-event report verified successfully.");
                }

                return ApiResponseHelper.Failure("Failed to verify post-event report.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying post-event report {reportId}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while verifying report."));
            }
        }

        // =============================================
        // COMPLETE SUPERVISION SUMMARY ENDPOINT
        // =============================================

        /// <summary>
        /// GET: api/Supervisor/EventSupervision/summary/{assignmentId}
        /// Get complete event supervision summary (Pre + During + Post)
        /// </summary>
        [HttpGet("summary/{assignmentId}")]
        public async Task<IActionResult> GetEventSupervisionSummary(long assignmentId)
        {
            try
            {
                long supervisorId = GetSupervisorId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                _logger.LogInformation($"Supervisor {supervisorId} fetching complete supervision summary for assignment {assignmentId}");

                var summary = await _eventSupervisionRepo.GetEventSupervisionSummaryAsync(assignmentId);

                if (summary == null)
                {
                    return ApiResponseHelper.Failure("Event supervision summary not found.");
                }

                return ApiResponseHelper.Success(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching supervision summary for assignment {assignmentId}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching supervision summary."));
            }
        }

        // =============================================
        // EVIDENCE UPLOAD ENDPOINTS
        // =============================================

        /// <summary>
        /// POST: api/Supervisor/EventSupervision/evidence/upload
        /// Upload timestamped evidence (photos/videos) for any phase
        /// </summary>
        [HttpPost("evidence/upload")]
        public async Task<IActionResult> UploadTimestampedEvidence([FromBody] UploadEvidenceRequest request)
        {
            try
            {
                long supervisorId = GetSupervisorId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                _logger.LogInformation($"Supervisor {supervisorId} uploading {request.Evidence.Count} evidence items for assignment {request.AssignmentId}, phase: {request.Phase}");

                var success = await _eventSupervisionRepo.UploadTimestampedEvidenceAsync(request.AssignmentId, request.Evidence, request.Phase);

                if (success)
                {
                    return ApiResponseHelper.Success(null, "Evidence uploaded successfully.");
                }

                return ApiResponseHelper.Failure("Failed to upload evidence.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading timestamped evidence");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while uploading evidence."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/EventSupervision/evidence/{assignmentId}
        /// Get all evidence for an assignment organized by phase
        /// </summary>
        [HttpGet("evidence/{assignmentId}")]
        public async Task<IActionResult> GetAssignmentEvidence(long assignmentId)
        {
            try
            {
                long supervisorId = GetSupervisorId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                _logger.LogInformation($"Supervisor {supervisorId} fetching all evidence for assignment {assignmentId}");

                var evidence = await _eventSupervisionRepo.GetAssignmentEvidenceAsync(assignmentId);

                return ApiResponseHelper.Success(evidence);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching assignment evidence for {assignmentId}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching evidence."));
            }
        }
    }

    #region Request Models

    public class ResendOTPRequest
    {
        public long AssignmentId { get; set; }
        public string Purpose { get; set; }
    }

    public class VerifyReportRequest
    {
        public string VerificationNotes { get; set; }
    }

    public class UploadEvidenceRequest
    {
        public long AssignmentId { get; set; }
        public List<TimestampedEvidence> Evidence { get; set; }
        public string Phase { get; set; } // PRE_EVENT, DURING_EVENT, POST_EVENT
    }

    #endregion
}
