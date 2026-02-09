/**
 * Analytics API Service
 * API calls for admin and partner analytics
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
// Admin Analytics APIs
// ============================================
export const adminAnalyticsApi = {
    /**
     * Get dashboard metrics with date range
     */
    getDashboardMetrics: async (fromDate = null, toDate = null) => {
        const params = new URLSearchParams();
        if (fromDate) params.append('fromDate', fromDate);
        if (toDate) params.append('toDate', toDate);

        const queryString = params.toString();
        return apiCall(`/admin/dashboard/v2/metrics${queryString ? `?${queryString}` : ''}`);
    },

    /**
     * Get revenue chart data
     */
    getRevenueChart: async (fromDate = null, toDate = null, granularity = 'day') => {
        const params = new URLSearchParams();
        if (fromDate) params.append('fromDate', fromDate);
        if (toDate) params.append('toDate', toDate);
        params.append('granularity', granularity);

        return apiCall(`/admin/dashboard/revenue-chart?${params.toString()}`);
    },

    /**
     * Get order analytics
     */
    getOrderAnalytics: async (fromDate = null, toDate = null) => {
        const params = new URLSearchParams();
        if (fromDate) params.append('fromDate', fromDate);
        if (toDate) params.append('toDate', toDate);

        const queryString = params.toString();
        return apiCall(`/admin/dashboard/order-analytics${queryString ? `?${queryString}` : ''}`);
    },

    /**
     * Get top performing partners
     */
    getTopPartners: async (fromDate = null, toDate = null, limit = 10) => {
        const params = new URLSearchParams();
        if (fromDate) params.append('fromDate', fromDate);
        if (toDate) params.append('toDate', toDate);
        params.append('limit', limit);

        return apiCall(`/admin/dashboard/top-partners?${params.toString()}`);
    },

    /**
     * Get recent orders
     */
    getRecentOrders: async (limit = 10) => {
        return apiCall(`/admin/dashboard/recent-orders?limit=${limit}`);
    },

    /**
     * Get popular categories
     */
    getPopularCategories: async (fromDate = null, toDate = null, limit = 10) => {
        const params = new URLSearchParams();
        if (fromDate) params.append('fromDate', fromDate);
        if (toDate) params.append('toDate', toDate);
        params.append('limit', limit);

        return apiCall(`/admin/dashboard/popular-categories?${params.toString()}`);
    },

    /**
     * Get user growth data
     */
    getUserGrowth: async (fromDate = null, toDate = null, granularity = 'day') => {
        const params = new URLSearchParams();
        if (fromDate) params.append('fromDate', fromDate);
        if (toDate) params.append('toDate', toDate);
        params.append('granularity', granularity);

        return apiCall(`/admin/dashboard/user-growth?${params.toString()}`);
    },

    /**
     * Get city revenue data
     */
    getCityRevenue: async (fromDate = null, toDate = null, limit = 10) => {
        const params = new URLSearchParams();
        if (fromDate) params.append('fromDate', fromDate);
        if (toDate) params.append('toDate', toDate);
        params.append('limit', limit);

        return apiCall(`/admin/dashboard/city-revenue?${params.toString()}`);
    },

    /**
     * Export analytics data
     */
    exportAnalytics: async (exportType, format, fromDate, toDate) => {
        return apiCall('/admin/dashboard/export', {
            method: 'POST',
            body: JSON.stringify({
                exportType,
                format,
                fromDate,
                toDate,
            }),
        });
    },
};

// ============================================
// Partner/Owner Analytics APIs
// ============================================
export const ownerAnalyticsApi = {
    /**
     * Get owner dashboard metrics
     */
    getDashboardMetrics: async () => {
        return apiCall('/Owner/OwnerDashboard/metrics');
    },

    /**
     * Get revenue chart
     */
    getRevenueChart: async (period = 'month') => {
        return apiCall(`/Owner/OwnerDashboard/revenue-chart?period=${period}`);
    },

    /**
     * Get orders chart
     */
    getOrdersChart: async (period = 'month') => {
        return apiCall(`/Owner/OwnerDashboard/orders-chart?period=${period}`);
    },

    /**
     * Get recent orders
     */
    getRecentOrders: async (limit = 5) => {
        return apiCall(`/Owner/OwnerDashboard/recent-orders?limit=${limit}`);
    },

    /**
     * Get upcoming events
     */
    getUpcomingEvents: async (days = 7) => {
        return apiCall(`/Owner/OwnerDashboard/upcoming-events?days=${days}`);
    },

    /**
     * Get top menu items
     */
    getTopMenuItems: async (limit = 10) => {
        return apiCall(`/Owner/OwnerDashboard/top-items?limit=${limit}`);
    },

    /**
     * Get performance insights
     */
    getPerformanceInsights: async () => {
        return apiCall('/Owner/OwnerDashboard/insights');
    },

    /**
     * Get revenue breakdown
     */
    getRevenueBreakdown: async () => {
        return apiCall('/Owner/OwnerDashboard/revenue-breakdown');
    },
};

// Default export with all APIs
export default {
    admin: adminAnalyticsApi,
    owner: ownerAnalyticsApi,
};
