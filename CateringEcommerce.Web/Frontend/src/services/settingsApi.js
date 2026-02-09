/**
 * Settings API Service
 * API calls for system settings, commission configs, and email templates
 */

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:44368';

// Helper function to get auth headers
const getAuthHeaders = () => {
    const token = localStorage.getItem('adminToken');
    return {
        'Content-Type': 'application/json',
        'Authorization': token ? `Bearer ${token}` : '',
    };
};

// Generic API call handler
const apiCall = async (endpoint, options = {}) => {
    try {
        const response = await fetch(`${API_BASE_URL}/api${endpoint}`, {
            ...options,
            headers: {
                ...getAuthHeaders(),
                ...options.headers,
            },
        });

        const data = await response.json();

        if (!response.ok) {
            throw new Error(data.message || 'API request failed');
        }

        return data;
    } catch (error) {
        console.error('API Error:', error);
        throw error;
    }
};

// ============================================
// System Settings APIs
// ============================================
export const systemSettingsApi = {
    /**
     * Get settings with pagination and filters
     */
    getSettings: async (filters = {}) => {
        const params = new URLSearchParams();
        if (filters.category) params.append('category', filters.category);
        if (filters.searchTerm) params.append('searchTerm', filters.searchTerm);
        if (filters.isActive !== undefined) params.append('isActive', filters.isActive);
        if (filters.pageNumber) params.append('pageNumber', filters.pageNumber);
        if (filters.pageSize) params.append('pageSize', filters.pageSize);
        if (filters.sortBy) params.append('sortBy', filters.sortBy);
        if (filters.sortOrder) params.append('sortOrder', filters.sortOrder);

        const queryString = params.toString();
        return apiCall(`/admin/settings${queryString ? `?${queryString}` : ''}`);
    },

    /**
     * Get a single setting by key
     */
    getSettingByKey: async (settingKey) => {
        return apiCall(`/admin/settings/${settingKey}`);
    },

    /**
     * Update a setting value
     */
    updateSetting: async (settingId, settingValue, changeReason) => {
        return apiCall(`/admin/settings/${settingId}`, {
            method: 'PUT',
            body: JSON.stringify({
                settingId,
                settingValue,
                changeReason,
            }),
        });
    },

    /**
     * Get setting change history
     */
    getSettingHistory: async (settingId, pageNumber = 1, pageSize = 50) => {
        return apiCall(`/admin/settings/${settingId}/history?pageNumber=${pageNumber}&pageSize=${pageSize}`);
    },

    /**
     * Export settings to JSON
     */
    exportSettings: async (category = null, includeSensitive = false) => {
        return apiCall('/admin/settings/export', {
            method: 'POST',
            body: JSON.stringify({
                category,
                includeSensitive,
            }),
        });
    },

    /**
     * Import settings from JSON
     */
    importSettings: async (settings, overwriteExisting = false) => {
        return apiCall('/admin/settings/import', {
            method: 'POST',
            body: JSON.stringify({
                settings,
                overwriteExisting,
            }),
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
        const params = new URLSearchParams();
        if (filters.configType) params.append('configType', filters.configType);
        if (filters.cateringOwnerId) params.append('cateringOwnerId', filters.cateringOwnerId);
        if (filters.isActive !== undefined) params.append('isActive', filters.isActive);
        if (filters.effectiveDate) params.append('effectiveDate', filters.effectiveDate);
        if (filters.pageNumber) params.append('pageNumber', filters.pageNumber);
        if (filters.pageSize) params.append('pageSize', filters.pageSize);
        if (filters.sortBy) params.append('sortBy', filters.sortBy);
        if (filters.sortOrder) params.append('sortOrder', filters.sortOrder);

        const queryString = params.toString();
        return apiCall(`/admin/settings/commission-configs${queryString ? `?${queryString}` : ''}`);
    },

    /**
     * Get a single commission config by ID
     */
    getCommissionConfigById: async (configId) => {
        return apiCall(`/admin/settings/commission-configs/${configId}`);
    },

    /**
     * Create a new commission configuration
     */
    createCommissionConfig: async (config) => {
        return apiCall('/admin/settings/commission-configs', {
            method: 'POST',
            body: JSON.stringify(config),
        });
    },

    /**
     * Update an existing commission configuration
     */
    updateCommissionConfig: async (configId, config) => {
        return apiCall(`/admin/settings/commission-configs/${configId}`, {
            method: 'PUT',
            body: JSON.stringify({
                ...config,
                configId,
            }),
        });
    },

    /**
     * Delete a commission configuration
     */
    deleteCommissionConfig: async (configId) => {
        return apiCall(`/admin/settings/commission-configs/${configId}`, {
            method: 'DELETE',
        });
    },
};

// ============================================
// Email Template APIs
// ============================================
export const emailTemplateApi = {
    /**
     * Get email templates with pagination and filters
     */
    getEmailTemplates: async (filters = {}) => {
        const params = new URLSearchParams();
        if (filters.category) params.append('category', filters.category);
        if (filters.searchTerm) params.append('searchTerm', filters.searchTerm);
        if (filters.isActive !== undefined) params.append('isActive', filters.isActive);
        if (filters.pageNumber) params.append('pageNumber', filters.pageNumber);
        if (filters.pageSize) params.append('pageSize', filters.pageSize);
        if (filters.sortBy) params.append('sortBy', filters.sortBy);
        if (filters.sortOrder) params.append('sortOrder', filters.sortOrder);

        const queryString = params.toString();
        return apiCall(`/admin/settings/email-templates${queryString ? `?${queryString}` : ''}`);
    },

    /**
     * Get a single email template by ID
     */
    getEmailTemplateById: async (templateId) => {
        return apiCall(`/admin/settings/email-templates/${templateId}`);
    },

    /**
     * Update an existing email template
     */
    updateEmailTemplate: async (templateId, subject, body, changeReason) => {
        return apiCall(`/admin/settings/email-templates/${templateId}`, {
            method: 'PUT',
            body: JSON.stringify({
                templateId,
                subject,
                body,
                changeReason,
            }),
        });
    },

    /**
     * Preview template with sample data
     */
    previewTemplate: async (templateId, templateCode, subject, body, sampleData) => {
        return apiCall('/admin/settings/email-templates/preview', {
            method: 'POST',
            body: JSON.stringify({
                templateId,
                templateCode,
                subject,
                body,
                sampleData,
            }),
        });
    },

    /**
     * Get available variables for a template
     */
    getTemplateVariables: async (templateCode) => {
        return apiCall(`/admin/settings/email-templates/${templateCode}/variables`);
    },

    /**
     * Send test email using template
     */
    sendTestEmail: async (templateId, toEmail, sampleData = {}) => {
        return apiCall(`/admin/settings/email-templates/${templateId}/send-test`, {
            method: 'POST',
            body: JSON.stringify({
                toEmail,
                ...sampleData,
            }),
        });
    },
};

// Default export with all APIs
export default {
    systemSettings: systemSettingsApi,
    commissionConfig: commissionConfigApi,
    emailTemplate: emailTemplateApi,
};
