using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Npgsql;
using System.Data;

namespace CateringEcommerce.BAL.Base.Admin
{
    public class AdminEarningsRepository : IAdminEarningsRepository
    {
        private readonly IDatabaseHelper _dbHelper;
        public AdminEarningsRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public AdminEarningsSummary GetEarningsSummary()
        {
            string query = $@"
                SELECT
                    COALESCE(SUM(CASE WHEN c_order_status = 'Completed' THEN c_platform_commission ELSE 0 END), 0) AS TotalCommission,
                    COALESCE(SUM(CASE WHEN c_order_status = 'Completed' THEN c_total_amount ELSE 0 END), 0) AS TotalOrderValue,
                    COUNT(CASE WHEN c_order_status = 'Completed' THEN 1 END) AS CompletedOrders,
                    COUNT(*) AS TotalOrders,
                    COALESCE(SUM(CASE WHEN c_order_status = 'Completed' AND EXTRACT(MONTH FROM c_createddate) = EXTRACT(MONTH FROM NOW()) AND EXTRACT(YEAR FROM c_createddate) = EXTRACT(YEAR FROM NOW()) THEN c_platform_commission ELSE 0 END), 0) AS ThisMonthEarnings,
                    COALESCE(SUM(CASE WHEN c_order_status = 'Completed' AND EXTRACT(MONTH FROM c_createddate) = EXTRACT(MONTH FROM NOW() - INTERVAL '1 month') AND EXTRACT(YEAR FROM c_createddate) = EXTRACT(YEAR FROM NOW() - INTERVAL '1 month') THEN c_platform_commission ELSE 0 END), 0) AS LastMonthEarnings
                FROM {Table.SysOrders}";

            var dt = _dbHelper.Execute(query);

            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                var totalCommission = Convert.ToDecimal(row["TotalCommission"]);
                var totalOrderValue = Convert.ToDecimal(row["TotalOrderValue"]);
                var completedOrders = Convert.ToInt32(row["CompletedOrders"]);
                var totalOrders = Convert.ToInt32(row["TotalOrders"]);
                var thisMonthEarnings = Convert.ToDecimal(row["ThisMonthEarnings"]);
                var lastMonthEarnings = Convert.ToDecimal(row["LastMonthEarnings"]);

                var avgCommissionRate = totalOrderValue > 0 ? (totalCommission / totalOrderValue) * 100 : 0;
                var growthPercentage = lastMonthEarnings > 0
                    ? ((thisMonthEarnings - lastMonthEarnings) / lastMonthEarnings) * 100
                    : 0;

                return new AdminEarningsSummary
                {
                    TotalPlatformEarnings = totalCommission,
                    TotalOrderValue = totalOrderValue,
                    TotalCommission = totalCommission,
                    AverageCommissionRate = avgCommissionRate,
                    TotalOrders = totalOrders,
                    CompletedOrders = completedOrders,
                    ThisMonthEarnings = thisMonthEarnings,
                    LastMonthEarnings = lastMonthEarnings,
                    GrowthPercentage = growthPercentage
                };
            }

            return new AdminEarningsSummary();
        }

