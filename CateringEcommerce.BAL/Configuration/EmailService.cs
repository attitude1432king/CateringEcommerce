using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Models.Common;
using Microsoft.Extensions.Caching.Distributed;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace CateringEcommerce.BAL.Configuration
{
    public class EmailService : IEmailService
    {
        private readonly ISystemSettingsProvider _settings;
        private readonly IDistributedCache _cache;
        private const string OtpCacheKeyPrefix = "email_otp:";

        public EmailService(ISystemSettingsProvider settings, IDistributedCache cache)
        {
            _settings = settings;
            _cache = cache;
        }

        public async Task SendOtpAsync(string toEmail, string otp)
        {
            var fromEmail = _settings.GetString("EMAIL.FROM_ADDRESS", _settings.GetString("EMAIL.SMTP_USERNAME"));
            var smtpHost = _settings.GetString("EMAIL.SMTP_HOST", "smtp.gmail.com");
            var smtpPort = _settings.GetInt("EMAIL.SMTP_PORT", 587);
            var smtpUsername = _settings.GetString("EMAIL.SMTP_USERNAME");
            var smtpPassword = _settings.GetString("EMAIL.SMTP_PASSWORD");
            var enableSsl = _settings.GetBool("EMAIL.ENABLE_SSL", true);

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
            var fromEmail = _settings.GetString("EMAIL.FROM_ADDRESS", _settings.GetString("EMAIL.SMTP_USERNAME"));
            var smtpHost = _settings.GetString("EMAIL.SMTP_HOST", "smtp.gmail.com");
            var smtpPort = _settings.GetInt("EMAIL.SMTP_PORT", 587);
            var smtpUsername = _settings.GetString("EMAIL.SMTP_USERNAME");
            var smtpPassword = _settings.GetString("EMAIL.SMTP_PASSWORD");
            var enableSsl = _settings.GetBool("EMAIL.ENABLE_SSL", true);

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
            var fromEmail = _settings.GetString("EMAIL.FROM_ADDRESS", _settings.GetString("EMAIL.SMTP_USERNAME"));
            var smtpHost = _settings.GetString("EMAIL.SMTP_HOST", "smtp.gmail.com");
            var smtpPort = _settings.GetInt("EMAIL.SMTP_PORT", 587);
            var smtpUsername = _settings.GetString("EMAIL.SMTP_USERNAME");
            var smtpPassword = _settings.GetString("EMAIL.SMTP_PASSWORD");
            var enableSsl = _settings.GetBool("EMAIL.ENABLE_SSL", true);

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
    }

}
