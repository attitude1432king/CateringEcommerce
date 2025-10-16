const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:44368';

import { fetchExternalApi,fetchApi, fileToBase64Dto } from './apiUtils';

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
            serviceTypeOptions,
            servingSlotTypeOptions
        ] = await Promise.all([
            fetchApi(`/Auth/Owner/Service/${1}`), // Assuming 1 is for Food Types
            fetchApi(`/Auth/Owner/Service/${2}`), // Assuming 2 is for Cuisine Types
            fetchApi(`/Auth/Owner/Service/${3}`), // Assuming 3 is for Event Types
            fetchApi(`/Auth/Owner/Service/${4}`) , // Assuming 4 is for Service Types
            fetchApi(`/Auth/Owner/Service/${5}`)  // Assuming 4 is for Service Types
        ]);

        // Return a new object with the data from the API calls
        return {
            foodTypeOptions,
            cuisineOptions,
            eventTypeOptions,
            serviceTypeOptions,
            servingSlotTypeOptions
        };
    },

    // Fetch the owner profile details
    getOwnerProfile: async () => fetchApi('/Owner/Profile/GetProfileDetails'),

    updateBusinessSettings: async (businessData) => {
        const payload = { ...businessData };
        if (businessData.newLogoFile) {
            payload.newLogoFile = await fileToBase64Dto(businessData.newLogoFile);
        }
        fetchApi('/Owner/Profile/UpdateBusiness', 'POST', payload);
    },

    updateAddressSettings: async (addressData) => fetchApi('/Owner/Profile/UpdateAddress', 'POST', addressData),

    updateServicesSettings: async (servicesData) => {
        const payload = JSON.parse(JSON.stringify(servicesData)); // Deep copy to handle complex objects

        if (servicesData.kitchenMedia) {
            const existingMedia = [];
            const newMediaFiles = [];

            for (const media of servicesData.kitchenMedia) {
                if (media.fileObject) {
                    newMediaFiles.push(media.fileObject);
                } else if (media.filePath) {
                    existingMedia.push({ id: media.id, type: media.mediaType, path: media.filePath });
                }
            }

            payload.existingKitchenMedia = existingMedia;
            payload.newKitchenMediaFiles = await Promise.all(
                newMediaFiles.map(file => fileToBase64Dto(file))
            );
            delete payload.kitchenMedia;
        }

        fetchApi('/Owner/Profile/UpdateServices', 'POST', payload);
    },

    updateLegalPaymentSettings: async (legalData) => fetchApi('/Owner/Profile/UpdateLegal', 'POST', legalData),

};  