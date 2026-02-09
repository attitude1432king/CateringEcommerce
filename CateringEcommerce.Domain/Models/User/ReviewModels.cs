using System;
using System.Collections.Generic;

namespace CateringEcommerce.Domain.Models.User
{
    /// <summary>
    /// User Review Submission Request
    /// </summary>
    public class SubmitReviewRequest
    {
        public long OrderId { get; set; }
        public long CateringId { get; set; }

        // Multi-dimensional Ratings (1.0 to 5.0)
        public decimal OverallRating { get; set; }
        public decimal? FoodQualityRating { get; set; }
        public decimal? HygieneRating { get; set; }
        public decimal? StaffBehaviorRating { get; set; }
        public decimal? DecorationRating { get; set; }
        public decimal? PunctualityRating { get; set; }

        // Review Content
        public string? ReviewTitle { get; set; }
        public string? ReviewComment { get; set; }

        // Media
        public List<string>? ReviewImageUrls { get; set; }
    }

    /// <summary>
    /// Review Submission Response
    /// </summary>
    public class SubmitReviewResponse
    {
        public long ReviewId { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; }
    }

    /// <summary>
    /// User's Review Detail (for display)
    /// </summary>
    public class UserReviewDetail
    {
        public long ReviewId { get; set; }
        public long OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public long CateringId { get; set; }
        public string CateringName { get; set; } = string.Empty;
        public string? CateringLogo { get; set; }

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
        public List<string> ReviewImages { get; set; } = new();

        // Owner Reply
        public string? OwnerReply { get; set; }
        public DateTime? OwnerReplyDate { get; set; }

        // Meta
        public DateTime ReviewDate { get; set; }
        public bool IsVerified { get; set; }
        public bool IsVisible { get; set; }
        public string? EventType { get; set; }
    }

    /// <summary>
    /// Catering Reviews (Public display for browsing)
    /// </summary>
    public class CateringReviewDisplayDto
    {
        public long ReviewId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserInitials { get; set; }

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
        public List<string> ReviewImages { get; set; } = new();

        // Owner Reply
        public string? OwnerReply { get; set; }
        public DateTime? OwnerReplyDate { get; set; }

        // Meta
        public DateTime ReviewDate { get; set; }
        public bool IsVerified { get; set; }
        public string? EventType { get; set; }
    }

    /// <summary>
    /// User's Review List (My Reviews page)
    /// </summary>
    public class UserReviewListItem
    {
        public long ReviewId { get; set; }
        public long OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public long CateringId { get; set; }
        public string CateringName { get; set; } = string.Empty;
        public string? CateringLogo { get; set; }
        public decimal OverallRating { get; set; }
        public string? ReviewTitle { get; set; }
        public string? ReviewComment { get; set; }
        public DateTime ReviewDate { get; set; }
        public bool HasOwnerReply { get; set; }
        public bool IsVisible { get; set; }
    }

    /// <summary>
    /// Check if user can review order
    /// </summary>
    public class CanReviewResponse
    {
        public bool CanReview { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool AlreadyReviewed { get; set; }
        public long? ExistingReviewId { get; set; }
    }

    /// <summary>
    /// Review Statistics for Catering
    /// </summary>
    public class ReviewStatsDto
    {
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int FiveStarCount { get; set; }
        public int FourStarCount { get; set; }
        public int ThreeStarCount { get; set; }
        public int TwoStarCount { get; set; }
        public int OneStarCount { get; set; }

        // Dimension Averages
        public decimal? AvgFoodQuality { get; set; }
        public decimal? AvgHygiene { get; set; }
        public decimal? AvgStaffBehavior { get; set; }
        public decimal? AvgDecoration { get; set; }
        public decimal? AvgPunctuality { get; set; }
    }

    /// <summary>
    /// Paginated Reviews for Catering Detail Page
    /// </summary>
    public class CateringReviewsResponse
    {
        public List<CateringReviewDisplayDto> Reviews { get; set; } = new();
        public ReviewStatsDto Stats { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Update Review Request (in case user wants to edit)
    /// </summary>
    public class UpdateReviewRequest
    {
        public long ReviewId { get; set; }

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
        public List<string>? ReviewImageUrls { get; set; }
    }
}
