using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.Admin;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace CateringEcommerce.API.Controllers.Admin
{
    [Route("api/admin/settings")]
    [ApiController]
    [AdminAuthorize]
    public class SettingsController : ControllerBase
    {
        private readonly string _connStr;
        private readonly IConfiguration _config;

        public SettingsController(IConfiguration config)
        {
            _config = config;
            _connStr = config.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
        }

        private (long adminId, string adminName) GetCurrentAdmin()
        {
            var adminIdClaim = User.Claims.LastOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            var adminNameClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);

            if (adminIdClaim == null || !long.TryParse(adminIdClaim.Value, out long adminId))
            {
                throw new UnauthorizedAccessException("Invalid admin session.");
            }

            return (adminId, adminNameClaim?.Value ?? "Unknown");
        }

        private async Task<bool> CheckPermissionAsync(long adminId, string permission)
        {
            var dbHelper = new SqlDatabaseManager(_config);
            var rbacRepo = new RBACRepository(dbHelper);
            return await rbacRepo.HasPermissionAsync(adminId, permission);
        }

        private async Task LogAuditAsync(long adminId, string adminName, string action, string module, long? targetId, string? targetType, object? details, string status, string? errorMessage = null)
        {
            var dbHelper = new SqlDatabaseManager(_config);
            var rbacRepo = new RBACRepository(dbHelper);

            await rbacRepo.LogAuditAsync(new AuditLogEntry
            {
                AdminId = adminId,
                AdminName = adminName,
                Action = action,
                Module = module,
                TargetId = targetId,
                TargetType = targetType,
                Details = details != null ? JsonSerializer.Serialize(details) : null,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                Status = status,
                ErrorMessage = errorMessage
            });
        }

        private string GetClientIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        // =============================================
        // SYSTEM SETTINGS ENDPOINTS
        // =============================================

        [HttpGet]
        public async Task<IActionResult> GetSettings([FromQuery] SettingsListRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckPermissionAsync(adminId, "SYSTEM_CONFIG"))
                {
                    await LogAuditAsync(adminId, adminName, "VIEW_SETTINGS", "SETTINGS", null, null, null, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("You don't have permission to view settings."));
                }

                var dbHelper = new SqlDatabaseManager(_config);
                var settingsRepo = new SettingsRepository(dbHelper, _config);

                var result = await settingsRepo.GetSettingsAsync(request);

                await LogAuditAsync(adminId, adminName, "VIEW_SETTINGS", "SETTINGS", null, null, request, "SUCCESS");
                return ApiResponseHelper.Success(result, "Settings retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("{settingKey}")]
        public async Task<IActionResult> GetSettingByKey(string settingKey)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckPermissionAsync(adminId, "SYSTEM_CONFIG"))
                {
                    await LogAuditAsync(adminId, adminName, "VIEW_SETTING", "SETTINGS", null, "Setting", settingKey, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("You don't have permission to view settings."));
                }

                var dbHelper = new SqlDatabaseManager(_config);
                var settingsRepo = new SettingsRepository(dbHelper, _config);

                var setting = await settingsRepo.GetSettingByKeyAsync(settingKey);

                if (setting == null)
                {
                    return ApiResponseHelper.Failure("Setting not found.");
                }

                await LogAuditAsync(adminId, adminName, "VIEW_SETTING", "SETTINGS", setting.SettingId, "Setting", settingKey, "SUCCESS");
                return ApiResponseHelper.Success(setting, "Setting retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPut("{settingId}")]
        public async Task<IActionResult> UpdateSetting(long settingId, [FromBody] UpdateSettingRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckPermissionAsync(adminId, "SYSTEM_CONFIG"))
                {
                    await LogAuditAsync(adminId, adminName, "UPDATE_SETTING", "SETTINGS", settingId, "Setting", request, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("You don't have permission to update settings."));
                }

                // Ensure settingId matches
                if (request.SettingId != settingId)
                {
                    return ApiResponseHelper.Failure("Setting ID mismatch.");
                }

                var dbHelper = new SqlDatabaseManager(_config);
                var settingsRepo = new SettingsRepository(dbHelper, _config);

                var success = await settingsRepo.UpdateSettingAsync(request, adminId, adminName, GetClientIpAddress());

                if (success)
                {
                    await LogAuditAsync(adminId, adminName, "UPDATE_SETTING", "SETTINGS", settingId, "Setting", request, "SUCCESS");
                    return ApiResponseHelper.Success(null, "Setting updated successfully.");
                }

                return ApiResponseHelper.Failure("Failed to update setting.");
            }
            catch (InvalidOperationException ex)
            {
                await LogAuditAsync(GetCurrentAdmin().adminId, GetCurrentAdmin().adminName, "UPDATE_SETTING", "SETTINGS", settingId, "Setting", request, "FAILED", ex.Message);
                return StatusCode(400, ApiResponseHelper.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("{settingId}/history")]
        public async Task<IActionResult> GetSettingHistory(long settingId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckPermissionAsync(adminId, "SYSTEM_CONFIG"))
                {
                    await LogAuditAsync(adminId, adminName, "VIEW_SETTING_HISTORY", "SETTINGS", settingId, "Setting", null, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("You don't have permission to view setting history."));
                }

                var dbHelper = new SqlDatabaseManager(_config);
                var settingsRepo = new SettingsRepository(dbHelper, _config);

                var result = await settingsRepo.GetSettingHistoryAsync(settingId, pageNumber, pageSize);

                await LogAuditAsync(adminId, adminName, "VIEW_SETTING_HISTORY", "SETTINGS", settingId, "Setting", null, "SUCCESS");
                return ApiResponseHelper.Success(result, "Setting history retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost("export")]
        public async Task<IActionResult> ExportSettings([FromBody] SettingsExportRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckPermissionAsync(adminId, "SYSTEM_CONFIG"))
                {
                    await LogAuditAsync(adminId, adminName, "EXPORT_SETTINGS", "SETTINGS", null, null, request, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("You don't have permission to export settings."));
                }

                var dbHelper = new SqlDatabaseManager(_config);
                var settingsRepo = new SettingsRepository(dbHelper, _config);

                var result = await settingsRepo.ExportSettingsAsync(request);

                await LogAuditAsync(adminId, adminName, "EXPORT_SETTINGS", "SETTINGS", null, null, request, "SUCCESS");
                return ApiResponseHelper.Success(result, "Settings exported successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportSettings([FromBody] SettingsImportRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckPermissionAsync(adminId, "SYSTEM_CONFIG"))
                {
                    await LogAuditAsync(adminId, adminName, "IMPORT_SETTINGS", "SETTINGS", null, null, request, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("You don't have permission to import settings."));
                }

                var dbHelper = new SqlDatabaseManager(_config);
                var settingsRepo = new SettingsRepository(dbHelper, _config);

                var result = await settingsRepo.ImportSettingsAsync(request, adminId);

                await LogAuditAsync(adminId, adminName, "IMPORT_SETTINGS", "SETTINGS", null, null, request, "SUCCESS");
                return ApiResponseHelper.Success(result, "Settings imported successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        // =============================================
        // COMMISSION CONFIGURATION ENDPOINTS
        // =============================================

        [HttpGet("commission-configs")]
        public async Task<IActionResult> GetCommissionConfigs([FromQuery] CommissionListRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckPermissionAsync(adminId, "SYSTEM_CONFIG"))
                {
                    await LogAuditAsync(adminId, adminName, "VIEW_COMMISSION_CONFIGS", "SETTINGS", null, null, null, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("You don't have permission to view commission configurations."));
                }

                var dbHelper = new SqlDatabaseManager(_config);
                var settingsRepo = new SettingsRepository(dbHelper, _config);

                var result = await settingsRepo.GetCommissionConfigsAsync(request);

                await LogAuditAsync(adminId, adminName, "VIEW_COMMISSION_CONFIGS", "SETTINGS", null, null, request, "SUCCESS");
                return ApiResponseHelper.Success(result, "Commission configurations retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("commission-configs/{configId}")]
        public async Task<IActionResult> GetCommissionConfigById(long configId)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckPermissionAsync(adminId, "SYSTEM_CONFIG"))
                {
                    await LogAuditAsync(adminId, adminName, "VIEW_COMMISSION_CONFIG", "SETTINGS", configId, "CommissionConfig", null, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("You don't have permission to view commission configurations."));
                }

                var dbHelper = new SqlDatabaseManager(_config);
                var settingsRepo = new SettingsRepository(dbHelper, _config);

                var config = await settingsRepo.GetCommissionConfigByIdAsync(configId);

                if (config == null)
                {
                    return ApiResponseHelper.Failure("Commission configuration not found.");
                }

                await LogAuditAsync(adminId, adminName, "VIEW_COMMISSION_CONFIG", "SETTINGS", configId, "CommissionConfig", null, "SUCCESS");
                return ApiResponseHelper.Success(config, "Commission configuration retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost("commission-configs")]
        public async Task<IActionResult> CreateCommissionConfig([FromBody] CreateCommissionConfigRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckPermissionAsync(adminId, "SYSTEM_CONFIG"))
                {
                    await LogAuditAsync(adminId, adminName, "CREATE_COMMISSION_CONFIG", "SETTINGS", null, "CommissionConfig", request, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("You don't have permission to create commission configurations."));
                }

                var dbHelper = new SqlDatabaseManager(_config);
                var settingsRepo = new SettingsRepository(dbHelper, _config);

                var newId = await settingsRepo.CreateCommissionConfigAsync(request, adminId);

                await LogAuditAsync(adminId, adminName, "CREATE_COMMISSION_CONFIG", "SETTINGS", newId, "CommissionConfig", request, "SUCCESS");
                return ApiResponseHelper.Success(new { ConfigId = newId }, "Commission configuration created successfully.");
            }
            catch (InvalidOperationException ex)
            {
                await LogAuditAsync(GetCurrentAdmin().adminId, GetCurrentAdmin().adminName, "CREATE_COMMISSION_CONFIG", "SETTINGS", null, "CommissionConfig", request, "FAILED", ex.Message);
                return StatusCode(400, ApiResponseHelper.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPut("commission-configs/{configId}")]
        public async Task<IActionResult> UpdateCommissionConfig(long configId, [FromBody] UpdateCommissionConfigRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckPermissionAsync(adminId, "SYSTEM_CONFIG"))
                {
                    await LogAuditAsync(adminId, adminName, "UPDATE_COMMISSION_CONFIG", "SETTINGS", configId, "CommissionConfig", request, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("You don't have permission to update commission configurations."));
                }

                // Ensure configId matches
                if (request.ConfigId != configId)
                {
                    return ApiResponseHelper.Failure("Configuration ID mismatch.");
                }

                var dbHelper = new SqlDatabaseManager(_config);
                var settingsRepo = new SettingsRepository(dbHelper, _config);

                var success = await settingsRepo.UpdateCommissionConfigAsync(request, adminId);

                if (success)
                {
                    await LogAuditAsync(adminId, adminName, "UPDATE_COMMISSION_CONFIG", "SETTINGS", configId, "CommissionConfig", request, "SUCCESS");
                    return ApiResponseHelper.Success(null, "Commission configuration updated successfully.");
                }

                return ApiResponseHelper.Failure("Failed to update commission configuration.");
            }
            catch (InvalidOperationException ex)
            {
                await LogAuditAsync(GetCurrentAdmin().adminId, GetCurrentAdmin().adminName, "UPDATE_COMMISSION_CONFIG", "SETTINGS", configId, "CommissionConfig", request, "FAILED", ex.Message);
                return StatusCode(400, ApiResponseHelper.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpDelete("commission-configs/{configId}")]
        public async Task<IActionResult> DeleteCommissionConfig(long configId)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckPermissionAsync(adminId, "SYSTEM_CONFIG"))
                {
                    await LogAuditAsync(adminId, adminName, "DELETE_COMMISSION_CONFIG", "SETTINGS", configId, "CommissionConfig", null, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("You don't have permission to delete commission configurations."));
                }

                var dbHelper = new SqlDatabaseManager(_config);
                var settingsRepo = new SettingsRepository(dbHelper, _config);

                var success = await settingsRepo.DeleteCommissionConfigAsync(configId);

                if (success)
                {
                    await LogAuditAsync(adminId, adminName, "DELETE_COMMISSION_CONFIG", "SETTINGS", configId, "CommissionConfig", null, "SUCCESS");
                    return ApiResponseHelper.Success(null, "Commission configuration deleted successfully.");
                }

                return ApiResponseHelper.Failure("Failed to delete commission configuration.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        // =============================================
        // EMAIL TEMPLATE ENDPOINTS
        // =============================================

        [HttpGet("email-templates")]
        public async Task<IActionResult> GetEmailTemplates([FromQuery] EmailTemplateListRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckPermissionAsync(adminId, "SYSTEM_CONFIG"))
                {
                    await LogAuditAsync(adminId, adminName, "VIEW_EMAIL_TEMPLATES", "SETTINGS", null, null, null, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("You don't have permission to view email templates."));
                }

                var dbHelper = new SqlDatabaseManager(_config);
                var settingsRepo = new SettingsRepository(dbHelper, _config);

                var result = await settingsRepo.GetEmailTemplatesAsync(request);

                await LogAuditAsync(adminId, adminName, "VIEW_EMAIL_TEMPLATES", "SETTINGS", null, null, request, "SUCCESS");
                return ApiResponseHelper.Success(result, "Email templates retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("email-templates/{templateId}")]
        public async Task<IActionResult> GetEmailTemplateById(long templateId)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckPermissionAsync(adminId, "SYSTEM_CONFIG"))
                {
                    await LogAuditAsync(adminId, adminName, "VIEW_EMAIL_TEMPLATE", "SETTINGS", templateId, "EmailTemplate", null, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("You don't have permission to view email templates."));
                }

                var dbHelper = new SqlDatabaseManager(_config);
                var settingsRepo = new SettingsRepository(dbHelper, _config);

                var template = await settingsRepo.GetEmailTemplateByIdAsync(templateId);

                if (template == null)
                {
                    return ApiResponseHelper.Failure("Email template not found.");
                }

                await LogAuditAsync(adminId, adminName, "VIEW_EMAIL_TEMPLATE", "SETTINGS", templateId, "EmailTemplate", null, "SUCCESS");
                return ApiResponseHelper.Success(template, "Email template retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPut("email-templates/{templateId}")]
        public async Task<IActionResult> UpdateEmailTemplate(long templateId, [FromBody] UpdateEmailTemplateRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckPermissionAsync(adminId, "SYSTEM_CONFIG"))
                {
                    await LogAuditAsync(adminId, adminName, "UPDATE_EMAIL_TEMPLATE", "SETTINGS", templateId, "EmailTemplate", request, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("You don't have permission to update email templates."));
                }

                // Ensure templateId matches
                if (request.TemplateId != templateId)
                {
                    return ApiResponseHelper.Failure("Template ID mismatch.");
                }

                var dbHelper = new SqlDatabaseManager(_config);
                var settingsRepo = new SettingsRepository(dbHelper, _config);

                var success = await settingsRepo.UpdateEmailTemplateAsync(request, adminId, adminName);

                if (success)
                {
                    await LogAuditAsync(adminId, adminName, "UPDATE_EMAIL_TEMPLATE", "SETTINGS", templateId, "EmailTemplate", request, "SUCCESS");
                    return ApiResponseHelper.Success(null, "Email template updated successfully.");
                }

                return ApiResponseHelper.Failure("Failed to update email template.");
            }
            catch (InvalidOperationException ex)
            {
                await LogAuditAsync(GetCurrentAdmin().adminId, GetCurrentAdmin().adminName, "UPDATE_EMAIL_TEMPLATE", "SETTINGS", templateId, "EmailTemplate", request, "FAILED", ex.Message);
                return StatusCode(400, ApiResponseHelper.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost("email-templates/preview")]
        public async Task<IActionResult> PreviewTemplate([FromBody] TemplatePreviewRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckPermissionAsync(adminId, "SYSTEM_CONFIG"))
                {
                    await LogAuditAsync(adminId, adminName, "PREVIEW_EMAIL_TEMPLATE", "SETTINGS", null, "EmailTemplate", request, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("You don't have permission to preview email templates."));
                }

                var dbHelper = new SqlDatabaseManager(_config);
                var settingsRepo = new SettingsRepository(dbHelper, _config);

                var result = await settingsRepo.PreviewTemplateAsync(request);

                await LogAuditAsync(adminId, adminName, "PREVIEW_EMAIL_TEMPLATE", "SETTINGS", request.TemplateId, "EmailTemplate", request, "SUCCESS");
                return ApiResponseHelper.Success(result, "Template preview generated successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(400, ApiResponseHelper.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("email-templates/{templateCode}/variables")]
        public async Task<IActionResult> GetTemplateVariables(string templateCode)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckPermissionAsync(adminId, "SYSTEM_CONFIG"))
                {
                    await LogAuditAsync(adminId, adminName, "VIEW_TEMPLATE_VARIABLES", "SETTINGS", null, "TemplateVariables", templateCode, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("You don't have permission to view template variables."));
                }

                var dbHelper = new SqlDatabaseManager(_config);
                var settingsRepo = new SettingsRepository(dbHelper, _config);

                var result = await settingsRepo.GetTemplateVariablesAsync(templateCode);

                await LogAuditAsync(adminId, adminName, "VIEW_TEMPLATE_VARIABLES", "SETTINGS", null, "TemplateVariables", templateCode, "SUCCESS");
                return ApiResponseHelper.Success(result, "Template variables retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost("email-templates/{templateId}/send-test")]
        public async Task<IActionResult> SendTestEmail(long templateId, [FromBody] Dictionary<string, string> request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckPermissionAsync(adminId, "SYSTEM_CONFIG"))
                {
                    await LogAuditAsync(adminId, adminName, "SEND_TEST_EMAIL", "SETTINGS", templateId, "EmailTemplate", request, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("You don't have permission to send test emails."));
                }

                if (!request.ContainsKey("toEmail"))
                {
                    return ApiResponseHelper.Failure("Email address is required.");
                }

                var toEmail = request["toEmail"];
                request.Remove("toEmail");

                var dbHelper = new SqlDatabaseManager(_config);
                var settingsRepo = new SettingsRepository(dbHelper, _config);

                var success = await settingsRepo.SendTestEmailAsync(templateId, toEmail, request);

                if (success)
                {
                    await LogAuditAsync(adminId, adminName, "SEND_TEST_EMAIL", "SETTINGS", templateId, "EmailTemplate", new { toEmail }, "SUCCESS");
                    return ApiResponseHelper.Success(null, "Test email sent successfully.");
                }

                return ApiResponseHelper.Failure("Failed to send test email.");
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(400, ApiResponseHelper.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }
    }
}
