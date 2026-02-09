using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Models.Supervisor;

namespace CateringEcommerce.Domain.Interfaces.Supervisor
{
    /// <summary>
    /// Careers Application Repository - Handles 6-Stage Strict Workflow
    /// APPLIED → RESUME_SCREENED → INTERVIEW_PASSED → BACKGROUND_VERIFICATION → TRAINING → CERTIFIED → PROBATION → ACTIVE
    /// </summary>
    public interface ICareersApplicationRepository
    {
        // =============================================
        // APPLICATION SUBMISSION
        // =============================================

        /// <summary>
        /// Submit new careers application (Stage 1: APPLIED)
        /// </summary>
        Task<long> SubmitCareersApplicationAsync(CareersApplicationSubmitDto application);

        /// <summary>
        /// Get application by ID
        /// </summary>
        Task<CareersApplicationModel> GetApplicationByIdAsync(long applicationId);

        /// <summary>
        /// Get application by supervisor ID
        /// </summary>
        Task<CareersApplicationModel> GetApplicationBySupervisorIdAsync(long supervisorId);

        // =============================================
        // STAGE PROGRESSION (Admin Actions)
        // =============================================

        /// <summary>
        /// Progress application to next stage
        /// Uses sp_ProgressCareersApplication stored procedure
        /// </summary>
        Task<bool> ProgressApplicationStageAsync(long applicationId, string nextStage, long processedBy, string notes);

        /// <summary>
        /// Reject application at any stage
        /// </summary>
        Task<bool> RejectApplicationAsync(long applicationId, long rejectedBy, string reason);

        // =============================================
        // STAGE 2: RESUME SCREENING
        // =============================================

        Task<bool> SubmitResumeScreeningAsync(ResumeScreeningDto screening);
        Task<List<CareersApplicationModel>> GetApplicationsForResumeScreeningAsync();

        // =============================================
        // STAGE 3: INTERVIEW
        // =============================================

        Task<bool> ScheduleInterviewAsync(ScheduleInterviewDto interview);
        Task<bool> SubmitInterviewResultAsync(InterviewResultDto result);
        Task<List<CareersApplicationModel>> GetApplicationsForInterviewAsync();

        // =============================================
        // STAGE 4: BACKGROUND VERIFICATION
        // =============================================

        Task<bool> InitiateBackgroundCheckAsync(long applicationId, long initiatedBy);
        Task<bool> SubmitBackgroundCheckResultAsync(BackgroundCheckResultDto result);
        Task<List<CareersApplicationModel>> GetApplicationsPendingBackgroundCheckAsync();

        // =============================================
        // STAGE 5: TRAINING
        // =============================================

        Task<bool> AssignTrainingAsync(long applicationId, List<long> moduleIds, long assignedBy);
        Task<bool> RecordTrainingProgressAsync(long applicationId, long moduleId, int progressPercentage);
        Task<bool> CompleteTrainingAsync(long applicationId, long completedBy);
        Task<List<CareersApplicationModel>> GetApplicationsInTrainingAsync();

        // =============================================
        // STAGE 6: CERTIFICATION
        // =============================================

        Task<bool> ScheduleCertificationExamAsync(long applicationId, DateTime examDate, long scheduledBy);
        Task<bool> SubmitCertificationResultAsync(CertificationResultDto result);
        Task<List<CareersApplicationModel>> GetApplicationsPendingCertificationAsync();

        // =============================================
        // STAGE 7: PROBATION
        // =============================================

        Task<bool> StartProbationAsync(long applicationId, int probationDays, long startedBy);
        Task<bool> CompleteProbationAsync(long applicationId, bool passed, long evaluatedBy, string evaluation);
        Task<List<CareersApplicationModel>> GetApplicationsInProbationAsync();

        // =============================================
        // FINAL ACTIVATION
        // =============================================

        Task<bool> ActivateSupervisorAsync(long applicationId, long activatedBy);

        // =============================================
        // WORKFLOW TRACKING
        // =============================================

        Task<List<ApplicationProgressDto>> GetApplicationProgressAsync(long applicationId);
        Task<ApplicationWorkflowStatusDto> GetWorkflowStatusAsync(long applicationId);

        // =============================================
        // ADMIN QUERIES
        // =============================================

        Task<List<CareersApplicationModel>> GetAllApplicationsAsync(string status = null);
        Task<List<CareersApplicationModel>> GetApplicationsByStageAsync(string stage);
        Task<ApplicationStatisticsDto> GetApplicationStatisticsAsync();
        Task<List<CareersApplicationModel>> SearchApplicationsAsync(ApplicationSearchDto filters);
    }

    #region DTOs

    public class CareersApplicationSubmitDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string ResumeUrl { get; set; }
        public string CoverLetter { get; set; }
        public int YearsOfExperience { get; set; }
        public string PreviousEmployer { get; set; }
        public string References { get; set; }
    }

    public class ResumeScreeningDto
    {
        public long ApplicationId { get; set; }
        public long ScreenedBy { get; set; }
        public bool Passed { get; set; }
        public int ResumeScore { get; set; }
        public string ScreeningNotes { get; set; }
    }

    public class ScheduleInterviewDto
    {
        public long ApplicationId { get; set; }
        public DateTime InterviewDateTime { get; set; }
        public string InterviewType { get; set; } // VIDEO, IN_PERSON, PHONE
        public string InterviewerName { get; set; }
        public string MeetingLink { get; set; }
        public long ScheduledBy { get; set; }
    }

    public class InterviewResultDto
    {
        public long ApplicationId { get; set; }
        public long InterviewedBy { get; set; }
        public bool Passed { get; set; }
        public int InterviewScore { get; set; }
        public string InterviewNotes { get; set; }
    }

    public class BackgroundCheckResultDto
    {
        public long ApplicationId { get; set; }
        public bool Passed { get; set; }
        public string VerificationAgency { get; set; }
        public DateTime VerificationDate { get; set; }
        public string VerificationReportUrl { get; set; }
        public string Notes { get; set; }
        public long SubmittedBy { get; set; }
    }

    public class CertificationResultDto
    {
        public long ApplicationId { get; set; }
        public bool Passed { get; set; }
        public int ExamScore { get; set; }
        public DateTime ExamDate { get; set; }
        public string CertificateNumber { get; set; }
        public string CertificateUrl { get; set; }
        public long EvaluatedBy { get; set; }
    }

    public class ApplicationProgressDto
    {
        public string Stage { get; set; }
        public DateTime? CompletedDate { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsCurrentStage { get; set; }
        public string Notes { get; set; }
    }

    public class ApplicationWorkflowStatusDto
    {
        public long ApplicationId { get; set; }
        public string CurrentStage { get; set; }
        public string Status { get; set; }
        public int CompletedStages { get; set; }
        public int TotalStages { get; set; }
        public int ProgressPercentage { get; set; }
        public DateTime? ExpectedCompletionDate { get; set; }
        public List<ApplicationProgressDto> StageHistory { get; set; }
    }

    public class ApplicationStatisticsDto
    {
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public int InProgressApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int RejectedApplications { get; set; }
        public Dictionary<string, int> ApplicationsByStage { get; set; }
        public double AverageProcessingDays { get; set; }
    }

    public class ApplicationSearchDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; }
        public string CurrentStage { get; set; }
        public DateTime? AppliedFrom { get; set; }
        public DateTime? AppliedTo { get; set; }
    }

    #endregion
}
