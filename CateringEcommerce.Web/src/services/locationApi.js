const API_BASE_URL = import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '');

import { fetchApi } from './apiUtils';

export const locationApiService = {


    // Get the default city from the backend
    fetchDefaultCity: async () => fetchApi(`/Common/Locations/default-city`),

    // Update the selected city on the backend
    updateCityOnBackend: async (city) => fetchApi(`/Common/Locations/update-city`, 'POST', city),

    getVerifiedCateringListAsync: async (city) => fetchApi(`/User/Home/CateringList?cityName=${city}`),
}
