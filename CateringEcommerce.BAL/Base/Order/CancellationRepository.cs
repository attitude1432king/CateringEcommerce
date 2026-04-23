using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Order;
using CateringEcommerce.Domain.Models.Order;
using Npgsql;
using NpgsqlTypes;
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
                new NpgsqlParameter("@OrderId", orderId),
                new NpgsqlParameter("@RefundPercentage", NpgsqlDbType.Double) { Direction = ParameterDirection.Output, Precision = 5, Scale = 2 },
                new NpgsqlParameter("@RefundAmount", NpgsqlDbType.Double) { Direction = ParameterDirection.Output, Precision = 18, Scale = 2 },
                new NpgsqlParameter("@PolicyTier", NpgsqlDbType.Varchar, 20) { Direction = ParameterDirection.Output },
                new NpgsqlParameter("@PartnerCompensation", NpgsqlDbType.Double) { Direction = ParameterDirection.Output, Precision = 18, Scale = 2 }
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
                new NpgsqlParameter("@OrderId", request.OrderId),
                new NpgsqlParameter("@UserId", request.UserId),
                new NpgsqlParameter("@CancellationReason", request.CancellationReason),
                new NpgsqlParameter("@IsForceMajeure", request.IsForceMajeure),
                new NpgsqlParameter("@ForceMajeureEvidence", (object)request.ForceMajeureEvidence ?? DBNull.Value)
            };

            var result = await _dbHelper.ExecuteStoredProcedureAsync<CancellationRequestModel>(
                "sp_ProcessCancellationRequest",
                parameters
            );

            return result;
        }

        public async Task<CancellationRequestModel> GetCancellationRequestAsync(long cancellationId)
        {
            var query = $@"
                SELECT * FROM {Table.SysCancellationRequests}
                WHERE c_cancellation_id = @CancellationId";

            var parameters = new[]
            {
                new NpgsqlParameter("@CancellationId", cancellationId)
            };

            var results = await _dbHelper.ExecuteQueryAsync<CancellationRequestModel>(query, parameters);
            return results.Count > 0 ? results[0] : null;
        }

        public async Task<CancellationRequestModel> GetCancellationRequestByOrderAsync(long orderId)
        {
            var query = $@"
                SELECT * FROM {Table.SysCancellationRequests}
                WHERE c_orderid = @OrderId
                ORDER BY c_createddate DESC";

            var parameters = new[]
            {
                new NpgsqlParameter("@OrderId", orderId)
            };

            var results = await _dbHelper.ExecuteQueryAsync<CancellationRequestModel>(query, parameters);
            return results.Count > 0 ? results[0] : null;
        }

        public async Task<List<CancellationRequestModel>> GetUserCancellationRequestsAsync(long userId)
        {
            var query = $@"
                SELECT * FROM {Table.SysCancellationRequests}
                WHERE c_userid = @UserId
                ORDER BY c_createddate DESC";

            var parameters = new[]
            {
                new NpgsqlParameter("@UserId", userId)
            };

            return await _dbHelper.ExecuteQueryAsync<CancellationRequestModel>(query, parameters);
        }

        public async Task<bool> ApproveCancellationRequestAsync(long cancellationId, long adminId, string adminNotes)
        {
            // CRITICAL FIX: Use transactional stored procedure
            var parameters = new[]
            {
                new NpgsqlParameter("@CancellationId", cancellationId),
                new NpgsqlParameter("@AdminId", adminId),
                new NpgsqlParameter("@AdminNotes", (object)adminNotes ?? DBNull.Value),
                new NpgsqlParameter("@Success", NpgsqlDbType.Boolean) { Direction = ParameterDirection.Output },
                new NpgsqlParameter("@ErrorMessage", NpgsqlDbType.Varchar, 500) { Direction = ParameterDirection.Output }
            };

            await _dbHelper.ExecuteStoredProcedureAsync<dynamic>("sp_ApproveCancellationTransaction", parameters);

            var success = parameters[3].Value != null && (bool)parameters[3].Value;
            var errorMessage = parameters[4].Value as string;

            if (!success && !string.IsNullOrEmpty(errorMessage))
            {
                throw new InvalidOperationException($"Cancellation approval failed: {errorMessage}");
            }

            return success;
        }

        public async Task<bool> RejectCancellationRequestAsync(long cancellationId, long adminId, string rejectionReason)
        {
            // CRITICAL FIX: Use transactional stored procedure
            var parameters = new[]
            {
                new NpgsqlParameter("@CancellationId", cancellationId),
                new NpgsqlParameter("@AdminId", adminId),
                new NpgsqlParameter("@RejectionReason", rejectionReason),
                new NpgsqlParameter("@Success", NpgsqlDbType.Boolean) { Direction = ParameterDirection.Output },
                new NpgsqlParameter("@ErrorMessage", NpgsqlDbType.Varchar, 500) { Direction = ParameterDirection.Output }
            };

            await _dbHelper.ExecuteStoredProcedureAsync<dynamic>("sp_RejectCancellationTransaction", parameters);

            var success = parameters[3].Value != null && (bool)parameters[3].Value;
            var errorMessage = parameters[4].Value as string;

            if (!success && !string.IsNullOrEmpty(errorMessage))
            {
                throw new InvalidOperationException($"Cancellation rejection failed: {errorMessage}");
            }

            return success;
        }

        public async Task<bool> ProcessRefundAsync(long cancellationId, string refundTransactionId, string refundMethod)
        {
            // CRITICAL FIX: Use stored procedure with transaction to prevent data corruption
            // Previous implementation had separate queries that could fail independently
            // causing refund to be marked complete while order remained active (revenue leakage)

            var parameters = new[]
            {
                new NpgsqlParameter("@CancellationId", cancellationId),
                new NpgsqlParameter("@RefundTransactionId", refundTransactionId),
                new NpgsqlParameter("@RefundMethod", refundMethod),
                new NpgsqlParameter("@Success", NpgsqlDbType.Boolean) { Direction = ParameterDirection.Output },
                new NpgsqlParameter("@ErrorMessage", NpgsqlDbType.Varchar, 500) { Direction = ParameterDirection.Output }
            };

            await _dbHelper.ExecuteStoredProcedureAsync<dynamic>("sp_ProcessRefundTransaction", parameters);

            var success = parameters[3].Value != null && (bool)parameters[3].Value;
            var errorMessage = parameters[4].Value as string;

            if (!success && !string.IsNullOrEmpty(errorMessage))
            {
                throw new InvalidOperationException($"Refund processing failed: {errorMessage}");
            }

            return success;
        }

        public async Task<List<CancellationRequestModel>> GetPendingCancellationRequestsAsync()
        {
            var query = $@"
                SELECT * FROM {Table.SysCancellationRequests}
                WHERE c_status = 'Pending'
                ORDER BY c_createddate ASC";

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
