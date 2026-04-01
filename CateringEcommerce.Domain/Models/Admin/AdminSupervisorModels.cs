namespace CateringEcommerce.Domain.Models.Admin
{
    #region Supervisor Registration Request Models (Tab 1: Pending)

    public class AdminSupervisorRegistrationListRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SearchTerm { get; set; }
        public int? Status { get; set; }  // SupervisorApprovalStatus enum: 0=Pending, 1=Approved, 2=Rejected, 3=UnderReview, 4=InfoRequested
        public string? SupervisorType { get; set; }  // CAREER, REGISTERED
        public string? SortBy { get; set; } = "CreatedDate";
        public string? SortOrder { get; set; } = "DESC";
    }

    public class AdminSupervisorRegistrationListItem
    {
        public long SupervisorId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string SupervisorType { get; set; } = string.Empty;
        public int Status { get; set; }
        public string? StatusReason { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
    }

    public class AdminSupervisorRegistrationListResponse
    {
        public List<AdminSupervisorRegistrationListItem> Registrations { get; set; } = new();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class AdminSupervisorStatusUpdate
    {
        public long SupervisorId { get; set; }
        public int Status { get; set; }
        public string? Reason { get; set; }
        public long UpdatedBy { get; set; }
    }

    #endregion

    #region Active Supervisor Models (Tab 2: Approved)

    public class AdminActiveSupervisorListRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SearchTerm { get; set; }
        public string? SupervisorType { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public bool? IsBlocked { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? SortBy { get; set; } = "CreatedDate";
        public string? SortOrder { get; set; } = "DESC";
    }

    public class AdminActiveSupervisorListItem
    {
        public long SupervisorId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string SupervisorType { get; set; } = string.Empty;
        public decimal? AverageRating { get; set; }
        public int TotalEventsSupervised { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastUpdated { get; set; }
    }

    public class AdminActiveSupervisorListResponse
    {
        public List<AdminActiveSupervisorListItem> Supervisors { get; set; } = new();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class AdminSupervisorExportItem
    {
        public long SupervisorId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string SupervisorType { get; set; } = string.Empty;
        public decimal? AverageRating { get; set; }
        public int TotalEventsSupervised { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    #endregion

    #region Supervisor Detail View

    public class AdminSupervisorDetailResponse
    {
        // Identity
        public long SupervisorId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? AlternatePhone { get; set; }
        public string? Gender { get; set; }
        public string? DateOfBirth { get; set; }

        // Address
        public string? AddressLine1 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string? Pincode { get; set; }
        public string? Locality { get; set; }

        // Type & Status
        public string SupervisorType { get; set; } = string.Empty;
        public int Status { get; set; }
        public string? StatusReason { get; set; }
        public string AuthorityLevel { get; set; } = string.Empty;

        // Experience
        public bool HasPriorExperience { get; set; }
        public string? PriorExperienceDetails { get; set; }
        public string? Specialization { get; set; }
        public string? LanguagesKnown { get; set; }

        // Identity & Documents
        public string? IdentityType { get; set; }
        public string? IdentityNumber { get; set; }
        public string? IdentityProofUrl { get; set; }
        public string? PhotoUrl { get; set; }
        public string? AddressProofUrl { get; set; }
        public string? ResumeUrl { get; set; }
        public string? AgreementUrl { get; set; }

        // Registration Workflow (from t_sys_supervisor_registration — may be null)
        public string? DocVerificationStatus { get; set; }
        public string? InterviewResult { get; set; }
        public bool TrainingCompleted { get; set; }
        public bool CertificationPassed { get; set; }
        public string? ActivationStatus { get; set; }

        // Banking (from t_sys_supervisor)
        public string? BankAccountHolderName { get; set; }
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankIfsc { get; set; }
        public string? CompensationType { get; set; }
        public decimal? PerEventRate { get; set; }
        public decimal? MonthlySalary { get; set; }
        public string? CancelledChequeUrl { get; set; }

        // Availability (from t_sys_supervisor)
        public string? AvailabilityCalendar { get; set; }
        public string? PreferredEventTypes { get; set; }
        public int? MaxEventsPerMonth { get; set; }

        // Performance
        public int TotalEventsSupervised { get; set; }
        public decimal? AverageRating { get; set; }
        public string? CertificationStatus { get; set; }

        // Dates
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    #endregion
}
