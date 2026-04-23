using CateringEcommerce.API.Attributes;
using CateringEcommerce.API.Hubs;
using CateringEcommerce.API.Notification;
using CateringEcommerce.BAL.Base.Admin;
using CateringEcommerce.BAL.Base.Common;
using CateringEcommerce.BAL.Base.Order;
using CateringEcommerce.BAL.Base.Owner;
using CateringEcommerce.BAL.Base.Owner.Dashboard;
using CateringEcommerce.BAL.Base.Owner.Menu;
using CateringEcommerce.BAL.Base.Supervisor;
using CateringEcommerce.BAL.Base.User;
using CateringEcommerce.BAL.Common;
using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.Configuration.Providers;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces.Sms;
using CateringEcommerce.Domain.Models.Notification;
using CateringEcommerce.BAL.Notification;
using CateringEcommerce.BAL.Services;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Interfaces.Notification;
using CateringEcommerce.Domain.Interfaces.Order;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Interfaces.Supervisor;
using CateringEcommerce.Domain.Interfaces.User;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using NETCore.MailKit.Core;
using RabbitMQ.Client; 
using System.Globalization;
using System.Text;
using System.Threading.RateLimiting;
using CateringEcommerce.BAL.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Core helpers
builder.Services.AddScoped<IDatabaseHelper, SqlDatabaseManager>();

// System Settings Provider (Singleton - loads all config from t_sys_settings)
var settingsProvider = new CateringEcommerce.BAL.Configuration.SystemSettingsProvider(builder.Configuration);
await settingsProvider.RefreshAsync();
builder.Services.AddSingleton<ISystemSettingsProvider>(settingsProvider);

