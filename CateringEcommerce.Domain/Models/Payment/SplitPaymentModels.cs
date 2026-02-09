using System;
using System.Collections.Generic;

namespace CateringEcommerce.Domain.Models.Payment
{
    // =============================================
    // Payment Transaction Models
    // =============================================

    public class PaymentTransaction
    {
        public long TransactionId { get; set; }
        public long OrderId { get; set; }
        public long UserId { get; set; }
        public long CateringOwnerId { get; set; }

        // Payment Details
        public string TransactionType { get; set; } // ADVANCE, FINAL, REFUND, COMMISSION
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } // UPI, CARD, NETBANKING, CASH, EMI
        public string PaymentGateway { get; set; } // RAZORPAY, PAYTM, PHONEPE

        // Gateway Details
        public string GatewayTransactionId { get; set; }
        public string GatewayOrderId { get; set; }
        public string GatewayPaymentId { get; set; }
        public string GatewaySignature { get; set; }

        // Payment Status
        public string PaymentStatus { get; set; } // PENDING, SUCCESS, FAILED, REFUNDED
        public string StatusReason { get; set; }

        // EMI Details
        public bool IsEMI { get; set; }
        public int? EMITenure { get; set; }
        public string EMIBank { get; set; }
        public decimal? EMIRate { get; set; }
        public decimal? EMIAmount { get; set; }

