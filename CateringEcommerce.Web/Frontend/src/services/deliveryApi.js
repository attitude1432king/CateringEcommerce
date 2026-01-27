import { fetchApi } from './apiUtils';

// ========================================
// USER ENDPOINTS
// ========================================

// ===================================
// SAMPLE DELIVERY - Get by Order ID
// ===================================
export const getSampleDelivery = async (orderId) => {
  try {
    const response = await fetchApi(`/User/SampleDelivery/${orderId}`, 'GET');
    return response;
  } catch (error) {
    console.error(`Error fetching sample delivery for order ${orderId}:`, error);
    throw error;
  }
};

// ===================================
// SAMPLE DELIVERY - Get Tracking Info
// ===================================
export const getSampleDeliveryTracking = async (orderId) => {
  try {
    const response = await fetchApi(`/User/SampleDelivery/track/${orderId}`, 'GET');
    return response;
  } catch (error) {
    console.error(`Error fetching sample delivery tracking for order ${orderId}:`, error);
    throw error;
  }
};

// ===================================
// EVENT DELIVERY - Get by Order ID (User)
// ===================================
export const getEventDelivery = async (orderId) => {
  try {
    const response = await fetchApi(`/User/EventDelivery/${orderId}`, 'GET');
    return response;
  } catch (error) {
    console.error(`Error fetching event delivery for order ${orderId}:`, error);
    throw error;
  }
};

// ===================================
// EVENT DELIVERY - Get Timeline (User)
// ===================================
export const getEventDeliveryTimeline = async (orderId) => {
  try {
    const response = await fetchApi(`/User/EventDelivery/timeline/${orderId}`, 'GET');
    return response;
  } catch (error) {
    console.error(`Error fetching event delivery timeline for order ${orderId}:`, error);
    throw error;
  }
};

// ========================================
// PARTNER (OWNER) ENDPOINTS
// ========================================

// ===================================
// EVENT DELIVERY - Initialize (Partner)
// ===================================
export const initEventDelivery = async (deliveryData) => {
  try {
    const response = await fetchApi('/Owner/EventDelivery/init', 'POST', deliveryData);
    return response;
  } catch (error) {
    console.error('Error initializing event delivery:', error);
    throw error;
  }
};

// ===================================
// EVENT DELIVERY - Update Status (Partner)
// ===================================
export const updateEventDeliveryStatus = async (statusUpdate) => {
  try {
    const response = await fetchApi('/Owner/EventDelivery/update-status', 'PUT', statusUpdate);
    return response;
  } catch (error) {
    console.error('Error updating event delivery status:', error);
    throw error;
  }
};

// ===================================
// EVENT DELIVERY - Get Active Deliveries (Partner)
// ===================================
export const getPartnerActiveDeliveries = async () => {
  try {
    const response = await fetchApi('/Owner/EventDelivery/active', 'GET');
    return response;
  } catch (error) {
    console.error('Error fetching partner active deliveries:', error);
    throw error;
  }
};

// ===================================
// EVENT DELIVERY - Get by Order ID (Partner)
// ===================================
export const getPartnerEventDelivery = async (orderId) => {
  try {
    const response = await fetchApi(`/Owner/EventDelivery/${orderId}`, 'GET');
    return response;
  } catch (error) {
    console.error(`Error fetching partner event delivery for order ${orderId}:`, error);
    throw error;
  }
};

// ===================================
// EVENT DELIVERY - Get Timeline (Partner)
// ===================================
export const getPartnerEventDeliveryTimeline = async (orderId) => {
  try {
    const response = await fetchApi(`/Owner/EventDelivery/timeline/${orderId}`, 'GET');
    return response;
  } catch (error) {
    console.error(`Error fetching partner event delivery timeline for order ${orderId}:`, error);
    throw error;
  }
};

// ========================================
// ADMIN ENDPOINTS
// ========================================

