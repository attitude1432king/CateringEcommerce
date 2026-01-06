using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace CateringEcommerce.BAL.Base.Owner
{
    public class OwnerRegister
    {
        private readonly SqlDatabaseManager _db;

        public OwnerRegister(string connectionString)
        {
            _db = new SqlDatabaseManager();
            _db.SetConnectionString(connectionString);
        }

        public Int64 CreateOwnerAccount(Dictionary<string, object> dicData)
        {
            try
            {
                #region Business/Account Details Variables
                string cateringName = dicData.ContainsKey("CateringName") ? dicData["CateringName"].ToString() : null;
                string mobileNumber = dicData.ContainsKey("Mobile") ? dicData["Mobile"].ToString() : null;
                string email = dicData.ContainsKey("Email") ? dicData["Email"].ToString() : null;
                string cateringNumber = dicData.ContainsKey("CateringNumber") ? dicData["CateringNumber"].ToString() : null;
                string ownerName = dicData.ContainsKey("OwnerName") ? dicData["OwnerName"].ToString() : null;
                string stdNumber = dicData.ContainsKey("StdNumber") ? dicData["StdNumber"].ToString() : null;
                bool isSameContact = dicData.ContainsKey("IsSameContact") ? Convert.ToBoolean(dicData["IsSameContact"]) : false;
                string supportContact = dicData.ContainsKey("SupportContact") ? dicData["SupportContact"].ToString() : null;
                string alternateEmail = dicData.ContainsKey("AlternateEmail") ? dicData["AlternateEmail"].ToString() : null;
                string whatsappNumber = dicData.ContainsKey("WhatsappNumber") ? dicData["WhatsappNumber"].ToString() : null;
                #endregion

                StringBuilder query = new StringBuilder();
                query.Append($@"
                    INSERT INTO {Table.SysCateringOwner} 
                    (c_catering_name, c_mobile, c_catering_number, c_owner_name, c_email, c_std_number, c_same_contact, c_phone_verified, c_email_verified,
                     c_support_contact_number, c_alternate_email, c_whatsapp_number, c_createddate)
                    VALUES
                    (@CateringName, @Mobile, @CateringNumber, @OwnerName, @Email, @StdNumber, @IsSameContact, @IsPhoneVerify, @IsEmailVerify,
                    @SupportContact, @AlternateEmail, @WhatsappNumber, @CreatedDate);
                    SELECT CAST(SCOPE_IDENTITY() AS int);
                ");

                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@CateringName", cateringName),
                    new SqlParameter("@Mobile", mobileNumber),
                    new SqlParameter("@CateringNumber", cateringNumber),
                    new SqlParameter("@OwnerName", ownerName),
                    new SqlParameter("@Email", email),
                    new SqlParameter("@StdNumber", string.IsNullOrEmpty(stdNumber) ? DBNull.Value : stdNumber),
                    new SqlParameter("@IsSameContact", isSameContact.ToBinary()),
                    new SqlParameter("@IsPhoneVerify", true.ToBinary()),
                    new SqlParameter("@IsEmailVerify", true.ToBinary()),
                    new SqlParameter("@SupportContact", string.IsNullOrEmpty(supportContact) ? DBNull.Value : supportContact),
                    new SqlParameter("@AlternateEmail", string.IsNullOrEmpty(alternateEmail) ? DBNull.Value : alternateEmail),
                    new SqlParameter("@WhatsappNumber", string.IsNullOrEmpty(whatsappNumber) ? DBNull.Value : whatsappNumber),
                    new SqlParameter("@CreatedDate", DateTime.Now)
                };

                var result = _db.ExecuteScalar(query.ToString(), parameters.ToArray());
                return result != null ? Convert.ToInt64(result) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void RegisterAddress(Int64 onwerId, Dictionary<string, object> dicData)
        {
            if (dicData == null || dicData.Count == 0)
                throw new ArgumentException("Address data cannot be null or empty.", nameof(dicData));
            StringBuilder query = new StringBuilder();
            #region Address Variables 
            string addressBuilding = dicData.ContainsKey("ShopNo") ? dicData["ShopNo"].ToString() : null;
            string addressStreet = dicData.ContainsKey("Street") ? dicData["Street"].ToString() : null; //  Tower or Street data
            string addressArea = dicData.ContainsKey("Area") ? dicData["Area"].ToString() : null;
            int stateID = dicData != null && dicData.ContainsKey("StateID") ? Convert.ToInt16(dicData["StateID"]) : 0;
            int cityID = dicData != null && dicData.ContainsKey("CityID") ? Convert.ToInt16(dicData["CityID"]) : 0;
            string pincode = dicData.ContainsKey("Pincode") ? dicData["Pincode"].ToString() : null;
            string latitude = dicData.ContainsKey("Latitude") ? dicData["Latitude"].ToString() : null;
            string longitude = dicData.ContainsKey("Longitude") ? dicData["Longitude"].ToString() : null;
            string mapUrl = dicData.ContainsKey("MapUrl") ? dicData["MapUrl"].ToString() : null;
            #endregion

            try
            {
                query.Append($@"INSERT INTO {Table.SysCateringOwnerAddress} 
                    (c_ownerid, c_building, c_street, c_area, c_city, c_state, c_pincode, c_latitude, c_longitude, c_mapurl,c_createddate) 
                    VALUES (@OwnerId, @Building, @Street, @Area, @CityId, @StateId, @Pincode, @Latitude, @Longitude, @MapUrl, @Createddate)");
                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerId", onwerId),
                    new SqlParameter("@Building", addressBuilding),
                    new SqlParameter("@Street", addressStreet),
                    new SqlParameter("@Area", addressArea),
                    new SqlParameter("@City", cityID),
                    new SqlParameter("@State", stateID),
                    new SqlParameter("@Pincode", pincode),
                    new SqlParameter("@Latitude", latitude),
                    new SqlParameter("@Longitude", longitude),
                    new SqlParameter("@MapUrl", !string.IsNullOrEmpty(mapUrl) ? mapUrl : DBNull.Value),
                    new SqlParameter("@Createddate", DateTime.Now)

                };
                _db.ExecuteNonQuery(query.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception("Error while registering address: " + ex.Message);
            }
        }

        public void RegisterServices(Int64 ownerId, Dictionary<string, object> dicData)
        {
            if (dicData == null || dicData.Count == 0)
                throw new ArgumentException("Services data cannot be null or empty.", nameof(dicData));
            StringBuilder query = new StringBuilder();
            #region Services Variables
            string serviceTypes = dicData.ContainsKey("ServiceTypes") ? dicData["ServiceTypes"].ToString(): null;
            string cuisines = dicData.ContainsKey("Cuisines") ? dicData["Cuisines"].ToString() : null;
            string eventTypes = dicData.ContainsKey("EventTypes") ? dicData["EventTypes"].ToString() : null;
            string foodTypes = dicData.ContainsKey("FoodTypes") ? dicData["FoodTypes"].ToString() : null;
            int minOrderValue = dicData.ContainsKey("MinOrderValue") ? Convert.ToInt32(dicData["MinOrderValue"]) : 0;
            int deliveryRediusKM = dicData.ContainsKey("DeliveryRediusKM") ? Convert.ToInt32(dicData["DeliveryRediusKM"]) : 0;
            string servingTimeSlots = dicData.ContainsKey("ServingTimeSlots") ? dicData["ServingTimeSlots"].ToString() : null;
            #endregion
            try
            {
                query.Append($@"INSERT INTO {Table.SysCateringOwnerService} 
                    (c_ownerid, c_cuisine_types, c_service_types, c_event_types, c_food_types, c_min_dish_order, c_delivery_radius_km, c_serving_time_slots, c_createddate) 
                    VALUES (@OwnerId, @CuisineType, @ServiceTypes, @EventTypes, @FoodTypes, @MinDishOrder, @RadiusKm, @ServingSlots, @Createddate)");
                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerId", ownerId),
                    new SqlParameter("@CuisineType", cuisines),
                    new SqlParameter("@ServiceTypes", serviceTypes),
                    new SqlParameter("@EventTypes", eventTypes),
                    new SqlParameter("@FoodTypes", foodTypes),
                    new SqlParameter("@MinDishOrder", minOrderValue),
                    new SqlParameter("@RadiusKm", deliveryRediusKM > 0 ? deliveryRediusKM : DBNull.Value),
                    new SqlParameter("@ServingSlots", !string.IsNullOrEmpty(servingTimeSlots) ? servingTimeSlots : DBNull.Value),
                    new SqlParameter("@Createddate", DateTime.Now)
                };
                _db.ExecuteNonQuery(query.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception("Error while registering services: " + ex.Message);
            }
        }

        public void RegisterLegalDocuments(Int64 ownerId, Dictionary<string, object> dicData)
        {
            if (dicData == null || dicData.Count == 0)
                throw new ArgumentException("Legal documents data cannot be null or empty.", nameof(dicData));
            StringBuilder query = new StringBuilder();
            #region Legal Documents Variables
            string fssaiNumber = dicData.ContainsKey("FssaiNumber") ? dicData["FssaiNumber"].ToString() : null;
            string fssaiExpiry = dicData.ContainsKey("FssaiExpiryDate") ? dicData["FssaiExpiryDate"].ToString(): null;
            DateTime fssaiExpiryDate = !string.IsNullOrEmpty(fssaiExpiry) && DateHelper.ParseDate(fssaiExpiry).HasValue
                ? DateHelper.ParseDate(fssaiExpiry).Value
                : DateTime.MinValue;
            string fssaiCertificate = dicData.ContainsKey("FssaiCertificatePath") ? dicData["FssaiCertificatePath"].ToString() : null;
            bool isGstApplicable = dicData.ContainsKey("IsGstApplicable") ? Convert.ToBoolean(dicData["IsGstApplicable"]) : false;
            string gstNumber = dicData.ContainsKey("GstNumber") ? dicData["GstNumber"].ToString() : null;
            string gstCertificate = dicData.ContainsKey("GstCertificatePath") ? dicData["GstCertificatePath"].ToString() : null;
            string panName = dicData.ContainsKey("PanHolderName") ? dicData["PanHolderName"].ToString() : null;
            string panCertificate = dicData.ContainsKey("PanCertificatePath") ? dicData["PanCertificatePath"].ToString() : null;
            string panNumber = dicData.ContainsKey("PanNumber") ? dicData["PanNumber"].ToString() : null;

            #endregion
            try
            {
                query.Append($@"INSERT INTO {Table.SysCateringOwnerLegal} 
                    (c_ownerid, c_fssai_number, c_fssai_expiry_date, c_fssai_certificate_path, c_gst_applicable, c_gst_number, c_gst_certificate_path,
                    c_pan_name, c_pan_number, c_pan_file_path, c_createddate) 
                    VALUES (@OwnerId, @FssaiNumber, @FssaiExpDate, @FssaiCertificate, @GstApplicable, @GstNumber, @GstCertificate,
                    @PanName , @PanNumber, @PanCertificate, @Createddate)");
                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerId", ownerId),
                    new SqlParameter("@FssaiNumber", fssaiNumber),
                    new SqlParameter("@FssaiExpDate", fssaiExpiryDate.Date.ToString()),
                    new SqlParameter("@FssaiCertificate", string.IsNullOrEmpty(fssaiCertificate) ? DBNull.Value : fssaiCertificate),
                    new SqlParameter("@GstApplicable", isGstApplicable.ToBinary()),
                    new SqlParameter("@GstNumber", gstNumber),
                    new SqlParameter("@GstCertificate", string.IsNullOrEmpty(gstCertificate) ? DBNull.Value : gstCertificate),
                    new SqlParameter("@PanName", panName),
                    new SqlParameter("@PanNumber", panNumber),
                    new SqlParameter("@PanCertificate", string.IsNullOrEmpty(panCertificate) ? DBNull.Value : panCertificate),
                    new SqlParameter("@Createddate", DateTime.Now)
                };
                _db.ExecuteNonQuery(query.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception("Error while registering legal documents: " + ex.Message);
            }
        }

        public void RegisterBankDetails(Int64 ownerId, Dictionary<string, object> dicData)
        {
            if (dicData == null || dicData.Count == 0)
                throw new ArgumentException("Bank details data cannot be null or empty.", nameof(dicData));
            StringBuilder query = new StringBuilder();
            #region Bank Details Variables
            string bankAccountNumber = dicData.ContainsKey("BankAccountNumber") ? dicData["BankAccountNumber"].ToString() : null;
            string accountHolderName = dicData.ContainsKey("BankAccountName") ? dicData["BankAccountName"].ToString() : null;
            string chequePath = dicData.ContainsKey("ChequePath") ? dicData["ChequePath"].ToString() : null;
            string upiId = dicData.ContainsKey("UpiId") ? dicData["UpiId"].ToString() : null;
            string ifscCode = dicData.ContainsKey("IfscCode") ? dicData["IfscCode"].ToString() : null;
            #endregion
            try
            {
                query.Append($@"INSERT INTO {Table.SysCateringOwnerBankDetails} 
                    (c_ownerid, c_account_holder_name, c_account_number, c_ifsc_code, c_cheque_path, c_upi_id, c_createddate) 
                    VALUES (@OwnerId, @AccountHolderName, @AccountNumber, @IfscCode, @ChequePath, @UpiId, @Createddate)");
                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerId", ownerId),
                    new SqlParameter("@AccountHolderName", accountHolderName),
                    new SqlParameter("@AccountNumber", bankAccountNumber),
                    new SqlParameter("@IfscCode", ifscCode),
                    new SqlParameter("@ChequePath", !string.IsNullOrEmpty(chequePath) ? chequePath : DBNull.Value),
                    new SqlParameter("@UpiId", !string.IsNullOrEmpty(upiId) ? upiId : DBNull.Value),
                    new SqlParameter("@Createddate", DateTime.Now)
                };
                _db.ExecuteNonQuery(query.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception("Error while registering bank details: " + ex.Message);
            }
        }

        public void UpdateLogoPath(Int64 ownerPkid, string logoPath)
        {
            if (ownerPkid <= 0)
                throw new ArgumentException("Partner PKID must be greater than zero.", nameof(ownerPkid));
            if (string.IsNullOrEmpty(logoPath))
                throw new ArgumentException("Logo path cannot be null or empty.", nameof(logoPath));
            StringBuilder query = new StringBuilder();
            query.Append($@"UPDATE {Table.SysCateringOwner} SET c_logo_path = @LogoPath WHERE c_ownerid = @OwnerPkid");
            List<SqlParameter> parameters = new()
            {
                new SqlParameter("@LogoPath", logoPath),
                new SqlParameter("@OwnerPkid", ownerPkid)
            };
            _db.ExecuteNonQuery(query.ToString(), parameters.ToArray());
        }

        public async Task<List<ServiceTypeDetails>> GetServiceDetailsByTypeId(int serviceTypeId)
        {
            if (serviceTypeId <= 0)
                throw new ArgumentException("Service Type ID must be greater than zero.", nameof(serviceTypeId));
            List<ServiceTypeDetails> serviceTypes = new List<ServiceTypeDetails>();
            StringBuilder query = new StringBuilder();
            query.Append($@"SELECT c_type_id AS TypeId, c_type_name AS ServiceName, c_description AS Description, c_is_active AS IsActive
                            FROM {Table.SysCateringTypeMaster} 
                            WHERE c_category_id = @ServiceTypeId AND c_is_active = 1");

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@ServiceTypeId", serviceTypeId)
            };

            try
            {
                // Use Task.Run to execute the database operation asynchronously
                DataTable serviceDetails = await Task.Run(() => _db.Execute(query.ToString(), parameters.ToArray()));

                if (serviceDetails == null || serviceDetails.Rows.Count == 0)
                {
                    throw new Exception("No service details found for the provided Service Type ID.");
                }
                foreach (DataRow row in serviceDetails.Rows)
                {
                    ServiceTypeDetails serviceType = new ServiceTypeDetails
                    {
                        TypeId = Convert.ToInt32(row["TypeId"]),
                        ServiceName = row["ServiceName"].ToString(),
                        Description = row["Description"] != DBNull.Value ? row["Description"].ToString() : string.Empty,
                        IsActive = Convert.ToBoolean(row["IsActive"])
                    };
                    serviceTypes.Add(serviceType);
                }
                return serviceTypes;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while fetching service details: " + ex.Message);
            }
        }
    }
}
