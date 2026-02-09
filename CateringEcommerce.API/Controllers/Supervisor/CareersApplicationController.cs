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
    /// Careers Application Controller
    /// Admin manages the 6-stage strict careers pipeline:
    /// APPLIED → RESUME_SCREENED → INTERVIEW_PASSED → BACKGROUND_VERIFICATION → TRAINING → CERTIFIED → PROBATION → ACTIVE
    /// Careers Portal is static page only - no public submission frontend (admin-managed)
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/Supervisor/[controller]")]
    public class CareersApplicationController : ControllerBase
    {
        private readonly ILogger<CareersApplicationController> _logger;
        private readonly ICareersApplicationRepository _careersRepo;

        public CareersApplicationController(
            ILogger<CareersApplicationController> logger,
            ICareersApplicationRepository careersRepo)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _careersRepo = careersRepo ?? throw new ArgumentNullException(nameof(careersRepo));
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
        // APPLICATION SUBMISSION & RETRIEVAL
        // =============================================

        /// <summary>
        /// POST: api/Supervisor/CareersApplication/submit
        /// Submit a new careers application
        /// </summary>
        [AllowAnonymous]
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitApplication([FromBody] CareersApplicationSubmitDto application)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(application.FirstName) || string.IsNullOrWhiteSpace(application.LastName))
                {
                    return ApiResponseHelper.Failure("First name and last name are required.");
                }

                if (string.IsNullOrWhiteSpace(application.Email))
                {
                    return ApiResponseHelper.Failure("Email is required.");
                }

                if (string.IsNullOrWhiteSpace(application.ResumeUrl))
                {
                    return ApiResponseHelper.Failure("Resume upload is required for careers applications.");
                }

                _logger.LogInformation("New careers application submitted: {Name}, {Email}",
                    application.FirstName + " " + application.LastName, application.Email);

                var applicationId = await _careersRepo.SubmitCareersApplicationAsync(application);

                if (applicationId > 0)
                {
                    _logger.LogInformation("Careers application submitted. ID: {ApplicationId}", applicationId);
                    return ApiResponseHelper.Success(
                        new { applicationId },
                        "Application submitted successfully. You will be contacted after resume screening.");
                }

                return ApiResponseHelper.Failure("Failed to submit application. Email may already be registered.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Application submission failed: {Message}", ex.Message);
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting careers application");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while submitting application."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/CareersApplication/{applicationId}
        /// Get application details
        /// </summary>
        [HttpGet("{applicationId}")]
        public async Task<IActionResult> GetApplication(long applicationId)
        {
            try
            {
                var application = await _careersRepo.GetApplicationByIdAsync(applicationId);

                if (application == null)
                {
                    return ApiResponseHelper.Failure("Application not found.");
                }

                return ApiResponseHelper.Success(application, "Application details retrieved.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching application {ApplicationId}", applicationId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching application details."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/CareersApplication/progress/{applicationId}
        /// Get application workflow progress
        /// </summary>
        [AllowAnonymous]
        [HttpGet("progress/{applicationId}")]
        public async Task<IActionResult> GetApplicationProgress(long applicationId)
        {
            try
            {
                var workflowStatus = await _careersRepo.GetWorkflowStatusAsync(applicationId);

                if (workflowStatus == null)
                {
                    return ApiResponseHelper.Failure("Application not found.");
                }

                return ApiResponseHelper.Success(workflowStatus, "Application progress retrieved.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching application progress for {ApplicationId}", applicationId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching application progress."));
            }
        }

        // =============================================
        // ADMIN - Pipeline Overview
        // =============================================

        /// <summary>
        /// GET: api/Supervisor/CareersApplication/admin/all
        /// Get all applications with optional status filter
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAllApplications([FromQuery] string status = null)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                var applications = await _careersRepo.GetAllApplicationsAsync(status);

                return ApiResponseHelper.Success(applications, $"Found {applications.Count} application(s).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all applications");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching applications."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/CareersApplication/admin/by-stage/{stage}
        /// Get applications at a specific stage
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/by-stage/{stage}")]
        public async Task<IActionResult> GetApplicationsByStage(string stage)
        {
            try
            {
                var applications = await _careersRepo.GetApplicationsByStageAsync(stage);

                return ApiResponseHelper.Success(applications, $"Found {applications.Count} application(s) at {stage} stage.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching applications at stage {Stage}", stage);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching applications."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/CareersApplication/admin/search
        /// Search applications with filters
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/search")]
        public async Task<IActionResult> SearchApplications([FromBody] ApplicationSearchDto filters)
        {
            try
            {
                var applications = await _careersRepo.SearchApplicationsAsync(filters);

                return ApiResponseHelper.Success(applications, $"Found {applications.Count} application(s).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching applications");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while searching applications."));
            }
        }

        /// <summary>
        /// GET: api/Supervisor/CareersApplication/admin/statistics
        /// Get application pipeline statistics
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/statistics")]
        public async Task<IActionResult> GetApplicationStatistics()
        {
            try
            {
                var statistics = await _careersRepo.GetApplicationStatisticsAsync();

                return ApiResponseHelper.Success(statistics, "Application statistics retrieved.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching application statistics");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching statistics."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/CareersApplication/admin/reject
        /// Reject application at any stage
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/reject")]
        public async Task<IActionResult> RejectApplication([FromBody] RejectCareersApplicationRequest request)
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

                _logger.LogInformation("Admin {AdminId} rejecting application {ApplicationId}. Reason: {Reason}",
                    adminId, request.ApplicationId, request.Reason);

                var success = await _careersRepo.RejectApplicationAsync(request.ApplicationId, adminId, request.Reason);

                if (success)
                {
                    return ApiResponseHelper.Success(null, "Application rejected. Applicant will be notified.");
                }

                return ApiResponseHelper.Failure("Failed to reject application.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting application {ApplicationId}", request.ApplicationId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while rejecting application."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/CareersApplication/admin/progress-stage
        /// Manually progress application to next stage
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/progress-stage")]
        public async Task<IActionResult> ProgressStage([FromBody] ProgressCareersStageRequest request)
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

                _logger.LogInformation("Admin {AdminId} progressing application {ApplicationId} to stage {NextStage}",
                    adminId, request.ApplicationId, request.NextStage);

                var success = await _careersRepo.ProgressApplicationStageAsync(
                    request.ApplicationId, request.NextStage, adminId, request.Notes);

                if (success)
                {
                    return ApiResponseHelper.Success(null, $"Application progressed to {request.NextStage} stage.");
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
                _logger.LogError(ex, "Error progressing stage for application {ApplicationId}", request.ApplicationId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while progressing stage."));
            }
        }

        // =============================================
        // ADMIN - Stage 2: Resume Screening
        // =============================================

        /// <summary>
        /// GET: api/Supervisor/CareersApplication/admin/pending-screening
        /// Get applications pending resume screening
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/pending-screening")]
        public async Task<IActionResult> GetPendingResumeScreening()
        {
            try
            {
                var applications = await _careersRepo.GetApplicationsForResumeScreeningAsync();

                return ApiResponseHelper.Success(applications, $"Found {applications.Count} application(s) pending resume screening.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending resume screenings");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching pending screenings."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/CareersApplication/admin/screen-resume
        /// Submit resume screening result
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/screen-resume")]
        public async Task<IActionResult> ScreenResume([FromBody] ResumeScreeningDto screening)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                screening.ScreenedBy = adminId;

                _logger.LogInformation("Admin {AdminId} screening resume for application {ApplicationId}. Passed: {Passed}, Score: {Score}",
                    adminId, screening.ApplicationId, screening.Passed, screening.ResumeScore);

                var success = await _careersRepo.SubmitResumeScreeningAsync(screening);

                if (success)
                {
                    var message = screening.Passed
                        ? "Resume approved. Application moved to interview stage."
                        : "Resume rejected. Applicant will be notified.";
                    return ApiResponseHelper.Success(null, message);
                }

                return ApiResponseHelper.Failure("Failed to submit resume screening.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error screening resume for application {ApplicationId}", screening.ApplicationId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while screening resume."));
            }
        }

        // =============================================
        // ADMIN - Stage 3: Interview
        // =============================================

        /// <summary>
        /// GET: api/Supervisor/CareersApplication/admin/pending-interview
        /// Get applications pending interview
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/pending-interview")]
        public async Task<IActionResult> GetPendingInterviews()
        {
            try
            {
                var applications = await _careersRepo.GetApplicationsForInterviewAsync();

                return ApiResponseHelper.Success(applications, $"Found {applications.Count} application(s) pending interview.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending interviews");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching pending interviews."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/CareersApplication/admin/schedule-interview
        /// Schedule interview for applicant
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/schedule-interview")]
        public async Task<IActionResult> ScheduleInterview([FromBody] ScheduleInterviewDto interview)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                interview.ScheduledBy = adminId;

                _logger.LogInformation("Admin {AdminId} scheduling interview for application {ApplicationId} on {Date}",
                    adminId, interview.ApplicationId, interview.InterviewDateTime.ToString("yyyy-MM-dd HH:mm"));

                var success = await _careersRepo.ScheduleInterviewAsync(interview);

                if (success)
                {
                    return ApiResponseHelper.Success(null, "Interview scheduled. Applicant will be notified.");
                }

                return ApiResponseHelper.Failure("Failed to schedule interview.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling interview for application {ApplicationId}", interview.ApplicationId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while scheduling interview."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/CareersApplication/admin/interview-result
        /// Submit interview result
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/interview-result")]
        public async Task<IActionResult> SubmitInterviewResult([FromBody] InterviewResultDto result)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                result.InterviewedBy = adminId;

                _logger.LogInformation("Admin {AdminId} submitting interview result for application {ApplicationId}. Passed: {Passed}",
                    adminId, result.ApplicationId, result.Passed);

                var success = await _careersRepo.SubmitInterviewResultAsync(result);

                if (success)
                {
                    var message = result.Passed
                        ? "Interview passed. Application moved to background verification."
                        : "Interview failed. Applicant will be notified.";
                    return ApiResponseHelper.Success(null, message);
                }

                return ApiResponseHelper.Failure("Failed to submit interview result.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting interview result for application {ApplicationId}", result.ApplicationId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while submitting interview result."));
            }
        }

        // =============================================
        // ADMIN - Stage 4: Background Verification
        // =============================================

        /// <summary>
        /// GET: api/Supervisor/CareersApplication/admin/pending-background-check
        /// Get applications pending background verification
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/pending-background-check")]
        public async Task<IActionResult> GetPendingBackgroundChecks()
        {
            try
            {
                var applications = await _careersRepo.GetApplicationsPendingBackgroundCheckAsync();

                return ApiResponseHelper.Success(applications, $"Found {applications.Count} application(s) pending background check.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending background checks");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching pending background checks."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/CareersApplication/admin/initiate-background-check
        /// Initiate background verification
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/initiate-background-check")]
        public async Task<IActionResult> InitiateBackgroundCheck([FromBody] InitiateBackgroundCheckRequest request)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                _logger.LogInformation("Admin {AdminId} initiating background check for application {ApplicationId}",
                    adminId, request.ApplicationId);

                var success = await _careersRepo.InitiateBackgroundCheckAsync(request.ApplicationId, adminId);

                if (success)
                {
                    return ApiResponseHelper.Success(null, "Background check initiated.");
                }

                return ApiResponseHelper.Failure("Failed to initiate background check.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating background check for application {ApplicationId}", request.ApplicationId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while initiating background check."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/CareersApplication/admin/background-check-result
        /// Submit background check result
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/background-check-result")]
        public async Task<IActionResult> SubmitBackgroundCheckResult([FromBody] BackgroundCheckResultDto result)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                result.SubmittedBy = adminId;

                _logger.LogInformation("Admin {AdminId} submitting background check result for application {ApplicationId}. Passed: {Passed}",
                    adminId, result.ApplicationId, result.Passed);

                var success = await _careersRepo.SubmitBackgroundCheckResultAsync(result);

                if (success)
                {
                    var message = result.Passed
                        ? "Background check passed. Application moved to training stage."
                        : "Background check failed. Applicant will be notified.";
                    return ApiResponseHelper.Success(null, message);
                }

                return ApiResponseHelper.Failure("Failed to submit background check result.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting background check result for application {ApplicationId}", result.ApplicationId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while submitting background check result."));
            }
        }

        // =============================================
        // ADMIN - Stage 5: Training
        // =============================================

        /// <summary>
        /// GET: api/Supervisor/CareersApplication/admin/in-training
        /// Get applications currently in training
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/in-training")]
        public async Task<IActionResult> GetApplicationsInTraining()
        {
            try
            {
                var applications = await _careersRepo.GetApplicationsInTrainingAsync();

                return ApiResponseHelper.Success(applications, $"Found {applications.Count} application(s) in training.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching applications in training");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching training applications."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/CareersApplication/admin/assign-training
        /// Assign training modules to applicant
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/assign-training")]
        public async Task<IActionResult> AssignTraining([FromBody] AssignCareersTrainingRequest request)
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

                _logger.LogInformation("Admin {AdminId} assigning {Count} training modules to application {ApplicationId}",
                    adminId, request.ModuleIds.Count, request.ApplicationId);

                var success = await _careersRepo.AssignTrainingAsync(request.ApplicationId, request.ModuleIds, adminId);

                if (success)
                {
                    return ApiResponseHelper.Success(null, "Training modules assigned successfully.");
                }

                return ApiResponseHelper.Failure("Failed to assign training.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning training for application {ApplicationId}", request.ApplicationId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while assigning training."));
            }
        }

        /// <summary>
        /// PUT: api/Supervisor/CareersApplication/admin/training-progress
        /// Record training progress for a module
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("admin/training-progress")]
        public async Task<IActionResult> RecordTrainingProgress([FromBody] TrainingProgressRequest request)
        {
            try
            {
                _logger.LogInformation("Recording training progress for application {ApplicationId}, module {ModuleId}: {Progress}%",
                    request.ApplicationId, request.ModuleId, request.ProgressPercentage);

                var success = await _careersRepo.RecordTrainingProgressAsync(
                    request.ApplicationId, request.ModuleId, request.ProgressPercentage);

                if (success)
                {
                    return ApiResponseHelper.Success(null, "Training progress updated.");
                }

                return ApiResponseHelper.Failure("Failed to update training progress.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording training progress for application {ApplicationId}", request.ApplicationId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while recording training progress."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/CareersApplication/admin/complete-training
        /// Mark training as completed
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/complete-training")]
        public async Task<IActionResult> CompleteTraining([FromBody] CompleteCareersTrainingRequest request)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                var success = await _careersRepo.CompleteTrainingAsync(request.ApplicationId, adminId);

                if (success)
                {
                    return ApiResponseHelper.Success(null, "Training completed. Application moved to certification stage.");
                }

                return ApiResponseHelper.Failure("Failed to complete training.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing training for application {ApplicationId}", request.ApplicationId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while completing training."));
            }
        }

        // =============================================
        // ADMIN - Stage 6: Certification
        // =============================================

        /// <summary>
        /// GET: api/Supervisor/CareersApplication/admin/pending-certification
        /// Get applications pending certification exam
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/pending-certification")]
        public async Task<IActionResult> GetPendingCertification()
        {
            try
            {
                var applications = await _careersRepo.GetApplicationsPendingCertificationAsync();

                return ApiResponseHelper.Success(applications, $"Found {applications.Count} application(s) pending certification.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending certifications");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching pending certifications."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/CareersApplication/admin/schedule-certification
        /// Schedule certification exam
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/schedule-certification")]
        public async Task<IActionResult> ScheduleCertification([FromBody] ScheduleCareersExamRequest request)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                _logger.LogInformation("Admin {AdminId} scheduling certification for application {ApplicationId} on {Date}",
                    adminId, request.ApplicationId, request.ExamDate.ToString("yyyy-MM-dd"));

                var success = await _careersRepo.ScheduleCertificationExamAsync(request.ApplicationId, request.ExamDate, adminId);

                if (success)
                {
                    return ApiResponseHelper.Success(null, "Certification exam scheduled.");
                }

                return ApiResponseHelper.Failure("Failed to schedule certification.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling certification for application {ApplicationId}", request.ApplicationId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while scheduling certification."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/CareersApplication/admin/certification-result
        /// Submit certification exam result
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/certification-result")]
        public async Task<IActionResult> SubmitCertificationResult([FromBody] CertificationResultDto result)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                result.EvaluatedBy = adminId;

                _logger.LogInformation("Admin {AdminId} submitting certification result for application {ApplicationId}. Passed: {Passed}, Score: {Score}",
                    adminId, result.ApplicationId, result.Passed, result.ExamScore);

                var success = await _careersRepo.SubmitCertificationResultAsync(result);

                if (success)
                {
                    var message = result.Passed
                        ? "Certification passed. Application moved to probation stage."
                        : "Certification failed. Applicant may retake the exam.";
                    return ApiResponseHelper.Success(null, message);
                }

                return ApiResponseHelper.Failure("Failed to submit certification result.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting certification result for application {ApplicationId}", result.ApplicationId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while submitting certification result."));
            }
        }

        // =============================================
        // ADMIN - Stage 7: Probation
        // =============================================

        /// <summary>
        /// GET: api/Supervisor/CareersApplication/admin/in-probation
        /// Get applications currently in probation
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/in-probation")]
        public async Task<IActionResult> GetApplicationsInProbation()
        {
            try
            {
                var applications = await _careersRepo.GetApplicationsInProbationAsync();

                return ApiResponseHelper.Success(applications, $"Found {applications.Count} application(s) in probation.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching applications in probation");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching probation applications."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/CareersApplication/admin/start-probation
        /// Start probation period
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/start-probation")]
        public async Task<IActionResult> StartProbation([FromBody] StartProbationRequest request)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                if (request.ProbationDays <= 0)
                {
                    return ApiResponseHelper.Failure("Probation days must be greater than zero.");
                }

                _logger.LogInformation("Admin {AdminId} starting {Days}-day probation for application {ApplicationId}",
                    adminId, request.ProbationDays, request.ApplicationId);

                var success = await _careersRepo.StartProbationAsync(request.ApplicationId, request.ProbationDays, adminId);

                if (success)
                {
                    return ApiResponseHelper.Success(null, $"Probation started ({request.ProbationDays} days).");
                }

                return ApiResponseHelper.Failure("Failed to start probation.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting probation for application {ApplicationId}", request.ApplicationId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while starting probation."));
            }
        }

        /// <summary>
        /// POST: api/Supervisor/CareersApplication/admin/complete-probation
        /// Complete probation with pass/fail evaluation
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/complete-probation")]
        public async Task<IActionResult> CompleteProbation([FromBody] CompleteProbationRequest request)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                _logger.LogInformation("Admin {AdminId} completing probation for application {ApplicationId}. Passed: {Passed}",
                    adminId, request.ApplicationId, request.Passed);

                var success = await _careersRepo.CompleteProbationAsync(
                    request.ApplicationId, request.Passed, adminId, request.Evaluation);

                if (success)
                {
                    var message = request.Passed
                        ? "Probation completed successfully. Supervisor is ready for full activation."
                        : "Probation failed. Applicant will be notified.";
                    return ApiResponseHelper.Success(null, message);
                }

                return ApiResponseHelper.Failure("Failed to complete probation evaluation.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing probation for application {ApplicationId}", request.ApplicationId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while completing probation."));
            }
        }

        // =============================================
        // ADMIN - Final Activation
        // =============================================

        /// <summary>
        /// POST: api/Supervisor/CareersApplication/admin/activate
        /// Final activation of career supervisor
        /// Sets authority to FULL after all stages completed
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/activate")]
        public async Task<IActionResult> ActivateSupervisor([FromBody] ActivateCareersRequest request)
        {
            try
            {
                long adminId = GetUserId();
                if (adminId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                _logger.LogInformation("Admin {AdminId} activating career supervisor for application {ApplicationId}", adminId, request.ApplicationId);

                var success = await _careersRepo.ActivateSupervisorAsync(request.ApplicationId, adminId);

                if (success)
                {
                    _logger.LogInformation("Career supervisor activated for application {ApplicationId}", request.ApplicationId);
                    return ApiResponseHelper.Success(null, "Career supervisor activated with FULL authority. They can now release payments and approve refunds.");
                }

                return ApiResponseHelper.Failure("Failed to activate supervisor. Ensure all stages including probation are completed.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Activation failed: {Message}", ex.Message);
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating career supervisor for application {ApplicationId}", request.ApplicationId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while activating supervisor."));
            }
        }
    }

    #region Careers Request Models

    public class RejectCareersApplicationRequest
    {
        public long ApplicationId { get; set; }
        public string Reason { get; set; }
    }

    public class ProgressCareersStageRequest
    {
        public long ApplicationId { get; set; }
        public string NextStage { get; set; }
        public string Notes { get; set; }
    }

    public class InitiateBackgroundCheckRequest
    {
        public long ApplicationId { get; set; }
    }

    public class AssignCareersTrainingRequest
    {
        public long ApplicationId { get; set; }
        public List<long> ModuleIds { get; set; }
    }

    public class TrainingProgressRequest
    {
        public long ApplicationId { get; set; }
        public long ModuleId { get; set; }
        public int ProgressPercentage { get; set; }
    }

    public class CompleteCareersTrainingRequest
    {
        public long ApplicationId { get; set; }
    }

    public class ScheduleCareersExamRequest
    {
        public long ApplicationId { get; set; }
        public DateTime ExamDate { get; set; }
    }

    public class StartProbationRequest
    {
        public long ApplicationId { get; set; }
        public int ProbationDays { get; set; } = 90;
    }

    public class CompleteProbationRequest
    {
        public long ApplicationId { get; set; }
        public bool Passed { get; set; }
        public string Evaluation { get; set; }
    }

    public class ActivateCareersRequest
    {
        public long ApplicationId { get; set; }
    }

    #endregion
}
