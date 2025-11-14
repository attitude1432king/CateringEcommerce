using CateringEcommerce.BAL.Common;
using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.Data.SqlClient;

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


        public async Task<List<DecorationsModel>> GetDecorations(long ownerPKID)
        {
            try
            {
                string selectQuery = $@"
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
                                    LEFT JOIN {Table.SysDecorationThemes} tt ON tt.c_theme_id = cd.c_theme_id
                                    CROSS APPLY STRING_SPLIT(cd.c_packageids, ',') AS split_ids
                                    LEFT JOIN {Table.SysMenuPackage} p ON p.c_packageid = TRY_CAST(split_ids.value AS INT)
                                    WHERE cd.c_ownerid = @OwnerPKID
                                    GROUP BY 
                                        cd.c_decoration_id, cd.c_decoration_name, cd.c_description, cd.c_theme_id, 
                                        tt.c_theme_name, cd.c_price, cd.c_status, cd.c_packageids
                                    ORDER BY cd.c_decoration_id DESC;";


                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerPKID", ownerPKID)
                };

                var decorationsData = await _db.ExecuteAsync(selectQuery, parameters.ToArray());
                if (decorationsData.Rows.Count == 0)
                    return new List<DecorationsModel>();

                var decorations = new List<DecorationsModel>();
                var mediaRepository = new MediaRepository(_db.GetConnectionString());

                foreach (System.Data.DataRow row in decorationsData.Rows)
                {
                    var decorationId = row["DecorationId"] != DBNull.Value ? Convert.ToInt64(row["DecorationId"]) : 0;

                    // Build Linked Packages (id + name)
                    var packageList = new List<LinkedPackageDto>();
                    if (row["PackageData"] != DBNull.Value && !string.IsNullOrWhiteSpace(row["PackageData"].ToString()))
                    {
                        var packagePairs = row["PackageData"].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var pair in packagePairs)
                        {
                            var parts = pair.Split(':');
                            if (parts.Length == 2 && long.TryParse(parts[0], out var pkgId))
                            {
                                packageList.Add(new LinkedPackageDto
                                {
                                    Id = pkgId,
                                    Name = parts[1].Trim()
                                });
                            }
                        }
                    }

                    // Fetch media files for this decoration
                    var decorationMediaFiles = await mediaRepository.GetMediaFiles(ownerPKID, Domain.Enums.DocumentType.EventSetup, decorationId);

                    decorations.Add(new DecorationsModel
                    {
                        Id = decorationId,
                        Name = row["DecorationName"]?.ToString(),
                        Description = row["Description"]?.ToString(),
                        ThemeName = row["ThemeName"]?.ToString(), // new property
                        Price = row["Price"] != DBNull.Value ? Convert.ToDecimal(row["Price"]) : 0,
                        ThemeId = row["ThemeId"] != DBNull.Value ? Convert.ToInt16(row["ThemeId"]) : 0,
                        Status = row["Status"] != DBNull.Value && Convert.ToBoolean(row["Status"]),
                        LinkedPackages = packageList,  // updated property
                        Media = decorationMediaFiles?.Select(m => new MediaFileModel
                        {
                            Id = m.Id,
                            FileName = m.FileName,
                            FilePath = m.FilePath,
                            MediaType = m.MediaType,
                            DocumentType = m.DocumentType
                        }).ToList()
                    });
                }

                return decorations;
            }
            catch (Exception)
            {
                throw;
            }
        }

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

        public async Task<int> DeleteDecoration(long ownerPKID, long decorationID)
        {
            try
            {
                string deleteQuery = $@"DELETE FROM {Table.SysCateringDecorations} WHERE c_ownerid = @OwnerPKID AND c_decoration_id = @DecorationID";
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


        public async Task<int> UpdateDecoration(long ownerPKID, DecorationsDto decoration)
        {
            try
            {
                string updateQuery = $@"UPDATE {Table.SysCateringDecorations}
                                    SET c_decoration_name = @DecorationName, c_description = @Description, c_packageids = @PackageIDs,
                                    c_theme_id = @ThemeID, c_price = @Price, c_status = @Status 
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
                    new SqlParameter("@Status", decoration.Status.ToBinary()) // Assuming 1 for active, 0 for inactive
                };

                return await _db.ExecuteNonQueryAsync(updateQuery.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<DecorationThemeModel>> GetDecorationThemes()
        {
            try
            {
                string query = $@"SELECT c_theme_id AS ThemeId, c_theme_name AS ThemeName
                                 FROM {Table.SysDecorationThemes}
                                 WHERE c_isactive = 1";
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

        public async Task<bool> IsDecorationNameExistsAsync(long ownerPKID, string decorationName, long? decorationId = null)
        {
            try
            {
                // Base query for checking duplicates
                string query = $@"
                            SELECT COUNT(1)
                            FROM {Table.SysCateringDecorations}
                            WHERE c_ownerid = @OwnerPKID
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

        public bool IsValidDecorationID(long ownerPKID, long decorationId)
        {
            try
            {
                string selectQuery = $@"
                        SELECT COUNT(1)
                        FROM {Table.SysCateringDecorations}
                        WHERE c_ownerid = @OwnerPKID
                            AND c_decoration_id = @DecorationId";

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


    }
}
