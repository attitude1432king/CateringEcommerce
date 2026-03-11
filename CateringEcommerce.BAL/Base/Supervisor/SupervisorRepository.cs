using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using System.Threading.Tasks;
using System.Reflection;
using System.Dynamic;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Supervisor;
using CateringEcommerce.Domain.Models.Supervisor;
using Microsoft.CSharp.RuntimeBinder;

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
                new SqlParameter("@SupervisorId", supervisorId)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<SupervisorModel>(
                "sp_GetSupervisorById", parameters);
        }

        public async Task<SupervisorModel> GetSupervisorByEmailAsync(string email)
        {
            var parameters = new[]
            {
                new SqlParameter("@Email", email)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<SupervisorModel>(
                "sp_GetSupervisorByEmail", parameters);
        }

        public async Task<SupervisorModel> GetSupervisorByPhoneAsync(string phone)
        {
            var parameters = new[]
            {
                new SqlParameter("@Phone", phone)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<SupervisorModel>(
                "sp_GetSupervisorByPhone", parameters);
        }

        public async Task<List<SupervisorModel>> GetAllSupervisorsAsync(SupervisorType? type = null, string status = null)
        {
            var parameters = new[]
            {
                new SqlParameter("@SupervisorType", (object)type?.ToString() ?? DBNull.Value),
                new SqlParameter("@Status", (object)status ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<SupervisorModel>>(
                "sp_GetAllSupervisors", parameters);
        }

        public async Task<bool> UpdateSupervisorAsync(long supervisorId, UpdateSupervisorDto updates)
        {
            var parameters = new[]
            {
                new SqlParameter("@SupervisorId", supervisorId),
                new SqlParameter("@FirstName", (object)updates.FirstName ?? DBNull.Value),
                new SqlParameter("@LastName", (object)updates.LastName ?? DBNull.Value),
                new SqlParameter("@Email", (object)updates.Email ?? DBNull.Value),
                new SqlParameter("@Phone", (object)updates.Phone ?? DBNull.Value),
                new SqlParameter("@Address", (object)updates.Address ?? DBNull.Value),
                new SqlParameter("@ZoneId", (object)updates.ZoneId ?? DBNull.Value),
                new SqlParameter("@EmergencyContactName", (object)updates.EmergencyContactName ?? DBNull.Value),
                new SqlParameter("@EmergencyContactPhone", (object)updates.EmergencyContactPhone ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_UpdateSupervisor", parameters);
        }

        public async Task<bool> DeleteSupervisorAsync(long supervisorId)
        {
            var parameters = new[]
            {
                new SqlParameter("@SupervisorId", supervisorId)
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
                new SqlParameter("@SupervisorId", supervisorId),
                new SqlParameter("@ActionType", actionType),
                new SqlParameter("@CanPerformAction", SqlDbType.Bit) { Direction = ParameterDirection.Output },
                new SqlParameter("@RequiresApproval", SqlDbType.Bit) { Direction = ParameterDirection.Output },
                new SqlParameter("@CurrentAuthority", SqlDbType.VarChar, 20) { Direction = ParameterDirection.Output }
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
                new SqlParameter("@SupervisorId", supervisorId),
                new SqlParameter("@NewAuthorityLevel", newLevel.ToString()),
                new SqlParameter("@UpdatedBy", updatedBy),
                new SqlParameter("@Reason", reason)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_UpdateAuthorityLevel", parameters);
        }

        public async Task<bool> GrantPermissionAsync(long supervisorId, string permissionType, long grantedBy)
        {
            var parameters = new[]
            {
                new SqlParameter("@SupervisorId", supervisorId),
                new SqlParameter("@PermissionType", permissionType),
                new SqlParameter("@GrantedBy", grantedBy)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_GrantPermission", parameters);
        }

        public async Task<bool> RevokePermissionAsync(long supervisorId, string permissionType, long revokedBy)
        {
            var parameters = new[]
            {
                new SqlParameter("@SupervisorId", supervisorId),
                new SqlParameter("@PermissionType", permissionType),
                new SqlParameter("@RevokedBy", revokedBy)
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
                new SqlParameter("@SupervisorId", supervisorId),
                new SqlParameter("@NewStatus", newStatus),
                new SqlParameter("@UpdatedBy", updatedBy),
                new SqlParameter("@Notes", (object)notes ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_UpdateSupervisorStatus", parameters);
        }

        public async Task<bool> ActivateSupervisorAsync(long supervisorId, long activatedBy)
        {
            var parameters = new[]
            {
                new SqlParameter("@SupervisorId", supervisorId),
                new SqlParameter("@ActivatedBy", activatedBy)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_ActivateSupervisor", parameters);
        }

        public async Task<bool> SuspendSupervisorAsync(long supervisorId, long suspendedBy, string reason)
        {
            var parameters = new[]
            {
                new SqlParameter("@SupervisorId", supervisorId),
                new SqlParameter("@SuspendedBy", suspendedBy),
                new SqlParameter("@Reason", reason)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_SuspendSupervisor", parameters);
        }

        public async Task<bool> TerminateSupervisorAsync(long supervisorId, long terminatedBy, string reason)
        {
            var parameters = new[]
            {
                new SqlParameter("@SupervisorId", supervisorId),
                new SqlParameter("@TerminatedBy", terminatedBy),
                new SqlParameter("@Reason", reason)
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
                new SqlParameter("@SupervisorId", supervisorId)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<SupervisorDashboardDto>(
                "sp_GetSupervisorDashboard", parameters);
        }

        public async Task<List<SupervisorPerformanceDto>> GetSupervisorPerformanceReportAsync(DateTime fromDate, DateTime toDate)
        {
            var parameters = new[]
            {
                new SqlParameter("@FromDate", fromDate),
                new SqlParameter("@ToDate", toDate)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<SupervisorPerformanceDto>>(
                "sp_GetSupervisorPerformanceReport", parameters);
        }

        public async Task<SupervisorStatisticsDto> GetSupervisorStatisticsAsync(SupervisorType? type = null)
        {
            var parameters = new[]
            {
                new SqlParameter("@SupervisorType", (object)type?.ToString() ?? DBNull.Value)
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
                new SqlParameter("@SupervisorId", supervisorId),
                new SqlParameter("@AvailabilityData", JsonSerializer.Serialize(availability))
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_UpdateSupervisorAvailability", parameters);
        }

        public async Task<List<AvailabilitySlot>> GetAvailabilityAsync(long supervisorId, DateTime date)
        {
            var parameters = new[]
            {
                new SqlParameter("@SupervisorId", supervisorId),
                new SqlParameter("@Date", date)
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
                new SqlParameter("@EventDate", eventDate),
                new SqlParameter("@EventType", eventType),
                new SqlParameter("@ZoneId", (object)zoneId ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<SupervisorModel>>(
                "sp_GetAvailableSupervisors", parameters);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            var parameters = new[]
            {
                new SqlParameter("@Email", email)
            };

            var result = await _dbHelper.ExecuteStoredProcedureAsync<DataTable>(
                "sp_CheckEmailExists", parameters);

            return ExtractBooleanValue(result, "Exists", false);
        }

        public async Task<bool> PhoneExistsAsync(string phone)
        {
            var parameters = new[]
            {
                new SqlParameter("@Phone", phone)
            };

            var result = await _dbHelper.ExecuteStoredProcedureAsync<dynamic>(
                "sp_CheckPhoneExists", parameters);

            return ExtractBooleanValue(result, "Exists", false);
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
                new SqlParameter("@Name", (object)filters.Name ?? DBNull.Value),
                new SqlParameter("@Email", (object)filters.Email ?? DBNull.Value),
                new SqlParameter("@Phone", (object)filters.Phone ?? DBNull.Value),
                new SqlParameter("@SupervisorType", (object)filters.SupervisorType?.ToString() ?? DBNull.Value),
                new SqlParameter("@AuthorityLevel", (object)filters.AuthorityLevel?.ToString() ?? DBNull.Value),
                new SqlParameter("@Status", (object)filters.Status ?? DBNull.Value),
                new SqlParameter("@ZoneId", (object)filters.ZoneId ?? DBNull.Value),
                new SqlParameter("@IsActive", (object)filters.IsActive ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<SupervisorModel>>(
                "sp_SearchSupervisors", parameters);
        }

        public async Task<List<SupervisorModel>> GetSupervisorsByZoneAsync(long zoneId)
        {
            var parameters = new[]
            {
                new SqlParameter("@ZoneId", zoneId)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<SupervisorModel>>(
                "sp_GetSupervisorsByZone", parameters);
        }

        public async Task<List<SupervisorModel>> GetSupervisorsByAuthorityAsync(AuthorityLevel authorityLevel)
        {
            var parameters = new[]
            {
                new SqlParameter("@AuthorityLevel", authorityLevel.ToString())
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<SupervisorModel>>(
                "sp_GetSupervisorsByAuthority", parameters);
        }
    }
}
