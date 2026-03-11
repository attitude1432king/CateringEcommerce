using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Models.User;
using Microsoft.Data.SqlClient;

namespace CateringEcommerce.BAL.Common
{
    public class UserRepository : IUserRepository
    {
        private readonly IDatabaseHelper _dbHelper;
        public UserRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public UserModel GetUserDetails(Int64 userPKID)
        {
            string query = $"SELECT * FROM {Table.SysUser} WHERE c_userid = @UserPKID";
            SqlParameter[] parameters = Array.Empty<SqlParameter>();

            if (userPKID > 0)
            {
                parameters = new[]
                {
                    new SqlParameter("@UserPKID", userPKID)
                };
            }

            var dt = _dbHelper.Execute(query, parameters);

            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                return new UserModel
                {
                    PkID = row["c_userid"] == DBNull.Value ? 0 : Convert.ToInt64(row["c_userid"].ToString()),
                    FullName = row["c_name"] == DBNull.Value ? string.Empty : row["c_name"].ToString(),
                    Phone = row["c_mobile"] == DBNull.Value ? string.Empty : row["c_mobile"].ToString(),
                    Email = row["c_email"] == DBNull.Value ? string.Empty : row["c_email"].ToString(),
                    IsEmailVerified = row["c_isemailverified"] == DBNull.Value ? false : row.GetBoolean("c_isemailverified"),
                    IsPhoneVerified = row["c_isphoneverified"] == DBNull.Value ? false : row.GetBoolean("c_isphoneverified"),
                    CityID = row["c_cityid"] == DBNull.Value ? 0 : Convert.ToInt32(row["c_cityid"]),
                    StateID = row["c_stateid"] == DBNull.Value ? 0 : Convert.ToInt32(row["c_stateid"]),
                    Description = row.Table.Columns.Contains("c_description") && row["c_description"] != DBNull.Value ? row["c_description"].ToString() : string.Empty,
                    ProfilePhoto = row.Table.Columns.Contains("c_picture") && row["c_picture"] != DBNull.Value ? row["c_picture"].ToString() : string.Empty
                };
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Check if the provided email already exists in the database.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public bool IsExistEmail(string email, string role = "User")
        {
            string tableName = GetUserTableName(role);
            string query = $"SELECT Count(c_email) FROM {tableName} WHERE c_email = @Email";
            SqlParameter[] parameters = {
                    new SqlParameter("@Email", email)
                    };
            return Convert.ToBoolean(_dbHelper.ExecuteScalar(query, parameters));
        }


        /// <summary>
        /// Check the phone number already exists in the database.
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public bool IsExistNumber(string phoneNumber, string role)
        {
            string tableName = GetUserTableName(role);
            string query = $"SELECT Count(c_mobile) FROM {tableName} WHERE c_mobile = @phoneNumber";
            SqlParameter[] parameters = {
                    new SqlParameter("@phoneNumber", phoneNumber)
                    };
            return Convert.ToBoolean(_dbHelper.ExecuteScalar(query, parameters));
        }

        public bool IsExistRoleBaseNumber(string phoneNumber, string type, string role)
        {
            string tableName = GetUserTableName(role);
            string numberColumn = type == "phone" ? "c_mobile" : "c_catering_number";
            string query = $"SELECT Count({numberColumn}) FROM {tableName} WHERE {numberColumn} = @phoneNumber";
            SqlParameter[] parameters = {
                    new SqlParameter("@phoneNumber", phoneNumber.Substring(3))
                    };
            return Convert.ToBoolean(_dbHelper.ExecuteScalar(query, parameters));
        }

        /// <summary>
        /// Get approval status for partner/owner by phone number
        /// Returns the approval status if owner exists, otherwise returns null
        /// </summary>
        public int? GetOwnerApprovalStatus(string phoneNumber)
        {
            string query = $"SELECT c_approval_status FROM {Table.SysCateringOwner} WHERE c_catering_number = @phoneNumber OR c_mobile = @phoneNumber";
            SqlParameter[] parameters = {
                    new SqlParameter("@phoneNumber", phoneNumber)
                    };
            
            var result = _dbHelper.ExecuteScalar(query, parameters);
            
            if (result != null && result != DBNull.Value)
            {
                return Convert.ToInt32(result);
            }
            
            return null;
        }

        /// <summary>
        /// Check if owner exists and get their details along with approval status
        /// </summary>
        public (bool exists, int? approvalStatus) CheckOwnerWithApprovalStatus(string phoneNumber)
        {
            string query = $"SELECT c_ownerid, c_approval_status FROM {Table.SysCateringOwner} WHERE c_catering_number = @phoneNumber OR c_mobile = @phoneNumber";
            SqlParameter[] parameters = {
                    new SqlParameter("@phoneNumber", phoneNumber)
                    };
            
            var dt = _dbHelper.Execute(query, parameters);
            
            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                int? approvalStatus = row["c_approval_status"] == DBNull.Value ? null : Convert.ToInt32(row["c_approval_status"]);
                return (true, approvalStatus);
            }
            
            return (false, null);
        }

        private string GetUserTableName(string role)
        {
            return role switch
            {
                "Owner" => Table.SysCateringOwner,
                "User" => Table.SysUser,
                _ => throw new ArgumentException("Invalid role specified.")
            };
        }

    }
}
