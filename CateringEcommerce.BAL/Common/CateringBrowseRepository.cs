using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Models.User;
using Microsoft.Data.SqlClient;
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
        private readonly SqlDatabaseManager _db;

        public CateringBrowseRepository(string connectionString)
        {
            _db = new SqlDatabaseManager();
            _db.SetConnectionString(connectionString);
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
                        status.c_global_status AS Status,
                        ISNULL(r.AverageRating, 0) AS AverageRating,
                        ISNULL(r.ReviewCount, 0) AS TotalReviews,
                        service.c_min_dish_order AS MinOrderValue,
                        service.c_delivery_radius_km AS DeliveryRadiusKm,
                        CASE WHEN o.c_isonline = 1 THEN 1 ELSE 0 END AS IsOnline,
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
                               CAST(ISNULL(AVG(CAST(c_overall_rating AS FLOAT)), 0) AS DECIMAL(3,2)) AS AverageRating,
                               COUNT(*) AS ReviewCount
                        FROM {Table.SysCateringReview}
                        WHERE c_is_visible = 1 and c_is_verified = 1
                        GROUP BY c_ownerid
                    ) r ON o.c_ownerid = r.c_ownerid
                    WHERE o.c_verified_by_admin = 1 
                        AND o.c_isactive = 1
                ");

                List<SqlParameter> parameters = new();

                // ✅ Optional city filter
                if (cityId.HasValue && cityId.Value > 0)
                {
                    sb.Append(" AND address.c_cityid = @CityID ");
                    parameters.Add(new SqlParameter("@CityID", cityId.Value));
                }

                sb.Append(" ORDER BY o.c_catering_name ASC ");

                var result = await _db.ExecuteAsync(sb.ToString(), parameters.ToArray());
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
        /// </summary>
        /// <param name="cateringId">The owner/catering ID</param>
        /// <returns>Detailed catering profile with contact info, services, etc.</returns>
        public async Task<CateringDetailDto> GetCateringDetailForUserBrowseAsync(long cateringId)
        {
            try
            {
                string query = $@"
                    SELECT 
                        o.c_ownerid AS CateringId,
                        o.c_businessname AS CateringName,
                        o.c_owner_name AS OwnerName,
                        o.c_logo AS LogoUrl,
                        o.c_owner_phone AS Phone,
                        o.c_owner_email AS Email,
                        o.c_catering_number AS CateringNumber,
                        ISNULL(o.c_whatsapp_number, '') AS WhatsAppNumber,
                        ISNULL(o.c_support_email, '') AS SupportEmail,
                        o.c_shop_no AS ShopNo,
                        o.c_street AS Street,
                        o.c_area AS Area,
                        o.c_pincode AS Pincode,
                        st.c_statename AS State,
                        ct.c_cityname AS City,
                        ISNULL(o.c_latitude, '') AS Latitude,
                        ISNULL(o.c_longitude, '') AS Longitude,
                        ISNULL(o.c_min_order_value, 0) AS MinOrderValue,
                        ISNULL(o.c_delivery_radius, 0) AS DeliveryRadiusKm,
                        CASE WHEN o.c_is_online = 1 THEN 1 ELSE 0 END AS IsOnline,
                        ISNULL(o.c_description, '') AS Description,
                        CASE WHEN o.c_is_verified = 1 THEN 1 ELSE 0 END AS IsVerifiedByAdmin,
                        o.c_verification_date AS VerificationDate,
                        ISNULL(r.AverageRating, 0) AS AverageRating,
                        ISNULL(r.ReviewCount, 0) AS TotalReviews
                    FROM {Table.SysCateringOwner} o
                    LEFT JOIN {Table.City} ct ON o.c_cityid = ct.c_cityid
                    LEFT JOIN {Table.State} st ON o.c_stateid = st.c_stateid
                    LEFT JOIN (
                        SELECT c_ownerid, 
                               CAST(ISNULL(AVG(CAST(c_rating AS FLOAT)), 0) AS DECIMAL(3,2)) AS AverageRating,
                               COUNT(*) AS ReviewCount
                        FROM {Table.SysCateringReview}
                        WHERE c_is_active = 1
                        GROUP BY c_ownerid
                    ) r ON o.c_ownerid = r.c_ownerid
                    WHERE o.c_ownerid = @CateringId 
                        AND o.c_is_verified = 1 
                        AND o.c_is_active = 1";

                var parameters = new[] { new SqlParameter("@CateringId", cateringId) };
                var result = await _db.ExecuteAsync(query, parameters);

                if (result.Rows.Count == 0)
                    return null;

                return MapOwnerRowToCateringDetailDto(result.Rows[0]);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error fetching catering detail for catering ID {cateringId}.", ex);
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
                    CateringId = Convert.ToInt64(row["CateringId"] ?? 0),
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
                Phone = row["Phone"]?.ToString(),
                Email = row["Email"]?.ToString(),
                CateringNumber = row["CateringNumber"]?.ToString(),
                WhatsAppNumber = row["WhatsAppNumber"]?.ToString(),
                SupportEmail = row["SupportEmail"]?.ToString(),
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
                IsVerifiedByAdmin = Convert.ToBoolean(row["IsVerifiedByAdmin"] ?? false),
                VerificationDate = row["VerificationDate"] != DBNull.Value 
                    ? Convert.ToDateTime(row["VerificationDate"]) 
                    : null,
                AverageRating = Convert.ToDouble(row["AverageRating"] ?? 0),
                TotalReviews = Convert.ToInt32(row["TotalReviews"] ?? 0)
            };
        }

        #endregion
    }
}
