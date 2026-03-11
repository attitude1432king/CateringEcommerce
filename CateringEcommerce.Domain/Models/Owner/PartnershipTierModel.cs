using System;

namespace CateringEcommerce.Domain.Models.Owner
{
    /// <summary>
    /// Model for partner partnership tier and commission structure
    /// </summary>
    public class PartnershipTierModel
    {
        public long TierId { get; set; }
        public long OwnerId { get; set; }

        // Current Tier
        public string TierName { get; set; } // FOUNDER_PARTNER, LAUNCH_PARTNER, EARLY_ADOPTER, STANDARD, PREMIUM
        public decimal CurrentCommissionRate { get; set; }

        // Tier Assignment
        public DateTime TierStartDate { get; set; }
        public DateTime? TierLockEndDate { get; set; }
        public bool IsLockPeriodActive { get; set; }
        public int DaysRemainingInLock { get; set; }

        // Qualification Criteria
        public DateTime JoiningDate { get; set; }
        public int JoiningOrderNumber { get; set; }
        public int RequiredOrdersForLock { get; set; }
        public int CompletedOrdersCount { get; set; }
        public bool LockQualified { get; set; }
        public DateTime? LockQualifiedDate { get; set; }

        // Performance-Based Commission Adjustment
        public int MonthlyOrderCount { get; set; }
        public decimal AverageRating { get; set; }
        public bool QualifiesForReducedCommission { get; set; }
        public decimal? PerformanceCommissionRate { get; set; }

        // Next Tier Transition
        public string NextTierName { get; set; }
        public decimal? NextTierCommissionRate { get; set; }
        public DateTime? NextTierEffectiveDate { get; set; }
        public bool TransitionNoticeSent { get; set; }
        public DateTime? TransitionNoticeSentDate { get; set; }

        // Badges & Benefits
        public bool HasFounderBadge { get; set; }
        public bool HasFeaturedListing { get; set; }
        public bool HasPrioritySupport { get; set; }
        public bool HasAccountManager { get; set; }

        // Audit
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    /// <summary>
    /// Commission tier history record
    /// </summary>
    public class CommissionTierHistoryModel
    {
        public long HistoryId { get; set; }
        public long OwnerId { get; set; }

        // Change Details
        public string OldTierName { get; set; }
        public string NewTierName { get; set; }
        public decimal OldCommissionRate { get; set; }
        public decimal NewCommissionRate { get; set; }

        // Reason & Timing
        public string ChangeReason { get; set; }
        public DateTime EffectiveDate { get; set; }
        public int NoticePeriodDays { get; set; }
        public DateTime? NoticeSentDate { get; set; }

        // Partner Communication
        public bool PartnerNotified { get; set; }
        public bool PartnerAcknowledged { get; set; }
        public DateTime? PartnerAcknowledgmentDate { get; set; }

        // Audit
        public long? ChangedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// Partner partnership dashboard summary
    /// </summary>
    public class PartnershipDashboard
    {
        // Current Status
        public string TierName { get; set; }
        public decimal CurrentCommissionRate { get; set; }
        public string TierBadge { get; set; }

        // Lock Status
        public bool IsInLockPeriod { get; set; }
        public int DaysRemainingInLock { get; set; }
        public string LockPeriodMessage { get; set; }

        // Progress
        public int CompletedOrders { get; set; }
        public int OrdersToQualifyForLock { get; set; }
        public int OrdersToNextTier { get; set; }
        public decimal ProgressToNextTier { get; set; }

        // Performance
        public int MonthlyOrders { get; set; }
        public decimal AverageRating { get; set; }
        public bool EligibleForPerformanceBonus { get; set; }
        public decimal? PerformanceCommissionRate { get; set; }

        // Benefits
        public bool HasFeaturedListing { get; set; }
        public bool HasPrioritySupport { get; set; }
        public bool HasDedicatedAccountManager { get; set; }

        // Next Tier Preview
        public string NextTierName { get; set; }
        public decimal? NextTierCommissionRate { get; set; }
        public DateTime? NextTierEffectiveDate { get; set; }
        public int DaysUntilNextTier { get; set; }
        public string TransitionWarning { get; set; }

        // Total Earnings Impact
        public decimal TotalEarnedToDate { get; set; }
        public decimal CommissionPaidToDate { get; set; }
        public decimal SavingsFromCurrentTier { get; set; }
    }

    /// <summary>
    /// DTO for acknowledging tier change
    /// </summary>
    public class AcknowledgeTierChangeDto
    {
        public long HistoryId { get; set; }
        public long OwnerId { get; set; }
        public bool Acknowledged { get; set; }
        public string Comments { get; set; }
    }
}
