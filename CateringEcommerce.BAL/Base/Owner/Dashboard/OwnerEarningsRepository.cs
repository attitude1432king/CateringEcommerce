using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace CateringEcommerce.BAL.Base.Owner.Dashboard
{
    public class OwnerEarningsRepository : IOwnerEarningsRepository
    {
        private readonly IDatabaseHelper _dbHelper;
        private const decimal MINIMUM_WITHDRAWAL_AMOUNT = 500.00m; // Minimum â‚¹500

        public OwnerEarningsRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        }

        public async Task<OwnerEarningsSummaryDto> GetEarningsSummaryAsync(long ownerId)
        {
            try
            {
                var query = $@"
                    SELECT
                        COALESCE(SUM(CASE WHEN c_status = 'RELEASED' THEN c_net_settlement_amount ELSE 0 END), 0) AS TotalEarnings,
                        COALESCE(SUM(CASE WHEN c_status = 'ESCROWED' THEN c_net_settlement_amount ELSE 0 END), 0) AS AvailableBalance,
                        COALESCE(SUM(CASE WHEN c_status = 'PENDING' THEN c_net_settlement_amount ELSE 0 END), 0) AS PendingSettlement,
                        COALESCE(SUM(c_platform_service_fee), 0) AS PlatformFees,
                        COUNT(DISTINCT c_orderid) AS TotalOrders,
                        COUNT(DISTINCT CASE WHEN c_status = 'RELEASED' THEN c_orderid END) AS CompletedOrders
                    FROM t_owner_payment
                    WHERE c_owner_id = @OwnerId;

                    SELECT c_released_at AS LastPayoutDate
                    FROM t_owner_payment
                    WHERE c_owner_id = @OwnerId
                      AND c_status = 'RELEASED'
                      AND c_released_at IS NOT NULL
                    ORDER BY c_released_at DESC
                    LIMIT 1;
                ";

                var parameters = new[] { new NpgsqlParameter("@OwnerId", ownerId) };

                var ds = await Task.Run(() => _dbHelper.ExecuteDataSet(query, parameters));

                var summary = new OwnerEarningsSummaryDto();

                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    var row = ds.Tables[0].Rows[0];
                    summary.TotalEarnings = Convert.ToDecimal(row["TotalEarnings"]);
                    summary.AvailableBalance = Convert.ToDecimal(row["AvailableBalance"]);
                    summary.PendingSettlement = Convert.ToDecimal(row["PendingSettlement"]);
                    summary.PlatformFees = Convert.ToDecimal(row["PlatformFees"]);
                    summary.TotalOrders = Convert.ToInt32(row["TotalOrders"]);
                    summary.CompletedOrders = Convert.ToInt32(row["CompletedOrders"]);
                    summary.TotalWithdrawn = summary.TotalEarnings - summary.AvailableBalance;
                }

                if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                {
                    var row = ds.Tables[1].Rows[0];
                    summary.LastPayoutDate = row["LastPayoutDate"] != DBNull.Value
                        ? Convert.ToDateTime(row["LastPayoutDate"])
                        : null;
                }

                return summary;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting earnings summary: {ex.Message}", ex);
            }
        }

        public async Task<AvailableBalanceDto> GetAvailableBalanceAsync(long ownerId)
        {
            try
            {
                var query = $@"
                    SELECT
                        COALESCE(SUM(CASE WHEN c_status = 'ESCROWED' THEN c_net_settlement_amount ELSE 0 END), 0) AS AvailableAmount,
                        COALESCE(SUM(CASE WHEN c_status = 'PENDING' THEN c_net_settlement_amount ELSE 0 END), 0) AS PendingRelease
                    FROM t_owner_payment
                    WHERE c_owner_id = @OwnerId;
                ";

                var parameters = new[] { new NpgsqlParameter("@OwnerId", ownerId) };

                var dt = await Task.Run(() => _dbHelper.Execute(query, parameters));

                var balance = new AvailableBalanceDto
                {
                    MinimumWithdrawal = MINIMUM_WITHDRAWAL_AMOUNT
                };

                if (dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    balance.AvailableAmount = Convert.ToDecimal(row["AvailableAmount"]);
                    balance.PendingRelease = Convert.ToDecimal(row["PendingRelease"]);
                }

                balance.CanWithdraw = balance.AvailableAmount >= MINIMUM_WITHDRAWAL_AMOUNT;
                if (!balance.CanWithdraw && balance.AvailableAmount > 0)
                {
                    balance.BlockReason = $"Minimum withdrawal amount is â‚¹{MINIMUM_WITHDRAWAL_AMOUNT}";
                }
                else if (balance.AvailableAmount == 0)
                {
                    balance.BlockReason = "No funds available for withdrawal";
                }

                return balance;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting available balance: {ex.Message}", ex);
            }
        }

        public async Task<(List<SettlementHistoryDto> Settlements, int TotalCount)> GetSettlementHistoryAsync(
            long ownerId,
            SettlementFilterDto filter)
        {
            try
            {
                var whereConditions = new List<string> { "c_owner_id = @OwnerId" };
                var parameters = new List<NpgsqlParameter>
                {
                    new NpgsqlParameter("@OwnerId", ownerId),
                    new NpgsqlParameter("@Offset", (filter.PageNumber - 1) * filter.PageSize),
                    new NpgsqlParameter("@PageSize", filter.PageSize)
                };

                if (filter.StartDate.HasValue)
                {
                    whereConditions.Add("c_settlement_period_start >= @StartDate");
                    parameters.Add(new NpgsqlParameter("@StartDate", filter.StartDate.Value));
                }

                if (filter.EndDate.HasValue)
                {
                    whereConditions.Add("c_settlement_period_end <= @EndDate");
                    parameters.Add(new NpgsqlParameter("@EndDate", filter.EndDate.Value));
                }

                if (!string.IsNullOrEmpty(filter.Status))
                {
                    whereConditions.Add("c_status = @Status");
                    parameters.Add(new NpgsqlParameter("@Status", filter.Status));
                }

                var whereClause = string.Join(" AND ", whereConditions);

                var query = $@"
                    SELECT COUNT(*) AS TotalCount
                    FROM t_owner_settlement
                    WHERE {whereClause};

                    SELECT
                        c_settlement_id AS SettlementId,
                        c_settlement_period_start AS PeriodStart,
                        c_settlement_period_end AS PeriodEnd,
                        c_total_gross_amount AS GrossAmount,
                        c_total_platform_fee AS PlatformFee,
                        c_total_adjustments AS Adjustments,
                        c_net_settlement_amount AS NetAmount,
                        c_status AS Status,
                        c_processed_at AS ProcessedAt,
                        c_bank_reference AS BankReference,
                        c_createddate AS CreatedAt
                    FROM t_owner_settlement
                    WHERE {whereClause}
                    ORDER BY c_createddate DESC
                    LIMIT @PageSize OFFSET @Offset;
                ";

                var ds = await Task.Run(() => _dbHelper.ExecuteDataSet(query, parameters.ToArray()));

                var settlements = new List<SettlementHistoryDto>();
                int totalCount = 0;

                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    totalCount = Convert.ToInt32(ds.Tables[0].Rows[0]["TotalCount"]);
                }

                if (ds.Tables.Count > 1)
                {
                    foreach (DataRow row in ds.Tables[1].Rows)
                    {
                        settlements.Add(new SettlementHistoryDto
                        {
                            SettlementId = Convert.ToInt64(row["SettlementId"]),
                            PeriodStart = Convert.ToDateTime(row["PeriodStart"]),
                            PeriodEnd = Convert.ToDateTime(row["PeriodEnd"]),
                            GrossAmount = Convert.ToDecimal(row["GrossAmount"]),
                            PlatformFee = Convert.ToDecimal(row["PlatformFee"]),
                            Adjustments = Convert.ToDecimal(row["Adjustments"]),
                            NetAmount = Convert.ToDecimal(row["NetAmount"]),
                            Status = row["Status"].ToString() ?? string.Empty,
                            ProcessedAt = row["ProcessedAt"] != DBNull.Value ? Convert.ToDateTime(row["ProcessedAt"]) : null,
                            BankReference = row["BankReference"] != DBNull.Value ? row["BankReference"].ToString() : null,
                            CreatedAt = Convert.ToDateTime(row["CreatedAt"])
                        });
                    }
                }

                return (settlements, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting settlement history: {ex.Message}", ex);
            }
        }

        public async Task<WithdrawalResponseDto> RequestWithdrawalAsync(long ownerId, WithdrawalRequestDto request)
        {
            try
            {
                // Validate available balance
                var balance = await GetAvailableBalanceAsync(ownerId);

                if (!balance.CanWithdraw)
                {
                    return new WithdrawalResponseDto
                    {
                        Status = "FAILED",
                        Message = balance.BlockReason ?? "Withdrawal not allowed",
                        RequestedAt = DateTime.Now
                    };
                }

                if (request.Amount > balance.AvailableAmount)
                {
                    return new WithdrawalResponseDto
                    {
                        Status = "FAILED",
                        Message = $"Insufficient balance. Available: â‚¹{balance.AvailableAmount}",
                        RequestedAt = DateTime.Now
                    };
                }

                if (request.Amount < MINIMUM_WITHDRAWAL_AMOUNT)
                {
                    return new WithdrawalResponseDto
                    {
                        Status = "FAILED",
                        Message = $"Minimum withdrawal amount is â‚¹{MINIMUM_WITHDRAWAL_AMOUNT}",
                        RequestedAt = DateTime.Now
                    };
                }

                // Create payout schedule entry
                var query = $@"
                    INSERT INTO t_owner_payout_schedule (
                        c_owner_id,
                        c_scheduled_amount,
                        c_scheduled_date,
                        c_is_released,
                        c_createddate,
                        c_modifieddate,
                        c_notes
                    )
                    VALUES (
                        @OwnerId,
                        @Amount,
                        NOW(),
                        0,
                        NOW(),
                        NOW(),
                        @Notes
                    )
                    RETURNING c_schedule_id;
                ";

                var parameters = new[]
                {
                    new NpgsqlParameter("@OwnerId", ownerId),
                    new NpgsqlParameter("@Amount", request.Amount),
                    new NpgsqlParameter("@Notes", (object?)request.Notes ?? DBNull.Value)
                };

                var dt = await Task.Run(() => _dbHelper.Execute(query, parameters));

                if (dt.Rows.Count > 0)
                {
                    var withdrawalId = Convert.ToInt64(dt.Rows[0][0]);

                    return new WithdrawalResponseDto
                    {
                        WithdrawalId = withdrawalId,
                        Amount = request.Amount,
                        Status = "PENDING",
                        RequestedAt = DateTime.Now,
                        Message = "Withdrawal request submitted successfully. Admin will process it shortly."
                    };
                }

                throw new Exception("Failed to create withdrawal request");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error requesting withdrawal: {ex.Message}", ex);
            }
        }

        public async Task<(List<PayoutHistoryDto> Payouts, int TotalCount)> GetPayoutHistoryAsync(
            long ownerId,
            PayoutFilterDto filter)
        {
            try
            {
                var whereConditions = new List<string> { "c_owner_id = @OwnerId" };
                var parameters = new List<NpgsqlParameter>
                {
                    new NpgsqlParameter("@OwnerId", ownerId),
                    new NpgsqlParameter("@Offset", (filter.PageNumber - 1) * filter.PageSize),
                    new NpgsqlParameter("@PageSize", filter.PageSize)
                };

                if (filter.StartDate.HasValue)
                {
                    whereConditions.Add("c_scheduled_date >= @StartDate");
                    parameters.Add(new NpgsqlParameter("@StartDate", filter.StartDate.Value));
                }

                if (filter.EndDate.HasValue)
                {
                    whereConditions.Add("c_scheduled_date <= @EndDate");
                    parameters.Add(new NpgsqlParameter("@EndDate", filter.EndDate.Value));
                }

                var whereClause = string.Join(" AND ", whereConditions);

                var query = $@"
                    SELECT COUNT(*) AS TotalCount
                    FROM t_owner_payout_schedule
                    WHERE {whereClause};

                    SELECT
                        c_schedule_id AS PayoutId,
                        c_scheduled_amount AS Amount,
                        c_release_method AS PaymentMethod,
                        CASE
                            WHEN c_is_released = TRUE THEN 'COMPLETED'
                            WHEN c_failed_at IS NOT NULL THEN 'FAILED'
                            ELSE 'PENDING'
                        END AS Status,
                        c_scheduled_date AS RequestedAt,
                        c_released_at AS CompletedAt,
                        c_transaction_id AS TransactionReference,
                        c_bank_reference AS BankReference,
                        c_failure_reason AS FailureReason,
                        c_createddate AS ProcessedAt
                    FROM t_owner_payout_schedule
                    WHERE {whereClause}
                    ORDER BY c_scheduled_date DESC
                    LIMIT @PageSize OFFSET @Offset;
                ";

                var ds = await Task.Run(() => _dbHelper.ExecuteDataSet(query, parameters.ToArray()));

                var payouts = new List<PayoutHistoryDto>();
                int totalCount = 0;

                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    totalCount = Convert.ToInt32(ds.Tables[0].Rows[0]["TotalCount"]);
                }

                if (ds.Tables.Count > 1)
                {
                    foreach (DataRow row in ds.Tables[1].Rows)
                    {
                        payouts.Add(new PayoutHistoryDto
                        {
                            PayoutId = Convert.ToInt64(row["PayoutId"]),
                            Amount = Convert.ToDecimal(row["Amount"]),
                            PaymentMethod = row["PaymentMethod"] != DBNull.Value ? row["PaymentMethod"].ToString() ?? string.Empty : string.Empty,
                            Status = row["Status"].ToString() ?? string.Empty,
                            RequestedAt = Convert.ToDateTime(row["RequestedAt"]),
                            ProcessedAt = row["ProcessedAt"] != DBNull.Value ? Convert.ToDateTime(row["ProcessedAt"]) : null,
                            CompletedAt = row["CompletedAt"] != DBNull.Value ? Convert.ToDateTime(row["CompletedAt"]) : null,
                            TransactionReference = row["TransactionReference"] != DBNull.Value ? row["TransactionReference"].ToString() : null,
                            BankReference = row["BankReference"] != DBNull.Value ? row["BankReference"].ToString() : null,
                            FailureReason = row["FailureReason"] != DBNull.Value ? row["FailureReason"].ToString() : null
                        });
                    }
                }

                return (payouts, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting payout history: {ex.Message}", ex);
            }
        }

        public async Task<TransactionDetailsDto?> GetTransactionDetailsAsync(long ownerId, long transactionId)
        {
            try
            {
                var query = $@"
                    SELECT
                        p.c_owner_payment_id AS TransactionId,
                        p.c_orderid AS OrderId,
                        o.c_order_number AS OrderNumber,
                        o.c_createddate AS OrderDate,
                        p.c_settlement_amount AS SettlementAmount,
                        p.c_platform_service_fee AS PlatformFee,
                        p.c_net_settlement_amount AS NetAmount,
                        p.c_status AS Status,
                        p.c_escrowed_at AS EscrowedAt,
                        p.c_released_at AS ReleasedAt,
                        p.c_payment_method AS PaymentMethod,
                        p.c_transaction_reference AS TransactionReference
                    FROM t_owner_payment p
                    INNER JOIN {Table.SysOrders} o ON p.c_orderid = o.c_orderid
                    WHERE p.c_owner_payment_id = @TransactionId
                      AND p.c_owner_id = @OwnerId;
                ";

                var parameters = new[]
                {
                    new NpgsqlParameter("@TransactionId", transactionId),
                    new NpgsqlParameter("@OwnerId", ownerId)
                };

                var dt = await Task.Run(() => _dbHelper.Execute(query, parameters));

                if (dt.Rows.Count == 0)
                    return null;

                var row = dt.Rows[0];

                return new TransactionDetailsDto
                {
                    TransactionId = Convert.ToInt64(row["TransactionId"]),
                    OrderId = Convert.ToInt64(row["OrderId"]),
                    OrderNumber = row["OrderNumber"].ToString() ?? string.Empty,
                    OrderDate = Convert.ToDateTime(row["OrderDate"]),
                    SettlementAmount = Convert.ToDecimal(row["SettlementAmount"]),
                    PlatformFee = Convert.ToDecimal(row["PlatformFee"]),
                    NetAmount = Convert.ToDecimal(row["NetAmount"]),
                    Status = row["Status"].ToString() ?? string.Empty,
                    EscrowedAt = row["EscrowedAt"] != DBNull.Value ? Convert.ToDateTime(row["EscrowedAt"]) : null,
                    ReleasedAt = row["ReleasedAt"] != DBNull.Value ? Convert.ToDateTime(row["ReleasedAt"]) : null,
                    PaymentMethod = row["PaymentMethod"] != DBNull.Value ? row["PaymentMethod"].ToString() : null,
                    TransactionReference = row["TransactionReference"] != DBNull.Value ? row["TransactionReference"].ToString() : null
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting transaction details: {ex.Message}", ex);
            }
        }

        public async Task<EarningsChartDataDto> GetEarningsChartDataAsync(long ownerId, string period)
        {
            try
            {
                string dateFormat;
                string groupBy;
                int daysBack;

                switch (period.ToLower())
                {
                    case "week":
                        dateFormat = "ddd"; // Mon, Tue, Wed
                        groupBy = "CAST(c_released_at AS DATE)";
                        daysBack = 7;
                        break;
                    case "month":
                        dateFormat = "MMM dd"; // Jan 01, Jan 02
                        groupBy = "CAST(c_released_at AS DATE)";
                        daysBack = 30;
                        break;
                    case "year":
                        dateFormat = "MMM"; // Jan, Feb, Mar
                        groupBy = "EXTRACT(YEAR FROM c_released_at), EXTRACT(MONTH FROM c_released_at)";
                        daysBack = 365;
                        break;
                    default:
                        dateFormat = "ddd";
                        groupBy = "CAST(c_released_at AS DATE)";
                        daysBack = 7;
                        break;
                }

                var query = $@"
                    SELECT
                        {groupBy} AS DateGroup,
                        CAST(c_released_at AS DATE) AS Date,
                        SUM(c_net_settlement_amount) AS Amount
                    FROM t_owner_payment
                    WHERE c_owner_id = @OwnerId
                      AND c_status = 'RELEASED'
                      AND c_released_at >= NOW() - (@DaysBack * INTERVAL '1 day')
                    GROUP BY {groupBy}, CAST(c_released_at AS DATE)
                    ORDER BY CAST(c_released_at AS DATE);
                ";

                var parameters = new[]
                {
                    new NpgsqlParameter("@OwnerId", ownerId),
                    new NpgsqlParameter("@DaysBack", daysBack)
                };

                var dt = await Task.Run(() => _dbHelper.Execute(query, parameters));

                var chartData = new EarningsChartDataDto
                {
                    Period = period,
                    Data = new List<EarningsChartPointDto>()
                };

                decimal totalEarnings = 0;

                foreach (DataRow row in dt.Rows)
                {
                    var amount = Convert.ToDecimal(row["Amount"]);
                    var date = Convert.ToDateTime(row["Date"]);

                    chartData.Data.Add(new EarningsChartPointDto
                    {
                        Label = date.ToString(dateFormat),
                        Amount = amount,
                        Date = date
                    });

                    totalEarnings += amount;
                }

                chartData.TotalEarnings = totalEarnings;

                return chartData;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting earnings chart data: {ex.Message}", ex);
            }
        }
    }
}

