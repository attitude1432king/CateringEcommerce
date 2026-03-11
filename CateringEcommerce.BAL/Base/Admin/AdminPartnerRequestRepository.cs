using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace CateringEcommerce.BAL.Base.Admin
{
    public class AdminPartnerRequestRepository : IAdminPartnerRequestRepository
    {
        private readonly IDatabaseHelper _dbHelper;
        public AdminPartnerRequestRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        #region List & Filter

        public AdminPartnerRequestListResponse GetAllPartnerRequests(AdminPartnerRequestListRequest request)
        {
            var response = new AdminPartnerRequestListResponse
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            // Build query for partner requests
            StringBuilder queryBuilder = new StringBuilder($@"
                SELECT
                    co.c_ownerid AS OwnerId,
                    co.c_catering_name AS BusinessName,
                    co.c_owner_name AS OwnerName,
                    co.c_mobile AS Phone,
                    co.c_email AS Email,
                    c.c_cityname AS City,
                    s.c_statename AS State,
                    ISNULL(co.c_approval_status, 'PENDING') AS Status,
                    ISNULL(co.c_priority, 'NORMAL') AS Priority,
                    co.c_createddate AS SubmittedDate,
                    co.c_reviewed_date AS ReviewedDate,
                    (SELECT COUNT(*) FROM {Table.SysCateringMediaUploads} WHERE c_ownerid = co.c_ownerid AND c_is_deleted = 0) AS DocumentCount
                FROM {Table.SysCateringOwner} co
                LEFT JOIN {Table.SysCateringOwnerAddress} addr ON co.c_ownerid = addr.c_ownerid
                LEFT JOIN {Table.City} c ON addr.c_cityid = c.c_cityid
                LEFT JOIN {Table.State} s ON addr.c_stateid = s.c_stateid
                WHERE 1=1");

            var parameters = new List<SqlParameter>();

            // Apply filters
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                queryBuilder.Append(" AND (co.c_catering_name LIKE @SearchTerm OR co.c_owner_name LIKE @SearchTerm OR co.c_mobile LIKE @SearchTerm OR co.c_email LIKE @SearchTerm)");
                parameters.Add(new SqlParameter("@SearchTerm", "%" + request.SearchTerm + "%"));
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                queryBuilder.Append(" AND co.c_approval_status = @Status");
                parameters.Add(new SqlParameter("@Status", request.Status));
            }

            if (request.CityId.HasValue)
            {
                queryBuilder.Append(" AND addr.c_cityid = @CityId");
                parameters.Add(new SqlParameter("@CityId", request.CityId.Value));
            }

            if (!string.IsNullOrEmpty(request.State))
            {
                queryBuilder.Append(" AND s.c_statename = @State");
                parameters.Add(new SqlParameter("@State", request.State));
            }

            if (request.FromDate.HasValue)
            {
                queryBuilder.Append(" AND co.c_createddate >= @FromDate");
                parameters.Add(new SqlParameter("@FromDate", request.FromDate.Value));
            }

            if (request.ToDate.HasValue)
            {
                queryBuilder.Append(" AND co.c_createddate <= @ToDate");
                parameters.Add(new SqlParameter("@ToDate", request.ToDate.Value));
            }

            if (!string.IsNullOrEmpty(request.Priority))
            {
                queryBuilder.Append(" AND co.c_priority = @Priority");
                parameters.Add(new SqlParameter("@Priority", request.Priority));
            }

            // Get total count
            var countQuery = $"SELECT COUNT(*) FROM ({queryBuilder}) AS CountQuery";
            response.TotalCount = Convert.ToInt32(_dbHelper.ExecuteScalar(countQuery, parameters.ToArray()));
            response.TotalPages = (int)Math.Ceiling((double)response.TotalCount / request.PageSize);

            // Add sorting
            var sortColumn = request.SortBy ?? "c_createddate";
            var sortOrder = request.SortOrder?.ToUpper() == "ASC" ? "ASC" : "DESC";
            queryBuilder.Append($" ORDER BY {sortColumn} {sortOrder}");

            // Add pagination
            var offset = (request.PageNumber - 1) * request.PageSize;
            queryBuilder.Append($" OFFSET {offset} ROWS FETCH NEXT {request.PageSize} ROWS ONLY");

            // Execute query
            var dataTable = _dbHelper.Execute(queryBuilder.ToString(), parameters.ToArray());

            foreach (DataRow row in dataTable.Rows)
            {
                response.Requests.Add(new AdminPartnerRequestListItem
                {
                    OwnerId = Convert.ToInt64(row["OwnerId"]),
                    BusinessName = row["BusinessName"].ToString() ?? string.Empty,
                    OwnerName = row["OwnerName"].ToString() ?? string.Empty,
                    Phone = row["Phone"].ToString() ?? string.Empty,
                    Email = row["Email"].ToString() ?? string.Empty,
                    City = row["City"] != DBNull.Value ? row["City"].ToString() : null,
                    State = row["State"] != DBNull.Value ? row["State"].ToString() : null,
                    Status = row["Status"].ToString() ?? "PENDING",
                    Priority = row["Priority"].ToString() ?? "NORMAL",
                    SubmittedDate = row["SubmittedDate"] != DBNull.Value ? Convert.ToDateTime(row["SubmittedDate"]) : DateTime.Now,
                    ReviewedDate = row["ReviewedDate"] != DBNull.Value ? Convert.ToDateTime(row["ReviewedDate"]) : null,
                    DocumentCount = row["DocumentCount"] != DBNull.Value ? Convert.ToInt32(row["DocumentCount"]) : 0,
                    PhotoCount = 0, // Can be calculated separately if needed
                    HasUnreadDocuments = false // Can be calculated based on last viewed time
                });
            }

            // Get stats
            response.Stats = GetPartnerRequestStats();

            return response;
        }

        private PartnerRequestStats GetPartnerRequestStats()
        {
            var stats = new PartnerRequestStats();

            var query = $@"
                SELECT
                    COUNT(*) AS TotalRequests,
                    SUM(CASE WHEN ISNULL(c_approval_status, 'PENDING') = 'PENDING' THEN 1 ELSE 0 END) AS PendingCount,
                    SUM(CASE WHEN c_approval_status = 'UNDER_REVIEW' THEN 1 ELSE 0 END) AS UnderReviewCount,
                    SUM(CASE WHEN c_approval_status = 'APPROVED' THEN 1 ELSE 0 END) AS ApprovedCount,
                    SUM(CASE WHEN c_approval_status = 'REJECTED' THEN 1 ELSE 0 END) AS RejectedCount,
                    SUM(CASE WHEN c_approval_status = 'INFO_REQUESTED' THEN 1 ELSE 0 END) AS InfoRequestedCount
                FROM {Table.SysCateringOwner}";

            var dataTable = _dbHelper.Execute(query);
            if (dataTable.Rows.Count > 0)
            {
                var row = dataTable.Rows[0];
                stats.TotalRequests = row["TotalRequests"] != DBNull.Value ? Convert.ToInt32(row["TotalRequests"]) : 0;
                stats.PendingCount = row["PendingCount"] != DBNull.Value ? Convert.ToInt32(row["PendingCount"]) : 0;
                stats.UnderReviewCount = row["UnderReviewCount"] != DBNull.Value ? Convert.ToInt32(row["UnderReviewCount"]) : 0;
                stats.ApprovedCount = row["ApprovedCount"] != DBNull.Value ? Convert.ToInt32(row["ApprovedCount"]) : 0;
                stats.RejectedCount = row["RejectedCount"] != DBNull.Value ? Convert.ToInt32(row["RejectedCount"]) : 0;
                stats.InfoRequestedCount = row["InfoRequestedCount"] != DBNull.Value ? Convert.ToInt32(row["InfoRequestedCount"]) : 0;
            }

            return stats;
        }

        #endregion

        #region Detail

        public AdminPartnerRequestDetail? GetPartnerRequestById(long ownerId)
        {
            var query = @"
                SELECT
                    co.c_ownerid AS OwnerId,
                    co.c_catering_name AS BusinessName,
                    co.c_owner_name AS OwnerName,
                    co.c_email AS Email,
                    co.c_mobile AS Phone,
                    co.c_support_contact_number AS AlternatePhone,
                    co.c_whatsapp_number AS WhatsAppNumber,
                    co.c_alternate_email AS AlternateEmail,
                    co.c_catering_number AS CateringNumber,
                    co.c_std_number AS StdNumber,
                    co.c_logo_path AS LogoUrl,
                    ISNULL(co.c_approval_status, 'PENDING') AS Status,
                    ISNULL(co.c_priority, 'NORMAL') AS Priority,
                    co.c_createddate AS SubmittedDate,
                    co.c_reviewed_date AS ReviewedDate,
                    co.c_reviewed_by AS ReviewedBy,
                    co.c_approved_date AS ApprovedDate,
                    co.c_approved_by AS ApprovedBy,
                    co.c_rejection_reason AS RejectionReason,
                    co.c_internal_notes AS InternalNotes,
                    co.c_isactive AS IsActive,
                    co.c_email_verified AS EmailVerified,
                    co.c_phone_verified AS PhoneVerified,
                    co.c_verified_by_admin AS VerifiedByAdmin
                FROM {Table.SysCateringOwner} co
                WHERE co.c_ownerid = @OwnerId";

            var parameters = new[] { new SqlParameter("@OwnerId", ownerId) };
            var dataTable = _dbHelper.Execute(query, parameters);

            if (dataTable.Rows.Count == 0)
                return null;

            var row = dataTable.Rows[0];
            var detail = new AdminPartnerRequestDetail
            {
                OwnerId = Convert.ToInt64(row["OwnerId"]),
                BusinessName = row["BusinessName"].ToString() ?? string.Empty,
                OwnerName = row["OwnerName"].ToString() ?? string.Empty,
                Email = row["Email"].ToString() ?? string.Empty,
                Phone = row["Phone"].ToString() ?? string.Empty,
                AlternatePhone = row["AlternatePhone"] != DBNull.Value ? row["AlternatePhone"].ToString() : null,
                WhatsAppNumber = row["WhatsAppNumber"] != DBNull.Value ? row["WhatsAppNumber"].ToString() : null,
                AlternateEmail = row["AlternateEmail"] != DBNull.Value ? row["AlternateEmail"].ToString() : null,
                CateringNumber = row["CateringNumber"] != DBNull.Value ? row["CateringNumber"].ToString() : null,
                StdNumber = row["StdNumber"] != DBNull.Value ? row["StdNumber"].ToString() : null,
                LogoUrl = row["LogoUrl"] != DBNull.Value ? row["LogoUrl"].ToString() : null,
                Status = row["Status"].ToString() ?? "PENDING",
                Priority = row["Priority"].ToString() ?? "NORMAL",
                SubmittedDate = row["SubmittedDate"] != DBNull.Value ? Convert.ToDateTime(row["SubmittedDate"]) : DateTime.Now,
                ReviewedDate = row["ReviewedDate"] != DBNull.Value ? Convert.ToDateTime(row["ReviewedDate"]) : null,
                ReviewedBy = row["ReviewedBy"] != DBNull.Value ? Convert.ToInt64(row["ReviewedBy"]) : null,
                ApprovedDate = row["ApprovedDate"] != DBNull.Value ? Convert.ToDateTime(row["ApprovedDate"]) : null,
                ApprovedBy = row["ApprovedBy"] != DBNull.Value ? Convert.ToInt64(row["ApprovedBy"]) : null,
                RejectionReason = row["RejectionReason"] != DBNull.Value ? row["RejectionReason"].ToString() : null,
                InternalNotes = row["InternalNotes"] != DBNull.Value ? row["InternalNotes"].ToString() : null,
                IsActive = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"]),
                EmailVerified = row["EmailVerified"] != DBNull.Value && Convert.ToBoolean(row["EmailVerified"]),
                PhoneVerified = row["PhoneVerified"] != DBNull.Value && Convert.ToBoolean(row["PhoneVerified"]),
                VerifiedByAdmin = row["VerifiedByAdmin"] != DBNull.Value && Convert.ToBoolean(row["VerifiedByAdmin"])
            };

            // Load related data
            detail.Address = GetPartnerAddress(ownerId);
            detail.Compliance = GetPartnerCompliance(ownerId);
            detail.BankDetails = GetPartnerBankDetails(ownerId);
            detail.Operations = GetPartnerOperations(ownerId);
            detail.Documents = GetPartnerDocuments(ownerId);
            detail.Photos = GetPartnerPhotos(ownerId);
            detail.Timeline = GetActionLog(ownerId);

            return detail;
        }

        private PartnerAddress? GetPartnerAddress(long ownerId)
        {
            var query = $@"
                SELECT
                    addr.c_addressid AS AddressId,
                    addr.c_building AS Building,
                    addr.c_street AS Street,
                    addr.c_area AS Area,
                    addr.c_stateid AS StateId,
                    s.c_statename AS StateName,
                    addr.c_cityid AS CityId,
                    c.c_cityname AS CityName,
                    addr.c_pincode AS Pincode,
                    addr.c_latitude AS Latitude,
                    addr.c_longitude AS Longitude,
                    addr.c_mapurl AS MapUrl
                FROM {Table.SysCateringOwnerAddress} addr
                LEFT JOIN {Table.State} s ON addr.c_stateid = s.c_stateid
                LEFT JOIN {Table.City} c ON addr.c_cityid = c.c_cityid
                WHERE addr.c_ownerid = @OwnerId";

            var parameters = new[] { new SqlParameter("@OwnerId", ownerId) };
            var dataTable = _dbHelper.Execute(query, parameters);

            if (dataTable.Rows.Count == 0)
                return null;

            var row = dataTable.Rows[0];
            return new PartnerAddress
            {
                AddressId = Convert.ToInt64(row["AddressId"]),
                Building = row["Building"].ToString() ?? string.Empty,
                Street = row["Street"] != DBNull.Value ? row["Street"].ToString() : null,
                Area = row["Area"] != DBNull.Value ? row["Area"].ToString() : null,
                StateId = row["StateId"] != DBNull.Value ? Convert.ToInt32(row["StateId"]) : null,
                StateName = row["StateName"] != DBNull.Value ? row["StateName"].ToString() : null,
                CityId = row["CityId"] != DBNull.Value ? Convert.ToInt32(row["CityId"]) : null,
                CityName = row["CityName"] != DBNull.Value ? row["CityName"].ToString() : null,
                Pincode = row["Pincode"].ToString() ?? string.Empty,
                Latitude = row["Latitude"] != DBNull.Value ? row["Latitude"].ToString() : null,
                Longitude = row["Longitude"] != DBNull.Value ? row["Longitude"].ToString() : null,
                MapUrl = row["MapUrl"] != DBNull.Value ? row["MapUrl"].ToString() : null
            };
        }

        private PartnerCompliance? GetPartnerCompliance(long ownerId)
        {
            var query = $@"
                SELECT
                    c_complianceid AS ComplianceId,
                    c_fssai_number AS FssaiNumber,
                    c_fssai_expiry_date AS FssaiExpiryDate,
                    c_fssai_certificate_path AS FssaiCertificatePath,
                    c_gst_applicable AS GstApplicable,
                    c_gst_number AS GstNumber,
                    c_gst_certificate_path AS GstCertificatePath,
                    c_pan_name AS PanName,
                    c_pan_number AS PanNumber,
                    c_pan_file_path AS PanFilePath
                FROM {Table.SysCateringOwnerLegal}
                WHERE c_ownerid = @OwnerId";

            var parameters = new[] { new SqlParameter("@OwnerId", ownerId) };
            var dataTable = _dbHelper.Execute(query, parameters);

            if (dataTable.Rows.Count == 0)
                return null;

            var row = dataTable.Rows[0];
            return new PartnerCompliance
            {
                ComplianceId = Convert.ToInt64(row["ComplianceId"]),
                FssaiNumber = row["FssaiNumber"].ToString() ?? string.Empty,
                FssaiExpiryDate = Convert.ToDateTime(row["FssaiExpiryDate"]),
                FssaiCertificatePath = row["FssaiCertificatePath"].ToString() ?? string.Empty,
                GstApplicable = row["GstApplicable"] != DBNull.Value && Convert.ToBoolean(row["GstApplicable"]),
                GstNumber = row["GstNumber"] != DBNull.Value ? row["GstNumber"].ToString() : null,
                GstCertificatePath = row["GstCertificatePath"] != DBNull.Value ? row["GstCertificatePath"].ToString() : null,
                PanName = row["PanName"].ToString() ?? string.Empty,
                PanNumber = row["PanNumber"].ToString() ?? string.Empty,
                PanFilePath = row["PanFilePath"] != DBNull.Value ? row["PanFilePath"].ToString() : null
            };
        }

        private PartnerBankDetails? GetPartnerBankDetails(long ownerId)
        {
            var query = $@"
                SELECT
                    c_bankid AS BankId,
                    c_account_number AS AccountNumber,
                    c_account_holder_name AS AccountHolderName,
                    c_ifsc_code AS IfscCode,
                    c_cheque_path AS ChequePath,
                    c_upi_id AS UpiId
                FROM {Table.SysCateringOwnerBankDetails}
                WHERE c_ownerid = @OwnerId";

            var parameters = new[] { new SqlParameter("@OwnerId", ownerId) };
            var dataTable = _dbHelper.Execute(query, parameters);

            if (dataTable.Rows.Count == 0)
                return null;

            var row = dataTable.Rows[0];
            return new PartnerBankDetails
            {
                BankId = Convert.ToInt64(row["BankId"]),
                AccountNumber = row["AccountNumber"].ToString() ?? string.Empty,
                AccountHolderName = row["AccountHolderName"].ToString() ?? string.Empty,
                IfscCode = row["IfscCode"].ToString() ?? string.Empty,
                ChequePath = row["ChequePath"] != DBNull.Value ? row["ChequePath"].ToString() : null,
                UpiId = row["UpiId"] != DBNull.Value ? row["UpiId"].ToString() : null
            };
        }

        private PartnerOperations? GetPartnerOperations(long ownerId)
        {
            var query = $@"
                SELECT
                    c_operationid AS OperationId,
                    c_cuisine_types AS CuisineTypes,
                    c_service_types AS ServiceTypes,
                    c_event_types AS EventTypes,
                    c_food_types AS FoodTypes,
                    c_min_dish_order AS MinDishOrder,
                    c_delivery_available AS DeliveryAvailable,
                    c_delivery_radius_km AS DeliveryRadiusKm,
                    c_serving_time_slots AS ServingTimeSlots
                FROM {Table.SysCateringOwnerService}
                WHERE c_ownerid = @OwnerId";

            var parameters = new[] { new SqlParameter("@OwnerId", ownerId) };
            var dataTable = _dbHelper.Execute(query, parameters);

            if (dataTable.Rows.Count == 0)
                return null;

            var row = dataTable.Rows[0];
            return new PartnerOperations
            {
                OperationId = Convert.ToInt64(row["OperationId"]),
                CuisineTypes = row["CuisineTypes"] != DBNull.Value ? row["CuisineTypes"].ToString() : null,
                ServiceTypes = row["ServiceTypes"] != DBNull.Value ? row["ServiceTypes"].ToString() : null,
                EventTypes = row["EventTypes"] != DBNull.Value ? row["EventTypes"].ToString() : null,
                FoodTypes = row["FoodTypes"] != DBNull.Value ? row["FoodTypes"].ToString() : null,
                MinDishOrder = row["MinDishOrder"] != DBNull.Value ? Convert.ToDecimal(row["MinDishOrder"]) : null,
                DeliveryAvailable = row["DeliveryAvailable"] != DBNull.Value && Convert.ToBoolean(row["DeliveryAvailable"]),
                DeliveryRadiusKm = row["DeliveryRadiusKm"] != DBNull.Value ? Convert.ToInt32(row["DeliveryRadiusKm"]) : null,
                ServingTimeSlots = row["ServingTimeSlots"] != DBNull.Value ? row["ServingTimeSlots"].ToString() : null
            };
        }

        private List<PartnerDocument> GetPartnerDocuments(long ownerId)
        {
            var documents = new List<PartnerDocument>();

            var query = $@"
                SELECT
                    m.c_media_id AS MediaId,
                    m.c_document_type_id AS DocumentTypeId,
                    dt.c_documenttype_name AS DocumentTypeName,
                    m.c_file_name AS FileName,
                    m.c_file_path AS FilePath,
                    m.c_extension AS Extension,
                    m.c_uploaded_at AS UploadedAt
                FROM {Table.SysCateringMediaUploads} m
                LEFT JOIN {Table.SysCateringDocumentTypes} dt ON m.c_document_type_id = dt.c_documenttype_id
                WHERE m.c_ownerid = @OwnerId
                AND m.c_is_deleted = 0
                AND m.c_document_type_id IS NOT NULL";

            var parameters = new[] { new SqlParameter("@OwnerId", ownerId) };
            var dataTable = _dbHelper.Execute(query, parameters);

            foreach (DataRow row in dataTable.Rows)
            {
                documents.Add(new PartnerDocument
                {
                    MediaId = Convert.ToInt64(row["MediaId"]),
                    DocumentTypeId = Convert.ToInt32(row["DocumentTypeId"]),
                    DocumentTypeName = row["DocumentTypeName"]?.ToString() ?? string.Empty,
                    FileName = row["FileName"].ToString() ?? string.Empty,
                    FilePath = row["FilePath"].ToString() ?? string.Empty,
                    Extension = row["Extension"].ToString() ?? string.Empty,
                    UploadedAt = row["UploadedAt"] != DBNull.Value ? Convert.ToDateTime(row["UploadedAt"]) : DateTime.Now
                });
            }

            return documents;
        }

        private List<PartnerPhoto> GetPartnerPhotos(long ownerId)
        {
            var photos = new List<PartnerPhoto>();

            var query = $@"
                SELECT
                    c_media_id AS MediaId,
                    c_file_name AS FileName,
                    c_file_path AS FilePath,
                    c_extension AS Extension,
                    c_uploaded_at AS UploadedAt
                FROM {Table.SysCateringMediaUploads}
                WHERE c_ownerid = @OwnerId
                AND c_is_deleted = 0
                AND (c_document_type_id IS NULL OR c_document_type_id = 0)";

            var parameters = new[] { new SqlParameter("@OwnerId", ownerId) };
            var dataTable = _dbHelper.Execute(query, parameters);

            foreach (DataRow row in dataTable.Rows)
            {
                photos.Add(new PartnerPhoto
                {
                    MediaId = Convert.ToInt64(row["MediaId"]),
                    FileName = row["FileName"].ToString() ?? string.Empty,
                    FilePath = row["FilePath"].ToString() ?? string.Empty,
                    Extension = row["Extension"].ToString() ?? string.Empty,
                    Category = "Photo",
                    UploadedAt = row["UploadedAt"] != DBNull.Value ? Convert.ToDateTime(row["UploadedAt"]) : DateTime.Now
                });
            }

            return photos;
        }

        #endregion

        #region Actions

        public PartnerRequestActionResponse ApprovePartnerRequest(PartnerRequestActionRequest request, long adminId)
        {
            var response = new PartnerRequestActionResponse();

            try
            {
                // Update status to APPROVED
                var updateQuery = $@"
                    UPDATE {Table.SysCateringOwner}
                    SET c_approval_status = 'APPROVED',
                        c_approved_date = GETDATE(),
                        c_approved_by = @AdminId,
                        c_verified_by_admin = 1,
                        c_isactive = 1,
                        c_modifieddate = GETDATE()
                    WHERE c_ownerid = @OwnerId";

                var parameters = new[]
                {
                    new SqlParameter("@OwnerId", request.OwnerId),
                    new SqlParameter("@AdminId", adminId)
                };

                _dbHelper.ExecuteNonQuery(updateQuery, parameters);

                // Log action
                LogAction(request.OwnerId, adminId, "APPROVED", null, "APPROVED", request.Remarks, null);

                // Send communication if requested
                if (request.Communication?.SendNotification == true)
                {
                    SendApprovalCommunication(request, adminId);
                }

                response.Success = true;
                response.Message = "Partner request approved successfully";
                response.NewStatus = "APPROVED";
                response.CateringId = request.OwnerId;

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error approving partner request: {ex.Message}";
                return response;
            }
        }

        public PartnerRequestActionResponse RejectPartnerRequest(PartnerRequestActionRequest request, long adminId)
        {
            var response = new PartnerRequestActionResponse();

            try
            {
                // Update status to REJECTED
                var updateQuery = $@"
                    UPDATE {Table.SysCateringOwner}
                    SET c_approval_status = 'REJECTED',
                        c_approved_date = GETDATE(),
                        c_approved_by = @AdminId,
                        c_rejection_reason = @RejectionReason,
                        c_modifieddate = GETDATE()
                    WHERE c_ownerid = @OwnerId";

                var parameters = new[]
                {
                    new SqlParameter("@OwnerId", request.OwnerId),
                    new SqlParameter("@AdminId", adminId),
                    new SqlParameter("@RejectionReason", (object?)request.RejectionReason ?? DBNull.Value)
                };

                _dbHelper.ExecuteNonQuery(updateQuery, parameters);

                // Log action
                LogAction(request.OwnerId, adminId, "REJECTED", null, "REJECTED", request.RejectionReason, null);

                // Send communication if requested
                if (request.Communication?.SendNotification == true)
                {
                    SendRejectionCommunication(request, adminId);
                }

                response.Success = true;
                response.Message = "Partner request rejected";
                response.NewStatus = "REJECTED";

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error rejecting partner request: {ex.Message}";
                return response;
            }
        }

        public PartnerRequestActionResponse RequestAdditionalInfo(PartnerRequestActionRequest request, long adminId)
        {
            var response = new PartnerRequestActionResponse();

            try
            {
                // Update status to INFO_REQUESTED
                var updateQuery = $@"
                    UPDATE {Table.SysCateringOwner}
                    SET c_approval_status = 'INFO_REQUESTED',
                        c_reviewed_date = GETDATE(),
                        c_reviewed_by = @AdminId,
                        c_modifieddate = GETDATE()
                    WHERE c_ownerid = @OwnerId";

                var parameters = new[]
                {
                    new SqlParameter("@OwnerId", request.OwnerId),
                    new SqlParameter("@AdminId", adminId)
                };

                _dbHelper.ExecuteNonQuery(updateQuery, parameters);

                // Log action
                var remarksWithRequirements = request.Remarks;
                if (request.InfoRequirements != null && request.InfoRequirements.Any())
                {
                    remarksWithRequirements = $"{request.Remarks}\n\nRequired Items:\n" + string.Join("\n", request.InfoRequirements.Select(r => $"- {r}"));
                }

                LogAction(request.OwnerId, adminId, "INFO_REQUESTED", null, "INFO_REQUESTED", remarksWithRequirements, null);

                // Send communication if requested
                if (request.Communication?.SendNotification == true)
                {
                    SendInfoRequestCommunication(request, adminId);
                }

                response.Success = true;
                response.Message = "Additional information requested";
                response.NewStatus = "INFO_REQUESTED";

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error requesting information: {ex.Message}";
                return response;
            }
        }

        #endregion

        #region Status & Notes

        public bool UpdatePartnerRequestStatus(long ownerId, string newStatus, long adminId, string? remarks = null)
        {
            try
            {
                var updateQuery = $@"
                    UPDATE {Table.SysCateringOwner}
                    SET c_approval_status = @NewStatus,
                        c_modifieddate = GETDATE()
                    WHERE c_ownerid = @OwnerId";

                var parameters = new[]
                {
                    new SqlParameter("@OwnerId", ownerId),
                    new SqlParameter("@NewStatus", newStatus)
                };

                _dbHelper.ExecuteNonQuery(updateQuery, parameters);
                LogAction(ownerId, adminId, "STATUS_CHANGED", null, newStatus, remarks, null);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateInternalNotes(long ownerId, string notes, long adminId)
        {
            try
            {
                var updateQuery = @$"
                    UPDATE {Table.SysCateringOwner}
                    SET c_internal_notes = @Notes,
                        c_modifieddate = GETDATE()
                    WHERE c_ownerid = @OwnerId";

                var parameters = new[]
                {
                    new SqlParameter("@OwnerId", ownerId),
                    new SqlParameter("@Notes", notes)
                };

                _dbHelper.ExecuteNonQuery(updateQuery, parameters);
                LogAction(ownerId, adminId, "NOTES_UPDATED", null, null, "Internal notes updated", null);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdatePriority(long ownerId, int priority, long adminId)
        {
            try
            {
                var updateQuery = $@"
                    UPDATE {Table.SysCateringOwner}
                    SET c_priority = @Priority,
                        c_modifieddate = GETDATE()
                    WHERE c_ownerid = @OwnerId";

                var parameters = new[]
                {
                    new SqlParameter("@OwnerId", ownerId),
                    new SqlParameter("@Priority", priority)
                };

                _dbHelper.ExecuteNonQuery(updateQuery, parameters);
                LogAction(ownerId, adminId, "PRIORITY_CHANGED", null, null, $"Priority changed to {priority}", null);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Action Log

        public List<PartnerActionLog> GetActionLog(long ownerId)
        {
            var actionLog = new List<PartnerActionLog>();

            var query = $@"
                SELECT
                    a.c_action_id AS ActionId,
                    a.c_adminid AS AdminId,
                    ad.c_fullname AS AdminName,
                    a.c_action_type AS ActionType,
                    a.c_old_status AS OldStatus,
                    a.c_new_status AS NewStatus,
                    a.c_remarks AS Remarks,
                    a.c_action_date AS ActionDate,
                    a.c_ip_address AS IpAddress
                FROM {Table.SysPartnerRequestActions} a
                LEFT JOIN {Table.SysAdmin} ad ON a.c_adminid = ad.c_adminid
                WHERE a.c_ownerid = @OwnerId
                ORDER BY a.c_action_date DESC";

            var parameters = new[] { new SqlParameter("@OwnerId", ownerId) };
            var dataTable = _dbHelper.Execute(query, parameters);

            foreach (DataRow row in dataTable.Rows)
            {
                actionLog.Add(new PartnerActionLog
                {
                    ActionId = Convert.ToInt64(row["ActionId"]),
                    AdminId = Convert.ToInt64(row["AdminId"]),
                    AdminName = row["AdminName"]?.ToString() ?? string.Empty,
                    ActionType = row["ActionType"].ToString() ?? string.Empty,
                    OldStatus = row["OldStatus"] != DBNull.Value ? row["OldStatus"].ToString() : null,
                    NewStatus = row["NewStatus"] != DBNull.Value ? row["NewStatus"].ToString() : null,
                    Remarks = row["Remarks"] != DBNull.Value ? row["Remarks"].ToString() : null,
                    ActionDate = Convert.ToDateTime(row["ActionDate"]),
                    IpAddress = row["IpAddress"] != DBNull.Value ? row["IpAddress"].ToString() : null
                });
            }

            return actionLog;
        }

        public bool LogAction(long ownerId, long adminId, string actionType, string? oldStatus, string? newStatus, string? remarks, string? ipAddress)
        {
            try
            {
                var query = $@"
                    INSERT INTO {Table.SysPartnerRequestActions}
                    (c_ownerid, c_adminid, c_action_type, c_old_status, c_new_status, c_remarks, c_ip_address, c_action_date)
                    VALUES
                    (@OwnerId, @AdminId, @ActionType, @OldStatus, @NewStatus, @Remarks, @IpAddress, GETDATE())";

                var parameters = new[]
                {
                    new SqlParameter("@OwnerId", ownerId),
                    new SqlParameter("@AdminId", adminId),
                    new SqlParameter("@ActionType", actionType),
                    new SqlParameter("@OldStatus", (object?)oldStatus ?? DBNull.Value),
                    new SqlParameter("@NewStatus", (object?)newStatus ?? DBNull.Value),
                    new SqlParameter("@Remarks", (object?)remarks ?? DBNull.Value),
                    new SqlParameter("@IpAddress", (object?)ipAddress ?? DBNull.Value)
                };

                _dbHelper.ExecuteNonQuery(query, parameters);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Communication

        public PartnerCommunicationResponse SendCommunication(PartnerCommunicationRequest request, long adminId)
        {
            var response = new PartnerCommunicationResponse();

            try
            {
                // Log communication
                var query = $@"
                    INSERT INTO {Table.SysPartnerRequestCommunications}
                    (c_ownerid, c_adminid, c_communication_type, c_subject, c_message, c_sent_to_email, c_sent_to_phone, c_email_sent, c_sms_sent, c_email_status, c_sms_status, c_sent_date)
                    VALUES
                    (@OwnerId, @AdminId, @CommunicationType, @Subject, @Message, @SentToEmail, @SentToPhone, @EmailSent, @SmsSent, @EmailStatus, @SmsStatus, GETDATE())";

                var emailSent = request.CommunicationType == "EMAIL" || request.CommunicationType == "BOTH";
                var smsSent = request.CommunicationType == "SMS" || request.CommunicationType == "BOTH";

                var parameters = new[]
                {
                    new SqlParameter("@OwnerId", request.OwnerId),
                    new SqlParameter("@AdminId", adminId),
                    new SqlParameter("@CommunicationType", request.CommunicationType),
                    new SqlParameter("@Subject", (object?)request.Subject ?? DBNull.Value),
                    new SqlParameter("@Message", request.Message),
                    new SqlParameter("@SentToEmail", (object?)request.SentToEmail ?? DBNull.Value),
                    new SqlParameter("@SentToPhone", (object?)request.SentToPhone ?? DBNull.Value),
                    new SqlParameter("@EmailSent", emailSent),
                    new SqlParameter("@SmsSent", smsSent),
                    new SqlParameter("@EmailStatus", emailSent ? "SENT" : DBNull.Value),
                    new SqlParameter("@SmsStatus", smsSent ? "SENT" : DBNull.Value)
                };

                _dbHelper.ExecuteNonQuery(query, parameters);

                response.Success = true;
                response.Message = "Communication sent successfully";
                response.EmailSent = emailSent;
                response.SmsSent = smsSent;
                response.EmailStatus = emailSent ? "SENT" : null;
                response.SmsStatus = smsSent ? "SENT" : null;

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error sending communication: {ex.Message}";
                return response;
            }
        }

        public List<PartnerCommunicationHistory> GetCommunicationHistory(long ownerId)
        {
            var history = new List<PartnerCommunicationHistory>();

            var query = $@"
                SELECT
                    c.c_communication_id AS CommunicationId,
                    c.c_adminid AS AdminId,
                    a.c_fullname AS AdminName,
                    c.c_communication_type AS CommunicationType,
                    c.c_subject AS Subject,
                    c.c_message AS Message,
                    c.c_sent_to_email AS SentToEmail,
                    c.c_sent_to_phone AS SentToPhone,
                    c.c_email_sent AS EmailSent,
                    c.c_sms_sent AS SmsSent,
                    c.c_email_status AS EmailStatus,
                    c.c_sms_status AS SmsStatus,
                    c.c_sent_date AS SentDate
                FROM {Table.SysPartnerRequestCommunications} c
                LEFT JOIN {Table.SysAdmin} a ON c.c_adminid = a.c_adminid
                WHERE c.c_ownerid = @OwnerId
                ORDER BY c.c_sent_date DESC";

            var parameters = new[] { new SqlParameter("@OwnerId", ownerId) };
            var dataTable = _dbHelper.Execute(query, parameters);

            foreach (DataRow row in dataTable.Rows)
            {
                history.Add(new PartnerCommunicationHistory
                {
                    CommunicationId = Convert.ToInt64(row["CommunicationId"]),
                    AdminId = Convert.ToInt64(row["AdminId"]),
                    AdminName = row["AdminName"]?.ToString() ?? string.Empty,
                    CommunicationType = row["CommunicationType"].ToString() ?? string.Empty,
                    Subject = row["Subject"] != DBNull.Value ? row["Subject"].ToString() : null,
                    Message = row["Message"].ToString() ?? string.Empty,
                    SentToEmail = row["SentToEmail"] != DBNull.Value ? row["SentToEmail"].ToString() : null,
                    SentToPhone = row["SentToPhone"] != DBNull.Value ? row["SentToPhone"].ToString() : null,
                    EmailSent = row["EmailSent"] != DBNull.Value && Convert.ToBoolean(row["EmailSent"]),
                    SmsSent = row["SmsSent"] != DBNull.Value && Convert.ToBoolean(row["SmsSent"]),
                    EmailStatus = row["EmailStatus"] != DBNull.Value ? row["EmailStatus"].ToString() : null,
                    SmsStatus = row["SmsStatus"] != DBNull.Value ? row["SmsStatus"].ToString() : null,
                    SentDate = Convert.ToDateTime(row["SentDate"])
                });
            }

            return history;
        }

        private void SendApprovalCommunication(PartnerRequestActionRequest request, long adminId)
        {
            // Get partner details for email/sms
            var partnerDetails = GetPartnerRequestById(request.OwnerId);
            if (partnerDetails == null) return;

            var commRequest = new PartnerCommunicationRequest
            {
                OwnerId = request.OwnerId,
                CommunicationType = request.Communication?.SendEmail == true && request.Communication?.SendSms == true ? "BOTH" :
                                   request.Communication?.SendEmail == true ? "EMAIL" : "SMS",
                Subject = request.Communication?.EmailSubject ?? "Partner Registration Approved",
                Message = request.Communication?.EmailBody ?? $"Dear {partnerDetails.OwnerName},\n\nCongratulations! Your partner registration has been approved.",
                SentToEmail = partnerDetails.Email,
                SentToPhone = partnerDetails.Phone
            };

            SendCommunication(commRequest, adminId);
        }

        private void SendRejectionCommunication(PartnerRequestActionRequest request, long adminId)
        {
            var partnerDetails = GetPartnerRequestById(request.OwnerId);
            if (partnerDetails == null) return;

            var commRequest = new PartnerCommunicationRequest
            {
                OwnerId = request.OwnerId,
                CommunicationType = request.Communication?.SendEmail == true && request.Communication?.SendSms == true ? "BOTH" :
                                   request.Communication?.SendEmail == true ? "EMAIL" : "SMS",
                Subject = request.Communication?.EmailSubject ?? "Partner Registration Status",
                Message = request.Communication?.EmailBody ?? $"Dear {partnerDetails.OwnerName},\n\nRegarding your partner registration application...",
                SentToEmail = partnerDetails.Email,
                SentToPhone = partnerDetails.Phone
            };

            SendCommunication(commRequest, adminId);
        }

        private void SendInfoRequestCommunication(PartnerRequestActionRequest request, long adminId)
        {
            var partnerDetails = GetPartnerRequestById(request.OwnerId);
            if (partnerDetails == null) return;

            var commRequest = new PartnerCommunicationRequest
            {
                OwnerId = request.OwnerId,
                CommunicationType = request.Communication?.SendEmail == true && request.Communication?.SendSms == true ? "BOTH" :
                                   request.Communication?.SendEmail == true ? "EMAIL" : "SMS",
                Subject = request.Communication?.EmailSubject ?? "Additional Information Required",
                Message = request.Communication?.EmailBody ?? $"Dear {partnerDetails.OwnerName},\n\nWe need additional information for your partner registration.",
                SentToEmail = partnerDetails.Email,
                SentToPhone = partnerDetails.Phone
            };

            SendCommunication(commRequest, adminId);
        }

        #endregion
    }
}