        public List<AdminEarningsByDateItem> GetEarningsByDate(AdminEarningsByDateRequest request)
        {
            string dateFormat = request.GroupBy switch
            {
                "Week" => "EXTRACT(YEAR FROM c_createddate), EXTRACT(WEEK FROM c_createddate)",
                "Month" => "EXTRACT(YEAR FROM c_createddate), EXTRACT(MONTH FROM c_createddate)",
                "Year" => "EXTRACT(YEAR FROM c_createddate)",
                _ => "CAST(c_createddate AS DATE)"
            };

            string periodStart = request.GroupBy switch
            {
                "Week" => "DATE_TRUNC('week', c_createddate)::date",
                "Month" => "DATE_TRUNC('month', c_createddate)::date",
                "Year" => "DATE_TRUNC('year', c_createddate)::date",
                _ => "CAST(c_createddate AS DATE)"
            };

            string periodEnd = request.GroupBy switch
            {
                "Week" => "(DATE_TRUNC('week', c_createddate)::date + INTERVAL '6 days')::date",
                "Month" => "(DATE_TRUNC('month', c_createddate)::date + INTERVAL '1 month - 1 day')::date",
                "Year" => "(DATE_TRUNC('year', c_createddate)::date + INTERVAL '1 year - 1 day')::date",
                _ => "CAST(c_createddate AS DATE)"
            };

            var query = $@"
                SELECT
                    {periodStart} AS PeriodStart,
                    {periodEnd} AS PeriodEnd,
                    COALESCE(SUM(c_total_amount), 0) AS TotalOrderValue,
                    COALESCE(SUM(c_platform_commission), 0) AS PlatformCommission,
                    COUNT(*) AS OrderCount
                FROM {Table.SysOrders}
                WHERE c_order_status = 'Completed'";

            var parameters = new List<NpgsqlParameter>();

            if (request.StartDate.HasValue)
            {
                query += " AND c_createddate >= @StartDate";
                parameters.Add(new NpgsqlParameter("@StartDate", request.StartDate.Value));
            }

            if (request.EndDate.HasValue)
            {
                query += " AND c_createddate <= @EndDate";
                parameters.Add(new NpgsqlParameter("@EndDate", request.EndDate.Value));
            }

            query += $" GROUP BY {dateFormat}, {periodStart}, {periodEnd} ORDER BY PeriodStart DESC";

            var dt = _dbHelper.Execute(query, parameters.ToArray());

            var result = new List<AdminEarningsByDateItem>();
            foreach (DataRow row in dt.Rows)
            {
                var periodStartDate = Convert.ToDateTime(row["PeriodStart"]);
                var periodEndDate = Convert.ToDateTime(row["PeriodEnd"]);

                var period = request.GroupBy switch
                {
                    "Week" => $"Week {periodStartDate:MMM dd} - {periodEndDate:MMM dd, yyyy}",
                    "Month" => periodStartDate.ToString("MMM yyyy"),
                    "Year" => periodStartDate.ToString("yyyy"),
                    _ => periodStartDate.ToString("MMM dd, yyyy")
                };

                result.Add(new AdminEarningsByDateItem
                {
                    Period = period,
                    TotalOrderValue = Convert.ToDecimal(row["TotalOrderValue"]),
                    PlatformCommission = Convert.ToDecimal(row["PlatformCommission"]),
                    OrderCount = Convert.ToInt32(row["OrderCount"]),
                    PeriodStart = periodStartDate,
                    PeriodEnd = periodEndDate
                });
            }

            return result;
        }

