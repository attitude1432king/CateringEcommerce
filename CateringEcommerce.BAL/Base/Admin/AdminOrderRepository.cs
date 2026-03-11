using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace CateringEcommerce.BAL.Base.Admin
{
    public class AdminOrderRepository : IAdminOrderRepository
    {
        private readonly IDatabaseHelper _dbHelper;

        public AdminOrderRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        }

        /// <summary>
        /// Get paginated list of orders with filtering
        /// </summary>
        public async Task<AdminOrderListResponse> GetOrdersAsync(AdminOrderListRequest request)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@PageNumber", request.PageNumber),
                    new SqlParameter("@PageSize", request.PageSize),
                    new SqlParameter("@SearchTerm", (object?)request.SearchTerm ?? DBNull.Value),
                    new SqlParameter("@OrderStatus", (object?)request.OrderStatus ?? DBNull.Value),
                    new SqlParameter("@PaymentStatus", (object?)request.PaymentStatus ?? DBNull.Value),
                    new SqlParameter("@StartDate", (object?)request.StartDate ?? DBNull.Value),
                    new SqlParameter("@EndDate", (object?)request.EndDate ?? DBNull.Value),
                    new SqlParameter("@UserId", (object?)request.UserId ?? DBNull.Value),
                    new SqlParameter("@CateringOwnerId", (object?)request.CateringOwnerId ?? DBNull.Value),
                    new SqlParameter("@MinAmount", (object?)request.MinAmount ?? DBNull.Value),
                    new SqlParameter("@MaxAmount", (object?)request.MaxAmount ?? DBNull.Value),
                    new SqlParameter("@SortBy", request.SortBy ?? "CreatedDate"),
                    new SqlParameter("@SortOrder", request.SortOrder ?? "DESC"),
                    new SqlParameter("@TotalCount", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };

                var dt = await _dbHelper.ExecuteStoredProcedureAsync<DataTable>("sp_Admin_GetOrders", parameters);

                var orders = new List<AdminOrderListItem>();
                if (dt == null)
                {
                    int totalCountWhenNull = parameters[13].Value != DBNull.Value ? Convert.ToInt32(parameters[13].Value) : 0;
                    int totalPagesWhenNull = (int)Math.Ceiling((double)totalCountWhenNull / request.PageSize);

                    return new AdminOrderListResponse
                    {
                        Orders = orders,
                        TotalCount = totalCountWhenNull,
                        PageNumber = request.PageNumber,
                        PageSize = request.PageSize,
                        TotalPages = totalPagesWhenNull
                    };
                }

                foreach (DataRow row in dt.Rows)
                {
                    orders.Add(new AdminOrderListItem
                    {
                        OrderId = Convert.ToInt64(row["c_orderid"]),
                        OrderNumber = row["c_order_number"]?.ToString() ?? string.Empty,
                        UserId = Convert.ToInt64(row["c_userid"]),
                        UserName = row["c_user_name"]?.ToString() ?? string.Empty,
                        UserEmail = row["c_user_email"]?.ToString(),
                        UserPhone = row["c_user_phone"]?.ToString(),
                        CateringOwnerId = Convert.ToInt64(row["c_cateringownerid"]),
                        CateringName = row["c_catering_name"]?.ToString() ?? string.Empty,
                        CateringOwnerName = row["c_owner_name"]?.ToString(),
                        EventDate = Convert.ToDateTime(row["c_event_date"]),
                        EventType = row["c_event_type"]?.ToString() ?? string.Empty,
                        GuestCount = Convert.ToInt32(row["c_guest_count"]),
                        TotalAmount = Convert.ToDecimal(row["c_total_amount"]),
                        OrderStatus = row["c_order_status"]?.ToString() ?? string.Empty,
                        PaymentStatus = row["c_payment_status"]?.ToString() ?? string.Empty,
                        CreatedDate = Convert.ToDateTime(row["c_createddate"]),
                        UpdatedDate = row["c_modifieddate"] != DBNull.Value ? Convert.ToDateTime(row["c_modifieddate"]) : null,
                        ContactPerson = row["c_contact_person"]?.ToString(),
                        ContactPhone = row["c_contact_phone"]?.ToString()
                    });
                }

                int totalCount = parameters[13].Value != DBNull.Value ? Convert.ToInt32(parameters[13].Value) : 0;
                int totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

                return new AdminOrderListResponse
                {
                    Orders = orders,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving orders: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get order details by ID
        /// </summary>
        public async Task<AdminOrderDetail?> GetOrderByIdAsync(long orderId)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@OrderId", orderId)
                };

                var dt = await _dbHelper.ExecuteStoredProcedureAsync<DataTable>("sp_Admin_GetOrderById", parameters);

                if (dt == null || dt.Rows.Count == 0)
                    return null;

                var row = dt.Rows[0];

                var orderDetail = new AdminOrderDetail
                {
                    OrderId = Convert.ToInt64(row["c_orderid"]),
                    OrderNumber = row["c_order_number"]?.ToString() ?? string.Empty,
                    OrderStatus = row["c_order_status"]?.ToString() ?? string.Empty,
                    CreatedDate = Convert.ToDateTime(row["c_createddate"]),
                    UpdatedDate = row["c_modifieddate"] != DBNull.Value ? Convert.ToDateTime(row["c_modifieddate"]) : null,

                    UserId = Convert.ToInt64(row["c_userid"]),
                    UserName = row["c_user_name"]?.ToString() ?? string.Empty,
                    UserEmail = row["c_user_email"]?.ToString(),
                    UserPhone = row["c_user_phone"]?.ToString(),

                    CateringOwnerId = Convert.ToInt64(row["c_cateringownerid"]),
                    CateringName = row["c_catering_name"]?.ToString() ?? string.Empty,
                    CateringOwnerName = row["c_owner_name"]?.ToString(),
                    CateringOwnerPhone = row["c_owner_phone"]?.ToString(),

                    EventDate = Convert.ToDateTime(row["c_event_date"]),
                    EventType = row["c_event_type"]?.ToString() ?? string.Empty,
                    GuestCount = Convert.ToInt32(row["c_guest_count"]),
                    EventLocation = row["c_event_location"]?.ToString(),
                    VenueAddress = row["c_venue_address"]?.ToString(),
                    ContactPerson = row["c_contact_person"]?.ToString(),
                    ContactPhone = row["c_contact_phone"]?.ToString(),
                    ContactEmail = row["c_contact_email"]?.ToString(),

                    TotalAmount = Convert.ToDecimal(row["c_total_amount"]),
                    PaymentStatus = row["c_payment_status"]?.ToString() ?? string.Empty,
                    AdvanceAmount = row["c_advance_amount"] != DBNull.Value ? Convert.ToDecimal(row["c_advance_amount"]) : null,
                    BalanceAmount = row["c_balance_amount"] != DBNull.Value ? Convert.ToDecimal(row["c_balance_amount"]) : null,
                    CommissionAmount = row["c_commission_amount"] != DBNull.Value ? Convert.ToDecimal(row["c_commission_amount"]) : null,
                    CommissionPercentage = row["c_commission_percentage"] != DBNull.Value ? Convert.ToDecimal(row["c_commission_percentage"]) : null
                };

                // Get order items
                orderDetail.OrderItems = await GetOrderItemsAsync(orderId);

                // Get payment stages
                orderDetail.PaymentStages = await GetOrderPaymentStagesAsync(orderId);

                // Get status history
                orderDetail.StatusHistory = await GetOrderStatusHistoryAsync(orderId);

                return orderDetail;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving order details: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get order items for an order
        /// </summary>
        private async Task<List<AdminOrderItemDetail>> GetOrderItemsAsync(long orderId)
        {
            try
            {
                string query = $@"
                    SELECT
                        c_order_item_id,
                        c_item_name,
                        c_item_type,
                        c_quantity,
                        c_price,
                        c_total_price
                    FROM {Table.SysOrderItems}
                    WHERE c_orderid = @OrderId
                    ORDER BY c_order_item_id
                ";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@OrderId", orderId)
                };

                var dt = await _dbHelper.ExecuteAsync(query, parameters);

                var items = new List<AdminOrderItemDetail>();
                if (dt == null)
                {
                    return items;
                }

                foreach (DataRow row in dt.Rows)
                {
                    items.Add(new AdminOrderItemDetail
                    {
                        OrderItemId = Convert.ToInt64(row["c_order_item_id"]),
                        ItemName = row["c_item_name"]?.ToString() ?? string.Empty,
                        ItemType = row["c_item_type"]?.ToString() ?? string.Empty,
                        Quantity = Convert.ToInt32(row["c_quantity"]),
                        Price = Convert.ToDecimal(row["c_price"]),
                        TotalPrice = Convert.ToDecimal(row["c_total_price"])
                    });
                }

                return items;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving order items: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get payment stages for an order
        /// </summary>
        private async Task<List<AdminOrderPaymentStage>> GetOrderPaymentStagesAsync(long orderId)
        {
            try
            {
                string query = $@"
                    SELECT
                        c_payment_stage_id,
                        c_stage_type,
                        c_stage_percentage,
                        c_stage_amount,
                        c_status,
                        c_payment_date,
                        c_due_date,
                        c_payment_method,
                        c_transaction_id
                    FROM {Table.SysOrderPaymentStages}
                    WHERE c_orderid = @OrderId
                    ORDER BY c_payment_stage_id
                ";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@OrderId", orderId)
                };

                var dt = await _dbHelper.ExecuteAsync(query, parameters);

                var stages = new List<AdminOrderPaymentStage>();
                if (dt == null)
                {
                    return stages;
                }

                foreach (DataRow row in dt.Rows)
                {
                    stages.Add(new AdminOrderPaymentStage
                    {
                        PaymentStageId = Convert.ToInt64(row["c_payment_stage_id"]),
                        StageType = row["c_stage_type"]?.ToString() ?? string.Empty,
                        StagePercentage = Convert.ToDecimal(row["c_stage_percentage"]),
                        StageAmount = Convert.ToDecimal(row["c_stage_amount"]),
                        Status = row["c_status"]?.ToString() ?? string.Empty,
                        PaymentDate = row["c_payment_date"] != DBNull.Value ? Convert.ToDateTime(row["c_payment_date"]) : null,
                        DueDate = row["c_due_date"] != DBNull.Value ? Convert.ToDateTime(row["c_due_date"]) : null,
                        PaymentMethod = row["c_payment_method"]?.ToString(),
                        TransactionId = row["c_transaction_id"]?.ToString()
                    });
                }

                return stages;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving payment stages: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get status history for an order
        /// </summary>
        private async Task<List<AdminOrderStatusHistory>> GetOrderStatusHistoryAsync(long orderId)
        {
            try
            {
                string query = $@"
                    SELECT
                        c_history_id,
                        c_status,
                        c_remarks,
                        c_modifieddate
                    FROM {Table.SysOrderStatusHistory}
                    WHERE c_orderid = @OrderId
                    ORDER BY c_modifieddate DESC
                ";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@OrderId", orderId)
                };

                var dt = await _dbHelper.ExecuteAsync(query, parameters);

                var history = new List<AdminOrderStatusHistory>();
                if (dt == null)
                {
                    return history;
                }

                foreach (DataRow row in dt.Rows)
                {
                    history.Add(new AdminOrderStatusHistory
                    {
                        HistoryId = Convert.ToInt64(row["c_history_id"]),
                        Status = row["c_status"]?.ToString() ?? string.Empty,
                        Remarks = row["c_remarks"]?.ToString(),
                        UpdatedDate = Convert.ToDateTime(row["c_modifieddate"])
                    });
                }

                return history;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving status history: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Update order status
        /// </summary>
        public async Task<bool> UpdateOrderStatusAsync(AdminOrderUpdateStatusRequest request)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@OrderId", request.OrderId),
                    new SqlParameter("@NewStatus", request.NewStatus),
                    new SqlParameter("@Remarks", (object?)request.Remarks ?? DBNull.Value),
                    new SqlParameter("@UpdatedBy", request.UpdatedBy),
                    new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output },
                    new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output }
                };

                await _dbHelper.ExecuteStoredProcedureAsync<dynamic>("sp_Admin_UpdateOrderStatus", parameters);

                var success = parameters[4].Value != null && (bool)parameters[4].Value;
                var errorMessage = parameters[5].Value as string;

                if (!success && !string.IsNullOrEmpty(errorMessage))
                {
                    throw new InvalidOperationException($"Order status update failed: {errorMessage}");
                }

                return success;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating order status: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get order statistics
        /// </summary>
        public async Task<AdminOrderStatsResponse> GetOrderStatsAsync()
        {
            try
            {
                var dt = await _dbHelper.ExecuteStoredProcedureAsync<DataTable>("sp_Admin_GetOrderStats", Array.Empty<SqlParameter>());

                if (dt == null || dt.Rows.Count == 0)
                {
                    return new AdminOrderStatsResponse();
                }

                var row = dt.Rows[0];

                return new AdminOrderStatsResponse
                {
                    TotalOrders = Convert.ToInt32(row["TotalOrders"]),
                    PendingOrders = Convert.ToInt32(row["PendingOrders"]),
                    ConfirmedOrders = Convert.ToInt32(row["ConfirmedOrders"]),
                    InProgressOrders = Convert.ToInt32(row["InProgressOrders"]),
                    CompletedOrders = Convert.ToInt32(row["CompletedOrders"]),
                    CancelledOrders = Convert.ToInt32(row["CancelledOrders"]),
                    TotalRevenue = Convert.ToDecimal(row["TotalRevenue"]),
                    PendingRevenue = Convert.ToDecimal(row["PendingRevenue"]),
                    TodayOrders = Convert.ToDecimal(row["TodayOrders"]),
                    ThisMonthOrders = Convert.ToDecimal(row["ThisMonthOrders"])
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving order stats: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Cancel order (admin initiated)
        /// </summary>
        public async Task<bool> CancelOrderAsync(long orderId, long adminId, string reason)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@OrderId", orderId),
                    new SqlParameter("@AdminId", adminId),
                    new SqlParameter("@CancellationReason", reason),
                    new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output },
                    new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output }
                };

                await _dbHelper.ExecuteStoredProcedureAsync<dynamic>("sp_Admin_CancelOrder", parameters);

                var success = parameters[3].Value != null && (bool)parameters[3].Value;
                var errorMessage = parameters[4].Value as string;

                if (!success && !string.IsNullOrEmpty(errorMessage))
                {
                    throw new InvalidOperationException($"Order cancellation failed: {errorMessage}");
                }

                return success;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error cancelling order: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Export orders to CSV
        /// </summary>
        public async Task<byte[]> ExportOrdersAsync(AdminOrderListRequest request)
        {
            try
            {
                // Get all orders without pagination
                request.PageSize = int.MaxValue;
                var response = await GetOrdersAsync(request);

                var csv = new StringBuilder();
                csv.AppendLine("Order Number,Customer Name,Customer Email,Catering Name,Event Date,Guest Count,Total Amount,Order Status,Payment Status,Created Date");

                foreach (var order in response.Orders)
                {
                    csv.AppendLine($"\"{order.OrderNumber}\",\"{order.UserName}\",\"{order.UserEmail}\",\"{order.CateringName}\",\"{order.EventDate:yyyy-MM-dd}\",{order.GuestCount},\"{order.TotalAmount}\",\"{order.OrderStatus}\",\"{order.PaymentStatus}\",\"{order.CreatedDate:yyyy-MM-dd HH:mm}\"");
                }

                return Encoding.UTF8.GetBytes(csv.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception($"Error exporting orders: {ex.Message}", ex);
            }
        }
    }
}
