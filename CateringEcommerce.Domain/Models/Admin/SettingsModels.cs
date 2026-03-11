using System;
using System.Collections.Generic;

namespace CateringEcommerce.Domain.Models.Admin
{
    // =============================================
    // System Settings Models
    // =============================================

    public class SystemSettingItem
    {
        public long SettingId { get; set; }
        public string SettingKey { get; set; }
        public string SettingValue { get; set; }
        public string Category { get; set; } // SYSTEM, EMAIL, PAYMENT, BUSINESS, NOTIFICATION
        public string ValueType { get; set; } // STRING, NUMBER, BOOLEAN, JSON, ENCRYPTED
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsSensitive { get; set; }
        public bool IsReadOnly { get; set; }
        public int DisplayOrder { get; set; }
        public string ValidationRegex { get; set; }
        public string DefaultValue { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public long? ModifiedBy { get; set; }
    }

    public class SettingsListRequest
    {
        public string? Category { get; set; } // Optional filter
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public string SortBy { get; set; } = "DisplayOrder";
        public string SortOrder { get; set; } = "ASC";
    }

    public class SettingsListResponse
    {
        public List<SystemSettingItem> Settings { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class UpdateSettingRequest
    {
        public long SettingId { get; set; }
        public string SettingValue { get; set; }
        public string ChangeReason { get; set; }
    }

    public class SettingHistoryItem
    {
        public long HistoryId { get; set; }
        public long SettingId { get; set; }
        public string SettingKey { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public long ChangedBy { get; set; }
        public string ChangedByName { get; set; }
        public DateTime ChangeDate { get; set; }
        public string ChangeReason { get; set; }
        public string IpAddress { get; set; }
    }

    public class SettingHistoryListResponse
    {
        public List<SettingHistoryItem> History { get; set; }
        public int TotalCount { get; set; }
    }

    // =============================================
    // Commission Configuration Models
    // =============================================

    public class CommissionConfigItem
    {
        public long ConfigId { get; set; }
        public string ConfigName { get; set; }
        public string ConfigType { get; set; } // GLOBAL, CATERING_SPECIFIC, TIERED
        public long? CateringOwnerId { get; set; }
        public string CateringOwnerName { get; set; } // For display
        public string BusinessName { get; set; } // For display
        public decimal CommissionRate { get; set; }
        public decimal FixedFee { get; set; }
        public decimal? MinOrderValue { get; set; }
        public decimal? MaxOrderValue { get; set; }
        public bool IsActive { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public DateTime CreatedDate { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public long? ModifiedBy { get; set; }
    }

    public class CommissionListRequest
    {
        public string? ConfigType { get; set; } // Optional filter
        public long? CateringOwnerId { get; set; } // Optional filter
        public bool? IsActive { get; set; }
        public DateTime? EffectiveDate { get; set; } // Filter by effective date range
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public string SortBy { get; set; } = "CreatedDate";
        public string SortOrder { get; set; } = "DESC";
    }

    public class CommissionListResponse
    {
        public List<CommissionConfigItem> Configs { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class CreateCommissionConfigRequest
    {
        public string ConfigName { get; set; }
        public string ConfigType { get; set; } // GLOBAL, CATERING_SPECIFIC, TIERED
        public long? CateringOwnerId { get; set; } // Required if ConfigType = CATERING_SPECIFIC
        public decimal CommissionRate { get; set; }
        public decimal FixedFee { get; set; }
        public decimal? MinOrderValue { get; set; } // For TIERED type
        public decimal? MaxOrderValue { get; set; } // For TIERED type
        public bool IsActive { get; set; } = true;
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }

    public class UpdateCommissionConfigRequest
    {
        public long ConfigId { get; set; }
        public string ConfigName { get; set; }
        public decimal CommissionRate { get; set; }
        public decimal FixedFee { get; set; }
        public decimal? MinOrderValue { get; set; }
        public decimal? MaxOrderValue { get; set; }
        public bool IsActive { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }

    public class ApplicableCommissionResult
    {
        public long? ConfigId { get; set; }
        public string ConfigName { get; set; }
        public string ConfigType { get; set; }
        public decimal CommissionRate { get; set; }
        public decimal FixedFee { get; set; }
        public decimal CalculatedCommission { get; set; }
    }

    // =============================================
    // Email Template Models
    // =============================================

    public class EmailTemplateItem
    {
        public long TemplateId { get; set; }
        public string TemplateCode { get; set; }
        public string TemplateName { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
        public string Channel { get; set; }
        public string Category { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public int Version { get; set; }
        public bool IsActive { get; set; }
        public int UsageCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public long? ModifiedBy { get; set; }
        public string ModifiedByName { get; set; }
    }

    public class EmailTemplateListRequest
    {
        public string? Category { get; set; } // Optional filter: USER, OWNER, ADMIN
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public string SortBy { get; set; } = "TemplateName";
        public string SortOrder { get; set; } = "ASC";
    }

    public class EmailTemplateListResponse
    {
        public List<EmailTemplateItem> Templates { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class CreateEmailTemplateRequest
    {
        public string TemplateCode { get; set; }
        public string TemplateName { get; set; }
        public string? Description { get; set; }
        public string Language { get; set; } = "en";
        public string Channel { get; set; } = "EMAIL";
        public string Category { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateEmailTemplateRequest
    {
        public long TemplateId { get; set; }
        public string? TemplateName { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public bool IsActive { get; set; }
        public string? ChangeReason { get; set; }
    }

    public class TemplatePreviewRequest
    {
        public long? TemplateId { get; set; }
        public string? TemplateCode { get; set; }
        public string? Subject { get; set; } // Optional - for preview without saving
        public string? Body { get; set; } // Optional - for preview without saving
        public Dictionary<string, string>? SampleData { get; set; } // Variable values for preview
    }

    public class TemplatePreviewResponse
    {
        public string RenderedSubject { get; set; }
        public string RenderedBody { get; set; }
        public List<string> MissingVariables { get; set; } // Variables in template but not in sample data
    }

    public class TemplateVariableItem
    {
        public long VariableId { get; set; }
        public string? TemplateCode { get; set; }
        public string? VariableName { get; set; }
        public string? VariableKey { get; set; } // e.g., {{ customer_name }}
        public string? Description { get; set; }
        public string? ExampleValue { get; set; }
    }

    public class TemplateVariablesResponse
    {
        public string TemplateCode { get; set; }
        public List<TemplateVariableItem> Variables { get; set; }
    }

    // =============================================
    // Settings Cache Models
    // =============================================

    public class SettingsCacheItem
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string ValueType { get; set; }
        public DateTime CachedAt { get; set; }
    }

    public class SettingsCacheInvalidationRequest
    {
        public string SettingKey { get; set; } // If null, invalidate all
        public string Category { get; set; } // If null, invalidate all
    }

    // =============================================
    // Settings Export/Import Models
    // =============================================

    public class SettingsExportRequest
    {
        public string Category { get; set; } // Optional - export specific category
        public bool IncludeSensitive { get; set; } = false;
    }

    public class SettingsExportItem
    {
        public string SettingKey { get; set; }
        public string SettingValue { get; set; }
        public string Category { get; set; }
        public string ValueType { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
    }

    public class SettingsImportRequest
    {
        public List<SettingsExportItem> Settings { get; set; }
        public bool OverwriteExisting { get; set; } = false;
    }

    public class SettingsImportResult
    {
        public int TotalSettings { get; set; }
        public int ImportedCount { get; set; }
        public int SkippedCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; }
    }

    // =============================================
    // Validation Result Model
    // =============================================

    public class SettingValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; }
    }
}
