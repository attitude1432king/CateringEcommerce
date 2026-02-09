/**
 * Partner Approval API Service (NEW - Enum-based)
 * Works with the new AdminPartnerApprovalRepository backend
 *
 * Uses INT-based enum values for status and priority instead of strings
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
    const url = `${API_BASE_URL}/api${endpoint}`;
    console.log('🌐 API Call:', { url, method: options.method || 'GET' });

    const response = await fetch(url, {
      ...options,
      headers: {
        ...getAuthHeaders(),
        ...options.headers,
      },
    });

    console.log('📡 Response Status:', response.status, response.statusText);

    const data = await response.json();
    console.log('📄 Response Data:', data);

    if (!response.ok) {
      console.error('❌ API Error Response:', data);
      throw new Error(data.message || `API request failed with status ${response.status}`);
    }

    return data;
  } catch (error) {
    console.error('❌ API Error:', error);
    throw error;
  }
};

/**
 * Approval Status Enum (matches backend ApprovalStatus enum)
 */
export const ApprovalStatus = {
  PENDING: 1,
  APPROVED: 2,
  REJECTED: 3,
  UNDER_REVIEW: 4,
  INFO_REQUESTED: 5
};

/**
 * Priority Status Enum (matches backend PriorityStatus enum)
 */
export const PriorityStatus = {
  LOW: 0,
  NORMAL: 1,
  HIGH: 2,
  URGENT: 3
};

/**
 * Partner Approval API Methods
 */
export const partnerApprovalApi = {
  /**
   * Get pending partner requests with filtering and pagination
   *
   * @param {Object} filters - Filter parameters
   * @param {number} filters.pageNumber - Page number (default: 1)
   * @param {number} filters.pageSize - Items per page (default: 20)
   * @param {string} filters.searchTerm - Search term
   * @param {number} filters.approvalStatusId - Approval status ID (1-5)
   * @param {number} filters.priorityId - Priority ID (0-3)
   * @param {number} filters.cityId - City ID
   * @param {string} filters.fromDate - From date (YYYY-MM-DD)
   * @param {string} filters.toDate - To date (YYYY-MM-DD)
   * @param {string} filters.sortBy - Sort column
   * @param {string} filters.sortOrder - Sort order (ASC/DESC)
   */
  getPendingRequests: async (filters = {}) => {
    const params = new URLSearchParams();

    // Add all non-empty filter parameters
    Object.keys(filters).forEach(key => {
      if (filters[key] !== null && filters[key] !== undefined && filters[key] !== '') {
        params.append(key, filters[key]);
      }
    });

    return apiCall(`/admin/partners/pending?${params.toString()}`);
  },

  /**
   * Get complete registration detail for a partner request
   *
   * @param {number} partnerId - Partner ID (OwnerId)
   */
  getPartnerDetail: async (partnerId) => {
    return apiCall(`/admin/partners/${partnerId}/registration-detail`);
  },

  /**
   * Approve a partner request
   *
   * @param {number} partnerId - Partner ID
   * @param {Object} data - Approval data
   * @param {string} data.remarks - Optional remarks
   * @param {boolean} data.sendNotification - Send notification (default: true)
   */
  approvePartner: async (partnerId, data = {}) => {
    return apiCall(`/admin/partners/${partnerId}/approve`, {
      method: 'POST',
      body: JSON.stringify({
        remarks: data.remarks || null,
        sendNotification: data.sendNotification !== false
      })
    });
  },

  /**
   * Reject a partner request
   *
   * @param {number} partnerId - Partner ID
   * @param {Object} data - Rejection data
   * @param {string} data.rejectionReason - Rejection reason (MANDATORY)
   * @param {boolean} data.sendNotification - Send notification (default: true)
   */
  rejectPartner: async (partnerId, data) => {
    if (!data.rejectionReason || data.rejectionReason.trim() === '') {
      throw new Error('Rejection reason is required');
    }

    return apiCall(`/admin/partners/${partnerId}/reject`, {
      method: 'POST',
      body: JSON.stringify({
        rejectionReason: data.rejectionReason,
        sendNotification: data.sendNotification !== false
      })
    });
  },

  /**
   * Update priority for a partner request
   *
   * @param {number} partnerId - Partner ID
   * @param {number} priorityId - Priority ID (0-3)
   */
  updatePriority: async (partnerId, priorityId) => {
    return apiCall(`/admin/partners/${partnerId}/priority`, {
      method: 'PUT',
      body: JSON.stringify({ priorityId })
    });
  },

  /**
   * Get all approval status options (for dropdowns)
   */
  getApprovalStatuses: async () => {
    return apiCall('/admin/partners/enums/approval-statuses');
  },

  /**
   * Get all priority options (for dropdowns)
   */
  getPriorities: async () => {
    return apiCall('/admin/partners/enums/priorities');
  }
};

// Export the API
export default partnerApprovalApi;
