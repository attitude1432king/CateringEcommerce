using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CateringEcommerce.BAL.Common.Admin
{
    public class AdminEarningsRepository : IAdminEarningsRepository
    {
        private readonly SqlDatabaseManager _db;

        public AdminEarningsRepository(string connectionString)
        {
            _db = new SqlDatabaseManager();
            _db.SetConnectionString(connectionString);
        }

        public AdminEarningsSummary GetEarningsSummary()
        {
            string query = $@"
                SELECT
                    ISNULL(SUM(CASE WHEN c_status = 'Completed' THEN c_platform_commission ELSE 0 END), 0) AS TotalCommission,
                    ISNULL(SUM(CASE WHEN c_status = 'Completed' THEN c_total_amount ELSE 0 END), 0) AS TotalOrderValue,
                    COUNT(CASE WHEN c_status = 'Completed' THEN 1 END) AS CompletedOrders,
                    COUNT(*) AS TotalOrders,
                    ISNULL(SUM(CASE WHEN c_status = 'Completed' AND MONTH(c_order_date) = MONTH(GETDATE()) AND YEAR(c_order_date) = YEAR(GETDATE()) THEN c_platform_commission ELSE 0 END), 0) AS ThisMonthEarnings,
                    ISNULL(SUM(CASE WHEN c_status = 'Completed' AND MONTH(c_order_date) = MONTH(DATEADD(MONTH, -1, GETDATE())) AND YEAR(c_order_date) = YEAR(DATEADD(MONTH, -1, GETDATE())) THEN c_platform_commission ELSE 0 END), 0) AS LastMonthEarnings
                FROM {Table.SysOrders}";

            var dt = _db.Execute(query);

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
                "Week" => "DATEPART(YEAR, c_order_date), DATEPART(WEEK, c_order_date)",
                "Month" => "DATEPART(YEAR, c_order_date), DATEPART(MONTH, c_order_date)",
                "Year" => "DATEPART(YEAR, c_order_date)",
                _ => "CAST(c_order_date AS DATE)"
            };

            string periodStart = request.GroupBy switch
            {
                "Week" => "DATEADD(WEEK, DATEDIFF(WEEK, 0, c_order_date), 0)",
                "Month" => "DATEADD(MONTH, DATEDIFF(MONTH, 0, c_order_date), 0)",
                "Year" => "DATEADD(YEAR, DATEDIFF(YEAR, 0, c_order_date), 0)",
                _ => "CAST(c_order_date AS DATE)"
            };

            string periodEnd = request.GroupBy switch
            {
                "Week" => "DATEADD(DAY, 6, DATEADD(WEEK, DATEDIFF(WEEK, 0, c_order_date), 0))",
                "Month" => "EOMONTH(c_order_date)",
                "Year" => "DATEADD(YEAR, 1, DATEADD(YEAR, DATEDIFF(YEAR, 0, c_order_date), 0)) - 1",
                _ => "CAST(c_order_date AS DATE)"
            };

            var query = $@"
                SELECT
                    {periodStart} AS PeriodStart,
                    {periodEnd} AS PeriodEnd,
                    ISNULL(SUM(c_total_amount), 0) AS TotalOrderValue,
                    ISNULL(SUM(c_platform_commission), 0) AS PlatformCommission,
                    COUNT(*) AS OrderCount
                FROM {Table.SysOrders}
                WHERE c_status = 'Completed'";

            var parameters = new List<SqlParameter>();

            if (request.StartDate.HasValue)
            {
                query += " AND c_order_date >= @StartDate";
                parameters.Add(new SqlParameter("@StartDate", request.StartDate.Value));
            }

            if (request.EndDate.HasValue)
            {
                query += " AND c_order_date <= @EndDate";
                parameters.Add(new SqlParameter("@EndDate", request.EndDate.Value));
            }

            query += $" GROUP BY {dateFormat}, {periodStart}, {periodEnd} ORDER BY PeriodStart DESC";

            var dt = _db.Execute(query, parameters.ToArray());

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
                    co.c_catering_ownerid AS CateringId,
                    co.c_business_name AS BusinessName,
                    c.c_cityname AS City,
                    ISNULL(SUM(o.c_total_amount), 0) AS TotalOrderValue,
                    ISNULL(SUM(o.c_platform_commission), 0) AS PlatformCommission,
                    ISNULL(AVG(o.c_commission_rate), 0) AS CommissionRate,
                    COUNT(o.c_orderid) AS TotalOrders,
                    COUNT(CASE WHEN o.c_status = 'Completed' THEN 1 END) AS CompletedOrders,
                    CASE WHEN COUNT(o.c_orderid) > 0 THEN ISNULL(SUM(o.c_total_amount), 0) / COUNT(o.c_orderid) ELSE 0 END AS AvgOrderValue
                FROM {Table.SysCateringOwner} co
                LEFT JOIN {Table.SysOrders} o ON co.c_catering_ownerid = o.c_catering_ownerid AND o.c_status = 'Completed'
                LEFT JOIN {Table.City} c ON co.c_cityid = c.c_cityid
                WHERE 1=1";

            var parameters = new List<SqlParameter>();

            if (request.StartDate.HasValue)
            {
                query += " AND o.c_order_date >= @StartDate";
                parameters.Add(new SqlParameter("@StartDate", request.StartDate.Value));
            }

            if (request.EndDate.HasValue)
            {
                query += " AND o.c_order_date <= @EndDate";
                parameters.Add(new SqlParameter("@EndDate", request.EndDate.Value));
            }

            query += " GROUP BY co.c_catering_ownerid, co.c_business_name, c.c_cityname";

            string sortColumn = request.SortBy switch
            {
                "BusinessName" => "co.c_business_name",
                "TotalOrders" => "TotalOrders",
                _ => "PlatformCommission"
            };

            query += $" ORDER BY {sortColumn} {request.SortOrder}";

            string countQuery = $@"
                SELECT COUNT(DISTINCT co.c_catering_ownerid)
                FROM {Table.SysCateringOwner} co
                LEFT JOIN {Table.SysOrders} o ON co.c_catering_ownerid = o.c_catering_ownerid AND o.c_status = 'Completed'";

            int totalRecords = Convert.ToInt32(_db.ExecuteScalar(countQuery));

            int offset = (request.PageNumber - 1) * request.PageSize;
            query += $" OFFSET {offset} ROWS FETCH NEXT {request.PageSize} ROWS ONLY";

            var dt = _db.Execute(query, parameters.ToArray());

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
                    YEAR(o.c_order_date) AS Year,
                    MONTH(o.c_order_date) AS Month,
                    DATENAME(MONTH, o.c_order_date) AS MonthName,
                    ISNULL(SUM(o.c_total_amount), 0) AS TotalOrderValue,
                    ISNULL(SUM(o.c_platform_commission), 0) AS PlatformCommission,
                    COUNT(o.c_orderid) AS OrderCount,
                    (SELECT COUNT(*) FROM {Table.SysCateringOwner} WHERE YEAR(c_created_date) = YEAR(o.c_order_date) AND MONTH(c_created_date) = MONTH(o.c_order_date)) AS NewCaterings,
                    (SELECT COUNT(*) FROM {Table.SysUser} WHERE YEAR(c_created_date) = YEAR(o.c_order_date) AND MONTH(c_created_date) = MONTH(o.c_order_date)) AS NewUsers
                FROM {Table.SysOrders} o
                WHERE YEAR(o.c_order_date) = @Year AND o.c_status = 'Completed'
                GROUP BY YEAR(o.c_order_date), MONTH(o.c_order_date), DATENAME(MONTH, o.c_order_date)
                ORDER BY Month";

            SqlParameter[] parameters = { new SqlParameter("@Year", year) };
            var dt = _db.Execute(query, parameters);

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
