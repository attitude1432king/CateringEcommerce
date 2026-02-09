using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CateringEcommerce.BAL.Base.Admin
{
    public partial class SettingsRepository
    {
        // =============================================
        // COMMISSION CONFIGURATION METHODS
        // =============================================

        public async Task<CommissionListResponse> GetCommissionConfigsAsync(CommissionListRequest request)
        {
            var conditions = new List<string>();
            var baseParameters = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(request.ConfigType))
            {
                conditions.Add("c.c_config_type = @ConfigType");
                baseParameters.Add(new SqlParameter("@ConfigType", request.ConfigType));
            }

            if (request.CateringOwnerId.HasValue)
            {
                conditions.Add("c.c_catering_ownerid = @CateringOwnerId");
                baseParameters.Add(new SqlParameter("@CateringOwnerId", request.CateringOwnerId.Value));
            }

            if (request.IsActive.HasValue)
            {
                conditions.Add("c.c_is_active = @IsActive");
                baseParameters.Add(new SqlParameter("@IsActive", request.IsActive.Value));
            }

            if (request.EffectiveDate.HasValue)
            {
                conditions.Add("(c.c_effective_from <= @EffectiveDate AND (c.c_effective_to IS NULL OR c.c_effective_to >= @EffectiveDate))");
                baseParameters.Add(new SqlParameter("@EffectiveDate", request.EffectiveDate.Value));
            }

            string whereClause = conditions.Count > 0
                ? "WHERE " + string.Join(" AND ", conditions)
                : string.Empty;

            // COUNT QUERY
            var countQuery = $@"
                SELECT COUNT(*)
                FROM {Table.SysCommissionConfig} c
                {whereClause}";

            var totalCount = Convert.ToInt32(
                await _dbHelper.ExecuteScalarAsync(
                    countQuery,
                    baseParameters.Select(CloneParameter).ToArray()
                )
            );

            // DATA QUERY
            var offset = (request.PageNumber - 1) * request.PageSize;

            var sortColumn = request.SortBy switch
            {
                "ConfigName" => "c.c_config_name",
                "ConfigType" => "c.c_config_type",
                "CommissionRate" => "c.c_commission_rate",
                "EffectiveFrom" => "c.c_effective_from",
                _ => "c.c_created_date"
            };

            var sortOrder = request.SortOrder?.ToUpper() == "DESC" ? "DESC" : "ASC";

            var dataParameters = baseParameters
                .Select(CloneParameter)
                .ToList();

            dataParameters.Add(new SqlParameter("@Offset", offset));
            dataParameters.Add(new SqlParameter("@PageSize", request.PageSize));

            var dataQuery = $@"
                SELECT
                    c.c_config_id AS ConfigId,
                    c.c_config_name AS ConfigName,
                    c.c_config_type AS ConfigType,
                    c.c_catering_ownerid AS CateringOwnerId,
                    o.c_fullname AS CateringOwnerName,
                    o.c_businessname AS BusinessName,
                    c.c_commission_rate AS CommissionRate,
                    c.c_fixed_fee AS FixedFee,
                    c.c_min_order_value AS MinOrderValue,
                    c.c_max_order_value AS MaxOrderValue,
                    c.c_is_active AS IsActive,
                    c.c_effective_from AS EffectiveFrom,
                    c.c_effective_to AS EffectiveTo,
                    c.c_created_date AS CreatedDate,
                    c.c_created_by AS CreatedBy,
                    c.c_modified_date AS ModifiedDate,
                    c.c_modified_by AS ModifiedBy
                FROM {Table.SysCommissionConfig} c
                LEFT JOIN t_owner_register o ON c.c_catering_ownerid = o.c_ownerid
                {whereClause}
                ORDER BY {sortColumn} {sortOrder}
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            var configs = new List<CommissionConfigItem>();

            using (var reader = await _dbHelper.ExecuteReaderAsync(dataQuery, dataParameters.ToArray()))
            {
                while (await reader.ReadAsync())
                {
                    configs.Add(new CommissionConfigItem
                    {
                        ConfigId = reader.GetInt64(reader.GetOrdinal("ConfigId")),
                        ConfigName = reader.GetString(reader.GetOrdinal("ConfigName")),
                        ConfigType = reader.GetString(reader.GetOrdinal("ConfigType")),
                        CateringOwnerId = reader.IsDBNull(reader.GetOrdinal("CateringOwnerId")) ? null : reader.GetInt64(reader.GetOrdinal("CateringOwnerId")),
                        CateringOwnerName = reader.IsDBNull(reader.GetOrdinal("CateringOwnerName")) ? null : reader.GetString(reader.GetOrdinal("CateringOwnerName")),
                        BusinessName = reader.IsDBNull(reader.GetOrdinal("BusinessName")) ? null : reader.GetString(reader.GetOrdinal("BusinessName")),
                        CommissionRate = reader.GetDecimal(reader.GetOrdinal("CommissionRate")),
                        FixedFee = reader.GetDecimal(reader.GetOrdinal("FixedFee")),
                        MinOrderValue = reader.IsDBNull(reader.GetOrdinal("MinOrderValue")) ? null : reader.GetDecimal(reader.GetOrdinal("MinOrderValue")),
                        MaxOrderValue = reader.IsDBNull(reader.GetOrdinal("MaxOrderValue")) ? null : reader.GetDecimal(reader.GetOrdinal("MaxOrderValue")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                        EffectiveFrom = reader.GetDateTime(reader.GetOrdinal("EffectiveFrom")),
                        EffectiveTo = reader.IsDBNull(reader.GetOrdinal("EffectiveTo")) ? null : reader.GetDateTime(reader.GetOrdinal("EffectiveTo")),
                        CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                        CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetInt64(reader.GetOrdinal("CreatedBy")),
                        ModifiedDate = reader.IsDBNull(reader.GetOrdinal("ModifiedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ModifiedDate")),
                        ModifiedBy = reader.IsDBNull(reader.GetOrdinal("ModifiedBy")) ? null : reader.GetInt64(reader.GetOrdinal("ModifiedBy"))
                    });
                }
            }

            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            return new CommissionListResponse
            {
                Configs = configs,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = totalPages
            };
        }

        public async Task<CommissionConfigItem> GetCommissionConfigByIdAsync(long configId)
        {
            var query = $@"
                SELECT
                    c.c_config_id AS ConfigId,
                    c.c_config_name AS ConfigName,
                    c.c_config_type AS ConfigType,
                    c.c_catering_ownerid AS CateringOwnerId,
                    o.c_fullname AS CateringOwnerName,
                    o.c_businessname AS BusinessName,
                    c.c_commission_rate AS CommissionRate,
                    c.c_fixed_fee AS FixedFee,
                    c.c_min_order_value AS MinOrderValue,
                    c.c_max_order_value AS MaxOrderValue,
                    c.c_is_active AS IsActive,
                    c.c_effective_from AS EffectiveFrom,
                    c.c_effective_to AS EffectiveTo,
                    c.c_created_date AS CreatedDate,
                    c.c_created_by AS CreatedBy,
                    c.c_modified_date AS ModifiedDate,
                    c.c_modified_by AS ModifiedBy
                FROM {Table.SysCommissionConfig} c
                LEFT JOIN t_owner_register o ON c.c_catering_ownerid = o.c_ownerid
                WHERE c.c_config_id = @ConfigId";

            var parameters = new[]
            {
                new SqlParameter("@ConfigId", configId)
            };

            using (var reader = await _dbHelper.ExecuteReaderAsync(query, parameters))
            {
                if (await reader.ReadAsync())
                {
                    return new CommissionConfigItem
                    {
                        ConfigId = reader.GetInt64(reader.GetOrdinal("ConfigId")),
                        ConfigName = reader.GetString(reader.GetOrdinal("ConfigName")),
                        ConfigType = reader.GetString(reader.GetOrdinal("ConfigType")),
                        CateringOwnerId = reader.IsDBNull(reader.GetOrdinal("CateringOwnerId")) ? null : reader.GetInt64(reader.GetOrdinal("CateringOwnerId")),
                        CateringOwnerName = reader.IsDBNull(reader.GetOrdinal("CateringOwnerName")) ? null : reader.GetString(reader.GetOrdinal("CateringOwnerName")),
                        BusinessName = reader.IsDBNull(reader.GetOrdinal("BusinessName")) ? null : reader.GetString(reader.GetOrdinal("BusinessName")),
                        CommissionRate = reader.GetDecimal(reader.GetOrdinal("CommissionRate")),
                        FixedFee = reader.GetDecimal(reader.GetOrdinal("FixedFee")),
                        MinOrderValue = reader.IsDBNull(reader.GetOrdinal("MinOrderValue")) ? null : reader.GetDecimal(reader.GetOrdinal("MinOrderValue")),
                        MaxOrderValue = reader.IsDBNull(reader.GetOrdinal("MaxOrderValue")) ? null : reader.GetDecimal(reader.GetOrdinal("MaxOrderValue")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                        EffectiveFrom = reader.GetDateTime(reader.GetOrdinal("EffectiveFrom")),
                        EffectiveTo = reader.IsDBNull(reader.GetOrdinal("EffectiveTo")) ? null : reader.GetDateTime(reader.GetOrdinal("EffectiveTo")),
                        CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                        CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetInt64(reader.GetOrdinal("CreatedBy")),
                        ModifiedDate = reader.IsDBNull(reader.GetOrdinal("ModifiedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ModifiedDate")),
                        ModifiedBy = reader.IsDBNull(reader.GetOrdinal("ModifiedBy")) ? null : reader.GetInt64(reader.GetOrdinal("ModifiedBy"))
                    };
                }
            }

            return null;
        }

        public async Task<long> CreateCommissionConfigAsync(CreateCommissionConfigRequest request, long adminId)
        {
            // Validate config type specific requirements
            if (request.ConfigType == "CATERING_SPECIFIC" && !request.CateringOwnerId.HasValue)
            {
                throw new InvalidOperationException("Catering Owner ID is required for CATERING_SPECIFIC config type");
            }

            // Check for overlaps
            var hasOverlap = await HasCommissionConfigOverlapAsync(
                request.ConfigType,
                request.CateringOwnerId,
                request.EffectiveFrom,
                request.EffectiveTo
            );

            if (hasOverlap)
            {
                throw new InvalidOperationException("This commission configuration overlaps with an existing active configuration");
            }

            var query = $@"
                INSERT INTO {Table.SysCommissionConfig}
                (c_config_name, c_config_type, c_catering_ownerid, c_commission_rate, c_fixed_fee,
                 c_min_order_value, c_max_order_value, c_is_active, c_effective_from, c_effective_to,
                 c_created_by, c_created_date)
                OUTPUT INSERTED.c_config_id
                VALUES
                (@ConfigName, @ConfigType, @CateringOwnerId, @CommissionRate, @FixedFee,
                 @MinOrderValue, @MaxOrderValue, @IsActive, @EffectiveFrom, @EffectiveTo,
                 @CreatedBy, GETDATE())";

            var parameters = new[]
            {
                new SqlParameter("@ConfigName", request.ConfigName),
                new SqlParameter("@ConfigType", request.ConfigType),
                new SqlParameter("@CateringOwnerId", (object)request.CateringOwnerId ?? DBNull.Value),
                new SqlParameter("@CommissionRate", request.CommissionRate),
                new SqlParameter("@FixedFee", request.FixedFee),
                new SqlParameter("@MinOrderValue", (object)request.MinOrderValue ?? DBNull.Value),
                new SqlParameter("@MaxOrderValue", (object)request.MaxOrderValue ?? DBNull.Value),
                new SqlParameter("@IsActive", request.IsActive),
                new SqlParameter("@EffectiveFrom", request.EffectiveFrom),
                new SqlParameter("@EffectiveTo", (object)request.EffectiveTo ?? DBNull.Value),
                new SqlParameter("@CreatedBy", adminId)
            };

            var configId = await _dbHelper.ExecuteScalarAsync(query, parameters);
            return Convert.ToInt64(configId);
        }

        public async Task<bool> UpdateCommissionConfigAsync(UpdateCommissionConfigRequest request, long adminId)
        {
            // Get existing config to check type
            var existing = await GetCommissionConfigByIdAsync(request.ConfigId);
            if (existing == null)
            {
                throw new InvalidOperationException("Commission config not found");
            }

            // Check for overlaps (excluding current config)
            var hasOverlap = await HasCommissionConfigOverlapAsync(
                existing.ConfigType,
                existing.CateringOwnerId,
                request.EffectiveFrom,
                request.EffectiveTo,
                request.ConfigId
            );

            if (hasOverlap)
            {
                throw new InvalidOperationException("This commission configuration overlaps with an existing active configuration");
            }

            var query = $@"
                UPDATE {Table.SysCommissionConfig}
                SET
                    c_config_name = @ConfigName,
                    c_commission_rate = @CommissionRate,
                    c_fixed_fee = @FixedFee,
                    c_min_order_value = @MinOrderValue,
                    c_max_order_value = @MaxOrderValue,
                    c_is_active = @IsActive,
                    c_effective_from = @EffectiveFrom,
                    c_effective_to = @EffectiveTo,
                    c_modified_by = @ModifiedBy,
                    c_modified_date = GETDATE()
                WHERE c_config_id = @ConfigId";

            var parameters = new[]
            {
                new SqlParameter("@ConfigName", request.ConfigName),
                new SqlParameter("@CommissionRate", request.CommissionRate),
                new SqlParameter("@FixedFee", request.FixedFee),
                new SqlParameter("@MinOrderValue", (object)request.MinOrderValue ?? DBNull.Value),
                new SqlParameter("@MaxOrderValue", (object)request.MaxOrderValue ?? DBNull.Value),
                new SqlParameter("@IsActive", request.IsActive),
                new SqlParameter("@EffectiveFrom", request.EffectiveFrom),
                new SqlParameter("@EffectiveTo", (object)request.EffectiveTo ?? DBNull.Value),
                new SqlParameter("@ModifiedBy", adminId),
                new SqlParameter("@ConfigId", request.ConfigId)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteCommissionConfigAsync(long configId)
        {
            var query = $@"
                DELETE FROM {Table.SysCommissionConfig}
                WHERE c_config_id = @ConfigId";

            var parameters = new[]
            {
                new SqlParameter("@ConfigId", configId)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<ApplicableCommissionResult> GetApplicableCommissionRateAsync(long? cateringOwnerId, decimal orderValue, DateTime orderDate)
        {
            // Priority: CATERING_SPECIFIC > TIERED > GLOBAL

            // 1. Check for Catering-Specific config
            if (cateringOwnerId.HasValue)
            {
                var specificQuery = $@"
                    SELECT TOP 1
                        c_config_id AS ConfigId,
                        c_config_name AS ConfigName,
                        c_config_type AS ConfigType,
                        c_commission_rate AS CommissionRate,
                        c_fixed_fee AS FixedFee
                    FROM {Table.SysCommissionConfig}
                    WHERE c_config_type = 'CATERING_SPECIFIC'
                        AND c_catering_ownerid = @CateringOwnerId
                        AND c_is_active = 1
                        AND c_effective_from <= @OrderDate
                        AND (c_effective_to IS NULL OR c_effective_to >= @OrderDate)
                    ORDER BY c_effective_from DESC";

                var specificParams = new[]
                {
                    new SqlParameter("@CateringOwnerId", cateringOwnerId.Value),
                    new SqlParameter("@OrderDate", orderDate)
                };

                using (var reader = await _dbHelper.ExecuteReaderAsync(specificQuery, specificParams))
                {
                    if (await reader.ReadAsync())
                    {
                        var rate = reader.GetDecimal(reader.GetOrdinal("CommissionRate"));
                        var fixedFee = reader.GetDecimal(reader.GetOrdinal("FixedFee"));

                        return new ApplicableCommissionResult
                        {
                            ConfigId = reader.GetInt64(reader.GetOrdinal("ConfigId")),
                            ConfigName = reader.GetString(reader.GetOrdinal("ConfigName")),
                            ConfigType = reader.GetString(reader.GetOrdinal("ConfigType")),
                            CommissionRate = rate,
                            FixedFee = fixedFee,
                            CalculatedCommission = (orderValue * rate / 100) + fixedFee
                        };
                    }
                }
            }

            // 2. Check for Tiered config
            var tieredQuery = $@"
                SELECT TOP 1
                    c_config_id AS ConfigId,
                    c_config_name AS ConfigName,
                    c_config_type AS ConfigType,
                    c_commission_rate AS CommissionRate,
                    c_fixed_fee AS FixedFee
                FROM {Table.SysCommissionConfig}
                WHERE c_config_type = 'TIERED'
                    AND c_is_active = 1
                    AND c_effective_from <= @OrderDate
                    AND (c_effective_to IS NULL OR c_effective_to >= @OrderDate)
                    AND @OrderValue >= ISNULL(c_min_order_value, 0)
                    AND @OrderValue <= ISNULL(c_max_order_value, 999999999)
                ORDER BY c_effective_from DESC";

            var tieredParams = new[]
            {
                new SqlParameter("@OrderDate", orderDate),
                new SqlParameter("@OrderValue", orderValue)
            };

            using (var reader = await _dbHelper.ExecuteReaderAsync(tieredQuery, tieredParams))
            {
                if (await reader.ReadAsync())
                {
                    var rate = reader.GetDecimal(reader.GetOrdinal("CommissionRate"));
                    var fixedFee = reader.GetDecimal(reader.GetOrdinal("FixedFee"));

                    return new ApplicableCommissionResult
                    {
                        ConfigId = reader.GetInt64(reader.GetOrdinal("ConfigId")),
                        ConfigName = reader.GetString(reader.GetOrdinal("ConfigName")),
                        ConfigType = reader.GetString(reader.GetOrdinal("ConfigType")),
                        CommissionRate = rate,
                        FixedFee = fixedFee,
                        CalculatedCommission = (orderValue * rate / 100) + fixedFee
                    };
                }
            }

            // 3. Fall back to Global config
            var globalQuery = $@"
                SELECT TOP 1
                    c_config_id AS ConfigId,
                    c_config_name AS ConfigName,
                    c_config_type AS ConfigType,
                    c_commission_rate AS CommissionRate,
                    c_fixed_fee AS FixedFee
                FROM {Table.SysCommissionConfig}
                WHERE c_config_type = 'GLOBAL'
                    AND c_is_active = 1
                    AND c_effective_from <= @OrderDate
                    AND (c_effective_to IS NULL OR c_effective_to >= @OrderDate)
                ORDER BY c_effective_from DESC";

            var globalParams = new[]
            {
                new SqlParameter("@OrderDate", orderDate)
            };

            using (var reader = await _dbHelper.ExecuteReaderAsync(globalQuery, globalParams))
            {
                if (await reader.ReadAsync())
                {
                    var rate = reader.GetDecimal(reader.GetOrdinal("CommissionRate"));
                    var fixedFee = reader.GetDecimal(reader.GetOrdinal("FixedFee"));

                    return new ApplicableCommissionResult
                    {
                        ConfigId = reader.GetInt64(reader.GetOrdinal("ConfigId")),
                        ConfigName = reader.GetString(reader.GetOrdinal("ConfigName")),
                        ConfigType = reader.GetString(reader.GetOrdinal("ConfigType")),
                        CommissionRate = rate,
                        FixedFee = fixedFee,
                        CalculatedCommission = (orderValue * rate / 100) + fixedFee
                    };
                }
            }

            // No applicable config found
            return null;
        }

        public async Task<bool> HasCommissionConfigOverlapAsync(string configType, long? cateringOwnerId, DateTime effectiveFrom, DateTime? effectiveTo, long? excludeConfigId = null)
        {
            var conditions = new List<string>
            {
                "c_config_type = @ConfigType",
                "c_is_active = 1"
            };

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@ConfigType", configType),
                new SqlParameter("@EffectiveFrom", effectiveFrom)
            };

            if (configType == "CATERING_SPECIFIC" && cateringOwnerId.HasValue)
            {
                conditions.Add("c_catering_ownerid = @CateringOwnerId");
                parameters.Add(new SqlParameter("@CateringOwnerId", cateringOwnerId.Value));
            }

            if (excludeConfigId.HasValue)
            {
                conditions.Add("c_config_id != @ExcludeConfigId");
                parameters.Add(new SqlParameter("@ExcludeConfigId", excludeConfigId.Value));
            }

            // Check for date overlap
            if (effectiveTo.HasValue)
            {
                parameters.Add(new SqlParameter("@EffectiveTo", effectiveTo.Value));
                conditions.Add(@"
                    (
                        (c_effective_from <= @EffectiveTo AND (c_effective_to IS NULL OR c_effective_to >= @EffectiveFrom))
                    )");
            }
            else
            {
                conditions.Add(@"
                    (
                        (c_effective_from <= @EffectiveFrom) OR
                        (c_effective_to IS NULL OR c_effective_to >= @EffectiveFrom)
                    )");
            }

            var query = $@"
                SELECT COUNT(*)
                FROM {Table.SysCommissionConfig}
                WHERE {string.Join(" AND ", conditions)}";

            var count = Convert.ToInt32(
                await _dbHelper.ExecuteScalarAsync(query, parameters.ToArray())
            );

            return count > 0;
        }
    }
}
