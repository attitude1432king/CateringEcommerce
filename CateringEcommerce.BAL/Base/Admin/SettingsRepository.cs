using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using CateringEcommerce.Domain.Models.Configuration;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;
using System.Text.RegularExpressions;

namespace CateringEcommerce.BAL.Base.Admin
{
    public partial class SettingsRepository : ISettingsRepository
    {
        private readonly IDatabaseHelper _dbHelper;
        private readonly string _encryptionKey;
        private readonly SmtpSettings _smtpSettings;

        public SettingsRepository(
            IDatabaseHelper dbHelper,
            IOptions<SecuritySettings> securityOptions,
            IOptions<SmtpSettings> smtpOptions)
        {
            _dbHelper = dbHelper;
            _encryptionKey = securityOptions?.Value?.EncryptionKey ?? string.Empty;
            _smtpSettings = smtpOptions?.Value ?? throw new ArgumentNullException(nameof(smtpOptions));
            if (string.IsNullOrEmpty(_encryptionKey))
                throw new InvalidOperationException("SYSTEM:ENCRYPTION_KEY is not configured in secure configuration");
        }

        // =============================================
        // SYSTEM SETTINGS METHODS
        // =============================================

        public async Task<SettingsListResponse> GetSettingsAsync(SettingsListRequest request)
        {
            var conditions = new List<string>();
            var baseParameters = new List<NpgsqlParameter>();

            if (!string.IsNullOrWhiteSpace(request.Category))
            {
                conditions.Add("s.c_category = @Category");
                baseParameters.Add(new NpgsqlParameter("@Category", request.Category));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                conditions.Add("(s.c_setting_key LIKE @SearchTerm OR s.c_display_name LIKE @SearchTerm OR s.c_description LIKE @SearchTerm)");
                baseParameters.Add(new NpgsqlParameter("@SearchTerm", $"%{request.SearchTerm}%"));
            }

            if (request.IsActive.HasValue)
            {
                conditions.Add("s.c_is_active = @IsActive");
                baseParameters.Add(new NpgsqlParameter("@IsActive", request.IsActive.Value));
            }

            string whereClause = conditions.Count > 0
                ? "WHERE " + string.Join(" AND ", conditions)
                : string.Empty;

            // COUNT QUERY
            var countQuery = $@"
                SELECT COUNT(*)
                FROM {Table.SysSettings} s
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
                "SettingKey" => "s.c_setting_key",
                "DisplayName" => "s.c_display_name",
                "Category" => "s.c_category",
                "DisplayOrder" => "s.c_display_order",
                _ => "s.c_display_order"
            };

            var sortOrder = request.SortOrder?.ToUpper() == "DESC" ? "DESC" : "ASC";

            var dataParameters = baseParameters
                .Select(CloneParameter)
                .ToList();

            dataParameters.Add(new NpgsqlParameter("@Offset", offset));
            dataParameters.Add(new NpgsqlParameter("@PageSize", request.PageSize));

            var dataQuery = $@"
                SELECT
                    s.c_setting_id AS SettingId,
                    s.c_setting_key AS SettingKey,
                    s.c_setting_value AS SettingValue,
                    s.c_category AS Category,
                    s.c_value_type AS ValueType,
                    s.c_display_name AS DisplayName,
                    s.c_description AS Description,
                    s.c_is_sensitive AS IsSensitive,
                    s.c_is_readonly AS IsReadOnly,
                    s.c_display_order AS DisplayOrder,
                    s.c_validation_regex AS ValidationRegex,
                    s.c_default_value AS DefaultValue,
                    s.c_is_active AS IsActive,
                    s.c_createddate AS CreatedDate,
                    s.c_createdby AS CreatedBy,
                    s.c_modifieddate AS ModifiedDate,
                    s.c_modifiedby AS ModifiedBy
                FROM {Table.SysSettings} s
                {whereClause}
                ORDER BY {sortColumn} {sortOrder}
                LIMIT @PageSize OFFSET @Offset";

