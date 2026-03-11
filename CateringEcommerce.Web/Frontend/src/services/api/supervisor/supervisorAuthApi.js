/**
 * Supervisor Authentication API
 * Login, logout, and auth-related endpoints
 */

import apiClient, { handleApiResponse, handleApiError } from './apiConfig';

/**
 * Login supervisor with email/phone and password
 */
export const supervisorLogin = async (identifier, password) => {
    try {
        const response = await apiClient.post('/api/Supervisor/auth/login', {
            identifier,
            password,
        });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

/**
 * Logout supervisor
 */
export const supervisorLogout = async () => {
    try {
        const response = await apiClient.post('/api/Supervisor/auth/logout');
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

/**
 * Request password reset
 */
export const requestPasswordReset = async (identifier) => {
    try {
        const response = await apiClient.post('/api/Supervisor/auth/forgot-password', {
            identifier,
        });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export default {
    supervisorLogin,
    supervisorLogout,
    requestPasswordReset,
};
