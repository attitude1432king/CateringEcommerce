using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Payment;
using CateringEcommerce.Domain.Models.Payment;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Npgsql;
using CateringEcommerce.BAL.Configuration;

namespace CateringEcommerce.BAL.Base.Payment
{
    public class SplitPaymentRepository : ISplitPaymentRepository
    {
        private readonly IDatabaseHelper _dbHelper;
        public SplitPaymentRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<InitializePaymentResponse> InitializeOrderPaymentAsync(InitializePaymentRequest request)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@OrderId", request.OrderId),
                new NpgsqlParameter("@TotalAmount", request.TotalAmount),
                new NpgsqlParameter("@AdvancePercentage", request.AdvancePercentage),
                new NpgsqlParameter("@CommissionRate", request.CommissionRate)
            };

            var result = await _dbHelper.ExecuteStoredProcedureAsync<InitializePaymentResponse>(
                "sp_InitializeOrderPayment",
                parameters
            );

            return result;
        }

        public async Task<PaymentTransaction> ProcessAdvancePaymentAsync(ProcessPaymentRequest request)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@OrderId", request.OrderId),
                new NpgsqlParameter("@UserId", request.UserId),
                new NpgsqlParameter("@CateringOwnerId", request.CateringOwnerId),
                new NpgsqlParameter("@Amount", request.Amount),
                new NpgsqlParameter("@PaymentMethod", request.PaymentMethod ?? (object)DBNull.Value),
                new NpgsqlParameter("@PaymentGateway", request.PaymentGateway ?? (object)DBNull.Value),
                new NpgsqlParameter("@GatewayTransactionId", request.GatewayTransactionId ?? (object)DBNull.Value),
                new NpgsqlParameter("@GatewayPaymentId", request.GatewayPaymentId ?? (object)DBNull.Value),
                new NpgsqlParameter("@GatewaySignature", request.GatewaySignature ?? (object)DBNull.Value),
                new NpgsqlParameter("@IsEMI", request.IsEMI),
                new NpgsqlParameter("@EMITenure", request.EMITenure ?? (object)DBNull.Value),
                new NpgsqlParameter("@EMIBank", request.EMIBank ?? (object)DBNull.Value),
                new NpgsqlParameter("@EMIRate", request.EMIRate ?? (object)DBNull.Value),
                new NpgsqlParameter("@EMIAmount", request.EMIAmount ?? (object)DBNull.Value)
            };

            var result = await _dbHelper.ExecuteStoredProcedureAsync<PaymentTransaction>(
                "sp_ProcessAdvancePayment",
                parameters
            );

            return result;
        }

        public async Task<PaymentTransaction> ProcessFinalPaymentAsync(ProcessPaymentRequest request)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@OrderId", request.OrderId),
                new NpgsqlParameter("@UserId", request.UserId),
                new NpgsqlParameter("@CateringOwnerId", request.CateringOwnerId),
                new NpgsqlParameter("@Amount", request.Amount),
                new NpgsqlParameter("@PaymentMethod", request.PaymentMethod ?? (object)DBNull.Value),
                new NpgsqlParameter("@PaymentGateway", request.PaymentGateway ?? (object)DBNull.Value),
                new NpgsqlParameter("@GatewayTransactionId", request.GatewayTransactionId ?? (object)DBNull.Value),
                new NpgsqlParameter("@GatewayPaymentId", request.GatewayPaymentId ?? (object)DBNull.Value),
                new NpgsqlParameter("@GatewaySignature", request.GatewaySignature ?? (object)DBNull.Value)
            };

            var result = await _dbHelper.ExecuteStoredProcedureAsync<PaymentTransaction>(
                "sp_ProcessFinalPayment",
                parameters
            );

            return result;
        }

        public async Task<OrderPaymentSummary> GetPaymentSummaryAsync(long orderId)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@OrderId", orderId)
            };

            var result = await _dbHelper.ExecuteStoredProcedureAsync<OrderPaymentSummary>(
                "sp_GetPaymentSummary",
                parameters
            );

            return result;
        }

        public async Task<List<PaymentTransaction>> GetOrderTransactionsAsync(long orderId)
        {
            var query = $@"
                SELECT * FROM {Table.SysPaymentTransantions}
                WHERE c_orderid = @OrderId
                ORDER BY c_createddate DESC";

            var parameters = new[]
            {
                new NpgsqlParameter("@OrderId", orderId)
            };

            var results = await _dbHelper.ExecuteQueryAsync<PaymentTransaction>(query, parameters);
            return results;
        }

        public async Task<List<EscrowLedger>> GetEscrowLedgerAsync(long orderId)
        {
            var query = $@"
                SELECT * FROM {Table.SysEscrowLedger}
                WHERE c_orderid = @OrderId
                ORDER BY c_createddate DESC";

            var parameters = new[]
            {
                new NpgsqlParameter("@OrderId", orderId)
            };

            var results = await _dbHelper.ExecuteQueryAsync<EscrowLedger>(query, parameters);
            return results;
        }

        public async Task<List<EMIPlan>> GetAvailableEMIPlansAsync(decimal orderAmount)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@OrderAmount", orderAmount)
            };

            var results = await _dbHelper.ExecuteQueryAsync<EMIPlan>(
                "sp_GetEMIPlans",
                parameters,
                CommandType.StoredProcedure
            );

            return results;
        }

        public async Task<EMICalculationResponse> CalculateEMIAsync(EMICalculationRequest request)
        {
            // Get EMI plan details
            var query = $@"
                SELECT * FROM {Table.SysEMIPlan}
                WHERE c_emiplanid = @EMIPlanId AND c_isactive = TRUE";

            var parameters = new[]
            {
                new NpgsqlParameter("@EMIPlanId", request.EMIPlanId)
            };

            var plans = await _dbHelper.ExecuteQueryAsync<EMIPlan>(query, parameters);

            if (plans == null || plans.Count == 0)
            {
                throw new Exception("EMI Plan not found or inactive");
            }

            var plan = plans[0];

            // Calculate EMI
            var principal = request.OrderAmount + plan.ProcessingFee;
            var monthlyRate = plan.InterestRate / 100 / 12;
            var tenure = plan.Tenure;

            // EMI Formula: P Ã— r Ã— (1 + r)^n / ((1 + r)^n - 1)
            var monthlyEMI = principal * monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), tenure) /
                            ((decimal)Math.Pow((double)(1 + monthlyRate), tenure) - 1);

            var totalPayable = monthlyEMI * tenure;
            var interestAmount = totalPayable - request.OrderAmount - plan.ProcessingFee;

            return new EMICalculationResponse
            {
                TotalAmount = request.OrderAmount,
                ProcessingFee = plan.ProcessingFee,
                InterestAmount = interestAmount,
                MonthlyEMI = Math.Round(monthlyEMI, 2),
                Tenure = tenure,
                TotalPayable = Math.Round(totalPayable, 2)
            };
        }

        public async Task<bool> ReleaseAdvanceToPartnerAsync(ReleaseAdvanceRequest request)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@OrderId", request.OrderId),
                new NpgsqlParameter("@ApprovedBy", request.ApprovedBy)
            };

            await _dbHelper.ExecuteStoredProcedureAsync<OrderPaymentSummary>(
                "sp_ReleaseAdvanceToVendor",
                parameters
            );

            return true;
        }

        public async Task<bool> ProcessFinalPartnerPayoutAsync(ProcessFinalPayoutRequest request)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@OrderId", request.OrderId),
                new NpgsqlParameter("@ProcessedBy", request.ProcessedBy)
            };

            await _dbHelper.ExecuteStoredProcedureAsync<OrderPaymentSummary>(
                "sp_ProcessFinalVendorPayout",
                parameters
            );

            return true;
        }

        public async Task<List<PartnerPayoutRequest>> GetPartnerPayoutRequestsAsync(long cateringOwnerId)
        {
            var query = $@"
                SELECT vpr.*, o.c_ordernumber AS OrderNumber, co.c_catering_name AS VendorName
                FROM {Table.SysPartnerPayoutRequests} vpr
                INNER JOIN {Table.SysOrders} o ON vpr.c_orderid = o.c_orderid
                INNER JOIN {Table.SysCateringOwner} co ON vpr.c_cateringownerid = co.c_ownerid
                WHERE vpr.c_cateringownerid = @CateringOwnerId
                ORDER BY vpr.c_requesteddate DESC";

            var parameters = new[]
            {
                new NpgsqlParameter("@CateringOwnerId", cateringOwnerId)
            };

            var results = await _dbHelper.ExecuteQueryAsync<PartnerPayoutRequest>(query, parameters);
            return results;
        }

        public async Task<PaymentDashboard> GetPaymentDashboardAsync()
        {
            var query = $@"
                SELECT
                    COALESCE(SUM(CASE WHEN CAST(c_completeddate AS DATE) = CAST(NOW() AS DATE) THEN c_amount ELSE 0 END), 0) AS TodayRevenue,
                    COALESCE(SUM(CASE WHEN CAST(c_completeddate AS DATE) = CAST(NOW() AS DATE) AND c_transactiontype = 'ADVANCE' THEN c_amount ELSE 0 END), 0) AS TodayAdvancePayments,
                    COALESCE(SUM(CASE WHEN CAST(c_completeddate AS DATE) = CAST(NOW() AS DATE) AND c_transactiontype = 'FINAL' THEN c_amount ELSE 0 END), 0) AS TodayFinalPayments,
                    COUNT(CASE WHEN CAST(c_completeddate AS DATE) = CAST(NOW() AS DATE) THEN 1 END) AS TodayTransactionCount,
                    COALESCE((SELECT SUM(c_escrowamount) FROM {Table.SysPaymentSummary} WHERE c_escrowstatus = 'HELD'), 0) AS EscrowBalance,
                    (SELECT COUNT(*) FROM {Table.SysPaymentSummary} WHERE c_advancepaid = TRUE AND c_vendoradvancereleased = FALSE) AS PendingAdvanceReleases,
                    (SELECT COUNT(*) FROM {Table.SysPaymentSummary} WHERE c_finalpaid = TRUE AND c_vendorpayoutstatus <> 'COMPLETED') AS PendingFinalPayouts
                FROM {Table.SysPaymentTransantions}
                WHERE c_paymentstatus = 'SUCCESS'";

            var dashboards = await _dbHelper.ExecuteQueryAsync<PaymentDashboard>(query);
            var dashboard = dashboards.Count > 0 ? dashboards[0] : new PaymentDashboard();

            // Get recent transactions
            var txnQuery = $@"
                SELECT * FROM {Table.SysPaymentTransantions}
                WHERE c_paymentstatus = 'SUCCESS'
                ORDER BY c_completeddate DESC
                LIMIT 10";

            dashboard.RecentTransactions = await _dbHelper.ExecuteQueryAsync<PaymentTransaction>(txnQuery);

            return dashboard;
        }

        public async Task<PartnerPayoutDashboard> GetPartnerPayoutDashboardAsync(long cateringOwnerId)
        {
            var query = $@"
                SELECT
                    COALESCE(SUM(ps.c_vendoradvanceamount), 0) + COALESCE(SUM(ps.c_vendorfinalpayout), 0) AS TotalEarnings,
                    COALESCE(SUM(ps.c_vendoradvanceamount), 0) AS AdvanceReceived,
                    COALESCE(SUM(CASE WHEN ps.c_finalpaid = TRUE AND ps.c_vendorpayoutstatus <> 'COMPLETED' THEN ps.c_total_amount - ps.c_commissionamount ELSE 0 END), 0) AS FinalPayoutPending,
                    COALESCE(SUM(ps.c_commissionamount), 0) AS TotalCommissionDeducted,
                    COUNT(CASE WHEN ps.c_finalpaid = FALSE THEN 1 END) AS PendingOrders,
                    COUNT(CASE WHEN ps.c_paymentcompleted = TRUE THEN 1 END) AS CompletedOrders
                FROM {Table.SysPaymentSummary} ps
                INNER JOIN {Table.SysOrders} o ON ps.c_orderid = o.c_orderid
                WHERE o.c_cateringownerid = @CateringOwnerId";

            var parameters = new[]
            {
                new NpgsqlParameter("@CateringOwnerId", cateringOwnerId)
            };

            var dashboards = await _dbHelper.ExecuteQueryAsync<PartnerPayoutDashboard>(query, parameters);
            var dashboard = dashboards.Count > 0 ? dashboards[0] : new PartnerPayoutDashboard();

            // Get recent payouts
            dashboard.RecentPayouts = await GetPartnerPayoutRequestsAsync(cateringOwnerId);

            return dashboard;
        }

        public async Task<EscrowDashboard> GetEscrowDashboardAsync()
        {
            var query = $@"
                SELECT
                    COALESCE(SUM(c_escrowamount), 0) AS TotalEscrowBalance,
                    (SELECT COUNT(*) FROM {Table.SysPaymentSummary} WHERE c_advancepaid = TRUE AND c_vendoradvancereleased = FALSE) AS PendingReleases,
                    COALESCE((SELECT SUM(c_advanceamount) FROM {Table.SysPaymentSummary} WHERE c_advancepaid = TRUE AND c_vendoradvancereleased = FALSE), 0) AS PendingReleaseAmount,
                    (SELECT COUNT(*) FROM {Table.SysEscrowLedger} WHERE CAST(c_createddate AS DATE) = CAST(NOW() AS DATE) AND c_status = 'COMPLETED') AS CompletedReleasesToday,
                    COALESCE((SELECT SUM(c_amount) FROM {Table.SysEscrowLedger} WHERE CAST(c_createddate AS DATE) = CAST(NOW() AS DATE) AND c_status = 'COMPLETED'), 0) AS CompletedReleaseAmountToday
                FROM {Table.SysPaymentSummary}
                WHERE c_escrowstatus = 'HELD'";

            var dashboards = await _dbHelper.ExecuteQueryAsync<EscrowDashboard>(query);
            var dashboard = dashboards.Count > 0 ? dashboards[0] : new EscrowDashboard();

            // Get recent transactions
            var txnQuery = $@"
                SELECT * FROM {Table.SysEscrowLedger}
                ORDER BY c_createddate DESC
                LIMIT 20";

            dashboard.RecentTransactions = await _dbHelper.ExecuteQueryAsync<EscrowLedger>(txnQuery);

            return dashboard;
        }

        public async Task<PaymentGatewayConfig> GetPaymentGatewayConfigAsync(string gatewayName)
        {
            var query = $@"
                SELECT * FROM {Table.SysPaymentGatewayConfig}
                WHERE c_gatewayname = @GatewayName AND c_isenabled = TRUE";

            var parameters = new[]
            {
                new NpgsqlParameter("@GatewayName", gatewayName)
            };

            var results = await _dbHelper.ExecuteQueryAsync<PaymentGatewayConfig>(query, parameters);
            return results.Count > 0 ? results[0] : null;
        }

        public async Task<List<PaymentGatewayConfig>> GetActivePaymentGatewaysAsync()
        {
            var query = $@"
                SELECT * FROM {Table.SysPaymentGatewayConfig}
                WHERE c_isenabled = TRUE
                ORDER BY c_priority";

            var results = await _dbHelper.ExecuteQueryAsync<PaymentGatewayConfig>(query);
            return results;
        }
    }
}

