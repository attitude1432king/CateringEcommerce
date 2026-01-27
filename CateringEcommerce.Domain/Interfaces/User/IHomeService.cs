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
    }
}
