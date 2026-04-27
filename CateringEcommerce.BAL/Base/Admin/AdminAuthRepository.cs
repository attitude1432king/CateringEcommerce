using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Npgsql;
using System.Data;

namespace CateringEcommerce.BAL.Base.Admin
{
    public class AdminAuthRepository : IAdminAuthRepository
    {
        private readonly IDatabaseHelper _dbHelper;
        public AdminAuthRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public AdminModel? AuthenticateAdmin(string username, string passwordHash)
        {
            string query = $@"
                SELECT c_adminid, c_username, c_email, c_fullname, c_role,
                       c_profilephoto, c_isactive, c_failedloginattempts,
                       c_islocked, c_lockeduntil, c_createddate, c_lastlogin
                FROM {Table.SysAdmin}
                WHERE c_username = @Username
                  AND c_passwordhash = @PasswordHash
                  AND c_isactive = TRUE
                  AND c_islocked = FALSE";

            NpgsqlParameter[] parameters = {
                new NpgsqlParameter("@Username", username),
                new NpgsqlParameter("@PasswordHash", passwordHash)
            };

            var dt = _dbHelper.Execute(query, parameters);

            if (dt.Rows.Count > 0)
            {
                return MapToAdminModel(dt.Rows[0]);
            }

            return null;
        }

        public AdminModel? GetAdminById(long adminId)
        {
            string query = $@"
                SELECT c_adminid, c_username, c_passwordhash, c_email, c_fullname, c_role,
                       c_profilephoto, c_isactive, c_failedloginattempts,
                       c_islocked, c_lockeduntil, c_createddate, c_lastlogin,
                       c_is_temporary_password
                FROM {Table.SysAdmin}
                WHERE c_adminid = @AdminId";

            NpgsqlParameter[] parameters = {
                new NpgsqlParameter("@AdminId", adminId)
            };

            var dt = _dbHelper.Execute(query, parameters);

            if (dt.Rows.Count > 0)
            {
                return MapToAdminModel(dt.Rows[0]);
            }

            return null;
        }

        public AdminModel? GetAdminByUsername(string username)
        {
            string query = $@"
                SELECT c_adminid, c_username, c_passwordhash, c_email, c_fullname, c_role,
                       c_profilephoto, c_isactive, c_failedloginattempts,
                       c_islocked, c_lockeduntil, c_createddate, c_lastlogin,
                       c_is_temporary_password
                FROM {Table.SysAdmin}
                WHERE c_username = @Username
                  AND c_isactive = TRUE";

            NpgsqlParameter[] parameters = {
                new NpgsqlParameter("@Username", username.Trim())
            };

            var dt = _dbHelper.Execute(query, parameters);

            if (dt.Rows.Count > 0)
            {
                return MapToAdminModel(dt.Rows[0]);
            }

            return null;
        }

        public void UpdateLastLogin(long adminId)
        {
            string query = $@"
                UPDATE {Table.SysAdmin}
                SET c_lastlogin = NOW()
                WHERE c_adminid = @AdminId";

            NpgsqlParameter[] parameters = {
                new NpgsqlParameter("@AdminId", adminId)
            };

            _dbHelper.ExecuteNonQuery(query, parameters);
        }

        public void IncrementFailedLoginAttempts(string username)
        {
            string query = $@"
                UPDATE {Table.SysAdmin}
                SET c_failedloginattempts = c_failedloginattempts + 1
                WHERE c_username = @Username";

            NpgsqlParameter[] parameters = {
                new NpgsqlParameter("@Username", username)
            };

            _dbHelper.ExecuteNonQuery(query, parameters);
        }

        public void ResetFailedLoginAttempts(long adminId)
        {
            string query = $@"
                UPDATE {Table.SysAdmin}
                SET c_failedloginattempts = 0
                WHERE c_adminid = @AdminId";

            NpgsqlParameter[] parameters = {
                new NpgsqlParameter("@AdminId", adminId)
            };

            _dbHelper.ExecuteNonQuery(query, parameters);
        }

