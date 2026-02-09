using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.Data.SqlClient;
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
            var query = @"
                SELECT vpt.*,
                       DATEDIFF(DAY, GETDATE(), vpt.c_tier_lock_end_date) AS c_days_remaining_in_lock,
                       co.c_catering_name,
                       co.c_owner_name,
                       co.c_email
                FROM t_sys_owner_partnership_tiers vpt
                INNER JOIN t_sys_catering_owner co ON vpt.c_ownerid = co.c_ownerid
                WHERE vpt.c_ownerid = @OwnerId";

            var parameters = new[]
            {
                new SqlParameter("@OwnerId", ownerId)
            };

            var results = await _dbHelper.ExecuteQueryAsync<PartnershipTierModel>(query, parameters);
            return results.Count > 0 ? results[0] : null;
        }

        public async Task<PartnershipDashboard> GetPartnerDashboardAsync(long ownerId)
        {
            var query = @"
                SELECT
                    vpt.c_tier_name AS CurrentTierName,
                    vpt.c_current_commission_rate AS CurrentCommissionRate,
                    vpt.c_next_tier_name AS NextTierName,
                    vpt.c_next_tier_commission_rate AS NextTierCommissionRate,
                    vpt.c_next_tier_effective_date AS NextTierEffectiveDate,
                    vpt.c_is_lock_period_active AS IsLockPeriodActive,
                    DATEDIFF(DAY, GETDATE(), vpt.c_tier_lock_end_date) AS DaysRemainingInLock,
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
                     FROM t_sys_deposit_transactions
                     WHERE c_ownerid = @OwnerId AND c_transaction_type = 'DEDUCTION') AS TotalDeductions,
                    (SELECT COUNT(*)
                     FROM t_sys_order_complaints oc
                     INNER JOIN t_sys_order o ON oc.c_orderid = o.c_orderid
                     WHERE o.c_cateringownerid = @OwnerId
                       AND oc.c_created_date >= DATEADD(MONTH, -3, GETDATE())) AS RecentComplaintsCount
                FROM t_sys_owner_partnership_tiers vpt
                LEFT JOIN t_sys_owner_security_deposits vsd ON vpt.c_ownerid = vsd.c_ownerid AND vsd.c_is_active = 1
                WHERE vpt.c_ownerid = @OwnerId";

            var parameters = new[]
            {
                new SqlParameter("@OwnerId", ownerId)
            };

            var results = await _dbHelper.ExecuteQueryAsync<PartnershipDashboard>(query, parameters);
            return results.Count > 0 ? results[0] : null;
        }

        public async Task<List<CommissionTierHistoryModel>> GetCommissionHistoryAsync(long ownerId)
        {
            var query = @"
                SELECT * FROM t_sys_commission_tier_history
                WHERE c_ownerid = @OwnerId
                ORDER BY c_effective_date DESC";

            var parameters = new[]
            {
                new SqlParameter("@OwnerId", ownerId)
            };

            return await _dbHelper.ExecuteQueryAsync<CommissionTierHistoryModel>(query, parameters);
        }

        public async Task<bool> AcknowledgeTierChangeAsync(long historyId, long ownerId)
        {
            var query = @"
                UPDATE t_sys_commission_tier_history
                SET c_acknowledged = 1,
                    c_acknowledged_date = GETDATE(),
                    c_modified_date = GETDATE()
                WHERE c_history_id = @HistoryId
                  AND c_ownerid = @OwnerId
                  AND c_acknowledged = 0";

            var parameters = new[]
            {
                new SqlParameter("@HistoryId", historyId),
                new SqlParameter("@OwnerId", ownerId)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateMonthlyOrderCountAsync(long ownerId, int orderCount)
        {
            var query = @"
                UPDATE t_sys_owner_partnership_tiers
                SET c_monthly_order_count = @OrderCount,
                    c_modified_date = GETDATE()
                WHERE c_ownerid = @OwnerId";

            var parameters = new[]
            {
                new SqlParameter("@OwnerId", ownerId),
                new SqlParameter("@OrderCount", orderCount)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateAverageRatingAsync(long ownerId, decimal averageRating)
        {
            var query = @"
                UPDATE t_sys_owner_partnership_tiers
                SET c_average_rating = @AverageRating,
                    c_modified_date = GETDATE()
                WHERE c_ownerid = @OwnerId";

            var parameters = new[]
            {
                new SqlParameter("@OwnerId", ownerId),
                new SqlParameter("@AverageRating", averageRating)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<PartnerSecurityDepositModel> GetPartnerSecurityDepositAsync(long ownerId)
        {
            var query = @"
                SELECT * FROM t_sys_partner_security_deposits
                WHERE c_ownerid = @OwnerId
                  AND c_is_active = 1";

            var parameters = new[]
            {
                new SqlParameter("@OwnerId", ownerId)
            };

            var results = await _dbHelper.ExecuteQueryAsync<PartnerSecurityDepositModel>(query, parameters);
            return results.Count > 0 ? results[0] : null;
        }

        public async Task<List<DepositTransactionModel>> GetDepositTransactionHistoryAsync(long ownerId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = @"
                SELECT * FROM t_sys_deposit_transactions
                WHERE c_ownerid = @OwnerId
                  AND (@StartDate IS NULL OR c_created_date >= @StartDate)
                  AND (@EndDate IS NULL OR c_created_date <= @EndDate)
                ORDER BY c_created_date DESC";

            var parameters = new[]
            {
                new SqlParameter("@OwnerId", ownerId),
                new SqlParameter("@StartDate", (object)startDate ?? DBNull.Value),
                new SqlParameter("@EndDate", (object)endDate ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteQueryAsync<DepositTransactionModel>(query, parameters);
        }

        public async Task<bool> RequestDepositRefundAsync(RequestDepositRefundDto request)
        {
            // Check if partner is eligible for refund
            var eligibilityQuery = @"
                SELECT c_deposit_id, c_current_balance, c_holds_amount, c_available_balance
                FROM t_sys_partner_security_deposits
                WHERE c_ownerid = @OwnerId
                  AND c_is_active = 1
                  AND c_status = 'Active'";

            var eligibilityResults = await _dbHelper.ExecuteQueryAsync<dynamic>(
                eligibilityQuery,
                new[] { new SqlParameter("@OwnerId", request.OwnerId) }
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
            var updateDepositQuery = @"
                UPDATE t_sys_partner_security_deposits
                SET c_refund_requested = 1,
                    c_refund_request_date = GETDATE(),
                    c_refund_amount = @AvailableBalance,
                    c_status = 'Refund_Requested',
                    c_modified_date = GETDATE()
                WHERE c_ownerid = @OwnerId
                  AND c_is_active = 1";

            var parameters = new[]
            {
                new SqlParameter("@OwnerId", request.OwnerId),
                new SqlParameter("@AvailableBalance", availableBalance)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(updateDepositQuery, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> ProcessDepositDeductionAsync(ProcessDepositDeductionDto request)
        {
            var query = @"
                DECLARE @DepositId BIGINT, @BalanceBefore DECIMAL(18,2), @BalanceAfter DECIMAL(18,2);

                SELECT @DepositId = c_deposit_id, @BalanceBefore = c_current_balance
                FROM t_sys_partner_security_deposits
                WHERE c_ownerid = @OwnerId AND c_is_active = 1;

                IF @DepositId IS NULL
                    RETURN;

                UPDATE t_sys_partner_security_deposits
                SET c_current_balance = c_current_balance - @Amount,
                    c_available_balance = c_available_balance - @Amount,
                    c_modified_date = GETDATE()
                WHERE c_deposit_id = @DepositId;

                SET @BalanceAfter = @BalanceBefore - @Amount;

                INSERT INTO t_sys_deposit_transactions (
                    c_deposit_id, c_ownerid, c_transaction_type, c_amount,
                    c_balance_before, c_balance_after, c_reason, c_reference_type, c_reference_id
                )
                VALUES (
                    @DepositId, @OwnerId, 'DEDUCTION', @Amount,
                    @BalanceBefore, @BalanceAfter, @Reason, @ReferenceType, @ReferenceId
                );";

            var parameters = new[]
            {
                new SqlParameter("@OwnerId", request.OwnerId),
                new SqlParameter("@Amount", request.Amount),
                new SqlParameter("@Reason", request.Reason),
                new SqlParameter("@ReferenceType", request.ReferenceType),
                new SqlParameter("@ReferenceId", request.ReferenceId)
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
            var query = @"
                DECLARE @CurrentTier VARCHAR(50), @NextTier VARCHAR(50), @NextTierRate DECIMAL(5,2);
                DECLARE @LockEndDate DATE, @IsLockActive BIT;

                SELECT
                    @CurrentTier = c_tier_name,
                    @NextTier = c_next_tier_name,
                    @NextTierRate = c_next_tier_commission_rate,
                    @LockEndDate = c_tier_lock_end_date,
                    @IsLockActive = c_is_lock_period_active
                FROM t_sys_partnership_tiers
                WHERE c_ownerid = @OwnerId;

                -- Check if lock period has ended
                IF @IsLockActive = 1 AND @LockEndDate < GETDATE()
                BEGIN
                    -- Create tier history record
                    INSERT INTO t_sys_commission_tier_history (
                        c_ownerid, c_previous_tier_name, c_previous_commission_rate,
                        c_new_tier_name, c_new_commission_rate, c_effective_date,
                        c_change_reason, c_acknowledged
                    )
                    VALUES (
                        @OwnerId, @CurrentTier, (SELECT c_current_commission_rate FROM t_sys_partnership_tiers WHERE c_ownerid = @OwnerId),
                        @NextTier, @NextTierRate, GETDATE(),
                        'Lock period expired - transitioning to next tier', 0
                    );

                    -- Update partner tier
                    UPDATE t_sys_owner_partnership_tiers
                    SET c_tier_name = @NextTier,
                        c_current_commission_rate = @NextTierRate,
                        c_is_lock_period_active = 0,
                        c_tier_lock_end_date = NULL,
                        c_next_tier_name = NULL,
                        c_next_tier_commission_rate = NULL,
                        c_next_tier_effective_date = NULL,
                        c_modified_date = GETDATE()
                    WHERE c_ownerid = @OwnerId;
                END";

            var parameters = new[]
            {
                new SqlParameter("@OwnerId", ownerId)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }
    }
}
