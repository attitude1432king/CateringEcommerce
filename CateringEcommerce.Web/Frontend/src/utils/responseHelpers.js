/**
 * Response Helper Utilities
 * Common utilities for handling API responses across the application
 */

/**
 * Check if the API response indicates success
 * @param {Object} response - The API response object
 * @returns {boolean} - True if the response is successful
 */
export const isSuccessResponse = (response) => {
    if (!response) {
        return false;
    }

    // Check for success property (common in our API responses)
    if (typeof response.success === 'boolean') {
        return response.success === true;
    }

    // Check for data property (alternative success indicator)
    if (response.data !== undefined) {
        return true;
    }

    // Check for statusCode property
    if (response.statusCode) {
        return response.statusCode >= 200 && response.statusCode < 300;
    }

    // Default to false if we can't determine success
    return false;
};

/**
 * Extract data from a successful API response
 * @param {Object} response - The API response object
 * @returns {any} - The extracted data, or null if not found
 */
export const extractData = (response) => {
    if (!response) {
        return null;
    }

    // Direct data property
    if (response.data !== undefined) {
        return response.data;
    }

    // Result property (some APIs use this)
    if (response.result !== undefined) {
        return response.result;
    }

    // Items property (for lists)
    if (response.items !== undefined) {
        return response.items;
    }

    // If response itself is an array, return it
    if (Array.isArray(response)) {
        return response;
    }

    // Return the entire response if we can't find a specific data property
    return response;
};

/**
 * Extract error message from an API response
 * @param {Object} response - The API response object or error
 * @returns {string} - The error message
 */
export const extractErrorMessage = (response) => {
    if (!response) {
        return 'An unknown error occurred';
    }

    // Direct message property
    if (response.message) {
        return response.message;
    }

    // Error property
    if (response.error) {
        return typeof response.error === 'string' ? response.error : response.error.message || 'An error occurred';
    }

    // Errors array (validation errors)
    if (response.errors && Array.isArray(response.errors)) {
        return response.errors.map(err => err.message || err).join(', ');
    }

    // Errors object (ASP.NET Core validation)
    if (response.errors && typeof response.errors === 'object') {
        const errorMessages = Object.values(response.errors).flat();
        return errorMessages.join(', ');
    }

    return 'An unknown error occurred';
};

/**
 * Extract pagination information from API response
 * @param {Object} response - The API response object
 * @returns {Object} - Pagination information
 */
export const extractPagination = (response) => {
    if (!response) {
        return {
            totalCount: 0,
            pageNumber: 1,
            pageSize: 20,
            totalPages: 0
        };
    }

    // Check for pagination property
    if (response.pagination) {
        return {
            totalCount: response.pagination.totalCount || 0,
            pageNumber: response.pagination.pageNumber || 1,
            pageSize: response.pagination.pageSize || 20,
            totalPages: response.pagination.totalPages || 0
        };
    }

    // Check for direct properties
    if (response.totalCount !== undefined) {
        return {
            totalCount: response.totalCount || 0,
            pageNumber: response.pageNumber || 1,
            pageSize: response.pageSize || 20,
            totalPages: response.totalPages || 0
        };
    }

    // Default pagination
    return {
        totalCount: 0,
        pageNumber: 1,
        pageSize: 20,
        totalPages: 0
    };
};

/**
 * Check if the response contains data
 * @param {Object} response - The API response object
 * @returns {boolean} - True if response contains data
 */
export const hasData = (response) => {
    const data = extractData(response);

    if (data === null || data === undefined) {
        return false;
    }

    // Check if array has items
    if (Array.isArray(data)) {
        return data.length > 0;
    }

    // Check if object has properties
    if (typeof data === 'object') {
        return Object.keys(data).length > 0;
    }

    return true;
};

/**
 * Handle API response with common error handling
 * @param {Promise} apiCall - The API call promise
 * @param {Function} onSuccess - Callback for successful response
 * @param {Function} onError - Callback for error response
 * @returns {Promise<void>}
 */
export const handleApiResponse = async (apiCall, onSuccess, onError) => {
    try {
        const response = await apiCall;

        if (isSuccessResponse(response)) {
            const data = extractData(response);
            if (onSuccess) {
                onSuccess(data, response);
            }
        } else {
            const errorMessage = extractErrorMessage(response);
            if (onError) {
                onError(errorMessage, response);
            }
        }
    } catch (error) {
        console.error('API call failed:', error);
        const errorMessage = extractErrorMessage(error);
        if (onError) {
            onError(errorMessage, error);
        }
    }
};

export default {
    isSuccessResponse,
    extractData,
    extractErrorMessage,
    extractPagination,
    hasData,
    handleApiResponse
};
