using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Security;
using CateringEcommerce.Domain.Models.Security;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CateringEcommerce.BAL.Base.Security
{
    /// <summary>
    /// Two-Factor Authentication Repository Implementation
    /// Uses TOTP (Time-based One-Time Password) algorithm
    /// </summary>
    public class TwoFactorAuthRepository : ITwoFactorAuthRepository
    {
        private readonly IDatabaseHelper _dbHelper;
        private const int TOTP_DIGITS = 6;
        private const int TOTP_PERIOD = 30; // seconds
        private const int BACKUP_CODE_COUNT = 10;
        private const int BACKUP_CODE_LENGTH = 8;
        private const int MAX_FAILED_ATTEMPTS = 5;
        private const int LOCKOUT_MINUTES = 15;
        private const int TRUSTED_DEVICE_DAYS = 30;

        public TwoFactorAuthRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        }

        #region Setup & Configuration

        public async Task<TwoFactorSetupDto> InitializeTwoFactorAsync(long userId, string userType, string accountName)
        {
            // Generate secret key (Base32 encoded)
            var secretKey = GenerateSecretKey();
            var issuer = "Enyvora Catering";

            // Generate setup URI for authenticator apps
            var setupUri = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(accountName)}?secret={secretKey}&issuer={Uri.EscapeDataString(issuer)}&digits={TOTP_DIGITS}&period={TOTP_PERIOD}";

            // Generate backup codes
            var backupCodes = await GenerateBackupCodesAsync(BACKUP_CODE_COUNT);
            var hashedBackupCodes = backupCodes.Select(HashBackupCode).ToList();

            // Store in database (but not enabled yet)
            var tableName = userType == "USER" ? Table.SysUser2FA : Table.SysOwner2FA;
            var userIdColumn = userType == "USER" ? "c_userid" : "c_ownerid";

            var query = $@"
                MERGE {tableName} AS target
                USING (SELECT @UserId AS {userIdColumn}) AS source
                ON target.{userIdColumn} = source.{userIdColumn}
                WHEN MATCHED THEN
                    UPDATE SET
                        c_secret_key = @SecretKey,
                        c_backup_codes = @BackupCodes,
                        c_backup_codes_used = 0,
                        c_setup_completed = 0,
                        c_is_enabled = 0,
                        c_modifieddate = GETDATE()
                WHEN NOT MATCHED THEN
                    INSERT ({userIdColumn}, c_secret_key, c_backup_codes, c_is_enabled, c_setup_completed)
                    VALUES (@UserId, @SecretKey, @BackupCodes, 0, 0);";

            var parameters = new[]
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@SecretKey", secretKey),
                new SqlParameter("@BackupCodes", JsonSerializer.Serialize(hashedBackupCodes))
            };

            await _dbHelper.ExecuteNonQueryAsync(query, parameters);

            // Generate QR code (Base64 data URL)
            var qrCodeDataUrl = GenerateQrCodeDataUrl(setupUri);

            return new TwoFactorSetupDto
            {
                SecretKey = secretKey,
                QrCodeDataUrl = qrCodeDataUrl,
                BackupCodes = backupCodes,
                SetupUri = setupUri,
                Issuer = issuer,
                AccountName = accountName
            };
        }

        public async Task<bool> EnableTwoFactorAsync(long userId, string userType, EnableTwoFactorDto request)
        {
            // Get current config
            var config = await GetTwoFactorConfigAsync(userId, userType);
            if (config == null) return false;

            // Verify the code before enabling
            if (!ValidateTotpCodeSync(config.SecretKey, request.VerificationCode))
            {
                return false;
            }

            // Enable 2FA
            var tableName = userType == "USER" ? Table.SysUser2FA : Table.SysOwner2FA;
            var userIdColumn = userType == "USER" ? "c_userid" : "c_ownerid";

            var query = $@"
                UPDATE {tableName}
                SET c_is_enabled = 1,
                    c_setup_completed = 1,
                    c_setup_date = GETDATE(),
                    c_verified_date = GETDATE(),
                    c_recovery_email = @RecoveryEmail,
                    c_recovery_phone = @RecoveryPhone,
                    c_failed_attempts = 0,
                    c_locked_until = NULL,
                    c_modifieddate = GETDATE()
                WHERE {userIdColumn} = @UserId";

            var parameters = new[]
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@RecoveryEmail", (object)request.RecoveryEmail ?? DBNull.Value),
                new SqlParameter("@RecoveryPhone", (object)request.RecoveryPhone ?? DBNull.Value)
            };

            var result = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        public async Task<bool> DisableTwoFactorAsync(long userId, string userType, DisableTwoFactorDto request)
        {
            // Verify the code before disabling
            var verifyResult = await VerifyTwoFactorCodeAsync(new VerifyTwoFactorDto
            {
                UserId = userId,
                UserType = userType,
                Code = request.VerificationCode,
                IsBackupCode = false
            });

            if (!verifyResult.IsValid)
            {
                return false;
            }

            // Disable 2FA
            var tableName = userType == "USER" ? Table.SysUser2FA : Table.SysOwner2FA;
            var userIdColumn = userType == "USER" ? "c_userid" : "c_ownerid";

            var query = $@"
                UPDATE {tableName}
                SET c_is_enabled = 0,
                    c_modifieddate = GETDATE()
                WHERE {userIdColumn} = @UserId";

            var parameters = new[]
            {
                new SqlParameter("@UserId", userId)
            };

            var result = await _dbHelper.ExecuteNonQueryAsync(query, parameters);

            // Also revoke all trusted devices
            if (result > 0)
            {
                await RevokeAllTrustedDevicesAsync(userId, userType);
            }

            return result > 0;
        }

        public async Task<TwoFactorAuthModel> GetTwoFactorConfigAsync(long userId, string userType)
        {
            var tableName = userType == "USER" ? Table.SysUser2FA : Table.SysOwner2FA;
            var userIdColumn = userType == "USER" ? "c_userid" : "c_ownerid";

            var query = $@"
                SELECT * FROM {tableName}
                WHERE {userIdColumn} = @UserId";

            var parameters = new[]
            {
                new SqlParameter("@UserId", userId)
            };

            var results = await _dbHelper.ExecuteQueryAsync<TwoFactorAuthModel>(query, parameters);
            return results.FirstOrDefault();
        }

        public async Task<bool> IsTwoFactorEnabledAsync(long userId, string userType)
        {
            var config = await GetTwoFactorConfigAsync(userId, userType);
            return config?.IsEnabled ?? false;
        }

        #endregion

        #region Verification

        public async Task<TwoFactorVerificationResult> VerifyTwoFactorCodeAsync(VerifyTwoFactorDto request)
        {
            var config = await GetTwoFactorConfigAsync(request.UserId, request.UserType);
            if (config == null || !config.IsEnabled)
            {
                return new TwoFactorVerificationResult
                {
                    IsValid = false,
                    Message = "2FA is not enabled for this account"
                };
            }

            // Check if locked
            if (config.LockedUntil.HasValue && config.LockedUntil.Value > DateTime.UtcNow)
            {
                return new TwoFactorVerificationResult
                {
                    IsValid = false,
                    IsLocked = true,
                    LockedUntil = config.LockedUntil,
                    Message = $"Account locked due to failed attempts. Try again after {config.LockedUntil.Value:yyyy-MM-dd HH:mm:ss}"
                };
            }

            bool isValid = false;
            string method = "TOTP";

            if (request.IsBackupCode)
            {
                isValid = await ValidateBackupCodeAsync(request.UserId, request.UserType, request.Code);
                method = "BACKUP_CODE";
            }
            else
            {
                isValid = ValidateTotpCodeSync(config.SecretKey, request.Code);
            }

            // Log attempt
            await LogVerificationAttemptAsync(
                request.UserId,
                request.UserType,
                method,
                isValid,
                request.IpAddress,
                request.UserAgent,
                isValid ? null : "Invalid code"
            );

            if (isValid)
            {
                // Reset failed attempts
                await ResetFailedAttemptsAsync(request.UserId, request.UserType);

                // Trust device if requested
                string deviceToken = null;
                if (request.RememberDevice)
                {
                    deviceToken = await TrustDeviceAsync(
                        request.UserId,
                        request.UserType,
                        request.DeviceInfo,
                        request.IpAddress,
                        request.UserAgent
                    );
                }

                return new TwoFactorVerificationResult
                {
                    IsValid = true,
                    Message = "Verification successful",
                    DeviceToken = deviceToken
                };
            }
            else
            {
                // Increment failed attempts
                var newFailedAttempts = config.FailedAttempts + 1;
                var remainingAttempts = MAX_FAILED_ATTEMPTS - newFailedAttempts;

                if (newFailedAttempts >= MAX_FAILED_ATTEMPTS)
                {
                    // Lock account
                    await LockAccountAsync(request.UserId, request.UserType, LOCKOUT_MINUTES);

                    return new TwoFactorVerificationResult
                    {
                        IsValid = false,
                        IsLocked = true,
                        RemainingAttempts = 0,
                        LockedUntil = DateTime.UtcNow.AddMinutes(LOCKOUT_MINUTES),
                        Message = $"Account locked for {LOCKOUT_MINUTES} minutes due to too many failed attempts"
                    };
                }
                else
                {
                    await IncrementFailedAttemptsAsync(request.UserId, request.UserType);

                    return new TwoFactorVerificationResult
                    {
                        IsValid = false,
                        RemainingAttempts = remainingAttempts,
                        Message = $"Invalid code. {remainingAttempts} attempts remaining"
                    };
                }
            }
        }

        public async Task<bool> IsDeviceTrustedAsync(long userId, string userType, string deviceToken)
        {
            if (string.IsNullOrWhiteSpace(deviceToken)) return false;

            var query = $@"
                SELECT COUNT(*)
                FROM {Table.SysTrustedDevice}
                WHERE c_user_type = @UserType
                  AND c_userid = @UserId
                  AND c_device_token = @DeviceToken
                  AND c_is_active = 1
                  AND c_expires_at > GETDATE()";

            var parameters = new[]
            {
                new SqlParameter("@UserType", userType),
                new SqlParameter("@UserId", userId),
                new SqlParameter("@DeviceToken", deviceToken)
            };

            var resultObj = await _dbHelper.ExecuteScalarAsync(query, parameters);
            var result = Convert.ToInt32(resultObj);
            return result > 0;
        }

        public Task<bool> ValidateTotpCodeAsync(string secretKey, string code)
        {
            return Task.FromResult(ValidateTotpCodeSync(secretKey, code));
        }

        private bool ValidateTotpCodeSync(string secretKey, string code)
        {
            try
            {
                // Simple TOTP validation using time windows
                var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var timeStep = currentTime / TOTP_PERIOD;

                // Check current window and ±1 window (allows for clock drift)
                for (int i = -1; i <= 1; i++)
                {
                    var computedCode = GenerateTotpCode(secretKey, timeStep + i);
                    if (code == computedCode)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ValidateBackupCodeAsync(long userId, string userType, string code)
        {
            var config = await GetTwoFactorConfigAsync(userId, userType);
            if (config == null || string.IsNullOrWhiteSpace(config.BackupCodesJson))
            {
                return false;
            }

            var hashedCodes = JsonSerializer.Deserialize<List<string>>(config.BackupCodesJson);
            if (hashedCodes == null || hashedCodes.Count == 0)
            {
                return false;
            }

            var codeHash = HashBackupCode(code);
            var codeIndex = hashedCodes.IndexOf(codeHash);

            if (codeIndex >= 0)
            {
                // Remove used code
                hashedCodes.RemoveAt(codeIndex);

                // Update database
                var tableName = userType == "USER" ? Table.SysUser2FA : Table.SysOwner2FA;
                var userIdColumn = userType == "USER" ? "c_userid" : "c_ownerid";

                var query = $@"
                    UPDATE {Table.SysUser2FA}
                        SET c_backup_codes = @BackupCodes,
                            c_backup_codes_used = c_backup_codes_used + 1,
                            c_modifieddate = GETDATE()
                        WHERE {userIdColumn} = @UserId";

                var parameters = new[]
                {
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@BackupCodes", JsonSerializer.Serialize(hashedCodes))
                };

                await _dbHelper.ExecuteNonQueryAsync(query, parameters);
                return true;
            }

            return false;
        }

        #endregion

        #region Backup Codes

        public Task<List<string>> GenerateBackupCodesAsync(int count = 10)
        {
            var codes = new List<string>();
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Exclude similar looking chars

            for (int i = 0; i < count; i++)
            {
                var code = new char[BACKUP_CODE_LENGTH];
                for (int j = 0; j < BACKUP_CODE_LENGTH; j++)
                {
                    code[j] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
                }
                codes.Add(new string(code));
            }

            return Task.FromResult(codes);
        }

        public async Task<List<string>> RegenerateBackupCodesAsync(long userId, string userType, RegenerateBackupCodesDto request)
        {
            // Verify code before regenerating
            var verifyResult = await VerifyTwoFactorCodeAsync(new VerifyTwoFactorDto
            {
                UserId = userId,
                UserType = userType,
                Code = request.VerificationCode,
                IsBackupCode = false
            });

            if (!verifyResult.IsValid)
            {
                return null;
            }

            // Generate new codes
            var newCodes = await GenerateBackupCodesAsync(BACKUP_CODE_COUNT);
            var hashedCodes = newCodes.Select(HashBackupCode).ToList();

            // Update database
            var tableName = userType == "USER" ? Table.SysUser2FA : Table.SysOwner2FA;
            var userIdColumn = userType == "USER" ? "c_userid" : "c_ownerid";

            var query = $@"
                UPDATE {tableName}
                SET c_backup_codes = @BackupCodes,
                    c_backup_codes_used = 0,
                    c_modifieddate = GETDATE()
                WHERE {userIdColumn} = @UserId";

            var parameters = new[]
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@BackupCodes", JsonSerializer.Serialize(hashedCodes))
            };

            await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return newCodes;
        }

        public async Task<int> GetRemainingBackupCodesCountAsync(long userId, string userType)
        {
            var config = await GetTwoFactorConfigAsync(userId, userType);
            if (config == null || string.IsNullOrWhiteSpace(config.BackupCodesJson))
            {
                return 0;
            }

            var hashedCodes = JsonSerializer.Deserialize<List<string>>(config.BackupCodesJson);
            return hashedCodes?.Count ?? 0;
        }

        #endregion

        #region Trusted Devices

        public async Task<string> TrustDeviceAsync(long userId, string userType, string deviceInfo, string ipAddress, string userAgent)
        {
            var deviceToken = Guid.NewGuid().ToString("N");
            var expiresAt = DateTime.UtcNow.AddDays(TRUSTED_DEVICE_DAYS);

            // Parse device info
            var deviceName = ParseDeviceName(userAgent);
            var browser = ParseBrowser(userAgent);
            var os = ParseOS(userAgent);

            var query = $@"
                INSERT INTO {Table.SysTrustedDevice} (
                    c_user_type, c_userid, c_device_token, c_device_name,
                    c_ip_address, c_user_agent, c_browser, c_os, c_expires_at
                )
                VALUES (
                    @UserType, @UserId, @DeviceToken, @DeviceName,
                    @IpAddress, @UserAgent, @Browser, @OS, @ExpiresAt
                )";

            var parameters = new[]
            {
                new SqlParameter("@UserType", userType),
                new SqlParameter("@UserId", userId),
                new SqlParameter("@DeviceToken", deviceToken),
                new SqlParameter("@DeviceName", deviceName),
                new SqlParameter("@IpAddress", (object)ipAddress ?? DBNull.Value),
                new SqlParameter("@UserAgent", (object)userAgent ?? DBNull.Value),
                new SqlParameter("@Browser", browser),
                new SqlParameter("@OS", os),
                new SqlParameter("@ExpiresAt", expiresAt)
            };

            await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return deviceToken;
        }

        public async Task<List<TrustedDeviceDto>> GetTrustedDevicesAsync(long userId, string userType)
        {
            var query = $@"
                SELECT
                    c_device_id AS DeviceId,
                    c_device_token AS DeviceToken,
                    c_device_name AS DeviceName,
                    c_ip_address AS IpAddress,
                    c_browser AS Browser,
                    c_os AS OperatingSystem,
                    c_trusted_date AS TrustedDate,
                    c_expires_at AS ExpiresAt,
                    c_last_used AS LastUsed,
                    c_is_active AS IsActive
                FROM {Table.SysTrustedDevice}
                WHERE c_user_type = @UserType
                  AND c_userid = @UserId
                  AND c_is_active = 1
                ORDER BY c_trusted_date DESC";

            var parameters = new[]
            {
                new SqlParameter("@UserType", userType),
                new SqlParameter("@UserId", userId)
            };

            return await _dbHelper.ExecuteQueryAsync<TrustedDeviceDto>(query, parameters);
        }

        public async Task<bool> RevokeTrustedDeviceAsync(long deviceId, RevokeTrustedDeviceDto request)
        {
            var query = $@"
                UPDATE {Table.SysTrustedDevice}
                SET c_is_active = 0,
                    c_revoked_date = GETDATE(),
                    c_revoked_reason = @Reason
                WHERE c_device_id = @DeviceId";

            var parameters = new[]
            {
                new SqlParameter("@DeviceId", deviceId),
                new SqlParameter("@Reason", (object)request.Reason ?? DBNull.Value)
            };

            var result = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        public async Task<bool> RevokeAllTrustedDevicesAsync(long userId, string userType)
        {
            var query = $@"
                UPDATE {Table.SysTrustedDevice}
                SET c_is_active = 0,
                    c_revoked_date = GETDATE(),
                    c_revoked_reason = 'All devices revoked by user'
                WHERE c_user_type = @UserType
                  AND c_userid = @UserId
                  AND c_is_active = 1";

            var parameters = new[]
            {
                new SqlParameter("@UserType", userType),
                new SqlParameter("@UserId", userId)
            };

            var result = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        #endregion

        #region Recovery

        public async Task<bool> SendRecoveryCodeAsync(string emailOrPhone)
        {
            // TODO: Implement email/SMS sending
            // Generate 6-digit recovery code
            // Store in database with expiry
            // Send via email or SMS
            await Task.CompletedTask;
            return true;
        }

        public async Task<bool> RecoverTwoFactorAccessAsync(RecoverTwoFactorAccessDto request)
        {
            // TODO: Implement recovery code validation and 2FA reset
            await Task.CompletedTask;
            return false;
        }

        #endregion

        #region Status & Monitoring

        public async Task<TwoFactorStatusDto> GetTwoFactorStatusAsync(long userId, string userType, string currentDeviceToken = null)
        {
            var config = await GetTwoFactorConfigAsync(userId, userType);
            if (config == null)
            {
                return new TwoFactorStatusDto
                {
                    IsEnabled = false,
                    SetupCompleted = false
                };
            }

            var backupCodesRemaining = await GetRemainingBackupCodesCountAsync(userId, userType);
            var trustedDevices = await GetTrustedDevicesAsync(userId, userType);

            // Mark current device if provided
            if (!string.IsNullOrWhiteSpace(currentDeviceToken))
            {
                foreach (var device in trustedDevices)
                {
                    device.IsCurrentDevice = device.DeviceToken == currentDeviceToken;
                }
            }

            return new TwoFactorStatusDto
            {
                IsEnabled = config.IsEnabled,
                SetupCompleted = config.SetupCompleted,
                Method = config.Method,
                EnabledDate = config.SetupDate,
                LastVerified = config.LastVerified,
                BackupCodesRemaining = backupCodesRemaining,
                HasRecoveryEmail = !string.IsNullOrWhiteSpace(config.RecoveryEmail),
                HasRecoveryPhone = !string.IsNullOrWhiteSpace(config.RecoveryPhone),
                TrustedDevices = trustedDevices
            };
        }

        public async Task LogVerificationAttemptAsync(long userId, string userType, string method, bool isSuccessful,
            string ipAddress, string userAgent, string failureReason = null)
        {
            var query = $@"
                INSERT INTO {Table.Sys2FAAttemptLog} (
                    c_user_type, c_userid, c_method_used, c_is_successful,
                    c_ip_address, c_user_agent, c_failure_reason
                )
                VALUES (
                    @UserType, @UserId, @Method, @IsSuccessful,
                    @IpAddress, @UserAgent, @FailureReason
                )";

            var parameters = new[]
            {
                new SqlParameter("@UserType", userType),
                new SqlParameter("@UserId", userId),
                new SqlParameter("@Method", method),
                new SqlParameter("@IsSuccessful", isSuccessful),
                new SqlParameter("@IpAddress", (object)ipAddress ?? DBNull.Value),
                new SqlParameter("@UserAgent", (object)userAgent ?? DBNull.Value),
                new SqlParameter("@FailureReason", (object)failureReason ?? DBNull.Value)
            };

            await _dbHelper.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<List<TwoFactorAttemptLog>> GetRecentAttemptsAsync(long userId, string userType, int limit = 10)
        {
            var query = $@"
                SELECT TOP (@Limit)
                    c_log_id AS LogId,
                    c_user_type AS UserType,
                    c_userid AS UserId,
                    c_method_used AS MethodUsed,
                    c_is_successful AS IsSuccessful,
                    c_ip_address AS IpAddress,
                    c_user_agent AS UserAgent,
                    c_failure_reason AS FailureReason,
                    c_attempt_date AS AttemptDate
                FROM {Table.Sys2FAAttemptLog}
                WHERE c_user_type = @UserType
                  AND c_userid = @UserId
                ORDER BY c_attempt_date DESC";

            var parameters = new[]
            {
                new SqlParameter("@UserType", userType),
                new SqlParameter("@UserId", userId),
                new SqlParameter("@Limit", limit)
            };

            return await _dbHelper.ExecuteQueryAsync<TwoFactorAttemptLog>(query, parameters);
        }

        public async Task<(bool IsLocked, DateTime? LockedUntil)> CheckLockStatusAsync(long userId, string userType)
        {
            var config = await GetTwoFactorConfigAsync(userId, userType);
            if (config == null)
            {
                return (false, null);
            }

            var isLocked = config.LockedUntil.HasValue && config.LockedUntil.Value > DateTime.UtcNow;
            return (isLocked, config.LockedUntil);
        }

        public async Task<bool> ResetFailedAttemptsAsync(long userId, string userType)
        {
            var tableName = userType == "USER" ? Table.SysUser2FA : Table.SysOwner2FA;
            var userIdColumn = userType == "USER" ? "c_userid" : "c_ownerid";

            var query = $@"
                UPDATE {tableName}
                SET c_failed_attempts = 0,
                    c_locked_until = NULL,
                    c_last_verified = GETDATE(),
                    c_modifieddate = GETDATE()
                WHERE {userIdColumn} = @UserId";

            var parameters = new[]
            {
                new SqlParameter("@UserId", userId)
            };

            var result = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        #endregion

        #region Helper Methods

        private string GenerateSecretKey()
        {
            // Generate 20 random bytes and Base32 encode
            var bytes = new byte[20];
            RandomNumberGenerator.Fill(bytes);
            return Base32Encode(bytes);
        }

        private string GenerateTotpCode(string secretKey, long timeStep)
        {
            var keyBytes = Base32Decode(secretKey);
            var timeBytes = BitConverter.GetBytes(timeStep);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(timeBytes);
            }

            using var hmac = new HMACSHA1(keyBytes);
            var hash = hmac.ComputeHash(timeBytes);

            var offset = hash[hash.Length - 1] & 0x0F;
            var binary =
                ((hash[offset] & 0x7F) << 24) |
                ((hash[offset + 1] & 0xFF) << 16) |
                ((hash[offset + 2] & 0xFF) << 8) |
                (hash[offset + 3] & 0xFF);

            var otp = binary % (int)Math.Pow(10, TOTP_DIGITS);
            return otp.ToString().PadLeft(TOTP_DIGITS, '0');
        }

        private string Base32Encode(byte[] data)
        {
            const string base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var result = new StringBuilder((data.Length * 8 + 4) / 5);

            int bits = 0;
            int value = 0;

            foreach (var b in data)
            {
                value = (value << 8) | b;
                bits += 8;

                while (bits >= 5)
                {
                    result.Append(base32Chars[(value >> (bits - 5)) & 0x1F]);
                    bits -= 5;
                }
            }

            if (bits > 0)
            {
                result.Append(base32Chars[(value << (5 - bits)) & 0x1F]);
            }

            return result.ToString();
        }

        private byte[] Base32Decode(string base32)
        {
            base32 = base32.ToUpperInvariant().Replace(" ", "").Replace("-", "");
            const string base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

            var result = new List<byte>();
            int bits = 0;
            int value = 0;

            foreach (var c in base32)
            {
                var index = base32Chars.IndexOf(c);
                if (index < 0) continue;

                value = (value << 5) | index;
                bits += 5;

                if (bits >= 8)
                {
                    result.Add((byte)(value >> (bits - 8)));
                    bits -= 8;
                }
            }

            return result.ToArray();
        }

        private string GenerateQrCodeDataUrl(string data)
        {
            // Placeholder: Generate QR code as base64 data URL
            // In production, use QRCoder or similar library
            return $"data:image/png;base64,placeholder_for_qr_code";
        }

        private string HashBackupCode(string code)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(code);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private async Task<bool> IncrementFailedAttemptsAsync(long userId, string userType)
        {
            var tableName = userType == "USER" ? Table.SysUser2FA : Table.SysOwner2FA;
            var userIdColumn = userType == "USER" ? "c_userid" : "c_ownerid";

            var query = $@"
                UPDATE {tableName}
                SET c_failed_attempts = c_failed_attempts + 1,
                    c_modifieddate = GETDATE()
                WHERE {userIdColumn} = @UserId";

            var parameters = new[]
            {
                new SqlParameter("@UserId", userId)
            };

            var result = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        private async Task<bool> LockAccountAsync(long userId, string userType, int minutes)
        {
            var tableName = userType == "USER" ? Table.SysUser2FA : Table.SysOwner2FA;
            var userIdColumn = userType == "USER" ? "c_userid" : "c_ownerid";

            var query = $@"
                UPDATE {tableName}
                SET c_locked_until = DATEADD(MINUTE, @Minutes, GETDATE()),
                    c_modifieddate = GETDATE()
                WHERE {userIdColumn} = @UserId";

            var parameters = new[]
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@Minutes", minutes)
            };

            var result = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        private string ParseDeviceName(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent)) return "Unknown Device";

            var browser = ParseBrowser(userAgent);
            var os = ParseOS(userAgent);
            return $"{browser} on {os}";
        }

        private string ParseBrowser(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent)) return "Unknown";

            if (userAgent.Contains("Edg/")) return "Edge";
            if (userAgent.Contains("Chrome/")) return "Chrome";
            if (userAgent.Contains("Firefox/")) return "Firefox";
            if (userAgent.Contains("Safari/") && !userAgent.Contains("Chrome")) return "Safari";
            if (userAgent.Contains("Opera/") || userAgent.Contains("OPR/")) return "Opera";

            return "Unknown";
        }

        private string ParseOS(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent)) return "Unknown";

            if (userAgent.Contains("Windows NT 10.0")) return "Windows 10";
            if (userAgent.Contains("Windows NT 6.3")) return "Windows 8.1";
            if (userAgent.Contains("Windows NT 6.2")) return "Windows 8";
            if (userAgent.Contains("Windows NT 6.1")) return "Windows 7";
            if (userAgent.Contains("Mac OS X")) return "macOS";
            if (userAgent.Contains("Android")) return "Android";
            if (userAgent.Contains("iPhone") || userAgent.Contains("iPad")) return "iOS";
            if (userAgent.Contains("Linux")) return "Linux";

            return "Unknown";
        }

        #endregion
    }
}
