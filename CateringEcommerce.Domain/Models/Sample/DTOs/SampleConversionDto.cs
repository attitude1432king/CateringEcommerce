using System;
using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Models.Sample.DTOs
{
    /// <summary>
    /// Request DTO for converting sample order to full event order
    /// </summary>
    public class ConvertToEventOrderRequest
    {
        [Required(ErrorMessage = "Sample order ID is required")]
        public long SampleOrderID { get; set; }

        [Required(ErrorMessage = "Event date is required")]
        public DateTime EventDate { get; set; }

        [Required(ErrorMessage = "Event time is required")]
        [MaxLength(20)]
        public string EventTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event type is required")]
        [MaxLength(100)]
        public string EventType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Guest count is required")]
        [Range(1, 100000)]
        public int GuestCount { get; set; }

        [MaxLength(1000)]
        public string? SpecialInstructions { get; set; }

        public bool UseSameAddress { get; set; } = true;

        public long? AlternateAddressID { get; set; }

        public bool ApplyConversionDiscount { get; set; } = true;
    }

    /// <summary>
    /// Response DTO for conversion to event order
    /// </summary>
    public class ConvertToEventOrderResponse
    {
        public bool IsSuccess { get; set; }
        public long? EventOrderID { get; set; }
        public string? OrderNumber { get; set; }
        public decimal? DiscountApplied { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? Message { get; set; }
        public DateTime? ConversionDate { get; set; }
    }

    /// <summary>
    /// DTO for conversion discount eligibility
    /// </summary>
    public class ConversionDiscountEligibilityDto
    {
        public bool IsEligible { get; set; }
        public string? IneligibilityReason { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal MaxDiscountAmount { get; set; }
        public DateTime? DiscountValidUntil { get; set; }
        public int DaysRemaining { get; set; }
        public bool HasProvidedFeedback { get; set; }
        public int? MinimumRatingRequired { get; set; }
    }

    /// <summary>
    /// DTO for sample to order conversion preview
    /// </summary>
    public class SampleToOrderPreviewDto
    {
        public long SampleOrderID { get; set; }
        public long CateringID { get; set; }
        public string CateringName { get; set; } = string.Empty;
        public decimal SampleOrderTotal { get; set; }
        public decimal EstimatedEventOrderTotal { get; set; }
        public decimal ConversionDiscount { get; set; }
        public decimal FinalEstimatedTotal { get; set; }
        public bool DiscountApplicable { get; set; }
        public string? DiscountTerms { get; set; }
        public DateTime? DiscountExpiryDate { get; set; }
        public bool AllItemsAvailable { get; set; }
        public string? UnavailableItemsMessage { get; set; }
    }
}