// Add services to the container.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(settingsProvider.GetInt("SYSTEM.SESSION_TIMEOUT_MINUTES", 20));
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
// -- OTP: MSG91 (exclusively for authentication OTP flows) --
builder.Services.AddHttpClient("msg91", c =>
{
    c.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddScoped<IOtpSmsProvider, Msg91OtpProvider>();
builder.Services.AddScoped<CateringEcommerce.Domain.Interfaces.Common.ISmsService, CateringEcommerce.BAL.Configuration.SmsService>();

// -- Notifications: AWS SNS (exclusively for order/system SMS) --
builder.Services.Configure<AwsSnsSettings>(builder.Configuration.GetSection("AwsSns"));
builder.Services.AddSingleton<INotificationSmsProvider, AwsSnsNotificationProvider>();
builder.Services.Configure<EncryptionSettings>(
    builder.Configuration.GetSection("EncryptionSettings"));
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Financial Strategy Repositories
builder.Services.AddScoped<ICancellationRepository, CancellationRepository>();
builder.Services.AddScoped<IOrderModificationRepository, CateringEcommerce.BAL.Base.Order.OrderModificationRepository>();
builder.Services.AddScoped<IComplaintRepository, ComplaintRepository>();
builder.Services.AddScoped<IPartnershipRepository, PartnershipRepository>();

// Supervisor Management Repositories
builder.Services.AddScoped<ISupervisorRepository, SupervisorRepository>();
builder.Services.AddScoped<ICareersApplicationRepository, CareersApplicationRepository>();
builder.Services.AddScoped<IRegistrationRepository, RegistrationRepository>();
builder.Services.AddScoped<ISupervisorAssignmentRepository, SupervisorAssignmentRepository>();
builder.Services.AddScoped<IEventSupervisionRepository, EventSupervisionRepository>();

// Background Jobs
builder.Services.AddScoped<CateringEcommerce.BAL.Services.FinancialStrategyJobs>();

// Owner Dashboard Repositories
builder.Services.AddScoped<IOwnerCustomerRepository, OwnerCustomerRepository>();
builder.Services.AddScoped<IOwnerDashboardRepository, OwnerDashboardRepository>();
builder.Services.AddScoped<IOwnerOrderRepository, OwnerOrderManagementRepository>();
builder.Services.AddScoped<IOwnerProfile, OwnerProfile>();
builder.Services.AddScoped<IOwnerReportsRepository, OwnerReportsRepository>();
builder.Services.AddScoped<OwnerReviewRepository>();
builder.Services.AddScoped<OwnerSupportRepository>();

// Partner side Menu Repositories
builder.Services.AddScoped<IFoodItems, FoodItems>();
builder.Services.AddScoped<IPackages, Packages>();

// Partner side Owner Modules Repositories
builder.Services.AddScoped<IAvailabilityRepository, AvailabilityRepository>();
builder.Services.AddScoped<IBannerService, BannerService>();
builder.Services.AddScoped<IDecorations, Decorations>();
builder.Services.AddScoped<IStaff, Staff>();
builder.Services.AddScoped<IDiscounts, Discounts>();
builder.Services.AddScoped<IOwnerRegister, OwnerRegister>();
// IPartnershipRepository already registered at line 83
builder.Services.AddScoped<IOrderModificationService, OrderModificationService>();
builder.Services.AddScoped<IMappingSyncService, MappingSyncService>();

// User Authentication and Profile
builder.Services.AddScoped<IAuthentication, CateringEcommerce.BAL.Base.User.AuthLogic.Authentication>();
builder.Services.AddScoped<IProfileSetting, CateringEcommerce.BAL.Base.User.Profile.ProfileSetting>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IOrderService, CateringEcommerce.BAL.Base.User.OrderService>();
builder.Services.AddScoped<CateringAvailabilityService>();
builder.Services.AddScoped<CateringEcommerce.Domain.Interfaces.User.IContactRepository, CateringEcommerce.BAL.Base.User.ContactRepository>();
builder.Services.AddScoped<UserAddressService>();
builder.Services.AddScoped<IUserReviewRepository, CateringEcommerce.BAL.Base.User.UserReviewRepository>();
builder.Services.AddScoped<IFavoritesRepository, FavoritesRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();

// Public Stats (Partner Login page — live DB counts, 1h in-process cache)
builder.Services.AddScoped<IPublicStatsRepository, PublicStatsRepository>();

// OAuth Authentication (Google, Facebook, etc.)
builder.Services.AddHttpClient(); // Required for OAuth API calls
builder.Services.AddScoped<CateringEcommerce.Domain.Interfaces.Security.IOAuthRepository, CateringEcommerce.BAL.Base.Security.OAuthRepository>();

// Two-Factor Authentication & Device Trust
builder.Services.AddScoped<CateringEcommerce.BAL.Base.Security.ITwoFactorAuthService, CateringEcommerce.BAL.Base.Security.TwoFactorAuthService>();

// Notification Services
builder.Services.AddScoped<INotificationRepository, CateringEcommerce.BAL.Notification.NotificationRepository>();

// RabbitMQ Configuration (from t_sys_settings)
var rabbitMQSettings = new CateringEcommerce.BAL.Services.RabbitMQSettings
{
    Enabled = settingsProvider.GetBool("RABBITMQ.ENABLED", false),
    HostName = settingsProvider.GetString("RABBITMQ.HOSTNAME", "localhost"),
    Port = settingsProvider.GetInt("RABBITMQ.PORT", 5672),
    UserName = settingsProvider.GetString("RABBITMQ.USERNAME", "guest"),
    Password = settingsProvider.GetString("RABBITMQ.PASSWORD", "guest"),
    VirtualHost = "/"
};
builder.Services.AddSingleton(rabbitMQSettings);
builder.Services.AddSingleton<CateringEcommerce.BAL.Services.RabbitMQPublisher>();

// Common Repositories
// MappingSyncService already registered as IMappingSyncService at line 115
builder.Services.AddScoped<ILocation, Locations>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPaymentStageRepository, PaymentStageRepository>();
builder.Services.AddScoped<PaymentStageService>();
builder.Services.AddScoped<CateringEcommerce.Domain.Interfaces.Invoice.IInvoiceRepository, CateringEcommerce.BAL.Base.Common.InvoiceRepository>();
builder.Services.AddScoped<CateringEcommerce.Domain.Interfaces.Invoice.IInvoicePdfService, CateringEcommerce.BAL.Services.InvoicePdfService>();
builder.Services.AddScoped<CateringEcommerce.Domain.Interfaces.Order.IPaymentStateMachineService, CateringEcommerce.BAL.Services.PaymentStateMachineService>();
builder.Services.AddScoped<CateringEcommerce.BAL.Services.InvoiceBackgroundJobs>();
builder.Services.AddScoped<CateringEcommerce.BAL.Services.InvoiceAutomationService>();
builder.Services.AddScoped<CateringEcommerce.BAL.Services.InvoiceNotificationService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMediaRepository, MediaRepository>();
builder.Services.AddScoped<IOwnerRepository, OwnerRepository>();


// Admin All Repositories
builder.Services.AddScoped<AdminAnalyticsRepository>();
builder.Services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();
builder.Services.AddScoped<IAdminAuthRepository, AdminAuthRepository>();
builder.Services.AddScoped<IAdminCateringRepository, AdminCateringRepository>();
builder.Services.AddScoped<IAdminSupervisorRepository, AdminSupervisorRepository>();
builder.Services.AddScoped<IAdminEarningsRepository, AdminEarningsRepository>();
builder.Services.AddScoped<IAdminManagementRepository, AdminManagementRepository>();
builder.Services.AddScoped<IAdminNotificationRepository, AdminNotificationRepository>();
builder.Services.AddScoped<IAdminOrderRepository, AdminOrderRepository>();
builder.Services.AddScoped<IAdminPartnerApprovalRepository, AdminPartnerApprovalRepository>();
builder.Services.AddScoped<IAdminPartnerRequestRepository, AdminPartnerRequestRepository>();
builder.Services.AddScoped<IAdminReviewRepository, AdminReviewRepository>();
builder.Services.AddScoped<IAdminUserRepository, AdminUserRepository>();
builder.Services.AddScoped<IAdminSearchRepository, AdminSearchRepository>();
builder.Services.AddScoped<IMasterDataRepository, MasterDataRepository>();
builder.Services.AddScoped<IRBACRepository, RBACRepository>();
builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();

// Delivery Services
builder.Services.AddScoped<IEventDeliveryService, EventDeliveryService>();
builder.Services.AddScoped<ISampleDeliveryService, SampleDeliveryService>();

// Payment Services
builder.Services.AddScoped<CateringEcommerce.Domain.Interfaces.Payment.IRazorpayPaymentService, CateringEcommerce.BAL.Services.RazorpayPaymentService>();

// Notification Services
builder.Services.AddScoped<CateringEcommerce.Domain.Interfaces.Notification.INotificationHelper, CateringEcommerce.BAL.Helpers.NotificationHelper>();
builder.Services.AddScoped<CateringEcommerce.BAL.Services.INotificationService, CateringEcommerce.BAL.Services.NotificationService>();

// Email and SMS Services
builder.Services.AddScoped<CateringEcommerce.Domain.Interfaces.Common.IEmailService, CateringEcommerce.BAL.Configuration.EmailService>();

// Token Service
builder.Services.AddScoped<ITokenService, CateringEcommerce.BAL.Configuration.TokenService>();


builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidateModelAttribute>();
});
// Register the IpApiGeoLocationService with a named HttpClient
builder.Services.AddHttpClient<IGeoLocationService, IpApiGeoLocationService>(
    client =>
    {
        client.Timeout = TimeSpan.FromSeconds(2);
    });

