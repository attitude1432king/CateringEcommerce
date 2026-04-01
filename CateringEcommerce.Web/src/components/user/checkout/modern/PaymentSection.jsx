import React, { useMemo } from 'react';
import { getPaymentMethodDisplay } from '../../../../utils/checkoutValidator';

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
  const total      = cart?.totalAmount || 0;
  const preBooking = useMemo(() => Math.round(total * 0.40), [total]);
  const postEvent  = useMemo(() => Math.round(total * 0.60), [total]);

  const fmt = (n) => `₹${Number(n).toLocaleString('en-IN')}`;

  const paymentMethods = [
    {
      value: 'online',
      title: 'Pay Full Amount Online',
      hint: `Pay ${fmt(total)} now via UPI, card or net banking. Fastest confirmation.`,
      badge: null,
    },
    {
      value: 'split',
      title: 'Split Payment (40 / 60)',
      hint: `Pay ${fmt(preBooking)} now to book + ${fmt(postEvent)} before your event.`,
      badge: 'Flexible',
    },
    {
      value: 'cod',
      title: 'Cash on Delivery',
      hint: 'Pay in cash at the time of your event. Available for select caterers.',
      badge: null,
    },
  ];

  const handleMethodChange = (method) => {
    updateCheckoutData('paymentMethod', method);
  };

  return (
    <div className="bg-white rounded-xl shadow-sm border border-neutral-200 p-6">
      {/* Step header */}
      <div className="flex items-center gap-3 mb-5">
        <div className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-semibold ${
          isCompleted ? 'bg-green-600 text-white' : isActive ? 'bg-rose-600 text-white' : 'bg-gray-200 text-gray-700'
        }`}>
          {isCompleted ? '✓' : stepNumber}
        </div>
        <div>
          <h3 className="text-lg font-semibold text-gray-900">Payment</h3>
          <p className="text-sm text-gray-600">Choose payment method and confirm order</p>
        </div>
      </div>

      {/* Completed summary */}
      {!isActive && isCompleted && (
        <p className="text-sm text-gray-700">
          <span className="font-medium">Payment:</span>{' '}
          {getPaymentMethodDisplay(checkoutData.paymentMethod)}
        </p>
      )}

      {/* Active form */}
      {isActive && (
        <div className="space-y-4">
          {/* Payment method options */}
          <div className="space-y-3">
            {paymentMethods.map((method) => {
              const selected = checkoutData.paymentMethod === method.value;
              return (
                <label
                  key={method.value}
                  className={`block border rounded-xl p-4 cursor-pointer transition-all ${
                    selected
                      ? 'border-rose-500 bg-rose-50 shadow-sm'
                      : 'border-gray-200 hover:border-gray-300 hover:bg-gray-50'
                  }`}
                >
                  <div className="flex items-start gap-3">
                    <input
                      type="radio"
                      name="paymentMethod"
                      value={method.value}
                      checked={selected}
                      onChange={(e) => handleMethodChange(e.target.value)}
                      className="mt-1 accent-rose-600"
                    />
                    <div className="flex-1">
                      <div className="flex items-center gap-2">
                        <p className="font-semibold text-gray-900">{method.title}</p>
                        {method.badge && (
                          <span className="text-xs font-medium px-2 py-0.5 rounded-full bg-indigo-100 text-indigo-700">
                            {method.badge}
                          </span>
                        )}
                      </div>
                      <p className="text-sm text-gray-500 mt-0.5">{method.hint}</p>
                    </div>
                  </div>

                  {/* Split payment breakdown */}
                  {method.value === 'split' && selected && (
                    <div className="mt-3 ml-6 grid grid-cols-2 gap-2">
                      <div className="bg-white rounded-lg border border-rose-200 p-3 text-center">
                        <p className="text-xs text-gray-500 mb-0.5">Pay Now (40%)</p>
                        <p className="text-base font-bold text-rose-600">{fmt(preBooking)}</p>
                      </div>
                      <div className="bg-white rounded-lg border border-gray-200 p-3 text-center">
                        <p className="text-xs text-gray-500 mb-0.5">Before Event (60%)</p>
                        <p className="text-base font-bold text-gray-700">{fmt(postEvent)}</p>
                      </div>
                    </div>
                  )}
                </label>
              );
            })}
          </div>

          {errors.paymentMethod && (
            <p className="text-xs text-red-600">{errors.paymentMethod}</p>
          )}

          {/* Terms */}
          <label className="flex items-start gap-3 cursor-pointer">
            <input
              type="checkbox"
              checked={Boolean(checkoutData.termsAccepted)}
              onChange={(e) => updateCheckoutData('termsAccepted', e.target.checked)}
              className="mt-1 accent-rose-600"
            />
            <span className="text-sm text-gray-700">
              I agree to the{' '}
              <span className="text-rose-600 font-medium">terms and conditions</span> and
              cancellation policy.
            </span>
          </label>
          {errors.termsAccepted && (
            <p className="text-xs text-red-600">{errors.termsAccepted}</p>
          )}

          {/* Submit error */}
          {errors.submit && (
            <p className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg p-3">
              {errors.submit}
            </p>
          )}

          {/* Place order CTA */}
          <button
            onClick={onSubmit}
            disabled={isSubmitting}
            className="w-full px-6 py-3 bg-rose-600 text-white rounded-xl font-semibold text-base hover:bg-rose-700 active:scale-[0.98] transition-all disabled:opacity-50 disabled:cursor-not-allowed shadow-sm"
          >
            {isSubmitting
              ? 'Placing Order…'
              : checkoutData.paymentMethod === 'cod'
                ? 'Place Order'
                : 'Place Order & Pay'}
          </button>
        </div>
      )}
    </div>
  );
};

export default PaymentSection;
