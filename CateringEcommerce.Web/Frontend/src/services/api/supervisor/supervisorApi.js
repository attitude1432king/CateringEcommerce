/**
 * Supervisor Core API
 * Handles supervisor CRUD, dashboard, profile, permissions
 * Routes: api/Supervisor/SupervisorManagement/*
 */

import apiClient, { handleApiResponse, handleApiError } from './apiConfig';

const BASE = '/api/Supervisor/SupervisorManagement';

// =====================================================
// SUPERVISOR SELF-SERVICE
// =====================================================

export const getDashboard = async () => {
    try {
        const response = await apiClient.get(`${BASE}/dashboard`);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const getProfile = async () => {
    try {
        const response = await apiClient.get(`${BASE}/profile`);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const updateProfile = async (updates) => {
    try {
        const response = await apiClient.put(`${BASE}/profile`, updates);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const getMyAuthority = async () => {
    try {
        const response = await apiClient.get(`${BASE}/authority`);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const getAvailability = async (date) => {
    try {
        const response = await apiClient.get(`${BASE}/availability`, {
            params: { date: date?.toISOString() },
        });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const updateAvailability = async (availabilitySlots) => {
    try {
        const response = await apiClient.put(`${BASE}/availability`, availabilitySlots);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

// =====================================================
// ADMIN APIs
// =====================================================

export const getAllSupervisors = async (type = null, status = null) => {
    try {
        const response = await apiClient.get(`${BASE}/admin/all`, {
            params: { type, status },
        });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const getSupervisorById = async (supervisorId) => {
    try {
        const response = await apiClient.get(`${BASE}/admin/${supervisorId}`);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const getSupervisorDashboard = async (supervisorId) => {
    try {
        const response = await apiClient.get(`${BASE}/admin/dashboard/${supervisorId}`);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const updateSupervisor = async (supervisorId, updates) => {
    try {
        const response = await apiClient.put(`${BASE}/admin/update/${supervisorId}`, updates);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const deleteSupervisor = async (supervisorId) => {
    try {
        const response = await apiClient.delete(`${BASE}/admin/delete/${supervisorId}`);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const checkAuthority = async (supervisorId, actionType) => {
    try {
        const response = await apiClient.get(`${BASE}/admin/check-authority/${supervisorId}`, {
            params: { actionType },
        });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const updateAuthorityLevel = async (supervisorId, newLevel, reason) => {
    try {
        const response = await apiClient.put(`${BASE}/admin/authority`, {
            supervisorId,
            newLevel,
            reason,
        });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const grantPermission = async (supervisorId, permissionType) => {
    try {
        const response = await apiClient.post(`${BASE}/admin/grant-permission`, {
            supervisorId,
            permissionType,
        });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const revokePermission = async (supervisorId, permissionType) => {
    try {
        const response = await apiClient.post(`${BASE}/admin/revoke-permission`, {
            supervisorId,
            permissionType,
        });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const activateSupervisor = async (supervisorId) => {
    try {
        const response = await apiClient.post(`${BASE}/admin/activate`, { supervisorId });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const suspendSupervisor = async (supervisorId, reason) => {
    try {
        const response = await apiClient.post(`${BASE}/admin/suspend`, { supervisorId, reason });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const terminateSupervisor = async (supervisorId, reason) => {
    try {
        const response = await apiClient.post(`${BASE}/admin/terminate`, { supervisorId, reason });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const searchSupervisors = async (filters) => {
    try {
        const response = await apiClient.post(`${BASE}/admin/search`, filters);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const getSupervisorsByZone = async (zoneId) => {
    try {
        const response = await apiClient.get(`${BASE}/admin/by-zone/${zoneId}`);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const getAvailableSupervisors = async (eventDate, eventType, zoneId) => {
    try {
        const response = await apiClient.get(`${BASE}/admin/available`, {
            params: { eventDate: eventDate?.toISOString(), eventType, zoneId },
        });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const getSupervisorStatistics = async (type = null) => {
    try {
        const response = await apiClient.get(`${BASE}/admin/statistics`, {
            params: { type },
        });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const getPerformanceReport = async (fromDate, toDate) => {
    try {
        const response = await apiClient.get(`${BASE}/admin/performance`, {
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
