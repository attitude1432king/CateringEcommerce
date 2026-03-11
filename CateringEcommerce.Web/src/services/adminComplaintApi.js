import { fetchApi } from './apiUtils';

// ===================================
// GET PENDING COMPLAINTS
// ===================================
export const getPendingComplaints = async () => {
    try {
        const response = await fetchApi('/admin/complaint/pending', 'GET');
        return response;
    } catch (error) {
        console.error('Error fetching pending complaints:', error);
        throw error;
    }
};

// ===================================
// GET COMPLAINT DETAILS (ADMIN VIEW)
// ===================================
export const getComplaintDetailsAdmin = async (complaintId) => {
    try {
        const response = await fetchApi(`/admin/complaint/${complaintId}`, 'GET');
        return response;
    } catch (error) {
        console.error(`Error fetching complaint details for complaint ${complaintId}:`, error);
        throw error;
    }
};

// ===================================
// CALCULATE REFUND FOR COMPLAINT
// ===================================
export const calculateComplaintRefund = async (complaintId) => {
    try {
        const response = await fetchApi(`/admin/complaint/calculate-refund/${complaintId}`, 'POST');
        return response;
    } catch (error) {
        console.error(`Error calculating refund for complaint ${complaintId}:`, error);
        throw error;
    }
};

// ===================================
// RESOLVE COMPLAINT
// ===================================
export const resolveComplaint = async (resolutionData) => {
    try {
        const response = await fetchApi('/admin/complaint/resolve', 'POST', resolutionData);
        return response;
    } catch (error) {
        console.error('Error resolving complaint:', error);
        throw error;
    }
};

// ===================================
// ESCALATE COMPLAINT
// ===================================
export const escalateComplaint = async (complaintId) => {
    try {
        const response = await fetchApi(`/admin/complaint/escalate/${complaintId}`, 'POST');
        return response;
    } catch (error) {
        console.error(`Error escalating complaint ${complaintId}:`, error);
        throw error;
    }
};
