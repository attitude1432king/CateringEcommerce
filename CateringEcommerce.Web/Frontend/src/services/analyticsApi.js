/**
 * Analytics API Service
 * API calls for admin and partner analytics
 */
import { apiCall } from './apiUtils'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:44368';

// ============================================
// Admin Analytics APIs
// ============================================
export const adminAnalyticsApi = {
    /**
     * Get dashboard metrics with date range
     */
    getDashboardMetrics: async (fromDate = null, toDate = null) => {
        const params = {};

        if (fromDate) params.fromDate = fromDate;
        if (toDate) params.toDate = toDate;

        return apiCall('/admin/dashboard/v2/metrics', 'GET', null, params);
    },

    /**
     * Get revenue chart data
     */
    getRevenueChart: async (fromDate = null, toDate = null, granularity = 'day') => {
        const params = {};

        if (fromDate) params.fromDate = fromDate;
        if (toDate) params.toDate = toDate;
        params.granularity = granularity;

        return apiCall('/admin/dashboard/revenue-chart', 'GET', null, params);
    },

    /**
     * Get order analytics
     */
    getOrderAnalytics: async (fromDate = null, toDate = null) => {
        const params = {};

        if (fromDate) params.fromDate = fromDate;
        if (toDate) params.toDate = toDate;

        return apiCall(`/admin/dashboard/order-analytics`, 'GET', null, params);
    },

    /**
     * Get top performing partners
     */
    getTopPartners: async (fromDate = null, toDate = null, limit = 10) => {
        const params = {};

        if (fromDate) params.fromDate = fromDate;
        if (toDate) params.toDate = toDate;
        params.limit = limit;

        return apiCall('/admin/dashboard/top-partners', 'GET', null, params);
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
        const params = {};

        if (fromDate) params.fromDate = fromDate;
        if (toDate) params.toDate = toDate;
        params.limit = limit;

        return apiCall('/admin/dashboard/popular-categories', 'GET', null, params);
    },

    /**
     * Get user growth data
     */
    getUserGrowth: async (fromDate = null, toDate = null, granularity = 'day') => {
        const params = {};

        if (fromDate) params.fromDate = fromDate;
        if (toDate) params.toDate = toDate;
        params.granularity = granularity;

        return apiCall('/admin/dashboard/user-growth', 'GET', null, params);
    },

    /**
     * Get city revenue data
     */
    getCityRevenue: async (fromDate = null, toDate = null, limit = 10) => {
        const params = {};

        if (fromDate) params.fromDate = fromDate;
        if (toDate) params.toDate = toDate;
        params.limit = limit;

        return apiCall('/admin/dashboard/city-revenue', 'GET', null, params);
    },

    /**
     * Export analytics data
     */
    exportAnalytics: async (exportType, format, fromDate, toDate) => {
        return apiCall('/admin/dashboard/export', 'POST', {
            exportType,
            format,
            fromDate,
            toDate,
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
