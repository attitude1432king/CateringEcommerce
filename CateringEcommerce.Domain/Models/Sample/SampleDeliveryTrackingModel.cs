using System;
using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Models.Sample
{
    /// <summary>
    /// Real-time delivery tracking for sample orders (like Swiggy/Zomato)
    /// </summary>
    public class SampleDeliveryTrackingModel
    {
        public long TrackingID { get; set; }

        [Required]
        public long SampleOrderID { get; set; }

        // Live Tracking Data
        [Required]
        [MaxLength(50)]
        public string DeliveryStatus { get; set; } = "PICKUP_ASSIGNED";

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        // Delivery Partner Info
        [MaxLength(200)]
        public string? PartnerName { get; set; }

        [MaxLength(20)]
        [Phone]
        public string? PartnerPhone { get; set; }

        [MaxLength(50)]
        public string? VehicleNumber { get; set; }

        [MaxLength(50)]
        public string? VehicleType { get; set; }

        // ETA Tracking
        public DateTime? EstimatedArrival { get; set; }

        [Range(0, 9999.99)]
        public decimal? DistanceRemaining { get; set; } // in KM

        // Status Update Details
        [MaxLength(500)]
        public string? StatusMessage { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Additional Tracking
        [Range(0, 200)]
        public decimal? Speed { get; set; } // km/h

        [Range(0, 100)]
        public int? BatteryLevel { get; set; } // For electric vehicles
    }
}
