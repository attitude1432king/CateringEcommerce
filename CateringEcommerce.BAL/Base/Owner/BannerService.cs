using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;
using Npgsql;
using System.Text;

namespace CateringEcommerce.BAL.Base.Owner
{
    public class BannerService: IBannerService
    {
        private readonly IDatabaseHelper _dbHelper;
        public BannerService(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        /// <summary>
        /// Get count of banners with filters
        /// </summary>
        public async Task<int> GetBannersCount(long ownerPKID, BannerFilter filter)
        {
            try
            {
                StringBuilder countQuery = new StringBuilder();
                countQuery.Append($@"SELECT COUNT(1) FROM {Table.SysCateringBanners} banner");

                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@OwnerPKID", ownerPKID)
                };

                countQuery.Append(BuildFilterQuery(filter, parameters));

                var result = await _dbHelper.ExecuteScalarAsync(countQuery.ToString(), parameters.ToArray());
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Get paginated list of banners
        /// </summary>
        public async Task<List<BannerDto>> GetBanners(long ownerPKID, int page, int pageSize, BannerFilter filter)
        {
            try
            {
                int offset = (page - 1) * pageSize;
                StringBuilder selectQuery = new StringBuilder();

                selectQuery.Append($@"
                    SELECT
                        c_bannerid,
                        c_title,
                        c_description,
                        media.c_file_path,
                        c_link_url,
                        c_display_order,
                        c_isactive,
                        c_start_date,
                        c_end_date,
                        c_click_count,
                        c_view_count
                    FROM {Table.SysCateringBanners} banner 
                    LEFT JOIN {Table.SysCateringMediaUploads} media ON media.c_reference_id = banner.c_bannerid");

                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@OwnerPKID", ownerPKID),
                    new NpgsqlParameter("@Offset", offset),
                    new NpgsqlParameter("@PageSize", pageSize),
                    new NpgsqlParameter("@DocumentTypeID", DocumentType.Banner.GetHashCode())
                };

                selectQuery.Append(BuildFilterQuery(filter, parameters));

                selectQuery.Append(@"
                    AND c_document_type_id = @DocumentTypeID
                    ORDER BY c_display_order ASC, c_createddate DESC
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY;
                ");

                var bannersData = await _dbHelper.ExecuteAsync(selectQuery.ToString(), parameters.ToArray());

                if (bannersData.Rows.Count == 0)
                    return new List<BannerDto>();

                var banners = new List<BannerDto>();

                foreach (System.Data.DataRow row in bannersData.Rows)
                {
                    banners.Add(new BannerDto
                    {
                        Id = row["c_bannerid"] != DBNull.Value ? Convert.ToInt64(row["c_bannerid"]) : 0,
                        Title = row["c_title"]?.ToString(),
                        Description = row["c_description"]?.ToString(),
                        ImagePath = row["c_file_path"]?.ToString(),
                        LinkUrl = row["c_link_url"]?.ToString(),
                        DisplayOrder = row["c_display_order"] != DBNull.Value ? Convert.ToInt32(row["c_display_order"]) : 0,
                        IsActive = row["c_isactive"] != DBNull.Value && Convert.ToBoolean(row["c_isactive"]),
                        StartDate = row["c_start_date"] != DBNull.Value ? Convert.ToDateTime(row["c_start_date"]) : null,
                        EndDate = row["c_end_date"] != DBNull.Value ? Convert.ToDateTime(row["c_end_date"]) : null,
                        ClickCount = row["c_click_count"] != DBNull.Value ? Convert.ToInt32(row["c_click_count"]) : 0,
                        ViewCount = row["c_view_count"] != DBNull.Value ? Convert.ToInt32(row["c_view_count"]) : 0
                    });
                }

                return banners;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Add new banner
        /// </summary>
        public async Task<long> AddBanner(long ownerPKID, BannerDto banner)
        {
            try
            {
                string insertQuery = $@"
                    INSERT INTO {Table.SysCateringBanners}
                    (c_ownerid, c_title, c_description, c_link_url, c_display_order, c_isactive, c_start_date, c_end_date, c_createddate)
                    VALUES
                    (@OwnerPKID, @Title, @Description, @LinkUrl, @DisplayOrder, @IsActive, @StartDate, @EndDate, NOW())
                    RETURNING c_bannerid;";

                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@OwnerPKID", ownerPKID),
                    new NpgsqlParameter("@Title", banner.Title),
                    new NpgsqlParameter("@Description", banner.Description ?? (object)DBNull.Value),
                    new NpgsqlParameter("@LinkUrl", banner.LinkUrl ?? (object)DBNull.Value),
                    new NpgsqlParameter("@DisplayOrder", banner.DisplayOrder),
                    new NpgsqlParameter("@IsActive", banner.IsActive ? 1 : 0),
                    new NpgsqlParameter("@StartDate", banner.StartDate ?? (object)DBNull.Value),
                    new NpgsqlParameter("@EndDate", banner.EndDate ?? (object)DBNull.Value)
                };

                var result = await _dbHelper.ExecuteScalarAsync(insertQuery, parameters.ToArray());
                return result != null ? Convert.ToInt64(result) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Update existing banner
        /// </summary>
        public async Task<bool> UpdateBanner(long ownerPKID, BannerDto banner)
        {
            try
            {
                string updateQuery = $@"
                    UPDATE {Table.SysCateringBanners}
                    SET
                        c_title = @Title,
                        c_description = @Description,
                        c_link_url = @LinkUrl,
                        c_display_order = @DisplayOrder,
                        c_isactive = @IsActive,
                        c_start_date = @StartDate,
                        c_end_date = @EndDate,
                        c_modifieddate = NOW()
                    WHERE c_bannerid = @Id
                    AND c_ownerid = @OwnerPKID
                    AND c_is_deleted = FALSE";

                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@Id", banner.Id),
                    new NpgsqlParameter("@OwnerPKID", ownerPKID),
                    new NpgsqlParameter("@Title", banner.Title),
                    new NpgsqlParameter("@Description", banner.Description ?? (object)DBNull.Value),
                    new NpgsqlParameter("@LinkUrl", banner.LinkUrl ?? (object)DBNull.Value),
                    new NpgsqlParameter("@DisplayOrder", banner.DisplayOrder),
                    new NpgsqlParameter("@IsActive", banner.IsActive ? 1 : 0),
                    new NpgsqlParameter("@StartDate", banner.StartDate ?? (object)DBNull.Value),
                    new NpgsqlParameter("@EndDate", banner.EndDate ?? (object)DBNull.Value)
                };

                var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(updateQuery, parameters.ToArray());
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Soft delete banner
        /// </summary>
        public async Task<bool> DeleteBanner(long ownerPKID, long bannerId)
        {
            try
            {
                string deleteQuery = $@"
                    UPDATE {Table.SysCateringBanners}
                    SET
                        c_is_deleted = TRUE,
                        c_modifieddate = NOW()
                    WHERE c_bannerid = @Id
                    AND c_ownerid = @OwnerPKID
                    AND c_is_deleted = FALSE";

                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@Id", bannerId),
                    new NpgsqlParameter("@OwnerPKID", ownerPKID)
                };

                var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(deleteQuery, parameters.ToArray());
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Update banner status (Active/Inactive)
        /// </summary>
        public async Task<bool> UpdateBannerStatus(long ownerPKID, long bannerId, bool isActive)
        {
            try
            {
                string updateQuery = $@"
                    UPDATE {Table.SysCateringBanners}
                    SET
                        c_isactive = @IsActive,
                        c_modifieddate = NOW()
                    WHERE c_bannerid = @Id
                    AND c_ownerid = @OwnerPKID
                    AND c_is_deleted = FALSE";

                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@Id", bannerId),
                    new NpgsqlParameter("@OwnerPKID", ownerPKID),
                    new NpgsqlParameter("@IsActive", isActive.ToString())
                };

                var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(updateQuery, parameters.ToArray());
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Check if banner title already exists
        /// </summary>
        public async Task<bool> IsBannerTitleExists(long ownerPKID, string title, long? excludeId = null)
        {
            try
            {
                string query = $@"
                    SELECT COUNT(1)
                    FROM {Table.SysCateringBanners}
                    WHERE c_ownerid = @OwnerPKID
                    AND LOWER(LTRIM(RTRIM(c_title))) = LOWER(LTRIM(RTRIM(@Title)))
                    AND c_is_deleted = FALSE";

                var parameters = new List<NpgsqlParameter>
                {
                    new NpgsqlParameter("@OwnerPKID", ownerPKID),
                    new NpgsqlParameter("@Title", title)
                };

                if (excludeId.HasValue && excludeId.Value > 0)
                {
                    query += " AND c_bannerid <> @ExcludeId";
                    parameters.Add(new NpgsqlParameter("@ExcludeId", excludeId.Value));
                }

                var result = await _dbHelper.ExecuteScalarAsync(query, parameters.ToArray());
                int count = result != null ? Convert.ToInt32(result) : 0;
                return count > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Get active banners for user homepage
        /// </summary>
        public async Task<List<BannerDto>> GetActiveBannersForHomepage()
        {
            try
            {
                string selectQuery = $@"
                    SELECT
                        b.c_bannerid,
                        b.c_title,
                        b.c_description,
                        bn.c_file_path,
                        b.c_link_url,
                        b.c_display_order
                    FROM {Table.SysCateringBanners} b
                    INNER JOIN {Table.SysCateringOwner} o ON b.c_ownerid = o.c_ownerid                    
                    INNER JOIN {Table.SysCateringMediaUploads} bn ON bn.c_reference_id = b.c_bannerid
                    WHERE b.c_isactive = TRUE
                    AND b.c_is_deleted = FALSE
                    AND (b.c_start_date IS NULL OR b.c_start_date <= NOW())
                    AND (b.c_end_date IS NULL OR b.c_end_date >= NOW())
                    AND o.c_isactive = TRUE
                    ORDER BY b.c_display_order ASC, b.c_createddate DESC
                    LIMIT 10";

                var bannersData = await _dbHelper.ExecuteAsync(selectQuery);

                if (bannersData.Rows.Count == 0)
                    return new List<BannerDto>();

                var banners = new List<BannerDto>();

                foreach (System.Data.DataRow row in bannersData.Rows)
                {
                    banners.Add(new BannerDto
                    {
                        Id = row["c_bannerid"] != DBNull.Value ? Convert.ToInt64(row["c_bannerid"]) : 0,
                        Title = row["c_title"]?.ToString(),
                        Description = row["c_description"]?.ToString(),
                        ImagePath = row["c_file_path"]?.ToString(),
                        LinkUrl = row["c_link_url"]?.ToString(),
                        DisplayOrder = row["c_display_order"] != DBNull.Value ? Convert.ToInt32(row["c_display_order"]) : 0
                    });
                }

                return banners;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Increment banner view count
        /// </summary>
        public async Task IncrementViewCount(long bannerId)
        {
            try
            {
                string updateQuery = $@"
                    UPDATE {Table.SysCateringBanners}
                    SET c_view_count = c_view_count + 1
                    WHERE c_bannerid = @Id AND c_is_deleted = FALSE";

                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@Id", bannerId)
                };

                await _dbHelper.ExecuteNonQueryAsync(updateQuery, parameters.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Increment banner click count
        /// </summary>
        public async Task IncrementClickCount(long bannerId)
        {
            try
            {
                string updateQuery = $@"
                    UPDATE {Table.SysCateringBanners}
                    SET c_click_count = c_click_count + 1
                    WHERE c_bannerid = @Id AND c_is_deleted = FALSE";

                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@Id", bannerId)
                };

                await _dbHelper.ExecuteNonQueryAsync(updateQuery, parameters.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Build filter query for banners
        /// </summary>
        private string BuildFilterQuery(BannerFilter filter, List<NpgsqlParameter> parameters)
        {
            StringBuilder where = new();
            where.Append(" WHERE banner.c_ownerid = @OwnerPKID AND banner.c_is_deleted = FALSE");

            // Search by title
            if (!string.IsNullOrWhiteSpace(filter?.Title))
            {
                where.Append(" AND LOWER(c_title) LIKE LOWER('%' || @SearchTitle || '%')");
                parameters.Add(new NpgsqlParameter("@SearchTitle", filter.Title));
            }

            // Filter by IsActive
            if (filter?.IsActive != null)
            {
                where.Append(" AND c_isactive = @IsActive");
                parameters.Add(new NpgsqlParameter("@IsActive", filter.IsActive.Value ? 1 : 0));
            }

            return where.ToString();
        }
    }
}
