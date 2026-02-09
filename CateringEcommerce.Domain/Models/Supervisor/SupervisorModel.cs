using System;
using System.Collections.Generic;

namespace CateringEcommerce.Domain.Models.Supervisor
{
    /// <summary>
    /// Core Supervisor Entity (Shared by both portals)
    /// </summary>
    public class SupervisorModel
    {
        public long SupervisorId { get; set; }

        // Supervisor Type (CRITICAL DISTINCTION)
        public SupervisorType SupervisorType { get; set; }

        // Basic Information
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string AlternatePhone { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }

        // Address
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Pincode { get; set; }
        public string Locality { get; set; }

        // Identity & Documents
        public string IdentityType { get; set; }
        public string IdentityNumber { get; set; }
        public string IdentityProofUrl { get; set; }
        public string PhotoUrl { get; set; }
        public string ResumeUrl { get; set; } // Careers only

        // Experience
        public int? YearsOfExperience { get; set; }
        public string PreviousEmployer { get; set; }
        public string Specialization { get; set; }
        public List<string> LanguagesKnown { get; set; }

        // Status & Lifecycle
        public SupervisorStatus CurrentStatus { get; set; }
        public string StatusReason { get; set; }

        // Authority Level
        public AuthorityLevel AuthorityLevel { get; set; }
        public bool CanReleasePayment { get; set; }
        public bool CanApproveRefund { get; set; }
        public bool CanMentorOthers { get; set; }

        // Availability
        public bool IsAvailable { get; set; }
        public Dictionary<string, bool> AvailabilityCalendar { get; set; }
        public List<string> PreferredEventTypes { get; set; }
        public int? MaxEventsPerMonth { get; set; }

        // Banking
        public string BankAccountNumber { get; set; }
        public string BankIFSC { get; set; }
        public string BankAccountHolderName { get; set; }
        public string BankName { get; set; }
        public string PANNumber { get; set; }

        // Compensation
        public CompensationType? CompensationType { get; set; }
        public decimal? MonthlySalary { get; set; } // Careers only
        public decimal? PerEventRate { get; set; } // Registration only
        public decimal IncentivePercentage { get; set; }

        // Performance Metrics
        public int TotalEventsSupervised { get; set; }
        public decimal? AverageRating { get; set; }
        public int TotalRatingsReceived { get; set; }
        public int ComplaintsCount { get; set; }
        public int DisputeResolutionCount { get; set; }

        // Training & Certification
        public DateTime? TrainingCompletedDate { get; set; }
        public DateTime? CertificationDate { get; set; }
        public DateTime? CertificationValidUntil { get; set; }
        public CertificationStatus? CertificationStatus { get; set; }

        // Probation (Careers only)
        public DateTime? ProbationStartDate { get; set; }
        public DateTime? ProbationEndDate { get; set; }
        public bool? IsProbationPassed { get; set; }

        // Admin Controls
        public long? AssignedAdminId { get; set; }
        public long? SuspendedBy { get; set; }
        public DateTime? SuspensionDate { get; set; }
        public string SuspensionReason { get; set; }

        // Agreement
        public bool AgreementSigned { get; set; }
        public DateTime? AgreementSignedDate { get; set; }
        public string AgreementUrl { get; set; }
        public string BackgroundCheckStatus { get; set; }
        public DateTime? BackgroundCheckDate { get; set; }

        // Audit
        public DateTime CreatedDate { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public bool IsDeleted { get; set; }
    }

    /// <summary>
    /// DTO for creating a new supervisor (common fields)
    /// </summary>
    public class CreateSupervisorDto
    {
        public SupervisorType SupervisorType { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Pincode { get; set; }
        public int? YearsOfExperience { get; set; }
        public string IdentityType { get; set; }
        public string IdentityNumber { get; set; }
    }

    /// <summary>
    /// DTO for updating supervisor profile
    /// </summary>
    public class UpdateSupervisorProfileDto
    {
        public long SupervisorId { get; set; }
        public string Phone { get; set; }
        public string AlternatePhone { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Locality { get; set; }
        public List<string> LanguagesKnown { get; set; }
        public string Specialization { get; set; }
    }

    /// <summary>
    /// DTO for supervisor portal dashboard summary (supervisor's own view)
    /// </summary>
    public class SupervisorPortalDashboardDto
    {
        public long SupervisorId { get; set; }
        public string FullName { get; set; }
        public SupervisorType SupervisorType { get; set; }
        public SupervisorStatus CurrentStatus { get; set; }
        public AuthorityLevel AuthorityLevel { get; set; }

        // Performance Stats
        public int TotalEventsSupervised { get; set; }
        public int EventsThisMonth { get; set; }
        public decimal? AverageRating { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal EarningsThisMonth { get; set; }

        // Current Assignments
        public int UpcomingAssignments { get; set; }
        public int PendingApprovals { get; set; }

        // Authority Info
        public bool CanReleasePayment { get; set; }
        public bool CanApproveRefund { get; set; }
        public bool CanMentorOthers { get; set; }

        // Certification Status
        public CertificationStatus? CertificationStatus { get; set; }
        public DateTime? CertificationValidUntil { get; set; }
        public bool RequiresRenewal { get; set; }
    }

    #region Enums

    /// <summary>
    /// Supervisor Type (CRITICAL DISTINCTION)
    /// </summary>
    public enum SupervisorType
    {
        CAREER,      // Core, trusted, long-term (Careers Portal)
        REGISTERED   // Flexible, on-demand (Registration Portal)
    }

    /// <summary>
    /// Supervisor Status (Lifecycle)
    /// </summary>
    public enum SupervisorStatus
    {
        // Common Statuses
        APPLIED,
        REJECTED,
        ACTIVE,
        SUSPENDED,
        DEACTIVATED,
        BLACKLISTED,

        // Careers Portal Specific
        RESUME_SCREENED,
        INTERVIEW_SCHEDULED,
        INTERVIEW_PASSED,
        BACKGROUND_VERIFICATION,
        TRAINING,
        CERTIFIED,
        PROBATION,

        // Registration Portal Specific
        DOCUMENT_VERIFICATION,
        AWAITING_INTERVIEW,
        AWAITING_TRAINING,
        AWAITING_CERTIFICATION
    }

    /// <summary>
    /// Authority Level (Determines what actions supervisor can perform)
    /// </summary>
    public enum AuthorityLevel
    {
        BASIC,          // Can check-in, quality check, request extra payment
        INTERMEDIATE,   // BASIC + can handle medium complexity issues
        ADVANCED,       // INTERMEDIATE + can mentor, handle VIP events
        FULL            // ADVANCED + can release payments, approve refunds (Careers only)
    }

    /// <summary>
    /// Compensation Type
    /// </summary>
    public enum CompensationType
    {
        MONTHLY_SALARY,  // Careers supervisors
        PER_EVENT,       // Registered supervisors
        HYBRID           // Salary + per event incentive
    }

    /// <summary>
    /// Certification Status
    /// </summary>
    public enum CertificationStatus
    {
        PENDING,
        CERTIFIED,
        EXPIRED,
        SUSPENDED
    }

    #endregion
}
