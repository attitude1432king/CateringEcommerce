using System;
using System.Collections.Generic;

namespace CateringEcommerce.Domain.Models.Owner
{
    // Order Filter DTO
    public class OrderFilterDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? OrderStatus { get; set; } // null = all
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? EventType { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string? SearchTerm { get; set; } // Order number or customer name
        public string? SortBy { get; set; } = "OrderDate"; // OrderDate, EventDate, Amount
        public string? SortOrder { get; set; } = "DESC"; // ASC, DESC
        public List<string>? ExcludeStatuses { get; set; } // e.g. ["Pending"] to exclude booking-stage orders
    }

    // Order List Item DTO
    public class OrderListItemDto
    {
        public long OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? EventType { get; set; }
        public DateTime EventDate { get; set; }
        public string? EventTime { get; set; }
        public string? VenueAddress { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public string? OrderStatus { get; set; }
        public string? PaymentStatus { get; set; }
        public int GuestCount { get; set; }
        public int DaysUntilEvent { get; set; }
        public List<string> MenuItems { get; set; } = new List<string>();
    }

    // Order Detail DTO
    public class OrderDetailDto
    {
        public long OrderId { get; set; }
        public string? OrderNumber { get; set; }

        // Customer Info
        public long CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }

        // Event Info
        public string? EventType { get; set; }
        public DateTime EventDate { get; set; }
        public string? EventTime { get; set; }
        public int GuestCount { get; set; }
        public string? VenueAddress { get; set; }
        public string? VenueCity { get; set; }
        public string? VenueState { get; set; }
        public string? VenuePincode { get; set; }

        // Order Info
        public DateTime OrderDate { get; set; }
        public string? OrderStatus { get; set; }
        public string? PaymentStatus { get; set; }
        public string? SpecialInstructions { get; set; }

        // Financial Info
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DeliveryCharges { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceAmount { get; set; }

        // Items
        public List<OrderItemDetailDto> Items { get; set; } = new List<OrderItemDetailDto>();

        // Status History
        public List<OrderStatusHistoryDto> StatusHistory { get; set; } = new List<OrderStatusHistoryDto>();

        // Additional Services
        public bool HasDecorations { get; set; }
        public bool HasStaff { get; set; }
        public string? DecorationsDetails { get; set; }
        public string? StaffDetails { get; set; }
    }

    // Order Item Detail DTO
    public class OrderItemDetailDto
    {
        public long OrderItemId { get; set; }
        public long MenuItemId { get; set; }
        public string? MenuItemName { get; set; }
        public string? Category { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? ImageUrl { get; set; }
        public string? SpecialRequest { get; set; }
        public string? PackageSelections { get; set; }
    }

    // Order Status Update DTO
    public class OrderStatusUpdateDto
    {
        public string? NewStatus { get; set; }
        public string? Comments { get; set; }
        public DateTime? EstimatedDeliveryTime { get; set; }
    }

    // Order Status History DTO
    public class OrderStatusHistoryDto
    {
        public long StatusId { get; set; }
        public string? Status { get; set; }
        public DateTime ChangedDate { get; set; }
        public string? ChangedBy { get; set; }
        public string? Comments { get; set; }
    }

    // Paginated Orders Response DTO
    public class PaginatedOrdersDto
    {
        public List<OrderListItemDto> Orders { get; set; } = new List<OrderListItemDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    // Order Stats DTO
    public class OrderStatsDto
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    // Booking Request Stats DTO (today/week/month breakdowns)
    public class BookingRequestStatsDto
    {
        public int TodayRequests { get; set; }
        public int WeekRequests { get; set; }
        public int MonthRequests { get; set; }
        public int TotalPending { get; set; }
        public int TotalConfirmed { get; set; }
        public int TotalRejected { get; set; }
    }

    // ─── Sample Tasting Request DTOs ──────────────────────────────────────────

    // Filter DTO for sample requests list
    public class SampleListFilterDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? StatusFilter { get; set; }   // null=all, "SAMPLE_REQUESTED", "SAMPLE_ACCEPTED", "SAMPLE_REJECTED"
        public string? SearchTerm { get; set; }
    }

    // Sample request list item — for BookingRequests page
    public class SampleRequestListItemDto
    {
        public long SampleOrderId { get; set; }
        public long? LinkedOrderId { get; set; }
        public long? LinkedOrderItemId { get; set; }
        public string? SourceType { get; set; }
        public string? ParentOrderNumber { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public decimal SamplePriceTotal { get; set; }
        public decimal DeliveryCharge { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Status { get; set; }
        public string? PaymentStatus { get; set; }
        public string? PickupAddress { get; set; }
        public List<string> SampleItems { get; set; } = new List<string>();
        public DateTime RequestedDate { get; set; }
        public string? RejectionReason { get; set; }
    }

    // Paginated sample requests response
    public class PaginatedSampleRequestsDto
    {
        public List<SampleRequestListItemDto> Requests { get; set; } = new List<SampleRequestListItemDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    // Accept / Reject action DTO
    public class SampleRequestActionDto
    {
        public string? Action { get; set; }            // "Accept" or "Reject"
        public string? RejectionReason { get; set; }
        public string? SourceType { get; set; }
        public long? LinkedOrderId { get; set; }
        public long? LinkedOrderItemId { get; set; }
    }
}
