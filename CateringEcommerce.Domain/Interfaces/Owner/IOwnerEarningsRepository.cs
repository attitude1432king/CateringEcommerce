using CateringEcommerce.Domain.Models.Owner;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CateringEcommerce.Domain.Interfaces.Owner
{
    /// <summary>
    /// Repository interface for owner earnings and withdrawal operations
    /// </summary>
    public interface IOwnerEarningsRepository
    {
        /// <summary>
        /// Get owner earnings summary
        /// </summary>
        Task<OwnerEarningsSummaryDto> GetEarningsSummaryAsync(long ownerId);

        /// <summary>
        /// Get available balance for withdrawal
        /// </summary>
        Task<AvailableBalanceDto> GetAvailableBalanceAsync(long ownerId);

        /// <summary>
        /// Get settlement history with pagination
        /// </summary>
        Task<(List<SettlementHistoryDto> Settlements, int TotalCount)> GetSettlementHistoryAsync(
            long ownerId,
            SettlementFilterDto filter);

        /// <summary>
        /// Request withdrawal
        /// </summary>
        Task<WithdrawalResponseDto> RequestWithdrawalAsync(
            long ownerId,
            WithdrawalRequestDto request);

        /// <summary>
        /// Get payout history with pagination
        /// </summary>
        Task<(List<PayoutHistoryDto> Payouts, int TotalCount)> GetPayoutHistoryAsync(
            long ownerId,
            PayoutFilterDto filter);

        /// <summary>
        /// Get transaction details
        /// </summary>
        Task<TransactionDetailsDto?> GetTransactionDetailsAsync(long ownerId, long transactionId);

        /// <summary>
        /// Get earnings chart data
        /// </summary>
        Task<EarningsChartDataDto> GetEarningsChartDataAsync(long ownerId, string period);
    }
}
