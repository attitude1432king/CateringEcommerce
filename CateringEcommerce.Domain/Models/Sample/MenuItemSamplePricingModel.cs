using System;
using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Models.Sample
{
    /// <summary>
    /// Sample-specific pricing for menu items (NOT package-based)
    /// </summary>
    public class MenuItemSamplePricingModel
    {
        public long ID { get; set; }

        [Required]
        public long MenuItemID { get; set; }

        // CRITICAL: Sample-specific pricing, NEVER derived from package
        [Required]
        [Range(0, 9999.99)]
        public decimal SamplePrice { get; set; }

        [Required]
        [Range(1, 10)]
        public int SampleQuantity { get; set; } = 1;

        // Availability
        public bool IsAvailableForSample { get; set; } = true;

        [Range(0, 10000)]
        public int? MinOrderQuantity { get; set; }

        // Partner Settings
        [Required]
        public long OwnerID { get; set; }

        public bool IsPartnerApproved { get; set; } = true;

        // Audit
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime LastModifiedDate { get; set; } = DateTime.Now;

        public long? CreatedBy { get; set; }

        public long? ModifiedBy { get; set; }
    }
}
