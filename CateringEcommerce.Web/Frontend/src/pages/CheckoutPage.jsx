import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCart } from '../contexts/CartContext';
import { useAuth } from '../contexts/AuthContext';
import { createOrder } from '../services/orderApi';
import EventDetailsForm from '../components/user/checkout/EventDetailsForm';
import AddressContactForm from '../components/user/checkout/AddressContactForm';
import PaymentReviewForm from '../components/user/checkout/PaymentReviewForm';
import OrderConfirmationModal from '../components/user/OrderConfirmationModal';
import EnhancedProgressStepper from '../components/user/checkout/EnhancedProgressStepper';
import OTPVerificationModal from '../components/common/OTPVerificationModal';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:44368';

const CheckoutPage = () => {
  const navigate = useNavigate();
  const { cart, clearCart } = useCart();
  const { user } = useAuth();
  const [currentStep, setCurrentStep] = useState(1);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showConfirmation, setShowConfirmation] = useState(false);
  const [createdOrder, setCreatedOrder] = useState(null);
  const [error, setError] = useState(null);
  const [showOtpModal, setShowOtpModal] = useState(false); // OTP verification modal
  const [otpVerified, setOtpVerified] = useState(false); // Track OTP verification status

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

  // Redirect if not logged in
  useEffect(() => {
    if (!user) {
      navigate('/');
    }
  }, [user, navigate]);

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

  // Handle OTP verification before submitting order
  const handleSubmitOrder = () => {
    // Show OTP modal for sensitive action verification
    setShowOtpModal(true);
  };

  // Handle OTP verification success
  const handleOtpVerified = async (otp, token) => {
    setOtpVerified(true);
    setShowOtpModal(false);
    // Proceed with actual order submission
    await actuallySubmitOrder();
  };

  // Actually submit the order after OTP verification
  const actuallySubmitOrder = async () => {
    setIsSubmitting(true);
    setError(null);

    try {
      // Prepare order items from cart
      const orderItems = [];

      // Add package item
      if (cart.packageId) {
        orderItems.push({
          itemType: 'Package',
          itemId: cart.packageId,
          itemName: cart.packageName,
          quantity: 1,
          unitPrice: cart.packagePrice,
          totalPrice: cart.packagePrice,
          packageSelections: cart.packageSelections ? JSON.stringify(cart.packageSelections) : null
        });
      }

      // Add decoration item
      if (cart.decorationId) {
        orderItems.push({
          itemType: 'Decoration',
          itemId: cart.decorationId,
          itemName: cart.decorationName,
          quantity: 1,
          unitPrice: cart.decorationPrice,
          totalPrice: cart.decorationPrice,
          packageSelections: null
        });
      }

      // Add additional food items
      if (cart.additionalItems && cart.additionalItems.length > 0) {
        cart.additionalItems.forEach(item => {
          orderItems.push({
            itemType: 'FoodItem',
            itemId: item.foodId,
            itemName: item.foodName || item.name,
            quantity: item.quantity || 1,
            unitPrice: item.price,
            totalPrice: item.price * (item.quantity || 1),
            packageSelections: null
          });
        });
      }

      // Prepare order data
      const orderData = {
        cateringId: cart.cateringId,
        eventDate: checkoutData.eventDate,
        eventTime: checkoutData.eventTime,
        eventType: checkoutData.eventType,
        eventLocation: checkoutData.eventLocation || '',
        guestCount: cart.guestCount,
        specialInstructions: checkoutData.specialInstructions || null,
        deliveryAddress: checkoutData.deliveryAddress,
        contactPerson: checkoutData.contactPerson,
        contactPhone: checkoutData.contactPhone,
        contactEmail: checkoutData.contactEmail,
        baseAmount: cart.baseAmount || 0,
        taxAmount: cart.taxAmount || 0,
        deliveryCharges: 0,
        discountAmount: cart.discountAmount || 0,
        totalAmount: cart.totalAmount,
        paymentMethod: checkoutData.paymentMethod,
        paymentProof: checkoutData.paymentMethod === 'BankTransfer' ? checkoutData.paymentProof : null,
        orderItems: orderItems
      };

      // Submit order
      const response = await createOrder(orderData);

      if (response.result) {
        setCreatedOrder(response.data);
        clearCart(); // Clear cart after successful order
        setShowConfirmation(true);
      } else {
        setError(response.message || 'Failed to create order. Please try again.');
      }
    } catch (error) {
      console.error('Error submitting order:', error);
      setError(error.message || 'An error occurred while placing your order. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const steps = [
    { number: 1, name: 'Cart Review', icon: '🛒' },
    { number: 2, name: 'Event Details', icon: '📅' },
    { number: 3, name: 'Address & Contact', icon: '📍' },
    { number: 4, name: 'Payment & Review', icon: '💳' }
  ];

  if (!cart) {
    return null;
  }

  return (
    <div className="min-h-screen bg-gray-100 py-8">
      <div className="max-w-6xl mx-auto px-4">
        {/* Progress Indicator */}
        <EnhancedProgressStepper steps={steps} currentStep={currentStep} className="mb-8" />

        {/* Error Message */}
        {error && (
          <div className="bg-red-50 border border-red-200 text-red-800 px-4 py-3 rounded-lg mb-6">
            <div className="flex items-center">
              <svg className="w-5 h-5 mr-2" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
              </svg>
              {error}
            </div>
          </div>
        )}

        {/* Form Content */}
        <div className="bg-white rounded-lg p-6 shadow-sm">
          {/* Step 1: Cart Review */}
          {currentStep === 1 && (
            <div className="cart-review-step">
              <h2 className="text-2xl font-bold mb-6">Cart Review</h2>
              <div className="space-y-4">
                {/* Catering Info */}
                <div className="flex items-center gap-4 pb-4 border-b">
                  {cart.cateringLogo && (
                    <img
                      src={`${API_BASE_URL}${cart.cateringLogo}`}
                      alt={cart.cateringName}
                      className="w-16 h-16 rounded-lg object-cover"
                    />
                  )}
                  <div>
                    <h3 className="font-semibold text-lg">{cart.cateringName}</h3>
                    <p className="text-sm text-gray-600">Selected Package: {cart.packageName}</p>
                  </div>
                </div>

                {/* Order Items */}
                <div>
                  <div className="flex justify-between py-2">
                    <span>Package: {cart.packageName}</span>
                    <span>₹{cart.packagePrice?.toFixed(2)}</span>
                  </div>
                  {cart.decorationName && (
                    <div className="flex justify-between py-2">
                      <span>Decoration: {cart.decorationName}</span>
                      <span>₹{cart.decorationPrice?.toFixed(2)}</span>
                    </div>
                  )}
                  {cart.additionalItems && cart.additionalItems.length > 0 && (
                    <div className="mt-2">
                      <p className="font-medium mb-2">Additional Items:</p>
                      {cart.additionalItems.map((item, index) => (
                        <div key={index} className="flex justify-between py-1 text-sm">
                          <span>{item.foodName || item.name} × {item.quantity || 1}</span>
                          <span>₹{(item.price * (item.quantity || 1)).toFixed(2)}</span>
                        </div>
                      ))}
                    </div>
                  )}
                </div>

                {/* Guest Count */}
                <div className="bg-gray-50 p-3 rounded">
                  <strong>Guest Count:</strong> {cart.guestCount}
                </div>

                {/* Price Summary */}
                <div className="border-t pt-4">
                  <div className="flex justify-between py-1">
                    <span>Subtotal</span>
                    <span>₹{cart.baseAmount?.toFixed(2)}</span>
                  </div>
                  <div className="flex justify-between py-1">
                    <span>Tax (18%)</span>
                    <span>₹{cart.taxAmount?.toFixed(2)}</span>
                  </div>
                  <div className="flex justify-between py-2 border-t mt-2 font-bold text-lg">
                    <span>Total</span>
                    <span className="text-red-500">₹{cart.totalAmount?.toFixed(2)}</span>
                  </div>
                </div>
              </div>

              <div className="flex justify-end mt-8">
                <button
                  onClick={handleNext}
                  className="px-6 py-3 bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors"
                >
                  Continue
                </button>
              </div>
            </div>
          )}

          {/* Step 2: Event Details */}
          {currentStep === 2 && (
            <EventDetailsForm
              formData={checkoutData}
              onUpdate={handleUpdateEventDetails}
              onNext={handleNext}
              onBack={handleBack}
            />
          )}

          {/* Step 3: Address & Contact */}
          {currentStep === 3 && (
            <AddressContactForm
              formData={checkoutData}
              onUpdate={handleUpdateAddressContact}
              onNext={handleNext}
              onBack={handleBack}
            />
          )}

          {/* Step 4: Payment & Review */}
          {currentStep === 4 && (
            <PaymentReviewForm
              formData={checkoutData}
              onUpdate={handleUpdatePayment}
              onSubmit={handleSubmitOrder}
              onBack={handleBack}
              isSubmitting={isSubmitting}
            />
          )}
        </div>
      </div>

      {/* Order Confirmation Modal */}
      {showConfirmation && createdOrder && (
        <OrderConfirmationModal
          order={createdOrder}
          onClose={() => setShowConfirmation(false)}
        />
      )}

      {/* OTP Verification Modal - Required before placing order */}
      <OTPVerificationModal
        isOpen={showOtpModal}
        onClose={() => setShowOtpModal(false)}
        onVerify={handleOtpVerified}
        purpose="Place Order"
        phoneNumber={user?.phone || checkoutData.contactPhone}
        actionDescription={`Confirm your order for ₹${cart.totalAmount?.toLocaleString('en-IN')} by verifying your identity`}
        requireOtp={true}
        autoSendOtp={true}
      />
    </div>
  );
};

export default CheckoutPage;
