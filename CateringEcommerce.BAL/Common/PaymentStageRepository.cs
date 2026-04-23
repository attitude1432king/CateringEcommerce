using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Models.User;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CateringEcommerce.BAL.Common
{
    public class PaymentStageRepository : IPaymentStageRepository
    {
        private readonly IDatabaseHelper _dbHelper;
        public PaymentStageRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // ===================================
        // INSERT PAYMENT STAGE
        // ===================================
        public async Task<long> InsertPaymentStageAsync(long orderId, string stageType, decimal stagePercentage, decimal stageAmount, DateTime? dueDate = null)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append($@"
                    INSERT INTO {Table.SysOrderPaymentStages} (
                        c_orderid, c_stage_type, c_stage_percentage, c_stage_amount,
                        c_status, c_due_date, c_reminder_sent_count, c_createddate
                    ) VALUES (
                        @OrderId, @StageType, @StagePercentage, @StageAmount,
                        'Pending', @DueDate, 0, NOW()
                    )
                    RETURNING c_payment_stage_id;
                ");

                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@OrderId", orderId),
                    new NpgsqlParameter("@StageType", stageType),
                    new NpgsqlParameter("@StagePercentage", stagePercentage),
                    new NpgsqlParameter("@StageAmount", stageAmount),
                    new NpgsqlParameter("@DueDate", (object)dueDate ?? DBNull.Value)
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
                throw new Exception("Error inserting payment stage: " + ex.Message, ex);
            }
        }

        // ===================================
        // GET PAYMENT STAGES BY ORDER ID
        // ===================================
        public async Task<List<PaymentStageDto>> GetPaymentStagesByOrderIdAsync(long orderId)
        {
            try
            {
                string query = $@"
                    SELECT
                        c_payment_stage_id, c_orderid, c_stage_type, c_stage_percentage, c_stage_amount,
                        c_payment_method, c_payment_gateway, c_razorpay_order_id, c_razorpay_payment_id,
                        c_transaction_id, c_upi_id, c_status, c_payment_date, c_due_date,
                        c_reminder_sent_count, c_last_reminder_date, c_createddate
                    FROM {Table.SysOrderPaymentStages}
                    WHERE c_orderid = @OrderId
                    ORDER BY c_stage_type ASC
                ";

                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@OrderId", orderId)
                };

                DataTable dt = await _dbHelper.ExecuteAsync(query, parameters);
                List<PaymentStageDto> stages = new List<PaymentStageDto>();

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        stages.Add(MapPaymentStageDto(row));
                    }
                }

                return stages;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving payment stages: " + ex.Message, ex);
            }
        }

        // ===================================
        // GET PENDIOG PAYMENT STAGES
        // ===================================
        public async Task<List<PaymentStageDto>> GetPendingPaymentStagesAsync(long orderId)
        {
            try
            {
                string query = $@"
                    SELECT
                        c_payment_stage_id, c_orderid, c_stage_type, c_stage_percentage, c_stage_amount,
                        c_payment_method, c_payment_gateway, c_razorpay_order_id, c_razorpay_payment_id,
                        c_transaction_id, c_upi_id, c_status, c_payment_date, c_due_date,
                        c_reminder_sent_count, c_last_reminder_date, c_createddate
                    FROM {Table.SysOrderPaymentStages}
                    WHERE c_orderid = @OrderId AND c_status = 'Pending'
                    ORDER BY c_stage_type ASC
                ";

                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@OrderId", orderId)
                };

                DataTable dt = await _dbHelper.ExecuteAsync(query, parameters);
                List<PaymentStageDto> stages = new List<PaymentStageDto>();

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        stages.Add(MapPaymentStageDto(row));
                    }
                }

                return stages;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving pending payment stages: " + ex.Message, ex);
            }
        }

        // ===================================
        // UPDATE PAYMENT STAGE STATUS (Legacy - Non-Transactional)
        // ===================================
        public async Task<bool> UpdatePaymentStageStatusAsync(long paymentStageId, string status, ProcessPaymentStageDto paymentData)
        {
            try
            {
                string query = $@"
                    UPDATE {Table.SysOrderPaymentStages}
                    SET
                        c_status = @Status,
                        c_payment_method = @PaymentMethod,
                        c_payment_gateway = @PaymentGateway,
                        c_razorpay_order_id = @RazorpayOrderId,
                        c_razorpay_payment_id = @RazorpayPaymentId,
                        c_transaction_id = @TransactionId,
                        c_upi_id = @UpiId,
                        c_payment_date = NOW()
                    WHERE c_payment_stage_id = @PaymentStageId
                ";

                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@PaymentStageId", paymentStageId),
                    new NpgsqlParameter("@Status", status),
                    new NpgsqlParameter("@PaymentMethod", paymentData.PaymentMethod),
                    new NpgsqlParameter("@PaymentGateway", (object)paymentData.PaymentGateway ?? DBNull.Value),
                    new NpgsqlParameter("@RazorpayOrderId", (object)paymentData.RazorpayOrderId ?? DBNull.Value),
                    new NpgsqlParameter("@RazorpayPaymentId", (object)paymentData.RazorpayPaymentId ?? DBNull.Value),
                    new NpgsqlParameter("@TransactionId", (object)paymentData.TransactionId ?? DBNull.Value),
                    new NpgsqlParameter("@UpiId", (object)paymentData.UpiId ?? DBNull.Value)
                };

                int rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating payment stage status: " + ex.Message, ex);
            }
        }

        // ===================================
        // UPDATE PAYMENT STAGE WITH ORDER STATUS (Transactional - OEW)
        // CRITICAL: This is the preferred method for payment verification
        // ===================================
        public async Task<bool> UpdatePaymentStageWithOrderStatusAsync(
            long paymentStageId,
            long orderId,
            string stageType,
            string status,
            ProcessPaymentStageDto paymentData,
            string newOrderStatus)
        {
            try
            {
                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@PaymentStageId", paymentStageId),
                    new NpgsqlParameter("@OrderId", orderId),
                    new NpgsqlParameter("@StageType", stageType),
                    new NpgsqlParameter("@Status", status),
                    new NpgsqlParameter("@PaymentMethod", paymentData.PaymentMethod),
                    new NpgsqlParameter("@PaymentGateway", (object)paymentData.PaymentGateway ?? DBNull.Value),
                    new NpgsqlParameter("@RazorpayOrderId", (object)paymentData.RazorpayOrderId ?? DBNull.Value),
                    new NpgsqlParameter("@RazorpayPaymentId", (object)paymentData.RazorpayPaymentId ?? DBNull.Value),
                    new NpgsqlParameter("@TransactionId", (object)paymentData.TransactionId ?? DBNull.Value),
                    new NpgsqlParameter("@UpiId", (object)paymentData.UpiId ?? DBNull.Value),
                    new NpgsqlParameter("@NewOrderStatus", (object)newOrderStatus ?? DBNull.Value),
                    new NpgsqlParameter("@Success", NpgsqlDbType.Boolean) { Direction = ParameterDirection.Output },
                    new NpgsqlParameter("@ErrorMessage", NpgsqlDbType.Varchar, 500) { Direction = ParameterDirection.Output }
                };

                await _dbHelper.ExecuteStoredProcedureAsync<dynamic>("sp_UpdatePaymentStageWithOrderStatus", parameters);

                var success = parameters[11].Value != null && (bool)parameters[11].Value;
                var errorMessage = parameters[12].Value as string;

                if (!success && !string.IsNullOrEmpty(errorMessage))
                {
                    throw new InvalidOperationException($"Payment update failed: {errorMessage}");
                }

                return success;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating payment stage with order status: " + ex.Message, ex);
            }
        }

        // ===================================
        // GET ORDERS WITH PENDIOG POST-EVENT PAYMENTS
        // ===================================
        public async Task<DataTable> GetOrdersWithPendingPostEventPaymentsAsync()
        {
            try
            {
                string query = $@"
                    SELECT DISTINCT
                        o.c_orderid, o.c_order_number, o.c_userid, o.c_event_date,
                        o.c_contact_email, o.c_contact_phone, o.c_contact_person,
                        ps.c_payment_stage_id, ps.c_stage_amount, ps.c_due_date,
                        ps.c_reminder_sent_count, ps.c_last_reminder_date
                    FROM {Table.SysOrders} o
                    INNER JOIN {Table.SysOrderPaymentStages} ps ON o.c_orderid = ps.c_orderid
                    WHERE ps.c_stage_type = 'PostEvent'
                    AND ps.c_status = 'Pending'
                    AND o.c_event_date < NOW()
                    AND o.c_order_status IN ('Completed', 'InProgress')
                    ORDER BY o.c_event_date ASC
                ";

                DataTable dt = await _dbHelper.ExecuteAsync(query, Array.Empty<NpgsqlParameter>());
                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving orders with pending post-event payments: " + ex.Message, ex);
            }
        }

        // ===================================
        // UPDATE REMINDER SENT COUNT
        // ===================================
        public async Task<bool> UpdateReminderSentCountAsync(long paymentStageId)
        {
            try
            {
                string query = $@"
                    UPDATE {Table.SysOrderPaymentStages}
                    SET
                        c_reminder_sent_count = c_reminder_sent_count + 1,
                        c_last_reminder_date = NOW()
                    WHERE c_payment_stage_id = @PaymentStageId
                ";

                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@PaymentStageId", paymentStageId)
                };

                int rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating reminder sent count: " + ex.Message, ex);
            }
        }

        // ===================================
        // HELPER: MAP PAYMENT STAGE DTO
        // ===================================
        private PaymentStageDto MapPaymentStageDto(DataRow row)
        {
            return new PaymentStageDto
            {
                PaymentStageId = Convert.ToInt64(row["c_payment_stage_id"]),
                OrderId = Convert.ToInt64(row["c_orderid"]),
                StageType = row["c_stage_type"].ToString() ?? string.Empty,
                StagePercentage = Convert.ToDecimal(row["c_stage_percentage"]),
                StageAmount = Convert.ToDecimal(row["c_stage_amount"]),
                PaymentMethod = row["c_payment_method"] != DBNull.Value ? row["c_payment_method"].ToString() : null,
                PaymentGateway = row["c_payment_gateway"] != DBNull.Value ? row["c_payment_gateway"].ToString() : null,
                RazorpayOrderId = row["c_razorpay_order_id"] != DBNull.Value ? row["c_razorpay_order_id"].ToString() : null,
                RazorpayPaymentId = row["c_razorpay_payment_id"] != DBNull.Value ? row["c_razorpay_payment_id"].ToString() : null,
                TransactionId = row["c_transaction_id"] != DBNull.Value ? row["c_transaction_id"].ToString() : null,
                UpiId = row["c_upi_id"] != DBNull.Value ? row["c_upi_id"].ToString() : null,
                Status = row["c_status"].ToString() ?? string.Empty,
                PaymentDate = row["c_payment_date"] != DBNull.Value ? Convert.ToDateTime(row["c_payment_date"]) : null,
                DueDate = row["c_due_date"] != DBNull.Value ? Convert.ToDateTime(row["c_due_date"]) : null,
                ReminderSentCount = Convert.ToInt32(row["c_reminder_sent_count"]),
                LastReminderDate = row["c_last_reminder_date"] != DBNull.Value ? Convert.ToDateTime(row["c_last_reminder_date"]) : null,
                CreatedDate = Convert.ToDateTime(row["c_createddate"])
            };
        }
    }
}

