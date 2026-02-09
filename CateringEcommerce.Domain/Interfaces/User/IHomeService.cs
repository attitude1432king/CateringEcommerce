using CateringEcommerce.Domain.Models.User;

namespace CateringEcommerce.Domain.Interfaces.User
{
    public interface IHomeService
    {
        /// <summary>
        /// Gets verified catering businesses by city ID.
        /// If cityId is less than or equal to 0, returns all verified catering businesses.
        /// </summary>
        /// <param name="cityId">The city ID to filter by. Pass 0 or negative value to get all.</param>
        /// <returns>List of verified catering businesses</returns>
        Task<List<CateringBusinessListDto>> GetVerifiedCateringListAsync(string cityName);

        /// <summary>
        /// Gets catering detail for browsing
        /// </summary>
        Task<CateringDetailDto> GetCateringDetailForBrowsingAsync(long cateringId);

        /// <summary>
        /// Gets featured caterers for homepage
        /// </summary>
        Task<List<FeaturedCatererDto>> GetFeaturedCaterersAsync();

        /// <summary>
        /// Gets testimonials for homepage
        /// </summary>
        Task<List<HomePageTestimonialDto>> GetHomePageTestimonialsAsync();

        /// <summary>
        /// Gets homepage statistics
        /// </summary>
        Task<HomePageStatsDto> GetHomePageStatsAsync();

        /// <summary>
        /// Gets catering packages
        /// </summary>
        Task<List<CateringPackageDto>> GetCateringPackagesAsync(long cateringId);

        /// <summary>
        /// Gets catering food items
        /// </summary>
        Task<List<CateringFoodItemDto>> GetCateringFoodItemsAsync(long cateringId, long? categoryId = null, bool? isPackageItem = null);

        /// <summary>
        /// Gets catering decorations
        /// </summary>
        Task<List<DecorationDto>> GetCateringDecorationsAsync(long cateringId);

        /// <summary>
        /// Gets catering reviews
        /// </summary>
        Task<List<CateringReviewDto>> GetCateringReviewsAsync(long cateringId, int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Gets food categories
        /// </summary>
        Task<List<FoodCategoryDisplayDto>> GetFoodCategoriesAsync();

        /// <summary>
        /// Gets package selection details
        /// </summary>
        Task<PackageSelectionDto> GetPackageSelectionDetailsAsync(long packageId, long cateringId);

        /// <summary>
        /// Gets food categories included in a specific package.
        /// Used for displaying category badges on package cards.
        /// </summary>
        /// <param name="packageId">The package ID</param>
        /// <param name="cateringId">The catering owner ID</param>
        /// <returns>List of food categories in the package</returns>
        Task<List<PackageCategoryBasicDto>> GetPackageCategoriesAsync(long packageId, long cateringId);

        /// <summary>
        /// Searches food items within a package selection by name.
        /// Filters the food items available for selection in a package based on search query.
        /// </summary>
        /// <param name="packageId">The package ID</param>
        /// <param name="cateringId">The catering owner ID</param>
        /// <param name="searchQuery">Search term to filter food items by name</param>
        /// <returns>Filtered package selection structure</returns>
        Task<PackageSelectionDto> SearchPackageFoodItemsAsync(long packageId, long cateringId, string searchQuery);

        /// <summary>
        /// Gets guest categories (food types) supported by a catering business.
        /// </summary>
        /// <param name="cateringId">The catering owner ID</param>
        /// <returns>Supported guest categories and minimum guest count</returns>
        Task<CateringGuestCategoriesDto> GetCateringGuestCategoriesAsync(long cateringId);

        /// <summary>
        /// Searches caterings based on filter criteria
        /// </summary>
        Task<CateringSearchResultDto> SearchCateringsAsync(CateringSearchFilterDto filter);
    }
}