        public AdminEarningsByCateringResponse GetEarningsByCatering(AdminEarningsByCateringRequest request)
        {
            var query = $@"
                SELECT
                    co.c_ownerid AS CateringId,
                    co.c_catering_name AS BusinessName,
                    COALESCE(loc.c_cityname, '') AS City,
                    COALESCE(SUM(o.c_total_amount), 0) AS TotalOrderValue,
                    COALESCE(SUM(o.c_platform_commission), 0) AS PlatformCommission,
                    COALESCE(AVG(o.c_commission_rate), 0) AS CommissionRate,
                    COUNT(o.c_orderid) AS TotalOrders,
                    COUNT(CASE WHEN o.c_order_status = 'Completed' THEN 1 END) AS CompletedOrders,
                    CASE WHEN COUNT(o.c_orderid) > 0 THEN COALESCE(SUM(o.c_total_amount), 0) / COUNT(o.c_orderid) ELSE 0 END AS AvgOrderValue
                FROM {Table.SysCateringOwner} co
                LEFT JOIN {Table.SysOrders} o ON co.c_ownerid = o.c_ownerid AND o.c_order_status = 'Completed'
                LEFT JOIN LATERAL (
                    SELECT c.c_cityname
                    FROM {Table.SysCateringOwnerAddress} coa
                    LEFT JOIN {Table.City} c ON coa.c_cityid = c.c_cityid
                    WHERE coa.c_ownerid = co.c_ownerid
                    ORDER BY coa.c_addressid DESC
                    LIMIT 1
                ) loc ON TRUE
                WHERE 1=1";

            var parameters = new List<NpgsqlParameter>();

            if (request.StartDate.HasValue)
            {
                query += " AND o.c_createddate >= @StartDate";
                parameters.Add(new NpgsqlParameter("@StartDate", request.StartDate.Value));
            }

            if (request.EndDate.HasValue)
            {
                query += " AND o.c_createddate <= @EndDate";
                parameters.Add(new NpgsqlParameter("@EndDate", request.EndDate.Value));
            }

            query += " GROUP BY co.c_ownerid, co.c_catering_name, loc.c_cityname";

            string sortColumn = request.SortBy switch
            {
                "BusinessName" => "co.c_catering_name",
                "TotalOrders" => "TotalOrders",
                _ => "PlatformCommission"
            };

            query += $" ORDER BY {sortColumn} {request.SortOrder}";

            string countQuery = $@"
                SELECT COUNT(DISTINCT co.c_ownerid)
                FROM {Table.SysCateringOwner} co
                LEFT JOIN {Table.SysOrders} o ON co.c_ownerid = o.c_ownerid AND o.c_order_status = 'Completed'";

            int totalRecords = Convert.ToInt32(_dbHelper.ExecuteScalar(countQuery));

            int offset = (request.PageNumber - 1) * request.PageSize;
            query += $" LIMIT {request.PageSize} OFFSET {offset}";

            var dt = _dbHelper.Execute(query, parameters.ToArray());

            var caterings = new List<AdminEarningsByCateringItem>();
            decimal grandTotalCommission = 0;

            foreach (DataRow row in dt.Rows)
            {
                var commission = Convert.ToDecimal(row["PlatformCommission"]);
                grandTotalCommission += commission;

                caterings.Add(new AdminEarningsByCateringItem
                {
                    CateringId = Convert.ToInt64(row["CateringId"]),
                    BusinessName = row["BusinessName"]?.ToString() ?? string.Empty,
                    City = row["City"]?.ToString() ?? string.Empty,
                    TotalOrderValue = Convert.ToDecimal(row["TotalOrderValue"]),
                    PlatformCommission = commission,
                    CommissionRate = Convert.ToDecimal(row["CommissionRate"]),
                    TotalOrders = Convert.ToInt32(row["TotalOrders"]),
                    CompletedOrders = Convert.ToInt32(row["CompletedOrders"]),
                    AverageOrderValue = Convert.ToDecimal(row["AvgOrderValue"])
                });
            }

            return new AdminEarningsByCateringResponse
            {
                Caterings = caterings,
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize),
                GrandTotalCommission = grandTotalCommission
            };
        }

        public List<AdminMonthlyReportItem> GetMonthlyReport(int year)
        {
            string query = $@"
                SELECT
                    EXTRACT(YEAR FROM o.c_createddate) AS Year,
                    EXTRACT(MONTH FROM o.c_createddate) AS Month,
                    TO_CHAR(o.c_createddate, 'Month') AS MonthName,
                    COALESCE(SUM(o.c_total_amount), 0) AS TotalOrderValue,
                    COALESCE(SUM(o.c_platform_commission), 0) AS PlatformCommission,
                    COUNT(o.c_orderid) AS OrderCount,
                    (SELECT COUNT(*) FROM {Table.SysCateringOwner} WHERE EXTRACT(YEAR FROM c_createddate) = EXTRACT(YEAR FROM o.c_createddate) AND EXTRACT(MONTH FROM c_createddate) = EXTRACT(MONTH FROM o.c_createddate)) AS NewCaterings,
                    (SELECT COUNT(*) FROM {Table.SysUser} WHERE EXTRACT(YEAR FROM c_createddate) = EXTRACT(YEAR FROM o.c_createddate) AND EXTRACT(MONTH FROM c_createddate) = EXTRACT(MONTH FROM o.c_createddate)) AS NewUsers
                FROM {Table.SysOrders} o
                WHERE EXTRACT(YEAR FROM o.c_createddate) = @Year AND o.c_order_status = 'Completed'
                GROUP BY EXTRACT(YEAR FROM o.c_createddate), EXTRACT(MONTH FROM o.c_createddate), TO_CHAR(o.c_createddate, 'Month')
                ORDER BY Month";

            NpgsqlParameter[] parameters = { new NpgsqlParameter("@Year", year) };
            var dt = _dbHelper.Execute(query, parameters);

            var result = new List<AdminMonthlyReportItem>();
            foreach (DataRow row in dt.Rows)
            {
                result.Add(new AdminMonthlyReportItem
                {
                    Year = Convert.ToInt32(row["Year"]),
                    Month = Convert.ToInt32(row["Month"]),
                    MonthName = row["MonthName"]?.ToString() ?? string.Empty,
                    TotalOrderValue = Convert.ToDecimal(row["TotalOrderValue"]),
                    PlatformCommission = Convert.ToDecimal(row["PlatformCommission"]),
                    OrderCount = Convert.ToInt32(row["OrderCount"]),
                    NewCaterings = Convert.ToInt32(row["NewCaterings"]),
                    NewUsers = Convert.ToInt32(row["NewUsers"])
                });
            }

            return result;
        }
    }
}

