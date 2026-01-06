using CateringEcommerce.Domain.Models.Common;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Mail;
using Twilio.Rest.Iam.V1;

namespace CateringEcommerce.BAL.Configuration
{
    public class EmailService
    {
        private readonly EmailSettings _emailSettings;
        private static readonly ConcurrentDictionary<string, OtpEntry> _otpStore = new();

        private const int OtpExpirySeconds = 300; // 5 minutes
        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendOtpAsync(string toEmail, string otp)
        {
            var email = new MailMessage();
            email.From = new MailAddress(_emailSettings.FromEmail);
            email.To.Add(new MailAddress(toEmail));
            email.Subject = "Your OTP Code";
            email.Body = $"Your OTP code is: {otp}";
            email.IsBodyHtml = false;

            using var smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.EnableSsl = true; 
            smtp.Credentials = new NetworkCredential(_emailSettings.FromEmail, _emailSettings.AppPassword);

            await Task.Run(() => smtp.Send(email)); // Use Task.Run to simulate async behavior
        }

        public void StoreOtp(string email, string otp)
        {
            var entry = new OtpEntry
            {
                Otp = otp,
                ExpiryTime = DateTime.UtcNow.AddSeconds(OtpExpirySeconds)
            };

            _otpStore[email] = entry;
        }

        public bool VerifyOtp(string email, string otp)
        {
            if (_otpStore.TryGetValue(email, out var entry))
            {
                if (entry.ExpiryTime >= DateTime.UtcNow && entry.Otp == otp)
                {
                    _otpStore.TryRemove(email, out _); // OTP used once
                    return true;
                }
            }

            return false;
        }
    }

}
