using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Models.Owner;
using Npgsql;

namespace CateringEcommerce.BAL.Common
{
    public class MediaRepository: IMediaRepository
    {
        private readonly IDatabaseHelper _dbHelper;
        public MediaRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<List<MediaFileModel>> GetMediaFiles(long ownerPKID, DocumentType documentTypeID, long referenceID = 0)
        {
            try
            {
                string query = $@"SELECT c_media_id AS ID, c_file_path AS FilePath, c_file_name AS FileName 
                        FROM {Table.SysCateringMediaUploads}
                        WHERE c_ownerid = @OwnerId AND c_document_type_id = @DocumentTypeID AND c_is_deleted = FALSE";
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>
                {
                    new NpgsqlParameter("@OwnerId", ownerPKID),
                    new NpgsqlParameter("@DocumentTypeID", documentTypeID.GetHashCode())
                };

                if (referenceID > 0)
                {
                    query += " AND c_reference_id = @ReferenceID";
                    parameters.Add(new NpgsqlParameter("@ReferenceID", referenceID));
                }
                var mediaData = await _dbHelper.ExecuteAsync(query.ToString(), parameters.ToArray());
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
                            MediaType = Path.GetExtension(row["FilePath"]?.ToString()),
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
    }
}
