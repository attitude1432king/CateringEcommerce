using CateringEcommerce.BAL.Common;
using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces.User;
using CateringEcommerce.Domain.Models.User;
using Microsoft.Data.SqlClient;

namespace CateringEcommerce.BAL.Base.User
{
    /// <summary>
    /// Service for user-side home page and catering browsing operations.
    /// Handles business logic for searching, browsing, and viewing catering businesses.
    /// </summary>
    public class HomeService : IHomeService
    {
    
        private readonly SqlDatabaseManager _db;


        public HomeService(string connectionString)
        {
            _db = new SqlDatabaseManager();
            _db.SetConnectionString(connectionString);
        }


        /// <summary>
        /// Gets verified catering businesses by city ID.
        /// If cityId is less than or equal to 0, returns all verified catering businesses.
        /// </summary>
        /// <param name="cityId">The city ID to filter by. Pass 0 or negative value to get all.</param>
        /// <returns>List of verified catering businesses</returns>
        public async Task<List<CateringBusinessListDto>> GetVerifiedCateringListAsync(string cityName)
        {
            try
            {
                List<CateringBusinessListDto> cateringList;
                CateringBrowseRepository cateringRepository = new CateringBrowseRepository(_db.GetConnectionString());
                Locations locations = new Locations(_db.GetConnectionString());

                int cityId = await locations.GetCityID(cityName);

                if (cityId > 0)
                {
                    cateringList = await cateringRepository.GetVerifiedCateringsForBrowseInternalAsync(cityId);
                }
                else
                {
                    cateringList = await cateringRepository.GetVerifiedCateringsForBrowseInternalAsync();
                }

                return cateringList ?? new List<CateringBusinessListDto>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error fetching catering businesses.", ex);
            }
        }

        /// <summary>
        /// Gets detailed catering profile for user browsing/viewing.
        /// Returns complete information including contact details, address, services, and ratings.
        /// </summary>
        /// <param name="cateringId">The catering/owner ID</param>
        /// <returns>Detailed catering profile</returns>
        public async Task<CateringDetailDto> GetCateringDetailForBrowsingAsync(long cateringId)
        {
            try
            {

                if (cateringId <= 0)
                {
                    throw new ArgumentException("Invalid catering ID.", nameof(cateringId));
                }

                CateringBrowseRepository cateringRepository = new CateringBrowseRepository(_db.GetConnectionString());
                var cateringDetail = await cateringRepository.GetCateringDetailForUserBrowseAsync(cateringId);

                if (cateringDetail == null)
                {
                    throw new KeyNotFoundException($"Catering with ID {cateringId} not found or not verified.");
                }

                return cateringDetail;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error fetching catering details.", ex);
            }
        }

        /// <summary>
        /// Gets featured caterers for homepage display
        /// </summary>
        /// <returns>List of featured caterers</returns>
        public async Task<List<FeaturedCatererDto>> GetFeaturedCaterersAsync()
        {
            try
            {
                CateringBrowseRepository cateringRepository = new CateringBrowseRepository(_db.GetConnectionString());
                return await cateringRepository.GetFeaturedCaterersAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error fetching featured caterers.", ex);
            }
        }

        /// <summary>
        /// Gets testimonials for homepage display
        /// </summary>
        /// <returns>List of testimonials</returns>
        public async Task<List<HomePageTestimonialDto>> GetHomePageTestimonialsAsync()
        {
            try
            {
                CateringBrowseRepository cateringRepository = new CateringBrowseRepository(_db.GetConnectionString());
                return await cateringRepository.GetHomePageTestimonialsAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error fetching testimonials.", ex);
            }
        }

        /// <summary>
        /// Gets homepage statistics
        /// </summary>
        /// <returns>Homepage statistics</returns>
        public async Task<HomePageStatsDto> GetHomePageStatsAsync()
        {
            try
            {
                CateringBrowseRepository cateringRepository = new CateringBrowseRepository(_db.GetConnectionString());
                return await cateringRepository.GetHomePageStatsAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error fetching homepage stats.", ex);
            }
        }

