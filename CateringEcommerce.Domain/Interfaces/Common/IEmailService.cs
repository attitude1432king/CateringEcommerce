using System.Threading.Tasks;

namespace CateringEcommerce.Domain.Interfaces.Common
{
    public interface IEmailService
    {
        /// <summary>
        /// Sends an OTP (One-Time Password) to the specified email address
        /// </summary>
        Task SendOtpAsync(string toEmail, string otp);

        /// <summary>
        /// Stores an OTP in memory for later verification
        /// </summary>
        void StoreOtp(string email, string otp);

        /// <summary>
        /// Verifies an OTP against the stored value
        /// </summary>
        bool VerifyOtp(string email, string otp);

        /// <summary>
        /// Sends a general-purpose email
        /// </summary>
        Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);

        /// <summary>
        /// Sends an email with a binary attachment (e.g., invoice PDF)
        /// </summary>
        Task SendEmailWithAttachmentAsync(
            string toEmail,
            string subject,
            string body,
            byte[] attachmentBytes,
            string attachmentFileName,
            bool isHtml = true);
    }
}