            var table = await _dbHelper.ExecuteAsync(dataQuery, dataParameters.ToArray());

            var settings = new List<SystemSettingItem>();

            foreach (DataRow row in table.Rows)
            {
                var setting = new SystemSettingItem
                {
                    SettingId = row.Field<long>("SettingId"),
                    SettingKey = row.Field<string>("SettingKey"),
                    SettingValue = row.Field<string>("SettingValue"),
                    Category = row.Field<string>("Category"),
                    ValueType = row.Field<string>("ValueType"),
                    DisplayName = row.Field<string>("DisplayName"),
                    Description = row.Field<string>("Description"),
                    IsSensitive = row.Field<bool>("IsSensitive"),
                    IsReadOnly = row.Field<bool>("IsReadOnly"),
                    DisplayOrder = row.Field<int>("DisplayOrder"),
                    ValidationRegex = row.Field<string>("ValidationRegex"),
                    DefaultValue = row.Field<string>("DefaultValue"),
                    IsActive = row.Field<bool>("IsActive"),
                    CreatedDate = row.Field<DateTime>("CreatedDate"),
                    CreatedBy = row.Field<long?>("CreatedBy"),
                    ModifiedDate = row.Field<DateTime?>("ModifiedDate"),
                    ModifiedBy = row.Field<long?>("ModifiedBy")
                };

                // Mask sensitive values
                if (setting.IsSensitive)
                {
                    setting.SettingValue = "***SENSITIVE***";
                }
                else if (setting.ValueType == "ENCRYPTED")
                {
                    try
                    {
                        setting.SettingValue = CryptoHelper.Decrypt(setting.SettingValue, _encryptionKey);
                    }
                    catch
                    {
                        setting.SettingValue = "***DECRYPTION_ERROR***";
                    }
                }

                settings.Add(setting);
            }

            return new SettingsListResponse
            {
                Settings = settings,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };
        }

        public async Task<SystemSettingItem> GetSettingByKeyAsync(string settingKey)
        {
            var query = $@"
                SELECT
                    s.c_setting_id AS SettingId,
                    s.c_setting_key AS SettingKey,
                    s.c_setting_value AS SettingValue,
                    s.c_category AS Category,
                    s.c_value_type AS ValueType,
                    s.c_display_name AS DisplayName,
                    s.c_description AS Description,
                    s.c_is_sensitive AS IsSensitive,
                    s.c_is_readonly AS IsReadOnly,
                    s.c_display_order AS DisplayOrder,
                    s.c_validation_regex AS ValidationRegex,
                    s.c_default_value AS DefaultValue,
                    s.c_is_active AS IsActive,
                    s.c_createddate AS CreatedDate,
                    s.c_createdby AS CreatedBy,
                    s.c_modifieddate AS ModifiedDate,
                    s.c_modifiedby AS ModifiedBy
                FROM {Table.SysSettings} s
                WHERE s.c_setting_key = @SettingKey";

            var parameters = new[]
            {
                new NpgsqlParameter("@SettingKey", settingKey)
            };

            var table = await _dbHelper.ExecuteAsync(query, parameters.ToArray());

            var settings = new List<SystemSettingItem>();

            foreach (DataRow row in table.Rows)
            {
                var setting = new SystemSettingItem
                {
                    SettingId = row.Field<long>("SettingId"),
                    SettingKey = row.Field<string>("SettingKey"),
                    SettingValue = row.Field<string>("SettingValue"),
                    Category = row.Field<string>("Category"),
                    ValueType = row.Field<string>("ValueType"),
                    DisplayName = row.Field<string>("DisplayName"),
                    Description = row.Field<string>("Description"),
                    IsSensitive = row.Field<bool>("IsSensitive"),
                    IsReadOnly = row.Field<bool>("IsReadOnly"),
                    DisplayOrder = row.Field<int>("DisplayOrder"),
                    ValidationRegex = row.Field<string>("ValidationRegex"),
                    DefaultValue = row.Field<string>("DefaultValue"),
                    IsActive = row.Field<bool>("IsActive"),
                    CreatedDate = row.Field<DateTime>("CreatedDate"),
                    CreatedBy = row.Field<long?>("CreatedBy"),
                    ModifiedDate = row.Field<DateTime?>("ModifiedDate"),
                    ModifiedBy = row.Field<long?>("ModifiedBy")
                };

                // Decrypt encrypted values
                if (setting.ValueType == "ENCRYPTED" && !setting.IsSensitive)
                {
                    try
                    {
                        setting.SettingValue = CryptoHelper.Decrypt(setting.SettingValue, _encryptionKey);
                    }
                    catch
                    {
                        setting.SettingValue = "***DECRYPTION_ERROR***";
                    }
                }
                // Mask sensitive values
                else if (setting.IsSensitive)
                {
                    setting.SettingValue = "***SENSITIVE***";
                }

                return setting;
            }

            return null;
        }

