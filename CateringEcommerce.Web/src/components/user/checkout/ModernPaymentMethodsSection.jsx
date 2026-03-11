import React from 'react';

const ModernPaymentMethodsSection = ({ selectedMethod, onMethodChange, error }) => {
  const paymentMethods = [
    {
      id: 'Razorpay',
      name: 'Online Payment',
      description: 'Pay securely using UPI, Cards, Net Banking & more',
      icon: (
        <svg className="w-8 h-8" viewBox="0 0 24 24" fill="none">
          <rect x="2" y="5" width="20" height="14" rx="2" stroke="currentColor" strokeWidth="2" />
          <path d="M2 10h20" stroke="currentColor" strokeWidth="2" />
          <circle cx="7" cy="15" r="1" fill="currentColor" />
          <circle cx="11" cy="15" r="1" fill="currentColor" />
        </svg>
      ),
      badges: [
        { name: 'UPI', color: 'bg-green-100 text-green-700' },
        { name: 'Cards', color: 'bg-blue-100 text-blue-700' },
        { name: 'Wallets', color: 'bg-purple-100 text-purple-700' },
        { name: 'NetBanking', color: 'bg-orange-100 text-orange-700' },
      ],
      recommended: true,
    },
    {
      id: 'COD',
      name: 'Cash on Delivery',
      description: 'Pay when the service is delivered',
      icon: (
        <svg className="w-8 h-8" viewBox="0 0 24 24" fill="none">
          <path d="M12 2v20M2 12h20" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
          <circle cx="12" cy="12" r="9" stroke="currentColor" strokeWidth="2" />
        </svg>
      ),
      badges: [],
      recommended: false,
    },
    {
      id: 'BankTransfer',
      name: 'Bank Transfer',
      description: 'Direct bank transfer with proof upload',
      icon: (
        <svg className="w-8 h-8" viewBox="0 0 24 24" fill="none">
          <path d="M3 9l9-7 9 7v11a2 2 0 01-2 2H5a2 2 0 01-2-2V9z" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
          <path d="M9 22V12h6v10" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
        </svg>
      ),
      badges: [
        { name: 'NEFT', color: 'bg-indigo-100 text-indigo-700' },
        { name: 'RTGS', color: 'bg-pink-100 text-pink-700' },
        { name: 'IMPS', color: 'bg-teal-100 text-teal-700' },
      ],
      recommended: false,
    },
  ];

  return (
    <div className="space-y-3">
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-lg font-semibold text-gray-900">
          Payment Method <span className="text-red-500">*</span>
        </h3>
        <div className="flex items-center text-xs text-gray-500">
          <svg className="w-4 h-4 mr-1 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
          </svg>
          Secure Payment
        </div>
      </div>

      <div className="space-y-3">
        {paymentMethods.map((method) => (
          <label
            key={method.id}
            className={`relative flex items-start p-4 border-2 rounded-xl cursor-pointer transition-all duration-200 ${
              selectedMethod === method.id
                ? 'border-red-500 bg-red-50 shadow-md'
                : 'border-gray-200 hover:border-gray-300 hover:bg-gray-50'
            }`}
          >
            {/* Recommended Badge */}
            {method.recommended && (
              <div className="absolute -top-3 left-4 px-3 py-1 bg-gradient-to-r from-red-500 to-orange-500 text-white text-xs font-semibold rounded-full shadow-md">
                Recommended
              </div>
            )}

            {/* Radio Input */}
            <input
              type="radio"
              name="paymentMethod"
              value={method.id}
              checked={selectedMethod === method.id}
              onChange={() => onMethodChange(method.id)}
              className="mt-1 mr-4 text-red-500 focus:ring-red-500 w-5 h-5 flex-shrink-0"
            />

            {/* Icon */}
            <div
              className={`mr-4 p-2 rounded-lg flex-shrink-0 ${
                selectedMethod === method.id
                  ? 'bg-red-100 text-red-600'
                  : 'bg-gray-100 text-gray-600'
              }`}
            >
              {method.icon}
            </div>

            {/* Content */}
            <div className="flex-1 min-w-0">
              <div className="flex items-center justify-between mb-1">
                <div className="font-semibold text-gray-900">{method.name}</div>
                {selectedMethod === method.id && (
                  <svg className="w-6 h-6 text-red-500" fill="currentColor" viewBox="0 0 20 20">
                    <path
                      fillRule="evenodd"
                      d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
                      clipRule="evenodd"
                    />
                  </svg>
                )}
              </div>
              <div className="text-sm text-gray-600 mb-2">{method.description}</div>

              {/* Payment Option Badges */}
              {method.badges.length > 0 && (
                <div className="flex flex-wrap gap-2 mt-2">
                  {method.badges.map((badge) => (
                    <span
                      key={badge.name}
                      className={`px-2 py-1 text-xs font-medium rounded-full ${badge.color}`}
                    >
                      {badge.name}
                    </span>
                  ))}
                </div>
              )}
            </div>
          </label>
        ))}
      </div>

      {error && (
        <p className="mt-2 text-sm text-red-600 flex items-center">
          <svg className="w-4 h-4 mr-1" fill="currentColor" viewBox="0 0 20 20">
            <path
              fillRule="evenodd"
              d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z"
              clipRule="evenodd"
            />
          </svg>
          {error}
        </p>
      )}

      {/* Security Info */}
      <div className="mt-4 p-3 bg-blue-50 border border-blue-200 rounded-lg">
        <div className="flex items-start">
          <svg className="w-5 h-5 text-blue-500 mt-0.5 mr-2 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <div className="text-xs text-blue-800">
            <p className="font-medium mb-1">Your payment information is secure</p>
            <p className="text-blue-700">
              We use industry-standard encryption to protect your payment details. Your financial information is never stored on our servers.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ModernPaymentMethodsSection;
