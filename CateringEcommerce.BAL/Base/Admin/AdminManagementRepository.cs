using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CateringEcommerce.BAL.Base.Admin
{
    public class AdminManagementRepository : IAdminManagementRepository
    {
        private readonly IDatabaseHelper _dbHelper;

        public AdminManagementRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // ==========================================
        // LIST & SEARCH
        // ==========================================

        public async Task<AdminListResponse> GetAllAdminsAsync(AdminListRequest request)
        {
            var conditions = new List<string>();
            var parameters = new List<SqlParameter>();

            // Search filter
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                conditions.Add("(a.c_fullname LIKE @SearchTerm OR a.c_email LIKE @SearchTerm OR a.c_username LIKE @SearchTerm)");
                parameters.Add(new SqlParameter("@SearchTerm", $"%{request.SearchTerm}%"));
            }

            // Role filter
            if (request.RoleId.HasValue)
            {
                conditions.Add("a.c_role_id = @RoleId");
                parameters.Add(new SqlParameter("@RoleId", request.RoleId.Value));
            }

            // Active status filter
            if (request.IsActive.HasValue)
            {
                conditions.Add("a.c_isactive = @IsActive");
                parameters.Add(new SqlParameter("@IsActive", request.IsActive.Value));
            }

            var whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";

            // Count total records
            var countQuery = $@"
                SELECT COUNT(*)
                FROM {Table.SysAdmin} a
                {whereClause}";

            var totalCount = Convert.ToInt32(await _dbHelper.ExecuteScalarAsync(countQuery, parameters.Select(CloneParameter).ToArray()));

            // Get paginated data
            var offset = (request.PageNumber - 1) * request.PageSize;
            var sortColumn = request.SortBy switch
            {
                "FullName" => "a.c_fullname",
                "Email" => "a.c_email",
                "LastLogin" => "a.c_lastlogin",
                "RoleName" => "r.c_role_name",
                _ => "a.c_createddate"
            };
            var sortOrder = request.SortOrder?.ToUpper() == "ASC" ? "ASC" : "DESC";

            var dataParameters = parameters
                .Select(CloneParameter)
                .ToList();

            var dataQuery = $@"
                SELECT
                    a.c_adminid,
                    a.c_username,
                    a.c_email,
                    a.c_fullname,
                    a.c_mobile,
                    a.c_isactive,
                    a.c_lastlogin,
                    a.c_createddate,
                    r.c_role_id,
                    r.c_role_name,
                    r.c_role_code,
                    r.c_color
                FROM {Table.SysAdmin} a
                INNER JOIN {Table.SysAdminRoles} r ON a.c_role_id = r.c_role_id
                {whereClause}
                ORDER BY {sortColumn} {sortOrder}
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            dataParameters.Add(new SqlParameter("@Offset", offset));
            dataParameters.Add(new SqlParameter("@PageSize", request.PageSize));

            var dt = await _dbHelper.ExecuteAsync(dataQuery, dataParameters.ToArray());
            var admins = new List<AdminListItem>();

            foreach (DataRow row in dt.Rows)
            {
                admins.Add(new AdminListItem
                {
                    AdminId = Convert.ToInt64(row["c_adminid"]),
                    Username = row["c_username"].ToString() ?? string.Empty,
                    Email = row["c_email"].ToString() ?? string.Empty,
                    FullName = row["c_fullname"].ToString() ?? string.Empty,
                    Mobile = row["c_mobile"] != DBNull.Value ? row["c_mobile"].ToString() : null,
                    IsActive = Convert.ToBoolean(row["c_isactive"]),
                    LastLogin = row["c_lastlogin"] != DBNull.Value ? Convert.ToDateTime(row["c_lastlogin"]) : null,
                    CreatedDate = Convert.ToDateTime(row["c_createddate"]),
                    RoleId = Convert.ToInt64(row["c_role_id"]),
                    RoleName = row["c_role_name"].ToString() ?? string.Empty,
                    RoleCode = row["c_role_code"].ToString() ?? string.Empty,
                    RoleColor = row["c_color"].ToString() ?? "#6366f1"
                });
            }

            return new AdminListResponse
            {
                Admins = admins,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };
        }

        public async Task<AdminDetailResponse?> GetAdminByIdAsync(long adminId)
        {
            var query = $@"
                SELECT
                    a.c_adminid, a.c_username, a.c_email, a.c_fullname, a.c_mobile,
                    a.c_profilephoto, a.c_isactive, a.c_islocked, a.c_lockeduntil,
                    a.c_failedloginattempts, a.c_force_password_reset,
                    a.c_lastlogin, a.c_createddate, a.c_createdby, a.c_lastmodified, a.c_modifiedby,
                    r.c_role_id, r.c_role_code, r.c_role_name, r.c_description AS role_description,
                    r.c_color, r.c_is_system_role,
                    creator.c_fullname AS created_by_name,
                    modifier.c_fullname AS modified_by_name
                FROM {Table.SysAdmin} a
                INNER JOIN {Table.SysAdminRoles} r ON a.c_role_id = r.c_role_id
                LEFT JOIN {Table.SysAdmin} creator ON a.c_createdby = creator.c_adminid
                LEFT JOIN {Table.SysAdmin} modifier ON a.c_modifiedby = modifier.c_adminid
                WHERE a.c_adminid = @AdminId";

            var parameters = new SqlParameter[] { new SqlParameter("@AdminId", adminId) };
            var dt = await _dbHelper.ExecuteAsync(query, parameters);

            if (dt.Rows.Count == 0) return null;

            var row = dt.Rows[0];
            var response = new AdminDetailResponse
            {
                AdminId = Convert.ToInt64(row["c_adminid"]),
                Username = row["c_username"].ToString() ?? string.Empty,
                Email = row["c_email"].ToString() ?? string.Empty,
                FullName = row["c_fullname"].ToString() ?? string.Empty,
                Mobile = row["c_mobile"] != DBNull.Value ? row["c_mobile"].ToString() : null,
                ProfilePhoto = row["c_profilephoto"] != DBNull.Value ? row["c_profilephoto"].ToString() : null,
                IsActive = Convert.ToBoolean(row["c_isactive"]),
                IsLocked = Convert.ToBoolean(row["c_islocked"]),
                LockedUntil = row["c_lockeduntil"] != DBNull.Value ? Convert.ToDateTime(row["c_lockeduntil"]) : null,
                FailedLoginAttempts = Convert.ToInt32(row["c_failedloginattempts"]),
                ForcePasswordReset = row["c_force_password_reset"] != DBNull.Value && Convert.ToBoolean(row["c_force_password_reset"]),
                LastLogin = row["c_lastlogin"] != DBNull.Value ? Convert.ToDateTime(row["c_lastlogin"]) : null,
                CreatedDate = Convert.ToDateTime(row["c_createddate"]),
                CreatedBy = row["c_createdby"] != DBNull.Value ? Convert.ToInt64(row["c_createdby"]) : null,
                CreatedByName = row["created_by_name"] != DBNull.Value ? row["created_by_name"].ToString() : null,
                LastModified = row["c_lastmodified"] != DBNull.Value ? Convert.ToDateTime(row["c_lastmodified"]) : null,
                ModifiedBy = row["c_modifiedby"] != DBNull.Value ? Convert.ToInt64(row["c_modifiedby"]) : null,
                ModifiedByName = row["modified_by_name"] != DBNull.Value ? row["modified_by_name"].ToString() : null,
                Role = new AdminRoleInfo
                {
                    RoleId = Convert.ToInt64(row["c_role_id"]),
                    RoleCode = row["c_role_code"].ToString() ?? string.Empty,
                    RoleName = row["c_role_name"].ToString() ?? string.Empty,
                    RoleDescription = row["role_description"] != DBNull.Value ? row["role_description"].ToString() : string.Empty,
                    RoleColor = row["c_color"].ToString() ?? "#6366f1",
                    IsSystemRole = Convert.ToBoolean(row["c_is_system_role"])
                }
            };

            // Get permissions for this admin's role
            var permQuery = $@"
                SELECT p.c_permission_code
                FROM {Table.SysAdminPermissions} p
                INNER JOIN {Table.SysAdminRolePermissions} rp ON p.c_permission_id = rp.c_permission_id
                WHERE rp.c_role_id = @RoleId AND p.c_is_active = 1";

            var permParams = new SqlParameter[] { new SqlParameter("@RoleId", response.Role.RoleId) };
            var permDt = await _dbHelper.ExecuteAsync(permQuery, permParams);

            foreach (DataRow permRow in permDt.Rows)
            {
                response.Permissions.Add(permRow["c_permission_code"].ToString() ?? string.Empty);
            }

            return response;
        }

        // ==========================================
        // CRUD OPERATIONS
        // ==========================================

        public async Task<long> CreateAdminAsync(CreateAdminRequest request, long createdBy)
        {
            var query = $@"
                INSERT INTO {Table.SysAdmin}
                (c_username, c_passwordhash, c_email, c_fullname, c_mobile, c_role_id,
                 c_isactive, c_force_password_reset, c_createddate, c_createdby)
                VALUES
                (@Username, @PasswordHash, @Email, @FullName, @Mobile, @RoleId,
                 @IsActive, @ForcePasswordReset, GETDATE(), @CreatedBy);
                SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Username", request.Username),
                new SqlParameter("@PasswordHash", request.Password),
                new SqlParameter("@Email", request.Email),
                new SqlParameter("@FullName", request.FullName),
                new SqlParameter("@Mobile", request.Mobile ?? (object)DBNull.Value),
                new SqlParameter("@RoleId", request.RoleId),
                new SqlParameter("@IsActive", request.IsActive),
                new SqlParameter("@ForcePasswordReset", request.ForcePasswordReset),
                new SqlParameter("@CreatedBy", createdBy)
            };

            var newAdminId = Convert.ToInt64(await _dbHelper.ExecuteScalarAsync(query, parameters));
            return newAdminId;
        }

        public async Task<bool> UpdateAdminAsync(UpdateAdminRequest request, long updatedBy)
        {
            var query = $@"
                UPDATE {Table.SysAdmin}
                SET c_email = @Email,
                    c_fullname = @FullName,
                    c_mobile = @Mobile,
                    c_profilephoto = @ProfilePhoto,
                    c_lastmodified = GETDATE(),
                    c_modifiedby = @ModifiedBy
                WHERE c_adminid = @AdminId";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@AdminId", request.AdminId),
                new SqlParameter("@Email", request.Email),
                new SqlParameter("@FullName", request.FullName),
                new SqlParameter("@Mobile", request.Mobile ?? (object)DBNull.Value),
                new SqlParameter("@ProfilePhoto", request.ProfilePhoto ?? (object)DBNull.Value),
                new SqlParameter("@ModifiedBy", updatedBy)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAdminAsync(long adminId, long deletedBy)
        {
            var query = $@"
                DELETE FROM {Table.SysAdmin}
                WHERE c_adminid = @AdminId";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@AdminId", adminId)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> RemoveAdminLoginAccessAsync(long adminId)
        {
            var query = $@"
                DELETE FROM {Table.SysAdminUsers}
                WHERE c_adminid = @AdminId";
            var parameters = new SqlParameter[] { new SqlParameter("@AdminId", adminId) };
            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        // ==========================================
        // STATUS MANAGEMENT
        // ==========================================

        public async Task<bool> UpdateAdminStatusAsync(long adminId, bool isActive, long updatedBy)
        {
            var query = $@"
                UPDATE {Table.SysAdmin}
                SET c_isactive = @IsActive,
                    c_lastmodified = GETDATE(),
                    c_modifiedby = @UpdatedBy
                WHERE c_adminid = @AdminId";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@AdminId", adminId),
                new SqlParameter("@IsActive", isActive),
                new SqlParameter("@UpdatedBy", updatedBy)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> AssignRoleToAdminAsync(long adminId, long roleId, long assignedBy)
        {
            var query = $@"
                UPDATE {Table.SysAdmin}
                SET c_role_id = @RoleId,
                    c_lastmodified = GETDATE(),
                    c_modifiedby = @AssignedBy
                WHERE c_adminid = @AdminId";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@AdminId", adminId),
                new SqlParameter("@RoleId", roleId),
                new SqlParameter("@AssignedBy", assignedBy)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        // ==========================================
        // PASSWORD MANAGEMENT
        // ==========================================

        public async Task<bool> ResetAdminPasswordAsync(long adminId, string newPasswordHash, bool forceReset, long resetBy)
        {
            var query = $@"
                UPDATE {Table.SysAdmin}
                SET c_passwordhash = @NewPasswordHash,
                    c_force_password_reset = @ForceReset,
                    c_failedloginattempts = 0,
                    c_islocked = 0,
                    c_lockeduntil = NULL,
                    c_lastmodified = GETDATE(),
                    c_modifiedby = @ResetBy
                WHERE c_adminid = @AdminId";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@AdminId", adminId),
                new SqlParameter("@NewPasswordHash", newPasswordHash),
                new SqlParameter("@ForceReset", forceReset),
                new SqlParameter("@ResetBy", resetBy)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> ForcePasswordResetAsync(long adminId, long updatedBy)
        {
            var query = $@"
                UPDATE {Table.SysAdmin}
                SET c_force_password_reset = 1,
                    c_lastmodified = GETDATE(),
                    c_modifiedby = @UpdatedBy
                WHERE c_adminid = @AdminId";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@AdminId", adminId),
                new SqlParameter("@UpdatedBy", updatedBy)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        // ==========================================
        // VALIDATION
        // ==========================================

        public async Task<bool> UsernameExistsAsync(string username, long? excludeAdminId = null)
        {
            var query = $@"
                SELECT COUNT(*)
                FROM {Table.SysAdmin}
                WHERE c_username = @Username";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@Username", username)
            };

            if (excludeAdminId.HasValue)
            {
                query += " AND c_adminid != @ExcludeAdminId";
                parameters.Add(new SqlParameter("@ExcludeAdminId", excludeAdminId.Value));
            }

            var count = Convert.ToInt32(await _dbHelper.ExecuteScalarAsync(query, parameters.ToArray()));
            return count > 0;
        }

        public async Task<bool> EmailExistsAsync(string email, long? excludeAdminId = null)
        {
            var query = $@"
                SELECT COUNT(*)
                FROM {Table.SysAdmin}
                WHERE c_email = @Email";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@Email", email)
            };

            if (excludeAdminId.HasValue)
            {
                query += " AND c_adminid != @ExcludeAdminId";
                parameters.Add(new SqlParameter("@ExcludeAdminId", excludeAdminId.Value));
            }

            var count = Convert.ToInt32(await _dbHelper.ExecuteScalarAsync(query, parameters.ToArray()));
            return count > 0;
        }

        // ==========================================
        // SUPER ADMIN PROTECTION
        // ==========================================

        public async Task<int> GetActiveSuperAdminCountAsync()
        {
            var query = $@"
                SELECT COUNT(*)
                FROM {Table.SysAdmin} a
                INNER JOIN {Table.SysAdminRoles} r ON a.c_role_id = r.c_role_id
                WHERE r.c_role_code = 'SUPER_ADMIN' AND a.c_isactive = 1";

            return Convert.ToInt32(await _dbHelper.ExecuteScalarAsync(query, null));
        }

        public async Task<bool> IsSuperAdminAsync(long adminId)
        {
            var query = $@"
                SELECT COUNT(*)
                FROM {Table.SysAdmin} a
                INNER JOIN {Table.SysAdminRoles} r ON a.c_role_id = r.c_role_id
                WHERE a.c_adminid = @AdminId AND r.c_role_code = 'SUPER_ADMIN'";

            var parameters = new SqlParameter[] { new SqlParameter("@AdminId", adminId) };
            var count = Convert.ToInt32(await _dbHelper.ExecuteScalarAsync(query, parameters));
            return count > 0;
        }

        public async Task<bool> CanDeactivateAdminAsync(long adminId)
        {
            // Check if admin is Super Admin
            var isSuperAdmin = await IsSuperAdminAsync(adminId);
            if (!isSuperAdmin)
            {
                // Non-super-admins can always be deactivated
                return true;
            }

            // Count active Super Admins
            var activeSuperAdminCount = await GetActiveSuperAdminCountAsync();

            // Cannot deactivate if this is the last active Super Admin
            return activeSuperAdminCount > 1;
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
    }
}
