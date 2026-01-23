namespace CateringEcommerce.Domain.Models.Admin
{
    #region Catering Management Models

    public class AdminCateringListRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SearchTerm { get; set; }
        public int? CityId { get; set; }
        public string? Status { get; set; } // Pending, Approved, Rejected, Blocked
        public string? VerificationStatus { get; set; } // Verified, Unverified
        public string? SortBy { get; set; } = "CreatedDate";
        public string? SortOrder { get; set; } = "DESC";
    }

    public class AdminCateringListItem
    {
        public long CateringId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public decimal? Rating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalEarnings { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
    }

    public class AdminCateringDetail
    {
        public long CateringId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AlternatePhone { get; set; }
        public string? GstNumber { get; set; }
        public string? FssaiNumber { get; set; }
        public string? PanNumber { get; set; }
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }
        public bool IsBlocked { get; set; }
        public string? BlockReason { get; set; }
        public decimal? AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal PlatformCommission { get; set; }
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? IfscCode { get; set; }
        public string? AccountHolderName { get; set; }
        public List<string> Images { get; set; } = new();
        public DateTime CreatedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? LastModified { get; set; }
    }

    public class AdminCateringStatusUpdate
    {
        public long CateringId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public long UpdatedBy { get; set; }
    }

    public class AdminCateringListResponse
    {
        public List<AdminCateringListItem> Caterings { get; set; } = new();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    #endregion
}
