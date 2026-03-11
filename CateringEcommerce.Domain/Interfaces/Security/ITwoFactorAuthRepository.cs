using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Models.Security;

namespace CateringEcommerce.Domain.Interfaces.Security
{
    /// <summary>
    /// Two-Factor Authentication Repository
    /// Handles 2FA for both Users and Owners/Partners
    /// </summary>
    public interface ITwoFactorAuthRepository
    {
        // =============================================
        // SETUP & CONFIGURATION
        // =============================================

        /// <summary>
        /// Initialize 2FA setup - generates secret key and QR code
        /// </summary>
        Task<TwoFactorSetupDto> InitializeTwoFactorAsync(long userId, string userType, string accountName);

        /// <summary>
        /// Enable 2FA after verifying the setup code
        /// </summary>
        Task<bool> EnableTwoFactorAsync(long userId, string userType, EnableTwoFactorDto request);

        /// <summary>
        /// Disable 2FA (requires verification)
        /// </summary>
        Task<bool> DisableTwoFactorAsync(long userId, string userType, DisableTwoFactorDto request);

        /// <summary>
        /// Get 2FA configuration for user
        /// </summary>
        Task<TwoFactorAuthModel> GetTwoFactorConfigAsync(long userId, string userType);

        /// <summary>
        /// Check if user has 2FA enabled
        /// </summary>
        Task<bool> IsTwoFactorEnabledAsync(long userId, string userType);

        // =============================================
        // VERIFICATION
        // =============================================

        /// <summary>
        /// Verify 2FA code (TOTP or backup code)
        /// </summary>
        Task<TwoFactorVerificationResult> VerifyTwoFactorCodeAsync(VerifyTwoFactorDto request);

        /// <summary>
        /// Check if device is trusted (skip 2FA)
        /// </summary>
        Task<bool> IsDeviceTrustedAsync(long userId, string userType, string deviceToken);

        /// <summary>
        /// Validate TOTP code
        /// </summary>
        Task<bool> ValidateTotpCodeAsync(string secretKey, string code);

        /// <summary>
        /// Validate backup code
        /// </summary>
        Task<bool> ValidateBackupCodeAsync(long userId, string userType, string code);

        // =============================================
        // BACKUP CODES
        // =============================================

        /// <summary>
        /// Generate new backup codes
        /// </summary>
        Task<List<string>> GenerateBackupCodesAsync(int count = 10);

        /// <summary>
        /// Regenerate backup codes (invalidates old ones)
        /// </summary>
        Task<List<string>> RegenerateBackupCodesAsync(long userId, string userType, RegenerateBackupCodesDto request);

        /// <summary>
        /// Get remaining backup codes count
        /// </summary>
        Task<int> GetRemainingBackupCodesCountAsync(long userId, string userType);

        // =============================================
        // TRUSTED DEVICES
        // =============================================

        /// <summary>
        /// Trust a device (skip 2FA for 30 days)
        /// </summary>
        Task<string> TrustDeviceAsync(long userId, string userType, string deviceInfo, string ipAddress, string userAgent);

        /// <summary>
        /// Get all trusted devices for user
        /// </summary>
        Task<List<TrustedDeviceDto>> GetTrustedDevicesAsync(long userId, string userType);

        /// <summary>
        /// Revoke trusted device
        /// </summary>
        Task<bool> RevokeTrustedDeviceAsync(long deviceId, RevokeTrustedDeviceDto request);

        /// <summary>
        /// Revoke all trusted devices
        /// </summary>
        Task<bool> RevokeAllTrustedDevicesAsync(long userId, string userType);

        // =============================================
        // RECOVERY
        // =============================================

        /// <summary>
        /// Send recovery code to email/phone
        /// </summary>
        Task<bool> SendRecoveryCodeAsync(string emailOrPhone);

        /// <summary>
        /// Verify recovery code and disable 2FA
        /// </summary>
        Task<bool> RecoverTwoFactorAccessAsync(RecoverTwoFactorAccessDto request);

        // =============================================
        // STATUS & MONITORING
        // =============================================

        /// <summary>
        /// Get 2FA status with statistics
        /// </summary>
        Task<TwoFactorStatusDto> GetTwoFactorStatusAsync(long userId, string userType, string currentDeviceToken = null);

        /// <summary>
        /// Log 2FA verification attempt
        /// </summary>
        Task LogVerificationAttemptAsync(long userId, string userType, string method, bool isSuccessful,
            string ipAddress, string userAgent, string failureReason = null);

        /// <summary>
        /// Get recent 2FA attempts
        /// </summary>
        Task<List<TwoFactorAttemptLog>> GetRecentAttemptsAsync(long userId, string userType, int limit = 10);

        /// <summary>
        /// Check if account is locked due to failed attempts
        /// </summary>
        Task<(bool IsLocked, DateTime? LockedUntil)> CheckLockStatusAsync(long userId, string userType);

        /// <summary>
        /// Reset failed attempts counter
        /// </summary>
        Task<bool> ResetFailedAttemptsAsync(long userId, string userType);
    }
}