        public async Task<SystemSettingItem> GetSettingByIdAsync(long settingId)
        {
            var query = $@"
                SELECT
                    s.c_setting_id AS SettingId,
                    s.c_setting_key AS SettingKey,
                    s.c_setting_value AS SettingValue,
                    s.c_category AS Category,
                    s.c_value_type AS ValueType,
                    s.c_display_name AS DisplayName,
                    s.c_description AS Description,
                    s.c_is_sensitive AS IsSensitive,
                    s.c_is_readonly AS IsReadOnly,
                    s.c_display_order AS DisplayOrder,
                    s.c_validation_regex AS ValidationRegex,
                    s.c_default_value AS DefaultValue,
                    s.c_is_active AS IsActive,
                    s.c_createddate AS CreatedDate,
                    s.c_createdby AS CreatedBy,
                    s.c_modifieddate AS ModifiedDate,
                    s.c_modifiedby AS ModifiedBy
                FROM {Table.SysSettings} s
                WHERE s.c_setting_id = @SettingId";

            var parameters = new[]
            {
                new NpgsqlParameter("@SettingId", settingId)
            };

            var table = await _dbHelper.ExecuteAsync(query, parameters.ToArray());

            var settings = new List<SystemSettingItem>();

            foreach (DataRow row in table.Rows)
            {
                var setting = new SystemSettingItem
                {
                    SettingId = row.Field<long>("SettingId"),
                    SettingKey = row.Field<string>("SettingKey"),
                    SettingValue = row.Field<string>("SettingValue"),
                    Category = row.Field<string>("Category"),
                    ValueType = row.Field<string>("ValueType"),
                    DisplayName = row.Field<string>("DisplayName"),
                    Description = row.Field<string>("Description"),
                    IsSensitive = row.Field<bool>("IsSensitive"),
                    IsReadOnly = row.Field<bool>("IsReadOnly"),
                    DisplayOrder = row.Field<int>("DisplayOrder"),
                    ValidationRegex = row.Field<string>("ValidationRegex"),
                    DefaultValue = row.Field<string>("DefaultValue"),
                    IsActive = row.Field<bool>("IsActive"),
                    CreatedDate = row.Field<DateTime>("CreatedDate"),
                    CreatedBy = row.Field<long?>("CreatedBy"),
                    ModifiedDate = row.Field<DateTime?>("ModifiedDate"),
                    ModifiedBy = row.Field<long?>("ModifiedBy")
                };

                // Decrypt encrypted values
                if (setting.ValueType == "ENCRYPTED" && !setting.IsSensitive)
                {
                    try
                    {
                        setting.SettingValue = CryptoHelper.Decrypt(setting.SettingValue, _encryptionKey);
                    }
                    catch
                    {
                        setting.SettingValue = "***DECRYPTION_ERROR***";
                    }
                }
                // Mask sensitive values
                else if (setting.IsSensitive)
                {
                    setting.SettingValue = "***SENSITIVE***";
                }

                return setting;
            }

            return null;
        }

