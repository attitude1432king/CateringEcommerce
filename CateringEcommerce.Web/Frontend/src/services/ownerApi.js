const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:44368';

import { fetchExternalApi,fetchApi, fileToBase64Dto } from './apiUtils';

export const ownerApiService = {
    /**
     * Registers a new catering partner (owner).
     * @param {Object} formData - The registration data.
     * @returns {Promise<Object>} - The response message.
    */
    isImageType: (type) => {
        const imageExtensions = ['.jpg', '.jpeg', '.png', '.gif', '.webp', '.bmp', 'image'];
        return imageExtensions.includes(type.toLowerCase());
    },

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
       return fetchApi('/Owner/Profile/UpdateBusiness', 'POST', payload);
    },

    updateAddressSettings: async (addressData) => fetchApi('/Owner/Profile/UpdateAddress', 'POST', addressData),

    updateServicesSettings: async (servicesData) => {
        const payload = JSON.parse(JSON.stringify(servicesData)); // Deep copy to handle complex objects

        if (servicesData.kitchenMedia) {
            const newMediaFiles = [];

            for (const media of servicesData.kitchenMedia) {
                if (media.fileObject) {
                    newMediaFiles.push(media.fileObject);
                }
            }

            payload.existingMediaPaths = servicesData.kitchenMedia
                .filter(item => item.filePath && !item.fileObject)
                .map(item => item.filePath);
            payload.newKitchenMediaFiles = await Promise.all(
                newMediaFiles.map(file => fileToBase64Dto(file))
            );
            delete payload.kitchenMedia;
        }

        return fetchApi('/Owner/Profile/UpdateServices', 'POST', payload);
    },

    updateLegalPaymentSettings: async (legalData) => fetchApi('/Owner/Profile/UpdateLegal', 'POST', legalData),

    // Menu Management

        // Food Pakages
        getFoodCategories: async () => fetchApi('/Owner/Menu/Packages/GetFoodCategory'),

        getPackages: async () => fetchApi('/Owner/Menu/Packages/GetPackages'),

        createPackage: async (packageData) => fetchApi('/Owner/Menu/Packages/AddPackage', 'POST', packageData),

        updatePackage: async (packageData) => fetchApi('/Owner/Menu/Packages/UpdatePackage', 'POST', packageData),

        deletePackage: async (packageId) => fetchApi('/Owner/Menu/Packages/DeletePackage', 'POST', packageId),

        // Food Items
        getFoodItems: async () => fetchApi('/Owner/Menu/FoodItem/Data'), 

        getCuisines: async () => fetchApi('/Owner/Menu/FoodItem/GetCuisineType'), 

        addFoodItem: async (itemData) => {

            const payload = JSON.parse(JSON.stringify(itemData)); // Deep copy to handle complex objects

            if (itemData.media) {
                const newMediaFiles = [];

                for (const media of itemData.media) {
                    if (media.fileObject) {
                        newMediaFiles.push(media.fileObject);
                    }
                }

                payload.foodItemMediaFiles = await Promise.all(
                    newMediaFiles.map(file => fileToBase64Dto(file))
                );
                delete payload.media;
            }

            return fetchApi('/Owner/Menu/FoodItem/Create', 'POST', payload);
        },

        updateFoodItem: async (itemData) => {

            const payload = JSON.parse(JSON.stringify(itemData)); // Deep copy to handle complex objects

            if (itemData.media) {
                const newMediaFiles = [];

                for (const media of itemData.media) {
                    if (media.fileObject) {
                        newMediaFiles.push(media.fileObject);
                    }
                }

                payload.existingFoodItemMediaPaths = itemData.media
                    .filter(item => item.filePath && !item.fileObject)
                    .map(item => item.filePath);
                payload.foodItemMediaFiles = await Promise.all(
                    newMediaFiles.map(file => fileToBase64Dto(file))
                );
                delete payload.media;
            }

            return fetchApi('/Owner/Menu/FoodItem/Udpate', 'POST', payload);
        },

        deleteFoodItem: async(itemId) => fetchApi('/Owner/Menu/FoodItem/Delete', 'POST', itemId),

    // Decorations Management

        getPackagesLookup: async () => fetchApi('/Owner/Menu/Packages/Lookup'),

        getDecorationThemes: async () => fetchApi('/Owner/Decorations/ThemeType'),

        getDecorations: async () => fetchApi('/Owner/Decorations/GetData'),

        addDecorations: async (itemData) => {

            const payload = JSON.parse(JSON.stringify(itemData)); // Deep copy to handle complex objects

            if (itemData.media) {
                const newMediaFiles = [];

                for (const media of itemData.media) {
                    if (media.fileObject) {
                        newMediaFiles.push(media.fileObject);
                    }
                }

                payload.DecorationsMediaFiles = await Promise.all(
                    newMediaFiles.map(file => fileToBase64Dto(file))
                );
                delete payload.media;
            }

            return fetchApi('/Owner/Decorations/Create', 'POST', payload);
        },

        updateDecorations: async (itemData) => {

            const payload = JSON.parse(JSON.stringify(itemData)); // Deep copy to handle complex objects

            if (itemData.media) {
                const newMediaFiles = [];

                for (const media of itemData.media) {
                    if (media.fileObject) {
                        newMediaFiles.push(media.fileObject);
                    }
                }
                payload.existingDecorationsMediaPaths = itemData.media
                    .filter(item => item.filePath && !item.fileObject)
                    .map(item => item.filePath);
                payload.DecorationsMediaFiles = await Promise.all(
                    newMediaFiles.map(file => fileToBase64Dto(file))
                );
                delete payload.media;
            }

            return fetchApi('/Owner/Decorations/Udpate', 'POST', payload);
        },

        deleteDecorations: async (itemId) => fetchApi('/Owner/Decorations/Delete', 'POST', itemId),

        
};  