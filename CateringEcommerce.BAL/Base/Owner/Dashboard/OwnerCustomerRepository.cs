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
    public class OwnerCustomerRepository : IOwnerCustomerRepository
    {
        private readonly IDatabaseHelper _dbHelper;
        public OwnerCustomerRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<PaginatedCustomersDto> GetCustomersList(long ownerId, CustomerFilterDto filter)
        {
            try
            {
                var query = new StringBuilder();
                var countQuery = new StringBuilder();

                // Base CTE for customer analytics
                var baseCTE = $@"
                    WITH CustomerAnalytics AS (
                        SELECT
                            u.c_userid AS CustomerId,
                            CONCAT(u.c_firstname, ' ', u.c_lastname) AS CustomerName,
                            u.c_email AS Email,
                            u.c_mobilenumber AS Phone,
                            u.c_createddate AS RegisteredDate,
                            COUNT(o.c_orderid) AS TotalOrders,
                            ISNULL(SUM(o.c_total_amount), 0) AS LifetimeValue,
                            MAX(o.c_createddate) AS LastOrderDate,
                            ISNULL(AVG(o.c_total_amount), 0) AS AverageOrderValue,
                            (SELECT TOP 1 c_event_type
                             FROM {Table.SysOrders}
                             WHERE c_userid = u.c_userid AND c_ownerid = @OwnerId
                             GROUP BY c_event_type
                             ORDER BY COUNT(*) DESC) AS PreferredEventType,
                            CASE
                                WHEN COUNT(o.c_orderid) = 1 THEN 'New'
                                WHEN COUNT(o.c_orderid) >= 5 OR SUM(o.c_total_amount) >= 50000 THEN 'VIP'
                                ELSE 'Regular'
                            END AS CustomerType
                        FROM {Table.SysUser} u
                        INNER JOIN {Table.SysOrders} o ON u.c_userid = o.c_userid
                        WHERE o.c_ownerid = @OwnerId
                        GROUP BY u.c_userid, u.c_firstname, u.c_lastname, u.c_email,
                                 u.c_mobilenumber, u.c_createddate
                    )";

                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@OwnerId", ownerId)
                };

                var whereClause = new StringBuilder(" WHERE 1=1");

                // Apply filters
                if (!string.IsNullOrEmpty(filter.CustomerType) && filter.CustomerType.ToLower() != "all")
                {
                    whereClause.Append(" AND CustomerType = @CustomerType");
                    parameters.Add(new SqlParameter("@CustomerType", filter.CustomerType));
                }

                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    whereClause.Append(" AND (CustomerName LIKE @SearchTerm OR Email LIKE @SearchTerm OR Phone LIKE @SearchTerm)");
                    parameters.Add(new SqlParameter("@SearchTerm", $"%{filter.SearchTerm}%"));
                }

                if (filter.RegisteredAfter.HasValue)
                {
                    whereClause.Append(" AND RegisteredDate >= @RegisteredAfter");
                    parameters.Add(new SqlParameter("@RegisteredAfter", filter.RegisteredAfter.Value));
                }

                if (filter.MinLifetimeValue.HasValue)
                {
                    whereClause.Append(" AND LifetimeValue >= @MinLifetimeValue");
                    parameters.Add(new SqlParameter("@MinLifetimeValue", filter.MinLifetimeValue.Value));
                }

                // Count query
                countQuery.Append(baseCTE);
                countQuery.Append($"SELECT COUNT(*) AS TotalCount FROM CustomerAnalytics{whereClause}");

                // Data query with pagination
                query.Append(baseCTE);
                query.Append($@"
                    SELECT
                        CustomerId, CustomerName, Email, Phone, CustomerType,
                        TotalOrders, LifetimeValue, LastOrderDate, RegisteredDate,
                        AverageOrderValue, PreferredEventType
                    FROM CustomerAnalytics
                    {whereClause}
                    ORDER BY ");

                // Apply sorting
                switch (filter.SortBy?.ToLower())
                {
                    case "totalorders":
                        query.Append("TotalOrders");
                        break;
                    case "lifetimevalue":
                        query.Append("LifetimeValue");
                        break;
                    case "lastorderdate":
                    default:
                        query.Append("LastOrderDate");
                        break;
                }

                query.Append(filter.SortOrder?.ToUpper() == "ASC" ? " ASC" : " DESC");

                // Add pagination
                int offset = (filter.Page - 1) * filter.PageSize;
                query.Append($" OFFSET {offset} ROWS FETCH NEXT {filter.PageSize} ROWS ONLY");

                // Execute count query
                var totalCount = 0;
                var countResult = await Task.Run(() => _dbHelper.ExecuteScalar(countQuery.ToString(), parameters.ToArray()));
                if (countResult != null)
                {
                    totalCount = Convert.ToInt32(countResult);
                }

                // Execute data query
                var dataTable = await Task.Run(() => _dbHelper.ExecuteAsync(query.ToString(), parameters.ToArray()));

                var customers = new List<CustomerListDto>();
                foreach (DataRow row in dataTable.Rows)
                {
                    customers.Add(new CustomerListDto
                    {
                        CustomerId = Convert.ToInt64(row["CustomerId"]),
                        CustomerName = row["CustomerName"].ToString(),
                        Email = row["Email"].ToString(),
                        Phone = row["Phone"].ToString(),
                        CustomerType = row["CustomerType"].ToString(),
                        TotalOrders = Convert.ToInt32(row["TotalOrders"]),
                        LifetimeValue = Convert.ToDecimal(row["LifetimeValue"]),
                        LastOrderDate = row["LastOrderDate"] != DBNull.Value ? Convert.ToDateTime(row["LastOrderDate"]) : (DateTime?)null,
                        RegisteredDate = Convert.ToDateTime(row["RegisteredDate"]),
                        AverageOrderValue = Convert.ToDecimal(row["AverageOrderValue"]),
                        PreferredEventType = row["PreferredEventType"] != DBNull.Value ? row["PreferredEventType"].ToString() : ""
                    });
                }

                var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

                return new PaginatedCustomersDto
                {
                    Customers = customers,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalPages = totalPages,
                    HasNextPage = filter.Page < totalPages,
                    HasPreviousPage = filter.Page > 1
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting customers list: {ex.Message}", ex);
            }
        }

        public async Task<CustomerDetailDto> GetCustomerDetails(long ownerId, long customerId)
        {
            try
            {
                // Validate that customer has orders with this owner
                if (!await ValidateCustomerOwnership(ownerId, customerId))
                {
                    throw new UnauthorizedAccessException("Customer has no orders with this owner.");
                }

                var query = $@"
                    SELECT
                        u.c_userid AS CustomerId,
                        CONCAT(u.c_firstname, ' ', u.c_lastname) AS CustomerName,
                        u.c_email AS Email,
                        u.c_mobilenumber AS Phone,
                        u.c_createddate AS RegisteredDate,
                        u.c_address AS Address,
                        u.c_city AS City,
                        u.c_state AS State,
                        u.c_pincode AS Pincode
                    FROM {Table.SysUser} u
                    WHERE u.c_userid = @CustomerId;

                    -- Order Statistics
                    SELECT
                        COUNT(*) AS TotalOrders,
                        SUM(CASE WHEN o.c_order_status = 'Completed' THEN 1 ELSE 0 END) AS CompletedOrders,
                        SUM(CASE WHEN o.c_order_status = 'Cancelled' THEN 1 ELSE 0 END) AS CancelledOrders,
                        ISNULL(SUM(o.c_total_amount), 0) AS LifetimeValue,
                        ISNULL(AVG(o.c_total_amount), 0) AS AverageOrderValue,
                        ISNULL(SUM(o.c_total_amount), 0) AS TotalSpent,
                        ISNULL(SUM(o.c_total_amount - ISNULL(pay.PaidAmount, 0)), 0) AS OutstandingBalance,
                        MAX(o.c_createddate) AS LastOrderDate,
                        (SELECT TOP 1 c_order_status FROM {Table.SysOrders}
                         WHERE c_userid = @CustomerId AND c_ownerid = @OwnerId
                         ORDER BY c_createddate DESC) AS LastOrderStatus,
                        (SELECT TOP 1 c_event_type FROM {Table.SysOrders}
                         WHERE c_userid = @CustomerId AND c_ownerid = @OwnerId
                         GROUP BY c_event_type ORDER BY COUNT(*) DESC) AS PreferredEventType,
                        ISNULL(AVG(o.c_guest_count), 0) AS AverageGuestCount,
                        MIN(CASE WHEN o.c_event_date >= GETDATE() THEN o.c_event_date END) AS NextEventDate
                    FROM {Table.SysOrders} o
                    OUTER APPLY (
                        SELECT SUM(ISNULL(p.c_paid_amount, p.c_amount)) AS PaidAmount
                        FROM {Table.SysOrderPayments} p
                        WHERE p.c_orderid = o.c_orderid
                          AND ISNULL(p.c_status, '') NOT IN ('Failed', 'Rejected', 'Cancelled')
                    ) pay
                    WHERE o.c_userid = @CustomerId AND o.c_ownerid = @OwnerId;

                    -- Favorite Menu Items (Packages and Individual Items)
                    SELECT TOP 3
                        ISNULL(f.c_foodname, p.c_packagename) AS ItemName,
                        CASE
                            WHEN f.c_ispackage_item = 1 THEN 'Package'
                            ELSE 'Individual Item'
                        END AS ItemType,
                        COUNT(*) AS OrderCount
                    FROM {Table.SysOrderItems} oi
                    INNER JOIN {Table.SysOrders} o ON oi.c_orderid = o.c_orderid
                    LEFT JOIN {Table.SysFoodItems} f ON oi.c_foodid = f.c_foodid
                    LEFT JOIN {Table.SysMenuPackage} p ON f.c_ispackage_item = 1
                        AND EXISTS (
                            SELECT 1 FROM {Table.SysMenuPackageItems} pi
                            WHERE pi.c_packageid = p.c_packageid
                        )
                    WHERE o.c_userid = @CustomerId AND o.c_ownerid = @OwnerId
                        AND f.c_is_deleted = 0
                    GROUP BY ISNULL(f.c_foodname, p.c_packagename), f.c_ispackage_item
                    ORDER BY COUNT(*) DESC;";

                var parameters = new[]
                {
                    new SqlParameter("@CustomerId", customerId),
                    new SqlParameter("@OwnerId", ownerId)
                };

                var dataSet = await Task.Run(() => _dbHelper.ExecuteDataSet(query, parameters));

                if (dataSet.Tables.Count == 0 || dataSet.Tables[0].Rows.Count == 0)
                {
                    throw new Exception("Customer not found.");
                }

                var row = dataSet.Tables[0].Rows[0];
                var customerDetail = new CustomerDetailDto
                {
                    CustomerId = Convert.ToInt64(row["CustomerId"]),
                    CustomerName = row["CustomerName"].ToString(),
                    Email = row["Email"].ToString(),
                    Phone = row["Phone"].ToString(),
                    RegisteredDate = Convert.ToDateTime(row["RegisteredDate"]),
                    Address = row["Address"] != DBNull.Value ? row["Address"].ToString() : "",
                    City = row["City"] != DBNull.Value ? row["City"].ToString() : "",
                    State = row["State"] != DBNull.Value ? row["State"].ToString() : "",
                    Pincode = row["Pincode"] != DBNull.Value ? row["Pincode"].ToString() : ""
                };

                // Order statistics
                if (dataSet.Tables.Count > 1 && dataSet.Tables[1].Rows.Count > 0)
                {
                    var statsRow = dataSet.Tables[1].Rows[0];
                    customerDetail.TotalOrders = Convert.ToInt32(statsRow["TotalOrders"]);
                    customerDetail.CompletedOrders = Convert.ToInt32(statsRow["CompletedOrders"]);
                    customerDetail.CancelledOrders = Convert.ToInt32(statsRow["CancelledOrders"]);
                    customerDetail.LifetimeValue = Convert.ToDecimal(statsRow["LifetimeValue"]);
                    customerDetail.AverageOrderValue = Convert.ToDecimal(statsRow["AverageOrderValue"]);
                    customerDetail.TotalSpent = Convert.ToDecimal(statsRow["TotalSpent"]);
                    customerDetail.OutstandingBalance = Convert.ToDecimal(statsRow["OutstandingBalance"]);
                    customerDetail.LastOrderDate = statsRow["LastOrderDate"] != DBNull.Value ? Convert.ToDateTime(statsRow["LastOrderDate"]) : (DateTime?)null;
                    customerDetail.LastOrderStatus = statsRow["LastOrderStatus"] != DBNull.Value ? statsRow["LastOrderStatus"].ToString() : "";
                    customerDetail.PreferredEventType = statsRow["PreferredEventType"] != DBNull.Value ? statsRow["PreferredEventType"].ToString() : "";
                    customerDetail.AverageGuestCount = Convert.ToInt32(statsRow["AverageGuestCount"]);
                    customerDetail.NextEventDate = statsRow["NextEventDate"] != DBNull.Value ? Convert.ToDateTime(statsRow["NextEventDate"]) : (DateTime?)null;

                    // Determine customer type
                    if (customerDetail.TotalOrders == 1)
                        customerDetail.CustomerType = "New";
                    else if (customerDetail.TotalOrders >= 5 || customerDetail.LifetimeValue >= 50000)
                        customerDetail.CustomerType = "VIP";
                    else
                        customerDetail.CustomerType = "Regular";
                }

                // Favorite menu items
                if (dataSet.Tables.Count > 2)
                {
                    foreach (DataRow itemRow in dataSet.Tables[2].Rows)
                    {
                        var itemName = itemRow["ItemName"].ToString();
                        var itemType = itemRow["ItemType"].ToString();
                        customerDetail.FavoriteMenuItems.Add($"{itemName} ({itemType})");
                    }
                }

                return customerDetail;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting customer details: {ex.Message}", ex);
            }
        }

        public async Task<CustomerOrderHistoryDto> GetCustomerOrderHistory(long ownerId, long customerId)
        {
            try
            {
                // Validate that customer has orders with this owner
                if (!await ValidateCustomerOwnership(ownerId, customerId))
                {
                    throw new UnauthorizedAccessException("Customer has no orders with this owner.");
                }

                var query = $@"
                    -- Customer info
                    SELECT CONCAT(c_firstname, ' ', c_lastname) AS CustomerName
                    FROM {Table.SysUser}
                    WHERE c_userid = @CustomerId;

                    -- Order history
                    SELECT
                        o.c_orderid AS OrderId,
                        o.c_order_number AS OrderNumber,
                        o.c_event_type AS EventType,
                        o.c_event_date AS EventDate,
                        o.c_createddate AS OrderDate,
                        o.c_guest_count AS GuestCount,
                        o.c_total_amount AS TotalAmount,
                        o.c_order_status AS OrderStatus,
                        o.c_payment_status AS PaymentStatus,
                        ISNULL(r.c_overall_rating, 0) AS Rating,
                        r.c_review_text AS ReviewText
                    FROM {Table.SysOrders} o
                    LEFT JOIN {Table.SysCateringReview} r ON o.c_orderid = r.c_orderid
                    WHERE o.c_userid = @CustomerId AND o.c_ownerid = @OwnerId
                    ORDER BY o.c_createddate DESC;

                    -- Summary
                    SELECT
                        COUNT(*) AS TotalOrders,
                        ISNULL(SUM(c_total_amount), 0) AS TotalSpent
                    FROM {Table.SysOrders}
                    WHERE c_userid = @CustomerId AND c_ownerid = @OwnerId;";

                var parameters = new[]
                {
                    new SqlParameter("@CustomerId", customerId),
                    new SqlParameter("@OwnerId", ownerId)
                };

                var dataSet = await Task.Run(() => _dbHelper.ExecuteDataSet(query, parameters));

                var result = new CustomerOrderHistoryDto
                {
                    CustomerId = customerId
                };

                if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
                {
                    result.CustomerName = dataSet.Tables[0].Rows[0]["CustomerName"].ToString();
                }

                if (dataSet.Tables.Count > 1)
                {
                    foreach (DataRow row in dataSet.Tables[1].Rows)
                    {
                        result.Orders.Add(new CustomerOrderDto
                        {
                            OrderId = Convert.ToInt64(row["OrderId"]),
                            OrderNumber = row["OrderNumber"].ToString(),
                            EventType = row["EventType"].ToString(),
                            EventDate = Convert.ToDateTime(row["EventDate"]),
                            OrderDate = Convert.ToDateTime(row["OrderDate"]),
                            GuestCount = Convert.ToInt32(row["GuestCount"]),
                            TotalAmount = Convert.ToDecimal(row["TotalAmount"]),
                            OrderStatus = row["OrderStatus"].ToString(),
                            PaymentStatus = row["PaymentStatus"].ToString(),
                            Rating = Convert.ToDecimal(row["Rating"]),
                            ReviewText = row["ReviewText"] != DBNull.Value ? row["ReviewText"].ToString() : ""
                        });
                    }
                }

                if (dataSet.Tables.Count > 2 && dataSet.Tables[2].Rows.Count > 0)
                {
                    var summaryRow = dataSet.Tables[2].Rows[0];
                    result.TotalOrders = Convert.ToInt32(summaryRow["TotalOrders"]);
                    result.TotalSpent = Convert.ToDecimal(summaryRow["TotalSpent"]);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting customer order history: {ex.Message}", ex);
            }
        }

        public async Task<CustomerInsightsDto> GetCustomerInsights(long ownerId)
        {
            try
            {
                var query = $@"
                    DECLARE @CurrentMonth DATE = DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0);

                    -- Customer Summary
                    SELECT
                        COUNT(DISTINCT o.c_userid) AS TotalCustomers,
                        COUNT(DISTINCT CASE WHEN u.c_createddate >= @CurrentMonth THEN o.c_userid END) AS NewCustomersThisMonth,
                        COUNT(DISTINCT CASE WHEN EXISTS (
                            SELECT 1 FROM {Table.SysOrders} o2
                            WHERE o2.c_userid = o.c_userid AND o2.c_ownerid = @OwnerId
                            AND o2.c_orderid != o.c_orderid
                        ) THEN o.c_userid END) AS ReturningCustomers,
                        ISNULL(AVG(o.c_total_amount), 0) AS AverageLifetimeValue
                    FROM {Table.SysOrders} o
                    INNER JOIN {Table.SysUser} u ON o.c_userid = u.c_userid
                    WHERE o.c_ownerid = @OwnerId;

                    -- Customer Satisfaction
                    SELECT ISNULL(AVG(CAST(c_overall_rating AS DECIMAL(10,2))), 0) AS CustomerSatisfactionScore
                    FROM {Table.SysCateringReview}
                    WHERE c_ownerid = @OwnerId;

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
                    GROUP BY u.c_userid, u.c_firstname, u.c_lastname, u.c_email, u.c_mobilenumber
                    ORDER BY LifetimeValue DESC;

                    -- Monthly Trends (Last 6 months)
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
                        ) THEN o.c_userid END) AS ReturningCustomers,
                        SUM(o.c_total_amount) AS TotalRevenue
                    FROM {Table.SysOrders} o
                    WHERE o.c_ownerid = @OwnerId
                        AND o.c_createddate >= DATEADD(MONTH, -6, GETDATE())
                    GROUP BY YEAR(o.c_createddate), MONTH(o.c_createddate), FORMAT(o.c_createddate, 'MMM yyyy')
                    ORDER BY YEAR(o.c_createddate), MONTH(o.c_createddate);";

                var parameters = new[] { new SqlParameter("@OwnerId", ownerId) };
                var dataSet = await Task.Run(() => _dbHelper.ExecuteDataSet(query, parameters));

                var insights = new CustomerInsightsDto();

                if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
                {
                    var row = dataSet.Tables[0].Rows[0];
                    insights.TotalCustomers = Convert.ToInt32(row["TotalCustomers"]);
                    insights.NewCustomersThisMonth = Convert.ToInt32(row["NewCustomersThisMonth"]);
                    insights.ReturningCustomers = Convert.ToInt32(row["ReturningCustomers"]);
                    insights.AverageLifetimeValue = Convert.ToDecimal(row["AverageLifetimeValue"]);

                    if (insights.TotalCustomers > 0)
                    {
                        insights.CustomerRetentionRate = (insights.ReturningCustomers * 100.0m / insights.TotalCustomers);
                    }
                }

                if (dataSet.Tables.Count > 1 && dataSet.Tables[1].Rows.Count > 0)
                {
                    insights.CustomerSatisfactionScore = Convert.ToDecimal(dataSet.Tables[1].Rows[0]["CustomerSatisfactionScore"]);
                }

                // Top customers
                if (dataSet.Tables.Count > 2)
                {
                    foreach (DataRow row in dataSet.Tables[2].Rows)
                    {
                        insights.TopCustomers.Add(new TopCustomerDto
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

                // Customer distribution by type
                var newCount = insights.TotalCustomers - insights.ReturningCustomers;
                var regularCount = insights.ReturningCustomers;
                var vipCount = insights.TopCustomers.Count(c => c.TotalOrders >= 5 || c.LifetimeValue >= 50000);

                insights.CustomersByType["New"] = newCount;
                insights.CustomersByType["Regular"] = regularCount - vipCount;
                insights.CustomersByType["VIP"] = vipCount;

                // Monthly trends
                if (dataSet.Tables.Count > 3)
                {
                    foreach (DataRow row in dataSet.Tables[3].Rows)
                    {
                        insights.MonthlyTrends.Add(new CustomerTrendDto
                        {
                            Month = row["Month"].ToString(),
                            NewCustomers = Convert.ToInt32(row["NewCustomers"]),
                            ReturningCustomers = Convert.ToInt32(row["ReturningCustomers"]),
                            TotalRevenue = Convert.ToDecimal(row["TotalRevenue"])
                        });
                    }
                }

                return insights;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting customer insights: {ex.Message}", ex);
            }
        }

        public async Task<List<TopCustomerDto>> GetTopCustomers(long ownerId, int limit = 10, string sortBy = "LifetimeValue")
        {
            try
            {
                var orderByClause = sortBy.ToLower() == "totalorders" ? "TotalOrders" : "LifetimeValue";

                var query = $@"
                    SELECT TOP {limit}
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
                    GROUP BY u.c_userid, u.c_firstname, u.c_lastname, u.c_email, u.c_mobilenumber
                    ORDER BY {orderByClause} DESC";

                var parameters = new[] { new SqlParameter("@OwnerId", ownerId) };
                var dataTable = await Task.Run(() => _dbHelper.ExecuteAsync(query, parameters));

                var topCustomers = new List<TopCustomerDto>();
                foreach (DataRow row in dataTable.Rows)
                {
                    topCustomers.Add(new TopCustomerDto
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

                return topCustomers;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting top customers: {ex.Message}", ex);
            }
        }

        public async Task<bool> ValidateCustomerOwnership(long ownerId, long customerId)
        {
            try
            {
                var query = $@"
                    SELECT COUNT(*) FROM {Table.SysOrders}
                    WHERE c_userid = @CustomerId AND c_ownerid = @OwnerId";

                var parameters = new[]
                {
                    new SqlParameter("@CustomerId", customerId),
                    new SqlParameter("@OwnerId", ownerId)
                };

                var result = await Task.Run(() => _dbHelper.ExecuteScalar(query, parameters));
                return result != null && Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error validating customer ownership: {ex.Message}", ex);
            }
        }
    }
}
