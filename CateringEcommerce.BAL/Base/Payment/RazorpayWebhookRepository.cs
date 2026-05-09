using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Payment;
using CateringEcommerce.Domain.Models.Payment;
using Npgsql;
using NpgsqlTypes;
using System.Data.Common;

namespace CateringEcommerce.BAL.Base.Payment
{
    public class RazorpayWebhookRepository : IRazorpayWebhookRepository
    {
        private readonly IDatabaseHelper _dbHelper;

        public RazorpayWebhookRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        }

        public async Task<long> CreateWebhookLogAsync(RazorpayWebhookLogEntry entry)
        {
            const string query = @"
                INSERT INTO t_sys_payment_webhook_log
                (
                    c_event_type, c_payment_id, c_order_id, c_payload, c_signature,
                    c_is_valid, c_error_message, c_processing_status
                )
                VALUES
                (
                    @EventType, @PaymentId, @OrderId, CAST(@Payload AS jsonb), @Signature,
                    @IsValid, @ErrorMessage, @ProcessingStatus
                )
                RETURNING c_id;";

            return await _dbHelper.ExecuteScalarAsync<long>(query, new DbParameter[]
            {
                TextParam("@EventType", entry.EventType),
                TextParam("@PaymentId", entry.PaymentId),
                TextParam("@OrderId", entry.OrderId),
                new NpgsqlParameter("@Payload", NpgsqlDbType.Jsonb) { Value = string.IsNullOrWhiteSpace(entry.Payload) ? "{}" : entry.Payload },
                TextParam("@Signature", entry.Signature),
                new NpgsqlParameter("@IsValid", NpgsqlDbType.Boolean) { Value = entry.IsValid },
                TextParam("@ErrorMessage", entry.ErrorMessage),
                TextParam("@ProcessingStatus", entry.ProcessingStatus)
            });
        }

        public async Task UpdateWebhookLogAsync(long webhookLogId, string processingStatus, string? errorMessage = null)
        {
            const string query = @"
                UPDATE t_sys_payment_webhook_log
                SET c_processing_status = @ProcessingStatus,
                    c_error_message = COALESCE(@ErrorMessage, c_error_message)
                WHERE c_id = @WebhookLogId;";

            await _dbHelper.ExecuteNonQueryAsync(query, new DbParameter[]
            {
                new NpgsqlParameter("@WebhookLogId", NpgsqlDbType.Bigint) { Value = webhookLogId },
                TextParam("@ProcessingStatus", processingStatus),
                TextParam("@ErrorMessage", errorMessage)
            });
        }

        public async Task<bool> IsPaymentSuccessfulAsync(string paymentId)
        {
            const string query = @"
                SELECT COUNT(*)
                FROM t_sys_payment_transactions
                WHERE (c_gateway_paymentid = @PaymentId OR c_razorpay_payment_id = @PaymentId)
                  AND UPPER(c_paymentstatus) = 'SUCCESS';";

            var count = await _dbHelper.ExecuteScalarAsync<int>(query, new DbParameter[]
            {
                TextParam("@PaymentId", paymentId)
            });

            return count > 0;
        }

        public async Task UpsertPaymentTransactionAsync(RazorpayPaymentTransactionUpsert transaction)
        {
            var context = await GetOrderContextAsync(transaction.OrderId);
            if (context == null)
            {
                throw new InvalidOperationException($"Order {transaction.OrderId} was not found for Razorpay webhook transaction.");
            }

            var existingId = await GetTransactionIdAsync(transaction.PaymentId);
            if (existingId.HasValue)
            {
                const string updateQuery = @"
                    UPDATE t_sys_payment_transactions
                    SET c_gateway_orderid = COALESCE(@RazorpayOrderId, c_gateway_orderid),
                        c_razorpay_order_id = COALESCE(@RazorpayOrderId, c_razorpay_order_id),
                        c_amount = @Amount,
                        c_paymentmethod = COALESCE(@PaymentMethod, c_paymentmethod),
                        c_paymentgateway = 'RAZORPAY',
                        c_paymentstatus = @Status,
                        c_event_type = @EventType,
                        c_webhook_log_id = @WebhookLogId,
                        c_statusreason = NULL,
                        c_completeddate = CASE WHEN @Status = 'SUCCESS' THEN NOW() ELSE c_completeddate END,
                        c_modifieddate = NOW(),
                        c_updated_at = NOW(),
                        c_metadata = @Payload
                    WHERE c_transactionid = @TransactionId;";

                await _dbHelper.ExecuteNonQueryAsync(updateQuery, BuildTransactionParameters(transaction, context, existingId.Value));
                return;
            }

            const string insertQuery = @"
                INSERT INTO t_sys_payment_transactions
                (
                    c_orderid, c_userid, c_cateringownerid, c_transactiontype, c_amount,
                    c_paymentmethod, c_paymentgateway, c_gateway_transactionid, c_gateway_orderid,
                    c_gateway_paymentid, c_gateway_signature, c_paymentstatus, c_completeddate,
                    c_metadata, c_razorpay_payment_id, c_razorpay_order_id, c_event_type,
                    c_webhook_log_id, c_updated_at
                )
                VALUES
                (
                    @OrderId, @UserId, @OwnerId, @TransactionType, @Amount,
                    @PaymentMethod, 'RAZORPAY', @PaymentId, @RazorpayOrderId,
                    @PaymentId, @Signature, @Status, CASE WHEN @Status = 'SUCCESS' THEN NOW() ELSE NULL END,
                    @Payload, @PaymentId, @RazorpayOrderId, @EventType,
                    @WebhookLogId, NOW()
                );";

            await _dbHelper.ExecuteNonQueryAsync(insertQuery, BuildTransactionParameters(transaction, context, null));
        }

