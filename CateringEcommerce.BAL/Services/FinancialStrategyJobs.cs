using CateringEcommerce.Domain.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CateringEcommerce.BAL.Services
{
    public class FinancialStrategyJobs
    {
        private readonly IDatabaseHelper _dbHelper;
        private readonly ILogger<FinancialStrategyJobs> _logger;

        public FinancialStrategyJobs(IDatabaseHelper dbHelper, ILogger<FinancialStrategyJobs> logger)
        {
            _dbHelper = dbHelper;
            _logger = logger;
        }

        /// <summary>
        /// Auto-lock guest count 5 days before event
        /// Runs every 60 minutes
        /// </summary>
        public async Task AutoLockGuestCount()
        {
            try
            {
                _logger.LogInformation("Starting AutoLockGuestCount job...");

                var result = await _dbHelper.ExecuteStoredProcedureAsync<dynamic>(
                    "sp_AutoLockGuestCount",
                    null
                );

                _logger.LogInformation($"AutoLockGuestCount completed. Orders locked: {result?.OrdersLocked ?? 0}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AutoLockGuestCount job");
                throw;
            }
        }

        /// <summary>
        /// Auto-lock menu 3 days before event
        /// Runs every 60 minutes
        /// </summary>
        public async Task AutoLockMenu()
        {
            try
            {
                _logger.LogInformation("Starting AutoLockMenu job...");

                var result = await _dbHelper.ExecuteStoredProcedureAsync<dynamic>(
                    "sp_AutoLockMenu",
                    null
                );

                _logger.LogInformation($"AutoLockMenu completed. Orders locked: {result?.OrdersLocked ?? 0}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AutoLockMenu job");
                throw;
            }
        }

        /// <summary>
        /// Send commission transition notices
        /// Runs daily at 9 AM
        /// </summary>
        public async Task SendCommissionTransitionNotices()
        {
            try
            {
                _logger.LogInformation("Starting SendCommissionTransitionNotices job...");

                // Get vendors whose lock-in expires in 60 days
                var query = @"
                    SELECT vpt.*, co.c_catering_name, co.c_email
                    FROM t_sys_vendor_partnership_tiers vpt
                    INNER JOIN t_sys_catering_owner co ON vpt.c_ownerid = co.c_ownerid
                    WHERE vpt.c_is_lock_period_active = 1
                      AND DATEDIFF(DAY, GETDATE(), vpt.c_tier_lock_end_date) = 60
                      AND vpt.c_transition_notice_sent = 0";

                var vendors = await _dbHelper.ExecuteQueryAsync<dynamic>(query);

                foreach (var vendor in vendors)
                {
                    // TODO: Send email notification
                    _logger.LogInformation($"Sending commission transition notice to vendor {vendor.c_ownerid}");

                    // Mark as sent
                    var updateQuery = @"
                        UPDATE t_sys_vendor_partnership_tiers
                        SET c_transition_notice_sent = 1,
                            c_transition_notice_sent_date = GETDATE()
                        WHERE c_tier_id = @TierId";

                    await _dbHelper.ExecuteNonQueryAsync(updateQuery, new[]
                    {
                        new SqlParameter("@TierId", vendor.c_tier_id)
                    });
                }

                _logger.LogInformation($"SendCommissionTransitionNotices completed. Notices sent: {vendors.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendCommissionTransitionNotices job");
                throw;
            }
        }

        /// <summary>
        /// Escalate unresolved complaints
        /// Runs every 2 hours
        /// </summary>
        public async Task EscalateStaleComplaints()
        {
            try
            {
                _logger.LogInformation("Starting EscalateStaleComplaints job...");

                var query = @"
                    UPDATE t_sys_order_complaints
                    SET c_status = 'Escalated',
                        c_modified_date = GETDATE()
                    WHERE c_status = 'Open'
                      AND DATEDIFF(HOUR, c_created_date, GETDATE()) > 12
                      AND c_severity IN ('CRITICAL', 'MAJOR')";

                var escalatedCount = await _dbHelper.ExecuteNonQueryAsync(query);

                _logger.LogInformation($"EscalateStaleComplaints completed. Complaints escalated: {escalatedCount}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EscalateStaleComplaints job");
                throw;
            }
        }
    }
}
