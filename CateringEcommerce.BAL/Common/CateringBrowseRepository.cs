using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Enums.Admin;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Models.User;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using System.Text;

namespace CateringEcommerce.BAL.Common
{
    /// <summary>
    /// Repository for user-side catering browsing operations.
    /// Handles all read-only operations for users browsing and viewing catering businesses.
    /// Separate from Locations.cs which handles location-specific operations (states, cities).
    /// </summary>
    public class CateringBrowseRepository
    {
        private readonly IDatabaseHelper _dbHelper;
        public CateringBrowseRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        #region Public Methods - Catering List for Browsing

        /// <summary>
        /// Gets verified and active catering businesses filtered by specific city.
        /// Method Name Rationale: "GetVerifiedCateringsForBrowseInternalAsync" - Clearly indicates it's for browsing, verified, filtered by city.
        /// </summary>
        /// <param name="cityId">The city ID to filter catering businesses by</param>
        /// <returns>List of verified catering businesses in the specified city</returns>
        public async Task<List<CateringBusinessListDto>> GetVerifiedCateringsForBrowseInternalAsync(int? cityId = null)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                sb.Append($@"
                    SELECT 
                        o.c_ownerid AS CateringId,
                        o.c_catering_name AS CateringName,
                        o.c_logo_path AS LogoUrl,
                        CASE WHEN status.c_global_status = 1 THEN 1 ELSE 0 END AS IsOnline,
                        COALESCE(r.AverageRating, 0) AS AverageRating,
                        COALESCE(r.ReviewCount, 0) AS TotalReviews,
                        service.c_min_guest_count AS MinOrderValue,
                        service.c_delivery_radius_km AS DeliveryRadiusKm,
                        ct.c_cityname AS City,
                        address.c_area AS Area,
                        0 AS DistanceKm
                    FROM {Table.SysCateringOwner} o
                    LEFT JOIN {Table.SysCateringOwnerAddress} address ON address.c_ownerid = o.c_ownerid
                    LEFT JOIN {Table.SysCateringOwnerService} service ON service.c_ownerid = o.c_ownerid                    
                    LEFT JOIN {Table.SysCateringAvailabilityGlobal} status ON status.c_ownerid = o.c_ownerid                    
                    LEFT JOIN {Table.City} ct ON address.c_cityid = ct.c_cityid                    
                    LEFT JOIN ( 
                        SELECT c_ownerid, 
                               CAST(COALESCE(AVG(CAST(c_overall_rating AS FLOAT)), 0) AS DECIMAL(3,2)) AS AverageRating,
                               COUNT(*) AS ReviewCount
                        FROM {Table.SysCateringReview}
                        WHERE c_is_visible = TRUE and c_is_verified = TRUE
                        GROUP BY c_ownerid
                    ) r ON o.c_ownerid = r.c_ownerid
                    WHERE o.c_approval_status = {ApprovalStatus.Approved.GetHashCode()}
                        AND o.c_isactive = TRUE
                ");

                List<NpgsqlParameter> parameters = new();

                // ? Optional city filter
                if (cityId.HasValue && cityId.Value > 0)
                {
                    sb.Append(" AND address.c_cityid = @CityID ");
                    parameters.Add(new NpgsqlParameter("@CityID", cityId.Value));
                }

                sb.Append(" ORDER BY o.c_catering_name ASC ");

                var result = await _dbHelper.ExecuteAsync(sb.ToString(), parameters.ToArray());
                return MapOwnerDataToCateringBusinessListDto(result);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error fetching verified catering businesses.", ex);
            }
        }

        #endregion



        #region Public Methods - Catering Detail for Browsing

        /// <summary>
        /// Gets detailed catering profile for a specific caterer.
        /// Method Name Rationale: "GetCateringDetailForUserBrowse" - Indicates detailed view for users, not admin management.
        /// NOTE: Contact details (phone, email, WhatsApp) are NOT included for user role - users must book through the platform.
        /// </summary>
        /// <param name="cateringId">The owner/catering ID</param>
        /// <returns>Detailed catering profile WITHOUT contact information</returns>
        public async Task<CateringDetailDto> GetCateringDetailForUserBrowseAsync(long cateringId)
        {
            try
            {
                string query = $@"
                    SELECT 
                        o.c_ownerid AS CateringId,
                        o.c_catering_name AS CateringName,
                        o.c_owner_name AS OwnerName,
                        o.c_logo_path AS LogoUrl,
                        ad.c_building AS ShopNo,
                        ad.c_street AS Street,
                        ad.c_area AS Area,
                        ad.c_pincode AS Pincode,
                        st.c_statename AS State,
                        ct.c_cityname AS City,
                        COALESCE(ad.c_latitude, '') AS Latitude,
                        COALESCE(ad.c_longitude, '') AS Longitude,
                        COALESCE(op.c_min_guest_count, 0) AS MinOrderValue,
                        COALESCE(op.c_delivery_radius_km, 0) AS DeliveryRadiusKm,
                        ag.c_global_status AS IsOnline,
                        '' AS Description,
                        COALESCE(r.AverageRating, 0) AS AverageRating,
                        COALESCE(r.ReviewCount, 0) AS TotalReviews
                    FROM {Table.SysCateringOwner} o
                    LEFT JOIN {Table.SysCateringOwnerAddress} ad ON ad.c_ownerid = o.c_ownerid
                    LEFT JOIN {Table.SysCateringOwnerService} op ON op.c_ownerid = o.c_ownerid
                    LEFT JOIN {Table.SysCateringAvailabilityGlobal} ag ON ag.c_ownerid = o.c_ownerid
                    LEFT JOIN {Table.City} ct ON ad.c_cityid = ct.c_cityid
                    LEFT JOIN {Table.State} st ON ad.c_stateid = st.c_stateid
                    LEFT JOIN (
                        SELECT c_ownerid, 
                               CAST(COALESCE(AVG(CAST(c_overall_rating AS FLOAT)), 0) AS DECIMAL(3,2)) AS AverageRating,
                               COUNT(*) AS ReviewCount
                        FROM {Table.SysCateringReview}
                        WHERE c_is_verified = TRUE
                        GROUP BY c_ownerid
                    ) r ON o.c_ownerid = r.c_ownerid
                    WHERE o.c_ownerid = @CateringId 
                        AND o.c_approval_status = {ApprovalStatus.Approved.GetHashCode()}
                        AND o.c_isactive = TRUE";

                var parameters = new[] { new NpgsqlParameter("@CateringId", cateringId) };
                var result = await _dbHelper.ExecuteAsync(query, parameters);

                if (result.Rows.Count == 0)
                    return null;

                var cateringDetail = MapOwnerRowToCateringDetailDto(result.Rows[0]);

                // Fetch kitchen photos and videos separately
                await LoadKitchenMediaAsync(cateringDetail);

                return cateringDetail;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error fetching catering detail for catering ID {cateringId}.", ex);
            }
        }

