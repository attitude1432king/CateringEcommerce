using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.User;
using CateringEcommerce.Domain.Models.User;
using Microsoft.Data.SqlClient;

namespace CateringEcommerce.BAL.Base.User.AuthLogic
{
    public class Authentication : IAuthentication
    {
        private readonly IDatabaseHelper _dbHelper;
        public Authentication(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        /// <summary>
        /// Create a new user account with the provided name and phone number.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public int CreateUserAccount(string name, string phoneNumber = null, Dictionary<string, string> dicData = null)
        {
            dicData ??= new Dictionary<string, string>(); // Fix for CS1736 and IDE0028
            bool isGoogleAuthention = dicData.Count > 0 && dicData.ContainsKey("isGoogleAuthention");
            string googleId = dicData.ContainsKey("googleId") ? dicData["googleId"] : null;
            bool isVerified = dicData.Count > 0 && dicData.ContainsKey("isVerified");
            string pictureUrl = dicData.ContainsKey("pictureUrl") ? dicData["pictureUrl"] : null;
            string query;
            List<SqlParameter> parameters = new()
            {
                new SqlParameter("@Name", name),
                new SqlParameter("@IsActive", true.ToBinary()),
                new SqlParameter("@CreatedDate", DateTime.Now)
            };

            if (isGoogleAuthention)
            {
                query = $@"INSERT INTO {Table.SysUser} 
                    (c_name, c_googleid, c_isemailverified, c_picture, c_isactive, c_createddate) 
                    VALUES (@Name, @GoogleId, @IsVerified, @PictureUrl, @IsActive, @CreatedDate)";

                parameters.Add(new SqlParameter("@GoogleId", googleId ?? (object)DBNull.Value));
                parameters.Add(new SqlParameter("@IsVerified", isVerified));
                parameters.Add(new SqlParameter("@PictureUrl", pictureUrl ?? (object)DBNull.Value));
            }
            else
            {
                query = $@"INSERT INTO {Table.SysUser} 
                    (c_name, c_mobile, c_isphoneverified, c_isactive, c_createddate) 
                    VALUES (@Name, @Phone, @IsVerified, @IsActive, @CreatedDate)";
                parameters.Add(new SqlParameter("@IsVerified", true.ToBinary()));
                parameters.Add(new SqlParameter("@Phone", phoneNumber ?? (object)DBNull.Value));
            }

            return _dbHelper.ExecuteNonQuery(query, parameters.ToArray());
        }

        
        public UserModel? GetUserData(string? phoneNumber = null)
        {
            string query = $"SELECT * FROM {Table.SysUser} WHERE c_mobile = @phoneNumber";
            SqlParameter[] parameters = Array.Empty<SqlParameter>();

            if (!string.IsNullOrEmpty(phoneNumber))
            {
                parameters = new[]
                {
                    new SqlParameter("@phoneNumber", phoneNumber)
                };
            }

            var dt = _dbHelper.Execute(query, parameters);

            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                return new UserModel
                {
                    PkID = row["c_userid"] == DBNull.Value ? 0 : Convert.ToInt64(row["c_userid"]),
                    FullName = row["c_name"] == DBNull.Value ? string.Empty : row["c_name"].ToString(),
                    Phone = row["c_mobile"] == DBNull.Value ? string.Empty : row["c_mobile"].ToString(),
                    Email = row["c_email"] == DBNull.Value ? string.Empty : row["c_email"].ToString(),
                    IsEmailVerified = row["c_isemailverified"] == DBNull.Value ? false : row.GetBoolean("c_isemailverified"),
                    IsPhoneVerified = row["c_isphoneverified"] == DBNull.Value ? false : row.GetBoolean("c_isphoneverified"),
                    CityID = row["c_cityid"] == DBNull.Value ? 0 : Convert.ToInt32(row["c_cityid"]),
                    StateID = row["c_stateid"] == DBNull.Value ? 0 : Convert.ToInt32(row["c_stateid"]),
                    Description = row.Table.Columns.Contains("c_description") && row["c_description"] != DBNull.Value ? row["c_description"].ToString() : string.Empty,
                    ProfilePhoto = row.Table.Columns.Contains("c_picture") && row["c_picture"] != DBNull.Value ? row["c_picture"].ToString() : string.Empty,
                    IsBlocked = row.Table.Columns.Contains("c_isblocked") && row["c_isblocked"] != DBNull.Value && Convert.ToBoolean(row["c_isblocked"]),
                    BlockReason = row.Table.Columns.Contains("c_block_reason") && row["c_block_reason"] != DBNull.Value ? row["c_block_reason"].ToString() : string.Empty
                };
            }
            else
            {
                return null;
            }
        }
    }
}
