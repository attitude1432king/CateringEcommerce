/**
 * Payment API
 * Handles supervisor earnings, payment history, payment requests
 * Routes: api/Supervisor/SupervisorPayment/*
 */

import apiClient, { handleApiResponse, handleApiError } from './apiConfig';

const BASE = '/api/Supervisor/SupervisorPayment';

// =====================================================
// SUPERVISOR APIs
// =====================================================

export const getEarnings = async () => {
  try {
    const response = await apiClient.get(`${BASE}/earnings`);
    return handleApiResponse(response);
  } catch (error) {
    return handleApiError(error);
  }
};

export const getPaymentHistory = async () => {
  try {
    const response = await apiClient.get(`${BASE}/history`);
    return handleApiResponse(response);
  } catch (error) {
    return handleApiError(error);
  }
};

export const requestPayment = async (assignmentId, amount, notes) => {
  try {
    const response = await apiClient.post(`${BASE}/request`, {
      assignmentId,
      amount,
      notes,
    });
    return handleApiResponse(response);
  } catch (error) {
    return handleApiError(error);
  }
};

export const getPaymentStatus = async (assignmentId) => {
  try {
    const response = await apiClient.get(`${BASE}/status/${assignmentId}`);
    return handleApiResponse(response);
  } catch (error) {
    return handleApiError(error);
  }
};

// =====================================================
// ADMIN APIs
// =====================================================

export const getPendingApprovals = async () => {
  try {
    const response = await apiClient.get(`${BASE}/admin/pending-approvals`);
    return handleApiResponse(response);
  } catch (error) {
    return handleApiError(error);
  }
};

export const approvePayment = async (assignmentId, notes) => {
  try {
    const response = await apiClient.post(`${BASE}/admin/approve`, {
      assignmentId,
      notes,
    });
    return handleApiResponse(response);
  } catch (error) {
    return handleApiError(error);
  }
};

export const rejectPayment = async (assignmentId, reason) => {
  try {
    const response = await apiClient.post(`${BASE}/admin/reject`, {
      assignmentId,
      reason,
    });
    return handleApiResponse(response);
  } catch (error) {
    return handleApiError(error);
  }
};

export const getPaymentSummary = async (fromDate, toDate) => {
  try {
    const response = await apiClient.get(`${BASE}/admin/payment-summary`, {
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
