import React, { useState } from 'react';
import { useCart } from '../../../contexts/CartContext';
import { usePayment } from '../../../contexts/PaymentContext';
import { validatePaymentReview, validateFileUpload, fileToBase64 } from '../../../utils/checkoutValidator';
import ModernPaymentMethodsSection from './ModernPaymentMethodsSection';
import RazorpayCheckoutButton from './RazorpayCheckoutButton';
import StickyPriceSummary from './StickyPriceSummary';

const PaymentReviewForm = ({ formData, onUpdate, onSubmit, onBack, isSubmitting }) => {
  const { cart } = useCart();
  const { isRazorpayLoaded } = usePayment();
  const [errors, setErrors] = useState({});
  const [paymentMethod, setPaymentMethod] = useState(formData.paymentMethod || 'Razorpay');
  const [paymentProof, setPaymentProof] = useState(formData.paymentProof || null);
  const [proofFileName, setProofFileName] = useState('');
  const [termsAccepted, setTermsAccepted] = useState(formData.termsAccepted || false);

  const handlePaymentMethodChange = (method) => {
    setPaymentMethod(method);
    // Clear payment proof if switching away from BankTransfer
    if (method !== 'BankTransfer') {
      setPaymentProof(null);
      setProofFileName('');
      setErrors(prev => ({ ...prev, paymentProof: undefined }));
    }
  };

  const handleFileUpload = async (e) => {
    const file = e.target.files[0];
    if (!file) return;

    // Validate file
    const validation = validateFileUpload(file);
    if (!validation.isValid) {
      setErrors(prev => ({ ...prev, paymentProof: validation.errors.file }));
      return;
    }

    try {
      // Convert to base64
      const base64 = await fileToBase64(file);
      setPaymentProof({
        base64,
        name: file.name
      });
      setProofFileName(file.name);
      setErrors(prev => ({ ...prev, paymentProof: undefined }));
    } catch (error) {
      console.error('Error converting file:', error);
      setErrors(prev => ({ ...prev, paymentProof: 'Failed to upload file' }));
    }
  };

  const handleRazorpaySuccess = (paymentResponse) => {
    // Update form data with Razorpay payment details
    const updatedData = {
      paymentMethod: 'Razorpay',
      paymentProof: null,
      termsAccepted,
      razorpayPaymentId: paymentResponse.razorpayPaymentId,
      razorpayOrderId: paymentResponse.razorpayOrderId,
      razorpaySignature: paymentResponse.razorpaySignature,
    };
    onUpdate(updatedData);
    onSubmit();
  };

  const handleRazorpayFailure = (error) => {
    console.error('Razorpay payment failed:', error);
    setErrors(prev => ({
      ...prev,
      payment: error.description || error.message || 'Payment failed. Please try again.',
    }));
  };

  const handleSubmit = () => {
    const dataToValidate = {
      paymentMethod,
      paymentProof,
      termsAccepted
    };

    const validation = validatePaymentReview(dataToValidate);

    if (!validation.isValid) {
      setErrors(validation.errors);
      return;
    }

    // Clear errors and submit
    setErrors({});
    onUpdate(dataToValidate);
    onSubmit();
  };

  return (
    <div className="payment-review-form">
      <h2 className="text-3xl font-bold mb-2 text-gray-900">Payment & Review</h2>
      <p className="text-gray-600 mb-8">Review your order and complete the payment</p>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Main Content - Left Side */}
        <div className="lg:col-span-2 space-y-6">
          {/* Event Details Summary Card */}
          <div className="bg-gradient-to-br from-blue-50 to-indigo-50 border border-blue-200 rounded-xl p-6 shadow-sm">
            <div className="flex items-center mb-4">
              <div className="p-2 bg-blue-500 rounded-lg mr-3">
                <svg className="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
              </div>
              <h3 className="text-lg font-semibold text-gray-900">Event Details</h3>
            </div>
            <div className="grid grid-cols-2 gap-4 text-sm">
              <div className="bg-white rounded-lg p-3 shadow-sm">
                <div className="text-gray-500 text-xs mb-1">Date</div>
                <div className="font-semibold text-gray-900">{formData.eventDate}</div>
              </div>
              <div className="bg-white rounded-lg p-3 shadow-sm">
                <div className="text-gray-500 text-xs mb-1">Time</div>
                <div className="font-semibold text-gray-900">{formData.eventTime}</div>
              </div>
              <div className="bg-white rounded-lg p-3 shadow-sm">
                <div className="text-gray-500 text-xs mb-1">Event Type</div>
                <div className="font-semibold text-gray-900">{formData.eventType}</div>
              </div>
              <div className="bg-white rounded-lg p-3 shadow-sm">
                <div className="text-gray-500 text-xs mb-1">Guests</div>
                <div className="font-semibold text-gray-900">{cart?.guestCount}</div>
              </div>
            </div>
          </div>

          {/* Payment Method Selection */}
          <div className="bg-white rounded-xl border border-gray-200 p-6 shadow-sm">
            <ModernPaymentMethodsSection
              selectedMethod={paymentMethod}
              onMethodChange={handlePaymentMethodChange}
              error={errors.paymentMethod}
            />
          </div>

          {/* Bank Transfer Section */}
          {paymentMethod === 'BankTransfer' && (
            <div className="bg-gradient-to-br from-blue-50 to-blue-100 border border-blue-300 rounded-xl p-6 shadow-sm">
              <div className="flex items-start mb-4">
                <div className="p-2 bg-blue-500 rounded-lg mr-3">
                  <svg className="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z" />
                  </svg>
                </div>
                <div className="flex-1">
                  <h4 className="font-semibold text-gray-900 mb-1">Bank Account Details</h4>
                  <p className="text-sm text-gray-600">Transfer the total amount to the account below</p>
                </div>
              </div>

              <div className="bg-white rounded-lg p-4 space-y-2 mb-4 shadow-sm">
                <div className="flex justify-between text-sm">
                  <span className="text-gray-600">Bank Name</span>
                  <span className="font-semibold text-gray-900">HDFC Bank</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-gray-600">Account Name</span>
                  <span className="font-semibold text-gray-900">Enyvora Catering Services</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-gray-600">Account Number</span>
                  <span className="font-semibold text-gray-900">1234567890123456</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-gray-600">IFSC Code</span>
                  <span className="font-semibold text-gray-900">HDFC0001234</span>
                </div>
              </div>

              <div className="border-t border-blue-200 pt-4">
                <label className="block font-medium text-gray-900 mb-3">
                  Upload Payment Proof <span className="text-red-500">*</span>
                </label>
                <input
                  type="file"
                  onChange={handleFileUpload}
                  accept="image/*,.pdf"
                  className="block w-full text-sm text-gray-600
                    file:mr-4 file:py-3 file:px-6
                    file:rounded-lg file:border-0
                    file:text-sm file:font-semibold
                    file:bg-gradient-to-r file:from-red-500 file:to-red-600
                    file:text-white
                    hover:file:from-red-600 hover:file:to-red-700
                    file:cursor-pointer file:shadow-md
                    cursor-pointer"
                />
                {proofFileName && (
                  <p className="mt-3 text-sm text-green-700 flex items-center bg-green-50 p-2 rounded-lg">
                    <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M5 13l4 4L19 7" />
                    </svg>
                    {proofFileName}
                  </p>
                )}
                {errors.paymentProof && (
                  <p className="mt-2 text-sm text-red-600">{errors.paymentProof}</p>
                )}
                <p className="mt-2 text-xs text-gray-600">
                  Accepted formats: JPEG, PNG, PDF (Max 5MB)
                </p>
              </div>
            </div>
          )}

          {/* Terms & Conditions */}
          <div className="bg-white rounded-xl border border-gray-200 p-6 shadow-sm">
            <label className="flex items-start cursor-pointer group">
              <input
                type="checkbox"
                checked={termsAccepted}
                onChange={(e) => setTermsAccepted(e.target.checked)}
                className={`mt-1 mr-4 w-5 h-5 text-red-500 focus:ring-red-500 rounded border-gray-300 ${
                  errors.termsAccepted ? 'border-red-500' : ''
                }`}
              />
              <span className="text-sm text-gray-700 group-hover:text-gray-900">
                I accept the{' '}
                <a href="#" className="text-red-500 hover:text-red-600 font-medium underline">
                  terms and conditions
                </a>{' '}
                and{' '}
                <a href="#" className="text-red-500 hover:text-red-600 font-medium underline">
                  cancellation policy
                </a>
                <span className="text-red-500">*</span>
              </span>
            </label>
            {errors.termsAccepted && (
              <p className="mt-2 text-sm text-red-600 ml-9">{errors.termsAccepted}</p>
            )}
          </div>

          {/* Error Messages */}
          {errors.payment && (
            <div className="bg-red-50 border border-red-200 rounded-xl p-4">
              <div className="flex items-center text-red-800">
                <svg className="w-5 h-5 mr-2 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
                </svg>
                <span>{errors.payment}</span>
              </div>
            </div>
          )}
        </div>

        {/* Sticky Price Summary - Right Side */}
        <div className="lg:col-span-1">
          <div className="sticky top-4">
            <StickyPriceSummary cart={cart} />
          </div>
        </div>
      </div>

      {/* Navigation Buttons */}
      <div className="flex justify-between items-center mt-8 pt-6 border-t border-gray-200">
        <button
          onClick={onBack}
          disabled={isSubmitting}
          className="px-6 py-3 border-2 border-gray-300 text-gray-700 font-semibold rounded-lg hover:bg-gray-50 hover:border-gray-400 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <svg className="w-5 h-5 inline mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M15 19l-7-7 7-7" />
          </svg>
          Back
        </button>

        {paymentMethod === 'Razorpay' ? (
          <RazorpayCheckoutButton
            amount={cart?.totalAmount}
            orderDetails={{
              cateringId: cart?.cateringId,
              eventDate: formData.eventDate,
              eventTime: formData.eventTime,
              eventType: formData.eventType,
              contactPerson: formData.contactPerson,
              contactEmail: formData.contactEmail,
              contactPhone: formData.contactPhone,
              description: `Catering for ${formData.eventType} on ${formData.eventDate}`,
            }}
            onSuccess={handleRazorpaySuccess}
            onFailure={handleRazorpayFailure}
            disabled={!termsAccepted || isSubmitting}
            className="min-w-[200px]"
          />
        ) : (
          <button
            onClick={handleSubmit}
            disabled={isSubmitting}
            className="px-8 py-3 bg-gradient-to-r from-red-500 to-red-600 text-white font-semibold rounded-lg hover:from-red-600 hover:to-red-700 transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center shadow-lg hover:shadow-xl"
          >
            {isSubmitting ? (
              <>
                <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                Placing Order...
              </>
            ) : (
              <>
                Place Order
                <svg className="w-5 h-5 ml-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 5l7 7-7 7" />
                </svg>
              </>
            )}
          </button>
        )}
      </div>
    </div>
  );
};

export default PaymentReviewForm;
