using System;

namespace CateringEcommerce.Domain.Models.Security
{
    /// <summary>
    /// 2FA requirement result
    /// </summary>
    public class TwoFactorRequirement
    {
        public string UserType { get; set; }
        public long UserId { get; set; }
        public bool IsRequired { get; set; }
        public string Reason { get; set; }
        public string Context { get; set; } // e.g., "2FA_ADMIN_LOGIN", "2FA_PAYMENT", "TRUSTED_DEVICE_LOGIN"
        public string DeviceFingerprint { get; set; }
        public string IpAddress { get; set; }
        public long? TrustedDeviceId { get; set; }
    }

    /// <summary>
    /// Trusted device model
    /// </summary>
    public class TrustedDeviceModel
    {
        public long DeviceId { get; set; }
        public string UserType { get; set; } // USER, OWNER, ADMIN
        public long UserId { get; set; }
        public string DeviceToken { get; set; }
        public string DeviceName { get; set; } // e.g., "Chrome on Windows"
        public string DeviceFingerprint { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string Browser { get; set; }
        public string OS { get; set; }
        public bool IsActive { get; set; }
        public DateTime TrustedDate { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? LastUsed { get; set; }
        public DateTime? RevokedDate { get; set; }
        public string RevokedReason { get; set; }

        // Computed properties
        public bool IsExpired => ExpiresAt < DateTime.Now;
        public bool IsCurrentlyTrusted => IsActive && !IsExpired;
        public int DaysUntilExpiry => IsExpired ? 0 : (ExpiresAt - DateTime.Now).Days;
    }

    /// <summary>
    /// OTP purpose for user-friendly messaging
    /// </summary>
    public class OtpPurpose
    {
        public string Code { get; set; } // VERIFICATION, 2FA_LOGIN, 2FA_PAYMENT, etc.
        public string UserMessage { get; set; } // "Verify your account", "Verify to continue"
        public string Description { get; set; } // Detailed message for user
    }

    /// <summary>
    /// Device registration request
    /// </summary>
    public class RegisterDeviceRequest
    {
        public string DeviceFingerprint { get; set; }
        public string DeviceName { get; set; }
        public string Browser { get; set; }
        public string OS { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public bool TrustDevice { get; set; } // "Remember this device" checkbox
    }

    /// <summary>
    /// Send OTP request with device info
    /// </summary>
    public class SendOtpRequest
    {
        public string Action { get; set; } // login, signup, payment, order
        public string PhoneNumber { get; set; }
        public string DeviceFingerprint { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public bool IsPartnerLogin { get; set; }
    }

    /// <summary>
    /// Verify OTP request with device info
    /// </summary>
    public class VerifyOtpRequest
    {
        public string Action { get; set; }
        public string PhoneNumber { get; set; }
        public string Name { get; set; } // For signup
        public string Otp { get; set; }
        public string DeviceFingerprint { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public bool TrustDevice { get; set; } // "Remember this device" flag
        public bool IsPartnerLogin { get; set; }
    }

    /// <summary>
    /// OTP verification response
    /// </summary>
    public class OtpVerificationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool IsNewUser { get; set; }
        public long? UserPKID { get; set; }
        public string Token { get; set; }
        public string Role { get; set; }
        public bool DeviceTrusted { get; set; }
        public long? TrustedDeviceId { get; set; }
        public OtpPurpose Purpose { get; set; }
    }
}
