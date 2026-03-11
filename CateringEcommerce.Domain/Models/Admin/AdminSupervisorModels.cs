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
}
