using System;
using System.Collections.Generic;

namespace CateringEcommerce.Domain.Models.Owner
{
    /// <summary>
    /// Owner earnings summary
    /// </summary>
    public class OwnerEarningsSummaryDto
    {
        public decimal TotalEarnings { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal PendingSettlement { get; set; }
        public decimal TotalWithdrawn { get; set; }
        public decimal PlatformFees { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public DateTime? LastPayoutDate { get; set; }
    }

    /// <summary>
    /// Available balance details
    /// </summary>
    public class AvailableBalanceDto
    {
        public decimal AvailableAmount { get; set; }
        public decimal PendingRelease { get; set; }
        public decimal MinimumWithdrawal { get; set; }
        public bool CanWithdraw { get; set; }
        public string? BlockReason { get; set; }
    }

    /// <summary>
    /// Settlement history item
    /// </summary>
    public class SettlementHistoryDto
    {
        public long SettlementId { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal Adjustments { get; set; }
        public decimal NetAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ProcessedAt { get; set; }
        public string? BankReference { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Withdrawal request
    /// </summary>
    public class WithdrawalRequestDto
    {
        public decimal Amount { get; set; }
        public long BankAccountId { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Withdrawal request response
    /// </summary>
    public class WithdrawalResponseDto
    {
        public long WithdrawalId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Payout history item
    /// </summary>
    public class PayoutHistoryDto
    {
        public long PayoutId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? TransactionReference { get; set; }
        public string? BankReference { get; set; }
        public string? FailureReason { get; set; }
    }

    /// <summary>
    /// Transaction details
    /// </summary>
    public class TransactionDetailsDto
    {
        public long TransactionId { get; set; }
        public long OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal SettlementAmount { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal NetAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? EscrowedAt { get; set; }
        public DateTime? ReleasedAt { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransactionReference { get; set; }
    }

    /// <summary>
    /// Settlement filter
    /// </summary>
    public class SettlementFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// Payout filter
    /// </summary>
    public class PayoutFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// Earnings chart data
    /// </summary>
    public class EarningsChartDataDto
    {
        public List<EarningsChartPointDto> Data { get; set; } = new List<EarningsChartPointDto>();
        public decimal TotalEarnings { get; set; }
        public string Period { get; set; } = string.Empty;
    }

    /// <summary>
    /// Earnings chart point
    /// </summary>
    public class EarningsChartPointDto
    {
        public string Label { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
