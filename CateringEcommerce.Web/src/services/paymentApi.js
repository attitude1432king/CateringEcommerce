import { fetchApi } from './apiUtils';

/**
 * Create a Razorpay order for an existing platform order.
 * @param {{ Amount: number, Receipt: string, OrderId: number, UserId: number, StageType: string, Notes?: string }} data
 * @returns {Promise<{ razorpayOrderId: string, amount: number, currency: string, key: string }>}
 */
export const createRazorpayOrder = (data) =>
    fetchApi('/User/PaymentGateway/CreateRazorpayOrder', 'POST', data);

/**
 * Verify Razorpay payment signature after the popup completes.
 * @param {{ RazorpayOrderId: string, RazorpayPaymentId: string, RazorpaySignature: string, OrderId: number, StageType: string }} data
 */
export const verifyRazorpayPayment = (data) =>
    fetchApi('/User/PaymentGateway/VerifyPayment', 'POST', data);
