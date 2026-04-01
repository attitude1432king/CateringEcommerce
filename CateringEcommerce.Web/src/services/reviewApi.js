import apiClient from '../services/api/supervisor/apiConfig';

const API_BASE = '/api/user/reviews';

/**
 * Submit a new review for an order
 * @param {Object} reviewData - Review submission data
 * @returns {Promise}
 */
export const submitReview = async (reviewData) => {
  try {
    const response = await apiClient.post(`${API_BASE}/submit`, reviewData);
    return response.data;
  } catch (error) {
    console.error('Error submitting review:', error);
    throw error.response?.data || error.message;
  }
};

/**
 * Check if user can review an order
 * @param {number} orderId - Order ID
 * @returns {Promise}
 */
export const canReviewOrder = async (orderId) => {
  try {
    const response = await apiClient.get(`${API_BASE}/can-review/${orderId}`);
    return response.data;
  } catch (error) {
    console.error('Error checking review eligibility:', error);
    throw error.response?.data || error.message;
  }
};

/**
 * Get user's review for a specific order
 * @param {number} orderId - Order ID
 * @returns {Promise}
 */
export const getReviewByOrder = async (orderId) => {
  try {
    const response = await apiClient.get(`${API_BASE}/by-order/${orderId}`);
    return response.data;
  } catch (error) {
    console.error('Error fetching review:', error);
    throw error.response?.data || error.message;
  }
};

/**
 * Get all reviews submitted by the user
 * @param {number} pageNumber - Page number
 * @param {number} pageSize - Page size
 * @returns {Promise}
 */
export const getMyReviews = async (pageNumber = 1, pageSize = 20) => {
  try {
    const response = await apiClient.get(`${API_BASE}/my-reviews`, {
      params: { pageNumber, pageSize }
    });
    return response.data;
  } catch (error) {
    console.error('Error fetching my reviews:', error);
    throw error.response?.data || error.message;
  }
};

/**
 * Get specific review details
 * @param {number} reviewId - Review ID
 * @returns {Promise}
 */
export const getReviewDetail = async (reviewId) => {
  try {
    const response = await apiClient.get(`${API_BASE}/${reviewId}`);
    return response.data;
  } catch (error) {
    console.error('Error fetching review detail:', error);
    throw error.response?.data || error.message;
  }
};

/**
 * Update an existing review
 * @param {Object} reviewData - Updated review data
 * @returns {Promise}
 */
export const updateReview = async (reviewData) => {
  try {
    const response = await apiClient.put(`${API_BASE}/update`, reviewData);
    return response.data;
  } catch (error) {
    console.error('Error updating review:', error);
    throw error.response?.data || error.message;
  }
};

/**
 * Delete a review
 * @param {number} reviewId - Review ID
 * @returns {Promise}
 */
export const deleteReview = async (reviewId) => {
  try {
    const response = await apiClient.delete(`${API_BASE}/${reviewId}`);
    return response.data;
  } catch (error) {
    console.error('Error deleting review:', error);
    throw error.response?.data || error.message;
  }
};

/**
 * Get reviews for a specific catering (Public - no auth required)
 * @param {number} cateringId - Catering ID
 * @param {number} pageNumber - Page number
 * @param {number} pageSize - Page size
 * @returns {Promise}
 */
export const getCateringReviews = async (cateringId, pageNumber = 1, pageSize = 10) => {
  try {
    const response = await apiClient.get(`${API_BASE}/catering/${cateringId}`, {
      params: { pageNumber, pageSize }
    });
    return response.data;
  } catch (error) {
    console.error('Error fetching catering reviews:', error);
    throw error.response?.data || error.message;
  }
};

/**
 * Get review statistics for a catering (Public - no auth required)
 * @param {number} cateringId - Catering ID
 * @returns {Promise}
 */
export const getCateringReviewStats = async (cateringId) => {
  try {
    const response = await apiClient.get(`${API_BASE}/catering/${cateringId}/stats`);
    return response.data;
  } catch (error) {
    console.error('Error fetching catering review stats:', error);
    throw error.response?.data || error.message;
  }
};

export default {
  submitReview,
  canReviewOrder,
  getReviewByOrder,
  getMyReviews,
  getReviewDetail,
  updateReview,
  deleteReview,
  getCateringReviews,
  getCateringReviewStats
};
