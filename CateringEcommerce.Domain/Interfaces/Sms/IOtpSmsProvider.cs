namespace CateringEcommerce.Domain.Interfaces.Sms
{
    /// <summary>
    /// Abstraction for SMS OTP delivery providers.
    /// Only used for authentication OTP flows (User, Admin, Partner, Supervisor, Password Reset).
    /// OTP lifecycle (generation, hashing, expiry, rate-limiting) is the caller's responsibility.
    /// </summary>
    public interface IOtpSmsProvider
    {
        string ProviderName { get; }

        /// <summary>
        /// Delivers a pre-generated OTP to the given phone number via SMS.
        /// </summary>
        /// <param name="phoneNumber">E.164 format: +919876543210</param>
        /// <param name="otp">Plain-text OTP to include in message</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<OtpSendResult> SendOtpAsync(string phoneNumber, string otp, CancellationToken cancellationToken = default);
    }
}
