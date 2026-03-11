/**
 * Registration API
 * Handles supervisor registration workflow (4-stage fast activation)
 * Routes: api/Supervisor/SupervisorRegistration/*
 */

import apiClient, { handleApiResponse, handleApiError } from './apiConfig';

const BASE = '/api/Supervisor/SupervisorRegistration';

// =====================================================
// PUBLIC APIs (No Auth Required)
// =====================================================

export const submitRegistration = async (registration) => {
    try {
        const response = await apiClient.post(`${BASE}/submit`, registration);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const getRegistrationProgress = async (registrationId) => {
    try {
        const response = await apiClient.get(`${BASE}/progress/${registrationId}`);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const getRegistrationById = async (registrationId) => {
    try {
        const response = await apiClient.get(`${BASE}/${registrationId}`);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

// =====================================================
// SUPERVISOR SELF-SERVICE (Auth Required)
// =====================================================

export const getMyRegistration = async () => {
    try {
        const response = await apiClient.get(`${BASE}/my-registration`);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const uploadDocument = async ({ file, documentType, supervisorId = null }) => {
    try {
        const formData = new FormData();
        formData.append('file', file);
        formData.append('documentType', documentType);
        if (supervisorId) {
            formData.append('supervisorId', supervisorId);
        }

        const response = await apiClient.post(`${BASE}/upload-document`, formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            },
        });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const submitBankingDetails = async (bankingDetails) => {
    try {
        const response = await apiClient.post(`${BASE}/banking-details`, bankingDetails);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const getBankingDetails = async () => {
    try {
        const response = await apiClient.get(`${BASE}/banking-details`);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

// =====================================================
// ADMIN APIs
// =====================================================

export const getAllRegistrations = async (status = null) => {
    try {
        const response = await apiClient.get(`${BASE}/admin/all`, {
            params: { status },
        });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const getRegistrationsByStage = async (stage) => {
    try {
        const response = await apiClient.get(`${BASE}/admin/by-stage/${stage}`);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const searchRegistrations = async (filters) => {
    try {
        const response = await apiClient.post(`${BASE}/admin/search`, filters);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const getRegistrationStatistics = async () => {
    try {
        const response = await apiClient.get(`${BASE}/admin/statistics`);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

// Stage 1: Document Verification
export const getPendingDocumentVerification = async () => {
    try {
        const response = await apiClient.get(`${BASE}/admin/pending-docs`);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const verifyDocuments = async (verification) => {
    try {
        const response = await apiClient.post(`${BASE}/admin/verify-docs`, verification);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

// Stage 2: Interview
export const getPendingInterview = async () => {
    try {
        const response = await apiClient.get(`${BASE}/admin/pending-interview`);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const scheduleInterview = async (interview) => {
    try {
        const response = await apiClient.post(`${BASE}/admin/schedule-interview`, interview);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const submitInterviewResult = async (result) => {
    try {
        const response = await apiClient.post(`${BASE}/admin/interview-result`, result);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

// Stage 3: Training
export const getPendingTraining = async () => {
    try {
        const response = await apiClient.get(`${BASE}/admin/pending-training`);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const assignTraining = async (registrationId, moduleIds) => {
    try {
        const response = await apiClient.post(`${BASE}/admin/assign-training`, {
            registrationId,
            moduleIds,
        });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const completeTraining = async (registrationId) => {
    try {
        const response = await apiClient.post(`${BASE}/admin/complete-training`, {
            registrationId,
        });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

// Stage 4: Certification
export const getPendingCertification = async () => {
    try {
        const response = await apiClient.get(`${BASE}/admin/pending-certification`);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const scheduleCertification = async (registrationId, examDate) => {
    try {
        const response = await apiClient.post(`${BASE}/admin/schedule-certification`, {
            registrationId,
            examDate: examDate.toISOString(),
        });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const submitCertificationResult = async (result) => {
    try {
        const response = await apiClient.post(`${BASE}/admin/certification-result`, result);
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

// Final Actions
export const activateRegistration = async (registrationId) => {
    try {
        const response = await apiClient.post(`${BASE}/admin/activate`, { registrationId });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const rejectRegistration = async (registrationId, reason) => {
    try {
        const response = await apiClient.post(`${BASE}/admin/reject`, {
            registrationId,
            reason,
        });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};

export const progressStage = async (registrationId, nextStage, notes) => {
    try {
        const response = await apiClient.post(`${BASE}/admin/progress-stage`, {
            registrationId,
            nextStage,
            notes,
        });
        return handleApiResponse(response);
    } catch (error) {
        return handleApiError(error);
    }
};
