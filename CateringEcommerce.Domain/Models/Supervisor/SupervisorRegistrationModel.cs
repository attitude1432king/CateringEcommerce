using System;
using System.Collections.Generic;

namespace CateringEcommerce.Domain.Models.Supervisor
{
    /// <summary>
    /// Supervisor Registration Workflow Model (FAST SCALING)
    /// MANDATORY ORDER: Register → Verify → Interview → Train → Certify → Activate
    /// </summary>
    public class SupervisorRegistrationModel
    {
        public long RegistrationId { get; set; }
        public long SupervisorId { get; set; }

        // Registration Details
        public string RegistrationNumber { get; set; }
        public DateTime RegisteredDate { get; set; }
        public string Source { get; set; } // WEBSITE, MOBILE_APP, REFERRAL
        public string ReferralCode { get; set; }

        // Document Verification (MANDATORY)
        public string DocumentVerificationStatus { get; set; } // PENDING, VERIFIED, REJECTED
        public long? DocumentVerifiedBy { get; set; }
        public DateTime? DocumentVerifiedDate { get; set; }
        public string DocumentRejectionReason { get; set; }

        // Short Interview (MANDATORY)
        public bool InterviewScheduled { get; set; }
        public DateTime? InterviewDate { get; set; }
        public string InterviewMode { get; set; } // VIDEO_CALL (default)
        public long? InterviewerId { get; set; }
        public bool InterviewCompleted { get; set; }
        public string InterviewNotes { get; set; }
        public string InterviewResult { get; set; } // PASSED, FAILED

        // Training Module (MANDATORY)
        public bool TrainingModuleAssigned { get; set; }
        public long? TrainingModuleId { get; set; }
        public DateTime? TrainingStartedDate { get; set; }
        public DateTime? TrainingCompletedDate { get; set; }
        public decimal? TrainingCompletionPercentage { get; set; }
        public bool TrainingPassed { get; set; }

        // Certification Test (MANDATORY)
        public bool CertificationTestAssigned { get; set; }
        public DateTime? CertificationTestDate { get; set; }
        public decimal? CertificationTestScore { get; set; }
        public bool CertificationTestPassed { get; set; }
        public string CertificationCertificateUrl { get; set; }

        // Activation
        public string ActivationStatus { get; set; } // PENDING, ACTIVATED, REJECTED, SUSPENDED
        public DateTime? ActivatedDate { get; set; }
        public long? ActivatedBy { get; set; }
        public string RejectionReason { get; set; }

        // Agreement
        public bool AgreementAccepted { get; set; }
        public DateTime? AgreementAcceptedDate { get; set; }
        public string AgreementUrl { get; set; }
        public string AgreementIpAddress { get; set; }

        // Availability Setup
        public bool AvailabilityConfigured { get; set; }
        public List<string> PreferredCities { get; set; }
        public List<string> PreferredLocalities { get; set; }
        public int? AvailableDaysPerWeek { get; set; }

        // Banking Setup (Post-Activation)
        public bool BankDetailsSubmitted { get; set; }
        public bool BankDetailsVerified { get; set; }
        public DateTime? BankVerificationDate { get; set; }

        // Audit
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    /// <summary>
    /// DTO for Supervisor Registration (Portal Submission)
    /// </summary>
    public class SubmitSupervisorRegistrationDto
    {
        // Personal Details (MANDATORY)
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string AlternatePhone { get; set; }
        public DateTime? DateOfBirth { get; set; }

        // Address (MANDATORY)
        public string AddressLine1 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Pincode { get; set; }
        public string Locality { get; set; }

        // Experience (MANDATORY)
        public int YearsOfExperience { get; set; }
        public string PreviousEmployer { get; set; }
        public string Specialization { get; set; }
        public List<string> LanguagesKnown { get; set; }

        // Identity (MANDATORY)
        public string IdentityType { get; set; } // AADHAAR, PAN, PASSPORT
        public string IdentityNumber { get; set; }
        public string IdentityProofUrl { get; set; } // Upload required
        public string PhotoUrl { get; set; } // Upload required

        // Availability (MANDATORY)
        public List<string> PreferredCities { get; set; }
        public List<string> PreferredLocalities { get; set; }
        public List<string> PreferredEventTypes { get; set; }
        public int AvailableDaysPerWeek { get; set; }

        // Agreement (MANDATORY)
        public bool AgreementAccepted { get; set; }

        // Source
        public string Source { get; set; } // WEBSITE, MOBILE_APP
        public string ReferralCode { get; set; }
    }

