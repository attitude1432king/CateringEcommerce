import React, { useMemo } from 'react';
import { getPaymentMethodDisplay } from '../../../../utils/checkoutValidator';

const paymentMethods = [
  { value: 'online', title: 'Pay Full Amount Online', hint: 'Recommended for faster confirmation.' },
  { value: 'partial', title: 'Pay Advance Amount', hint: 'Pay minimum advance and settle later.' },
  { value: 'cod', title: 'Cash on Delivery', hint: 'Pay at delivery/event time.' }
];

const PaymentSection = ({
  stepNumber,
  isActive,
  isCompleted,
  checkoutData,
  updateCheckoutData,
  errors = {},
  cart,
  onSubmit,
  isSubmitting
}) => {
  const minimumAdvance = useMemo(() => {
    const total = cart?.totalAmount || 0;
    return Math.ceil(total * 0.3);
  }, [cart]);

  const handleMethodChange = (method) => {
    updateCheckoutData('paymentMethod', method);
    if (method !== 'partial') {
      updateCheckoutData('advanceAmount', 0);
    } else if (!checkoutData.advanceAmount) {
      updateCheckoutData('advanceAmount', minimumAdvance);
    }
  };

  return (
    <div className="bg-white rounded-xl shadow-sm border border-neutral-200 p-6">
      <div className="flex items-center justify-between mb-5">
        <div className="flex items-center gap-3">
          <div className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-semibold ${isCompleted ? 'bg-green-600 text-white' : isActive ? 'bg-rose-600 text-white' : 'bg-gray-200 text-gray-700'}`}>
            {isCompleted ? '✓' : stepNumber}
          </div>
          <div>
            <h3 className="text-lg font-semibold text-gray-900">Payment</h3>
            <p className="text-sm text-gray-600">Choose payment method and confirm order</p>
          </div>
        </div>
      </div>

      {!isActive && isCompleted ? (
        <p className="text-sm text-gray-700">
          <span className="font-medium">Payment:</span> {getPaymentMethodDisplay(checkoutData.paymentMethod)}
        </p>
      ) : null}

      {isActive && (
        <div className="space-y-4">
          <div className="space-y-3">
            {paymentMethods.map((method) => (
              <label
                key={method.value}
                className={`block border rounded-lg p-4 cursor-pointer transition ${checkoutData.paymentMethod === method.value ? 'border-rose-500 bg-rose-50' : 'border-gray-200 hover:border-gray-300'}`}
              >
                <div className="flex items-start gap-3">
                  <input
                    type="radio"
                    name="paymentMethod"
                    value={method.value}
                    checked={checkoutData.paymentMethod === method.value}
                    onChange={(e) => handleMethodChange(e.target.value)}
                    className="mt-1"
                  />
                  <div>
                    <p className="font-medium text-gray-900">{method.title}</p>
                    <p className="text-sm text-gray-600">{method.hint}</p>
                  </div>
                </div>
              </label>
            ))}
          </div>
          {errors.paymentMethod && <p className="text-xs text-red-600">{errors.paymentMethod}</p>}

          {checkoutData.paymentMethod === 'partial' && (
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Advance Amount (Minimum Rs. {minimumAdvance.toLocaleString('en-IN')})
              </label>
              <input
                type="number"
                min={minimumAdvance}
                max={cart?.totalAmount || undefined}
                value={checkoutData.advanceAmount || ''}
                onChange={(e) => updateCheckoutData('advanceAmount', Number(e.target.value) || 0)}
                className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-rose-500 focus:border-transparent ${errors.advanceAmount ? 'border-red-500' : 'border-gray-300'}`}
              />
              {errors.advanceAmount && <p className="text-xs text-red-600 mt-1">{errors.advanceAmount}</p>}
            </div>
          )}

          <label className="flex items-start gap-3">
            <input
              type="checkbox"
              checked={Boolean(checkoutData.termsAccepted)}
              onChange={(e) => updateCheckoutData('termsAccepted', e.target.checked)}
              className="mt-1"
            />
            <span className="text-sm text-gray-700">
              I agree to the terms and conditions and cancellation policy.
            </span>
          </label>
          {errors.termsAccepted && <p className="text-xs text-red-600">{errors.termsAccepted}</p>}
          {errors.submit && <p className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg p-3">{errors.submit}</p>}

          <button
            onClick={onSubmit}
            disabled={isSubmitting}
            className="w-full px-6 py-3 bg-rose-600 text-white rounded-lg font-medium hover:bg-rose-700 transition disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isSubmitting ? 'Placing Order...' : 'Place Order'}
          </button>
        </div>
      )}
    </div>
  );
};

export default PaymentSection;
