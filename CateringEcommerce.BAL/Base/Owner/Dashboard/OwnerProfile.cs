using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Models;
using CateringEcommerce.Domain.Models.APIModels.Owner;
using Microsoft.Data.SqlClient;
using System.Text;
using static CateringEcommerce.Domain.Models.APIModels.Owner.UpdateOwnerProfileDto;

namespace CateringEcommerce.BAL.Base.Owner.Dashboard
{
    public class OwnerProfile : IOwnerProfile
    {
        private readonly SqlDatabaseManager _db;

        public OwnerProfile(string connectionString)
        {
            _db = new SqlDatabaseManager();
            _db.SetConnectionString(connectionString);
        }

        public OwnerModel GetOwnerDetails(long ownerPKID)
        {
            try
            {
                OwnerModel ownerModel = new OwnerModel();
                ownerModel.OwnerBusiness = GetOwnerBusiness(ownerPKID);
                ownerModel.CateringAddress = GetCateringAddress(ownerPKID);
                ownerModel.CateringServices = GetOwnerServices(ownerPKID);
                ownerModel.CateringServices.CateringMedia = GetMediaPath(ownerPKID, DocumentType.Kitchen);
                ownerModel.OwnerLegalDocument = GetLegalDocumentsDetails(ownerPKID);
                return ownerModel;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public OwnerBusinessModel GetOwnerBusiness(Int64 onwerId)
        {

            try
            {
                string query = $@"SELECT c_catering_name AS CateringName, c_owner_name AS OwnerName, 
                    c_mobile AS Phone, c_email AS Email, c_catering_number AS CateringNumber, c_logo_path AS LogoPath, 
                    c_std_number AS StdNumber, c_support_contact_number AS SupportContact, 
                    c_alternate_email AS AlternateEmail, c_whatsapp_number AS WhatsappNumber 
                    FROM {Table.SysCateringOwner} WHERE c_ownerid = @OwnerId";

                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerId", onwerId)

                };

                var ownerData = _db.Execute(query.ToString(), parameters.ToArray());
                if (ownerData.Rows.Count > 0)
                {
                    var row = ownerData.Rows[0];
                    return new OwnerBusinessModel
                    {
                        CateringName = row["CateringName"]?.ToString(),
                        OwnerName = row["OwnerName"]?.ToString(),
                        Phone = row["Phone"]?.ToString(),
                        Email = row["Email"]?.ToString(),
                        CateringNumber = row["CateringNumber"]?.ToString(),
                        LogoPath = row["LogoPath"]?.ToString(),
                        StdNumber = row["StdNumber"]?.ToString(),
                        SupportContact = row["SupportContact"]?.ToString(),
                        AlternateEmail = row["AlternateEmail"]?.ToString(),
                        WhatsappNumber = row["WhatsappNumber"]?.ToString()
                    };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public CateringAddressModel GetCateringAddress(Int64 onwerId)
        {
            try
            {
                string query = $@"SELECT c_building AS Building, c_street AS Street, c_area AS Area, c_city AS City, 
                    c_state AS State, c_pincode AS Pincode, c_latitude AS Latitude, c_longitude AS Longitude, c_mapurl AS MapUrl 
                    FROM {Table.SysCateringOwnerAddress} WHERE c_ownerid = @OwnerId";
                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerId", onwerId)

                };
                var addressData = _db.Execute(query.ToString(), parameters.ToArray());

                if (addressData.Rows.Count > 0)
                {
                    var row = addressData.Rows[0];
                    return new CateringAddressModel
                    {
                        ShopNo = row["Building"]?.ToString(),
                        Street = row["Street"]?.ToString(),
                        Area = row["Area"]?.ToString(),
                        City = row["City"]?.ToString(),
                        State = row["State"]?.ToString(),
                        Pincode = row["Pincode"]?.ToString(),
                        Latitude = row["Latitude"]?.ToString(),
                        Longitude = row["Longitude"]?.ToString(),
                        MapUrl = row["MapUrl"]?.ToString()
                    };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while getting address details: " + ex.Message);
            }
        }

        public CateringServicesModel GetOwnerServices(Int64 ownerId)
        {
            try
            {

                string query = $@"SELECT c_cuisine_types AS CuisineTypes, c_service_types AS ServiceTypes, 
                    c_event_types AS EventTypes, c_food_types AS FoodTypes, c_min_dish_order AS MinDishOrder, 
                    c_delivery_radius_km AS DeliveryRadiusKm, c_serving_time_slots AS ServingTimeSlots 
                    FROM {Table.SysCateringOwnerService} WHERE c_ownerid = @OwnerId";
                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerId", ownerId),
                };
                var serviceData = _db.Execute(query.ToString(), parameters.ToArray());
                if (serviceData.Rows.Count > 0)
                {
                    var row = serviceData.Rows[0];
                    return new CateringServicesModel
                    {
                        CuisineTypeIds = row["CuisineTypes"] != DBNull.Value ? ArrayHelper.ConvertStringToIntArray(row["CuisineTypes"].ToString()) : null,
                        ServiceTypeIds = row["ServiceTypes"] != DBNull.Value ? ArrayHelper.ConvertStringToIntArray(row["ServiceTypes"].ToString()) : null,
                        EventTypeIds = row["EventTypes"] != DBNull.Value ? ArrayHelper.ConvertStringToIntArray(row["EventTypes"].ToString()) : null,
                        FoodTypeIds = row["FoodTypes"] != DBNull.Value ? ArrayHelper.ConvertStringToIntArray(row["FoodTypes"].ToString()) : null,
                        MinOrderValue = row["MinDishOrder"] != DBNull.Value ? Convert.ToInt32(row["MinDishOrder"]) : 0,
                        DeliveryRediusKm = row["DeliveryRadiusKm"] != DBNull.Value ? Convert.ToInt16(row["DeliveryRadiusKm"]) : 0,
                        ServingSlots = row["ServingTimeSlots"] != DBNull.Value ? ArrayHelper.ConvertStringToIntArray(row["ServingTimeSlots"].ToString()) : null
                    };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while getting services details: " + ex.Message);
            }
        }

        public OwnerLegalModel GetLegalDocumentsDetails(Int64 ownerId)
        {
            try
            {
                OwnerLegalModel ownerLegalAndBank = new OwnerLegalModel();
                string query = $@"SELECT c_fssai_number AS FssaiNumber, c_fssai_expiry_date AS FssaiExpiryDate, 
                        c_fssai_certificate_path AS FssaiCertificatePath, c_gst_applicable AS IsGstApplicable, 
                        c_gst_number AS GstNumber, c_gst_certificate_path AS GstCertificatePath, 
                        c_pan_name AS PanHolderName, c_pan_number AS PanNumber, c_pan_file_path AS PanCertificatePath 
                        FROM {Table.SysCateringOwnerLegal} WHERE c_ownerid = @OwnerId";
                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerId", ownerId)
                };
                var legalDocumentData = _db.Execute(query.ToString(), parameters.ToArray());
                ownerLegalAndBank = GetBankDetails(ownerId);
                if (legalDocumentData.Rows.Count > 0)
                {
                    var row = legalDocumentData.Rows[0];
                    ownerLegalAndBank.FssaiNumber = row["FssaiNumber"]?.ToString();
                    ownerLegalAndBank.FssaiExpiryDate = row["FssaiExpiryDate"] != DBNull.Value ? Convert.ToDateTime(row["FssaiExpiryDate"]) : DateTime.MinValue;
                    ownerLegalAndBank.FssaiCertificatePath = row["FssaiCertificatePath"]?.ToString();
                    ownerLegalAndBank.GstNumber = row["GstNumber"]?.ToString();
                    ownerLegalAndBank.IsGstApplicable = row["IsGstApplicable"] != DBNull.Value && Convert.ToBoolean(row["IsGstApplicable"]);
                    ownerLegalAndBank.GstCertificatePath = row["GstCertificatePath"]?.ToString();
                    ownerLegalAndBank.PanHolderName = row["PanHolderName"]?.ToString();
                    ownerLegalAndBank.PanNumber = row["PanNumber"]?.ToString();
                    ownerLegalAndBank.PanCertificatePath = row["PanCertificatePath"]?.ToString();
                    return ownerLegalAndBank;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while getting legal documents: " + ex.Message);
            }
        }

        public OwnerLegalModel GetBankDetails(Int64 ownerId)
        {
            try
            {
                string query = $@"SELECT c_account_holder_name AS AccountHolderName, c_account_number AS AccountNumber, 
                        c_ifsc_code AS IfscCode, c_cheque_path AS ChequePath, c_upi_id AS UpiId 
                        FROM {Table.SysCateringOwnerBankDetails} WHERE c_ownerid = @OwnerId";
                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerId", ownerId)
                };
                var bankDetails = _db.Execute(query.ToString(), parameters.ToArray());
                if (bankDetails.Rows.Count > 0)
                {
                    var row = bankDetails.Rows[0];
                    return new OwnerLegalModel
                    {
                        AccountHolderName = row["AccountHolderName"]?.ToString(),
                        BankAccountNumber = row["AccountNumber"]?.ToString(),
                        IfscCode = row["IfscCode"]?.ToString(),
                        ChequePath = row["ChequePath"]?.ToString(),
                        UpiId = row["UpiId"]?.ToString()
                    };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while getting bank details: " + ex.Message);
            }
        }

        public List<MediaFileModel> GetMediaPath(long ownerPKID, DocumentType documentTypeID)
        {
            try
            {
                string query = $@"SELECT c_media_id AS ID, c_file_path AS FilePath, c_file_name AS FileName 
                        FROM {Table.SysCateringMediaUploads}
                        WHERE c_ownerid = @OwnerId AND c_document_type_id = @DocumentTypeID";
                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@OwnerId", ownerPKID),
                    new SqlParameter("@DocumentTypeID", documentTypeID.GetHashCode())
                };
                var mediaData = _db.Execute(query.ToString(), parameters.ToArray());
                var mediaList = new List<MediaFileModel>();
                if (mediaData.Rows.Count > 0)
                {
                    foreach (System.Data.DataRow row in mediaData.Rows)
                    {
                        mediaList.Add(new MediaFileModel
                        {
                            Id = Convert.ToInt64(row["ID"]),
                            FilePath = row["FilePath"]?.ToString(),
                            FileName = row["FileName"]?.ToString(),
                            MediaType = Path.GetExtension(row["FileName"]?.ToString()),
                            DocumentType = documentTypeID,
                        });
                    }
                }
                return mediaList;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while getting media path: " + ex.Message);
            }
        }

        public async Task UpdateOwnerProfile(long ownerPKID, UpdateOwnerProfileDto updateOwnerProfileDto)
        {
            try
            {
                // Update the various sections of the owner's profile
                if (updateOwnerProfileDto.OwnerBusiness != null)
                {
                    UpdateOwnerBusiness(ownerPKID, updateOwnerProfileDto.OwnerBusiness);
                }
                if (updateOwnerProfileDto.CateringAddress != null)
                {
                    UpdateCateringAddress(ownerPKID, updateOwnerProfileDto.CateringAddress);
                }
                if (updateOwnerProfileDto.CateringServices != null)
                {
                    UpdateCateringServices(ownerPKID, updateOwnerProfileDto.CateringServices);
                }
                if (updateOwnerProfileDto.OwnerLegalDocument != null)
                {
                    UpdateLegalAndBankDetails(ownerPKID, updateOwnerProfileDto.OwnerLegalDocument);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void UpdateOwnerBusiness(long ownerPKID, BusinessSettingsDto businessSettings)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append($@"UPDATE {Table.SysCateringOwner} SET c_catering_name = @CateringName, c_owner_name = @OwnerName, 
                    c_catering_number = @CateringNumber, c_std_number = @StdCode, c_whatsapp_number = @WhatsAppNumber, 
                    c_support_contact_number = @SupportEmail WHERE c_ownerid = @OwnerId ");
                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@CateringName", businessSettings.CateringName),
                    new SqlParameter("@OwnerName", businessSettings.OwnerName),
                    new SqlParameter("@CateringNumber", businessSettings.CateringNumber),
                    new SqlParameter("@StdCode", string.IsNullOrEmpty(businessSettings.StdCode) ? DBNull.Value : businessSettings.StdCode),
                    new SqlParameter("@WhatsAppNumber", string.IsNullOrEmpty(businessSettings.WhatsAppNumber) ? DBNull.Value : businessSettings.WhatsAppNumber),
                    new SqlParameter("@SupportEmail", string.IsNullOrEmpty(businessSettings.SupportEmail) ? DBNull.Value : businessSettings.SupportEmail),
                    new SqlParameter("@OwnerId", ownerPKID)
                };
                _db.ExecuteNonQuery(query.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception("Error while updating owner business details: " + ex.Message);
            }
        }

        public void UpdateCateringAddress(long ownerPKID, AddressSettingsDto addressSettings)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append($@"UPDATE {Table.SysCateringOwnerAddress} SET c_building = @ShopNo, c_street = @Street, 
                    c_area = @Area, c_city = @City, c_state = @State, c_pincode = @Pincode, c_latitude = @Latitude, 
                    c_longitude = @Longitude WHERE c_ownerid = @OwnerId ");
                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@ShopNo", addressSettings.ShopNo),
                    new SqlParameter("@Street", addressSettings.Street),
                    new SqlParameter("@Area", string.IsNullOrEmpty(addressSettings.Floor) ? DBNull.Value : addressSettings.Floor),
                    new SqlParameter("@City", addressSettings.City),
                    new SqlParameter("@State", addressSettings.State),
                    new SqlParameter("@Pincode", addressSettings.Pincode),
                    new SqlParameter("@Latitude", string.IsNullOrEmpty(addressSettings.Latitude) ? DBNull.Value : addressSettings.Latitude),
                    new SqlParameter("@Longitude", string.IsNullOrEmpty(addressSettings.Longitude) ? DBNull.Value : addressSettings.Longitude),
                    new SqlParameter("@OwnerId", ownerPKID)
                };
                _db.ExecuteNonQuery(query.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception("Error while updating catering address: " + ex.Message);
            }
        }

        public void UpdateCateringServices(long ownerPKID, ServicesSettingsDto servicesSettings)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append($@"UPDATE {Table.SysCateringOwnerService} SET c_cuisine_types = @CuisineTypes, 
                    c_service_types = @ServiceTypes, c_event_types = @EventTypes, c_food_types = @FoodTypes, 
                    c_min_dish_order = @MinDishOrder, c_delivery_radius_km = @DeliveryRadiusKm, 
                    c_serving_time_slots = @ServingTimeSlots WHERE c_ownerid = @OwnerId ");
                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@CuisineTypes", servicesSettings.CuisineTypeIds != null ? string.Join(",", servicesSettings.CuisineTypeIds) : DBNull.Value),
                    new SqlParameter("@ServiceTypes", servicesSettings.ServiceTypeIds != null ? string.Join(",", servicesSettings.ServiceTypeIds) : DBNull.Value),
                    new SqlParameter("@EventTypes", servicesSettings.EventTypeIds != null ? string.Join(",", servicesSettings.EventTypeIds) : DBNull.Value),
                    new SqlParameter("@FoodTypes", servicesSettings.FoodTypeIds != null ? string.Join(",", servicesSettings.FoodTypeIds) : DBNull.Value),
                    new SqlParameter("@MinDishOrder", servicesSettings.MinOrderValue),
                    new SqlParameter("@DeliveryRadiusKm", servicesSettings.DeliveryRediusKm),
                    new SqlParameter("@ServingTimeSlots", servicesSettings.ServingSlots != null ? string.Join(",", servicesSettings.ServingSlots)  : DBNull.Value),
                    new SqlParameter("@OwnerId", ownerPKID)
                };
                _db.ExecuteNonQuery(query.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception("Error while updating catering services: " + ex.Message);
            }
        }

        public void UpdateLegalAndBankDetails(long ownerPKID, LegalPaymentSettingsDto legalPaymentSettings)
        {
            try
            {
                StringBuilder legalQuery = new StringBuilder();
                legalQuery.Append($@"UPDATE {Table.SysCateringOwnerLegal} SET c_fssai_number = @FssaiNumber, 
                    c_fssai_expiry_date = @FssaiExpiryDate, c_gst_applicable = @IsGstApplicable, 
                    c_gst_number = @GstNumber, c_pan_name = @PanHolderName, c_pan_number = @PanNumber 
                    WHERE c_ownerid = @OwnerId ");
                List<SqlParameter> legalParameters = new()
                {
                    new SqlParameter("@FssaiNumber", string.IsNullOrEmpty(legalPaymentSettings.FssaiNumber) ? DBNull.Value : legalPaymentSettings.FssaiNumber),
                    new SqlParameter("@FssaiExpiryDate", string.IsNullOrEmpty(legalPaymentSettings.FssaiExpiryDate) ? DBNull.Value : Convert.ToDateTime(legalPaymentSettings.FssaiExpiryDate)),
                    new SqlParameter("@IsGstApplicable", legalPaymentSettings.IsGstApplicable),
                    new SqlParameter("@GstNumber", string.IsNullOrEmpty(legalPaymentSettings.GstNumber) ? DBNull.Value : legalPaymentSettings.GstNumber),
                    new SqlParameter("@PanHolderName", string.IsNullOrEmpty(legalPaymentSettings.PanHolderName) ? DBNull.Value : legalPaymentSettings.PanHolderName),
                    new SqlParameter("@PanNumber", string.IsNullOrEmpty(legalPaymentSettings.PanNumber) ? DBNull.Value : legalPaymentSettings.PanNumber),
                    new SqlParameter("@OwnerId", ownerPKID)
                };
                _db.ExecuteNonQuery(legalQuery.ToString(), legalParameters.ToArray());
                StringBuilder bankQuery = new StringBuilder();
                bankQuery.Append($@"UPDATE {Table.SysCateringOwnerBankDetails} SET c_account_holder_name = @AccountHolderName, 
                    c_account_number = @AccountNumber, c_ifsc_code = @IfscCode WHERE c_ownerid = @OwnerId ");
                List<SqlParameter> bankParameters = new()
                {
                    new SqlParameter("@AccountHolderName", legalPaymentSettings.AccountHolderName),
                    new SqlParameter("@AccountNumber", legalPaymentSettings.BankAccountNumber),
                    new SqlParameter("@IfscCode", legalPaymentSettings.IfscCode),
                    new SqlParameter("@OwnerId", ownerPKID)
                };
                _db.ExecuteNonQuery(bankQuery.ToString(), bankParameters.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception("Error while updating legal and bank details: " + ex.Message);
            }
        }
    }
}
