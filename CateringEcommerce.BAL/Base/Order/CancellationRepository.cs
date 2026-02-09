using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Order;
using CateringEcommerce.Domain.Models.Order;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Threading.Tasks;

namespace CateringEcommerce.BAL.Base.Order
{
    public class CancellationRepository : ICancellationRepository
    {
        private readonly IDatabaseHelper _dbHelper;

        public CancellationRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<CancellationPolicyResponse> CalculateCancellationRefundAsync(long orderId)
        {
            var parameters = new[]
            {
                new SqlParameter("@OrderId", orderId),
                new SqlParameter("@RefundPercentage", SqlDbType.Decimal) { Direction = ParameterDirection.Output, Precision = 5, Scale = 2 },
                new SqlParameter("@RefundAmount", SqlDbType.Decimal) { Direction = ParameterDirection.Output, Precision = 18, Scale = 2 },
                new SqlParameter("@PolicyTier", SqlDbType.VarChar, 20) { Direction = ParameterDirection.Output },
                new SqlParameter("@VendorCompensation", SqlDbType.Decimal) { Direction = ParameterDirection.Output, Precision = 18, Scale = 2 }
            };

            var result = await _dbHelper.ExecuteStoredProcedureAsync<CancellationPolicyResponse>(
                "sp_CalculateCancellationRefund",
                parameters
            );

            if (result != null)
            {
                result.PolicyDescription = GetPolicyDescription(result.PolicyTier);
                result.Warning = GetPolicyWarning(result.DaysBeforeEvent);
            }

            return result;
        }

        public async Task<CancellationRequestModel> ProcessCancellationRequestAsync(CreateCancellationRequestDto request)
        {
            var parameters = new[]
            {
                new SqlParameter("@OrderId", request.OrderId),
                new SqlParameter("@UserId", request.UserId),
                new SqlParameter("@CancellationReason", request.CancellationReason),
                new SqlParameter("@IsForceMajeure", request.IsForceMajeure),
                new SqlParameter("@ForceMajeureEvidence", (object)request.ForceMajeureEvidence ?? DBNull.Value)
            };

            var result = await _dbHelper.ExecuteStoredProcedureAsync<CancellationRequestModel>(
                "sp_ProcessCancellationRequest",
                parameters
            );

            return result;
        }

        public async Task<CancellationRequestModel> GetCancellationRequestAsync(long cancellationId)
        {
            var query = @"
                SELECT * FROM t_sys_cancellation_requests
                WHERE c_cancellation_id = @CancellationId";

            var parameters = new[]
            {
                new SqlParameter("@CancellationId", cancellationId)
            };

            var results = await _dbHelper.ExecuteQueryAsync<CancellationRequestModel>(query, parameters);
            return results.Count > 0 ? results[0] : null;
        }

        public async Task<CancellationRequestModel> GetCancellationRequestByOrderAsync(long orderId)
        {
            var query = @"
                SELECT * FROM t_sys_cancellation_requests
                WHERE c_orderid = @OrderId
                ORDER BY c_created_date DESC";

            var parameters = new[]
            {
                new SqlParameter("@OrderId", orderId)
            };

            var results = await _dbHelper.ExecuteQueryAsync<CancellationRequestModel>(query, parameters);
            return results.Count > 0 ? results[0] : null;
        }

        public async Task<List<CancellationRequestModel>> GetUserCancellationRequestsAsync(long userId)
        {
            var query = @"
                SELECT * FROM t_sys_cancellation_requests
                WHERE c_userid = @UserId
                ORDER BY c_created_date DESC";

            var parameters = new[]
            {
                new SqlParameter("@UserId", userId)
            };

            return await _dbHelper.ExecuteQueryAsync<CancellationRequestModel>(query, parameters);
        }

