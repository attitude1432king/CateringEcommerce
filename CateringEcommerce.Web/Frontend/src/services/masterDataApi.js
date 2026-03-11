const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:44368';


const apiCall = async (endpoint, options = {}) => {
    const response = await fetch(`${API_BASE_URL}/api/admin/master-data${endpoint}`, {
        ...options,
        headers: {
            'Content-Type': 'application/json',
            ...options.headers,
        },
        credentials: 'include',
    });

    if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || 'API request failed');
    }

    return response.json();
};

const createEntityApi = (path) => ({
    getAll: (params) => {
        const query = new URLSearchParams(
            Object.entries(params || {}).filter(([_, value]) => value !== null && value !== undefined && value !== '')
        ).toString();
        return apiCall(`/${path}${query ? `?${query}` : ''}`);
    },
    getById: (id) => apiCall(`/${path}/${id}`),
    create: (data) => apiCall(`/${path}`, {
        method: 'POST',
        body: JSON.stringify(data)
    }),
    update: (id, data) => apiCall(`/${path}/${id}`, {
        method: 'PUT',
        body: JSON.stringify(data)
    }),
    updateStatus: (id, isActive) => apiCall(`/${path}/${id}/status`, {
        method: 'PUT',
        body: JSON.stringify({ id, isActive })
    }),
    checkUsage: (id) => apiCall(`/${path}/${id}/usage`),
});

const createCateringTypeApi = (categoryId) => ({
    getAll: (params) => {
        const query = new URLSearchParams(
            Object.entries(params || {}).filter(([_, value]) => value !== null && value !== undefined && value !== '')
        ).toString();
        return apiCall(`/catering-types/${categoryId}${query ? `?${query}` : ''}`);
    },
    getById: (id) => apiCall(`/catering-types/${categoryId}/${id}`),
    create: (data) => apiCall(`/catering-types/${categoryId}`, {
        method: 'POST',
        body: JSON.stringify(data)
    }),
    update: (id, data) => apiCall(`/catering-types/${categoryId}/${id}`, {
        method: 'PUT',
        body: JSON.stringify(data)
    }),
    updateStatus: (id, isActive) => apiCall(`/catering-types/${categoryId}/${id}/status`, {
        method: 'PUT',
        body: JSON.stringify({ id, isActive })
    }),
    checkUsage: (id) => apiCall(`/catering-types/${categoryId}/${id}/usage`),
});

export const masterDataApi = {
    cities: createEntityApi('cities'),
    foodCategories: createEntityApi('food-categories'),
    cuisineTypes: createCateringTypeApi(2), // Category ID 2 = Cuisine Types
    foodTypes: createCateringTypeApi(1),    // Category ID 1 = Food Types
    eventTypes: createCateringTypeApi(3),   // Category ID 3 = Event Types
    serviceTypes: createCateringTypeApi(4), // Category ID 4 = Service Types
    guestCategories: createEntityApi('guest-categories'),
    themes: createEntityApi('themes'),

    // Helper endpoints
    getStates: () => apiCall('/states'),
    updateDisplayOrder: (data) => apiCall('/display-order', {
        method: 'PUT',
        body: JSON.stringify(data)
    }),
};

export default masterDataApi;
