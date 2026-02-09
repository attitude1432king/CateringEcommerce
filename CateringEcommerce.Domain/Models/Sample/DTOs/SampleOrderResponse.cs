using System;
using System.Collections.Generic;

namespace CateringEcommerce.Domain.Models.Sample.DTOs
{
    /// <summary>
    /// Complete response DTO for sample order details
    /// </summary>
    public class SampleOrderResponse
    {
        public long SampleOrderID { get; set; }
        public long UserID { get; set; }
        public long CateringID { get; set; }
        public string CateringName { get; set; } = string.Empty;
        public string CateringLogo { get; set; } = string.Empty;
        public string CateringPhone { get; set; } = string.Empty;

        // Pricing
        public decimal SamplePriceTotal { get; set; }
        public decimal DeliveryCharge { get; set; }
        public decimal TotalAmount { get; set; }

        // Status
        public string Status { get; set; } = string.Empty;
        public string StatusDisplay { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public bool IsPaid { get; set; }

        // Addresses
        public long DeliveryAddressID { get; set; }
        public string DeliveryAddressFull { get; set; } = string.Empty;
        public string PickupAddress { get; set; } = string.Empty;
        public decimal? PickupLatitude { get; set; }
        public decimal? PickupLongitude { get; set; }
        public decimal? DeliveryLatitude { get; set; }
        public decimal? DeliveryLongitude { get; set; }

        // Payment
        public long? PaymentID { get; set; }
        public string? PaymentGatewayOrderID { get; set; }
        public string? PaymentGatewayTransactionID { get; set; }

        // Partner Response
        public DateTime? PartnerResponseDate { get; set; }
        public string? RejectionReason { get; set; }

        // Delivery
        public string? DeliveryProvider { get; set; }
        public string? DeliveryPartnerOrderID { get; set; }
        public string? DeliveryPartnerName { get; set; }
        public string? DeliveryPartnerPhone { get; set; }
        public string? DeliveryVehicleNumber { get; set; }
        public DateTime? EstimatedPickupTime { get; set; }
        public DateTime? ActualPickupTime { get; set; }
        public DateTime? EstimatedDeliveryTime { get; set; }
        public DateTime? ActualDeliveryTime { get; set; }

        // Feedback
        public string? ClientFeedback { get; set; }
        public int? TasteRating { get; set; }
        public int? HygieneRating { get; set; }
        public int? OverallRating { get; set; }
        public DateTime? FeedbackDate { get; set; }

        // Conversion
        public bool ConvertedToEventOrder { get; set; }
        public long? EventOrderID { get; set; }
        public DateTime? ConversionDate { get; set; }

        // Items
        public List<SampleOrderItemResponse> Items { get; set; } = new List<SampleOrderItemResponse>();

        // Tracking
        public SampleTrackingResponse? CurrentTracking { get; set; }

        // Timestamps
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }

        // Business Logic Flags
        public bool CanCancel { get; set; }
        public bool CanProvideFeedback { get; set; }
        public bool CanConvertToOrder { get; set; }
        public bool CanRequestRefund { get; set; }
    }

    /// <summary>
    /// Sample order item response DTO
    /// </summary>
    public class SampleOrderItemResponse
    {
        public long SampleItemID { get; set; }
        public long MenuItemID { get; set; }
        public string MenuItemName { get; set; } = string.Empty;
        public decimal SamplePrice { get; set; }
        public int SampleQuantity { get; set; }
        public string? Category { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? CuisineType { get; set; }
        public bool? IsVeg { get; set; }
        public bool IsFromPackage { get; set; }
        public long? PackageID { get; set; }
        public string? PackageName { get; set; }
    }
}
