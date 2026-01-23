import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCart } from '../contexts/CartContext';
import { useAuth } from '../contexts/AuthContext';
import { createOrder } from '../services/orderApi';
import AccountSection from '../components/user/checkout/modern/AccountSection';
import EventDetailsSection from '../components/user/checkout/modern/EventDetailsSection';
import DeliveryTypeSection from '../components/user/checkout/modern/DeliveryTypeSection';
import PaymentSection from '../components/user/checkout/modern/PaymentSection';
import CartSummary from '../components/user/checkout/modern/CartSummary';
import OrderConfirmationModal from '../components/user/OrderConfirmationModal';
import { validateCheckoutData } from '../utils/checkoutValidator';

/**
 * Modern Checkout Page - Two Column Layout
 * Left: Checkout Steps (Vertical Timeline)
 * Right: Cart Summary (Sticky)
 */
const ModernCheckoutPage = () => {
  const navigate = useNavigate();
  const { cart, clearCart } = useCart();
  const { user, isAuthenticated } = useAuth();

  const [currentStep, setCurrentStep] = useState(1);
  const [completedSteps, setCompletedSteps] = useState([]);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showConfirmation, setShowConfirmation] = useState(false);
  const [createdOrder, setCreatedOrder] = useState(null);
  const [errors, setErrors] = useState({});

  const [checkoutData, setCheckoutData] = useState({
    // Account (Step 1)
    isGuest: false,
    guestEmail: '',
    guestPhone: '',

    // Event Details (Step 2)
    eventType: '',
    eventDate: null,
    eventTime: null,
    guestCount: cart?.guestCount || 50,
    eventLocation: '',
    eventAddress: {
      street: '',
      city: '',
      state: '',
      pincode: '',
      landmark: ''
    },
    specialInstructions: '',

    // Delivery Type (Step 3)
    deliveryType: 'event', // 'sample' or 'event'
    scheduledDispatchTime: null,

    // Payment (Step 4)
    paymentMethod: 'online', // 'online', 'partial', 'cod'
    advanceAmount: 0,
    termsAccepted: false
  });

  // Redirect if cart is empty
  useEffect(() => {
    if (!cart || !cart.cateringId) {
      navigate('/');
    }
  }, [cart, navigate]);

  // Auto-advance to step 2 if authenticated
  useEffect(() => {
    if (isAuthenticated && currentStep === 1) {
      markStepComplete(1);
      setCurrentStep(2);
    }
  }, [isAuthenticated]);

  const updateCheckoutData = (field, value) => {
    setCheckoutData(prev => ({
      ...prev,
      [field]: value
    }));
    // Clear error for this field
    if (errors[field]) {
      setErrors(prev => {
        const newErrors = { ...prev };
        delete newErrors[field];
        return newErrors;
      });
    }
  };

  const markStepComplete = (step) => {
    if (!completedSteps.includes(step)) {
      setCompletedSteps(prev => [...prev, step]);
    }
  };

  const validateStep = (step) => {
    const validation = validateCheckoutData(checkoutData, cart, step);

    if (!validation.isValid) {
      setErrors(validation.errors);
      return false;
    }

    setErrors({});
    return true;
  };

  const handleStepComplete = (step) => {
    if (validateStep(step)) {
      markStepComplete(step);
      if (step < 4) {
        setCurrentStep(step + 1);
        window.scrollTo({ top: 0, behavior: 'smooth' });
      }
    }
  };

  const handleSubmitOrder = async () => {
    // Final validation
    if (!validateStep(4)) {
      return;
    }

    if (!checkoutData.termsAccepted) {
      setErrors({ termsAccepted: 'Please accept terms and conditions' });
      return;
    }

    setIsSubmitting(true);

    try {
      // Prepare order items
      const orderItems = [];

      if (cart.packageId) {
        orderItems.push({
          itemType: 'Package',
          itemId: cart.packageId,
          itemName: cart.packageName,
          quantity: 1,
          unitPrice: cart.packagePrice,
          totalPrice: cart.packagePrice * checkoutData.guestCount,
          packageSelections: cart.packageSelections ? JSON.stringify(cart.packageSelections) : null
        });
      }

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
        eventLocation: `${checkoutData.eventAddress.street}, ${checkoutData.eventAddress.city}, ${checkoutData.eventAddress.state} - ${checkoutData.eventAddress.pincode}`,
        guestCount: checkoutData.guestCount,
        specialInstructions: checkoutData.specialInstructions || null,
        deliveryAddress: `${checkoutData.eventAddress.street}, ${checkoutData.eventAddress.city}`,
        contactPerson: user?.name || 'Guest',
        contactPhone: checkoutData.isGuest ? checkoutData.guestPhone : user?.phone,
        contactEmail: checkoutData.isGuest ? checkoutData.guestEmail : user?.email,
        baseAmount: cart.baseAmount || 0,
        taxAmount: cart.taxAmount || 0,
        deliveryCharges: 0,
        discountAmount: cart.discountAmount || 0,
        totalAmount: cart.totalAmount,
        paymentMethod: checkoutData.paymentMethod,
        deliveryType: checkoutData.deliveryType,
        scheduledDispatchTime: checkoutData.scheduledDispatchTime,
        orderItems: orderItems
      };

      const response = await createOrder(orderData);

      if (response.result) {
        setCreatedOrder(response.data);
        clearCart();
        setShowConfirmation(true);
      } else {
        setErrors({ submit: response.message || 'Failed to create order' });
      }
    } catch (error) {
      console.error('Error submitting order:', error);
      setErrors({ submit: 'An error occurred. Please try again.' });
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!cart) {
    return null;
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white border-b sticky top-0 z-40 shadow-sm">
        <div className="max-w-7xl mx-auto px-4 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <button
                onClick={() => navigate(-1)}
                className="text-gray-600 hover:text-gray-900"
              >
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                </svg>
              </button>
              <div>
                <h1 className="text-2xl font-bold text-gray-900">Checkout</h1>
                <p className="text-sm text-gray-600">Complete your catering order</p>
              </div>
            </div>
            <div className="hidden md:flex items-center gap-2 text-sm">
              <svg className="w-5 h-5 text-green-500" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M2.166 4.999A11.954 11.954 0 0010 1.944 11.954 11.954 0 0017.834 5c.11.65.166 1.32.166 2.001 0 5.225-3.34 9.67-8 11.317C5.34 16.67 2 12.225 2 7c0-.682.057-1.35.166-2.001zm11.541 3.708a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
              </svg>
              <span className="text-gray-700">Secure Checkout</span>
            </div>
          </div>
        </div>
      </div>

      {/* Main Content - Two Column Layout */}
      <div className="max-w-7xl mx-auto px-4 py-8">
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
          {/* LEFT COLUMN - Checkout Steps */}
          <div className="lg:col-span-7 space-y-4">
            {/* Account Section */}
            <AccountSection
              stepNumber={1}
              isActive={currentStep === 1}
              isCompleted={completedSteps.includes(1)}
              checkoutData={checkoutData}
              updateCheckoutData={updateCheckoutData}
              errors={errors}
              onComplete={() => handleStepComplete(1)}
              onEdit={() => setCurrentStep(1)}
            />

            {/* Event Details Section */}
            <EventDetailsSection
              stepNumber={2}
              isActive={currentStep === 2}
              isCompleted={completedSteps.includes(2)}
              checkoutData={checkoutData}
              updateCheckoutData={updateCheckoutData}
              errors={errors}
              onComplete={() => handleStepComplete(2)}
              onEdit={() => setCurrentStep(2)}
              cart={cart}
            />

            {/* Delivery Type Section */}
            <DeliveryTypeSection
              stepNumber={3}
              isActive={currentStep === 3}
              isCompleted={completedSteps.includes(3)}
              checkoutData={checkoutData}
              updateCheckoutData={updateCheckoutData}
              errors={errors}
              onComplete={() => handleStepComplete(3)}
              onEdit={() => setCurrentStep(3)}
            />

            {/* Payment Section */}
            <PaymentSection
              stepNumber={4}
              isActive={currentStep === 4}
              isCompleted={completedSteps.includes(4)}
              checkoutData={checkoutData}
              updateCheckoutData={updateCheckoutData}
              errors={errors}
              cart={cart}
              onSubmit={handleSubmitOrder}
              isSubmitting={isSubmitting}
            />
          </div>

          {/* RIGHT COLUMN - Cart Summary (Sticky) */}
          <div className="lg:col-span-5">
            <div className="sticky top-24">
              <CartSummary
                cart={cart}
                checkoutData={checkoutData}
                canPlaceOrder={completedSteps.length >= 3 && currentStep === 4}
                onPlaceOrder={handleSubmitOrder}
                isSubmitting={isSubmitting}
              />
            </div>
          </div>
        </div>
      </div>

      {/* Order Confirmation Modal */}
      {showConfirmation && createdOrder && (
        <OrderConfirmationModal
          order={createdOrder}
          onClose={() => {
            setShowConfirmation(false);
            navigate('/my-orders');
          }}
        />
      )}
    </div>
  );
};

export default ModernCheckoutPage;
