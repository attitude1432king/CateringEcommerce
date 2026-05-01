using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CateringEcommerce.BAL.Base.Owner.Dashboard
{
    public class OwnerDashboardRepository : IOwnerDashboardRepository
    {
        private readonly IDatabaseHelper _dbHelper;
        public OwnerDashboardRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<DashboardMetricsDto> GetDashboardMetrics(long ownerId)
        {
            try
            {
                var query = $@"
                    -- Current Period Metrics
                    SELECT
                        COALESCE(SUM(c_total_amount), 0) AS TotalRevenue,
                        COUNT(*) AS TotalOrders,
                        SUM(CASE WHEN c_order_status = 'Pending' THEN 1 ELSE 0 END) AS PendingOrders,
                        COUNT(DISTINCT c_userid) AS TotalCustomers,
                        COALESCE(AVG(c_total_amount), 0) AS AverageOrderValue,
                        SUM(CASE WHEN c_event_date >= NOW() THEN 1 ELSE 0 END) AS UpcomingEvents
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId AND c_createddate >= DATE_TRUNC('month', NOW());

                    -- Previous Period Metrics
                    SELECT
                        COALESCE(SUM(c_total_amount), 0) AS PrevTotalRevenue,
                        COUNT(*) AS PrevTotalOrders,
                        SUM(CASE WHEN c_order_status = 'Pending' THEN 1 ELSE 0 END) AS PrevPendingOrders,
                        COUNT(DISTINCT c_userid) AS PrevTotalCustomers,
                        COALESCE(AVG(c_total_amount), 0) AS PrevAverageOrderValue
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId
                        AND c_createddate >= DATE_TRUNC('month', NOW()) - INTERVAL '1 month'
                        AND c_createddate < DATE_TRUNC('month', NOW());

                    -- Customer Satisfaction
                    SELECT COALESCE(AVG(CAST(c_overall_rating AS DECIMAL(10,2))), 0) AS CustomerSatisfaction
                    FROM {Table.SysCateringReview}
                    WHERE c_ownerid = @OwnerId AND c_overall_rating > 0";

                var parameters = new[] { new NpgsqlParameter("@OwnerId", ownerId) };
                var dataSet = await Task.Run(() => _dbHelper.ExecuteDataSet(query, parameters));

                var metrics = new DashboardMetricsDto();
                if (dataSet == null || dataSet.Tables.Count == 0)
                {
                    return metrics;
                }

                if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
                {
                    var currentRow = dataSet.Tables[0].Rows[0];
                    metrics.TotalRevenue = GetDecimalValue(currentRow, "TotalRevenue");
                    metrics.TotalOrders = GetIntValue(currentRow, "TotalOrders");
                    metrics.PendingOrders = GetIntValue(currentRow, "PendingOrders");
                    metrics.TotalCustomers = GetIntValue(currentRow, "TotalCustomers");
                    metrics.AverageOrderValue = GetDecimalValue(currentRow, "AverageOrderValue");
                    metrics.UpcomingEvents = GetIntValue(currentRow, "UpcomingEvents");
                }

                if (dataSet.Tables.Count > 1 && dataSet.Tables[1].Rows.Count > 0)
                {
                    var prevRow = dataSet.Tables[1].Rows[0];
                    var prevRevenue = GetDecimalValue(prevRow, "PrevTotalRevenue");
                    var prevOrders = GetIntValue(prevRow, "PrevTotalOrders");
                    var prevPending = GetIntValue(prevRow, "PrevPendingOrders");
                    var prevCustomers = GetIntValue(prevRow, "PrevTotalCustomers");
                    var prevAvgOrder = GetDecimalValue(prevRow, "PrevAverageOrderValue");

                    metrics.RevenueChange = CalculatePercentageChange(metrics.TotalRevenue, prevRevenue);
                    metrics.OrdersChange = CalculatePercentageChange(metrics.TotalOrders, prevOrders);
                    metrics.PendingOrdersChange = CalculatePercentageChange(metrics.PendingOrders, prevPending);
                    metrics.CustomersChange = CalculatePercentageChange(metrics.TotalCustomers, prevCustomers);
                    metrics.AverageOrderValueChange = CalculatePercentageChange(metrics.AverageOrderValue, prevAvgOrder);
                }

                if (dataSet.Tables.Count > 2 && dataSet.Tables[2].Rows.Count > 0)
                {
                    metrics.CustomerSatisfaction = GetDecimalValue(dataSet.Tables[2].Rows[0], "CustomerSatisfaction");
                }

                return metrics;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting dashboard metrics: {ex.Message}", ex);
            }
        }

        public async Task<RevenueChartDto> GetRevenueChart(long ownerId, string period = "month")
        {
            try
            {
                var chart = new RevenueChartDto { Period = period };
                string query = "";
                string dateFormat = "";
                string dateGrouping = "";

                switch (period.ToLower())
                {
                    case "day":
                        dateFormat = "TO_CHAR(c_createddate, 'YYYY-MM-DD')";
                        dateGrouping = "DATE_TRUNC('day', c_createddate)";
                        query = $@"
                            SELECT
                                {dateFormat} AS Period,
                                COALESCE(SUM(c_total_amount), 0) AS Revenue
                            FROM {Table.SysOrders}
                            WHERE c_ownerid = @OwnerId
                                AND c_createddate >= NOW() - INTERVAL '30 days'
                            GROUP BY {dateGrouping}, {dateFormat}
                            ORDER BY {dateGrouping}";
                        break;

                    case "week":
                        dateFormat = "CONCAT('Week ', EXTRACT(WEEK FROM c_createddate), ' - ', EXTRACT(YEAR FROM c_createddate))";
                        query = $@"
                            SELECT
                                {dateFormat} AS Period,
                                COALESCE(SUM(c_total_amount), 0) AS Revenue
                            FROM {Table.SysOrders}
                            WHERE c_ownerid = @OwnerId
                                AND c_createddate >= NOW() - INTERVAL '12 weeks'
                            GROUP BY EXTRACT(WEEK FROM c_createddate), EXTRACT(YEAR FROM c_createddate)
                            ORDER BY EXTRACT(YEAR FROM c_createddate), EXTRACT(WEEK FROM c_createddate)";
                        break;

                    case "year":
                        dateFormat = "CAST(EXTRACT(YEAR FROM c_createddate) AS VARCHAR(4))";
                        query = $@"
                            SELECT
                                {dateFormat} AS Period,
                                COALESCE(SUM(c_total_amount), 0) AS Revenue
                            FROM {Table.SysOrders}
                            WHERE c_ownerid = @OwnerId
                                AND c_createddate >= NOW() - INTERVAL '5 years'
                            GROUP BY EXTRACT(YEAR FROM c_createddate)
                            ORDER BY EXTRACT(YEAR FROM c_createddate)";
                        break;

                    case "month":
                    default:
                        dateFormat = "TO_CHAR(c_createddate, 'Mon YYYY')";
                        query = $@"
                            SELECT
                                {dateFormat} AS Period,
                                COALESCE(SUM(c_total_amount), 0) AS Revenue
                            FROM {Table.SysOrders}
                            WHERE c_ownerid = @OwnerId
                                AND c_createddate >= NOW() - INTERVAL '12 months'
                            GROUP BY EXTRACT(YEAR FROM c_createddate), EXTRACT(MONTH FROM c_createddate), TO_CHAR(c_createddate, 'Mon YYYY')
                            ORDER BY EXTRACT(YEAR FROM c_createddate), EXTRACT(MONTH FROM c_createddate)";
                        break;
                }

                var chartParameters = new[] { new NpgsqlParameter("@OwnerId", ownerId) };
                var dataTable = await Task.Run(() => _dbHelper.ExecuteAsync(query, chartParameters));

                foreach (DataRow row in dataTable.Rows)
                {
                    chart.Labels.Add(row["Period"].ToString());
                    chart.Data.Add(GetDecimalValue(row, "Revenue"));
                }

                chart.TotalRevenue = chart.Data.Sum();
                chart.AverageRevenue = chart.Data.Count > 0 ? chart.Data.Average() : 0;

                return chart;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting revenue chart: {ex.Message}", ex);
            }
        }

        public async Task<OrderChartDto> GetOrdersChart(long ownerId, string period = "month")
        {
            try
            {
                var chart = new OrderChartDto { Period = period };
                string query = "";
                string dateFormat = "";

                switch (period.ToLower())
                {
                    case "day":
                        dateFormat = "TO_CHAR(c_createddate, 'YYYY-MM-DD')";
                        query = $@"
                            SELECT
                                {dateFormat} AS Period,
                                COUNT(*) AS OrderCount
                            FROM {Table.SysOrders}
                            WHERE c_ownerid = @OwnerId
                                AND c_createddate >= NOW() - INTERVAL '30 days'
                            GROUP BY {dateFormat}
                            ORDER BY {dateFormat}";
                        break;

                    case "week":
                        dateFormat = "CONCAT('Week ', EXTRACT(WEEK FROM c_createddate), ' - ', EXTRACT(YEAR FROM c_createddate))";
                        query = $@"
                            SELECT
                                {dateFormat} AS Period,
                                COUNT(*) AS OrderCount
                            FROM {Table.SysOrders}
                            WHERE c_ownerid = @OwnerId
                                AND c_createddate >= NOW() - INTERVAL '12 weeks'
                            GROUP BY EXTRACT(WEEK FROM c_createddate), EXTRACT(YEAR FROM c_createddate)
                            ORDER BY EXTRACT(YEAR FROM c_createddate), EXTRACT(WEEK FROM c_createddate)";
                        break;

                    case "year":
                        dateFormat = "CAST(EXTRACT(YEAR FROM c_createddate) AS VARCHAR(4))";
                        query = $@"
                            SELECT
                                {dateFormat} AS Period,
                                COUNT(*) AS OrderCount
                            FROM {Table.SysOrders}
                            WHERE c_ownerid = @OwnerId
                                AND c_createddate >= NOW() - INTERVAL '5 years'
                            GROUP BY EXTRACT(YEAR FROM c_createddate)
                            ORDER BY EXTRACT(YEAR FROM c_createddate)";
                        break;

                    case "month":
                    default:
                        dateFormat = "TO_CHAR(c_createddate, 'Mon YYYY')";
                        query = $@"
                            SELECT
                                {dateFormat} AS Period,
                                COUNT(*) AS OrderCount
                            FROM {Table.SysOrders}
                            WHERE c_ownerid = @OwnerId
                                AND c_createddate >= NOW() - INTERVAL '12 months'
                            GROUP BY EXTRACT(YEAR FROM c_createddate), EXTRACT(MONTH FROM c_createddate), TO_CHAR(c_createddate, 'Mon YYYY')
                            ORDER BY EXTRACT(YEAR FROM c_createddate), EXTRACT(MONTH FROM c_createddate)";
                        break;
                }

                var parameters = new[] { new NpgsqlParameter("@OwnerId", ownerId) };
                var dataTable = await Task.Run(() => _dbHelper.ExecuteAsync(query, parameters));

                foreach (DataRow row in dataTable.Rows)
                {
                    chart.Labels.Add(row["Period"].ToString());
                    chart.Data.Add(GetIntValue(row, "OrderCount"));
                }

                chart.TotalOrders = chart.Data.Sum();

                // Get orders by status
                var statusQuery = $@"
                    SELECT c_order_status AS Status, COUNT(*) AS Count
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId
                    GROUP BY c_order_status";

                var statusParameters = new[] { new NpgsqlParameter("@OwnerId", ownerId) };
                var statusTable = await Task.Run(() => _dbHelper.ExecuteAsync(statusQuery, statusParameters));
                foreach (DataRow row in statusTable.Rows)
                {
                    chart.OrdersByStatus[row["Status"].ToString()] = GetIntValue(row, "Count");
                }

                return chart;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting orders chart: {ex.Message}", ex);
            }
        }

        public async Task<List<RecentOrderDto>> GetRecentOrders(long ownerId, int limit = 5)
        {
            try
            {
                var query = $@"
                    SELECT
                        o.c_orderid AS OrderId,
                        o.c_order_number AS OrderNumber,
                        u.c_name AS CustomerName,
                        o.c_event_type AS EventType,
                        o.c_event_date AS EventDate,
                        o.c_total_amount AS TotalAmount,
                        o.c_order_status AS OrderStatus,
                        o.c_createddate AS OrderDate,
                        o.c_guest_count AS GuestCount
                    FROM {Table.SysOrders} o
                    INNER JOIN {Table.SysUser} u ON o.c_userid = u.c_userid
                    WHERE o.c_ownerid = @OwnerId
                    ORDER BY o.c_createddate DESC
                    LIMIT {limit}";

                var parameters = new[] { new NpgsqlParameter("@OwnerId", ownerId) };
                var dataTable = await Task.Run(() => _dbHelper.ExecuteAsync(query, parameters));

                var orders = new List<RecentOrderDto>();
                foreach (DataRow row in dataTable.Rows)
                {
                    orders.Add(new RecentOrderDto
                    {
                        OrderId = GetLongValue(row, "OrderId"),
                        OrderNumber = row["OrderNumber"].ToString(),
                        CustomerName = row["CustomerName"].ToString(),
                        EventType = row["EventType"].ToString(),
                        EventDate = GetDateTimeValue(row, "EventDate"),
                        TotalAmount = GetDecimalValue(row, "TotalAmount"),
                        OrderStatus = row["OrderStatus"].ToString(),
                        OrderDate = GetDateTimeValue(row, "OrderDate"),
                        GuestCount = GetIntValue(row, "GuestCount")
                    });
                }

                return orders;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting recent orders: {ex.Message}", ex);
            }
        }

        public async Task<List<UpcomingEventDto>> GetUpcomingEvents(long ownerId, int days = 7)
        {
            try
            {
                var query = $@"
                    SELECT
                        o.c_orderid AS OrderId,
                        o.c_order_number AS OrderNumber,
                        u.c_name AS CustomerName,
                        u.c_mobile AS CustomerPhone,
                        o.c_event_type AS EventType,
                        o.c_event_date AS EventDate,
                        o.c_event_time AS EventTime,
                        o.c_delivery_address AS VenueAddress,
                        o.c_guest_count AS GuestCount,
                        o.c_total_amount AS TotalAmount,
                        o.c_order_status AS OrderStatus,
                        (o.c_event_date::date - CURRENT_DATE) AS DaysUntilEvent
                    FROM {Table.SysOrders} o
                    INNER JOIN {Table.SysUser} u ON o.c_userid = u.c_userid
                    WHERE o.c_ownerid = @OwnerId
                        AND o.c_event_date >= CAST(NOW() AS DATE)
                        AND o.c_event_date <= NOW() + (@Days * INTERVAL '1 day')
                        AND o.c_order_status NOT IN ('Cancelled', 'Completed')
                    ORDER BY o.c_event_date, o.c_event_time";

                var parameters = new[]
                {
                    new NpgsqlParameter("@OwnerId", ownerId),
                    new NpgsqlParameter("@Days", days)
                };

                var dataTable = await Task.Run(() => _dbHelper.ExecuteAsync(query, parameters));

                var events = new List<UpcomingEventDto>();
                foreach (DataRow row in dataTable.Rows)
                {
                    var daysUntil = GetIntValue(row, "DaysUntilEvent");
                    events.Add(new UpcomingEventDto
                    {
                        OrderId = GetLongValue(row, "OrderId"),
                        OrderNumber = row["OrderNumber"].ToString(),
                        CustomerName = row["CustomerName"].ToString(),
                        CustomerPhone = row["CustomerPhone"].ToString(),
                        EventType = row["EventType"].ToString(),
                        EventDate = GetDateTimeValue(row, "EventDate"),
                        EventTime = row["EventTime"].ToString(),
                        VenueAddress = row["VenueAddress"].ToString(),
                        GuestCount = GetIntValue(row, "GuestCount"),
                        TotalAmount = GetDecimalValue(row, "TotalAmount"),
                        OrderStatus = row["OrderStatus"].ToString(),
                        DaysUntilEvent = daysUntil,
                        IsUrgent = daysUntil <= 2
                    });
                }

                return events;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting upcoming events: {ex.Message}", ex);
            }
        }

        public async Task<List<TopMenuItemDto>> GetTopMenuItems(long ownerId, int limit = 10)
        {
            try
            {
                var query = $@"
                    SELECT
                        oi.c_foodid AS MenuItemId,
                        COALESCE(f.c_foodname, p.c_packagename) AS MenuItemName,
                        COALESCE(fc.c_categoryname, 'Package') AS Category,
                        COUNT(DISTINCT oi.c_orderid) AS OrderCount,
                        SUM(oi.c_quantity) AS TotalQuantitySold,
                        SUM(oi.c_item_total) AS TotalRevenue,
                        COALESCE(AVG(CAST(r.c_overall_rating AS DECIMAL(10,2))), 0) AS AverageRating,
                        '' AS ImageUrl,
                        f.c_price AS Price,
                        CASE
                            WHEN f.c_ispackage_item = TRUE THEN 'Package'
                            ELSE 'Individual Item'
                        END AS ItemType
                    FROM {Table.SysOrderItems} oi
                    INNER JOIN {Table.SysOrders} o ON oi.c_orderid = o.c_orderid
                    INNER JOIN {Table.SysFoodItems} f ON oi.c_foodid = f.c_foodid
                    LEFT JOIN {Table.SysFoodCategory} fc ON f.c_categoryid = fc.c_categoryid
                    LEFT JOIN {Table.SysMenuPackage} p ON f.c_ispackage_item = TRUE
                        AND EXISTS (
                            SELECT 1 FROM {Table.SysMenuPackageItems} pi
                            WHERE pi.c_packageid = p.c_packageid
                        )
                    LEFT JOIN {Table.SysCateringReview} r ON o.c_orderid = r.c_orderid
                    WHERE o.c_ownerid = @OwnerId
                        AND o.c_order_status = 'Completed'
                        AND f.c_is_deleted = FALSE
                    GROUP BY oi.c_foodid, COALESCE(f.c_foodname, p.c_packagename),
                             COALESCE(fc.c_categoryname, 'Package'), f.c_price, f.c_ispackage_item
                    ORDER BY TotalRevenue DESC
                    LIMIT {limit}";

                var parameters = new[] { new NpgsqlParameter("@OwnerId", ownerId) };
                var dataTable = await Task.Run(() => _dbHelper.ExecuteAsync(query, parameters));

                var items = new List<TopMenuItemDto>();
                foreach (DataRow row in dataTable.Rows)
                {
                    items.Add(new TopMenuItemDto
                    {
                        MenuItemId = GetLongValue(row, "MenuItemId"),
                        MenuItemName = row["MenuItemName"].ToString(),
                        Category = row["Category"].ToString(),
                        OrderCount = GetIntValue(row, "OrderCount"),
                        TotalQuantitySold = GetIntValue(row, "TotalQuantitySold"),
                        TotalRevenue = GetDecimalValue(row, "TotalRevenue"),
                        AverageRating = GetDecimalValue(row, "AverageRating"),
                        ImageUrl = row["ImageUrl"].ToString(),
                        Price = GetDecimalValue(row, "Price")
                    });
                }

                return items;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting top menu items: {ex.Message}", ex);
            }
        }

        public async Task<PerformanceInsightsDto> GetPerformanceInsights(long ownerId)
        {
            try
            {
                var query = $@"
                    -- Growth metrics
                    SELECT
                        COALESCE(SUM(CASE WHEN c_createddate >= DATE_TRUNC('month', NOW()) THEN c_total_amount ELSE 0 END), 0) AS CurrentRevenue,
                        COALESCE(SUM(CASE WHEN c_createddate >= DATE_TRUNC('month', NOW()) - INTERVAL '1 month' AND c_createddate < DATE_TRUNC('month', NOW()) THEN c_total_amount ELSE 0 END), 0) AS PreviousRevenue,
                        COUNT(CASE WHEN c_createddate >= DATE_TRUNC('month', NOW()) THEN 1 END) AS CurrentOrders,
                        COUNT(CASE WHEN c_createddate >= DATE_TRUNC('month', NOW()) - INTERVAL '1 month' AND c_createddate < DATE_TRUNC('month', NOW()) THEN 1 END) AS PreviousOrders,
                        COUNT(DISTINCT CASE WHEN c_createddate >= DATE_TRUNC('month', NOW()) THEN c_userid END) AS CurrentCustomers,
                        COUNT(DISTINCT CASE WHEN c_createddate < DATE_TRUNC('month', NOW()) THEN c_userid END) AS ReturningCustomers
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId;

                    -- Ratings
                    SELECT COALESCE(AVG(CAST(c_overall_rating AS DECIMAL(10,2))), 0) AS AvgRating
                    FROM {Table.SysCateringReview}
                    WHERE c_ownerid = @OwnerId;

                    -- Cancellation rate
                    SELECT
                        CAST(COUNT(CASE WHEN c_order_status = 'Cancelled' THEN 1 END) * 100.0 / COUNT(*) AS INT) AS CancellationRate
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId;

                    -- Best performing category (based on food items and packages)
                    SELECT COALESCE(fc.c_categoryname, 'Package') AS BestCategory
                    FROM {Table.SysOrderItems} oi
                    INNER JOIN {Table.SysOrders} o ON oi.c_orderid = o.c_orderid
                    INNER JOIN {Table.SysFoodItems} f ON oi.c_foodid = f.c_foodid
                    LEFT JOIN {Table.SysFoodCategory} fc ON f.c_categoryid = fc.c_categoryid
                    WHERE o.c_ownerid = @OwnerId
                        AND f.c_is_deleted = FALSE
                    GROUP BY COALESCE(fc.c_categoryname, 'Package')
                    ORDER BY SUM(oi.c_item_total) DESC
                    LIMIT 1;

                    -- Peak order day
                    SELECT TO_CHAR(c_createddate, 'FMDay') AS PeakDay
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId
                    GROUP BY TO_CHAR(c_createddate, 'FMDay')
                    ORDER BY COUNT(*) DESC
                    LIMIT 1;

                    -- Pending payments
                    SELECT COALESCE(SUM(o.c_total_amount - COALESCE(pay.PaidAmount, 0)), 0) AS PendingPayments
                    FROM {Table.SysOrders} o
                    LEFT JOIN LATERAL (
                        SELECT SUM(COALESCE(p.c_paid_amount, p.c_amount)) AS PaidAmount
                        FROM {Table.SysOrderPayments} p
                        WHERE p.c_orderid = o.c_orderid
                          AND COALESCE(p.c_status, '') NOT IN ('Failed', 'Rejected', 'Cancelled')
                    ) pay ON TRUE
                    WHERE o.c_ownerid = @OwnerId AND o.c_payment_status != 'Completed';";

                var parameters = new[] { new NpgsqlParameter("@OwnerId", ownerId) };
                var dataSet = await Task.Run(() => _dbHelper.ExecuteDataSet(query, parameters));

                var insights = new PerformanceInsightsDto();

                if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
                {
                    var row = dataSet.Tables[0].Rows[0];
                    var currentRevenue = GetDecimalValue(row, "CurrentRevenue");
                    var previousRevenue = GetDecimalValue(row, "PreviousRevenue");
                    var currentOrders = GetIntValue(row, "CurrentOrders");
                    var previousOrders = GetIntValue(row, "PreviousOrders");
                    var currentCustomers = GetIntValue(row, "CurrentCustomers");
                    var returningCustomers = GetIntValue(row, "ReturningCustomers");

                    insights.RevenueGrowth = CalculatePercentageChange(currentRevenue, previousRevenue);
                    insights.OrderGrowth = CalculatePercentageChange(currentOrders, previousOrders);
                    insights.CustomerRetentionRate = currentCustomers > 0
                        ? (returningCustomers * 100.0m / currentCustomers)
                        : 0;
                }

                if (dataSet.Tables.Count > 1 && dataSet.Tables[1].Rows.Count > 0)
                {
                    insights.AverageDeliveryRating = GetDecimalValue(dataSet.Tables[1].Rows[0], "AvgRating");
                }

                if (dataSet.Tables.Count > 2 && dataSet.Tables[2].Rows.Count > 0)
                {
                    insights.CancellationRate = GetIntValue(dataSet.Tables[2].Rows[0], "CancellationRate");
                }

                if (dataSet.Tables.Count > 3 && dataSet.Tables[3].Rows.Count > 0)
                {
                    insights.BestPerformingCategory = dataSet.Tables[3].Rows[0]["BestCategory"].ToString();
                }

                if (dataSet.Tables.Count > 4 && dataSet.Tables[4].Rows.Count > 0)
                {
                    insights.PeakOrderDay = dataSet.Tables[4].Rows[0]["PeakDay"].ToString();
                }

                if (dataSet.Tables.Count > 5 && dataSet.Tables[5].Rows.Count > 0)
                {
                    insights.PendingPaymentsAmount = GetDecimalValue(dataSet.Tables[5].Rows[0], "PendingPayments");
                }

                return insights;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting performance insights: {ex.Message}", ex);
            }
        }

        public async Task<RevenueBreakdownDto> GetRevenueBreakdown(long ownerId)
        {
            try
            {
                var query = $@"
                    -- By Event Type
                    SELECT
                        c_event_type AS EventType,
                        SUM(c_total_amount) AS Revenue
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId
                    GROUP BY c_event_type;

                    -- By Payment Status
                    SELECT
                        c_payment_status AS PaymentStatus,
                        SUM(c_total_amount) AS Revenue
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId
                    GROUP BY c_payment_status;

                    -- By Month (Last 12 months)
                    SELECT
                        TO_CHAR(c_createddate, 'Mon YYYY') AS Month,
                        SUM(c_total_amount) AS Revenue
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId
                        AND c_createddate >= NOW() - INTERVAL '12 months'
                    GROUP BY EXTRACT(YEAR FROM c_createddate), EXTRACT(MONTH FROM c_createddate), TO_CHAR(c_createddate, 'Mon YYYY')
                    ORDER BY EXTRACT(YEAR FROM c_createddate), EXTRACT(MONTH FROM c_createddate);

                    -- Financial Summary
                    SELECT
                        SUM(c_total_amount) AS GrossRevenue,
                        SUM(c_total_amount - c_tax_amount) AS NetRevenue,
                        SUM(c_tax_amount) AS TaxAmount,
                        SUM(c_discount_amount) AS DiscountAmount
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId;";

                var parameters = new[] { new NpgsqlParameter("@OwnerId", ownerId) };
                var dataSet = await Task.Run(() => _dbHelper.ExecuteDataSet(query, parameters));

                var breakdown = new RevenueBreakdownDto();

                if (dataSet.Tables.Count > 0)
                {
                    foreach (DataRow row in dataSet.Tables[0].Rows)
                    {
                        breakdown.ByEventType[row["EventType"].ToString()] = GetDecimalValue(row, "Revenue");
                    }
                }

                if (dataSet.Tables.Count > 1)
                {
                    foreach (DataRow row in dataSet.Tables[1].Rows)
                    {
                        breakdown.ByPaymentStatus[row["PaymentStatus"].ToString()] = GetDecimalValue(row, "Revenue");
                    }
                }

                if (dataSet.Tables.Count > 2)
                {
                    foreach (DataRow row in dataSet.Tables[2].Rows)
                    {
                        breakdown.ByMonth[row["Month"].ToString()] = GetDecimalValue(row, "Revenue");
                    }
                }

                if (dataSet.Tables.Count > 3 && dataSet.Tables[3].Rows.Count > 0)
                {
                    var row = dataSet.Tables[3].Rows[0];
                    breakdown.GrossRevenue = GetDecimalValue(row, "GrossRevenue");
                    breakdown.NetRevenue = GetDecimalValue(row, "NetRevenue");
                    breakdown.TaxAmount = GetDecimalValue(row, "TaxAmount");
                    breakdown.DiscountAmount = GetDecimalValue(row, "DiscountAmount");
                }

                return breakdown;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting revenue breakdown: {ex.Message}", ex);
            }
        }

        private decimal CalculatePercentageChange(decimal current, decimal previous)
        {
            if (previous == 0)
                return current > 0 ? 100 : 0;

            return Math.Round(((current - previous) / previous) * 100, 2);
        }

        private static decimal GetDecimalValue(DataRow row, string columnName, decimal defaultValue = 0m)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columnName))
            {
                return defaultValue;
            }

            var value = row[columnName];
            return value == null || value == DBNull.Value ? defaultValue : Convert.ToDecimal(value);
        }

        private static int GetIntValue(DataRow row, string columnName, int defaultValue = 0)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columnName))
            {
                return defaultValue;
            }

            var value = row[columnName];
            return value == null || value == DBNull.Value ? defaultValue : Convert.ToInt32(value);
        }

        private static long GetLongValue(DataRow row, string columnName, long defaultValue = 0L)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columnName))
            {
                return defaultValue;
            }

            var value = row[columnName];
            return value == null || value == DBNull.Value ? defaultValue : Convert.ToInt64(value);
        }

        private static DateTime GetDateTimeValue(DataRow row, string columnName, DateTime? defaultValue = null)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columnName))
            {
                return defaultValue ?? DateTime.MinValue;
            }

            var value = row[columnName];
            return value == null || value == DBNull.Value ? (defaultValue ?? DateTime.MinValue) : Convert.ToDateTime(value);
        }
    }
}

