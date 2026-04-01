/**
 * Assignment API
 * Handles supervisor assignments (accept, reject, check-in, payment requests)
 * Routes: api/Supervisor/SupervisorAssignment/*
 */

import apiClient, { handleApiResponse, handleApiError } from './apiConfig';

const BASE = '/api/Supervisor/SupervisorAssignment';

// =====================================================
// SUPERVISOR APIs
// =====================================================

export const getMyAssignments = async (status = null) => {
    try {
        const response = await apiClient.get(`${BASE}/my-assignments`, {
            params: { status },
        });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const getAssignmentById = async (assignmentId) => {
    try {
        const response = await apiClient.get(`${BASE}/${assignmentId}`);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const getAssignmentsBySupervisor = async (supervisorId) => {
    try {
        const response = await apiClient.get(`${BASE}/${supervisorId}`);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const acceptAssignment = async (assignmentId, notes) => {
    try {
        const response = await apiClient.post(`${BASE}/accept`, {
            assignmentId,
            notes,
        });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const rejectAssignment = async (assignmentId, reason) => {
    try {
        const response = await apiClient.post(`${BASE}/reject`, {
            assignmentId,
            reason,
        });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const checkIn = async (checkInData) => {
    try {
        const response = await apiClient.post(`${BASE}/checkin`, checkInData);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const requestPaymentRelease = async (assignmentId, amount, notes) => {
    try {
        const response = await apiClient.post(`${BASE}/request-payment`, {
            assignmentId,
            amount,
            notes,
        });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const completeAssignment = async (assignmentId, completionNotes) => {
    try {
        const response = await apiClient.post(`${BASE}/complete`, {
            assignmentId,
            completionNotes,
        });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

// =====================================================
// ADMIN APIs
// =====================================================

export const findEligibleSupervisors = async (criteria) => {
    try {
        const response = await apiClient.post(`${BASE}/admin/find-eligible`, criteria);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const assignSupervisorToEvent = async (assignment) => {
    try {
        const response = await apiClient.post(`${BASE}/admin/assign`, assignment);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const bulkAssign = async (assignments) => {
    try {
        const response = await apiClient.post(`${BASE}/admin/bulk-assign`, { assignments });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const getAssignmentsByOrder = async (orderId) => {
    try {
        const response = await apiClient.get(`${BASE}/admin/by-order/${orderId}`);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const getAllAssignments = async (fromDate = null, toDate = null) => {
    try {
        const response = await apiClient.get(`${BASE}/admin/all`, {
            params: {
                fromDate: fromDate?.toISOString(),
                toDate: toDate?.toISOString(),
            },
        });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const searchAssignments = async (filters) => {
    try {
        const response = await apiClient.post(`${BASE}/admin/search`, filters);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const updateAssignmentStatus = async (assignmentId, newStatus, notes) => {
    try {
        const response = await apiClient.put(`${BASE}/admin/update-status`, {
            assignmentId,
            newStatus,
            notes,
        });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const cancelAssignment = async (assignmentId, reason) => {
    try {
        const response = await apiClient.post(`${BASE}/admin/cancel`, {
            assignmentId,
            reason,
        });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const approvePaymentRelease = async (assignmentId, notes) => {
    try {
        const response = await apiClient.post(`${BASE}/admin/approve-payment`, {
            assignmentId,
            notes,
        });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const getUpcomingAssignments = async (daysAhead = 7) => {
    try {
        const response = await apiClient.get(`${BASE}/admin/upcoming`, {
            params: { daysAhead },
        });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const getOverdueAssignments = async () => {
    try {
        const response = await apiClient.get(`${BASE}/admin/overdue`);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const getAssignmentStatistics = async () => {
    try {
        const response = await apiClient.get(`${BASE}/admin/statistics`);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const getSupervisorWorkload = async (supervisorId) => {
    try {
        const response = await apiClient.get(`${BASE}/admin/workload/${supervisorId}`);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};
