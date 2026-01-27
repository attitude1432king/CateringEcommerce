using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Enums
{
    public enum AvailabilityStatus
    {
        [Display(Name = "OPEN")]
        OPEN = 1,
        [Display(Name = "CLOSED")]
        CLOSED = 2,
        [Display(Name = "FULLY_BOOKED")]
        FULLY_BOOKED = 3
    }
}