        public async Task<bool> UpdateSettingAsync(UpdateSettingRequest request, long adminId, string adminName, string ipAddress)
        {
            // Get current setting to check if it's readonly and for history
            var currentSetting = await GetSettingByIdAsync(request.SettingId);
            if (currentSetting == null)
            {
                throw new InvalidOperationException("Setting not found");
            }

            if (currentSetting.IsReadOnly)
            {
                throw new InvalidOperationException("Cannot update read-only setting");
            }

            // Validate the new value
            var validationResult = await ValidateSettingValueAsync(request.SettingId, request.SettingValue);
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException($"Validation failed: {string.Join(", ", validationResult.Errors)}");
            }

            // Get the actual current value from database (not masked)
            var currentValueQuery = $@"
                SELECT c_setting_value, c_value_type
                FROM {Table.SysSettings}
                WHERE c_setting_id = @SettingId";

            string currentValue = null;
            string valueType = null;

            var currentValueTable = await _dbHelper.ExecuteAsync(currentValueQuery, new[] { new NpgsqlParameter("@SettingId", request.SettingId) });
            if (currentValueTable.Rows.Count > 0)
            {
                var row = currentValueTable.Rows[0];
                currentValue = row.Field<string>(0);
                valueType = row.Field<string>(1);
            }

            // Encrypt value if needed
            string valueToStore = request.SettingValue;
            if (valueType == "ENCRYPTED")
            {
                valueToStore = CryptoHelper.Encrypt(request.SettingValue, _encryptionKey);
            }

            // Update setting
            var updateQuery = $@"
                UPDATE {Table.SysSettings}
                SET
                    c_setting_value = @SettingValue,
                    c_modifieddate = NOW(),
                    c_modifiedby = @ModifiedBy
                WHERE c_setting_id = @SettingId";

            var updateParameters = new[]
            {
                new NpgsqlParameter("@SettingValue", valueToStore),
                new NpgsqlParameter("@ModifiedBy", adminId),
                new NpgsqlParameter("@SettingId", request.SettingId)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(updateQuery, updateParameters);

            // Log history
            if (rowsAffected > 0)
            {
                var historyQuery = $@"
                    INSERT INTO {Table.SysSettingsHistory}
                    (c_setting_id, c_setting_key, c_old_value, c_new_value, c_changed_by, c_changed_by_name, c_change_reason, c_ip_address)
                    VALUES
                    (@SettingId, @SettingKey, @OldValue, @NewValue, @ChangedBy, @ChangedByName, @ChangeReason, @IpAddress)";

                var historyParameters = new[]
                {
                    new NpgsqlParameter("@SettingId", request.SettingId),
                    new NpgsqlParameter("@SettingKey", currentSetting.SettingKey),
                    new NpgsqlParameter("@OldValue", currentValue ?? (object)DBNull.Value),
                    new NpgsqlParameter("@NewValue", valueToStore),
                    new NpgsqlParameter("@ChangedBy", adminId),
                    new NpgsqlParameter("@ChangedByName", adminName ?? (object)DBNull.Value),
                    new NpgsqlParameter("@ChangeReason", request.ChangeReason ?? (object)DBNull.Value),
                    new NpgsqlParameter("@IpAddress", ipAddress ?? (object)DBNull.Value)
                };

                await _dbHelper.ExecuteNonQueryAsync(historyQuery, historyParameters);
            }

            return rowsAffected > 0;
        }

