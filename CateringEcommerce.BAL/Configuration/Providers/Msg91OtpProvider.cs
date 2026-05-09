using CateringEcommerce.Domain.Interfaces.Sms;
using CateringEcommerce.Domain.Models.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

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

        private readonly string _authKey;
        private readonly string _senderId;
        private readonly string _templateId;
        // Actual MSG91 OTP v5 API endpoint (NOT the docs URL)
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

            if (string.IsNullOrEmpty(_authKey))
                throw new InvalidOperationException("Configuration 'MSG91:AUTH_KEY' is required for Msg91OtpProvider.");
            if (string.IsNullOrEmpty(_templateId))
                throw new InvalidOperationException("Configuration 'MSG91:TEMPLATE_ID' is required for Msg91OtpProvider.");
        }

        public async Task<OtpSendResult> SendOtpAsync(
            string phoneNumber,
            string otp,
            CancellationToken cancellationToken = default)
        {
            // MSG91 expects mobile without '+' prefix (e.g. 919876543210)
            var mobile = phoneNumber.StartsWith('+') ? phoneNumber[1..] : phoneNumber;
            var maskedMobile = MaskPhone(mobile);

            try
            {
                var client = _httpClientFactory.CreateClient(HttpClientName);

                var payload = new
                {
                    mobile,
                    otp,
                    template_id = _templateId,
                    sender = _senderId,
                    otp_expiry = 10
                };

                var json = JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var request = new HttpRequestMessage(HttpMethod.Post, Msg91BaseUrl);

                // MSG91 v5: authkey goes in the request header, not the URL
                request.Headers.TryAddWithoutValidation("authkey", _authKey);
                request.Headers.TryAddWithoutValidation("Accept", "application/json");
                request.Content = content;

                _logger.LogInformation(
                    "MSG91 OTP POST → {Url} for {Phone} (templateId={TemplateId})",
                    Msg91BaseUrl, maskedMobile, _templateId);

                var response = await client.SendAsync(request, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                _logger.LogInformation(
                    "MSG91 response HTTP {StatusCode} for {Phone}: {Body}",
                    (int)response.StatusCode, maskedMobile,
                    responseBody.Length > 500 ? responseBody[..500] : responseBody);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "MSG91 HTTP {StatusCode} for {Phone}: {Body}",
                        (int)response.StatusCode, maskedMobile, responseBody);

                    return new OtpSendResult
                    {
                        Success = false,
                        ErrorMessage = $"MSG91 HTTP {(int)response.StatusCode}",
                        ProviderName = ProviderName
                    };
                }

                return ParseMsg91Response(responseBody, maskedMobile);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError("MSG91 API timed out for {Phone}", maskedMobile);
                return new OtpSendResult
                {
                    Success = false,
                    ErrorMessage = "MSG91 request timed out",
                    ProviderName = ProviderName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Msg91OtpProvider.SendOtpAsync failed for {Phone}", maskedMobile);
                return new OtpSendResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ProviderName = ProviderName
                };
            }
        }

        // ─── Helpers ─────────────────────────────────────────────────────────

        private OtpSendResult ParseMsg91Response(string responseBody, string maskedPhone)
        {
            if (string.IsNullOrWhiteSpace(responseBody))
            {
                _logger.LogError("MSG91 returned empty response for {Phone}", maskedPhone);
                return new OtpSendResult
                {
                    Success = false,
                    ErrorMessage = "Empty response from MSG91",
                    ProviderName = ProviderName
                };
            }

            // Detect HTML error page — indicates wrong endpoint URL or server-side redirect
            if (responseBody.TrimStart().StartsWith("<", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError(
                    "MSG91 returned HTML (not JSON) for {Phone}. Wrong endpoint URL or redirect. Preview: {Preview}",
                    maskedPhone,
                    responseBody.Length > 300 ? responseBody[..300] : responseBody);

                return new OtpSendResult
                {
                    Success = false,
                    ErrorMessage = "MSG91 returned HTML instead of JSON. Check API endpoint configuration.",
                    ProviderName = ProviderName
                };
            }

            try
            {
                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                // MSG91 v5 success: { "type": "success", "message": "3.1" }
                // MSG91 v5 error:   { "type": "error",   "message": "..." }
                var type = root.TryGetProperty("type", out var typeProp)
                    ? typeProp.GetString()
                    : null;

                var message = root.TryGetProperty("message", out var msgProp)
                    ? msgProp.GetString()
                    : null;

                var isSuccess = "success".Equals(type, StringComparison.OrdinalIgnoreCase);

                if (!isSuccess)
                {
                    _logger.LogError(
                        "MSG91 API error for {Phone}: type={Type}, message={Msg}",
                        maskedPhone, type, message);
                }

                return new OtpSendResult
                {
                    Success = isSuccess,
                    MessageId = isSuccess ? message : null,
                    ErrorMessage = isSuccess ? null : (message ?? $"Unknown error, type: {type}"),
                    ProviderName = ProviderName
                };
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    "MSG91 JSON parse failed for {Phone}. Raw: {Body}. Error: {Err}",
                    maskedPhone,
                    responseBody.Length > 300 ? responseBody[..300] : responseBody,
                    ex.Message);

                return new OtpSendResult
                {
                    Success = false,
                    ErrorMessage = "MSG91 returned unparseable response",
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