// ===================================
// ADMIN - Get All Deliveries for Monitoring
// ===================================
export const getAdminDeliveryMonitor = async () => {
  try {
    const response = await fetchApi('/Admin/DeliveryMonitor', 'GET');
    return response;
  } catch (error) {
    console.error('Error fetching admin delivery monitor:', error);
    throw error;
  }
};

// ===================================
// ADMIN - Get Delivery by Order ID
// ===================================
export const getAdminDeliveryByOrder = async (orderId) => {
  try {
    const response = await fetchApi(`/Admin/DeliveryMonitor/${orderId}`, 'GET');
    return response;
  } catch (error) {
    console.error(`Error fetching admin delivery for order ${orderId}:`, error);
    throw error;
  }
};

// ===================================
// ADMIN - Get Delivery Timeline
// ===================================
export const getAdminDeliveryTimeline = async (orderId) => {
  try {
    const response = await fetchApi(`/Admin/DeliveryMonitor/timeline/${orderId}`, 'GET');
    return response;
  } catch (error) {
    console.error(`Error fetching admin delivery timeline for order ${orderId}:`, error);
    throw error;
  }
};

// ===================================
// ADMIN - Override Delivery Status
// ===================================
export const adminOverrideDeliveryStatus = async (overrideData) => {
  try {
    const response = await fetchApi('/Admin/DeliveryMonitor/override', 'POST', overrideData);
    return response;
  } catch (error) {
    console.error('Error overriding delivery status:', error);
    throw error;
  }
};

// ========================================
// DELIVERY STATUS CONSTANTS
// ========================================

export const SAMPLE_DELIVERY_STATUS = {
  REQUESTED: 1,
  PICKED_UP: 2,
  IN_TRANSIT: 3,
  DELIVERED: 4,
  FAILED: 5,
};

export const EVENT_DELIVERY_STATUS = {
  PREPARATION_STARTED: 1,
  VEHICLE_READY: 2,
  DISPATCHED: 3,
  ARRIVED_AT_VENUE: 4,
  EVENT_COMPLETED: 5,
};

// ========================================
// HELPER FUNCTIONS
// ========================================

export const getSampleDeliveryStatusText = (status) => {
  const statusMap = {
    [SAMPLE_DELIVERY_STATUS.REQUESTED]: 'Requested',
    [SAMPLE_DELIVERY_STATUS.PICKED_UP]: 'Picked Up',
    [SAMPLE_DELIVERY_STATUS.IN_TRANSIT]: 'In Transit',
    [SAMPLE_DELIVERY_STATUS.DELIVERED]: 'Delivered',
    [SAMPLE_DELIVERY_STATUS.FAILED]: 'Failed',
  };
  return statusMap[status] || 'Unknown';
};

export const getEventDeliveryStatusText = (status) => {
  const statusMap = {
    [EVENT_DELIVERY_STATUS.PREPARATION_STARTED]: 'Food Preparation Started',
    [EVENT_DELIVERY_STATUS.VEHICLE_READY]: 'Vehicle Ready',
    [EVENT_DELIVERY_STATUS.DISPATCHED]: 'Dispatched',
    [EVENT_DELIVERY_STATUS.ARRIVED_AT_VENUE]: 'Arrived at Venue',
    [EVENT_DELIVERY_STATUS.EVENT_COMPLETED]: 'Event Completed',
  };
  return statusMap[status] || 'Unknown';
};

export const getEventDeliveryStatusIcon = (status) => {
  const iconMap = {
    [EVENT_DELIVERY_STATUS.PREPARATION_STARTED]: '🍳',
    [EVENT_DELIVERY_STATUS.VEHICLE_READY]: '🚛',
    [EVENT_DELIVERY_STATUS.DISPATCHED]: '🚚',
    [EVENT_DELIVERY_STATUS.ARRIVED_AT_VENUE]: '📍',
    [EVENT_DELIVERY_STATUS.EVENT_COMPLETED]: '✅',
  };
  return iconMap[status] || '❓';
};
