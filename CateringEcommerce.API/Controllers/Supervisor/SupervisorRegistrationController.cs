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
    /// Supervisor Registration Controller
    /// Handles the 4-stage fast activation workflow:
    /// APPLIED → DOCUMENT_VERIFICATION → AWAITING_INTERVIEW → AWAITING_TRAINING → AWAITING_CERTIFICATION → ACTIVE
    /// Public endpoints for registration submission, admin endpoints for approval workflow
    /// </summary>
    [ApiController]
    [Route("api/Supervisor/[controller]")]
    public class SupervisorRegistrationController : ControllerBase
    {
        private readonly ILogger<SupervisorRegistrationController> _logger;
        private readonly IRegistrationRepository _registrationRepo;

        public SupervisorRegistrationController(
            ILogger<SupervisorRegistrationController> logger,
            IRegistrationRepository registrationRepo)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _registrationRepo = registrationRepo ?? throw new ArgumentNullException(nameof(registrationRepo));
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
        // PUBLIC ENDPOINTS - Registration Submission
        // =============================================

        /// <summary>
        /// POST: api/Supervisor/SupervisorRegistration/submit
        /// Public endpoint - Submit new supervisor registration
        /// No authentication required
        /// </summary>
        [AllowAnonymous]
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitRegistration([FromBody] SupervisorRegistrationSubmitDto registration)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(registration.FirstName) || string.IsNullOrWhiteSpace(registration.LastName))
                {
                    return ApiResponseHelper.Failure("First name and last name are required.");
                }

                if (string.IsNullOrWhiteSpace(registration.Email))
                {
                    return ApiResponseHelper.Failure("Email is required.");
                }

                if (string.IsNullOrWhiteSpace(registration.Phone))
                {
                    return ApiResponseHelper.Failure("Phone number is required.");
                }

                if (string.IsNullOrWhiteSpace(registration.IDProofNumber))
                {
                    return ApiResponseHelper.Failure("Identity proof number is required.");
                }

                _logger.LogInformation("New supervisor registration submitted: {Name}, {Email}", registration.FirstName + " " + registration.LastName, registration.Email);

                var registrationId = await _registrationRepo.SubmitRegistrationAsync(registration);

                if (registrationId > 0)
                {
                    _logger.LogInformation("Registration submitted successfully. ID: {RegistrationId}", registrationId);
                    return ApiResponseHelper.Success(
                        new { registrationId },
                        "Registration submitted successfully. You will be notified once your documents are verified.");
                }

                return ApiResponseHelper.Failure("Failed to submit registration. Email or phone may already be registered.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Registration submission failed: {Message}", ex.Message);
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting supervisor registration");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while submitting registration."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/SupervisorRegistration/progress/{registrationId}
        /// Public endpoint - Track registration progress
        /// </summary>
        [AllowAnonymous]
        [HttpGet("progress/{registrationId}")]
        public async Task<IActionResult> GetRegistrationProgress(long registrationId)
        {
            try
            {
                _logger.LogInformation("Fetching registration progress for ID: {RegistrationId}", registrationId);

                var workflowStatus = await _registrationRepo.GetWorkflowStatusAsync(registrationId);

                if (workflowStatus == null)
                {
                    return ApiResponseHelper.Failure("Registration not found.");
                }

                return ApiResponseHelper.Success(workflowStatus, "Registration progress retrieved.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching registration progress for ID: {RegistrationId}", registrationId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching registration progress."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/SupervisorRegistration/{registrationId}
        /// Get registration details
        /// </summary>
        [AllowAnonymous]
        [HttpGet("{registrationId}")]
        public async Task<IActionResult> GetRegistration(long registrationId)
        {
            try
            {
                var registration = await _registrationRepo.GetRegistrationByIdAsync(registrationId);

                if (registration == null)
                {
                    return ApiResponseHelper.Failure("Registration not found.");
                }

                return ApiResponseHelper.Success(registration, "Registration details retrieved.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching registration {RegistrationId}", registrationId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching registration details."));
            }
        }

        // =============================================
        // SUPERVISOR SELF-SERVICE ENDPOINTS (Authenticated)
        // =============================================

        /// <summary>
        /// GET: api/Supervisor/SupervisorRegistration/my-registration
        /// Get the logged-in supervisor's registration details
        /// </summary>
        [Authorize]
        [HttpGet("my-registration")]
        public async Task<IActionResult> GetMyRegistration()
        {
            try
            {
                long supervisorId = GetUserId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                var registration = await _registrationRepo.GetRegistrationBySupervisorIdAsync(supervisorId);

                if (registration == null)
                {
                    return ApiResponseHelper.Failure("Registration not found for this supervisor.");
                }

                return ApiResponseHelper.Success(registration, "Registration details retrieved.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching registration for supervisor");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching registration details."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/SupervisorRegistration/banking-details
        /// Submit banking details (post-activation)
        /// </summary>
        [Authorize]
        [HttpPost("banking-details")]
        public async Task<IActionResult> SubmitBankingDetails([FromBody] BankingDetailsDto bankingDetails)
        {
            try
            {
                long supervisorId = GetUserId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                bankingDetails.SupervisorId = supervisorId;

                if (string.IsNullOrWhiteSpace(bankingDetails.AccountNumber))
                {
                    return ApiResponseHelper.Failure("Account number is required.");
                }

                if (string.IsNullOrWhiteSpace(bankingDetails.IFSCCode))
                {
                    return ApiResponseHelper.Failure("IFSC code is required.");
                }

                _logger.LogInformation("Supervisor {SupervisorId} submitting banking details", supervisorId);

                var success = await _registrationRepo.SubmitBankingDetailsAsync(bankingDetails);

                if (success)
                {
                    return ApiResponseHelper.Success(null, "Banking details submitted successfully.");
                }

                return ApiResponseHelper.Failure("Failed to submit banking details.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting banking details");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while submitting banking details."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/SupervisorRegistration/banking-details
        /// Get banking details for logged-in supervisor
        /// </summary>
        [Authorize]
        [HttpGet("banking-details")]
        public async Task<IActionResult> GetBankingDetails()
        {
            try
            {
                long supervisorId = GetUserId();
                if (supervisorId <= 0)
                {
                    return ApiResponseHelper.Failure("Supervisor not authenticated.");
                }

                var banking = await _registrationRepo.GetBankingDetailsAsync(supervisorId);

                if (banking == null)
                {
                    return ApiResponseHelper.Failure("Banking details not found. Please submit your banking details.", "info");
                }

                return ApiResponseHelper.Success(banking, "Banking details retrieved.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching banking details");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching banking details."));
            }
        }

        // =============================================
        // ADMIN ENDPOINTS - Registration Approval Queue
        // =============================================

        /// <summary>
        /// GET: api/Supervisor/SupervisorRegistration/admin/all
        /// Get all registrations with optional status filter
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAllRegistrations([FromQuery] string status = null)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                _logger.LogInformation("Admin {AdminId} fetching all registrations, filter: {Status}", adminId, status ?? "ALL");

                var registrations = await _registrationRepo.GetAllRegistrationsAsync(status);

                return ApiResponseHelper.Success(registrations, $"Found {registrations.Count} registration(s).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all registrations");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching registrations."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/SupervisorRegistration/admin/by-stage/{stage}
        /// Get registrations by specific stage
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/by-stage/{stage}")]
        public async Task<IActionResult> GetRegistrationsByStage(string stage)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                _logger.LogInformation("Admin {AdminId} fetching registrations at stage: {Stage}", adminId, stage);

                var registrations = await _registrationRepo.GetRegistrationsByStageAsync(stage);

                return ApiResponseHelper.Success(registrations, $"Found {registrations.Count} registration(s) at {stage} stage.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching registrations by stage {Stage}", stage);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching registrations."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/SupervisorRegistration/admin/search
        /// Search registrations with filters
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/search")]
        public async Task<IActionResult> SearchRegistrations([FromBody] RegistrationSearchDto filters)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                var registrations = await _registrationRepo.SearchRegistrationsAsync(filters);

                return ApiResponseHelper.Success(registrations, $"Found {registrations.Count} registration(s).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching registrations");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while searching registrations."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/SupervisorRegistration/admin/statistics
        /// Get registration statistics
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/statistics")]
        public async Task<IActionResult> GetRegistrationStatistics()
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                var statistics = await _registrationRepo.GetRegistrationStatisticsAsync();

                return ApiResponseHelper.Success(statistics, "Registration statistics retrieved.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching registration statistics");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching statistics."));
            }
        }

        // =============================================
        // ADMIN - Stage 1: Document Verification
        // =============================================

        /// <summary>
        /// GET: api/Supervisor/SupervisorRegistration/admin/pending-docs
        /// Get registrations pending document verification
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/pending-docs")]
        public async Task<IActionResult> GetPendingDocumentVerifications()
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                var registrations = await _registrationRepo.GetRegistrationsPendingDocumentVerificationAsync();

                return ApiResponseHelper.Success(registrations, $"Found {registrations.Count} registration(s) pending document verification.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending document verifications");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching pending verifications."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/SupervisorRegistration/admin/verify-docs
        /// Verify or reject registration documents
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/verify-docs")]
        public async Task<IActionResult> VerifyDocuments([FromBody] DocumentVerificationDto verification)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                verification.VerifiedBy = adminId;

                _logger.LogInformation("Admin {AdminId} verifying documents for registration {RegistrationId}. Result: {Passed}",
                    adminId, verification.RegistrationId, verification.Passed);

                var success = await _registrationRepo.SubmitDocumentVerificationAsync(verification);

                if (success)
                {
                    var message = verification.Passed
                        ? "Documents verified successfully. Registration moved to interview stage."
                        : "Documents rejected. Supervisor will be notified.";
                    return ApiResponseHelper.Success(null, message);
                }

                return ApiResponseHelper.Failure("Failed to process document verification.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Document verification failed: {Message}", ex.Message);
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying documents for registration {RegistrationId}", verification.RegistrationId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while verifying documents."));
            }
        }

        // =============================================
        // ADMIN - Stage 2: Interview
        // =============================================

        /// <summary>
        /// GET: api/Supervisor/SupervisorRegistration/admin/pending-interview
        /// Get registrations pending interview
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/pending-interview")]
        public async Task<IActionResult> GetPendingInterviews()
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                var registrations = await _registrationRepo.GetRegistrationsPendingInterviewAsync();

                return ApiResponseHelper.Success(registrations, $"Found {registrations.Count} registration(s) pending interview.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending interviews");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching pending interviews."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/SupervisorRegistration/admin/schedule-interview
        /// Schedule a quick interview
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/schedule-interview")]
        public async Task<IActionResult> ScheduleInterview([FromBody] QuickInterviewDto interview)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                interview.ScheduledBy = adminId;

                _logger.LogInformation("Admin {AdminId} scheduling interview for registration {RegistrationId} on {Date}",
                    adminId, interview.RegistrationId, interview.InterviewDateTime.ToString("yyyy-MM-dd HH:mm"));

                var success = await _registrationRepo.ScheduleQuickInterviewAsync(interview);

                if (success)
                {
                    return ApiResponseHelper.Success(null, "Interview scheduled successfully. Supervisor will be notified.");
                }

                return ApiResponseHelper.Failure("Failed to schedule interview.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Schedule interview failed: {Message}", ex.Message);
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling interview for registration {RegistrationId}", interview.RegistrationId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while scheduling interview."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/SupervisorRegistration/admin/interview-result
        /// Submit interview result (pass/fail)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/interview-result")]
        public async Task<IActionResult> SubmitInterviewResult([FromBody] QuickInterviewResultDto result)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                result.InterviewedBy = adminId;

                _logger.LogInformation("Admin {AdminId} submitting interview result for registration {RegistrationId}. Passed: {Passed}",
                    adminId, result.RegistrationId, result.Passed);

                var success = await _registrationRepo.SubmitQuickInterviewResultAsync(result);

                if (success)
                {
                    var message = result.Passed
                        ? "Interview passed. Registration moved to training stage."
                        : "Interview failed. Supervisor will be notified.";
                    return ApiResponseHelper.Success(null, message);
                }

                return ApiResponseHelper.Failure("Failed to submit interview result.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Submit interview result failed: {Message}", ex.Message);
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting interview result for registration {RegistrationId}", result.RegistrationId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while submitting interview result."));
            }
        }

        // =============================================
        // ADMIN - Stage 3: Training
        // =============================================

        /// <summary>
        /// GET: api/Supervisor/SupervisorRegistration/admin/pending-training
        /// Get registrations pending training
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/pending-training")]
        public async Task<IActionResult> GetPendingTraining()
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                var registrations = await _registrationRepo.GetRegistrationsPendingTrainingAsync();

                return ApiResponseHelper.Success(registrations, $"Found {registrations.Count} registration(s) pending training.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending training registrations");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching pending training."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/SupervisorRegistration/admin/assign-training
        /// Assign condensed training modules
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/assign-training")]
        public async Task<IActionResult> AssignTraining([FromBody] AssignTrainingRequest request)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                if (request.ModuleIds == null || request.ModuleIds.Count == 0)
                {
                    return ApiResponseHelper.Failure("At least one training module must be selected.");
                }

                _logger.LogInformation("Admin {AdminId} assigning {Count} training modules to registration {RegistrationId}",
                    adminId, request.ModuleIds.Count, request.RegistrationId);

                var success = await _registrationRepo.AssignCondensedTrainingAsync(request.RegistrationId, request.ModuleIds, adminId);

                if (success)
                {
                    return ApiResponseHelper.Success(null, "Training modules assigned successfully.");
                }

                return ApiResponseHelper.Failure("Failed to assign training.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Assign training failed: {Message}", ex.Message);
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning training for registration {RegistrationId}", request.RegistrationId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while assigning training."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/SupervisorRegistration/admin/complete-training
        /// Mark condensed training as completed
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/complete-training")]
        public async Task<IActionResult> CompleteTraining([FromBody] CompleteTrainingRequest request)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                _logger.LogInformation("Admin {AdminId} marking training complete for registration {RegistrationId}", adminId, request.RegistrationId);

                var success = await _registrationRepo.CompleteCondensedTrainingAsync(request.RegistrationId, adminId);

                if (success)
                {
                    return ApiResponseHelper.Success(null, "Training completed. Registration moved to certification stage.");
                }

                return ApiResponseHelper.Failure("Failed to complete training.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Complete training failed: {Message}", ex.Message);
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing training for registration {RegistrationId}", request.RegistrationId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while completing training."));
            }
        }

        // =============================================
        // ADMIN - Stage 4: Certification
        // =============================================

        /// <summary>
        /// GET: api/Supervisor/SupervisorRegistration/admin/pending-certification
        /// Get registrations pending certification
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/pending-certification")]
        public async Task<IActionResult> GetPendingCertification()
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                var registrations = await _registrationRepo.GetRegistrationsPendingCertificationAsync();

                return ApiResponseHelper.Success(registrations, $"Found {registrations.Count} registration(s) pending certification.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending certification registrations");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching pending certifications."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/SupervisorRegistration/admin/schedule-certification
        /// Schedule quick certification exam
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/schedule-certification")]
        public async Task<IActionResult> ScheduleCertification([FromBody] ScheduleCertificationRequest request)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                _logger.LogInformation("Admin {AdminId} scheduling certification for registration {RegistrationId} on {Date}",
                    adminId, request.RegistrationId, request.ExamDate.ToString("yyyy-MM-dd"));

                var success = await _registrationRepo.ScheduleQuickCertificationAsync(request.RegistrationId, request.ExamDate, adminId);

                if (success)
                {
                    return ApiResponseHelper.Success(null, "Certification exam scheduled. Supervisor will be notified.");
                }

                return ApiResponseHelper.Failure("Failed to schedule certification.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Schedule certification failed: {Message}", ex.Message);
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling certification for registration {RegistrationId}", request.RegistrationId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while scheduling certification."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/SupervisorRegistration/admin/certification-result
        /// Submit certification test result
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/certification-result")]
        public async Task<IActionResult> SubmitCertificationResult([FromBody] QuickCertificationResultDto result)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                result.EvaluatedBy = adminId;

                _logger.LogInformation("Admin {AdminId} submitting certification result for registration {RegistrationId}. Passed: {Passed}, Score: {Score}",
                    adminId, result.RegistrationId, result.Passed, result.ExamScore);

                var success = await _registrationRepo.SubmitQuickCertificationResultAsync(result);

                if (success)
                {
                    var message = result.Passed
                        ? "Certification passed. Supervisor is ready for activation."
                        : "Certification failed. Supervisor may retake the exam.";
                    return ApiResponseHelper.Success(null, message);
                }

                return ApiResponseHelper.Failure("Failed to submit certification result.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Submit certification result failed: {Message}", ex.Message);
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting certification result for registration {RegistrationId}", result.RegistrationId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while submitting certification result."));
            }
        }

        // =============================================
        // ADMIN - Final Activation
        // =============================================

        /// <summary>
        /// POST: api/Supervisor/SupervisorRegistration/admin/activate
        /// Activate a registered supervisor (final step)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/activate")]
        public async Task<IActionResult> ActivateSupervisor([FromBody] ActivateRegistrationRequest request)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                _logger.LogInformation("Admin {AdminId} activating registration {RegistrationId}", adminId, request.RegistrationId);

                var success = await _registrationRepo.ActivateRegisteredSupervisorAsync(request.RegistrationId, adminId);

                if (success)
                {
                    _logger.LogInformation("Registration {RegistrationId} activated by admin {AdminId}", request.RegistrationId, adminId);
                    return ApiResponseHelper.Success(null, "Supervisor activated successfully. They can now receive event assignments.");
                }

                return ApiResponseHelper.Failure("Failed to activate supervisor. Ensure all stages are completed.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Activation failed: {Message}", ex.Message);
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating registration {RegistrationId}", request.RegistrationId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while activating supervisor."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/SupervisorRegistration/admin/reject
        /// Reject a registration at any stage
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/reject")]
        public async Task<IActionResult> RejectRegistration([FromBody] RejectRegistrationRequest request)
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
                    return ApiResponseHelper.Failure("Rejection reason is required.");
                }

                _logger.LogInformation("Admin {AdminId} rejecting registration {RegistrationId}. Reason: {Reason}",
                    adminId, request.RegistrationId, request.Reason);

                var success = await _registrationRepo.RejectRegistrationAsync(request.RegistrationId, adminId, request.Reason);

                if (success)
                {
                    _logger.LogInformation("Registration {RegistrationId} rejected by admin {AdminId}", request.RegistrationId, adminId);
                    return ApiResponseHelper.Success(null, "Registration rejected. Supervisor will be notified.");
                }

                return ApiResponseHelper.Failure("Failed to reject registration.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting registration {RegistrationId}", request.RegistrationId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while rejecting registration."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/SupervisorRegistration/admin/progress-stage
        /// Manually progress registration to next stage
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/progress-stage")]
        public async Task<IActionResult> ProgressStage([FromBody] ProgressStageRequest request)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                if (string.IsNullOrWhiteSpace(request.NextStage))
                {
                    return ApiResponseHelper.Failure("Next stage is required.");
                }

                _logger.LogInformation("Admin {AdminId} progressing registration {RegistrationId} to stage {NextStage}",
                    adminId, request.RegistrationId, request.NextStage);

                var success = await _registrationRepo.ProgressRegistrationStageAsync(
                    request.RegistrationId, request.NextStage, adminId, request.Notes);

                if (success)
                {
                    return ApiResponseHelper.Success(null, $"Registration progressed to {request.NextStage} stage.");
                }

                return ApiResponseHelper.Failure("Failed to progress stage. Ensure the current stage is complete.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Progress stage failed: {Message}", ex.Message);
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error progressing stage for registration {RegistrationId}", request.RegistrationId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while progressing stage."));
            }
        }
    }

    #region Registration Request Models

    public class AssignTrainingRequest
    {
        public long RegistrationId { get; set; }
        public List<long> ModuleIds { get; set; }
    }

    public class CompleteTrainingRequest
    {
        public long RegistrationId { get; set; }
    }

    public class ScheduleCertificationRequest
    {
        public long RegistrationId { get; set; }
        public DateTime ExamDate { get; set; }
    }

    public class ActivateRegistrationRequest
    {
        public long RegistrationId { get; set; }
    }

    public class RejectRegistrationRequest
    {
        public long RegistrationId { get; set; }
        public string Reason { get; set; }
    }

    public class ProgressStageRequest
    {
        public long RegistrationId { get; set; }
        public string NextStage { get; set; }
        public string Notes { get; set; }
    }

    #endregion
}
