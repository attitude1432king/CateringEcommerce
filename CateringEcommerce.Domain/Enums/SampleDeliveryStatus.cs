using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Enums
{
    /// <summary>
    /// Represents the delivery tracking status for sample orders
    /// </summary>
    public enum SampleDeliveryStatus
    {
        [Display(Name = "Pickup Assigned")]
        PICKUP_ASSIGNED = 1,

        [Display(Name = "Picked Up")]
        PICKED_UP = 2,

        [Display(Name = "In Transit")]
        IN_TRANSIT = 3,

        [Display(Name = "Delivered")]
        DELIVERED = 4,

        [Display(Name = "Failed")]
        FAILED = 5
    }
}
