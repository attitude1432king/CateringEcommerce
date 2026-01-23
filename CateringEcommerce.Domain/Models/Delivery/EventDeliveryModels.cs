namespace CateringEcommerce.Domain.Models.Delivery
{
    /// <summary>
    /// Event Catering Delivery - Status-based (NO GPS)
    /// </summary>
    public class EventDeliveryDto
    {
        public long EventDeliveryId { get; set; }
        public long OrderId { get; set; }
        public long OwnerId { get; set; }

        // Vehicle & Driver Information
        public string? VehicleNumber { get; set; }
        public string? DriverName { get; set; }
        public string? DriverPhone { get; set; }

        // Status-driven delivery
        public EventDeliveryStatus DeliveryStatus { get; set; }
        public string DeliveryStatusText => DeliveryStatus.ToString();

        // Scheduling & Timing
        public DateTime? ScheduledDispatchTime { get; set; }
        public DateTime? ActualDispatchTime { get; set; }
        public DateTime? ArrivedTime { get; set; }
        public DateTime? CompletedTime { get; set; }

        // Notes
        public string? Notes { get; set; }

        // System timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Request to initialize event delivery
    /// </summary>
    public class InitEventDeliveryRequest
    {
        public long OrderId { get; set; }
        public long OwnerId { get; set; }
        public DateTime? ScheduledDispatchTime { get; set; }
        public string? VehicleNumber { get; set; }
        public string? DriverName { get; set; }
        public string? DriverPhone { get; set; }
    }

    /// <summary>
    /// Request to update event delivery status
    /// </summary>
    public class UpdateEventDeliveryStatusRequest
    {
        public long EventDeliveryId { get; set; }
        public EventDeliveryStatus NewStatus { get; set; }
        public string? VehicleNumber { get; set; }
        public string? DriverName { get; set; }
        public string? DriverPhone { get; set; }
        public string? Notes { get; set; }
        public long? ChangedByUserId { get; set; }
        public string? ChangedByType { get; set; } // 'Partner', 'Admin', 'System'
    }

    /// <summary>
    /// Event Delivery Status History/Audit Log
    /// </summary>
    public class EventDeliveryHistoryDto
    {
        public long HistoryId { get; set; }
        public long EventDeliveryId { get; set; }
        public long OrderId { get; set; }
        public EventDeliveryStatus? PreviousStatus { get; set; }
        public EventDeliveryStatus NewStatus { get; set; }
        public long? ChangedByUserId { get; set; }
        public string? ChangedByType { get; set; }
        public string? Notes { get; set; }
        public DateTime ChangedAt { get; set; }
    }

    /// <summary>
    /// Event Delivery Status Enum
    /// Status flow: Preparation Started → Vehicle Ready → Dispatched → Arrived At Venue → Event Completed
    /// </summary>
    public enum EventDeliveryStatus
    {
        PreparationStarted = 1,
        VehicleReady = 2,
        Dispatched = 3,
        ArrivedAtVenue = 4,
        EventCompleted = 5
    }

    /// <summary>
    /// Response for delivery timeline
    /// </summary>
    public class DeliveryTimelineResponse
    {
        public EventDeliveryDto? EventDelivery { get; set; }
        public List<EventDeliveryHistoryDto> StatusHistory { get; set; } = new();
        public string CurrentStatusText { get; set; } = string.Empty;
        public bool CanAdvanceStatus { get; set; }
        public EventDeliveryStatus? NextAllowedStatus { get; set; }
    }

    /// <summary>
    /// Admin delivery monitoring view
    /// </summary>
    public class AdminDeliveryMonitorDto
    {
        public long OrderId { get; set; }
        public long OwnerId { get; set; }
        public string? OwnerBusinessName { get; set; }
        public EventDeliveryStatus CurrentStatus { get; set; }
        public DateTime? ScheduledDispatchTime { get; set; }
        public DateTime? ActualDispatchTime { get; set; }
        public bool IsDelayed { get; set; }
        public int? DelayMinutes { get; set; }
        public DateTime EventDate { get; set; }
        public string? VehicleNumber { get; set; }
        public string? DriverName { get; set; }
    }
}
