using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Hangfire;

namespace CateringEcommerce.API.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "password", "newPassword", "confirmPassword", "currentPassword", "token", "accessToken",
            "refreshToken", "authorization", "secret", "key", "otp", "card", "cookie", "set-cookie"
        };

        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IBackgroundJobClient backgroundJobClient,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _backgroundJobClient = backgroundJobClient ?? throw new ArgumentNullException(nameof(backgroundJobClient));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestBody = await TryReadRequestBodyAsync(context);

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var errorId = Guid.NewGuid();
                var entry = BuildLogEntry(context, ex, errorId, stopwatch.ElapsedMilliseconds, requestBody);

                _logger.LogError(ex, "Unhandled exception captured. ErrorId: {ErrorId}", errorId);

                try
                {
                    _backgroundJobClient.Enqueue<IErrorLoggingService>(service => service.LogAsync(entry));
                }
                catch (Exception logEx)
                {
                    _logger.LogError(logEx, "Failed to enqueue error log. ErrorId: {ErrorId}", errorId);
                }

                if (!context.Response.HasStarted)
                {
                    context.Response.Clear();
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";

                    await context.Response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        result = false,
                        success = false,
                        message = "An unexpected error occurred. Please contact support with the Error ID.",
                        errorId
                    }));
                }
            }
        }

        private ErrorLogEntry BuildLogEntry(
            HttpContext context,
            Exception exception,
            Guid errorId,
            long executionTimeMs,
            string? requestBody)
        {
            var userId = TryGetUserId(context.User);
            var role = context.User.FindFirst(ClaimTypes.Role)?.Value;
            var statusCode = context.Response.StatusCode >= 500
                ? context.Response.StatusCode
                : StatusCodes.Status500InternalServerError;

            return new ErrorLogEntry
            {
                ErrorId = errorId,
                Message = exception.Message,
                ExceptionType = exception.GetType().FullName,
                StackTrace = exception.ToString(),
                InnerException = exception.InnerException?.ToString(),
                Source = exception.Source,
                RequestPath = context.Request.Path.Value,
                RequestMethod = context.Request.Method,
                QueryParams = SerializeQuery(context.Request.Query),
                RequestBody = requestBody,
                ResponseStatusCode = statusCode,
                UserId = userId,
                UserRole = string.IsNullOrWhiteSpace(role) ? "Anonymous" : role,
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers.UserAgent.ToString(),
                TraceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier,
                CorrelationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? context.TraceIdentifier,
                Environment = _environment.EnvironmentName,
                MachineName = Environment.MachineName,
                ApplicationName = _environment.ApplicationName,
                LogLevel = "Error",
                ExecutionTimeMs = executionTimeMs > int.MaxValue ? int.MaxValue : (int)executionTimeMs
            };
        }

        private async Task<string?> TryReadRequestBodyAsync(HttpContext context)
        {
            if (!_configuration.GetValue<bool>("Logging:CaptureRequestBody"))
            {
                return null;
            }

            if (!IsJsonRequest(context.Request) || context.Request.ContentLength is null or 0)
            {
                return null;
            }

            var maxBytes = _configuration.GetValue("Logging:MaxRequestBodyBytes", 16384);
            if (context.Request.ContentLength > maxBytes)
            {
                return JsonSerializer.Serialize(new { truncated = true, reason = "Request body exceeds logging limit." });
            }

            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            return RedactJson(body);
        }

        private static bool IsJsonRequest(HttpRequest request)
        {
            return request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true;
        }

        private static long? TryGetUserId(ClaimsPrincipal user)
        {
            var claimValue = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst("AdminId")?.Value
                ?? user.FindFirst("UserId")?.Value;

            return long.TryParse(claimValue, out var userId) ? userId : null;
        }

        private static string SerializeQuery(IQueryCollection query)
        {
            if (query.Count == 0)
            {
                return "{}";
            }

            var values = query.ToDictionary(
                item => item.Key,
                item => SensitiveKeys.Contains(item.Key)
                    ? "[REDACTED]"
                    : item.Value.ToString());

            return JsonSerializer.Serialize(values);
        }

        private static string? RedactJson(string json)
        {
            try
            {
                using var document = JsonDocument.Parse(json);
                var redacted = RedactElement(document.RootElement);
                return JsonSerializer.Serialize(redacted);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static object? RedactElement(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Object => element.EnumerateObject().ToDictionary(
                    property => property.Name,
                    property => SensitiveKeys.Contains(property.Name) ? "[REDACTED]" : RedactElement(property.Value)),
                JsonValueKind.Array => element.EnumerateArray().Select(RedactElement).ToList(),
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.TryGetInt64(out var longValue) ? longValue : element.GetDecimal(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => element.ToString()
            };
        }
    }
}
