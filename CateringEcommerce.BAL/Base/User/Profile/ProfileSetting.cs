using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces.User;
using Microsoft.Data.SqlClient;
using System.Text;

namespace CateringEcommerce.BAL.Base.User.Profile
{
    public class ProfileSetting : IProfileSetting
    {
        private readonly SqlDatabaseManager _db;

        public ProfileSetting(string connectionString)
        {
            _db = new SqlDatabaseManager();
            _db.SetConnectionString(connectionString);
        }

        public void UpdateUserDetails(long? userPKID, Dictionary<string, string> dicData = null)
        {
            StringBuilder updateQuery = new StringBuilder();
            string email = dicData != null && dicData.ContainsKey("email") ? dicData["email"] : string.Empty;
            string phone = dicData != null && dicData.ContainsKey("phone") ? dicData["phone"] : string.Empty;
            string pictureUrl = dicData != null && dicData.ContainsKey("pictureUrl") ? dicData["pictureUrl"] : string.Empty;
            int stateID = dicData != null && dicData.ContainsKey("stateID") ? int.Parse(dicData["stateID"]) : 0;
            int cityID = dicData != null && dicData.ContainsKey("cityID") ? int.Parse(dicData["cityID"]) : 0;
            string Description = dicData != null && dicData.ContainsKey("description") ? dicData["description"] : string.Empty;

            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>
                {
                    new SqlParameter("@UserPKID", userPKID),
                    new SqlParameter("@ModifiedDate ", DateTime.Now)
                };
                updateQuery.Append("UPDATE " + Table.SysUser + " SET ");

                if (!string.IsNullOrEmpty(email))
                {
                    updateQuery.Append("c_email = @Email, ");
                    updateQuery.Append("c_isemailverified = 1, ");
                    parameters.Add(new SqlParameter("@Email", email));
                }

                if (!string.IsNullOrEmpty(pictureUrl))
                {
                    updateQuery.Append("c_picture = @PictureUrl, ");
                    parameters.Add(new SqlParameter("@PictureUrl", pictureUrl));
                }
                if (!string.IsNullOrEmpty(phone))
                {
                    updateQuery.Append("c_phone = @Phone, ");
                    updateQuery.Append("c_isphoneverified = 1, ");
                    parameters.Add(new SqlParameter("@Phone", phone));
                }

                if (stateID > 0 && cityID > 0)
                {
                    updateQuery.Append("c_stateid = @StateID, ");
                    updateQuery.Append("c_cityid = @CityID, ");
                    parameters.Add(new SqlParameter("@StateID", stateID));
                    parameters.Add(new SqlParameter("@CityID", cityID));
                }

                if (!string.IsNullOrEmpty(Description))
                {
                    updateQuery.Append("c_description = @Description, ");
                    parameters.Add(new SqlParameter("@Description", Description));
                }
                updateQuery.Append("c_modifieddate = @ModifiedDate, ");

                // Remove the last comma and space
                if (updateQuery.Length > 0)
                {
                    updateQuery.Length -= 2; // Remove the last ", "
                }

                updateQuery.Append(" WHERE c_userid = @UserPKID");


                _db.ExecuteNonQuery(updateQuery.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                throw new Exception("Error updating user details: " + ex.Message);

            }
        }
    }
}
