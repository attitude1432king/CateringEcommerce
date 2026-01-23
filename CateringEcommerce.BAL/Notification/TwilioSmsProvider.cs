using CateringEcommerce.Domain.Interfaces.Notification;
using CateringEcommerce.Domain.Models.Notification;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using System.Text.RegularExpressions;
using Twilio;
using Twilio.Clients;
using Twilio.Rest.Api.V2010;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace CateringEcommerce.BAL.Notification
{
    public class TwilioSmsProvider : ISmsProvider
    {
        private readonly ITwilioRestClient _client;
        private readonly ILogger<TwilioSmsProvider> _logger;
        private readonly TwilioSettings _settings;
        private readonly AsyncCircuitBreakerPolicy _circuitBreaker;

        public string ProviderName => "Twilio";
        public int Priority => 1;
        public bool IsAvailable => _circuitBreaker.CircuitState != CircuitState.Open;

        public TwilioSmsProvider(
            IOptions<TwilioSettings> settings,
            ILogger<TwilioSmsProvider> logger)
        {
            _settings = settings.Value;
            TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);
            _client = TwilioClient.GetRestClient();
            _logger = logger;

            _circuitBreaker = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(3, TimeSpan.FromSeconds(60));
        }

        public async Task<SmsResult> SendAsync(SmsMessage message, CancellationToken cancellationToken)
        {
            try
            {
                // Validate phone number
                if (!IsValidPhoneNumber(message.To))
                {
                    return new SmsResult
                    {
                        Success = false,
                        ErrorMessage = "Invalid phone number format",
                        ProviderName = ProviderName
                    };
                }

                // Validate message length
                if (message.Message.Length > 1600) // Twilio limit
                {
                    return new SmsResult
                    {
                        Success = false,
                        ErrorMessage = "Message exceeds maximum length",
                        ProviderName = ProviderName
                    };
                }

                var twilioMessage = await _circuitBreaker.ExecuteAsync(async () =>
                    await MessageResource.CreateAsync(
                        to: new PhoneNumber(message.To),
                        from: new PhoneNumber(_settings.FromNumber),
                        body: message.Message,
                        validityPeriod: message.ValidityPeriod > 0
                            ? message.ValidityPeriod
                            : (int?)null,
                        client: _client
                    ));

                _logger.LogInformation(
                    "SMS sent via Twilio. MessageSid: {MessageSid}, Status: {Status}",
                    twilioMessage.Sid, twilioMessage.Status);

                return new SmsResult
                {
                    Success = twilioMessage.ErrorCode == null,
                    MessageId = Guid.NewGuid().ToString(),
                    ProviderMessageId = twilioMessage.Sid,
                    SentAt = DateTime.UtcNow,
                    ProviderName = ProviderName,
                    ErrorMessage = twilioMessage.ErrorMessage,
                    Cost = twilioMessage.Price != null ? Math.Abs(decimal.Parse(twilioMessage.Price)) : 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS via Twilio");
                return new SmsResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ProviderName = ProviderName
                };
            }
        }

        public async Task<bool> HealthCheckAsync(CancellationToken cancellationToken)
        {
            try
            {
                await AccountResource.FetchAsync(client: _client);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            // E.164 format validation
            return Regex.IsMatch(phoneNumber, @"^\+[1-9]\d{1,14}$");
        }
    }
}
