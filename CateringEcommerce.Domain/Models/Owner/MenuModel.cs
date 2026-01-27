using CateringEcommerce.Domain.Models.APIModels.Owner;
using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Models.Owner
{
    public class FoodCategoryDto
    {
        public int CategoryId { get; set; }
        public string? Name { get; set; }

    }

    public class PackageDto
    {
        public long PackageId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public List<PackageItemDto> Items { get; set; }
    }

    public class PackageItemDto
    {
        public long PackageItemId { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; } // For display
        public int Quantity { get; set; }
    }

    public class FoodItemDto
    {
        public long? Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public int? TypeId { get; set; }  // Cuisine Type Id

        [Required]
        public decimal Price { get; set; }
        public bool IsVeg { get; set; }
        public bool IsLiveCounter { get; set; }
        public bool IsPackageItem { get; set; }
        public bool IsSampleTaste { get; set; }

        public bool Status { get; set; }
        public List<FileUploadDto>? FoodItemMediaFiles { get; set; }
        public List<string>? ExistingFoodItemMediaPaths { get; set; }
    }

    public class FoodItemModel
    {
        public long? Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public int? TypeId { get; set; }

        [Required]
        public decimal Price { get; set; }
        public bool IsVeg { get; set; }
        public bool IsLiveCounter { get; set; }

        public bool IsPackageItem { get; set; }
        public bool IsSampleTaste { get; set; }

        public bool Status { get; set; }
        public string? CategoryName { get; set; }
        public string? TypeName { get; set; }
        public List<MediaFileModel>? Media { get; set; }
    }

    public class FoodItemFilter
    {
        public string Name { get; set; }
        public List<int> CategoryIds { get; set; }
        public List<int> CuisineIds { get; set; }
        public string Status { get; set; }
        public bool? IsPackageItem { get; set; }
        public bool? IsSampleTaste { get; set; }
    }
}
