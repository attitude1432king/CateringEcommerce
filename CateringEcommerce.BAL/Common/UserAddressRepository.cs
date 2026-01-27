using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Models.User;
using Microsoft.Data.SqlClient;

namespace CateringEcommerce.BAL.Common
{
    public class UserAddressRepository
    {
        private readonly SqlDatabaseManager _db;

        public UserAddressRepository(string connectionString)
        {
            _db = new SqlDatabaseManager();
            _db.SetConnectionString(connectionString);
        }

        // ===================================
        // GET USER ADDRESSES
        // ===================================
        public async Task<List<SavedAddressDto>> GetUserAddressesAsync(long userId)
        {
            try
            {
                string query = $@"
                    SELECT
                        c_address_id, c_userid, c_address_label, c_full_address, c_landmark,
                        c_city, c_state, c_pincode, c_contact_person, c_contact_phone,
                        c_is_default, c_created_date, c_isactive
                    FROM {Table.SysUserAddresses}
                    WHERE c_userid = @UserId AND c_isactive = 1
                    ORDER BY c_is_default DESC, c_created_date DESC
                ";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserId", userId)
                };

                DataTable dt = await _db.ExecuteAsync(query, parameters);
                List<SavedAddressDto> addresses = new List<SavedAddressDto>();

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        addresses.Add(new SavedAddressDto
                        {
                            AddressId = Convert.ToInt64(row["c_address_id"]),
                            UserId = Convert.ToInt64(row["c_userid"]),
                            AddressLabel = row["c_address_label"].ToString() ?? string.Empty,
                            FullAddress = row["c_full_address"].ToString() ?? string.Empty,
                            Landmark = row["c_landmark"] != DBNull.Value ? row["c_landmark"].ToString() : null,
                            City = row["c_city"].ToString() ?? string.Empty,
                            State = row["c_state"].ToString() ?? string.Empty,
                            Pincode = row["c_pincode"].ToString() ?? string.Empty,
                            ContactPerson = row["c_contact_person"].ToString() ?? string.Empty,
                            ContactPhone = row["c_contact_phone"].ToString() ?? string.Empty,
                            IsDefault = Convert.ToBoolean(row["c_is_default"]),
                            CreatedDate = Convert.ToDateTime(row["c_created_date"]),
                            IsActive = Convert.ToBoolean(row["c_isactive"])
                        });
                    }
                }

                return addresses;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving user addresses: " + ex.Message, ex);
            }
        }

        // ===================================
        // GET ADDRESS BY ID
        // ===================================
        public async Task<SavedAddressDto?> GetAddressByIdAsync(long addressId, long userId)
        {
            try
            {
                string query = $@"
                    SELECT
                        c_address_id, c_userid, c_address_label, c_full_address, c_landmark,
                        c_city, c_state, c_pincode, c_contact_person, c_contact_phone,
                        c_is_default, c_created_date, c_isactive
                    FROM {Table.SysUserAddresses}
                    WHERE c_address_id = @AddressId AND c_userid = @UserId AND c_isactive = 1
                ";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@AddressId", addressId),
                    new SqlParameter("@UserId", userId)
                };

                DataTable dt = await _db.ExecuteAsync(query, parameters);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    return new SavedAddressDto
                    {
                        AddressId = Convert.ToInt64(row["c_address_id"]),
                        UserId = Convert.ToInt64(row["c_userid"]),
                        AddressLabel = row["c_address_label"].ToString() ?? string.Empty,
                        FullAddress = row["c_full_address"].ToString() ?? string.Empty,
                        Landmark = row["c_landmark"] != DBNull.Value ? row["c_landmark"].ToString() : null,
                        City = row["c_city"].ToString() ?? string.Empty,
                        State = row["c_state"].ToString() ?? string.Empty,
                        Pincode = row["c_pincode"].ToString() ?? string.Empty,
                        ContactPerson = row["c_contact_person"].ToString() ?? string.Empty,
                        ContactPhone = row["c_contact_phone"].ToString() ?? string.Empty,
                        IsDefault = Convert.ToBoolean(row["c_is_default"]),
                        CreatedDate = Convert.ToDateTime(row["c_created_date"]),
                        IsActive = Convert.ToBoolean(row["c_isactive"])
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving address: " + ex.Message, ex);
            }
        }

        // ===================================
        // COUNT USER ADDRESSES
        // ===================================
        public async Task<int> CountUserAddressesAsync(long userId)
        {
            try
            {
                string query = $@"
                    SELECT COUNT(*)
                    FROM {Table.SysUserAddresses}
                    WHERE c_userid = @UserId AND c_isactive = 1
                ";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserId", userId)
                };

                DataTable dt = await _db.ExecuteAsync(query, parameters);
                if (dt.Rows.Count > 0)
                {
                    return Convert.ToInt32(dt.Rows[0][0]);
                }

                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error counting user addresses: " + ex.Message, ex);
            }
        }

        // ===================================
        // INSERT ADDRESS
        // ===================================
        public async Task<long> InsertAddressAsync(long userId, CreateAddressDto addressData)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append($@"
                    INSERT INTO {Table.SysUserAddresses} (
                        c_userid, c_address_label, c_full_address, c_landmark, c_city, c_state,
                        c_pincode, c_contact_person, c_contact_phone, c_is_default, c_created_date, c_isactive
                    ) VALUES (
                        @UserId, @AddressLabel, @FullAddress, @Landmark, @City, @State,
                        @Pincode, @ContactPerson, @ContactPhone, @IsDefault, GETDATE(), 1
                    );
                    SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
                ");

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@AddressLabel", addressData.AddressLabel),
                    new SqlParameter("@FullAddress", addressData.FullAddress),
                    new SqlParameter("@Landmark", (object)addressData.Landmark ?? DBNull.Value),
                    new SqlParameter("@City", addressData.City),
                    new SqlParameter("@State", addressData.State),
                    new SqlParameter("@Pincode", addressData.Pincode),
                    new SqlParameter("@ContactPerson", addressData.ContactPerson),
                    new SqlParameter("@ContactPhone", addressData.ContactPhone),
                    new SqlParameter("@IsDefault", addressData.IsDefault)
                };

                DataTable dt = await _db.ExecuteAsync(query.ToString(), parameters);
                if (dt.Rows.Count > 0)
                {
                    return Convert.ToInt64(dt.Rows[0][0]);
                }

                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error inserting address: " + ex.Message, ex);
            }
        }

        // ===================================
        // UPDATE ADDRESS
        // ===================================
        public async Task<bool> UpdateAddressAsync(long userId, UpdateAddressDto addressData)
        {
            try
            {
                string query = $@"
                    UPDATE {Table.SysUserAddresses}
                    SET
                        c_address_label = @AddressLabel,
                        c_full_address = @FullAddress,
                        c_landmark = @Landmark,
                        c_city = @City,
                        c_state = @State,
                        c_pincode = @Pincode,
                        c_contact_person = @ContactPerson,
                        c_contact_phone = @ContactPhone,
                        c_is_default = @IsDefault
                    WHERE c_address_id = @AddressId AND c_userid = @UserId AND c_isactive = 1
                ";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@AddressId", addressData.AddressId),
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@AddressLabel", addressData.AddressLabel),
                    new SqlParameter("@FullAddress", addressData.FullAddress),
                    new SqlParameter("@Landmark", (object)addressData.Landmark ?? DBNull.Value),
                    new SqlParameter("@City", addressData.City),
                    new SqlParameter("@State", addressData.State),
                    new SqlParameter("@Pincode", addressData.Pincode),
                    new SqlParameter("@ContactPerson", addressData.ContactPerson),
                    new SqlParameter("@ContactPhone", addressData.ContactPhone),
                    new SqlParameter("@IsDefault", addressData.IsDefault)
                };

                int rowsAffected = await _db.ExecuteNonQueryAsync(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating address: " + ex.Message, ex);
            }
        }

        // ===================================
        // DELETE ADDRESS (Soft Delete)
        // ===================================
        public async Task<bool> DeleteAddressAsync(long addressId, long userId)
        {
            try
            {
                string query = $@"
                    UPDATE {Table.SysUserAddresses}
                    SET c_isactive = 0
                    WHERE c_address_id = @AddressId AND c_userid = @UserId AND c_isactive = 1
                ";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@AddressId", addressId),
                    new SqlParameter("@UserId", userId)
                };

                int rowsAffected = await _db.ExecuteNonQueryAsync(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting address: " + ex.Message, ex);
            }
        }

        // ===================================
        // SET DEFAULT ADDRESS
        // ===================================
        public async Task<bool> SetDefaultAddressAsync(long addressId, long userId)
        {
            try
            {
                // First, unset all other default addresses for this user
                string unsetQuery = $@"
                    UPDATE {Table.SysUserAddresses}
                    SET c_is_default = 0
                    WHERE c_userid = @UserId AND c_isactive = 1
                ";

                SqlParameter[] unsetParams = new SqlParameter[]
                {
                    new SqlParameter("@UserId", userId)
                };

                await _db.ExecuteNonQueryAsync(unsetQuery, unsetParams);

                // Then set the specified address as default
                string setQuery = $@"
                    UPDATE {Table.SysUserAddresses}
                    SET c_is_default = 1
                    WHERE c_address_id = @AddressId AND c_userid = @UserId AND c_isactive = 1
                ";

                SqlParameter[] setParams = new SqlParameter[]
                {
                    new SqlParameter("@AddressId", addressId),
                    new SqlParameter("@UserId", userId)
                };

                int rowsAffected = await _db.ExecuteNonQueryAsync(setQuery, setParams);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error setting default address: " + ex.Message, ex);
            }
        }
    }
}
