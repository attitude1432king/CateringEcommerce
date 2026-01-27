using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace CateringEcommerce.BAL.Common.Admin
{
    public class AdminCateringRepository : IAdminCateringRepository
    {
        private readonly SqlDatabaseManager _db;

        public AdminCateringRepository(string connectionString)
        {
            _db = new SqlDatabaseManager();
            _db.SetConnectionString(connectionString);
        }

        public AdminCateringListResponse GetAllCaterings(AdminCateringListRequest request)
        {
            var queryBuilder = new StringBuilder($@"
                SELECT
                    co.c_catering_ownerid AS CateringId,
                    co.c_business_name AS BusinessName,
                    co.c_name AS OwnerName,
                    co.c_mobile AS Phone,
                    co.c_email AS Email,
                    c.c_cityname AS City,
                    s.c_statename AS State,
                    co.c_status AS Status,
                    co.c_isverified AS IsVerified,
                    ISNULL(AVG(CAST(cr.c_rating AS DECIMAL(3,2))), 0) AS Rating,
                    COUNT(DISTINCT cr.c_reviewid) AS TotalReviews,
                    COUNT(DISTINCT o.c_orderid) AS TotalOrders,
                    ISNULL(SUM(o.c_total_amount), 0) AS TotalEarnings,
                    co.c_created_date AS CreatedDate,
                    co.c_approved_date AS ApprovedDate
                FROM {Table.SysCateringOwner} co
                LEFT JOIN {Table.City} c ON co.c_cityid = c.c_cityid
                LEFT JOIN {Table.State} s ON co.c_stateid = s.c_stateid
                LEFT JOIN {Table.SysCateringReview} cr ON co.c_catering_ownerid = cr.c_catering_ownerid
                LEFT JOIN {Table.SysOrders} o ON co.c_catering_ownerid = o.c_catering_ownerid AND o.c_status = 'Completed'
                WHERE 1=1");

            var parameters = new List<SqlParameter>();

            // Apply filters
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                queryBuilder.Append(" AND (co.c_business_name LIKE @SearchTerm OR co.c_name LIKE @SearchTerm OR co.c_mobile LIKE @SearchTerm)");
                parameters.Add(new SqlParameter("@SearchTerm", "%" + request.SearchTerm + "%"));
            }

            if (request.CityId.HasValue)
            {
                queryBuilder.Append(" AND co.c_cityid = @CityId");
                parameters.Add(new SqlParameter("@CityId", request.CityId.Value));
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                queryBuilder.Append(" AND co.c_status = @Status");
                parameters.Add(new SqlParameter("@Status", request.Status));
            }

            if (!string.IsNullOrEmpty(request.VerificationStatus))
            {
                bool isVerified = request.VerificationStatus.Equals("Verified", StringComparison.OrdinalIgnoreCase);
                queryBuilder.Append(" AND co.c_isverified = @IsVerified");
                parameters.Add(new SqlParameter("@IsVerified", isVerified));
            }

            queryBuilder.Append(@"
                GROUP BY co.c_catering_ownerid, co.c_business_name, co.c_name, co.c_mobile,
                         co.c_email, c.c_cityname, s.c_statename, co.c_status, co.c_isverified,
                         co.c_created_date, co.c_approved_date");

            // Add sorting
            string sortColumn = request.SortBy switch
            {
                "BusinessName" => "co.c_business_name",
                "Rating" => "Rating",
                _ => "co.c_created_date"
            };

            queryBuilder.Append($" ORDER BY {sortColumn} {request.SortOrder}");

            // Get total count
            string countQuery = $@"
                SELECT COUNT(DISTINCT co.c_catering_ownerid)
                FROM {Table.SysCateringOwner} co
                WHERE 1=1" + (parameters.Count > 0 ? GetWhereClauseForCount(request) : "");

            int totalRecords = Convert.ToInt32(_db.ExecuteScalar(countQuery, parameters.ToArray()));

            // Add pagination
            int offset = (request.PageNumber - 1) * request.PageSize;
            queryBuilder.Append($" OFFSET {offset} ROWS FETCH NEXT {request.PageSize} ROWS ONLY");

            var dt = _db.Execute(queryBuilder.ToString(), parameters.ToArray());

            var caterings = new List<AdminCateringListItem>();
            foreach (DataRow row in dt.Rows)
            {
                caterings.Add(new AdminCateringListItem
                {
                    CateringId = Convert.ToInt64(row["CateringId"]),
                    BusinessName = row["BusinessName"]?.ToString() ?? string.Empty,
                    OwnerName = row["OwnerName"]?.ToString() ?? string.Empty,
                    Phone = row["Phone"]?.ToString() ?? string.Empty,
                    Email = row["Email"]?.ToString() ?? string.Empty,
                    City = row["City"]?.ToString() ?? string.Empty,
                    State = row["State"]?.ToString() ?? string.Empty,
                    Status = row["Status"]?.ToString() ?? string.Empty,
                    IsVerified = Convert.ToBoolean(row["IsVerified"]),
                    Rating = row["Rating"] != DBNull.Value ? Convert.ToDecimal(row["Rating"]) : null,
                    TotalReviews = Convert.ToInt32(row["TotalReviews"]),
                    TotalOrders = Convert.ToInt32(row["TotalOrders"]),
                    TotalEarnings = Convert.ToDecimal(row["TotalEarnings"]),
                    CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                    ApprovedDate = row["ApprovedDate"] != DBNull.Value ? Convert.ToDateTime(row["ApprovedDate"]) : null
                });
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
                    co.c_catering_ownerid, co.c_business_name, co.c_name, co.c_mobile, co.c_email,
                    co.c_catering_number, co.c_gst_number, co.c_fssai_license, co.c_pan_number,
                    addr.c_address_line1, addr.c_address_line2, c.c_cityname, s.c_statename, addr.c_pincode,
                    co.c_status, co.c_isverified, co.c_isactive, co.c_isblocked, co.c_block_reason,
                    bank.c_bank_name, bank.c_account_number, bank.c_ifsc_code, bank.c_account_holder_name,
                    co.c_created_date, co.c_approved_date, co.c_last_modified
                FROM {Table.SysCateringOwner} co
                LEFT JOIN {Table.SysCateringOwnerAddress} addr ON co.c_catering_ownerid = addr.c_catering_ownerid
                LEFT JOIN {Table.City} c ON addr.c_cityid = c.c_cityid
                LEFT JOIN {Table.State} s ON addr.c_stateid = s.c_stateid
                LEFT JOIN {Table.SysCateringOwnerBankDetails} bank ON co.c_catering_ownerid = bank.c_catering_ownerid
                WHERE co.c_catering_ownerid = @CateringId";

            SqlParameter[] parameters = { new SqlParameter("@CateringId", cateringId) };
            var dt = _db.Execute(query, parameters);

            if (dt.Rows.Count == 0) return null;

            var row = dt.Rows[0];

            // Get stats
            var stats = GetCateringStats(cateringId);

            // Get images
            var images = GetCateringImages(cateringId);

            return new AdminCateringDetail
            {
                CateringId = Convert.ToInt64(row["c_catering_ownerid"]),
                BusinessName = row["c_business_name"]?.ToString() ?? string.Empty,
                OwnerName = row["c_name"]?.ToString() ?? string.Empty,
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
                Status = row["c_status"]?.ToString() ?? string.Empty,
                IsVerified = row["c_isverified"] != DBNull.Value && Convert.ToBoolean(row["c_isverified"]),
                IsActive = row["c_isactive"] != DBNull.Value && Convert.ToBoolean(row["c_isactive"]),
                IsBlocked = row["c_isblocked"] != DBNull.Value && Convert.ToBoolean(row["c_isblocked"]),
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
                CreatedDate = Convert.ToDateTime(row["c_created_date"]),
                ApprovedDate = row["c_approved_date"] != DBNull.Value ? Convert.ToDateTime(row["c_approved_date"]) : null,
                LastModified = row["c_last_modified"] != DBNull.Value ? Convert.ToDateTime(row["c_last_modified"]) : null
            };
        }

        public bool UpdateCateringStatus(AdminCateringStatusUpdate request)
        {
            string query = $@"
                UPDATE {Table.SysCateringOwner}
                SET c_status = @Status,
                    c_isactive = CASE WHEN @Status = 'Approved' THEN 1 ELSE c_isactive END,
                    c_isblocked = CASE WHEN @Status = 'Blocked' THEN 1 ELSE 0 END,
                    c_block_reason = @Reason,
                    c_approved_date = CASE WHEN @Status = 'Approved' AND c_approved_date IS NULL THEN GETDATE() ELSE c_approved_date END,
                    c_last_modified = GETDATE()
                WHERE c_catering_ownerid = @CateringId";

            SqlParameter[] parameters = {
                new SqlParameter("@CateringId", request.CateringId),
                new SqlParameter("@Status", request.Status),
                new SqlParameter("@Reason", (object?)request.Reason ?? DBNull.Value)
            };

            int rowsAffected = _db.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        public bool DeleteCatering(long cateringId, long deletedBy)
        {
            string query = $@"
                UPDATE {Table.SysCateringOwner}
                SET c_isdeleted = 1,
                    c_deleted_by = @DeletedBy,
                    c_deleted_date = GETDATE()
                WHERE c_catering_ownerid = @CateringId";

            SqlParameter[] parameters = {
                new SqlParameter("@CateringId", cateringId),
                new SqlParameter("@DeletedBy", deletedBy)
            };

            int rowsAffected = _db.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        private (decimal? AverageRating, int TotalReviews, int TotalOrders, decimal TotalEarnings, decimal PlatformCommission) GetCateringStats(long cateringId)
        {
            string query = $@"
                SELECT
                    ISNULL(AVG(CAST(cr.c_rating AS DECIMAL(3,2))), 0) AS AvgRating,
                    COUNT(DISTINCT cr.c_reviewid) AS TotalReviews,
                    COUNT(DISTINCT o.c_orderid) AS TotalOrders,
                    ISNULL(SUM(o.c_total_amount), 0) AS TotalEarnings,
                    ISNULL(SUM(o.c_platform_commission), 0) AS Commission
                FROM {Table.SysCateringOwner} co
                LEFT JOIN {Table.SysCateringReview} cr ON co.c_catering_ownerid = cr.c_catering_ownerid
                LEFT JOIN {Table.SysOrders} o ON co.c_catering_ownerid = o.c_catering_ownerid AND o.c_status = 'Completed'
                WHERE co.c_catering_ownerid = @CateringId";

            SqlParameter[] parameters = { new SqlParameter("@CateringId", cateringId) };
            var dt = _db.Execute(query, parameters);

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
                WHERE c_catering_ownerid = @CateringId
                ORDER BY c_created_date DESC";

            SqlParameter[] parameters = { new SqlParameter("@CateringId", cateringId) };
            var dt = _db.Execute(query, parameters);

            var images = new List<string>();
            foreach (DataRow row in dt.Rows)
            {
                images.Add(row["c_image_url"]?.ToString() ?? string.Empty);
            }

            return images;
        }

        private string GetWhereClauseForCount(AdminCateringListRequest request)
        {
            var whereBuilder = new StringBuilder();

            if (!string.IsNullOrEmpty(request.SearchTerm))
                whereBuilder.Append(" AND (co.c_business_name LIKE @SearchTerm OR co.c_name LIKE @SearchTerm OR co.c_mobile LIKE @SearchTerm)");

            if (request.CityId.HasValue)
                whereBuilder.Append(" AND co.c_cityid = @CityId");

            if (!string.IsNullOrEmpty(request.Status))
                whereBuilder.Append(" AND co.c_status = @Status");

            if (!string.IsNullOrEmpty(request.VerificationStatus))
            {
                bool isVerified = request.VerificationStatus.Equals("Verified", StringComparison.OrdinalIgnoreCase);
                whereBuilder.Append(" AND co.c_isverified = " + (isVerified ? "1" : "0"));
            }

            return whereBuilder.ToString();
        }
    }
}
