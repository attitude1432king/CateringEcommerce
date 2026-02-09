using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Models.Supervisor;

namespace CateringEcommerce.Domain.Interfaces.Supervisor
{
    /// <summary>
    /// Supervisor Registration Repository - Handles 4-Stage Fast Activation Workflow
    /// APPLIED → DOCUMENT_VERIFICATION → AWAITING_INTERVIEW → AWAITING_TRAINING → AWAITING_CERTIFICATION → ACTIVE
    /// </summary>
    public interface IRegistrationRepository
    {
        // =============================================
        // REGISTRATION SUBMISSION
        // =============================================

        /// <summary>
        /// Submit new supervisor registration (Public)
        /// Creates supervisor record with type=REGISTERED
        /// </summary>
        Task<long> SubmitRegistrationAsync(SupervisorRegistrationSubmitDto registration);

        /// <summary>
        /// Get registration by ID
        /// </summary>
        Task<SupervisorRegistrationModel> GetRegistrationByIdAsync(long registrationId);

        /// <summary>
        /// Get registration by supervisor ID
        /// </summary>
        Task<SupervisorRegistrationModel> GetRegistrationBySupervisorIdAsync(long supervisorId);

        // =============================================
        // STAGE PROGRESSION
        // =============================================

        /// <summary>
        /// Progress registration to next stage
        /// Uses sp_ProgressRegistrationStatus stored procedure
        /// </summary>
        Task<bool> ProgressRegistrationStageAsync(long registrationId, string nextStage, long processedBy, string notes);

        /// <summary>
        /// Reject registration at any stage
        /// </summary>
        Task<bool> RejectRegistrationAsync(long registrationId, long rejectedBy, string reason);

        // =============================================
        // STAGE 1: DOCUMENT VERIFICATION
        // =============================================

        Task<bool> SubmitDocumentVerificationAsync(DocumentVerificationDto verification);
        Task<List<SupervisorRegistrationModel>> GetRegistrationsPendingDocumentVerificationAsync();

        // =============================================
        // STAGE 2: INTERVIEW (Simplified)
        // =============================================

        Task<bool> ScheduleQuickInterviewAsync(QuickInterviewDto interview);
        Task<bool> SubmitQuickInterviewResultAsync(QuickInterviewResultDto result);
        Task<List<SupervisorRegistrationModel>> GetRegistrationsPendingInterviewAsync();

        // =============================================
        // STAGE 3: TRAINING (Condensed)
        // =============================================

        Task<bool> AssignCondensedTrainingAsync(long registrationId, List<long> moduleIds, long assignedBy);
        Task<bool> CompleteCondensedTrainingAsync(long registrationId, long completedBy);
        Task<List<SupervisorRegistrationModel>> GetRegistrationsPendingTrainingAsync();

        // =============================================
        // STAGE 4: QUICK CERTIFICATION
        // =============================================

        Task<bool> ScheduleQuickCertificationAsync(long registrationId, DateTime examDate, long scheduledBy);
        Task<bool> SubmitQuickCertificationResultAsync(QuickCertificationResultDto result);
        Task<List<SupervisorRegistrationModel>> GetRegistrationsPendingCertificationAsync();

        // =============================================
        // BANKING DETAILS (For Payment)
        // =============================================

        Task<bool> SubmitBankingDetailsAsync(BankingDetailsDto banking);
        Task<BankingDetailsModel> GetBankingDetailsAsync(long supervisorId);

        // =============================================
        // FINAL ACTIVATION (No Probation)
        // =============================================

        Task<bool> ActivateRegisteredSupervisorAsync(long registrationId, long activatedBy);

        // =============================================
        // WORKFLOW TRACKING
        // =============================================

        Task<List<RegistrationProgressDto>> GetRegistrationProgressAsync(long registrationId);
        Task<RegistrationWorkflowStatusDto> GetWorkflowStatusAsync(long registrationId);

        // =============================================
        // ADMIN QUERIES
        // =============================================

