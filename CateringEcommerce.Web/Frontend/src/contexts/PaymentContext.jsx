import React, { createContext, useContext, useState, useEffect } from 'react';

const PaymentContext = createContext();

export const usePayment = () => {
  const context = useContext(PaymentContext);
  if (!context) {
    throw new Error('usePayment must be used within a PaymentProvider');
  }
  return context;
};

export const PaymentProvider = ({ children }) => {
  const [isRazorpayLoaded, setIsRazorpayLoaded] = useState(false);
  const [razorpayError, setRazorpayError] = useState(null);

  useEffect(() => {
    // Load Razorpay SDK
    const loadRazorpay = () => {
      return new Promise((resolve, reject) => {
        // Check if already loaded
        if (window.Razorpay) {
          setIsRazorpayLoaded(true);
          resolve();
          return;
        }

        // Check if script is already in DOM
        const existingScript = document.querySelector('script[src*="razorpay"]');
        if (existingScript) {
          existingScript.addEventListener('load', () => {
            setIsRazorpayLoaded(true);
            resolve();
          });
          return;
        }

        // Create and load script
        const script = document.createElement('script');
        script.src = 'https://checkout.razorpay.com/v1/checkout.js';
        script.async = true;

        script.onload = () => {
          setIsRazorpayLoaded(true);
          resolve();
        };

        script.onerror = () => {
          setRazorpayError('Failed to load Razorpay SDK');
          reject(new Error('Failed to load Razorpay SDK'));
        };

        document.body.appendChild(script);
      });
    };

    loadRazorpay().catch(error => {
      console.error('Razorpay loading error:', error);
    });
  }, []);

  const createRazorpayOrder = async (orderData) => {
    try {
      // Call backend API to create a Razorpay order
      const response = await fetch(`${import.meta.env.VITE_API_BASE_URL}/api/User/PaymentGateway/CreateRazorpayOrder`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include', // Include cookies for authentication
        body: JSON.stringify(orderData),
      });

      if (!response.ok) {
        throw new Error('Failed to create Razorpay order');
      }

      const data = await response.json();
      return data;
    } catch (error) {
      console.error('Error creating Razorpay order:', error);
      throw error;
    }
  };

  const openRazorpayCheckout = ({
    amount,
    currency = 'INR',
    orderId,
    name,
    description,
    prefill = {},
    notes = {},
    onSuccess,
    onFailure,
  }) => {
    if (!isRazorpayLoaded || !window.Razorpay) {
      const error = 'Razorpay is not loaded yet';
      console.error(error);
      if (onFailure) onFailure(new Error(error));
      return;
    }

    const options = {
      key: import.meta.env.VITE_RAZORPAY_KEY_ID,
      amount: amount * 100, // Razorpay expects amount in paise
      currency: currency,
      name: name || 'ENYVORA Catering',
      description: description || 'Catering Service Payment',
      order_id: orderId,
      prefill: {
        name: prefill.name || '',
        email: prefill.email || '',
        contact: prefill.contact || '',
      },
      notes: notes,
      theme: {
        color: '#EF4444', // Red theme matching your brand
      },
      handler: function (response) {
        // Payment successful
        if (onSuccess) {
          onSuccess({
            razorpayPaymentId: response.razorpay_payment_id,
            razorpayOrderId: response.razorpay_order_id,
            razorpaySignature: response.razorpay_signature,
          });
        }
      },
      modal: {
        ondismiss: function () {
          if (onFailure) {
            onFailure(new Error('Payment cancelled by user'));
          }
        },
      },
    };

    const razorpayInstance = new window.Razorpay(options);

    razorpayInstance.on('payment.failed', function (response) {
      if (onFailure) {
        onFailure({
          code: response.error.code,
          description: response.error.description,
          source: response.error.source,
          step: response.error.step,
          reason: response.error.reason,
          metadata: response.error.metadata,
        });
      }
    });

    razorpayInstance.open();
  };

  const verifyPayment = async (paymentData) => {
    try {
      // Call backend API to verify the payment signature
      const response = await fetch(`${import.meta.env.VITE_API_BASE_URL}/api/User/PaymentGateway/VerifyPayment`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include', // Include cookies for authentication
        body: JSON.stringify(paymentData),
      });

      if (!response.ok) {
        throw new Error('Payment verification failed');
      }

      const data = await response.json();
      return data;
    } catch (error) {
      console.error('Error verifying payment:', error);
      throw error;
    }
  };

  const value = {
    isRazorpayLoaded,
    razorpayError,
    createRazorpayOrder,
    openRazorpayCheckout,
    verifyPayment,
  };

  return (
    <PaymentContext.Provider value={value}>
      {children}
    </PaymentContext.Provider>
  );
};
