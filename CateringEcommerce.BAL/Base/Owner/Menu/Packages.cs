using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace CateringEcommerce.BAL.Base.Owner.Menu
{
    public class Packages : IPackages
    {
        private readonly SqlDatabaseManager _db;

        public Packages(string connectionString)
        {
            _db = new SqlDatabaseManager();
            _db.SetConnectionString(connectionString);
        }

        /// <summary>
        /// Get Food Categories
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<FoodCategoryDto>> GetCategories()
        {
            try
            {
                string query = "SELECT c_categoryid, c_categoryname FROM " + Table.SysFoodCategory + " WHERE c_is_active = 1";
                var dtCategory = await _db.ExecuteAsync(query);
                var foodCategoryList = new List<FoodCategoryDto>();
                if (dtCategory.Rows.Count > 0)
                {
                    foreach (System.Data.DataRow row in dtCategory.Rows)
                    {
                        foodCategoryList.Add(new FoodCategoryDto
                        {
                            CategoryId = Convert.ToInt16(row["c_categoryid"]),
                            Name = row["c_categoryname"]?.ToString(),
                        });
                    }
                }
                else
                {
                    return null;
                }
                return foodCategoryList;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Add new package
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="packageDto"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<long> AddPackage(long ownerPKID, PackageDto packageDto)
        {
            try
            {
                string insertQuery = $@"INSERT INTO {Table.SysMenuPackage} (c_packagename, c_description, c_price, c_ownerid) VALUES 
                                (@PackageName, @Description, @Price, @OwnerPKID); SELECT CAST(SCOPE_IDENTITY() AS int); ";
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@PackageName", packageDto.Name),
                    new SqlParameter("@Description", packageDto.Description),
                    new SqlParameter("@Price", packageDto.Price),
                    new SqlParameter("@OwnerPKID", ownerPKID)
                };
                var result = await _db.ExecuteScalarAsync(insertQuery.ToString(), parameters);
                return result != null ? Convert.ToInt64(result) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Get count for packages with search filter
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="searchPackageName"></param>
        /// <returns></returns>
        public async Task<Int32> GetPackageCount(long ownerPKID, string searchPackageName = "")
        {
            try
            {
                StringBuilder selectQuery = new StringBuilder();
                selectQuery.Append($@"SELECT COUNT(*) FROM {Table.SysMenuPackage}
                            WHERE c_ownerid = @OwnerPKID AND c_is_deleted = 0");

                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerPKID", ownerPKID)
                };

                // 🔍 Apply Search Filter (case-insensitive)
                if (!string.IsNullOrWhiteSpace(searchPackageName))
                {
                    selectQuery.Append(" AND LOWER(c_packagename) LIKE LOWER(@Search) ");
                    parameters.Add(new SqlParameter("@Search", $"%{searchPackageName.ToLower()}%"));
                }

                var resultObj = await _db.ExecuteScalarAsync(selectQuery.ToString(), parameters.ToArray());
                Int32 result = Convert.ToInt32(resultObj);
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Get Packages with pagination and search
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="searchPackageName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<PackageDto>> GetPackages(long ownerPKID, int page, int pageSize, string searchPackageName = "")
        {
            try
            {
                int offset = (page - 1) * pageSize;

                // -----------------------------
                //  STEP 1: PAGINATE PACKAGE IDs
                // -----------------------------
                StringBuilder sql1 = new StringBuilder();
                sql1.Append($@"
                    SELECT DISTINCT p.c_packageid
                    FROM {Table.SysMenuPackage} p
                    WHERE p.c_ownerid = @OwnerPKID AND c_is_deleted = 0
                ");

                if (!string.IsNullOrWhiteSpace(searchPackageName))
                {
                    sql1.Append(" AND LOWER(p.c_packagename) LIKE LOWER(@Search) ");
                }

                sql1.Append(@"
                    ORDER BY p.c_packageid
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY;
                ");

                List<SqlParameter> p1 = new()
                {
                    new SqlParameter("@OwnerPKID", ownerPKID),
                    new SqlParameter("@Offset", offset),
                    new SqlParameter("@PageSize", pageSize)
                };

                if (!string.IsNullOrWhiteSpace(searchPackageName))
                    p1.Add(new SqlParameter("@Search", $"%{searchPackageName.ToLower()}%"));

                var idTable = await _db.ExecuteAsync(sql1.ToString(), p1.ToArray());

                if (idTable.Rows.Count == 0)
                    return new List<PackageDto>();

                // Extract package IDs
                var packageIds = idTable.Rows
                    .Cast<DataRow>()
                    .Select(r => Convert.ToInt64(r["c_packageid"]))
                    .ToList();

                // Convert to comma-separated ID list
                string idList = string.Join(",", packageIds);

                // -------------------------------
                //  STEP 2: LOAD ITEMS FOR PACKAGES
                // -------------------------------
                string sql2 = $@"
                    SELECT p.c_packageid, p.c_packagename, p.c_description, p.c_price,
                           pi.c_itemid, pi.c_categoryid, pi.c_quantity,
                           fc.c_categoryname
                    FROM {Table.SysMenuPackage} p
                    LEFT JOIN {Table.SysMenuPackageItems} pi 
                        ON p.c_packageid = pi.c_packageid
                    LEFT JOIN {Table.SysFoodCategory} fc 
                        ON pi.c_categoryid = fc.c_categoryid
                    WHERE p.c_packageid IN ({idList})
                    ORDER BY p.c_packageid, pi.c_itemid;";

                var packageData = await _db.ExecuteAsync(sql2);

                // -----------------------
                //  STEP 3: BUILD DTO LIST
                // -----------------------
                var packageDict = new Dictionary<long, PackageDto>();

                foreach (DataRow row in packageData.Rows)
                {
                    long packageId = Convert.ToInt64(row["c_packageid"]);

                    if (!packageDict.ContainsKey(packageId))
                    {
                        packageDict[packageId] = new PackageDto
                        {
                            PackageId = packageId,
                            Name = row["c_packagename"]?.ToString(),
                            Description = row["c_description"]?.ToString(),
                            Price = Convert.ToDecimal(row["c_price"]),
                            Items = new List<PackageItemDto>()
                        };
                    }

                    if (row["c_itemid"] != DBNull.Value)
                    {
                        packageDict[packageId].Items.Add(new PackageItemDto
                        {
                            PackageItemId = Convert.ToInt64(row["c_itemid"]),
                            CategoryId = Convert.ToInt16(row["c_categoryid"]),
                            Quantity = Convert.ToInt16(row["c_quantity"]),
                            CategoryName = row["c_categoryname"]?.ToString()
                        });
                    }
                }

                return packageDict.Values.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching packages: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Update the package details
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="packageDto"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task UpdatePackage(long? ownerPKID, PackageDto packageDto)
        {
            try
            {
                StringBuilder updateQuery = new StringBuilder();
                updateQuery.Append($@"UPDATE {Table.SysMenuPackage} SET c_packagename = @PackageName, c_description = @Description,
                                   c_price = @Price, c_modified_date = @ModifiedDate WHERE c_packageid = @PackagePKID AND c_ownerid = @OwnerPKID");

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@PackageName", packageDto.Name),
                    new SqlParameter("@Description", packageDto.Description),
                    new SqlParameter("@Price", packageDto.Price),
                    new SqlParameter("@ModifiedDate", DateTime.Now),
                    new SqlParameter("@PackagePKID", packageDto.PackageId),
                    new SqlParameter("@OwnerPKID", ownerPKID)
                };
                var result = await _db.ExecuteNonQueryAsync(updateQuery.ToString(), parameters);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Add Package Items 
        /// </summary>
        /// <param name="packagePKID"></param>
        /// <param name="packageItem"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task AddPackageItems(long? packagePKID, PackageItemDto packageItem)
        {
            try
            {
                string insertQuery = $@"INSERT INTO {Table.SysMenuPackageItems} (c_packageid, c_categoryid, c_quantity) VALUES 
                                (@PackagePKID, @CategoryId, @Quantity)";
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@PackagePKID", packagePKID),
                    new SqlParameter("@CategoryId", packageItem.CategoryId),
                    new SqlParameter("@Quantity", packageItem.Quantity),
                };
                var result = await _db.ExecuteNonQueryAsync(insertQuery.ToString(), parameters);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Update Package Items
        /// </summary>
        /// <param name="packagePKID"></param>
        /// <param name="packageItem"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task UpdatePackageItems(long? packagePKID, PackageItemDto packageItem)
        {
            try
            {
                if (packageItem.PackageItemId <= 0)
                {
                    throw new Exception("Package Item must be required.");
                }
                string insertQuery = $@"UPDATE {Table.SysMenuPackageItems} SET c_categoryid = @CategoryId, c_quantity = @Quantity, c_modified_date = @ModifiedDate  
                                 WHERE c_itemid = @ItemId AND c_packageid = @PackagePKID";
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@PackagePKID", packagePKID),
                    new SqlParameter("@ItemId", packageItem.PackageItemId),
                    new SqlParameter("@CategoryId", packageItem.CategoryId),
                    new SqlParameter("@Quantity", packageItem.Quantity),
                    new SqlParameter("@ModifiedDate", DateTime.Now),
                };
                var result = await _db.ExecuteNonQueryAsync(insertQuery.ToString(), parameters);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Delete the Package items
        /// </summary>
        /// <param name="packagePKID"></param>
        /// <param name="packageItemId"></param>
        /// <returns></returns>
        public async Task DeletePackageItems(long? packagePKID, long packageItemId = 0)
        {
            try
            {
                StringBuilder deleteQuery = new StringBuilder();
                deleteQuery.Append($@"DELETE FROM {Table.SysMenuPackageItems} WHERE c_packageid = @packagePKID");
                List<SqlParameter> parameters = new List<SqlParameter>
                {
                   new SqlParameter("@PackagePKID", packagePKID)
                };

                if(packageItemId > 0)
                {
                    deleteQuery.Append(" AND c_itemid = @ItemId");
                    parameters.Add(new SqlParameter("@ItemId", packageItemId));
                }
                var result = await _db.ExecuteNonQueryAsync(deleteQuery.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Soft delete package items
        /// </summary>
        /// <param name="packagePKID"></param>
        /// <returns></returns>
        public async Task SoftDeletePackage(long? packagePKID)
        {
            try
            {
                string query = $@"UPDATE {Table.SysMenuPackage} SET c_is_deleted = 1, c_is_active = 0, c_modified_date = GETDATE() WHERE c_packageid = @packagePKID"; 
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@PackagePKID", packagePKID),
                };
                var result = await _db.ExecuteNonQueryAsync(query.ToString(), parameters);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Package exist or not
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public bool PackageExistOrNot(Int64 ownerPKID, string packageName)
        {
            try
            {
                string selectQuery = $@"SELECT COUNT(c_packagename) FROM {Table.SysMenuPackage} WHERE c_packagename = @PackageName
                                    AND c_ownerid = @OwnerPKID AND c_is_deleted = 0";
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@PackageName", packageName),
                    new SqlParameter("@OwnerPKID", ownerPKID),
                };

                return Convert.ToInt16(_db.ExecuteScalar(selectQuery, parameters)) > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Get Package Items by Package PKID
        /// </summary>
        /// <param name="packagePKID"></param>
        /// <returns></returns>
        public async Task<List<PackageItemDto>> GetPackageItems(long packagePKID)
        {
            try
            {
                string selectQuery = $@"SELECT c_itemid, c_categoryid, c_quantity FROM {Table.SysMenuPackageItems} WHERE c_packageid = @PackagePKID";
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@PackagePKID", packagePKID),
                };

                var packageItemList = await _db.ExecuteAsync(selectQuery, parameters);
                if (packageItemList.Rows.Count > 0)
                {
                    List<PackageItemDto> packageItems = new List<PackageItemDto>();
                    foreach (System.Data.DataRow row in packageItemList.Rows)
                    {
                        var item = new PackageItemDto
                        {
                            PackageItemId = Convert.ToInt64(row["c_itemid"]),
                            CategoryId = Convert.ToInt16(row["c_categoryid"]),
                            Quantity = Convert.ToInt16(row["c_quantity"]),
                        };
                        packageItems.Add(item);
                    }
                    return packageItems;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Get Packages Lookup for dropdown
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<PackageDto>> GetPackagesLookup(long ownerPKID)
        {
            try
            {
                string selectQuery = $@"SELECT c_packageid, c_packagename FROM {Table.SysMenuPackage} 
                                    WHERE c_ownerid = @OwnerPKID AND c_is_active = 1";
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@OwnerPKID", ownerPKID),
                };
                var packageData = await _db.ExecuteAsync(selectQuery, parameters);
                if (packageData.Rows.Count > 0)
                {
                    List<PackageDto> packageList = new List<PackageDto>();
                    foreach (System.Data.DataRow row in packageData.Rows)
                    {
                        var package = new PackageDto
                        {
                            PackageId = Convert.ToInt64(row["c_packageid"]),
                            Name = row["c_packagename"]?.ToString(),
                        };
                        packageList.Add(package);
                    }
                    return packageList;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> IsValidPackageID(long ownerPKID, long packagePKID)
        {
            try
            {
                string selectQuery = $@"SELECT COUNT(c_packageid) FROM {Table.SysMenuPackage} 
                                    WHERE c_ownerid = @OwnerPKID AND c_packageid = @PackagePKID AND c_is_deleted = 0";
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@OwnerPKID", ownerPKID),
                    new SqlParameter("@PackagePKID", packagePKID),
                };
                return Convert.ToInt16(await _db.ExecuteScalarAsync(selectQuery, parameters)) > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void UpdatePackageStatus(long? packagePKID, bool isActive)
        {
            throw new NotImplementedException();
        }
        
    }
}
