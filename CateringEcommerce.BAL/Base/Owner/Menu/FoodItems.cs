using CateringEcommerce.BAL.Common;
using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.Data.SqlClient;

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


        public async Task<long> AddFoodItem(long ownerPKID, FoodItemDto foodItem)
        {
            try
            {
                string insertQuery = $@"INSERT INTO {Table.SysFoodItems}
                                       (c_ownerid, c_foodname, c_description, c_categoryid, c_cuisinetypeid, c_price, c_ispackage_item, c_status)
                                       VALUES (@OwnerPKID, @FoodName, @Description, @CategoryID, @CuisineID, @Price, @IsPackageItem, @Status);
                                       SELECT SCOPE_IDENTITY();";

                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerPKID", ownerPKID),
                    new SqlParameter("@FoodName", foodItem.Name),
                    new SqlParameter("@Description", foodItem.Description ?? (object)DBNull.Value),
                    new SqlParameter("@CategoryID", foodItem.CategoryId),
                    new SqlParameter("@CuisineID", foodItem.TypeId ?? (object)DBNull.Value),
                    new SqlParameter("@Price", foodItem.Price),
                    new SqlParameter("@IsPackageItem", foodItem.IsPackageItem.ToBinary()),
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
        

        public async Task<List<FoodItemModel>> GetFoodItems(long ownerPKID)
        {
            try
            {
                string selectQuery = $@"SELECT ft.c_foodid AS FoodId, ft.c_foodname AS FoodName, ft.c_description AS Description, ft.c_categoryid AS CategoryID, ft.c_cuisinetypeid AS CuisineTypeID, ft.c_price AS Price, 
                                    ft.c_ispackage_item AS IsPackageItem, ft.c_status AS Status, fc.c_categoryname AS CategoryName, tm.c_type_name AS CuisineTypeName
                                FROM {Table.SysFoodItems} ft 
                                LEFT JOIN {Table.SysFoodCategory} fc ON ft.c_categoryid = fc.c_categoryid 
                                LEFT JOIN {Table.SysCateringTypeMaster} tm ON ft.c_cuisinetypeid = tm.c_type_id
                                WHERE ft.c_ownerid = @OwnerPKID";
                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerPKID", ownerPKID)
                };

                var foodItemsData = await _db.ExecuteAsync(selectQuery, parameters.ToArray());
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
        public async Task<int> UpdateFoodItem(long ownerPKID, FoodItemDto foodItem)
        {
            try
            {

                string updateQuery = $@"UPDATE {Table.SysFoodItems}
                       SET c_foodname = @FoodName, c_description = @Description, c_categoryid = @CategoryID, 
                        c_cuisinetypeid = @CuisineID, c_price = @Price, c_ispackage_item = @IsPackageItem, c_status = @Status
                        WHERE c_foodid = @FoodID AND c_ownerid = @OwnerPKID";

                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerPKID", ownerPKID),
                    new SqlParameter("@FoodName", foodItem.Name),
                    new SqlParameter("@Description", foodItem.Description ?? (object)DBNull.Value),
                    new SqlParameter("@CategoryID", foodItem.CategoryId),
                    new SqlParameter("@CuisineID", foodItem.TypeId ?? (object)DBNull.Value),
                    new SqlParameter("@Price", foodItem.Price),
                    new SqlParameter("@IsPackageItem", foodItem.IsPackageItem.ToBinary()),
                    new SqlParameter("@FoodID", foodItem.Id),
                    new SqlParameter("@Status", foodItem.Status.ToBinary()) // Assuming 1 for active, 0 for inactive
                };

                return await _db.ExecuteNonQueryAsync(updateQuery.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<int> DeleteFoodItem(long ownerPKID, long foodItemPKID)
        {
            try
            {
                string deleteQuery = $@"DELETE FROM {Table.SysFoodItems} WHERE c_ownerid = @OwnerPKID AND c_foodid = @FoodID";
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

        public async Task<bool> IsFoodItemNameExists(long ownerPKID, string foodItemName, long? foodItemPKID = null)
        {
            try
            {
                // Base query for checking duplicates
                string query = $@"
                            SELECT COUNT(1)
                            FROM {Table.SysFoodItems}
                            WHERE c_ownerid = @OwnerPKID
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
    }
}
