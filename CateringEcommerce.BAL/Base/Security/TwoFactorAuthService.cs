using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using CateringEcommerce.Domain.Models.Security;

namespace CateringEcommerce.BAL.Base.Security
{
    /// <summary>
    /// Two-Factor Authentication Service
    /// Handles role-based 2FA logic, device tracking, and OTP context management
    /// </summary>
    public class TwoFactorAuthService : ITwoFactorAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public TwoFactorAuthService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        #region Device Fingerprinting & Trust Management

        /// <summary>
        /// Check if 2FA is required for this login attempt based on role and device trust
        /// </summary>
        public async Task<TwoFactorRequirement> CheckTwoFactorRequirementAsync(
            string userType,
            long userId,
            string deviceFingerprint,
            string ipAddress,
            string userAgent)
        {
            var requirement = new TwoFactorRequirement
            {
                UserType = userType,
                UserId = userId,
                DeviceFingerprint = deviceFingerprint,
                IpAddress = ipAddress
            };

            // RULE 1: ADMIN always requires 2FA on every login
            if (userType.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
            {
                requirement.IsRequired = true;
                requirement.Reason = "Admin role requires 2FA on every login";
                requirement.Context = "2FA_ADMIN_LOGIN";
                return requirement;
            }

            // RULE 2: PARTNER/OWNER always requires 2FA on every login
            if (userType.Equals("OWNER", StringComparison.OrdinalIgnoreCase) ||
                userType.Equals("PARTNER", StringComparison.OrdinalIgnoreCase))
            {
                requirement.IsRequired = true;
                requirement.Reason = "Partner role requires 2FA on every login";
                requirement.Context = "2FA_PARTNER_LOGIN";
                return requirement;
            }

            // RULE 3: USER - Check if device is trusted
            if (userType.Equals("USER", StringComparison.OrdinalIgnoreCase))
            {
                var trustedDevice = await GetTrustedDeviceAsync(userType, userId, deviceFingerprint);

                if (trustedDevice != null && trustedDevice.IsActive && trustedDevice.ExpiresAt > DateTime.Now)
                {
                    // Device is trusted and not expired - NO 2FA required
                    requirement.IsRequired = false;
                    requirement.Reason = "Trusted device";
                    requirement.Context = "TRUSTED_DEVICE_LOGIN";
                    requirement.TrustedDeviceId = trustedDevice.DeviceId;

                    // Update last used timestamp
                    await UpdateDeviceLastUsedAsync(trustedDevice.DeviceId);

                    return requirement;
                }
                else
                {
                    // New device or expired trust - 2FA required
                    requirement.IsRequired = true;
                    requirement.Reason = trustedDevice == null ? "New device detected" : "Device trust expired";
                    requirement.Context = "2FA_USER_NEW_DEVICE";
                    return requirement;
                }
            }

            // Default: Require 2FA for unknown user types
            requirement.IsRequired = true;
            requirement.Reason = "Unknown user type - default security";
            requirement.Context = "2FA_UNKNOWN_TYPE";
            return requirement;
        }

        /// <summary>
        /// Check if 2FA is required for a sensitive action
        /// </summary>
        public async Task<TwoFactorRequirement> CheckSensitiveActionRequirementAsync(
            string userType,
            long userId,
            string actionType,
            string deviceFingerprint)
        {
            var requirement = new TwoFactorRequirement
            {
                UserType = userType,
                UserId = userId,
                DeviceFingerprint = deviceFingerprint,
                IsRequired = true // Always required for sensitive actions
            };

            switch (actionType.ToUpper())
            {
                case "PLACE_ORDER":
                    requirement.Reason = "Order placement requires verification";
                    requirement.Context = "2FA_PLACE_ORDER";
                    break;
                case "MAKE_PAYMENT":
                    requirement.Reason = "Payment requires verification";
                    requirement.Context = "2FA_PAYMENT";
                    break;
                case "APPROVE_FINAL_PAYMENT":
                    requirement.Reason = "Final event payment approval requires verification";
                    requirement.Context = "2FA_FINAL_PAYMENT";
                    break;
                case "UPDATE_BANK_DETAILS":
                    requirement.Reason = "Bank details update requires verification";
                    requirement.Context = "2FA_UPDATE_BANK";
                    break;
                case "WITHDRAW_FUNDS":
                    requirement.Reason = "Fund withdrawal requires verification";
                    requirement.Context = "2FA_WITHDRAW";
                    break;
                default:
                    requirement.Reason = "Sensitive action requires verification";
                    requirement.Context = $"2FA_{actionType}";
                    break;
            }

            return await Task.FromResult(requirement);
        }

        /// <summary>
        /// Get trusted device by fingerprint
        /// </summary>
        public async Task<TrustedDeviceModel> GetTrustedDeviceAsync(string userType, long userId, string deviceFingerprint)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                    SELECT TOP 1
                        c_device_id AS DeviceId,
                        c_user_type AS UserType,
                        c_user_id AS UserId,
                        c_device_token AS DeviceToken,
                        c_device_name AS DeviceName,
                        c_device_fingerprint AS DeviceFingerprint,
                        c_ip_address AS IpAddress,
                        c_user_agent AS UserAgent,
                        c_browser AS Browser,
                        c_os AS OS,
                        c_is_active AS IsActive,
                        c_trusted_date AS TrustedDate,
                        c_expires_at AS ExpiresAt,
                        c_last_used AS LastUsed
                    FROM t_sys_trusted_device
                    WHERE c_user_type = @UserType
                      AND c_user_id = @UserId
                      AND c_device_fingerprint = @DeviceFingerprint
                      AND c_is_active = 1
                      AND c_expires_at > GETDATE()
                    ORDER BY c_trusted_date DESC";

                return await connection.QueryFirstOrDefaultAsync<TrustedDeviceModel>(query, new
                {
                    UserType = userType,
                    UserId = userId,
                    DeviceFingerprint = deviceFingerprint
                });
            }
        }

        /// <summary>
        /// Register a new trusted device (for users only - 30-day trust)
        /// </summary>
        public async Task<long> RegisterTrustedDeviceAsync(TrustedDeviceModel device)
        {
            // Only users can have trusted devices
            if (!device.UserType.Equals("USER", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Only users can register trusted devices. Partners and Admins must use 2FA on every login.");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                var deviceToken = Guid.NewGuid().ToString("N");
                var expiresAt = DateTime.Now.AddDays(30); // 30-day trust period for users

                var query = @"
                    INSERT INTO t_sys_trusted_device (
                        c_user_type,
                        c_user_id,
                        c_device_token,
                        c_device_name,
                        c_device_fingerprint,
                        c_ip_address,
                        c_user_agent,
                        c_browser,
                        c_os,
                        c_is_active,
                        c_trusted_date,
                        c_expires_at,
                        c_last_used
                    ) VALUES (
                        @UserType,
                        @UserId,
                        @DeviceToken,
                        @DeviceName,
                        @DeviceFingerprint,
                        @IpAddress,
                        @UserAgent,
                        @Browser,
                        @OS,
                        1,
                        GETDATE(),
                        @ExpiresAt,
                        GETDATE()
                    );
                    SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

                var deviceId = await connection.QuerySingleAsync<long>(query, new
                {
                    device.UserType,
                    device.UserId,
                    DeviceToken = deviceToken,
                    device.DeviceName,
                    device.DeviceFingerprint,
                    device.IpAddress,
                    device.UserAgent,
                    device.Browser,
                    device.OS,
                    ExpiresAt = expiresAt
                });

                return deviceId;
            }
        }

        /// <summary>
        /// Update device last used timestamp
        /// </summary>
        public async Task UpdateDeviceLastUsedAsync(long deviceId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                    UPDATE t_sys_trusted_device
                    SET c_last_used = GETDATE()
                    WHERE c_device_id = @DeviceId";

                await connection.ExecuteAsync(query, new { DeviceId = deviceId });
            }
        }

        /// <summary>
        /// Get all trusted devices for a user
        /// </summary>
        public async Task<List<TrustedDeviceModel>> GetUserTrustedDevicesAsync(string userType, long userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                    SELECT
                        c_device_id AS DeviceId,
                        c_user_type AS UserType,
                        c_user_id AS UserId,
                        c_device_token AS DeviceToken,
                        c_device_name AS DeviceName,
                        c_device_fingerprint AS DeviceFingerprint,
                        c_ip_address AS IpAddress,
                        c_user_agent AS UserAgent,
                        c_browser AS Browser,
                        c_os AS OS,
                        c_is_active AS IsActive,
                        c_trusted_date AS TrustedDate,
                        c_expires_at AS ExpiresAt,
                        c_last_used AS LastUsed,
                        c_revoked_date AS RevokedDate,
                        c_revoked_reason AS RevokedReason
                    FROM t_sys_trusted_device
                    WHERE c_user_type = @UserType
                      AND c_user_id = @UserId
                    ORDER BY c_trusted_date DESC";

                var devices = await connection.QueryAsync<TrustedDeviceModel>(query, new
                {
                    UserType = userType,
                    UserId = userId
                });

                return devices.ToList();
            }
        }

        /// <summary>
        /// Revoke a trusted device
        /// </summary>
        public async Task<bool> RevokeTrustedDeviceAsync(long deviceId, string reason = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                    UPDATE t_sys_trusted_device
                    SET c_is_active = 0,
                        c_revoked_date = GETDATE(),
                        c_revoked_reason = @Reason
                    WHERE c_device_id = @DeviceId";

                var rowsAffected = await connection.ExecuteAsync(query, new
                {
                    DeviceId = deviceId,
                    Reason = reason ?? "User revoked"
                });

                return rowsAffected > 0;
            }
        }

        /// <summary>
        /// Revoke all trusted devices for a user (e.g., on password change or security concern)
        /// </summary>
        public async Task<int> RevokeAllUserDevicesAsync(string userType, long userId, string reason = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                    UPDATE t_sys_trusted_device
                    SET c_is_active = 0,
                        c_revoked_date = GETDATE(),
                        c_revoked_reason = @Reason
                    WHERE c_user_type = @UserType
                      AND c_user_id = @UserId
                      AND c_is_active = 1";

                var rowsAffected = await connection.ExecuteAsync(query, new
                {
                    UserType = userType,
                    UserId = userId,
                    Reason = reason ?? "Security revocation"
                });

                return rowsAffected;
            }
        }

        #endregion

        #region OTP Context Management

        /// <summary>
        /// Determine OTP purpose for user-friendly messaging
        /// </summary>
        public OtpPurpose DetermineOtpPurpose(string action, bool isNewUser, bool isNewDevice)
        {
            if (action.Equals("signup", StringComparison.OrdinalIgnoreCase) || isNewUser)
            {
                return new OtpPurpose
                {
                    Code = "VERIFICATION",
                    UserMessage = "Verify your account",
                    Description = "We've sent a verification code to your phone number"
                };
            }

            if (action.Equals("login", StringComparison.OrdinalIgnoreCase) && isNewDevice)
            {
                return new OtpPurpose
                {
                    Code = "2FA_NEW_DEVICE",
                    UserMessage = "Verify to continue",
                    Description = "We detected a new device. Please verify it's you"
                };
            }

            if (action.Equals("login", StringComparison.OrdinalIgnoreCase))
            {
                return new OtpPurpose
                {
                    Code = "2FA_LOGIN",
                    UserMessage = "Two-factor authentication required",
                    Description = "Enter the verification code sent to your phone"
                };
            }

            // Sensitive actions
            if (action.StartsWith("PLACE_ORDER", StringComparison.OrdinalIgnoreCase))
            {
                return new OtpPurpose
                {
                    Code = "2FA_ORDER",
                    UserMessage = "Verify to place order",
                    Description = "Confirm your order by entering the verification code"
                };
            }

            if (action.StartsWith("MAKE_PAYMENT", StringComparison.OrdinalIgnoreCase) ||
                action.StartsWith("PAYMENT", StringComparison.OrdinalIgnoreCase))
            {
                return new OtpPurpose
                {
                    Code = "2FA_PAYMENT",
                    UserMessage = "Verify to complete payment",
                    Description = "Confirm your payment by entering the verification code"
                };
            }

            // Default
            return new OtpPurpose
            {
                Code = "2FA_ACTION",
                UserMessage = "Verification required",
                Description = "Please verify to continue"
            };
        }

        #endregion

        #region 2FA Attempt Logging

        /// <summary>
        /// Log 2FA verification attempt
        /// </summary>
        public async Task LogTwoFactorAttemptAsync(TwoFactorAttemptLog log)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                    INSERT INTO t_sys_2fa_attempt_log (
                        c_user_type,
                        c_user_id,
                        c_method_used,
                        c_is_successful,
                        c_ip_address,
                        c_user_agent,
                        c_failure_reason,
                        c_attempt_date
                    ) VALUES (
                        @UserType,
                        @UserId,
                        @MethodUsed,
                        @IsSuccessful,
                        @IpAddress,
                        @UserAgent,
                        @FailureReason,
                        GETDATE()
                    )";

                await connection.ExecuteAsync(query, new
                {
                    log.UserType,
                    log.UserId,
                    log.MethodUsed,
                    log.IsSuccessful,
                    log.IpAddress,
                    log.UserAgent,
                    log.FailureReason
                });
            }
        }

        /// <summary>
        /// Get recent failed 2FA attempts (for rate limiting)
        /// </summary>
        public async Task<int> GetRecentFailedAttemptsAsync(string userType, long userId, int minutesWindow = 15)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                    SELECT COUNT(*)
                    FROM t_sys_2fa_attempt_log
                    WHERE c_user_type = @UserType
                      AND c_user_id = @UserId
                      AND c_is_successful = 0
                      AND c_attempt_date > DATEADD(MINUTE, -@MinutesWindow, GETDATE())";

                return await connection.QuerySingleAsync<int>(query, new
                {
                    UserType = userType,
                    UserId = userId,
                    MinutesWindow = minutesWindow
                });
            }
        }

        #endregion
    }

    #region Interfaces

    public interface ITwoFactorAuthService
    {
        Task<TwoFactorRequirement> CheckTwoFactorRequirementAsync(string userType, long userId, string deviceFingerprint, string ipAddress, string userAgent);
        Task<TwoFactorRequirement> CheckSensitiveActionRequirementAsync(string userType, long userId, string actionType, string deviceFingerprint);
        Task<TrustedDeviceModel> GetTrustedDeviceAsync(string userType, long userId, string deviceFingerprint);
        Task<long> RegisterTrustedDeviceAsync(TrustedDeviceModel device);
        Task UpdateDeviceLastUsedAsync(long deviceId);
        Task<List<TrustedDeviceModel>> GetUserTrustedDevicesAsync(string userType, long userId);
        Task<bool> RevokeTrustedDeviceAsync(long deviceId, string reason = null);
        Task<int> RevokeAllUserDevicesAsync(string userType, long userId, string reason = null);
        OtpPurpose DetermineOtpPurpose(string action, bool isNewUser, bool isNewDevice);
        Task LogTwoFactorAttemptAsync(TwoFactorAttemptLog log);
        Task<int> GetRecentFailedAttemptsAsync(string userType, long userId, int minutesWindow = 15);
    }

    #endregion
}
