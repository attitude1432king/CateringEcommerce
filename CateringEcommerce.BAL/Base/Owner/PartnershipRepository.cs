using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CateringEcommerce.BAL.Base.Owner
{
    public class PartnershipRepository : IPartnershipRepository
    {
        private readonly IDatabaseHelper _dbHelper;

        public PartnershipRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<PartnershipTierModel> GetPartnerTierAsync(long ownerId)
        {
            var query = $@"
                SELECT vpt.*,
                       (vpt.c_tier_lock_end_date::date - CURRENT_DATE) AS c_days_remaining_in_lock,
                       co.c_catering_name,
                       co.c_owner_name,
                       co.c_email
                FROM {Table.SysOwnerPartnershipTiers} vpt
                INNER JOIN {Table.SysCateringOwner} co ON vpt.c_ownerid = co.c_ownerid
                WHERE vpt.c_ownerid = @OwnerId";

            var parameters = new[]
            {
                new NpgsqlParameter("@OwnerId", ownerId)
            };

            var results = await _dbHelper.ExecuteQueryAsync<PartnershipTierModel>(query, parameters);
            return results.Count > 0 ? results[0] : null;
        }

        public async Task<PartnershipDashboard> GetPartnerDashboardAsync(long ownerId)
        {
            var query = $@"
                SELECT
                    vpt.c_tier_name AS CurrentTierName,
                    vpt.c_current_commission_rate AS CurrentCommissionRate,
                    vpt.c_next_tier_name AS NextTierName,
                    vpt.c_next_tier_commission_rate AS NextTierCommissionRate,
                    vpt.c_next_tier_effective_date AS NextTierEffectiveDate,
                    vpt.c_is_lock_period_active AS IsLockPeriodActive,
                    (vpt.c_tier_lock_end_date::date - CURRENT_DATE) AS DaysRemainingInLock,
                    vpt.c_completed_orders_count AS CompletedOrdersCount,
                    vpt.c_monthly_order_count AS MonthlyOrderCount,
                    vpt.c_average_rating AS AverageRating,
                    vpt.c_has_founder_badge AS HasFounderBadge,
                    vpt.c_has_featured_listing AS HasFeaturedListing,
                    vpt.c_has_priority_support AS HasPrioritySupport,
                    vpt.c_transition_notice_sent AS TransitionNoticeSent,
                    vsd.c_deposit_amount AS SecurityDepositAmount,
                    vsd.c_current_balance AS SecurityDepositBalance,
                    vsd.c_available_balance AS SecurityDepositAvailable,
                    vsd.c_holds_amount AS SecurityDepositHolds,
                    (SELECT SUM(c_amount)
                     FROM {Table.SysDepositTransactions}
                     WHERE c_ownerid = @OwnerId AND c_transaction_type = 'DEDUCTION') AS TotalDeductions,
                    (SELECT COUNT(*)
                     FROM {Table.SysOrderComplaints} oc
                     INNER JOIN {Table.SysOrders} o ON oc.c_orderid = o.c_orderid
                     WHERE o.c_cateringownerid = @OwnerId
                       AND oc.c_createddate >= NOW() - INTERVAL '3 months') AS RecentComplaintsCount
                FROM {Table.SysOwnerPartnershipTiers} vpt
                LEFT JOIN {Table.SysPartnerSecurityDeposits} vsd ON vpt.c_ownerid = vsd.c_ownerid AND vsd.c_is_active = TRUE
                WHERE vpt.c_ownerid = @OwnerId";

            var parameters = new[]
            {
                new NpgsqlParameter("@OwnerId", ownerId)
            };

            var results = await _dbHelper.ExecuteQueryAsync<PartnershipDashboard>(query, parameters);
            return results.Count > 0 ? results[0] : null;
        }

        public async Task<List<CommissionTierHistoryModel>> GetCommissionHistoryAsync(long ownerId)
        {
            var query = $@"
                SELECT * FROM {Table.SysCommissionTierHistory}
                WHERE c_ownerid = @OwnerId
                ORDER BY c_effective_date DESC";

            var parameters = new[]
            {
                new NpgsqlParameter("@OwnerId", ownerId)
            };

            return await _dbHelper.ExecuteQueryAsync<CommissionTierHistoryModel>(query, parameters);
        }

        public async Task<bool> AcknowledgeTierChangeAsync(long historyId, long ownerId)
        {
            var query = $@"
                UPDATE {Table.SysCommissionTierHistory}
                SET c_acknowledged = 1,
                    c_acknowledged_date = NOW(),
                    c_modifieddate = NOW()
                WHERE c_history_id = @HistoryId
                  AND c_ownerid = @OwnerId
                  AND c_acknowledged = 0";

            var parameters = new[]
            {
                new NpgsqlParameter("@HistoryId", historyId),
                new NpgsqlParameter("@OwnerId", ownerId)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateMonthlyOrderCountAsync(long ownerId, int orderCount)
        {
            var query = $@"
                UPDATE {Table.SysOwnerPartnershipTiers}
                SET c_monthly_order_count = @OrderCount,
                    c_modifieddate = NOW()
                WHERE c_ownerid = @OwnerId";

            var parameters = new[]
            {
                new NpgsqlParameter("@OwnerId", ownerId),
                new NpgsqlParameter("@OrderCount", orderCount)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateAverageRatingAsync(long ownerId, decimal averageRating)
        {
            var query = $@"
                UPDATE {Table.SysOwnerPartnershipTiers}
                SET c_average_rating = @AverageRating,
                    c_modifieddate = NOW()
                WHERE c_ownerid = @OwnerId";

            var parameters = new[]
            {
                new NpgsqlParameter("@OwnerId", ownerId),
                new NpgsqlParameter("@AverageRating", averageRating)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<PartnerSecurityDepositModel> GetPartnerSecurityDepositAsync(long ownerId)
        {
            var query = $@"
                SELECT * FROM {Table.SysPartnerSecurityDeposits}
                WHERE c_ownerid = @OwnerId
                  AND c_is_active = TRUE";

            var parameters = new[]
            {
                new NpgsqlParameter("@OwnerId", ownerId)
            };

            var results = await _dbHelper.ExecuteQueryAsync<PartnerSecurityDepositModel>(query, parameters);
            return results.Count > 0 ? results[0] : null;
        }

        public async Task<List<DepositTransactionModel>> GetDepositTransactionHistoryAsync(long ownerId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = $@"
                SELECT * FROM {Table.SysDepositTransactions}
                WHERE c_ownerid = @OwnerId
                  AND (@StartDate IS NULL OR c_createddate >= @StartDate)
                  AND (@EndDate IS NULL OR c_createddate <= @EndDate)
                ORDER BY c_createddate DESC";

            var parameters = new[]
            {
                new NpgsqlParameter("@OwnerId", ownerId),
                new NpgsqlParameter("@StartDate", (object)startDate ?? DBNull.Value),
                new NpgsqlParameter("@EndDate", (object)endDate ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteQueryAsync<DepositTransactionModel>(query, parameters);
        }

        public async Task<bool> RequestDepositRefundAsync(RequestDepositRefundDto request)
        {
            // Check if partner is eligible for refund
            var eligibilityQuery = $@"
                SELECT c_deposit_id, c_current_balance, c_holds_amount, c_available_balance
                FROM {Table.SysPartnerSecurityDeposits}
                WHERE c_ownerid = @OwnerId
                  AND c_is_active = TRUE
                  AND c_status = 'Active'";

            var eligibilityResults = await _dbHelper.ExecuteQueryAsync<dynamic>(
                eligibilityQuery,
                new[] { new NpgsqlParameter("@OwnerId", request.OwnerId) }
            );

            if (eligibilityResults.Count == 0)
            {
                return false; // No active deposit found
            }

            var deposit = eligibilityResults[0];
            decimal availableBalance = deposit.c_available_balance;

            if (availableBalance <= 0)
            {
                return false; // No available balance to refund
            }

            // Create refund request for full available balance
            var updateDepositQuery = $@"
                UPDATE {Table.SysPartnerSecurityDeposits}
                SET c_refund_requested = 1,
                    c_refund_request_date = NOW(),
                    c_refund_amount = @AvailableBalance,
                    c_status = 'Refund_Requested',
                    c_modifieddate = NOW()
                WHERE c_ownerid = @OwnerId
                  AND c_is_active = TRUE";

            var parameters = new[]
            {
                new NpgsqlParameter("@OwnerId", request.OwnerId),
                new NpgsqlParameter("@AvailableBalance", availableBalance)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(updateDepositQuery, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> ProcessDepositDeductionAsync(ProcessDepositDeductionDto request)
        {
            var query = $@"
                WITH deposit AS (
                    SELECT c_deposit_id, c_current_balance
                    FROM {Table.SysPartnerSecurityDeposits}
                    WHERE c_ownerid = @OwnerId AND c_is_active = TRUE
                    LIMIT 1
                ),
                updated AS (
                    UPDATE {Table.SysPartnerSecurityDeposits} psd
                    SET c_current_balance = psd.c_current_balance - @Amount,
                        c_available_balance = psd.c_available_balance - @Amount,
                        c_modifieddate = NOW()
                    FROM deposit
                    WHERE psd.c_deposit_id = deposit.c_deposit_id
                    RETURNING deposit.c_deposit_id, deposit.c_current_balance
                )
                INSERT INTO {Table.SysDepositTransactions} (
                    c_deposit_id, c_ownerid, c_transaction_type, c_amount,
                    c_balance_before, c_balance_after, c_reason, c_reference_type, c_reference_id
                )
                SELECT
                    c_deposit_id, @OwnerId, 'DEDUCTION', @Amount,
                    c_current_balance, c_current_balance - @Amount, @Reason, @ReferenceType, @ReferenceId
                FROM updated;";

            var parameters = new[]
            {
                new NpgsqlParameter("@OwnerId", request.OwnerId),
                new NpgsqlParameter("@Amount", request.Amount),
                new NpgsqlParameter("@Reason", request.Reason),
                new NpgsqlParameter("@ReferenceType", request.ReferenceType),
                new NpgsqlParameter("@ReferenceId", request.ReferenceId)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<decimal> CalculateCommissionAsync(long ownerId, decimal orderAmount)
        {
            var tier = await GetPartnerTierAsync(ownerId);
            if (tier == null)
            {
                // Default commission rate if no tier found
                return orderAmount * 0.15m; // 15%
            }

            return orderAmount * (tier.CurrentCommissionRate / 100m);
        }

        public async Task<bool> CheckAndTransitionTierAsync(long ownerId)
        {
            var query = $@"
                WITH current_tier AS (
                    SELECT c_tier_name, c_current_commission_rate, c_next_tier_name,
                           c_next_tier_commission_rate, c_tier_lock_end_date
                    FROM {Table.SysOwnerPartnershipTiers}
                    WHERE c_ownerid = @OwnerId
                      AND c_is_lock_period_active = TRUE
                      AND c_tier_lock_end_date < NOW()
                ),
                history AS (
                    INSERT INTO {Table.SysCommissionTierHistory} (
                        c_ownerid, c_previous_tier_name, c_previous_commission_rate,
                        c_new_tier_name, c_new_commission_rate, c_effective_date,
                        c_change_reason, c_acknowledged
                    )
                    SELECT
                        @OwnerId, c_tier_name, c_current_commission_rate,
                        c_next_tier_name, c_next_tier_commission_rate, NOW(),
                        'Lock period expired - transitioning to next tier', FALSE
                    FROM current_tier
                    RETURNING c_ownerid
                )
                UPDATE {Table.SysOwnerPartnershipTiers} vpt
                    SET c_tier_name = current_tier.c_next_tier_name,
                        c_current_commission_rate = current_tier.c_next_tier_commission_rate,
                        c_is_lock_period_active = FALSE,
                        c_tier_lock_end_date = NULL,
                        c_next_tier_name = NULL,
                        c_next_tier_commission_rate = NULL,
                        c_next_tier_effective_date = NULL,
                        c_modifieddate = NOW()
                FROM current_tier
                WHERE vpt.c_ownerid = @OwnerId;";

            var parameters = new[]
            {
                new NpgsqlParameter("@OwnerId", ownerId)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }
    }
}

