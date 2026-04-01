using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace CateringEcommerce.BAL.Base.Owner
{
    public class OwnerRegister: IOwnerRegister
    {
        private readonly IDatabaseHelper _dbHelper;
        public OwnerRegister(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
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

                var result = _dbHelper.ExecuteScalar(query.ToString(), parameters.ToArray());
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
                    (c_ownerid, c_building, c_street, c_area, c_cityid, c_stateid, c_pincode, c_latitude, c_longitude, c_mapurl,c_createddate) 
                    VALUES (@OwnerId, @Building, @Street, @Area, @CityId, @StateId, @Pincode, @Latitude, @Longitude, @MapUrl, @Createddate)");
                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerId", onwerId),
                    new SqlParameter("@Building", addressBuilding),
                    new SqlParameter("@Street", addressStreet),
                    new SqlParameter("@Area", addressArea),
                    new SqlParameter("@CityId", cityID),
                    new SqlParameter("@StateId", stateID),
                    new SqlParameter("@Pincode", pincode),
                    new SqlParameter("@Latitude", latitude),
                    new SqlParameter("@Longitude", longitude),
                    new SqlParameter("@MapUrl", !string.IsNullOrEmpty(mapUrl) ? mapUrl : DBNull.Value),
                    new SqlParameter("@Createddate", DateTime.Now)

                };
                _dbHelper.ExecuteNonQuery(query.ToString(), parameters.ToArray());
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
            int dailyBookingCapacity = dicData.ContainsKey("DailyBookingCapacity") ? Convert.ToInt32(dicData["DailyBookingCapacity"]) : 0;
            string servingTimeSlots = dicData.ContainsKey("ServingTimeSlots") ? dicData["ServingTimeSlots"].ToString() : null;
            #endregion
            try
            {
                query.Append($@"INSERT INTO {Table.SysCateringOwnerService} 
                    (c_ownerid, c_cuisine_types, c_service_types, c_event_types, c_food_types, c_min_dish_order, c_delivery_radius_km, c_daily_booking_capacity, c_serving_time_slots, c_createddate) 
                    VALUES (@OwnerId, @CuisineType, @ServiceTypes, @EventTypes, @FoodTypes, @MinDishOrder, @RadiusKm, @DailyBookingCapacity, @ServingSlots, @Createddate)");
                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerId", ownerId),
                    new SqlParameter("@CuisineType", cuisines),
                    new SqlParameter("@ServiceTypes", serviceTypes),
                    new SqlParameter("@EventTypes", eventTypes),
                    new SqlParameter("@FoodTypes", foodTypes),
                    new SqlParameter("@MinDishOrder", minOrderValue),
                    new SqlParameter("@RadiusKm", deliveryRediusKM > 0 ? deliveryRediusKM : DBNull.Value),
                    new SqlParameter("@DailyBookingCapacity", dailyBookingCapacity > 0 ? dailyBookingCapacity : DBNull.Value),
                    new SqlParameter("@ServingSlots", !string.IsNullOrEmpty(servingTimeSlots) ? servingTimeSlots : DBNull.Value),
                    new SqlParameter("@Createddate", DateTime.Now)
                };
                _dbHelper.ExecuteNonQuery(query.ToString(), parameters.ToArray());
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
                _dbHelper.ExecuteNonQuery(query.ToString(), parameters.ToArray());
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
                _dbHelper.ExecuteNonQuery(query.ToString(), parameters.ToArray());
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
            string requestNumber = GenerateRunningNumber(ownerPkid, "PR");
            query.Append($@"UPDATE {Table.SysCateringOwner} SET c_logo_path = @LogoPath, c_partnernumber = @PartnerNumber WHERE c_ownerid = @OwnerPkid");
            List<SqlParameter> parameters = new()
            {
                new SqlParameter("@LogoPath", logoPath),
                new SqlParameter("@OwnerPkid", ownerPkid),
                new SqlParameter("@PartnerNumber", requestNumber)
            };
            _dbHelper.ExecuteNonQuery(query.ToString(), parameters.ToArray());
        }

        public void RegisterAgreement(Int64 ownerId, Dictionary<string, object> dicData, string baseUploadPath)
        {
            if (dicData == null || dicData.Count == 0)
                throw new ArgumentException("Agreement data cannot be null or empty.", nameof(dicData));

            StringBuilder query = new StringBuilder();

            #region Agreement Variables
            bool agreementAccepted = dicData.ContainsKey("AgreementAccepted") ? Convert.ToBoolean(dicData["AgreementAccepted"]) : false;
            string signaturePath = dicData.ContainsKey("SignaturePath") ? dicData["SignaturePath"].ToString() : null;
            string signatureBase64 = dicData.ContainsKey("SignatureBase64") ? dicData["SignatureBase64"].ToString() : null;
            string agreementText = dicData.ContainsKey("AgreementText") ? dicData["AgreementText"].ToString() : null;
            string businessName = dicData.ContainsKey("CateringName") ? dicData["CateringName"].ToString() : string.Empty;
            string ownerName = dicData.ContainsKey("OwnerName") ? dicData["OwnerName"].ToString() : string.Empty;
            string ipAddress = dicData.ContainsKey("IpAddress") ? dicData["IpAddress"].ToString() : null;
            string userAgent = dicData.ContainsKey("UserAgent") ? dicData["UserAgent"].ToString() : null;
            #endregion

            string agreementPdfPath = null;

            try
            {
                // Generate Agreement PDF if we have the required data
                if (!string.IsNullOrEmpty(agreementText) && !string.IsNullOrEmpty(signatureBase64))
                {
                    try
                    {
                        // Generate PDF using the AgreementPdfGenerator
                        byte[] pdfBytes = Services.AgreementPdfGenerator.GenerateAgreementPdf(
                            agreementText,
                            businessName,
                            ownerName,
                            signatureBase64,
                            DateTime.Now
                        );

                        // Save PDF to secure_uploads/owner_{id}/agreements/
                        agreementPdfPath = Services.AgreementPdfGenerator.SaveAgreementPdf(
                            pdfBytes,
                            ownerId,
                            baseUploadPath
                        );
                    }
                    catch (Exception pdfEx)
                    {
                        // Log the PDF generation error but don't fail the entire registration
                        Console.WriteLine($"Warning: Failed to generate agreement PDF: {pdfEx.Message}");
                    }
                }

                // Insert agreement record into database
                query.Append($@"INSERT INTO {Table.SysCateringOwnerAgreement}
                    (c_ownerid, c_agreement_text, c_agreement_accepted, c_signature_data, c_signature_path,
                     c_agreement_pdf_path, c_ip_address, c_user_agent, c_accepted_date, c_createddate)
                    VALUES (@OwnerId, @AgreementText, @AgreementAccepted, @SignatureData, @SignaturePath,
                            @AgreementPdfPath, @IpAddress, @UserAgent, @AcceptanceDate, @Createddate)");

                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerId", ownerId),
                    new SqlParameter("@AgreementText", string.IsNullOrEmpty(agreementText) ? DBNull.Value : agreementText),
                    new SqlParameter("@AgreementAccepted", agreementAccepted.ToBinary()),
                    new SqlParameter("@SignatureData", string.IsNullOrEmpty(signatureBase64) ? DBNull.Value : signatureBase64),
                    new SqlParameter("@SignaturePath", string.IsNullOrEmpty(signaturePath) ? DBNull.Value : signaturePath),
                    new SqlParameter("@AgreementPdfPath", string.IsNullOrEmpty(agreementPdfPath) ? DBNull.Value : agreementPdfPath),
                    new SqlParameter("@IpAddress", string.IsNullOrEmpty(ipAddress) ? DBNull.Value : ipAddress),
                    new SqlParameter("@UserAgent", string.IsNullOrEmpty(userAgent) ? DBNull.Value : userAgent),
                    new SqlParameter("@AcceptanceDate", DateTime.Now),
                    new SqlParameter("@Createddate", DateTime.Now)
                };

                _dbHelper.ExecuteNonQuery(query.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception("Error while registering agreement: " + ex.Message);
            }
        }

        public async Task<List<ServiceTypeDetails>> GetServiceDetailsByTypeId(int serviceTypeId)
        {
            if (serviceTypeId <= 0)
                throw new ArgumentException("Service Type ID must be greater than zero.", nameof(serviceTypeId));
            List<ServiceTypeDetails> serviceTypes = new List<ServiceTypeDetails>();
            StringBuilder query = new StringBuilder();
            query.Append($@"SELECT c_typeid AS TypeId, c_type_name AS ServiceName, c_description AS Description, c_is_active AS IsActive
                            FROM {Table.SysCateringTypeMaster} 
                            WHERE c_categoryid = @ServiceTypeId AND c_isactive = 1");

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@ServiceTypeId", serviceTypeId)
            };

            try
            {
                // Use Task.Run to execute the database operation asynchronously
                DataTable serviceDetails = await Task.Run(() => _dbHelper.Execute(query.ToString(), parameters.ToArray()));

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

        private string GenerateRunningNumber(long pkid, string prefix, int sequenceLength = 4)
        {
            if (pkid <= 0)
                throw new ArgumentException("PKID must be greater than zero.");

            if (string.IsNullOrWhiteSpace(prefix))
                throw new ArgumentException("Prefix cannot be empty.");

            int year = DateTime.UtcNow.Year;

            // 🔑 Auto-adjust sequence length
            int actualLength = Math.Max(sequenceLength, pkid.ToString().Length);

            string paddedNumber = pkid.ToString().PadLeft(actualLength, '0');

            return $"{prefix}-{year}-{paddedNumber}";
        }

    }
}