        public async Task<SettingHistoryListResponse> GetSettingHistoryAsync(long settingId, int pageNumber = 1, int pageSize = 50)
        {
            // Count query
            var countQuery = $@"
                SELECT COUNT(*)
                FROM {Table.SysSettingsHistory}
                WHERE c_setting_id = @SettingId";

            var countParameters = new[]
            {
                new NpgsqlParameter("@SettingId", settingId)
            };

            var totalCount = Convert.ToInt32(
                await _dbHelper.ExecuteScalarAsync(countQuery, countParameters)
            );

            // Data query
            var offset = (pageNumber - 1) * pageSize;

            var dataQuery = $@"
                SELECT
                    h.c_history_id AS HistoryId,
                    h.c_setting_id AS SettingId,
                    h.c_setting_key AS SettingKey,
                    h.c_old_value AS OldValue,
                    h.c_new_value AS NewValue,
                    h.c_changed_by AS ChangedBy,
                    h.c_changed_by_name AS ChangedByName,
                    h.c_change_date AS ChangeDate,
                    h.c_change_reason AS ChangeReason,
                    h.c_ip_address AS IpAddress
                FROM {Table.SysSettingsHistory} h
                WHERE h.c_setting_id = @SettingId
                ORDER BY h.c_change_date DESC
                LIMIT @PageSize OFFSET @Offset";

            var dataParameters = new[]
            {
                new NpgsqlParameter("@SettingId", settingId),
                new NpgsqlParameter("@Offset", offset),
                new NpgsqlParameter("@PageSize", pageSize)
            };

            var history = new List<SettingHistoryItem>();

            using (var reader = await _dbHelper.ExecuteReaderAsync(dataQuery, dataParameters))
            {
                while (await reader.ReadAsync())
                {
                    history.Add(new SettingHistoryItem
                    {
                        HistoryId = reader.GetInt64(reader.GetOrdinal("HistoryId")),
                        SettingId = reader.GetInt64(reader.GetOrdinal("SettingId")),
                        SettingKey = reader.GetString(reader.GetOrdinal("SettingKey")),
                        OldValue = reader.IsDBNull(reader.GetOrdinal("OldValue")) ? null : reader.GetString(reader.GetOrdinal("OldValue")),
                        NewValue = reader.IsDBNull(reader.GetOrdinal("NewValue")) ? null : reader.GetString(reader.GetOrdinal("NewValue")),
                        ChangedBy = reader.GetInt64(reader.GetOrdinal("ChangedBy")),
                        ChangedByName = reader.IsDBNull(reader.GetOrdinal("ChangedByName")) ? null : reader.GetString(reader.GetOrdinal("ChangedByName")),
                        ChangeDate = reader.GetDateTime(reader.GetOrdinal("ChangeDate")),
                        ChangeReason = reader.IsDBNull(reader.GetOrdinal("ChangeReason")) ? null : reader.GetString(reader.GetOrdinal("ChangeReason")),
                        IpAddress = reader.IsDBNull(reader.GetOrdinal("IpAddress")) ? null : reader.GetString(reader.GetOrdinal("IpAddress"))
                    });
                }
            }

            return new SettingHistoryListResponse
            {
                History = history,
                TotalCount = totalCount
            };
        }

