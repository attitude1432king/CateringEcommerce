/**
 * Admin API Service
 * Centralized API calls for admin module
 */

const API_BASE_URL = 'http://localhost:5000/api/admin';

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
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
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
  getMetrics: () => apiCall('/dashboard/metrics'),
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

// Export all APIs
export default {
  auth: authApi,
  dashboard: dashboardApi,
  caterings: cateringApi,
  users: userApi,
  earnings: earningsApi,
  reviews: reviewApi,
};