        /// <summary>
        /// Loads kitchen photos and videos for a catering detail.
        /// Document Type 2 = Kitchen images/videos
        /// </summary>
        private async Task LoadKitchenMediaAsync(CateringDetailDto cateringDetail)
        {
            try
            {
                string mediaQuery = $@"
                    SELECT
                        c_media_id AS MediaId,
                        c_file_path AS FilePath,
                        c_extension AS FileType,
                        c_uploaded_at AS UploadedAt
                    FROM {Table.SysCateringMediaUploads}
                    WHERE c_ownerid = @CateringId
                        AND c_document_type_id = {DocumentType.Kitchen.GetHashCode()}
                        AND c_is_deleted = FALSE
                    ORDER BY c_uploaded_at DESC";

                var parameters = new[] { new NpgsqlParameter("@CateringId", cateringDetail.CateringId) };
                var mediaResult = await _dbHelper.ExecuteAsync(mediaQuery, parameters);

                cateringDetail.KitchenPhotos = new List<CateringMediaDto>();
                cateringDetail.KitchenVideos = new List<CateringMediaDto>();

                if (mediaResult != null && mediaResult.Rows.Count > 0)
                {
                    foreach (DataRow mediaRow in mediaResult.Rows)
                    {
                        var fileType = mediaRow["FileType"]?.ToString()?.ToLower() ?? "";
                        var media = new CateringMediaDto
                        {
                            MediaId = Convert.ToInt64(mediaRow["MediaId"]),
                            MediaUrl = mediaRow["FilePath"]?.ToString(),
                            MediaType = mediaRow["FileType"]?.ToString(),
                            Caption = null,
                            DisplayOrder = 0
                        };

                        // Classify as photo or video based on file type
                        if (fileType.Contains("image") || fileType.EndsWith("jpg") || fileType.EndsWith("jpeg") ||
                            fileType.EndsWith("png") || fileType.EndsWith("webp"))
                        {
                            cateringDetail.KitchenPhotos.Add(media);
                        }
                        else if (fileType.Contains("video") || fileType.EndsWith("mp4") || fileType.EndsWith("webm"))
                        {
                            cateringDetail.KitchenVideos.Add(media);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw - kitchen media is optional
                Console.WriteLine($"Error loading kitchen media: {ex.Message}");
                cateringDetail.KitchenPhotos = new List<CateringMediaDto>();
                cateringDetail.KitchenVideos = new List<CateringMediaDto>();
            }
        }

        #endregion

        #region Public Methods - Homepage Data

        /// <summary>
        /// Gets featured caterers for homepage display (max 6)
        /// </summary>
        public async Task<List<FeaturedCatererDto>> GetFeaturedCaterersAsync()
        {
            try
            {
                string query = $@"
                    SELECT
                            o.c_ownerid AS Id,
                            o.c_catering_name AS Name,
                            Cuisine.CuisineNames AS Cuisine,
                            COALESCE(r.AverageRating, 0) AS Rating,
                            COALESCE(r.ReviewCount, 0) AS Reviews,
                            o.c_logo_path AS Image,
                            COALESCE(ops.c_min_guest_count, 0) AS MinOrder,
                            CASE WHEN o.c_approval_status = {ApprovalStatus.Approved.GetHashCode()} THEN 1 ELSE 0 END AS Verified,
                            CASE WHEN o.c_isfeatured = true THEN 1 ELSE 0 END AS Featured
                        FROM {Table.SysCateringOwner} o

                        -- Owner operations (single join)
                        LEFT JOIN {Table.SysCateringOwnerService} ops 
                            ON ops.c_ownerid = o.c_ownerid

                        -- Ratings
                        LEFT JOIN (
                            SELECT 
                                c_ownerid,
                                CAST(AVG(CAST(c_overall_rating AS FLOAT)) AS DECIMAL(3,2)) AS AverageRating,
                                COUNT(*) AS ReviewCount
                            FROM {Table.SysCateringReview}
                            WHERE c_is_visible = TRUE 
                              AND c_is_verified = TRUE
                            GROUP BY c_ownerid
                        ) r ON r.c_ownerid = o.c_ownerid

                        -- Cuisine aggregation (SAFE)
                        LEFT JOIN (
                            SELECT 
                                o2.c_ownerid,
                                STRING_AGG(tm.c_type_name, ', ') AS CuisineNames
                            FROM t_sys_catering_owner o2
                            INNER JOIN {Table.SysCateringOwnerService} ops2 
                                ON ops2.c_ownerid = o2.c_ownerid
                            INNER JOIN {Table.SysCateringTypeMaster} tm
                                ON (',' || ops2.c_cuisine_types || ',') 
                                   LIKE ('%,' || CAST(tm.c_typeid AS VARCHAR) || ',%')
                               AND tm.c_categoryid = {ServiceType.CuisineType.GetHashCode()}
                            GROUP BY o2.c_ownerid
                        ) Cuisine ON Cuisine.c_ownerid = o.c_ownerid

                        WHERE o.c_approval_status = {ApprovalStatus.Approved.GetHashCode()}
                          AND o.c_isactive = TRUE
                          AND (
                                o.c_isfeatured = true
                                OR COALESCE(r.AverageRating, 0) >= 4.5
                              )

                        ORDER BY 
                            o.c_isfeatured DESC,
                            COALESCE(r.AverageRating, 0) DESC
                        LIMIT 6;";

                var result = await _dbHelper.ExecuteAsync(query);
                return MapToFeaturedCatererDto(result);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error fetching featured caterers.", ex);
            }
        }

        /// <summary>
        /// Gets testimonials/reviews for homepage display (max 6)
        /// </summary>
        public async Task<List<HomePageTestimonialDto>> GetHomePageTestimonialsAsync()
        {
            try
            {
                string query = $@"
                    SELECT
                        r.c_reviewid AS Id,
                        r.c_review_comment AS Text,
                        u.c_name AS Author,
                        COALESCE(r.c_review_title, 'Customer') AS Role,
                        r.c_overall_rating AS Rating,
                        COALESCE(ct.c_cityname, 'India') AS Location,
                        u.c_picture AS Image,
                        'Event - Order #' || CAST(r.c_orderid AS VARCHAR) AS Event
                    FROM {Table.SysCateringReview} r
                    INNER JOIN {Table.SysUser} u ON u.c_userid = r.c_userid
                    LEFT JOIN {Table.City} ct ON u.c_cityid = ct.c_cityid
                    WHERE r.c_is_visible = TRUE
                        AND r.c_is_verified = TRUE
                        AND r.c_overall_rating >= 4.5
                        AND LENGTH(COALESCE(r.c_review_comment, '')) > 100
                    ORDER BY r.c_createddate DESC, r.c_overall_rating DESC
                    LIMIT 6";

                var result = await _dbHelper.ExecuteAsync(query);
                return MapToHomePageTestimonialDto(result);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error fetching testimonials.", ex);
            }
        }

        /// <summary>
        /// Gets homepage statistics
        /// </summary>
        public async Task<HomePageStatsDto> GetHomePageStatsAsync()
        {
            try
            {
                string query = $@"
                    SELECT
                        c_total_events_catered AS TotalEventsCatered,
                        c_total_catering_partners AS TotalCateringPartners,
                        c_total_happy_customers AS TotalHappyCustomers,
                        c_satisfaction_rate AS SatisfactionRate
                    FROM {Table.SysHomepageStats}
                    ORDER BY c_last_updated DESC
                    LIMIT 1";

                var result = await _dbHelper.ExecuteAsync(query);

                if (result.Rows.Count == 0)
                {
                    // Return default stats if none exist
                    return new HomePageStatsDto
                    {
                        TotalEventsCatered = 5000,
                        TotalCateringPartners = 500,
                        TotalHappyCustomers = 50000,
                        SatisfactionRate = 98.00m
                    };
                }

                return MapToHomePageStatsDto(result.Rows[0]);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error fetching homepage stats.", ex);
            }
        }

        #endregion

        #region Private Mapping Methods

        /// <summary>
        /// Maps DataTable rows to CateringBusinessListDto models.
        /// Used for browse/search list results.
        /// </summary>
        private List<CateringBusinessListDto> MapOwnerDataToCateringBusinessListDto(DataTable dataTable)
        {
            var cateringList = new List<CateringBusinessListDto>();

            if (dataTable == null || dataTable.Rows.Count == 0)
                return cateringList;

            foreach (DataRow row in dataTable.Rows)
            {
                cateringList.Add(new CateringBusinessListDto
                {
                    Id = Convert.ToInt64(row["CateringId"] ?? 0),
                    CateringName = row["CateringName"]?.ToString(),
                    LogoUrl = row["LogoUrl"]?.ToString(),
                    AverageRating = Convert.ToDouble(row["AverageRating"] ?? 0),
                    TotalReviews = Convert.ToInt32(row["TotalReviews"] ?? 0),
                    MinOrderValue = Convert.ToDecimal(row["MinOrderValue"] ?? 0),
                    DeliveryRadiusKm = Convert.ToInt32(row["DeliveryRadiusKm"] == DBNull.Value ? 0 : row["DeliveryRadiusKm"]),
                    IsOnline = Convert.ToBoolean(row["IsOnline"] ?? false),
                    City = row["City"]?.ToString(),
                    Area = row["Area"]?.ToString(),
                    DistanceKm = Convert.ToDouble(row["DistanceKm"] ?? 0),
                    CuisineTypes = new List<string>(),
                    ServiceTypes = new List<string>()
                });
            }

            return cateringList;
        }

        /// <summary>
        /// Maps a single DataRow to CateringDetailDto model.
        /// Used for detailed catering profile view.
        /// </summary>
        private CateringDetailDto MapOwnerRowToCateringDetailDto(DataRow row)
        {
            return new CateringDetailDto
            {
                CateringId = Convert.ToInt64(row["CateringId"] ?? 0),
                CateringName = row["CateringName"]?.ToString(),
                OwnerName = row["OwnerName"]?.ToString(),
                LogoUrl = row["LogoUrl"]?.ToString(),
                ShopNo = row["ShopNo"]?.ToString(),
                Street = row["Street"]?.ToString(),
                Area = row["Area"]?.ToString(),
                Pincode = row["Pincode"]?.ToString(),
                State = row["State"]?.ToString(),
                City = row["City"]?.ToString(),
                Latitude = row["Latitude"]?.ToString(),
                Longitude = row["Longitude"]?.ToString(),
                MinOrderValue = Convert.ToDecimal(row["MinOrderValue"] ?? 0),
                DeliveryRadiusKm = Convert.ToInt32(row["DeliveryRadiusKm"] ?? 0),
                IsOnline = Convert.ToBoolean(row["IsOnline"] ?? false),
                Description = row["Description"]?.ToString(),
                AverageRating = Convert.ToDouble(row["AverageRating"] ?? 0),
                TotalReviews = Convert.ToInt32(row["TotalReviews"] ?? 0)
            };
        }

        /// <summary>
        /// Maps DataTable rows to FeaturedCatererDto models
        /// </summary>
        private List<FeaturedCatererDto> MapToFeaturedCatererDto(DataTable dataTable)
        {
            var caterers = new List<FeaturedCatererDto>();

            if (dataTable == null || dataTable.Rows.Count == 0)
                return caterers;

            foreach (DataRow row in dataTable.Rows)
            {
                caterers.Add(new FeaturedCatererDto
                {
                    Id = Convert.ToInt64(row["Id"] ?? 0),
                    Name = row["Name"]?.ToString(),
                    Cuisine = row["Cuisine"]?.ToString() ?? "Multi-Cuisine",
                    Rating = Convert.ToDouble(row["Rating"] ?? 0),
                    Reviews = Convert.ToInt32(row["Reviews"] ?? 0),
                    Image = row["Image"]?.ToString() ?? "https://images.unsplash.com/photo-1555244162-803834f70033?w=400&auto=format&fit=crop",
                    MinOrder = Convert.ToInt32(row["MinOrder"] ?? 50),
                    Verified = Convert.ToBoolean(row["Verified"] ?? false),
                    Featured = Convert.ToBoolean(row["Featured"] ?? false),
                    Specialties = new List<string> { "Wedding", "Corporate" } // Can be enhanced from DB
                });
            }

            return caterers;
        }

        /// Maps DataTable rows to HomePageTestimonialDto models
        /// </summary>
        private List<HomePageTestimonialDto> MapToHomePageTestimonialDto(DataTable dataTable)
        {
            var testimonials = new List<HomePageTestimonialDto>();

            if (dataTable == null || dataTable.Rows.Count == 0)
                return testimonials;

            foreach (DataRow row in dataTable.Rows)
            {
                var userName = row["Author"]?.ToString() ?? "Anonymous";
                testimonials.Add(new HomePageTestimonialDto
                {
                    Id = Convert.ToInt64(row["Id"] ?? 0),
                    Text = row["Text"]?.ToString(),
                    Author = userName,
                    Role = row["Role"]?.ToString() ?? "Customer",
                    Rating = Convert.ToInt32(row["Rating"] ?? 5),
                    Location = row["Location"]?.ToString() ?? "India",
                    Image = row["Image"]?.ToString() ?? $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(userName)}&background=FF6B35&color=fff&size=128",
                    Event = row["Event"]?.ToString() ?? "Event"
                });
            }

            return testimonials;
        }

        /// <summary>
        /// Maps a single DataRow to HomePageStatsDto model
        /// </summary>
        private HomePageStatsDto MapToHomePageStatsDto(DataRow row)
        {
            return new HomePageStatsDto
            {
                TotalEventsCatered = Convert.ToInt32(row["TotalEventsCatered"] ?? 0),
                TotalCateringPartners = Convert.ToInt32(row["TotalCateringPartners"] ?? 0),
                TotalHappyCustomers = Convert.ToInt32(row["TotalHappyCustomers"] ?? 0),
                SatisfactionRate = Convert.ToDecimal(row["SatisfactionRate"] ?? 0)
            };
        }

        #endregion

        #region Public Methods - Catering Packages, Food Items, Decorations, Reviews

        /// <summary>
        /// Gets all active packages for a specific caterer
        /// </summary>
        /// <param name="cateringId">The catering owner ID</param>
        /// <returns>List of packages with details</returns>
        public async Task<List<CateringPackageDto>> GetCateringPackagesAsync(long cateringId)
        {
            try
            {
                string query = $@"
                    SELECT
                        p.c_packageid AS PackageId,
                        p.c_packagename AS Name,
                        p.c_description AS Description,
                        p.c_price AS PricePerPerson,
                        50 AS MinGuests,
                        5000 AS MaxGuests,
                        p.c_is_active AS IsAvailable
                    FROM {Table.SysMenuPackage} p
                    WHERE p.c_ownerid = @CateringId
                        AND p.c_is_active = TRUE
                        AND p.c_is_deleted = FALSE
                    ORDER BY p.c_price ASC";

                var parameters = new[] { new NpgsqlParameter("@CateringId", cateringId) };
                var result = await _dbHelper.ExecuteAsync(query, parameters);
                return MapToCateringPackageDto(result);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error fetching packages for catering ID {cateringId}.", ex);
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
                StringBuilder sb = new StringBuilder();

                sb.Append($@"
                    SELECT
                        f.c_foodid AS FoodItemId,
                        f.c_foodname AS Name,
                        f.c_description AS Description,
                        f.c_categoryid AS CategoryId,
                        cat.c_categoryname AS CategoryName,
                        f.c_cuisinetypeid AS CuisineTypeId,
                        f.c_price AS Price,
                        CASE WHEN f.c_isveg = TRUE THEN 1 ELSE 0 END AS IsVegetarian,
                        CASE WHEN f.c_ispackage_item = TRUE THEN 1 ELSE 0 END AS IsIncludedInPackage,
                        CASE WHEN f.c_issample_tasted = TRUE THEN 1 ELSE 0 END AS IsSampleTasted,
                        f.c_status AS IsAvailable,
                        (
                            SELECT STRING_AGG(m.c_file_path, ',' ORDER BY m.c_uploaded_at DESC)
                            FROM {Table.SysCateringMediaUploads} m
                            WHERE m.c_reference_id = f.c_foodid
                                AND m.c_document_type_id = 1
                                AND m.c_is_deleted = FALSE
                                AND m.c_extension NOT IN ('mp4','mov','avi','webm','mkv')
                        ) AS ImagePaths,
                        (
                            SELECT m.c_file_path
                            FROM {Table.SysCateringMediaUploads} m
                            WHERE m.c_reference_id = f.c_foodid
                                AND m.c_document_type_id = 1
                                AND m.c_is_deleted = FALSE
                                AND m.c_extension IN ('mp4','mov','avi','webm','mkv')
                            ORDER BY m.c_uploaded_at DESC
                            LIMIT 1
                        ) AS VideoUrl
                    FROM {Table.SysFoodItems} f
                    LEFT JOIN {Table.SysFoodCategory} cat ON cat.c_categoryid = f.c_categoryid
                    WHERE f.c_ownerid = @CateringId
                        AND f.c_is_deleted = FALSE");

                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@CateringId", cateringId)
                };

                // Optional category filter
                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    sb.Append(" AND f.c_categoryid = @CategoryId");
                    parameters.Add(new NpgsqlParameter("@CategoryId", categoryId.Value));
                }

                // Optional package item filter
                if (isPackageItem.HasValue)
                {
                    sb.Append(" AND f.c_ispackage_item = @IsPackageItem");
                    parameters.Add(new NpgsqlParameter("@IsPackageItem", isPackageItem.Value));
                }

                sb.Append(" ORDER BY cat.c_categoryname, f.c_foodname");

                var result = await _dbHelper.ExecuteAsync(sb.ToString(), parameters.ToArray());
                return MapToCateringFoodItemDto(result);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error fetching food items for catering ID {cateringId}.", ex);
            }
        }

        /// <summary>
        /// Gets decoration themes for a specific caterer
        /// </summary>
        /// <param name="cateringId">The catering owner ID</param>
        /// <returns>List of decoration themes</returns>
        public async Task<List<DecorationDto>> GetCateringDecorationsAsync(long cateringId)
        {
            try
            {
                string query = $@"
                    SELECT
                        d.c_decoration_id AS DecorationId,
                        d.c_decoration_name AS Name,
                        d.c_description AS Description,
                        d.c_theme_id AS ThemeId,
                        t.c_theme_name AS ThemeName,
                        (
                            SELECT m.c_file_path
                            FROM {Table.SysCateringMediaUploads} m
                            WHERE m.c_reference_id = d.c_decoration_id
                                AND m.c_document_type_id = 3
                                AND m.c_is_deleted = FALSE
                                AND m.c_extension NOT IN ('mp4','mov','avi','webm','mkv')
                            ORDER BY m.c_uploaded_at DESC
                            LIMIT 1
                        ) AS ThumbnailUrl,
                        (
                            SELECT m.c_file_path
                            FROM {Table.SysCateringMediaUploads} m
                            WHERE m.c_reference_id = d.c_decoration_id
                                AND m.c_document_type_id = 3
                                AND m.c_is_deleted = FALSE
                                AND m.c_extension IN ('mp4','mov','avi','webm','mkv')
                            ORDER BY m.c_uploaded_at DESC
                            LIMIT 1
                        ) AS VideoUrl,
                        t.c_description AS ThemeDescription,
                        d.c_price AS Price,
                        d.c_packageids AS IncludedInPackageIds,
                        d.c_status AS IsAvailable
                    FROM {Table.SysCateringDecorations} d
                    LEFT JOIN {Table.SysCateringThemeTypes} t ON t.c_theme_id = d.c_theme_id
                    WHERE d.c_ownerid = @CateringId
                        AND d.c_status = 1
                        AND d.c_is_deleted = FALSE
                    ORDER BY d.c_price ASC";

                var parameters = new[] { new NpgsqlParameter("@CateringId", cateringId) };
                var result = await _dbHelper.ExecuteAsync(query, parameters);
                return MapToDecorationDto(result);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error fetching decorations for catering ID {cateringId}.", ex);
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
                string query = $@"
                    SELECT
                        r.c_reviewid AS ReviewId,
                        r.c_ownerid AS CateringId,
                        r.c_userid AS UserId,
                        u.c_name AS UserName,
                        u.c_picture AS UserPhotoUrl,
                        CAST(r.c_overall_rating AS INT) AS Rating,
                        r.c_review_title AS Title,
                        r.c_review_comment AS ReviewText,
                        r.c_createddate AS ReviewDate,
                        'Event Order #' || CAST(r.c_orderid AS VARCHAR) AS EventType
                    FROM {Table.SysCateringReview} r
                    INNER JOIN {Table.SysUser} u ON u.c_userid = r.c_userid
                    WHERE r.c_ownerid = @CateringId
                        AND r.c_is_visible = TRUE
                        AND r.c_is_verified = TRUE
                    ORDER BY r.c_createddate DESC
                    LIMIT @PageSize OFFSET (@PageNumber - 1) * @PageSize";

                var parameters = new[]
                {
                    new NpgsqlParameter("@CateringId", cateringId),
                    new NpgsqlParameter("@PageNumber", pageNumber),
                    new NpgsqlParameter("@PageSize", pageSize)
                };

                var result = await _dbHelper.ExecuteAsync(query, parameters);
                return MapToCateringReviewDto(result);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error fetching reviews for catering ID {cateringId}.", ex);
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
                string query = $@"
                    SELECT
                        c.c_categoryid AS CategoryId,
                        c.c_categoryname AS Name,
                        c.c_description AS Description,
                        COUNT(f.c_foodid) AS ItemCount
                    FROM {Table.SysFoodCategory} c
                    LEFT JOIN {Table.SysFoodItems} f
                        ON f.c_categoryid = c.c_categoryid
                        AND f.c_is_deleted = FALSE
                        AND f.c_status = 1
                    WHERE c.c_isactive = TRUE
                        AND c.c_is_global = TRUE
                    GROUP BY c.c_categoryid, c.c_categoryname, c.c_description
                    ORDER BY c.c_categoryname";

                var result = await _dbHelper.ExecuteAsync(query);
                return MapToFoodCategoryDisplayDto(result);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error fetching food categories.", ex);
            }
        }

        #endregion

        #region Private Mapping Methods - Extended

        /// <summary>
        /// Maps DataTable to CateringPackageDto list
        /// </summary>
        private List<CateringPackageDto> MapToCateringPackageDto(DataTable dataTable)
        {
            var packages = new List<CateringPackageDto>();

            if (dataTable == null || dataTable.Rows.Count == 0)
                return packages;

            foreach (DataRow row in dataTable.Rows)
            {
                packages.Add(new CateringPackageDto
                {
                    PackageId = Convert.ToInt64(row["PackageId"]),
                    Name = row["Name"]?.ToString(),
                    Description = row["Description"]?.ToString(),
                    PricePerPerson = Convert.ToDecimal(row["PricePerPerson"] ?? 0),
                    MinGuests = Convert.ToInt32(row["MinGuests"] ?? 50),
                    MaxGuests = Convert.ToInt32(row["MaxGuests"] ?? 5000),
                    IsAvailable = Convert.ToBoolean(row["IsAvailable"] ?? false),
                    Items = new List<PackageItemDetailDto>() // Will be populated separately if needed
                });
            }

            return packages;
        }

        /// <summary>
        /// Maps DataTable to CateringFoodItemDto list
        /// </summary>
        private List<CateringFoodItemDto> MapToCateringFoodItemDto(DataTable dataTable)
        {
            var foodItems = new List<CateringFoodItemDto>();

            if (dataTable == null || dataTable.Rows.Count == 0)
                return foodItems;

            foreach (DataRow row in dataTable.Rows)
            {
                foodItems.Add(new CateringFoodItemDto
                {
                    FoodItemId = Convert.ToInt64(row["FoodItemId"]),
                    Name = row["Name"]?.ToString(),
                    Description = row["Description"]?.ToString(),
                    CategoryId = Convert.ToInt32(row["CategoryId"] ?? 0),
                    CategoryName = row["CategoryName"]?.ToString(),
                    CuisineTypeId = row["CuisineTypeId"] != DBNull.Value
                        ? Convert.ToInt32(row["CuisineTypeId"])
                        : (int?)null,
                    Price = Convert.ToDecimal(row["Price"] ?? 0),
                    IsVegetarian = Convert.ToBoolean(row["IsVegetarian"] ?? false),
                    IsIncludedInPackage = Convert.ToBoolean(row["IsIncludedInPackage"] ?? false),
                    IsSampleTasted = Convert.ToBoolean(row["IsSampleTasted"] ?? false),
                    IsAvailable = Convert.ToBoolean(row["IsAvailable"] ?? false),
                    ImageUrls = string.IsNullOrEmpty(row["ImagePaths"]?.ToString())
                        ? new List<string>()
                        : row["ImagePaths"].ToString()!.Split(',').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)).ToList(),
                    VideoUrl = row["VideoUrl"]?.ToString()
                });
            }

