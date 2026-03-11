using System;
using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Models.Sample.DTOs
{
    /// <summary>
    /// DTO for listing sample orders (user side)
    /// </summary>
    public class SampleOrderListDto
    {
        public long SampleOrderID { get; set; }
        public long CateringID { get; set; }
        public string CateringName { get; set; } = string.Empty;
        public string CateringLogo { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusDisplay { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public bool IsPaid { get; set; }
        public int ItemCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? EstimatedDeliveryTime { get; set; }
        public bool CanTrack { get; set; }
        public bool CanProvideFeedback { get; set; }
        public bool ConvertedToEventOrder { get; set; }
    }

    /// <summary>
    /// DTO for partner (owner) sample order list
    /// </summary>
    public class PartnerSampleOrderListDto
    {
        public long SampleOrderID { get; set; }
        public long UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserPhone { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusDisplay { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public string DeliveryArea { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? PartnerResponseDate { get; set; }
        public bool RequiresAction { get; set; }
        public bool IsConverted { get; set; }
        public int? CustomerRating { get; set; }
    }

    /// <summary>
    /// Filter parameters for sample order listing
    /// </summary>
    public class SampleOrderFilterDto
    {
        public string? Status { get; set; }
        public string? PaymentStatus { get; set; }
        public long? CateringID { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? ConvertedToOrder { get; set; }
        public bool? HasFeedback { get; set; }

        // Pagination
        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        public string SortBy { get; set; } = "CreatedDate";
        public string SortOrder { get; set; } = "DESC";
    }
}
