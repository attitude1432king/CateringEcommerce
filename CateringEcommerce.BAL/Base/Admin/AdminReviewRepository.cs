using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Npgsql;
using System.Data;
using System.Linq;
using System.Text;

namespace CateringEcommerce.BAL.Base.Admin
{
    public class AdminReviewRepository : IAdminReviewRepository
    {
        private readonly IDatabaseHelper _dbHelper;
        public AdminReviewRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public AdminReviewListResponse GetAllReviews(AdminReviewListRequest request)
        {
            var queryBuilder = new StringBuilder($@"
                SELECT
                    r.c_reviewid AS ReviewId,
                    r.c_ownerid AS CateringId,
                    co.c_catering_name AS CateringName,
                    r.c_userid AS UserId,
                    u.c_name AS UserName,
                    r.c_overall_rating AS Rating,
                    r.c_review_comment AS Comment,
                    COALESCE(r.c_ishidden, FALSE) AS IsHidden,
                    r.c_hidden_reason AS HiddenReason,
                    r.c_createddate AS ReviewDate,
                    r.c_orderid AS OrderId
                FROM {Table.SysCateringReview} r
                JOIN {Table.SysCateringOwner} co ON r.c_ownerid = co.c_ownerid
                JOIN {Table.SysUser} u ON r.c_userid = u.c_userid
                WHERE 1=1");

            var parameters = new List<NpgsqlParameter>();

            if (request.CateringId.HasValue)
            {
                queryBuilder.Append(" AND r.c_ownerid = @CateringId");
                parameters.Add(new NpgsqlParameter("@CateringId", request.CateringId.Value));
            }

            if (request.UserId.HasValue)
            {
                queryBuilder.Append(" AND r.c_userid = @UserId");
                parameters.Add(new NpgsqlParameter("@UserId", request.UserId.Value));
            }

            if (request.MinRating.HasValue)
            {
                queryBuilder.Append(" AND r.c_overall_rating >= @MinRating");
                parameters.Add(new NpgsqlParameter("@MinRating", request.MinRating.Value));
            }

            if (request.MaxRating.HasValue)
            {
                queryBuilder.Append(" AND r.c_overall_rating <= @MaxRating");
                parameters.Add(new NpgsqlParameter("@MaxRating", request.MaxRating.Value));
            }

            if (request.IsHidden.HasValue)
            {
                queryBuilder.Append(" AND COALESCE(r.c_ishidden, FALSE) = @IsHidden");
                parameters.Add(new NpgsqlParameter("@IsHidden", request.IsHidden.Value));
            }

            string sortColumn = request.SortBy switch
            {
                "Rating" => "r.c_overall_rating",
                "CateringName" => "co.c_catering_name",
                _ => "r.c_createddate"
            };

            queryBuilder.Append($" ORDER BY {sortColumn} {request.SortOrder}");

            string countQuery = $@"
                SELECT COUNT(*)
                FROM {Table.SysCateringReview} r
                WHERE 1=1" + GetWhereClauseForCount(request);

            int totalRecords = Convert.ToInt32(_dbHelper.ExecuteScalar(countQuery, CloneParameters(parameters)));

            int offset = (request.PageNumber - 1) * request.PageSize;
            queryBuilder.Append($" LIMIT {request.PageSize} OFFSET {offset}");

            var dt = _dbHelper.Execute(queryBuilder.ToString(), CloneParameters(parameters));
            if (dt == null)
            {
                return new AdminReviewListResponse
                {
                    Reviews = new List<AdminReviewListItem>(),
                    TotalRecords = totalRecords,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize)
                };
            }

            var reviews = new List<AdminReviewListItem>();
            foreach (DataRow row in dt.Rows)
            {
                reviews.Add(new AdminReviewListItem
                {
                    ReviewId = Convert.ToInt64(row["ReviewId"]),
                    CateringId = Convert.ToInt64(row["CateringId"]),
                    CateringName = row["CateringName"]?.ToString() ?? string.Empty,
                    UserId = Convert.ToInt64(row["UserId"]),
                    UserName = row["UserName"]?.ToString() ?? string.Empty,
                    Rating = Convert.ToInt32(row["Rating"]),
                    Comment = row["Comment"]?.ToString(),
                    IsHidden = Convert.ToBoolean(row["IsHidden"]),
                    HiddenReason = row["HiddenReason"]?.ToString(),
                    ReviewDate = Convert.ToDateTime(row["ReviewDate"]),
                    OrderId = row["OrderId"] != DBNull.Value ? Convert.ToInt64(row["OrderId"]) : null
                });
            }

            return new AdminReviewListResponse
            {
                Reviews = reviews,
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize)
            };
        }