        Task<List<SupervisorRegistrationModel>> GetAllRegistrationsAsync(string status = null);
        Task<List<SupervisorRegistrationModel>> GetRegistrationsByStageAsync(string stage);
        Task<RegistrationStatisticsDto> GetRegistrationStatisticsAsync();
        Task<List<SupervisorRegistrationModel>> SearchRegistrationsAsync(RegistrationSearchDto filters);
    }

    #region DTOs

    public class SupervisorRegistrationSubmitDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string IDProofType { get; set; }
        public string IDProofNumber { get; set; }
        public string IDProofUrl { get; set; }
        public string AddressProofUrl { get; set; }
        public string PhotoUrl { get; set; }
        public long? PreferredZoneId { get; set; }
        public bool HasPriorExperience { get; set; }
        public string PriorExperienceDetails { get; set; }
    }

    public class DocumentVerificationDto
    {
        public long RegistrationId { get; set; }
        public long VerifiedBy { get; set; }
        public bool Passed { get; set; }
        public bool IDProofVerified { get; set; }
        public bool AddressProofVerified { get; set; }
        public bool PhotoVerified { get; set; }
        public string VerificationNotes { get; set; }
    }

    public class QuickInterviewDto
    {
        public long RegistrationId { get; set; }
        public DateTime InterviewDateTime { get; set; }
        public string InterviewType { get; set; } // VIDEO, PHONE
        public string InterviewerName { get; set; }
        public string MeetingLink { get; set; }
        public long ScheduledBy { get; set; }
    }

    public class QuickInterviewResultDto
    {
        public long RegistrationId { get; set; }
        public long InterviewedBy { get; set; }
        public bool Passed { get; set; }
        public int Score { get; set; }
        public string Notes { get; set; }
    }

    public class QuickCertificationResultDto
    {
        public long RegistrationId { get; set; }
        public bool Passed { get; set; }
        public int ExamScore { get; set; }
        public DateTime ExamDate { get; set; }
        public string CertificateNumber { get; set; }
        public long EvaluatedBy { get; set; }
    }

    public class BankingDetailsDto
    {
        public long SupervisorId { get; set; }
        public string AccountHolderName { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string IFSCCode { get; set; }
        public string BranchName { get; set; }
        public string AccountType { get; set; } // SAVINGS, CURRENT
        public string CancelledChequeUrl { get; set; }
    }

    public class BankingDetailsModel
    {
        public long BankingId { get; set; }
        public long SupervisorId { get; set; }
        public string AccountHolderName { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string IFSCCode { get; set; }
        public string BranchName { get; set; }
        public string AccountType { get; set; }
        public string CancelledChequeUrl { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class RegistrationProgressDto
    {
        public string Stage { get; set; }
        public DateTime? CompletedDate { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsCurrentStage { get; set; }
        public string Notes { get; set; }
    }

    public class RegistrationWorkflowStatusDto
    {
        public long RegistrationId { get; set; }
        public string CurrentStage { get; set; }
        public string Status { get; set; }
        public int CompletedStages { get; set; }
        public int TotalStages { get; set; }
        public int ProgressPercentage { get; set; }
        public DateTime? ExpectedActivationDate { get; set; }
        public List<RegistrationProgressDto> StageHistory { get; set; }
    }

    public class RegistrationStatisticsDto
    {
        public int TotalRegistrations { get; set; }
        public int PendingRegistrations { get; set; }
        public int InProgressRegistrations { get; set; }
        public int ApprovedRegistrations { get; set; }
        public int RejectedRegistrations { get; set; }
        public Dictionary<string, int> RegistrationsByStage { get; set; }
        public double AverageProcessingDays { get; set; }
    }

    public class RegistrationSearchDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; }
        public string CurrentStage { get; set; }
        public long? ZoneId { get; set; }
        public DateTime? RegisteredFrom { get; set; }
        public DateTime? RegisteredTo { get; set; }
    }

    #endregion
}
