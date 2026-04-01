import React from 'react';
import { useAuth } from '../../../../contexts/AuthContext';

/**
 * Account Section - Step 1 of Modern Checkout
 * Shows logged-in user info or guest checkout option
 */
const AccountSection = ({ checkoutData, onUpdate, onComplete, errors = {} }) => {
  const { user, isAuthenticated } = useAuth();

  const handleGuestCheckout = () => {
    onUpdate({ isGuest: true });
  };

  const handleLoginClick = () => {
    // Navigate to login (handled by parent)
    window.location.href = '/';
  };

  const handleNext = () => {
    if (!isAuthenticated && !checkoutData.isGuest) {
      alert('Please log in or continue as guest');
      return;
    }

    if (checkoutData.isGuest) {
      if (!checkoutData.guestEmail || !checkoutData.guestPhone) {
        alert('Please provide your email and phone number');
        return;
      }
    }

      onComplete();
  };

  return (
    <div className="bg-white rounded-xl shadow-sm border border-neutral-200 p-6">
      <div className="flex items-center gap-3 mb-6">
        <div className="w-10 h-10 bg-rose-100 rounded-full flex items-center justify-center">
          <svg className="w-6 h-6 text-rose-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
          </svg>
        </div>
        <div>
          <h3 className="text-lg font-semibold text-neutral-800">Account</h3>
          <p className="text-sm text-neutral-600">Verify your identity</p>
        </div>
      </div>

      {isAuthenticated ? (
        <div className="space-y-4">
          <div className="bg-green-50 border border-green-200 rounded-lg p-4">
            <div className="flex items-start gap-3">
              <svg className="w-5 h-5 text-green-600 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <div className="flex-1">
                <p className="text-sm font-semibold text-green-800">Logged in as</p>
                <p className="text-sm text-green-700">{user?.name || user?.email}</p>
                <p className="text-xs text-green-600 mt-1">{user?.email}</p>
              </div>
            </div>
          </div>

          <button
            onClick={handleNext}
            className="w-full px-6 py-3 bg-rose-600 text-white rounded-lg font-medium hover:bg-rose-700 transition"
          >
            Continue
          </button>
        </div>
      ) : (
        <div className="space-y-4">
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
            <p className="text-sm text-blue-800">
              You can log in to track your order or continue as a guest.
            </p>
          </div>

          {checkoutData.isGuest ? (
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-neutral-700 mb-2">
                  Email Address *
                </label>
                <input
                  type="email"
                  value={checkoutData.guestEmail || ''}
                  onChange={(e) => onUpdate({ guestEmail: e.target.value })}
                  className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-rose-500 focus:border-transparent ${
                    errors.guestEmail ? 'border-red-500' : 'border-neutral-300'
                  }`}
                  placeholder="your@email.com"
                />
                {errors.guestEmail && (
                  <p className="text-xs text-red-600 mt-1">{errors.guestEmail}</p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-neutral-700 mb-2">
                  Phone Number *
                </label>
                <input
                  type="tel"
                  value={checkoutData.guestPhone || ''}
                  onChange={(e) => onUpdate({ guestPhone: e.target.value })}
                  className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-rose-500 focus:border-transparent ${
                    errors.guestPhone ? 'border-red-500' : 'border-neutral-300'
                  }`}
                  placeholder="10-digit mobile number"
                  maxLength="10"
                />
                {errors.guestPhone && (
                  <p className="text-xs text-red-600 mt-1">{errors.guestPhone}</p>
                )}
              </div>

              <button
                onClick={handleNext}
                className="w-full px-6 py-3 bg-rose-600 text-white rounded-lg font-medium hover:bg-rose-700 transition"
              >
                Continue as Guest
              </button>

              <button
                onClick={() => onUpdate({ isGuest: false, guestEmail: '', guestPhone: '' })}
                className="w-full px-6 py-3 bg-neutral-100 text-neutral-700 rounded-lg font-medium hover:bg-neutral-200 transition"
              >
                Back to Login Options
              </button>
            </div>
          ) : (
            <div className="space-y-3">
              <button
                onClick={handleLoginClick}
                className="w-full px-6 py-3 bg-rose-600 text-white rounded-lg font-medium hover:bg-rose-700 transition"
              >
                Log In
              </button>

              <div className="relative">
                <div className="absolute inset-0 flex items-center">
                  <div className="w-full border-t border-neutral-300"></div>
                </div>
                <div className="relative flex justify-center text-sm">
                  <span className="px-2 bg-white text-neutral-500">Or</span>
                </div>
              </div>

              <button
                onClick={handleGuestCheckout}
                className="w-full px-6 py-3 bg-neutral-100 text-neutral-700 rounded-lg font-medium hover:bg-neutral-200 transition"
              >
                Continue as Guest
              </button>
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default AccountSection;
