using System.Security.Cryptography;
using System.Text;
using CateringEcommerce.Domain.Interfaces.Payment;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CateringEcommerce.BAL.Services
{
    public class RazorpaySignatureVerifier : IRazorpaySignatureVerifier
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RazorpaySignatureVerifier> _logger;

        public RazorpaySignatureVerifier(
            IConfiguration configuration,
            ILogger<RazorpaySignatureVerifier> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool VerifyWebhookSignature(string rawBody, string signature)
        {
            if (string.IsNullOrWhiteSpace(rawBody) || string.IsNullOrWhiteSpace(signature))
            {
                return false;
            }

            var secret = _configuration["RAZORPAY_WEBHOOK_SECRET"]
                ?? _configuration["PAYMENT:RAZORPAY_WEBHOOK_SECRET"];

            if (string.IsNullOrWhiteSpace(secret))
            {
                _logger.LogCritical("Razorpay webhook secret is not configured.");
                return false;
            }

            var expectedSignature = GenerateSignature(rawBody, secret);
            return FixedTimeEquals(expectedSignature, signature);
        }

        private static string GenerateSignature(string payload, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        private static bool FixedTimeEquals(string expected, string received)
        {
            var expectedBytes = Encoding.UTF8.GetBytes(expected.Trim());
            var receivedBytes = Encoding.UTF8.GetBytes(received.Trim().ToLowerInvariant());
            return expectedBytes.Length == receivedBytes.Length
                && CryptographicOperations.FixedTimeEquals(expectedBytes, receivedBytes);
        }
    }
}
