using Amazon;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using CateringEcommerce.Domain.Interfaces.Notification;
using CateringEcommerce.Domain.Models.Notification;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace CateringEcommerce.BAL.Notification
{
    /// <summary>
    /// AWS SNS SMS provider for all order and system notifications.
    /// NOT used for OTP — OTP is handled exclusively by Msg91OtpProvider.
    /// </summary>
    public class AwsSnsNotificationProvider : INotificationSmsProvider, IDisposable
    {
        private readonly AwsSnsSettings _settings;
        private readonly ILogger<AwsSnsNotificationProvider> _logger;
        private readonly AmazonSimpleNotificationServiceClient _snsClient;
        private readonly AsyncRetryPolicy _retryPolicy;

        public string ProviderName => "AWSSNS";
        public int Priority => 1;
        public bool IsAvailable =>
            !string.IsNullOrEmpty(_settings.AccessKey) &&
            !string.IsNullOrEmpty(_settings.SecretKey);

        public AwsSnsNotificationProvider(
            IOptions<AwsSnsSettings> options,
            ILogger<AwsSnsNotificationProvider> logger)
        {
            _settings = options.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var credentials = new BasicAWSCredentials(_settings.AccessKey, _settings.SecretKey);
            var region = RegionEndpoint.GetBySystemName(_settings.Region);
            _snsClient = new AmazonSimpleNotificationServiceClient(credentials, region);

            // Retry: 3 attempts, exponential backoff (2s, 4s, 8s)
            _retryPolicy = Policy
                .Handle<AmazonSimpleNotificationServiceException>()
                .Or<HttpRequestException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (ex, delay, attempt, _) =>
                    {
                        _logger.LogWarning(
                            "AWS SNS retry {Attempt}/3 after {Delay}s: {Error}",
                            attempt, delay.TotalSeconds, ex.Message);
                    });
        }

        public async Task<SmsResult> SendAsync(SmsMessage message, CancellationToken cancellationToken = default)
        {
            if (!IsAvailable)
            {
                return new SmsResult
                {
                    Success = false,
                    ErrorMessage = "AWS SNS credentials not configured",
                    ProviderName = ProviderName
                };
            }

            try
            {
                var result = await _retryPolicy.ExecuteAsync(async () =>
                {
                    var request = new PublishRequest
                    {
                        PhoneNumber = message.To,
                        Message = message.Message,
                        MessageAttributes = new Dictionary<string, MessageAttributeValue>
                        {
                            ["AWS.SNS.SMS.SMSType"] = new MessageAttributeValue
                            {
                                DataType = "String",
                                StringValue = "Transactional"
                            },
                            ["AWS.SNS.SMS.SenderID"] = new MessageAttributeValue
                            {
                                DataType = "String",
                                StringValue = string.IsNullOrEmpty(message.From)
                                    ? _settings.SenderId
                                    : message.From
                            }
                        }
                    };

                    return await _snsClient.PublishAsync(request, cancellationToken);
                });

                _logger.LogInformation(
                    "AWS SNS SMS delivered to {Phone}. MessageId: {Id}",
                    MaskPhone(message.To), result.MessageId);

                return new SmsResult
                {
                    Success = true,
                    MessageId = result.MessageId,
                    ProviderMessageId = result.MessageId,
                    ProviderName = ProviderName,
                    SentAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "AWS SNS failed to deliver SMS to {Phone}",
                    MaskPhone(message.To));

                return new SmsResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ProviderName = ProviderName,
                    SentAt = DateTime.UtcNow
                };
            }
        }

        public async Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default)
        {
            if (!IsAvailable) return false;
            try
            {
                // GetSMSAttributes is a lightweight call that verifies credentials
                await _snsClient.GetSMSAttributesAsync(
                    new GetSMSAttributesRequest(),
                    cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("AWS SNS health check failed: {Error}", ex.Message);
                return false;
            }
        }

        public void Dispose() => _snsClient?.Dispose();

        private static string MaskPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone) || phone.Length <= 4) return "****";
            return phone[..^4] + "****";
        }
    }
}
