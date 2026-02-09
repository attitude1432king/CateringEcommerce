using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Order;
using CateringEcommerce.Domain.Models.Order;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace CateringEcommerce.BAL.Base.Order
{
    public class ComplaintRepository : IComplaintRepository
    {
        private readonly IDatabaseHelper _dbHelper;

        public ComplaintRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<FileComplaintResponse> FileComplaintAsync(FileComplaintDto request)
        {
            var parameters = new[]
            {
                new SqlParameter("@OrderId", request.OrderId),
                new SqlParameter("@UserId", request.UserId),
                new SqlParameter("@ComplaintType", request.ComplaintType),
                new SqlParameter("@ComplaintDescription", request.ComplaintDetails),
                new SqlParameter("@ItemsAffected", (object)request.AffectedItems ?? DBNull.Value),
                new SqlParameter("@EvidenceUrls", (object)request.PhotoEvidencePaths ?? DBNull.Value),
                new SqlParameter("@ComplaintId", SqlDbType.BigInt) { Direction = ParameterDirection.Output },
                new SqlParameter("@Severity", SqlDbType.VarChar, 20) { Direction = ParameterDirection.Output },
                new SqlParameter("@IsFlaggedSuspicious", SqlDbType.Bit) { Direction = ParameterDirection.Output },
                new SqlParameter("@ExpectedResolutionHours", SqlDbType.Int) { Direction = ParameterDirection.Output }
            };

            var result = await _dbHelper.ExecuteStoredProcedureAsync<FileComplaintResponse>(
                "sp_FileCustomerComplaint",
                parameters
            );

            return result;
        }

        public async Task<ComplaintRefundCalculation> CalculateComplaintRefundAsync(long complaintId)
        {
            var parameters = new[]
            {
                new SqlParameter("@ComplaintId", complaintId),
                new SqlParameter("@RefundAmount", SqlDbType.Decimal) { Direction = ParameterDirection.Output, Precision = 18, Scale = 2 },
                new SqlParameter("@SeverityFactor", SqlDbType.Decimal) { Direction = ParameterDirection.Output, Precision = 5, Scale = 2 },
                new SqlParameter("@ItemValuePercentage", SqlDbType.Decimal) { Direction = ParameterDirection.Output, Precision = 5, Scale = 2 },
                new SqlParameter("@PartnerDeduction", SqlDbType.Decimal) { Direction = ParameterDirection.Output, Precision = 18, Scale = 2 }
            };

            var result = await _dbHelper.ExecuteStoredProcedureAsync<ComplaintRefundCalculation>(
                "sp_CalculateComplaintRefund",
                parameters
            );

            return result;
        }

        public async Task<CustomerComplaintModel> GetComplaintAsync(long complaintId)
        {
            var query = @"
                SELECT c.*,
                       o.c_ordernumber,
                       o.c_event_date,
                       o.c_total_amount,
                       u.c_username,
                       u.c_email,
                       co.c_catering_name
                FROM t_sys_order_complaints c
                INNER JOIN t_sys_order o ON c.c_orderid = o.c_orderid
                INNER JOIN t_sys_user u ON c.c_userid = u.c_userid
                INNER JOIN t_sys_catering_owner co ON o.c_cateringownerid = co.c_ownerid
                WHERE c.c_complaint_id = @ComplaintId";

            var parameters = new[]
            {
                new SqlParameter("@ComplaintId", complaintId)
            };

            var results = await _dbHelper.ExecuteQueryAsync<CustomerComplaintModel>(query, parameters);
            return results.Count > 0 ? results[0] : null;
        }

        public async Task<List<CustomerComplaintModel>> GetComplaintsByOrderAsync(long orderId)
        {
            var query = @"
                SELECT * FROM t_sys_order_complaints
                WHERE c_orderid = @OrderId
                ORDER BY c_created_date DESC";

            var parameters = new[]
            {
                new SqlParameter("@OrderId", orderId)
            };

            return await _dbHelper.ExecuteQueryAsync<CustomerComplaintModel>(query, parameters);
        }

        public async Task<List<CustomerComplaintModel>> GetComplaintsByUserAsync(long userId)
        {
            var query = @"
                SELECT c.*,
                       o.c_ordernumber,
                       o.c_event_date,
                       co.c_catering_name
                FROM t_sys_order_complaints c
                INNER JOIN t_sys_order o ON c.c_orderid = o.c_orderid
                INNER JOIN t_sys_catering_owner co ON o.c_cateringownerid = co.c_ownerid
                WHERE c.c_userid = @UserId
                ORDER BY c.c_created_date DESC";

            var parameters = new[]
            {
                new SqlParameter("@UserId", userId)
            };

            return await _dbHelper.ExecuteQueryAsync<CustomerComplaintModel>(query, parameters);
        }

        public async Task<List<CustomerComplaintModel>> GetPendingComplaintsAsync()
        {
            var query = @"
                SELECT c.*,
                       o.c_ordernumber,
                       o.c_event_date,
                       o.c_total_amount,
                       u.c_username,
                       u.c_email,
                       co.c_catering_name,
                       DATEDIFF(HOUR, c.c_created_date, GETDATE()) AS c_hours_open
                FROM t_sys_order_complaints c
                INNER JOIN t_sys_order o ON c.c_orderid = o.c_orderid
                INNER JOIN t_sys_user u ON c.c_userid = u.c_userid
                INNER JOIN t_sys_catering_owner co ON o.c_cateringownerid = co.c_ownerid
                WHERE c.c_status IN ('Open', 'Under_Investigation', 'Escalated')
                ORDER BY
                    CASE c.c_severity
                        WHEN 'CRITICAL' THEN 1
                        WHEN 'MAJOR' THEN 2
                        WHEN 'MINOR' THEN 3
                    END,
                    c.c_created_date ASC";

            return await _dbHelper.ExecuteQueryAsync<CustomerComplaintModel>(query);
        }

        public async Task<bool> ResolveComplaintAsync(ResolveComplaintDto request)
        {
            // Calculate partner penalty (same as refund amount for valid complaints)
            decimal partnerPenaltyAmount = request.IsValidComplaint ? request.RefundAmount : 0;

            // Determine status based on resolution
            string status = request.IsValidComplaint ? "Resolved" : "Rejected";

            var query = @"
                UPDATE t_sys_order_complaints
                SET c_status = @Status,
                    c_resolution_type = @ResolutionType,
                    c_admin_resolved_by = @AdminId,
                    c_resolution_date = GETDATE(),
                    c_resolution_notes = @ResolutionNotes,
                    c_refund_amount = @RefundAmount,
                    c_goodwill_credit = @GoodwillCredit,
                    c_is_valid_complaint = @IsValidComplaint,
                    c_validity_reason = @ValidityReason,
                    c_partner_penalty_amount = @PartnerPenaltyAmount,
                    c_modified_date = GETDATE()
                WHERE c_complaint_id = @ComplaintId";

            var parameters = new[]
            {
                new SqlParameter("@ComplaintId", request.ComplaintId),
                new SqlParameter("@AdminId", request.AdminId),
                new SqlParameter("@Status", status),
                new SqlParameter("@ResolutionType", request.ResolutionType),
                new SqlParameter("@ResolutionNotes", request.ResolutionNotes),
                new SqlParameter("@RefundAmount", request.RefundAmount),
                new SqlParameter("@GoodwillCredit", request.GoodwillCredit),
                new SqlParameter("@IsValidComplaint", request.IsValidComplaint),
                new SqlParameter("@ValidityReason", (object)request.ValidityReason ?? DBNull.Value),
                new SqlParameter("@PartnerPenaltyAmount", partnerPenaltyAmount)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);

            if (rowsAffected > 0 && request.RefundAmount > 0)
            {
                // Process refund
                await ProcessComplaintRefundAsync(request.ComplaintId, request.RefundAmount);
            }

            if (rowsAffected > 0 && partnerPenaltyAmount > 0)
            {
                // Deduct from partner security deposit
                await DeductFromPartnerDepositAsync(request.ComplaintId, partnerPenaltyAmount);
            }

            return rowsAffected > 0;
        }

        public async Task<bool> AddPartnerResponseAsync(long complaintId, long partnerId, string response, bool admitsFault, bool offeredReplacement)
        {
            var query = @"
                UPDATE t_sys_order_complaints
                SET c_partner_response = @Response,
                    c_partner_admits_fault = @AdmitsFault,
                    c_partner_offered_replacement = @OfferedReplacement,
                    c_partner_response_date = GETDATE(),
                    c_status = CASE
                        WHEN @AdmitsFault = 1 THEN 'Under_Investigation'
                        ELSE c_status
                    END,
                    c_modified_date = GETDATE()
                WHERE c_complaint_id = @ComplaintId";

            var parameters = new[]
            {
                new SqlParameter("@ComplaintId", complaintId),
                new SqlParameter("@Response", response),
                new SqlParameter("@AdmitsFault", admitsFault),
                new SqlParameter("@OfferedReplacement", offeredReplacement)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> EscalateComplaintAsync(long complaintId)
        {
            var query = @"
                UPDATE t_sys_order_complaints
                SET c_status = 'Escalated',
                    c_modified_date = GETDATE()
                WHERE c_complaint_id = @ComplaintId
                  AND c_status IN ('Open', 'Under_Investigation')";

            var parameters = new[]
            {
                new SqlParameter("@ComplaintId", complaintId)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<Dictionary<string, int>> GetComplaintStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = @"
                SELECT
                    c_status AS Status,
                    COUNT(*) AS Count
                FROM t_sys_order_complaints
                WHERE (@StartDate IS NULL OR c_created_date >= @StartDate)
                  AND (@EndDate IS NULL OR c_created_date <= @EndDate)
                GROUP BY c_status";

            var parameters = new[]
            {
                new SqlParameter("@StartDate", (object)startDate ?? DBNull.Value),
                new SqlParameter("@EndDate", (object)endDate ?? DBNull.Value)
            };

            var results = await _dbHelper.ExecuteQueryAsync<dynamic>(query, parameters);

            var statistics = new Dictionary<string, int>();
            foreach (var result in results)
            {
                statistics[result.Status] = result.Count;
            }

            return statistics;
        }

        #region Helper Methods

        private async Task ProcessComplaintRefundAsync(long complaintId, decimal refundAmount)
        {
            // Get complaint details
            var complaint = await GetComplaintAsync(complaintId);
            if (complaint == null) return;

            // Update payment summary
            var updatePaymentQuery = @"
                UPDATE t_sys_payment_summary
                SET c_refund_amount = ISNULL(c_refund_amount, 0) + @RefundAmount,
                    c_modified_date = GETDATE()
                WHERE c_orderid = @OrderId";

            var parameters = new[]
            {
                new SqlParameter("@OrderId", complaint.OrderId),
                new SqlParameter("@RefundAmount", refundAmount)
            };

            await _dbHelper.ExecuteNonQueryAsync(updatePaymentQuery, parameters);

            // Log refund transaction
            var insertRefundQuery = @"
                INSERT INTO t_sys_payment_transactions (
                    c_orderid, c_payment_type, c_amount, c_payment_status,
                    c_transaction_reference, c_payment_notes
                )
                VALUES (
                    @OrderId, 'REFUND', @RefundAmount, 'Completed',
                    'COMPLAINT-REFUND-' + CAST(@ComplaintId AS VARCHAR),
                    'Refund for complaint ID ' + CAST(@ComplaintId AS VARCHAR)
                )";

            await _dbHelper.ExecuteNonQueryAsync(insertRefundQuery, new[]
            {
                new SqlParameter("@OrderId", complaint.OrderId),
                new SqlParameter("@RefundAmount", refundAmount),
                new SqlParameter("@ComplaintId", complaintId)
            });
        }

        private async Task DeductFromPartnerDepositAsync(long complaintId, decimal penaltyAmount)
        {
            // Get complaint details
            var complaint = await GetComplaintAsync(complaintId);
            if (complaint == null) return;

            // Get partner ID from order
            var getPartnerQuery = @"
                SELECT c_cateringownerid FROM t_sys_order WHERE c_orderid = @OrderId";

            var partnerResults = await _dbHelper.ExecuteQueryAsync<dynamic>(
                getPartnerQuery,
                new[] { new SqlParameter("@OrderId", complaint.OrderId) }
            );

            if (partnerResults.Count == 0) return;
            long partnerId = partnerResults[0].c_cateringownerid;

            // Deduct from security deposit
            var updateDepositQuery = @"
                UPDATE t_sys_partner_security_deposits
                SET c_current_balance = c_current_balance - @PenaltyAmount,
                    c_available_balance = c_available_balance - @PenaltyAmount,
                    c_modified_date = GETDATE()
                WHERE c_ownerid = @PartnerId
                  AND c_is_active = 1";

            await _dbHelper.ExecuteNonQueryAsync(updateDepositQuery, new[]
            {
                new SqlParameter("@PartnerId", partnerId),
                new SqlParameter("@PenaltyAmount", penaltyAmount)
            });

            // Log deposit transaction
            var getDepositIdQuery = @"
                SELECT c_deposit_id, c_current_balance
                FROM t_sys_partner_security_deposits
                WHERE c_ownerid = @PartnerId AND c_is_active = 1";

            var depositResults = await _dbHelper.ExecuteQueryAsync<dynamic>(
                getDepositIdQuery,
                new[] { new SqlParameter("@PartnerId", partnerId) }
            );

            if (depositResults.Count > 0)
            {
                long depositId = depositResults[0].c_deposit_id;
                decimal balanceBefore = depositResults[0].c_current_balance + penaltyAmount;
                decimal balanceAfter = depositResults[0].c_current_balance;

                var insertTransactionQuery = @"
                    INSERT INTO t_sys_deposit_transactions (
                        c_deposit_id, c_ownerid, c_transaction_type, c_amount,
                        c_balance_before, c_balance_after, c_reason, c_reference_type, c_reference_id
                    )
                    VALUES (
                        @DepositId, @PartnerId, 'DEDUCTION', @PenaltyAmount,
                        @BalanceBefore, @BalanceAfter, 'Penalty for complaint ID ' + CAST(@ComplaintId AS VARCHAR),
                        'COMPLAINT', @ComplaintId
                    )";

                await _dbHelper.ExecuteNonQueryAsync(insertTransactionQuery, new[]
                {
                    new SqlParameter("@DepositId", depositId),
                    new SqlParameter("@PartnerId", partnerId),
                    new SqlParameter("@PenaltyAmount", penaltyAmount),
                    new SqlParameter("@BalanceBefore", balanceBefore),
                    new SqlParameter("@BalanceAfter", balanceAfter),
                    new SqlParameter("@ComplaintId", complaintId)
                });
            }
        }

        private string GetSeverityDescription(string severity)
        {
            return severity switch
            {
                "CRITICAL" => "Critical issue - Immediate attention required. May result in full event failure.",
                "MAJOR" => "Major issue - Significant impact on event quality. Requires prompt resolution.",
                "MINOR" => "Minor issue - Limited impact. Will be reviewed within standard timeframe.",
                _ => "Issue logged for review."
            };
        }

        private string GetComplaintNextSteps(bool isFlaggedSuspicious)
        {
            if (isFlaggedSuspicious)
            {
                return "⚠️ Your complaint requires additional verification due to multiple recent complaints. An admin will review with partner evidence.";
            }
            else
            {
                return "Your complaint has been logged. An admin will investigate and respond within 12-24 hours.";
            }
        }

        #endregion
    }
}
