namespace CateringEcommerce.Domain.Models.Admin
{
    #region User Management Models

    public class AdminUserListRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SearchTerm { get; set; }
        public bool? IsBlocked { get; set; }
        public string? SortBy { get; set; } = "CreatedDate";
        public string? SortOrder { get; set; } = "DESC";
    }

    public class AdminUserListItem
    {
        public long UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsPhoneVerified { get; set; }
        public bool IsBlocked { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public int TotalReviews { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLogin { get; set; }
    }

    public class AdminUserDetail
    {
        public long UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? ProfilePhoto { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsPhoneVerified { get; set; }
        public bool IsBlocked { get; set; }
        public string? BlockReason { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public int TotalReviews { get; set; }
        public decimal AverageRating { get; set; }
        public List<AdminUserOrderSummary> RecentOrders { get; set; } = new();
        public List<AdminUserReviewSummary> RecentReviews { get; set; } = new();
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLogin { get; set; }
    }

    public class AdminUserOrderSummary
    {
        public long OrderId { get; set; }
        public string CateringName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime EventDate { get; set; }
    }

    public class AdminUserReviewSummary
    {
        public long ReviewId { get; set; }
        public string CateringName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime ReviewDate { get; set; }
    }

    public class AdminUserStatusUpdate
    {
        public long UserId { get; set; }
        public bool IsBlocked { get; set; }
        public string? Reason { get; set; }
        public long UpdatedBy { get; set; }
    }

    public class AdminUserListResponse
    {
        public List<AdminUserListItem> Users { get; set; } = new();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    #endregion
}
