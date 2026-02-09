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
using CateringEcommerce.BAL.DatabaseHelper;
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
using CateringEcommerce.Domain.Models.Common;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using NETCore.MailKit.Core;
using RabbitMQ.Client; 
using System.Globalization;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Twilio.Base;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<CateringEcommerce.Domain.Interfaces.Common.ISmsService, CateringEcommerce.BAL.Configuration.SmsService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Core helpers
builder.Services.AddScoped<IDatabaseHelper, SqlDatabaseManager>();

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
builder.Services.AddScoped<IPartnershipRepository, PartnershipRepository>();
builder.Services.AddScoped<IOrderModificationService, OrderModificationService>();
builder.Services.AddScoped<IMappingSyncService, MappingSyncService>();

// User Authentication and Profile
builder.Services.AddScoped<IAuthentication, CateringEcommerce.BAL.Base.User.AuthLogic.Authentication>();
builder.Services.AddScoped<IProfileSetting, CateringEcommerce.BAL.Base.User.Profile.ProfileSetting>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IOrderService, CateringEcommerce.BAL.Base.User.OrderService>();
builder.Services.AddScoped<UserAddressService>();
builder.Services.AddScoped<IUserReviewRepository, CateringEcommerce.BAL.Base.User.UserReviewRepository>();
builder.Services.AddScoped<IFavoritesRepository, FavoritesRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();

// OAuth Authentication (Google, Facebook, etc.)
builder.Services.AddHttpClient(); // Required for OAuth API calls
builder.Services.AddScoped<CateringEcommerce.Domain.Interfaces.Security.IOAuthRepository, CateringEcommerce.BAL.Base.Security.OAuthRepository>();

// Two-Factor Authentication & Device Trust
builder.Services.AddScoped<CateringEcommerce.BAL.Base.Security.ITwoFactorAuthService, CateringEcommerce.BAL.Base.Security.TwoFactorAuthService>();

// Notification Services
builder.Services.AddScoped<INotificationRepository, CateringEcommerce.BAL.Notification.NotificationRepository>();

// RabbitMQ Configuration
var rabbitMQSettings = new CateringEcommerce.BAL.Services.RabbitMQSettings
{
    Enabled = builder.Configuration.GetValue<bool>("RabbitMQ:Enabled", false),
    HostName = builder.Configuration.GetValue<string>("RabbitMQ:HostName", "localhost") ?? "localhost",
    Port = builder.Configuration.GetValue<int>("RabbitMQ:Port", 5672),
    UserName = builder.Configuration.GetValue<string>("RabbitMQ:UserName", "guest") ?? "guest",
    Password = builder.Configuration.GetValue<string>("RabbitMQ:Password", "guest") ?? "guest",
    VirtualHost = builder.Configuration.GetValue<string>("RabbitMQ:VirtualHost", "/")
};
builder.Services.AddSingleton(rabbitMQSettings);
builder.Services.AddSingleton<CateringEcommerce.BAL.Services.RabbitMQPublisher>();

// Common Repositories 
builder.Services.AddScoped<MappingSyncService>();
builder.Services.AddScoped<ILocation, Locations>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPaymentStageRepository, PaymentStageRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMediaRepository, MediaRepository>();
builder.Services.AddScoped<IOwnerRepository, OwnerRepository>();


// Admin All Repositories
builder.Services.AddScoped<AdminAnalyticsRepository>();
builder.Services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();
builder.Services.AddScoped<IAdminAuthRepository, AdminAuthRepository>();
builder.Services.AddScoped<IAdminCateringRepository, AdminCateringRepository>();
builder.Services.AddScoped<IAdminEarningsRepository, AdminEarningsRepository>();
builder.Services.AddScoped<IAdminManagementRepository, AdminManagementRepository>();
builder.Services.AddScoped<IAdminNotificationRepository, AdminNotificationRepository>();    
builder.Services.AddScoped<IAdminPartnerApprovalRepository, AdminPartnerApprovalRepository>();
builder.Services.AddScoped<IAdminPartnerRequestRepository, AdminPartnerRequestRepository>();
builder.Services.AddScoped<IAdminReviewRepository, AdminReviewRepository>();
builder.Services.AddScoped<IAdminUserRepository, AdminUserRepository>();
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


builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<EncryptionSettings>(
    builder.Configuration.GetSection("EncryptionSettings"));
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
    // Set the limit to 50 MB, for example
    options.Limits.MaxRequestBodySize = long.MaxValue; // 100 MB;
});

