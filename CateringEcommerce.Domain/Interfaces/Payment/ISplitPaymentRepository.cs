using CateringEcommerce.Domain.Models.Payment;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CateringEcommerce.Domain.Interfaces.Payment
{
    public interface ISplitPaymentRepository
    {
        // Payment Initialization
        Task<InitializePaymentResponse> InitializeOrderPaymentAsync(InitializePaymentRequest request);

        // Payment Processing
        Task<PaymentTransaction> ProcessAdvancePaymentAsync(ProcessPaymentRequest request);
        Task<PaymentTransaction> ProcessFinalPaymentAsync(ProcessPaymentRequest request);

        // Payment Summary & Details
        Task<OrderPaymentSummary> GetPaymentSummaryAsync(long orderId);
        Task<List<PaymentTransaction>> GetOrderTransactionsAsync(long orderId);
        Task<List<EscrowLedger>> GetEscrowLedgerAsync(long orderId);

        // EMI Management
        Task<List<EMIPlan>> GetAvailableEMIPlansAsync(decimal orderAmount);
        Task<EMICalculationResponse> CalculateEMIAsync(EMICalculationRequest request);

        // Partner Payout Management
        Task<bool> ReleaseAdvanceToPartnerAsync(ReleaseAdvanceRequest request);
        Task<bool> ProcessFinalPartnerPayoutAsync(ProcessFinalPayoutRequest request);
        Task<List<PartnerPayoutRequest>> GetPartnerPayoutRequestsAsync(long cateringOwnerId);

        // Admin Dashboards
        Task<PaymentDashboard> GetPaymentDashboardAsync();
        Task<PartnerPayoutDashboard> GetPartnerPayoutDashboardAsync(long cateringOwnerId);
        Task<EscrowDashboard> GetEscrowDashboardAsync();

        // Payment Gateway Configuration
        Task<PaymentGatewayConfig> GetPaymentGatewayConfigAsync(string gatewayName);
        Task<List<PaymentGatewayConfig>> GetActivePaymentGatewaysAsync();
    }
}