// 1. Configure Kestrel for the overall request body size
builder.Services.Configure<KestrelServerOptions>(options =>
{
    // SECURITY FIX: Set reasonable limit to prevent DoS attacks
    // Allows multiple file uploads (images, documents, menus) up to 100 MB total
    options.Limits.MaxRequestBodySize = 104_857_600; // 100 MB (100 * 1024 * 1024)
});

// 2. Configure FormOptions to increase the limit for individual values
builder.Services.Configure<FormOptions>(options =>
{
    // SECURITY FIX: Set reasonable limits to prevent memory exhaustion
    // ValueLengthLimit: Maximum size for individual form field values
    options.ValueLengthLimit = 104_857_600; // 100 MB

    // MultipartBodyLengthLimit: Maximum size for multipart/form-data requests
    options.MultipartBodyLengthLimit = 104_857_600; // 100 MB

    // MemoryBufferThreshold: Files smaller than this are buffered in memory, larger are streamed to disk
    // Set to 2 MB to avoid loading large files entirely in memory
    options.MemoryBufferThreshold = 2_097_152; // 2 MB (2 * 1024 * 1024)
});

// --- END OF SECTION --

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = settingsProvider.GetString("JWT.ISSUER"),
            ValidAudience = settingsProvider.GetString("JWT.AUDIENCE"),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settingsProvider.GetString("JWT.KEY")))
        };

        // SECURITY FIX: Read JWT token from httpOnly cookie (fallback to Authorization header)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Try reading token from httpOnly cookies first
                var token = context.Request.Cookies["adminToken"]
                         ?? context.Request.Cookies["authToken"]
                         ?? context.Request.Cookies["supervisorToken"];

                // Fallback to Authorization header
                if (string.IsNullOrEmpty(token))
                {
                    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                    token = authHeader?.Split(" ").Last();
                }

                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated successfully");
                return Task.CompletedTask;
            }
        };
    })
    .AddCookie("CateringCookieAuth", authenticationScheme =>
    {
        authenticationScheme.ExpireTimeSpan = TimeSpan.FromDays(settingsProvider.GetInt("SYSTEM.COOKIE_EXPIRY_DAYS", 7));
        authenticationScheme.SlidingExpiration = true;
        authenticationScheme.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        authenticationScheme.Cookie.HttpOnly = true;
        authenticationScheme.Cookie.Name = "CateringAuthCookie";
        authenticationScheme.Cookie.SameSite = SameSiteMode.None;

        authenticationScheme.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };
    })
    .AddCookie("adminToken", authenticationScheme =>
    {
        authenticationScheme.ExpireTimeSpan = TimeSpan.FromDays(settingsProvider.GetInt("SYSTEM.COOKIE_EXPIRY_DAYS", 7));
        authenticationScheme.SlidingExpiration = true;
        authenticationScheme.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        authenticationScheme.Cookie.HttpOnly = true;
        authenticationScheme.Cookie.Name = "adminToken";
        authenticationScheme.Cookie.SameSite = SameSiteMode.None;

        authenticationScheme.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

// ==========================================
// SECURITY: CSRF Protection (Anti-Forgery)
// ==========================================
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "CSRF-TOKEN";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
    options.Cookie.SameSite = SameSiteMode.None;
});

