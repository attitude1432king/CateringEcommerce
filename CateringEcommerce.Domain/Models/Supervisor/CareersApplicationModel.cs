using System;

namespace CateringEcommerce.Domain.Models.Supervisor
{
    /// <summary>
    /// Careers Application Workflow Model (STRICT PIPELINE)
    /// </summary>
    public class CareersApplicationModel
    {
        public long ApplicationId { get; set; }
        public long SupervisorId { get; set; }

        // Application Details
        public string ApplicationNumber { get; set; }
        public DateTime AppliedDate { get; set; }
        public string Source { get; set; } // WEBSITE, REFERRAL, LINKEDIN
        public string ReferralCode { get; set; }

        // Resume Screening Stage
        public bool ResumeScreened { get; set; }
        public long? ResumeScreenedBy { get; set; }
        public DateTime? ResumeScreenedDate { get; set; }
        public string ResumeScreeningNotes { get; set; }
        public string ResumeScreeningStatus { get; set; } // PASSED, FAILED, ON_HOLD

        // Interview Stage
        public bool InterviewScheduled { get; set; }
        public DateTime? InterviewDate { get; set; }
        public string InterviewMode { get; set; } // IN_PERSON, VIDEO_CALL, PHONE
        public long? InterviewerId { get; set; }
        public bool InterviewCompleted { get; set; }
        public string InterviewFeedback { get; set; }
        public decimal? InterviewScore { get; set; } // Out of 100
        public string InterviewResult { get; set; } // PASSED, FAILED

        // Background Verification Stage
        public bool BackgroundVerificationInitiated { get; set; }
        public string BackgroundVerificationAgency { get; set; }
        public DateTime? BackgroundVerificationDate { get; set; }
        public string BackgroundVerificationResult { get; set; } // CLEAR, ISSUES_FOUND, PENDING
        public string BackgroundVerificationReportUrl { get; set; }

        // Training Stage
        public long? TrainingBatchId { get; set; }
        public DateTime? TrainingStartDate { get; set; }
        public DateTime? TrainingEndDate { get; set; }
        public decimal? TrainingAttendancePercentage { get; set; }
        public bool TrainingCompleted { get; set; }

        // Certification Stage
        public DateTime? CertificationTestDate { get; set; }
        public decimal? CertificationTestScore { get; set; }
        public bool CertificationPassed { get; set; }
        public string CertificationCertificateUrl { get; set; }

        // Probation Stage
        public bool ProbationAssigned { get; set; }
        public DateTime? ProbationStartDate { get; set; }
        public int? ProbationDurationDays { get; set; }
        public long? ProbationSupervisorId { get; set; } // Mentor
        public DateTime? ProbationEvaluationDate { get; set; }
        public string ProbationEvaluationNotes { get; set; }
        public bool? ProbationPassed { get; set; }

        // Final Decision
        public string FinalDecision { get; set; } // ACCEPTED, REJECTED, ON_HOLD
        public DateTime? FinalDecisionDate { get; set; }
        public long? FinalDecisionBy { get; set; }
        public string RejectionReason { get; set; }

        // Onboarding
        public bool OnboardingCompleted { get; set; }
        public DateTime? JoiningDate { get; set; }
        public string OfferLetterUrl { get; set; }
        public string EmployeeId { get; set; }

        // Audit
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    // All DTOs for Careers Application workflow are defined in ICareersApplicationRepository.cs
    // to keep them close to the repository methods that use them.
}
