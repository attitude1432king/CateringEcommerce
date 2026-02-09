using System;
using System.Collections.Generic;

namespace CateringEcommerce.Domain.Models.Owner
{
    // Owner Review List Request (filter + pagination)
    public class OwnerReviewFilterDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? Rating { get; set; } // Filter by star rating (1-5)
        public bool? HasReply { get; set; } // Filter replied/unreplied
        public string? SortBy { get; set; } = "ReviewDate"; // ReviewDate, Rating
        public string? SortOrder { get; set; } = "DESC";
    }

    // Single Review Item for listing
    public class OwnerReviewItemDto
    {
        public long ReviewId { get; set; }
        public long OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public long UserId { get; set; }
        public string CustomerName { get; set; } = string.Empty;

        // Ratings
        public decimal OverallRating { get; set; }
        public decimal? FoodQualityRating { get; set; }
        public decimal? HygieneRating { get; set; }
        public decimal? StaffBehaviorRating { get; set; }
        public decimal? DecorationRating { get; set; }
        public decimal? PunctualityRating { get; set; }

        // Content
        public string? ReviewTitle { get; set; }
        public string? ReviewComment { get; set; }
        public string? EventType { get; set; }

        // Owner Reply
        public string? OwnerReply { get; set; }
        public DateTime? OwnerReplyDate { get; set; }

        // Meta
        public DateTime ReviewDate { get; set; }
        public bool IsVerified { get; set; }
        public bool IsVisible { get; set; }
    }

    // Paginated Response
    public class PaginatedReviewsDto
    {
        public List<OwnerReviewItemDto> Reviews { get; set; } = new List<OwnerReviewItemDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    // Owner Reply Request
    public class OwnerReviewReplyDto
    {
        public string ReplyText { get; set; } = string.Empty;
    }

    // Review Stats for Owner Dashboard
    public class OwnerReviewStatsDto
    {
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int FiveStarCount { get; set; }
        public int FourStarCount { get; set; }
        public int ThreeStarCount { get; set; }
        public int TwoStarCount { get; set; }
        public int OneStarCount { get; set; }
        public int UnrepliedCount { get; set; }
        public decimal? AvgFoodQuality { get; set; }
        public decimal? AvgHygiene { get; set; }
        public decimal? AvgStaffBehavior { get; set; }
        public decimal? AvgPunctuality { get; set; }
    }
}
