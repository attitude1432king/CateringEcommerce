using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Enums.Admin;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.Data.SqlClient;
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
                    ISNULL(s.c_city, '') AS City,
                    ISNULL(s.c_state, '') AS State,
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
                WHERE s.c_current_status IN ('APPLIED','REJECTED','DOCUMENT_VERIFICATION','AWAITING_INTERVIEW','AWAITING_TRAINING','AWAITING_CERTIFICATION','RESUME_SCREENED','INTERVIEW_SCHEDULED','INTERVIEW_PASSED','BACKGROUND_VERIFICATION','TRAINING')");

            var parameters = new List<SqlParameter>();
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
            var countParams = new List<SqlParameter>();
            AppendRegistrationFilters(countBuilder, countParams, request);

            int totalRecords = Convert.ToInt32(_dbHelper.ExecuteScalar(countBuilder.ToString(), countParams.ToArray()));

            // Pagination
            int offset = (request.PageNumber - 1) * request.PageSize;
            queryBuilder.Append($" OFFSET {offset} ROWS FETCH NEXT {request.PageSize} ROWS ONLY");

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
                    c_modified_by = @UpdatedBy,
                    c_modifieddate = GETDATE()
                WHERE c_supervisor_id = @SupervisorId";

            SqlParameter[] parameters = {
                new SqlParameter("@SupervisorId", request.SupervisorId),
                new SqlParameter("@DbStatus", dbStatus),
                new SqlParameter("@StatusReason", (object?)statusReason ?? DBNull.Value),
                new SqlParameter("@UpdatedBy", request.UpdatedBy)
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
                    ISNULL(s.c_city, '') AS City,
                    ISNULL(s.c_state, '') AS State,
                    s.c_supervisor_type AS SupervisorType,
                    s.c_average_rating AS AverageRating,
                    ISNULL(s.c_total_events_supervised, 0) AS TotalEventsSupervised,
                    s.c_current_status AS CurrentStatus,
                    ISNULL(s.c_is_available, 0) AS IsAvailable,
                    CASE WHEN s.c_current_status = 'SUSPENDED' THEN 1 ELSE 0 END AS IsBlocked,
                    ISNULL(s.c_is_deleted, 0) AS IsDeleted,
                    s.c_createddate AS CreatedDate,
                    s.c_modifieddate AS LastUpdated
                FROM {Table.SysSupervisor} s
                WHERE s.c_current_status IN ('ACTIVE','SUSPENDED','DEACTIVATED')");

            var parameters = new List<SqlParameter>();
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
                WHERE s.c_current_status IN ('ACTIVE','SUSPENDED','DEACTIVATED')");
            var countParams = new List<SqlParameter>();
            AppendActiveFilters(countBuilder, countParams, request);

            int totalRecords = Convert.ToInt32(_dbHelper.ExecuteScalar(countBuilder.ToString(), countParams.ToArray()));

            // Pagination
            int offset = (request.PageNumber - 1) * request.PageSize;
            queryBuilder.Append($" OFFSET {offset} ROWS FETCH NEXT {request.PageSize} ROWS ONLY");

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
                    c_suspension_date = GETDATE(),
                    c_modified_by = @BlockedBy,
                    c_modifieddate = GETDATE()
                WHERE c_supervisor_id = @SupervisorId AND c_current_status = 'ACTIVE'";

            SqlParameter[] parameters = {
                new SqlParameter("@SupervisorId", supervisorId),
                new SqlParameter("@BlockedBy", blockedBy),
                new SqlParameter("@Reason", (object?)reason ?? DBNull.Value)
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
                    c_modified_by = @UnblockedBy,
                    c_modifieddate = GETDATE()
                WHERE c_supervisor_id = @SupervisorId AND c_current_status = 'SUSPENDED'";

            SqlParameter[] parameters = {
                new SqlParameter("@SupervisorId", supervisorId),
                new SqlParameter("@UnblockedBy", unblockedBy)
            };

            int rowsAffected = _dbHelper.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        public bool DeleteSupervisor(long supervisorId, long deletedBy)
        {
            string query = $@"
                UPDATE {Table.SysSupervisor}
                SET c_is_deleted = 1,
                    c_modified_by = @DeletedBy,
                    c_modifieddate = GETDATE()
                WHERE c_supervisor_id = @SupervisorId";

            SqlParameter[] parameters = {
                new SqlParameter("@SupervisorId", supervisorId),
                new SqlParameter("@DeletedBy", deletedBy)
            };

            int rowsAffected = _dbHelper.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        public bool RestoreSupervisor(long supervisorId, long restoredBy)
        {
            string query = $@"
                UPDATE {Table.SysSupervisor}
                SET c_is_deleted = 0,
                    c_modified_by = @RestoredBy,
                    c_modifieddate = GETDATE()
                WHERE c_supervisor_id = @SupervisorId AND c_is_deleted = 1";

            SqlParameter[] parameters = {
                new SqlParameter("@SupervisorId", supervisorId),
                new SqlParameter("@RestoredBy", restoredBy)
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
                    ISNULL(s.c_city, '') AS City,
                    ISNULL(s.c_state, '') AS State,
                    s.c_supervisor_type AS SupervisorType,
                    s.c_average_rating AS AverageRating,
                    ISNULL(s.c_total_events_supervised, 0) AS TotalEventsSupervised,
                    s.c_current_status AS CurrentStatus,
                    s.c_createddate AS CreatedDate
                FROM {Table.SysSupervisor} s
                WHERE s.c_current_status IN ('ACTIVE','SUSPENDED','DEACTIVATED')");

            var parameters = new List<SqlParameter>();
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

        private void AppendRegistrationFilters(StringBuilder queryBuilder, List<SqlParameter> parameters, AdminSupervisorRegistrationListRequest request)
        {
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                queryBuilder.Append(" AND (s.c_full_name LIKE @SearchTerm OR s.c_email LIKE @SearchTerm OR s.c_phone LIKE @SearchTerm)");
                parameters.Add(new SqlParameter("@SearchTerm", "%" + request.SearchTerm + "%"));
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
                parameters.Add(new SqlParameter("@SupervisorType", request.SupervisorType));
            }

            // Exclude soft-deleted
            queryBuilder.Append(" AND ISNULL(s.c_is_deleted, 0) = 0");
        }

        private void AppendActiveFilters(StringBuilder queryBuilder, List<SqlParameter> parameters, AdminActiveSupervisorListRequest request)
        {
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                queryBuilder.Append(" AND (s.c_full_name LIKE @SearchTerm OR s.c_email LIKE @SearchTerm OR s.c_phone LIKE @SearchTerm)");
                parameters.Add(new SqlParameter("@SearchTerm", "%" + request.SearchTerm + "%"));
            }

            if (!string.IsNullOrEmpty(request.SupervisorType))
            {
                queryBuilder.Append(" AND s.c_supervisor_type = @SupervisorType");
                parameters.Add(new SqlParameter("@SupervisorType", request.SupervisorType));
            }

            if (!string.IsNullOrEmpty(request.City))
            {
                queryBuilder.Append(" AND s.c_city LIKE @City");
                parameters.Add(new SqlParameter("@City", "%" + request.City + "%"));
            }

            if (!string.IsNullOrEmpty(request.State))
            {
                queryBuilder.Append(" AND s.c_state LIKE @State");
                parameters.Add(new SqlParameter("@State", "%" + request.State + "%"));
            }

            if (request.IsBlocked.HasValue && request.IsBlocked.Value)
            {
                queryBuilder.Append(" AND s.c_current_status = 'SUSPENDED'");
            }

            if (request.DateFrom.HasValue)
            {
                queryBuilder.Append(" AND s.c_createddate >= @DateFrom");
                parameters.Add(new SqlParameter("@DateFrom", request.DateFrom.Value.Date));
            }

            if (request.DateTo.HasValue)
            {
                queryBuilder.Append(" AND s.c_createddate <= @DateTo");
                parameters.Add(new SqlParameter("@DateTo", request.DateTo.Value.Date.AddDays(1).AddTicks(-1)));
            }

            if (request.IsDeleted.HasValue && request.IsDeleted.Value)
            {
                queryBuilder.Append(" AND ISNULL(s.c_is_deleted, 0) = 1");
            }
            else
            {
                queryBuilder.Append(" AND ISNULL(s.c_is_deleted, 0) = 0");
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