        private async Task<long?> GetTransactionIdAsync(string paymentId)
        {
            const string query = @"
                SELECT c_transactionid
                FROM t_sys_payment_transactions
                WHERE c_gateway_paymentid = @PaymentId OR c_razorpay_payment_id = @PaymentId
                ORDER BY c_transactionid DESC
                LIMIT 1;";

            var result = await _dbHelper.ExecuteScalarAsync(query, new DbParameter[]
            {
                TextParam("@PaymentId", paymentId)
            });

            return result == null || result == DBNull.Value ? null : Convert.ToInt64(result);
        }

        private async Task<OrderContext?> GetOrderContextAsync(long orderId)
        {
            var query = $@"
                SELECT c_userid, c_ownerid
                FROM {Table.SysOrders}
                WHERE c_orderid = @OrderId;";

            var table = await _dbHelper.ExecuteAsync(query, new DbParameter[]
            {
                new NpgsqlParameter("@OrderId", NpgsqlDbType.Bigint) { Value = orderId }
            });

            if (table.Rows.Count == 0)
            {
                return null;
            }

            return new OrderContext(
                Convert.ToInt64(table.Rows[0]["c_userid"]),
                Convert.ToInt64(table.Rows[0]["c_ownerid"]));
        }

        private static DbParameter[] BuildTransactionParameters(
            RazorpayPaymentTransactionUpsert transaction,
            OrderContext context,
            long? transactionId)
        {
            var parameters = new List<DbParameter>
            {
                new NpgsqlParameter("@OrderId", NpgsqlDbType.Bigint) { Value = transaction.OrderId },
                new NpgsqlParameter("@UserId", NpgsqlDbType.Bigint) { Value = context.UserId },
                new NpgsqlParameter("@OwnerId", NpgsqlDbType.Bigint) { Value = context.OwnerId },
                TextParam("@TransactionType", MapTransactionType(transaction.StageType)),
                new NpgsqlParameter("@Amount", NpgsqlDbType.Numeric) { Value = transaction.Amount },
                TextParam("@PaymentMethod", transaction.PaymentMethod?.ToUpperInvariant()),
                TextParam("@PaymentId", transaction.PaymentId),
                TextParam("@RazorpayOrderId", transaction.RazorpayOrderId),
                TextParam("@Signature", transaction.Signature),
                TextParam("@Status", transaction.Status),
                TextParam("@EventType", transaction.EventType),
                new NpgsqlParameter("@WebhookLogId", NpgsqlDbType.Bigint) { Value = transaction.WebhookLogId },
                TextParam("@Payload", transaction.Payload)
            };

            if (transactionId.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@TransactionId", NpgsqlDbType.Bigint) { Value = transactionId.Value });
            }

            return parameters.ToArray();
        }

        private static string MapTransactionType(string? stageType)
        {
            return stageType?.Trim().ToUpperInvariant() switch
            {
                "PREBOOKING" => "ADVANCE",
                "POSTEVENT" => "FINAL",
                "FULL" => "FULL",
                _ => "RAZORPAY"
            };
        }

        private static NpgsqlParameter TextParam(string name, string? value)
        {
            return new NpgsqlParameter(name, NpgsqlDbType.Text) { Value = string.IsNullOrEmpty(value) ? DBNull.Value : value };
        }

        private sealed record OrderContext(long UserId, long OwnerId);
    }
}
