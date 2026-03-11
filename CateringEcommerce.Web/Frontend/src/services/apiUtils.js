const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:44368';

/**
 * A common utility for fetching data from the API.
 * CONSOLIDATED: This replaces the old apiCall.js with portal-aware redirects
 * @param {string} endpoint - The API endpoint (e.g., '/profile/123').
 * @param {string} method - The HTTP method (GET, POST, PUT).
 * @param {object} [body=null] - The request body for POST/PUT requests.
 * @param {object} [queryParams={}] - An object of query parameters.
 * @returns {Promise<any>} - The JSON response from the API.
 */
export const fetchApi = async (endpoint, method = 'GET', body = null, queryParams = {}) => {
    let url = `${API_BASE_URL}/api${endpoint}`;

    // Append query parameters to the URL
    const params = new URLSearchParams(queryParams);
    if (params.toString()) {
        url += `?${params.toString()}`;
    }

    const options = {
        method,
        credentials: 'include', // httpOnly cookie sent automatically
        headers: {}
    };

    // Handle body
    if (body) {
        if (body instanceof FormData) {
            // For file uploads / multipart forms, do NOT set Content-Type
            // The browser sets the correct multipart boundary automatically
            options.body = body;
        } else {
            // JSON payload
            options.headers['Content-Type'] = 'application/json';
            options.body = JSON.stringify(body);
        }
    }

    try {
        const response = await fetch(url, options);

        // Unauthorized handling - redirect to the correct login page based on current portal
        if (response.status === 401) {
            const path = window.location.pathname;

            if (path.startsWith('/admin')) {
                localStorage.removeItem('admin');
                window.location.href = '/admin/login';
            } else if (path.startsWith('/supervisor')) {
                localStorage.removeItem('supervisorToken');
                localStorage.removeItem('supervisorId');
                window.location.href = '/supervisor/login';
            } else if (path.startsWith('/owner') || path.startsWith('/partner')) {
                //localStorage.removeItem('enyvora_user');
                //window.location.href = '/partner-login';
            } else {
                localStorage.removeItem('enyvora_user');
                window.location.href = '/';
            }

            throw new Error('Unauthorized');
        }

        // Handle errors
        if (!response.ok) {
            const errorData = await response.json().catch(() => ({ message: 'An unknown error occurred' }));
            console.error(errorData.errors);
            throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
        }

        // No content
        if (response.status === 204) return null;

        return response.json();
    } catch (error) {
        console.error('API fetch error:', error);
        throw error;
    }
};

// Helper function to convert a File object to a Base64 DTO
export const fileToBase64Dto = (file) => {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onload = () => resolve({
            base64: reader.result,
            name: file.name,
            type: file.type,
        });
        reader.onerror = error => reject(error);
    });
};

// P3 FIX: Export alias for backward compatibility (consolidating apiCall.js)
export const apiCall = fetchApi;

