using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Enums
{
    public enum ServiceType
    {
        [Display(Name = "Food Type")]
        FoodType = 1,
        [Display(Name = "Cuisine Type")]
        CuisineType = 2,
        [Display(Name = "Event Type")]
        EventType = 3,
        // Buffet Style, Plate Service
        [Display(Name = "Service Type")]
        ServiceType = 4
    }
}
