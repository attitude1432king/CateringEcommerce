using System;
using System.Collections.Generic;

namespace CateringEcommerce.Domain.Models.Sample.DTOs
{
    /// <summary>
    /// Real-time tracking information for sample delivery
    /// </summary>
    public class SampleTrackingResponse
    {
        public long TrackingID { get; set; }
        public long SampleOrderID { get; set; }

        // Current Status
        public string DeliveryStatus { get; set; } = string.Empty;
        public string DeliveryStatusDisplay { get; set; } = string.Empty;

        // Live Location
        public decimal? CurrentLatitude { get; set; }
        public decimal? CurrentLongitude { get; set; }

        // Delivery Partner
        public string? PartnerName { get; set; }
        public string? PartnerPhone { get; set; }
        public string? VehicleNumber { get; set; }
        public string? VehicleType { get; set; }

        // ETA
        public DateTime? EstimatedArrival { get; set; }
        public decimal? DistanceRemaining { get; set; }
        public int? MinutesRemaining { get; set; }

        // Status Message
        public string? StatusMessage { get; set; }
        public DateTime Timestamp { get; set; }

        // Additional Info
        public decimal? Speed { get; set; }
        public int? BatteryLevel { get; set; }

        // Journey Progress
        public DeliveryJourneyProgress? JourneyProgress { get; set; }

        // Historical Tracking Points
        public List<TrackingHistoryPoint>? TrackingHistory { get; set; }
    }

    /// <summary>
    /// Delivery journey progress information
    /// </summary>
    public class DeliveryJourneyProgress
    {
        public bool PickupAssigned { get; set; }
        public DateTime? PickupAssignedTime { get; set; }

        public bool PickedUp { get; set; }
        public DateTime? PickupTime { get; set; }

        public bool InTransit { get; set; }
        public DateTime? TransitStartTime { get; set; }

        public bool Delivered { get; set; }
        public DateTime? DeliveryTime { get; set; }

        public int ProgressPercentage { get; set; }
    }

    /// <summary>
    /// Historical tracking point for delivery route
    /// </summary>
    public class TrackingHistoryPoint
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public DateTime Timestamp { get; set; }
        public string? StatusMessage { get; set; }
    }
}
