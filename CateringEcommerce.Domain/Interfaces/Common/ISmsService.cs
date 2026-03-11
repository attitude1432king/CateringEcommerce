namespace CateringEcommerce.Domain.Interfaces.Common
{
    public interface ISmsService
    {
        void SendOtp(string phoneNumber);
        bool VerifyOtp(string phoneNumber, string code);

        /// <summary>
        /// Sends a generic SMS message (not an OTP) to the given phone number
        /// </summary>
        Task SendSmsAsync(string phoneNumber, string message);
    }
}
