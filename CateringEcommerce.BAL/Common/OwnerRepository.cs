using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Xml.Linq;

namespace CateringEcommerce.BAL.Common
{
    public class OwnerRepository : IOwnerRepository
    {
        private readonly SqlDatabaseManager _db;

        public OwnerRepository(string connectionString)
        {
            _db = new SqlDatabaseManager();
            _db.SetConnectionString(connectionString);
        }

        public async Task<bool> IsOwnerExistAsync(long ownerPkID)
        {
            string query = $"SELECT COUNT(*) FROM {Table.SysCateringOwner} WHERE c_ownerid = @OwnerPkID";
            SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@OwnerPkID", ownerPkID) };
            int count = Convert.ToInt32(await _db.ExecuteScalarAsync(query, parameters));
            return count > 0;
        }

        public bool IsOwnerPhoneExist(string mobileNumber)
        {
            if (string.IsNullOrEmpty(mobileNumber))
                throw new ArgumentException("Mobile number cannot be null or empty.", nameof(mobileNumber));
            string query = $"SELECT COUNT(*) FROM {Table.SysCateringOwner} WHERE c_mobile = @MobileNumber";
            SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@MobileNumber", mobileNumber) };
            int count = Convert.ToInt32(_db.ExecuteScalar(query, parameters));
            return count > 0;
        }

        public bool IsEmailExist(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));
            string query = $"SELECT COUNT(*) FROM {Table.SysCateringOwner} WHERE c_email = @Email";
            SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@Email", email) };
            int count = Convert.ToInt32(_db.ExecuteScalar(query, parameters));
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

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@OwnerPkid", ownerPkid),
                    new SqlParameter("@DocumentTypeID", documentType.GetHashCode()),
                    new SqlParameter("@FilePath", filePath),
                    new SqlParameter("@FileName", fileName),
                    new SqlParameter("@Extesion", extension),
                    new SqlParameter("@ReferenceID", referenceID > 0 ? referenceID : DBNull.Value)
                };

                return await _db.ExecuteNonQueryAsync(query, parameters);
            }
            catch (SqlException ex)
            {
                throw new Exception("An error occurred while saving the file path.", ex);
            }
        }

        public OwnerBusinessModel GetOwnerDetails(string number = null, long ownerPkid = 0)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                SqlParameter[] parameters;

                query.Append($"SELECT * FROM {Table.SysCateringOwner} WHERE ");

                if (ownerPkid > 0)
                {
                    query.Append("c_ownerid = @OwnerPkid");
                    parameters = new SqlParameter[] { new SqlParameter("@OwnerPkid", ownerPkid) };
                }
                else
                {
                    query.Append("c_mobile = @MobileNumber");
                    parameters = new SqlParameter[] { new SqlParameter("@MobileNumber", number) };
                }

                var dt = _db.Execute(query.ToString(), parameters);

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
                        IsVerifiedBy_Admin = row["c_verified_by_admin"] == DBNull.Value ? false : row.GetBoolean("c_verified_by_admin"),
                        IsOnline = row["c_isonline"] == DBNull.Value ? false : row.GetBoolean("c_isonline"),
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
                
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@documentPKID", documentPKID),
                };

                return await _db.ExecuteNonQueryAsync(query, parameters);

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
                string query = $"UPDATE {Table.SysCateringMediaUploads} SET c_is_deleted = 1, c_updated_at = GETDATE()  WHERE c_media_id = @documentPKID";
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@documentPKID", documentPKID),
                };
                return await _db.ExecuteNonQueryAsync(query, parameters);
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
                string query = $@"SELECT c_type_id AS TypeId, c_type_name AS ServiceName, c_description AS Description, c_is_active AS IsActive
                            FROM {Table.SysCateringTypeMaster} 
                            WHERE c_category_id = @ServiceTypeId AND c_is_active = 1";

                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@ServiceTypeId", cateringMasterCategory.GetHashCode())
                };
                var dt = await _db.ExecuteAsync(query, parameters.ToArray());
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
                string query = @$"UPDATE {Table.SysCateringMediaUploads} SET c_file_path = @FilePath, c_updated_at = GETDATE()  WHERE c_reference_id = @ReferenceID
                               AND c_document_type_id = @DocumentTypeID";
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@ReferenceID", referenceID),
                    new SqlParameter("@FilePath", filePath),
                    new SqlParameter("@DocumentTypeID", documentType.GetHashCode()),
                };
                return await _db.ExecuteNonQueryAsync(query, parameters);
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
                                SET c_is_deleted = 1, c_updated_at = GETDATE()  
                                WHERE c_reference_id = @ReferenceID 
                                AND c_document_type_id = @DocumentTypeID";
                
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@ReferenceID", referenceID),
                    new SqlParameter("@DocumentTypeID", documentType.GetHashCode()),
                };
                
                return await _db.ExecuteNonQueryAsync(query, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
