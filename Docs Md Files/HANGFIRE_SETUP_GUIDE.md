# Hangfire Background Jobs Setup Guide

## Overview
This guide explains how to set up Hangfire for automated background jobs that implement the financial strategy rules (auto-locking guest count, menu, commission transitions, etc.).

---

## Step 1: Install Hangfire NuGet Packages

Run these commands in the Package Manager Console:

```powershell
# In CateringEcommerce.API project
Install-Package Hangfire.Core
Install-Package Hangfire.SqlServer
Install-Package Hangfire.AspNetCore
```

Or add to `CateringEcommerce.API.csproj`:

```xml
<PackageReference Include="Hangfire.Core" Version="1.8.6" />
<PackageReference Include="Hangfire.SqlServer" Version="1.8.6" />
<PackageReference Include="Hangfire.AspNetCore" Version="1.8.6" />
```

---

## Step 2: Configure Hangfire in Program.cs

### Option A: .NET 6+ (Minimal API)

```csharp
using Hangfire;
using Hangfire.SqlServer;

var builder = WebApplication.CreateBuilder(args);

// Add Hangfire services
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

// Add Hangfire server
builder.Services.AddHangfireServer();

var app = builder.Build();

// Configure Hangfire Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() } // See Step 3
});

app.Run();
```

### Option B: .NET Core 3.1-5 (Startup.cs)

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Add Hangfire
    services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));

    services.AddHangfireServer();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireAuthorizationFilter() }
    });
}
```

---

## Step 3: Create Hangfire Authorization Filter

Create `Infrastructure/HangfireAuthorizationFilter.cs`:

```csharp
using Hangfire.Dashboard;

namespace CateringEcommerce.API.Infrastructure
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // In production, add proper authentication
            // For now, only allow in development
            #if DEBUG
                return true;
            #else
                var httpContext = context.GetHttpContext();
                return httpContext.User.IsInRole("Admin");
            #endif
        }
    }
}
```

---

## Step 4: Create Background Job Services

Create `BackgroundJobs/FinancialStrategyJobs.cs`:

```csharp
using CateringEcommerce.Domain.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CateringEcommerce.API.BackgroundJobs
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
```

---

## Step 5: Register Jobs in Startup

Add to `Program.cs` or `Startup.cs`:

```csharp
// Register the job service
builder.Services.AddScoped<FinancialStrategyJobs>();

// After app.UseHangfireDashboard():

// Schedule recurring jobs
RecurringJob.AddOrUpdate<FinancialStrategyJobs>(
    "auto-lock-guest-count",
    x => x.AutoLockGuestCount(),
    Cron.Hourly // Every hour
);

RecurringJob.AddOrUpdate<FinancialStrategyJobs>(
    "auto-lock-menu",
    x => x.AutoLockMenu(),
    Cron.Hourly // Every hour
);

RecurringJob.AddOrUpdate<FinancialStrategyJobs>(
    "commission-transition-notices",
    x => x.SendCommissionTransitionNotices(),
    Cron.Daily(9) // Daily at 9 AM
);

RecurringJob.AddOrUpdate<FinancialStrategyJobs>(
    "escalate-stale-complaints",
    x => x.EscalateStaleComplaints(),
    "0 */2 * * *" // Every 2 hours
);
```

---

## Step 6: Access Hangfire Dashboard

1. Run your application
2. Navigate to: `https://localhost:5001/hangfire`
3. You'll see:
   - **Recurring Jobs**: Scheduled jobs (auto-lock, notifications, etc.)
   - **Enqueued Jobs**: Jobs waiting to run
   - **Processing**: Currently running jobs
   - **Succeeded**: Completed jobs
   - **Failed**: Jobs that encountered errors

---

## Step 7: Database Tables (Auto-Created)

Hangfire will automatically create these tables in your database:
- `HangFire.Schema`
- `HangFire.Job`
- `HangFire.State`
- `HangFire.Set`
- `HangFire.List`
- `HangFire.Hash`
- `HangFire.Counter`
- `HangFire.AggregatedCounter`
- `HangFire.Server`

---

## Common Cron Expressions

```
Every minute:       Cron.Minutely()
Every hour:         Cron.Hourly()
Every 2 hours:      "0 */2 * * *"
Daily at 9 AM:      Cron.Daily(9)
Daily at midnight:  Cron.Daily()
Weekly on Monday:   Cron.Weekly(DayOfWeek.Monday)
Monthly on 1st:     Cron.Monthly()
```

---

## Monitoring & Troubleshooting

### View Job History
```csharp
// In Hangfire Dashboard, go to "Succeeded" or "Failed" tabs
// Click on a job to see:
// - Execution time
// - Parameters
// - Exception details (if failed)
// - Retry attempts
```

### Manual Job Trigger (for Testing)
```csharp
// In any controller or service:
BackgroundJob.Enqueue<FinancialStrategyJobs>(x => x.AutoLockGuestCount());
```

### Disable Jobs Temporarily
```csharp
// In Hangfire Dashboard:
// Go to "Recurring Jobs" > Click on job > Click "Delete"
// To re-enable, add the RecurringJob.AddOrUpdate code again
```

---

## Production Recommendations

1. **Use Hangfire Pro** for:
   - Better dashboard UI
   - Batches
   - Delayed jobs
   - Concurrent execution limits

2. **Configure Retry Policy**:
```csharp
GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 3 });
```

3. **Add Logging**:
```csharp
GlobalJobFilters.Filters.Add(new LogEverythingAttribute());
```

4. **Secure Dashboard**:
```csharp
// Use proper authentication in HangfireAuthorizationFilter
return httpContext.User.IsInRole("Admin");
```

---

## Testing Jobs Locally

```csharp
// Create a test endpoint
[HttpPost("test/run-auto-lock")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> TestAutoLock([FromServices] FinancialStrategyJobs jobs)
{
    await jobs.AutoLockGuestCount();
    await jobs.AutoLockMenu();
    return Ok("Jobs executed successfully");
}
```

---

**Status**: ✅ Setup guide complete. Follow steps above to implement Hangfire.
