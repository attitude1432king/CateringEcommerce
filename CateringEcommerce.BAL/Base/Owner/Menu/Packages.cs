using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces.Owner;
using Microsoft.Data.SqlClient;
using System.Text;
using CateringEcommerce.Domain.Models.Owner;

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

        public async Task<List<PackageDto>> GetPackages(long ownerPKID)
        {
            try
            {
                List<PackageDto> packageList = new List<PackageDto>();  
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append($@"SELECT p.c_packageid, p.c_packagename, p.c_description, p.c_price,
                                pi.c_itemid, pi.c_categoryid, pi.c_quantity,
                                fc.c_categoryname
                                FROM {Table.SysMenuPackage} p
                                LEFT JOIN {Table.SysMenuPackageItems} pi ON p.c_packageid = pi.c_packageid
                                LEFT JOIN {Table.SysFoodCategory} fc ON pi.c_categoryid = fc.c_categoryid
                                WHERE p.c_is_active = 1 AND p.c_ownerid = @OwnerPKID");

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@OwnerPKID", ownerPKID)
                };

                var packageData = await _db.ExecuteAsync(stringBuilder.ToString(), parameters);
                if (packageData.Rows.Count > 0)
                {
                    var packageDict = new Dictionary<long, PackageDto>();
                    foreach (System.Data.DataRow row in packageData.Rows)
                    {
                        long packageId = Convert.ToInt64(row["c_packageid"]);
                        if (!packageDict.ContainsKey(packageId))
                        {
                            var package = new PackageDto
                            {
                                PackageId = packageId,
                                Name = row["c_packagename"]?.ToString(),
                                Description = row["c_description"]?.ToString(),
                                Price = Convert.ToDecimal(row["c_price"]),
                                Items = new List<PackageItemDto>()
                            };
                            packageDict[packageId] = package;
                        }
                        if (row["c_itemid"] != DBNull.Value)
                        {
                            var item = new PackageItemDto
                            {
                                PackageItemId = Convert.ToInt64(row["c_itemid"]),
                                CategoryId = Convert.ToInt16(row["c_categoryid"]),
                                Quantity = Convert.ToInt16(row["c_quantity"]),
                                CategoryName = row["c_categoryname"]?.ToString()
                            };
                            packageDict[packageId].Items.Add(item);
                        }
                    }
                    return packageList = packageDict.Values.ToList();
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

        public async Task UpdatePackageItems(long? packagePKID, PackageItemDto packageItem)
        {
            try
            {
                if (packageItem.PackageItemId <= 0)
                {
                    throw new Exception("Package Item must be required.");
                }
                string insertQuery = $@"UPDATE {Table.SysMenuPackageItems} SET c_categoryid = @CategoryId, c_quantity = @Quantity 
                                 WHERE c_itemid = @ItemId AND c_packageid = @PackagePKID";
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@PackagePKID", packagePKID),
                    new SqlParameter("@ItemId", packageItem.PackageItemId),
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

        public async Task DeletePackage(long? packagePKID)
        {
            try
            {
                string deleteQuery = $@"DELETE FROM {Table.SysMenuPackage} WHERE c_packageid = @packagePKID";
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@PackagePKID", packagePKID),
                };
                var result = await _db.ExecuteNonQueryAsync(deleteQuery.ToString(), parameters);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        
        public bool PackageExistOrNot(Int64 ownerPKID, string packageName)
        {
            try
            {
                string selectQuery = $@"SELECT COUNT(c_packagename) FROM {Table.SysMenuPackage} WHERE c_packagename = @PackageName
                                    AND c_ownerid = @OwnerPKID";
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

        public void UpdatePackageStatus(long? packagePKID, bool isActive)
        {
            throw new NotImplementedException();
        }
        
    }
}
