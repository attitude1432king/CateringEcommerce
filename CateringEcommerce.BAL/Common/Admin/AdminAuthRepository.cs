using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CateringEcommerce.BAL.Common.Admin
{
    public class AdminAuthRepository : IAdminAuthRepository
    {
        private readonly SqlDatabaseManager _db;

        public AdminAuthRepository(string connectionString)
        {
            _db = new SqlDatabaseManager();
            _db.SetConnectionString(connectionString);
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
                  AND c_isactive = 1
                  AND c_islocked = 0";

            SqlParameter[] parameters = {
                new SqlParameter("@Username", username),
                new SqlParameter("@PasswordHash", passwordHash)
            };

            var dt = _db.Execute(query, parameters);

            if (dt.Rows.Count > 0)
            {
                return MapToAdminModel(dt.Rows[0]);
            }

            return null;
        }

        public AdminModel? GetAdminById(long adminId)
        {
            string query = $@"
                SELECT c_adminid, c_username, c_email, c_fullname, c_role,
                       c_profilephoto, c_isactive, c_failedloginattempts,
                       c_islocked, c_lockeduntil, c_createddate, c_lastlogin
                FROM {Table.SysAdmin}
                WHERE c_adminid = @AdminId";

            SqlParameter[] parameters = {
                new SqlParameter("@AdminId", adminId)
            };

            var dt = _db.Execute(query, parameters);

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
                SET c_lastlogin = GETDATE()
                WHERE c_adminid = @AdminId";

            SqlParameter[] parameters = {
                new SqlParameter("@AdminId", adminId)
            };

            _db.ExecuteNonQuery(query, parameters);
        }

        public void IncrementFailedLoginAttempts(string username)
        {
            string query = $@"
                UPDATE {Table.SysAdmin}
                SET c_failedloginattempts = c_failedloginattempts + 1
                WHERE c_username = @Username";

            SqlParameter[] parameters = {
                new SqlParameter("@Username", username)
            };

            _db.ExecuteNonQuery(query, parameters);
        }

        public void ResetFailedLoginAttempts(long adminId)
        {
            string query = $@"
                UPDATE {Table.SysAdmin}
                SET c_failedloginattempts = 0
                WHERE c_adminid = @AdminId";

            SqlParameter[] parameters = {
                new SqlParameter("@AdminId", adminId)
            };

            _db.ExecuteNonQuery(query, parameters);
        }

        public void LockAccount(string username, DateTime lockUntil)
        {
            string query = $@"
                UPDATE {Table.SysAdmin}
                SET c_islocked = 1,
                    c_lockeduntil = @LockUntil
                WHERE c_username = @Username";

            SqlParameter[] parameters = {
                new SqlParameter("@Username", username),
                new SqlParameter("@LockUntil", lockUntil)
            };

            _db.ExecuteNonQuery(query, parameters);
        }

        public bool IsAccountLocked(string username)
        {
            string query = $@"
                SELECT CASE
                    WHEN c_islocked = 1 AND (c_lockeduntil IS NULL OR c_lockeduntil > GETDATE())
                    THEN 1
                    ELSE 0
                END
                FROM {Table.SysAdmin}
                WHERE c_username = @Username";

            SqlParameter[] parameters = {
                new SqlParameter("@Username", username)
            };

            var result = _db.ExecuteScalar(query, parameters);
            return Convert.ToBoolean(result);
        }

        public void LogAdminActivity(long adminId, string action, string? details = null)
        {
            string query = $@"
                INSERT INTO {Table.SysAdminActivityLog}
                (c_adminid, c_action, c_details, c_ipaddress, c_createddate)
                VALUES (@AdminId, @Action, @Details, @IpAddress, GETDATE())";

            SqlParameter[] parameters = {
                new SqlParameter("@AdminId", adminId),
                new SqlParameter("@Action", action),
                new SqlParameter("@Details", (object?)details ?? DBNull.Value),
                new SqlParameter("@IpAddress", DBNull.Value)
            };

            _db.ExecuteNonQuery(query, parameters);
        }

        private AdminModel MapToAdminModel(DataRow row)
        {
            return new AdminModel
            {
                AdminId = Convert.ToInt64(row["c_adminid"]),
                Username = row["c_username"].ToString() ?? string.Empty,
                Email = row["c_email"].ToString() ?? string.Empty,
                FullName = row["c_fullname"].ToString() ?? string.Empty,
                Role = row["c_role"].ToString() ?? string.Empty,
                ProfilePhoto = row["c_profilephoto"] != DBNull.Value ? row["c_profilephoto"].ToString() : null,
                IsActive = Convert.ToBoolean(row["c_isactive"]),
                FailedLoginAttempts = Convert.ToInt32(row["c_failedloginattempts"]),
                IsLocked = Convert.ToBoolean(row["c_islocked"]),
                LockedUntil = row["c_lockeduntil"] != DBNull.Value ? Convert.ToDateTime(row["c_lockeduntil"]) : null,
                CreatedDate = Convert.ToDateTime(row["c_createddate"]),
                LastLogin = row["c_lastlogin"] != DBNull.Value ? Convert.ToDateTime(row["c_lastlogin"]) : null
            };
        }
    }
}
