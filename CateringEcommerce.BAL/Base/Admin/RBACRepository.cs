using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CateringEcommerce.BAL.Base.Admin
{
    public class RBACRepository : IRBACRepository
    {
        private readonly IDatabaseHelper _dbHelper;

        public RBACRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // =====================================================
        // ROLE MANAGEMENT
        // =====================================================

        public async Task<List<RoleItem>> GetAllRolesAsync()
        {
            var query = @"
                SELECT
                    r.c_role_id,
                    r.c_role_code,
                    r.c_role_name,
                    r.c_description,
                    r.c_color,
                    r.c_is_system_role,
                    r.c_is_active,
                    r.c_created_date,
                    (SELECT COUNT(*) FROM t_sys_admin_role_permissions WHERE c_role_id = r.c_role_id) AS PermissionCount,
                    (SELECT COUNT(*) FROM t_sys_admin_user_roles WHERE c_role_id = r.c_role_id AND c_is_active = 1) AS AdminCount
                FROM t_sys_admin_roles r
                WHERE r.c_is_active = 1
                ORDER BY
                    CASE WHEN r.c_role_code = 'SUPER_ADMIN' THEN 0 ELSE 1 END,
                    r.c_role_name";

            var dt = await _dbHelper.ExecuteAsync(query, null);
            var roles = new List<RoleItem>();

            foreach (DataRow row in dt.Rows)
            {
                roles.Add(new RoleItem
                {
                    RoleId = Convert.ToInt64(row["c_role_id"]),
                    RoleCode = row["c_role_code"].ToString() ?? string.Empty,
                    RoleName = row["c_role_name"].ToString() ?? string.Empty,
                    Description = row["c_description"] != DBNull.Value ? row["c_description"].ToString() : null,
                    Color = row["c_color"].ToString() ?? "#6366f1",
                    IsSystemRole = Convert.ToBoolean(row["c_is_system_role"]),
                    IsActive = Convert.ToBoolean(row["c_is_active"]),
                    PermissionCount = Convert.ToInt32(row["PermissionCount"]),
                    AdminCount = Convert.ToInt32(row["AdminCount"]),
                    CreatedDate = Convert.ToDateTime(row["c_created_date"])
                });
            }

            return roles;
        }

        public async Task<RoleDetailResponse?> GetRoleByIdAsync(long roleId)
        {
            var roleQuery = @"
                SELECT
                    c_role_id, c_role_code, c_role_name, c_description, c_color,
                    c_is_system_role, c_is_active, c_created_date
                FROM t_sys_admin_roles
                WHERE c_role_id = @RoleId";

            var roleParams = new SqlParameter[] { new SqlParameter("@RoleId", roleId) };
            var roleDt = await _dbHelper.ExecuteAsync(roleQuery, roleParams);

            if (roleDt.Rows.Count == 0) return null;

            var row = roleDt.Rows[0];
            var response = new RoleDetailResponse
            {
                RoleId = Convert.ToInt64(row["c_role_id"]),
                RoleCode = row["c_role_code"].ToString() ?? string.Empty,
                RoleName = row["c_role_name"].ToString() ?? string.Empty,
                Description = row["c_description"] != DBNull.Value ? row["c_description"].ToString() : null,
                Color = row["c_color"].ToString() ?? "#6366f1",
                IsSystemRole = Convert.ToBoolean(row["c_is_system_role"]),
                IsActive = Convert.ToBoolean(row["c_is_active"]),
                CreatedDate = Convert.ToDateTime(row["c_created_date"])
            };

            // Get permissions
            var permQuery = @"
                SELECT
                    p.c_permission_id, p.c_permission_code, p.c_permission_name,
                    p.c_description, p.c_module, p.c_action, p.c_is_active
                FROM t_sys_admin_permissions p
                INNER JOIN t_sys_admin_role_permissions rp ON p.c_permission_id = rp.c_permission_id
                WHERE rp.c_role_id = @RoleId AND p.c_is_active = 1
                ORDER BY p.c_module, p.c_permission_name";

            var permDt = await _dbHelper.ExecuteAsync(permQuery, roleParams);
            response.Permissions = new List<PermissionItem>();

            foreach (DataRow permRow in permDt.Rows)
            {
                response.Permissions.Add(new PermissionItem
                {
                    PermissionId = Convert.ToInt64(permRow["c_permission_id"]),
                    PermissionCode = permRow["c_permission_code"].ToString() ?? string.Empty,
                    PermissionName = permRow["c_permission_name"].ToString() ?? string.Empty,
                    Description = permRow["c_description"] != DBNull.Value ? permRow["c_description"].ToString() : null,
                    Module = permRow["c_module"].ToString() ?? string.Empty,
                    Action = permRow["c_action"].ToString() ?? string.Empty,
                    IsActive = Convert.ToBoolean(permRow["c_is_active"])
                });
            }

            // Get admin users
            var adminQuery = @"
                SELECT
                    a.c_adminid, a.c_full_name, a.c_email, a.c_is_active, a.c_created_date
                FROM t_sys_admin_users a
                INNER JOIN t_sys_admin_user_roles ur ON a.c_adminid = ur.c_adminid
                WHERE ur.c_role_id = @RoleId AND ur.c_is_active = 1
                ORDER BY a.c_full_name";

            var adminDt = await _dbHelper.ExecuteAsync(adminQuery, roleParams);
            response.AdminUsers = new List<AdminUserItem>();

            foreach (DataRow adminRow in adminDt.Rows)
            {
                response.AdminUsers.Add(new AdminUserItem
                {
                    AdminId = Convert.ToInt64(adminRow["c_adminid"]),
                    FullName = adminRow["c_full_name"].ToString() ?? string.Empty,
                    Email = adminRow["c_email"].ToString() ?? string.Empty,
                    IsActive = Convert.ToBoolean(adminRow["c_is_active"]),
                    CreatedDate = Convert.ToDateTime(adminRow["c_created_date"])
                });
            }

            return response;
        }

        public async Task<AdminRole?> GetRoleByCodeAsync(string roleCode)
        {
            var query = @"
                SELECT
                    c_role_id, c_role_code, c_role_name, c_description, c_color,
                    c_is_system_role, c_is_active, c_created_date, c_created_by,
                    c_updated_date, c_updated_by
                FROM t_sys_admin_roles
                WHERE c_role_code = @RoleCode AND c_is_active = 1";

            var parameters = new SqlParameter[] { new SqlParameter("@RoleCode", roleCode) };
            var dt = await _dbHelper.ExecuteAsync(query, parameters);

            if (dt.Rows.Count == 0) return null;

            var row = dt.Rows[0];
            return new AdminRole
            {
                RoleId = Convert.ToInt64(row["c_role_id"]),
                RoleCode = row["c_role_code"].ToString() ?? string.Empty,
                RoleName = row["c_role_name"].ToString() ?? string.Empty,
                Description = row["c_description"] != DBNull.Value ? row["c_description"].ToString() : null,
                Color = row["c_color"].ToString() ?? "#6366f1",
                IsSystemRole = Convert.ToBoolean(row["c_is_system_role"]),
                IsActive = Convert.ToBoolean(row["c_is_active"]),
                CreatedDate = Convert.ToDateTime(row["c_created_date"]),
                CreatedBy = row["c_created_by"] != DBNull.Value ? Convert.ToInt64(row["c_created_by"]) : null,
                UpdatedDate = row["c_updated_date"] != DBNull.Value ? Convert.ToDateTime(row["c_updated_date"]) : null,
                UpdatedBy = row["c_updated_by"] != DBNull.Value ? Convert.ToInt64(row["c_updated_by"]) : null
            };
        }

        public async Task<long> CreateRoleAsync(CreateRoleRequest request, long createdBy)
        {
            var insertRoleQuery = @"
                INSERT INTO t_sys_admin_roles (c_role_code, c_role_name, c_description, c_color, c_is_system_role, c_created_by)
                VALUES (@RoleCode, @RoleName, @Description, @Color, 0, @CreatedBy);
                SELECT CAST(SCOPE_IDENTITY() as bigint);";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@RoleCode", request.RoleCode),
                new SqlParameter("@RoleName", request.RoleName),
                new SqlParameter("@Description", request.Description ?? (object)DBNull.Value),
                new SqlParameter("@Color", request.Color),
                new SqlParameter("@CreatedBy", createdBy)
            };

            var roleId = Convert.ToInt64(await _dbHelper.ExecuteScalarAsync(insertRoleQuery, parameters));

            // Assign permissions
            if (request.PermissionCodes.Any())
            {
                await AssignPermissionsToRoleAsync(roleId, request.PermissionCodes, createdBy);
            }

            return roleId;
        }

        public async Task<bool> UpdateRoleAsync(UpdateRoleRequest request, long updatedBy)
        {
            var updateRoleQuery = @"
                UPDATE t_sys_admin_roles
                SET c_role_name = @RoleName,
                    c_description = @Description,
                    c_color = @Color,
                    c_updated_date = GETDATE(),
                    c_updated_by = @UpdatedBy
                WHERE c_role_id = @RoleId AND c_is_system_role = 0";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@RoleId", request.RoleId),
                new SqlParameter("@RoleName", request.RoleName),
                new SqlParameter("@Description", request.Description ?? (object)DBNull.Value),
                new SqlParameter("@Color", request.Color),
                new SqlParameter("@UpdatedBy", updatedBy)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(updateRoleQuery, parameters);

            // Update permissions
            await AssignPermissionsToRoleAsync(request.RoleId, request.PermissionCodes, updatedBy);

            return rowsAffected > 0;
        }

        public async Task<bool> DeleteRoleAsync(long roleId, long deletedBy)
        {
            // Only allow deletion of non-system roles
            var query = @"
                UPDATE t_sys_admin_roles
                SET c_is_active = 0, c_updated_date = GETDATE(), c_updated_by = @DeletedBy
                WHERE c_role_id = @RoleId AND c_is_system_role = 0";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@RoleId", roleId),
                new SqlParameter("@DeletedBy", deletedBy)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> RoleCodeExistsAsync(string roleCode, long? excludeRoleId = null)
        {
            var query = @"
                SELECT COUNT(*)
                FROM t_sys_admin_roles
                WHERE c_role_code = @RoleCode AND c_is_active = 1";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@RoleCode", roleCode)
            };

            if (excludeRoleId.HasValue)
            {
                query += " AND c_role_id != @ExcludeRoleId";
                parameters.Add(new SqlParameter("@ExcludeRoleId", excludeRoleId.Value));
            }

            var count = Convert.ToInt32(await _dbHelper.ExecuteScalarAsync(query, parameters.ToArray()));
            return count > 0;
        }

        // =====================================================
        // PERMISSION MANAGEMENT
        // =====================================================

        public async Task<PermissionListResponse> GetAllPermissionsAsync()
        {
            var query = @"
                SELECT
                    c_permission_id, c_permission_code, c_permission_name,
                    c_description, c_module, c_action, c_is_active
                FROM t_sys_admin_permissions
                WHERE c_is_active = 1
                ORDER BY c_module, c_permission_name";

            var dt = await _dbHelper.ExecuteAsync(query, null);
            var permissionsByModule = new Dictionary<string, List<PermissionItem>>();

            foreach (DataRow row in dt.Rows)
            {
                var module = row["c_module"].ToString() ?? "OTHER";
                var permission = new PermissionItem
                {
                    PermissionId = Convert.ToInt64(row["c_permission_id"]),
                    PermissionCode = row["c_permission_code"].ToString() ?? string.Empty,
                    PermissionName = row["c_permission_name"].ToString() ?? string.Empty,
                    Description = row["c_description"] != DBNull.Value ? row["c_description"].ToString() : null,
                    Module = module,
                    Action = row["c_action"].ToString() ?? string.Empty,
                    IsActive = Convert.ToBoolean(row["c_is_active"])
                };

                if (!permissionsByModule.ContainsKey(module))
                {
                    permissionsByModule[module] = new List<PermissionItem>();
                }
                permissionsByModule[module].Add(permission);
            }

            var response = new PermissionListResponse
            {
                Groups = permissionsByModule.Select(kvp => new PermissionGroup
                {
                    Module = kvp.Key,
                    ModuleName = FormatModuleName(kvp.Key),
                    Permissions = kvp.Value
                }).ToList()
            };

            return response;
        }

        public async Task<List<string>> GetRolePermissionsAsync(long roleId)
        {
            var query = @"
                SELECT p.c_permission_code
                FROM t_sys_admin_permissions p
                INNER JOIN t_sys_admin_role_permissions rp ON p.c_permission_id = rp.c_permission_id
                WHERE rp.c_role_id = @RoleId AND p.c_is_active = 1";

            var parameters = new SqlParameter[] { new SqlParameter("@RoleId", roleId) };
            var dt = await _dbHelper.ExecuteAsync(query, parameters);

            var permissions = new List<string>();
            foreach (DataRow row in dt.Rows)
            {
                permissions.Add(row["c_permission_code"].ToString() ?? string.Empty);
            }

            return permissions;
        }

        public async Task<bool> AssignPermissionsToRoleAsync(long roleId, List<string> permissionCodes, long assignedBy)
        {
            // First, remove all existing permissions for this role
            var deleteQuery = "DELETE FROM t_sys_admin_role_permissions WHERE c_role_id = @RoleId";
            var deleteParams = new SqlParameter[] { new SqlParameter("@RoleId", roleId) };
            await _dbHelper.ExecuteNonQueryAsync(deleteQuery, deleteParams);

            // Then, insert new permissions
            if (permissionCodes.Any())
            {
                var insertQuery = @"
                    INSERT INTO t_sys_admin_role_permissions (c_role_id, c_permission_id, c_assigned_by)
                    SELECT @RoleId, c_permission_id, @AssignedBy
                    FROM t_sys_admin_permissions
                    WHERE c_permission_code IN (" + string.Join(",", permissionCodes.Select((_, i) => $"@Perm{i}")) + ")";

                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@RoleId", roleId),
                    new SqlParameter("@AssignedBy", assignedBy)
                };

                for (int i = 0; i < permissionCodes.Count; i++)
                {
                    parameters.Add(new SqlParameter($"@Perm{i}", permissionCodes[i]));
                }

                await _dbHelper.ExecuteNonQueryAsync(insertQuery, parameters.ToArray());
            }

            return true;
        }

        public async Task<List<string>> GetAdminPermissionsAsync(long adminId)
        {
            var query = @"
                SELECT DISTINCT p.c_permission_code
                FROM t_sys_admin_permissions p
                INNER JOIN t_sys_admin_role_permissions rp ON p.c_permission_id = rp.c_permission_id
                INNER JOIN t_sys_admin_user_roles ur ON rp.c_role_id = ur.c_role_id
                WHERE ur.c_adminid = @AdminId AND ur.c_is_active = 1 AND p.c_is_active = 1";

            var parameters = new SqlParameter[] { new SqlParameter("@AdminId", adminId) };
            var dt = await _dbHelper.ExecuteAsync(query, parameters);

            var permissions = new List<string>();
            foreach (DataRow row in dt.Rows)
            {
                permissions.Add(row["c_permission_code"].ToString() ?? string.Empty);
            }

            return permissions;
        }

        // =====================================================
        // USER-ROLE MANAGEMENT
        // =====================================================

        public async Task<List<string>> GetAdminRolesAsync(long adminId)
        {
            var query = @"
                SELECT r.c_role_code
                FROM t_sys_admin_roles r
                INNER JOIN t_sys_admin_user_roles ur ON r.c_role_id = ur.c_role_id
                WHERE ur.c_adminid = @AdminId AND ur.c_is_active = 1 AND r.c_is_active = 1";

            var parameters = new SqlParameter[] { new SqlParameter("@AdminId", adminId) };
            var dt = await _dbHelper.ExecuteAsync(query, parameters);

            var roles = new List<string>();
            foreach (DataRow row in dt.Rows)
            {
                roles.Add(row["c_role_code"].ToString() ?? string.Empty);
            }

            return roles;
        }

        public async Task<bool> AssignRolesToAdminAsync(long adminId, List<long> roleIds, long assignedBy)
        {
            // First, deactivate all existing role assignments
            var deactivateQuery = "UPDATE t_sys_admin_user_roles SET c_is_active = 0 WHERE c_adminid = @AdminId";
            var deactivateParams = new SqlParameter[] { new SqlParameter("@AdminId", adminId) };
            await _dbHelper.ExecuteNonQueryAsync(deactivateQuery, deactivateParams);

            // Then, insert/reactivate new role assignments
            foreach (var roleId in roleIds)
            {
                var upsertQuery = @"
                    IF EXISTS (SELECT 1 FROM t_sys_admin_user_roles WHERE c_adminid = @AdminId AND c_role_id = @RoleId)
                        UPDATE t_sys_admin_user_roles SET c_is_active = 1 WHERE c_adminid = @AdminId AND c_role_id = @RoleId
                    ELSE
                        INSERT INTO t_sys_admin_user_roles (c_adminid, c_role_id, c_assigned_by)
                        VALUES (@AdminId, @RoleId, @AssignedBy)";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@AdminId", adminId),
                    new SqlParameter("@RoleId", roleId),
                    new SqlParameter("@AssignedBy", assignedBy)
                };

                await _dbHelper.ExecuteNonQueryAsync(upsertQuery, parameters);
            }

            return true;
        }

        public async Task<bool> RemoveRoleFromAdminAsync(long adminId, long roleId)
        {
            var query = @"
                UPDATE t_sys_admin_user_roles
                SET c_is_active = 0
                WHERE c_adminid = @AdminId AND c_role_id = @RoleId";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@AdminId", adminId),
                new SqlParameter("@RoleId", roleId)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> AdminHasPermissionAsync(long adminId, string permissionCode)
        {
            // Check if user is SUPER_ADMIN first
            var roles = await GetAdminRolesAsync(adminId);
            if (roles.Contains("SUPER_ADMIN")) return true;

            var query = @"
                SELECT COUNT(*)
                FROM t_sys_admin_permissions p
                INNER JOIN t_sys_admin_role_permissions rp ON p.c_permission_id = rp.c_permission_id
                INNER JOIN t_sys_admin_user_roles ur ON rp.c_role_id = ur.c_role_id
                WHERE ur.c_adminid = @AdminId
                    AND p.c_permission_code = @PermissionCode
                    AND ur.c_is_active = 1
                    AND p.c_is_active = 1";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@AdminId", adminId),
                new SqlParameter("@PermissionCode", permissionCode)
            };

            var count = Convert.ToInt32(await _dbHelper.ExecuteScalarAsync(query, parameters));
            return count > 0;
        }

        // Alias for backward compatibility
        public async Task<bool> HasPermissionAsync(long adminId, string permissionCode)
        {
            return await AdminHasPermissionAsync(adminId, permissionCode);
        }

        public async Task<bool> AdminHasAnyPermissionAsync(long adminId, List<string> permissionCodes)
        {
            // Check if user is SUPER_ADMIN first
            var roles = await GetAdminRolesAsync(adminId);
            if (roles.Contains("SUPER_ADMIN")) return true;

            if (!permissionCodes.Any()) return false;

            var query = @"
                SELECT COUNT(*)
                FROM t_sys_admin_permissions p
                INNER JOIN t_sys_admin_role_permissions rp ON p.c_permission_id = rp.c_permission_id
                INNER JOIN t_sys_admin_user_roles ur ON rp.c_role_id = ur.c_role_id
                WHERE ur.c_adminid = @AdminId
                    AND p.c_permission_code IN (" + string.Join(",", permissionCodes.Select((_, i) => $"@Perm{i}")) + @")
                    AND ur.c_is_active = 1
                    AND p.c_is_active = 1";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@AdminId", adminId)
            };

            for (int i = 0; i < permissionCodes.Count; i++)
            {
                parameters.Add(new SqlParameter($"@Perm{i}", permissionCodes[i]));
            }

            var count = Convert.ToInt32(await _dbHelper.ExecuteScalarAsync(query, parameters.ToArray()));
            return count > 0;
        }

        public async Task<bool> AdminHasAllPermissionsAsync(long adminId, List<string> permissionCodes)
        {
            // Check if user is SUPER_ADMIN first
            var roles = await GetAdminRolesAsync(adminId);
            if (roles.Contains("SUPER_ADMIN")) return true;

            if (!permissionCodes.Any()) return false;

            var adminPermissions = await GetAdminPermissionsAsync(adminId);
            return permissionCodes.All(p => adminPermissions.Contains(p));
        }

        public async Task<AdminPermissionsResponse> GetAdminPermissionContextAsync(long adminId)
        {
            var roles = await GetAdminRolesAsync(adminId);
            var permissions = await GetAdminPermissionsAsync(adminId);

            return new AdminPermissionsResponse
            {
                Roles = roles,
                Permissions = permissions,
                IsSuperAdmin = roles.Contains("SUPER_ADMIN")
            };
        }

        public async Task<bool> IsSuperAdminAsync(long adminId)
        {
            var query = @"
                SELECT COUNT(*)
                FROM t_sys_admin a
                INNER JOIN t_sys_admin_roles r ON a.c_role_id = r.c_role_id
                WHERE a.c_adminid = @AdminId AND r.c_role_code = 'SUPER_ADMIN'";

            var parameters = new SqlParameter[] { new SqlParameter("@AdminId", adminId) };
            var count = Convert.ToInt32(await _dbHelper.ExecuteScalarAsync(query, parameters));
            return count > 0;
        }

        // =====================================================
        // AUDIT LOGGING
        // =====================================================

        public async Task LogAuditAsync(AuditLogEntry entry)
        {
            var query = @"
                INSERT INTO t_sys_admin_audit_logs
                (c_adminid, c_admin_name, c_action, c_module, c_target_id, c_target_type,
                 c_details, c_ip_address, c_user_agent, c_status, c_error_message)
                VALUES
                (@AdminId, @AdminName, @Action, @Module, @TargetId, @TargetType,
                 @Details, @IpAddress, @UserAgent, @Status, @ErrorMessage)";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@AdminId", entry.AdminId),
                new SqlParameter("@AdminName", entry.AdminName),
                new SqlParameter("@Action", entry.Action),
                new SqlParameter("@Module", entry.Module ?? (object)DBNull.Value),
                new SqlParameter("@TargetId", entry.TargetId ?? (object)DBNull.Value),
                new SqlParameter("@TargetType", entry.TargetType ?? (object)DBNull.Value),
                new SqlParameter("@Details", entry.Details ?? (object)DBNull.Value),
                new SqlParameter("@IpAddress", entry.IpAddress ?? (object)DBNull.Value),
                new SqlParameter("@UserAgent", entry.UserAgent ?? (object)DBNull.Value),
                new SqlParameter("@Status", entry.Status),
                new SqlParameter("@ErrorMessage", entry.ErrorMessage ?? (object)DBNull.Value)
            };

            await _dbHelper.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<AuditLogListResponse> GetAuditLogsAsync(AuditLogListRequest request)
        {
            var conditions = new List<string>();
            var parameters = new List<SqlParameter>();

            if (request.AdminId.HasValue)
            {
                conditions.Add("c_adminid = @AdminId");
                parameters.Add(new SqlParameter("@AdminId", request.AdminId.Value));
            }

            if (!string.IsNullOrEmpty(request.Action))
            {
                conditions.Add("c_action LIKE @Action");
                parameters.Add(new SqlParameter("@Action", $"%{request.Action}%"));
            }

            if (!string.IsNullOrEmpty(request.Module))
            {
                conditions.Add("c_module = @Module");
                parameters.Add(new SqlParameter("@Module", request.Module));
            }

            if (request.StartDate.HasValue)
            {
                conditions.Add("c_timestamp >= @StartDate");
                parameters.Add(new SqlParameter("@StartDate", request.StartDate.Value));
            }

            if (request.EndDate.HasValue)
            {
                conditions.Add("c_timestamp <= @EndDate");
                parameters.Add(new SqlParameter("@EndDate", request.EndDate.Value));
            }

            var whereClause = conditions.Any() ? "WHERE " + string.Join(" AND ", conditions) : "";

            var countQuery = $"SELECT COUNT(*) FROM t_sys_admin_audit_logs {whereClause}";
            var totalCount = Convert.ToInt32(await _dbHelper.ExecuteScalarAsync(countQuery, parameters.ToArray()));

            var offset = (request.PageNumber - 1) * request.PageSize;
            var dataQuery = $@"
                SELECT
                    c_audit_id, c_adminid, c_admin_name, c_action, c_module,
                    c_target_id, c_target_type, c_details, c_ip_address, c_user_agent,
                    c_timestamp, c_status, c_error_message
                FROM t_sys_admin_audit_logs
                {whereClause}
                ORDER BY c_timestamp DESC
                OFFSET {offset} ROWS
                FETCH NEXT {request.PageSize} ROWS ONLY";

            var dt = await _dbHelper.ExecuteAsync(dataQuery, parameters.ToArray());

            var logs = new List<AuditLogEntry>();
            foreach (DataRow row in dt.Rows)
            {
                logs.Add(new AuditLogEntry
                {
                    AuditId = Convert.ToInt64(row["c_audit_id"]),
                    AdminId = Convert.ToInt64(row["c_adminid"]),
                    AdminName = row["c_admin_name"].ToString() ?? string.Empty,
                    Action = row["c_action"].ToString() ?? string.Empty,
                    Module = row["c_module"] != DBNull.Value ? row["c_module"].ToString() ?? string.Empty : string.Empty,
                    TargetId = row["c_target_id"] != DBNull.Value ? Convert.ToInt64(row["c_target_id"]) : null,
                    TargetType = row["c_target_type"] != DBNull.Value ? row["c_target_type"].ToString() : null,
                    Details = row["c_details"] != DBNull.Value ? row["c_details"].ToString() : null,
                    IpAddress = row["c_ip_address"] != DBNull.Value ? row["c_ip_address"].ToString() : null,
                    UserAgent = row["c_user_agent"] != DBNull.Value ? row["c_user_agent"].ToString() : null,
                    Timestamp = Convert.ToDateTime(row["c_timestamp"]),
                    Status = row["c_status"].ToString() ?? "SUCCESS",
                    ErrorMessage = row["c_error_message"] != DBNull.Value ? row["c_error_message"].ToString() : null
                });
            }

            return new AuditLogListResponse
            {
                Logs = logs,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        // =====================================================
        // ADMIN ROLE INITIALIZATION & UTILITIES
        // =====================================================

        /// <summary>
        /// Ensures an admin has at least one active role assigned
        /// If no role is assigned, assigns SUPER_ADMIN role (for first admin) or CATERING_ADMIN (for others)
        /// </summary>
        public async Task<bool> EnsureAdminHasRoleAsync(long adminId, long? assignedBy = null)
        {
            // Check if admin already has an active role
            var existingRoles = await GetAdminRolesAsync(adminId);
            if (existingRoles.Any())
            {
                return true; // Admin already has roles
            }

            // Get admin details to determine if this is the first admin
            var checkAdminQuery = "SELECT c_adminid FROM t_sys_admin_users WHERE c_adminid = @AdminId";
            var adminParams = new SqlParameter[] { new SqlParameter("@AdminId", adminId) };
            var adminDt = await _dbHelper.ExecuteAsync(checkAdminQuery, adminParams);

            if (adminDt.Rows.Count == 0)
            {
                return false; // Admin not found
            }

            // Determine which role to assign
            string roleCode = adminId == 1 ? "SUPER_ADMIN" : "CATERING_ADMIN";

            // Get role ID
            var roleQuery = "SELECT c_role_id FROM t_sys_admin_roles WHERE c_role_code = @RoleCode AND c_is_active = 1";
            var roleParams = new SqlParameter[] { new SqlParameter("@RoleCode", roleCode) };
            var roleIdObj = await _dbHelper.ExecuteScalarAsync(roleQuery, roleParams);

            if (roleIdObj == null || roleIdObj == DBNull.Value)
            {
                return false; // Role not found
            }

            long roleId = Convert.ToInt64(roleIdObj);
            long assignerId = assignedBy ?? adminId; // Self-assign if no assignedBy provided

            // Assign role
            await AssignRolesToAdminAsync(adminId, new List<long> { roleId }, assignerId);

            return true;
        }

        /// <summary>
        /// Gets admin roles (with fallback to auto-assign if missing)
        /// This is the SAFE version that ensures data integrity
        /// </summary>
        public async Task<List<string>> GetAdminRolesWithFallbackAsync(long adminId)
        {
            var roles = await GetAdminRolesAsync(adminId);

            // If no roles found, try to auto-assign
            if (!roles.Any())
            {
                await EnsureAdminHasRoleAsync(adminId);
                roles = await GetAdminRolesAsync(adminId);
            }

            return roles;
        }

        /// <summary>
        /// Gets admin permissions (with fallback to auto-assign role if missing)
        /// This is the SAFE version that ensures data integrity
        /// </summary>
        public async Task<List<string>> GetAdminPermissionsWithFallbackAsync(long adminId)
        {
            var permissions = await GetAdminPermissionsAsync(adminId);

            // If no permissions found, try to auto-assign role
            if (!permissions.Any())
            {
                await EnsureAdminHasRoleAsync(adminId);
                permissions = await GetAdminPermissionsAsync(adminId);
            }

            return permissions;
        }

        /// <summary>
        /// Checks if admin has role assignment data
        /// Returns diagnostic information for debugging
        /// </summary>
        public async Task<AdminRoleDiagnostics> DiagnoseAdminRolesAsync(long adminId)
        {
            var diagnostics = new AdminRoleDiagnostics
            {
                AdminId = adminId
            };

            // Check if admin exists
            var adminQuery = "SELECT c_adminid, c_full_name, c_email FROM t_sys_admin_users WHERE c_adminid = @AdminId";
            var adminParams = new SqlParameter[] { new SqlParameter("@AdminId", adminId) };
            var adminDt = await _dbHelper.ExecuteAsync(adminQuery, adminParams);

            diagnostics.AdminExists = adminDt.Rows.Count > 0;

            if (diagnostics.AdminExists)
            {
                diagnostics.AdminName = adminDt.Rows[0]["c_full_name"]?.ToString() ?? "";
                diagnostics.AdminEmail = adminDt.Rows[0]["c_email"]?.ToString() ?? "";
            }

            // Check role assignments
            var roleAssignmentQuery = @"
                SELECT
                    ur.c_id,
                    ur.c_role_id,
                    r.c_role_code,
                    r.c_role_name,
                    ur.c_is_active,
                    ur.c_assigned_date
                FROM t_sys_admin_user_roles ur
                INNER JOIN t_sys_admin_roles r ON ur.c_role_id = r.c_role_id
                WHERE ur.c_adminid = @AdminId
                ORDER BY ur.c_is_active DESC, ur.c_assigned_date DESC";

            var roleDt = await _dbHelper.ExecuteAsync(roleAssignmentQuery, adminParams);
            diagnostics.TotalRoleAssignments = roleDt.Rows.Count;
            diagnostics.ActiveRoleAssignments = roleDt.AsEnumerable()
                .Count(r => Convert.ToBoolean(r["c_is_active"]));

            if (roleDt.Rows.Count > 0)
            {
                diagnostics.RoleAssignments = new List<string>();
                foreach (DataRow row in roleDt.Rows)
                {
                    var isActive = Convert.ToBoolean(row["c_is_active"]);
                    var roleCode = row["c_role_code"]?.ToString() ?? "";
                    var roleName = row["c_role_name"]?.ToString() ?? "";
                    diagnostics.RoleAssignments.Add($"{roleCode} ({roleName}) - Active: {isActive}");
                }
            }

            // Get roles using existing method
            diagnostics.Roles = await GetAdminRolesAsync(adminId);

            // Get permissions using existing method
            diagnostics.Permissions = await GetAdminPermissionsAsync(adminId);

            // Determine if fix is needed
            diagnostics.NeedsFix = diagnostics.AdminExists && diagnostics.ActiveRoleAssignments == 0;

            return diagnostics;
        }

        // =====================================================
        // HELPER METHODS
        // =====================================================

        private string FormatModuleName(string module)
        {
            return module switch
            {
                "CATERING" => "Catering Management",
                "USER" => "User Management",
                "REVIEW" => "Review Management",
                "EARNINGS" => "Finance Management",
                "PAYOUT" => "Payout Management",
                "COMMISSION" => "Commission Management",
                "DISCOUNT" => "Discount Management",
                "BANNER" => "Banner Management",
                "CAMPAIGN" => "Campaign Management",
                "EVENT" => "Event Management",
                "SYSTEM" => "System Administration",
                "ADMIN" => "Admin User Management",
                "AUDIT" => "Audit Management",
                _ => module
            };
        }
    }
}
