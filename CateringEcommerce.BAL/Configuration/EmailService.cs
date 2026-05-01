using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Models.Configuration;
using CateringEcommerce.Domain.Models.Common;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace CateringEcommerce.BAL.Configuration
{
    public class EmailService : IEmailService
    {
        private readonly ISystemSettingsProvider _settings;
        private readonly SmtpSettings _smtpSettings;
        private readonly IDistributedCache _cache;
        private const string OtpCacheKeyPrefix = "email_otp:";

        public EmailService(ISystemSettingsProvider settings, IOptions<SmtpSettings> smtpOptions, IDistributedCache cache)
        {
            _settings = settings;
            _smtpSettings = smtpOptions?.Value ?? throw new ArgumentNullException(nameof(smtpOptions));
            _cache = cache;
        }

        public async Task SendOtpAsync(string toEmail, string otp)
        {
            var fromEmail = ResolveFromEmail();
            var smtpHost = _smtpSettings.Host;
            var smtpPort = _smtpSettings.Port;
            var smtpUsername = _smtpSettings.Username;
            var smtpPassword = _smtpSettings.Password;
            var enableSsl = _smtpSettings.EnableSsl;

            var email = new MailMessage();
            email.From = new MailAddress(fromEmail);
            email.To.Add(new MailAddress(toEmail));
            email.Subject = "Your OTP Code";
            email.Body = $"Your OTP code is: {otp}";
            email.IsBodyHtml = false;

            using var smtp = new SmtpClient(smtpHost, smtpPort);
            smtp.EnableSsl = enableSsl;
            smtp.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

            await Task.Run(() => smtp.Send(email));
        }

        public void StoreOtp(string email, string otp)
        {
            var otpExpirySeconds = _settings.GetInt("SYSTEM.OTP_EXPIRY_SECONDS", 300);
            var entry = new OtpEntry
            {
                Otp = otp,
                ExpiryTime = DateTime.UtcNow.AddSeconds(otpExpirySeconds)
            };

            // SECURITY FIX: Use distributed cache instead of static in-memory dictionary
            // This allows multi-instance deployments to share OTP state
            var cacheKey = OtpCacheKeyPrefix + email.ToLowerInvariant();
            var serialized = JsonSerializer.Serialize(entry);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(otpExpirySeconds)
            };

            _cache.SetString(cacheKey, serialized, cacheOptions);
        }

        public bool VerifyOtp(string email, string otp)
        {
            var cacheKey = OtpCacheKeyPrefix + email.ToLowerInvariant();
            var serialized = _cache.GetString(cacheKey);

            if (!string.IsNullOrEmpty(serialized))
            {
                var entry = JsonSerializer.Deserialize<OtpEntry>(serialized);
                if (entry != null && entry.ExpiryTime >= DateTime.UtcNow && entry.Otp == otp)
                {
                    // Remove OTP after successful verification (one-time use)
                    _cache.Remove(cacheKey);
                    return true;
                }
            }

            return false;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            var fromEmail = ResolveFromEmail();
            var smtpHost = _smtpSettings.Host;
            var smtpPort = _smtpSettings.Port;
            var smtpUsername = _smtpSettings.Username;
            var smtpPassword = _smtpSettings.Password;
            var enableSsl = _smtpSettings.EnableSsl;

            var email = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
            email.To.Add(new MailAddress(toEmail));

            using var smtp = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            await Task.Run(() => smtp.Send(email));
        }

        public async Task SendEmailWithAttachmentAsync(
            string toEmail,
            string subject,
            string body,
            byte[] attachmentBytes,
            string attachmentFileName,
            bool isHtml = true)
        {
            var fromEmail = ResolveFromEmail();
            var smtpHost = _smtpSettings.Host;
            var smtpPort = _smtpSettings.Port;
            var smtpUsername = _smtpSettings.Username;
            var smtpPassword = _smtpSettings.Password;
            var enableSsl = _smtpSettings.EnableSsl;

            var email = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
            email.To.Add(new MailAddress(toEmail));

            if (attachmentBytes != null && attachmentBytes.Length > 0)
            {
                var attachment = new Attachment(new MemoryStream(attachmentBytes), attachmentFileName);
                email.Attachments.Add(attachment);
            }

            using var smtp = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            await Task.Run(() => smtp.Send(email));
        }

        private string ResolveFromEmail()
        {
            return string.IsNullOrWhiteSpace(_smtpSettings.FromAddress)
                ? _smtpSettings.Username
                : _smtpSettings.FromAddress;
        }
    }

}
