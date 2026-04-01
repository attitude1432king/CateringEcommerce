/**
 * Event Supervision API
 * Handles Pre/During/Post event workflow
 * Routes: api/Supervisor/EventSupervision/*
 */

import apiClient, { handleApiResponse, handleApiError } from './apiConfig';

const BASE = '/api/Supervisor/EventSupervision';

// =====================================================
// PRE-EVENT VERIFICATION
// =====================================================

export const submitPreEventVerification = async (verification) => {
  try {
    const response = await apiClient.post(`${BASE}/pre-event/submit`, verification);
    return handleApiResponse(response);
  } catch (error) {
    return handleApiError(error);
  }
};

export const getPreEventVerification = async (assignmentId) => {
  try {
    const response = await apiClient.get(`${BASE}/pre-event/${assignmentId}`);
    return handleApiResponse(response);
  } catch (error) {
    return handleApiError(error);
  }
};

export const updatePreEventChecklist = async (assignmentId, updates) => {
  try {
    const response = await apiClient.put(`${BASE}/pre-event/${assignmentId}`, updates);
    return handleApiResponse(response);
  } catch (error) {
    return handleApiError(error);
  }
};

// =====================================================
// DURING-EVENT MONITORING
// =====================================================

export const recordFoodServingMonitor = async (monitorData) => {
  try {
    const response = await apiClient.post(`${BASE}/during/food-serving`, monitorData);
    return handleApiResponse(response);
  } catch (error) {
    return handleApiError(error);
  }
};

export const updateGuestCount = async (guestCountData) => {
  try {
    const response = await apiClient.post(`${BASE}/during/guest-count`, guestCountData);
    return handleApiResponse(response);
  } catch (error) {
    return handleApiError(error);
  }
};

export const requestExtraQuantity = async (extraQuantityRequest) => {
  try {
    const response = await apiClient.post(`${BASE}/during/extra-quantity`, extraQuantityRequest);
    return handleApiResponse(response);
  } catch (error) {
    return handleApiError(error);
  }
};

export const verifyClientOTP = async (otpData) => {
  try {
    const response = await apiClient.post(`${BASE}/during/verify-otp`, otpData);
    return handleApiResponse(response);
  } catch (error) {
    return handleApiError(error);
  }
};

export const resendClientOTP = async (assignmentId, purpose) => {
  try {
    const response = await apiClient.post(`${BASE}/during/resend-otp`, {
      assignmentId,
      purpose,
    });
    return handleApiResponse(response);
  } catch (error) {
    return handleApiError(error);
  }
};

export const getDuringEventTracking = async (assignmentId) => {
  try {
    const response = await apiClient.get(`${BASE}/during/tracking/${assignmentId}`);
    return handleApiResponse(response);
  } catch (error) {
    return handleApiError(error);
  }
};

// =====================================================
// POST-EVENT COMPLETION
// =====================================================

export const submitPostEventReport = async (report) => {
  try {
    const response = await apiClient.post(`${BASE}/post-event/submit`, report);
    return handleApiResponse(response);
  } catch (error) {
    return handleApiError(error);
  }
};

export const getPostEventReport = async (assignmentId) => {
  try {
    const response = await apiClient.get(`${BASE}/post-event/${assignmentId}`);
    return handleApiResponse(response);
  } catch (error) {
    return handleApiError(error);
  }
};

export const updatePostEventReport = async (assignmentId, updates) => {
  try {
    const response = await apiClient.put(`${BASE}/post-event/${assignmentId}`, updates);
    return handleApiResponse(response);
  } catch (error) {
    return handleApiError(error);
  }
};

// =====================================================
// COMPLETE EVENT SUPERVISION SUMMARY
// =====================================================

export const getEventSupervisionSummary = async (assignmentId) => {
  try {
    const response = await apiClient.get(`${BASE}/summary/${assignmentId}`);
    return handleApiResponse(response);
  } catch (error) {
    return handleApiError(error);
  }
};

// =====================================================
// EVIDENCE & DOCUMENTATION
// =====================================================

export const uploadTimestampedEvidence = async (formData) => {
  try {
    const response = await apiClient.post(`${BASE}/evidence/upload`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return handleApiResponse(response);
  } catch (error) {
    return handleApiError(error);
  }
};

export const getAssignmentEvidence = async (assignmentId) => {
  try {
    const response = await apiClient.get(`${BASE}/evidence/${assignmentId}`);
    return handleApiResponse(response);
  } catch (error) {
    return handleApiError(error);
  }
};

// =====================================================
// ADMIN APIs
// =====================================================

export const verifyPostEventReport = async (reportId, verificationNotes) => {
  try {
    const response = await apiClient.post(`${BASE}/admin/verify-report`, {
      reportId,
      verificationNotes,
    });
    return handleApiResponse(response);
  } catch (error) {
    return handleApiError(error);
  }
};