        public void LockAccount(string username, DateTime lockUntil)
        {
            string query = $@"
                UPDATE {Table.SysAdmin}
                SET c_islocked = TRUE,
                    c_lockeduntil = @LockUntil
                WHERE c_username = @Username";

            NpgsqlParameter[] parameters = {
                new NpgsqlParameter("@Username", username),
                new NpgsqlParameter("@LockUntil", lockUntil)
            };

            _dbHelper.ExecuteNonQuery(query, parameters);
        }

        public bool IsAccountLocked(string username)
        {
            string query = $@"
                SELECT CASE
                    WHEN c_islocked = TRUE AND (c_lockeduntil IS NULL OR c_lockeduntil > NOW())
                    THEN 1
                    ELSE 0
                END
                FROM {Table.SysAdmin}
                WHERE c_username = @Username";

            NpgsqlParameter[] parameters = {
                new NpgsqlParameter("@Username", username)
            };

            var result = _dbHelper.ExecuteScalar(query, parameters);
            return Convert.ToBoolean(result);
        }

        public bool IsTemporaryPassword(long adminId)
        {
            string query = $@"
                SELECT c_is_temporary_password
                FROM {Table.SysAdmin}
                WHERE c_adminid = @AdminId AND c_isactive = TRUE";

            NpgsqlParameter[] parameters = {
                new NpgsqlParameter("@AdminId", adminId)
            };

            var result = _dbHelper.ExecuteScalar(query, parameters);
            return result != null && result != DBNull.Value && Convert.ToBoolean(result);
        }

        public bool ChangeTempPassword(long adminId, string newPasswordHash)
        {
            string query = $@"
                UPDATE {Table.SysAdmin}
                SET c_passwordhash = @NewHash,
                    c_is_temporary_password = FALSE,
                    c_lastmodified = NOW()
                WHERE c_adminid = @AdminId";

            NpgsqlParameter[] parameters = {
                new NpgsqlParameter("@AdminId", adminId),
                new NpgsqlParameter("@NewHash", newPasswordHash)
            };

            int rowsAffected = _dbHelper.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        public void LogAdminActivity(long adminId, string action, string? details = null)
        {
            string query = $@"
                INSERT INTO {Table.SysAdminActivityLog}
                (c_adminid, c_action, c_details, c_ipaddress, c_createddate)
                VALUES (@AdminId, @Action, @Details, @IpAddress, NOW())";

            NpgsqlParameter[] parameters = {
                new NpgsqlParameter("@AdminId", adminId),
                new NpgsqlParameter("@Action", action),
                new NpgsqlParameter("@Details", (object?)details ?? DBNull.Value),
                new NpgsqlParameter("@IpAddress", DBNull.Value)
            };

            _dbHelper.ExecuteNonQuery(query, parameters);
        }

        private AdminModel MapToAdminModel(DataRow row)
        {
            return new AdminModel
            {
                AdminId = Convert.ToInt64(row["c_adminid"]),
                Username = row["c_username"].ToString() ?? string.Empty,
                PasswordHash = row.Table.Columns.Contains("c_passwordhash") && row["c_passwordhash"] != DBNull.Value
                    ? row["c_passwordhash"].ToString() ?? string.Empty
                    : string.Empty,
                Email = row["c_email"].ToString() ?? string.Empty,
                FullName = row["c_fullname"].ToString() ?? string.Empty,
                Role = row["c_role"].ToString() ?? string.Empty,
                ProfilePhoto = row["c_profilephoto"] != DBNull.Value ? row["c_profilephoto"].ToString() : null,
                IsActive = Convert.ToBoolean(row["c_isactive"]),
                FailedLoginAttempts = Convert.ToInt32(row["c_failedloginattempts"]),
                IsLocked = Convert.ToBoolean(row["c_islocked"]),
                LockedUntil = row["c_lockeduntil"] != DBNull.Value ? Convert.ToDateTime(row["c_lockeduntil"]) : null,
                CreatedDate = Convert.ToDateTime(row["c_createddate"]),
                LastLogin = row["c_lastlogin"] != DBNull.Value ? Convert.ToDateTime(row["c_lastlogin"]) : null,
                IsTemporaryPassword = row.Table.Columns.Contains("c_is_temporary_password") && row["c_is_temporary_password"] != DBNull.Value && Convert.ToBoolean(row["c_is_temporary_password"])
            };
        }
    }
}

