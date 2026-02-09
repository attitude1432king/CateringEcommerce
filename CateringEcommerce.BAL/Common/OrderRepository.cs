using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.User;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CateringEcommerce.BAL.Common
{
    public class OrderRepository: IOrderRepository
    {
        private readonly IDatabaseHelper _dbHelper;
        public OrderRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // ===================================
        // INSERT ORDER
        // ===================================
        public async Task<long> InsertOrderAsync(long userId, CreateOrderDto orderData, string orderNumber)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append($@"
                    INSERT INTO {Table.SysOrders} (
                        c_userid, c_ownerid, c_order_number, c_event_date, c_event_time,
                        c_event_type, c_event_location, c_guest_count, c_special_instructions,
                        c_delivery_address, c_contact_person, c_contact_phone, c_contact_email,
                        c_base_amount, c_tax_amount, c_delivery_charges, c_discount_amount, c_total_amount,
                        c_payment_method, c_payment_status, c_order_status,
                        c_payment_split_enabled, c_prebooking_amount, c_postevent_amount, c_prebooking_status, c_postevent_status,
                        c_event_latitude, c_event_longitude, c_event_place_id, c_saved_address_id,
                        c_created_date, c_isactive
                    ) VALUES (
                        @UserId, @CateringId, @OrderNumber, @EventDate, @EventTime,
                        @EventType, @EventLocation, @GuestCount, @SpecialInstructions,
                        @DeliveryAddress, @ContactPerson, @ContactPhone, @ContactEmail,
                        @BaseAmount, @TaxAmount, @DeliveryCharges, @DiscountAmount, @TotalAmount,
                        @PaymentMethod, @PaymentStatus, @OrderStatus,
                        @PaymentSplitEnabled, @PreBookingAmount, @PostEventAmount, @PreBookingStatus, @PostEventStatus,
                        @EventLatitude, @EventLongitude, @EventPlaceId, @SavedAddressId,
                        GETDATE(), 1
                    );
                    SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
                ");

                string paymentStatus = orderData.PaymentMethod == "COD" ? "Pending" : "AwaitingVerification";
                string? preBookingStatus = orderData.EnableSplitPayment ? "Pending" : null;
                string? postEventStatus = orderData.EnableSplitPayment ? "Pending" : null;

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@CateringId", orderData.CateringId),
                    new SqlParameter("@OrderNumber", orderNumber),
                    new SqlParameter("@EventDate", orderData.EventDate),
                    new SqlParameter("@EventTime", orderData.EventTime),
                    new SqlParameter("@EventType", orderData.EventType),
                    new SqlParameter("@EventLocation", orderData.EventLocation),
                    new SqlParameter("@GuestCount", orderData.GuestCount),
                    new SqlParameter("@SpecialInstructions", (object)orderData.SpecialInstructions ?? DBNull.Value),
                    new SqlParameter("@DeliveryAddress", orderData.DeliveryAddress),
                    new SqlParameter("@ContactPerson", orderData.ContactPerson),
                    new SqlParameter("@ContactPhone", orderData.ContactPhone),
                    new SqlParameter("@ContactEmail", orderData.ContactEmail),
                    new SqlParameter("@BaseAmount", orderData.BaseAmount),
                    new SqlParameter("@TaxAmount", orderData.TaxAmount),
                    new SqlParameter("@DeliveryCharges", orderData.DeliveryCharges),
                    new SqlParameter("@DiscountAmount", orderData.DiscountAmount),
                    new SqlParameter("@TotalAmount", orderData.TotalAmount),
                    new SqlParameter("@PaymentMethod", orderData.PaymentMethod),
                    new SqlParameter("@PaymentStatus", paymentStatus),
                    new SqlParameter("@OrderStatus", "Pending"),
                    new SqlParameter("@PaymentSplitEnabled", orderData.EnableSplitPayment),
                    new SqlParameter("@PreBookingAmount", (object)orderData.PreBookingAmount ?? DBNull.Value),
                    new SqlParameter("@PostEventAmount", (object)orderData.PostEventAmount ?? DBNull.Value),
                    new SqlParameter("@PreBookingStatus", (object)preBookingStatus ?? DBNull.Value),
                    new SqlParameter("@PostEventStatus", (object)postEventStatus ?? DBNull.Value),
                    new SqlParameter("@EventLatitude", (object)orderData.EventLatitude ?? DBNull.Value),
                    new SqlParameter("@EventLongitude", (object)orderData.EventLongitude ?? DBNull.Value),
                    new SqlParameter("@EventPlaceId", (object)orderData.EventPlaceId ?? DBNull.Value),
                    new SqlParameter("@SavedAddressId", (object)orderData.SavedAddressId ?? DBNull.Value)
                };

                DataTable dt = await _dbHelper.ExecuteAsync(query.ToString(), parameters);
                if (dt.Rows.Count > 0)
                {
                    return Convert.ToInt64(dt.Rows[0][0]);
                }

                return 0;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error inserting order.", ex);
            }
        }

        // ===================================
        // INSERT ORDER ITEMS (BATCH)
        // ===================================
        public async Task<bool> InsertOrderItemsAsync(long orderId, List<CreateOrderItemDto> items)
        {
            try
            {
                if (items == null || items.Count == 0)
                    return false;

                StringBuilder query = new StringBuilder();
                List<SqlParameter> parameters = new List<SqlParameter>();

                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    query.Append($@"
                        INSERT INTO {Table.SysOrderItems} (
                            c_orderid, c_item_type, c_item_id, c_item_name, c_quantity,
                            c_unit_price, c_total_price, c_package_selections, c_created_date
                        ) VALUES (
                            @OrderId{i}, @ItemType{i}, @ItemId{i}, @ItemName{i}, @Quantity{i},
                            @UnitPrice{i}, @TotalPrice{i}, @PackageSelections{i}, GETDATE()
                        );
                    ");

                    parameters.Add(new SqlParameter($"@OrderId{i}", orderId));
                    parameters.Add(new SqlParameter($"@ItemType{i}", item.ItemType));
                    parameters.Add(new SqlParameter($"@ItemId{i}", item.ItemId));
                    parameters.Add(new SqlParameter($"@ItemName{i}", item.ItemName));
                    parameters.Add(new SqlParameter($"@Quantity{i}", item.Quantity));
                    parameters.Add(new SqlParameter($"@UnitPrice{i}", item.UnitPrice));
                    parameters.Add(new SqlParameter($"@TotalPrice{i}", item.TotalPrice));
                    parameters.Add(new SqlParameter($"@PackageSelections{i}", (object)item.PackageSelections ?? DBNull.Value));
                }

                await _dbHelper.ExecuteNonQueryAsync(query.ToString(), parameters.ToArray());
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error inserting order items.", ex);
            }
        }

        // ===================================
        // INSERT ORDER PAYMENT
        // ===================================
        public async Task<bool> InsertOrderPaymentAsync(long orderId, string paymentMethod, decimal amount, string? paymentProofPath = null, string? paymentStageType = "Full")
        {
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append($@"
                    INSERT INTO {Table.SysOrderPayments} (
                        c_orderid, c_payment_method, c_amount, c_status, c_payment_proof_path, c_payment_stage_type, c_created_date
                    ) VALUES (
                        @OrderId, @PaymentMethod, @Amount, @Status, @PaymentProofPath, @PaymentStageType, GETDATE()
                    );
                ");

                string status = paymentMethod == "COD" ? "Pending" : "AwaitingVerification";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@OrderId", orderId),
                    new SqlParameter("@PaymentMethod", paymentMethod),
                    new SqlParameter("@Amount", amount),
                    new SqlParameter("@Status", status),
                    new SqlParameter("@PaymentProofPath", (object)paymentProofPath ?? DBNull.Value),
                    new SqlParameter("@PaymentStageType", (object)paymentStageType ?? DBNull.Value)
                };

                await _dbHelper.ExecuteNonQueryAsync(query.ToString(), parameters);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error inserting order payment.", ex);
            }
        }

        // ===================================
        // INSERT ORDER STATUS HISTORY
        // ===================================
        public async Task<bool> InsertOrderStatusHistoryAsync(long orderId, string status, string? remarks = null, long? updatedBy = null)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append($@"
                    INSERT INTO {Table.SysOrderStatusHistory} (
                        c_orderid, c_status, c_remarks, c_updated_by, c_updated_date
                    ) VALUES (
                        @OrderId, @Status, @Remarks, @UpdatedBy, GETDATE()
                    );
                ");

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@OrderId", orderId),
                    new SqlParameter("@Status", status),
                    new SqlParameter("@Remarks", (object)remarks ?? DBNull.Value),
                    new SqlParameter("@UpdatedBy", (object)updatedBy ?? DBNull.Value)
                };

                await _dbHelper.ExecuteNonQueryAsync(query.ToString(), parameters);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error inserting order status history.", ex);
            }
        }

        // ===================================
        // GENERATE ORDER NUMBER
        // ===================================
        public async Task<string> GenerateOrderNumberAsync()
        {
            try
            {
                string datePrefix = DateTime.Now.ToString("yyyyMMdd");
                string query = $@"
                    SELECT COUNT(*)
                    FROM {Table.SysOrders}
                    WHERE c_order_number LIKE 'ORD-{datePrefix}-%'
                ";

                DataTable dt = await _dbHelper.ExecuteAsync(query, null);
                int count = dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0][0]) : 0;
                int nextNumber = count + 1;

                return $"ORD-{datePrefix}-{nextNumber:D5}";
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error generating order number.", ex);
            }
        }

        // ===================================
        // GET ORDERS BY USER ID (PAGINATED)
        // ===================================
        public async Task<List<OrderListItemDto>> GetOrdersByUserIdAsync(long userId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                int offset = (pageNumber - 1) * pageSize;

                StringBuilder query = new StringBuilder();
                query.Append($@"
                    SELECT
                        o.c_orderid AS OrderId,
                        o.c_order_number AS OrderNumber,
                        o.c_ownerid AS CateringId,
                        ca.c_catering_name AS CateringName,
                        ca.c_logo_path AS CateringLogo,
                        o.c_event_date AS EventDate,
                        o.c_event_type AS EventType,
                        o.c_guest_count AS GuestCount,
                        o.c_total_amount AS TotalAmount,
                        o.c_order_status AS OrderStatus,
                        o.c_payment_status AS PaymentStatus,
                        o.c_payment_method AS PaymentMethod,
                        o.c_created_date AS CreatedDate
                    FROM {Table.SysOrders} o
                    LEFT JOIN {Table.SysCateringOwner} ca ON o.c_ownerid = ca.c_ownerid
                    WHERE o.c_userid = @UserId AND o.c_isactive = 1
                    ORDER BY o.c_created_date DESC
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY
                ");

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@Offset", offset),
                    new SqlParameter("@PageSize", pageSize)
                };

                DataTable dt = await _dbHelper.ExecuteAsync(query.ToString(), parameters);
                return MapToOrderListItemDto(dt);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error fetching user orders.", ex);
            }
        }

        // ===================================
        // GET ORDER BY ID
        // ===================================
        public async Task<OrderDto?> GetOrderByIdAsync(long orderId, long userId)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append($@"
                    SELECT
                        o.c_orderid AS OrderId,
                        o.c_userid AS UserId,
                        o.c_ownerid AS CateringId,
                        ca.c_catering_name AS CateringName,
                        ca.c_logo_path AS CateringLogo,
                        o.c_order_number AS OrderNumber,
                        o.c_event_date AS EventDate,
                        o.c_event_time AS EventTime,
                        o.c_event_type AS EventType,
                        o.c_event_location AS EventLocation,
                        o.c_guest_count AS GuestCount,
                        o.c_special_instructions AS SpecialInstructions,
                        o.c_delivery_address AS DeliveryAddress,
                        o.c_contact_person AS ContactPerson,
                        o.c_contact_phone AS ContactPhone,
                        o.c_contact_email AS ContactEmail,
                        o.c_base_amount AS BaseAmount,
                        o.c_tax_amount AS TaxAmount,
                        o.c_delivery_charges AS DeliveryCharges,
                        o.c_discount_amount AS DiscountAmount,
                        o.c_total_amount AS TotalAmount,
                        o.c_payment_method AS PaymentMethod,
                        o.c_payment_status AS PaymentStatus,
                        o.c_order_status AS OrderStatus,
                        o.c_created_date AS CreatedDate,
                        o.c_updated_date AS UpdatedDate
                    FROM {Table.SysOrders} o
                    LEFT JOIN {Table.SysCateringOwner} ca ON o.c_ownerid = ca.c_ownerid
                    WHERE o.c_orderid = @OrderId AND o.c_userid = @UserId AND o.c_isactive = 1
                ");

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@OrderId", orderId),
                    new SqlParameter("@UserId", userId)
                };

                DataTable dt = await _dbHelper.ExecuteAsync(query.ToString(), parameters);
                if (dt.Rows.Count == 0)
                    return null;

                OrderDto order = MapToOrderDto(dt.Rows[0]);

                // Get order items
                order.OrderItems = await GetOrderItemsAsync(orderId);

                // Get payment info
                order.Payment = await GetOrderPaymentAsync(orderId);

                // Get status history
                order.StatusHistory = await GetOrderStatusHistoryAsync(orderId);

                // Get live event status (only for InProgress or Completed orders)
                if (order.OrderStatus == "InProgress" || order.OrderStatus == "Completed")
                {
                    order.LiveEventStatus = await GetLiveEventStatusAsync(orderId);
                }

                return order;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error fetching order details.", ex);
            }
        }

        // ===================================
        // GET ORDER ITEMS
        // ===================================
        private async Task<List<OrderItemDto>> GetOrderItemsAsync(long orderId)
        {
            try
            {
                string query = $@"
                    SELECT
                        c_order_item_id AS OrderItemId,
                        c_orderid AS OrderId,
                        c_item_type AS ItemType,
                        c_item_id AS ItemId,
                        c_item_name AS ItemName,
                        c_quantity AS Quantity,
                        c_unit_price AS UnitPrice,
                        c_total_price AS TotalPrice,
                        c_package_selections AS PackageSelections,
                        c_created_date AS CreatedDate
                    FROM {Table.SysOrderItems}
                    WHERE c_orderid = @OrderId
                    ORDER BY c_order_item_id ASC
                ";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@OrderId", orderId)
                };

                DataTable dt = await _dbHelper.ExecuteAsync(query, parameters);
                return MapToOrderItemDto(dt);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error fetching order items.", ex);
            }
        }

        // ===================================
        // GET ORDER PAYMENT
        // ===================================
        private async Task<OrderPaymentDto?> GetOrderPaymentAsync(long orderId)
        {
            try
            {
                string query = $@"
                    SELECT TOP 1
                        c_payment_id AS PaymentId,
                        c_orderid AS OrderId,
                        c_payment_method AS PaymentMethod,
                        c_payment_gateway AS PaymentGateway,
                        c_transaction_id AS TransactionId,
                        c_payment_proof_path AS PaymentProofPath,
                        c_amount AS Amount,
                        c_status AS Status,
                        c_payment_date AS PaymentDate,
                        c_verified_by AS VerifiedBy,
                        c_verified_date AS VerifiedDate,
                        c_created_date AS CreatedDate
                    FROM {Table.SysOrderPayments}
                    WHERE c_orderid = @OrderId
                    ORDER BY c_created_date DESC
                ";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@OrderId", orderId)
                };

                DataTable dt = await _dbHelper.ExecuteAsync(query, parameters);
                if (dt.Rows.Count == 0)
                    return null;

                return MapToOrderPaymentDto(dt.Rows[0]);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error fetching order payment.", ex);
            }
        }

        // ===================================
        // GET ORDER STATUS HISTORY
        // ===================================
        private async Task<List<OrderStatusHistoryDto>> GetOrderStatusHistoryAsync(long orderId)
        {
            try
            {
                string query = $@"
                    SELECT
                        c_history_id AS HistoryId,
                        c_orderid AS OrderId,
                        c_status AS Status,
                        c_remarks AS Remarks,
                        c_updated_by AS UpdatedBy,
                        c_updated_date AS UpdatedDate
                    FROM {Table.SysOrderStatusHistory}
                    WHERE c_orderid = @OrderId
                    ORDER BY c_updated_date ASC
                ";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@OrderId", orderId)
                };

                DataTable dt = await _dbHelper.ExecuteAsync(query, parameters);
                return MapToOrderStatusHistoryDto(dt);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error fetching order status history.", ex);
            }
        }

        // ===================================
        // GET LIVE EVENT STATUS
        // ===================================
        private async Task<LiveEventStatusDto?> GetLiveEventStatusAsync(long orderId)
        {
            try
            {
                string query = $@"
                    SELECT
                        sa.c_assignment_id AS AssignmentId,
                        sa.c_status AS AssignmentStatus,
                        s.c_first_name AS SupervisorName,
                        sa.c_check_in_time AS CheckInTime,
                        sa.c_quality_rating AS QualityRating,
                        sa.c_payment_release_requested AS PaymentReleaseRequested,
                        sa.c_payment_release_approved AS PaymentReleaseApproved,
                        sa.c_extra_charges_amount AS ExtraChargesAmount,
                        sa.c_modified_date AS LastUpdated,
                        per.c_report_id AS ReportId,
                        per.c_final_guest_count AS ActualGuestCount,
                        per.c_event_rating AS EventRating,
                        per.c_supervisor_notes AS SupervisorNotes,
                        per.c_submitted_date AS ReportSubmittedDate,
                        per.c_final_payable_amount AS FinalPayableAmount,
                        pec.c_supervisor_signed_off AS PreEventSignedOff
                    FROM {Table.SysSupervisorAssignment} sa
                    INNER JOIN {Table.SysSupervisor} s ON sa.c_supervisor_id = s.c_supervisor_id
                    LEFT JOIN {Table.SysPostEventReport} per ON sa.c_assignment_id = per.c_assignment_id
                    LEFT JOIN {Table.SysPreEventChecklist} pec ON sa.c_assignment_id = pec.c_assignment_id
                    WHERE sa.c_order_id = @OrderId
                        AND sa.c_status NOT IN ('CANCELLED', 'REJECTED')
                    ORDER BY sa.c_assigned_date DESC
                ";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@OrderId", orderId)
                };

                DataTable dt = await _dbHelper.ExecuteAsync(query, parameters);
                if (dt.Rows.Count == 0)
                    return null;

                DataRow row = dt.Rows[0];
                string assignmentStatus = row["AssignmentStatus"]?.ToString() ?? "";
                bool hasReport = row["ReportId"] != DBNull.Value;
                bool preEventDone = row["PreEventSignedOff"] != DBNull.Value && Convert.ToBoolean(row["PreEventSignedOff"]);

                // Determine event timeline stage from assignment status
                string timelineStage = assignmentStatus switch
                {
                    "ASSIGNED" or "ACCEPTED" => preEventDone ? "Prepared" : "Assigned",
                    "CHECKED_IN" => "Arrived",
                    "IN_PROGRESS" => "InProgress",
                    "COMPLETED" => "Completed",
                    _ => "Assigned"
                };

                return new LiveEventStatusDto
                {
                    SupervisorAssigned = true,
                    SupervisorName = row["SupervisorName"]?.ToString(),
                    EventTimelineStage = timelineStage,
                    LastUpdatedAt = row["LastUpdated"] != DBNull.Value ? Convert.ToDateTime(row["LastUpdated"]) : null,
                    ActualGuestCount = row["ActualGuestCount"] != DBNull.Value ? Convert.ToInt32(row["ActualGuestCount"]) : null,
                    ServiceQualityRating = row["QualityRating"] != DBNull.Value ? Convert.ToInt32(row["QualityRating"]) : null,
                    SupervisorNotes = row["SupervisorNotes"] != DBNull.Value ? row["SupervisorNotes"].ToString() : null,
                    SupervisorReportSubmitted = hasReport,
                    PaymentRequestRaised = row["PaymentReleaseRequested"] != DBNull.Value && Convert.ToBoolean(row["PaymentReleaseRequested"]),
                    ExtraChargesAmount = row["ExtraChargesAmount"] != DBNull.Value ? Convert.ToDecimal(row["ExtraChargesAmount"]) : null,
                    FinalPayableAmount = row["FinalPayableAmount"] != DBNull.Value ? Convert.ToDecimal(row["FinalPayableAmount"]) : null
                };
            }
            catch (Exception ex)
            {
                // Non-critical - return null if supervisor data can't be fetched
                return null;
            }
        }

        // ===================================
        // UPDATE ORDER STATUS
        // ===================================
        public async Task<bool> UpdateOrderStatusAsync(long orderId, string status, string? remarks = null)
        {
            try
            {
                string query = $@"
                    UPDATE {Table.SysOrders}
                    SET c_order_status = @Status, c_updated_date = GETDATE()
                    WHERE c_orderid = @OrderId
                ";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@OrderId", orderId),
                    new SqlParameter("@Status", status)
                };

                await _dbHelper.ExecuteNonQueryAsync(query, parameters);

                // Insert into status history
                await InsertOrderStatusHistoryAsync(orderId, status, remarks);

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error updating order status.", ex);
            }
        }

        // ===================================
        // CANCEL ORDER
        // ===================================
        public async Task<bool> CancelOrderAsync(long orderId, long userId, string reason)
        {
            try
            {
                // Check if order can be cancelled (Pending status and within 2 hours)
                string checkQuery = $@"
                    SELECT c_order_status, c_created_date
                    FROM {Table.SysOrders}
                    WHERE c_orderid = @OrderId AND c_userid = @UserId AND c_isactive = 1
                ";

                SqlParameter[] checkParams = new SqlParameter[]
                {
                    new SqlParameter("@OrderId", orderId),
                    new SqlParameter("@UserId", userId)
                };

                DataTable dt = await _dbHelper.ExecuteAsync(checkQuery, checkParams);
                if (dt.Rows.Count == 0)
                    throw new InvalidOperationException("Order not found.");

                string currentStatus = dt.Rows[0]["c_order_status"].ToString() ?? "";
                DateTime createdDate = Convert.ToDateTime(dt.Rows[0]["c_created_date"]);
                TimeSpan timeSinceCreation = DateTime.Now - createdDate;

                if (currentStatus == "InProgress")
                    throw new InvalidOperationException("Order cannot be cancelled during a live event.");

                if (currentStatus != "Pending")
                    throw new InvalidOperationException("Order cannot be cancelled - already confirmed by vendor.");

                if (timeSinceCreation.TotalHours > 2)
                    throw new InvalidOperationException("Order can only be cancelled within 2 hours of placement.");

                // Update order status to Cancelled
                await UpdateOrderStatusAsync(orderId, "Cancelled", $"Cancelled by user. Reason: {reason}");

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error cancelling order: {ex.Message}", ex);
            }
        }

        // ===================================
        // CHECK CATERING AVAILABILITY
        // ===================================
        public async Task<bool> CheckCateringAvailabilityAsync(long cateringId, DateTime eventDate)
        {
            try
            {
                // Check global availability
                string globalQuery = $@"
                    SELECT c_global_status
                    FROM {Table.SysCateringAvailabilityGlobal}
                    WHERE c_ownerid = @CateringId
                ";

                SqlParameter[] globalParams = new SqlParameter[]
                {
                    new SqlParameter("@CateringId", cateringId)
                };

                DataTable globalDt = await _dbHelper.ExecuteAsync(globalQuery, globalParams);
                if (globalDt.Rows.Count > 0)
                {
                    string globalStatus = globalDt.Rows[0]["c_global_status"].ToString() ?? "";
                    if (globalStatus.Equals("Unavailable", StringComparison.OrdinalIgnoreCase))
                        return false;
                }

                // Check date-specific availability
                string dateQuery = $@"
                    SELECT c_status
                    FROM {Table.SysCateringAvailabilityDate}
                    WHERE c_ownerid = @CateringId
                    AND CAST(c_date AS DATE) = CAST(@EventDate AS DATE)
                ";

                SqlParameter[] dateParams = new SqlParameter[]
                {
                    new SqlParameter("@CateringId", cateringId),
                    new SqlParameter("@EventDate", eventDate)
                };

                DataTable dateDt = await _dbHelper.ExecuteAsync(dateQuery, dateParams);
                if (dateDt.Rows.Count > 0)
                {
                    string dateStatus = dateDt.Rows[0]["c_status"].ToString() ?? "";
                    if (dateStatus.Equals("Unavailable", StringComparison.OrdinalIgnoreCase))
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error checking catering availability.", ex);
            }
        }

        // ===================================
        // MAPPING METHODS
        // ===================================
        private List<OrderListItemDto> MapToOrderListItemDto(DataTable dt)
        {
            List<OrderListItemDto> orders = new List<OrderListItemDto>();
            foreach (DataRow row in dt.Rows)
            {
                orders.Add(new OrderListItemDto
                {
                    OrderId = Convert.ToInt64(row["OrderId"]),
                    OrderNumber = row["OrderNumber"].ToString() ?? "",
                    CateringId = Convert.ToInt64(row["CateringId"]),
                    CateringName = row["CateringName"].ToString() ?? "",
                    CateringLogo = row["CateringLogo"].ToString() ?? "",
                    EventDate = Convert.ToDateTime(row["EventDate"]),
                    EventType = row["EventType"].ToString() ?? "",
                    GuestCount = Convert.ToInt32(row["GuestCount"]),
                    TotalAmount = Convert.ToDecimal(row["TotalAmount"]),
                    OrderStatus = row["OrderStatus"].ToString() ?? "",
                    PaymentStatus = row["PaymentStatus"].ToString() ?? "",
                    PaymentMethod = row["PaymentMethod"].ToString() ?? "",
                    CreatedDate = Convert.ToDateTime(row["CreatedDate"])
                });
            }
            return orders;
        }

        private OrderDto MapToOrderDto(DataRow row)
        {
            return new OrderDto
            {
                OrderId = Convert.ToInt64(row["OrderId"]),
                UserId = Convert.ToInt64(row["UserId"]),
                CateringId = Convert.ToInt64(row["CateringId"]),
                CateringName = row["CateringName"].ToString() ?? "",
                CateringLogo = row["CateringLogo"].ToString() ?? "",
                OrderNumber = row["OrderNumber"].ToString() ?? "",
                EventDate = Convert.ToDateTime(row["EventDate"]),
                EventTime = row["EventTime"].ToString() ?? "",
                EventType = row["EventType"].ToString() ?? "",
                EventLocation = row["EventLocation"].ToString() ?? "",
                GuestCount = Convert.ToInt32(row["GuestCount"]),
                SpecialInstructions = row["SpecialInstructions"] != DBNull.Value ? row["SpecialInstructions"].ToString() : null,
                DeliveryAddress = row["DeliveryAddress"].ToString() ?? "",
                ContactPerson = row["ContactPerson"].ToString() ?? "",
                ContactPhone = row["ContactPhone"].ToString() ?? "",
                ContactEmail = row["ContactEmail"].ToString() ?? "",
                BaseAmount = Convert.ToDecimal(row["BaseAmount"]),
                TaxAmount = Convert.ToDecimal(row["TaxAmount"]),
                DeliveryCharges = Convert.ToDecimal(row["DeliveryCharges"]),
                DiscountAmount = Convert.ToDecimal(row["DiscountAmount"]),
                TotalAmount = Convert.ToDecimal(row["TotalAmount"]),
                PaymentMethod = row["PaymentMethod"].ToString() ?? "",
                PaymentStatus = row["PaymentStatus"].ToString() ?? "",
                OrderStatus = row["OrderStatus"].ToString() ?? "",
                CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                UpdatedDate = row["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(row["UpdatedDate"]) : null
            };
        }

        private List<OrderItemDto> MapToOrderItemDto(DataTable dt)
        {
            List<OrderItemDto> items = new List<OrderItemDto>();
            foreach (DataRow row in dt.Rows)
            {
                items.Add(new OrderItemDto
                {
                    OrderItemId = Convert.ToInt64(row["OrderItemId"]),
                    OrderId = Convert.ToInt64(row["OrderId"]),
                    ItemType = row["ItemType"].ToString() ?? "",
                    ItemId = Convert.ToInt64(row["ItemId"]),
                    ItemName = row["ItemName"].ToString() ?? "",
                    Quantity = Convert.ToInt32(row["Quantity"]),
                    UnitPrice = Convert.ToDecimal(row["UnitPrice"]),
                    TotalPrice = Convert.ToDecimal(row["TotalPrice"]),
                    PackageSelections = row["PackageSelections"] != DBNull.Value ? row["PackageSelections"].ToString() : null,
                    CreatedDate = Convert.ToDateTime(row["CreatedDate"])
                });
            }
            return items;
        }

        private OrderPaymentDto MapToOrderPaymentDto(DataRow row)
        {
            return new OrderPaymentDto
            {
                PaymentId = Convert.ToInt64(row["PaymentId"]),
                OrderId = Convert.ToInt64(row["OrderId"]),
                PaymentMethod = row["PaymentMethod"].ToString() ?? "",
                PaymentGateway = row["PaymentGateway"] != DBNull.Value ? row["PaymentGateway"].ToString() : null,
                TransactionId = row["TransactionId"] != DBNull.Value ? row["TransactionId"].ToString() : null,
                PaymentProofPath = row["PaymentProofPath"] != DBNull.Value ? row["PaymentProofPath"].ToString() : null,
                Amount = Convert.ToDecimal(row["Amount"]),
                Status = row["Status"].ToString() ?? "",
                PaymentDate = row["PaymentDate"] != DBNull.Value ? Convert.ToDateTime(row["PaymentDate"]) : null,
                VerifiedBy = row["VerifiedBy"] != DBNull.Value ? Convert.ToInt64(row["VerifiedBy"]) : null,
                VerifiedDate = row["VerifiedDate"] != DBNull.Value ? Convert.ToDateTime(row["VerifiedDate"]) : null,
                CreatedDate = Convert.ToDateTime(row["CreatedDate"])
            };
        }

        private List<OrderStatusHistoryDto> MapToOrderStatusHistoryDto(DataTable dt)
        {
            List<OrderStatusHistoryDto> history = new List<OrderStatusHistoryDto>();
            foreach (DataRow row in dt.Rows)
            {
                history.Add(new OrderStatusHistoryDto
                {
                    HistoryId = Convert.ToInt64(row["HistoryId"]),
                    OrderId = Convert.ToInt64(row["OrderId"]),
                    Status = row["Status"].ToString() ?? "",
                    Remarks = row["Remarks"] != DBNull.Value ? row["Remarks"].ToString() : null,
                    UpdatedBy = row["UpdatedBy"] != DBNull.Value ? Convert.ToInt64(row["UpdatedBy"]) : null,
                    UpdatedDate = Convert.ToDateTime(row["UpdatedDate"])
                });
            }
            return history;
        }
    }
}
