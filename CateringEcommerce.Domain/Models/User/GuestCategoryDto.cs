namespace CateringEcommerce.Domain.Models.User
{
    /// <summary>
    /// Represents a guest category (food type) supported by a catering business
    /// </summary>
    public class GuestCategoryDto
    {
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Contains guest categories and event setup defaults for a catering business
    /// </summary>
    public class CateringGuestCategoriesDto
    {
        public int MinimumGuests { get; set; }
        public int DefaultGuests { get; set; } 
        public List<GuestCategoryDto> SupportedCategories { get; set; }

        public CateringGuestCategoriesDto()
        {
            SupportedCategories = new List<GuestCategoryDto>();
        }
    }
}
