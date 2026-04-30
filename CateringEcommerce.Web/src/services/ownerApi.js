const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

import { fetchApi } from './apiUtils';

// Helper: append all scalar/array fields to FormData (skips File/Blob values)
function appendFields(fd, obj) {
    Object.entries(obj).forEach(([key, val]) => {
        if (val === null || val === undefined) return;
        if (val instanceof File || val instanceof Blob) return;
        if (Array.isArray(val)) val.forEach(v => fd.append(key, String(v)));
        else fd.append(key, String(val));
    });
}

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

    registerOwner: async (formData) => {
        const { cateringLogo, fssaiCertificate, gstCertificate, panCard, signature, chequeCopy, ...rest } = formData;
        const fd = new FormData();
        fd.append('JsonData', JSON.stringify(rest));
        if (cateringLogo) fd.append('CateringLogo', cateringLogo, cateringLogo.name);
        if (fssaiCertificate) fd.append('FssaiCertificate', fssaiCertificate, fssaiCertificate.name);
        if (gstCertificate) fd.append('GstCertificate', gstCertificate, gstCertificate.name);
        if (panCard) fd.append('PanCard', panCard, panCard.name);
        if (signature) fd.append('Signature', signature, 'signature.png');
        if (chequeCopy) fd.append('ChequeCopy', chequeCopy, chequeCopy.name);
        return fetchApi(`/Auth/Owner/Register`, 'POST', fd);
    },

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
        const { newLogoFile, ...rest } = businessData;
        const fd = new FormData();
        appendFields(fd, rest);
        if (newLogoFile) fd.append('NewLogoFile', newLogoFile);
        return fetchApi('/Owner/Profile/UpdateBusiness', 'POST', fd);
    },

    updateAddressSettings: async (addressData) => fetchApi('/Owner/Profile/UpdateAddress', 'POST', addressData),

    updateServicesSettings: async (servicesData) => {
        const { kitchenMedia, ...rest } = servicesData;
        const fd = new FormData();
        appendFields(fd, rest);
        if (kitchenMedia) {
            kitchenMedia
                .filter(item => item.filePath && !item.fileObject)
                .forEach(item => fd.append('ExistingMediaPaths', item.filePath));
            kitchenMedia
                .filter(item => item.fileObject)
                .forEach(item => fd.append('NewKitchenMediaFiles', item.fileObject));
        }
        return fetchApi('/Owner/Profile/UpdateServices', 'POST', fd);
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
            const { media, ...rest } = itemData;
            const fd = new FormData();
            appendFields(fd, rest);
            if (media) {
                media.filter(m => m.fileObject).forEach(m => fd.append('FoodItemMediaFiles', m.fileObject));
            }
            return fetchApi('/Owner/Menu/FoodItem/Create', 'POST', fd);
        },

        updateFoodItem: async (itemData) => {
            const { media, ...rest } = itemData;
            const fd = new FormData();
            appendFields(fd, rest);
            if (media) {
                media.filter(m => m.filePath && !m.fileObject).forEach(m => fd.append('ExistingFoodItemMediaPaths', m.filePath));
                media.filter(m => m.fileObject).forEach(m => fd.append('FoodItemMediaFiles', m.fileObject));
            }
            return fetchApi('/Owner/Menu/FoodItem/Udpate', 'POST', fd);
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
            const { media, ...rest } = itemData;
            const fd = new FormData();
            appendFields(fd, rest);
            if (media) {
                media.filter(m => m.fileObject).forEach(m => fd.append('DecorationsMediaFiles', m.fileObject));
            }
            return fetchApi('/Owner/Decorations/Create', 'POST', fd);
        },

        updateDecorations: async (itemData) => {
            const { media, ...rest } = itemData;
            const fd = new FormData();
            appendFields(fd, rest);
            if (media) {
                media.filter(m => m.filePath && !m.fileObject).forEach(m => fd.append('ExistingDecorationsMediaPaths', m.filePath));
                media.filter(m => m.fileObject).forEach(m => fd.append('DecorationsMediaFiles', m.fileObject));
            }
            return fetchApi('/Owner/Decorations/Udpate', 'POST', fd);
        },

        deleteDecorations: async (itemId) => fetchApi('/Owner/Decorations/Delete', 'POST', itemId),

        updateDecorationStatus: async (itemId, value) => fetchApi(`/Owner/Decorations/UpdateStatus?decorationId=${itemId}&status=${value}`, 'POST'),

    // Staff Management

        getStaffCount: async (filterJson) => fetchApi(`/Owner/Staff/Count?filterJson=${filterJson}`),

        getStaffList: async (currentPage, itemsPerPage, filterJson) => fetchApi(`/Owner/Staff/Data?page=${currentPage}&pageSize=${itemsPerPage}&filterJson=${filterJson}`),

        createStaffMember: async (staffData) => {
            const { photo, idProof, resume, ...rest } = staffData;
            const fd = new FormData();
            fd.append('JsonData', JSON.stringify(rest));
            if (photo?.[0]?.fileObject) fd.append('Profile', photo[0].fileObject);
            if (idProof?.[0]?.fileObject) fd.append('IdentityDocument', idProof[0].fileObject);
            if (resume?.[0]?.fileObject) fd.append('ResumeDocument', resume[0].fileObject);
            return fetchApi('/Owner/Staff/Create', 'POST', fd);
        },

        updateStaffMember: async (staffData, filesToDelete = []) => {
            const { photo, idProof, resume, ...rest } = staffData;
            const fd = new FormData();
            fd.append('JsonData', JSON.stringify({ ...rest, filesToDelete }));
            if (photo?.[0]?.fileObject) fd.append('Profile', photo[0].fileObject);
            if (idProof?.[0]?.fileObject) fd.append('IdentityDocument', idProof[0].fileObject);
            if (resume?.[0]?.fileObject) fd.append('ResumeDocument', resume[0].fileObject);
            return fetchApi('/Owner/Staff/Update', 'POST', fd);
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

        createBanner: async (bannerData) => {
            const { bannerImage, ...rest } = bannerData;
            const fd = new FormData();
            appendFields(fd, rest);
            if (bannerImage) fd.append('BannerImage', bannerImage);
            return fetchApi('/Owner/Banners/Create', 'POST', fd);
        },

        updateBanner: async (bannerData) => {
            const { bannerImage, ...rest } = bannerData;
            const fd = new FormData();
            appendFields(fd, rest);
            if (bannerImage) fd.append('BannerImage', bannerImage);
            return fetchApi('/Owner/Banners/Update', 'POST', fd);
        },

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

        // Get paginated sample tasting requests list
        getSampleRequestsList: async (page = 1, pageSize = 20, filters = {}) =>
            fetchApi('/Owner/OwnerOrders/sample-list', 'POST', { page, pageSize, ...filters }),

        // Accept or Reject a sample tasting request
        actionSampleRequest: async (sampleOrderId, actionData) =>
            fetchApi(`/Owner/OwnerOrders/sample/${sampleOrderId}/action`, 'PUT', actionData),

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