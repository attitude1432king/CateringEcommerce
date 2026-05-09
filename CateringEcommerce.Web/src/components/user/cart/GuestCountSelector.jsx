import React, { useState } from 'react';

/**
 * Guest Count Selector Component
 * Modern UI for adjusting guest count with presets and manual input
 */
const GuestCountSelector = ({ guestCount, onGuestCountChange, packageMinimum = 50 }) => {
  const [isEditing, setIsEditing] = useState(false);
  const [tempValue, setTempValue] = useState(guestCount);

  const quickPresets = [50, 100, 150, 200, 300, 500];

  const handleIncrement = (amount) => {
    const newCount = guestCount + amount;
    if (newCount <= 10000) {
      onGuestCountChange(newCount);
    }
  };

  const handleDecrement = (amount) => {
    const newCount = guestCount - amount;
    if (newCount >= packageMinimum) {
      onGuestCountChange(newCount);
    }
  };

  const handlePresetClick = (preset) => {
    if (preset >= packageMinimum) {
      onGuestCountChange(preset);
    }
  };

  const handleManualInput = (value) => {
    const num = parseInt(value) || packageMinimum;
    setTempValue(num);
  };

  const handleManualSubmit = () => {
    const finalValue = Math.max(packageMinimum, Math.min(10000, tempValue));
    onGuestCountChange(finalValue);
    setIsEditing(false);
  };

  const getPriceImpact = () => {
    // This shows how guest count affects total price
    const basePrice = 500; // Example base price per plate
    return basePrice * guestCount;
  };

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
            <svg className="w-6 h-6 text-blue-600" fill="currentColor" viewBox="0 0 20 20">
              <path d="M13 6a3 3 0 11-6 0 3 3 0 016 0zM18 8a2 2 0 11-4 0 2 2 0 014 0zM14 15a4 4 0 00-8 0v3h8v-3zM6 8a2 2 0 11-4 0 2 2 0 014 0zM16 18v-3a5.972 5.972 0 00-.75-2.906A3.005 3.005 0 0119 15v3h-3zM4.75 12.094A5.973 5.973 0 004 15v3H1v-3a3 3 0 013.75-2.906z" />
            </svg>
          </div>
          <div>
            <h3 className="font-bold text-neutral-900">Guest Count</h3>
            <p className="text-xs text-neutral-600">Adjust the number of people attending</p>
          </div>
        </div>
        {packageMinimum > 50 && (
          <span className="text-xs bg-yellow-100 text-yellow-800 px-2 py-1 rounded-full">
            Min: {packageMinimum} guests
          </span>
        )}
      </div>

      {/* Main Counter */}
      <div className="bg-gradient-to-r from-orange-50 to-red-50 rounded-xl p-6 mb-4">
        <div className="flex items-center justify-center gap-4">
          {/* Decrease Buttons */}
          <div className="flex gap-2">
            <button
              onClick={() => handleDecrement(50)}
              disabled={guestCount - 50 < packageMinimum}
              className="w-10 h-10 flex items-center justify-center bg-white border-2 border-gray-300 rounded-lg hover:bg-gray-50 hover:border-orange-500 disabled:opacity-40 disabled:cursor-not-allowed transition-all"
              title="Decrease by 50"
            >
              <span className="text-sm font-bold text-neutral-700">-50</span>
            </button>
            <button
              onClick={() => handleDecrement(10)}
              disabled={guestCount - 10 < packageMinimum}
              className="w-10 h-10 flex items-center justify-center bg-white border-2 border-gray-300 rounded-lg hover:bg-gray-50 hover:border-orange-500 disabled:opacity-40 disabled:cursor-not-allowed transition-all"
              title="Decrease by 10"
            >
              <svg className="w-5 h-5 text-neutral-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 12H4" />
              </svg>
            </button>
          </div>

          {/* Current Count Display */}
          {!isEditing ? (
            <button
              onClick={() => {
                setIsEditing(true);
                setTempValue(guestCount);
              }}
              className="group relative"
            >
              <div className="text-center min-w-[120px] px-6 py-3 bg-white rounded-xl border-2 border-orange-500 hover:border-orange-600 transition-all shadow-md">
                <div className="text-4xl font-bold text-primary leading-none">
                  {guestCount}
                </div>
                <div className="text-xs text-neutral-600 mt-1 uppercase tracking-wide font-medium">
                  Guests
                </div>
              </div>
              <div className="absolute -bottom-6 left-1/2 transform -translate-x-1/2 opacity-0 group-hover:opacity-100 transition-opacity">
                <span className="text-xs text-neutral-500 whitespace-nowrap">Click to edit</span>
              </div>
            </button>
          ) : (
            <div className="min-w-[120px]">
              <input
                type="number"
                value={tempValue}
                onChange={(e) => handleManualInput(e.target.value)}
                onBlur={handleManualSubmit}
                onKeyPress={(e) => {
                  if (e.key === 'Enter') {
                    handleManualSubmit();
                  }
                }}
                autoFocus
                className="w-full text-center text-4xl font-bold text-primary px-4 py-3 border-2 border-orange-500 rounded-xl focus:outline-none focus:ring-2 focus:ring-orange-500"
                min={packageMinimum}
                max={10000}
              />
            </div>
          )}

          {/* Increase Buttons */}
          <div className="flex gap-2">
            <button
              onClick={() => handleIncrement(10)}
              disabled={guestCount + 10 > 10000}
              className="w-10 h-10 flex items-center justify-center bg-white border-2 border-gray-300 rounded-lg hover:bg-gray-50 hover:border-orange-500 disabled:opacity-40 disabled:cursor-not-allowed transition-all"
              title="Increase by 10"
            >
              <svg className="w-5 h-5 text-neutral-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
              </svg>
            </button>
            <button
              onClick={() => handleIncrement(50)}
              disabled={guestCount + 50 > 10000}
              className="w-10 h-10 flex items-center justify-center bg-white border-2 border-gray-300 rounded-lg hover:bg-gray-50 hover:border-orange-500 disabled:opacity-40 disabled:cursor-not-allowed transition-all"
              title="Increase by 50"
            >
              <span className="text-sm font-bold text-neutral-700">+50</span>
            </button>
          </div>
        </div>

        {/* Range Indicator */}
        <div className="mt-4 text-center">
          <div className="text-xs text-neutral-600">
            Range: {packageMinimum} - 10,000 guests
          </div>
        </div>
      </div>

      {/* Quick Presets */}
      <div>
        <p className="text-xs font-medium text-neutral-700 mb-2">Quick Select:</p>
        <div className="grid grid-cols-3 sm:grid-cols-6 gap-2">
          {quickPresets.map((preset) => (
            <button
              key={preset}
              onClick={() => handlePresetClick(preset)}
              disabled={preset < packageMinimum}
              className={`py-2 px-3 rounded-lg text-sm font-medium transition-all ${
                guestCount === preset
                  ? 'bg-primary text-white shadow-md'
                  : preset < packageMinimum
                  ? 'bg-gray-100 text-gray-400 cursor-not-allowed'
                  : 'bg-gray-100 text-neutral-700 hover:bg-primary/10 hover:text-orange-700 border border-gray-200'
              }`}
            >
              {preset}
            </button>
          ))}
        </div>
      </div>

      {/* Info Banner */}
      <div className="mt-4 flex items-start gap-2 p-3 bg-blue-50 border border-blue-200 rounded-lg">
        <svg className="w-5 h-5 text-blue-600 mt-0.5 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
          <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
        </svg>
        <div className="flex-1">
          <p className="text-xs text-blue-900">
            <strong>Note:</strong> Package prices are calculated per plate. Changing guest count will update the total amount accordingly.
          </p>
        </div>
      </div>
    </div>
  );
};

export default GuestCountSelector;
