using System;
using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Models.Sample
{
    /// <summary>
    /// Represents a sample order for tasting menu items before placing full event order
    /// </summary>
    public class SampleOrderModel
    {
        public long SampleOrderID { get; set; }

        [Required]
        public long UserID { get; set; }

        [Required]
        public long CateringID { get; set; }

        // Pricing (NEVER derived from package price)
        [Required]
        [Range(0, 999999.99)]
        public decimal SamplePriceTotal { get; set; }

        [Range(0, 9999.99)]
        public decimal DeliveryCharge { get; set; } = 0;

        [Required]
        [Range(0, 999999.99)]
        public decimal TotalAmount { get; set; }

        // Status Management
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "SAMPLE_REQUESTED";

        [Required]
        [MaxLength(50)]
        public string PaymentStatus { get; set; } = "PENDING";

        public bool IsPaid { get; set; } = false;

        // Addresses
        [Required]
        public long DeliveryAddressID { get; set; }

        [Required]
        [MaxLength(500)]
        public string PickupAddress { get; set; } = string.Empty;

        public decimal? PickupLatitude { get; set; }

        public decimal? PickupLongitude { get; set; }

        public decimal? DeliveryLatitude { get; set; }

        public decimal? DeliveryLongitude { get; set; }

        // Payment Reference
        public long? PaymentID { get; set; }

        [MaxLength(100)]
        public string? PaymentGatewayOrderID { get; set; }

        [MaxLength(100)]
        public string? PaymentGatewayTransactionID { get; set; }

        // Partner Response
        public DateTime? PartnerResponseDate { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        // Third-Party Delivery Integration
        [MaxLength(50)]
        public string? DeliveryProvider { get; set; }

        [MaxLength(100)]
        public string? DeliveryPartnerOrderID { get; set; }

        [MaxLength(200)]
        public string? DeliveryPartnerName { get; set; }

        [MaxLength(20)]
        [Phone]
        public string? DeliveryPartnerPhone { get; set; }

        [MaxLength(50)]
        public string? DeliveryVehicleNumber { get; set; }

        public DateTime? EstimatedPickupTime { get; set; }

        public DateTime? ActualPickupTime { get; set; }

        public DateTime? EstimatedDeliveryTime { get; set; }

        public DateTime? ActualDeliveryTime { get; set; }

        // Customer Feedback & Conversion
        [MaxLength(1000)]
        public string? ClientFeedback { get; set; }

        [Range(1, 5)]
        public int? TasteRating { get; set; }

        [Range(1, 5)]
        public int? HygieneRating { get; set; }

        [Range(1, 5)]
        public int? OverallRating { get; set; }

        public DateTime? FeedbackDate { get; set; }

        public bool ConvertedToEventOrder { get; set; } = false;

        public long? EventOrderID { get; set; }

        public DateTime? ConversionDate { get; set; }

        // Audit Fields
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime LastModifiedDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;
    }
}
