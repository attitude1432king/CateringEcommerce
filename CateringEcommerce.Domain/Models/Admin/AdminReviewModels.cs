namespace CateringEcommerce.Domain.Models.Admin
{
    #region Review Management Models

    public class AdminReviewListRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public long? CateringId { get; set; }
        public long? UserId { get; set; }
        public int? MinRating { get; set; }
        public int? MaxRating { get; set; }
        public bool? IsHidden { get; set; }
        public string? SortBy { get; set; } = "ReviewDate";
        public string? SortOrder { get; set; } = "DESC";
    }

    public class AdminReviewListItem
    {
        public long ReviewId { get; set; }
        public long CateringId { get; set; }
        public string CateringName { get; set; } = string.Empty;
        public long UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public bool IsHidden { get; set; }
        public string? HiddenReason { get; set; }
        public DateTime ReviewDate { get; set; }
        public long? OrderId { get; set; }
    }

    public class AdminReviewDetail
    {
        public long ReviewId { get; set; }
        public long CateringId { get; set; }
        public string CateringName { get; set; } = string.Empty;
        public long UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserPhone { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public bool IsHidden { get; set; }
        public string? HiddenReason { get; set; }
        public long? HiddenBy { get; set; }
        public DateTime? HiddenDate { get; set; }
        public DateTime ReviewDate { get; set; }
        public long? OrderId { get; set; }
        public List<string> ReviewImages { get; set; } = new();
    }

    public class AdminReviewHideRequest
    {
        public long ReviewId { get; set; }
        public bool IsHidden { get; set; }
        public string? Reason { get; set; }
        public long UpdatedBy { get; set; }
    }

    public class AdminReviewListResponse
    {
        public List<AdminReviewListItem> Reviews { get; set; } = new();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    #endregion
}
