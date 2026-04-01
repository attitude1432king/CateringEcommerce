using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Enums.Admin;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace CateringEcommerce.BAL.Base.Admin
{
    /// <summary>
    /// Repository for Admin Partner Request Approval & Rejection Flow
    /// This repository handles ONLY registration-time data review and approval workflow
    /// DO NOT confuse with OwnerProfile.cs which is used AFTER approval for partner operations
    /// </summary>
    public class AdminPartnerApprovalRepository : IAdminPartnerApprovalRepository
    {
        private readonly IDatabaseHelper _dbHelper;
        public AdminPartnerApprovalRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        #region Partner Request Listing (Grid View)

        /// <summary>
        /// Gets all partner requests with filtering, sorting, and pagination
        /// Returns ONLY registration data (NOT post-approval operational data)
        /// </summary>
        public PartnerRequestListResponse GetPendingPartnerRequests(PartnerRequestFilterRequest filter)
        {
            var response = new PartnerRequestListResponse
            {
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };

            // Build query - Uses INT columns for approval_status and priority
            StringBuilder queryBuilder = new StringBuilder($@"
                SELECT
                    co.c_ownerid AS OwnerId,
                    co.c_catering_name AS BusinessName,
                    co.c_owner_name AS OwnerName,
                    co.c_partnernumber AS PartnerNumber,
                    co.c_mobile AS Phone,
                    co.c_email AS Email,
                    c.c_cityname AS City,
                    s.c_statename AS State,
                    ISNULL(co.c_approval_status, 1) AS ApprovalStatusId,
                    ISNULL(co.c_priority, 1) AS PriorityId,
                    co.c_createddate AS RegistrationDate,
                    co.c_approved_date AS ApprovedDate,
                    (SELECT COUNT(*) FROM {Table.SysCateringMediaUploads} WHERE c_ownerid = co.c_ownerid AND c_isdeleted = 0) AS DocumentCount
                FROM {Table.SysCateringOwner} co
                LEFT JOIN {Table.SysCateringOwnerAddress} addr ON co.c_ownerid = addr.c_ownerid
                LEFT JOIN {Table.City} c ON addr.c_cityid = c.c_cityid
                LEFT JOIN {Table.State} s ON addr.c_stateid = s.c_stateid
                WHERE 1=1");

            var parameters = new List<SqlParameter>();

            // Filter by approval status (INT enum value)
            if (filter.ApprovalStatusId.HasValue)
            {
                queryBuilder.Append(" AND co.c_approval_status = @ApprovalStatusId");
                parameters.Add(new SqlParameter("@ApprovalStatusId", filter.ApprovalStatusId.Value));
            }
            else
            {
                // Default: Show only PENDING requests
                queryBuilder.Append($" AND co.c_approval_status = {(int)ApprovalStatus.Pending}");
            }

            // Filter by priority (INT enum value)
            if (filter.PriorityId.HasValue)
            {
                queryBuilder.Append(" AND co.c_priority = @PriorityId");
                parameters.Add(new SqlParameter("@PriorityId", filter.PriorityId.Value));
            }

            // Search filter
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                queryBuilder.Append(@" AND (co.c_catering_name LIKE @SearchTerm
                    OR co.c_owner_name LIKE @SearchTerm
                    OR co.c_mobile LIKE @SearchTerm
                    OR co.c_email LIKE @SearchTerm)");
                parameters.Add(new SqlParameter("@SearchTerm", $"%{filter.SearchTerm}%"));
            }

            // City filter
            if (filter.CityId.HasValue)
            {
                queryBuilder.Append(" AND addr.c_cityid = @CityId");
                parameters.Add(new SqlParameter("@CityId", filter.CityId.Value));
            }

            // Date range filter
            if (filter.FromDate.HasValue)
            {
                queryBuilder.Append(" AND co.c_createddate >= @FromDate");
                parameters.Add(new SqlParameter("@FromDate", filter.FromDate.Value));
            }

            if (filter.ToDate.HasValue)
            {
                queryBuilder.Append(" AND co.c_createddate <= @ToDate");
                parameters.Add(new SqlParameter("@ToDate", filter.ToDate.Value.AddDays(1).AddSeconds(-1)));
            }

            // Get total count
            var countQuery = $"SELECT COUNT(*) FROM ({queryBuilder}) AS CountQuery";
            response.TotalCount = Convert.ToInt32(_dbHelper.ExecuteScalar(countQuery, parameters.ToArray()));
            response.TotalPages = (int)Math.Ceiling((double)response.TotalCount / filter.PageSize);

            // Add sorting
            var sortColumn = !string.IsNullOrEmpty(filter.SortBy) ? $"co.{filter.SortBy}" : "co.c_createddate";
            var sortOrder = filter.SortOrder?.ToUpper() == "ASC" ? "ASC" : "DESC";
            queryBuilder.Append($" ORDER BY {sortColumn} {sortOrder}");

            // Add pagination
            var offset = (filter.PageNumber - 1) * filter.PageSize;
            queryBuilder.Append($" OFFSET {offset} ROWS FETCH NEXT {filter.PageSize} ROWS ONLY");

            // Execute query
            var dataTable = _dbHelper.Execute(queryBuilder.ToString(), parameters.ToArray());

            foreach (DataRow row in dataTable.Rows)
            {
                var approvalStatusId = row["ApprovalStatusId"] != DBNull.Value ? Convert.ToInt32(row["ApprovalStatusId"]) : (int)ApprovalStatus.Pending;
                var priorityId = row["PriorityId"] != DBNull.Value ? Convert.ToInt32(row["PriorityId"]) : (int)PriorityStatus.Normal;

                response.Requests.Add(new PartnerRequestListItem
                {
                    OwnerId = Convert.ToInt64(row["OwnerId"]),
                    BusinessName = row["BusinessName"]?.ToString() ?? string.Empty,
                    OwnerName = row["OwnerName"]?.ToString() ?? string.Empty,
                    Phone = row["Phone"]?.ToString() ?? string.Empty,
                    Email = row["Email"]?.ToString() ?? string.Empty,
                    City = row["City"] != DBNull.Value ? row["City"].ToString() : null,
                    RequestNumber = row["PartnerNumber"] != DBNull.Value ? row["PartnerNumber"].ToString() : null,
                    State = row["State"] != DBNull.Value ? row["State"].ToString() : null,
                    ApprovalStatusId = approvalStatusId,
                    ApprovalStatusName = EnumHelper.GetDisplayNameFromInt<ApprovalStatus>(approvalStatusId),
                    PriorityId = priorityId,
                    PriorityName = EnumHelper.GetDisplayNameFromInt<PriorityStatus>(priorityId),
                    RegistrationDate = row["RegistrationDate"] != DBNull.Value ? Convert.ToDateTime(row["RegistrationDate"]) : DateTime.Now,
                    ApprovedDate = row["ApprovedDate"] != DBNull.Value ? Convert.ToDateTime(row["ApprovedDate"]) : null,
                    DocumentCount = row["DocumentCount"] != DBNull.Value ? Convert.ToInt32(row["DocumentCount"]) : 0
                });
            }

            // Get statistics
            response.Stats = GetPartnerRequestStats();

            return response;
        }

        /// <summary>
        /// Gets summary statistics for all partner requests grouped by status
        /// </summary>
        private PartnerRequestStatistics GetPartnerRequestStats()
        {
            var query = $@"
                SELECT
                    COUNT(*) AS TotalRequests,
                    SUM(CASE WHEN ISNULL(c_approval_status, 1) = {(int)ApprovalStatus.Pending} THEN 1 ELSE 0 END) AS PendingCount,
                    SUM(CASE WHEN c_approval_status = {(int)ApprovalStatus.Approved} THEN 1 ELSE 0 END) AS ApprovedCount,
                    SUM(CASE WHEN c_approval_status = {(int)ApprovalStatus.Rejected} THEN 1 ELSE 0 END) AS RejectedCount,
                    SUM(CASE WHEN c_approval_status = {(int)ApprovalStatus.UnderReview} THEN 1 ELSE 0 END) AS UnderReviewCount,
                    SUM(CASE WHEN c_approval_status = {(int)ApprovalStatus.Info_Requested} THEN 1 ELSE 0 END) AS InfoRequestedCount
                FROM {Table.SysCateringOwner}";

            var dataTable = _dbHelper.Execute(query);
            var stats = new PartnerRequestStatistics();

            if (dataTable.Rows.Count > 0)
            {
                var row = dataTable.Rows[0];
                stats.TotalRequests = row["TotalRequests"] != DBNull.Value ? Convert.ToInt32(row["TotalRequests"]) : 0;
                stats.PendingCount = row["PendingCount"] != DBNull.Value ? Convert.ToInt32(row["PendingCount"]) : 0;
                stats.ApprovedCount = row["ApprovedCount"] != DBNull.Value ? Convert.ToInt32(row["ApprovedCount"]) : 0;
                stats.RejectedCount = row["RejectedCount"] != DBNull.Value ? Convert.ToInt32(row["RejectedCount"]) : 0;
                stats.UnderReviewCount = row["UnderReviewCount"] != DBNull.Value ? Convert.ToInt32(row["UnderReviewCount"]) : 0;
                stats.InfoRequestedCount = row["InfoRequestedCount"] != DBNull.Value ? Convert.ToInt32(row["InfoRequestedCount"]) : 0;
            }

            return stats;
        }

        #endregion

        #region Partner Request Detail View

        /// <summary>
        /// Gets complete registration details for a specific partner request
        /// Returns ALL data submitted during registration for admin review
        /// This is READ-ONLY data - Admin does NOT edit partner registration data
        /// </summary>
        public PartnerRequestDetailResponse? GetPartnerRequestDetail(long ownerId)
        {
            // Get main owner/business details
            var query = $@"
                SELECT
                    co.c_ownerid AS OwnerId,
                    co.c_catering_name AS BusinessName,
                    co.c_owner_name AS OwnerName,
                    co.c_email AS Email,
                    co.c_mobile AS Phone,
                    co.c_support_contact_number AS SupportContact,
                    co.c_whatsapp_number AS WhatsAppNumber,
                    co.c_alternate_email AS AlternateEmail,
                    co.c_catering_number AS CateringNumber,
                    co.c_std_number AS StdNumber,
                    co.c_logo_path AS LogoPath,
                    ISNULL(co.c_approval_status, 1) AS ApprovalStatusId,
                    ISNULL(co.c_priority, 1) AS PriorityId,
                    co.c_createddate AS RegistrationDate,
                    co.c_approved_date AS ApprovedDate,
                    co.c_approved_by AS ApprovedBy,
                    co.c_rejection_reason AS RejectionReason,
                    co.c_isactive AS IsActive,
                    co.c_email_verified AS EmailVerified,
                    co.c_phone_verified AS PhoneVerified
                FROM {Table.SysCateringOwner} co
                WHERE co.c_ownerid = @OwnerId";

            var parameters = new[] { new SqlParameter("@OwnerId", ownerId) };
            var dataTable = _dbHelper.Execute(query, parameters);

            if (dataTable.Rows.Count == 0)
                return null;

            var row = dataTable.Rows[0];
            var approvalStatusId = row["ApprovalStatusId"] != DBNull.Value ? Convert.ToInt32(row["ApprovalStatusId"]) : (int)ApprovalStatus.Pending;
            var priorityId = row["PriorityId"] != DBNull.Value ? Convert.ToInt32(row["PriorityId"]) : (int)PriorityStatus.Normal;

            var detail = new PartnerRequestDetailResponse
            {
                // Basic Info
                OwnerId = Convert.ToInt64(row["OwnerId"]),
                BusinessName = row["BusinessName"]?.ToString() ?? string.Empty,
                OwnerName = row["OwnerName"]?.ToString() ?? string.Empty,
                Email = row["Email"]?.ToString() ?? string.Empty,
                Phone = row["Phone"]?.ToString() ?? string.Empty,
                SupportContact = row["SupportContact"] != DBNull.Value ? row["SupportContact"].ToString() : null,
                WhatsAppNumber = row["WhatsAppNumber"] != DBNull.Value ? row["WhatsAppNumber"].ToString() : null,
                AlternateEmail = row["AlternateEmail"] != DBNull.Value ? row["AlternateEmail"].ToString() : null,
                CateringNumber = row["CateringNumber"] != DBNull.Value ? row["CateringNumber"].ToString() : null,
                StdNumber = row["StdNumber"] != DBNull.Value ? row["StdNumber"].ToString() : null,
                LogoPath = row["LogoPath"] != DBNull.Value ? row["LogoPath"].ToString() : null,

                // Status & Priority (with enum conversion)
                ApprovalStatusId = approvalStatusId,
                ApprovalStatusName = EnumHelper.GetDisplayNameFromInt<ApprovalStatus>(approvalStatusId),
                PriorityId = priorityId,
                PriorityName = EnumHelper.GetDisplayNameFromInt<PriorityStatus>(priorityId),

                // Dates
                RegistrationDate = row["RegistrationDate"] != DBNull.Value ? Convert.ToDateTime(row["RegistrationDate"]) : DateTime.Now,
                ApprovedDate = row["ApprovedDate"] != DBNull.Value ? Convert.ToDateTime(row["ApprovedDate"]) : null,
                ApprovedBy = row["ApprovedBy"] != DBNull.Value ? Convert.ToInt64(row["ApprovedBy"]) : null,
                RejectionReason = row["RejectionReason"] != DBNull.Value ? row["RejectionReason"].ToString() : null,

                // Flags
                IsActive = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"]),
                EmailVerified = row["EmailVerified"] != DBNull.Value && Convert.ToBoolean(row["EmailVerified"]),
                PhoneVerified = row["PhoneVerified"] != DBNull.Value && Convert.ToBoolean(row["PhoneVerified"]),
            };

            // Load related registration data
            detail.Address = GetPartnerAddressDetails(ownerId);
            detail.LegalCompliance = GetPartnerLegalDetails(ownerId);
            detail.BankDetails = GetPartnerBankDetails(ownerId);
            detail.ServiceOperations = GetPartnerOperationsDetails(ownerId);
            detail.Documents = GetPartnerDocuments(ownerId);
            detail.Photos = GetPartnerKitchenMedia(ownerId);

            return detail;
        }

        private PartnerAddressDetails? GetPartnerAddressDetails(long ownerId)
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
            return new PartnerAddressDetails
            {
                AddressId = Convert.ToInt64(row["AddressId"]),
                Building = row["Building"]?.ToString() ?? string.Empty,
                Street = row["Street"] != DBNull.Value ? row["Street"].ToString() : null,
                Area = row["Area"] != DBNull.Value ? row["Area"].ToString() : null,
                StateId = row["StateId"] != DBNull.Value ? Convert.ToInt32(row["StateId"]) : null,
                StateName = row["StateName"] != DBNull.Value ? row["StateName"].ToString() : null,
                CityId = row["CityId"] != DBNull.Value ? Convert.ToInt32(row["CityId"]) : null,
                CityName = row["CityName"] != DBNull.Value ? row["CityName"].ToString() : null,
                Pincode = row["Pincode"]?.ToString() ?? string.Empty,
                Latitude = row["Latitude"] != DBNull.Value ? row["Latitude"].ToString() : null,
                Longitude = row["Longitude"] != DBNull.Value ? row["Longitude"].ToString() : null,
                MapUrl = row["MapUrl"] != DBNull.Value ? row["MapUrl"].ToString() : null
            };
        }

        private PartnerLegalComplianceDetails? GetPartnerLegalDetails(long ownerId)
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
            return new PartnerLegalComplianceDetails
            {
                ComplianceId = Convert.ToInt64(row["ComplianceId"]),
                FssaiNumber = row["FssaiNumber"]?.ToString() ?? string.Empty,
                FssaiExpiryDate = row["FssaiExpiryDate"] != DBNull.Value ? Convert.ToDateTime(row["FssaiExpiryDate"]) : DateTime.MinValue,
                FssaiCertificatePath = row["FssaiCertificatePath"]?.ToString() ?? string.Empty,
                GstApplicable = row["GstApplicable"] != DBNull.Value && Convert.ToBoolean(row["GstApplicable"]),
                GstNumber = row["GstNumber"] != DBNull.Value ? row["GstNumber"].ToString() : null,
                GstCertificatePath = row["GstCertificatePath"] != DBNull.Value ? row["GstCertificatePath"].ToString() : null,
                PanName = row["PanName"]?.ToString() ?? string.Empty,
                PanNumber = row["PanNumber"]?.ToString() ?? string.Empty,
                PanFilePath = row["PanFilePath"] != DBNull.Value ? row["PanFilePath"].ToString() : null
            };
        }

        private PartnerBankAccountDetails? GetPartnerBankDetails(long ownerId)
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
            return new PartnerBankAccountDetails
            {
                BankId = Convert.ToInt64(row["BankId"]),
                AccountNumber = row["AccountNumber"]?.ToString() ?? string.Empty,
                AccountHolderName = row["AccountHolderName"]?.ToString() ?? string.Empty,
                IfscCode = row["IfscCode"]?.ToString() ?? string.Empty,
                ChequePath = row["ChequePath"] != DBNull.Value ? row["ChequePath"].ToString() : null,
                UpiId = row["UpiId"] != DBNull.Value ? row["UpiId"].ToString() : null
            };
        }

        private PartnerServiceOperationsDetails? GetPartnerOperationsDetails(long ownerId)
        {
            var query = $@"
                 SELECT
                    co.c_operationid AS OperationId,

                    CuisineTypes = (
                        SELECT STRING_AGG(tm.c_type_name, ', ')
                        FROM STRING_SPLIT(co.c_cuisine_types, ',') s
                        JOIN {Table.SysCateringTypeMaster} tm 
                            ON tm.c_typeid = TRY_CAST(s.value AS INT)
                    ),

                    ServiceTypes = (
                        SELECT STRING_AGG(tm.c_type_name, ', ')
                        FROM STRING_SPLIT(co.c_service_types, ',') s
                        JOIN {Table.SysCateringTypeMaster} tm 
                            ON tm.c_typeid = TRY_CAST(s.value AS INT)
                    ),

                    EventTypes = (
                        SELECT STRING_AGG(tm.c_type_name, ', ')
                        FROM STRING_SPLIT(co.c_event_types, ',') s
                        JOIN {Table.SysCateringTypeMaster} tm 
                            ON tm.c_typeid = TRY_CAST(s.value AS INT)
                    ),

                    FoodTypes = (
                        SELECT STRING_AGG(tm.c_type_name, ', ')
                        FROM STRING_SPLIT(co.c_food_types, ',') s
                        JOIN {Table.SysCateringTypeMaster} tm 
                            ON tm.c_typeid = TRY_CAST(s.value AS INT)
                    ),

                    co.c_min_dish_order AS MinDishOrder,
                    co.c_delivery_available AS DeliveryAvailable,
                    co.c_delivery_radius_km AS DeliveryRadiusKm,

                    ServingTimeSlots = (
                        SELECT STRING_AGG(tm.c_type_name, ', ')
                        FROM STRING_SPLIT(co.c_serving_time_slots, ',') s
                        JOIN {Table.SysCateringTypeMaster} tm 
                            ON tm.c_typeid = TRY_CAST(s.value AS INT)
                    )

                FROM {Table.SysCateringOwnerService} co;";

            var parameters = new[] { new SqlParameter("@OwnerId", ownerId) };
            var dataTable = _dbHelper.Execute(query, parameters);

            if (dataTable.Rows.Count == 0)
                return null;

            var row = dataTable.Rows[0];
            return new PartnerServiceOperationsDetails
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

        private List<PartnerDocumentInfo> GetPartnerDocuments(long ownerId)
        {
            var documents = new List<PartnerDocumentInfo>();

            var query = $@"
                SELECT
                    m.c_media_id AS MediaId,
                    m.c_document_type_id AS DocumentTypeId,
                    m.c_file_name AS FileName,
                    m.c_file_path AS FilePath,
                    m.c_extension AS Extension,
                    m.c_uploaded_at AS UploadedAt
                FROM {Table.SysCateringMediaUploads} m
                WHERE m.c_ownerid = @OwnerId
                AND m.c_is_deleted = 0
                AND m.c_document_type_id = 0
                ORDER BY m.c_uploaded_at DESC";

            var parameters = new[] { new SqlParameter("@OwnerId", ownerId) };
            var dataTable = _dbHelper.Execute(query, parameters);

            foreach (DataRow row in dataTable.Rows)
            {
                documents.Add(new PartnerDocumentInfo
                {
                    MediaId = Convert.ToInt64(row["MediaId"]),
                    DocumentTypeId = row["DocumentTypeId"] != DBNull.Value ? Convert.ToInt32(row["DocumentTypeId"]) : 0,
                    FileName = row["FileName"]?.ToString() ?? string.Empty,
                    FilePath = row["FilePath"]?.ToString() ?? string.Empty,
                    Extension = row["Extension"]?.ToString() ?? string.Empty,
                    UploadedAt = row["UploadedAt"] != DBNull.Value ? Convert.ToDateTime(row["UploadedAt"]) : DateTime.Now
                });
            }

            return documents;
        }

        private List<PartnerPhotoInfo> GetPartnerKitchenMedia(long ownerId)
        {
            var photos = new List<PartnerPhotoInfo>();

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
                AND c_document_type_id = {DocumentType.Kitchen.GetHashCode()}
                ORDER BY c_uploaded_at DESC";

            var parameters = new[] { new SqlParameter("@OwnerId", ownerId) };
            var dataTable = _dbHelper.Execute(query, parameters);

            foreach (DataRow row in dataTable.Rows)
            {
                photos.Add(new PartnerPhotoInfo
                {
                    MediaId = Convert.ToInt64(row["MediaId"]),
                    FileName = row["FileName"]?.ToString() ?? string.Empty,
                    FilePath = row["FilePath"]?.ToString() ?? string.Empty,
                    Extension = row["Extension"]?.ToString() ?? string.Empty,
                    UploadedAt = row["UploadedAt"] != DBNull.Value ? Convert.ToDateTime(row["UploadedAt"]) : DateTime.Now
                });
            }

            return photos;
        }

        #endregion

        #region Approval & Rejection Actions

        /// <summary>
        /// Approves a partner request
        /// Validates: Must be in PENDING status, cannot approve already approved partners
        /// </summary>
        public ApprovalActionResult ApprovePartnerRequest(long ownerId, long adminId, string? remarks)
        {
            var result = new ApprovalActionResult();

            try
            {
                // Validate current status
                var currentStatus = GetCurrentApprovalStatus(ownerId);
                if (currentStatus == null)
                {
                    result.Success = false;
                    result.Message = "Partner request not found";
                    return result;
                }

                if (currentStatus != ApprovalStatus.Pending && currentStatus != ApprovalStatus.UnderReview)
                {
                    result.Success = false;
                    result.Message = $"Cannot approve partner request with status: {EnumHelper.GetDisplayName(currentStatus.Value)}";
                    return result;
                }

                // Update to APPROVED status
                var updateQuery = $@"
                    UPDATE {Table.SysCateringOwner}
                    SET c_approval_status = @ApprovalStatus,
                        c_approved_date = GETDATE(),
                        c_approved_by = @AdminId,
                        c_verified_by_admin = 1,
                        c_isactive = 1,
                        c_modifieddate = GETDATE()
                    WHERE c_ownerid = @OwnerId";

                var parameters = new[]
                {
                    new SqlParameter("@OwnerId", ownerId),
                    new SqlParameter("@ApprovalStatus", (int)ApprovalStatus.Approved),
                    new SqlParameter("@AdminId", adminId)
                };

                _dbHelper.ExecuteNonQuery(updateQuery, parameters);

                // Initialize global availability for the newly approved partner (status=1 = OPEN)
                var availabilityQuery = $@"
                    IF NOT EXISTS (SELECT 1 FROM {Table.SysCateringAvailabilityGlobal} WHERE c_ownerid = @OwnerId)
                        INSERT INTO {Table.SysCateringAvailabilityGlobal} (c_ownerid, c_global_status, c_modifieddate)
                        VALUES (@OwnerId, 1, GETDATE())";

                _dbHelper.ExecuteNonQuery(availabilityQuery, new[] { new SqlParameter("@OwnerId", ownerId) });

                result.Success = true;
                result.Message = "Partner request approved successfully";
                result.NewStatusId = (int)ApprovalStatus.Approved;
                result.NewStatusName = EnumHelper.GetDisplayName(ApprovalStatus.Approved);

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error approving partner request: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// Rejects a partner request
        /// Validates: Must be in PENDING status, rejection reason is mandatory
        /// </summary>
        public ApprovalActionResult RejectPartnerRequest(long ownerId, long adminId, string rejectionReason)
        {
            var result = new ApprovalActionResult();

            try
            {
                if (string.IsNullOrWhiteSpace(rejectionReason))
                {
                    result.Success = false;
                    result.Message = "Rejection reason is required";
                    return result;
                }

                // Validate current status
                var currentStatus = GetCurrentApprovalStatus(ownerId);
                if (currentStatus == null)
                {
                    result.Success = false;
                    result.Message = "Partner request not found";
                    return result;
                }

                if (currentStatus == ApprovalStatus.Approved)
                {
                    result.Success = false;
                    result.Message = "Cannot reject an already approved partner request";
                    return result;
                }

                if (currentStatus == ApprovalStatus.Rejected)
                {
                    result.Success = false;
                    result.Message = "Partner request is already rejected";
                    return result;
                }

                // Update to REJECTED status
                var updateQuery = $@"
                    UPDATE {Table.SysCateringOwner}
                    SET c_approval_status = @ApprovalStatus,
                        c_approved_date = GETDATE(),
                        c_approved_by = @AdminId,
                        c_rejection_reason = @RejectionReason,
                        c_modifieddate = GETDATE()
                    WHERE c_ownerid = @OwnerId";

                var parameters = new[]
                {
                    new SqlParameter("@OwnerId", ownerId),
                    new SqlParameter("@ApprovalStatus", (int)ApprovalStatus.Rejected),
                    new SqlParameter("@AdminId", adminId),
                    new SqlParameter("@RejectionReason", rejectionReason)
                };

                _dbHelper.ExecuteNonQuery(updateQuery, parameters);

                result.Success = true;
                result.Message = "Partner request rejected";
                result.NewStatusId = (int)ApprovalStatus.Rejected;
                result.NewStatusName = EnumHelper.GetDisplayName(ApprovalStatus.Rejected);

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error rejecting partner request: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// Updates priority for a partner request
        /// Uses PriorityStatus enum (INT values)
        /// </summary>
        public bool UpdatePriority(long ownerId, PriorityStatus priority, long adminId)
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
                    new SqlParameter("@Priority", (int)priority)
                };

                _dbHelper.ExecuteNonQuery(updateQuery, parameters);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the current approval status of a partner request
        /// Returns the enum value, not the display name or string
        /// </summary>
        private ApprovalStatus? GetCurrentApprovalStatus(long ownerId)
        {
            var query = $@"
                SELECT ISNULL(c_approval_status, 1) AS ApprovalStatus
                FROM {Table.SysCateringOwner}
                WHERE c_ownerid = @OwnerId";

            var parameters = new[] { new SqlParameter("@OwnerId", ownerId) };
            var result = _dbHelper.ExecuteScalar(query, parameters);

            if (result != null && result != DBNull.Value)
            {
                var statusId = Convert.ToInt32(result);
                return EnumHelper.GetEnumFromInt<ApprovalStatus>(statusId);
            }

            return null;
        }

        #endregion
    }
}