// ==========================================
// SECURITY: Rate Limiting Configuration
// ==========================================
builder.Services.AddRateLimiter(options =>
{
    // Admin login rate limiting - Prevent brute force attacks
    options.AddFixedWindowLimiter("admin_login", config =>
    {
        config.PermitLimit = settingsProvider.GetInt("SECURITY.ADMIN_LOGIN_PERMITS", 3);
        config.Window = TimeSpan.FromMinutes(settingsProvider.GetInt("SECURITY.ADMIN_LOGIN_WINDOW_MINUTES", 15));
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 0;
    });

    // User login rate limiting
    options.AddFixedWindowLimiter("user_login", config =>
    {
        config.PermitLimit = settingsProvider.GetInt("SECURITY.USER_LOGIN_PERMITS", 5);
        config.Window = TimeSpan.FromMinutes(settingsProvider.GetInt("SECURITY.USER_LOGIN_WINDOW_MINUTES", 10));
        config.QueueLimit = 0;
    });

    // OTP sending rate limiting - Prevent SMS/Email spam
    options.AddSlidingWindowLimiter("otp_send", config =>
    {
        config.PermitLimit = settingsProvider.GetInt("SECURITY.OTP_SEND_PERMITS", 3);
        config.Window = TimeSpan.FromMinutes(settingsProvider.GetInt("SECURITY.OTP_SEND_WINDOW_MINUTES", 60));
        config.SegmentsPerWindow = 4;
        config.QueueLimit = 0;
    });

    // OTP verification rate limiting - Prevent brute force
    options.AddFixedWindowLimiter("otp_verify", config =>
    {
        config.PermitLimit = settingsProvider.GetInt("SECURITY.OTP_VERIFY_PERMITS", 5);
        config.Window = TimeSpan.FromMinutes(settingsProvider.GetInt("SECURITY.OTP_VERIFY_WINDOW_MINUTES", 5));
        config.QueueLimit = 0;
    });

    // API general rate limiting - Prevent DoS
    options.AddFixedWindowLimiter("api_general", config =>
    {
        config.PermitLimit = settingsProvider.GetInt("SECURITY.API_GENERAL_PERMITS", 100);
        config.Window = TimeSpan.FromMinutes(settingsProvider.GetInt("SECURITY.API_GENERAL_WINDOW_MINUTES", 1));
        config.QueueLimit = 10;
    });

    // File upload rate limiting - Prevent abuse
    options.AddFixedWindowLimiter("file_upload", config =>
    {
        config.PermitLimit = settingsProvider.GetInt("SECURITY.FILE_UPLOAD_PERMITS", 10);
        config.Window = TimeSpan.FromMinutes(settingsProvider.GetInt("SECURITY.FILE_UPLOAD_WINDOW_MINUTES", 10));
        config.QueueLimit = 0;
    });

    // Global fallback handler
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = 429; // Too Many Requests
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            success = false,
            message = "Rate limit exceeded. Please try again later.",
            retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                ? (double?)retryAfter.TotalSeconds
                : null
        }, cancellationToken: cancellationToken);
    };
});

builder.Services.AddOpenApi();

