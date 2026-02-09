using CateringEcommerce.Domain.Models.Owner;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CateringEcommerce.Domain.Interfaces.Owner
{
    public interface IPartnershipRepository
    {
        /// <summary>
        /// Get partner's current partnership tier
        /// </summary>
        Task<PartnershipTierModel> GetPartnerTierAsync(long ownerId);

        /// <summary>
        /// Get comprehensive partner dashboard with tier, deposit, and performance metrics
        /// </summary>
        Task<PartnershipDashboard> GetPartnerDashboardAsync(long ownerId);

        /// <summary>
        /// Get commission tier change history for partner
        /// </summary>
        Task<List<CommissionTierHistoryModel>> GetCommissionHistoryAsync(long ownerId);

        /// <summary>
        /// Acknowledge a tier change notification
        /// </summary>
        Task<bool> AcknowledgeTierChangeAsync(long historyId, long ownerId);

        /// <summary>
        /// Update partner's monthly order count (called by background job)
        /// </summary>
        Task<bool> UpdateMonthlyOrderCountAsync(long ownerId, int orderCount);

        /// <summary>
        /// Update partner's average rating (called after review submission)
        /// </summary>
        Task<bool> UpdateAverageRatingAsync(long ownerId, decimal averageRating);

        /// <summary>
        /// Get partner's security deposit details
        /// </summary>
        Task<PartnerSecurityDepositModel> GetPartnerSecurityDepositAsync(long ownerId);

        /// <summary>
        /// Get deposit transaction history
        /// </summary>
        Task<List<DepositTransactionModel>> GetDepositTransactionHistoryAsync(long ownerId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Request refund of security deposit (partner-initiated)
        /// </summary>
        Task<bool> RequestDepositRefundAsync(RequestDepositRefundDto request);

        /// <summary>
        /// Process deduction from security deposit (admin-initiated for penalties/complaints)
        /// </summary>
        Task<bool> ProcessDepositDeductionAsync(ProcessDepositDeductionDto request);

        /// <summary>
        /// Calculate commission amount for an order based on partner's tier
        /// </summary>
        Task<decimal> CalculateCommissionAsync(long ownerId, decimal orderAmount);

        /// <summary>
        /// Check if partner's lock period has ended and transition to next tier
        /// </summary>
        Task<bool> CheckAndTransitionTierAsync(long ownerId);
    }
}
