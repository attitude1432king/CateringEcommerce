using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Models.Admin;

namespace CateringEcommerce.Domain.Interfaces.Admin
{
    public interface ISettingsRepository
    {
        // =============================================
        // System Settings Methods
        // =============================================

        /// <summary>
        /// Get settings with pagination and filters
        /// </summary>
        Task<SettingsListResponse> GetSettingsAsync(SettingsListRequest request);

        /// <summary>
        /// Get a single setting by key
        /// </summary>
        Task<SystemSettingItem> GetSettingByKeyAsync(string settingKey);

        /// <summary>
        /// Get a single setting by ID
        /// </summary>
        Task<SystemSettingItem> GetSettingByIdAsync(long settingId);

        /// <summary>
        /// Update a setting value with history tracking
        /// </summary>
        Task<bool> UpdateSettingAsync(UpdateSettingRequest request, long adminId, string adminName, string ipAddress);

        /// <summary>
        /// Get change history for a setting
        /// </summary>
        Task<SettingHistoryListResponse> GetSettingHistoryAsync(long settingId, int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// Get all settings by category
        /// </summary>
        Task<List<SystemSettingItem>> GetSettingsByCategoryAsync(string category);

        /// <summary>
        /// Validate setting value against regex and constraints
        /// </summary>
        Task<SettingValidationResult> ValidateSettingValueAsync(long settingId, string value);

        /// <summary>
        /// Get all settings as key-value pairs (for caching)
        /// </summary>
        Task<Dictionary<string, string>> GetAllSettingsKeyValueAsync();

        /// <summary>
        /// Export settings to JSON
        /// </summary>
        Task<List<SettingsExportItem>> ExportSettingsAsync(SettingsExportRequest request);

        /// <summary>
        /// Import settings from JSON
        /// </summary>
        Task<SettingsImportResult> ImportSettingsAsync(SettingsImportRequest request, long adminId);

        // =============================================
        // Commission Configuration Methods
        // =============================================

        /// <summary>
        /// Get commission configs with pagination and filters
        /// </summary>
        Task<CommissionListResponse> GetCommissionConfigsAsync(CommissionListRequest request);

        /// <summary>
        /// Get a single commission config by ID
        /// </summary>
        Task<CommissionConfigItem> GetCommissionConfigByIdAsync(long configId);

        /// <summary>
        /// Create a new commission configuration
        /// </summary>
        Task<long> CreateCommissionConfigAsync(CreateCommissionConfigRequest request, long adminId);

        /// <summary>
        /// Update an existing commission configuration
        /// </summary>
        Task<bool> UpdateCommissionConfigAsync(UpdateCommissionConfigRequest request, long adminId);

        /// <summary>
        /// Delete a commission configuration
        /// </summary>
        Task<bool> DeleteCommissionConfigAsync(long configId);

        /// <summary>
        /// Get applicable commission rate for a catering owner and order value
        /// Priority: CATERING_SPECIFIC > TIERED > GLOBAL
        /// </summary>
        Task<ApplicableCommissionResult> GetApplicableCommissionRateAsync(long? cateringOwnerId, decimal orderValue, DateTime orderDate);

        /// <summary>
        /// Check if a commission config overlaps with existing configs
        /// </summary>
        Task<bool> HasCommissionConfigOverlapAsync(string configType, long? cateringOwnerId, DateTime effectiveFrom, DateTime? effectiveTo, long? excludeConfigId = null);

        // =============================================
        // Email Template Methods
        // =============================================

        /// <summary>
        /// Get email templates with pagination and filters
        /// </summary>
        Task<EmailTemplateListResponse> GetEmailTemplatesAsync(EmailTemplateListRequest request);

        /// <summary>
        /// Get a single email template by ID
        /// </summary>
        Task<EmailTemplateItem> GetEmailTemplateByIdAsync(long templateId);

        /// <summary>
        /// Get a single email template by code
        /// </summary>
        Task<EmailTemplateItem> GetEmailTemplateByCodeAsync(string templateCode);

        /// <summary>
        /// Update an existing email template (increments version)
        /// </summary>
        Task<bool> UpdateEmailTemplateAsync(UpdateEmailTemplateRequest request, long adminId, string adminName);

        /// <summary>
        /// Preview template with sample data (does not save)
        /// </summary>
        Task<TemplatePreviewResponse> PreviewTemplateAsync(TemplatePreviewRequest request);

        /// <summary>
        /// Get available variables for a template
        /// </summary>
        Task<TemplateVariablesResponse> GetTemplateVariablesAsync(string templateCode);

        /// <summary>
        /// Test send email using template
        /// </summary>
        Task<bool> SendTestEmailAsync(long templateId, string toEmail, Dictionary<string, string> sampleData);
    }
}
