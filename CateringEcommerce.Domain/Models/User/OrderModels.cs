using CateringEcommerce.Domain.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Models.User
{
    // ===================================
    // CREATE ORDER REQUEST DTO
    // ===================================
    public class CreateOrderDto
    {
        [Required]
        public long CateringId { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string EventTime { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string EventType { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string EventLocation { get; set; } = string.Empty;

        [Required]
        [Range(1, 100000)]
        public int GuestCount { get; set; }

        [MaxLength(1000)]
        public string? SpecialInstructions { get; set; }

        [Required]
        [MaxLength(500)]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ContactPerson { get; set; } = string.Empty;

        [Required]
        [Phone]
        [MaxLength(20)]
        public string ContactPhone { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string ContactEmail { get; set; } = string.Empty;

        [Required]
        public decimal BaseAmount { get; set; }

        [Required]
        public decimal TaxAmount { get; set; }

        public decimal DeliveryCharges { get; set; } = 0;

        public decimal DiscountAmount { get; set; } = 0;

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = "COD"; // COD, BankTransfer

        public FileUploadDto? PaymentProof { get; set; }

        // Split Payment fields
        public bool EnableSplitPayment { get; set; } = false;

        public decimal? PreBookingAmount { get; set; } // 40% amount

        public decimal? PostEventAmount { get; set; } // 60% amount

        // Google Maps Location fields
        public decimal? EventLatitude { get; set; }

        public decimal? EventLongitude { get; set; }

        [MaxLength(200)]
        public string? EventPlaceId { get; set; }

        // Saved Address reference
        public long? SavedAddressId { get; set; }

        [Required]
        public List<CreateOrderItemDto> OrderItems { get; set; } = new List<CreateOrderItemDto>();
    }

    // ===================================
    // CREATE ORDER ITEM DTO
    // ===================================
    public class CreateOrderItemDto
    {
        [Required]
        [MaxLength(20)]
        public string ItemType { get; set; } = string.Empty; // Package, FoodItem, Decoration

        [Required]
        public long ItemId { get; set; }

        [Required]
        [MaxLength(200)]
        public string ItemName { get; set; } = string.Empty;

        [Required]
        [Range(1, 10000)]
        public int Quantity { get; set; } = 1;

        [Required]
        public decimal UnitPrice { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }

        public string? PackageSelections { get; set; } // JSON for package item selections
    }

    // ===================================
    // ORDER RESPONSE DTO
    // ===================================
    public class OrderDto
    {
        public long OrderId { get; set; }
        public long UserId { get; set; }
        public long CateringId { get; set; }
        public string CateringName { get; set; } = string.Empty;
        public string CateringLogo { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string EventTime { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string EventLocation { get; set; } = string.Empty;
        public int GuestCount { get; set; }
        public string? SpecialInstructions { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public decimal BaseAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DeliveryCharges { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;

        // Split Payment fields
        public bool PaymentSplitEnabled { get; set; }
        public decimal? PreBookingAmount { get; set; }
        public decimal? PostEventAmount { get; set; }
        public string? PreBookingStatus { get; set; }
        public string? PostEventStatus { get; set; }

        // Google Maps Location fields
        public decimal? EventLatitude { get; set; }
        public decimal? EventLongitude { get; set; }
        public string? EventPlaceId { get; set; }

        // Saved Address reference
        public long? SavedAddressId { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
        public OrderPaymentDto? Payment { get; set; }
        public List<OrderStatusHistoryDto> StatusHistory { get; set; } = new List<OrderStatusHistoryDto>();

        // Live Event Status (populated when order is InProgress or Completed with supervisor)
        public LiveEventStatusDto? LiveEventStatus { get; set; }
    }

    // ===================================
    // LIVE EVENT STATUS DTO
    // ===================================
    public class LiveEventStatusDto
    {
        public bool SupervisorAssigned { get; set; }
        public string? SupervisorName { get; set; }
        public string EventTimelineStage { get; set; } = string.Empty; // Prepared, Dispatched, Arrived, InProgress, Completed
        public DateTime? LastUpdatedAt { get; set; }
        public int? ActualGuestCount { get; set; }
        public int? ServiceQualityRating { get; set; }
        public string? SupervisorNotes { get; set; }
        public bool SupervisorReportSubmitted { get; set; }
        public bool PaymentRequestRaised { get; set; }
        public decimal? ExtraChargesAmount { get; set; }
        public decimal? FinalPayableAmount { get; set; }
    }

    // ===================================
    // ORDER ITEM DTO
    // ===================================
    public class OrderItemDto
    {
        public long OrderItemId { get; set; }
        public long OrderId { get; set; }
        public string ItemType { get; set; } = string.Empty;
        public long ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? PackageSelections { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    // ===================================
    // ORDER PAYMENT DTO
    // ===================================
    public class OrderPaymentDto
    {
        public long PaymentId { get; set; }
        public long OrderId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? PaymentGateway { get; set; }
        public string? TransactionId { get; set; }
        public string? PaymentProofPath { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty; // Pending, Success, Failed, AwaitingVerification
        public DateTime? PaymentDate { get; set; }
        public long? VerifiedBy { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    // ===================================
    // ORDER STATUS HISTORY DTO
    // ===================================
    public class OrderStatusHistoryDto
    {
        public long HistoryId { get; set; }
        public long OrderId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    // ===================================
    // ORDER LIST ITEM DTO (For Listing)
    // ===================================
    public class OrderListItemDto
    {
        public long OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public long CateringId { get; set; }
        public string CateringName { get; set; } = string.Empty;
        public string CateringLogo { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string EventType { get; set; } = string.Empty;
        public int GuestCount { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    // ===================================
    // CANCEL ORDER REQUEST DTO
    // ===================================
    public class CancelOrderDto
    {
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}
