using System;
using System.Collections.Generic;

namespace CateringEcommerce.Domain.Models.Security
{
    /// <summary>
    /// Two-Factor Authentication Configuration
    /// </summary>
    public class TwoFactorAuthModel
    {
        public long TwoFactorId { get; set; }
        public long UserId { get; set; }
        public string UserType { get; set; } // USER or OWNER

        // Configuration
        public bool IsEnabled { get; set; }
        public string SecretKey { get; set; } // Base32 encoded
        public string Method { get; set; } // TOTP, SMS, EMAIL

        // Setup
        public bool SetupCompleted { get; set; }
        public DateTime? SetupDate { get; set; }
        public DateTime? VerifiedDate { get; set; }

        // Backup Codes
        public string BackupCodesJson { get; set; } // JSON array
        public int BackupCodesUsed { get; set; }

        // Recovery
        public string RecoveryEmail { get; set; }
        public string RecoveryPhone { get; set; }

        // Security
        public int FailedAttempts { get; set; }
        public DateTime? LockedUntil { get; set; }
        public DateTime? LastVerified { get; set; }

        // Audit
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    /// <summary>
    /// 2FA Setup Response with QR Code
    /// </summary>
    public class TwoFactorSetupDto
    {
        public string SecretKey { get; set; }
        public string QrCodeDataUrl { get; set; } // Base64 encoded QR code image
        public List<string> BackupCodes { get; set; }
        public string SetupUri { get; set; } // otpauth:// URI
        public string Issuer { get; set; }
        public string AccountName { get; set; }
    }

    /// <summary>
    /// 2FA Enable Request
    /// </summary>
    public class EnableTwoFactorDto
    {
        public string VerificationCode { get; set; } // 6-digit code from authenticator app
        public string RecoveryEmail { get; set; }
        public string RecoveryPhone { get; set; }
    }

    /// <summary>
    /// 2FA Verification Request
    /// </summary>
    public class VerifyTwoFactorDto
    {
        public long UserId { get; set; }
        public string UserType { get; set; } // USER or OWNER
        public string Code { get; set; } // 6-digit TOTP or backup code
        public bool IsBackupCode { get; set; }
        public bool RememberDevice { get; set; } // "Remember this device for 30 days"

        // Context (for logging)
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string DeviceInfo { get; set; }
    }

    /// <summary>
    /// 2FA Verification Result
    /// </summary>
    public class TwoFactorVerificationResult
    {
        public bool IsValid { get; set; }
        public bool IsLocked { get; set; }
        public int RemainingAttempts { get; set; }
        public DateTime? LockedUntil { get; set; }
        public string Message { get; set; }
        public string DeviceToken { get; set; } // If "remember device" was requested
    }

    /// <summary>
    /// 2FA Status Response
    /// </summary>
    public class TwoFactorStatusDto
    {
        public bool IsEnabled { get; set; }
        public bool SetupCompleted { get; set; }
        public string Method { get; set; }
        public DateTime? EnabledDate { get; set; }
        public DateTime? LastVerified { get; set; }
        public int BackupCodesRemaining { get; set; }
        public bool HasRecoveryEmail { get; set; }
        public bool HasRecoveryPhone { get; set; }
        public List<TrustedDeviceDto> TrustedDevices { get; set; }
    }

    /// <summary>
    /// Regenerate Backup Codes Request
    /// </summary>
    public class RegenerateBackupCodesDto
    {
        public string VerificationCode { get; set; } // Must verify with current 2FA code
    }

    /// <summary>
    /// Disable 2FA Request
    /// </summary>
    public class DisableTwoFactorDto
    {
        public string VerificationCode { get; set; } // Must verify before disabling
        public string Reason { get; set; }
    }

    /// <summary>
    /// Trusted Device Model
    /// </summary>
    public class TrustedDeviceDto
    {
        public long DeviceId { get; set; }
        public string DeviceToken { get; set; }
        public string DeviceName { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string OperatingSystem { get; set; }
        public DateTime TrustedDate { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? LastUsed { get; set; }
        public bool IsActive { get; set; }
        public bool IsCurrentDevice { get; set; }
    }

    /// <summary>
    /// Revoke Trusted Device Request
    /// </summary>
    public class RevokeTrustedDeviceDto
    {
        public long DeviceId { get; set; }
        public string Reason { get; set; }
    }

    /// <summary>
    /// 2FA Recovery Request
    /// </summary>
    public class RecoverTwoFactorAccessDto
    {
        public string EmailOrPhone { get; set; }
        public string RecoveryCode { get; set; } // Sent to recovery email/phone
    }

    /// <summary>
    /// 2FA Attempt Log
    /// </summary>
    public class TwoFactorAttemptLog
    {
        public long LogId { get; set; }
        public string UserType { get; set; }
        public long UserId { get; set; }
        public string MethodUsed { get; set; }
        public bool IsSuccessful { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string FailureReason { get; set; }
        public DateTime AttemptDate { get; set; }
    }
}