// SignalR for real-time notifications and tracking
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            // SECURITY FIX: Support both HTTP (dev) and HTTPS (production)
            // In production, remove http://localhost:5173 and use only your production domain
            policy.WithOrigins(
                      "http://localhost:5173",   // Vite dev server (development)
                      "https://localhost:5173",  // HTTPS local (if configured)
                      settingsProvider.GetString("CORS.PRODUCTION_ORIGIN", "https://yourdomain.com") // Production
                  )
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();  // SECURITY FIX: Required for httpOnly cookies
        });
});

// Configure Hangfire
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new PostgreSqlStorageOptions
        {
            QueuePollInterval = TimeSpan.FromMilliseconds(50), // FIX
            InvisibilityTimeout = TimeSpan.FromMinutes(5)
        }));

// Add Hangfire server
builder.Services.AddHangfireServer();

var app = builder.Build();
app.UseSession();

// Configure Hangfire Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() },
    DashboardTitle = "Catering E-commerce Background Jobs",
    StatsPollingInterval = 2000
});

// Schedule recurring jobs
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
RecurringJob.AddOrUpdate<PaymentReminderJob>(
    "post-event-payment-reminders",
    job => job.SendPostEventPaymentRemindersAsync(),
    Cron.Daily(10), // Run daily at 10:00 AM
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Local
    });

// Financial Strategy Background Jobs
RecurringJob.AddOrUpdate<CateringEcommerce.BAL.Services.FinancialStrategyJobs>(
    "auto-lock-guest-count",
    x => x.AutoLockGuestCount(),
    Cron.Hourly, // Every hour
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Local
    });

RecurringJob.AddOrUpdate<CateringEcommerce.BAL.Services.FinancialStrategyJobs>(
    "auto-lock-menu",
    x => x.AutoLockMenu(),
    Cron.Hourly, // Every hour
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Local
    });

RecurringJob.AddOrUpdate<CateringEcommerce.BAL.Services.FinancialStrategyJobs>(
    "commission-transition-notices",
    x => x.SendCommissionTransitionNotices(),
    Cron.Daily(9), // Daily at 9 AM
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Local
    });

RecurringJob.AddOrUpdate<CateringEcommerce.BAL.Services.FinancialStrategyJobs>(
    "escalate-stale-complaints",
    x => x.EscalateStaleComplaints(),
    "0 */2 * * *", // Every 2 hours
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Local
    });

// Invoice System Background Jobs
RecurringJob.AddOrUpdate<CateringEcommerce.BAL.Services.InvoiceBackgroundJobs>(
    "mark-overdue-invoices",
    x => x.MarkOverdueInvoicesAsync(),
    Cron.Hourly, // Every hour
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Local
    });

RecurringJob.AddOrUpdate<CateringEcommerce.BAL.Services.InvoiceBackgroundJobs>(
    "auto-generate-pre-event-invoices",
    x => x.AutoGeneratePreEventInvoicesAsync(),
    Cron.Hourly, // Every hour
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Local
    });

RecurringJob.AddOrUpdate<CateringEcommerce.BAL.Services.InvoiceBackgroundJobs>(
    "send-payment-reminders",
    x => x.SendPaymentRemindersAsync(),
    Cron.Daily(9), // Daily at 9:00 AM
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Local
    });

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles(); // Serves wwwroot

// SECURITY FIX: Directory browsing removed - prevents enumeration of uploaded files
// Files are still accessible via direct URL, but directory listing is disabled

var cultureInfo = new CultureInfo("en-GB"); // or "en-CA"
cultureInfo.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;


app.UseHttpsRedirection();

