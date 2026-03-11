import React, { useState } from 'react';
import { usePayment } from '../../../contexts/PaymentContext';
import { useAuth } from '../../../contexts/AuthContext';

const RazorpayCheckoutButton = ({
  amount,
  orderDetails,
  orderId, // System order ID (required for backend)
  stageType = 'PreBooking', // Payment stage: PreBooking, PostEvent, Full
  onSuccess,
  onFailure,
  disabled = false,
  className = '',
}) => {
  const { isRazorpayLoaded, openRazorpayCheckout, createRazorpayOrder } = usePayment();
  const { user } = useAuth();
  const [isProcessing, setIsProcessing] = useState(false);

  const handlePayment = async () => {
    if (!isRazorpayLoaded) {
      alert('Payment gateway is loading. Please wait...');
      return;
    }

    setIsProcessing(true);

    try {
      // Validate required parameters
      if (!orderId) {
        throw new Error('Order ID is required for payment');
      }
      if (!user?.pkid) {
        throw new Error('User must be authenticated to make payment');
      }

      // Step 1: Create Razorpay order on backend with required parameters
      const orderResponse = await createRazorpayOrder({
        Amount: amount, // Backend expects PascalCase
        Receipt: `order_${orderId}_${Date.now()}`,
        OrderId: orderId, // System order ID (required by backend)
        UserId: user.pkid, // Authenticated user ID (required by backend)
        StageType: stageType, // Payment stage type (required by backend)
        Notes: JSON.stringify({
          ...orderDetails,
          userId: user.pkid,
          timestamp: new Date().toISOString(),
        }),
      });

      if (!orderResponse.result || !orderResponse.data) {
        throw new Error(orderResponse.message || 'Failed to create payment order');
      }

      // Step 2: Open Razorpay checkout with the created order ID
      openRazorpayCheckout({
        amount: amount,
        currency: 'INR',
        orderId: orderResponse.data.id || orderResponse.data.Id, // Backend returns PascalCase 'Id'
        name: 'ENYVORA Catering',
        description: orderDetails.description || 'Catering Service',
        prefill: {
          name: user?.name || orderDetails.contactPerson || '',
          email: user?.email || orderDetails.contactEmail || '',
          contact: user?.phone || orderDetails.contactPhone || '',
        },
        notes: {
          cateringId: orderDetails.cateringId,
          eventDate: orderDetails.eventDate,
        },
        onSuccess: (paymentResponse) => {
          setIsProcessing(false);
          if (onSuccess) {
            onSuccess({
              ...paymentResponse,
              orderId: orderId, // Pass the system order ID
              stageType: stageType, // Include stage type for verification
            });
          }
        },
        onFailure: (error) => {
          setIsProcessing(false);
          if (onFailure) {
            onFailure(error);
          } else {
            alert('Payment failed: ' + (error.description || error.message || 'Unknown error'));
          }
        },
      });
    } catch (error) {
      setIsProcessing(false);
      console.error('Payment initialization error:', error);
      if (onFailure) {
        onFailure(error);
      } else {
        alert('Failed to initialize payment: ' + error.message);
      }
    }
  };

  return (
    <button
      type="button"
      onClick={handlePayment}
      disabled={disabled || isProcessing || !isRazorpayLoaded}
      className={`
        px-6 py-3 bg-gradient-to-r from-red-500 to-red-600 text-white font-semibold rounded-lg
        hover:from-red-600 hover:to-red-700 transition-all duration-200
        disabled:opacity-50 disabled:cursor-not-allowed
        flex items-center justify-center
        shadow-lg hover:shadow-xl
        ${className}
      `}
    >
      {isProcessing ? (
        <>
          <svg
            className="animate-spin -ml-1 mr-3 h-5 w-5 text-white"
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
          >
            <circle
              className="opacity-25"
              cx="12"
              cy="12"
              r="10"
              stroke="currentColor"
              strokeWidth="4"
            ></circle>
            <path
              className="opacity-75"
              fill="currentColor"
              d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
            ></path>
          </svg>
          Processing...
        </>
      ) : !isRazorpayLoaded ? (
        <>
          <svg
            className="animate-spin -ml-1 mr-3 h-5 w-5 text-white"
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
          >
            <circle
              className="opacity-25"
              cx="12"
              cy="12"
              r="10"
              stroke="currentColor"
              strokeWidth="4"
            ></circle>
            <path
              className="opacity-75"
              fill="currentColor"
              d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
            ></path>
          </svg>
          Loading Payment Gateway...
        </>
      ) : (
        <>
          <svg
            className="w-5 h-5 mr-2"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth="2"
              d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"
            />
          </svg>
          Pay ₹{amount?.toFixed(2)}
        </>
      )}
    </button>
  );
};

export default RazorpayCheckoutButton;
