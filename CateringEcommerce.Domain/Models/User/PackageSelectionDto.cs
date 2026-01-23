namespace CateringEcommerce.Domain.Models.User
{
    /// <summary>
    /// DTO for food item available in package
    /// </summary>
    public class PackageFoodItemDto
    {
        public long FoodId { get; set; }
        public string FoodName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? CuisineType { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
    }

    /// <summary>
    /// DTO for category with food items and quantity restrictions
    /// </summary>
    public class PackageCategoryDto
    {
        public long CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? CategoryDescription { get; set; }
        public int AllowedQuantity { get; set; }
        public List<PackageFoodItemDto> FoodItems { get; set; } = new List<PackageFoodItemDto>();
    }

    /// <summary>
    /// Main DTO for package selection popup
    /// </summary>
    public class PackageSelectionDto
    {
        public long PackageId { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public List<PackageCategoryDto> Categories { get; set; } = new List<PackageCategoryDto>();
    }

    /// <summary>
    /// DTO for user's food item selection (request from frontend)
    /// </summary>
    public class UserPackageSelectionDto
    {
        public long PackageId { get; set; }
        public List<CategorySelectionDto> Selections { get; set; } = new List<CategorySelectionDto>();
    }

    /// <summary>
    /// DTO for category-wise selection
    /// </summary>
    public class CategorySelectionDto
    {
        public long CategoryId { get; set; }
        public List<long> SelectedFoodIds { get; set; } = new List<long>();
    }

    /// <summary>
    /// Basic DTO for package category (used for displaying categories in package cards)
    /// </summary>
    public class PackageCategoryBasicDto
    {
        public long CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
