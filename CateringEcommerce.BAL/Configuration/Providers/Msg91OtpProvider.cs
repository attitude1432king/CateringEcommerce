using CateringEcommerce.Domain.Interfaces.Sms;
using CateringEcommerce.Domain.Models.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CateringEcommerce.BAL.Configuration.Providers
{
    /// <summary>
    /// SMS OTP delivery via MSG91 OTP API v5.
    /// OTP lifecycle (generation, hashing, verification) is handled by SmsService.
    /// This class only delivers the pre-generated OTP via MSG91 HTTP API.
    ///
    /// API Reference: https://docs.msg91.com/reference/send-otp
    /// </summary>
    public class Msg91OtpProvider : IOtpSmsProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<Msg91OtpProvider> _logger;

        // Settings loaded once at construction
        private readonly string _authKey;
        private readonly string _senderId;
        private readonly string _templateId;
        private readonly string _route;

        private const string Msg91BaseUrl = "https://api.msg91.com/api/v5/otp";
        private const string HttpClientName = "msg91";

        public string ProviderName => "MSG91";

        public Msg91OtpProvider(
            IOptions<Msg91Settings> options,
            IHttpClientFactory httpClientFactory,
            ILogger<Msg91OtpProvider> logger)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var settings = options.Value;
            _authKey = settings.AuthKey;
            _senderId = settings.SenderId;
            _templateId = settings.TemplateId;
            _route = settings.Route; // 4 = transactional

            //if (string.IsNullOrEmpty(_authKey))
            //    throw new InvalidOperationException("Secure configuration 'MSG91:AUTH_KEY' is required for Msg91OtpProvider.");
            //if (string.IsNullOrEmpty(_templateId))
            //    throw new InvalidOperationException("Configuration 'MSG91:TEMPLATE_ID' is required for Msg91OtpProvider.");
        }

        public async Task<OtpSendResult> SendOtpAsync(
            string phoneNumber,
            string otp,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // MSG91 expects mobile without '+' prefix
                var mobile = phoneNumber.StartsWith("+")
                    ? phoneNumber[1..]
                    : phoneNumber;

                var url = BuildUrl(mobile, otp);
                var client = _httpClientFactory.CreateClient(HttpClientName);

                var response = await client.GetAsync(url, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "MSG91 HTTP {StatusCode} for {Phone}: {Body}",
                        (int)response.StatusCode, MaskPhone(mobile), responseBody);

                    return new OtpSendResult
                    {
                        Success = false,
                        ErrorMessage = $"MSG91 HTTP {(int)response.StatusCode}: {responseBody}",
                        ProviderName = ProviderName
                    };
                }

                var result = ParseMsg91Response(responseBody);

                if (!result.Success)
                {
                    _logger.LogError(
                        "MSG91 API error for {Phone}: {Error}",
                        MaskPhone(mobile), result.ErrorMessage);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Msg91OtpProvider.SendOtpAsync failed");
                return new OtpSendResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ProviderName = ProviderName
                };
            }
        }

        // ─── Helpers ─────────────────────────────────────────────────────────

        private string BuildUrl(string mobile, string otp)
        {
            var query = new System.Collections.Specialized.NameValueCollection
            {
                ["authkey"] = _authKey,
                ["mobile"] = mobile,
                ["otp"] = otp,
                ["template_id"] = _templateId,
                ["sender"] = _senderId,
                ["otp_expiry"] = "10"  // minutes
            };

            // Build query string without exposing auth key in logs
            var sb = new System.Text.StringBuilder(Msg91BaseUrl);
            sb.Append('?');
            foreach (string key in query)
            {
                sb.Append($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(query[key] ?? string.Empty)}&");
            }

            return sb.ToString().TrimEnd('&');
        }

        private OtpSendResult ParseMsg91Response(string responseBody)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                // MSG91 v5 success: { "type": "success", "message": "3.1" }
                // MSG91 v5 error:   { "type": "error", "message": "..." }
                var type = root.TryGetProperty("type", out var typeProp)
                    ? typeProp.GetString()
                    : null;

                var message = root.TryGetProperty("message", out var msgProp)
                    ? msgProp.GetString()
                    : null;

                var isSuccess = "success".Equals(type, StringComparison.OrdinalIgnoreCase);

                return new OtpSendResult
                {
                    Success = isSuccess,
                    MessageId = isSuccess ? message : null,
                    ErrorMessage = isSuccess ? null : message,
                    ProviderName = ProviderName
                };
            }
            catch
            {
                // Non-JSON response
                return new OtpSendResult
                {
                    Success = false,
                    ErrorMessage = $"Unexpected response: {responseBody}",
                    ProviderName = ProviderName
                };
            }
        }

        private static string MaskPhone(string phone)
        {
            if (phone.Length <= 4) return "****";
            return phone[..^4] + "****";
        }
    }
}
