using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CateringEcommerce.BAL.Base.Admin
{
    public class MasterDataRepository : IMasterDataRepository
    {
        private readonly IDatabaseHelper _dbHelper;

        public MasterDataRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // ===== CITY MANAGEMENT =====
        #region City Management
        public async Task<MasterDataListResponse<CityMasterItem>> GetCitiesAsync(MasterDataListRequest request)
        {
            var conditions = new List<string>();

            var baseParameters = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                conditions.Add("(c.c_cityname LIKE @SearchTerm OR s.c_statename LIKE @SearchTerm)");
                baseParameters.Add(new SqlParameter("@SearchTerm", $"%{request.SearchTerm}%"));
            }

            if (request.IsActive.HasValue)
            {
                conditions.Add("c.c_isactive = @IsActive");
                baseParameters.Add(new SqlParameter("@IsActive", request.IsActive.Value));
            }

            if (request.StateId.HasValue)
            {
                conditions.Add("c.c_stateid = @StateId");
                baseParameters.Add(new SqlParameter("@StateId", request.StateId.Value));
            }

            string whereClause = conditions.Count > 0
                ? "WHERE " + string.Join(" AND ", conditions)
                : string.Empty;

            // =========================
            // COUNT QUERY
            // =========================
            var countQuery = $@"
                SELECT COUNT(*)
                FROM {Table.City} c
                INNER JOIN {Table.State} s ON c.c_stateid = s.c_stateid
                {whereClause}";

            var totalCount = Convert.ToInt32(
                await _dbHelper.ExecuteScalarAsync(
                    countQuery,
                    baseParameters.Select(CloneParameter).ToArray()
                )
            );

            // =========================
            // DATA QUERY
            // =========================
            var offset = (request.PageNumber - 1) * request.PageSize;

            var sortColumn = request.SortBy switch
            {
                "Name" => "c.c_cityname",
                "State" => "s.c_statename",
                "DisplayOrder" => "c.c_cityid",
                _ => "c.c_cityid"
            };

            var sortOrder = request.SortOrder?.ToUpper() == "DESC" ? "DESC" : "ASC";

            var dataParameters = baseParameters
                .Select(CloneParameter)
                .ToList();

            dataParameters.Add(new SqlParameter("@Offset", offset));
            dataParameters.Add(new SqlParameter("@PageSize", request.PageSize));

            var dataQuery = $@"
                SELECT
                    c.c_cityid AS Id,
                    c.c_cityname AS Name,
                    c.c_stateid AS StateId,
                    s.c_statename AS StateName,
                    c.c_cityid AS DisplayOrder,
                    c.c_isactive AS IsActive,
                    c.c_createddate AS CreatedDate,
                    c.c_modifieddate AS ModifiedDate,
                    creator.c_fullname AS CreatedByName,
                    modifier.c_fullname AS ModifiedByName,
                    0 AS UsageCount
                FROM {Table.City} c
                INNER JOIN {Table.State} s ON c.c_stateid = s.c_stateid
                LEFT JOIN {Table.SysAdmin} creator ON c.c_createdby = creator.c_adminid
                LEFT JOIN {Table.SysAdmin} modifier ON c.c_modifiedby = modifier.c_adminid
                {whereClause}
                ORDER BY {sortColumn} {sortOrder}
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var dt = await _dbHelper.ExecuteAsync(dataQuery, dataParameters.ToArray());

            var items = new List<CityMasterItem>();
            foreach (DataRow row in dt.Rows)
            {
                items.Add(new CityMasterItem
                {
                    Id = Convert.ToInt64(row["Id"]),
                    Name = row["Name"]?.ToString() ?? string.Empty,
                    StateId = Convert.ToInt32(row["StateId"]),
                    StateName = row["StateName"]?.ToString() ?? string.Empty,
                    DisplayOrder = Convert.ToInt32(row["DisplayOrder"]),
                    IsActive = Convert.ToBoolean(row["IsActive"]),
                    CreatedDate = row["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(row["CreatedDate"]) : DateTime.MinValue,
                    ModifiedDate = row["ModifiedDate"] != DBNull.Value ? Convert.ToDateTime(row["ModifiedDate"]) : null,
                    CreatedByName = row["CreatedByName"]?.ToString(),
                    ModifiedByName = row["ModifiedByName"]?.ToString(),
                    UsageCount = Convert.ToInt32(row["UsageCount"])
                });
            }

            return new MasterDataListResponse<CityMasterItem>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };
        }


        public async Task<CityMasterItem?> GetCityByIdAsync(long id)
        {
            var query = $@"
                SELECT
                    c.c_cityid AS Id,
                    c.c_cityname AS Name,
                    c.c_stateid AS StateId,
                    s.c_state_name AS StateName,
                    c.c_display_order AS DisplayOrder,
                    c.c_isactive AS IsActive,
                    c.c_createddate AS CreatedDate,
                    c.c_modifieddate AS ModifiedDate,
                    creator.c_fullname AS CreatedByName,
                    modifier.c_fullname AS ModifiedByName
                FROM {Table.City} c
                INNER JOIN {Table.State} s ON c.c_stateid = s.c_stateid
                LEFT JOIN {Table.SysAdmin} creator ON c.c_createdby = creator.c_adminid
                LEFT JOIN {Table.SysAdmin} modifier ON c.c_modifiedby = modifier.c_adminid
                WHERE c.c_cityid = @Id";

            var parameters = new SqlParameter[] { new SqlParameter("@Id", id) };
            var dt = await _dbHelper.ExecuteAsync(query, parameters);

            if (dt.Rows.Count == 0) return null;

            var row = dt.Rows[0];
            return new CityMasterItem
            {
                Id = Convert.ToInt64(row["Id"]),
                Name = row["Name"].ToString() ?? string.Empty,
                StateId = Convert.ToInt32(row["StateId"]),
                StateName = row["StateName"].ToString() ?? string.Empty,
                DisplayOrder = Convert.ToInt32(row["DisplayOrder"]),
                IsActive = Convert.ToBoolean(row["IsActive"]),
                CreatedDate = row["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(row["CreatedDate"]) : DateTime.Now,
                ModifiedDate = row["ModifiedDate"] != DBNull.Value ? Convert.ToDateTime(row["ModifiedDate"]) : null,
                CreatedByName = row["CreatedByName"] != DBNull.Value ? row["CreatedByName"].ToString() : null,
                ModifiedByName = row["ModifiedByName"] != DBNull.Value ? row["ModifiedByName"].ToString() : null
            };
        }

        public async Task<long> CreateCityAsync(CreateMasterDataRequest request, long createdBy)
        {
            var query = $@"
                INSERT INTO {Table.City}
                    (c_cityid, c_cityname, c_stateid, c_isactive, c_createddate, c_createdby)
                VALUES
                    ((select MAX(c_cityid + 1) FROM {Table.City} WHERE c_stateid = @StateId), @Name, @StateId, 1, GETDATE(), @CreatedBy);
                SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Name", request.Name),
                new SqlParameter("@StateId", request.StateId ?? 0),
                new SqlParameter("@CreatedBy", createdBy)
            };

            await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            var result = await _dbHelper.ExecuteScalarAsync(
                "SELECT MAX(c_cityid) FROM " + Table.City + " WHERE c_stateid = @StateId",
                new SqlParameter[] { new SqlParameter("@StateId", request.StateId) }
            );
            return Convert.ToInt64(result);
        }

        public async Task<bool> UpdateCityAsync(UpdateMasterDataRequest request, long updatedBy)
        {
            var query = $@"
                UPDATE {Table.City}
                SET c_cityname = @Name,
                    c_modifieddate = GETDATE(),
                    c_modifiedby = @UpdatedBy
                WHERE c_cityid = @Id";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", request.Id),
                new SqlParameter("@Name", request.Name),
                new SqlParameter("@UpdatedBy", updatedBy)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateCityStatusAsync(long id, bool isActive, long updatedBy)
        {
            var query = $@"
                UPDATE {Table.City}
                SET c_isactive = @IsActive,
                    c_modifieddate = GETDATE(),
                    c_modifiedby = @UpdatedBy
                WHERE c_cityid = @Id";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", id),
                new SqlParameter("@IsActive", isActive),
                new SqlParameter("@UpdatedBy", updatedBy)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<List<StateDropdownItem>> GetStatesAsync()
        {
            var query = $@"
                SELECT c_stateid AS StateId, c_statename AS StateName
                FROM {Table.State}
                ORDER BY c_statename";

            var dt = await _dbHelper.ExecuteAsync(query, Array.Empty<SqlParameter>());
            var states = new List<StateDropdownItem>();

            foreach (DataRow row in dt.Rows)
            {
                states.Add(new StateDropdownItem
                {
                    StateId = Convert.ToInt32(row["StateId"]),
                    StateName = row["StateName"].ToString() ?? string.Empty
                });
            }

            return states;
        }
        #endregion

        // ===== FOOD CATEGORY MANAGEMENT =====

        #region Food Category Management
        public async Task<MasterDataListResponse<FoodCategoryMasterItem>> GetFoodCategoriesAsync(MasterDataListRequest request)
        {
            var conditions = new List<string>();
            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                conditions.Add("fc.c_categoryname LIKE @SearchTerm");
                parameters.Add(new SqlParameter("@SearchTerm", $"%{request.SearchTerm}%"));
            }

            if (request.IsActive.HasValue)
            {
                conditions.Add("fc.c_isactive = @IsActive");
                parameters.Add(new SqlParameter("@IsActive", request.IsActive.Value));
            }

            string whereClause = string.Empty;
            if (conditions.Count > 0)
                whereClause = "WHERE " + string.Join(" AND ", conditions);

            var countQuery = $@"
                SELECT COUNT(*)
                FROM {Table.SysFoodCategory} fc
                {whereClause}";

            var totalCount = Convert.ToInt32(await _dbHelper.ExecuteScalarAsync(countQuery, parameters.Select(CloneParameter).ToArray()));

            var offset = (request.PageNumber - 1) * request.PageSize;
            var sortColumn = request.SortBy switch
            {
                "Name" => "c_categoryname",
                "DisplayOrder" => "c_categoryid",
                _ => "c_categoryid"
            };
            var sortOrder = request.SortOrder?.ToUpper() == "DESC" ? "DESC" : "ASC";

            var dataParameters = parameters
                .Select(CloneParameter)
                .ToList();

            dataParameters.Add(new SqlParameter("@Offset", offset));
            dataParameters.Add(new SqlParameter("@PageSize", request.PageSize));

            var dataQuery = $@"
                SELECT
                    fc.c_categoryid AS Id,
                    fc.c_categoryname AS Name,
                    fc.c_is_global AS IsGlobal,
                    fc.c_categoryid AS DisplayOrder,                    
                    fc.c_description AS Description,
                    fc.c_isactive AS IsActive,
                    fc.c_createddate AS CreatedDate,
                    c_modifieddate AS ModifiedDate,
                    creator.c_fullname AS CreatedByName,
                    modifier.c_fullname AS ModifiedByName,
                    0 AS UsageCount
                FROM {Table.SysFoodCategory} fc
                LEFT JOIN {Table.SysAdmin} creator ON fc.c_createdby = creator.c_adminid
                LEFT JOIN {Table.SysAdmin} modifier ON fc.c_modifiedby = modifier.c_adminid
                {whereClause}
                ORDER BY {sortColumn} {sortOrder}
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";


            var dt = await _dbHelper.ExecuteAsync(dataQuery, dataParameters.ToArray());
            var items = new List<FoodCategoryMasterItem>();

            foreach (DataRow row in dt.Rows)
            {
                items.Add(new FoodCategoryMasterItem
                {
                    Id = Convert.ToInt64(row["Id"]),
                    Name = row["Name"].ToString() ?? string.Empty,
                    IsGlobal = row["IsGlobal"] != DBNull.Value && Convert.ToBoolean(row["IsGlobal"]),
                    Description = row["Description"].ToString() ?? string.Empty,
                    DisplayOrder = Convert.ToInt32(row["DisplayOrder"]),
                    IsActive = Convert.ToBoolean(row["IsActive"]),
                    CreatedDate = row["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(row["CreatedDate"]) : DateTime.Now,
                    ModifiedDate = row["ModifiedDate"] != DBNull.Value ? Convert.ToDateTime(row["ModifiedDate"]) : null,
                    CreatedByName = row["CreatedByName"] != DBNull.Value ? row["CreatedByName"].ToString() : null,
                    ModifiedByName = row["ModifiedByName"] != DBNull.Value ? row["ModifiedByName"].ToString() : null,
                    UsageCount = Convert.ToInt32(row["UsageCount"])
                });
            }

            return new MasterDataListResponse<FoodCategoryMasterItem>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };
        }

        public async Task<FoodCategoryMasterItem?> GetFoodCategoryByIdAsync(long id)
        {
            var query = $@"
                SELECT
                    c_categoryid AS Id,
                    c_categoryname AS Name,
                    c_is_global AS IsGlobal,
                    fc.c_description AS Description,
                    c_display_order AS DisplayOrder,
                    c_isactive AS IsActive,
                    c_createddate AS CreatedDate,
                    c_modifieddate AS ModifiedDate,
                    creator.c_fullname AS CreatedByName,
                    modifier.c_fullname AS ModifiedByName
                FROM {Table.SysFoodCategory}
                LEFT JOIN {Table.SysAdmin} creator ON c_createdby = creator.c_adminid
                LEFT JOIN {Table.SysAdmin} modifier ON c_modifiedby = modifier.c_adminid
                WHERE c_categoryid = @Id";

            var parameters = new SqlParameter[] { new SqlParameter("@Id", id) };
            var dt = await _dbHelper.ExecuteAsync(query, parameters);

            if (dt.Rows.Count == 0) return null;

            var row = dt.Rows[0];
            return new FoodCategoryMasterItem
            {
                Id = Convert.ToInt64(row["Id"]),
                Name = row["Name"].ToString() ?? string.Empty,
                IsGlobal = row["IsGlobal"] != DBNull.Value && Convert.ToBoolean(row["IsGlobal"]),
                Description = row["Description"].ToString() ?? string.Empty,
                DisplayOrder = Convert.ToInt32(row["DisplayOrder"]),
                IsActive = Convert.ToBoolean(row["IsActive"]),
                CreatedDate = row["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(row["CreatedDate"]) : DateTime.Now,
                ModifiedDate = row["ModifiedDate"] != DBNull.Value ? Convert.ToDateTime(row["ModifiedDate"]) : null,
                CreatedByName = row["CreatedByName"] != DBNull.Value ? row["CreatedByName"].ToString() : null,
                ModifiedByName = row["ModifiedByName"] != DBNull.Value ? row["ModifiedByName"].ToString() : null
            };
        }

        public async Task<long> CreateFoodCategoryAsync(CreateMasterDataRequest request, long createdBy)
        {
            var query = $@"
                INSERT INTO {Table.SysFoodCategory}
                    (c_categoryname, c_is_global, c_isactive, c_description, c_createddate, c_createdby)
                VALUES
                    (@Name, @IsGlobal, 1, @Description, GETDATE(), @CreatedBy);
                SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Name", request.Name),
                new SqlParameter("@IsGlobal", request.IsGlobal ?? false),
                new SqlParameter("@Description", (object?)request.Description ?? DBNull.Value),
                new SqlParameter("@CreatedBy", createdBy)
            };

            var result = await _dbHelper.ExecuteScalarAsync(query, parameters);
            return Convert.ToInt64(result);
        }

        public async Task<bool> UpdateFoodCategoryAsync(UpdateMasterDataRequest request, long updatedBy)
        {
            var query = $@"
                UPDATE {Table.SysFoodCategory}
                SET c_categoryname = @Name,
                    c_description = @Description,  
                    c_is_global = @IsGlobal,
                    c_modifieddate = GETDATE(),
                    c_modifiedby = @UpdatedBy
                WHERE c_categoryid = @Id";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", request.Id),
                new SqlParameter("@Name", request.Name),
                new SqlParameter("@IsGlobal", request.IsGlobal ?? false),
                new SqlParameter("@Description", (object?)request.Description ?? DBNull.Value),
                new SqlParameter("@UpdatedBy", updatedBy)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateFoodCategoryStatusAsync(long id, bool isActive, long updatedBy)
        {
            var query = $@"
                UPDATE {Table.SysFoodCategory}
                SET c_isactive = @IsActive,
                    c_modifieddate = GETDATE(),
                    c_modifiedby = @UpdatedBy
                WHERE c_categoryid = @Id";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", id),
                new SqlParameter("@IsActive", isActive),
                new SqlParameter("@UpdatedBy", updatedBy)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }
        #endregion

        // ===== CATERING TYPE MANAGEMENT =====

        #region Catering Type Management
        public async Task<MasterDataListResponse<CateringTypeMasterItem>> GetCateringTypesAsync(int categoryId, MasterDataListRequest request)
        {
            var conditions = new List<string> { "c_categoryid = @CategoryId" };
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@CategoryId", categoryId)
            };

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                conditions.Add("cm.c_type_name LIKE @SearchTerm");
                parameters.Add(new SqlParameter("@SearchTerm", $"%{request.SearchTerm}%"));
            }

            if (request.IsActive.HasValue)
            {
                conditions.Add("cm.c_isactive = @IsActive");
                parameters.Add(new SqlParameter("@IsActive", request.IsActive.Value));
            }

            var whereClause = "WHERE " + string.Join(" AND ", conditions);

            var countQuery = $@"
                SELECT COUNT(*)
                FROM {Table.SysCateringTypeMaster} cm
                {whereClause}";

            var totalCount = Convert.ToInt32(await _dbHelper.ExecuteScalarAsync(countQuery, parameters.Select(CloneParameter).ToArray()));

            var offset = (request.PageNumber - 1) * request.PageSize;
            var sortColumn = request.SortBy switch
            {
                "Name" => "c_type_name",
                "DisplayOrder" => "c_typeid",
                _ => "c_typeid"
            };
            var sortOrder = request.SortOrder?.ToUpper() == "DESC" ? "DESC" : "ASC";

            var dataParameters = parameters
                .Select(CloneParameter)
                .ToList();

            dataParameters.Add(new SqlParameter("@Offset", offset));
            dataParameters.Add(new SqlParameter("@PageSize", request.PageSize));

            var dataQuery = $@"
                SELECT
                    cm.c_typeid AS Id,
                    cm.c_type_name AS Name,
                    cm.c_categoryid AS CategoryId,
                    cm.c_description AS Description,
                    cm.c_typeid AS DisplayOrder,
                    cm.c_isactive AS IsActive,
                    cm.c_createddate AS CreatedDate,
                    cm.c_modifieddate AS ModifiedDate,
                    creator.c_fullname AS CreatedByName,
                    modifier.c_fullname AS ModifiedByName,
                    0 AS UsageCount
                FROM {Table.SysCateringTypeMaster} cm
                LEFT JOIN {Table.SysAdmin} creator ON cm.c_createdby = creator.c_adminid
                LEFT JOIN {Table.SysAdmin} modifier ON cm.c_modifiedby = modifier.c_adminid
                {whereClause}
                ORDER BY {sortColumn} {sortOrder}
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var dt = await _dbHelper.ExecuteAsync(dataQuery, dataParameters.ToArray());
            var items = new List<CateringTypeMasterItem>();

            var categoryName = GetCateogryName(categoryId);
            foreach (DataRow row in dt.Rows)
            {
                items.Add(new CateringTypeMasterItem
                {
                    Id = Convert.ToInt64(row["Id"]),
                    Name = row["Name"].ToString() ?? string.Empty,
                    CategoryId = Convert.ToInt32(row["CategoryId"]),
                    CategoryName = categoryName,
                    Description = row["Description"].ToString() ?? string.Empty,
                    DisplayOrder = Convert.ToInt32(row["DisplayOrder"]),
                    IsActive = Convert.ToBoolean(row["IsActive"]),
                    CreatedDate = row["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(row["CreatedDate"]) : DateTime.Now,
                    ModifiedDate = row["ModifiedDate"] != DBNull.Value ? Convert.ToDateTime(row["ModifiedDate"]) : null,
                    CreatedByName = row["CreatedByName"] != DBNull.Value ? row["CreatedByName"].ToString() : null,
                    ModifiedByName = row["ModifiedByName"] != DBNull.Value ? row["ModifiedByName"].ToString() : null,
                    UsageCount = Convert.ToInt32(row["UsageCount"])
                });
            }

            return new MasterDataListResponse<CateringTypeMasterItem>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };
        }

        public async Task<CateringTypeMasterItem?> GetCateringTypeByIdAsync(long id)
        {
            var query = $@"
                SELECT
                    c_typeid AS Id,
                    c_type_name AS Name,
                    c_categoryid AS CategoryId,
                    c_description AS Description,
                    c_typeid AS DisplayOrder,
                    c_isactive AS IsActive,
                    c_createddate AS CreatedDate,
                    c_modifieddate AS ModifiedDate,
                    creator.c_fullname AS CreatedByName,
                    modifier.c_fullname AS ModifiedByName
                FROM {Table.SysCateringTypeMaster}
                LEFT JOIN {Table.SysAdmin} creator ON c_createdby = creator.c_adminid
                LEFT JOIN {Table.SysAdmin} modifier ON c_modifiedby = modifier.c_adminid
                WHERE c_typeid = @Id";

            var parameters = new SqlParameter[] { new SqlParameter("@Id", id) };
            var dt = await _dbHelper.ExecuteAsync(query, parameters);

            if (dt.Rows.Count == 0) return null;

            var row = dt.Rows[0];
            var categoryId = Convert.ToInt32(row["CategoryId"]);
            var categoryName = GetCateogryName(categoryId);

            return new CateringTypeMasterItem
            {
                Id = Convert.ToInt64(row["Id"]),
                Name = row["Name"].ToString() ?? string.Empty,
                CategoryId = categoryId,
                Description = row["Description"].ToString() ?? string.Empty,
                CategoryName = categoryName,
                DisplayOrder = Convert.ToInt32(row["DisplayOrder"]),
                IsActive = Convert.ToBoolean(row["IsActive"]),
                CreatedDate = row["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(row["CreatedDate"]) : DateTime.Now,
                ModifiedDate = row["ModifiedDate"] != DBNull.Value ? Convert.ToDateTime(row["ModifiedDate"]) : null,
                CreatedByName = row["CreatedByName"] != DBNull.Value ? row["CreatedByName"].ToString() : null,
                ModifiedByName = row["ModifiedByName"] != DBNull.Value ? row["ModifiedByName"].ToString() : null
            };
        }

        public async Task<long> CreateCateringTypeAsync(CreateMasterDataRequest request, long createdBy)
        {
            var query = $@"
                INSERT INTO {Table.SysCateringTypeMaster}
                    (c_type_name, c_categoryid, c_isactive, c_description, c_createddate, c_createdby)
                VALUES
                    (@Name, @CategoryId, 1, @Description, GETDATE(), @CreatedBy);
                SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Name", request.Name),
                new SqlParameter("@CategoryId", request.CategoryId ?? 0),
                new SqlParameter("@Description", (object?)request.Description ?? DBNull.Value),
                new SqlParameter("@CreatedBy", createdBy)
            };

            var result = await _dbHelper.ExecuteScalarAsync(query, parameters);
            return Convert.ToInt64(result);
        }

        public async Task<bool> UpdateCateringTypeAsync(UpdateMasterDataRequest request, long updatedBy)
        {
            var query = $@"
                UPDATE {Table.SysCateringTypeMaster}
                SET c_type_name = @Name,
                    c_description = @Description,
                    c_modifieddate = GETDATE(),
                    c_modifiedby = @UpdatedBy
                WHERE c_typeid = @Id";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", request.Id),
                new SqlParameter("@Name", request.Name),
                new SqlParameter("@Description", (object?)request.Description ?? DBNull.Value),
                new SqlParameter("@UpdatedBy", updatedBy)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateCateringTypeStatusAsync(long id, bool isActive, long updatedBy)
        {
            var query = $@"
                UPDATE {Table.SysCateringTypeMaster}
                SET c_isactive = @IsActive,
                    c_modifieddate = GETDATE(),
                    c_modifiedby = @UpdatedBy
                WHERE c_typeid = @Id";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", id),
                new SqlParameter("@IsActive", isActive),
                new SqlParameter("@UpdatedBy", updatedBy)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }
        #endregion

        // ===== GUEST CATEGORY MANAGEMENT =====
        #region Guest Category Management
        public async Task<MasterDataListResponse<GuestCategoryMasterItem>> GetGuestCategoriesAsync(MasterDataListRequest request)
        {
            var conditions = new List<string>();
            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                conditions.Add("(gc.c_categoryname LIKE @SearchTerm OR gc.c_description LIKE @SearchTerm)");
                parameters.Add(new SqlParameter("@SearchTerm", $"%{request.SearchTerm}%"));
            }

            if (request.IsActive.HasValue)
            {
                conditions.Add("gc.c_isactive = @IsActive");
                parameters.Add(new SqlParameter("@IsActive", request.IsActive.Value));
            }

            string whereClause = string.Empty;
            if(conditions.Count > 0)
                whereClause = "WHERE " + string.Join(" AND ", conditions);

            var countQuery = $@"
                SELECT COUNT(*)
                FROM {Table.SysGuestCategory} gc
                {whereClause}";

            var totalCount = Convert.ToInt32(await _dbHelper.ExecuteScalarAsync(countQuery, parameters.Select(CloneParameter).ToArray()));

            var offset = (request.PageNumber - 1) * request.PageSize;
            var sortColumn = request.SortBy switch
            {
                "Name" => "c_categoryname",
                "DisplayOrder" => "c_guest_category_id",
                _ => "c_guest_category_id"
            };
            var sortOrder = request.SortOrder?.ToUpper() == "DESC" ? "DESC" : "ASC";

            var dataParameters = parameters
                .Select(CloneParameter)
                .ToList();

            dataParameters.Add(new SqlParameter("@Offset", offset));
            dataParameters.Add(new SqlParameter("@PageSize", request.PageSize));

            var dataQuery = $@"
                SELECT
                    gc.c_guest_category_id AS Id,
                    gc.c_categoryname AS Name,
                    gc.c_description AS Description,
                    gc.c_guest_category_id AS DisplayOrder,
                    gc.c_isactive AS IsActive,
                    gc.c_createddate AS CreatedDate,
                    gc.c_modifieddate AS ModifiedDate,
                    creator.c_fullname AS CreatedByName,
                    modifier.c_fullname AS ModifiedByName,
                    0 AS UsageCount
                FROM {Table.SysGuestCategory} gc
                LEFT JOIN {Table.SysAdmin} creator ON gc.c_createdby = creator.c_adminid
                LEFT JOIN {Table.SysAdmin} modifier ON gc.c_modifiedby = modifier.c_adminid
                {whereClause}
                ORDER BY {sortColumn} {sortOrder}
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var dt = await _dbHelper.ExecuteAsync(dataQuery, dataParameters.ToArray());
            var items = new List<GuestCategoryMasterItem>();

            foreach (DataRow row in dt.Rows)
            {
                items.Add(new GuestCategoryMasterItem
                {
                    Id = Convert.ToInt64(row["Id"]),
                    Name = row["Name"].ToString() ?? string.Empty,
                    Description = row["Description"] != DBNull.Value ? row["Description"].ToString() : null,
                    DisplayOrder = Convert.ToInt32(row["DisplayOrder"]),
                    IsActive = Convert.ToBoolean(row["IsActive"]),
                    CreatedDate = row["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(row["CreatedDate"]) : DateTime.Now,
                    ModifiedDate = row["ModifiedDate"] != DBNull.Value ? Convert.ToDateTime(row["ModifiedDate"]) : null,
                    CreatedByName = row["CreatedByName"] != DBNull.Value ? row["CreatedByName"].ToString() : null,
                    ModifiedByName = row["ModifiedByName"] != DBNull.Value ? row["ModifiedByName"].ToString() : null,
                    UsageCount = Convert.ToInt32(row["UsageCount"])
                });
            }

            return new MasterDataListResponse<GuestCategoryMasterItem>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };
        }

        public async Task<GuestCategoryMasterItem?> GetGuestCategoryByIdAsync(long id)
        {
            var query = $@"
                SELECT
                    c_guest_category_id AS Id,
                    c_categoryname AS Name,
                    c_description AS Description,
                    c_guest_category_id AS DisplayOrder,
                    c_isactive AS IsActive,
                    c_createddate AS CreatedDate,
                    c_modifieddate AS ModifiedDate,
                    creator.c_fullname AS CreatedByName,
                    modifier.c_fullname AS ModifiedByName
                FROM {Table.SysGuestCategory}
                LEFT JOIN {Table.SysAdmin} creator ON c_createdby = creator.c_adminid
                LEFT JOIN {Table.SysAdmin} modifier ON c_modifiedby = modifier.c_adminid
                WHERE c_guest_category_id = @Id";

            var parameters = new SqlParameter[] { new SqlParameter("@Id", id) };
            var dt = await _dbHelper.ExecuteAsync(query, parameters);

            if (dt.Rows.Count == 0) return null;

            var row = dt.Rows[0];
            return new GuestCategoryMasterItem
            {
                Id = Convert.ToInt64(row["Id"]),
                Name = row["Name"].ToString() ?? string.Empty,
                Description = row["Description"] != DBNull.Value ? row["Description"].ToString() : null,
                DisplayOrder = Convert.ToInt32(row["DisplayOrder"]),
                IsActive = Convert.ToBoolean(row["IsActive"]),
                CreatedDate = row["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(row["CreatedDate"]) : DateTime.Now,
                ModifiedDate = row["ModifiedDate"] != DBNull.Value ? Convert.ToDateTime(row["ModifiedDate"]) : null,
                CreatedByName = row["CreatedByName"] != DBNull.Value ? row["CreatedByName"].ToString() : null,
                ModifiedByName = row["ModifiedByName"] != DBNull.Value ? row["ModifiedByName"].ToString() : null
            };
        }

        public async Task<long> CreateGuestCategoryAsync(CreateMasterDataRequest request, long createdBy)
        {
            var query = $@"
                INSERT INTO {Table.SysGuestCategory}
                    (c_categoryname, c_description, c_isactive, c_createddate, c_createdby)
                VALUES
                    (@Name, @Description, 1, GETDATE(), @CreatedBy);
                SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Name", request.Name),
                new SqlParameter("@Description", (object?)request.Description ?? DBNull.Value),
                new SqlParameter("@DisplayOrder", request.DisplayOrder),
                new SqlParameter("@CreatedBy", createdBy)
            };

            var result = await _dbHelper.ExecuteScalarAsync(query, parameters);
            return Convert.ToInt64(result);
        }

        public async Task<bool> UpdateGuestCategoryAsync(UpdateMasterDataRequest request, long updatedBy)
        {
            var query = $@"
                UPDATE {Table.SysGuestCategory}
                SET c_categoryname = @Name,
                    c_description = @Description,
                    c_modifieddate = GETDATE(),
                    c_modifiedby = @UpdatedBy
                WHERE c_guest_category_id = @Id";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", request.Id),
                new SqlParameter("@Name", request.Name),
                new SqlParameter("@Description", (object?)request.Description ?? DBNull.Value),
                new SqlParameter("@UpdatedBy", updatedBy)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateGuestCategoryStatusAsync(long id, bool isActive, long updatedBy)
        {
            var query = $@"
                UPDATE {Table.SysGuestCategory}
                SET c_isactive = @IsActive,
                    c_modifieddate = GETDATE(),
                    c_modifiedby = @UpdatedBy
                WHERE c_guest_category_id = @Id";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", id),
                new SqlParameter("@IsActive", isActive),
                new SqlParameter("@UpdatedBy", updatedBy)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }
        #endregion

        // ===== THEME MANAGEMENT =====
        #region Theme Management
        public async Task<MasterDataListResponse<ThemeMasterItem>> GetThemesAsync(MasterDataListRequest request)
        {
            var conditions = new List<string>();
            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                conditions.Add("theme.c_theme_name LIKE @SearchTerm");
                parameters.Add(new SqlParameter("@SearchTerm", $"%{request.SearchTerm}%"));
            }

            if (request.IsActive.HasValue)
            {
                conditions.Add("theme.c_isactive = @IsActive");
                parameters.Add(new SqlParameter("@IsActive", request.IsActive.Value));
            }

            string whereClause = string.Empty;
            if (conditions.Count > 0)
                whereClause = "WHERE " + string.Join(" AND ", conditions);

            var countQuery = $@"
                SELECT COUNT(*)
                FROM {Table.SysCateringThemeTypes} theme
                {whereClause}";

            var totalCount = Convert.ToInt32(await _dbHelper.ExecuteScalarAsync(countQuery, parameters.Select(CloneParameter).ToArray()));

            var offset = (request.PageNumber - 1) * request.PageSize;
            var sortColumn = request.SortBy switch
            {
                "Name" => "c_theme_name",
                "DisplayOrder" => "c_theme_id",
                _ => "c_theme_id"
            };
            var sortOrder = request.SortOrder?.ToUpper() == "DESC" ? "DESC" : "ASC";

            var dataParameters = parameters
                .Select(CloneParameter)
                .ToList();

            dataParameters.Add(new SqlParameter("@Offset", offset));
            dataParameters.Add(new SqlParameter("@PageSize", request.PageSize));

            var dataQuery = $@"
                SELECT
                    theme.c_theme_id AS Id,
                    theme.c_theme_name AS Name,
                    theme.c_description AS Description,
                    theme.c_theme_id AS DisplayOrder,
                    theme.c_isactive AS IsActive,
                    theme.c_createddate AS CreatedDate,
                    theme.c_modifieddate AS ModifiedDate,
                    creator.c_fullname AS CreatedByName,
                    modifier.c_fullname AS ModifiedByName,
                    0 AS UsageCount
                FROM {Table.SysCateringThemeTypes} theme
                LEFT JOIN {Table.SysAdmin} creator ON theme.c_createdby = creator.c_adminid
                LEFT JOIN {Table.SysAdmin} modifier ON theme.c_modifiedby = modifier.c_adminid
                {whereClause}
                ORDER BY {sortColumn} {sortOrder}
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var dt = await _dbHelper.ExecuteAsync(dataQuery, dataParameters.ToArray());
            var items = new List<ThemeMasterItem>();

            foreach (DataRow row in dt.Rows)
            {
                items.Add(new ThemeMasterItem
                {
                    Id = Convert.ToInt64(row["Id"]),
                    Name = row["Name"].ToString() ?? string.Empty,
                    DisplayOrder = Convert.ToInt32(row["DisplayOrder"]),
                    Description = row["Description"].ToString() ?? string.Empty,
                    IsActive = Convert.ToBoolean(row["IsActive"]),
                    CreatedDate = row["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(row["CreatedDate"]) : DateTime.Now,
                    ModifiedDate = row["ModifiedDate"] != DBNull.Value ? Convert.ToDateTime(row["ModifiedDate"]) : null,
                    CreatedByName = row["CreatedByName"] != DBNull.Value ? row["CreatedByName"].ToString() : null,
                    ModifiedByName = row["ModifiedByName"] != DBNull.Value ? row["ModifiedByName"].ToString() : null,
                    UsageCount = Convert.ToInt32(row["UsageCount"])
                });
            }

            return new MasterDataListResponse<ThemeMasterItem>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };
        }

        public async Task<ThemeMasterItem?> GetThemeByIdAsync(long id)
        {
            var query = $@"
                SELECT
                    c_theme_id AS Id,
                    c_theme_name AS Name,
                    c_display_order AS DisplayOrder,
                    c_isactive AS IsActive,
                    c_createddate AS CreatedDate,
                    c_modifieddate AS ModifiedDate,
                    creator.c_fullname AS CreatedByName,
                    modifier.c_fullname AS ModifiedByName
                FROM {Table.SysCateringThemeTypes}
                LEFT JOIN {Table.SysAdmin} creator ON c_createdby = creator.c_adminid
                LEFT JOIN {Table.SysAdmin} modifier ON c_modifiedby = modifier.c_adminid
                WHERE c_theme_id = @Id  ";

            var parameters = new SqlParameter[] { new SqlParameter("@Id", id) };
            var dt = await _dbHelper.ExecuteAsync(query, parameters);

            if (dt.Rows.Count == 0) return null;

            var row = dt.Rows[0];
            return new ThemeMasterItem
            {
                Id = Convert.ToInt64(row["Id"]),
                Name = row["Name"].ToString() ?? string.Empty,
                DisplayOrder = Convert.ToInt32(row["DisplayOrder"]),
                IsActive = Convert.ToBoolean(row["IsActive"]),
                CreatedDate = row["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(row["CreatedDate"]) : DateTime.Now,
                ModifiedDate = row["ModifiedDate"] != DBNull.Value ? Convert.ToDateTime(row["ModifiedDate"]) : null,
                CreatedByName = row["CreatedByName"] != DBNull.Value ? row["CreatedByName"].ToString() : null,
                ModifiedByName = row["ModifiedByName"] != DBNull.Value ? row["ModifiedByName"].ToString() : null
            };
        }

        public async Task<long> CreateThemeAsync(CreateMasterDataRequest request, long createdBy)
        {
            var query = $@"
                INSERT INTO {Table.SysCateringThemeTypes}
                    (c_theme_name0, c_description, c_isactive, c_createddate, c_createdby)
                VALUES
                    (@Name, @Description, 1, GETDATE(), @CreatedBy);
                SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Name", request.Name),
                new SqlParameter("@Description", (object?)request.Description ?? DBNull.Value),
                new SqlParameter("@CreatedBy", createdBy)
            };

            var result = await _dbHelper.ExecuteScalarAsync(query, parameters);
            return Convert.ToInt64(result);
        }

        public async Task<bool> UpdateThemeAsync(UpdateMasterDataRequest request, long updatedBy)
        {
            var query = $@"
                UPDATE {Table.SysCateringThemeTypes}
                SET c_theme_name = @Name,
                    c_description = @Description,
                    c_modifieddate = GETDATE(),
                    c_modifiedby = @UpdatedBy
                WHERE c_theme_id = @Id  ";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", request.Id),
                new SqlParameter("@Name", request.Name),
                new SqlParameter("@Description", (object?)request.Description ?? DBNull.Value),
                new SqlParameter("@UpdatedBy", updatedBy)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateThemeStatusAsync(long id, bool isActive, long updatedBy)
        {
            var query = $@"
                UPDATE {Table.SysCateringThemeTypes}
                SET c_isactive = @IsActive,
                    c_modifieddate = GETDATE(),
                    c_modifiedby = @UpdatedBy
                WHERE c_theme_id = @Id  ";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", id),
                new SqlParameter("@IsActive", isActive),
                new SqlParameter("@UpdatedBy", updatedBy)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }
        #endregion

        // ===== COMMON OPERATIONS =====


        public async Task<UsageCheckResponse> CheckUsageAsync(string tableName, string idColumn, long id)
        {
            // This is a placeholder implementation
            // In a real scenario, you would query related tables to check usage
            // For now, we'll return a simple response allowing deactivation

            return new UsageCheckResponse
            {
                CanDeactivate = true,
                UsageCount = 0,
                Message = "This item is not currently in use and can be deactivated.",
                UsageDetails = new List<string>()
            };
        }

        public async Task<bool> NameExistsAsync(string tableName, string nameColumn, string name, long? excludeId = null)
        {
            var query = excludeId.HasValue
                ? $"SELECT COUNT(*) FROM {tableName} WHERE {nameColumn} = @Name   AND {GetIdColumn(tableName)} != @ExcludeId"
                : $"SELECT COUNT(*) FROM {tableName} WHERE {nameColumn} = @Name  ";

            var parameters = excludeId.HasValue
                ? new SqlParameter[]
                {
                    new SqlParameter("@Name", name),
                    new SqlParameter("@ExcludeId", excludeId.Value)
                }
                : new SqlParameter[] { new SqlParameter("@Name", name) };

            var count = Convert.ToInt32(await _dbHelper.ExecuteScalarAsync(query, parameters));
            return count > 0;
        }

        private string GetIdColumn(string tableName)
        {
            return tableName switch
            {
                var t when t == Table.City => "c_cityid",
                var t when t == Table.SysFoodCategory => "c_categoryid",
                var t when t == Table.SysCateringTypeMaster => "c_typeid",
                var t when t == Table.SysGuestCategory => "c_guest_category_id",
                var t when t == Table.SysCateringThemeTypes => "c_theme_id",
                _ => "id"
            };
        }

        private static SqlParameter CloneParameter(SqlParameter p)
        {
            return new SqlParameter(p.ParameterName, p.Value)
            {
                DbType = p.DbType,
                Size = p.Size,
                Precision = p.Precision,
                Scale = p.Scale
            };
        }

        private string GetCateogryName(int categoryId)
        {
            var categoryName = categoryId switch
            {
                1 => ServiceType.FoodType.GetDisplayName(),
                2 => ServiceType.CuisineType.GetDisplayName(),
                3 => ServiceType.EventType.GetDisplayName(),
                4 => ServiceType.ServiceType.GetDisplayName(),
                5 => ServiceType.ServingSlotType.GetDisplayName(),
                _ => "Unknown"
            };
            return categoryName;
        }

    }
}
