using CateringEcommerce.Domain.Interfaces.Common;
using Twilio;
using Twilio.Rest.Verify.V2.Service;

namespace CateringEcommerce.BAL.Configuration
    {
    /// <summary>
    /// Service for sending and verifying OTPs via SMS using Twilio Verify API.
    /// Handles phone number validation and formatting for Indian phone numbers (+91).
    /// </summary>
    public class SmsService : ISmsService
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _verifyServiceSid;

        /// <summary>
        /// Initializes the SMS service with Twilio credentials from configuration.
        /// </summary>
        /// <param name="config">Application configuration containing Twilio settings</param>
        /// <exception cref="InvalidOperationException">Thrown when required configuration is missing</exception>
        public SmsService(IConfiguration config)
        {
            _accountSid = config["Twilio:AccountSID"] 
                ?? throw new InvalidOperationException("Configuration 'Twilio:AccountSid' is required");
            _authToken = config["Twilio:AuthToken"] 
                ?? throw new InvalidOperationException("Configuration 'Twilio:AuthToken' is required");
            _verifyServiceSid = config["Twilio:VerifyServiceSID"] 
                ?? throw new InvalidOperationException("Configuration 'Twilio:VerifyServiceSid' is required");

            TwilioClient.Init(_accountSid, _authToken);
        }

        /// <summary>
        /// Sends an OTP to the specified phone number via SMS.
        /// </summary>
        /// <param name="phoneNumber">Phone number without country code (e.g., "9876543210" for India)</param>
        /// <exception cref="ArgumentException">Thrown when phone number format is invalid</exception>
        /// <exception cref="InvalidOperationException">Thrown when SMS sending fails</exception>
        /// <remarks>
        /// Phone number should be 10 digits for Indian numbers.
        /// Automatically prepends +91 country code.
        /// </remarks>
        public void SendOtp(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentException(
                    "Phone number cannot be null or empty.", 
                    nameof(phoneNumber));
            }

            var formattedNumber = FormatPhoneNumber(phoneNumber);

            try
            {
                VerificationResource.Create(
                    to: formattedNumber,
                    channel: "sms",
                    pathServiceSid: _verifyServiceSid
                );
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to send OTP to {formattedNumber}", ex);
            }
        }

        /// <summary>
        /// Verifies the OTP code sent to the phone number.
        /// </summary>
        /// <param name="phoneNumber">Phone number without country code (e.g., "9876543210" for India)</param>
        /// <param name="code">OTP code to verify (typically 6 digits)</param>
        /// <returns>True if OTP is valid and approved, false otherwise</returns>
        /// <exception cref="ArgumentException">Thrown when inputs are invalid</exception>
        /// <remarks>
        /// Returns false instead of throwing on verification failure.
        /// Phone number should be 10 digits for Indian numbers.
        /// </remarks>
        public bool VerifyOtp(string phoneNumber, string code)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentException(
                    "Phone number cannot be null or empty.", 
                    nameof(phoneNumber));
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException(
                    "OTP code cannot be null or empty.", 
                    nameof(code));
            }

            var formattedNumber = FormatPhoneNumber(phoneNumber);
            var cleanCode = code.Trim();

            try
            {
                var result = VerificationCheckResource.Create(
                    to: formattedNumber,
                    code: cleanCode,
                    pathServiceSid: _verifyServiceSid
                );

                return result?.Status == "approved";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"OTP verification failed for {formattedNumber}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Formats phone number with +91 country code for India.
        /// </summary>
        /// <param name="phoneNumber">Raw phone number input</param>
        /// <returns>Formatted phone number with +91 prefix</returns>
        /// <exception cref="ArgumentException">Thrown when phone number is invalid</exception>
        private string FormatPhoneNumber(string phoneNumber)
        {
            var cleanNumber = phoneNumber
                .Trim()
                .Replace(" ", string.Empty)
                .Replace("-", string.Empty)
                .Replace("(", string.Empty)
                .Replace(")", string.Empty);

            // Remove leading +91 if already present
            if (cleanNumber.StartsWith("+91"))
                cleanNumber = cleanNumber.Substring(3);
            else if (cleanNumber.StartsWith("91"))
                cleanNumber = cleanNumber.Substring(2);

            if (cleanNumber.Length != 10 || !cleanNumber.All(char.IsDigit))
            {
                throw new ArgumentException(
                    $"Phone number must be 10 digits. Received: '{phoneNumber}'",
                    nameof(phoneNumber));
            }

            return $"+91{cleanNumber}";
        }
    }
}
