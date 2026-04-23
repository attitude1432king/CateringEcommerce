using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Supervisor;
using CateringEcommerce.Domain.Models.Supervisor;
using Microsoft.CSharp.RuntimeBinder;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data;
using System.Dynamic;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace CateringEcommerce.BAL.Base.Supervisor
{
    public class SupervisorRepository : ISupervisorRepository
    {
        private readonly IDatabaseHelper _dbHelper;

        public SupervisorRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // =============================================
        // BASIC CRUD
        // =============================================

        public async Task<SupervisorModel> GetSupervisorByIdAsync(long supervisorId)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@SupervisorId", supervisorId)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<SupervisorModel>(
                "sp_GetSupervisorById", parameters);
        }

        public async Task<SupervisorModel> GetSupervisorByEmailAsync(string email)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@Email", email)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<SupervisorModel>(
                "sp_GetSupervisorByEmail", parameters);
        }

        public async Task<SupervisorModel> GetSupervisorByPhoneAsync(string phone)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@Phone", phone)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<SupervisorModel>(
                "sp_GetSupervisorByPhone", parameters);
        }

        public async Task<SupervisorLoginInfo> GetSupervisorForLoginAsync(string identifier)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@Identifier", identifier)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<SupervisorLoginInfo>(
                "sp_GetSupervisorForLogin", parameters);
        }

        public async Task UpdateLastLoginAsync(long supervisorId)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@SupervisorId", supervisorId)
            };

            await _dbHelper.ExecuteNonQueryAsync(
                "sp_UpdateSupervisorLastLogin", parameters, CommandType.StoredProcedure);
        }

        public async Task<List<SupervisorModel>> GetAllSupervisorsAsync(SupervisorType? type = null, string status = null)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@SupervisorType", (object)type?.ToString() ?? DBNull.Value),
                new NpgsqlParameter("@Status", (object)status ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<SupervisorModel>>(
                "sp_GetAllSupervisors", parameters);
        }

        public async Task<bool> UpdateSupervisorAsync(long supervisorId, UpdateSupervisorDto updates)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@SupervisorId", supervisorId),
                new NpgsqlParameter("@FirstName", (object)updates.FirstName ?? DBNull.Value),
                new NpgsqlParameter("@LastName", (object)updates.LastName ?? DBNull.Value),
                new NpgsqlParameter("@Email", (object)updates.Email ?? DBNull.Value),
                new NpgsqlParameter("@Phone", (object)updates.Phone ?? DBNull.Value),
                new NpgsqlParameter("@Address", (object)updates.Address ?? DBNull.Value),
                new NpgsqlParameter("@ZoneId", (object)updates.ZoneId ?? DBNull.Value),
                new NpgsqlParameter("@EmergencyContactName", (object)updates.EmergencyContactName ?? DBNull.Value),
                new NpgsqlParameter("@EmergencyContactPhone", (object)updates.EmergencyContactPhone ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_UpdateSupervisor", parameters);
        }

        public async Task<bool> DeleteSupervisorAsync(long supervisorId)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@SupervisorId", supervisorId)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_DeleteSupervisor", parameters);
        }

        // =============================================
        // AUTHORITY MANAGEMENT
        // =============================================

        public async Task<AuthorityCheckResult> CheckSupervisorAuthorityAsync(long supervisorId, string actionType)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@SupervisorId", supervisorId),
                new NpgsqlParameter("@ActionType", actionType),
                new NpgsqlParameter("@CanPerformAction", NpgsqlDbType.Boolean) { Direction = ParameterDirection.Output },
                new NpgsqlParameter("@RequiresApproval", NpgsqlDbType.Boolean) { Direction = ParameterDirection.Output },
                new NpgsqlParameter("@CurrentAuthority", NpgsqlDbType.Varchar, 20) { Direction = ParameterDirection.Output }
            };

            await _dbHelper.ExecuteStoredProcedureAsync<object>("sp_CheckSupervisorAuthority", parameters);

            var canPerform = parameters[2].Value != DBNull.Value && Convert.ToBoolean(parameters[2].Value);
            var requiresApproval = parameters[3].Value != DBNull.Value && Convert.ToBoolean(parameters[3].Value);
            var authority = parameters[4].Value?.ToString();

            return new AuthorityCheckResult
            {
                CanPerformAction = canPerform,
                RequiresApproval = requiresApproval,
                CurrentAuthority = Enum.TryParse<AuthorityLevel>(authority, out var level) ? level : AuthorityLevel.BASIC,
                Message = canPerform ? "Authorized" : requiresApproval ? "Requires approval" : "Not authorized"
            };
        }

        public async Task<bool> UpdateAuthorityLevelAsync(long supervisorId, AuthorityLevel newLevel, long updatedBy, string reason)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@SupervisorId", supervisorId),
                new NpgsqlParameter("@NewAuthorityLevel", newLevel.ToString()),
                new NpgsqlParameter("@UpdatedBy", updatedBy),
                new NpgsqlParameter("@Reason", reason)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_UpdateAuthorityLevel", parameters);
        }

        public async Task<bool> GrantPermissionAsync(long supervisorId, string permissionType, long grantedBy)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@SupervisorId", supervisorId),
                new NpgsqlParameter("@PermissionType", permissionType),
                new NpgsqlParameter("@GrantedBy", grantedBy)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_GrantPermission", parameters);
        }

        public async Task<bool> RevokePermissionAsync(long supervisorId, string permissionType, long revokedBy)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@SupervisorId", supervisorId),
                new NpgsqlParameter("@PermissionType", permissionType),
                new NpgsqlParameter("@RevokedBy", revokedBy)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_RevokePermission", parameters);
        }

        // =============================================
        // STATUS MANAGEMENT
        // =============================================

        public async Task<bool> UpdateStatusAsync(long supervisorId, string newStatus, long updatedBy, string notes)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@SupervisorId", supervisorId),
                new NpgsqlParameter("@NewStatus", newStatus),
                new NpgsqlParameter("@UpdatedBy", updatedBy),
                new NpgsqlParameter("@Notes", (object)notes ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_UpdateSupervisorStatus", parameters);
        }

        public async Task<bool> ActivateSupervisorAsync(long supervisorId, long activatedBy)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@SupervisorId", supervisorId),
                new NpgsqlParameter("@ActivatedBy", activatedBy)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_ActivateSupervisor", parameters);
        }

        public async Task<bool> SuspendSupervisorAsync(long supervisorId, long suspendedBy, string reason)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@SupervisorId", supervisorId),
                new NpgsqlParameter("@SuspendedBy", suspendedBy),
                new NpgsqlParameter("@Reason", reason)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_SuspendSupervisor", parameters);
        }

        public async Task<bool> TerminateSupervisorAsync(long supervisorId, long terminatedBy, string reason)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@SupervisorId", supervisorId),
                new NpgsqlParameter("@TerminatedBy", terminatedBy),
                new NpgsqlParameter("@Reason", reason)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_TerminateSupervisor", parameters);
        }

        // =============================================
        // DASHBOARD & ANALYTICS
        // =============================================

        public async Task<SupervisorDashboardDto> GetSupervisorDashboardAsync(long supervisorId)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@SupervisorId", supervisorId)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<SupervisorDashboardDto>(
                "sp_GetSupervisorDashboard", parameters);
        }

        public async Task<List<SupervisorPerformanceDto>> GetSupervisorPerformanceReportAsync(DateTime fromDate, DateTime toDate)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@FromDate", fromDate),
                new NpgsqlParameter("@ToDate", toDate)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<SupervisorPerformanceDto>>(
                "sp_GetSupervisorPerformanceReport", parameters);
        }

        public async Task<SupervisorStatisticsDto> GetSupervisorStatisticsAsync(SupervisorType? type = null)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@SupervisorType", (object)type?.ToString() ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<SupervisorStatisticsDto>(
                "sp_GetSupervisorStatistics", parameters);
        }

        // =============================================
        // AVAILABILITY & SCHEDULING
        // =============================================

        public async Task<bool> UpdateAvailabilityAsync(long supervisorId, List<AvailabilitySlot> availability)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@SupervisorId", supervisorId),
                new NpgsqlParameter("@AvailabilityData", JsonSerializer.Serialize(availability))
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_UpdateSupervisorAvailability", parameters);
        }

        public async Task<List<AvailabilitySlot>> GetAvailabilityAsync(long supervisorId, DateTime date)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@SupervisorId", supervisorId),
                new NpgsqlParameter("@Date", date)
            };

            var result = await _dbHelper.ExecuteStoredProcedureAsync<dynamic>("sp_GetSupervisorAvailability", parameters);

            if (result?.AvailabilityData != null)
            {
                return JsonSerializer.Deserialize<List<AvailabilitySlot>>(result.AvailabilityData.ToString());
            }

            return new List<AvailabilitySlot>();
        }

        public async Task<List<SupervisorModel>> GetAvailableSupervisorsAsync(DateTime eventDate, string eventType, long? zoneId = null)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@EventDate", eventDate),
                new NpgsqlParameter("@EventType", eventType),
                new NpgsqlParameter("@ZoneId", (object)zoneId ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<SupervisorModel>>(
                "sp_GetAvailableSupervisors", parameters);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@Email", email)
            };

            var result = await _dbHelper.ExecuteStoredProcedureAsync<ExistsCheckResult>(
                "sp_CheckEmailExists", parameters);

            return result?.Exists ?? false;
        }

        public async Task<bool> PhoneExistsAsync(string phone)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@Phone", phone)
            };

            var result = await _dbHelper.ExecuteStoredProcedureAsync<ExistsCheckResult>(
                "sp_CheckPhoneExists", parameters);

            return result?.Exists ?? false;
        }

        private class ExistsCheckResult
        {
            public bool Exists { get; set; }
        }

        /// <summary>
        /// Extracts boolean value from various return types (DataTable, Dictionary, dynamic object, etc.)
        /// </summary>
        private static bool ExtractBooleanValue(dynamic result, string columnName, bool defaultValue = false)
        {
            if (result == null)
                return defaultValue;

            try
            {
                // Handle DataTable result
                if (result is System.Data.DataTable dataTable && dataTable.Rows.Count > 0)
                {
                    var row = dataTable.Rows[0];
                    if (dataTable.Columns.Contains(columnName))
                    {
                        var value = row[columnName];
                        return value != DBNull.Value && Convert.ToBoolean(value);
                    }
                }

                // Handle List<DataRow> or IEnumerable
                if (result is System.Collections.IEnumerable enumerable && !(result is string))
                {
                    var enumerator = enumerable.GetEnumerator();
                    if (enumerator.MoveNext())
                    {
                        var firstItem = enumerator.Current;

                        if (firstItem is System.Data.DataRow dataRow)
                        {
                            if (dataRow.Table.Columns.Contains(columnName))
                            {
                                var value = dataRow[columnName];
                                return value != DBNull.Value && Convert.ToBoolean(value);
                            }
                        }
                    }
                }

                // Handle Dictionary result
                if (result is IDictionary<string, object> dict)
                {
                    if (dict.TryGetValue(columnName, out var value))
                        return Convert.ToBoolean(value);

                    var key = dict.Keys.FirstOrDefault(k => 
                        k.Equals(columnName, StringComparison.OrdinalIgnoreCase));

                    if (key != null && dict.TryGetValue(key, out var caseInsensitiveValue))
                        return Convert.ToBoolean(caseInsensitiveValue);
                }

                // Handle dynamic object with properties using reflection
                var objType = result.GetType();

                // Try exact case match first
                var property = objType.GetProperty(columnName, 
                    System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public);

                if (property != null)
                {
                    var value = property.GetValue(result);
                    return Convert.ToBoolean(value ?? defaultValue);
                }

                // If nothing matched, return default value
                return defaultValue;
            }
            catch (RuntimeBinderException)
            {
                return defaultValue;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error extracting boolean from {columnName}: {ex.Message}");
                return defaultValue;
            }
        }

        // =============================================
        // SEARCH & FILTERING
        // =============================================

        public async Task<List<SupervisorModel>> SearchSupervisorsAsync(SupervisorSearchDto filters)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@Name", (object)filters.Name ?? DBNull.Value),
                new NpgsqlParameter("@Email", (object)filters.Email ?? DBNull.Value),
                new NpgsqlParameter("@Phone", (object)filters.Phone ?? DBNull.Value),
                new NpgsqlParameter("@SupervisorType", (object)filters.SupervisorType?.ToString() ?? DBNull.Value),
                new NpgsqlParameter("@AuthorityLevel", (object)filters.AuthorityLevel?.ToString() ?? DBNull.Value),
                new NpgsqlParameter("@Status", (object)filters.Status ?? DBNull.Value),
                new NpgsqlParameter("@ZoneId", (object)filters.ZoneId ?? DBNull.Value),
                new NpgsqlParameter("@IsActive", (object)filters.IsActive ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<SupervisorModel>>(
                "sp_SearchSupervisors", parameters);
        }

        public async Task<List<SupervisorModel>> GetSupervisorsByZoneAsync(long zoneId)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@ZoneId", zoneId)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<SupervisorModel>>(
                "sp_GetSupervisorsByZone", parameters);
        }

        public async Task<List<SupervisorModel>> GetSupervisorsByAuthorityAsync(AuthorityLevel authorityLevel)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@AuthorityLevel", authorityLevel.ToString())
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<SupervisorModel>>(
                "sp_GetSupervisorsByAuthority", parameters);
        }
    }
}