        public async Task<bool> ApproveCancellationRequestAsync(long cancellationId, long adminId, string adminNotes)
        {
            var query = @"
                UPDATE t_sys_cancellation_requests
                SET c_status = 'Approved',
                    c_admin_approved_by = @AdminId,
                    c_admin_approval_date = GETDATE(),
                    c_admin_notes = @AdminNotes,
                    c_modified_date = GETDATE()
                WHERE c_cancellation_id = @CancellationId";

            var parameters = new[]
            {
                new SqlParameter("@CancellationId", cancellationId),
                new SqlParameter("@AdminId", adminId),
                new SqlParameter("@AdminNotes", (object)adminNotes ?? DBNull.Value)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> RejectCancellationRequestAsync(long cancellationId, long adminId, string rejectionReason)
        {
            var query = @"
                UPDATE t_sys_cancellation_requests
                SET c_status = 'Rejected',
                    c_admin_approved_by = @AdminId,
                    c_admin_approval_date = GETDATE(),
                    c_admin_notes = @RejectionReason,
                    c_modified_date = GETDATE()
                WHERE c_cancellation_id = @CancellationId";

            var parameters = new[]
            {
                new SqlParameter("@CancellationId", cancellationId),
                new SqlParameter("@AdminId", adminId),
                new SqlParameter("@RejectionReason", rejectionReason)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> ProcessRefundAsync(long cancellationId, string refundTransactionId, string refundMethod)
        {
            var query = @"
                UPDATE t_sys_cancellation_requests
                SET c_status = 'Refunded',
                    c_refund_initiated_date = GETDATE(),
                    c_refund_completed_date = GETDATE(),
                    c_refund_transaction_id = @RefundTransactionId,
                    c_refund_method = @RefundMethod,
                    c_modified_date = GETDATE()
                WHERE c_cancellation_id = @CancellationId
                  AND c_status = 'Approved'";

            var parameters = new[]
            {
                new SqlParameter("@CancellationId", cancellationId),
                new SqlParameter("@RefundTransactionId", refundTransactionId),
                new SqlParameter("@RefundMethod", refundMethod)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);

            if (rowsAffected > 0)
            {
                // Update order status
                var updateOrderQuery = @"
                    UPDATE t_sys_order
                    SET c_order_status = 'Cancelled',
                        c_modifieddate = GETDATE()
                    WHERE c_orderid = (SELECT c_orderid FROM t_sys_cancellation_requests WHERE c_cancellation_id = @CancellationId)";

                await _dbHelper.ExecuteNonQueryAsync(updateOrderQuery, new[] { new SqlParameter("@CancellationId", cancellationId) });
            }

            return rowsAffected > 0;
        }

        public async Task<List<CancellationRequestModel>> GetPendingCancellationRequestsAsync()
        {
            var query = @"
                SELECT * FROM t_sys_cancellation_requests
                WHERE c_status = 'Pending'
                ORDER BY c_created_date ASC";

            return await _dbHelper.ExecuteQueryAsync<CancellationRequestModel>(query);
        }

        #region Helper Methods

        private string GetPolicyDescription(string policyTier)
        {
            return policyTier switch
            {
                "FULL_REFUND" => "You are entitled to a 100% refund as you are cancelling more than 7 days before the event.",
                "PARTIAL_REFUND" => "You are entitled to a 50% refund as you are cancelling between 3-7 days before the event.",
                "NO_REFUND" => "No refund is available as you are cancelling less than 48 hours before the event. The vendor has already procured materials.",
                "FORCE_MAJEURE" => "Due to exceptional circumstances, a 50% refund will be provided (shared loss between customer and vendor).",
                _ => "Standard cancellation policy applies."
            };
        }

        private string GetPolicyWarning(int daysBeforeEvent)
        {
            if (daysBeforeEvent > 7)
                return "You have plenty of time. Full refund available.";
            else if (daysBeforeEvent >= 3)
                return "⚠️ Partial refund only! Consider rescheduling instead.";
            else if (daysBeforeEvent >= 2)
                return "⚠️ WARNING: No refund if you cancel within 48 hours!";
            else
                return "❌ CRITICAL: No refund available. Partner has already procured materials.";
        }

        #endregion
    }
}
