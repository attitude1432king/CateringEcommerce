import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '');

// Create axios instance with auth
const api = axios.create({
    baseURL: API_BASE_URL,
    withCredentials: true,
    headers: {
        'Content-Type': 'application/json'
    }
});

// Invoice API Service
const invoiceApi = {
    /**
     * Get all invoices for an order
     * @param {number} orderId - Order ID
     * @returns {Promise} Invoice list
     */
    getInvoicesByOrder: async (orderId) => {
        try {
            const response = await api.get(`/invoice/order/${orderId}`);
            return response.data;
        } catch (error) {
            console.error('Error fetching invoices:', error);
            throw error;
        }
    },

    /**
     * Get invoice by ID
     * @param {number} invoiceId - Invoice ID
     * @returns {Promise} Invoice details
     */
    getInvoiceById: async (invoiceId) => {
        try {
            const response = await api.get(`/invoice/${invoiceId}`);
            return response.data;
        } catch (error) {
            console.error('Error fetching invoice:', error);
            throw error;
        }
    },

    /**
     * Get invoice by invoice number
     * @param {string} invoiceNumber - Invoice number
     * @returns {Promise} Invoice details
     */
    getInvoiceByNumber: async (invoiceNumber) => {
        try {
            const response = await api.get(`/invoice/number/${invoiceNumber}`);
            return response.data;
        } catch (error) {
            console.error('Error fetching invoice by number:', error);
            throw error;
        }
    },

    /**
     * Get user invoices (paginated)
     * @param {number} userId - User ID
     * @param {number} pageNumber - Page number (default: 1)
     * @param {number} pageSize - Page size (default: 10)
     * @param {string} status - Filter by status (optional)
     * @param {string} type - Filter by type (optional)
     * @returns {Promise} Paginated invoice list
     */
    getUserInvoices: async (userId, pageNumber = 1, pageSize = 10, status = null, type = null) => {
        try {
            const params = new URLSearchParams({
                pageNumber,
                pageSize
            });

            if (status) params.append('status', status);
            if (type) params.append('type', type);

            const response = await api.get(`/invoice/user/${userId}?${params.toString()}`);
            return response.data;
        } catch (error) {
            console.error('Error fetching user invoices:', error);
            throw error;
        }
    },

    /**
     * Generate new invoice
     * Admin/Owner only
     * @param {object} invoiceData - Invoice generation data
     * @returns {Promise} Created invoice
     */
    generateInvoice: async (invoiceData) => {
        try {
            const response = await api.post('/invoice/generate', invoiceData);
            return response.data;
        } catch (error) {
            console.error('Error generating invoice:', error);
            throw error;
        }
    },

    /**
     * Download invoice PDF
     * @param {number} invoiceId - Invoice ID
     * @param {boolean} includeLogo - Include company logo (default: true)
     * @returns {Promise} Blob for download
     */
    downloadInvoicePdf: async (invoiceId, includeLogo = true) => {
        try {
            const response = await api.get(`/invoice/${invoiceId}/download`, {
                params: { includeLogo },
                responseType: 'blob'
            });

            // Create download link
            const url = window.URL.createObjectURL(new Blob([response.data]));
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', `Invoice_${invoiceId}.pdf`);
            document.body.appendChild(link);
            link.click();
            link.remove();

            return response.data;
        } catch (error) {
            console.error('Error downloading invoice PDF:', error);
            throw error;
        }
    },

    /**
     * Download payment receipt PDF
     * @param {number} invoiceId - Invoice ID
     * @param {string} paymentId - Razorpay payment ID
     * @returns {Promise} Blob for download
     */
    downloadReceipt: async (invoiceId, paymentId) => {
        try {
            const response = await api.get(`/invoice/${invoiceId}/receipt`, {
                params: { paymentId },
                responseType: 'blob'
            });

            const url = window.URL.createObjectURL(new Blob([response.data]));
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', `Receipt_${invoiceId}_${paymentId}.pdf`);
            document.body.appendChild(link);
            link.click();
            link.remove();

            return response.data;
        } catch (error) {
            console.error('Error downloading receipt:', error);
            throw error;
        }
    },

    /**
     * Download consolidated statement for an order
     * @param {number} orderId - Order ID
     * @returns {Promise} Blob for download
     */
    downloadConsolidatedStatement: async (orderId) => {
        try {
            const response = await api.get(`/invoice/order/${orderId}/statement`, {
                responseType: 'blob'
            });

            const url = window.URL.createObjectURL(new Blob([response.data]));
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', `Statement_Order_${orderId}.pdf`);
            document.body.appendChild(link);
            link.click();
            link.remove();

            return response.data;
        } catch (error) {
            console.error('Error downloading statement:', error);
            throw error;
        }
    },

    /**
     * Generate credit note PDF
     * Admin/Owner only
     * @param {number} invoiceId - Invoice ID
     * @param {number} refundAmount - Refund amount
     * @param {string} reason - Refund reason
     * @returns {Promise} Blob for download
     */
    generateCreditNote: async (invoiceId, refundAmount, reason) => {
        try {
            const response = await api.post(`/invoice/${invoiceId}/credit-note`, {
                refundAmount,
                reason
            }, {
                responseType: 'blob'
            });

            const url = window.URL.createObjectURL(new Blob([response.data]));
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', `CreditNote_${invoiceId}.pdf`);
            document.body.appendChild(link);
            link.click();
            link.remove();

            return response.data;
        } catch (error) {
            console.error('Error generating credit note:', error);
            throw error;
        }
    },

    /**
     * Update invoice status
     * Admin only
     * @param {number} invoiceId - Invoice ID
     * @param {number} status - New status
     * @returns {Promise} Success result
     */
    updateInvoiceStatus: async (invoiceId, status) => {
        try {
            const response = await api.put(`/invoice/${invoiceId}/status`, status);
            return response.data;
        } catch (error) {
            console.error('Error updating invoice status:', error);
            throw error;
        }
    },

    /**
     * Link payment to invoice
     * @param {object} paymentData - Payment linkage data
     * @returns {Promise} Success result
     */
    linkPaymentToInvoice: async (paymentData) => {
        try {
            const response = await api.post('/invoice/link-payment', paymentData);
            return response.data;
        } catch (error) {
            console.error('Error linking payment to invoice:', error);
            throw error;
        }
    },

    /**
     * Get invoice statistics for an order
     * @param {number} orderId - Order ID
     * @returns {Promise} Invoice statistics
     */
    getOrderInvoiceStats: async (orderId) => {
        try {
            const response = await api.get(`/invoice/order/${orderId}/statistics`);
            return response.data;
        } catch (error) {
            console.error('Error fetching invoice statistics:', error);
            throw error;
        }
    },

    /**
     * Get total paid amount for an order
     * @param {number} orderId - Order ID
     * @returns {Promise} Total paid amount
     */
    getTotalPaid: async (orderId) => {
        try {
            const response = await api.get(`/invoice/order/${orderId}/total-paid`);
            return response.data;
        } catch (error) {
            console.error('Error fetching total paid:', error);
            throw error;
        }
    },

    /**
     * Get payment progress percentage for an order
     * @param {number} orderId - Order ID
     * @returns {Promise} Payment progress data
     */
    getPaymentProgress: async (orderId) => {
        try {
            const response = await api.get(`/invoice/order/${orderId}/progress`);
            return response.data;
        } catch (error) {
            console.error('Error fetching payment progress:', error);
            throw error;
        }
    },

    /**
     * Validate PDF settings
     * Admin only
     * @returns {Promise} Validation result
     */
    validatePdfSettings: async () => {
        try {
            const response = await api.get('/invoice/pdf/validate');
            return response.data;
        } catch (error) {
            console.error('Error validating PDF settings:', error);
            throw error;
        }
    }
};

// Export
export default invoiceApi;

// Named exports for convenience
export const {
    getInvoicesByOrder,
    getInvoiceById,
    getInvoiceByNumber,
    getUserInvoices,
    generateInvoice,
    downloadInvoicePdf,
    downloadReceipt,
    downloadConsolidatedStatement,
    generateCreditNote,
    updateInvoiceStatus,
    linkPaymentToInvoice,
    getOrderInvoiceStats,
    getTotalPaid,
    getPaymentProgress,
    validatePdfSettings
} = invoiceApi;
