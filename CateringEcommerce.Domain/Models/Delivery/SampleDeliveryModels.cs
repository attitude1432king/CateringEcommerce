namespace CateringEcommerce.Domain.Models.Delivery
{
    /// <summary>
    /// Sample Delivery - Third-party real-time tracking
    /// </summary>
    public class SampleDeliveryDto
    {
        public long SampleDeliveryId { get; set; }
        public long OrderId { get; set; }
        public long UserId { get; set; }
        public long OwnerId { get; set; }

        // Third-party provider info
        public string? Provider { get; set; }          // Dunzo / Porter / Shadowfax
        public string? TrackingUrl { get; set; }
        public string? TrackingId { get; set; }

        // Status
        public SampleDeliveryStatus DeliveryStatus { get; set; }
        public string DeliveryStatusText => DeliveryStatus.ToString();

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Request to create sample delivery
    /// </summary>
    public class CreateSampleDeliveryRequest
    {
        public long OrderId { get; set; }
        public long UserId { get; set; }
        public long OwnerId { get; set; }
        public string Provider { get; set; } = "Dunzo"; // Default provider
    }

    /// <summary>
    /// Sample delivery status update from third-party
    /// </summary>
    public class UpdateSampleDeliveryStatusRequest
    {
        public long SampleDeliveryId { get; set; }
        public SampleDeliveryStatus NewStatus { get; set; }
        public string? TrackingUrl { get; set; }
        public string? TrackingId { get; set; }
    }

    /// <summary>
    /// Sample Delivery Status Enum
    /// </summary>
    public enum SampleDeliveryStatus
    {
        Requested = 1,
        PickedUp = 2,
        InTransit = 3,
        Delivered = 4,
        Failed = 5
    }
}
