using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.User;
using Npgsql;
using System.Text;

namespace CateringEcommerce.BAL.Base.User.Profile
{
    public class ProfileSetting : IProfileSetting
    {
        private readonly IDatabaseHelper _dbHelper;
        public ProfileSetting(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task UpdateUserDetails(long? userPKID, Dictionary<string, string> dicData = null)
        {
            if (userPKID == null || userPKID <= 0)
                throw new ArgumentException("Invalid UserPKID");

            try
            {
                var updates = new List<string>();
                var parameters = new List<NpgsqlParameter>();

                // Mandatory parameter
                parameters.Add(new NpgsqlParameter("@UserPKID", userPKID));

                // Always update modified date
                updates.Add("c_modifieddate = @ModifiedDate");
                parameters.Add(new NpgsqlParameter("@ModifiedDate", DateTime.UtcNow)); // ✅ FIXED (no space)

                // Extract values safely
                string email = dicData?.GetValueOrDefault("email");
                string phone = dicData?.GetValueOrDefault("phone");
                string pictureUrl = dicData?.GetValueOrDefault("pictureUrl");
                string description = dicData?.GetValueOrDefault("description");

                int stateID = int.TryParse(dicData?.GetValueOrDefault("stateID"), out var sId) ? sId : 0;
                int cityID = int.TryParse(dicData?.GetValueOrDefault("cityID"), out var cId) ? cId : 0;

                // Conditional updates

                if (!string.IsNullOrWhiteSpace(email))
                {
                    updates.Add("c_email = @Email");
                    updates.Add("c_isemailverified = TRUE");
                    parameters.Add(new NpgsqlParameter("@Email", email));
                }

                if (!string.IsNullOrWhiteSpace(pictureUrl))
                {
                    updates.Add("c_picture = @PictureUrl");
                    parameters.Add(new NpgsqlParameter("@PictureUrl", pictureUrl));
                }

                if (!string.IsNullOrWhiteSpace(phone))
                {
                    updates.Add("c_phone = @Phone");
                    updates.Add("c_isphoneverified = TRUE");
                    parameters.Add(new NpgsqlParameter("@Phone", phone));
                }

                if (stateID > 0 && cityID > 0)
                {
                    updates.Add("c_stateid = @StateID");
                    updates.Add("c_cityid = @CityID");
                    parameters.Add(new NpgsqlParameter("@StateID", stateID));
                    parameters.Add(new NpgsqlParameter("@CityID", cityID));
                }

                if (!string.IsNullOrWhiteSpace(description))
                {
                    updates.Add("c_description = @Description");
                    parameters.Add(new NpgsqlParameter("@Description", description));
                }

                // Build final query safely
                string query = $@"
                    UPDATE {Table.SysUser}
                    SET {string.Join(", ", updates)}
                    WHERE c_userid = @UserPKID;
                ";

                // Execute
                await _dbHelper.ExecuteNonQueryAsync(query, parameters.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating user details: " + ex.Message, ex);
            }
        }

        public async Task<string> GetUserProfilePicture(long userPkid)
        {
            try
            {
                const string query = "SELECT c_picture FROM " + Table.SysUser + " WHERE c_userid = @UserPKID";
                
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>
                {
                    new NpgsqlParameter("@UserPKID", userPkid)
                };

                var result = await _dbHelper.ExecuteScalarAsync(query, parameters.ToArray());

                // Return the picture URL if it exists and is not empty, otherwise return empty string
                return !string.IsNullOrEmpty(result?.ToString()) ? result.ToString() : string.Empty;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                throw new Exception($"Error retrieving user profile picture for UserId {userPkid}: " + ex.Message);
            }
        }
    }
}
