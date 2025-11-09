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

    // Get the auth token from localStorage
    const token = localStorage.getItem('authToken');

    const options = {
        method,
        headers: {
            'Content-Type': 'application/json'
        },
    };

    // If a token exists, add it to the Authorization header
    if (token) {
        // In a real app, you'd add the Authorization header here
        options.headers['Authorization'] = `Bearer ${token}`;
    }


    if (body) {
        options.body = JSON.stringify(body);
    }

    try {
        const response = await fetch(url, options);

        // If the token is invalid or expired, the API should return a 401
        if (response.status === 401) {
            // Clear user data from storage
            localStorage.removeItem('feasto_user');
            localStorage.removeItem('authToken');

            // Determine where to redirect
            // A simple way is to check the current path
            if (window.location.pathname.startsWith('/owner')) {
                window.location.href = '/partner-login';
            } else {
                window.location.href = '/';
            }

            // Throw an error to stop further execution in the calling function
            throw new Error('Unauthorized');
        }

        if (!response.ok) {
            const errorData = await response.json().catch(() => ({ message: 'An unknown error occurred' }));
            console.log(errorData.errors);
            throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
        }

        // Handle responses with no content
        if (response.status === 204) {
            return null;
        }

        return response.json();
    } catch (error) {
        console.error('API fetch error:', error);
        throw error;
    }
};

/**
 * NEW - A common utility for fetching data from any external API.
 * @param {string} fullUrl - The complete URL for the external API.
 * @returns {Promise<any>} - The JSON response from the API.
 */
export const fetchExternalApi = async (fullUrl) => {
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