        /// <summary>
        /// Gets all active packages for a specific caterer
        /// </summary>
        /// <param name="cateringId">The catering owner ID</param>
        /// <returns>List of packages</returns>
        public async Task<List<CateringPackageDto>> GetCateringPackagesAsync(long cateringId)
        {
            try
            {
                if (cateringId <= 0)
                {
                    throw new ArgumentException("Invalid catering ID.", nameof(cateringId));
                }

                CateringBrowseRepository cateringRepository = new CateringBrowseRepository(_db.GetConnectionString());
                return await cateringRepository.GetCateringPackagesAsync(cateringId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error fetching catering packages.", ex);
            }
        }

        /// <summary>
        /// Gets food items for a specific caterer with optional filters
        /// </summary>
        /// <param name="cateringId">The catering owner ID</param>
        /// <param name="categoryId">Optional category filter</param>
        /// <param name="isPackageItem">Optional filter: true = included in packages, false = add-ons, null = all</param>
        /// <returns>List of food items</returns>
        public async Task<List<CateringFoodItemDto>> GetCateringFoodItemsAsync(
            long cateringId,
            long? categoryId = null,
            bool? isPackageItem = null)
        {
            try
            {
                if (cateringId <= 0)
                {
                    throw new ArgumentException("Invalid catering ID.", nameof(cateringId));
                }

                CateringBrowseRepository cateringRepository = new CateringBrowseRepository(_db.GetConnectionString());
                return await cateringRepository.GetCateringFoodItemsAsync(cateringId, categoryId, isPackageItem);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error fetching food items.", ex);
            }
        }

        /// <summary>
        /// Gets decoration themes for a specific caterer
        /// </summary>
        /// <param name="cateringId">The catering owner ID</param>
        /// <returns>List of decorations</returns>
        public async Task<List<DecorationDto>> GetCateringDecorationsAsync(long cateringId)
        {
            try
            {
                if (cateringId <= 0)
                {
                    throw new ArgumentException("Invalid catering ID.", nameof(cateringId));
                }

                CateringBrowseRepository cateringRepository = new CateringBrowseRepository(_db.GetConnectionString());
                return await cateringRepository.GetCateringDecorationsAsync(cateringId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error fetching decorations.", ex);
            }
        }

        /// <summary>
        /// Gets customer reviews for a specific caterer with pagination
        /// </summary>
        /// <param name="cateringId">The catering owner ID</param>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Number of reviews per page</param>
        /// <returns>List of reviews</returns>
        public async Task<List<CateringReviewDto>> GetCateringReviewsAsync(
            long cateringId,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                if (cateringId <= 0)
                {
                    throw new ArgumentException("Invalid catering ID.", nameof(cateringId));
                }

                CateringBrowseRepository cateringRepository = new CateringBrowseRepository(_db.GetConnectionString());
                return await cateringRepository.GetCateringReviewsAsync(cateringId, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error fetching reviews.", ex);
            }
        }

        /// <summary>
        /// Gets all active food categories
        /// </summary>
        /// <returns>List of food categories</returns>
        public async Task<List<FoodCategoryDisplayDto>> GetFoodCategoriesAsync()
        {
            try
            {
                CateringBrowseRepository cateringRepository = new CateringBrowseRepository(_db.GetConnectionString());
                return await cateringRepository.GetFoodCategoriesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error fetching food categories.", ex);
            }
        }

        /// <summary>
        /// Gets package selection details with categories, allowed quantities, and eligible food items.
        /// Returns hierarchical data structure for package selection popup.
        /// </summary>
        /// <param name="packageId">The package ID</param>
        /// <param name="cateringId">The catering owner ID (for validation)</param>
        /// <returns>Complete package selection structure</returns>
        public async Task<PackageSelectionDto> GetPackageSelectionDetailsAsync(long packageId, long cateringId)
        {
            try
            {
                if (packageId <= 0)
                {
                    throw new ArgumentException("Invalid package ID.", nameof(packageId));
                }

                if (cateringId <= 0)
                {
                    throw new ArgumentException("Invalid catering ID.", nameof(cateringId));
                }

                CateringBrowseRepository cateringRepository = new CateringBrowseRepository(_db.GetConnectionString());
                return await cateringRepository.GetPackageSelectionDetailsAsync(packageId, cateringId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error fetching package selection details for package ID {packageId}.", ex);
            }
        }

        /// <summary>
        /// Gets food categories included in a specific package.
        /// Used for displaying category badges on package cards.
        /// </summary>
        /// <param name="packageId">The package ID</param>
        /// <param name="cateringId">The catering owner ID</param>
        /// <returns>List of food categories in the package</returns>
        public async Task<List<PackageCategoryBasicDto>> GetPackageCategoriesAsync(long packageId, long cateringId)
        {
            try
            {
                if (packageId <= 0)
                {
                    throw new ArgumentException("Invalid package ID.", nameof(packageId));
                }

                if (cateringId <= 0)
                {
                    throw new ArgumentException("Invalid catering ID.", nameof(cateringId));
                }

                CateringBrowseRepository cateringRepository = new CateringBrowseRepository(_db.GetConnectionString());
                return await cateringRepository.GetPackageCategoriesAsync(packageId, cateringId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error fetching package categories for package ID {packageId}.", ex);
            }
        }

        /// <summary>
        /// Searches food items within a package selection by name.
        /// Filters the food items available for selection in a package based on search query.
        /// </summary>
        /// <param name="packageId">The package ID</param>
        /// <param name="cateringId">The catering owner ID (for validation)</param>
        /// <param name="searchQuery">Search term to filter food items by name</param>
        /// <returns>Filtered package selection structure</returns>
        public async Task<PackageSelectionDto> SearchPackageFoodItemsAsync(long packageId, long cateringId, string searchQuery)
        {
            try
            {
                if (packageId <= 0)
                {
                    throw new ArgumentException("Invalid package ID.", nameof(packageId));
                }

                if (cateringId <= 0)
                {
                    throw new ArgumentException("Invalid catering ID.", nameof(cateringId));
                }

                CateringBrowseRepository cateringRepository = new CateringBrowseRepository(_db.GetConnectionString());
                var packageSelection = await cateringRepository.GetPackageSelectionDetailsAsync(packageId, cateringId);

                // If no search query, return all items
                if (string.IsNullOrWhiteSpace(searchQuery))
                {
                    return packageSelection;
                }

                // Filter food items by search query (case-insensitive)
                var filteredCategories = packageSelection.Categories.Select(category => new PackageCategoryDto
                {
                    CategoryId = category.CategoryId,
                    CategoryName = category.CategoryName,
                    CategoryDescription = category.CategoryDescription,
                    AllowedQuantity = category.AllowedQuantity,
                    FoodItems = category.FoodItems
                        .Where(food => food.FoodName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                        .ToList()
                }).ToList();

                // Return filtered package selection
                return new PackageSelectionDto
                {
                    PackageId = packageSelection.PackageId,
                    PackageName = packageSelection.PackageName,
                    Price = packageSelection.Price,
                    Categories = filteredCategories
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching food items in package ID {packageId}.", ex);
            }
        }

        /// <summary>
        /// Gets guest categories (food types) supported by a catering business.
        /// Fetches from t_sys_catering_type_master based on c_food_types in t_sys_catering_owner_operations.
        /// Also returns minimum guest count from c_min_dish_order.
        /// </summary>
        /// <param name="cateringId">The catering owner ID</param>
        /// <returns>Supported guest categories and minimum guest count</returns>
        public async Task<CateringGuestCategoriesDto> GetCateringGuestCategoriesAsync(long cateringId)
        {
            try
            {
                if (cateringId <= 0)
                {
                    throw new ArgumentException("Invalid catering ID.", nameof(cateringId));
                }

                // Query to get food types and minimum dish order from catering operations
                var operationsQuery = $@"
                    SELECT
                        c_food_types,
                        c_min_dish_order
                    FROM {Table.SysCateringOwnerService}
                    WHERE c_catering_id = @CateringId";

                var operationsData = await _db.ExecuteAsync(
                    operationsQuery,
                    new[] { new SqlParameter("@CateringId", cateringId) });

                if (operationsData == null)
                {
                    throw new InvalidOperationException($"No operations data found for catering ID {cateringId}");
                }

                string foodTypesStr = operationsData.Rows[0]["c_food_types"]?.ToString() ?? "";
                 
                int minDishOrder = operationsData.Rows[0]["c_min_dish_order"] != DBNull.Value ? Convert.ToInt16(operationsData.Rows[0]["c_min_dish_order"]) : 50;

                // Parse food type IDs (comma-separated)
                var foodTypeIds = foodTypesStr
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => long.TryParse(id.Trim(), out var parsed) ? parsed : 0)
                    .Where(id => id > 0)
                    .ToList();

                var categories = new List<GuestCategoryDto>();

                if (foodTypeIds.Any())
                {
                    // Query to get category details from master table
                    var categoriesQuery = $@"
                        SELECT
                            c_type_id as CategoryId,
                            c_type_name as CategoryName,
                            c_description as Description,
                        FROM {Table.SysCateringTypeMaster}
                        WHERE c_type_id IN @FoodTypeIds
                        AND c_is_active = 1
                        ORDER BY c_type_name";

                    var categoriesTable = await _db.ExecuteAsync(categoriesQuery, null);
                    categories = new List<GuestCategoryDto>();
                    if (categoriesTable != null)
                    {
                        foreach (System.Data.DataRow row in categoriesTable.Rows)
                        {
                            categories.Add(new GuestCategoryDto
                            {
                                CategoryId = row["CategoryId"] != DBNull.Value ? Convert.ToInt64(row["CategoryId"]) : 0,
                                CategoryName = row["CategoryName"]?.ToString(),
                                Description = row["Description"]?.ToString()
                            });
                        }
                    }
                }

                // If no categories found, add a default REGULAR category
                if (!categories.Any())
                {
                    categories.Add(new GuestCategoryDto
                    {
                        CategoryId = 0,
                        CategoryName = "Regular",
                        Description = "Standard vegetarian/non-vegetarian meals",
                    });
                }

                return new CateringGuestCategoriesDto
                {
                    MinimumGuests = minDishOrder,
                    DefaultGuests = minDishOrder,
                    SupportedCategories = categories
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error fetching guest categories for catering ID {cateringId}.", ex);
            }
        }

        /// <summary>
        /// Comprehensive search for catering services with multiple filters.
        /// Supports city-based search, catering type/category filters, keyword search, and combinations.
        /// </summary>
        /// <param name="filter">Search filter criteria</param>
        /// <returns>Paginated search results</returns>
        public async Task<CateringSearchResultDto> SearchCateringsAsync(CateringSearchFilterDto filter)
        {
            try
            {
                if (filter == null)
                {
                    throw new ArgumentNullException(nameof(filter));
                }

                CateringBrowseRepository cateringRepository = new CateringBrowseRepository(_db.GetConnectionString());
                Locations locations = new Locations(_db.GetConnectionString());

                // Get city ID if city name is provided
                int? cityId = null;
                if (!string.IsNullOrWhiteSpace(filter.City))
                {
                    cityId = await locations.GetCityID(filter.City);
                }

                return await cateringRepository.SearchCateringsWithFiltersAsync(filter, cityId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error performing catering search.", ex);
            }
        }
    }
}