        public AdminReviewDetail? GetReviewById(long reviewId)
        {
            string query = $@"
                SELECT
                    r.c_reviewid, r.c_ownerid, co.c_catering_name AS CateringName,
                    r.c_userid, u.c_name AS UserName, u.c_mobile AS UserPhone,
                    r.c_overall_rating, r.c_review_comment,
                    COALESCE(r.c_ishidden, FALSE) AS IsHidden,
                    r.c_hidden_reason, r.c_hidden_by, r.c_hidden_date,
                    r.c_createddate, r.c_orderid
                FROM {Table.SysCateringReview} r
                JOIN {Table.SysCateringOwner} co ON r.c_ownerid = co.c_ownerid
                JOIN {Table.SysUser} u ON r.c_userid = u.c_userid
                WHERE r.c_reviewid = @ReviewId";

            NpgsqlParameter[] parameters = { new NpgsqlParameter("@ReviewId", reviewId) };
            var dt = _dbHelper.Execute(query, parameters);

            if (dt == null || dt.Rows.Count == 0) return null;

            var row = dt.Rows[0];

            return new AdminReviewDetail
            {
                ReviewId = Convert.ToInt64(row["c_reviewid"]),
                CateringId = Convert.ToInt64(row["c_ownerid"]),
                CateringName = row["CateringName"]?.ToString() ?? string.Empty,
                UserId = Convert.ToInt64(row["c_userid"]),
                UserName = row["UserName"]?.ToString() ?? string.Empty,
                UserPhone = row["UserPhone"]?.ToString() ?? string.Empty,
                Rating = Convert.ToInt32(row["c_overall_rating"]),
                Comment = row["c_review_comment"]?.ToString(),
                IsHidden = Convert.ToBoolean(row["IsHidden"]),
                HiddenReason = row["c_hidden_reason"]?.ToString(),
                HiddenBy = row["c_hidden_by"] != DBNull.Value ? Convert.ToInt64(row["c_hidden_by"]) : null,
                HiddenDate = row["c_hidden_date"] != DBNull.Value ? Convert.ToDateTime(row["c_hidden_date"]) : null,
                ReviewDate = Convert.ToDateTime(row["c_createddate"]),
                OrderId = row["c_orderid"] != DBNull.Value ? Convert.ToInt64(row["c_orderid"]) : null,
                ReviewImages = new List<string>()
            };
        }

        public bool UpdateReviewVisibility(AdminReviewHideRequest request)
        {
            string query = $@"
                UPDATE {Table.SysCateringReview}
                SET c_ishidden = @IsHidden,
                    c_hidden_reason = @Reason,
                    c_hidden_by = @UpdatedBy,
                    c_hidden_date = CASE WHEN @IsHidden THEN NOW() ELSE NULL END
                WHERE c_reviewid = @ReviewId";

            NpgsqlParameter[] parameters = {
                new NpgsqlParameter("@ReviewId", request.ReviewId),
                new NpgsqlParameter("@IsHidden", request.IsHidden),
                new NpgsqlParameter("@Reason", (object?)request.Reason ?? DBNull.Value),
                new NpgsqlParameter("@UpdatedBy", request.UpdatedBy)
            };

            int rowsAffected = _dbHelper.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        public bool DeleteReview(long reviewId, long deletedBy)
        {
            string query = $@"
                DELETE FROM {Table.SysCateringReview}
                WHERE c_reviewid = @ReviewId";

            NpgsqlParameter[] parameters = {
                new NpgsqlParameter("@ReviewId", reviewId)
            };

            int rowsAffected = _dbHelper.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        private string GetWhereClauseForCount(AdminReviewListRequest request)
        {
            var whereBuilder = new StringBuilder();

            if (request.CateringId.HasValue)
                whereBuilder.Append(" AND r.c_ownerid = @CateringId");

            if (request.UserId.HasValue)
                whereBuilder.Append(" AND r.c_userid = @UserId");

            if (request.MinRating.HasValue)
                whereBuilder.Append(" AND r.c_overall_rating >= @MinRating");

            if (request.MaxRating.HasValue)
                whereBuilder.Append(" AND r.c_overall_rating <= @MaxRating");

            if (request.IsHidden.HasValue)
                whereBuilder.Append($" AND COALESCE(r.c_ishidden, FALSE) = {(request.IsHidden.Value ? "TRUE" : "FALSE")}");

            return whereBuilder.ToString();
        }

        private static NpgsqlParameter[] CloneParameters(IEnumerable<NpgsqlParameter> source)
        {
            return source
                .Select(p => new NpgsqlParameter
                {
                    ParameterName = p.ParameterName,
                    NpgsqlDbType = p.NpgsqlDbType,
                    Size = p.Size,
                    Precision = p.Precision,
                    Scale = p.Scale,
                    Direction = p.Direction,
                    IsNullable = p.IsNullable,
                    Value = p.Value
                })
                .ToArray();
        }
    }
}

