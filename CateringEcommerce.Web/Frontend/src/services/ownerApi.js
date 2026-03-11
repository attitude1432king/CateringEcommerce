const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:44368';

import { fetchApi, fileToBase64Dto } from './apiUtils';

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

    uploadOwnerFiles: async (ownerId, uploadedFiles) => fetchApi(`/Auth/Owner/UploadMedia?ownerId=${ownerId}`, 'POST', uploadedFiles),

    // Get the Pincode details from the external API
    getPincodeDetails: async (pincode) => fetchApi(`/Common/Locations/pincode/${pincode}`),


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

        getPackageCount: async (searchQuery) => fetchApi(`/Owner/Menu/Packages/Count?searchPackage=${searchQuery}`),

        getPackagesData: async (currentPage, itemsPerPage, searchQuery) => fetchApi(`/Owner/Menu/Packages/Data?page=${currentPage}&pageSize=${itemsPerPage}&searchPackage=${searchQuery}`),

        createPackage: async (packageData) => fetchApi('/Owner/Menu/Packages/AddPackage', 'POST', packageData),

        updatePackage: async (packageData) => fetchApi('/Owner/Menu/Packages/UpdatePackage', 'POST', packageData),

        deletePackage: async (packageId) => fetchApi('/Owner/Menu/Packages/DeletePackage', 'POST', packageId),

        // Food Items

        getFoodItemsCount: async (filterJson) => fetchApi(`/Owner/Menu/FoodItem/Count?filterJson=${filterJson}`), 

        getFoodItems: async(currentPage, itemsPerPage, filterJson) => fetchApi(`/Owner/Menu/FoodItem/Data?page=${currentPage}&pageSize=${itemsPerPage}&filterJson=${filterJson}`), 

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

        deleteFoodItem: async (itemId) => fetchApi('/Owner/Menu/FoodItem/Delete', 'POST', itemId),

    // Lookup Data

        getPackagesLookup: async () => fetchApi('/Owner/Menu/Packages/Lookup'),

        getFoodItemsLookup: async () => fetchApi('/Owner/Menu/FoodItem/Lookup'),

    // Decorations Management


        getDecorationThemes: async () => fetchApi('/Owner/Decorations/ThemeType'),

        getDecorationsCount: async (filterJson) => fetchApi(`/Owner/Decorations/Count?filterJson=${filterJson}`),

        getDecorations: async (currentPage, itemsPerPage, filterJson) => fetchApi(`/Owner/Decorations/Data?page=${currentPage}&pageSize=${itemsPerPage}&filterJson=${filterJson}`),

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

        updateDecorationStatus: async (itemId, value) => fetchApi(`/Owner/Decorations/UpdateStatus?decorationId=${itemId}&status=${value}`, 'POST'),

    // Staff Management

        getStaffCount: async (filterJson) => fetchApi(`/Owner/Staff/Count?filterJson=${filterJson}`),

        getStaffList: async (currentPage, itemsPerPage, filterJson) => fetchApi(`/Owner/Staff/Data?page=${currentPage}&pageSize=${itemsPerPage}&filterJson=${filterJson}`),

        createStaffMember: async (staffData) => {
            // Deep copy input data (avoid mutation)
            const payload = JSON.parse(JSON.stringify(staffData));

            // Handle file uploads
            const fileConversions = [];

            // Photo upload
            if (staffData.photo.length > 0 && staffData.photo[0].fileObject) {
                fileConversions.push(
                    fileToBase64Dto(staffData.photo[0].fileObject).then(base64 => {
                        payload.profile = base64;
                    })
                );
                delete payload.photo;
            }

            // ID Proof upload
            if (staffData.idProof.length > 0 && staffData.idProof[0].fileObject) {
                fileConversions.push(
                    fileToBase64Dto(staffData.idProof[0].fileObject).then(base64 => {
                        payload.identityDocument = base64;
                    })
                );
                delete payload.idProof;
            }

            // Resume upload
            if (staffData.resume.length > 0 && staffData.resume[0].fileObject) {
                fileConversions.push(
                    fileToBase64Dto(staffData.resume[0].fileObject).then(base64 => {
                        payload.resumeDocument = base64;
                    })
                );
                delete payload.resume;
            }

            // Wait for all file conversions to complete
            await Promise.all(fileConversions);

            // Make API call
            return fetchApi('/Owner/Staff/Create', 'POST', payload);
        },

        updateStaffMember: async (staffData, filesToDelete = []) => {
            const payload = JSON.parse(JSON.stringify(staffData));

            // Handle file uploads
            const fileConversions = [];

            // Photo upload
            if (staffData.photo.length > 0 && staffData.photo[0].fileObject) {
                fileConversions.push(
                    fileToBase64Dto(staffData.photo[0].fileObject).then(base64 => {
                        payload.profile = base64;
                    })
                );
                delete payload.photo;
            }

            // ID Proof upload
            if (staffData.idProof.length > 0 && staffData.idProof[0].fileObject) {
                fileConversions.push(
                    fileToBase64Dto(staffData.idProof[0].fileObject).then(base64 => {
                        payload.identityDocument = base64;
                    })
                );
                delete payload.idProof;
            }

            // Resume upload
            if (staffData.resume.length > 0 && staffData.resume[0].fileObject) {
                fileConversions.push(
                    fileToBase64Dto(staffData.resume[0].fileObject).then(base64 => {
                        payload.resumeDocument = base64;
                    })
                );
                delete payload.resume;
            }

            payload.filesToDelete = filesToDelete;

            // Wait for all file conversions to complete
            await Promise.all(fileConversions);

            // Make API call
            return fetchApi('/Owner/Staff/Update', 'POST', payload);
        },

        deleteStaffMember: async (itemId) => fetchApi('/Owner/Staff/Delete', 'POST', itemId),

        updateStaffStatus: async (itemId, value) => fetchApi(`/Owner/Staff/UpdateStatus?staffId=${itemId}&status=${value}`, 'POST'),


    // Discounts Management

        getDiscountCount: async (filterJson) => fetchApi(`/Owner/Discounts/Count?filterJson=${filterJson}`),

        getDiscountList: async (currentPage, itemsPerPage, filterJson) => fetchApi(`/Owner/Discounts/Data?page=${currentPage}&pageSize=${itemsPerPage}&filterJson=${filterJson}`),

        createDiscount: async (discountData) => fetchApi('/Owner/Discounts/Create', 'POST', discountData),

        updateDiscount: async (discountData) => fetchApi('/Owner/Discounts/Update', 'POST', discountData),

        deleteDiscount: async (discountId) => fetchApi(`/Owner/Discounts/${discountId}`, 'DELETE'),

    // Availability Management

        getAvailability: async (year, month) => fetchApi(`/Owner/Availability/GetAvailability?Year=${year}&month=${month}`),

        updateGlobalAvailability: async (status) => fetchApi(`/Owner/Availability/UpdateStatus`, 'POST', status),

        updateDateAvailability: async (dateData) => fetchApi('/Owner/Availability/UpdateDateStatus', 'POST', dateData),

    // Banner Management

        getBannersCount: async (filterJson) => fetchApi(`/Owner/Banners/Count?filterJson=${filterJson}`),

        getBannersList: async (currentPage, itemsPerPage, filterJson) => fetchApi(`/Owner/Banners/Data?page=${currentPage}&pageSize=${itemsPerPage}&filterJson=${filterJson}`),

        createBanner: async (bannerData) => fetchApi('/Owner/Banners/Create', 'POST', bannerData),

        updateBanner: async (bannerData) => fetchApi('/Owner/Banners/Update', 'POST', bannerData),

        deleteBanner: async (bannerId) => fetchApi('/Owner/Banners/Delete', 'POST', bannerId),

        updateBannerStatus: async (bannerId, isActive) => fetchApi(`/Owner/Banners/UpdateStatus?bannerId=${bannerId}&isActive=${isActive}`, 'POST'),

    // ===================================
    // Dashboard APIs
    // ===================================

        // Dashboard Metrics & Charts
        getDashboardMetrics: async () => fetchApi('/Owner/OwnerDashboard/metrics'),

        getRevenueChart: async (period = 'month') => fetchApi(`/Owner/OwnerDashboard/revenue-chart?period=${period}`),

        getOrdersChart: async (period = 'month') => fetchApi(`/Owner/OwnerDashboard/orders-chart?period=${period}`),

        getRecentOrders: async (limit = 5) => fetchApi(`/Owner/OwnerDashboard/recent-orders?limit=${limit}`),

        getUpcomingEvents: async (days = 7) => fetchApi(`/Owner/OwnerDashboard/upcoming-events?days=${days}`),

        getTopMenuItems: async (limit = 10) => fetchApi(`/Owner/OwnerDashboard/top-items?limit=${limit}`),

        getPerformanceInsights: async () => fetchApi('/Owner/OwnerDashboard/insights'),

        getRevenueBreakdown: async () => fetchApi('/Owner/OwnerDashboard/revenue-breakdown'),

    // ===================================
    // Order Management APIs
    // ===================================

        // Get paginated orders list with filters
        getOrdersList: async (page = 1, pageSize = 10, filters = {}) =>
            fetchApi('/Owner/OwnerOrders/list', 'POST', {
                page,
                pageSize,
                ...filters
            }),

        // Get complete order details
        getOrderDetails: async (orderId) => fetchApi(`/Owner/OwnerOrders/${orderId}`),

        // Update order status
        updateOrderStatus: async (orderId, statusData) =>
            fetchApi(`/Owner/OwnerOrders/${orderId}/status`, 'PUT', statusData),

        // Get order status history
        getOrderStatusHistory: async (orderId) => fetchApi(`/Owner/OwnerOrders/${orderId}/history`),

        // Get order statistics
        getOrderStats: async () => fetchApi('/Owner/OwnerOrders/stats'),

        // Get booking request statistics (today/week/month)
        getBookingRequestStats: async () => fetchApi('/Owner/OwnerOrders/booking-request-stats'),

    // ===================================
    // Customer Management APIs
    // ===================================

        // Get paginated customers list with filters
        getCustomersList: async (page = 1, pageSize = 10, filters = {}) =>
            fetchApi('/Owner/OwnerCustomers/list', 'POST', {
                page,
                pageSize,
                ...filters
            }),

        // Get customer details with statistics
        getCustomerDetails: async (customerId) => fetchApi(`/Owner/OwnerCustomers/${customerId}`),

        // Get customer order history
        getCustomerOrderHistory: async (customerId) => fetchApi(`/Owner/OwnerCustomers/${customerId}/orders`),

        // Get customer insights and analytics
        getCustomerInsights: async () => fetchApi('/Owner/OwnerCustomers/insights'),

        // Get top customers
        getTopCustomers: async (limit = 10, sortBy = 'LifetimeValue') =>
            fetchApi(`/Owner/OwnerCustomers/top?limit=${limit}&sortBy=${sortBy}`),

    // ===================================
    // Reports APIs
    // ===================================

        // Generate sales report
        generateSalesReport: async (filters = {}) =>
            fetchApi('/Owner/OwnerReports/sales', 'POST', filters),

        // Generate revenue report
        generateRevenueReport: async (filters = {}) =>
            fetchApi('/Owner/OwnerReports/revenue', 'POST', filters),

        // Generate customer report
        generateCustomerReport: async (filters = {}) =>
            fetchApi('/Owner/OwnerReports/customers', 'POST', filters),

        // Generate menu performance report
        generateMenuPerformanceReport: async (filters = {}) =>
            fetchApi('/Owner/OwnerReports/menu-performance', 'POST', filters),

        // Generate financial report
        generateFinancialReport: async (filters = {}) =>
            fetchApi('/Owner/OwnerReports/financial', 'POST', filters),

    // ===================================
    // Review Management APIs
    // ===================================

        // Get paginated reviews list with filters
        getReviews: async (page = 1, pageSize = 10, filters = {}) =>
            fetchApi('/Owner/OwnerReviews/list', 'POST', {
                page,
                pageSize,
                ...filters
            }),

        // Get review statistics
        getReviewStats: async () => fetchApi('/Owner/OwnerReviews/stats'),

        // Submit owner reply to a review
        submitReviewReply: async (reviewId, replyText) =>
            fetchApi(`/Owner/OwnerReviews/${reviewId}/reply`, 'POST', { replyText }),

    // ===================================
    // Support Ticket APIs
    // ===================================

        // Create a new support ticket
        createSupportTicket: async (ticketData) =>
            fetchApi('/Owner/OwnerSupport/create', 'POST', ticketData),

        // Get paginated tickets list with filters
        getSupportTickets: async (page = 1, pageSize = 10, filters = {}) =>
            fetchApi('/Owner/OwnerSupport/list', 'POST', {
                page,
                pageSize,
                ...filters
            }),

        // Get ticket details with messages
        getSupportTicketDetail: async (ticketId) =>
            fetchApi(`/Owner/OwnerSupport/${ticketId}`),

        // Send a message on a ticket
        sendTicketMessage: async (ticketId, messageText) =>
            fetchApi(`/Owner/OwnerSupport/${ticketId}/message`, 'POST', { messageText }),

        // Get ticket statistics
        getSupportTicketStats: async () => fetchApi('/Owner/OwnerSupport/stats'),

    // ===================================
    // Report Export APIs
    // ===================================

        // Export report (returns file download URL or blob)
        exportReport: async (reportType, format = 'csv', filters = {}) => {
            const response = await fetch(`${API_BASE_URL}/api/Owner/OwnerReports/export?type=${reportType}&format=${format}`, {
                method: 'POST',
                credentials: 'include',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(filters)
            });

            if (!response.ok) {
                throw new Error('Failed to export report');
            }

            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);
            const filename = response.headers.get('content-disposition')?.split('filename=')[1] || `${reportType}_report.${format}`;

            // Trigger download
            const link = document.createElement('a');
            link.href = url;
            link.download = filename.replace(/"/g, '');
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            window.URL.revokeObjectURL(url);

            return { success: true, message: 'Report exported successfully' };
        },

};