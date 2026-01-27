using CateringEcommerce.BAL.Common;
using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Text;

namespace CateringEcommerce.BAL.Base.Owner.Menu
{
    public class FoodItems: IFoodItems
    {
        private readonly SqlDatabaseManager _db;

        public FoodItems(string connectionString)
        {
            _db = new SqlDatabaseManager();
            _db.SetConnectionString(connectionString);
        }

        /// <summary>
        /// Get total count of food items based on filter
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Int32> GetFoodItemsCount(long ownerPKID, FoodItemFilter filter)
        {
            try
            {
                StringBuilder countQuery = new StringBuilder();
                countQuery.Append($@"SELECT COUNT(*) FROM {Table.SysFoodItems} ft");
                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerPKID", ownerPKID)
                };
                countQuery.Append(BuildFilterQuery(filter, parameters));
                var result = await _db.ExecuteScalarAsync(countQuery.ToString(), parameters.ToArray());
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Add new food item
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="foodItem"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<long> AddFoodItem(long ownerPKID, FoodItemDto foodItem)
        {
            try
            {
                string insertQuery = $@"INSERT INTO {Table.SysFoodItems}
                                       (c_ownerid, c_foodname, c_description, c_categoryid, c_cuisinetypeid, c_price, c_isveg, c_islive_counter, c_ispackage_item, c_issample_tasted, c_status)
                                       VALUES (@OwnerPKID, @FoodName, @Description, @CategoryID, @CuisineID, @Price, @IsVeg, @IsLiveCounter, @IsPackageItem, @IsSampleTaste,  @Status);
                                       SELECT SCOPE_IDENTITY();";

                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerPKID", ownerPKID),
                    new SqlParameter("@FoodName", foodItem.Name),
                    new SqlParameter("@Description", foodItem.Description ?? (object)DBNull.Value),
                    new SqlParameter("@CategoryID", foodItem.CategoryId),
                    new SqlParameter("@CuisineID", foodItem.TypeId ?? (object)DBNull.Value),
                    new SqlParameter("@Price", foodItem.Price),
                    new SqlParameter("@IsVeg", foodItem.IsVeg.ToBinary()),
                    new SqlParameter("@IsLiveCounter", foodItem.IsLiveCounter.ToBinary()),
                    new SqlParameter("@IsPackageItem", foodItem.IsPackageItem.ToBinary()),
                    new SqlParameter("@IsSampleTaste", foodItem.IsSampleTaste.ToBinary()),
                    new SqlParameter("@Status", foodItem.Status.ToBinary()) // Assuming 1 for active, 0 for inactive
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
        /// Get list of food items based on filter with pagination
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<FoodItemModel>> GetFoodItems(long ownerPKID, int page, int pageSize, FoodItemFilter filter)
        {
            try
            {
                int offset = (page - 1) * pageSize;
                StringBuilder selectQuery = new StringBuilder();
                selectQuery.Append($@"SELECT ft.c_foodid AS FoodId, ft.c_foodname AS FoodName, ft.c_description AS Description, ft.c_categoryid AS CategoryID, ft.c_cuisinetypeid AS CuisineTypeID, ft.c_price AS Price, 
                                    ft.c_ispackage_item AS IsPackageItem, ft.c_isveg AS IsVeg, ft.c_islive_counter As IsLiveCounter,
                                    ft.c_issample_tasted AS IsSampleTasted, ft.c_status AS Status, fc.c_categoryname AS CategoryName, tm.c_type_name AS CuisineTypeName
                                    FROM {Table.SysFoodItems} ft 
                                    LEFT JOIN {Table.SysFoodCategory} fc ON ft.c_categoryid = fc.c_categoryid 
                                    LEFT JOIN {Table.SysCateringTypeMaster} tm ON ft.c_cuisinetypeid = tm.c_type_id");
                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerPKID", ownerPKID),
                    new SqlParameter("@Offset", offset),
                    new SqlParameter("@PageSize", pageSize),
                };

                selectQuery.Append(BuildFilterQuery(filter, parameters));

                selectQuery.Append(@"
                    ORDER BY ft.c_foodid DESC
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY;
                ");

                var foodItemsData = await _db.ExecuteAsync(selectQuery.ToString(), parameters.ToArray());
                if (foodItemsData.Rows.Count == 0)
                    return new List<FoodItemModel>();
                var foodItems = new List<FoodItemModel>();
                MediaRepository mediaRepository = new MediaRepository(_db.GetConnectionString());
                foreach (System.Data.DataRow row in foodItemsData.Rows)
                {
                    var FoodItemId = row["FoodId"] != DBNull.Value ? Convert.ToInt64(row["FoodId"]) : 0;
                    foodItems.Add(new FoodItemModel
                    {
                        Id = FoodItemId,
                        Name = row["FoodName"]?.ToString(),
                        Description = row["Description"]?.ToString(),
                        CategoryId = row["CategoryID"] != DBNull.Value ? Convert.ToInt32(row["CategoryID"]) : 0,
                        TypeId = row["CuisineTypeID"] != DBNull.Value ? Convert.ToInt32(row["CuisineTypeID"]) : null,
                        Price = row["Price"] != DBNull.Value ? Convert.ToDecimal(row["Price"]) : 0,
                        IsPackageItem = row["IsPackageItem"] != DBNull.Value && Convert.ToBoolean(row["IsPackageItem"]),
                        IsVeg = row["IsVeg"] != DBNull.Value && Convert.ToBoolean(row["IsVeg"]),
                        IsLiveCounter = row["IsLiveCounter"] != DBNull.Value && Convert.ToBoolean(row["IsLiveCounter"]),
                        IsSampleTaste = row["IsSampleTasted"] != DBNull.Value && Convert.ToBoolean(row["IsSampleTasted"]),
                        Status = row["Status"] != DBNull.Value && Convert.ToBoolean(row["Status"]),
                        CategoryName = row["CategoryName"]?.ToString(),
                        TypeName = row["CuisineTypeName"]?.ToString()
                    });
                    var foodMediaFiles =  await mediaRepository.GetMediaFiles(ownerPKID, Domain.Enums.DocumentType.Food, FoodItemId);
                    foodItems.Last().Media = foodMediaFiles?.Select(m => new MediaFileModel
                    {
                        Id = m.Id,
                        FileName = m.FileName,
                        FilePath = m.FilePath,
                        MediaType = m.MediaType,
                        DocumentType = m.DocumentType
                    }).ToList();
                }

                return foodItems;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Update existing food item
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="foodItem"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<int> UpdateFoodItem(long ownerPKID, FoodItemDto foodItem)
        {
            try
            {
                string updateQuery = $@"UPDATE {Table.SysFoodItems}
                       SET c_foodname = @FoodName, c_description = @Description, c_categoryid = @CategoryID, 
                        c_cuisinetypeid = @CuisineID, c_price = @Price, c_ispackage_item = @IsPackageItem, c_isveg = @IsVeg,
                        c_issample_tasted = @IsSampleTaste, c_islive_counter = @IsLiveCounter, c_status = @Status, c_modifieddate = GETDATE()
                        WHERE c_foodid = @FoodID AND c_ownerid = @OwnerPKID";

                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerPKID", ownerPKID),
                    new SqlParameter("@FoodName", foodItem.Name),
                    new SqlParameter("@Description", foodItem.Description ?? (object)DBNull.Value),
                    new SqlParameter("@CategoryID", foodItem.CategoryId),
                    new SqlParameter("@CuisineID", foodItem.TypeId ?? (object)DBNull.Value),
                    new SqlParameter("@Price", foodItem.Price),
                    new SqlParameter("@IsVeg", foodItem.IsVeg.ToBinary()),
                    new SqlParameter("@IsLiveCounter", foodItem.IsLiveCounter.ToBinary()),
                    new SqlParameter("@IsPackageItem", foodItem.IsPackageItem.ToBinary()),
                    new SqlParameter("@FoodID", foodItem.Id),
                    new SqlParameter("@Status", foodItem.Status.ToBinary()), // Assuming 1 for active, 0 for inactive
                    new SqlParameter("@IsSampleTaste", foodItem.IsSampleTaste.ToBinary()),
                };

                return await _db.ExecuteNonQueryAsync(updateQuery.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Soft delete food item
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="foodItemPKID"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<int> SoftDeleteFoodItem(long ownerPKID, long foodItemPKID)
        {
            try
            {
                string deleteQuery = $@"UPDATE {Table.SysFoodItems} SET c_is_deleted = 1, c_status = 0, c_modifieddate = GETDATE()
                                     WHERE c_ownerid = @OwnerPKID AND c_foodid = @FoodID";
                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerPKID", ownerPKID),
                    new SqlParameter("@FoodID", foodItemPKID)
                };

                return await _db.ExecuteNonQueryAsync(deleteQuery, parameters.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Check if food item name already exists for the owner
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="foodItemName"></param>
        /// <param name="foodItemPKID"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> IsFoodItemNameExists(long ownerPKID, string foodItemName, long? foodItemPKID = null)
        {
            try
            {
                // Base query for checking duplicates
                string query = $@"
                            SELECT COUNT(1)
                            FROM {Table.SysFoodItems}
                            WHERE c_ownerid = @OwnerPKID AND c_is_deleted = 0
                              AND LOWER(LTRIM(RTRIM(c_foodname))) = LOWER(LTRIM(RTRIM(@FoodItemName)))";

                // Exclude the current record in case of update
                if (foodItemPKID.HasValue && foodItemPKID.Value > 0)
                {
                    query += " AND c_foodid <> @FoodID";
                }

                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@OwnerPKID", ownerPKID),
                    new SqlParameter("@FoodItemName", foodItemName)
                };

                if (foodItemPKID.HasValue && foodItemPKID.Value > 0)
                    parameters.Add(new SqlParameter("@FoodID", foodItemPKID.Value));

                var result = await _db.ExecuteScalarAsync(query, parameters.ToArray());
                int count = result != null ? Convert.ToInt32(result) : 0;
                return count > 0; 
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Build filter query for food items
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string BuildFilterQuery(FoodItemFilter filter, List<SqlParameter> parameters)
        {
            StringBuilder where = new();
            where.Append("  WHERE c_ownerid = @OwnerPKID AND c_is_deleted = 0");
            // Search by name
            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                where.Append(" AND LOWER(ft.c_foodname) LIKE LOWER('%' + @SearchName + '%') ");
                parameters.Add(new SqlParameter("@SearchName", filter.Name));
            }

            // Category multi-select
            if (filter.CategoryIds != null && filter.CategoryIds.Count > 0)
            {
                where.Append($" AND ft.c_categoryid IN ({string.Join(",", filter.CategoryIds)}) ");
            }

            // Cuisine multi-select
            if (filter.CuisineIds != null && filter.CuisineIds.Count > 0)
            {
                where.Append($" AND ft.c_cuisinetypeid IN ({string.Join(",", filter.CuisineIds)}) ");
            }

            // Status (Active / Inactive)
            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                where.Append($" AND ft.c_status = @Status ");
                parameters.Add(new SqlParameter("@Status", filter.Status));
            }

            // Toggle: Package Item?
            if (filter.IsPackageItem == true)
            {
                where.Append(" AND ft.c_ispackage_item = 1 ");
            }

            // Toggle: Sample Taste?
            if (filter.IsSampleTaste == true)
            {
                where.Append(" AND ft.c_issample_tasted = 1 ");
            }

            return where.ToString();
        }

        /// <summary>
        /// Get food items lookup for dropdowns
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<FoodItemDto>> GetFoodItemsLookup(long ownerPKID)
        {
            try
            {
                string selectQuery = $@"SELECT c_foodid AS ID, c_foodname AS Name FROM {Table.SysFoodItems} 
                                    WHERE c_ownerid = @OwnerPKID AND c_status = 1 and c_ispackage_item = 0";
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@OwnerPKID", ownerPKID),
                };
                var packageData = await _db.ExecuteAsync(selectQuery, parameters);
                if (packageData.Rows.Count > 0)
                {
                    List<FoodItemDto> foodItemList = new List<FoodItemDto>();
                    foreach (System.Data.DataRow row in packageData.Rows)
                    {
                        var package = new FoodItemDto
                        {
                            Id = Convert.ToInt64(row["ID"]),
                            Name = row["Name"]?.ToString(),
                        };
                        foodItemList.Add(package);
                    }
                    return foodItemList;
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

        public async Task<bool> IsValidFoodItemID(long ownerPKID, long foodItemPKID)
        {
            try
            {
                string query = $@"
                            SELECT COUNT(1)
                            FROM {Table.SysFoodItems}
                            WHERE c_ownerid = @OwnerPKID AND c_foodid = @FoodItemID AND c_is_deleted = 0";
                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@OwnerPKID", ownerPKID),
                    new SqlParameter("@FoodItemID", foodItemPKID)
                };
                var result = await _db.ExecuteScalarAsync(query, parameters.ToArray());
                int count = result != null ? Convert.ToInt32(result) : 0;
                return count > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
