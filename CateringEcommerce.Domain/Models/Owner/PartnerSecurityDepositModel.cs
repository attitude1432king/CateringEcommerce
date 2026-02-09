using System;

namespace CateringEcommerce.Domain.Models.Owner
{
    /// <summary>
    /// Model for partner security deposit
    /// </summary>
    public class PartnerSecurityDepositModel
    {
        public long DepositId { get; set; }
        public long OwnerId { get; set; }

        // Deposit Details
        public decimal DepositAmount { get; set; }
        public bool DepositPaid { get; set; }
        public DateTime? DepositPaidDate { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionId { get; set; }

        // Current Balance
        public decimal CurrentBalance { get; set; }
        public decimal HoldsAmount { get; set; }
        public decimal AvailableBalance { get; set; }

        // Status
        public string Status { get; set; } // Pending, Active, Depleted, Refunded
        public bool IsActive { get; set; }

        // Refund Details
        public bool RefundRequested { get; set; }
        public DateTime? RefundRequestDate { get; set; }
        public bool RefundApproved { get; set; }
        public DateTime? RefundProcessedDate { get; set; }
        public decimal? RefundAmount { get; set; }
        public string RefundTransactionId { get; set; }

        // Audit
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    /// <summary>
    /// Security deposit transaction log
    /// </summary>
    public class DepositTransactionModel
    {
        public long TransactionId { get; set; }
        public long DepositId { get; set; }
        public long OwnerId { get; set; }
        public long? OrderId { get; set; }

        // Transaction Type
        public string TransactionType { get; set; } // DEPOSIT, DEDUCTION, REFUND, HOLD, RELEASE_HOLD, TOP_UP
        public decimal Amount { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }

        // Reason & Reference
        public string Reason { get; set; }
        public string ReferenceType { get; set; } // PARTNER_NO_SHOW, COMPLAINT_REFUND, CANCELLATION_COMPENSATION, etc.
        public long? ReferenceId { get; set; }

        // Approval
        public long? ApprovedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }

        // Audit
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// DTO for requesting deposit refund
    /// </summary>
    public class RequestDepositRefundDto
    {
        public long OwnerId { get; set; }
        public string RefundReason { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankIFSC { get; set; }
        public string AccountHolderName { get; set; }
    }

    /// <summary>
    /// DTO for processing deposit deduction (Admin)
    /// </summary>
    public class ProcessDepositDeductionDto
    {
        public long OwnerId { get; set; }
        public long? OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; }
        public string ReferenceType { get; set; }
        public long? ReferenceId { get; set; }
        public long ApprovedBy { get; set; }
    }
}
