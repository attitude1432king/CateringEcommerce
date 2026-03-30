/**
 * API Configuration for Supervisor Portal
 * Centralized axios configuration with interceptors
 */

import axios from 'axios';
import { sanitizeForLogging } from '../../../utils/securityUtils';

// Base API URL from environment variable
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:44368';

// Create axios instance with default config
// SECURITY: withCredentials sends the httpOnly supervisorToken cookie automatically.
// The token is never stored in or read from localStorage.
const apiClient = axios.create({
    baseURL: API_BASE_URL,
    timeout: 30000, // 30 seconds
    withCredentials: true,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Request interceptor
apiClient.interceptors.request.use(
    (config) => {
        // SECURITY FIX: Log request in development with sanitized data
        if (import.meta.env.DEV) {
            // Sanitize config to avoid logging sensitive data (tokens, passwords, etc.)
            const sanitizedConfig = {
                method: config.method,
                url: config.url,
                data: sanitizeForLogging(config.data),
                // Do NOT log headers as they contain Authorization tokens
                // headers: config.headers - REMOVED
            };
            console.log(`[API Request] ${config.method?.toUpperCase()} ${config.url}`, sanitizedConfig.data);
        }

        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

// Response interceptor - Handle errors globally
apiClient.interceptors.response.use(
    (response) => {
        // SECURITY FIX: Log response in development with sanitized data
        if (import.meta.env.DEV) {
            const sanitizedData = sanitizeForLogging(response.data);
            console.log(`[API Response] ${response.config.url}`, sanitizedData);
        }

        return response;
    },
    (error) => {
        // SECURITY FIX: Log error in development with sanitized data
        if (import.meta.env.DEV) {
            const sanitizedError = sanitizeForLogging(error.response?.data || error.message);
            console.error(`[API Error] ${error.config?.url}`, sanitizedError);
        }

        // Handle specific error codes
        if (error.response) {
            switch (error.response.status) {
                case 401:
                    // Unauthorized - session cookie expired or invalid, redirect to login
                    // Guard: skip redirect if already on login page to avoid infinite reload loop
                    if (!window.location.pathname.includes('/supervisor/login')) {
                        window.location.href = '/supervisor/login';
                    }
                    break;
                case 403:
                    // Forbidden - insufficient permissions
                    console.error('Insufficient permissions');
                    break;
                case 404:
                    // Not found
                    console.error('Resource not found');
                    break;
                case 500:
                    // Server error
                    console.error('Server error');
                    break;
                default:
                    console.error('API Error:', error.response.data);
            }
        } else if (error.request) {
            // Network error - no response received
            console.error('Network error: No response from server');
        } else {
            // Request setup error
            console.error('Request error:', error.message);
        }

        return Promise.reject(error);
    }
);

/**
 * Helper function to handle API responses
 * Standardizes response format across all API calls
 */
export const handleApiResponse = (response) => {
    return {
        success: true,
        data: response.data,
        message: response.data?.message || 'Success',
    };
};

/**
 * Helper function to handle API errors
 * Standardizes error format across all API calls
 */
export const handleApiError = (error) => {
    let message = 'An error occurred';
    let errors = {};

    if (error.response) {
        // Server responded with error
        message = error.response.data?.message || error.response.statusText;
        errors = error.response.data?.errors || {};
    } else if (error.request) {
        // No response from server
        message = 'Unable to connect to server. Please check your internet connection.';
    } else {
        // Request setup error
        message = error.message;
    }

    return {
        success: false,
        message,
        errors,
    };
};

/**
 * File upload helper
 * Uploads file to pre-signed URL
 */
export const uploadFile = async (file, presignedUrl) => {
    try {
        await axios.put(presignedUrl, file, {
            headers: {
                'Content-Type': file.type,
            },
        });
        return { success: true };
    } catch (error) {
        console.error('File upload error:', error);
        return { success: false, message: 'File upload failed' };
    }
};

/**
 * Get upload URL for file
 */
export const getUploadUrl = async (fileName, fileType, folder = 'supervisor') => {
    try {
        const response = await apiClient.post('/api/upload/get-presigned-url', {
            fileName,
            fileType,
            folder,
        });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export default apiClient;
