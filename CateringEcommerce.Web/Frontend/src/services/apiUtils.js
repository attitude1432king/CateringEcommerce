const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:44368';

/**
 * A common utility for fetching data from the API.
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
        headers: {
            'Content-Type': 'application/json',
            // In a real app, you'd add the Authorization header here
            // 'Authorization': `Bearer ${your_jwt_token}`
        },
    };

    if (body) {
        options.body = JSON.stringify(body);
    }

    const response = await fetch(url, options);

    if (!response.ok) {
        const errorData = await response.json().catch(() => ({ message: 'An unknown error occurred' }));
        throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
    }

    return response.json();
};

/**
 * NEW - A common utility for fetching data from any external API.
 * @param {string} fullUrl - The complete URL for the external API.
 * @returns {Promise<any>} - The JSON response from the API.
 */
export const fetchExternalApi = async (fullUrl) => {
    debugger;
    const response = await fetch(fullUrl, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
        }
    });

    if (!response.ok) {
        const errorData = await response.json().catch(() => ({ message: 'An unknown error occurred' }));
        throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
    }

    return response.json();
};
