using CateringEcommerce.BAL.Common;
using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Text;

namespace CateringEcommerce.BAL.Base.Owner
{
    public class Discounts : IDiscounts
    {
        private readonly IDatabaseHelper _dbHelper;
        public Discounts(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        /// <summary>
        /// Add a new discount for the owner
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="discount"></param>
        /// <returns></returns>
        public async Task<int> AddDiscountAsync(long ownerPKID, DiscountDto discount)
        {
            try
            {
                String insertQuery = $@"INSERT INTO {Table.SysCateringDiscount} 
                                     (c_ownerid, c_discount_name, c_discount_description, c_discount_type, c_discount_mode, c_discount_value, c_min_order_value,
                                     c_max_discount_value, c_discount_code, c_max_uses_per_order, c_max_uses_per_user, c_is_stackable, c_startdate, c_enddate, c_isactive, c_isautodisable) 
                                     VALUES 
                                     (@OwnerPKID, @DiscountName, @Description, @Type, @Mode, @DiscountValue, @MinOrderValue,
                                     @MaxDiscountValue, @Code, @MaxUsesPerOrder, @MaxUsesPerUser, @IsStackable, @Startdate, @Enddate, @IsActive, @AutoDisable);
                                     SELECT SCOPE_IDENTITY(); ";

                string discountCode = await GenerateUniqueDiscountCodeAsync(discount.Name, discount.Value, discount.Mode);

                // ✅ Prepare SQL parameters
                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerPKID", ownerPKID),
                    new SqlParameter("@DiscountName", discount.Name?.ToString()),
                    new SqlParameter("@Description", discount.Description?.ToString()),
                    new SqlParameter("@Type", discount.Type),
                    new SqlParameter("@Mode", discount.Mode),
                    new SqlParameter("@DiscountValue", discount.Value),
                    new SqlParameter("@MinOrderValue", discount.MinOrderValue ?? (object)DBNull.Value),
                    new SqlParameter("@MaxDiscountValue", discount.MaxDiscount ?? (object)DBNull.Value),
                    new SqlParameter("@Code", discountCode),
                    new SqlParameter("@MaxUsesPerOrder", discount.MaxUsesPerOrder ?? (object)DBNull.Value),
                    new SqlParameter("@MaxUsesPerUser", discount.MaxUsesPerUser ?? (object)DBNull.Value),
                    new SqlParameter("@IsStackable", discount.IsStackable.ToBinary()),
                    new SqlParameter("@Startdate", discount.StartDate),
                    new SqlParameter("@Enddate", discount.EndDate),
                    new SqlParameter("@IsActive", discount.IsActive.ToBinary()),
                    new SqlParameter("@AutoDisable", discount.AutoDisable.ToBinary()),
                };

                // ✅ Execute insert query
                var result = await _dbHelper.ExecuteScalarAsync(insertQuery, parameters.ToArray());

                // ✅ Convert result to int (new record ID)
                int newDiscountId = result != null ? Convert.ToInt32(result) : 0;
                return newDiscountId;
            }
            catch (Exception ex)
            {    
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Soft delete the discount for the owner
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="discountPKID"></param>
        /// <returns></returns>
        public async Task<bool> SoftDeleteDiscountAsync(long ownerPKID, long discountPKID)
        {
            try
            {
                string query = $@"UPDATE {Table.SysCateringDiscount} SET c_is_deleted = 1, c_status = 0, c_modifieddate = GETDATE() 
                                WHERE c_ownerid = @OwnerPKID AND c_discountid = @DiscountID";
                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerPKID", ownerPKID),
                    new SqlParameter("@DiscountID", discountPKID)
                }; 
                return Convert.ToInt16(await _dbHelper.ExecuteNonQueryAsync(query, parameters.ToArray())) > 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get the list of discounts for the owner with pagination and filtering
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="filterJson"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<List<DiscountModel>> GetDiscountListAsync(long ownerPKID, int page, int pageSize, string filterJson)
        {
            try
            {
                var filter = string.IsNullOrWhiteSpace(filterJson)
                    ? new DiscountFilter()
                    : JsonConvert.DeserializeObject<DiscountFilter>(filterJson) ?? new DiscountFilter();

                int offset = (page - 1) * pageSize;

                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerPKID", ownerPKID),
                    new SqlParameter("@Offset", offset),
                    new SqlParameter("@PageSize", pageSize)
                };

                StringBuilder selectQuery = new();
                selectQuery.Append($@"
                    SELECT 
                        d.c_discountid AS ID,
                        d.c_discount_name AS Name,
                        d.c_discount_description AS Description,
                        d.c_discount_code AS Code,
                        d.c_discount_type AS Type,
                        d.c_discount_mode AS Mode,
                        d.c_discount_value AS Value,
                        d.c_min_order_value AS MinOrder,
                        d.c_max_discount_value AS MaxValue,
                        d.c_max_uses_per_order AS MaxUsersOrder,
                        d.c_max_uses_per_user AS MaxUsersUser,
                        d.c_isactive AS IsActive,
                        d.c_startdate AS StartDate,
                        d.c_enddate AS EndDate,
                        d.c_is_stackable AS IsStackable,
                        d.c_isautodisable AS AutoDisable
                    FROM {Table.SysCateringDiscount} d
                ");

                selectQuery.Append(BuildDiscountFilterQuery(filter, parameters));

                selectQuery.Append(@"
                    ORDER BY d.c_discountid DESC
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY;
                ");

                var discountData = await _dbHelper.ExecuteAsync(selectQuery.ToString(), parameters.ToArray());
                if (discountData.Rows.Count == 0)
                    return new List<DiscountModel>();

                var discountList = discountData.AsEnumerable()
                    .Select(row => new DiscountModel
                    {
                        ID = row.GetValue<long?>("ID"),
                        Name = row.GetValue<string>("Name"),
                        Description = row.GetValue<string>("Description"),
                        Code = row.GetValue<string>("Code"),
                        Type = row.GetValue<int>("Type"),
                        Mode = row.GetValue<int>("Mode"),
                        Value = row.GetValue<decimal>("Value"),
                        MinOrderValue = row.GetValue<decimal?>("MinOrder"),
                        MaxDiscount = row.GetValue<decimal?>("MaxValue"),
                        MaxUsesPerOrder = row.GetValue<int>("MaxUsersOrder"),
                        MaxUsesPerUser = row.GetValue<int>("MaxUsersUser"),
                        IsActive = row.GetValue<bool>("IsActive"),
                        StartDate = row.IsNull("StartDate") ? null : DateOnly.FromDateTime(row.Field<DateTime>("StartDate")),
                        EndDate = row.IsNull("EndDate") ? null : DateOnly.FromDateTime(row.Field<DateTime>("EndDate")),
                        IsStackable = row.GetValue<bool>("IsStackable"),
                        AutoDisable = row.GetValue<bool>("AutoDisable"),
                        SelectedItems = new List<long>()
                    })
                    .ToList();

                MappingSyncService _mappingSyncService = new MappingSyncService(_dbHelper);
                // ✅ Load selected items based on Discount Type
                foreach (var discount in discountList)
                {
                    if (!discount.ID.HasValue)
                        continue;

                    switch ((DiscountType)discount.Type)
                    {
                        case DiscountType.Item:
                            discount.SelectedItems =
                                await _mappingSyncService.GetChildIdsByParentAsync(
                                    Table.SysCateringDiscountItemMapping,
                                    "c_discountid",
                                    "c_foodid",
                                    discount.ID.Value
                                );
                            break;

                        case DiscountType.Package:
                            discount.SelectedItems =
                                await _mappingSyncService.GetChildIdsByParentAsync(
                                    Table.SysCateringDiscountPackageMapping,
                                    "c_discountid",
                                    "c_packageid",
                                    discount.ID.Value
                                );
                            break;
                             
                        case DiscountType.EntireCatering:
                            // No mapping required
                            break;
                    }
                }

                return discountList;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error while fetching discount list: {ex.Message}", ex);
            }
        }


        /// <summary>
        /// Get the count of discounts for the owner based on filters
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="filterJson"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<int> GetDiscountsCountAsync(long ownerPKID, string filterJson)
        {
            try
            {
                var filter = string.IsNullOrWhiteSpace(filterJson)
                ? new DiscountFilter()
                : JsonConvert.DeserializeObject<DiscountFilter>(filterJson) ?? new DiscountFilter();

                StringBuilder selectQuery = new StringBuilder();
                selectQuery.Append($"SELECT COUNT(1) FROM {Table.SysCateringDiscount} AS d");
                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerPKID", ownerPKID)
                };

                selectQuery.Append(BuildDiscountFilterQuery(filter, parameters));

                var result = await _dbHelper.ExecuteScalarAsync(selectQuery.ToString(), parameters.ToArray());
                int count = result != null ? Convert.ToInt32(result) : 0;

                return count;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while getting discount count: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Is Valid discount ID for the owner
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="discountPKID"></param>
        /// <returns></returns>
        public async Task<bool> IsValidDiscountId(long ownerPKID, long discountPKID)
        {
            try
            {
                string selectQuery = $@"SELECT COUNT(*) FROM {Table.SysCateringDiscount}
                                     WHERE c_ownerid = @OwnerPKID AND c_is_deleted = 0
                                    AND c_discountid = @DiscountID";
                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerPKID", ownerPKID),
                    new SqlParameter("@DiscountID", discountPKID)
                };
                var result = await _dbHelper.ExecuteScalarAsync(selectQuery, parameters.ToArray());
                int count = result != null ? Convert.ToInt32(result) : 0;
                return count > 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Update the discount details for the owner
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="discount"></param>
        /// <returns></returns>
        public async Task<int> UpdateDiscountAsync(long ownerPKID, DiscountDto discount)
        {
            try
            {
                StringBuilder updateQuery = new();
                updateQuery.Append($@"
                UPDATE {Table.SysCateringDiscount}
                SET 
                    c_discount_name = @DiscountName, 
                    c_discount_description = @Description, 
                    c_discount_type = @Type, 
                    c_discount_mode = @Mode, 
                    c_discount_value = @DiscountValue, 
                    c_min_order_value = @MinOrderValue,
                    c_max_discount_value = @MaxDiscountValue, 
                    c_max_uses_per_order = @MaxUsesPerOrder, 
                    c_max_uses_per_user = @MaxUsesPerUser, 
                    c_is_stackable = @IsStackable, 
                    c_startdate = @Startdate, 
                    c_enddate = @EndDate, 
                    c_isactive = @IsActive, 
                    c_isautodisable = @AutoDisable,
                    c_modifieddate = GETDATE()"); 

                // ✅ Prepare SQL parameters
                List<SqlParameter> parameters = new List<SqlParameter>
                {
                    new SqlParameter("@OwnerPKID", ownerPKID),
                    new SqlParameter("@DiscountID", discount.ID),
                    new SqlParameter("@DiscountName", discount.Name?.ToString()),
                    new SqlParameter("@Description", discount.Description?.ToString()),
                    new SqlParameter("@Type", discount.Type),
                    new SqlParameter("@Mode", discount.Mode),
                    new SqlParameter("@DiscountValue", discount.Value),
                    new SqlParameter("@MinOrderValue", discount.MinOrderValue > 0 ? discount.MinOrderValue : DBNull.Value),
                    new SqlParameter("@MaxDiscountValue", discount.MaxDiscount),
                    new SqlParameter("@MaxUsesPerOrder", discount.MaxUsesPerOrder),
                    new SqlParameter("@MaxUsesPerUser", discount.MaxUsesPerUser),
                    new SqlParameter("@IsStackable", discount.IsStackable.ToBinary()),
                    new SqlParameter("@Startdate", discount.StartDate),
                    new SqlParameter("@Enddate", discount.EndDate),
                    new SqlParameter("@IsActive", discount.IsActive.ToBinary()),
                    new SqlParameter("@AutoDisable", discount.AutoDisable.ToBinary()),
                };

                if (discount.IsChangeDiscountCode)
                {
                    // Generate new unique discount code
                    string discountCode = await GenerateUniqueDiscountCodeAsync(discount.Name, discount.Value, discount.Mode);
                    updateQuery.Append(", c_discount_code = @Code ");
                    parameters.Add(new SqlParameter("@Code", discountCode));
                }
                
                updateQuery.Append(" WHERE c_ownerid = @OwnerPKID AND c_discountid = @DiscountID");

                // ✅ Execute update query
                var result = await _dbHelper.ExecuteNonQueryAsync(updateQuery.ToString(), parameters.ToArray());
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error facing the during the updating time: " + ex.Message);
            }
        }

        /// <summary>
        /// Build the SQL WHERE clause based on the discount filter
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string BuildDiscountFilterQuery(DiscountFilter filter, List<SqlParameter> parameters)
        {
            StringBuilder where = new();
            where.Append(" WHERE d.c_ownerid = @OwnerPKID AND c_is_deleted = 0");

            // Name search
            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                where.Append(" AND LOWER(d.c_discount_name) LIKE LOWER('%' + @Name + '%') ");
                parameters.Add(new SqlParameter("@Name", filter.Name));
            }

            // Theme filter
            if (filter.Type != DiscountType.All.GetHashCode())
            {
                where.Append($" AND d.c_discount_type = @DiscountType");
                parameters.Add(new SqlParameter("@DiscountType", filter.Type));
            }

            // Status filter
            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                where.Append(" AND d.c_isactive = @Status ");
                parameters.Add(new SqlParameter("@Status", filter.Status));
            }

            return where.ToString();
        }


        #region Generate discount code with check the uniqueness

        /// <summary>
        /// Generate the unique discount code as per the format and ensure its uniqueness
        /// </summary>
        /// <param name="discountName"></param>
        /// <param name="discountValue"></param>
        /// <param name="discountMode"></param>
        /// <param name="maxAttempts"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<string> GenerateUniqueDiscountCodeAsync(string discountName, decimal discountValue, int discountMode, int maxAttempts = 10)
        {
            int attempt = 0;

            while (attempt < maxAttempts)
            {
                attempt++;

                string code = GenerateDiscountCode(
                    discountName,
                    discountValue,
                    discountMode
                );

                bool exists = await IsDiscountCodeExistsAsync(code);

                if (!exists)
                    return code;
            }

            throw new Exception("Unable to generate unique discount code. Please try again.");
        }

        /// <summary>
        /// generate discount code based on the specified format
        /// </summary>
        /// <param name="discountName"></param>
        /// <param name="discountValue"></param>
        /// <param name="discountMode"></param>
        /// <param name="nameLength"></param>
        /// <param name="randomLength"></param>
        /// <returns></returns>
        private static string GenerateDiscountCode(string discountName, decimal discountValue, int discountMode, int nameLength = 6, int randomLength = 3)
        {
            // STEP 1: Normalize name
            string normalizedName = new string(
                discountName.Where(char.IsLetterOrDigit).ToArray()
            ).ToUpper();

            string nameKey = normalizedName.Length >= nameLength
                ? normalizedName.Substring(0, nameLength)
                : normalizedName;

            // STEP 2: Resolve enum + display name
            var enumType = (DiscountMode)discountMode;
            string typeName = Utils.GetEnumDisplayName(enumType);

            string valueCode = typeName.Equals("Percentage", StringComparison.OrdinalIgnoreCase)
                ? $"{discountValue}P"
                : $"{discountValue}F";

            // STEP 3: Random suffix
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();

            string randomSuffix = new string(
                Enumerable.Range(0, randomLength)
                    .Select(_ => chars[random.Next(chars.Length)])
                    .ToArray()
            );

            return $"{nameKey}-{valueCode}-{randomSuffix}";
        }


        /// <summary>
        /// Check if the generated discount code already exists in the database
        /// </summary>
        /// <param name="discountCode"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<bool> IsDiscountCodeExistsAsync(string discountCode)
        {
            try
            {
                string query = $@"SELECT COUNT(1) FROM {Table.SysCateringDiscount} WHERE c_discount_code = @DiscountCode";
                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@DiscountCode", discountCode)
                };
                var result = await _dbHelper.ExecuteScalarAsync(query, parameters.ToArray());
                int count = result != null ? Convert.ToInt32(result) : 0;
                return count > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while checking discount code existence: {ex.Message}", ex);
            }
        }
        #endregion

        /// <summary>
        /// Check if the discount name already exists for the owner
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="discountName"></param>
        /// <param name="discountPKID"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> IsDiscountNameExists(long ownerPKID, string discountName, long? discountPKID = null)
        {
            try
            {
                // Base query for checking duplicates
                string query = $@"
                            SELECT COUNT(1)
                            FROM {Table.SysCateringDiscount}
                            WHERE c_ownerid = @OwnerPKID AND c_is_deleted = 0
                              AND LOWER(LTRIM(RTRIM(c_discount_name))) = LOWER(LTRIM(RTRIM(@Name)))";

                // Exclude the current record in case of update
                if (discountPKID.HasValue && discountPKID.Value > 0)
                {
                    query += " AND c_discountid <> @DiscountPKID";
                }

                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@OwnerPKID", ownerPKID),
                    new SqlParameter("@Name", discountName)
                };

                if (discountPKID.HasValue && discountPKID.Value > 0)
                    parameters.Add(new SqlParameter("@DiscountPKID", discountPKID.Value));

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
        /// Check if the given price is more than or equal to any of the selected item prices for the owner   
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="selectedItemIds"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> IsHigherThanSelectedItemPrice(string tableName, string pkColumnName, long ownerPKID, decimal price, List<long> selectedItemIds)
        {
            try
            {
                string query = $@"SELECT COUNT(*) FROM {tableName}
                            WHERE c_ownerid = @OwnerPKID AND {pkColumnName} = @ID AND c_price <= @Price";
                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@OwnerPKID", ownerPKID),
                    new SqlParameter("@Price", price)
                };
                foreach (var itemId in selectedItemIds)
                {
                    parameters.Add(new SqlParameter("@ID", itemId));
                    var result = await _dbHelper.ExecuteScalarAsync(query, parameters.ToArray());
                    int count = result != null ? Convert.ToInt32(result) : 0;
                    if (count > 0)
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
