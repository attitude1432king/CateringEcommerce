using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;
using Npgsql;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

                var baseQuery = $@"
            FROM {Table.SysOrders} o
            INNER JOIN {Table.SysUser} u ON o.c_userid = u.c_userid

            LEFT JOIN LATERAL (
                SELECT SUM(COALESCE(p.c_paid_amount, p.c_amount)) AS PaidAmount
                FROM {Table.SysOrderPayments} p
                WHERE p.c_orderid = o.c_orderid
                  AND COALESCE(p.c_status, '') NOT IN ('Failed', 'Rejected', 'Cancelled')
            ) pay ON TRUE

            LEFT JOIN LATERAL (
                SELECT STRING_AGG(src.FoodName, ', ' ORDER BY src.FoodName) AS MenuItemNames
                FROM (
                    SELECT src.FoodName
                    FROM (
                        -- Source 1: Direct FoodItem rows
                        SELECT f.c_foodname AS FoodName
                        FROM {Table.SysOrderItems} oi
                        INNER JOIN {Table.SysFoodItems} f ON oi.c_item_id = f.c_foodid
                        WHERE oi.c_orderid   = o.c_orderid
                          AND oi.c_item_type = 'FoodItem'
                          AND f.c_is_deleted = FALSE

                        UNION ALL

                        -- Source 2: Food names inside Package JSON
                        SELECT si.value ->> 'foodName' AS FoodName
                        FROM {Table.SysOrderItems} oi
                        CROSS JOIN LATERAL jsonb_array_elements((oi.c_package_selections::jsonb) -> 'selections') sel(value)
                        CROSS JOIN LATERAL jsonb_array_elements(sel.value -> 'selectedItems') si(value)
                        WHERE oi.c_orderid   = o.c_orderid
                          AND oi.c_item_type = 'Package'
                          AND oi.c_package_selections IS NOT NULL
                          AND si.value ->> 'foodName' IS NOT NULL

                    ) src
                    WHERE src.FoodName IS NOT NULL
                    LIMIT 6
                ) src
            ) menu ON TRUE

            WHERE o.c_ownerid = @OwnerId";

                var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter("@OwnerId", ownerId)
        };

                // â”€â”€ Filters â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                if (!string.IsNullOrEmpty(filter.OrderStatus) && filter.OrderStatus.ToLower() != "all")
                {
                    baseQuery += " AND o.c_order_status = @OrderStatus";
                    parameters.Add(new NpgsqlParameter("@OrderStatus", filter.OrderStatus));
                }

                if (filter.StartDate.HasValue)
                {
                    baseQuery += " AND o.c_createddate >= @StartDate";
                    parameters.Add(new NpgsqlParameter("@StartDate", filter.StartDate.Value));
                }

                if (filter.EndDate.HasValue)
                {
                    baseQuery += " AND o.c_createddate <= @EndDate";
                    parameters.Add(new NpgsqlParameter("@EndDate", filter.EndDate.Value.AddDays(1).AddSeconds(-1)));
                }

                if (!string.IsNullOrEmpty(filter.EventType))
                {
                    baseQuery += " AND o.c_event_type = @EventType";
                    parameters.Add(new NpgsqlParameter("@EventType", filter.EventType));
                }

                if (filter.MinAmount.HasValue)
                {
                    baseQuery += " AND o.c_total_amount >= @MinAmount";
                    parameters.Add(new NpgsqlParameter("@MinAmount", filter.MinAmount.Value));
                }

                if (filter.MaxAmount.HasValue)
                {
                    baseQuery += " AND o.c_total_amount <= @MaxAmount";
                    parameters.Add(new NpgsqlParameter("@MaxAmount", filter.MaxAmount.Value));
                }

                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    baseQuery += @" AND (o.c_order_number LIKE @SearchTerm
                            OR u.c_name           LIKE @SearchTerm
                            OR u.c_mobile         LIKE @SearchTerm)";
                    parameters.Add(new NpgsqlParameter("@SearchTerm", $"%{filter.SearchTerm}%"));
                }

                if (filter.ExcludeStatuses?.Count > 0)
                {
                    var placeholders = string.Join(",", filter.ExcludeStatuses
                        .Select((s, i) =>
                        {
                            parameters.Add(new NpgsqlParameter($"@ExclStatus{i}", s));
                            return $"@ExclStatus{i}";
                        }));
                    baseQuery += $" AND o.c_order_status NOT IN ({placeholders})";
                }

                // â”€â”€ Count query â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                countQuery.Append($"SELECT COUNT(*) AS TotalCount {baseQuery}");

                // â”€â”€ Data query â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                query.Append($@"
            SELECT
                o.c_orderid                                                    AS OrderId,
                o.c_order_number                                               AS OrderNumber,
                u.c_name                                                       AS CustomerName,
                u.c_mobile                                                     AS CustomerPhone,
                o.c_event_type                                                 AS EventType,
                o.c_event_date                                                 AS EventDate,
                o.c_createddate                                                AS OrderDate,
                o.c_total_amount                                               AS TotalAmount,
                COALESCE(pay.PaidAmount, 0)                                      AS PaidAmount,
                (o.c_total_amount - COALESCE(pay.PaidAmount, 0))                AS BalanceAmount,
                o.c_order_status                                               AS OrderStatus,
                o.c_payment_status                                             AS PaymentStatus,
                o.c_guest_count                                                AS GuestCount,
                (o.c_event_date::date - CURRENT_DATE)                     AS DaysUntilEvent,
                COALESCE(o.c_event_time, '')                                    AS EventTime,
                COALESCE(o.c_event_location, COALESCE(o.c_delivery_address, '')) AS VenueAddress,
                COALESCE(menu.MenuItemNames, '')                                AS MenuItemNames
            {baseQuery}
            ORDER BY ");

                // â”€â”€ Sorting â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                switch (filter.SortBy?.ToLower())
                {
                    case "eventdate": query.Append("o.c_event_date"); break;
                    case "amount": query.Append("o.c_total_amount"); break;
                    case "orderdate":
                    default: query.Append("o.c_createddate"); break;
                }

                query.Append(filter.SortOrder?.ToUpper() == "ASC" ? " ASC" : " DESC");

                // â”€â”€ Pagination â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                int offset = (filter.Page - 1) * filter.PageSize;
                query.Append($" LIMIT {filter.PageSize} OFFSET {offset}");

                // â”€â”€ Execute count â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                var totalCount = 0;
                var countResult = await Task.Run(() =>
                    _dbHelper.ExecuteScalar(countQuery.ToString(), CloneParameters(parameters)));
                if (countResult != null)
                    totalCount = Convert.ToInt32(countResult);

                // â”€â”€ Execute data â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                var dataTable = await Task.Run(() =>
                    _dbHelper.ExecuteAsync(query.ToString(), CloneParameters(parameters)));

                var orders = new List<OrderListItemDto>();
                foreach (DataRow row in dataTable.Rows)
                {
                    var rawMenuItems = row.GetValue<string>("MenuItemNames", string.Empty);
                    orders.Add(new OrderListItemDto
                    {
                        OrderId = row.GetValue<long>("OrderId"),
                        OrderNumber = row.GetValue<string>("OrderNumber", string.Empty),
                        CustomerName = row.GetValue<string>("CustomerName", string.Empty),
                        CustomerPhone = row.GetValue<string>("CustomerPhone", string.Empty),
                        EventType = row.GetValue<string>("EventType", string.Empty),
                        EventDate = row.GetValue<DateTime>("EventDate"),
                        EventTime = row.GetValue<string>("EventTime", string.Empty),
                        VenueAddress = row.GetValue<string>("VenueAddress", string.Empty),
                        OrderDate = row.GetValue<DateTime>("OrderDate"),
                        TotalAmount = row.GetValue<decimal>("TotalAmount"),
                        PaidAmount = row.GetValue<decimal>("PaidAmount"),
                        BalanceAmount = row.GetValue<decimal>("BalanceAmount"),
                        OrderStatus = row.GetValue<string>("OrderStatus", string.Empty),
                        PaymentStatus = row.GetValue<string>("PaymentStatus", string.Empty),
                        GuestCount = row.GetValue<int>("GuestCount"),
                        DaysUntilEvent = row.GetValue<int>("DaysUntilEvent"),
                        MenuItems = string.IsNullOrEmpty(rawMenuItems)
                            ? []
                            : rawMenuItems
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => s.Trim())
                                .Where(s => !string.IsNullOrEmpty(s))
                                .ToList()
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
                        u.c_name AS CustomerName,
                        u.c_email AS CustomerEmail,
                        u.c_mobile AS CustomerPhone,
                        o.c_event_type AS EventType,
                        o.c_event_date AS EventDate,
                        o.c_event_time AS EventTime,
                        o.c_guest_count AS GuestCount,
                        o.c_event_location AS EventLocation,
                        o.c_delivery_address AS DeliveryAddress,
                        COALESCE(o.c_event_location, COALESCE(o.c_delivery_address, '')) AS VenueAddress,
                        '' AS VenueCity,
                        '' AS VenueState,
                        '' AS VenuePincode,
                        o.c_createddate AS OrderDate,
                        o.c_order_status AS OrderStatus,
                        o.c_payment_method AS PaymentMethod,
                        o.c_payment_status AS PaymentStatus,
                        o.c_contact_person AS ContactPerson,
                        o.c_contact_phone AS ContactPhone,
                        o.c_contact_email AS ContactEmail,
                        o.c_special_instructions AS SpecialInstructions,
                        COALESCE(o.c_base_amount, 0) AS SubTotal,
                        COALESCE(o.c_tax_amount, 0) AS TaxAmount,
                        COALESCE(o.c_discount_amount, 0) AS DiscountAmount,
                        COALESCE(o.c_delivery_charges, 0) AS DeliveryCharges,
                        o.c_total_amount AS TotalAmount,
                        COALESCE(o.c_payment_split_enabled, 0) AS PaymentSplitEnabled,
                        o.c_prebooking_amount AS PreBookingAmount,
                        o.c_postevent_amount AS PostEventAmount,
                        o.c_prebooking_status AS PreBookingStatus,
                        o.c_postevent_status AS PostEventStatus,
                        COALESCE(pay.PaidAmount, 0) AS PaidAmount,
                        (o.c_total_amount - COALESCE(pay.PaidAmount, 0)) AS BalanceAmount
                    FROM {Table.SysOrders} o
                    INNER JOIN {Table.SysUser} u ON o.c_userid = u.c_userid
                    LEFT JOIN LATERAL (
                        SELECT SUM(COALESCE(p.c_paid_amount, p.c_amount)) AS PaidAmount
                        FROM {Table.SysOrderPayments} p
                        WHERE p.c_orderid = o.c_orderid
                          AND COALESCE(p.c_status, '') NOT IN ('Failed', 'Rejected', 'Cancelled')
                    ) pay ON TRUE
                    WHERE o.c_orderid = @OrderId;

                    -- Order Items (Food Items and Packages)
                    SELECT
                        oi.c_order_item_id AS OrderItemId,
                        oi.c_item_id AS MenuItemId,
                        oi.c_item_name AS MenuItemName,
                        CASE
                            WHEN oi.c_item_type = 'FoodItem' THEN COALESCE(fc.c_categoryname, 'Food Item')
                            WHEN oi.c_item_type = 'Package' THEN 'Package'
                            WHEN oi.c_item_type = 'Decoration' THEN 'Decoration'
                            ELSE COALESCE(oi.c_item_type, 'Item')
                        END AS Category,
                        oi.c_item_type AS ItemType,
                        oi.c_quantity AS Quantity,
                        oi.c_unit_price AS UnitPrice,
                        oi.c_total_price AS TotalPrice,
                        '' AS ImageUrl,
                        CAST(NULL AS NVARCHAR(MAX)) AS SpecialRequest,
                        oi.c_package_selections AS PackageSelections
                    FROM {Table.SysOrderItems} oi
                    LEFT JOIN {Table.SysFoodItems} f
                        ON oi.c_item_type = 'FoodItem'
                       AND oi.c_item_id = f.c_foodid
                    LEFT JOIN {Table.SysFoodCategory} fc ON f.c_categoryid = fc.c_categoryid
                    WHERE oi.c_orderid = @OrderId
                    ORDER BY oi.c_order_item_id ASC;";

                var parameters = new[] { new NpgsqlParameter("@OrderId", orderId) };
                var dataSet = await Task.Run(() => _dbHelper.ExecuteDataSet(query, parameters));

                if (dataSet.Tables.Count == 0 || dataSet.Tables[0].Rows.Count == 0)
                {
                    throw new Exception("Order not found.");
                }

                var row = dataSet.Tables[0].Rows[0];
                var orderDetail = new OrderDetailDto
                {
                    OrderId = row.GetValue<long>("OrderId"),
                    OrderNumber = row.GetValue<string>("OrderNumber", string.Empty),
                    CustomerId = row.GetValue<long>("CustomerId"),
                    CustomerName = row.GetValue<string>("CustomerName", string.Empty),
                    CustomerEmail = row.GetValue<string>("CustomerEmail", string.Empty),
                    CustomerPhone = row.GetValue<string>("CustomerPhone", string.Empty),
                    EventType = row.GetValue<string>("EventType", string.Empty),
                    EventDate = row.GetValue<DateTime>("EventDate"),
                    EventTime = row.GetValue<string>("EventTime", string.Empty),
                    GuestCount = row.GetValue<int>("GuestCount"),
                    EventLocation = row.GetValue<string>("EventLocation", string.Empty),
                    DeliveryAddress = row.GetValue<string>("DeliveryAddress", string.Empty),
                    VenueAddress = row.GetValue<string>("VenueAddress", string.Empty),
                    VenueCity = row.GetValue<string>("VenueCity", string.Empty),
                    VenueState = row.GetValue<string>("VenueState", string.Empty),
                    VenuePincode = row.GetValue<string>("VenuePincode", string.Empty),
                    OrderDate = row.GetValue<DateTime>("OrderDate"),
                    OrderStatus = row.GetValue<string>("OrderStatus", string.Empty),
                    PaymentMethod = row.GetValue<string>("PaymentMethod", string.Empty),
                    PaymentStatus = row.GetValue<string>("PaymentStatus", string.Empty),
                    ContactPerson = row.GetValue<string>("ContactPerson", string.Empty),
                    ContactPhone = row.GetValue<string>("ContactPhone", string.Empty),
                    ContactEmail = row.GetValue<string>("ContactEmail", string.Empty),
                    SpecialInstructions = row.GetValue<string>("SpecialInstructions", string.Empty),
                    PaymentSplitEnabled = row["PaymentSplitEnabled"] != DBNull.Value && Convert.ToBoolean(row["PaymentSplitEnabled"]),
                    PreBookingAmount = row["PreBookingAmount"] != DBNull.Value ? Convert.ToDecimal(row["PreBookingAmount"]) : null,
                    PostEventAmount = row["PostEventAmount"] != DBNull.Value ? Convert.ToDecimal(row["PostEventAmount"]) : null,
                    PreBookingStatus = row.GetValue<string>("PreBookingStatus", string.Empty),
                    PostEventStatus = row.GetValue<string>("PostEventStatus", string.Empty),
                    SubTotal = row.GetValue<decimal>("SubTotal"),
                    TaxAmount = row.GetValue<decimal>("TaxAmount"),
                    DiscountAmount = row.GetValue<decimal>("DiscountAmount"),
                    DeliveryCharges = row.GetValue<decimal>("DeliveryCharges"),
                    TotalAmount = row.GetValue<decimal>("TotalAmount"),
                    PaidAmount = row.GetValue<decimal>("PaidAmount"),
                    BalanceAmount = row.GetValue<decimal>("BalanceAmount")
                };

                // Add order items
                if (dataSet.Tables.Count > 1)
                {
                    foreach (DataRow itemRow in dataSet.Tables[1].Rows)
                    {
                        orderDetail.Items.Add(new OrderItemDetailDto
                        {
                            OrderItemId = itemRow.GetValue<long>("OrderItemId"),
                            MenuItemId = itemRow.GetValue<long>("MenuItemId"),
                            MenuItemName = itemRow.GetValue<string>("MenuItemName", string.Empty),
                            Category = itemRow.GetValue<string>("Category", string.Empty),
                            ItemType = itemRow.GetValue<string>("ItemType", string.Empty),
                            Quantity = itemRow.GetValue<int>("Quantity"),
                            UnitPrice = itemRow.GetValue<decimal>("UnitPrice"),
                            TotalPrice = itemRow.GetValue<decimal>("TotalPrice"),
                            ImageUrl = itemRow.GetValue<string>("ImageUrl", string.Empty),
                            SpecialRequest = itemRow.GetValue<string>("SpecialRequest", string.Empty),
                            PackageSelections = itemRow.GetValue<string>("PackageSelections", string.Empty)
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
                        c_modifieddate = @ModifiedDate");

                var parameters = new List<NpgsqlParameter>
                {
                    new NpgsqlParameter("@OrderId", orderId),
                    new NpgsqlParameter("@NewStatus", statusUpdate.NewStatus),
                    new NpgsqlParameter("@ModifiedDate", DateTime.Now)
                };

                if (statusUpdate.EstimatedDeliveryTime.HasValue)
                {
                    query.Append(", c_estimated_delivery_time = @EstimatedDeliveryTime");
                    parameters.Add(new NpgsqlParameter("@EstimatedDeliveryTime", statusUpdate.EstimatedDeliveryTime.Value));
                }

                query.Append(" WHERE c_orderid = @OrderId;");

                // Insert status history (if you have a status history table)
                query.Append($@"
                    INSERT INTO {Table.SysOrderStatusHistory}
                    (c_orderid, c_status, c_remarks, c_modifieddate)
                    VALUES (@OrderId, @NewStatus, @Comments, @ModifiedDate);");

                parameters.Add(new NpgsqlParameter("@Comments", statusUpdate.Comments ?? ""));

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
                        c_history_id AS StatusId,
                        c_status AS Status,
                        c_modifieddate AS ChangedDate,
                        '' AS ChangedBy,
                        c_remarks AS Comments
                    FROM {Table.SysOrderStatusHistory}
                    WHERE c_orderid = @OrderId
                    ORDER BY c_modifieddate DESC";

                var parameters = new[] { new NpgsqlParameter("@OrderId", orderId) };
                var dataTable = await Task.Run(() => _dbHelper.ExecuteAsync(query, parameters));

                var history = new List<OrderStatusHistoryDto>();
                foreach (DataRow row in dataTable.Rows)
                {
                    history.Add(new OrderStatusHistoryDto
                    {
                        StatusId = row.GetValue<long>("StatusId"),
                        Status = row.GetValue<string>("Status", string.Empty),
                        ChangedDate = row.GetValue<DateTime>("ChangedDate"),
                        ChangedBy = row.GetValue<string>("ChangedBy", string.Empty),
                        Comments = row.GetValue<string>("Comments", string.Empty)
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
                        COALESCE(SUM(c_total_amount), 0) AS TotalRevenue,
                        COALESCE(AVG(c_total_amount), 0) AS AverageOrderValue
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId";

                var parameters = new[] { new NpgsqlParameter("@OwnerId", ownerId) };
                var dataTable = await Task.Run(() => _dbHelper.ExecuteAsync(query, parameters));

                if (dataTable.Rows.Count > 0)
                {
                    var row = dataTable.Rows[0];
                    return new OrderStatsDto
                    {
                        TotalOrders = row.GetValue<int>("TotalOrders"),
                        PendingOrders = row.GetValue<int>("PendingOrders"),
                        ConfirmedOrders = row.GetValue<int>("ConfirmedOrders"),
                        CompletedOrders = row.GetValue<int>("CompletedOrders"),
                        CancelledOrders = row.GetValue<int>("CancelledOrders"),
                        TotalRevenue = row.GetValue<decimal>("TotalRevenue"),
                        AverageOrderValue = row.GetValue<decimal>("AverageOrderValue")
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
                        SUM(CASE WHEN CAST(c_createddate AS DATE) = CAST(NOW() AS DATE) THEN 1 ELSE 0 END) AS TodayRequests,
                        SUM(CASE WHEN c_createddate >= NOW() - INTERVAL '7 days' THEN 1 ELSE 0 END) AS WeekRequests,
                        SUM(CASE WHEN c_createddate >= NOW() - INTERVAL '30 days' THEN 1 ELSE 0 END) AS MonthRequests,
                        SUM(CASE WHEN c_order_status = 'Pending' THEN 1 ELSE 0 END) AS TotalPending,
                        SUM(CASE WHEN c_order_status = 'Confirmed' THEN 1 ELSE 0 END) AS TotalConfirmed,
                        SUM(CASE WHEN c_order_status = 'Cancelled' THEN 1 ELSE 0 END) AS TotalRejected
                    FROM {Table.SysOrders}
                    WHERE c_ownerid = @OwnerId AND c_isactive = TRUE";

                var parameters = new[] { new NpgsqlParameter("@OwnerId", ownerId) };
                var dataTable = await Task.Run(() => _dbHelper.ExecuteAsync(query, parameters));

                if (dataTable.Rows.Count > 0)
                {
                    var row = dataTable.Rows[0];
                    return new BookingRequestStatsDto
                    {
                        TodayRequests = row.GetValue<int>("TodayRequests"),
                        WeekRequests = row.GetValue<int>("WeekRequests"),
                        MonthRequests = row.GetValue<int>("MonthRequests"),
                        TotalPending = row.GetValue<int>("TotalPending"),
                        TotalConfirmed = row.GetValue<int>("TotalConfirmed"),
                        TotalRejected = row.GetValue<int>("TotalRejected")
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
                    new NpgsqlParameter("@OrderId", orderId),
                    new NpgsqlParameter("@OwnerId", ownerId)
                };

                var result = await Task.Run(() => _dbHelper.ExecuteScalar(query, parameters));
                return result != null && Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error validating order ownership: {ex.Message}", ex);
            }
        }

        // ===================================
        // GET SAMPLE REQUESTS LIST
        // ===================================
        public async Task<PaginatedSampleRequestsDto> GetSampleRequestsList(long ownerId, int page, int pageSize, string? statusFilter, string? searchTerm)
        {
            try
            {
                var whereClause = "WHERE so.c_ownerid = @OwnerId AND so.c_is_deleted = FALSE";
                var parameters = new List<NpgsqlParameter> { new NpgsqlParameter("@OwnerId", ownerId) };

                if (!string.IsNullOrEmpty(statusFilter))
                {
                    whereClause += " AND so.c_status = @StatusFilter";
                    parameters.Add(new NpgsqlParameter("@StatusFilter", statusFilter));
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    whereClause += " AND (u.c_name LIKE @SearchTerm OR CAST(so.c_sample_order_id AS NVARCHAR) LIKE @SearchTerm)";
                    parameters.Add(new NpgsqlParameter("@SearchTerm", $"%{searchTerm}%"));
                }

                var dataQuery = $@"
                    SELECT
                        so.c_sample_order_id   AS SampleOrderID,
                        CAST(NULL AS BIGINT)   AS LinkedOrderId,
                        CAST(NULL AS BIGINT)   AS LinkedOrderItemId,
                        'sample-order'         AS SourceType,
                        CAST(NULL AS NVARCHAR(100)) AS ParentOrderNumber,
                        u.c_name               AS CustomerName,
                        u.c_mobile             AS CustomerPhone,
                        so.c_sample_price_total AS SamplePriceTotal,
                        so.c_delivery_charge   AS DeliveryCharge,
                        so.c_total_amount      AS TotalAmount,
                        so.c_status            AS Status,
                        so.c_payment_status    AS PaymentStatus,
                        so.c_pickup_address    AS PickupAddress,
                        so.c_createddate       AS RequestedDate,
                        so.c_rejection_reason  AS RejectionReason,
                        COALESCE(items.ItemNames, '') AS ItemNames
                    FROM {Table.SysSampleOrders} so
                    INNER JOIN {Table.SysUser} u ON so.c_userid = u.c_userid
                    LEFT JOIN LATERAL (
                        SELECT STRING_AGG(COALESCE(soi.c_menu_item_name, ''), ', ' ORDER BY soi.c_menu_item_name) AS ItemNames
                        FROM (
                            SELECT c_menu_item_name
                            FROM {Table.SysSampleOrderItems} soi
                            WHERE soi.c_sample_order_id = so.c_sample_order_id
                            LIMIT 6
                        ) soi
                    ) items ON TRUE
                    {whereClause}
                    ORDER BY so.c_createddate DESC";

                var dataTable = await Task.Run(() => _dbHelper.ExecuteAsync(dataQuery, CloneParameters(parameters)));

                var requests = new List<SampleRequestListItemDto>();
                foreach (DataRow row in dataTable.Rows)
                {
                    var rawItems = row.GetValue<string>("ItemNames", string.Empty);
                    requests.Add(new SampleRequestListItemDto
                    {
                        SampleOrderId = row.GetValue<long>("SampleOrderID"),
                        LinkedOrderId = row.GetValue<long?>("LinkedOrderId"),
                        LinkedOrderItemId = row.GetValue<long?>("LinkedOrderItemId"),
                        SourceType = row.GetValue<string>("SourceType", "sample-order"),
                        ParentOrderNumber = row.GetValue<string>("ParentOrderNumber", string.Empty),
                        CustomerName = row.GetValue<string>("CustomerName", string.Empty),
                        CustomerPhone = row.GetValue<string>("CustomerPhone", string.Empty),
                        SamplePriceTotal = row.GetValue<decimal>("SamplePriceTotal"),
                        DeliveryCharge = row.GetValue<decimal>("DeliveryCharge"),
                        TotalAmount = row.GetValue<decimal>("TotalAmount"),
                        Status = row.GetValue<string>("Status", string.Empty),
                        PaymentStatus = row.GetValue<string>("PaymentStatus", string.Empty),
                        PickupAddress = row.GetValue<string>("PickupAddress", string.Empty),
                        RequestedDate = row.GetValue<DateTime>("RequestedDate"),
                        RejectionReason = row.GetValue<string>("RejectionReason", string.Empty),
                        SampleItems = string.IsNullOrEmpty(rawItems)
                            ? []
                            : rawItems.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => s.Trim())
                                .Where(s => !string.IsNullOrEmpty(s))
                                .ToList()
                    });
                }

                requests.AddRange(await GetOrderLinkedSampleRequests(ownerId, statusFilter, searchTerm));

                var mergedRequests = requests
                    .OrderByDescending(request => request.RequestedDate)
                    .ToList();

                int totalCount = mergedRequests.Count;
                int totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
                var pagedRequests = mergedRequests
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return new PaginatedSampleRequestsDto
                {
                    Requests = pagedRequests,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting sample requests list: {ex.Message}", ex);
            }
        }

        // ===================================
        // ACTION SAMPLE REQUEST (Accept / Reject)
        // ===================================
        public async Task<bool> ActionSampleRequest(long ownerId, long sampleOrderId, SampleRequestActionDto action)
        {
            try
            {
                if (string.Equals(action.SourceType, "event-order", StringComparison.OrdinalIgnoreCase))
                {
                    return await ActionOrderLinkedSampleRequest(ownerId, sampleOrderId, action);
                }

                // Validate ownership
                var validateQuery = $"SELECT COUNT(*) FROM {Table.SysSampleOrders} WHERE c_sample_order_id = @SampleOrderId AND c_ownerid = @OwnerId AND c_is_deleted = FALSE";
                var validateParams = new[]
                {
                    new NpgsqlParameter("@SampleOrderId", sampleOrderId),
                    new NpgsqlParameter("@OwnerId", ownerId)
                };
                var validateResult = await Task.Run(() => _dbHelper.ExecuteScalar(validateQuery, validateParams));
                if (validateResult == null || Convert.ToInt32(validateResult) == 0)
                    throw new UnauthorizedAccessException("Sample order does not belong to this owner.");

                string newStatus;
                string updateQuery;
                NpgsqlParameter[] updateParams;

                if (action.Action?.Equals("Accept", StringComparison.OrdinalIgnoreCase) == true)
                {
                    newStatus = "SAMPLE_ACCEPTED";
                    updateQuery = $@"
                        UPDATE {Table.SysSampleOrders}
                        SET c_status = @Status,
                            c_partner_response_date = NOW(),
                            c_modifieddate = NOW()
                        WHERE c_sample_order_id = @SampleOrderId";
                    updateParams = new[]
                    {
                        new NpgsqlParameter("@Status", newStatus),
                        new NpgsqlParameter("@SampleOrderId", sampleOrderId)
                    };
                }
                else
                {
                    newStatus = "SAMPLE_REJECTED";
                    updateQuery = $@"
                        UPDATE {Table.SysSampleOrders}
                        SET c_status = @Status,
                            c_rejection_reason = @RejectionReason,
                            c_partner_response_date = NOW(),
                            c_modifieddate = NOW()
                        WHERE c_sample_order_id = @SampleOrderId";
                    updateParams = new[]
                    {
                        new NpgsqlParameter("@Status", newStatus),
                        new NpgsqlParameter("@RejectionReason", action.RejectionReason ?? string.Empty),
                        new NpgsqlParameter("@SampleOrderId", sampleOrderId)
                    };
                }

                var result = await Task.Run(() => _dbHelper.ExecuteNonQuery(updateQuery, updateParams));
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error actioning sample request: {ex.Message}", ex);
            }
        }

        private async Task<List<SampleRequestListItemDto>> GetOrderLinkedSampleRequests(long ownerId, string? statusFilter, string? searchTerm)
        {
            var query = $@"
                SELECT
                    o.c_orderid AS OrderId,
                    o.c_order_number AS OrderNumber,
                    oi.c_order_item_id AS OrderItemId,
                    u.c_name AS CustomerName,
                    u.c_mobile AS CustomerPhone,
                    o.c_delivery_address AS PickupAddress,
                    o.c_createddate AS RequestedDate,
                    oi.c_package_selections AS PackageSelections,
                    oi.c_total_price AS ItemTotal
                FROM {Table.SysOrders} o
                INNER JOIN {Table.SysOrderItems} oi ON o.c_orderid = oi.c_orderid
                INNER JOIN {Table.SysUser} u ON o.c_userid = u.c_userid
                WHERE o.c_ownerid = @OwnerId
                  AND COALESCE(oi.c_package_selections, '') <> ''
                  AND oi.c_package_selections LIKE '%sampleTasteSelections%'";

            var dataTable = await Task.Run(() => _dbHelper.ExecuteAsync(query, new[] { new NpgsqlParameter("@OwnerId", ownerId) }));
            var results = new List<SampleRequestListItemDto>();

            foreach (DataRow row in dataTable.Rows)
            {
                var packageSelectionsJson = row.GetValue<string>("PackageSelections", string.Empty);
                if (string.IsNullOrWhiteSpace(packageSelectionsJson))
                {
                    continue;
                }

                try
                {
                    var payload = JObject.Parse(packageSelectionsJson);
                    var sampleSelections = payload["sampleTasteSelections"] as JArray;
                    if (sampleSelections == null || sampleSelections.Count == 0)
                    {
                        continue;
                    }

                    var sampleStatus = payload["sampleTasteMeta"]?["status"]?.ToString();
                    if (!string.IsNullOrEmpty(statusFilter) && !string.Equals(statusFilter, sampleStatus, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var sampleItemNames = sampleSelections
                        .SelectMany(category => (category["selectedItems"] as JArray ?? new JArray())
                            .Select(item => item?["name"]?.ToString()))
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .ToList();

                    var customerName = row.GetValue<string>("CustomerName", string.Empty);
                    var orderNumber = row.GetValue<string>("OrderNumber", string.Empty);
                    var searchMatches = string.IsNullOrWhiteSpace(searchTerm)
                        || customerName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                        || orderNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                        || sampleItemNames.Any(name => name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));

                    if (!searchMatches)
                    {
                        continue;
                    }

                    results.Add(new SampleRequestListItemDto
                    {
                        SampleOrderId = row.GetValue<long>("OrderItemId"),
                        LinkedOrderId = row.GetValue<long>("OrderId"),
                        LinkedOrderItemId = row.GetValue<long>("OrderItemId"),
                        SourceType = "event-order",
                        ParentOrderNumber = orderNumber,
                        CustomerName = customerName,
                        CustomerPhone = row.GetValue<string>("CustomerPhone", string.Empty),
                        SamplePriceTotal = 0,
                        DeliveryCharge = 0,
                        TotalAmount = row.GetValue<decimal>("ItemTotal"),
                        Status = sampleStatus ?? "SAMPLE_REQUESTED",
                        PaymentStatus = "Included In Event Order",
                        PickupAddress = row.GetValue<string>("PickupAddress", string.Empty),
                        RequestedDate = row.GetValue<DateTime>("RequestedDate"),
                        RejectionReason = payload["sampleTasteMeta"]?["rejectionReason"]?.ToString(),
                        SampleItems = sampleItemNames
                    });
                }
                catch
                {
                    // Ignore malformed historical payloads
                }
            }

            return results;
        }

        private async Task<bool> ActionOrderLinkedSampleRequest(long ownerId, long sampleOrderId, SampleRequestActionDto action)
        {
            var query = $@"
                SELECT
                    oi.c_order_item_id AS OrderItemId,
                    oi.c_package_selections AS PackageSelections
                FROM {Table.SysOrderItems} oi
                INNER JOIN {Table.SysOrders} o ON oi.c_orderid = o.c_orderid
                WHERE o.c_ownerid = @OwnerId
                  AND oi.c_order_item_id = @OrderItemId
                LIMIT 1";

            var dataTable = await Task.Run(() => _dbHelper.ExecuteAsync(query, new[]
            {
                new NpgsqlParameter("@OwnerId", ownerId),
                new NpgsqlParameter("@OrderItemId", action.LinkedOrderItemId ?? sampleOrderId)
            }));

            if (dataTable.Rows.Count == 0)
            {
                throw new UnauthorizedAccessException("Sample request does not belong to this owner.");
            }

            var payload = JObject.Parse(dataTable.Rows[0].GetValue<string>("PackageSelections", "{}") ?? "{}");
            payload["sampleTasteMeta"] ??= new JObject();
            payload["sampleTasteMeta"]!["status"] = string.Equals(action.Action, "Accept", StringComparison.OrdinalIgnoreCase)
                ? "SAMPLE_ACCEPTED"
                : "SAMPLE_REJECTED";
            payload["sampleTasteMeta"]!["rejectionReason"] = string.Equals(action.Action, "Reject", StringComparison.OrdinalIgnoreCase)
                ? action.RejectionReason ?? string.Empty
                : null;
            payload["sampleTasteMeta"]!["respondedAt"] = DateTime.UtcNow;

            var updateQuery = $@"
                UPDATE {Table.SysOrderItems}
                SET c_package_selections = @PackageSelections
                WHERE c_order_item_id = @OrderItemId";

            var updated = await Task.Run(() => _dbHelper.ExecuteNonQuery(updateQuery, new[]
            {
                new NpgsqlParameter("@PackageSelections", payload.ToString()),
                new NpgsqlParameter("@OrderItemId", action.LinkedOrderItemId ?? sampleOrderId)
            }));

            return updated > 0;
        }

        private static NpgsqlParameter[] CloneParameters(List<NpgsqlParameter> parameters)
        {
            var cloned = new NpgsqlParameter[parameters.Count];
            for (var i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                cloned[i] = new NpgsqlParameter(parameter.ParameterName, parameter.Value ?? DBNull.Value);
            }

            return cloned;
        }
    }
}

