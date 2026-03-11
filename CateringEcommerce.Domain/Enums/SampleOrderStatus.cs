using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Enums
{
    /// <summary>
    /// Represents the lifecycle status of a sample order
    /// </summary>
    public enum SampleOrderStatus
    {
        [Display(Name = "Sample Requested")]
        SAMPLE_REQUESTED = 1,

        [Display(Name = "Sample Accepted")]
        SAMPLE_ACCEPTED = 2,

        [Display(Name = "Sample Rejected")]
        SAMPLE_REJECTED = 3,

        [Display(Name = "Sample Preparing")]
        SAMPLE_PREPARING = 4,

        [Display(Name = "Ready for Pickup")]
        READY_FOR_PICKUP = 5,

        [Display(Name = "In Transit")]
        IN_TRANSIT = 6,

        [Display(Name = "Delivered")]
        DELIVERED = 7,

        [Display(Name = "Refunded")]
        REFUNDED = 8
    }
}
