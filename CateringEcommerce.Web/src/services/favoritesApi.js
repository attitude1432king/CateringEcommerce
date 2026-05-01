/**
 * Favorites/Wishlist API Service
 * Handles all favorite-related API calls
 */

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '');

/**
 * Generic fetch wrapper with error handling
 */
const fetchApi = async (endpoint, method = 'GET', body = null) => {
  const config = {
    method,
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
  };

  if (body && method !== 'GET') {
    config.body = JSON.stringify(body);
  }

  try {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, config);
    const data = await response.json();

    if (!response.ok) {
      throw new Error(data.message || `HTTP error! status: ${response.status}`);
    }

    return data;
  } catch (error) {
    console.error(`API Error (${endpoint}):`, error);
    throw error;
  }
};

/**
 * Add a catering to favorites
 * @param {number} cateringId - Catering ID to add
 * @returns {Promise<Object>} Response with result and message
 */
export const addToFavorites = async (cateringId) => {
  return await fetchApi('/api/User/Favorites/Add', 'POST', { cateringId });
};

/**
 * Remove a catering from favorites
 * @param {number} cateringId - Catering ID to remove
 * @returns {Promise<Object>} Response with result and message
 */
export const removeFromFavorites = async (cateringId) => {
  return await fetchApi(`/api/User/Favorites/${cateringId}`, 'DELETE');
};

/**
 * Get all favorites for the current user
 * @param {number} pageNumber - Page number (default: 1)
 * @param {number} pageSize - Page size (default: 20)
 * @returns {Promise<Object>} Response with favorites array and pagination info
 */
export const getFavorites = async (pageNumber = 1, pageSize = 20) => {
  return await fetchApi(`/api/User/Favorites?pageNumber=${pageNumber}&pageSize=${pageSize}`, 'GET');
};

/**
 * Check if a specific catering is in favorites
 * @param {number} cateringId - Catering ID to check
 * @returns {Promise<Object>} Response with isFavorite boolean
 */
export const isFavorite = async (cateringId) => {
  return await fetchApi(`/api/User/Favorites/Check/${cateringId}`, 'GET');
};

/**
 * Get total favorites count for the current user
 * @returns {Promise<Object>} Response with count
 */
export const getFavoritesCount = async () => {
  return await fetchApi('/api/User/Favorites/Count', 'GET');
};

/**
 * Get favorite status for multiple caterings (batch check)
 * @param {Array<number>} cateringIds - Array of catering IDs
 * @returns {Promise<Object>} Response with dictionary of catering ID to boolean
 */
export const getFavoriteStatus = async (cateringIds) => {
  if (!Array.isArray(cateringIds) || cateringIds.length === 0) {
    throw new Error('cateringIds must be a non-empty array');
  }

  if (cateringIds.length > 50) {
    throw new Error('Maximum 50 catering IDs allowed per request');
  }

  return await fetchApi('/api/User/Favorites/Status', 'POST', { cateringIds });
};

/**
 * Toggle favorite status (add if not exists, remove if exists)
 * @param {number} cateringId - Catering ID to toggle
 * @returns {Promise<Object>} Response with new favorite status
 */
export const toggleFavorite = async (cateringId) => {
  return await fetchApi('/api/User/Favorites/Toggle', 'POST', { cateringId });
};

/**
 * Hook for React components to use favorites with state management
 * Usage example:
 *
 * const { isFavorite, toggle, isLoading } = useFavorite(cateringId);
 *
 * <button onClick={toggle} disabled={isLoading}>
 *   {isFavorite ? '❤️' : '🤍'}
 * </button>
 */
export const useFavoriteHook = () => {
  // This is a placeholder for a custom React hook
  // Implementation can be added in a separate hooks file if needed
  return {
    addToFavorites,
    removeFromFavorites,
    toggleFavorite,
    isFavorite,
    getFavorites,
    getFavoritesCount,
    getFavoriteStatus
  };
};

export default {
  addToFavorites,
  removeFromFavorites,
  getFavorites,
  isFavorite,
  getFavoritesCount,
  getFavoriteStatus,
  toggleFavorite
};
