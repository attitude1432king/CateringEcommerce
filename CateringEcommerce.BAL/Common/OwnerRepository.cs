using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace CateringEcommerce.BAL.Common
{
    public class OwnerRepository : IOwnerRepository
    {
        private readonly SqlDatabaseManager _db;
        private readonly Logger<OwnerRepository> _logger;

        public OwnerRepository(string connectionString)
        {
            _db = new SqlDatabaseManager();
            _db.SetConnectionString(connectionString);
        }

        public bool IsOwnerPhoneExist(string mobileNumber)
        {
            if (string.IsNullOrEmpty(mobileNumber))
                throw new ArgumentException("Mobile number cannot be null or empty.", nameof(mobileNumber));
            string query = $"SELECT COUNT(*) FROM {Table.SysCateringOwner} WHERE c_owner_number = @MobileNumber";
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
        public async Task<int> SaveFilePath(string filePath, Int64 ownerPkid, string fileName, DocumentType documentType)
        {
            string query = $"INSERT INTO {Table.SysCateringMediaUploads} (c_ownerid, c_file_name, c_file_path, c_document_type_id, c_uploaded_at) " +
                           "VALUES (@OwnerPkid, @FileName, @FilePath, @DocumentTypeID, @UploadAt)";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@OwnerPkid", ownerPkid),
                new SqlParameter("@DocumentTypeID", documentType.GetHashCode()),
                new SqlParameter("@FilePath", filePath),
                new SqlParameter("@FileName", fileName),
                new SqlParameter("@UploadAt", DateTime.Now)
            };

            try
            {
                return _db.ExecuteNonQuery(query, parameters);
            }
            catch (SqlException ex)
            {
                throw new Exception("An error occurred while saving the file path.", ex);
            }
        }
    }
}
