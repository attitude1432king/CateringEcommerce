import { fetchApi } from './apiUtils';

/**
 * Homepage API Services
 */

/**
 * Get featured caterers for homepage display
 * @returns {Promise<Object>} Featured caterers data
 */
export const getFeaturedCaterers = async () => {
    return fetchApi('/User/Home/FeaturedCaterers', 'GET');
};

/**
 * Get testimonials for homepage display
 * @returns {Promise<Object>} Testimonials data
 */
export const getTestimonials = async () => {
    return fetchApi('/User/Home/Testimonials', 'GET');
};

/**
 * Get homepage statistics
 * @returns {Promise<Object>} Homepage stats data
 */
export const getHomePageStats = async () => {
    return fetchApi('/User/Home/Stats', 'GET');
};

/**
 * Get verified catering list
 * @param {string} cityName - Optional city name to filter
 * @returns {Promise<Object>} Catering list data
 */
export const getCateringList = async (cityName = '') => {
    const queryParams = cityName ? { cityName } : {};
    return fetchApi('/User/Home/CateringList', 'GET', null, queryParams);
};

/**
 * Comprehensive search for catering services
 * @param {Object} searchParams - Search parameters
 * @param {string} searchParams.city - City name for location-based search
 * @param {string} searchParams.cuisineTypes - Comma-separated cuisine type IDs
 * @param {string} searchParams.serviceTypes - Comma-separated service type IDs
 * @param {string} searchParams.eventTypes - Comma-separated event type IDs
 * @param {string} searchParams.keyword - Search keyword
 * @param {number} searchParams.minRating - Minimum average rating filter
 * @param {boolean} searchParams.onlineOnly - Filter for only online caterers
 * @param {boolean} searchParams.verifiedOnly - Filter for only verified caterers
 * @param {number} searchParams.minOrderFrom - Minimum order value range - from
 * @param {number} searchParams.minOrderTo - Minimum order value range - to
 * @param {number} searchParams.deliveryRadius - Delivery radius in km
 * @param {number} searchParams.pageNumber - Page number (default: 1)
 * @param {number} searchParams.pageSize - Results per page (default: 20)
 * @returns {Promise<Object>} Search results with pagination
 */
export const searchCaterings = async (searchParams = {}) => {
    const {
        city,
        cuisineTypes,
        serviceTypes,
        eventTypes,
        keyword,
        minRating,
        onlineOnly,
        verifiedOnly = true,
        minOrderFrom,
        minOrderTo,
        deliveryRadius,
        pageNumber = 1,
        pageSize = 20
    } = searchParams;

    const queryParams = {};

    if (city) queryParams.city = city;
    if (cuisineTypes) queryParams.cuisineTypes = cuisineTypes;
    if (serviceTypes) queryParams.serviceTypes = serviceTypes;
    if (eventTypes) queryParams.eventTypes = eventTypes;
    if (keyword) queryParams.keyword = keyword;
    if (minRating !== undefined) queryParams.minRating = minRating;
    if (onlineOnly !== undefined) queryParams.onlineOnly = onlineOnly;
    if (verifiedOnly !== undefined) queryParams.verifiedOnly = verifiedOnly;
    if (minOrderFrom !== undefined) queryParams.minOrderFrom = minOrderFrom;
    if (minOrderTo !== undefined) queryParams.minOrderTo = minOrderTo;
    if (deliveryRadius !== undefined) queryParams.deliveryRadius = deliveryRadius;
    queryParams.pageNumber = pageNumber;
    queryParams.pageSize = pageSize;

    return fetchApi('/User/Home/Search', 'GET', null, queryParams);
};

/**
 * Get active banners for homepage
 * @returns {Promise<Array>} Active banners data
 */
export const getActiveBanners = async () => {
    return fetchApi('/User/Banners/Active', 'GET');
};

/**
 * Track banner view
 * @param {number} bannerId - Banner ID
 * @returns {Promise<void>}
 */
export const trackBannerView = async (bannerId) => {
    return fetchApi('/User/Banners/TrackView', 'POST', bannerId);
};

/**
 * Track banner click
 * @param {number} bannerId - Banner ID
 * @returns {Promise<void>}
 */
export const trackBannerClick = async (bannerId) => {
    return fetchApi('/User/Banners/TrackClick', 'POST', bannerId);
};

export const homeApiService = {
    getFeaturedCaterers,
    getTestimonials,
    getHomePageStats,
    getCateringList,
    searchCaterings,
    getActiveBanners,
    trackBannerView,
    trackBannerClick,
};
