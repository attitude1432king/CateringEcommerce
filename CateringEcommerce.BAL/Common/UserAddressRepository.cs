using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Models.User;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CateringEcommerce.BAL.Common
{
    public class UserAddressRepository
    {
        private readonly IDatabaseHelper _dbHelper;
        public UserAddressRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
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
                        c_is_default, c_createddate, c_isactive
                    FROM {Table.SysUserAddresses}
                    WHERE c_userid = @UserId AND c_isactive = TRUE
                    ORDER BY c_is_default DESC, c_createddate DESC
                ";

                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@UserId", userId)
                };

                DataTable dt = await _dbHelper.ExecuteAsync(query, parameters);
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
                            CreatedDate = Convert.ToDateTime(row["c_createddate"]),
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
                        c_is_default, c_createddate, c_isactive
                    FROM {Table.SysUserAddresses}
                    WHERE c_address_id = @AddressId AND c_userid = @UserId AND c_isactive = TRUE
                ";

                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@AddressId", addressId),
                    new NpgsqlParameter("@UserId", userId)
                };

                DataTable dt = await _dbHelper.ExecuteAsync(query, parameters);

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
                        CreatedDate = Convert.ToDateTime(row["c_createddate"]),
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
                    WHERE c_userid = @UserId AND c_isactive = TRUE
                ";

                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@UserId", userId)
                };

                DataTable dt = await _dbHelper.ExecuteAsync(query, parameters);
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
                        c_pincode, c_contact_person, c_contact_phone, c_is_default, c_createddate, c_isactive
                    ) VALUES (
                        @UserId, @AddressLabel, @FullAddress, @Landmark, @City, @State,
                        @Pincode, @ContactPerson, @ContactPhone, @IsDefault, NOW(), TRUE
                    )
                    RETURNING c_address_id;
                ");

                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@UserId", userId),
                    new NpgsqlParameter("@AddressLabel", addressData.AddressLabel),
                    new NpgsqlParameter("@FullAddress", addressData.FullAddress),
                    new NpgsqlParameter("@Landmark", (object)addressData.Landmark ?? DBNull.Value),
                    new NpgsqlParameter("@City", addressData.City),
                    new NpgsqlParameter("@State", addressData.State),
                    new NpgsqlParameter("@Pincode", addressData.Pincode),
                    new NpgsqlParameter("@ContactPerson", addressData.ContactPerson),
                    new NpgsqlParameter("@ContactPhone", addressData.ContactPhone),
                    new NpgsqlParameter("@IsDefault", addressData.IsDefault)
                };

                DataTable dt = await _dbHelper.ExecuteAsync(query.ToString(), parameters);
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
                    WHERE c_address_id = @AddressId AND c_userid = @UserId AND c_isactive = TRUE
                ";

                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@AddressId", addressData.AddressId),
                    new NpgsqlParameter("@UserId", userId),
                    new NpgsqlParameter("@AddressLabel", addressData.AddressLabel),
                    new NpgsqlParameter("@FullAddress", addressData.FullAddress),
                    new NpgsqlParameter("@Landmark", (object)addressData.Landmark ?? DBNull.Value),
                    new NpgsqlParameter("@City", addressData.City),
                    new NpgsqlParameter("@State", addressData.State),
                    new NpgsqlParameter("@Pincode", addressData.Pincode),
                    new NpgsqlParameter("@ContactPerson", addressData.ContactPerson),
                    new NpgsqlParameter("@ContactPhone", addressData.ContactPhone),
                    new NpgsqlParameter("@IsDefault", addressData.IsDefault)
                };

                int rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
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
                    SET c_isactive = FALSE
                    WHERE c_address_id = @AddressId AND c_userid = @UserId AND c_isactive = TRUE
                ";

                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@AddressId", addressId),
                    new NpgsqlParameter("@UserId", userId)
                };

                int rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
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
                    SET c_is_default = FALSE
                    WHERE c_userid = @UserId AND c_isactive = TRUE
                ";

                NpgsqlParameter[] unsetParams = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@UserId", userId)
                };

                await _dbHelper.ExecuteNonQueryAsync(unsetQuery, unsetParams);

                // Then set the specified address as default
                string setQuery = $@"
                    UPDATE {Table.SysUserAddresses}
                    SET c_is_default = TRUE
                    WHERE c_address_id = @AddressId AND c_userid = @UserId AND c_isactive = TRUE
                ";

                NpgsqlParameter[] setParams = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@AddressId", addressId),
                    new NpgsqlParameter("@UserId", userId)
                };

                int rowsAffected = await _dbHelper.ExecuteNonQueryAsync(setQuery, setParams);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error setting default address: " + ex.Message, ex);
            }
        }
    }
}