        public async Task<List<SystemSettingItem>> GetSettingsByCategoryAsync(string category)
        {
            var query = $@"
                SELECT
                    s.c_setting_id AS SettingId,
                    s.c_setting_key AS SettingKey,
                    s.c_setting_value AS SettingValue,
                    s.c_category AS Category,
                    s.c_value_type AS ValueType,
                    s.c_display_name AS DisplayName,
                    s.c_description AS Description,
                    s.c_is_sensitive AS IsSensitive,
                    s.c_is_readonly AS IsReadOnly,
                    s.c_display_order AS DisplayOrder,
                    s.c_validation_regex AS ValidationRegex,
                    s.c_default_value AS DefaultValue,
                    s.c_is_active AS IsActive,
                    s.c_createddate AS CreatedDate,
                    s.c_createdby AS CreatedBy,
                    s.c_modifieddate AS ModifiedDate,
                    s.c_modifiedby AS ModifiedBy
                FROM {Table.SysSettings} s
                WHERE s.c_category = @Category AND s.c_is_active = TRUE
                ORDER BY s.c_display_order ASC";

            var parameters = new[]
            {
                new NpgsqlParameter("@Category", category)
            };

            var settings = new List<SystemSettingItem>();

            var settingsTable = await _dbHelper.ExecuteAsync(query, parameters);
            foreach (DataRow row in settingsTable.Rows)
            {
                var setting = new SystemSettingItem
                {
                    SettingId = row.Field<long>("SettingId"),
                    SettingKey = row.Field<string>("SettingKey"),
                    SettingValue = row.Field<string>("SettingValue"),
                    Category = row.Field<string>("Category"),
                    ValueType = row.Field<string>("ValueType"),
                    DisplayName = row.Field<string>("DisplayName"),
                    Description = row.IsNull("Description") ? null : row.Field<string>("Description"),
                    IsSensitive = row.Field<bool>("IsSensitive"),
                    IsReadOnly = row.Field<bool>("IsReadOnly"),
                    DisplayOrder = row.Field<int>("DisplayOrder"),
                    ValidationRegex = row.IsNull("ValidationRegex") ? null : row.Field<string>("ValidationRegex"),
                    DefaultValue = row.IsNull("DefaultValue") ? null : row.Field<string>("DefaultValue"),
                    IsActive = row.Field<bool>("IsActive"),
                    CreatedDate = row.Field<DateTime>("CreatedDate"),
                    CreatedBy = row.IsNull("CreatedBy") ? null : row.Field<long>("CreatedBy"),
                    ModifiedDate = row.IsNull("ModifiedDate") ? null : row.Field<DateTime>("ModifiedDate"),
                    ModifiedBy = row.IsNull("ModifiedBy") ? null : row.Field<long>("ModifiedBy")
                };

                // Decrypt encrypted values
                if (setting.ValueType == "ENCRYPTED" && !setting.IsSensitive)
                {
                    try
                    {
                        setting.SettingValue = CryptoHelper.Decrypt(setting.SettingValue, _encryptionKey);
                    }
                    catch
                    {
                        setting.SettingValue = "***DECRYPTION_ERROR***";
                    }
                }
                // Mask sensitive values
                else if (setting.IsSensitive)
                {
                    setting.SettingValue = "***SENSITIVE***";
                }

                settings.Add(setting);
            }

            return settings;
        }

        public async Task<SettingValidationResult> ValidateSettingValueAsync(long settingId, string value)
        {
            var setting = await GetSettingByIdAsync(settingId);
            if (setting == null)
            {
                return new SettingValidationResult
                {
                    IsValid = false,
                    Errors = new List<string> { "Setting not found" }
                };
            }

            var errors = new List<string>();

            // Check if value is empty
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add("Value cannot be empty");
            }

            // Validate based on value type
            switch (setting.ValueType)
            {
                case "NUMBER":
                    if (!decimal.TryParse(value, out _))
                    {
                        errors.Add("Value must be a valid number");
                    }
                    break;

                case "BOOLEAN":
                    if (!bool.TryParse(value, out _) && value != "0" && value != "1")
                    {
                        errors.Add("Value must be true or false");
                    }
                    break;

                case "JSON":
                    // Basic JSON validation
                    if (!value.TrimStart().StartsWith("{") && !value.TrimStart().StartsWith("["))
                    {
                        errors.Add("Value must be valid JSON");
                    }
                    break;
            }

            // Validate against regex if provided
            if (!string.IsNullOrWhiteSpace(setting.ValidationRegex))
            {
                try
                {
                    if (!Regex.IsMatch(value, setting.ValidationRegex))
                    {
                        errors.Add("Value does not match the required format");
                    }
                }
                catch
                {
                    errors.Add("Invalid validation regex configured");
                }
            }

