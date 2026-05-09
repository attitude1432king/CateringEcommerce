using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Models.Owner;
using Npgsql;
using System.Text;
using System.Xml.Linq;

namespace CateringEcommerce.BAL.Common
{
    public class OwnerRepository : IOwnerRepository
    {
        private readonly IDatabaseHelper _dbHelper;
        public OwnerRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<bool> IsOwnerExistAsync(long ownerPkID)
        {
            string query = $"SELECT COUNT(*) FROM {Table.SysCateringOwner} WHERE c_ownerid = @OwnerPkID";
            NpgsqlParameter[] parameters = new NpgsqlParameter[] { new NpgsqlParameter("@OwnerPkID", ownerPkID) };
            int count = Convert.ToInt32(await _dbHelper.ExecuteScalarAsync(query, parameters));
            return count > 0;
        }

        public bool IsOwnerPhoneExist(string mobileNumber)
        {
            if (string.IsNullOrEmpty(mobileNumber))
                throw new ArgumentException("Mobile number cannot be null or empty.", nameof(mobileNumber));
            string query = $"SELECT COUNT(*) FROM {Table.SysCateringOwner} WHERE c_mobile = @MobileNumber";
            NpgsqlParameter[] parameters = new NpgsqlParameter[] { new NpgsqlParameter("@MobileNumber", mobileNumber) };
            int count = Convert.ToInt32(_dbHelper.ExecuteScalar(query, parameters));
            return count > 0;
        }

