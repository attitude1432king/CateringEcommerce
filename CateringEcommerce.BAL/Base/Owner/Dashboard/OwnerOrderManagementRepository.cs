using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace CateringEcommerce.BAL.Base.Owner.Dashboard
{
    public class OwnerOrderManagementRepository : IOwnerOrderRepository
    {
        private readonly IDatabaseHelper _dbHelper;
        public OwnerOrderManagementRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<PaginatedOrdersDto> GetOrdersList(long ownerId, OrderFilterDto filter)
        {
            try
            {
                var query = new StringBuilder();
                var countQuery = new StringBuilder();

                // Base query
                var baseQuery = $@"
                    FROM {Table.SysOrders} o
                    INNER JOIN {Table.SysUser} u ON o.c_userid = u.c_userid
                    WHERE o.c_ownerid = @OwnerId";

                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@OwnerId", ownerId)
                };

                // Apply filters
                if (!string.IsNullOrEmpty(filter.OrderStatus) && filter.OrderStatus.ToLower() != "all")
                {
                    baseQuery += " AND o.c_order_status = @OrderStatus";
                    parameters.Add(new SqlParameter("@OrderStatus", filter.OrderStatus));
                }

                if (filter.StartDate.HasValue)
                {
                    baseQuery += " AND o.c_created_date >= @StartDate";
                    parameters.Add(new SqlParameter("@StartDate", filter.StartDate.Value));
                }

                if (filter.EndDate.HasValue)
                {
                    baseQuery += " AND o.c_created_date <= @EndDate";
                    parameters.Add(new SqlParameter("@EndDate", filter.EndDate.Value.AddDays(1).AddSeconds(-1)));
                }

                if (!string.IsNullOrEmpty(filter.EventType))
                {
                    baseQuery += " AND o.c_event_type = @EventType";
                    parameters.Add(new SqlParameter("@EventType", filter.EventType));
                }

                if (filter.MinAmount.HasValue)
                {
                    baseQuery += " AND o.c_final_amount >= @MinAmount";
                    parameters.Add(new SqlParameter("@MinAmount", filter.MinAmount.Value));
                }

                if (filter.MaxAmount.HasValue)
                {
                    baseQuery += " AND o.c_final_amount <= @MaxAmount";
                    parameters.Add(new SqlParameter("@MaxAmount", filter.MaxAmount.Value));
                }

                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    baseQuery += @" AND (o.c_order_number LIKE @SearchTerm
                                    OR CONCAT(u.c_firstname, ' ', u.c_lastname) LIKE @SearchTerm
                                    OR u.c_mobilenumber LIKE @SearchTerm)";
                    parameters.Add(new SqlParameter("@SearchTerm", $"%{filter.SearchTerm}%"));
                }

                // Count query
                countQuery.Append($"SELECT COUNT(*) AS TotalCount {baseQuery}");

                // Data query with pagination
                query.Append($@"
                    SELECT
                        o.c_orderid AS OrderId,
                        o.c_order_number AS OrderNumber,
                        CONCAT(u.c_firstname, ' ', u.c_lastname) AS CustomerName,
                        u.c_mobilenumber AS CustomerPhone,
                        o.c_event_type AS EventType,
                        o.c_event_date AS EventDate,
                        o.c_created_date AS OrderDate,
                        o.c_final_amount AS TotalAmount,
                        ISNULL(o.c_paid_amount, 0) AS PaidAmount,
                        (o.c_final_amount - ISNULL(o.c_paid_amount, 0)) AS BalanceAmount,
                        o.c_order_status AS OrderStatus,
                        o.c_payment_status AS PaymentStatus,
                        o.c_guest_count AS GuestCount,
                        DATEDIFF(DAY, GETDATE(), o.c_event_date) AS DaysUntilEvent
                    {baseQuery}
                    ORDER BY ");

                // Apply sorting
                switch (filter.SortBy?.ToLower())
                {
                    case "eventdate":
                        query.Append("o.c_event_date");
                        break;
                    case "amount":
                        query.Append("o.c_final_amount");
                        break;
                    case "orderdate":
                    default:
                        query.Append("o.c_created_date");
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

                var orders = new List<OrderListItemDto>();
                foreach (DataRow row in dataTable.Rows)
                {
                    orders.Add(new OrderListItemDto
                    {
                        OrderId = Convert.ToInt64(row["OrderId"]),
                        OrderNumber = row["OrderNumber"].ToString(),
                        CustomerName = row["CustomerName"].ToString(),
                        CustomerPhone = row["CustomerPhone"].ToString(),
                        EventType = row["EventType"].ToString(),
                        EventDate = Convert.ToDateTime(row["EventDate"]),
                        OrderDate = Convert.ToDateTime(row["OrderDate"]),
                        TotalAmount = Convert.ToDecimal(row["TotalAmount"]),
                        PaidAmount = Convert.ToDecimal(row["PaidAmount"]),
                        BalanceAmount = Convert.ToDecimal(row["BalanceAmount"]),
                        OrderStatus = row["OrderStatus"].ToString(),
                        PaymentStatus = row["PaymentStatus"].ToString(),
                        GuestCount = Convert.ToInt32(row["GuestCount"]),
                        DaysUntilEvent = Convert.ToInt32(row["DaysUntilEvent"])
                    });
                }

                var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

                return new PaginatedOrdersDto
                {
                    Orders = orders,
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
                throw new Exception($"Error getting orders list: {ex.Message}", ex);
            }
        }

        public async Task<OrderDetailDto> GetOrderDetails(long ownerId, long orderId)
        {
            try
            {
                // Validate ownership first
                if (!await ValidateOrderOwnership(ownerId, orderId))
                {
                    throw new UnauthorizedAccessException("Order does not belong to this owner.");
                }

                var query = $@"
                    -- Order Details
                    SELECT
                        o.c_orderid AS OrderId,
                        o.c_order_number AS OrderNumber,
                        o.c_userid AS CustomerId,
                        CONCAT(u.c_firstname, ' ', u.c_lastname) AS CustomerName,
                        u.c_email AS CustomerEmail,
                        u.c_mobilenumber AS CustomerPhone,
                        o.c_event_type AS EventType,
                        o.c_event_date AS EventDate,
                        o.c_event_time AS EventTime,
                        o.c_guest_count AS GuestCount,
                        o.c_venue_address AS VenueAddress,
                        o.c_venue_city AS VenueCity,
                        o.c_venue_state AS VenueState,
                        o.c_venue_pincode AS VenuePincode,
                        o.c_created_date AS OrderDate,
                        o.c_order_status AS OrderStatus,
                        o.c_payment_status AS PaymentStatus,
                        o.c_special_instructions AS SpecialInstructions,
                        o.c_subtotal AS SubTotal,
                        ISNULL(o.c_tax_amount, 0) AS TaxAmount,
                        ISNULL(o.c_discount_amount, 0) AS DiscountAmount,
                        ISNULL(o.c_delivery_charges, 0) AS DeliveryCharges,
                        o.c_final_amount AS TotalAmount,
                        ISNULL(o.c_paid_amount, 0) AS PaidAmount,
                        (o.c_final_amount - ISNULL(o.c_paid_amount, 0)) AS BalanceAmount
                    FROM {Table.SysOrders} o
                    INNER JOIN {Table.SysUser} u ON o.c_userid = u.c_userid
                    WHERE o.c_orderid = @OrderId;

                    -- Order Items (Food Items and Packages)
                    SELECT
                        oi.c_order_item_id AS OrderItemId,
                        oi.c_foodid AS MenuItemId,
                        ISNULL(f.c_foodname, p.c_packagename) AS MenuItemName,
                        ISNULL(fc.c_categoryname, 'Package') AS Category,
                        oi.c_quantity AS Quantity,
                        oi.c_unit_price AS UnitPrice,
                        oi.c_item_total AS TotalPrice,
                        '' AS ImageUrl,
                        oi.c_special_request AS SpecialRequest,
                        CASE
                            WHEN f.c_ispackage_item = 1 THEN 'Package'
                            ELSE 'Individual Item'
                        END AS ItemType
                    FROM {Table.SysOrderItems} oi
                    INNER JOIN {Table.SysFoodItems} f ON oi.c_foodid = f.c_foodid
                    LEFT JOIN {Table.SysFoodCategory} fc ON f.c_categoryid = fc.c_categoryid
                    LEFT JOIN {Table.SysMenuPackage} p ON f.c_ispackage_item = 1
                        AND EXISTS (
                            SELECT 1 FROM {Table.SysMenuPackageItems} pi
                            WHERE pi.c_packageid = p.c_packageid
                        )
                    WHERE oi.c_orderid = @OrderId
                        AND f.c_is_deleted = 0;";

                var parameters = new[] { new SqlParameter("@OrderId", orderId) };
                var dataSet = await Task.Run(() => _dbHelper.ExecuteDataSet(query, parameters));

                if (dataSet.Tables.Count == 0 || dataSet.Tables[0].Rows.Count == 0)
                {
                    throw new Exception("Order not found.");
                }

                var row = dataSet.Tables[0].Rows[0];
                var orderDetail = new OrderDetailDto
                {
                    OrderId = Convert.ToInt64(row["OrderId"]),
                    OrderNumber = row["OrderNumber"].ToString(),
                    CustomerId = Convert.ToInt64(row["CustomerId"]),
                    CustomerName = row["CustomerName"].ToString(),
                    CustomerEmail = row["CustomerEmail"].ToString(),
                    CustomerPhone = row["CustomerPhone"].ToString(),
                    EventType = row["EventType"].ToString(),
                    EventDate = Convert.ToDateTime(row["EventDate"]),
                    EventTime = row["EventTime"].ToString(),
                    GuestCount = Convert.ToInt32(row["GuestCount"]),
                    VenueAddress = row["VenueAddress"].ToString(),
                    VenueCity = row["VenueCity"] != DBNull.Value ? row["VenueCity"].ToString() : "",
                    VenueState = row["VenueState"] != DBNull.Value ? row["VenueState"].ToString() : "",
                    VenuePincode = row["VenuePincode"] != DBNull.Value ? row["VenuePincode"].ToString() : "",
                    OrderDate = Convert.ToDateTime(row["OrderDate"]),
                    OrderStatus = row["OrderStatus"].ToString(),
                    PaymentStatus = row["PaymentStatus"].ToString(),
                    SpecialInstructions = row["SpecialInstructions"] != DBNull.Value ? row["SpecialInstructions"].ToString() : "",
                    SubTotal = Convert.ToDecimal(row["SubTotal"]),
                    TaxAmount = Convert.ToDecimal(row["TaxAmount"]),
                    DiscountAmount = Convert.ToDecimal(row["DiscountAmount"]),
                    DeliveryCharges = Convert.ToDecimal(row["DeliveryCharges"]),
                    TotalAmount = Convert.ToDecimal(row["TotalAmount"]),
                    PaidAmount = Convert.ToDecimal(row["PaidAmount"]),
                    BalanceAmount = Convert.ToDecimal(row["BalanceAmount"])
                };

                // Add order items
                if (dataSet.Tables.Count > 1)
                {
                    foreach (DataRow itemRow in dataSet.Tables[1].Rows)
                    {
                        orderDetail.Items.Add(new OrderItemDetailDto
                        {
                            OrderItemId = Convert.ToInt64(itemRow["OrderItemId"]),
                            MenuItemId = Convert.ToInt64(itemRow["MenuItemId"]),
                            MenuItemName = itemRow["MenuItemName"].ToString(),
                            Category = itemRow["Category"].ToString(),
                            Quantity = Convert.ToInt32(itemRow["Quantity"]),
                            UnitPrice = Convert.ToDecimal(itemRow["UnitPrice"]),
                            TotalPrice = Convert.ToDecimal(itemRow["TotalPrice"]),
                            ImageUrl = itemRow["ImageUrl"] != DBNull.Value ? itemRow["ImageUrl"].ToString() : "",
                            SpecialRequest = itemRow["SpecialRequest"] != DBNull.Value ? itemRow["SpecialRequest"].ToString() : ""
                        });
                    }
                }

                // Get status history
                orderDetail.StatusHistory = await GetOrderStatusHistory(orderId);

                return orderDetail;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting order details: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateOrderStatus(long ownerId, long orderId, OrderStatusUpdateDto statusUpdate)
        {
            try
            {
                // Validate ownership first
                if (!await ValidateOrderOwnership(ownerId, orderId))
                {
                    throw new UnauthorizedAccessException("Order does not belong to this owner.");
                }

                var query = new StringBuilder();

                // Update order status
                query.Append($@"
                    UPDATE {Table.SysOrders}
                    SET c_order_status = @NewStatus,
                        c_modified_date = @ModifiedDate");

                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@OrderId", orderId),
                    new SqlParameter("@NewStatus", statusUpdate.NewStatus),
                    new SqlParameter("@ModifiedDate", DateTime.Now)
                };

                if (statusUpdate.EstimatedDeliveryTime.HasValue)
                {
                    query.Append(", c_estimated_delivery_time = @EstimatedDeliveryTime");
                    parameters.Add(new SqlParameter("@EstimatedDeliveryTime", statusUpdate.EstimatedDeliveryTime.Value));
                }

                query.Append(" WHERE c_orderid = @OrderId;");

                // Insert status history (if you have a status history table)
                query.Append($@"
                    INSERT INTO {Table.SysOrderStatusHistory}
                    (c_orderid, c_status, c_comments, c_changed_date, c_changed_by)
                    VALUES (@OrderId, @NewStatus, @Comments, @ModifiedDate, @ChangedBy);");

                parameters.Add(new SqlParameter("@Comments", statusUpdate.Comments ?? ""));
                parameters.Add(new SqlParameter("@ChangedBy", ownerId.ToString()));

                var result = await Task.Run(() => _dbHelper.ExecuteNonQuery(query.ToString(), parameters.ToArray()));

                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating order status: {ex.Message}", ex);
            }
        }

        public async Task<List<OrderStatusHistoryDto>> GetOrderStatusHistory(long orderId)
        {
            try
            {
                var query = $@"
                    SELECT
                        c_status_id AS StatusId,
                        c_status AS Status,
                        c_changed_date AS ChangedDate,
                        c_changed_by AS ChangedBy,
                        c_comments AS Comments
                    FROM {Table.SysOrderStatusHistory}
                    WHERE c_orderid = @OrderId
                    ORDER BY c_changed_date DESC";

                var parameters = new[] { new SqlParameter("@OrderId", orderId) };
                var dataTable = await Task.Run(() => _dbHelper.ExecuteAsync(query, parameters));

                var history = new List<OrderStatusHistoryDto>();
                foreach (DataRow row in dataTable.Rows)
                {
                    history.Add(new OrderStatusHistoryDto
                    {
                        StatusId = Convert.ToInt64(row["StatusId"]),
                        Status = row["Status"].ToString(),
                        ChangedDate = Convert.ToDateTime(row["ChangedDate"]),
                        ChangedBy = row["ChangedBy"].ToString(),
                        Comments = row["Comments"] != DBNull.Value ? row["Comments"].ToString() : ""
                    });
                }

                return history;
            }
            catch (Exception ex)
            {
                // If the table doesn't exist, return empty list
                return new List<OrderStatusHistoryDto>();
            }
        }

        public async Task<OrderStatsDto> GetOrderStats(long ownerId)
        {
            try
            {
                var query = $@"
                    SELECT
                        COUNT(*) AS TotalOrders,
                        SUM(CASE WHEN c_order_status = 'Pending' THEN 1 ELSE 0 END) AS PendingOrders,
                        SUM(CASE WHEN c_order_status = 'Confirmed' THEN 1 ELSE 0 END) AS ConfirmedOrders,
                        SUM(CASE WHEN c_order_status = 'Completed' THEN 1 ELSE 0 END) AS CompletedOrders,
                        SUM(CASE WHEN c_order_status = 'Cancelled' THEN 1 ELSE 0 END) AS CancelledOrders,
                        ISNULL(SUM(c_final_amount), 0) AS TotalRevenue,
                        ISNULL(AVG(c_final_amount), 0) AS AverageOrderValue
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId";

                var parameters = new[] { new SqlParameter("@OwnerId", ownerId) };
                var dataTable = await Task.Run(() => _dbHelper.ExecuteAsync(query, parameters));

                if (dataTable.Rows.Count > 0)
                {
                    var row = dataTable.Rows[0];
                    return new OrderStatsDto
                    {
                        TotalOrders = Convert.ToInt32(row["TotalOrders"]),
                        PendingOrders = Convert.ToInt32(row["PendingOrders"]),
                        ConfirmedOrders = Convert.ToInt32(row["ConfirmedOrders"]),
                        CompletedOrders = Convert.ToInt32(row["CompletedOrders"]),
                        CancelledOrders = Convert.ToInt32(row["CancelledOrders"]),
                        TotalRevenue = Convert.ToDecimal(row["TotalRevenue"]),
                        AverageOrderValue = Convert.ToDecimal(row["AverageOrderValue"])
                    };
                }

                return new OrderStatsDto();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting order stats: {ex.Message}", ex);
            }
        }

        // ===================================
        // GET BOOKING REQUEST STATS (Today/Week/Month)
        // ===================================
        public async Task<BookingRequestStatsDto> GetBookingRequestStats(long ownerId)
        {
            try
            {
                var query = $@"
                    SELECT
                        SUM(CASE WHEN CAST(c_created_date AS DATE) = CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END) AS TodayRequests,
                        SUM(CASE WHEN c_created_date >= DATEADD(DAY, -7, GETDATE()) THEN 1 ELSE 0 END) AS WeekRequests,
                        SUM(CASE WHEN c_created_date >= DATEADD(DAY, -30, GETDATE()) THEN 1 ELSE 0 END) AS MonthRequests,
                        SUM(CASE WHEN c_order_status = 'Pending' THEN 1 ELSE 0 END) AS TotalPending,
                        SUM(CASE WHEN c_order_status = 'Confirmed' THEN 1 ELSE 0 END) AS TotalConfirmed,
                        SUM(CASE WHEN c_order_status = 'Cancelled' THEN 1 ELSE 0 END) AS TotalRejected
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId AND c_isactive = 1";

                var parameters = new[] { new SqlParameter("@OwnerId", ownerId) };
                var dataTable = await Task.Run(() => _dbHelper.ExecuteAsync(query, parameters));

                if (dataTable.Rows.Count > 0)
                {
                    var row = dataTable.Rows[0];
                    return new BookingRequestStatsDto
                    {
                        TodayRequests = Convert.ToInt32(row["TodayRequests"]),
                        WeekRequests = Convert.ToInt32(row["WeekRequests"]),
                        MonthRequests = Convert.ToInt32(row["MonthRequests"]),
                        TotalPending = Convert.ToInt32(row["TotalPending"]),
                        TotalConfirmed = Convert.ToInt32(row["TotalConfirmed"]),
                        TotalRejected = Convert.ToInt32(row["TotalRejected"])
                    };
                }

                return new BookingRequestStatsDto();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting booking request stats: {ex.Message}", ex);
            }
        }

        public async Task<bool> ValidateOrderOwnership(long ownerId, long orderId)
        {
            try
            {
                var query = $@"
                    SELECT COUNT(*) FROM {Table.SysOrders}
                    WHERE c_orderid = @OrderId AND c_ownerid = @OwnerId";

                var parameters = new[]
                {
                    new SqlParameter("@OrderId", orderId),
                    new SqlParameter("@OwnerId", ownerId)
                };

                var result = await Task.Run(() => _dbHelper.ExecuteScalar(query, parameters));
                return result != null && Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error validating order ownership: {ex.Message}", ex);
            }
        }
    }
}
