/**
 * Admin API Service
 * Centralized API calls for admin module
 * 
 * SECURITY NOTE: 
 * - Authentication token is stored in httpOnly cookie (not accessible to JavaScript)
 * - Browser automatically sends httpOnly cookies with credentials: 'include'
 * - Server extracts token from cookie and validates it
 * - No Authorization header needed for admin endpoints
 */

import { apiCall } from './apiUtils'; // P3 FIX: Use consolidated apiUtils

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '');

// ============================================
// Authentication APIs
// ============================================
export const authApi = {
    login: async (username, password) => {
        const response = await fetch(`${API_BASE_URL}/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username, password }),
        });
        return response.json();
    },

    getCurrentAdmin: () => apiCall('/auth/me'),

    logout: () => apiCall('/auth/logout', { method: 'POST' }),

    // Change temporary password forced on first login for admin-created accounts
    changeTempPassword: (dto) =>
        apiCall('/admin/auth/change-temporary-password', 'POST', dto),
};

// ============================================
// Dashboard APIs
// ============================================
export const dashboardApi = {
    getMetrics: () => apiCall('/admin/dashboard/metrics'),
};

// ============================================
// Catering Management APIs
// ============================================
export const cateringApi = {
    getAll: (params) => apiCall('/admin/caterings', 'GET', null, params),

    getById: (id) => apiCall(`/admin/caterings/${id}`),

    updateStatus: (id, status, reason) =>
        apiCall(`/admin/caterings/${id}/status`, 'PUT', { status, reason }),

    delete: (id) => apiCall(`/admin/caterings/${id}`, 'DELETE'),

    restore: (id) => apiCall(`/admin/caterings/${id}/restore`, 'POST'),

    toggleFeatured: (id, isFeatured) =>
        apiCall(`/admin/caterings/${id}/featured`, 'PATCH', { isFeatured }),

    exportCaterings: (params) => {
        const queryString = new URLSearchParams(params).toString();
        const url = `${API_BASE_URL}/api/admin/caterings/export?${queryString}`;
        return fetch(url, { credentials: 'include' });
    },
};

// ============================================
// Customer User Management APIs
// ============================================
export const userApi = {
    getAll: (params) => apiCall('/admin/users', 'GET', null, params),

    getById: (id) => apiCall(`/admin/users/${id}`),

    updateStatus: (id, isBlocked, reason) =>
        apiCall(`/admin/users/${id}/status`, 'PUT', { isBlocked, reason }),

    deleteUser: (id) => apiCall(`/admin/users/${id}`, 'DELETE'),

    restoreUser: (id) => apiCall(`/admin/users/${id}/restore`, 'POST'),

    exportUsers: (params) => {
        const queryString = new URLSearchParams(params).toString();
        const url = `${API_BASE_URL}/api/admin/users/export?${queryString}`;
        return fetch(url, { credentials: 'include' });
    },
};

// ============================================
// Location APIs (for filters)
// ============================================
export const locationApi = {
    getStates: () => apiCall('/Common/Locations/states'),
    getCities: (stateId) => apiCall(`/Common/Locations/cities/${stateId}`),
};

// ============================================
// Earnings & Revenue APIs
// ============================================
export const earningsApi = {
    getSummary: () => apiCall('/earnings/summary'),

    getByDate: (params) => {
        const queryString = new URLSearchParams(params).toString();
        return apiCall(`/earnings/by-date?${queryString}`);
    },

    getByCatering: (params) => {
        const queryString = new URLSearchParams(params).toString();
        return apiCall(`/earnings/by-catering?${queryString}`);
    },

    getMonthlyReport: (year) =>
        apiCall(`/earnings/monthly-report?year=${year}`),
};

// ============================================
// Review Management APIs
// ============================================
export const reviewApi = {
    getAll: (params) => {
        const queryString = new URLSearchParams(params).toString();
        return apiCall(`/reviews?${queryString}`);
    },

    getById: (id) => apiCall(`/reviews/${id}`),

    updateVisibility: (id, isHidden, reason) =>
        apiCall(`/reviews/${id}/hide`, {
            method: 'PUT',
            body: JSON.stringify({ isHidden, reason }),
        }),

    delete: (id) =>
        apiCall(`/reviews/${id}`, {
            method: 'DELETE',
        }),
};

// ============================================
// Admin User Management APIs
// ============================================
export const adminManagementApi = {
    // Get all admins with pagination and filters
    getAdmins: (params) => {
        const queryString = new URLSearchParams(params).toString();
        return apiCall(`/admin/admins?${queryString}`);
    },

    // Get admin by ID
    getAdminById: (id) => apiCall(`/admin/admins/${id}`),


    // Create new admin
    createAdmin: (data) => apiCall('/admin/admins', 'POST', data),


    // Update admin information
    updateAdmin: (id, data) => apiCall(`/admin/admins/${id}`, 'PUT', data),


    // Update admin status (activate/deactivate)
    updateAdminStatus: (id, isActive) => apiCall(`/admin/admins/${id}/status`, 'PUT', { adminId: id, isActive }),


    // Assign role to admin
    assignRole: (id, data) => apiCall(`/admin/admins/${id}/role`, 'PUT', data),


    // Reset admin password
    resetPassword: (id, newPasswordHash, forcePasswordReset = true) => apiCall(`/admin/admins/${id}/reset-password`, 'POST',{ adminId: id, newPasswordHash, forcePasswordReset }),


    // Delete admin (soft delete)
    deleteAdmin: (id) => apiCall(`/admin/admins/${id}`, 'DELETE'),

    // Check if username exists
    checkUsername: (username, excludeAdminId = null) => {
        const params = new URLSearchParams({ username });
        if (excludeAdminId) params.append('excludeAdminId', excludeAdminId);
        return apiCall(`/admin/admins/check-username?${params.toString()}`);
    },

    // Check if email exists
    checkEmail: (email, excludeAdminId = null) => {
        const params = new URLSearchParams({ email });
        if (excludeAdminId) params.append('excludeAdminId', excludeAdminId);
        return apiCall(`/admin/admins/check-email?${params.toString()}`);
    },
};

// ============================================
// Role Management APIs
// ============================================
export const roleManagementApi = {
    // Get all roles
    getRoles: () => apiCall('/admin/roles'),

    // Get role by ID with permissions
    getRoleById: (id) => apiCall(`/admin/roles/${id}`),

    // Create new role
    createRole: (data) => apiCall('/admin/roles', 'POST', data),

    // Update role
    updateRole: (id, data) => apiCall(`/admin/roles/${id}`, 'PUT', data),

    // Delete role
    deleteRole: (id) => (`/admin/roles/${id}`, 'DELETE'),

    // Get all permissions grouped by module
    getPermissions: () => apiCall('/admin/permissions'),
};

// ============================================
// Partner Approval APIs (NEW - Enum-based)
// ============================================
export const partnerApprovalApi = {
    // Get pending partner requests
    getPendingRequests: (params) => {
        const queryString = new URLSearchParams(params).toString();
        return apiCall(`/admin/partners/pending?${queryString}`);
    },

    // Get partner detail
    getPartnerDetail: (partnerId) =>
        apiCall(`/admin/partners/${partnerId}/registration-detail`),

    // Approve partner
    approvePartner: (partnerId, data) =>
        apiCall(`/admin/partners/${partnerId}/approve`, {
            method: 'POST',
            body: JSON.stringify(data),
        }),

    // Reject partner
    rejectPartner: (partnerId, data) =>
        apiCall(`/admin/partners/${partnerId}/reject`, {
            method: 'POST',
            body: JSON.stringify(data),
        }),

    // Update priority
    updatePriority: (partnerId, priorityId) =>
        apiCall(`/admin/partners/${partnerId}/priority`, {
            method: 'PUT',
            body: JSON.stringify({ priorityId }),
        }),

    // Get approval status enums
    getApprovalStatuses: () =>
        apiCall('/admin/partners/enums/approval-statuses'),

    // Get priority enums
    getPriorities: () =>
        apiCall('/admin/partners/enums/priorities'),
};

// ============================================
// Supervisor Management APIs
// ============================================
export const supervisorManagementApi = {
    // Tab 1: Pending Supervisor Requests
    getRegistrations: (params) => apiCall('/admin/supervisors/registrations', 'GET', null, params),

    updateStatus: (id, status, reason) =>
        apiCall(`/admin/supervisors/${id}/status`, 'PUT', { status, reason }),

    // Tab 2: Approved Supervisors
    getActiveSupervisors: (params) => apiCall('/admin/supervisors/active', 'GET', null, params),

    blockSupervisor: (id, reason) =>
        apiCall(`/admin/supervisors/${id}/block`, 'PUT', { reason }),

    unblockSupervisor: (id) =>
        apiCall(`/admin/supervisors/${id}/unblock`, 'PUT'),

    deleteSupervisor: (id) => apiCall(`/admin/supervisors/${id}`, 'DELETE'),

    restoreSupervisor: (id) => apiCall(`/admin/supervisors/${id}/restore`, 'POST'),

    getSupervisorDetails: (id) => apiCall(`/admin/supervisors/${id}`),

    exportSupervisors: (params) => {
        const queryString = new URLSearchParams(params).toString();
        const url = `${API_BASE_URL}/api/admin/supervisors/export?${queryString}`;
        return fetch(url, { credentials: 'include' });
    },
};

// ============================================
// Global Search API
// ============================================
export const searchApi = {
    globalSearch: (query) =>
        apiCall(`/admin/search?q=${encodeURIComponent(query)}`),
};

// ============================================
// Logs API
// ============================================
export const logsApi = {
    getLogs: (params) => apiCall('/admin/logs', 'GET', null, params),
    getLogById: (id) => apiCall(`/admin/logs/${id}`),
};

// Export all APIs
export default {
    auth: authApi,
    dashboard: dashboardApi,
    caterings: cateringApi,
    users: userApi,
    earnings: earningsApi,
    reviews: reviewApi,
    adminManagement: adminManagementApi,
    roleManagement: roleManagementApi,
    partnerApproval: partnerApprovalApi,
    locations: locationApi,
    supervisorManagement: supervisorManagementApi,
    search: searchApi,
    logs: logsApi,
};