    /// <summary>
    /// DTO for Document Verification
    /// </summary>
    public class VerifyDocumentsDto
    {
        public long RegistrationId { get; set; }
        public long VerifiedBy { get; set; } // Admin ID
        public string VerificationStatus { get; set; } // VERIFIED, REJECTED
        public string RejectionReason { get; set; }
        public string VerificationNotes { get; set; }
    }

    /// <summary>
    /// DTO for Scheduling Short Interview
    /// </summary>
    public class ScheduleShortInterviewDto
    {
        public long RegistrationId { get; set; }
        public DateTime InterviewDate { get; set; }
        public long InterviewerId { get; set; }
        public string VideoCallLink { get; set; }
    }

    /// <summary>
    /// DTO for Recording Interview Result
    /// </summary>
    public class RecordShortInterviewResultDto
    {
        public long RegistrationId { get; set; }
        public long InterviewerId { get; set; }
        public string InterviewResult { get; set; } // PASSED, FAILED
        public string InterviewNotes { get; set; }
        public string SuitabilityScore { get; set; } // LOW, MEDIUM, HIGH
    }

    /// <summary>
    /// DTO for Assigning Training Module
    /// </summary>
    public class AssignTrainingModuleDto
    {
        public long RegistrationId { get; set; }
        public long TrainingModuleId { get; set; }
    }

    /// <summary>
    /// DTO for Training Progress Update
    /// </summary>
    public class UpdateTrainingProgressDto
    {
        public long RegistrationId { get; set; }
        public long SupervisorId { get; set; }
        public decimal CompletionPercentage { get; set; }
        public bool Completed { get; set; }
    }

    /// <summary>
    /// DTO for Certification Test Submission
    /// </summary>
    public class SubmitCertificationTestDto
    {
        public long RegistrationId { get; set; }
        public long SupervisorId { get; set; }
        public Dictionary<string, string> Answers { get; set; } // QuestionId -> Answer
    }

    /// <summary>
    /// DTO for Quick Certification Test Result (Registration Workflow)
    /// </summary>
    public class RegistrationCertificationTestResultDto
    {
        public long RegistrationId { get; set; }
        public decimal TestScore { get; set; }
        public bool Passed { get; set; }
        public string CertificateUrl { get; set; }
        public DateTime CertificationDate { get; set; }
    }

    /// <summary>
    /// DTO for Activation Decision
    /// </summary>
    public class ActivationDecisionDto
    {
        public long RegistrationId { get; set; }
        public long ActivatedBy { get; set; } // Admin ID
        public string Decision { get; set; } // ACTIVATED, REJECTED
        public string RejectionReason { get; set; }
    }

    /// <summary>
    /// DTO for Banking Details Submission (Post-Activation)
    /// </summary>
    public class SubmitBankingDetailsDto
    {
        public long SupervisorId { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankIFSC { get; set; }
        public string BankAccountHolderName { get; set; }
        public string BankName { get; set; }
        public string PANNumber { get; set; }
        public string CancelledChequeUrl { get; set; } // Proof
    }

    /// <summary>
    /// Registration Progress Summary (overall view)
    /// </summary>
    public class RegistrationProgressSummaryDto
    {
        public long RegistrationId { get; set; }
        public string RegistrationNumber { get; set; }
        public string SupervisorName { get; set; }
        public string SupervisorEmail { get; set; }
        public DateTime RegisteredDate { get; set; }

        // Stage Completion (MANDATORY ORDER)
        public bool DocumentVerified { get; set; }
        public bool InterviewCompleted { get; set; }
        public bool TrainingCompleted { get; set; }
        public bool CertificationCompleted { get; set; }

        // Current Stage
        public string CurrentStage { get; set; }
        public string CurrentStageStatus { get; set; }

        // Activation Status
        public string ActivationStatus { get; set; }
        public DateTime? ActivatedDate { get; set; }

        // Progress Percentage
        public int ProgressPercentage { get; set; } // 0-100

        // Blockers
        public List<string> PendingActions { get; set; }
    }

    /// <summary>
    /// DTO for Availability Configuration
    /// </summary>
    public class ConfigureAvailabilityDto
    {
        public long SupervisorId { get; set; }
        public List<string> PreferredCities { get; set; }
        public List<string> PreferredLocalities { get; set; }
        public List<string> PreferredEventTypes { get; set; }
        public Dictionary<string, bool> AvailabilityCalendar { get; set; } // Date -> Available
        public int MaxEventsPerMonth { get; set; }
    }
}
