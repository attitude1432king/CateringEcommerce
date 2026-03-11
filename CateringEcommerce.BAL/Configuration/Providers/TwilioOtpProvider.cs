using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Sms;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace CateringEcommerce.BAL.Configuration.Providers
{
    /// <summary>
    /// SMS OTP delivery via Twilio Messaging API.
    /// OTP lifecycle (generation, hashing, verification) is handled by SmsService.
    /// This class only sends the pre-composed SMS message.
    /// </summary>
    public class TwilioOtpProvider : ISmsOtpProvider
    {
        private readonly string _fromNumber;
        private readonly ILogger<TwilioOtpProvider> _logger;

        public string ProviderName => "TWILIO";

        public TwilioOtpProvider(ISystemSettingsProvider settings, ILogger<TwilioOtpProvider> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var accountSid = settings.GetString("TWILIO.ACCOUNT_SID");
            var authToken = settings.GetString("TWILIO.AUTH_TOKEN");
            _fromNumber = settings.GetString("TWILIO.FROM_PHONE_NUMBER");

            if (string.IsNullOrEmpty(accountSid))
                throw new InvalidOperationException("Setting 'TWILIO.ACCOUNT_SID' is required for TwilioOtpProvider.");
            if (string.IsNullOrEmpty(authToken))
                throw new InvalidOperationException("Setting 'TWILIO.AUTH_TOKEN' is required for TwilioOtpProvider.");
            if (string.IsNullOrEmpty(_fromNumber))
                throw new InvalidOperationException("Setting 'TWILIO.FROM_PHONE_NUMBER' is required for TwilioOtpProvider.");

            TwilioClient.Init(accountSid, authToken);
        }

        public async Task<OtpSendResult> SendOtpAsync(
            string phoneNumber,
            string otp,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var message = await MessageResource.CreateAsync(
                    to: new PhoneNumber(phoneNumber),
                    from: new PhoneNumber(_fromNumber),
                    body: BuildMessage(otp));

                if (message.ErrorCode != null)
                {
                    _logger.LogError(
                        "Twilio returned error {Code}: {Message}",
                        message.ErrorCode, message.ErrorMessage);

                    return new OtpSendResult
                    {
                        Success = false,
                        ErrorMessage = $"Twilio error {message.ErrorCode}: {message.ErrorMessage}",
                        ProviderName = ProviderName
                    };
                }

                return new OtpSendResult
                {
                    Success = true,
                    MessageId = message.Sid,
                    ProviderName = ProviderName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TwilioOtpProvider.SendOtpAsync failed");
                return new OtpSendResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ProviderName = ProviderName
                };
            }
        }

        private static string BuildMessage(string otp)
            => $"[Enyvora] Your OTP is {otp}. Valid for 10 minutes. Do not share this code with anyone.";
    }
}