            return new SettingValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors
            };
        }

        public async Task<Dictionary<string, string>> GetAllSettingsKeyValueAsync()
        {
            var query = $@"
                SELECT c_setting_key, c_setting_value, c_value_type
                FROM {Table.SysSettings}
                WHERE c_is_active = TRUE";

            var settings = new Dictionary<string, string>();

            using (var reader = await _dbHelper.ExecuteReaderAsync(query, Array.Empty<NpgsqlParameter>()))
            {
                while (await reader.ReadAsync())
                {
                    var key = reader.GetString(0);
                    var value = reader.GetString(1);
                    var valueType = reader.GetString(2);

                    // Decrypt if encrypted
                    if (valueType == "ENCRYPTED")
                    {
                        try
                        {
                            value = CryptoHelper.Decrypt(value, _encryptionKey);
                        }
                        catch
                        {
                            // Skip decryption errors
                            continue;
                        }
                    }

                    settings[key] = value;
                }
            }

            return settings;
        }

        public async Task<List<SettingsExportItem>> ExportSettingsAsync(SettingsExportRequest request)
        {
            var conditions = new List<string>();
            var parameters = new List<NpgsqlParameter>();

            if (!string.IsNullOrWhiteSpace(request.Category))
            {
                conditions.Add("c_category = @Category");
                parameters.Add(new NpgsqlParameter("@Category", request.Category));
            }

            if (!request.IncludeSensitive)
            {
                conditions.Add("c_is_sensitive = FALSE");
            }

            string whereClause = conditions.Count > 0
                ? "WHERE " + string.Join(" AND ", conditions)
                : string.Empty;

            var query = $@"
                SELECT
                    c_setting_key AS SettingKey,
                    c_setting_value AS SettingValue,
                    c_category AS Category,
                    c_value_type AS ValueType,
                    c_display_name AS DisplayName,
                    c_description AS Description
                FROM {Table.SysSettings}
                {whereClause}
                ORDER BY c_category, c_display_order";

            var exportItems = new List<SettingsExportItem>();

            using (var reader = await _dbHelper.ExecuteReaderAsync(query, parameters.ToArray()))
            {
                while (await reader.ReadAsync())
                {
                    var valueType = reader.GetString(reader.GetOrdinal("ValueType"));
                    var value = reader.GetString(reader.GetOrdinal("SettingValue"));

                    // Decrypt encrypted values
                    if (valueType == "ENCRYPTED")
                    {
                        try
                        {
                            value = CryptoHelper.Decrypt(value, _encryptionKey);
                        }
                        catch
                        {
                            value = "***DECRYPTION_ERROR***";
                        }
                    }

                    exportItems.Add(new SettingsExportItem
                    {
                        SettingKey = reader.GetString(reader.GetOrdinal("SettingKey")),
                        SettingValue = value,
                        Category = reader.GetString(reader.GetOrdinal("Category")),
                        ValueType = valueType,
                        DisplayName = reader.GetString(reader.GetOrdinal("DisplayName")),
                        Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description"))
                    });
                }
            }

            return exportItems;
        }

        public async Task<SettingsImportResult> ImportSettingsAsync(SettingsImportRequest request, long adminId)
        {
            var result = new SettingsImportResult
            {
                TotalSettings = request.Settings.Count,
                Errors = new List<string>()
            };

            foreach (var setting in request.Settings)
            {
                try
                {
                    // Check if setting exists
                    var existing = await GetSettingByKeyAsync(setting.SettingKey);

                    if (existing != null && !request.OverwriteExisting)
                    {
                        result.SkippedCount++;
                        continue;
                    }

                    if (existing == null)
                    {
                        result.Errors.Add($"Setting '{setting.SettingKey}' not found in database");
                        result.ErrorCount++;
                        continue;
                    }

                    // Update setting
                    var updateRequest = new UpdateSettingRequest
                    {
                        SettingId = existing.SettingId,
                        SettingValue = setting.SettingValue,
                        ChangeReason = "Imported from file"
                    };

                    await UpdateSettingAsync(updateRequest, adminId, "System Import", "127.0.0.1");
                    result.ImportedCount++;
                }
                catch (Exception ex)
                {
                    result.ErrorCount++;
                    result.Errors.Add($"Error importing '{setting.SettingKey}': {ex.Message}");
                }
            }

            return result;
        }

        // =============================================
        // HELPER METHODS
        // =============================================

        private NpgsqlParameter CloneParameter(NpgsqlParameter param)
        {
            return new NpgsqlParameter(param.ParameterName, param.Value ?? DBNull.Value)
            {
                NpgsqlDbType = param.NpgsqlDbType,
                Size = param.Size,
                Direction = param.Direction
            };
        }
    }
}


