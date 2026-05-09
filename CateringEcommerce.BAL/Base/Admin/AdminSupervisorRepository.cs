using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Enums.Admin;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Npgsql;
using System.Data;
using System.Text;

namespace CateringEcommerce.BAL.Base.Admin
{
    public class AdminSupervisorRepository : IAdminSupervisorRepository
    {
        private readonly IDatabaseHelper _dbHelper;

        public AdminSupervisorRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        #region Tab 1: Pending Supervisor Requests

        public AdminSupervisorRegistrationListResponse GetRegistrationRequests(AdminSupervisorRegistrationListRequest request)
        {
            var queryBuilder = new StringBuilder($@"
                SELECT
                    s.c_supervisor_id AS SupervisorId,
                    s.c_full_name AS FullName,
                    s.c_email AS Email,
                    s.c_phone AS Phone,
                    COALESCE(c.c_cityname, '') AS City,
                    COALESCE(st.c_statename, '') AS State,
                    s.c_supervisor_type AS SupervisorType,
                    CASE
                        WHEN s.c_current_status = 'ACTIVE' THEN {(int)SupervisorApprovalStatus.Approved}
                        WHEN s.c_current_status = 'REJECTED' THEN {(int)SupervisorApprovalStatus.Rejected}
                        WHEN s.c_status_reason LIKE 'UNDER_REVIEW:%' THEN {(int)SupervisorApprovalStatus.UnderReview}
                        WHEN s.c_status_reason LIKE 'INFO_REQUESTED:%' THEN {(int)SupervisorApprovalStatus.InfoRequested}
                        ELSE {(int)SupervisorApprovalStatus.Pending}
                    END AS Status,
                    s.c_status_reason AS StatusReason,
                    s.c_createddate AS CreatedDate,
                    s.c_modifieddate AS ApprovedDate
                FROM {Table.SysSupervisor} s
                LEFT JOIN {Table.City} c on c.c_cityid = s.c_cityid
                LEFT JOIN {Table.State} st on st.c_stateid = s.c_stateid
                WHERE s.c_current_status IN ('APPLIED','REJECTED','DOCUMENT_VERIFICATION','AWAITING_INTERVIEW','AWAITING_TRAINING','AWAITING_CERTIFICATION','RESUME_SCREENED','INTERVIEW_SCHEDULED','INTERVIEW_PASSED','BACKGROUND_VERIFICATION','TRAINING')");

            var parameters = new List<NpgsqlParameter>();
            AppendRegistrationFilters(queryBuilder, parameters, request);

            // Sorting
            string sortColumn = request.SortBy switch
            {
                "FullName" => "s.c_full_name",
                _ => "s.c_createddate"
            };
            queryBuilder.Append($" ORDER BY {sortColumn} {(request.SortOrder == "ASC" ? "ASC" : "DESC")}");

            // Count query
            var countBuilder = new StringBuilder($@"
                SELECT COUNT(*)
                FROM {Table.SysSupervisor} s
                WHERE s.c_current_status IN ('APPLIED','REJECTED','DOCUMENT_VERIFICATION','AWAITING_INTERVIEW','AWAITING_TRAINING','AWAITING_CERTIFICATION','RESUME_SCREENED','INTERVIEW_SCHEDULED','INTERVIEW_PASSED','BACKGROUND_VERIFICATION','TRAINING')");
            var countParams = new List<NpgsqlParameter>();
            AppendRegistrationFilters(countBuilder, countParams, request);

            int totalRecords = Convert.ToInt32(_dbHelper.ExecuteScalar(countBuilder.ToString(), countParams.ToArray()));

            // Pagination
            int offset = (request.PageNumber - 1) * request.PageSize;
            queryBuilder.Append($" LIMIT {request.PageSize} OFFSET {offset}");

            var dt = _dbHelper.Execute(queryBuilder.ToString(), parameters.ToArray());

            var registrations = new List<AdminSupervisorRegistrationListItem>();
            foreach (DataRow row in dt.Rows)
            {
                registrations.Add(MapRegistrationItem(row));
            }

            return new AdminSupervisorRegistrationListResponse
            {
                Registrations = registrations,
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize)
            };
        }

        public AdminSupervisorDetailResponse? GetSupervisorDetails(long supervisorId)
        {
            var query = $@"
                SELECT
                    s.c_supervisor_id,
                    s.c_full_name,
                    s.c_email,
                    s.c_phone,
                    s.c_alternate_phone,
                    s.c_gender,
                    TO_CHAR(s.c_date_of_birth, 'DD/MM/YYYY') AS c_date_of_birth,
                    s.c_address_line1,
                    COALESCE(ci.c_cityname, '') AS City,
                    COALESCE(st.c_statename, '') AS State,
                    s.c_pincode,
                    s.c_locality,
                    s.c_supervisor_type,
                    CASE
                        WHEN s.c_current_status = 'ACTIVE'                          THEN {(int)SupervisorApprovalStatus.Approved}
                        WHEN s.c_current_status = 'REJECTED'                        THEN {(int)SupervisorApprovalStatus.Rejected}
                        WHEN s.c_status_reason LIKE 'UNDER_REVIEW:%'                THEN {(int)SupervisorApprovalStatus.UnderReview}
                        WHEN s.c_status_reason LIKE 'INFO_REQUESTED:%'              THEN {(int)SupervisorApprovalStatus.InfoRequested}
                        ELSE {(int)SupervisorApprovalStatus.Pending}
                    END AS Status,
                    s.c_status_reason,
                    s.c_authority_level,
                    COALESCE(s.c_has_prior_experience, FALSE) AS c_has_prior_experience,
                    s.c_prior_experience_details,
                    s.c_specialization,
                    s.c_languages_known,
                    s.c_identity_type,
                    s.c_identity_number,
                    s.c_identity_proof_url,
                    s.c_photo_url,
                    s.c_address_url,
                    s.c_resume_url,
                    s.c_agreement_url,
                    COALESCE(r.c_doc_verification_status, r.c_document_verification_status) AS c_doc_verification_status,
                    r.c_interview_result,
                    COALESCE(r.c_training_passed, FALSE) AS c_training_passed,
                    r.c_activation_status,
                    -- Banking
                    s.c_bank_account_holder_name,
                    s.c_bank_name,
                    s.c_bank_account_number,
                    s.c_bank_ifsc,
                    s.c_compensation_type,
                    s.c_per_event_rate,
                    s.c_monthly_salary,
                    s.c_cancelled_cheque_url,
                    -- Availability
                    s.c_availability_calendar,
                    s.c_preferred_event_types,
                    COALESCE(s.c_max_events_per_month, 0) AS c_max_events_per_month,
                    -- Performance
                    COALESCE(s.c_total_events_supervised, 0) AS c_total_events_supervised,
                    s.c_average_rating,
                    s.c_certification_status,
                    s.c_createddate,
                    s.c_modifieddate
                FROM {Table.SysSupervisor} s
                LEFT JOIN {Table.City} ci ON ci.c_cityid = s.c_cityid
                LEFT JOIN {Table.State} st ON st.c_stateid = s.c_stateid
                LEFT JOIN {Table.SysSupervisorRegistration} r ON r.c_supervisor_id = s.c_supervisor_id
                WHERE s.c_supervisor_id = @SupervisorId
                  AND s.c_is_deleted = FALSE";

            var parameters = new[] { new NpgsqlParameter("@SupervisorId", supervisorId) };
            var dt = _dbHelper.Execute(query, parameters);

            if (dt.Rows.Count == 0) return null;

            var row = dt.Rows[0];
            return new AdminSupervisorDetailResponse
            {
                SupervisorId    = Convert.ToInt64(row["c_supervisor_id"]),
                FullName        = row["c_full_name"].ToString() ?? string.Empty,
                Email           = row["c_email"].ToString() ?? string.Empty,
                Phone           = row["c_phone"].ToString() ?? string.Empty,
                AlternatePhone  = row["c_alternate_phone"] != DBNull.Value ? row["c_alternate_phone"].ToString() : null,
                Gender          = row["c_gender"] != DBNull.Value ? row["c_gender"].ToString() : null,
                DateOfBirth     = row["c_date_of_birth"] != DBNull.Value ? row["c_date_of_birth"].ToString() : null,
                AddressLine1    = row["c_address_line1"] != DBNull.Value ? row["c_address_line1"].ToString() : null,
                City            = row["City"].ToString() ?? string.Empty,
                State           = row["State"].ToString() ?? string.Empty,
                Pincode         = row["c_pincode"] != DBNull.Value ? row["c_pincode"].ToString() : null,
                Locality        = row["c_locality"] != DBNull.Value ? row["c_locality"].ToString() : null,
                SupervisorType  = row["c_supervisor_type"].ToString() ?? string.Empty,
                Status          = Convert.ToInt32(row["Status"]),
                StatusReason    = row["c_status_reason"] != DBNull.Value ? row["c_status_reason"].ToString() : null,
                AuthorityLevel  = row["c_authority_level"].ToString() ?? string.Empty,
                HasPriorExperience      = Convert.ToBoolean(row["c_has_prior_experience"]),
                PriorExperienceDetails  = row["c_prior_experience_details"] != DBNull.Value ? row["c_prior_experience_details"].ToString() : null,
                Specialization  = row["c_specialization"] != DBNull.Value ? row["c_specialization"].ToString() : null,
                LanguagesKnown  = row["c_languages_known"] != DBNull.Value ? row["c_languages_known"].ToString() : null,
                IdentityType    = row["c_identity_type"] != DBNull.Value ? row["c_identity_type"].ToString() : null,
                IdentityNumber  = row["c_identity_number"] != DBNull.Value ? row["c_identity_number"].ToString() : null,
                IdentityProofUrl = row["c_identity_proof_url"] != DBNull.Value ? row["c_identity_proof_url"].ToString() : null,
                PhotoUrl        = row["c_photo_url"] != DBNull.Value ? row["c_photo_url"].ToString() : null,
                AddressProofUrl = row["c_address_url"] != DBNull.Value ? row["c_address_url"].ToString() : null,
                ResumeUrl       = row["c_resume_url"] != DBNull.Value ? row["c_resume_url"].ToString() : null,
                AgreementUrl    = row["c_agreement_url"] != DBNull.Value ? row["c_agreement_url"].ToString() : null,
                DocVerificationStatus = row["c_doc_verification_status"] != DBNull.Value ? row["c_doc_verification_status"].ToString() : null,
                InterviewResult  = row["c_interview_result"] != DBNull.Value ? row["c_interview_result"].ToString() : null,
                TrainingCompleted  = Convert.ToBoolean(row["c_training_passed"]),
                ActivationStatus = row["c_activation_status"] != DBNull.Value ? row["c_activation_status"].ToString() : null,
                BankAccountHolderName = row["c_bank_account_holder_name"] != DBNull.Value ? row["c_bank_account_holder_name"].ToString() : null,
                BankName          = row["c_bank_name"]               != DBNull.Value ? row["c_bank_name"].ToString() : null,
                BankAccountNumber = row["c_bank_account_number"]     != DBNull.Value ? row["c_bank_account_number"].ToString() : null,
                BankIfsc          = row["c_bank_ifsc"]               != DBNull.Value ? row["c_bank_ifsc"].ToString() : null,
                CompensationType  = row["c_compensation_type"]       != DBNull.Value ? row["c_compensation_type"].ToString() : null,
                PerEventRate      = row["c_per_event_rate"]          != DBNull.Value ? Convert.ToDecimal(row["c_per_event_rate"]) : null,
                MonthlySalary     = row["c_monthly_salary"]          != DBNull.Value ? Convert.ToDecimal(row["c_monthly_salary"]) : null,
                CancelledChequeUrl = row["c_cancelled_cheque_url"]   != DBNull.Value ? row["c_cancelled_cheque_url"].ToString() : null,
                AvailabilityCalendar = row["c_availability_calendar"] != DBNull.Value ? row["c_availability_calendar"].ToString() : null,
                PreferredEventTypes  = row["c_preferred_event_types"] != DBNull.Value ? row["c_preferred_event_types"].ToString() : null,
                MaxEventsPerMonth    = row["c_max_events_per_month"]  != DBNull.Value ? Convert.ToInt32(row["c_max_events_per_month"]) : null,
                TotalEventsSupervised = Convert.ToInt32(row["c_total_events_supervised"]),
                AverageRating        = row["c_average_rating"]        != DBNull.Value ? Convert.ToDecimal(row["c_average_rating"]) : null,
                CertificationStatus  = row["c_certification_status"]  != DBNull.Value ? row["c_certification_status"].ToString() : null,
                CreatedDate     = Convert.ToDateTime(row["c_createddate"]),
                ModifiedDate    = row["c_modifieddate"] != DBNull.Value ? Convert.ToDateTime(row["c_modifieddate"]) : null
            };
        }

        public bool UpdateSupervisorStatus(AdminSupervisorStatusUpdate request)
        {
            if (!Enum.IsDefined(typeof(SupervisorApprovalStatus), request.Status))
                return false;

            var statusEnum = (SupervisorApprovalStatus)request.Status;

            string dbStatus;
            string? statusReason = request.Reason;

            switch (statusEnum)
            {
                case SupervisorApprovalStatus.Approved:
                    dbStatus = "ACTIVE";
                    statusReason = null;
                    break;
                case SupervisorApprovalStatus.Rejected:
                    dbStatus = "REJECTED";
                    break;
                case SupervisorApprovalStatus.UnderReview:
                    dbStatus = "APPLIED";
                    statusReason = "UNDER_REVIEW:" + (request.Reason ?? "");
                    break;
                case SupervisorApprovalStatus.InfoRequested:
                    dbStatus = "APPLIED";
                    statusReason = "INFO_REQUESTED:" + (request.Reason ?? "");
                    break;
                default: // Pending
                    dbStatus = "APPLIED";
                    break;
            }

            string query = $@"
                UPDATE {Table.SysSupervisor}
                SET c_current_status = @DbStatus,
                    c_status_reason = @StatusReason,
                    c_modifiedby = @UpdatedBy,
                    c_modifieddate = NOW()
                WHERE c_supervisor_id = @SupervisorId";

            NpgsqlParameter[] parameters = {
                new NpgsqlParameter("@SupervisorId", request.SupervisorId),
                new NpgsqlParameter("@DbStatus", dbStatus),
                new NpgsqlParameter("@StatusReason", (object?)statusReason ?? DBNull.Value),
                new NpgsqlParameter("@UpdatedBy", request.UpdatedBy)
            };

            int rowsAffected = _dbHelper.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        #endregion

        #region Tab 2: Approved Supervisors

        public AdminActiveSupervisorListResponse GetActiveSupervisors(AdminActiveSupervisorListRequest request)
        {
            var queryBuilder = new StringBuilder($@"
                SELECT
                    s.c_supervisor_id AS SupervisorId,
                    s.c_full_name AS FullName,
                    s.c_email AS Email,
                    s.c_phone AS Phone,
                    COALESCE(c.c_cityname, '') AS City,
                    COALESCE(st.c_statename, '') AS State,
                    s.c_supervisor_type AS SupervisorType,
                    s.c_average_rating AS AverageRating,
                    COALESCE(s.c_total_events_supervised, 0) AS TotalEventsSupervised,
                    s.c_current_status AS CurrentStatus,
                    COALESCE(s.c_is_available, FALSE) AS IsAvailable,
                    CASE WHEN s.c_current_status = 'SUSPENDED' THEN TRUE ELSE FALSE END AS IsBlocked,
                    COALESCE(s.c_is_deleted, FALSE) AS IsDeleted,
                    s.c_createddate AS CreatedDate,
                    s.c_modifieddate AS LastUpdated
                FROM {Table.SysSupervisor} s
                LEFT JOIN {Table.City} c  ON c.c_cityid   = s.c_cityid
                LEFT JOIN {Table.State} st ON st.c_stateid = s.c_stateid
                WHERE s.c_current_status IN ('ACTIVE','SUSPENDED','DEACTIVATED')");

            var parameters = new List<NpgsqlParameter>();
            AppendActiveFilters(queryBuilder, parameters, request);

            // Sorting
            string sortColumn = request.SortBy switch
            {
                "FullName" => "s.c_full_name",
                "Rating" => "s.c_average_rating",
                "TotalEvents" => "s.c_total_events_supervised",
                "LastUpdated" => "s.c_modifieddate",
                _ => "s.c_createddate"
            };
            queryBuilder.Append($" ORDER BY {sortColumn} {(request.SortOrder == "ASC" ? "ASC" : "DESC")}");

            // Count query
            var countBuilder = new StringBuilder($@"
                SELECT COUNT(*)
                FROM {Table.SysSupervisor} s
                LEFT JOIN {Table.City} c  ON c.c_cityid   = s.c_cityid
                LEFT JOIN {Table.State} st ON st.c_stateid = s.c_stateid
                WHERE s.c_current_status IN ('ACTIVE','SUSPENDED','DEACTIVATED')");
            var countParams = new List<NpgsqlParameter>();
            AppendActiveFilters(countBuilder, countParams, request);

            int totalRecords = Convert.ToInt32(_dbHelper.ExecuteScalar(countBuilder.ToString(), countParams.ToArray()));

            // Pagination
            int offset = (request.PageNumber - 1) * request.PageSize;
            queryBuilder.Append($" LIMIT {request.PageSize} OFFSET {offset}");

            var dt = _dbHelper.Execute(queryBuilder.ToString(), parameters.ToArray());

            var supervisors = new List<AdminActiveSupervisorListItem>();
            foreach (DataRow row in dt.Rows)
            {
                supervisors.Add(MapActiveItem(row));
            }

            return new AdminActiveSupervisorListResponse
            {
                Supervisors = supervisors,
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize)
            };
        }

        public bool BlockSupervisor(long supervisorId, long blockedBy, string? reason)
        {
            string query = $@"
                UPDATE {Table.SysSupervisor}
                SET c_current_status = 'SUSPENDED',
                    c_suspended_by = @BlockedBy,
                    c_suspension_reason = @Reason,
                    c_suspension_date = NOW(),
                    c_modifiedby = @BlockedBy,
                    c_modifieddate = NOW()
                WHERE c_supervisor_id = @SupervisorId AND c_current_status = 'ACTIVE'";

            NpgsqlParameter[] parameters = {
                new NpgsqlParameter("@SupervisorId", supervisorId),
                new NpgsqlParameter("@BlockedBy", blockedBy),
                new NpgsqlParameter("@Reason", (object?)reason ?? DBNull.Value)
            };

            int rowsAffected = _dbHelper.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        public bool UnblockSupervisor(long supervisorId, long unblockedBy)
        {
            string query = $@"
                UPDATE {Table.SysSupervisor}
                SET c_current_status = 'ACTIVE',
                    c_suspended_by = NULL,
                    c_suspension_reason = NULL,
                    c_suspension_date = NULL,
                    c_modifiedby = @UnblockedBy,
                    c_modifieddate = NOW()
                WHERE c_supervisor_id = @SupervisorId AND c_current_status = 'SUSPENDED'";

            NpgsqlParameter[] parameters = {
                new NpgsqlParameter("@SupervisorId", supervisorId),
                new NpgsqlParameter("@UnblockedBy", unblockedBy)
            };

            int rowsAffected = _dbHelper.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        public bool DeleteSupervisor(long supervisorId, long deletedBy)
        {
            string query = $@"
                UPDATE {Table.SysSupervisor}
                SET c_is_deleted = TRUE,
                    c_modifiedby = @DeletedBy,
                    c_modifieddate = NOW()
                WHERE c_supervisor_id = @SupervisorId";

            NpgsqlParameter[] parameters = {
                new NpgsqlParameter("@SupervisorId", supervisorId),
                new NpgsqlParameter("@DeletedBy", deletedBy)
            };

            int rowsAffected = _dbHelper.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        public bool RestoreSupervisor(long supervisorId, long restoredBy)
        {
            string query = $@"
                UPDATE {Table.SysSupervisor}
                SET c_is_deleted = FALSE,
                    c_modifiedby = @RestoredBy,
                    c_modifieddate = NOW()
                WHERE c_supervisor_id = @SupervisorId AND c_is_deleted = TRUE";

            NpgsqlParameter[] parameters = {
                new NpgsqlParameter("@SupervisorId", supervisorId),
                new NpgsqlParameter("@RestoredBy", restoredBy)
            };

            int rowsAffected = _dbHelper.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        public List<AdminSupervisorExportItem> GetSupervisorsForExport(AdminActiveSupervisorListRequest request)
        {
            var queryBuilder = new StringBuilder($@"
                SELECT
                    s.c_supervisor_id AS SupervisorId,
                    s.c_full_name AS FullName,
                    s.c_email AS Email,
                    s.c_phone AS Phone,
                    COALESCE(c.c_cityname, '') AS City,
                    COALESCE(st.c_statename, '') AS State,
                    s.c_supervisor_type AS SupervisorType,
                    s.c_average_rating AS AverageRating,
                    COALESCE(s.c_total_events_supervised, 0) AS TotalEventsSupervised,
                    s.c_current_status AS CurrentStatus,
                    s.c_createddate AS CreatedDate
                FROM {Table.SysSupervisor} s
                LEFT JOIN {Table.City} c  ON c.c_cityid   = s.c_cityid
                LEFT JOIN {Table.State} st ON st.c_stateid = s.c_stateid
                WHERE s.c_current_status IN ('ACTIVE','SUSPENDED','DEACTIVATED')");

            var parameters = new List<NpgsqlParameter>();
            AppendActiveFilters(queryBuilder, parameters, request);

            queryBuilder.Append(" ORDER BY s.c_createddate DESC");

            var dt = _dbHelper.Execute(queryBuilder.ToString(), parameters.ToArray());

            var items = new List<AdminSupervisorExportItem>();
            foreach (DataRow row in dt.Rows)
            {
                items.Add(new AdminSupervisorExportItem
                {
                    SupervisorId = Convert.ToInt64(row["SupervisorId"]),
                    FullName = row["FullName"]?.ToString() ?? string.Empty,
                    Email = row["Email"]?.ToString() ?? string.Empty,
                    Phone = row["Phone"]?.ToString() ?? string.Empty,
                    City = row["City"]?.ToString() ?? string.Empty,
                    State = row["State"]?.ToString() ?? string.Empty,
                    SupervisorType = row["SupervisorType"]?.ToString() ?? string.Empty,
                    AverageRating = row["AverageRating"] != DBNull.Value ? Convert.ToDecimal(row["AverageRating"]) : null,
                    TotalEventsSupervised = Convert.ToInt32(row["TotalEventsSupervised"]),
                    CurrentStatus = row["CurrentStatus"]?.ToString() ?? string.Empty,
                    CreatedDate = Convert.ToDateTime(row["CreatedDate"])
                });
            }

            return items;
        }

        #endregion

        #region Private Helpers

        private void AppendRegistrationFilters(StringBuilder queryBuilder, List<NpgsqlParameter> parameters, AdminSupervisorRegistrationListRequest request)
        {
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                queryBuilder.Append(" AND (s.c_full_name LIKE @SearchTerm OR s.c_email LIKE @SearchTerm OR s.c_phone LIKE @SearchTerm)");
                parameters.Add(new NpgsqlParameter("@SearchTerm", "%" + request.SearchTerm + "%"));
            }

            if (request.Status.HasValue)
            {
                var statusEnum = (SupervisorApprovalStatus)request.Status.Value;
                switch (statusEnum)
                {
                    case SupervisorApprovalStatus.Pending:
                        queryBuilder.Append(" AND s.c_current_status NOT IN ('ACTIVE','REJECTED') AND (s.c_status_reason IS NULL OR (s.c_status_reason NOT LIKE 'UNDER_REVIEW:%' AND s.c_status_reason NOT LIKE 'INFO_REQUESTED:%'))");
                        break;
                    case SupervisorApprovalStatus.Approved:
                        queryBuilder.Append(" AND s.c_current_status = 'ACTIVE'");
                        break;
                    case SupervisorApprovalStatus.Rejected:
                        queryBuilder.Append(" AND s.c_current_status = 'REJECTED'");
                        break;
                    case SupervisorApprovalStatus.UnderReview:
                        queryBuilder.Append(" AND s.c_status_reason LIKE 'UNDER_REVIEW:%'");
                        break;
                    case SupervisorApprovalStatus.InfoRequested:
                        queryBuilder.Append(" AND s.c_status_reason LIKE 'INFO_REQUESTED:%'");
                        break;
                }
            }

            if (!string.IsNullOrEmpty(request.SupervisorType))
            {
                queryBuilder.Append(" AND s.c_supervisor_type = @SupervisorType");
                parameters.Add(new NpgsqlParameter("@SupervisorType", request.SupervisorType));
            }

            // Exclude soft-deleted
            queryBuilder.Append(" AND COALESCE(s.c_is_deleted, FALSE) = FALSE");
        }

        private void AppendActiveFilters(StringBuilder queryBuilder, List<NpgsqlParameter> parameters, AdminActiveSupervisorListRequest request)
        {
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                queryBuilder.Append(" AND (s.c_full_name LIKE @SearchTerm OR s.c_email LIKE @SearchTerm OR s.c_phone LIKE @SearchTerm)");
                parameters.Add(new NpgsqlParameter("@SearchTerm", "%" + request.SearchTerm + "%"));
            }

            if (!string.IsNullOrEmpty(request.SupervisorType))
            {
                queryBuilder.Append(" AND s.c_supervisor_type = @SupervisorType");
                parameters.Add(new NpgsqlParameter("@SupervisorType", request.SupervisorType));
            }

            if (!string.IsNullOrEmpty(request.City))
            {
                queryBuilder.Append(" AND c.c_cityname LIKE @City");
                parameters.Add(new NpgsqlParameter("@City", "%" + request.City + "%"));
            }

            if (!string.IsNullOrEmpty(request.State))
            {
                queryBuilder.Append(" AND st.c_statename LIKE @State");
                parameters.Add(new NpgsqlParameter("@State", "%" + request.State + "%"));
            }

            if (request.IsBlocked.HasValue && request.IsBlocked.Value)
            {
                queryBuilder.Append(" AND s.c_current_status = 'SUSPENDED'");
            }

            if (request.DateFrom.HasValue)
            {
                queryBuilder.Append(" AND s.c_createddate >= @DateFrom");
                parameters.Add(new NpgsqlParameter("@DateFrom", request.DateFrom.Value.Date));
            }

            if (request.DateTo.HasValue)
            {
                queryBuilder.Append(" AND s.c_createddate <= @DateTo");
                parameters.Add(new NpgsqlParameter("@DateTo", request.DateTo.Value.Date.AddDays(1).AddTicks(-1)));
            }

            if (request.IsDeleted.HasValue && request.IsDeleted.Value)
            {
                queryBuilder.Append(" AND COALESCE(s.c_is_deleted, FALSE) = TRUE");
            }
            else
            {
                queryBuilder.Append(" AND COALESCE(s.c_is_deleted, FALSE) = FALSE");
            }
        }

        private AdminSupervisorRegistrationListItem MapRegistrationItem(DataRow row)
        {
            return new AdminSupervisorRegistrationListItem
            {
                SupervisorId = Convert.ToInt64(row["SupervisorId"]),
                FullName = row["FullName"]?.ToString() ?? string.Empty,
                Email = row["Email"]?.ToString() ?? string.Empty,
                Phone = row["Phone"]?.ToString() ?? string.Empty,
                City = row["City"]?.ToString() ?? string.Empty,
                State = row["State"]?.ToString() ?? string.Empty,
                SupervisorType = row["SupervisorType"]?.ToString() ?? string.Empty,
                Status = Convert.ToInt32(row["Status"]),
                StatusReason = row["StatusReason"]?.ToString(),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                ApprovedDate = row["ApprovedDate"] != DBNull.Value ? Convert.ToDateTime(row["ApprovedDate"]) : null
            };
        }

        private AdminActiveSupervisorListItem MapActiveItem(DataRow row)
        {
            return new AdminActiveSupervisorListItem
            {
                SupervisorId = Convert.ToInt64(row["SupervisorId"]),
                FullName = row["FullName"]?.ToString() ?? string.Empty,
                Email = row["Email"]?.ToString() ?? string.Empty,
                Phone = row["Phone"]?.ToString() ?? string.Empty,
                City = row["City"]?.ToString() ?? string.Empty,
                State = row["State"]?.ToString() ?? string.Empty,
                SupervisorType = row["SupervisorType"]?.ToString() ?? string.Empty,
                AverageRating = row["AverageRating"] != DBNull.Value ? Convert.ToDecimal(row["AverageRating"]) : null,
                TotalEventsSupervised = Convert.ToInt32(row["TotalEventsSupervised"]),
                CurrentStatus = row["CurrentStatus"]?.ToString() ?? string.Empty,
                IsAvailable = Convert.ToBoolean(row["IsAvailable"]),
                IsBlocked = Convert.ToBoolean(row["IsBlocked"]),
                IsDeleted = Convert.ToBoolean(row["IsDeleted"]),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                LastUpdated = row["LastUpdated"] != DBNull.Value ? Convert.ToDateTime(row["LastUpdated"]) : null
            };
        }

        #endregion
    }
}