        public bool IsEmailExist(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));
            string query = $"SELECT COUNT(*) FROM {Table.SysCateringOwner} WHERE c_email = @Email";
            NpgsqlParameter[] parameters = new NpgsqlParameter[] { new NpgsqlParameter("@Email", email) };
            int count = Convert.ToInt32(_dbHelper.ExecuteScalar(query, parameters));
            return count > 0;
        }
        public async Task<int> SaveFilePath(string filePath, long ownerPkid, string fileName, DocumentType documentType, long referenceID = 0)
        {
            try
            {
                string extension = Path.GetExtension(fileName);
                fileName = Path.GetFileNameWithoutExtension(fileName);
                string query = $@"INSERT INTO {Table.SysCateringMediaUploads} (c_ownerid, c_file_name, c_file_path, c_document_type_id, c_extension, c_reference_id) 
                            VALUES (@OwnerPkid, @FileName, @FilePath, @DocumentTypeID, @Extesion, @ReferenceID)";

                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@OwnerPkid", ownerPkid),
                    new NpgsqlParameter("@DocumentTypeID", documentType.GetHashCode()),
                    new NpgsqlParameter("@FilePath", filePath),
                    new NpgsqlParameter("@FileName", fileName),
                    new NpgsqlParameter("@Extesion", extension),
                    new NpgsqlParameter("@ReferenceID", referenceID > 0 ? referenceID : DBNull.Value)
                };

                return await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            }
            catch (NpgsqlException ex)
            {
                throw new Exception("An error occurred while saving the file path.", ex);
            }
        }

        public async Task<OwnerBusinessModel> GetOwnerDetails(string number = null, long ownerPkid = 0)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                NpgsqlParameter[] parameters;

                query.Append($"SELECT * FROM {Table.SysCateringOwner} WHERE ");

                if (ownerPkid > 0)
                {
                    query.Append("c_ownerid = @OwnerPkid");
                    parameters = new NpgsqlParameter[] { new NpgsqlParameter("@OwnerPkid", ownerPkid) };
                }
                else
                {
                    query.Append("c_mobile = @MobileNumber");
                    parameters = new NpgsqlParameter[] { new NpgsqlParameter("@MobileNumber", number) };
                }

                var dt = await _dbHelper.ExecuteAsync(query.ToString(), parameters);

                if (dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    return new OwnerBusinessModel
                    {
                        PkID = row["c_ownerid"] == DBNull.Value ? 0 : Convert.ToInt64(row["c_ownerid"]),
                        OwnerName = row["c_owner_name"] == DBNull.Value ? string.Empty : row["c_owner_name"].ToString(),
                        CateringName = row["c_catering_name"] == DBNull.Value ? string.Empty : row["c_catering_name"].ToString(),
                        Phone = row["c_mobile"] == DBNull.Value ? string.Empty : row["c_mobile"].ToString(),
                        CateringNumber = row["c_catering_number"] == DBNull.Value ? string.Empty : row["c_catering_number"].ToString(),
                        Email = row["c_email"] == DBNull.Value ? string.Empty : row["c_email"].ToString(),
                        LogoPath = row["c_logo_path"] == DBNull.Value ? string.Empty : row["c_logo_path"].ToString(),
                        StdNumber = row["c_std_number"] == DBNull.Value ? string.Empty : row["c_std_number"].ToString(),
                        WhatsAppNumber = row["c_whatsapp_number"] == DBNull.Value ? string.Empty : row["c_whatsapp_number"].ToString(),
                        AlternateEmail = row["c_alternate_email"] == DBNull.Value ? string.Empty : row["c_alternate_email"].ToString(),
                        SupportContact = row["c_support_contact_number"] == DBNull.Value ? string.Empty : row["c_support_contact_number"].ToString(),
                        IsEmailVerified = row["c_email_verified"] == DBNull.Value ? false : row.GetBoolean("c_email_verified"),
                        IsPhoneVerified = row["c_phone_verified"] == DBNull.Value ? false : row.GetBoolean("c_phone_verified"),
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

        public async Task<int> DeleteDocumentFile(long documentPKID)
        {
            try
            {
                string query = $"DELETE FROM {Table.SysCateringMediaUploads} WHERE c_media_id = @documentPKID";
                
                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@documentPKID", documentPKID),
                };

                return await _dbHelper.ExecuteNonQueryAsync(query, parameters);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<int> SoftDeleteDocumentFile(long documentPKID)
        {
            try
            {
                string query = $"UPDATE {Table.SysCateringMediaUploads} SET c_is_deleted = TRUE, c_modifieddate = NOW()  WHERE c_media_id = @documentPKID";
                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@documentPKID", documentPKID),
                };
                return await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        public async Task<List<CateringMasterTypeModel>> GetCateringMasterType(CateringMaster cateringMasterCategory)
        {
            try
            {
                string query = $@"SELECT c_typeid AS TypeId, c_type_name AS ServiceName, c_description AS Description, c_isactive AS IsActive
                            FROM {Table.SysCateringTypeMaster} 
                            WHERE c_categoryid = @ServiceTypeId AND c_isactive = TRUE";

                var parameters = new List<NpgsqlParameter>
                {
                    new NpgsqlParameter("@ServiceTypeId", cateringMasterCategory.GetHashCode())
                };
                var dt = await _dbHelper.ExecuteAsync(query, parameters.ToArray());
                List<CateringMasterTypeModel> cateringMasterTypes = new List<CateringMasterTypeModel>();
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    cateringMasterTypes.Add(new CateringMasterTypeModel
                    {
                        TypeId = row["TypeId"] == DBNull.Value ? 0 : Convert.ToInt32(row["TypeId"]),
                        TypeName = row["ServiceName"] == DBNull.Value ? string.Empty : row["ServiceName"].ToString(),
                        Description = row["Description"] == DBNull.Value ? string.Empty : row["Description"].ToString(),
                        IsActive = row["IsActive"] == DBNull.Value ? false : Convert.ToBoolean(row["IsActive"]),
                        CategoryId = cateringMasterCategory.GetHashCode()
                    });
                }
                return cateringMasterTypes;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<int> UpdateDocumentFilePath(long referenceID, DocumentType documentType, string filePath)
        {
            try
            {
                string query = @$"UPDATE {Table.SysCateringMediaUploads} SET c_file_path = @FilePath, c_modifieddate = NOW()  WHERE c_reference_id = @ReferenceID
                               AND c_document_type_id = @DocumentTypeID";
                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@ReferenceID", referenceID),
                    new NpgsqlParameter("@FilePath", filePath),
                    new NpgsqlParameter("@DocumentTypeID", documentType.GetHashCode()),
                };
                return await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<int> SoftDeleteByReferenceID(long referenceID, DocumentType documentType)
        {
            try
            {
                string query = $@"UPDATE {Table.SysCateringMediaUploads} 
                                SET c_is_deleted = TRUE, c_modifieddate = NOW()  
                                WHERE c_reference_id = @ReferenceID 
                                AND c_document_type_id = @DocumentTypeID";
                
                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@ReferenceID", referenceID),
                    new NpgsqlParameter("@DocumentTypeID", documentType.GetHashCode()),
                };
                
                return await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