// ==========================================
// SECURITY: Security Headers Middleware
// ==========================================
app.Use(async (context, next) =>
{
    // Prevent clickjacking attacks
    context.Response.Headers.Add("X-Frame-Options", "DENY");

    // Prevent MIME-type sniffing
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

    // Enable XSS protection
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");

    // Referrer policy
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

    // Content Security Policy (CSP)
    // SECURITY FIX: Removed unsafe-inline and unsafe-eval to prevent XSS attacks
    // If you need inline scripts, use nonce-based CSP or move scripts to external files
    var cspPolicy = app.Environment.IsDevelopment()
        ? "default-src 'self'; " +
          "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +  // Dev: Allow inline for HMR/Vite
          "style-src 'self' 'unsafe-inline'; " +                 // Dev: Allow inline styles
          "img-src 'self' data: https:; " +
          "font-src 'self' data:; " +
          "connect-src 'self' ws: wss: http://localhost:* https://localhost:*; " +  // Dev: Allow Vite HMR
          "frame-ancestors 'none'"
        : "default-src 'self'; " +
          "script-src 'self'; " +                               // Prod: No inline scripts
          "style-src 'self'; " +                                // Prod: No inline styles
          "img-src 'self' data: https:; " +
          "font-src 'self' data:; " +
          "connect-src 'self' wss:; " +                         // Prod: Only secure WebSocket
          "frame-ancestors 'none'";

    context.Response.Headers.Add("Content-Security-Policy", cspPolicy);

    // Strict Transport Security (HSTS) - Force HTTPS
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers.Add("Strict-Transport-Security",
            "max-age=31536000; includeSubDomains; preload");
    }

    // Permissions Policy (formerly Feature Policy)
    context.Response.Headers.Add("Permissions-Policy",
        "geolocation=(), " +
        "microphone=(), " +
        "camera=(), " +
        "payment=(), " +
        "usb=(), " +
        "magnetometer=()");

    // Remove server header (information disclosure)
    context.Response.Headers.Remove("Server");
    context.Response.Headers.Remove("X-Powered-By");
    context.Response.Headers.Remove("X-AspNet-Version");

    await next();
});

// SECURITY: Enable rate limiting middleware
app.UseRateLimiter();

app.UseRouting();

// Only one correct UseCors line here 👇
app.UseCors("AllowReactApp");

app.UseAuthentication();

app.UseAuthorization();

try
{
    app.MapControllers();
}
catch (System.Reflection.ReflectionTypeLoadException ex)
{
    Console.WriteLine("=== REFLECTION TYPE LOAD EXCEPTION ===");
    Console.WriteLine("Main Exception: " + ex.Message);
    Console.WriteLine("\n=== LOADER EXCEPTIONS ===");

    if (ex.LoaderExceptions != null)
    {
        foreach (var loaderEx in ex.LoaderExceptions)
        {
            Console.WriteLine("\n--- Loader Exception ---");
            Console.WriteLine(loaderEx?.Message);
            Console.WriteLine(loaderEx?.StackTrace);

            if (loaderEx is System.IO.FileNotFoundException fnfEx)
            {
                Console.WriteLine($"Missing Assembly: {fnfEx.FileName}");
                Console.WriteLine($"Fusion Log: {fnfEx.FusionLog}");
            }
        }
    }

    throw; // Re-throw to stop the application
}

// Map SignalR Hubs for real-time notifications and tracking
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<SupervisorTrackingHub>("/hubs/supervisor-tracking");

app.Run();

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationMicroservice(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // RabbitMQ Connection
        services.AddSingleton<IConnection>(sp =>
        {
            var settings = sp.GetRequiredService<ISystemSettingsProvider>();
            var factory = new ConnectionFactory
            {
                HostName = settings.GetString("RABBITMQ.HOSTNAME", "localhost"),
                Port = settings.GetInt("RABBITMQ.PORT", 5672),
                UserName = settings.GetString("RABBITMQ.USERNAME", "guest"),
                Password = settings.GetString("RABBITMQ.PASSWORD", "guest"),
                VirtualHost = "/",
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                RequestedHeartbeat = TimeSpan.FromSeconds(60)
            };

            return factory.CreateConnection();
        });

        // Email Providers
        services.AddSingleton<IEmailProvider, SendGridEmailProvider>();
        services.AddSingleton<IEmailProvider, AwsSesEmailProvider>();
        services.AddScoped<CateringEcommerce.Domain.Interfaces.Notification.IEmailService, CateringEcommerce.BAL.Notification.EmailService>();

        // SMS Providers
        services.AddSingleton<INotificationSmsProvider, AwsSnsNotificationProvider>();
        services.AddScoped<CateringEcommerce.Domain.Interfaces.Notification.ISmsService, CateringEcommerce.BAL.Notification.SmsService>();

        // In-App
        services.AddSignalR();
        services.AddScoped<IInAppNotificationService, InAppNotificationService>();

        // Template Service
        services.AddMemoryCache();
        services.AddScoped<ITemplateService, TemplateService>();

        // Repositories
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<ITemplateRepository, TemplateRepository>();

        // Rate Limiter
        services.AddSingleton<IRateLimiter, CateringEcommerce.BAL.Notification.RateLimiter>();

        // Consumers
        services.AddHostedService<EmailConsumer>();
        services.AddHostedService<SmsConsumer>();
        services.AddHostedService<InAppConsumer>();

        return services;
    }
}



