/*
========================================
File: src/services/cateringApi.js
========================================
API service for catering discovery features.
Handles all API calls for packages, food items, decorations, reviews, and categories.
*/
import { fetchApi } from './apiUtils';
import { isSuccessResponse, extractData, extractPagination } from '../utils/responseHelpers';

/**
 * Catering API Service
 * All endpoints return the standard API response format:
 * { success: boolean, message: string, data: any[], count: number }
 */
export const cateringApi = {
    /**
     * Get all food categories
     * Used for the horizontal category filter bar
     */
    getFoodCategories: async () => {
        try {
            const response = await fetchApi('/User/Home/FoodCategories', 'GET');
            return response;
        } catch (error) {
            console.error('Error fetching food categories:', error);
            throw error;
        }
    },

    /**
     * Get catering list by city name
     * @param {string} cityName - City name to filter by (optional)
     */
    getCateringList: async (cityName = '') => {
        try {
            const queryParams = cityName ? { cityName } : {};
            const response = await fetchApi('/User/Home/CateringList', 'GET', null, queryParams);
            return response;
        } catch (error) {
            console.error('Error fetching catering list:', error);
            throw error;
        }
    },

    /**
     * Get detailed information about a specific caterer
     * @param {number} cateringId - The caterer's ID
     */
    getCateringDetail: async (cateringId) => {
        try {
            const response = await fetchApi(`/User/Home/Catering/${cateringId}/Detail`, 'GET');
            return response;
        } catch (error) {
            console.error(`Error fetching catering detail for caterer ${cateringId}:`, error);
            throw error;
        }
    },

    /**
     * Check availability for a catering on a specific date
     * @param {number} cateringId
     * @param {string} date - yyyy-MM-dd
     */
    checkAvailability: async (cateringId, date) => {
        try {
            return await fetchApi(`/catering/${cateringId}/availability`, 'GET', null, { date });
        } catch (error) {
            console.error(`Error checking availability for caterer ${cateringId}:`, error);
            throw error;
        }
    },

    /**
     * Get blocked dates for a catering calendar month
     * @param {number} cateringId
     * @param {number} year
     * @param {number} month - 1-based
     */
    getAvailabilityCalendar: async (cateringId, year, month) => {
        try {
            return await fetchApi(`/catering/${cateringId}/availability/calendar`, 'GET', null, { year, month });
        } catch (error) {
            console.error(`Error fetching availability calendar for caterer ${cateringId}:`, error);
            throw error;
        }
    },

    /**
     * Get available discount coupons for a specific caterer
     * Returns only active, non-expired EntireCatering-type discounts
     * @param {number} cateringId - The caterer's owner ID
     */
    getAvailableCoupons: async (cateringId) => {
        try {
            const response = await fetchApi(`/User/Coupons/Available/${cateringId}`, 'GET');
            return response;
        } catch (error) {
            console.error(`Error fetching available coupons for caterer ${cateringId}:`, error);
            throw error;
        }
    },

    /**
     * Get all packages for a specific caterer
     * @param {number} cateringId - The caterer's ID
     */
    getPackages: async (cateringId) => {
        try {
            const response = await fetchApi(`/User/Home/Catering/${cateringId}/Packages`, 'GET');
            return response;
        } catch (error) {
            console.error(`Error fetching packages for caterer ${cateringId}:`, error);
            throw error;
        }
    },

    /**
     * Get categories included in a specific package
     * @param {number} cateringId - The caterer's ID
     * @param {number} packageId - The package ID
     */
    getPackageCategories: async (cateringId, packageId) => {
        try {
            const response = await fetchApi(`/User/Home/Catering/${cateringId}/Package/${packageId}/Categories`, 'GET');
            return response;
        } catch (error) {
            console.error(`Error fetching package categories for package ${packageId}:`, error);
            // Return empty array on error to prevent UI breaking
            return { success: false, message: 'Failed to fetch categories', data: [] };
        }
    },

    /**
     * Get food items for a caterer with optional filters
     * @param {number} cateringId - The caterer's ID
     * @param {object} filters - Optional filters
     * @param {number} filters.categoryId - Filter by food category
     * @param {boolean} filters.isPackageItem - true = included in packages, false = add-ons, null = all
     */
    getFoodItems: async (cateringId, filters = {}) => {
        try {
            const queryParams = {};

            if (filters.categoryId) {
                queryParams.categoryId = filters.categoryId;
            }

            if (filters.isPackageItem !== undefined && filters.isPackageItem !== null) {
                queryParams.isPackageItem = filters.isPackageItem;
            }

            const response = await fetchApi(
                `/User/Home/Catering/${cateringId}/FoodItems`,
                'GET',
                null,
                queryParams
            );
            return response;
        } catch (error) {
            console.error(`Error fetching food items for caterer ${cateringId}:`, error);
            throw error;
        }
    },

    /**
     * Get food items included in packages
     * @param {number} cateringId - The caterer's ID
     * @param {number} categoryId - Optional category filter
     */
    getIncludedFoodItems: async (cateringId, categoryId = null) => {
        const filters = { isPackageItem: true };
        if (categoryId) {
            filters.categoryId = categoryId;
        }
        return cateringApi.getFoodItems(cateringId, filters);
    },

    /**
     * Get add-on food items (excluded from packages)
     * @param {number} cateringId - The caterer's ID
     * @param {number} categoryId - Optional category filter
     */
    getAddonFoodItems: async (cateringId, categoryId = null) => {
        const filters = { isPackageItem: false };
        if (categoryId) {
            filters.categoryId = categoryId;
        }
        return cateringApi.getFoodItems(cateringId, filters);
    },

    /**
     * Get decoration themes for a caterer
     * @param {number} cateringId - The caterer's ID
     */
    getDecorations: async (cateringId) => {
        try {
            const response = await fetchApi(`/User/Home/Catering/${cateringId}/Decorations`, 'GET');
            return response;
        } catch (error) {
            console.error(`Error fetching decorations for caterer ${cateringId}:`, error);
            throw error;
        }
    },

    /**
     * Get customer reviews for a caterer with pagination
     * @param {number} cateringId - The caterer's ID
     * @param {number} pageNumber - Page number (1-based, default: 1)
     * @param {number} pageSize - Reviews per page (default: 10)
     */
    getReviews: async (cateringId, pageNumber = 1, pageSize = 10) => {
        try {
            const queryParams = { pageNumber, pageSize };
            const response = await fetchApi(
                `/User/Home/Catering/${cateringId}/Reviews`,
                'GET',
                null,
                queryParams
            );
            return response;
        } catch (error) {
            console.error(`Error fetching reviews for caterer ${cateringId}:`, error);
            throw error;
        }
    },

    /**
     * Get featured caterers for homepage
     */
    getFeaturedCaterers: async () => {
        try {
            const response = await fetchApi('/User/Home/FeaturedCaterers', 'GET');
            return response;
        } catch (error) {
            console.error('Error fetching featured caterers:', error);
            throw error;
        }
    },

    /**
     * Get homepage testimonials
     */
    getTestimonials: async () => {
        try {
            const response = await fetchApi('/User/Home/Testimonials', 'GET');
            return response;
        } catch (error) {
            console.error('Error fetching testimonials:', error);
            throw error;
        }
    },

    /**
     * Get homepage statistics
     */
    getStats: async () => {
        try {
            const response = await fetchApi('/User/Home/Stats', 'GET');
            return response;
        } catch (error) {
            console.error('Error fetching stats:', error);
            throw error;
        }
    }
};

// Re-export response helper functions from centralized location
// These functions are now imported from '../utils/responseHelpers' at the top
export { isSuccessResponse, extractData, extractPagination };
