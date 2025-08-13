const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:44368';

import { fetchExternalApi,fetchApi } from './apiUtils';

export const ownerApiService = {
    /**
     * Registers a new catering partner (owner).
     * @param {Object} formData - The registration data.
     * @returns {Promise<Object>} - The response message.
    */
    registerOwner: async (formData) => fetchApi(`/Auth/Owner/Register`, 'POST', formData),

    // Get the Pincode details from the external API
    getPincodeDetails: async (pincodeUrl) => fetchExternalApi(pincodeUrl),


    // CORRECTED FUNCTION to get all options for Step 3 in one call
    getRegistrationOptions: async () => {
        console.log("Fetching all registration options from API...");

        // Use Promise.all to run all API calls in parallel for efficiency
        const [
            foodTypeOptions,
            cuisineOptions,
            eventTypeOptions,
            serviceTypeOptions
        ] = await Promise.all([
            fetchApi(`/Auth/Owner/Service/${1}`), // Assuming 1 is for Food Types
            fetchApi(`/Auth/Owner/Service/${2}`), // Assuming 2 is for Cuisine Types
            fetchApi(`/Auth/Owner/Service/${3}`), // Assuming 3 is for Event Types
            fetchApi(`/Auth/Owner/Service/${4}`)  // Assuming 4 is for Service Types
        ]);

        // Return a new object with the data from the API calls
        return {
            foodTypeOptions,
            cuisineOptions,
            eventTypeOptions,
            serviceTypeOptions
        };
    },

};  