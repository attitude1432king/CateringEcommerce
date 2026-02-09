using System;
using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Models.Sample
{
    /// <summary>
    /// System-wide configuration for sample tasting feature
    /// </summary>
    public class SampleConfigurationModel
    {
        public int ConfigID { get; set; }

        // Item Selection Limits (ABUSE PREVENTION)
        [Required]
        [Range(1, 5)]
        public int MaxSampleItemsAllowed { get; set; } = 3;

        [Required]
        [Range(1, 5)]
        public int MinSampleItemsRequired { get; set; } = 1;

        // Pricing Model
        [Required]
        [MaxLength(50)]
        public string PricingModel { get; set; } = "PER_ITEM";

        [Range(0, 9999.99)]
        public decimal? FixedSampleFee { get; set; }

        // Delivery Configuration
        [Required]
        [MaxLength(50)]
        public string DefaultDeliveryProvider { get; set; } = "DUNZO";

        public bool EnableLiveTracking { get; set; } = true;

        [Range(0, 9999.99)]
        public decimal DeliveryChargeFlat { get; set; } = 50;

        [Range(0, 9999.99)]
        public decimal FreeDeliveryAbove { get; set; } = 500;

        // Business Rules
        public bool RequirePartnerApproval { get; set; } = true;

        [Range(1, 168)]
        public int AutoRejectAfterHours { get; set; } = 24;

        public bool AllowSameDaySampling { get; set; } = true;

        // Abuse Prevention
        [Range(1, 10)]
        public int MaxSamplesPerUserPerMonth { get; set; } = 2;

        [Range(1, 720)]
        public int SampleCooldownHours { get; set; } = 24;

        [Range(0, 9999.99)]
        public decimal? RequireMinSpendForSample { get; set; }

        // Conversion Tracking
        public bool ShowConversionCTAAfterDelivery { get; set; } = true;

        [Range(0, 100)]
        public decimal? ConversionDiscountPercent { get; set; }

        // System Settings
        public bool IsActive { get; set; } = true;

        public DateTime EffectiveFrom { get; set; } = DateTime.Now;

        public DateTime? EffectiveTo { get; set; }

        public DateTime LastModifiedDate { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string? ModifiedBy { get; set; }
    }
}
