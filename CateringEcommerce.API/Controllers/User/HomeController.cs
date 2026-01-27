using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.User;
using CateringEcommerce.Domain.Interfaces.User;
using CateringEcommerce.Domain.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CateringEcommerce.API.Controllers.User
{
    [ApiController]
    [Route("api/User/Home")]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string _connStr;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connStr = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
        }

        /// <summary>
        /// Gets verified catering businesses.
        /// If cityId is less than or equal to 0, returns all verified catering businesses.
        /// Otherwise, returns catering businesses for the specified city.
        /// </summary>
        /// <param name="cityName">The city Name to filter by (optional). Use 0 or negative to get all.</param>
        /// <returns>List of verified catering businesses</returns>
        [AllowAnonymous]
        [HttpGet("CateringList")]
        public async Task<IActionResult> GetVerifiedCateringListAsync([FromQuery] string cityName)
        {
            try
            {
                HomeService  homeService = new HomeService(_connStr);
                _logger.LogInformation("Request received to fetch verified catering list. City Name: {0}", cityName);

                var cateringList = await homeService.GetVerifiedCateringListAsync(cityName);

                _logger.LogInformation("Successfully retrieved {Count} catering businesses.", cateringList.Count);

                return Ok(new
                {
                    success = true,
                    message = string.IsNullOrEmpty(cityName)
                        ? "All verified catering businesses retrieved successfully."
                        : $"Verified catering businesses for {cityName} city retrieved successfully.",
                    data = cateringList,
                    count = cateringList.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred. City Name: {cityName}", cityName);
                return StatusCode(500, ApiResponseHelper.Failure("An unexpected error occurred."));
            }
        }

        /// <summary>
        /// Comprehensive search for catering services with multiple filters.
        /// Supports city-based search, catering type/category filters, keyword search, and combinations.
        /// </summary>
        /// <param name="city">City name for location-based search (case-insensitive, trimmed)</param>
        /// <param name="cuisineTypes">Comma-separated cuisine type IDs (e.g., "1,2,3")</param>
        /// <param name="serviceTypes">Comma-separated service type IDs (e.g., "1,2")</param>
        /// <param name="eventTypes">Comma-separated event type IDs (e.g., "1,2")</param>
        /// <param name="keyword">Search keyword (searches across name, tags, cuisine, description, packages)</param>
        /// <param name="minRating">Minimum average rating filter (0.0 to 5.0)</param>
        /// <param name="onlineOnly">Filter for only online/available caterers</param>
        /// <param name="verifiedOnly">Filter for only admin-verified caterers (default: true)</param>
        /// <param name="minOrderFrom">Minimum order value range - from</param>
        /// <param name="minOrderTo">Minimum order value range - to</param>
        /// <param name="deliveryRadius">Delivery radius in km</param>
        /// <param name="pageNumber">Page number (1-based, default: 1)</param>
        /// <param name="pageSize">Number of results per page (default: 20, max: 100)</param>
        /// <returns>Paginated search results with catering businesses matching the criteria</returns>
        [AllowAnonymous]
        [HttpGet("Search")]
        public async Task<IActionResult> SearchCateringsAsync(
            [FromQuery] string? city = null,
            [FromQuery] string? cuisineTypes = null,
            [FromQuery] string? serviceTypes = null,
            [FromQuery] string? eventTypes = null,
            [FromQuery] string? keyword = null,
            [FromQuery] double? minRating = null,
            [FromQuery] bool? onlineOnly = null,
            [FromQuery] bool? verifiedOnly = true,
            [FromQuery] decimal? minOrderFrom = null,
            [FromQuery] decimal? minOrderTo = null,
            [FromQuery] int? deliveryRadius = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                // Validate and normalize inputs
                pageNumber = Math.Max(1, pageNumber);
                pageSize = Math.Clamp(pageSize, 1, 100);

                // Parse comma-separated IDs
                List<int>? cuisineTypeIds = ParseCommaSeparatedIds(cuisineTypes);
                List<int>? serviceTypeIds = ParseCommaSeparatedIds(serviceTypes);
                List<int>? eventTypeIds = ParseCommaSeparatedIds(eventTypes);

                // Create filter DTO
                var filter = new Domain.Models.User.CateringSearchFilterDto
                {
                    City = city?.Trim(),
                    SearchKeyword = keyword?.Trim(),
                    CuisineTypeIds = cuisineTypeIds,
                    ServiceTypeIds = serviceTypeIds,
                    EventTypeIds = eventTypeIds,
                    MinRating = minRating,
                    OnlineOnly = onlineOnly,
                    VerifiedOnly = verifiedOnly,
                    MinOrderValueFrom = minOrderFrom,
                    MinOrderValueTo = minOrderTo,
                    DeliveryRadiusKm = deliveryRadius,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                HomeService homeService = new HomeService(_connStr);
                _logger.LogInformation("Search request received - City: {City}, Keyword: {Keyword}, Page: {Page}",
                    city, keyword, pageNumber);

                var searchResult = await homeService.SearchCateringsAsync(filter);

                _logger.LogInformation("Search completed - Found {Count} results (Total: {Total})",
                    searchResult.Results?.Count ?? 0, searchResult.TotalCount);

                return Ok(new
                {
                    success = true,
                    message = "Search completed successfully.",
                    data = searchResult.Results,
                    pagination = new
                    {
                        totalCount = searchResult.TotalCount,
                        pageNumber = searchResult.PageNumber,
                        pageSize = searchResult.PageSize,
                        totalPages = (int)Math.Ceiling((double)searchResult.TotalCount / searchResult.PageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during catering search");
                return StatusCode(500, ApiResponseHelper.Failure("An unexpected error occurred during search."));
            }
        }

        /// <summary>
        /// Helper method to parse comma-separated IDs into a list of integers
        /// </summary>
        private List<int>? ParseCommaSeparatedIds(string? commaSeparated)
        {
            if (string.IsNullOrWhiteSpace(commaSeparated))
                return null;

            return commaSeparated
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => int.TryParse(s, out _))
                .Select(int.Parse)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Gets featured caterers for homepage display
        /// </summary>
        /// <returns>List of top 6 featured caterers</returns>
        [AllowAnonymous]
        [HttpGet("FeaturedCaterers")]
        public async Task<IActionResult> GetFeaturedCaterersAsync()
        {
            try
            {
                HomeService homeService = new HomeService(_connStr);
                _logger.LogInformation("Request received to fetch featured caterers for homepage");

                var featuredCaterers = await homeService.GetFeaturedCaterersAsync();

                _logger.LogInformation("Successfully retrieved {Count} featured caterers.", featuredCaterers.Count);

                return Ok(new
                {
                    success = true,
                    message = "Featured caterers retrieved successfully.",
                    data = featuredCaterers,
                    count = featuredCaterers.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching featured caterers");
                return StatusCode(500, ApiResponseHelper.Failure("An unexpected error occurred."));
            }
        }

        /// <summary>
        /// Gets testimonials/reviews for homepage display
        /// </summary>
        /// <returns>List of top testimonials</returns>
        [AllowAnonymous]
        [HttpGet("Testimonials")]
        public async Task<IActionResult> GetTestimonialsAsync()
        {
            try
            {
                HomeService homeService = new HomeService(_connStr);
                _logger.LogInformation("Request received to fetch testimonials for homepage");

                var testimonials = await homeService.GetHomePageTestimonialsAsync();

                _logger.LogInformation("Successfully retrieved {Count} testimonials.", testimonials.Count);

                return Ok(new
                {
                    success = true,
                    message = "Testimonials retrieved successfully.",
                    data = testimonials,
                    count = testimonials.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching testimonials");
                return StatusCode(500, ApiResponseHelper.Failure("An unexpected error occurred."));
            }
        }

        /// <summary>
        /// Gets homepage statistics
        /// </summary>
        /// <returns>Homepage statistics (events, partners, customers, satisfaction rate)</returns>
        [AllowAnonymous]
        [HttpGet("Stats")]
        public async Task<IActionResult> GetHomePageStatsAsync()
        {
            try
            {
                HomeService homeService = new HomeService(_connStr);
                _logger.LogInformation("Request received to fetch homepage statistics");

                var stats = await homeService.GetHomePageStatsAsync();

                _logger.LogInformation("Successfully retrieved homepage statistics.");

                return Ok(new
                {
                    success = true,
                    message = "Homepage statistics retrieved successfully.",
                    data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching homepage statistics");
                return StatusCode(500, ApiResponseHelper.Failure("An unexpected error occurred."));
            }
        }

        /// <summary>
        /// Gets all active packages for a specific caterer
        /// </summary>
        /// <param name="cateringId">The catering owner ID</param>
        /// <returns>List of packages with pricing and details</returns>
        [AllowAnonymous]
        [HttpGet("Catering/{cateringId}/Packages")]
        public async Task<IActionResult> GetCateringPackagesAsync(long cateringId)
        {
            try
            {
                HomeService homeService = new HomeService(_connStr);
                _logger.LogInformation("Request received to fetch packages for catering ID: {CateringId}", cateringId);

                var packages = await homeService.GetCateringPackagesAsync(cateringId);

                _logger.LogInformation("Successfully retrieved {Count} packages for catering ID: {CateringId}", packages.Count, cateringId);

                return Ok(new
                {
                    success = true,
                    message = "Catering packages retrieved successfully.",
                    data = packages,
                    count = packages.Count
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid catering ID: {CateringId}", cateringId);
                return BadRequest(ApiResponseHelper.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching packages for catering ID: {CateringId}", cateringId);
                return StatusCode(500, ApiResponseHelper.Failure("An unexpected error occurred."));
            }
        }

        /// <summary>
        /// Gets food items for a specific caterer with optional filters
        /// </summary>
        /// <param name="cateringId">The catering owner ID</param>
        /// <param name="categoryId">Optional category filter</param>
        /// <param name="isPackageItem">Optional filter: true = included in packages, false = add-ons, null = all</param>
        /// <returns>List of food items</returns>
        [AllowAnonymous]
        [HttpGet("Catering/{cateringId}/FoodItems")]
        public async Task<IActionResult> GetCateringFoodItemsAsync(
            long cateringId,
            [FromQuery] long? categoryId = null,
            [FromQuery] bool? isPackageItem = null)
        {
            try
            {
                HomeService homeService = new HomeService(_connStr);
                _logger.LogInformation("Request received to fetch food items for catering ID: {CateringId}, Category: {CategoryId}, IsPackageItem: {IsPackageItem}",
                    cateringId, categoryId, isPackageItem);

                var foodItems = await homeService.GetCateringFoodItemsAsync(cateringId, categoryId, isPackageItem);

                _logger.LogInformation("Successfully retrieved {Count} food items for catering ID: {CateringId}", foodItems.Count, cateringId);

                return Ok(new
                {
                    success = true,
                    message = "Food items retrieved successfully.",
                    data = foodItems,
                    count = foodItems.Count
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid catering ID: {CateringId}", cateringId);
                return BadRequest(ApiResponseHelper.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching food items for catering ID: {CateringId}", cateringId);
                return StatusCode(500, ApiResponseHelper.Failure("An unexpected error occurred."));
            }
        }

        /// <summary>
        /// Gets decoration themes for a specific caterer
        /// </summary>
        /// <param name="cateringId">The catering owner ID</param>
        /// <returns>List of decoration themes with pricing</returns>
        [AllowAnonymous]
        [HttpGet("Catering/{cateringId}/Decorations")]
        public async Task<IActionResult> GetCateringDecorationsAsync(long cateringId)
        {
            try
            {
                HomeService homeService = new HomeService(_connStr);
                _logger.LogInformation("Request received to fetch decorations for catering ID: {CateringId}", cateringId);

                var decorations = await homeService.GetCateringDecorationsAsync(cateringId);

                _logger.LogInformation("Successfully retrieved {Count} decorations for catering ID: {CateringId}", decorations.Count, cateringId);

                return Ok(new
                {
                    success = true,
                    message = "Decorations retrieved successfully.",
                    data = decorations,
                    count = decorations.Count
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid catering ID: {CateringId}", cateringId);
                return BadRequest(ApiResponseHelper.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching decorations for catering ID: {CateringId}", cateringId);
                return StatusCode(500, ApiResponseHelper.Failure("An unexpected error occurred."));
            }
        }

        /// <summary>
        /// Gets customer reviews for a specific caterer with pagination
        /// </summary>
        /// <param name="cateringId">The catering owner ID</param>
        /// <param name="pageNumber">Page number (1-based, default: 1)</param>
        /// <param name="pageSize">Number of reviews per page (default: 10)</param>
        /// <returns>List of customer reviews</returns>
        [AllowAnonymous]
        [HttpGet("Catering/{cateringId}/Reviews")]
        public async Task<IActionResult> GetCateringReviewsAsync(
            long cateringId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                HomeService homeService = new HomeService(_connStr);
                _logger.LogInformation("Request received to fetch reviews for catering ID: {CateringId}, Page: {PageNumber}, Size: {PageSize}",
                    cateringId, pageNumber, pageSize);

                var reviews = await homeService.GetCateringReviewsAsync(cateringId, pageNumber, pageSize);

                _logger.LogInformation("Successfully retrieved {Count} reviews for catering ID: {CateringId}", reviews.Count, cateringId);

                return Ok(new
                {
                    success = true,
                    message = "Reviews retrieved successfully.",
                    data = reviews,
                    count = reviews.Count,
                    pageNumber = pageNumber,
                    pageSize = pageSize
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid catering ID: {CateringId}", cateringId);
                return BadRequest(ApiResponseHelper.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching reviews for catering ID: {CateringId}", cateringId);
                return StatusCode(500, ApiResponseHelper.Failure("An unexpected error occurred."));
            }
        }

        /// <summary>
        /// Gets all active food categories
        /// </summary>
        /// <returns>List of food categories</returns>
        [AllowAnonymous]
        [HttpGet("FoodCategories")]
        public async Task<IActionResult> GetFoodCategoriesAsync()
        {
            try
            {
                HomeService homeService = new HomeService(_connStr);
                _logger.LogInformation("Request received to fetch food categories");

                var categories = await homeService.GetFoodCategoriesAsync();

                _logger.LogInformation("Successfully retrieved {Count} food categories", categories.Count);

                return Ok(new
                {
                    success = true,
                    message = "Food categories retrieved successfully.",
                    data = categories,
                    count = categories.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching food categories");
                return StatusCode(500, ApiResponseHelper.Failure("An unexpected error occurred."));
            }
        }

        /// <summary>
        /// Gets detailed information about a specific caterer
        /// </summary>
        /// <param name="cateringId">The catering owner ID</param>
        /// <returns>Detailed catering profile</returns>
        [AllowAnonymous]
        [HttpGet("Catering/{cateringId}/Detail")]
        public async Task<IActionResult> GetCateringDetailAsync(long cateringId)
        {
            try
            {
                HomeService homeService = new HomeService(_connStr);
                _logger.LogInformation("Request received to fetch catering detail for ID: {CateringId}", cateringId);

                var cateringDetail = await homeService.GetCateringDetailForBrowsingAsync(cateringId);

                if (cateringDetail == null)
                {
                    _logger.LogWarning("Catering detail not found for ID: {CateringId}", cateringId);
                    return NotFound(ApiResponseHelper.Failure("Catering not found."));
                }

                _logger.LogInformation("Successfully retrieved catering detail for ID: {CateringId}", cateringId);

                return Ok(new
                {
                    success = true,
                    message = "Catering detail retrieved successfully.",
                    data = cateringDetail
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid catering ID: {CateringId}", cateringId);
                return BadRequest(ApiResponseHelper.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching catering detail for ID: {CateringId}", cateringId);
                return StatusCode(500, ApiResponseHelper.Failure("An unexpected error occurred."));
            }
        }

        /// <summary>
        /// Gets package selection details with categories, allowed quantities, and eligible food items.
        /// Used for the package selection popup where users choose specific items from each category.
        /// Only returns items where c_ispackage_item = TRUE, c_status = TRUE, c_is_deleted = 0.
        /// </summary>
        /// <param name="cateringId">The catering owner ID</param>
        /// <param name="packageId">The package ID</param>
        /// <returns>Hierarchical package selection data (Package → Categories → Food Items)</returns>
        [AllowAnonymous]
        [HttpGet("Catering/{cateringId}/Package/{packageId}/Selection")]
        public async Task<IActionResult> GetPackageSelectionDetailsAsync(long cateringId, long packageId)
        {
            try
            {
                HomeService homeService = new HomeService(_connStr);
                _logger.LogInformation("Request received to fetch package selection for Catering ID: {CateringId}, Package ID: {PackageId}",
                    cateringId, packageId);

                var packageSelection = await homeService.GetPackageSelectionDetailsAsync(packageId, cateringId);

                _logger.LogInformation("Successfully retrieved package selection for Package ID: {PackageId}", packageId);

                return Ok(new
                {
                    success = true,
                    message = "Package selection details retrieved successfully.",
                    data = packageSelection
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid ID - Catering: {CateringId}, Package: {PackageId}", cateringId, packageId);
                return BadRequest(ApiResponseHelper.Failure(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Package not found - Catering: {CateringId}, Package: {PackageId}", cateringId, packageId);
                return NotFound(ApiResponseHelper.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching package selection - Catering: {CateringId}, Package: {PackageId}",
                    cateringId, packageId);
                return StatusCode(500, ApiResponseHelper.Failure("An unexpected error occurred."));
            }
        }

        /// <summary>
        /// Gets food categories included in a specific package.
        /// Used for displaying category badges on package cards in the UI.
        /// Returns distinct categories from package items.
        /// </summary>
        /// <param name="cateringId">The catering owner ID</param>
        /// <param name="packageId">The package ID</param>
        /// <returns>List of food categories included in the package</returns>
        [AllowAnonymous]
        [HttpGet("Catering/{cateringId}/Package/{packageId}/Categories")]
        public async Task<IActionResult> GetPackageCategoriesAsync(long cateringId, long packageId)
        {
            try
            {
                HomeService homeService = new HomeService(_connStr);
                _logger.LogInformation("Request received to fetch package categories for Catering ID: {CateringId}, Package ID: {PackageId}",
                    cateringId, packageId);

                var categories = await homeService.GetPackageCategoriesAsync(packageId, cateringId);

                _logger.LogInformation("Successfully retrieved {Count} categories for Package ID: {PackageId}", categories.Count, packageId);

                return Ok(new
                {
                    success = true,
                    message = "Package categories retrieved successfully.",
                    data = categories,
                    count = categories.Count
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid ID - Catering: {CateringId}, Package: {PackageId}", cateringId, packageId);
                return BadRequest(ApiResponseHelper.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching package categories - Catering: {CateringId}, Package: {PackageId}",
                    cateringId, packageId);
                return StatusCode(500, ApiResponseHelper.Failure("An unexpected error occurred."));
            }
        }

        /// <summary>
        /// Searches food items within a package selection by name.
        /// Filters the food items available for selection in a package based on search query.
        /// </summary>
        /// <param name="cateringId">The catering owner ID</param>
        /// <param name="packageId">The package ID</param>
        /// <param name="searchQuery">Search term to filter food items by name</param>
        /// <returns>Package selection data filtered by search query</returns>
        [AllowAnonymous]
        [HttpGet("Catering/{cateringId}/Package/{packageId}/Selection/Search")]
        public async Task<IActionResult> SearchPackageFoodItemsAsync(
            long cateringId,
            long packageId,
            [FromQuery] string searchQuery)
        {
            try
            {
                HomeService homeService = new HomeService(_connStr);
                _logger.LogInformation("Request received to search food items in package - Catering ID: {CateringId}, Package ID: {PackageId}, Query: {SearchQuery}",
                    cateringId, packageId, searchQuery);

                var packageSelection = await homeService.SearchPackageFoodItemsAsync(packageId, cateringId, searchQuery);

                _logger.LogInformation("Successfully retrieved filtered package selection for Package ID: {PackageId}", packageId);

                return Ok(new
                {
                    success = true,
                    message = "Food items search completed successfully.",
                    data = packageSelection
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid ID - Catering: {CateringId}, Package: {PackageId}", cateringId, packageId);
                return BadRequest(ApiResponseHelper.Failure(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Package not found - Catering: {CateringId}, Package: {PackageId}", cateringId, packageId);
                return NotFound(ApiResponseHelper.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while searching food items - Catering: {CateringId}, Package: {PackageId}",
                    cateringId, packageId);
                return StatusCode(500, ApiResponseHelper.Failure("An unexpected error occurred."));
            }
        }

        /// <summary>
        /// Gets guest categories (food types) supported by a catering business and minimum guest count.
        /// Returns categories from t_sys_catering_type_master based on c_food_types in t_sys_catering_owner_operations.
        /// </summary>
        /// <param name="cateringId">The catering owner ID</param>
        /// <returns>Supported guest categories and minimum guest count</returns>
        [AllowAnonymous]
        [HttpGet("Catering/{cateringId}/GuestCategories")]
        public async Task<IActionResult> GetCateringGuestCategoriesAsync(long cateringId)
        {
            try
            {
                HomeService homeService = new HomeService(_connStr);
                _logger.LogInformation("Request received to fetch guest categories for Catering ID: {CateringId}", cateringId);

                var guestCategoriesData = await homeService.GetCateringGuestCategoriesAsync(cateringId);

                _logger.LogInformation("Successfully retrieved guest categories for Catering ID: {CateringId}", cateringId);

                return Ok(new
                {
                    success = true,
                    message = "Guest categories retrieved successfully.",
                    data = guestCategoriesData
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid Catering ID: {CateringId}", cateringId);
                return BadRequest(ApiResponseHelper.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching guest categories for Catering ID: {CateringId}", cateringId);
                return StatusCode(500, ApiResponseHelper.Failure("An unexpected error occurred."));
            }
        }
    }
}