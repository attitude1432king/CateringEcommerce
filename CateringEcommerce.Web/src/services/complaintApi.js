import { fetchApi } from './apiUtils';

// ===================================
// FILE COMPLAINT
// ===================================
export const fileComplaint = async (complaintData) => {
  try {
    const response = await fetchApi('/User/Complaint/File', 'POST', complaintData);
    return response;
  } catch (error) {
    console.error('Error filing complaint:', error);
    throw error;
  }
};

// ===================================
// GET MY COMPLAINTS
// ===================================
export const getMyComplaints = async () => {
  try {
    const response = await fetchApi('/User/Complaint/My-Complaints', 'GET');
    return response;
  } catch (error) {
    console.error('Error fetching user complaints:', error);
    throw error;
  }
};

// ===================================
// GET COMPLAINT DETAILS
// ===================================
export const getComplaintDetails = async (complaintId) => {
  try {
    const response = await fetchApi(`/User/Complaint/${complaintId}`, 'GET');
    return response;
  } catch (error) {
    console.error(`Error fetching complaint details for complaint ${complaintId}:`, error);
    throw error;
  }
};

// ===================================
// GET ORDER COMPLAINTS
// ===================================
export const getOrderComplaints = async (orderId) => {
  try {
    const response = await fetchApi(`/User/Complaint/Order/${orderId}`, 'GET');
    return response;
  } catch (error) {
    console.error(`Error fetching complaints for order ${orderId}:`, error);
    throw error;
  }
};
