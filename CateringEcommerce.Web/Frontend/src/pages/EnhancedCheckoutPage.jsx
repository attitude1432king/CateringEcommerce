/**
 * EnhancedCheckoutPage Component
 *
 * Modern checkout flow with authentication guard
 * Redirects to auth modal if user not logged in
 */

import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCart } from '../contexts/CartContext';
import { useAuthGuard } from '../hooks/useAuthGuard';
import { createOrder } from '../services/orderApi';
import EventDetailsForm from '../components/user/checkout/EventDetailsForm';
import AddressContactForm from '../components/user/checkout/AddressContactForm';
import PaymentReviewForm from '../components/user/checkout/PaymentReviewForm';
import OrderConfirmationModal from '../components/user/OrderConfirmationModal';
import EnhancedProgressStepper from '../components/user/checkout/EnhancedProgressStepper';
import OTPVerificationModal from '../components/common/OTPVerificationModal';
import AuthModal from '../components/user/AuthModal';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:44368';

const EnhancedCheckoutPage = () => {
  const navigate = useNavigate();
  const { cart, clearCart } = useCart();
  const { isAuthenticated, user, showAuthModal, handleAuthClose } = useAuthGuard();
  const [currentStep, setCurrentStep] = useState(1);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showConfirmation, setShowConfirmation] = useState(false);
  const [createdOrder, setCreatedOrder] = useState(null);
  const [error, setError] = useState(null);
  const [showOtpModal, setShowOtpModal] = useState(false);
  const [otpVerified, setOtpVerified] = useState(false);

  const [checkoutData, setCheckoutData] = useState({
    // Event Details
    eventDate: '',
    eventTime: '',
    eventType: '',
    eventLocation: '',
    specialInstructions: '',
    // Address & Contact
    deliveryAddress: '',
    contactPerson: '',
    contactPhone: '',
    contactEmail: '',
    // Payment
    paymentMethod: 'COD',
    paymentProof: null,
    termsAccepted: false
  });

  // Redirect if cart is empty
  useEffect(() => {
    if (!cart || !cart.cateringId) {
      navigate('/');
    }
  }, [cart, navigate]);

  // Show auth modal if not authenticated
  useEffect(() => {
    if (!isAuthenticated) {
      // Don't redirect - auth guard hook will show modal
      console.log('User not authenticated, auth modal will be shown via cart');
    }
  }, [isAuthenticated]);

  // Handle successful authentication
  const handleAuthSuccess = () => {
    handleAuthClose();
    // User is now authenticated, continue with checkout
    console.log('Authentication successful, continuing with checkout');
  };

  const handleUpdateEventDetails = (data) => {
    setCheckoutData(prev => ({ ...prev, ...data }));
  };

  const handleUpdateAddressContact = (data) => {
    setCheckoutData(prev => ({ ...prev, ...data }));
  };

  const handleUpdatePayment = (data) => {
    setCheckoutData(prev => ({ ...prev, ...data }));
  };

  const handleNext = () => {
    if (currentStep < 4) {
      setCurrentStep(currentStep + 1);
      window.scrollTo(0, 0);
    }
  };

  const handleBack = () => {
    if (currentStep > 1) {
      setCurrentStep(currentStep - 1);
      window.scrollTo(0, 0);
    }
  };

  const handlePlaceOrder = async () => {
    if (!isAuthenticated || !user) {
      alert('Please login to place order');
      return;
    }

    setIsSubmitting(true);
    setError(null);

    try {
      const orderPayload = {
        userId: user.pkid,
        cateringId: cart.cateringId,
        packageId: cart.packageId,
        guestCount: cart.guestCount,
        eventDate: checkoutData.eventDate,
        eventTime: checkoutData.eventTime,
        eventType: checkoutData.eventType,
        eventLocation: checkoutData.eventLocation,
        deliveryAddress: checkoutData.deliveryAddress,
        contactPerson: checkoutData.contactPerson,
        contactPhone: checkoutData.contactPhone,
        contactEmail: checkoutData.contactEmail,
        paymentMethod: checkoutData.paymentMethod,
        specialInstructions: checkoutData.specialInstructions,
        totalAmount: cart.totalAmount,
        taxAmount: cart.taxAmount,
        additionalItems: cart.additionalItems || [],
        decorationId: cart.decorationId || null
      };

      const response = await createOrder(orderPayload);

      if (response.success) {
        setCreatedOrder(response.data);
        clearCart();
        setShowConfirmation(true);
      } else {
        setError(response.message || 'Failed to place order');
      }
    } catch (err) {
      console.error('Order placement error:', err);
      setError(err.message || 'An error occurred while placing your order');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleConfirmationClose = () => {
    setShowConfirmation(false);
    navigate('/orders');
  };

  if (!cart) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <div className="w-16 h-16 border-4 border-rose-500 border-t-transparent rounded-full animate-spin mx-auto mb-4"></div>
          <p className="text-gray-600">Loading...</p>
        </div>
      </div>
    );
  }

  return (
    <>
      <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 py-8">
        <div className="container mx-auto px-4 max-w-6xl">
          {/* Header */}
          <div className="bg-white rounded-2xl shadow-lg p-6 mb-8">
            <div className="flex items-center justify-between mb-6">
              <div>
                <h1 className="text-3xl font-bold text-gray-900 mb-2">Checkout</h1>
                <p className="text-gray-600">Complete your order for {cart.cateringName}</p>
              </div>
              <button
                onClick={() => navigate(-1)}
                className="text-gray-600 hover:text-gray-900 transition-colors"
              >
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>

            {/* Progress Stepper */}
            <EnhancedProgressStepper currentStep={currentStep} />
          </div>

          {/* Error Display */}
          {error && (
            <div className="bg-red-50 border-l-4 border-red-500 text-red-700 p-4 rounded-lg mb-6 shadow-md">
              <div className="flex items-center">
                <svg className="w-5 h-5 mr-2" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
                </svg>
                <span className="font-medium">{error}</span>
              </div>
            </div>
          )}

          {/* Step Content */}
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            {/* Main Content */}
            <div className="lg:col-span-2">
              {currentStep === 1 && (
                <EventDetailsForm
                  data={checkoutData}
                  onUpdate={handleUpdateEventDetails}
                  onNext={handleNext}
                />
              )}
              {currentStep === 2 && (
                <AddressContactForm
                  data={checkoutData}
                  onUpdate={handleUpdateAddressContact}
                  onNext={handleNext}
                  onBack={handleBack}
                />
              )}
              {currentStep === 3 && (
                <PaymentReviewForm
                  data={checkoutData}
                  cart={cart}
                  onUpdate={handleUpdatePayment}
                  onBack={handleBack}
                  onPlaceOrder={handlePlaceOrder}
                  isSubmitting={isSubmitting}
                />
              )}
            </div>

            {/* Order Summary Sidebar */}
            <div className="lg:col-span-1">
              <div className="bg-white rounded-2xl shadow-lg p-6 sticky top-8">
                <h3 className="text-xl font-bold text-gray-900 mb-4">Order Summary</h3>
                <div className="space-y-4">
                  <div className="flex items-center gap-3 pb-4 border-b">
                    <img
                      src={cart.cateringLogo ? `${API_BASE_URL}${cart.cateringLogo}` : '/placeholder.png'}
                      alt={cart.cateringName}
                      className="w-12 h-12 rounded-lg object-cover"
                    />
                    <div>
                      <p className="font-semibold text-gray-900">{cart.cateringName}</p>
                      <p className="text-sm text-gray-600">{cart.packageName}</p>
                    </div>
                  </div>
                  <div className="space-y-2">
                    <div className="flex justify-between text-gray-700">
                      <span>Guests</span>
                      <span className="font-medium">{cart.guestCount}</span>
                    </div>
                    <div className="flex justify-between text-gray-700">
                      <span>Subtotal</span>
                      <span className="font-medium">₹{(cart.totalAmount - cart.taxAmount).toLocaleString()}</span>
                    </div>
                    <div className="flex justify-between text-gray-700">
                      <span>GST (18%)</span>
                      <span className="font-medium">₹{cart.taxAmount.toLocaleString()}</span>
                    </div>
                    <div className="pt-3 border-t-2 border-dashed flex justify-between">
                      <span className="font-bold text-gray-900 text-lg">Total</span>
                      <span className="font-bold text-rose-600 text-lg">₹{cart.totalAmount.toLocaleString()}</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Order Confirmation Modal */}
      {showConfirmation && createdOrder && (
        <OrderConfirmationModal
          order={createdOrder}
          onClose={handleConfirmationClose}
        />
      )}

      {/* OTP Verification Modal */}
      {showOtpModal && (
        <OTPVerificationModal
          isOpen={showOtpModal}
          onClose={() => setShowOtpModal(false)}
          onVerified={() => {
            setOtpVerified(true);
            setShowOtpModal(false);
          }}
        />
      )}

      {/* Auth Modal (shown when not authenticated) */}
      {showAuthModal && (
        <AuthModal
          isOpen={showAuthModal}
          onClose={handleAuthClose}
          isPartnerLogin={false}
        />
      )}
    </>
  );
};

export default EnhancedCheckoutPage;
