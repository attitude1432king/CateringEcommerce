/**
 * Settings API Service
 * API calls for system settings, commission configs, and email templates
 */
import { apiCall } from './apiUtils' // P3 FIX: Use consolidated apiUtils

// ============================================
// System Settings APIs
// ============================================
export const systemSettingsApi = {
    /**
     * Get settings with pagination and filters
     */
    getSettings: async (filters = {}) => {
        const params = {};

        Object.entries(filters).forEach(([key, value]) => {
            if (value !== undefined && value !== null && value !== '') {
                params[key] = value;
            }
        });

        return apiCall('/admin/settings', 'GET', null, params);
    },


    /**
     * Get a single setting by key
     */
    getSettingByKey: async (settingKey) =>  apiCall(`/admin/settings/${settingKey}`),

    /**
     * Update a setting value
     */
    updateSetting: async (settingId, settingValue, changeReason) => {
        const payload = {
            settingId,
            settingValue,
            changeReason
        };

        return apiCall(`/admin/settings/${settingId}`, 'PUT', payload);
    },


    /**
     * Get setting change history
     */
    getSettingHistory: async (settingId, pageNumber = 1, pageSize = 50) => apiCall(`/admin/settings/${settingId}/history?pageNumber=${pageNumber}&pageSize=${pageSize}`),

    /**
     * Export settings to JSON
     */
    exportSettings: async (category = null, includeSensitive = false) => {
        return apiCall('/admin/settings/export', 'POST', {
            category,
            includeSensitive
        });
    },

    /**
     * Import settings from JSON
     */
    importSettings: async (settings, overwriteExisting = false) => {
        return apiCall('/admin/settings/import', 'POST',{
                settings,
                overwriteExisting,
            });
    },
};

// ============================================
// Commission Configuration APIs
// ============================================
export const commissionConfigApi = {
    /**
     * Get commission configs with pagination and filters
     */
    getCommissionConfigs: async (filters = {}) => {
        const params = {};

        Object.entries(filters).forEach(([key, value]) => {
            if (value !== undefined && value !== null && value !== '') {
                params[key] = value;
            }
        });

        return apiCall(`/admin/settings/commission-configs`, 'GET', null, params);
    },

    /**
     * Get a single commission config by ID
     */
    getCommissionConfigById: async (configId) => apiCall(`/admin/settings/commission-configs/${configId}`),

    /**
     * Create a new commission configuration
     */
    createCommissionConfig: async (config) => apiCall('/admin/settings/commission-configs', 'POST', config),

    /**
     * Update an existing commission configuration
     */
    updateCommissionConfig: async (configId, config) => {
        return apiCall(`/admin/settings/commission-configs/${configId}`, 'PUT', {
            ...config,
            configId,
        });
    },

    /**
     * Delete a commission configuration
     */
    deleteCommissionConfig: async (configId) => apiCall(`/admin/settings/commission-configs/${configId}`, 'DELETE'),
};

// ============================================
// Email Template APIs
// ============================================
export const emailTemplateApi = {
    /**
     * Get email templates with pagination and filters
     */
    getEmailTemplates: async (filters = {}) => {
        const params = {};

        Object.entries(filters).forEach(([key, value]) => {
            if (value !== undefined && value !== null && value !== '') {
                params[key] = value;
            }
        });

        return apiCall(`/admin/settings/email-templates`, 'GET', null, params);
    },

    /**
     * Get a single email template by ID
     */
    getEmailTemplateById: async (templateId) => apiCall(`/admin/settings/email-templates/${templateId}`),

    /**
     * Create a new email template
     */
    createEmailTemplate: async (templateData) => {
        return apiCall('/admin/settings/email-templates', 'POST', templateData);
    },

    /**
     * Update an existing email template
     */
    updateEmailTemplate: async (templateId, data) => {
        return apiCall(`/admin/settings/email-templates/${templateId}`, 'PUT', {
            templateId,
            ...data,
        });
    },

    /**
     * Preview template with sample data
     */
    previewTemplate: async (templateId, templateCode, subject, body, sampleData) => {
        return apiCall('/admin/settings/email-templates/preview', 'POST', {
                templateId,
                templateCode,
                subject,
                body,
                sampleData,
        });
    },

    /**
     * Get available variables for a template
     */
    getTemplateVariables: async (templateCode) => apiCall(`/admin/settings/email-templates/${templateCode}/variables`),

    /**
     * Send test email using template
     */
    sendTestEmail: async (templateId, toEmail, sampleData = {}) => {
        return apiCall(`/admin/settings/email-templates/${templateId}/send-test`, 'POST', {
                toEmail,
                ...sampleData,
        });
    },
};

// ============================================
// Public Settings API (no auth required)
// ============================================
export const publicSettingsApi = {
    getPublicSettings: async () => apiCall('/app-settings', 'GET'),
    refreshSettings: async () => apiCall('/app-settings/refresh', 'POST'),
};

// Default export with all APIs
export default {
    systemSettings: systemSettingsApi,
    commissionConfig: commissionConfigApi,
    emailTemplate: emailTemplateApi,
    publicSettings: publicSettingsApi,
};
