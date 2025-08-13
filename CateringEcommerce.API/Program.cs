using CateringEcommerce.API.Attributes;
using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text;
using Twilio.Base;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidateModelAttribute>();
});

// 1. Configure Kestrel for the overall request body size
builder.Services.Configure<KestrelServerOptions>(options =>
{
    // Set the limit to 50 MB, for example
    options.Limits.MaxRequestBodySize = 52428800;
});

// 2. Configure FormOptions to increase the limit for individual values
builder.Services.Configure<FormOptions>(options =>
{
    // Set the value length limit to a large value
    options.ValueLengthLimit = int.MaxValue;
    // Also increase the multipart body length limit
    options.MultipartBodyLengthLimit = int.MaxValue; // Or a specific large value
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

var app = builder.Build();

// Only one correct UseCors line here 👇
app.UseCors("AllowReactApp");

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
