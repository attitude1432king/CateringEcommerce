import { fetchApi } from './apiUtils';

// ===================================
// CREATE ORDER
// ===================================
export const createOrder = async (orderData) => {
  try {
    const response = await fetchApi('/User/Orders/Create', 'POST', orderData);
    return response;
  } catch (error) {
    console.error('Error creating order:', error);
    throw error;
  }
};

// ===================================
// GET USER ORDERS (PAGINATED)
// ===================================
export const getUserOrders = async (pageNumber = 1, pageSize = 10) => {
  try {
    const response = await fetchApi('/User/Orders', 'GET', null, {
      pageNumber,
      pageSize,
    });
    return response;
  } catch (error) {
    console.error('Error fetching user orders:', error);
    throw error;
  }
};

// ===================================
// GET ORDER DETAILS BY ID
// ===================================
export const getOrderDetails = async (orderId) => {
  try {
    const response = await fetchApi(`/User/Orders/${orderId}`, 'GET');
    return response;
  } catch (error) {
    console.error(`Error fetching order details for order ${orderId}:`, error);
    throw error;
  }
};

// ===================================
// CANCEL ORDER
// ===================================
export const cancelOrder = async (orderId, reason) => {
  try {
    const response = await fetchApi(`/User/Orders/${orderId}/Cancel`, 'POST', {
      reason,
    });
    return response;
  } catch (error) {
    console.error(`Error cancelling order ${orderId}:`, error);
    throw error;
  }
};
