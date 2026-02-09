/**
 * Admin API Service
 * Centralized API calls for admin module
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
    getAll: (params) => {
        const queryString = new URLSearchParams(params).toString();
        return apiCall(`/caterings?${queryString}`);
    },

    getById: (id) => apiCall(`/caterings/${id}`),

    updateStatus: (id, status, reason) =>
        apiCall(`/caterings/${id}/status`, {
            method: 'PUT',
            body: JSON.stringify({ status, reason }),
        }),

    delete: (id) =>
        apiCall(`/caterings/${id}`, {
            method: 'DELETE',
        }),
};

// ============================================
// User Management APIs
// ============================================
export const userApi = {
    getAll: (params) => {
        const queryString = new URLSearchParams(params).toString();
        return apiCall(`/users?${queryString}`);
    },

    getById: (id) => apiCall(`/users/${id}`),

    updateStatus: (id, isBlocked, reason) =>
        apiCall(`/users/${id}/status`, {
            method: 'PUT',
            body: JSON.stringify({ isBlocked, reason }),
        }),
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
        return apiCall(`/admins?${queryString}`);
    },

    // Get admin by ID
    getAdminById: (id) => apiCall(`/admins/${id}`),

    // Create new admin
    createAdmin: (data) =>
        apiCall('/admins', {
            method: 'POST',
            body: JSON.stringify(data),
        }),

    // Update admin information
    updateAdmin: (id, data) =>
        apiCall(`/admins/${id}`, {
            method: 'PUT',
            body: JSON.stringify(data),
        }),

    // Update admin status (activate/deactivate)
    updateAdminStatus: (id, isActive) =>
        apiCall(`/admins/${id}/status`, {
            method: 'PUT',
            body: JSON.stringify({ adminId: id, isActive }),
        }),

    // Assign role to admin
    assignRole: (id, roleId) =>
        apiCall(`/admins/${id}/role`, {
            method: 'PUT',
            body: JSON.stringify({ adminId: id, roleId }),
        }),

    // Reset admin password
    resetPassword: (id, newPasswordHash, forcePasswordReset = true) =>
        apiCall(`/admins/${id}/reset-password`, {
            method: 'POST',
            body: JSON.stringify({ adminId: id, newPasswordHash, forcePasswordReset }),
        }),

    // Delete admin (soft delete)
    deleteAdmin: (id) =>
        apiCall(`/admins/${id}`, {
            method: 'DELETE',
        }),

    // Check if username exists
    checkUsername: (username, excludeAdminId = null) => {
        const params = new URLSearchParams({ username });
        if (excludeAdminId) params.append('excludeAdminId', excludeAdminId);
        return apiCall(`/admins/check-username?${params.toString()}`);
    },

    // Check if email exists
    checkEmail: (email, excludeAdminId = null) => {
        const params = new URLSearchParams({ email });
        if (excludeAdminId) params.append('excludeAdminId', excludeAdminId);
        return apiCall(`/admins/check-email?${params.toString()}`);
    },
};

// ============================================
// Role Management APIs
// ============================================
export const roleManagementApi = {
    // Get all roles
    getRoles: () => apiCall('/roles'),

    // Get role by ID with permissions
    getRoleById: (id) => apiCall(`/roles/${id}`),

    // Create new role
    createRole: (data) =>
        apiCall('/roles', {
            method: 'POST',
            body: JSON.stringify(data),
        }),

    // Update role
    updateRole: (id, data) =>
        apiCall(`/roles/${id}`, {
            method: 'PUT',
            body: JSON.stringify(data),
        }),

    // Delete role
    deleteRole: (id) =>
        apiCall(`/roles/${id}`, {
            method: 'DELETE',
        }),

    // Get all permissions grouped by module
    getPermissions: () => apiCall('/permissions'),
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
};