// 2. Configure FormOptions to increase the limit for individual values
builder.Services.Configure<FormOptions>(options =>
{
    // Set the value length limit to a large value
    options.ValueLengthLimit = int.MaxValue;
    // Also increase the multipart body length limit
    options.MultipartBodyLengthLimit = long.MaxValue; ; // 100 MB
    options.MemoryBufferThreshold = int.MaxValue;
});

// --- END OF SECTION --

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        var jwt = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]))
        };

        // SECURITY FIX: Read JWT token from httpOnly cookie (fallback to Authorization header)
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // First, try to get token from cookie (most secure)
                var token = context.Request.Cookies["adminToken"]
                         ?? context.Request.Cookies["authToken"]
                         ?? context.Request.Cookies["supervisorToken"];

                // Fallback to Authorization header (for backward compatibility)
                if (string.IsNullOrEmpty(token))
                {
                    token = context.Request.Headers["Authorization"]
                        .FirstOrDefault()?.Split(" ").Last();
                }

                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            }
        };
    })
    .AddCookie("CateringCookieAuth", authenticationScheme =>
    {
        authenticationScheme.ExpireTimeSpan = TimeSpan.FromDays(7);
        authenticationScheme.SlidingExpiration = true;
        authenticationScheme.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        authenticationScheme.Cookie.HttpOnly = true;
        authenticationScheme.Cookie.Name = "CateringAuthCookie";
        authenticationScheme.Cookie.SameSite = SameSiteMode.Strict;// Adjust the path as needed

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
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// ==========================================
// SECURITY: Rate Limiting Configuration
// ==========================================
builder.Services.AddRateLimiter(options =>
{
    // Admin login rate limiting - Prevent brute force attacks
    options.AddFixedWindowLimiter("admin_login", config =>
    {
        config.PermitLimit = 3;  // 3 attempts
        config.Window = TimeSpan.FromMinutes(15);  // per 15 minutes
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 0;  // No queuing
    });

    // User login rate limiting
    options.AddFixedWindowLimiter("user_login", config =>
    {
        config.PermitLimit = 5;  // 5 attempts
        config.Window = TimeSpan.FromMinutes(10);  // per 10 minutes
        config.QueueLimit = 0;
    });

    // OTP sending rate limiting - Prevent SMS/Email spam
    options.AddSlidingWindowLimiter("otp_send", config =>
    {
        config.PermitLimit = 3;  // 3 OTPs
        config.Window = TimeSpan.FromHours(1);  // per hour
        config.SegmentsPerWindow = 4;  // Sliding window segments
        config.QueueLimit = 0;
    });

    // OTP verification rate limiting - Prevent brute force
    options.AddFixedWindowLimiter("otp_verify", config =>
    {
        config.PermitLimit = 5;  // 5 verification attempts
        config.Window = TimeSpan.FromMinutes(5);  // per 5 minutes
        config.QueueLimit = 0;
    });

    // API general rate limiting - Prevent DoS
    options.AddFixedWindowLimiter("api_general", config =>
    {
        config.PermitLimit = 100;  // 100 requests
        config.Window = TimeSpan.FromMinutes(1);  // per minute
        config.QueueLimit = 10;
    });

    // File upload rate limiting - Prevent abuse
    options.AddFixedWindowLimiter("file_upload", config =>
    {
        config.PermitLimit = 10;  // 10 uploads
        config.Window = TimeSpan.FromMinutes(10);  // per 10 minutes
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
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();  // SECURITY FIX: Required for httpOnly cookies
        });
});

// Configure Hangfire
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

// Only one correct UseCors line here 👇
app.UseCors("AllowReactApp");

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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles(); // Serves wwwroot

// Optional: Add directory browsing (for testing only)
app.UseDirectoryBrowser(new DirectoryBrowserOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")),
    RequestPath = "/uploads"
});

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
    context.Response.Headers.Add("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +  // Note: Remove unsafe-* in production
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: https:; " +
        "font-src 'self' data:; " +
        "connect-src 'self' ws: wss:; " +  // Allow WebSocket for SignalR
        "frame-ancestors 'none'");

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
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:Host"],
                Port = int.Parse(configuration["RabbitMQ:Port"]),
                UserName = configuration["RabbitMQ:Username"],
                Password = configuration["RabbitMQ:Password"],
                VirtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/",
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
        services.AddSingleton<ISmsProvider, TwilioSmsProvider>();
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