        // Timestamps
        public DateTime InitiatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public string Metadata { get; set; }
        public string IPAddress { get; set; }
    }

    public class InitiatePaymentRequest
    {
        public long OrderId { get; set; }
        public string PaymentType { get; set; } // ADVANCE, FINAL
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentGateway { get; set; }
        public bool IsEMI { get; set; }
        public long? EMIPlanId { get; set; }
    }

    public class ProcessPaymentRequest
    {
        public long OrderId { get; set; }
        public long UserId { get; set; }
        public long CateringOwnerId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentGateway { get; set; }
        public string GatewayTransactionId { get; set; }
        public string GatewayPaymentId { get; set; }
        public string GatewaySignature { get; set; }
        public bool IsEMI { get; set; }
        public int? EMITenure { get; set; }
        public string EMIBank { get; set; }
        public decimal? EMIRate { get; set; }
        public decimal? EMIAmount { get; set; }
    }

    // =============================================
    // Order Payment Summary Models
    // =============================================

    public class OrderPaymentSummary
    {
        public long PaymentSummaryId { get; set; }
        public long OrderId { get; set; }

        // Total Amount Breakdown
        public decimal TotalAmount { get; set; }
        public decimal AdvancePercentage { get; set; }
        public decimal AdvanceAmount { get; set; }
        public decimal FinalAmount { get; set; }

        // Payment Status
        public bool AdvancePaid { get; set; }
        public DateTime? AdvancePaidDate { get; set; }
        public bool FinalPaid { get; set; }
        public DateTime? FinalPaidDate { get; set; }
        public bool PaymentCompleted { get; set; }

        // Escrow Management
        public string EscrowStatus { get; set; } // HELD, RELEASED_TO_PARTNER, REFUNDED
        public decimal? EscrowAmount { get; set; }
        public DateTime? EscrowReleasedDate { get; set; }

        // Commission
        public decimal? CommissionRate { get; set; }
        public decimal? CommissionAmount { get; set; }
        public bool CommissionPaid { get; set; }

        // Partner Payout
        public string PartnerPayoutStatus { get; set; } // PENDING, ADVANCE_RELEASED, FINAL_RELEASED, COMPLETED
        public bool PartnerAdvanceReleased { get; set; }
        public decimal? PartnerAdvanceAmount { get; set; }
        public DateTime? PartnerAdvanceReleasedDate { get; set; }
        public decimal? PartnerFinalPayout { get; set; }
        public DateTime? PartnerFinalPayoutDate { get; set; }

        public string PaymentMode { get; set; } // SPLIT, FULL_ADVANCE, FULL_CASH
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation Properties
        public string OrderNumber { get; set; }
        public string PartnerName { get; set; }
    }

    public class InitializePaymentRequest
    {
        public long OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AdvancePercentage { get; set; } = 30.00M;
        public decimal CommissionRate { get; set; }
    }

    public class InitializePaymentResponse
    {
        public long PaymentSummaryId { get; set; }
        public long OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AdvanceAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public decimal CommissionAmount { get; set; }
    }

    // =============================================
    // EMI Models
    // =============================================

    public class EMIPlan
    {
        public long EMIPlanId { get; set; }
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public decimal MinOrderValue { get; set; }
        public decimal? MaxOrderValue { get; set; }
        public int Tenure { get; set; }
        public decimal InterestRate { get; set; }
        public decimal ProcessingFee { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public string TermsAndConditions { get; set; }
        public decimal MonthlyEMI { get; set; } // Calculated
    }

    public class EMICalculationRequest
    {
        public decimal OrderAmount { get; set; }
        public long EMIPlanId { get; set; }
    }

    public class EMICalculationResponse
    {
        public decimal TotalAmount { get; set; }
        public decimal ProcessingFee { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal MonthlyEMI { get; set; }
        public int Tenure { get; set; }
        public decimal TotalPayable { get; set; }
    }

    // =============================================
    // Escrow Models
    // =============================================

    public class EscrowLedger
    {
        public long EscrowId { get; set; }
        public long OrderId { get; set; }
        public long? TransactionId { get; set; }
        public string TransactionType { get; set; } // CREDIT, DEBIT, HOLD, RELEASE
        public decimal Amount { get; set; }
        public decimal? Balance { get; set; }
        public string FromEntity { get; set; }
        public string ToEntity { get; set; }
        public string Status { get; set; }
        public string StatusReason { get; set; }
        public bool RequiresApproval { get; set; }
        public long? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class EscrowDashboard
    {
        public decimal TotalEscrowBalance { get; set; }
        public int PendingReleases { get; set; }
        public decimal PendingReleaseAmount { get; set; }
        public int CompletedReleasesToday { get; set; }
        public decimal CompletedReleaseAmountToday { get; set; }
        public List<EscrowLedger> RecentTransactions { get; set; }
    }

    // =============================================
    // Partner Payout Models
    // =============================================

    public class PartnerPayoutRequest
    {
        public long PayoutRequestId { get; set; }
        public long OrderId { get; set; }
        public long CateringOwnerId { get; set; }
        public string RequestType { get; set; } // ADVANCE, FINAL
        public decimal RequestAmount { get; set; }
        public string RequestStatus { get; set; } // PENDING, APPROVED, REJECTED, PROCESSED

        // Bank Details
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string IFSCCode { get; set; }
        public string AccountHolderName { get; set; }

        // Processing
        public long? ProcessedBy { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public string TransactionReference { get; set; }
        public string StatusReason { get; set; }

        public DateTime RequestedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation Properties
        public string OrderNumber { get; set; }
        public string PartnerName { get; set; }
    }

    public class ReleaseAdvanceRequest
    {
        public long OrderId { get; set; }
        public long ApprovedBy { get; set; }
        public string Remarks { get; set; }
    }

    public class ProcessFinalPayoutRequest
    {
        public long OrderId { get; set; }
        public long ProcessedBy { get; set; }
        public string TransactionReference { get; set; }
        public string Remarks { get; set; }
    }

    // =============================================
    // Payment Gateway Models
    // =============================================

    public class PaymentGatewayConfig
    {
        public long ConfigId { get; set; }
        public string GatewayName { get; set; }
        public string APIKey { get; set; }
        public string APISecret { get; set; }
        public string MerchantId { get; set; }
        public string WebhookUrl { get; set; }
        public string RedirectUrl { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsTest { get; set; }
        public int Priority { get; set; }
        public bool SupportsUPI { get; set; }
        public bool SupportsCard { get; set; }
        public bool SupportsNetBanking { get; set; }
        public bool SupportsEMI { get; set; }
    }

    public class CreatePaymentOrderRequest
    {
        public long OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public string PaymentType { get; set; } // ADVANCE, FINAL
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
    }

    public class CreatePaymentOrderResponse
    {
        public string GatewayOrderId { get; set; }
        public string GatewayKey { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string PaymentType { get; set; }
    }

    public class VerifyPaymentRequest
    {
        public string GatewayOrderId { get; set; }
        public string GatewayPaymentId { get; set; }
        public string GatewaySignature { get; set; }
    }

    // =============================================
    // Dashboard/Reporting Models
    // =============================================

    public class PaymentDashboard
    {
        public decimal TodayRevenue { get; set; }
        public decimal TodayAdvancePayments { get; set; }
        public decimal TodayFinalPayments { get; set; }
        public int TodayTransactionCount { get; set; }
        public decimal EscrowBalance { get; set; }
        public int PendingAdvanceReleases { get; set; }
        public int PendingFinalPayouts { get; set; }
        public List<PaymentTransaction> RecentTransactions { get; set; }
    }

    public class PartnerPayoutDashboard
    {
        public decimal TotalEarnings { get; set; }
        public decimal AdvanceReceived { get; set; }
        public decimal FinalPayoutPending { get; set; }
        public decimal TotalCommissionDeducted { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public List<PartnerPayoutRequest> RecentPayouts { get; set; }
    }
}
