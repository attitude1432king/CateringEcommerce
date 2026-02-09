using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
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
                        ISNULL(SUM(c_final_amount), 0) AS TotalRevenue,
                        ISNULL(AVG(c_final_amount), 0) AS AverageOrderValue,
                        SUM(c_guest_count) AS TotalGuestsServed
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId
                        AND c_created_date >= @StartDate
                        AND c_created_date <= @EndDate;

                    -- Event Type Breakdown
                    SELECT
                        c_event_type AS EventType,
                        COUNT(*) AS OrderCount,
                        SUM(c_final_amount) AS TotalRevenue,
                        AVG(c_final_amount) AS AverageOrderValue
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId
                        AND c_created_date >= @StartDate
                        AND c_created_date <= @EndDate
                    GROUP BY c_event_type;

                    -- Time Series Data
                    SELECT
                        FORMAT(c_created_date, 'MMM yyyy') AS Period,
                        COUNT(*) AS OrderCount,
                        SUM(c_final_amount) AS Revenue,
                        SUM(c_guest_count) AS GuestsServed
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId
                        AND c_created_date >= @StartDate
                        AND c_created_date <= @EndDate
                    GROUP BY YEAR(c_created_date), MONTH(c_created_date), FORMAT(c_created_date, 'MMM yyyy')
                    ORDER BY YEAR(c_created_date), MONTH(c_created_date);

                    -- Comparison with previous period
                    DECLARE @PreviousStartDate DATE = DATEADD(DAY, -DATEDIFF(DAY, @StartDate, @EndDate), @StartDate);
                    DECLARE @PreviousEndDate DATE = @StartDate;

                    SELECT
                        ISNULL(SUM(c_final_amount), 0) AS PreviousRevenue,
                        COUNT(*) AS PreviousOrders
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId
                        AND c_created_date >= @PreviousStartDate
                        AND c_created_date < @PreviousEndDate;";

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
                    report.TotalOrders = Convert.ToInt32(row["TotalOrders"]);
                    report.CompletedOrders = Convert.ToInt32(row["CompletedOrders"]);
                    report.PendingOrders = Convert.ToInt32(row["PendingOrders"]);
                    report.CancelledOrders = Convert.ToInt32(row["CancelledOrders"]);
                    report.TotalRevenue = Convert.ToDecimal(row["TotalRevenue"]);
                    report.AverageOrderValue = Convert.ToDecimal(row["AverageOrderValue"]);
                    report.TotalGuestsServed = Convert.ToInt32(row["TotalGuestsServed"]);
                }

                // Event type breakdown
                if (dataSet.Tables.Count > 1)
                {
                    var totalRevenue = report.TotalRevenue;
                    foreach (DataRow row in dataSet.Tables[1].Rows)
                    {
                        var eventType = row["EventType"].ToString();
                        var revenue = Convert.ToDecimal(row["TotalRevenue"]);
                        report.EventTypeBreakdown[eventType] = new SalesBreakdownDto
                        {
                            OrderCount = Convert.ToInt32(row["OrderCount"]),
                            TotalRevenue = revenue,
                            AverageOrderValue = Convert.ToDecimal(row["AverageOrderValue"]),
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
                            Period = row["Period"].ToString(),
                            OrderCount = Convert.ToInt32(row["OrderCount"]),
                            Revenue = Convert.ToDecimal(row["Revenue"]),
                            GuestsServed = Convert.ToInt32(row["GuestsServed"])
                        });
                    }
                }

                // Growth comparison
                if (dataSet.Tables.Count > 3 && dataSet.Tables[3].Rows.Count > 0)
                {
                    var row = dataSet.Tables[3].Rows[0];
                    var previousRevenue = Convert.ToDecimal(row["PreviousRevenue"]);
                    var previousOrders = Convert.ToInt32(row["PreviousOrders"]);

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
                        SUM(c_final_amount) AS GrossRevenue,
                        SUM(c_final_amount - c_tax_amount) AS NetRevenue,
                        SUM(c_tax_amount) AS TotalTax,
                        SUM(c_discount_amount) AS TotalDiscounts,
                        SUM(c_delivery_charges) AS DeliveryCharges,
                        SUM(CASE WHEN c_payment_status != 'Completed' THEN c_final_amount - ISNULL(c_paid_amount, 0) ELSE 0 END) AS PendingPayments
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId
                        AND c_created_date >= @StartDate
                        AND c_created_date <= @EndDate;

                    -- Payment Method Breakdown
                    SELECT
                        c_payment_method AS PaymentMethod,
                        SUM(c_paid_amount) AS Amount
                    FROM {Table.SysOrderPayments}
                    WHERE c_orderid IN (
                        SELECT c_orderid FROM {Table.SysOrders}
                        WHERE c_ownerid = @OwnerId
                            AND c_created_date >= @StartDate
                            AND c_created_date <= @EndDate
                    )
                    GROUP BY c_payment_method;

                    -- Payment Status Breakdown
                    SELECT
                        c_payment_status AS PaymentStatus,
                        SUM(c_final_amount) AS Amount
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId
                        AND c_created_date >= @StartDate
                        AND c_created_date <= @EndDate
                    GROUP BY c_payment_status;

                    -- Monthly Revenue
                    SELECT
                        FORMAT(c_created_date, 'MMM yyyy') AS Month,
                        YEAR(c_created_date) AS Year,
                        SUM(c_final_amount) AS GrossRevenue,
                        SUM(c_final_amount - c_tax_amount) AS NetRevenue,
                        SUM(c_tax_amount) AS TaxAmount,
                        SUM(c_discount_amount) AS DiscountAmount,
                        COUNT(*) AS OrderCount
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId
                        AND c_created_date >= @StartDate
                        AND c_created_date <= @EndDate
                    GROUP BY YEAR(c_created_date), MONTH(c_created_date), FORMAT(c_created_date, 'MMM yyyy')
                    ORDER BY YEAR(c_created_date), MONTH(c_created_date);

                    -- Revenue by Event Type
                    SELECT
                        c_event_type AS EventType,
                        SUM(c_final_amount) AS Revenue
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId
                        AND c_created_date >= @StartDate
                        AND c_created_date <= @EndDate
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
                    report.GrossRevenue = Convert.ToDecimal(row["GrossRevenue"]);
                    report.NetRevenue = Convert.ToDecimal(row["NetRevenue"]);
                    report.TotalTax = Convert.ToDecimal(row["TotalTax"]);
                    report.TotalDiscounts = Convert.ToDecimal(row["TotalDiscounts"]);
                    report.DeliveryCharges = Convert.ToDecimal(row["DeliveryCharges"]);
                    report.PendingPayments = Convert.ToDecimal(row["PendingPayments"]);
                    report.ProfitMargin = report.GrossRevenue > 0
                        ? (report.NetRevenue / report.GrossRevenue) * 100
                        : 0;
                }

                // Payment method breakdown
                if (dataSet.Tables.Count > 1)
                {
                    foreach (DataRow row in dataSet.Tables[1].Rows)
                    {
                        report.PaymentMethodBreakdown[row["PaymentMethod"].ToString()] = Convert.ToDecimal(row["Amount"]);
                    }
                }

                // Payment status breakdown
                if (dataSet.Tables.Count > 2)
                {
                    foreach (DataRow row in dataSet.Tables[2].Rows)
                    {
                        report.PaymentStatusBreakdown[row["PaymentStatus"].ToString()] = Convert.ToDecimal(row["Amount"]);
                    }
                }

                // Monthly revenue
                if (dataSet.Tables.Count > 3)
                {
                    foreach (DataRow row in dataSet.Tables[3].Rows)
                    {
                        report.MonthlyRevenue.Add(new MonthlyRevenueDto
                        {
                            Month = row["Month"].ToString(),
                            Year = Convert.ToInt32(row["Year"]),
                            GrossRevenue = Convert.ToDecimal(row["GrossRevenue"]),
                            NetRevenue = Convert.ToDecimal(row["NetRevenue"]),
                            TaxAmount = Convert.ToDecimal(row["TaxAmount"]),
                            DiscountAmount = Convert.ToDecimal(row["DiscountAmount"]),
                            OrderCount = Convert.ToInt32(row["OrderCount"])
                        });
                    }
                }

                // Revenue by event type
                if (dataSet.Tables.Count > 4)
                {
                    foreach (DataRow row in dataSet.Tables[4].Rows)
                    {
                        report.RevenueByEventType[row["EventType"].ToString()] = Convert.ToDecimal(row["Revenue"]);
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
                        COUNT(DISTINCT CASE WHEN u.c_created_date >= @StartDate THEN o.c_userid END) AS NewCustomers,
                        COUNT(DISTINCT CASE WHEN EXISTS (
                            SELECT 1 FROM {Table.SysOrders} o2
                            WHERE o2.c_userid = o.c_userid AND o2.c_ownerid = @OwnerId
                            AND o2.c_created_date < @StartDate
                        ) THEN o.c_userid END) AS ReturningCustomers,
                        ISNULL(AVG(CustomerLifetime.LifetimeValue), 0) AS AverageLifetimeValue
                    FROM {Table.SysOrders} o
                    INNER JOIN {Table.SysUser} u ON o.c_userid = u.c_userid
                    LEFT JOIN (
                        SELECT c_userid, SUM(c_final_amount) AS LifetimeValue
                        FROM {Table.SysOrders}
                        WHERE c_ownerid = @OwnerId
                        GROUP BY c_userid
                    ) CustomerLifetime ON o.c_userid = CustomerLifetime.c_userid
                    WHERE o.c_ownerid = @OwnerId
                        AND o.c_created_date >= @StartDate
                        AND o.c_created_date <= @EndDate;

                    -- Customer Satisfaction
                    SELECT ISNULL(AVG(CAST(c_rating AS DECIMAL(10,2))), 0) AS CustomerSatisfactionScore
                    FROM {Table.SysCateringReview} r
                    INNER JOIN {Table.SysOrders} o ON r.c_orderid = o.c_orderid
                    WHERE o.c_ownerid = @OwnerId
                        AND o.c_created_date >= @StartDate
                        AND o.c_created_date <= @EndDate;

                    -- Top Customers
                    SELECT TOP 10
                        u.c_userid AS CustomerId,
                        CONCAT(u.c_firstname, ' ', u.c_lastname) AS CustomerName,
                        u.c_email AS Email,
                        u.c_mobilenumber AS Phone,
                        COUNT(o.c_orderid) AS TotalOrders,
                        SUM(o.c_final_amount) AS LifetimeValue,
                        MAX(o.c_created_date) AS LastOrderDate
                    FROM {Table.SysUser} u
                    INNER JOIN {Table.SysOrders} o ON u.c_userid = o.c_userid
                    WHERE o.c_ownerid = @OwnerId
                        AND o.c_created_date >= @StartDate
                        AND o.c_created_date <= @EndDate
                    GROUP BY u.c_userid, u.c_firstname, u.c_lastname, u.c_email, u.c_mobilenumber
                    ORDER BY LifetimeValue DESC;

                    -- Customer Acquisition by Month
                    SELECT
                        FORMAT(o.c_created_date, 'MMM yyyy') AS Month,
                        COUNT(DISTINCT CASE WHEN NOT EXISTS (
                            SELECT 1 FROM {Table.SysOrders} o2
                            WHERE o2.c_userid = o.c_userid AND o2.c_ownerid = @OwnerId
                            AND o2.c_created_date < DATEADD(MONTH, DATEDIFF(MONTH, 0, o.c_created_date), 0)
                        ) THEN o.c_userid END) AS NewCustomers,
                        COUNT(DISTINCT CASE WHEN EXISTS (
                            SELECT 1 FROM {Table.SysOrders} o2
                            WHERE o2.c_userid = o.c_userid AND o2.c_ownerid = @OwnerId
                            AND o2.c_created_date < DATEADD(MONTH, DATEDIFF(MONTH, 0, o.c_created_date), 0)
                        ) THEN o.c_userid END) AS ReturningCustomers
                    FROM {Table.SysOrders} o
                    WHERE o.c_ownerid = @OwnerId
                        AND o.c_created_date >= @StartDate
                        AND o.c_created_date <= @EndDate
                    GROUP BY YEAR(o.c_created_date), MONTH(o.c_created_date), FORMAT(o.c_created_date, 'MMM yyyy')
                    ORDER BY YEAR(o.c_created_date), MONTH(o.c_created_date);";

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
                    report.TotalCustomers = Convert.ToInt32(row["TotalCustomers"]);
                    report.NewCustomers = Convert.ToInt32(row["NewCustomers"]);
                    report.ReturningCustomers = Convert.ToInt32(row["ReturningCustomers"]);
                    report.AverageLifetimeValue = Convert.ToDecimal(row["AverageLifetimeValue"]);
                    report.CustomerRetentionRate = report.TotalCustomers > 0
                        ? (report.ReturningCustomers * 100.0m / report.TotalCustomers)
                        : 0;
                }

                // Customer satisfaction
                if (dataSet.Tables.Count > 1 && dataSet.Tables[1].Rows.Count > 0)
                {
                    report.CustomerSatisfactionScore = Convert.ToDecimal(dataSet.Tables[1].Rows[0]["CustomerSatisfactionScore"]);
                }

                // Top customers
                if (dataSet.Tables.Count > 2)
                {
                    foreach (DataRow row in dataSet.Tables[2].Rows)
                    {
                        report.TopCustomers.Add(new TopCustomerDto
                        {
                            CustomerId = Convert.ToInt64(row["CustomerId"]),
                            CustomerName = row["CustomerName"].ToString(),
                            Email = row["Email"].ToString(),
                            Phone = row["Phone"].ToString(),
                            TotalOrders = Convert.ToInt32(row["TotalOrders"]),
                            LifetimeValue = Convert.ToDecimal(row["LifetimeValue"]),
                            LastOrderDate = Convert.ToDateTime(row["LastOrderDate"])
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
                            Month = row["Month"].ToString(),
                            NewCustomers = Convert.ToInt32(row["NewCustomers"]),
                            ReturningCustomers = Convert.ToInt32(row["ReturningCustomers"])
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
                        ISNULL(AVG(CAST(r.c_rating AS DECIMAL(10,2))), 0) AS AverageRating,
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
                        AND (o.c_orderid IS NULL OR (o.c_created_date >= @StartDate AND o.c_created_date <= @EndDate))
                    GROUP BY f.c_foodid, ISNULL(f.c_foodname, p.c_packagename),
                             ISNULL(fc.c_categoryname, 'Package'), f.c_price, f.c_ispackage_item;

                    -- Category Performance (Food Categories + Packages)
                    SELECT
                        ISNULL(fc.c_categoryname, 'Package') AS CategoryName,
                        COUNT(DISTINCT f.c_foodid) AS ItemCount,
                        COUNT(DISTINCT oi.c_orderid) AS TotalOrders,
                        SUM(oi.c_item_total) AS TotalRevenue,
                        ISNULL(AVG(CAST(r.c_rating AS DECIMAL(10,2))), 0) AS AverageRating
                    FROM {Table.SysFoodItems} f
                    LEFT JOIN {Table.SysFoodCategory} fc ON f.c_categoryid = fc.c_categoryid
                    LEFT JOIN {Table.SysOrderItems} oi ON f.c_foodid = oi.c_foodid
                    LEFT JOIN {Table.SysOrders} o ON oi.c_orderid = o.c_orderid
                    LEFT JOIN {Table.SysCateringReview} r ON o.c_orderid = r.c_orderid
                    WHERE f.c_ownerid = @OwnerId
                        AND f.c_is_deleted = 0
                        AND (o.c_orderid IS NULL OR (o.c_created_date >= @StartDate AND o.c_created_date <= @EndDate))
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
                        var revenue = Convert.ToDecimal(row["TotalRevenue"]);
                        totalRevenue += revenue;

                        var item = new MenuItemPerformanceDto
                        {
                            MenuItemId = Convert.ToInt64(row["MenuItemId"]),
                            MenuItemName = row["MenuItemName"].ToString(),
                            Category = row["Category"].ToString(),
                            OrderCount = Convert.ToInt32(row["OrderCount"]),
                            TotalQuantitySold = Convert.ToInt32(row["TotalQuantitySold"]),
                            TotalRevenue = revenue,
                            AverageRating = Convert.ToDecimal(row["AverageRating"]),
                            Price = Convert.ToDecimal(row["Price"])
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
                        var categoryRevenue = Convert.ToDecimal(row["TotalRevenue"]);
                        report.CategoryPerformance[row["CategoryName"].ToString()] = new CategoryPerformanceDto
                        {
                            CategoryName = row["CategoryName"].ToString(),
                            ItemCount = Convert.ToInt32(row["ItemCount"]),
                            TotalOrders = Convert.ToInt32(row["TotalOrders"]),
                            TotalRevenue = categoryRevenue,
                            AverageRating = Convert.ToDecimal(row["AverageRating"]),
                            RevenuePercentage = totalRevenue > 0 ? (categoryRevenue / totalRevenue) * 100 : 0
                        };
                    }
                }

                // Total items (Food Items + Packages)
                if (dataSet.Tables.Count > 2 && dataSet.Tables[2].Rows.Count > 0)
                {
                    var statsRow = dataSet.Tables[2].Rows[0];
                    report.TotalMenuItems = Convert.ToInt32(statsRow["TotalItems"]);
                    report.ActiveItems = report.TotalMenuItems;

                    // Additional package vs individual item stats
                    var totalPackages = Convert.ToInt32(statsRow["TotalPackages"]);
                    var totalIndividualItems = Convert.ToInt32(statsRow["TotalIndividualItems"]);
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
                        SUM(c_final_amount) AS TotalIncome,
                        SUM(c_subtotal) AS FoodRevenue,
                        0 AS DecorationRevenue,
                        0 AS StaffRevenue,
                        SUM(c_delivery_charges) AS OtherRevenue
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId
                        AND c_created_date >= @StartDate
                        AND c_created_date <= @EndDate;

                    -- Outstanding Payments
                    SELECT
                        o.c_orderid AS OrderId,
                        o.c_order_number AS OrderNumber,
                        CONCAT(u.c_firstname, ' ', u.c_lastname) AS CustomerName,
                        o.c_event_date AS EventDate,
                        o.c_final_amount AS TotalAmount,
                        ISNULL(o.c_paid_amount, 0) AS PaidAmount,
                        (o.c_final_amount - ISNULL(o.c_paid_amount, 0)) AS BalanceAmount,
                        DATEDIFF(DAY, o.c_event_date, GETDATE()) AS DaysOverdue
                    FROM {Table.SysOrders} o
                    INNER JOIN {Table.SysUser} u ON o.c_userid = u.c_userid
                    WHERE o.c_ownerid = @OwnerId
                        AND o.c_payment_status != 'Completed'
                        AND (o.c_final_amount - ISNULL(o.c_paid_amount, 0)) > 0
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
                    report.TotalIncome = Convert.ToDecimal(row["TotalIncome"]);
                    report.FoodRevenue = Convert.ToDecimal(row["FoodRevenue"]);
                    report.DecorationRevenue = Convert.ToDecimal(row["DecorationRevenue"]);
                    report.StaffRevenue = Convert.ToDecimal(row["StaffRevenue"]);
                    report.OtherRevenue = Convert.ToDecimal(row["OtherRevenue"]);
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
                            OrderId = Convert.ToInt64(row["OrderId"]),
                            OrderNumber = row["OrderNumber"].ToString(),
                            CustomerName = row["CustomerName"].ToString(),
                            EventDate = Convert.ToDateTime(row["EventDate"]),
                            TotalAmount = Convert.ToDecimal(row["TotalAmount"]),
                            PaidAmount = Convert.ToDecimal(row["PaidAmount"]),
                            BalanceAmount = Convert.ToDecimal(row["BalanceAmount"]),
                            DaysOverdue = Convert.ToInt32(row["DaysOverdue"])
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
