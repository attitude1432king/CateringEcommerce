using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System.Data;

namespace CateringEcommerce.BAL.Configuration
{
    public class SystemSettingsProvider : ISystemSettingsProvider
    {
        private readonly string _connectionString;
        private ConcurrentDictionary<string, string> _settings = new();
        private HashSet<string> _sensitiveKeys = new();
        private bool _initialized = false;
        private readonly SemaphoreSlim _initLock = new(1, 1);

        public SystemSettingsProvider(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection not configured");
        }

        private async Task EnsureInitializedAsync()
        {
            if (_initialized) return;

            await _initLock.WaitAsync();
            try
            {
                if (_initialized) return;
                await LoadSettingsFromDatabaseAsync();
                _initialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        private void EnsureInitialized()
        {
            if (_initialized) return;
            EnsureInitializedAsync().GetAwaiter().GetResult();
        }

        private async Task LoadSettingsFromDatabaseAsync()
        {
            var newSettings = new ConcurrentDictionary<string, string>();
            var newSensitiveKeys = new HashSet<string>();
            string? encryptionKey = null;

            // First pass: load all settings and find the encryption key (stored as raw STRING)
            var rawSettings = new List<(string Key, string Value, string ValueType, bool IsSensitive)>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = $@"SELECT c_setting_key, c_setting_value, c_value_type, c_is_sensitive
                              FROM {Table.SysSettings}
                              WHERE c_is_active = 1";

                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var key = reader.GetString(0);
                            var value = reader.IsDBNull(1) ? "" : reader.GetString(1);
                            var valueType = reader.IsDBNull(2) ? "STRING" : reader.GetString(2);
                            var isSensitive = reader.GetBoolean(3);

                            rawSettings.Add((key, value, valueType, isSensitive));

                            if (key == "SYSTEM.ENCRYPTION_KEY")
                            {
                                encryptionKey = value;
                            }
                        }
                    }
                }
            }

            // Second pass: decrypt ENCRYPTED values and store all settings
            foreach (var (key, value, valueType, isSensitive) in rawSettings)
            {
                if (isSensitive)
                {
                    newSensitiveKeys.Add(key);
                }

                if (valueType == "ENCRYPTED" && !string.IsNullOrEmpty(encryptionKey) && key != "SYSTEM.ENCRYPTION_KEY")
                {
                    try
                    {
                        newSettings[key] = CryptoHelper.Decrypt(value, encryptionKey);
                    }
                    catch
                    {
                        newSettings[key] = value; // fallback to raw value
                    }
                }
                else
                {
                    newSettings[key] = value;
                }
            }

            _settings = newSettings;
            _sensitiveKeys = newSensitiveKeys;
        }

        public async Task RefreshAsync()
        {
            await _initLock.WaitAsync();
            try
            {
                await LoadSettingsFromDatabaseAsync();
                _initialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        public string GetString(string key, string defaultValue = "")
        {
            EnsureInitialized();
            return _settings.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            EnsureInitialized();
            if (_settings.TryGetValue(key, out var value) && int.TryParse(value, out var result))
                return result;
            return defaultValue;
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            EnsureInitialized();
            if (_settings.TryGetValue(key, out var value))
            {
                if (bool.TryParse(value, out var result)) return result;
                if (value == "1") return true;
                if (value == "0") return false;
            }
            return defaultValue;
        }

        public decimal GetDecimal(string key, decimal defaultValue = 0m)
        {
            EnsureInitialized();
            if (_settings.TryGetValue(key, out var value) && decimal.TryParse(value, out var result))
                return result;
            return defaultValue;
        }

        public Dictionary<string, string> GetPublicSettings()
        {
            EnsureInitialized();
            return _settings
                .Where(kvp => !_sensitiveKeys.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
