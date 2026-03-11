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
    public class AdminCateringRepository : IAdminCateringRepository
    {
        private readonly IDatabaseHelper _dbHelper;
        public AdminCateringRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public AdminCateringListResponse GetAllCaterings(AdminCateringListRequest request)
        {
            var queryBuilder = new StringBuilder($@"
                SELECT
                    co.c_ownerid AS CateringId,
                    co.c_catering_name AS BusinessName,
                    co.c_owner_name AS OwnerName,
                    co.c_mobile AS Phone,
                    co.c_email AS Email,
                    c.c_cityname AS City,
                    s.c_statename AS State,
                    co.c_approval_status AS Status,
                    ISNULL(co.c_verified_by_admin, 0) AS IsVerified,
                    ISNULL(co.c_isactive, 0) AS IsActive,
                    ISNULL(co.c_isblocked, 0) AS IsBlocked,
                    ISNULL(co.c_isdeleted, 0) AS IsDeleted,
                    ISNULL(AVG(CAST(cr.c_overall_rating AS DECIMAL(3,2))), 0) AS Rating,
                    COUNT(DISTINCT cr.c_reviewid) AS TotalReviews,
                    COUNT(DISTINCT o.c_orderid) AS TotalOrders,
                    ISNULL(SUM(o.c_total_amount), 0) AS TotalEarnings,
                    co.c_createddate AS CreatedDate,
                    co.c_approved_date AS ApprovedDate
                FROM {Table.SysCateringOwner} co
                LEFT JOIN {Table.SysCateringOwnerAddress} cd ON cd.c_ownerid = co.c_ownerid
                LEFT JOIN {Table.City} c ON cd.c_cityid = c.c_cityid
                LEFT JOIN {Table.State} s ON cd.c_stateid = s.c_stateid
                LEFT JOIN {Table.SysCateringReview} cr ON co.c_ownerid = cr.c_ownerid
                LEFT JOIN {Table.SysOrders} o ON co.c_ownerid = o.c_ownerid AND o.c_order_status = 'Completed'
                WHERE 1=1");

            var parameters = new List<SqlParameter>();
            AppendFilters(queryBuilder, parameters, request);

            queryBuilder.Append(@"
                GROUP BY co.c_ownerid, co.c_catering_name, co.c_owner_name, co.c_mobile,
                         co.c_email, c.c_cityname, s.c_statename, co.c_approval_status, co.c_verified_by_admin,
                         co.c_isactive, co.c_isblocked, co.c_isdeleted,
                         co.c_createddate, co.c_approved_date");

            // Sorting
            string sortColumn = request.SortBy switch
            {
                "BusinessName" => "co.c_catering_name",
                "Rating" => "Rating",
                "TotalOrders" => "TotalOrders",
                "TotalEarnings" => "TotalEarnings",
                _ => "co.c_createddate"
            };
            queryBuilder.Append($" ORDER BY {sortColumn} {(request.SortOrder == "ASC" ? "ASC" : "DESC")}");

            // Count query
            var countBuilder = new StringBuilder($@"
                SELECT COUNT(DISTINCT co.c_ownerid)
                FROM {Table.SysCateringOwner} co
                LEFT JOIN {Table.SysCateringOwnerAddress} cd ON cd.c_ownerid = co.c_ownerid
                LEFT JOIN {Table.City} c ON cd.c_cityid = c.c_cityid
                LEFT JOIN {Table.State} s ON cd.c_stateid = s.c_stateid
                WHERE 1=1");
            var countParams = new List<SqlParameter>();
            AppendFilters(countBuilder, countParams, request);

            int totalRecords = Convert.ToInt32(_dbHelper.ExecuteScalar(countBuilder.ToString(), countParams.ToArray()));

            // Pagination
            int offset = (request.PageNumber - 1) * request.PageSize;
            queryBuilder.Append($" OFFSET {offset} ROWS FETCH NEXT {request.PageSize} ROWS ONLY");

            var dt = _dbHelper.Execute(queryBuilder.ToString(), parameters.ToArray());

            var caterings = new List<AdminCateringListItem>();
            foreach (DataRow row in dt.Rows)
            {
                caterings.Add(MapListItem(row));
            }

            return new AdminCateringListResponse
            {
                Caterings = caterings,
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize)
            };
        }

        public AdminCateringDetail? GetCateringById(long cateringId)
        {
            string query = $@"
                SELECT
                    co.c_ownerid, co.c_catering_name, co.c_owner_name, co.c_mobile, co.c_email,
                    co.c_catering_number, co.c_gst_number, co.c_fssai_license, co.c_pan_number,
                    addr.c_address_line1, addr.c_address_line2, c.c_cityname, s.c_statename, addr.c_pincode,
                    co.c_approval_status, ISNULL(co.c_verified_by_admin, 0) AS c_verified_by_admin,
                    ISNULL(co.c_isactive, 0) AS c_isactive, ISNULL(co.c_isblocked, 0) AS c_isblocked,
                    co.c_block_reason,
                    bank.c_bank_name, bank.c_account_number, bank.c_ifsc_code, bank.c_account_holder_name,
                    co.c_createddate, co.c_approved_date, co.c_modifieddate
                FROM {Table.SysCateringOwner} co
                LEFT JOIN {Table.SysCateringOwnerAddress} addr ON co.c_ownerid = addr.c_ownerid
                LEFT JOIN {Table.City} c ON addr.c_cityid = c.c_cityid
                LEFT JOIN {Table.State} s ON addr.c_stateid = s.c_stateid
                LEFT JOIN {Table.SysCateringOwnerBankDetails} bank ON co.c_ownerid = bank.c_ownerid
                WHERE co.c_ownerid = @CateringId";

            SqlParameter[] parameters = { new SqlParameter("@CateringId", cateringId) };
            var dt = _dbHelper.Execute(query, parameters);

            if (dt.Rows.Count == 0) return null;

            var row = dt.Rows[0];
            var stats = GetCateringStats(cateringId);
            var images = GetCateringImages(cateringId);

            return new AdminCateringDetail
            {
                CateringId = Convert.ToInt64(row["c_ownerid"]),
                BusinessName = row["c_catering_name"]?.ToString() ?? string.Empty,
                OwnerName = row["c_owner_name"]?.ToString() ?? string.Empty,
                Phone = row["c_mobile"]?.ToString() ?? string.Empty,
                Email = row["c_email"]?.ToString() ?? string.Empty,
                AlternatePhone = row["c_catering_number"]?.ToString(),
                GstNumber = row["c_gst_number"]?.ToString(),
                FssaiNumber = row["c_fssai_license"]?.ToString(),
                PanNumber = row["c_pan_number"]?.ToString(),
                AddressLine1 = row["c_address_line1"]?.ToString() ?? string.Empty,
                AddressLine2 = row["c_address_line2"]?.ToString(),
                City = row["c_cityname"]?.ToString() ?? string.Empty,
                State = row["c_statename"]?.ToString() ?? string.Empty,
                Pincode = row["c_pincode"]?.ToString() ?? string.Empty,
                Status = row["c_approval_status"] != DBNull.Value ? Convert.ToInt32(row["c_approval_status"]) : 1,
                IsVerified = Convert.ToBoolean(row["c_verified_by_admin"]),
                IsActive = Convert.ToBoolean(row["c_isactive"]),
                IsBlocked = Convert.ToBoolean(row["c_isblocked"]),
                BlockReason = row["c_block_reason"]?.ToString(),
                AverageRating = stats.AverageRating,
                TotalReviews = stats.TotalReviews,
                TotalOrders = stats.TotalOrders,
                TotalEarnings = stats.TotalEarnings,
                PlatformCommission = stats.PlatformCommission,
                BankName = row["c_bank_name"]?.ToString(),
                AccountNumber = row["c_account_number"]?.ToString(),
                IfscCode = row["c_ifsc_code"]?.ToString(),
                AccountHolderName = row["c_account_holder_name"]?.ToString(),
                Images = images,
                CreatedDate = Convert.ToDateTime(row["c_createddate"]),
                ApprovedDate = row["c_approved_date"] != DBNull.Value ? Convert.ToDateTime(row["c_approved_date"]) : null,
                LastModified = row["c_modifieddate"] != DBNull.Value ? Convert.ToDateTime(row["c_modifieddate"]) : null
            };
        }

        public bool UpdateCateringStatus(AdminCateringStatusUpdate request)
        {
            // Status enum: 1=Pending, 2=Approved, 3=Rejected, 4=UnderReview, 5=InfoRequested
            string query = $@"
                UPDATE {Table.SysCateringOwner}
                SET c_approval_status = @Status,
                    c_isactive = CASE WHEN @Status = 2 THEN 1 ELSE c_isactive END,
                    c_isblocked = CASE WHEN @Status = 3 THEN 1 ELSE 0 END,
                    c_block_reason = @Reason,
                    c_approved_date = CASE WHEN @Status = 2 AND c_approved_date IS NULL THEN GETDATE() ELSE c_approved_date END,
                    c_modifieddate = GETDATE()
                WHERE c_ownerid = @CateringId";

            SqlParameter[] parameters = {
                new SqlParameter("@CateringId", request.CateringId),
                new SqlParameter("@Status", request.Status),
                new SqlParameter("@Reason", (object?)request.Reason ?? DBNull.Value)
            };

            int rowsAffected = _dbHelper.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        public bool DeleteCatering(long cateringId, long deletedBy)
        {
            string query = $@"
                UPDATE {Table.SysCateringOwner}
                SET c_isdeleted = 1,
                    c_isactive = 0,
                    c_deleted_by = @DeletedBy,
                    c_deleted_date = GETDATE()
                WHERE c_ownerid = @CateringId";

            SqlParameter[] parameters = {
                new SqlParameter("@CateringId", cateringId),
                new SqlParameter("@DeletedBy", deletedBy)
            };

            int rowsAffected = _dbHelper.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        public bool RestoreCatering(long cateringId, long restoredBy)
        {
            string query = $@"
                UPDATE {Table.SysCateringOwner}
                SET c_isdeleted = 0,
                    c_isactive = 1,
                    c_deleted_by = NULL,
                    c_deleted_date = NULL,
                    c_modifieddate = GETDATE()
                WHERE c_ownerid = @CateringId AND c_isdeleted = 1";

            SqlParameter[] parameters = {
                new SqlParameter("@CateringId", cateringId)
            };

            int rowsAffected = _dbHelper.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        public List<AdminCateringExportItem> GetCateringsForExport(AdminCateringListRequest request)
        {
            var queryBuilder = new StringBuilder($@"
                SELECT
                    co.c_ownerid AS CateringId,
                    co.c_catering_name AS BusinessName,
                    co.c_owner_name AS OwnerName,
                    co.c_mobile AS Phone,
                    co.c_email AS Email,
                    c.c_cityname AS City,
                    s.c_statename AS State,
                    co.c_approval_status AS Status,
                    ISNULL(co.c_verified_by_admin, 0) AS IsVerified,
                    ISNULL(co.c_isactive, 0) AS IsActive,
                    ISNULL(co.c_isblocked, 0) AS IsBlocked,
                    ISNULL(AVG(CAST(cr.c_overall_rating AS DECIMAL(3,2))), 0) AS Rating,
                    COUNT(DISTINCT cr.c_reviewid) AS TotalReviews,
                    COUNT(DISTINCT o.c_orderid) AS TotalOrders,
                    ISNULL(SUM(o.c_total_amount), 0) AS TotalEarnings,
                    co.c_createddate AS CreatedDate
                FROM {Table.SysCateringOwner} co
                LEFT JOIN {Table.SysCateringOwnerAddress} cd ON cd.c_ownerid = co.c_ownerid
                LEFT JOIN {Table.City} c ON cd.c_cityid = c.c_cityid
                LEFT JOIN {Table.State} s ON cd.c_stateid = s.c_stateid
                LEFT JOIN {Table.SysCateringReview} cr ON co.c_ownerid = cr.c_ownerid
                LEFT JOIN {Table.SysOrders} o ON co.c_ownerid = o.c_ownerid AND o.c_order_status = 'Completed'
                WHERE 1=1");

            var parameters = new List<SqlParameter>();
            AppendFilters(queryBuilder, parameters, request);

            queryBuilder.Append(@"
                GROUP BY co.c_ownerid, co.c_catering_name, co.c_owner_name, co.c_mobile,
                         co.c_email, c.c_cityname, s.c_statename, co.c_approval_status, co.c_verified_by_admin,
                         co.c_isactive, co.c_isblocked,
                         co.c_createddate
                ORDER BY co.c_createddate DESC");

            var dt = _dbHelper.Execute(queryBuilder.ToString(), parameters.ToArray());

            var items = new List<AdminCateringExportItem>();
            foreach (DataRow row in dt.Rows)
            {
                items.Add(new AdminCateringExportItem
                {
                    CateringId = Convert.ToInt64(row["CateringId"]),
                    BusinessName = row["BusinessName"]?.ToString() ?? string.Empty,
                    OwnerName = row["OwnerName"]?.ToString() ?? string.Empty,
                    Phone = row["Phone"]?.ToString() ?? string.Empty,
                    Email = row["Email"]?.ToString() ?? string.Empty,
                    City = row["City"]?.ToString() ?? string.Empty,
                    State = row["State"]?.ToString() ?? string.Empty,
                    Status = row["Status"] != DBNull.Value ? Convert.ToInt32(row["Status"]) : 1,
                    IsVerified = Convert.ToBoolean(row["IsVerified"]),
                    IsActive = Convert.ToBoolean(row["IsActive"]),
                    IsBlocked = Convert.ToBoolean(row["IsBlocked"]),
                    Rating = row["Rating"] != DBNull.Value ? Convert.ToDecimal(row["Rating"]) : null,
                    TotalReviews = Convert.ToInt32(row["TotalReviews"]),
                    TotalOrders = Convert.ToInt32(row["TotalOrders"]),
                    TotalEarnings = Convert.ToDecimal(row["TotalEarnings"]),
                    CreatedDate = Convert.ToDateTime(row["CreatedDate"])
                });
            }

            return items;
        }

        #region Private Helpers

        private void AppendFilters(StringBuilder queryBuilder, List<SqlParameter> parameters, AdminCateringListRequest request)
        {
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                queryBuilder.Append(" AND (co.c_catering_name LIKE @SearchTerm OR co.c_owner_name LIKE @SearchTerm OR co.c_mobile LIKE @SearchTerm OR co.c_email LIKE @SearchTerm)");
                parameters.Add(new SqlParameter("@SearchTerm", "%" + request.SearchTerm + "%"));
            }

            if (request.StateId.HasValue)
            {
                queryBuilder.Append(" AND cd.c_stateid = @StateId");
                parameters.Add(new SqlParameter("@StateId", request.StateId.Value));
            }

            if (request.CityId.HasValue)
            {
                queryBuilder.Append(" AND cd.c_cityid = @CityId");
                parameters.Add(new SqlParameter("@CityId", request.CityId.Value));
            }

            if (request.Status.HasValue)
            {
                queryBuilder.Append(" AND co.c_approval_status = @Status");
                parameters.Add(new SqlParameter("@Status", request.Status.Value));
            }

            if (request.IsBlocked.HasValue)
            {
                queryBuilder.Append(" AND ISNULL(co.c_isblocked, 0) = @IsBlocked");
                parameters.Add(new SqlParameter("@IsBlocked", request.IsBlocked.Value ? 1 : 0));
            }

            if (request.IsActive.HasValue)
            {
                queryBuilder.Append(" AND ISNULL(co.c_isactive, 0) = @IsActive");
                parameters.Add(new SqlParameter("@IsActive", request.IsActive.Value ? 1 : 0));
            }

            if (request.IsDeleted.HasValue && request.IsDeleted.Value)
            {
                queryBuilder.Append(" AND ISNULL(co.c_isdeleted, 0) = 1");
            }
            else
            {
                queryBuilder.Append(" AND ISNULL(co.c_isdeleted, 0) = 0");
            }

            if (!string.IsNullOrEmpty(request.VerificationStatus))
            {
                bool isVerified = request.VerificationStatus.Equals("Verified", StringComparison.OrdinalIgnoreCase);
                queryBuilder.Append(" AND ISNULL(co.c_verified_by_admin, 0) = @IsVerified");
                parameters.Add(new SqlParameter("@IsVerified", isVerified ? 1 : 0));
            }
        }

        private AdminCateringListItem MapListItem(DataRow row)
        {
            return new AdminCateringListItem
            {
                CateringId = Convert.ToInt64(row["CateringId"]),
                BusinessName = row["BusinessName"]?.ToString() ?? string.Empty,
                OwnerName = row["OwnerName"]?.ToString() ?? string.Empty,
                Phone = row["Phone"]?.ToString() ?? string.Empty,
                Email = row["Email"]?.ToString() ?? string.Empty,
                City = row["City"]?.ToString() ?? string.Empty,
                State = row["State"]?.ToString() ?? string.Empty,
                Status = row["Status"] != DBNull.Value ? Convert.ToInt32(row["Status"]) : 1,
                IsVerified = Convert.ToBoolean(row["IsVerified"]),
                IsActive = Convert.ToBoolean(row["IsActive"]),
                IsBlocked = Convert.ToBoolean(row["IsBlocked"]),
                IsDeleted = Convert.ToBoolean(row["IsDeleted"]),
                Rating = row["Rating"] != DBNull.Value ? Convert.ToDecimal(row["Rating"]) : null,
                TotalReviews = Convert.ToInt32(row["TotalReviews"]),
                TotalOrders = Convert.ToInt32(row["TotalOrders"]),
                TotalEarnings = Convert.ToDecimal(row["TotalEarnings"]),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                ApprovedDate = row["ApprovedDate"] != DBNull.Value ? Convert.ToDateTime(row["ApprovedDate"]) : null
            };
        }

        private (decimal? AverageRating, int TotalReviews, int TotalOrders, decimal TotalEarnings, decimal PlatformCommission) GetCateringStats(long cateringId)
        {
            string query = $@"
                SELECT
                    ISNULL(AVG(CAST(cr.c_overall_rating AS DECIMAL(3,2))), 0) AS AvgRating,
                    COUNT(DISTINCT cr.c_reviewid) AS TotalReviews,
                    COUNT(DISTINCT o.c_orderid) AS TotalOrders,
                    ISNULL(SUM(o.c_total_amount), 0) AS TotalEarnings,
                    ISNULL(SUM(o.c_platform_commission), 0) AS Commission
                FROM {Table.SysCateringOwner} co
                LEFT JOIN {Table.SysCateringReview} cr ON co.c_ownerid = cr.c_ownerid
                LEFT JOIN {Table.SysOrders} o ON co.c_ownerid = o.c_ownerid AND o.c_order_status = 'Completed'
                WHERE co.c_ownerid = @CateringId";

            SqlParameter[] parameters = { new SqlParameter("@CateringId", cateringId) };
            var dt = _dbHelper.Execute(query, parameters);

            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                return (
                    row["AvgRating"] != DBNull.Value ? Convert.ToDecimal(row["AvgRating"]) : null,
                    Convert.ToInt32(row["TotalReviews"]),
                    Convert.ToInt32(row["TotalOrders"]),
                    Convert.ToDecimal(row["TotalEarnings"]),
                    Convert.ToDecimal(row["Commission"])
                );
            }

            return (null, 0, 0, 0, 0);
        }

        private List<string> GetCateringImages(long cateringId)
        {
            string query = $@"
                SELECT c_image_url
                FROM {Table.SysCateringOwnerImages}
                WHERE c_ownerid = @CateringId
                ORDER BY c_createddate DESC";

            SqlParameter[] parameters = { new SqlParameter("@CateringId", cateringId) };
            var dt = _dbHelper.Execute(query, parameters);

            var images = new List<string>();
            foreach (DataRow row in dt.Rows)
            {
                images.Add(row["c_image_url"]?.ToString() ?? string.Empty);
            }

            return images;
        }

        #endregion
    }
}