            return foodItems;
        }

        /// <summary>
        /// Maps DataTable to DecorationDto list
        /// </summary>
        private List<DecorationDto> MapToDecorationDto(DataTable dataTable)
        {
            var decorations = new List<DecorationDto>();

            if (dataTable == null || dataTable.Rows.Count == 0)
                return decorations;

            foreach (DataRow row in dataTable.Rows)
            {
                decorations.Add(new DecorationDto
                {
                    DecorationId = Convert.ToInt64(row["DecorationId"]),
                    Name = row["Name"]?.ToString(),
                    Description = row["Description"]?.ToString(),
                    ThemeId = Convert.ToInt32(row["ThemeId"] ?? 0),
                    ThemeName = row["ThemeName"]?.ToString(),
                    ThemeDescription = row["ThemeDescription"]?.ToString(),
                    Price = Convert.ToDecimal(row["Price"] ?? 0),
                    IncludedInPackageIds = row["IncludedInPackageIds"]?.ToString(),
                    IsAvailable = Convert.ToBoolean(row["IsAvailable"] ?? false),
                    ThumbnailUrl = row["ThumbnailUrl"]?.ToString(),
                    VideoUrl = row["VideoUrl"]?.ToString()
                });
            }

            return decorations;
        }

        /// <summary>
        /// Maps DataTable to CateringReviewDto list
        /// </summary>
        private List<CateringReviewDto> MapToCateringReviewDto(DataTable dataTable)
        {
            var reviews = new List<CateringReviewDto>();

            if (dataTable == null || dataTable.Rows.Count == 0)
                return reviews;

            foreach (DataRow row in dataTable.Rows)
            {
                reviews.Add(new CateringReviewDto
                {
                    ReviewId = Convert.ToInt64(row["ReviewId"]),
                    CateringId = Convert.ToInt64(row["CateringId"]),
                    UserId = Convert.ToInt64(row["UserId"]),
                    UserName = row["UserName"]?.ToString(),
                    UserPhotoUrl = row["UserPhotoUrl"]?.ToString(),
                    Rating = Convert.ToInt32(row["Rating"] ?? 0),
                    Title = row["Title"]?.ToString(),
                    ReviewText = row["ReviewText"]?.ToString(),
                    ReviewDate = Convert.ToDateTime(row["ReviewDate"]),
                    PhotoUrls = new List<string>() // Will be populated separately if needed
                });
            }

            return reviews;
        }

        /// <summary>
        /// Maps DataTable to FoodCategoryDisplayDto list
        /// </summary>
        private List<FoodCategoryDisplayDto> MapToFoodCategoryDisplayDto(DataTable dataTable)
        {
            var categories = new List<FoodCategoryDisplayDto>();

            if (dataTable == null || dataTable.Rows.Count == 0)
                return categories;

            foreach (DataRow row in dataTable.Rows)
            {
                categories.Add(new FoodCategoryDisplayDto
                {
                    CategoryId = Convert.ToInt32(row["CategoryId"]),
                    Name = row["Name"]?.ToString(),
                    Description = row["Description"]?.ToString(),
                    ItemCount = Convert.ToInt32(row["ItemCount"] ?? 0)
                });
            }

            return categories;
        }

        private static bool IsVideoExtension(string extension)
        {
            return extension is "mp4" or "mov" or "avi" or "webm" or "mkv" or "ogg" or "m4v";
        }

        #endregion

        #region Comprehensive Search

        /// <summary>
        /// Advanced search for catering services with comprehensive filtering.
        /// Supports: city, keyword (name/tags/cuisine/description/packages), catering types, rating, price range.
        /// Uses full-text search capabilities for optimal performance.
        /// </summary>
        /// <param name="filter">Search filter criteria</param>
        /// <param name="cityId">Resolved city ID (nullable)</param>
        /// <returns>Paginated search results with total count</returns>
        public async Task<CateringSearchResultDto> SearchCateringsWithFiltersAsync(CateringSearchFilterDto filter,int? cityId)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                List<NpgsqlParameter> parameters = new();

                // ==============================
                // STEP 1: CREATE TEMP TABLE
                // ==============================
                sb.Append($@"
            SELECT
                o.c_ownerid AS CateringId,
                o.c_catering_name AS CateringName,
                o.c_logo_path AS LogoUrl,
                CASE WHEN status.c_global_status = 1 THEN 1 ELSE 0 END AS IsOnline,
                COALESCE(r.AverageRating, 0) AS AverageRating,
                COALESCE(r.ReviewCount, 0) AS TotalReviews,
                service.c_min_guest_count AS MinOrderValue,
                service.c_delivery_radius_km AS DeliveryRadiusKm,
                ct.c_cityname AS City,
                address.c_area AS Area,
                0 AS DistanceKm,
                service.c_cuisine_types AS CuisineTypesIds,
                service.c_service_types AS ServiceTypesIds,
                service.c_event_types AS EventTypesIds
            FROM {Table.SysCateringOwner} o
            LEFT JOIN {Table.SysCateringOwnerAddress} address 
                ON address.c_ownerid = o.c_ownerid
            LEFT JOIN {Table.SysCateringOwnerService} service 
                ON service.c_ownerid = o.c_ownerid
            LEFT JOIN {Table.SysCateringAvailabilityGlobal} status 
                ON status.c_ownerid = o.c_ownerid
            LEFT JOIN {Table.City} ct 
                ON address.c_cityid = ct.c_cityid
            LEFT JOIN (
                SELECT c_ownerid,
                       CAST(AVG(CAST(c_overall_rating AS FLOAT)) AS DECIMAL(3,2)) AS AverageRating,
                       COUNT(*) AS ReviewCount
                FROM {Table.SysCateringReview}
                WHERE c_is_visible = TRUE AND c_is_verified = TRUE
                GROUP BY c_ownerid
            ) r ON o.c_ownerid = r.c_ownerid
            WHERE o.c_isactive = TRUE
        ");

                // ==============================
                // STEP 2: APPLY FILTERS
                // ==============================

                if (filter.VerifiedOnly != false)
                    sb.Append($" AND o.c_approval_status = {ApprovalStatus.Approved.GetHashCode()} ");

                if (cityId.HasValue && cityId.Value > 0)
                {
                    sb.Append(" AND address.c_cityid = @CityID ");
                    parameters.Add(new NpgsqlParameter("@CityID", cityId.Value));
                }

                if (filter.OnlineOnly == true)
                    sb.Append(" AND status.c_global_status = 1 ");

                if (filter.MinRating.HasValue && filter.MinRating.Value > 0)
                {
                    sb.Append(" AND COALESCE(r.AverageRating,0) >= @MinRating ");
                    parameters.Add(new NpgsqlParameter("@MinRating", filter.MinRating.Value));
                }

                if (filter.MinOrderValueFrom.HasValue)
                {
                    sb.Append(" AND service.c_min_guest_count >= @MinOrderFrom ");
                    parameters.Add(new NpgsqlParameter("@MinOrderFrom", filter.MinOrderValueFrom.Value));
                }

                if (filter.MinOrderValueTo.HasValue)
                {
                    sb.Append(" AND service.c_min_guest_count <= @MinOrderTo ");
                    parameters.Add(new NpgsqlParameter("@MinOrderTo", filter.MinOrderValueTo.Value));
                }

                if (filter.DeliveryRadiusKm.HasValue && filter.DeliveryRadiusKm.Value > 0)
                {
                    sb.Append(" AND service.c_delivery_radius_km >= @DeliveryRadius ");
                    parameters.Add(new NpgsqlParameter("@DeliveryRadius", filter.DeliveryRadiusKm.Value));
                }

                // Cuisine filters
                if (filter.CuisineTypeIds?.Any() == true)
                {
                    sb.Append(" AND (");
                    for (int i = 0; i < filter.CuisineTypeIds.Count; i++)
                    {
                        if (i > 0) sb.Append(" OR ");
                        sb.Append($" (',' || service.c_cuisine_types || ',') LIKE ('%,' || CAST(@Cuisine{i} AS TEXT) || ',%') ");
                        parameters.Add(new NpgsqlParameter($"@Cuisine{i}", filter.CuisineTypeIds[i]));
                    }
                    sb.Append(") ");
                }

                // Service filters
                if (filter.ServiceTypeIds?.Any() == true)
                {
                    sb.Append(" AND (");
                    for (int i = 0; i < filter.ServiceTypeIds.Count; i++)
                    {
                        if (i > 0) sb.Append(" OR ");
                        sb.Append($" (',' || service.c_service_types || ',') LIKE ('%,' || CAST(@Service{i} AS TEXT) || ',%') ");
                        parameters.Add(new NpgsqlParameter($"@Service{i}", filter.ServiceTypeIds[i]));
                    }
                    sb.Append(") ");
                }

                // Event filters
                if (filter.EventTypeIds?.Any() == true)
                {
                    sb.Append(" AND (");
                    for (int i = 0; i < filter.EventTypeIds.Count; i++)
                    {
                        if (i > 0) sb.Append(" OR ");
                        sb.Append($" (',' || service.c_event_types || ',') LIKE ('%,' || CAST(@Event{i} AS TEXT) || ',%') ");
                        parameters.Add(new NpgsqlParameter($"@Event{i}", filter.EventTypeIds[i]));
                    }
                    sb.Append(") ");
                }

                // Decorations filter - filter caterings that have decorations
                if (filter.HasDecorations == true)
                {
                    sb.Append($@"
                AND EXISTS (
                    SELECT 1
                    FROM {Table.SysCateringDecorations} deco
                    WHERE deco.c_ownerid = o.c_ownerid
                    AND deco.c_is_active = TRUE
                ) ");
                }

                // Keyword search
                if (!string.IsNullOrWhiteSpace(filter.SearchKeyword))
                {
                    sb.Append(@"
                AND (
                    o.c_catering_name LIKE @Keyword
                    OR address.c_area LIKE @Keyword
                )
            ");
                    parameters.Add(new NpgsqlParameter("@Keyword", $"%{filter.SearchKeyword.Trim()}%"));
                }

                // ==============================
                // STEP 3: PAGINATED RESULT
                // ==============================
                sb.Insert(0, "SELECT *, COUNT(*) OVER() AS TotalCount FROM (");
                sb.Append(@"
            ) CateringSearch
            ORDER BY AverageRating DESC, CateringName ASC
            LIMIT @PageSize OFFSET (@PageNumber - 1) * @PageSize;
        ");

                parameters.Add(new NpgsqlParameter("@PageNumber", filter.PageNumber));
                parameters.Add(new NpgsqlParameter("@PageSize", filter.PageSize));

                var result = await _dbHelper.ExecuteAsync(sb.ToString(), parameters.ToArray());

                int totalCount = result.Rows.Count > 0 && result.Columns.Contains("TotalCount") && result.Rows[0]["TotalCount"] != DBNull.Value
                    ? Convert.ToInt32(result.Rows[0]["TotalCount"])
                    : 0;

                var cateringList = MapOwnerDataToCateringBusinessListDto(result);

                return new CateringSearchResultDto
                {
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    Results = cateringList
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Error performing advanced catering search.",
                    ex
                );
            }
        }


        #endregion

        #region Package Selection

        /// <summary>
        /// Gets package selection details with categories, allowed quantities, and eligible food items
        /// Returns hierarchical data: Package ? Categories ? Food Items
        /// Only returns food items where c_ispackage_item = TRUE, c_status = TRUE, c_is_deleted = FALSE
        /// </summary>
        /// <param name="packageId">The package ID</param>
        /// <param name="cateringId">The catering owner ID (for validation)</param>
        /// <returns>Complete package selection structure</returns>
        public async Task<PackageSelectionDto> GetPackageSelectionDetailsAsync(long packageId, long cateringId)
        {
            try
            {
                // Step 1: Get Package Basic Info
                string packageQuery = $@"
                    SELECT
                        p.c_packageid AS PackageId,
                        p.c_packagename AS PackageName,
                        p.c_description AS Description,
                        p.c_price AS Price,
                        p.c_ownerid AS OwnerId
                    FROM {Table.SysMenuPackage} p
                    WHERE p.c_packageid = @PackageId
                        AND p.c_ownerid = @CateringId
                        AND p.c_is_active = TRUE
                        AND p.c_is_deleted = FALSE";

                var packageParams = new[]
                {
                    new NpgsqlParameter("@PackageId", packageId),
                    new NpgsqlParameter("@CateringId", cateringId)
                };

                var packageResult = await _dbHelper.ExecuteAsync(packageQuery, packageParams);

                if (packageResult == null || packageResult.Rows.Count == 0)
                {
                    throw new KeyNotFoundException($"Package ID {packageId} not found or not active.");
                }

                var packageRow = packageResult.Rows[0];
                var packageDto = new PackageSelectionDto
                {
                    PackageId = Convert.ToInt64(packageRow["PackageId"]),
                    PackageName = packageRow["PackageName"]?.ToString(),
                    Description = packageRow["Description"]?.ToString(),
                    Price = Convert.ToDecimal(packageRow["Price"] ?? 0),
                    Categories = new List<PackageCategoryDto>(),
                    Decorations = new List<PackageDecorationDto>()
                };

                // Step 2: Get Categories with Allowed Quantities
                string categoryQuery = $@"
                    SELECT
                        pi.c_categoryid AS CategoryId,
                        fc.c_categoryname AS CategoryName,
                        fc.c_description AS CategoryDescription,
                        pi.c_quantity AS AllowedQuantity
                    FROM {Table.SysMenuPackageItems} pi
                    INNER JOIN {Table.SysFoodCategory} fc ON fc.c_categoryid = pi.c_categoryid
                    WHERE pi.c_packageid = @PackageId
                        AND fc.c_isactive = TRUE
                    ORDER BY fc.c_categoryname";

                var categoryParams = new[] { new NpgsqlParameter("@PackageId", packageId) };
                var categoryResult = await _dbHelper.ExecuteAsync(categoryQuery, categoryParams);

                if (categoryResult == null || categoryResult.Rows.Count == 0)
                {
                    // Package has no category mappings
                    return packageDto;
                }

                // Step 3: For each category, get eligible food items
                foreach (DataRow catRow in categoryResult.Rows)
                {
                    long categoryId = Convert.ToInt64(catRow["CategoryId"]);

                    var categoryDto = new PackageCategoryDto
                    {
                        CategoryId = categoryId,
                        CategoryName = catRow["CategoryName"]?.ToString(),
                        CategoryDescription = catRow["CategoryDescription"]?.ToString(),
                        AllowedQuantity = Convert.ToInt32(catRow["AllowedQuantity"] ?? 0),
                        FoodItems = new List<PackageFoodItemDto>()
                    };

                    // Get food items for this category WITH IMAGES
                    string foodItemsQuery = $@"
                        SELECT
                            f.c_foodid AS FoodId,
                            f.c_foodname AS FoodName,
                            f.c_description AS Description,
                            f.c_price AS Price,
                            COALESCE(ct.c_type_name, '') AS CuisineType,
                            -- Get food images (comma-separated if multiple)
                            (
                                SELECT STRING_AGG(m.c_file_path, ',' ORDER BY m.c_uploaded_at DESC)
                                FROM {Table.SysCateringMediaUploads} m
                                WHERE m.c_reference_id = f.c_foodid
                                    AND m.c_document_type_id = 1  -- Food document type
                                    AND m.c_is_deleted = FALSE
                            ) AS ImagePaths
                        FROM {Table.SysFoodItems} f
                        LEFT JOIN {Table.SysCateringTypeMaster} ct ON ct.c_typeid = f.c_cuisinetypeid
                        WHERE f.c_ownerid = @CateringId
                            AND f.c_categoryid = @CategoryId
                            AND f.c_ispackage_item = TRUE
                            AND f.c_status = 1
                            AND f.c_is_deleted = FALSE
                        ORDER BY f.c_foodname";

                    var foodItemParams = new[]
                    {
                        new NpgsqlParameter("@CateringId", cateringId),
                        new NpgsqlParameter("@CategoryId", categoryId)
                    };

                    var foodItemsResult = await _dbHelper.ExecuteAsync(foodItemsQuery, foodItemParams);

                    if (foodItemsResult != null && foodItemsResult.Rows.Count > 0)
                    {
                        foreach (DataRow foodRow in foodItemsResult.Rows)
                        {
                            var imagePathsString = foodRow["ImagePaths"]?.ToString() ?? "";
                            var imageUrls = string.IsNullOrEmpty(imagePathsString)
                                ? new List<string>()
                                : imagePathsString.Split(',').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)).ToList();

                            categoryDto.FoodItems.Add(new PackageFoodItemDto
                            {
                                FoodId = Convert.ToInt64(foodRow["FoodId"]),
                                FoodName = foodRow["FoodName"]?.ToString(),
                                Description = foodRow["Description"]?.ToString(),
                                Price = Convert.ToDecimal(foodRow["Price"] ?? 0),
                                CuisineType = foodRow["CuisineType"]?.ToString(),
                                ImageUrls = imageUrls
                            });
                        }
                    }

                    packageDto.Categories.Add(categoryDto);
                }

                packageDto.Decorations = await GetPackageDecorationsAsync(packageId, cateringId);

                return packageDto;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error fetching package selection details for package ID {packageId}.", ex);
            }
        }

        private async Task<List<PackageDecorationDto>> GetPackageDecorationsAsync(long packageId, long cateringId)
        {
            string decorationsQuery = $@"
                SELECT
                    d.c_decoration_id AS DecorationId,
                    d.c_decoration_name AS Name,
                    d.c_description AS Description,
                    t.c_theme_name AS ThemeName,
                    d.c_price AS Price,
                    d.c_status AS IsAvailable,
                    d.c_packageids AS IncludedInPackageIds,
                    (
                        SELECT m.c_file_path
                        FROM {Table.SysCateringMediaUploads} m
                        WHERE m.c_reference_id = d.c_decoration_id
                            AND m.c_document_type_id = 3
                            AND m.c_is_deleted = FALSE
                            AND LOWER(COALESCE(m.c_extension, '')) NOT IN ('mp4', 'mov', 'avi', 'webm', 'mkv')
                        ORDER BY m.c_uploaded_at DESC
                        LIMIT 1
                    ) AS ThumbnailUrl,
                    (
                        SELECT m.c_file_path
                        FROM {Table.SysCateringMediaUploads} m
                        WHERE m.c_reference_id = d.c_decoration_id
                            AND m.c_document_type_id = 3
                            AND m.c_is_deleted = FALSE
                            AND LOWER(COALESCE(m.c_extension, '')) IN ('mp4', 'mov', 'avi', 'webm', 'mkv')
                        ORDER BY m.c_uploaded_at DESC
                        LIMIT 1
                    ) AS VideoUrl
                FROM {Table.SysCateringDecorations} d
                LEFT JOIN {Table.SysCateringThemeTypes} t ON t.c_theme_id = d.c_theme_id
                WHERE d.c_ownerid = @CateringId
                    AND d.c_status = 1
                    AND d.c_is_deleted = FALSE
                    AND EXISTS (
                        SELECT 1
                        FROM unnest(string_to_array(COALESCE(d.c_packageids, ''), ',')) AS linked(value)
                        WHERE CAST(NULLIF(TRIM(linked.value), '') AS BIGINT) = @PackageId
                    )
                ORDER BY d.c_price ASC, d.c_decoration_name ASC";

            var decorationParams = new[]
            {
                new NpgsqlParameter("@CateringId", cateringId),
                new NpgsqlParameter("@PackageId", packageId)
            };

            var decorationsResult = await _dbHelper.ExecuteAsync(decorationsQuery, decorationParams);
            var decorations = new List<PackageDecorationDto>();

            if (decorationsResult == null || decorationsResult.Rows.Count == 0)
            {
                return decorations;
            }

            foreach (DataRow decorationRow in decorationsResult.Rows)
            {
                long decorationId = Convert.ToInt64(decorationRow["DecorationId"]);
                var decoration = new PackageDecorationDto
                {
                    DecorationId = decorationId,
                    Name = decorationRow["Name"]?.ToString() ?? string.Empty,
                    Description = decorationRow["Description"]?.ToString(),
                    ThemeName = decorationRow["ThemeName"]?.ToString(),
                    Price = Convert.ToDecimal(decorationRow["Price"] ?? 0),
                    IsAvailable = Convert.ToBoolean(decorationRow["IsAvailable"] ?? false),
                    IncludedInPackageIds = decorationRow["IncludedInPackageIds"]?.ToString(),
                    ThumbnailUrl = decorationRow["ThumbnailUrl"]?.ToString(),
                    VideoUrl = decorationRow["VideoUrl"]?.ToString(),
                    MediaItems = new List<PackageDecorationMediaDto>()
                };

                string mediaQuery = $@"
                    SELECT
                        m.c_file_path AS FilePath,
                        m.c_file_name AS FileName,
                        m.c_extension AS FileExtension
                    FROM {Table.SysCateringMediaUploads} m
                    WHERE m.c_reference_id = @DecorationId
                        AND m.c_document_type_id = 3
                        AND m.c_is_deleted = FALSE
                    ORDER BY m.c_uploaded_at DESC";

                var mediaResult = await _dbHelper.ExecuteAsync(
                    mediaQuery,
                    new[] { new NpgsqlParameter("@DecorationId", decorationId) }
                );

                if (mediaResult != null && mediaResult.Rows.Count > 0)
                {
                    foreach (DataRow mediaRow in mediaResult.Rows)
                    {
                        var extension = mediaRow["FileExtension"]?.ToString()?.Trim().ToLowerInvariant() ?? string.Empty;
                        decoration.MediaItems.Add(new PackageDecorationMediaDto
                        {
                            FilePath = mediaRow["FilePath"]?.ToString() ?? string.Empty,
                            FileName = mediaRow["FileName"]?.ToString(),
                            Label = decoration.Name,
                            MediaType = IsVideoExtension(extension) ? "video" : "image"
                        });
                    }
                }

                decorations.Add(decoration);
            }

            return decorations;
        }

        /// <summary>
        /// Gets food categories included in a specific package.
        /// Returns category information from package items.
        /// </summary>
        /// <param name="packageId">The package ID</param>
        /// <param name="cateringId">The catering owner ID</param>
        /// <returns>List of food categories in the package</returns>
        public async Task<List<PackageCategoryBasicDto>> GetPackageCategoriesAsync(long packageId, long cateringId)
        {
            try
            {
                string query = $@"
                    SELECT DISTINCT
                        fc.c_categoryid AS CategoryId,
                        fc.c_categoryname AS CategoryName,
                        fc.c_description AS Description
                    FROM {Table.SysMenuPackageItems} pi
                    INNER JOIN {Table.SysFoodCategory} fc ON fc.c_categoryid = pi.c_categoryid
                    INNER JOIN {Table.SysMenuPackage} p ON p.c_packageid = pi.c_packageid
                    WHERE pi.c_packageid = @PackageId
                        AND p.c_ownerid = @CateringId
                        AND p.c_is_active = TRUE
                        AND p.c_is_deleted = FALSE
                        AND fc.c_isactive = TRUE
                    ORDER BY fc.c_categoryname";

                var parameters = new[]
                {
                    new NpgsqlParameter("@PackageId", packageId),
                    new NpgsqlParameter("@CateringId", cateringId)
                };

                var result = await _dbHelper.ExecuteAsync(query, parameters);
                var categories = new List<PackageCategoryBasicDto>();

                if (result != null && result.Rows.Count > 0)
                {
                    foreach (DataRow row in result.Rows)
                    {
                        categories.Add(new PackageCategoryBasicDto
                        {
                            CategoryId = Convert.ToInt64(row["CategoryId"]),
                            CategoryName = row["CategoryName"]?.ToString(),
                            Description = row["Description"]?.ToString()
                        });
                    }
                }

                return categories;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error fetching package categories for package ID {packageId}.", ex);
            }
        }

        #endregion
    }
}

