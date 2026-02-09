using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Models.Sample.DTOs
{
    /// <summary>
    /// DTO for selecting individual items for sample order
    /// </summary>
    public class SampleItemSelectionDto
    {
        [Required(ErrorMessage = "Menu item ID is required")]
        public long MenuItemID { get; set; }

        public bool IsFromPackage { get; set; } = false;

        public long? PackageID { get; set; }

        // Additional item details for validation/display
        public string? MenuItemName { get; set; }

        public decimal? SamplePrice { get; set; }
    }
}
