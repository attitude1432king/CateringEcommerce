using System;
using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Models.Sample
{
    /// <summary>
    /// Represents individual items in a sample order
    /// </summary>
    public class SampleOrderItemModel
    {
        public long SampleItemID { get; set; }

        [Required]
        public long SampleOrderID { get; set; }

        // Item Reference
        [Required]
        public long MenuItemID { get; set; }

        [Required]
        [MaxLength(200)]
        public string MenuItemName { get; set; } = string.Empty;

        // CRITICAL: Sample pricing is PER ITEM, NOT from package
        [Required]
        [Range(0, 9999.99)]
        public decimal SamplePrice { get; set; }

        [Required]
        [Range(1, 10)]
        public int SampleQuantity { get; set; } = 1;

        // Item Details (Snapshot for historical record)
        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [MaxLength(100)]
        public string? CuisineType { get; set; }

        public bool? IsVeg { get; set; }

        // Source Tracking
        public bool IsFromPackage { get; set; } = false;

        public long? PackageID { get; set; }

        // Audit
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
