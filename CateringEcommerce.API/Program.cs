using CateringEcommerce.API.Attributes;
using CateringEcommerce.API.Notification;
using CateringEcommerce.BAL.Base.User;
using CateringEcommerce.BAL.Common;
using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.Notification;
using CateringEcommerce.BAL.Services;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Interfaces.Notification;
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
using Twilio.Base;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<CateringEcommerce.Domain.Interfaces.Common.ISmsService, CateringEcommerce.BAL.Configuration.SmsService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Delivery Services
builder.Services.AddScoped<ISampleDeliveryService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var connStr = config.GetConnectionString("DefaultConnection");
    return new CateringEcommerce.BAL.Base.Common.SampleDeliveryService(connStr);
});
builder.Services.AddScoped<IEventDeliveryService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var connStr = config.GetConnectionString("DefaultConnection");
    return new CateringEcommerce.BAL.Base.Common.EventDeliveryService(connStr);
});
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

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyMethod()
                  .AllowAnyHeader();
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

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

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
