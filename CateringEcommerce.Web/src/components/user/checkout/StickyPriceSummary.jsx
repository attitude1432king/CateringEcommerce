import React from 'react';

const StickyPriceSummary = ({ cart, className = '' }) => {
  if (!cart) return null;

  const items = [
    {
      label: 'Base Amount',
      value: cart.baseAmount || 0,
      className: 'text-gray-700',
    },
    ...(cart.decorationAmount > 0
      ? [
          {
            label: 'Decoration',
            value: cart.decorationAmount,
            className: 'text-gray-700',
          },
        ]
      : []),
    ...(cart.additionalItemsTotal > 0
      ? [
          {
            label: 'Additional Items',
            value: cart.additionalItemsTotal,
            className: 'text-gray-700',
          },
        ]
      : []),
    ...(cart.discountAmount > 0
      ? [
          {
            label: 'Discount',
            value: -cart.discountAmount,
            className: 'text-green-600',
            prefix: '-',
          },
        ]
      : []),
    {
      label: 'GST (18%)',
      value: cart.taxAmount || 0,
      className: 'text-gray-700',
    },
  ];

  return (
    <div className={`bg-white rounded-lg shadow-lg border border-gray-200 overflow-hidden ${className}`}>
      {/* Header */}
      <div className="bg-gradient-to-r from-red-500 to-red-600 px-6 py-4">
        <h3 className="text-lg font-semibold text-white flex items-center">
          <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth="2"
              d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
            />
          </svg>
          Price Summary
        </h3>
      </div>

      {/* Items */}
      <div className="px-6 py-4">
        <div className="space-y-3">
          {items.map((item, index) => (
            <div key={index} className="flex justify-between items-center">
              <span className={`text-sm ${item.className}`}>{item.label}</span>
              <span className={`text-sm font-medium ${item.className}`}>
                {item.prefix}₹{Math.abs(item.value).toFixed(2)}
              </span>
            </div>
          ))}
        </div>

        {/* Divider */}
        <div className="my-4 border-t border-gray-200"></div>

        {/* Total */}
        <div className="flex justify-between items-center">
          <span className="text-base font-semibold text-gray-900">Total Amount</span>
          <div className="text-right">
            <div className="text-2xl font-bold text-red-600">
              ₹{cart.totalAmount?.toFixed(2)}
            </div>
            <div className="text-xs text-gray-500 mt-1">
              For {cart.guestCount} guests
            </div>
          </div>
        </div>

        {/* Per Person Cost */}
        {cart.guestCount > 0 && (
          <div className="mt-3 pt-3 border-t border-gray-100">
            <div className="flex justify-between items-center text-sm text-gray-600">
              <span>Per Person Cost</span>
              <span className="font-medium">
                ₹{(cart.totalAmount / cart.guestCount).toFixed(2)}
              </span>
            </div>
          </div>
        )}
      </div>

      {/* Footer Info */}
      <div className="px-6 py-3 bg-gray-50 border-t border-gray-200">
        <div className="flex items-start text-xs text-gray-600">
          <svg className="w-4 h-4 mr-1 mt-0.5 text-blue-500 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth="2"
              d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
            />
          </svg>
          <span>
            All prices are inclusive of taxes. Additional charges may apply based on location.
          </span>
        </div>
      </div>
    </div>
  );
};

export default StickyPriceSummary;
