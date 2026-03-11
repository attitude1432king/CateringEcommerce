using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Interfaces.Sms;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace CateringEcommerce.BAL.Configuration
{
    /// <summary>
    /// Unified OTP service for all roles (User, Owner, Admin, Supervisor).
    /// Owns the full OTP lifecycle: generate → hash → cache → deliver → verify.
    /// SMS delivery is delegated to the configured ISmsOtpProvider.
    /// </summary>
    public class SmsService : ISmsService
    {
        private readonly ISmsOtpProvider _provider;
        private readonly IDistributedCache _cache;
        private readonly ISystemSettingsProvider _settings;
        private readonly ILogger<SmsService> _logger;

        // Cache key prefixes
        private const string OtpEntryPrefix = "otp:entry:";
        private const string OtpSendCountPrefix = "otp:sends:";

        public SmsService(
            ISmsOtpProvider provider,
            IDistributedCache cache,
            ISystemSettingsProvider settings,
            ILogger<SmsService> logger)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public void SendOtp(string phoneNumber)
            => SendOtpAsync(phoneNumber).GetAwaiter().GetResult();

        /// <inheritdoc />
        public bool VerifyOtp(string phoneNumber, string code)
            => VerifyOtpAsync(phoneNumber, code).GetAwaiter().GetResult();

        /// <summary>
        /// Send a generic SMS message
        /// </summary>
        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            var formatted = FormatPhoneNumber(phoneNumber);

            _logger.LogInformation(
                "Sending SMS via {Provider} to {Phone}",
                _provider.ProviderName, MaskPhone(formatted));

            var result = await _provider.SendOtpAsync(formatted, message);

            if (!result.Success)
            {
                _logger.LogError(
                    "SMS delivery failed via {Provider} for {Phone}: {Error}",
                    _provider.ProviderName, MaskPhone(formatted), result.ErrorMessage);

                throw new InvalidOperationException(
                    $"Failed to send SMS via {_provider.ProviderName}: {result.ErrorMessage}");
            }

            _logger.LogInformation(
                "SMS delivered via {Provider} to {Phone}. MessageId: {Id}",
                _provider.ProviderName, MaskPhone(formatted), result.MessageId ?? "n/a");
        }

        // ─── Private async implementations ──────────────────────────────────

        private async Task SendOtpAsync(string phoneNumber)
        {
            var formatted = FormatPhoneNumber(phoneNumber);
            await EnforceRateLimitAsync(formatted);

            var otp = GenerateOtp();
            await StoreOtpAsync(formatted, otp);

            var appName = _settings.GetString("APP.NAME", "Enyvora");
            var expiry = _settings.GetInt("OTP.EXPIRY_MINUTES", 10);
            var template = _settings.GetString(
                "OTP.MESSAGE_TEMPLATE",
                $"Your {{AppName}} OTP is {{OTP}}. Valid for {{EXPIRY}} minutes. Do not share this code.");

            var message = template
                .Replace("{AppName}", appName)
                .Replace("{OTP}", otp)
                .Replace("{EXPIRY}", expiry.ToString());

            // OTP is NEVER logged — only masked placeholder
            _logger.LogInformation(
                "Sending OTP via {Provider} to {Phone}",
                _provider.ProviderName, MaskPhone(formatted));

            var result = await _provider.SendOtpAsync(formatted, otp);

            if (!result.Success)
            {
                _logger.LogError(
                    "OTP delivery failed via {Provider} for {Phone}: {Error}",
                    _provider.ProviderName, MaskPhone(formatted), result.ErrorMessage);

                throw new InvalidOperationException(
                    $"Failed to send OTP via {_provider.ProviderName}: {result.ErrorMessage}");
            }

            _logger.LogInformation(
                "OTP delivered via {Provider} to {Phone}. MessageId: {Id}",
                _provider.ProviderName, MaskPhone(formatted), result.MessageId ?? "n/a");
        }

        private async Task<bool> VerifyOtpAsync(string phoneNumber, string code)
        {
            var bypass = _settings.GetBool("OTP.BYPASS_VERIFICATION", false);
            if (bypass)
            {
                _logger.LogWarning("OTP verification bypassed by configuration for {Phone}", MaskPhone(phoneNumber));
                return true;
            }

            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(code))
                return false;

            var formatted = FormatPhoneNumber(phoneNumber);
            var entryKey = OtpEntryPrefix + formatted;
            var rawEntry = await _cache.GetStringAsync(entryKey);

            if (rawEntry == null)
            {
                _logger.LogWarning("OTP entry not found or expired for {Phone}", MaskPhone(formatted));
                return false;
            }

            // Entry format: "{hash}|{attempts}|{expiry_unix}"
            var parts = rawEntry.Split('|');
            if (parts.Length != 3)
            {
                await _cache.RemoveAsync(entryKey);
                return false;
            }

            var storedHash = parts[0];
            var attempts = int.TryParse(parts[1], out var a) ? a : 0;
            var expiryUnix = long.TryParse(parts[2], out var e) ? e : 0L;
            var maxAttempts = _settings.GetInt("OTP.MAX_VERIFY_ATTEMPTS", 5);

            // Check expiry
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiryUnix)
            {
                await _cache.RemoveAsync(entryKey);
                _logger.LogWarning("OTP expired for {Phone}", MaskPhone(formatted));
                return false;
            }

            // Check max attempts lockout
            if (attempts >= maxAttempts)
            {
                _logger.LogWarning(
                    "OTP max attempts ({Max}) reached for {Phone}",
                    maxAttempts, MaskPhone(formatted));
                return false;
            }

            var incomingHash = HashOtp(code.Trim());

            if (!CryptographicEquals(storedHash, incomingHash))
            {
                // Increment attempt counter
                var updated = $"{storedHash}|{attempts + 1}|{expiryUnix}";
                var remaining = expiryUnix - DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (remaining > 0)
                {
                    await _cache.SetStringAsync(entryKey, updated,
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(remaining)
                        });
                }
                return false;
            }

            // ✔ Valid — remove entry immediately (replay prevention)
            await _cache.RemoveAsync(entryKey);
            _logger.LogInformation("OTP verified successfully for {Phone}", MaskPhone(formatted));
            return true;
        }

        // ─── OTP generation ──────────────────────────────────────────────────

        private string GenerateOtp()
        {
            var length = _settings.GetInt("OTP.LENGTH", 6);
            var bytes = new byte[4];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            var value = Math.Abs(BitConverter.ToInt32(bytes, 0)) % (int)Math.Pow(10, length);
            return value.ToString().PadLeft(length, '0');
        }

        // ─── OTP storage ─────────────────────────────────────────────────────

        private async Task StoreOtpAsync(string phoneNumber, string otp)
        {
            var expiryMinutes = _settings.GetInt("OTP.EXPIRY_MINUTES", 10);
            var expiryUnix = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes).ToUnixTimeSeconds();
            var hash = HashOtp(otp);

            // Format: "{hash}|{attempts}|{expiry_unix}"
            var entry = $"{hash}|0|{expiryUnix}";
            var entryKey = OtpEntryPrefix + phoneNumber;

            await _cache.SetStringAsync(entryKey, entry,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expiryMinutes)
                });
        }

        // ─── Rate limiting ───────────────────────────────────────────────────

        private async Task EnforceRateLimitAsync(string phoneNumber)
        {
            var maxSends = _settings.GetInt("SECURITY.OTP_SEND_PERMITS", 3);
            var windowMinutes = _settings.GetInt("SECURITY.OTP_SEND_WINDOW_MINUTES", 60);
            var hourBucket = DateTime.UtcNow.ToString("yyyyMMddHH");
            var rateLimitKey = $"{OtpSendCountPrefix}{phoneNumber}:{hourBucket}";

            var countStr = await _cache.GetStringAsync(rateLimitKey);
            var count = int.TryParse(countStr, out var c) ? c : 0;

            if (count >= maxSends)
            {
                _logger.LogWarning(
                    "OTP send rate limit ({Max}/{Window}min) exceeded for {Phone}",
                    maxSends, windowMinutes, MaskPhone(phoneNumber));

                throw new InvalidOperationException(
                    $"Too many OTP requests. Please try again after some time.");
            }

            await _cache.SetStringAsync(rateLimitKey, (count + 1).ToString(),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(windowMinutes)
                });
        }

        // ─── Helpers ─────────────────────────────────────────────────────────

        private static string HashOtp(string otp)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(otp));
            return Convert.ToHexString(bytes);
        }

        /// <summary>Constant-time string comparison to prevent timing attacks.</summary>
        private static bool CryptographicEquals(string a, string b)
        {
            if (a.Length != b.Length) return false;
            var aBytes = Encoding.UTF8.GetBytes(a);
            var bBytes = Encoding.UTF8.GetBytes(b);
            return CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
        }

        private static string FormatPhoneNumber(string phoneNumber)
        {
            var clean = phoneNumber.Trim()
                .Replace(" ", string.Empty)
                .Replace("-", string.Empty)
                .Replace("(", string.Empty)
                .Replace(")", string.Empty);

            if (clean.StartsWith("+91")) clean = clean[3..];
            else if (clean.StartsWith("91") && clean.Length == 12) clean = clean[2..];

            if (clean.Length != 10 || !clean.All(char.IsDigit))
                throw new ArgumentException($"Phone number must be 10 digits.", nameof(phoneNumber));

            return $"+91{clean}";
        }

        private static string MaskPhone(string phone)
        {
            if (phone.Length <= 4) return "****";
            return phone[..^4] + "****";
        }
    }
}
