import React from 'react';
import { getEventTypeDisplay, formatEventDate, formatEventTime } from '../../../../utils/checkoutValidator';
import { useAppSettings } from '../../../../contexts/AppSettingsContext';

const eventTypes = [
  { value: 'wedding', label: 'Wedding' },
  { value: 'birthday', label: 'Birthday Party' },
  { value: 'corporate', label: 'Corporate Event' },
  { value: 'anniversary', label: 'Anniversary' },
  { value: 'religious', label: 'Religious Function' },
  { value: 'social', label: 'Social Gathering' },
  { value: 'other', label: 'Other Event' }
];

const getMinBookingDate = (minAdvanceBookingDays) => {
  const minDate = new Date();
  minDate.setHours(0, 0, 0, 0);
  minDate.setDate(minDate.getDate() + minAdvanceBookingDays);
  return minDate.toISOString().split('T')[0];
};

const StepHeader = ({ stepNumber, title, subtitle, isActive, isCompleted, onEdit }) => (
  <div className="flex items-center justify-between mb-5">
    <div className="flex items-center gap-3">
      <div className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-semibold ${isCompleted ? 'bg-green-600 text-white' : isActive ? 'bg-rose-600 text-white' : 'bg-gray-200 text-gray-700'}`}>
        {isCompleted ? '✓' : stepNumber}
      </div>
      <div>
        <h3 className="text-lg font-semibold text-gray-900">{title}</h3>
        <p className="text-sm text-gray-600">{subtitle}</p>
      </div>
    </div>
    {isCompleted && !isActive && (
      <button onClick={onEdit} className="text-sm font-medium text-rose-600 hover:text-rose-700">
        Edit
      </button>
    )}
  </div>
);

const EventDetailsSection = ({
  stepNumber,
  isActive,
  isCompleted,
  checkoutData,
  updateCheckoutData,
  errors = {},
  onComplete,
  onEdit
}) => {
  const { getInt } = useAppSettings();
  const minAdvanceBookingDays = getInt('BUSINESS.MIN_ADVANCE_BOOKING_DAYS', 5);
  const updateAddressField = (field, value) => {
    updateCheckoutData('eventAddress', {
      ...checkoutData.eventAddress,
      [field]: value
    });
  };

  const summary = (
    <div className="text-sm text-gray-700 space-y-1">
      <p><span className="font-medium">Type:</span> {getEventTypeDisplay(checkoutData.eventType)}</p>
      <p><span className="font-medium">Date:</span> {formatEventDate(checkoutData.eventDate)}</p>
      {checkoutData.eventTime && <p><span className="font-medium">Time:</span> {formatEventTime(checkoutData.eventTime)}</p>}
      <p><span className="font-medium">Guests:</span> {checkoutData.guestCount}</p>
      <p className="text-gray-600">
        {checkoutData.eventAddress?.street}, {checkoutData.eventAddress?.city}, {checkoutData.eventAddress?.state} - {checkoutData.eventAddress?.pincode}
      </p>
    </div>
  );

  return (
    <div className="bg-white rounded-xl shadow-sm border border-neutral-200 p-6">
      <StepHeader
        stepNumber={stepNumber}
        title="Event Details"
        subtitle="Tell us about your event"
        isActive={isActive}
        isCompleted={isCompleted}
        onEdit={onEdit}
      />

      {!isActive && isCompleted ? summary : null}

      {isActive && (
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Event Type *</label>
            <select
              value={checkoutData.eventType || ''}
              onChange={(e) => updateCheckoutData('eventType', e.target.value)}
              className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-rose-500 focus:border-transparent ${errors.eventType ? 'border-red-500' : 'border-gray-300'}`}
            >
              <option value="">Select event type</option>
              {eventTypes.map((type) => (
                <option key={type.value} value={type.value}>
                  {type.label}
                </option>
              ))}
            </select>
            {errors.eventType && <p className="text-xs text-red-600 mt-1">{errors.eventType}</p>}
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Event Date *</label>
              <input
                type="date"
                min={getMinBookingDate(minAdvanceBookingDays)}
                value={checkoutData.eventDate || ''}
                onChange={(e) => updateCheckoutData('eventDate', e.target.value)}
                className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-rose-500 focus:border-transparent ${errors.eventDate ? 'border-red-500' : 'border-gray-300'}`}
              />
              {errors.eventDate && <p className="text-xs text-red-600 mt-1">{errors.eventDate}</p>}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Event Time *</label>
              <input
                type="time"
                value={checkoutData.eventTime || ''}
                onChange={(e) => updateCheckoutData('eventTime', e.target.value)}
                className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-rose-500 focus:border-transparent ${errors.eventTime ? 'border-red-500' : 'border-gray-300'}`}
              />
              {errors.eventTime && <p className="text-xs text-red-600 mt-1">{errors.eventTime}</p>}
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Guest Count *</label>
            <input
              type="number"
              min="1"
              value={checkoutData.guestCount || 1}
              onChange={(e) => updateCheckoutData('guestCount', Number(e.target.value) || 1)}
              className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-rose-500 focus:border-transparent ${errors.guestCount ? 'border-red-500' : 'border-gray-300'}`}
            />
            {errors.guestCount && <p className="text-xs text-red-600 mt-1">{errors.guestCount}</p>}
          </div>

          <div className="border rounded-lg p-4 bg-gray-50">
            <h4 className="text-sm font-semibold text-gray-900 mb-3">Event Address</h4>
            <div className="space-y-3">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Street *</label>
                <input
                  type="text"
                  value={checkoutData.eventAddress?.street || ''}
                  onChange={(e) => updateAddressField('street', e.target.value)}
                  className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-rose-500 focus:border-transparent ${errors.eventAddressStreet ? 'border-red-500' : 'border-gray-300'}`}
                  placeholder="House no, building, street"
                />
                {errors.eventAddressStreet && <p className="text-xs text-red-600 mt-1">{errors.eventAddressStreet}</p>}
              </div>

              <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">City *</label>
                  <input
                    type="text"
                    value={checkoutData.eventAddress?.city || ''}
                    onChange={(e) => updateAddressField('city', e.target.value)}
                    className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-rose-500 focus:border-transparent ${errors.eventAddressCity ? 'border-red-500' : 'border-gray-300'}`}
                    placeholder="City"
                  />
                  {errors.eventAddressCity && <p className="text-xs text-red-600 mt-1">{errors.eventAddressCity}</p>}
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">State *</label>
                  <input
                    type="text"
                    value={checkoutData.eventAddress?.state || ''}
                    onChange={(e) => updateAddressField('state', e.target.value)}
                    className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-rose-500 focus:border-transparent ${errors.eventAddressState ? 'border-red-500' : 'border-gray-300'}`}
                    placeholder="State"
                  />
                  {errors.eventAddressState && <p className="text-xs text-red-600 mt-1">{errors.eventAddressState}</p>}
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Pincode *</label>
                  <input
                    type="text"
                    maxLength="6"
                    value={checkoutData.eventAddress?.pincode || ''}
                    onChange={(e) => updateAddressField('pincode', e.target.value.replace(/\D/g, ''))}
                    className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-rose-500 focus:border-transparent ${errors.eventAddressPincode ? 'border-red-500' : 'border-gray-300'}`}
                    placeholder="6-digit pincode"
                  />
                  {errors.eventAddressPincode && <p className="text-xs text-red-600 mt-1">{errors.eventAddressPincode}</p>}
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Landmark</label>
                <input
                  type="text"
                  value={checkoutData.eventAddress?.landmark || ''}
                  onChange={(e) => updateAddressField('landmark', e.target.value)}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-rose-500 focus:border-transparent"
                  placeholder="Nearby landmark (optional)"
                />
              </div>
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Special Instructions</label>
            <textarea
              rows={3}
              value={checkoutData.specialInstructions || ''}
              onChange={(e) => updateCheckoutData('specialInstructions', e.target.value)}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-rose-500 focus:border-transparent"
              placeholder="Any setup, serving, or menu instructions"
            />
          </div>

          <button
            onClick={onComplete}
            className="w-full px-6 py-3 bg-rose-600 text-white rounded-lg font-medium hover:bg-rose-700 transition"
          >
            Continue to Delivery Type
          </button>
        </div>
      )}
    </div>
  );
};

export default EventDetailsSection;
