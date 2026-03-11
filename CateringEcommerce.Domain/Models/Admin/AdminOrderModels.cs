namespace CateringEcommerce.Domain.Models.Admin
{
    #region Order Management Models

    public class AdminOrderListRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SearchTerm { get; set; } // Search by order number, customer name, catering name
        public string? OrderStatus { get; set; } // Pending, Confirmed, InProgress, Completed, Cancelled
        public string? PaymentStatus { get; set; } // Pending, AdvancePaid, FullyPaid, Refunded
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public long? UserId { get; set; }
        public long? CateringOwnerId { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string? SortBy { get; set; } = "CreatedDate";
        public string? SortOrder { get; set; } = "DESC";
    }

    public class AdminOrderListItem
    {
        public long OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public long UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserEmail { get; set; }
        public string? UserPhone { get; set; }
        public long CateringOwnerId { get; set; }
        public string CateringName { get; set; } = string.Empty;
        public string? CateringOwnerName { get; set; }
        public DateTime EventDate { get; set; }
        public string EventType { get; set; } = string.Empty;
        public int GuestCount { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactPhone { get; set; }
    }

    public class AdminOrderListResponse
    {
        public List<AdminOrderListItem> Orders { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class AdminOrderDetail
    {
        // Order Info
        public long OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Customer Info
        public long UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserEmail { get; set; }
        public string? UserPhone { get; set; }

        // Catering Partner Info
        public long CateringOwnerId { get; set; }
        public string CateringName { get; set; } = string.Empty;
        public string? CateringOwnerName { get; set; }
        public string? CateringOwnerPhone { get; set; }

        // Event Details
        public DateTime EventDate { get; set; }
        public string EventType { get; set; } = string.Empty;
        public int GuestCount { get; set; }
        public string? EventLocation { get; set; }
        public string? VenueAddress { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }

        // Financial Details
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public decimal? AdvanceAmount { get; set; }
        public decimal? BalanceAmount { get; set; }
        public decimal? CommissionAmount { get; set; }
        public decimal? CommissionPercentage { get; set; }

        // Order Items
        public List<AdminOrderItemDetail> OrderItems { get; set; } = new();

        // Payment Stages
        public List<AdminOrderPaymentStage> PaymentStages { get; set; } = new();

        // Status History
        public List<AdminOrderStatusHistory> StatusHistory { get; set; } = new();
    }

    public class AdminOrderItemDetail
    {
        public long OrderItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty; // Package or FoodItem
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class AdminOrderPaymentStage
    {
        public long PaymentStageId { get; set; }
        public string StageType { get; set; } = string.Empty; // PreBooking, Balance, PostEvent
        public decimal StagePercentage { get; set; }
        public decimal StageAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? PaymentDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
    }

    public class AdminOrderStatusHistory
    {
        public long HistoryId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class AdminOrderUpdateStatusRequest
    {
        public long OrderId { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public long UpdatedBy { get; set; }
    }

    public class AdminOrderStatsResponse
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int InProgressOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal PendingRevenue { get; set; }
        public decimal TodayOrders { get; set; }
        public decimal ThisMonthOrders { get; set; }
    }

    #endregion
}
