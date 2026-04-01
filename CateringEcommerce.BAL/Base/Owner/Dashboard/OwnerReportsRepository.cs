using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CateringEcommerce.BAL.Base.Owner.Dashboard
{
    public class OwnerReportsRepository : IOwnerReportsRepository
    {
        private readonly IDatabaseHelper _dbHelper;
        public OwnerReportsRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<SalesReportDto> GenerateSalesReport(long ownerId, ReportFilterDto filter)
        {
            try
            {
                var startDate = filter.StartDate ?? DateTime.Now.AddMonths(-1);
                var endDate = filter.EndDate ?? DateTime.Now;

                var query = $@"
                    -- Summary
                    SELECT
                        COUNT(*) AS TotalOrders,
                        SUM(CASE WHEN c_order_status = 'Completed' THEN 1 ELSE 0 END) AS CompletedOrders,
                        SUM(CASE WHEN c_order_status = 'Pending' OR c_order_status = 'Confirmed' THEN 1 ELSE 0 END) AS PendingOrders,
                        SUM(CASE WHEN c_order_status = 'Cancelled' THEN 1 ELSE 0 END) AS CancelledOrders,
                        ISNULL(SUM(c_total_amount), 0) AS TotalRevenue,
                        ISNULL(AVG(c_total_amount), 0) AS AverageOrderValue,
                        SUM(c_guest_count) AS TotalGuestsServed
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId
                        AND c_createddate >= @StartDate
                        AND c_createddate <= @EndDate;

                    -- Event Type Breakdown
                    SELECT
                        c_event_type AS EventType,
                        COUNT(*) AS OrderCount,
                        SUM(c_total_amount) AS TotalRevenue,
                        AVG(c_total_amount) AS AverageOrderValue
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId
                        AND c_createddate >= @StartDate
                        AND c_createddate <= @EndDate
                    GROUP BY c_event_type;

                    -- Time Series Data
                    SELECT
                        FORMAT(c_createddate, 'MMM yyyy') AS Period,
                        COUNT(*) AS OrderCount,
                        SUM(c_total_amount) AS Revenue,
                        SUM(c_guest_count) AS GuestsServed
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId
                        AND c_createddate >= @StartDate
                        AND c_createddate <= @EndDate
                    GROUP BY YEAR(c_createddate), MONTH(c_createddate), FORMAT(c_createddate, 'MMM yyyy')
                    ORDER BY YEAR(c_createddate), MONTH(c_createddate);

                    -- Comparison with previous period
                    DECLARE @PreviousStartDate DATE = DATEADD(DAY, -DATEDIFF(DAY, @StartDate, @EndDate), @StartDate);
                    DECLARE @PreviousEndDate DATE = @StartDate;

                    SELECT
                        ISNULL(SUM(c_total_amount), 0) AS PreviousRevenue,
                        COUNT(*) AS PreviousOrders
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId
                        AND c_createddate >= @PreviousStartDate
                        AND c_createddate < @PreviousEndDate;";

                var parameters = new[]
                {
                    new SqlParameter("@OwnerId", ownerId),
                    new SqlParameter("@StartDate", startDate),
                    new SqlParameter("@EndDate", endDate)
                };

                var dataSet = await Task.Run(() => _dbHelper.ExecuteDataSet(query, parameters));

                var report = new SalesReportDto
                {
                    StartDate = startDate,
                    EndDate = endDate
                };

                // Summary
                if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
                {
                    var row = dataSet.Tables[0].Rows[0];
                    report.TotalOrders = row.GetValue<int>("TotalOrders");
                    report.CompletedOrders = row.GetValue<int>("CompletedOrders");
                    report.PendingOrders = row.GetValue<int>("PendingOrders");
                    report.CancelledOrders = row.GetValue<int>("CancelledOrders");
                    report.TotalRevenue = row.GetValue<decimal>("TotalRevenue");
                    report.AverageOrderValue = row.GetValue<decimal>("AverageOrderValue");
                    report.TotalGuestsServed = row.GetValue<int>("TotalGuestsServed");
                }

                // Event type breakdown
                if (dataSet.Tables.Count > 1)
                {
                    var totalRevenue = report.TotalRevenue;
                    foreach (DataRow row in dataSet.Tables[1].Rows)
                    {
                        var eventType = row.GetValue<string>("EventType", "Unknown");
                        var revenue = row.GetValue<decimal>("TotalRevenue");
                        report.EventTypeBreakdown[eventType] = new SalesBreakdownDto
                        {
                            OrderCount = row.GetValue<int>("OrderCount"),
                            TotalRevenue = revenue,
                            AverageOrderValue = row.GetValue<decimal>("AverageOrderValue"),
                            Percentage = totalRevenue > 0 ? (revenue / totalRevenue) * 100 : 0
                        };
                    }
                }

                // Time series
                if (dataSet.Tables.Count > 2)
                {
                    foreach (DataRow row in dataSet.Tables[2].Rows)
                    {
                        report.TimeSeries.Add(new SalesTimeSeriesDto
                        {
                            Period = row.GetValue<string>("Period"),
                            OrderCount = row.GetValue<int>("OrderCount"),
                            Revenue = row.GetValue<decimal>("Revenue"),
                            GuestsServed = row.GetValue<int>("GuestsServed")
                        });
                    }
                }

                // Growth comparison
                if (dataSet.Tables.Count > 3 && dataSet.Tables[3].Rows.Count > 0)
                {
                    var row = dataSet.Tables[3].Rows[0];
                    var previousRevenue = row.GetValue<decimal>("PreviousRevenue");
                    var previousOrders = row.GetValue<int>("PreviousOrders");

                    report.RevenueGrowth = CalculatePercentageChange(report.TotalRevenue, previousRevenue);
                    report.OrderGrowth = CalculatePercentageChange(report.TotalOrders, previousOrders);
                }

                return report;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating sales report: {ex.Message}", ex);
            }
        }

        public async Task<RevenueReportDto> GenerateRevenueReport(long ownerId, ReportFilterDto filter)
        {
            try
            {
                var startDate = filter.StartDate ?? DateTime.Now.AddMonths(-1);
                var endDate = filter.EndDate ?? DateTime.Now;

                var query = $@"
                    -- Revenue Summary
                    SELECT
                        SUM(o.c_total_amount) AS GrossRevenue,
                        SUM(o.c_total_amount - o.c_tax_amount) AS NetRevenue,
                        SUM(o.c_tax_amount) AS TotalTax,
                        SUM(o.c_discount_amount) AS TotalDiscounts,
                        SUM(o.c_delivery_charges) AS DeliveryCharges,
                        SUM(CASE WHEN o.c_payment_status != 'Completed' THEN o.c_total_amount - ISNULL(pay.PaidAmount, 0) ELSE 0 END) AS PendingPayments
                    FROM {Table.SysOrders} o
                    OUTER APPLY (
                        SELECT SUM(ISNULL(p.c_paid_amount, p.c_amount)) AS PaidAmount
                        FROM {Table.SysOrderPayments} p
                        WHERE p.c_orderid = o.c_orderid
                          AND ISNULL(p.c_status, '') NOT IN ('Failed', 'Rejected', 'Cancelled')
                    ) pay
                    WHERE o.c_ownerid = @OwnerId
                        AND o.c_createddate >= @StartDate
                        AND o.c_createddate <= @EndDate;

                    -- Payment Method Breakdown
                    SELECT
                        c_payment_method AS PaymentMethod,
                        SUM(ISNULL(c_paid_amount, c_amount)) AS Amount
                    FROM {Table.SysOrderPayments}
                    WHERE c_orderid IN (
                        SELECT c_orderid FROM {Table.SysOrders}
                        WHERE c_ownerid = @OwnerId
                            AND c_createddate >= @StartDate
                            AND c_createddate <= @EndDate
                    )
                    GROUP BY c_payment_method;

                    -- Payment Status Breakdown
                    SELECT
                        c_payment_status AS PaymentStatus,
                        SUM(c_total_amount) AS Amount
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId
                        AND c_createddate >= @StartDate
                        AND c_createddate <= @EndDate
                    GROUP BY c_payment_status;

                    -- Monthly Revenue
                    SELECT
                        FORMAT(c_createddate, 'MMM yyyy') AS Month,
                        YEAR(c_createddate) AS Year,
                        SUM(c_total_amount) AS GrossRevenue,
                        SUM(c_total_amount - c_tax_amount) AS NetRevenue,
                        SUM(c_tax_amount) AS TaxAmount,
                        SUM(c_discount_amount) AS DiscountAmount,
                        COUNT(*) AS OrderCount
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId
                        AND c_createddate >= @StartDate
                        AND c_createddate <= @EndDate
                    GROUP BY YEAR(c_createddate), MONTH(c_createddate), FORMAT(c_createddate, 'MMM yyyy')
                    ORDER BY YEAR(c_createddate), MONTH(c_createddate);

                    -- Revenue by Event Type
                    SELECT
                        c_event_type AS EventType,
                        SUM(c_total_amount) AS Revenue
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId
                        AND c_createddate >= @StartDate
                        AND c_createddate <= @EndDate
                    GROUP BY c_event_type;";

                var parameters = new[]
                {
                    new SqlParameter("@OwnerId", ownerId),
                    new SqlParameter("@StartDate", startDate),
                    new SqlParameter("@EndDate", endDate)
                };

                var dataSet = await Task.Run(() => _dbHelper.ExecuteDataSet(query, parameters));

                var report = new RevenueReportDto
                {
                    StartDate = startDate,
                    EndDate = endDate
                };

                // Revenue summary
                if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
                {
                    var row = dataSet.Tables[0].Rows[0];
                    report.GrossRevenue = row.GetValue<decimal>("GrossRevenue");
                    report.NetRevenue = row.GetValue<decimal>("NetRevenue");
                    report.TotalTax = row.GetValue<decimal>("TotalTax");
                    report.TotalDiscounts = row.GetValue<decimal>("TotalDiscounts");
                    report.DeliveryCharges = row.GetValue<decimal>("DeliveryCharges");
                    report.PendingPayments = row.GetValue<decimal>("PendingPayments");
                    report.ProfitMargin = report.GrossRevenue > 0
                        ? (report.NetRevenue / report.GrossRevenue) * 100
                        : 0;
                }

                // Payment method breakdown
                if (dataSet.Tables.Count > 1)
                {
                    foreach (DataRow row in dataSet.Tables[1].Rows)
                    {
                        report.PaymentMethodBreakdown[row.GetValue<string>("PaymentMethod", "Unknown")] = row.GetValue<decimal>("Amount");
                    }
                }

                // Payment status breakdown
                if (dataSet.Tables.Count > 2)
                {
                    foreach (DataRow row in dataSet.Tables[2].Rows)
                    {
                        report.PaymentStatusBreakdown[row.GetValue<string>("PaymentStatus", "Unknown")] = row.GetValue<decimal>("Amount");
                    }
                }

                // Monthly revenue
                if (dataSet.Tables.Count > 3)
                {
                    foreach (DataRow row in dataSet.Tables[3].Rows)
                    {
                        report.MonthlyRevenue.Add(new MonthlyRevenueDto
                        {
                            Month = row.GetValue<string>("Month"),
                            Year = row.GetValue<int>("Year"),
                            GrossRevenue = row.GetValue<decimal>("GrossRevenue"),
                            NetRevenue = row.GetValue<decimal>("NetRevenue"),
                            TaxAmount = row.GetValue<decimal>("TaxAmount"),
                            DiscountAmount = row.GetValue<decimal>("DiscountAmount"),
                            OrderCount = row.GetValue<int>("OrderCount")
                        });
                    }
                }

                // Revenue by event type
                if (dataSet.Tables.Count > 4)
                {
                    foreach (DataRow row in dataSet.Tables[4].Rows)
                    {
                        report.RevenueByEventType[row.GetValue<string>("EventType", "Unknown")] = row.GetValue<decimal>("Revenue");
                    }
                }

                return report;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating revenue report: {ex.Message}", ex);
            }
        }

        public async Task<CustomerReportDto> GenerateCustomerReport(long ownerId, ReportFilterDto filter)
        {
            try
            {
                var startDate = filter.StartDate ?? DateTime.Now.AddMonths(-1);
                var endDate = filter.EndDate ?? DateTime.Now;

                var query = $@"
                    -- Customer Summary
                    SELECT
                        COUNT(DISTINCT o.c_userid) AS TotalCustomers,
                        COUNT(DISTINCT CASE WHEN u.c_createddate >= @StartDate THEN o.c_userid END) AS NewCustomers,
                        COUNT(DISTINCT CASE WHEN EXISTS (
                            SELECT 1 FROM {Table.SysOrders} o2
                            WHERE o2.c_userid = o.c_userid AND o2.c_ownerid = @OwnerId
                            AND o2.c_createddate < @StartDate
                        ) THEN o.c_userid END) AS ReturningCustomers,
                        ISNULL(AVG(CustomerLifetime.LifetimeValue), 0) AS AverageLifetimeValue
                    FROM {Table.SysOrders} o
                    INNER JOIN {Table.SysUser} u ON o.c_userid = u.c_userid
                    LEFT JOIN (
                        SELECT c_userid, SUM(c_total_amount) AS LifetimeValue
                        FROM {Table.SysOrders}
                        WHERE c_ownerid = @OwnerId
                        GROUP BY c_userid
                    ) CustomerLifetime ON o.c_userid = CustomerLifetime.c_userid
                    WHERE o.c_ownerid = @OwnerId
                        AND o.c_createddate >= @StartDate
                        AND o.c_createddate <= @EndDate;

                    -- Customer Satisfaction
                    SELECT ISNULL(AVG(CAST(c_overall_rating AS DECIMAL(10,2))), 0) AS CustomerSatisfactionScore
                    FROM {Table.SysCateringReview} r
                    INNER JOIN {Table.SysOrders} o ON r.c_orderid = o.c_orderid
                    WHERE o.c_ownerid = @OwnerId
                        AND o.c_createddate >= @StartDate
                        AND o.c_createddate <= @EndDate;

                    -- Top Customers
                    SELECT TOP 10
                        u.c_userid AS CustomerId,
                        CONCAT(u.c_firstname, ' ', u.c_lastname) AS CustomerName,
                        u.c_email AS Email,
                        u.c_mobilenumber AS Phone,
                        COUNT(o.c_orderid) AS TotalOrders,
                        SUM(o.c_total_amount) AS LifetimeValue,
                        MAX(o.c_createddate) AS LastOrderDate
                    FROM {Table.SysUser} u
                    INNER JOIN {Table.SysOrders} o ON u.c_userid = o.c_userid
                    WHERE o.c_ownerid = @OwnerId
                        AND o.c_createddate >= @StartDate
                        AND o.c_createddate <= @EndDate
                    GROUP BY u.c_userid, u.c_firstname, u.c_lastname, u.c_email, u.c_mobilenumber
                    ORDER BY LifetimeValue DESC;

                    -- Customer Acquisition by Month
                    SELECT
                        FORMAT(o.c_createddate, 'MMM yyyy') AS Month,
                        COUNT(DISTINCT CASE WHEN NOT EXISTS (
                            SELECT 1 FROM {Table.SysOrders} o2
                            WHERE o2.c_userid = o.c_userid AND o2.c_ownerid = @OwnerId
                            AND o2.c_createddate < DATEADD(MONTH, DATEDIFF(MONTH, 0, o.c_createddate), 0)
                        ) THEN o.c_userid END) AS NewCustomers,
                        COUNT(DISTINCT CASE WHEN EXISTS (
                            SELECT 1 FROM {Table.SysOrders} o2
                            WHERE o2.c_userid = o.c_userid AND o2.c_ownerid = @OwnerId
                            AND o2.c_createddate < DATEADD(MONTH, DATEDIFF(MONTH, 0, o.c_createddate), 0)
                        ) THEN o.c_userid END) AS ReturningCustomers
                    FROM {Table.SysOrders} o
                    WHERE o.c_ownerid = @OwnerId
                        AND o.c_createddate >= @StartDate
                        AND o.c_createddate <= @EndDate
                    GROUP BY YEAR(o.c_createddate), MONTH(o.c_createddate), FORMAT(o.c_createddate, 'MMM yyyy')
                    ORDER BY YEAR(o.c_createddate), MONTH(o.c_createddate);";

                var parameters = new[]
                {
                    new SqlParameter("@OwnerId", ownerId),
                    new SqlParameter("@StartDate", startDate),
                    new SqlParameter("@EndDate", endDate)
                };

                var dataSet = await Task.Run(() => _dbHelper.ExecuteDataSet(query, parameters));

                var report = new CustomerReportDto
                {
                    StartDate = startDate,
                    EndDate = endDate
                };

                // Customer summary
                if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
                {
                    var row = dataSet.Tables[0].Rows[0];
                    report.TotalCustomers = row.GetValue<int>("TotalCustomers");
                    report.NewCustomers = row.GetValue<int>("NewCustomers");
                    report.ReturningCustomers = row.GetValue<int>("ReturningCustomers");
                    report.AverageLifetimeValue = row.GetValue<decimal>("AverageLifetimeValue");
                    report.CustomerRetentionRate = report.TotalCustomers > 0
                        ? (report.ReturningCustomers * 100.0m / report.TotalCustomers)
                        : 0;
                }

                // Customer satisfaction
                if (dataSet.Tables.Count > 1 && dataSet.Tables[1].Rows.Count > 0)
                {
                    report.CustomerSatisfactionScore = dataSet.Tables[1].Rows[0].GetValue<decimal>("CustomerSatisfactionScore");
                }

                // Top customers
                if (dataSet.Tables.Count > 2)
                {
                    foreach (DataRow row in dataSet.Tables[2].Rows)
                    {
                        report.TopCustomers.Add(new TopCustomerDto
                        {
                            CustomerId = row.GetValue<long>("CustomerId"),
                            CustomerName = row.GetValue<string>("CustomerName"),
                            Email = row.GetValue<string>("Email"),
                            Phone = row.GetValue<string>("Phone"),
                            TotalOrders = row.GetValue<int>("TotalOrders"),
                            LifetimeValue = row.GetValue<decimal>("LifetimeValue"),
                            LastOrderDate = row.GetValue<DateTime>("LastOrderDate")
                        });
                    }
                }

                // Customer acquisition
                if (dataSet.Tables.Count > 3)
                {
                    foreach (DataRow row in dataSet.Tables[3].Rows)
                    {
                        report.CustomerAcquisition.Add(new CustomerAcquisitionDto
                        {
                            Month = row.GetValue<string>("Month"),
                            NewCustomers = row.GetValue<int>("NewCustomers"),
                            ReturningCustomers = row.GetValue<int>("ReturningCustomers")
                        });
                    }
                }

                // Customer type distribution
                var newCount = report.NewCustomers;
                var regularCount = report.ReturningCustomers;
                var vipCount = report.TopCustomers.Count(c => c.TotalOrders >= 5 || c.LifetimeValue >= 50000);

                report.CustomersByType["New"] = newCount;
                report.CustomersByType["Regular"] = regularCount - vipCount;
                report.CustomersByType["VIP"] = vipCount;

                return report;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating customer report: {ex.Message}", ex);
            }
        }

        public async Task<MenuPerformanceReportDto> GenerateMenuPerformanceReport(long ownerId, ReportFilterDto filter)
        {
            try
            {
                var startDate = filter.StartDate ?? DateTime.Now.AddMonths(-1);
                var endDate = filter.EndDate ?? DateTime.Now;

                var query = $@"
                    -- Food Items & Package Performance
                    SELECT
                        f.c_foodid AS MenuItemId,
                        ISNULL(f.c_foodname, p.c_packagename) AS MenuItemName,
                        ISNULL(fc.c_categoryname, 'Package') AS Category,
                        COUNT(DISTINCT oi.c_orderid) AS OrderCount,
                        SUM(oi.c_quantity) AS TotalQuantitySold,
                        SUM(oi.c_item_total) AS TotalRevenue,
                        ISNULL(AVG(CAST(r.c_overall_rating AS DECIMAL(10,2))), 0) AS AverageRating,
                        f.c_price AS Price,
                        CASE
                            WHEN f.c_ispackage_item = 1 THEN 'Package'
                            ELSE 'Individual Item'
                        END AS ItemType
                    FROM {Table.SysFoodItems} f
                    LEFT JOIN {Table.SysFoodCategory} fc ON f.c_categoryid = fc.c_categoryid
                    LEFT JOIN {Table.SysMenuPackage} p ON f.c_ispackage_item = 1
                        AND EXISTS (
                            SELECT 1 FROM {Table.SysMenuPackageItems} pi
                            WHERE pi.c_packageid = p.c_packageid
                        )
                    LEFT JOIN {Table.SysOrderItems} oi ON f.c_foodid = oi.c_foodid
                    LEFT JOIN {Table.SysOrders} o ON oi.c_orderid = o.c_orderid
                    LEFT JOIN {Table.SysCateringReview} r ON o.c_orderid = r.c_orderid
                    WHERE f.c_ownerid = @OwnerId
                        AND f.c_is_deleted = 0
                        AND (o.c_orderid IS NULL OR (o.c_createddate >= @StartDate AND o.c_createddate <= @EndDate))
                    GROUP BY f.c_foodid, ISNULL(f.c_foodname, p.c_packagename),
                             ISNULL(fc.c_categoryname, 'Package'), f.c_price, f.c_ispackage_item;

                    -- Category Performance (Food Categories + Packages)
                    SELECT
                        ISNULL(fc.c_categoryname, 'Package') AS CategoryName,
                        COUNT(DISTINCT f.c_foodid) AS ItemCount,
                        COUNT(DISTINCT oi.c_orderid) AS TotalOrders,
                        SUM(oi.c_item_total) AS TotalRevenue,
                        ISNULL(AVG(CAST(r.c_overall_rating AS DECIMAL(10,2))), 0) AS AverageRating
                    FROM {Table.SysFoodItems} f
                    LEFT JOIN {Table.SysFoodCategory} fc ON f.c_categoryid = fc.c_categoryid
                    LEFT JOIN {Table.SysOrderItems} oi ON f.c_foodid = oi.c_foodid
                    LEFT JOIN {Table.SysOrders} o ON oi.c_orderid = o.c_orderid
                    LEFT JOIN {Table.SysCateringReview} r ON o.c_orderid = r.c_orderid
                    WHERE f.c_ownerid = @OwnerId
                        AND f.c_is_deleted = 0
                        AND (o.c_orderid IS NULL OR (o.c_createddate >= @StartDate AND o.c_createddate <= @EndDate))
                    GROUP BY ISNULL(fc.c_categoryname, 'Package');

                    -- Total Active Items (Food Items + Packages)
                    SELECT
                        COUNT(*) AS TotalItems,
                        SUM(CASE WHEN c_ispackage_item = 1 THEN 1 ELSE 0 END) AS TotalPackages,
                        SUM(CASE WHEN c_ispackage_item = 0 OR c_ispackage_item IS NULL THEN 1 ELSE 0 END) AS TotalIndividualItems
                    FROM {Table.SysFoodItems}
                    WHERE c_ownerid = @OwnerId
                        AND c_status = 1
                        AND c_is_deleted = 0;";

                var parameters = new[]
                {
                    new SqlParameter("@OwnerId", ownerId),
                    new SqlParameter("@StartDate", startDate),
                    new SqlParameter("@EndDate", endDate)
                };

                var dataSet = await Task.Run(() => _dbHelper.ExecuteDataSet(query, parameters));

                var report = new MenuPerformanceReportDto
                {
                    StartDate = startDate,
                    EndDate = endDate
                };

                var allItems = new List<MenuItemPerformanceDto>();
                decimal totalRevenue = 0;

                // Menu item performance
                if (dataSet.Tables.Count > 0)
                {
                    foreach (DataRow row in dataSet.Tables[0].Rows)
                    {
                        var revenue = row.GetValue<decimal>("TotalRevenue");
                        totalRevenue += revenue;

                        var item = new MenuItemPerformanceDto
                        {
                            MenuItemId = row.GetValue<long>("MenuItemId"),
                            MenuItemName = row.GetValue<string>("MenuItemName"),
                            Category = row.GetValue<string>("Category"),
                            OrderCount = row.GetValue<int>("OrderCount"),
                            TotalQuantitySold = row.GetValue<int>("TotalQuantitySold"),
                            TotalRevenue = revenue,
                            AverageRating = row.GetValue<decimal>("AverageRating"),
                            Price = row.GetValue<decimal>("Price")
                        };

                        allItems.Add(item);
                    }

                    // Calculate percentage and categorize
                    foreach (var item in allItems)
                    {
                        item.RevenuePercentage = totalRevenue > 0 ? (item.TotalRevenue / totalRevenue) * 100 : 0;

                        if (item.TotalRevenue > 0 && item.RevenuePercentage >= 5)
                            item.PerformanceCategory = "Hot";
                        else if (item.TotalRevenue == 0)
                            item.PerformanceCategory = "Cold";
                        else
                            item.PerformanceCategory = "Average";
                    }

                    report.TopItems = allItems.OrderByDescending(i => i.TotalRevenue).Take(10).ToList();
                    report.LowPerformingItems = allItems.Where(i => i.PerformanceCategory == "Cold").Take(10).ToList();
                    report.TotalMenuRevenue = totalRevenue;
                }

                // Category performance
                if (dataSet.Tables.Count > 1)
                {
                    foreach (DataRow row in dataSet.Tables[1].Rows)
                    {
                        var categoryRevenue = row.GetValue<decimal>("TotalRevenue");
                        report.CategoryPerformance[row.GetValue<string>("CategoryName", "Uncategorized")] = new CategoryPerformanceDto
                        {
                            CategoryName = row.GetValue<string>("CategoryName", "Uncategorized"),
                            ItemCount = row.GetValue<int>("ItemCount"),
                            TotalOrders = row.GetValue<int>("TotalOrders"),
                            TotalRevenue = categoryRevenue,
                            AverageRating = row.GetValue<decimal>("AverageRating"),
                            RevenuePercentage = totalRevenue > 0 ? (categoryRevenue / totalRevenue) * 100 : 0
                        };
                    }
                }

                // Total items (Food Items + Packages)
                if (dataSet.Tables.Count > 2 && dataSet.Tables[2].Rows.Count > 0)
                {
                    var statsRow = dataSet.Tables[2].Rows[0];
                    report.TotalMenuItems = statsRow.GetValue<int>("TotalItems");
                    report.ActiveItems = report.TotalMenuItems;

                    // Additional package vs individual item stats
                    var totalPackages = statsRow.GetValue<int>("TotalPackages");
                    var totalIndividualItems = statsRow.GetValue<int>("TotalIndividualItems");
                }

                // Generate recommendations
                report.Recommendations = GenerateMenuRecommendations(report);

                return report;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating menu performance report: {ex.Message}", ex);
            }
        }

        public async Task<FinancialReportDto> GenerateFinancialReport(long ownerId, ReportFilterDto filter)
        {
            try
            {
                var startDate = filter.StartDate ?? DateTime.Now.AddMonths(-1);
                var endDate = filter.EndDate ?? DateTime.Now;

                var query = $@"
                    -- Income Summary
                    SELECT
                        SUM(c_total_amount) AS TotalIncome,
                        SUM(c_subtotal) AS FoodRevenue,
                        0 AS DecorationRevenue,
                        0 AS StaffRevenue,
                        SUM(c_delivery_charges) AS OtherRevenue
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId
                        AND c_createddate >= @StartDate
                        AND c_createddate <= @EndDate;

                    -- Outstanding Payments
                    SELECT
                        o.c_orderid AS OrderId,
                        o.c_order_number AS OrderNumber,
                        CONCAT(u.c_firstname, ' ', u.c_lastname) AS CustomerName,
                        o.c_event_date AS EventDate,
                        o.c_total_amount AS TotalAmount,
                        ISNULL(pay.PaidAmount, 0) AS PaidAmount,
                        (o.c_total_amount - ISNULL(pay.PaidAmount, 0)) AS BalanceAmount,
                        DATEDIFF(DAY, o.c_event_date, GETDATE()) AS DaysOverdue
                    FROM {Table.SysOrders} o
                    INNER JOIN {Table.SysUser} u ON o.c_userid = u.c_userid
                    OUTER APPLY (
                        SELECT SUM(ISNULL(p.c_paid_amount, p.c_amount)) AS PaidAmount
                        FROM {Table.SysOrderPayments} p
                        WHERE p.c_orderid = o.c_orderid
                          AND ISNULL(p.c_status, '') NOT IN ('Failed', 'Rejected', 'Cancelled')
                    ) pay
                    WHERE o.c_ownerid = @OwnerId
                        AND o.c_payment_status != 'Completed'
                        AND (o.c_total_amount - ISNULL(pay.PaidAmount, 0)) > 0
                    ORDER BY DaysOverdue DESC;";

                var parameters = new[]
                {
                    new SqlParameter("@OwnerId", ownerId),
                    new SqlParameter("@StartDate", startDate),
                    new SqlParameter("@EndDate", endDate)
                };

                var dataSet = await Task.Run(() => _dbHelper.ExecuteDataSet(query, parameters));

                var report = new FinancialReportDto
                {
                    StartDate = startDate,
                    EndDate = endDate
                };

                // Income summary
                if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
                {
                    var row = dataSet.Tables[0].Rows[0];
                    report.TotalIncome = row.GetValue<decimal>("TotalIncome");
                    report.FoodRevenue = row.GetValue<decimal>("FoodRevenue");
                    report.DecorationRevenue = row.GetValue<decimal>("DecorationRevenue");
                    report.StaffRevenue = row.GetValue<decimal>("StaffRevenue");
                    report.OtherRevenue = row.GetValue<decimal>("OtherRevenue");
                    report.NetProfit = report.TotalIncome - report.TotalExpenses;
                    report.ProfitMargin = report.TotalIncome > 0
                        ? (report.NetProfit / report.TotalIncome) * 100
                        : 0;
                }

                // Outstanding payments
                if (dataSet.Tables.Count > 1)
                {
                    foreach (DataRow row in dataSet.Tables[1].Rows)
                    {
                        var outstanding = new OutstandingPaymentDto
                        {
                            OrderId = row.GetValue<long>("OrderId"),
                            OrderNumber = row.GetValue<string>("OrderNumber"),
                            CustomerName = row.GetValue<string>("CustomerName"),
                            EventDate = row.GetValue<DateTime>("EventDate"),
                            TotalAmount = row.GetValue<decimal>("TotalAmount"),
                            PaidAmount = row.GetValue<decimal>("PaidAmount"),
                            BalanceAmount = row.GetValue<decimal>("BalanceAmount"),
                            DaysOverdue = row.GetValue<int>("DaysOverdue")
                        };

                        report.OutstandingPayments.Add(outstanding);
                        report.OutstandingReceivables += outstanding.BalanceAmount;
                    }
                }

                return report;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating financial report: {ex.Message}", ex);
            }
        }

        public async Task<byte[]> ExportReportToCSV(long ownerId, string reportType, ReportFilterDto filter)
        {
            try
            {
                var csv = new StringBuilder();

                switch (reportType.ToLower())
                {
                    case "sales":
                        var salesReport = await GenerateSalesReport(ownerId, filter);
                        csv.AppendLine("Sales Report");
                        csv.AppendLine($"Period: {salesReport.StartDate:yyyy-MM-dd} to {salesReport.EndDate:yyyy-MM-dd}");
                        csv.AppendLine();
                        csv.AppendLine("Metric,Value");
                        csv.AppendLine($"Total Orders,{salesReport.TotalOrders}");
                        csv.AppendLine($"Completed Orders,{salesReport.CompletedOrders}");
                        csv.AppendLine($"Pending Orders,{salesReport.PendingOrders}");
                        csv.AppendLine($"Cancelled Orders,{salesReport.CancelledOrders}");
                        csv.AppendLine($"Total Revenue,{salesReport.TotalRevenue:C}");
                        csv.AppendLine($"Average Order Value,{salesReport.AverageOrderValue:C}");
                        csv.AppendLine($"Total Guests Served,{salesReport.TotalGuestsServed}");
                        break;

                    case "revenue":
                        var revenueReport = await GenerateRevenueReport(ownerId, filter);
                        csv.AppendLine("Revenue Report");
                        csv.AppendLine($"Period: {revenueReport.StartDate:yyyy-MM-dd} to {revenueReport.EndDate:yyyy-MM-dd}");
                        csv.AppendLine();
                        csv.AppendLine("Metric,Value");
                        csv.AppendLine($"Gross Revenue,{revenueReport.GrossRevenue:C}");
                        csv.AppendLine($"Net Revenue,{revenueReport.NetRevenue:C}");
                        csv.AppendLine($"Total Tax,{revenueReport.TotalTax:C}");
                        csv.AppendLine($"Total Discounts,{revenueReport.TotalDiscounts:C}");
                        csv.AppendLine($"Delivery Charges,{revenueReport.DeliveryCharges:C}");
                        csv.AppendLine($"Pending Payments,{revenueReport.PendingPayments:C}");
                        break;

                    case "customer":
                        var customerReport = await GenerateCustomerReport(ownerId, filter);
                        csv.AppendLine("Customer Report");
                        csv.AppendLine($"Period: {customerReport.StartDate:yyyy-MM-dd} to {customerReport.EndDate:yyyy-MM-dd}");
                        csv.AppendLine();
                        csv.AppendLine("Metric,Value");
                        csv.AppendLine($"Total Customers,{customerReport.TotalCustomers}");
                        csv.AppendLine($"New Customers,{customerReport.NewCustomers}");
                        csv.AppendLine($"Returning Customers,{customerReport.ReturningCustomers}");
                        csv.AppendLine($"Customer Retention Rate,{customerReport.CustomerRetentionRate:F2}%");
                        csv.AppendLine($"Average Lifetime Value,{customerReport.AverageLifetimeValue:C}");
                        csv.AppendLine($"Customer Satisfaction Score,{customerReport.CustomerSatisfactionScore:F2}");
                        break;

                    case "menu":
                        var menuReport = await GenerateMenuPerformanceReport(ownerId, filter);
                        csv.AppendLine("Menu Performance Report");
                        csv.AppendLine($"Period: {menuReport.StartDate:yyyy-MM-dd} to {menuReport.EndDate:yyyy-MM-dd}");
                        csv.AppendLine();
                        csv.AppendLine("Item Name,Category,Order Count,Quantity Sold,Total Revenue,Average Rating");
                        foreach (var item in menuReport.TopItems)
                        {
                            csv.AppendLine($"{item.MenuItemName},{item.Category},{item.OrderCount},{item.TotalQuantitySold},{item.TotalRevenue:C},{item.AverageRating:F2}");
                        }
                        break;
                }

                return Encoding.UTF8.GetBytes(csv.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception($"Error exporting report to CSV: {ex.Message}", ex);
            }
        }

        public async Task<byte[]> ExportReportToPDF(long ownerId, string reportType, ReportFilterDto filter)
        {
            // PDF generation would require a library like iTextSharp or similar
            // For now, return empty byte array - can be implemented later
            await Task.CompletedTask;
            throw new NotImplementedException("PDF export will be implemented in a future update.");
        }

        private decimal CalculatePercentageChange(decimal current, decimal previous)
        {
            if (previous == 0)
                return current > 0 ? 100 : 0;

            return Math.Round(((current - previous) / previous) * 100, 2);
        }

        private List<string> GenerateMenuRecommendations(MenuPerformanceReportDto report)
        {
            var recommendations = new List<string>();

            if (report.LowPerformingItems.Count > 0)
            {
                recommendations.Add($"Consider removing or revamping {report.LowPerformingItems.Count} low-performing menu items to optimize your menu.");
            }

            if (report.TopItems.Count > 0)
            {
                var topItem = report.TopItems.First();
                recommendations.Add($"'{topItem.MenuItemName}' is your best performer with ₹{topItem.TotalRevenue:N0} in revenue. Consider creating variations or promoting it more.");
            }

            var lowRatedItems = report.TopItems.Where(i => i.AverageRating < 3.5m).ToList();
            if (lowRatedItems.Count > 0)
            {
                recommendations.Add($"{lowRatedItems.Count} popular items have low ratings. Focus on quality improvement for these items.");
            }

            return recommendations;
        }
    }
}

