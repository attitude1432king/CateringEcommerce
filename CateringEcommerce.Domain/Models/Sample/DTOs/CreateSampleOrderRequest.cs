using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Models.Sample.DTOs
{
    /// <summary>
    /// Request DTO for creating a new sample order
    /// </summary>
    public class CreateSampleOrderRequest
    {
        [Required(ErrorMessage = "Catering ID is required")]
        public long CateringID { get; set; }

        [Required(ErrorMessage = "Delivery address is required")]
        public long DeliveryAddressID { get; set; }

        [Required(ErrorMessage = "Pickup address is required")]
        [MaxLength(500)]
        public string PickupAddress { get; set; } = string.Empty;

        [Range(-90, 90)]
        public decimal? PickupLatitude { get; set; }

        [Range(-180, 180)]
        public decimal? PickupLongitude { get; set; }

        [Range(-90, 90)]
        public decimal? DeliveryLatitude { get; set; }

        [Range(-180, 180)]
        public decimal? DeliveryLongitude { get; set; }

        [Required(ErrorMessage = "At least one sample item is required")]
        [MinLength(1, ErrorMessage = "At least one sample item is required")]
        public List<SampleItemSelectionDto> SelectedItems { get; set; } = new List<SampleItemSelectionDto>();

        [MaxLength(50)]
        public string? PreferredDeliveryProvider { get; set; }

        [MaxLength(500)]
        public string? SpecialInstructions { get; set; }
    }
}
