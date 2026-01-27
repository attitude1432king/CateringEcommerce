using CateringEcommerce.BAL.Common;
using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Text;

namespace CateringEcommerce.BAL.Base.Owner
{
    public class Decorations : IDecorations
    {
        private readonly SqlDatabaseManager _db;

        public Decorations(string connectionString)
        {
            _db = new SqlDatabaseManager();
            _db.SetConnectionString(connectionString);
        }

        /// <summary>
        /// Get the count of decorations based on filters
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="filterJson"></param>
        /// <returns></returns>
        public async Task<int> GetDecorationsCount(long ownerPKID, string filterJson)
        {
            var filter = string.IsNullOrWhiteSpace(filterJson)
                ? new DecorationFilter()
                : JsonConvert.DeserializeObject<DecorationFilter>(filterJson) ?? new DecorationFilter();

            List<SqlParameter> parameters = new()
            {
                new SqlParameter("@OwnerPKID", ownerPKID)
            };

            StringBuilder sql = new();
                 sql.Append($@"
                    SELECT COUNT(*) 
                    FROM {Table.SysCateringDecorations} cd
                ");

            sql.Append(BuildDecorationFilterQuery(filter, parameters));

            var result = await _db.ExecuteScalarAsync(sql.ToString(), parameters.ToArray());
            return result != null ? Convert.ToInt32(result) : 0;
        }

        /// <summary>
        /// Get the decorations list with filters and pagination
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="filterJson"></param>
        /// <returns></returns>
        public async Task<List<DecorationsModel>> GetDecorations(long ownerPKID, int page, int pageSize, string filterJson)
        {
            try
            {
                var filter = string.IsNullOrWhiteSpace(filterJson)
                    ? new DecorationFilter()
                    : JsonConvert.DeserializeObject<DecorationFilter>(filterJson) ?? new DecorationFilter();

                int offset = (page - 1) * pageSize;

                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerPKID", ownerPKID),
                    new SqlParameter("@Offset", offset),
                    new SqlParameter("@PageSize", pageSize)
                };

                StringBuilder sql = new();

                sql.Append($@"
                    SELECT 
                        cd.c_decoration_id AS DecorationId,
                        cd.c_decoration_name AS DecorationName,
                        cd.c_description AS Description,
                        cd.c_theme_id AS ThemeId,
                        tt.c_theme_name AS ThemeName,
                        cd.c_price AS Price,
                        cd.c_status AS Status,
                        cd.c_packageids AS PackageIds,
                        STRING_AGG(CONCAT(p.c_packageid, ':', p.c_packagename), ',') AS PackageData
                    FROM {Table.SysCateringDecorations} cd
                    LEFT JOIN {Table.SysDecorationThemes} tt 
                        ON tt.c_theme_id = cd.c_theme_id
                    OUTER APPLY (
                        SELECT value 
                        FROM STRING_SPLIT(cd.c_packageids, ',')
                    ) AS split_ids
                    LEFT JOIN {Table.SysMenuPackage} p 
                        ON p.c_packageid = TRY_CAST(split_ids.value AS INT)
                ");

                // Attach dynamic filters
                sql.Append(BuildDecorationFilterQuery(filter, parameters));

                sql.Append(@"
                    GROUP BY 
                        cd.c_decoration_id, cd.c_decoration_name, cd.c_description, cd.c_theme_id, 
                        tt.c_theme_name, cd.c_price, cd.c_status, cd.c_packageids

                    ORDER BY cd.c_decoration_id DESC
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY;
                ");

                var raw = await _db.ExecuteAsync(sql.ToString(), parameters.ToArray());
                if (raw.Rows.Count == 0)
                    return new List<DecorationsModel>();

                var decorations = new List<DecorationsModel>();
                var mediaRepo = new MediaRepository(_db.GetConnectionString());

                foreach (DataRow row in raw.Rows)
                {
                    long decorationId = Convert.ToInt64(row["DecorationId"]);

                    // 🔗 Parse linked packages
                    List<LinkedPackageDto> packageList = new();
                    if (row["PackageData"] != DBNull.Value)
                    {
                        var pairs = row["PackageData"].ToString().Split(',');
                        foreach (var pair in pairs)
                        {
                            var parts = pair.Split(':');
                            if (parts.Length == 2 && long.TryParse(parts[0], out long pkgId))
                            {
                                packageList.Add(new LinkedPackageDto
                                {
                                    Id = pkgId,
                                    Name = parts[1].Trim()
                                });
                            }
                        }
                    }

                    // 📁 Load media files
                    var mediaFiles = await mediaRepo.GetMediaFiles(ownerPKID, Domain.Enums.DocumentType.EventSetup, decorationId);

                    decorations.Add(new DecorationsModel
                    {
                        Id = decorationId,
                        Name = row["DecorationName"]?.ToString(),
                        Description = row["Description"]?.ToString(),
                        ThemeId = Convert.ToInt32(row["ThemeId"]),
                        ThemeName = row["ThemeName"]?.ToString(),
                        Price = Convert.ToDecimal(row["Price"]),
                        Status = Convert.ToBoolean(row["Status"]),
                        LinkedPackages = packageList,
                        Media = mediaFiles?.Select(m => new MediaFileModel
                        {
                            Id = m.Id,
                            FileName = m.FileName,
                            FilePath = m.FilePath,
                            DocumentType = m.DocumentType,
                            MediaType = m.MediaType
                        }).ToList()
                    });
                }

                return decorations;
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// Add new decoration record
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="decoration"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<long> AddDecoration(long ownerPKID, DecorationsDto decoration)
        {
            try
            {
                string insertQuery = $@"INSERT INTO {Table.SysCateringDecorations} 
                                       (c_ownerid, c_decoration_name, c_description, c_packageids, c_theme_id, c_price, c_status)
                                       VALUES (@OwnerPKID, @DecorationName, @Description, @PackageIDs, @ThemeID, @Price, @Status);
                                       SELECT SCOPE_IDENTITY();";

                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerPKID", ownerPKID),
                    new SqlParameter("@DecorationName", decoration.Name),
                    new SqlParameter("@Description", decoration.Description ?? (object)DBNull.Value),
                    new SqlParameter("@PackageIDs", decoration.LinkedPackageIds != null && decoration.LinkedPackageIds.Length > 0
                    ? string.Join(",", decoration.LinkedPackageIds) : (object)DBNull.Value),
                    new SqlParameter("@ThemeID", decoration.ThemeId),
                    new SqlParameter("@Price", decoration.Price > 0 ? decoration.Price : (object)DBNull.Value),
                    new SqlParameter("@Status", decoration.Status.ToBinary()) // Assuming 1 for active, 0 for inactive
                };

                var result = await _db.ExecuteScalarAsync(insertQuery.ToString(), parameters.ToArray());
                return result != null ? Convert.ToInt64(result) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Soft delete decoration record for the owner
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="decorationID"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<int> SoftDeleteDecoration(long ownerPKID, long decorationID)
        {
            try
            {
                string deleteQuery = $@"UPDATE {Table.SysCateringDecorations} SET c_is_deleted = 1, c_status = 0, c_modifieddate = GETDATE()
                                    WHERE c_ownerid = @OwnerPKID AND c_decoration_id = @DecorationID";
                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerPKID", ownerPKID),
                    new SqlParameter("@DecorationID", decorationID)
                };

                return await _db.ExecuteNonQueryAsync(deleteQuery, parameters.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Update decoration record based on owner
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="decoration"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<int> UpdateDecoration(long ownerPKID, DecorationsDto decoration)
        {
            try
            {
                string updateQuery = $@"UPDATE {Table.SysCateringDecorations}
                                    SET c_decoration_name = @DecorationName, c_description = @Description, c_packageids = @PackageIDs,
                                    c_theme_id = @ThemeID, c_price = @Price, c_status = @Status, c_modifieddate = GETDATE() 
                                    WHERE c_decoration_id = @DecorationID AND c_ownerid = @OwnerPKID";

                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerPKID", ownerPKID),
                    new SqlParameter("@DecorationID", decoration.Id),
                    new SqlParameter("@DecorationName", decoration.Name),
                    new SqlParameter("@Description", decoration.Description ?? (object)DBNull.Value),
                    new SqlParameter("@PackageIDs", decoration.LinkedPackageIds != null && decoration.LinkedPackageIds.Length > 0
                    ? string.Join(",", decoration.LinkedPackageIds) : (object)DBNull.Value),
                    new SqlParameter("@ThemeID", decoration.ThemeId),
                    new SqlParameter("@Price", decoration.Price),
                    new SqlParameter("@Status", decoration.Status.ToBinary()), // Assuming 1 for active, 0 for inactive
                };

                return await _db.ExecuteNonQueryAsync(updateQuery.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Get the list of decoration themes
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<DecorationThemeModel>> GetDecorationThemes()
        {
            try
            {
                string query = $@"SELECT c_theme_id AS ThemeId, c_theme_name AS ThemeName
                                 FROM {Table.SysDecorationThemes}
                                 WHERE c_isactive = 1 ORDER BY c_theme_name";
                var dt = await _db.ExecuteAsync(query);
                List<DecorationThemeModel> themes = new List<DecorationThemeModel>();
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    themes.Add(new DecorationThemeModel
                    {
                        ThemeId = row["ThemeId"] == DBNull.Value ? 0 : Convert.ToInt32(row["ThemeId"]),
                        ThemeName = row["ThemeName"] == DBNull.Value ? string.Empty : row["ThemeName"].ToString()
                    });
                }
                return themes;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Is decoration name already exists for the owner
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="decorationName"></param>
        /// <param name="decorationId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> IsDecorationNameExistsAsync(long ownerPKID, string decorationName, long? decorationId = null)
        {
            try
            {
                // Base query for checking duplicates
                string query = $@"
                            SELECT COUNT(1)
                            FROM {Table.SysCateringDecorations}
                            WHERE c_ownerid = @OwnerPKID AND c_is_deleted = 0
                              AND LOWER(LTRIM(RTRIM(c_decoration_name))) = LOWER(LTRIM(RTRIM(@DecorationName)))";

                // Exclude the current record in case of update
                if (decorationId.HasValue && decorationId.Value > 0)
                {
                    query += " AND c_decoration_id <> @DecorationId";
                }

                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@OwnerPKID", ownerPKID),
                    new SqlParameter("@DecorationName", decorationName)
                };

                if (decorationId.HasValue && decorationId.Value > 0)
                    parameters.Add(new SqlParameter("@DecorationId", decorationId.Value));

                var result = await _db.ExecuteScalarAsync(query, parameters.ToArray());
                int count = result != null ? Convert.ToInt32(result) : 0;
                return count > 0; // true → name already exists
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking decoration name: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Is valid decoration ID for the owner
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="decorationId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool IsValidDecorationID(long ownerPKID, long decorationId)
        {
            try
            {
                string selectQuery = $@"
                        SELECT COUNT(1)
                        FROM {Table.SysCateringDecorations}
                        WHERE c_ownerid = @OwnerPKID
                            AND c_decoration_id = @DecorationId AND c_is_deleted = 0";

                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerPKID", ownerPKID),
                    new SqlParameter("@DecorationId", decorationId)
                };

                var result = _db.ExecuteScalar(selectQuery, parameters.ToArray());
                int count = result != null ? Convert.ToInt32(result) : 0;

                // If count == 0 → ID does not belong to this owner
                return count > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error verifying Decoration ID: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Update the decoration status (active/inactive)
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="decorationId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task UpdateDecorationStatus(long ownerPKID, long decorationId, bool status)
        {
            try
            {
                string updateQuery = $@"UPDATE {Table.SysCateringDecorations} SET c_status = @Status 
                                   WHERE c_ownerid = @OwnerPKID
                                    AND c_decoration_id = @DecorationId";

                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerPKID", ownerPKID),
                    new SqlParameter("@DecorationId", decorationId),
                    new SqlParameter("@Status", status.ToBinary())
                };

                var result = await _db.ExecuteNonQueryAsync(updateQuery, parameters.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Build Decoration filter query
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string BuildDecorationFilterQuery(DecorationFilter filter, List<SqlParameter> parameters)
        {
            StringBuilder where = new();
            where.Append(" WHERE cd.c_ownerid = @OwnerPKID AND cd.c_is_deleted = 0 ");

            // Name search
            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                where.Append(" AND LOWER(cd.c_decoration_name) LIKE LOWER('%' + @Name + '%') ");
                parameters.Add(new SqlParameter("@Name", filter.Name));
            }

            // Theme filter
            if (filter.ThemeIds?.Count > 0)
            {
                where.Append($" AND cd.c_theme_id IN ({string.Join(",", filter.ThemeIds)}) ");
            }

            // Status filter
            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                where.Append(" AND cd.c_status = @Status ");
                parameters.Add(new SqlParameter("@Status", filter.Status));
            }

            // Linked Package filter
            if (filter.PackageIds?.Count > 0)
            {
                where.Append(@"
                    AND EXISTS (
                        SELECT 1 
                        FROM STRING_SPLIT(cd.c_packageids, ',') AS sp
                        WHERE TRY_CAST(sp.value AS INT) IN (" + string.Join(",", filter.PackageIds) + @")
                    )
                ");
            }

            return where.ToString();
        }

    }
}
