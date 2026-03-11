using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CateringEcommerce.BAL.Base.Admin
{
    public class AdminDashboardRepository : IAdminDashboardRepository
    {
        private readonly IDatabaseHelper _dbHelper;
        public AdminDashboardRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public AdminDashboardMetrics GetDashboardMetrics()
        {
            var metrics = new AdminDashboardMetrics();

            // User metrics
            var userMetrics = GetUserMetrics();
            metrics.TotalUsers = userMetrics.Total;
            metrics.ActiveUsers = userMetrics.Active;
            metrics.NewUsersToday = userMetrics.Today;
            metrics.NewUsersThisMonth = userMetrics.ThisMonth;

            // Catering metrics
            var cateringMetrics = GetCateringMetrics();
            metrics.TotalCaterings = cateringMetrics.Total;
            metrics.ActiveCaterings = cateringMetrics.Active;
            metrics.PendingApprovals = cateringMetrics.Pending;
            metrics.NewCateringsToday = cateringMetrics.Today;
            metrics.NewCateringsThisMonth = cateringMetrics.ThisMonth;

            // Order metrics
            var orderMetrics = GetOrderMetrics();
            metrics.TotalOrders = orderMetrics.Total;
            metrics.PendingOrders = orderMetrics.Pending;
            metrics.CompletedOrders = orderMetrics.Completed;
            metrics.CancelledOrders = orderMetrics.Cancelled;
            metrics.TodayOrders = orderMetrics.Today;
            metrics.ThisMonthOrders = orderMetrics.ThisMonth;

            // Revenue metrics
            var revenueMetrics = GetRevenueMetrics();
            metrics.TotalRevenue = revenueMetrics.Total;
            metrics.TodayRevenue = revenueMetrics.Today;
            metrics.ThisMonthRevenue = revenueMetrics.ThisMonth;
            metrics.TotalCommission = revenueMetrics.Commission;
            metrics.AverageOrderValue = revenueMetrics.AvgOrderValue;

            // Review metrics
            var reviewMetrics = GetReviewMetrics();
            metrics.AverageRating = reviewMetrics.AvgRating;
            metrics.TotalReviews = reviewMetrics.Total;
            metrics.ReviewsThisMonth = reviewMetrics.ThisMonth;

            // Top caterings
            metrics.TopCaterings = GetTopCaterings();

            // Recent orders
            metrics.RecentOrders = GetRecentOrders();

            // Revenue chart (last 7 days)
            metrics.RevenueChart = GetRevenueChart();

            return metrics;
        }

        private (int Total, int Active, int Today, int ThisMonth) GetUserMetrics()
        {
            string query = $@"
                SELECT
                    COUNT(*) AS Total,
                    COUNT(CASE WHEN ISNULL(c_isblocked, 0) = 0 THEN 1 END) AS Active,
                    COUNT(CASE WHEN CAST(c_createddate AS DATE) = CAST(GETDATE() AS DATE) THEN 1 END) AS Today,
                    COUNT(CASE WHEN MONTH(c_createddate) = MONTH(GETDATE()) AND YEAR(c_createddate) = YEAR(GETDATE()) THEN 1 END) AS ThisMonth
                FROM {Table.SysUser}";

            var dt = _dbHelper.Execute(query);
            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                return (
                    Convert.ToInt32(row["Total"]),
                    Convert.ToInt32(row["Active"]),
                    Convert.ToInt32(row["Today"]),
                    Convert.ToInt32(row["ThisMonth"])
                );
            }
            return (0, 0, 0, 0);
        }

        private (int Total, int Active, int Pending, int Today, int ThisMonth) GetCateringMetrics()
        {
            string query = $@"
                SELECT
                    COUNT(*) AS Total,
                    COUNT(CASE WHEN c_isactive = 1 THEN 1 END) AS Active,
                    COUNT(CASE WHEN c_approval_status = 1 THEN 1 END) AS Pending,
                    COUNT(CASE WHEN CAST(c_createddate AS DATE) = CAST(GETDATE() AS DATE) THEN 1 END) AS Today,
                    COUNT(CASE WHEN MONTH(c_createddate) = MONTH(GETDATE()) AND YEAR(c_createddate) = YEAR(GETDATE()) THEN 1 END) AS ThisMonth
                FROM {Table.SysCateringOwner}";

            var dt = _dbHelper.Execute(query);
            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                return (
                    Convert.ToInt32(row["Total"]),
                    Convert.ToInt32(row["Active"]),
                    Convert.ToInt32(row["Pending"]),
                    Convert.ToInt32(row["Today"]),
                    Convert.ToInt32(row["ThisMonth"])
                );
            }
            return (0, 0, 0, 0, 0);
        }

        private (int Total, int Pending, int Completed, int Cancelled, int Today, int ThisMonth) GetOrderMetrics()
        {
            string query = $@"
                SELECT
                    COUNT(*) AS Total,
                    COUNT(CASE WHEN c_order_status = 'Pending' THEN 1 END) AS Pending,
                    COUNT(CASE WHEN c_order_status = 'Completed' THEN 1 END) AS Completed,
                    COUNT(CASE WHEN c_order_status = 'Cancelled' THEN 1 END) AS Cancelled,
                    COUNT(CASE WHEN CAST(c_createddate AS DATE) = CAST(GETDATE() AS DATE) THEN 1 END) AS Today,
                    COUNT(CASE WHEN MONTH(c_createddate) = MONTH(GETDATE()) AND YEAR(c_createddate) = YEAR(GETDATE()) THEN 1 END) AS ThisMonth
                FROM {Table.SysOrders}";

            var dt = _dbHelper.Execute(query);
            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                return (
                    Convert.ToInt32(row["Total"]),
                    Convert.ToInt32(row["Pending"]),
                    Convert.ToInt32(row["Completed"]),
                    Convert.ToInt32(row["Cancelled"]),
                    Convert.ToInt32(row["Today"]),
                    Convert.ToInt32(row["ThisMonth"])
                );
            }
            return (0, 0, 0, 0, 0, 0);
        }

        private (decimal Total, decimal Today, decimal ThisMonth, decimal Commission, decimal AvgOrderValue) GetRevenueMetrics()
        {
            string query = $@"
                SELECT
                    ISNULL(SUM(CASE WHEN c_order_status = 'Completed' THEN c_total_amount ELSE 0 END), 0) AS Total,
                    ISNULL(SUM(CASE WHEN c_order_status = 'Completed' AND CAST(c_createddate AS DATE) = CAST(GETDATE() AS DATE) THEN c_total_amount ELSE 0 END), 0) AS Today,
                    ISNULL(SUM(CASE WHEN c_order_status = 'Completed' AND MONTH(c_createddate) = MONTH(GETDATE()) AND YEAR(c_createddate) = YEAR(GETDATE()) THEN c_total_amount ELSE 0 END), 0) AS ThisMonth,
                    ISNULL(SUM(CASE WHEN c_order_status = 'Completed' THEN c_platform_commission ELSE 0 END), 0) AS Commission,
                    CASE WHEN COUNT(CASE WHEN c_order_status = 'Completed' THEN 1 END) > 0
                         THEN ISNULL(SUM(CASE WHEN c_order_status = 'Completed' THEN c_total_amount ELSE 0 END), 0) / COUNT(CASE WHEN c_order_status = 'Completed' THEN 1 END)
                         ELSE 0 END AS AvgOrderValue
                FROM {Table.SysOrders}";

            var dt = _dbHelper.Execute(query);
            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                return (
                    Convert.ToDecimal(row["Total"]),
                    Convert.ToDecimal(row["Today"]),
                    Convert.ToDecimal(row["ThisMonth"]),
                    Convert.ToDecimal(row["Commission"]),
                    Convert.ToDecimal(row["AvgOrderValue"])
                );
            }
            return (0, 0, 0, 0, 0);
        }

        private (decimal AvgRating, int Total, int ThisMonth) GetReviewMetrics()
        {
            string query = $@"
                SELECT
                    ISNULL(AVG(CAST(c_overall_rating AS DECIMAL(3,2))), 0) AS AvgRating,
                    COUNT(*) AS Total,
                    COUNT(CASE WHEN MONTH(c_createddate) = MONTH(GETDATE()) AND YEAR(c_createddate) = YEAR(GETDATE()) THEN 1 END) AS ThisMonth
                FROM {Table.SysCateringReview}";

            var dt = _dbHelper.Execute(query);
            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                return (
                    Convert.ToDecimal(row["AvgRating"]),
                    Convert.ToInt32(row["Total"]),
                    Convert.ToInt32(row["ThisMonth"])
                );
            }
            return (0, 0, 0);
        }

        private List<AdminTopCatering> GetTopCaterings()
        {
            string query = $@"
                SELECT TOP 5
                    co.c_ownerid AS CateringId,
                    co.c_catering_name AS BusinessName,
                    c.c_cityname AS City,
                    ISNULL(SUM(o.c_total_amount), 0) AS TotalEarnings,
                    COUNT(o.c_orderid) AS TotalOrders,
                    ISNULL(AVG(CAST(r.c_overall_rating AS DECIMAL(3,2))), 0) AS Rating
                FROM {Table.SysCateringOwner} co
                LEFT JOIN {Table.SysOrders} o ON co.c_ownerid = o.c_ownerid AND o.c_order_status = 'Completed'
                LEFT JOIN {Table.SysCateringReview} r ON co.c_ownerid = r.c_ownerid
                LEFT JOIN {Table.SysCateringOwnerAddress} ad ON co.c_ownerid = ad.c_ownerid
                LEFT JOIN {Table.City} c ON ad.c_cityid = c.c_cityid
                WHERE co.c_isactive = 1
                GROUP BY co.c_ownerid, co.c_catering_name, c.c_cityname
                ORDER BY TotalEarnings DESC";

            var dt = _dbHelper.Execute(query);
            var result = new List<AdminTopCatering>();

            foreach (DataRow row in dt.Rows)
            {
                result.Add(new AdminTopCatering
                {
                    CateringId = Convert.ToInt64(row["CateringId"]),
                    BusinessName = row["BusinessName"]?.ToString() ?? string.Empty,
                    City = row["City"]?.ToString() ?? string.Empty,
                    TotalEarnings = Convert.ToDecimal(row["TotalEarnings"]),
                    TotalOrders = Convert.ToInt32(row["TotalOrders"]),
                    Rating = Convert.ToDecimal(row["Rating"])
                });
            }

            return result;
        }

        private List<AdminRecentOrder> GetRecentOrders()
        {
            string query = $@"
                SELECT TOP 10
                    o.c_orderid AS OrderId,
                    u.c_name AS CustomerName,
                    co.c_catering_name AS CateringName,
                    o.c_total_amount AS TotalAmount,
                    o.c_order_status AS Status,
                    o.c_createddate AS OrderDate,
                    o.c_event_date AS EventDate
                FROM {Table.SysOrders} o
                JOIN {Table.SysUser} u ON o.c_userid = u.c_userid
                JOIN {Table.SysCateringOwner} co ON o.c_ownerid = co.c_ownerid
                ORDER BY o.c_createddate DESC";

            var dt = _dbHelper.Execute(query);
            var result = new List<AdminRecentOrder>();

            foreach (DataRow row in dt.Rows)
            {
                result.Add(new AdminRecentOrder
                {
                    OrderId = Convert.ToInt64(row["OrderId"]),
                    CustomerName = row["CustomerName"]?.ToString() ?? string.Empty,
                    CateringName = row["CateringName"]?.ToString() ?? string.Empty,
                    TotalAmount = Convert.ToDecimal(row["TotalAmount"]),
                    Status = row["Status"]?.ToString() ?? string.Empty,
                    OrderDate = Convert.ToDateTime(row["OrderDate"]),
                    EventDate = Convert.ToDateTime(row["EventDate"])
                });
            }

            return result;
        }

        private List<AdminRevenueChart> GetRevenueChart()
        {
            string query = $@"
                SELECT
                    CAST(o.c_createddate AS DATE) AS Date,
                    ISNULL(SUM(o.c_total_amount), 0) AS Revenue,
                    ISNULL(SUM(o.c_platform_commission), 0) AS Commission,
                    COUNT(o.c_orderid) AS OrderCount
                FROM {Table.SysOrders} o
                WHERE o.c_order_status = 'Completed'
                  AND o.c_createddate >= DATEADD(DAY, -7, GETDATE())
                GROUP BY CAST(o.c_createddate AS DATE)
                ORDER BY Date";

            var dt = _dbHelper.Execute(query);
            var result = new List<AdminRevenueChart>();

            foreach (DataRow row in dt.Rows)
            {
                result.Add(new AdminRevenueChart
                {
                    Date = Convert.ToDateTime(row["Date"]).ToString("MMM dd"),
                    Revenue = Convert.ToDecimal(row["Revenue"]),
                    Commission = Convert.ToDecimal(row["Commission"]),
                    OrderCount = Convert.ToInt32(row["OrderCount"])
                });
            }

            return result;
        }
    }
}
