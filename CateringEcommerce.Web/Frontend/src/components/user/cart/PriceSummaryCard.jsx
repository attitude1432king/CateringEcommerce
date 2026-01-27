import React from 'react';

/**
 * Price Summary Card Component (Sticky Right Column)
 * Shows comprehensive price breakdown with all charges
 */
const PriceSummaryCard = ({ cart, discountAmount, appliedCoupon, onProceedToCheckout }) => {
  if (!cart) return null;

  const calculateTotals = () => {
    const packageTotal = (cart.packagePrice || 0) * (cart.guestCount || 50);
    const decorationTotal = cart.decorationPrice || 0;
    const additionalTotal = cart.additionalItems?.reduce(
      (sum, item) => sum + (item.price * (item.quantity || 1) * cart.guestCount),
      0
    ) || 0;

    const subtotal = packageTotal + decorationTotal + additionalTotal;
    const discount = discountAmount || 0;
    const subtotalAfterDiscount = subtotal - discount;
    const gst = subtotalAfterDiscount * 0.18; // 18% GST
    const platformFee = subtotalAfterDiscount * 0.02; // 2% platform fee
    const total = subtotalAfterDiscount + gst + platformFee;

    return {
      packageTotal,
      decorationTotal,
      additionalTotal,
      subtotal,
      discount,
      subtotalAfterDiscount,
      gst,
      platformFee,
      total
    };
  };

  const totals = calculateTotals();

  const formatCurrency = (amount) => {
    return `₹${amount.toLocaleString('en-IN', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    })}`;
  };

  return (
    <div className="bg-white rounded-xl shadow-lg border border-gray-200 overflow-hidden">
      {/* Header */}
      <div className="bg-gradient-to-r from-orange-500 to-red-500 p-4 text-white">
        <h3 className="font-bold text-lg flex items-center gap-2">
          <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
            <path d="M4 4a2 2 0 00-2 2v1h16V6a2 2 0 00-2-2H4z" />
            <path fillRule="evenodd" d="M18 9H2v5a2 2 0 002 2h12a2 2 0 002-2V9zM4 13a1 1 0 011-1h1a1 1 0 110 2H5a1 1 0 01-1-1zm5-1a1 1 0 100 2h1a1 1 0 100-2H9z" clipRule="evenodd" />
          </svg>
          Bill Summary
        </h3>
      </div>

      {/* Price Breakdown */}
      <div className="p-5 space-y-3">
        {/* Package Total */}
        <div className="flex items-center justify-between text-sm">
          <div className="flex items-center gap-2">
            <span className="text-gray-700">Package ({cart.guestCount} guests)</span>
            <div className="group relative">
              <svg className="w-4 h-4 text-gray-400 cursor-help" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-8-3a1 1 0 00-.867.5 1 1 0 11-1.731-1A3 3 0 0113 8a3.001 3.001 0 01-2 2.83V11a1 1 0 11-2 0v-1a1 1 0 011-1 1 1 0 100-2zm0 8a1 1 0 100-2 1 1 0 000 2z" clipRule="evenodd" />
              </svg>
              <div className="absolute left-0 bottom-full mb-2 hidden group-hover:block w-48 p-2 bg-gray-900 text-white text-xs rounded shadow-lg z-10">
                ₹{cart.packagePrice?.toLocaleString('en-IN')} × {cart.guestCount} guests
              </div>
            </div>
          </div>
          <span className="font-semibold text-gray-900">{formatCurrency(totals.packageTotal)}</span>
        </div>

        {/* Decoration */}
        {totals.decorationTotal > 0 && (
          <div className="flex items-center justify-between text-sm">
            <span className="text-gray-700">Decoration & Setup</span>
            <span className="font-semibold text-gray-900">{formatCurrency(totals.decorationTotal)}</span>
          </div>
        )}

        {/* Additional Items */}
        {totals.additionalTotal > 0 && (
          <div className="flex items-center justify-between text-sm">
            <div className="flex items-center gap-2">
              <span className="text-gray-700">Additional Items</span>
              <div className="group relative">
                <svg className="w-4 h-4 text-gray-400 cursor-help" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-8-3a1 1 0 00-.867.5 1 1 0 11-1.731-1A3 3 0 0113 8a3.001 3.001 0 01-2 2.83V11a1 1 0 11-2 0v-1a1 1 0 011-1 1 1 0 100-2zm0 8a1 1 0 100-2 1 1 0 000 2z" clipRule="evenodd" />
                </svg>
                <div className="absolute left-0 bottom-full mb-2 hidden group-hover:block w-48 p-2 bg-gray-900 text-white text-xs rounded shadow-lg z-10">
                  {cart.additionalItems?.length} extra items
                </div>
              </div>
            </div>
            <span className="font-semibold text-gray-900">{formatCurrency(totals.additionalTotal)}</span>
          </div>
        )}

        {/* Subtotal */}
        <div className="flex items-center justify-between text-sm pt-2 border-t border-dashed">
          <span className="font-medium text-gray-900">Item Total</span>
          <span className="font-semibold text-gray-900">{formatCurrency(totals.subtotal)}</span>
        </div>

        {/* Discount */}
        {totals.discount > 0 && (
          <div className="flex items-center justify-between text-sm bg-green-50 -mx-5 px-5 py-2">
            <div className="flex items-center gap-2">
              <svg className="w-4 h-4 text-green-600" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M5 5a3 3 0 015-2.236A3 3 0 0114.83 6H16a2 2 0 110 4h-5V9a1 1 0 10-2 0v1H4a2 2 0 110-4h1.17C5.06 5.687 5 5.35 5 5zm4 1V5a1 1 0 10-1 1h1zm3 0a1 1 0 10-1-1v1h1z" clipRule="evenodd" />
                <path d="M9 11H3v5a2 2 0 002 2h4v-7zM11 18h4a2 2 0 002-2v-5h-6v7z" />
              </svg>
              <span className="font-medium text-green-700">
                Coupon Discount {appliedCoupon && `(${appliedCoupon.code || appliedCoupon.couponCode})`}
              </span>
            </div>
            <span className="font-semibold text-green-700">-{formatCurrency(totals.discount)}</span>
          </div>
        )}

        {/* GST */}
        <div className="flex items-center justify-between text-sm">
          <div className="flex items-center gap-2">
            <span className="text-gray-700">GST (18%)</span>
            <div className="group relative">
              <svg className="w-4 h-4 text-gray-400 cursor-help" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-8-3a1 1 0 00-.867.5 1 1 0 11-1.731-1A3 3 0 0113 8a3.001 3.001 0 01-2 2.83V11a1 1 0 11-2 0v-1a1 1 0 011-1 1 1 0 100-2zm0 8a1 1 0 100-2 1 1 0 000 2z" clipRule="evenodd" />
              </svg>
              <div className="absolute left-0 bottom-full mb-2 hidden group-hover:block w-48 p-2 bg-gray-900 text-white text-xs rounded shadow-lg z-10">
                Goods and Services Tax included
              </div>
            </div>
          </div>
          <span className="font-semibold text-gray-900">{formatCurrency(totals.gst)}</span>
        </div>

        {/* Platform Fee */}
        <div className="flex items-center justify-between text-sm">
          <div className="flex items-center gap-2">
            <span className="text-gray-700">Platform Fee (2%)</span>
            <div className="group relative">
              <svg className="w-4 h-4 text-gray-400 cursor-help" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-8-3a1 1 0 00-.867.5 1 1 0 11-1.731-1A3 3 0 0113 8a3.001 3.001 0 01-2 2.83V11a1 1 0 11-2 0v-1a1 1 0 011-1 1 1 0 100-2zm0 8a1 1 0 100-2 1 1 0 000 2z" clipRule="evenodd" />
              </svg>
              <div className="absolute left-0 bottom-full mb-2 hidden group-hover:block w-56 p-2 bg-gray-900 text-white text-xs rounded shadow-lg z-10">
                Helps us maintain the platform and provide better service
              </div>
            </div>
          </div>
          <span className="font-semibold text-gray-900">{formatCurrency(totals.platformFee)}</span>
        </div>

        {/* Delivery Charges */}
        <div className="flex items-center justify-between text-sm">
          <span className="text-gray-700">Delivery Charges</span>
          <span className="font-semibold text-green-600">FREE</span>
        </div>
      </div>

      {/* Total Amount */}
      <div className="bg-gradient-to-r from-orange-50 to-red-50 border-t-2 border-orange-200 p-5">
        <div className="flex items-center justify-between mb-2">
          <span className="text-lg font-bold text-gray-900">Total Amount</span>
          <div className="text-right">
            <div className="text-2xl font-bold text-red-600">
              {formatCurrency(totals.total)}
            </div>
            <div className="text-xs text-gray-600">All taxes included</div>
          </div>
        </div>

        {/* Savings Banner */}
        {totals.discount > 0 && (
          <div className="mt-3 p-2 bg-green-100 border border-green-300 rounded-lg text-center">
            <div className="flex items-center justify-center gap-2 text-green-800 text-sm font-semibold">
              <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M6.267 3.455a3.066 3.066 0 001.745-.723 3.066 3.066 0 013.976 0 3.066 3.066 0 001.745.723 3.066 3.066 0 012.812 2.812c.051.643.304 1.254.723 1.745a3.066 3.066 0 010 3.976 3.066 3.066 0 00-.723 1.745 3.066 3.066 0 01-2.812 2.812 3.066 3.066 0 00-1.745.723 3.066 3.066 0 01-3.976 0 3.066 3.066 0 00-1.745-.723 3.066 3.066 0 01-2.812-2.812 3.066 3.066 0 00-.723-1.745 3.066 3.066 0 010-3.976 3.066 3.066 0 00.723-1.745 3.066 3.066 0 012.812-2.812zm7.44 5.252a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
              </svg>
              You saved {formatCurrency(totals.discount)} on this order!
            </div>
          </div>
        )}
      </div>

      {/* Proceed to Checkout Button */}
      <div className="p-5 border-t bg-gray-50">
        <button
          onClick={onProceedToCheckout}
          className="w-full bg-gradient-to-r from-orange-500 to-red-500 hover:from-orange-600 hover:to-red-600 text-white font-bold py-4 px-6 rounded-xl shadow-lg hover:shadow-xl transition-all transform hover:-translate-y-0.5"
        >
          <span className="flex items-center justify-center gap-2">
            Proceed to Checkout
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 7l5 5m0 0l-5 5m5-5H6" />
            </svg>
          </span>
        </button>

        {/* Trust Signals */}
        <div className="mt-4 flex items-center justify-center gap-4 text-xs text-gray-600">
          <div className="flex items-center gap-1">
            <svg className="w-4 h-4 text-green-600" fill="currentColor" viewBox="0 0 20 20">
              <path fillRule="evenodd" d="M2.166 4.999A11.954 11.954 0 0010 1.944 11.954 11.954 0 0017.834 5c.11.65.166 1.32.166 2.001 0 5.225-3.34 9.67-8 11.317C5.34 16.67 2 12.225 2 7c0-.682.057-1.35.166-2.001zm11.541 3.708a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
            </svg>
            <span>Secure Payment</span>
          </div>
          <div className="flex items-center gap-1">
            <svg className="w-4 h-4 text-blue-600" fill="currentColor" viewBox="0 0 20 20">
              <path d="M8 16.5a1.5 1.5 0 11-3 0 1.5 1.5 0 013 0zM15 16.5a1.5 1.5 0 11-3 0 1.5 1.5 0 013 0z" />
              <path d="M3 4a1 1 0 00-1 1v10a1 1 0 001 1h1.05a2.5 2.5 0 014.9 0H10a1 1 0 001-1V5a1 1 0 00-1-1H3zM14 7a1 1 0 00-1 1v6.05A2.5 2.5 0 0115.95 16H17a1 1 0 001-1v-5a1 1 0 00-.293-.707l-2-2A1 1 0 0015 7h-1z" />
            </svg>
            <span>Safe Delivery</span>
          </div>
        </div>
      </div>
    </div>
  );
};

export default PriceSummaryCard;
