import { fetchApi } from './apiUtils';

/**
 * Partner Request API Service
 *
 * Handles all API calls related to partner registration requests
 */

const BASE_URL = '/admin/partner-requests';

export const partnerRequestApi = {
  /**
   * Get all partner requests with filters
   */
  async getAll(filters = {}) {
    const queryParams = new URLSearchParams();

    if (filters.status) queryParams.append('Status', filters.status);
    if (filters.city) queryParams.append('CityId', filters.city);
    if (filters.state) queryParams.append('State', filters.state);
    if (filters.dateFrom) queryParams.append('FromDate', filters.dateFrom);
    if (filters.dateTo) queryParams.append('ToDate', filters.dateTo);
    if (filters.searchTerm) queryParams.append('SearchTerm', filters.searchTerm);
    if (filters.pageNumber) queryParams.append('PageNumber', filters.pageNumber);
    if (filters.pageSize) queryParams.append('PageSize', filters.pageSize);
    if (filters.sortBy) queryParams.append('SortBy', filters.sortBy);
    if (filters.sortOrder) queryParams.append('SortOrder', filters.sortOrder);
    if (filters.priority) queryParams.append('Priority', filters.priority);

        return fetchApi(`${BASE_URL}`, 'GET', null, queryParams);
  },

  /**
   * Get partner request details
   */
  async getDetails(requestId) {
      return fetchApi(`${BASE_URL}/${requestId}`);
  },

  /**
   * Update partner request status (Approve/Reject/Request Info)
   */
  async updateStatus(requestId, data) {
      return fetchApi(`${BASE_URL}/${requestId}/status`, 'PUT', data);
  },

  /**
   * Send communication to partner
   */
  async sendCommunication(requestId, data) {
      return fetchApi(`${BASE_URL}/${requestId}/communicate`, 'POST', data);
  },

  /**
   * Get communication templates
   */
  async getTemplates() {
      return fetchApi(`${BASE_URL}/templates`);
  },

  /**
   * Export partner requests
   */
  async export(filters = {}, format = 'EXCEL') {
    const queryParams = new URLSearchParams();

    if (filters.status) queryParams.append('status', filters.status);
    if (filters.city) queryParams.append('city', filters.city);
    if (filters.dateFrom) queryParams.append('dateFrom', filters.dateFrom);
    if (filters.dateTo) queryParams.append('dateTo', filters.dateTo);
    queryParams.append('format', format);

    // For file download, we need to handle differently
    const url = `${BASE_URL}/export}`;

    try {
        const response = await fetchApi(url, 'GET', null, queryParams); 

      if (response.ok) {
        const blob = await response.blob();
        const downloadUrl = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = downloadUrl;
        link.download = `partner-requests-${new Date().toISOString().split('T')[0]}.${format.toLowerCase()}`;
        document.body.appendChild(link);
        link.click();
        link.remove();

        return { success: true, message: 'Export completed successfully' };
      } else {
        return { success: false, message: 'Export failed' };
      }
    } catch (error) {
      console.error('Export error:', error);
      return { success: false, message: 'Export failed' };
    }
  }
};

export default partnerRequestApi;
