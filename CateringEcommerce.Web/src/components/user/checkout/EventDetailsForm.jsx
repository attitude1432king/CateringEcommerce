import React, { useState, useEffect } from 'react';
import { validateEventDetails } from '../../../utils/checkoutValidator';
import { useAppSettings } from '../../../contexts/AppSettingsContext';
import EventLocationMapPicker from './EventLocationMapPicker';

const EventDetailsForm = ({ formData, onUpdate, onNext, onBack }) => {
  const { getInt } = useAppSettings();
  const [errors, setErrors] = useState({});
  const [eventDate, setEventDate] = useState(formData.eventDate || '');
  const [eventTime, setEventTime] = useState(formData.eventTime || '');
  const [eventType, setEventType] = useState(formData.eventType || '');
  const [eventLocation, setEventLocation] = useState(formData.eventLocation || '');
  const [specialInstructions, setSpecialInstructions] = useState(formData.specialInstructions || '');

  // Calculate min date (tomorrow)
  const getMinDate = () => {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    return tomorrow.toISOString().split('T')[0];
  };

  // Calculate max date from settings
  const getMaxDate = () => {
    const maxDays = getInt('BUSINESS.MAX_ADVANCE_BOOKING_DAYS', 90);
    const maxDate = new Date();
    maxDate.setDate(maxDate.getDate() + maxDays);
    return maxDate.toISOString().split('T')[0];
  };

  const eventTypes = [
    'Wedding',
    'Corporate Event',
    'Birthday Party',
    'Anniversary',
    'Engagement',
    'Baby Shower',
    'Housewarming',
    'Festival Celebration',
    'Retirement Party',
    'Other'
  ];

  const timeSlots = [
    '09:00 AM',
    '10:00 AM',
    '11:00 AM',
    '12:00 PM',
    '01:00 PM',
    '02:00 PM',
    '03:00 PM',
    '04:00 PM',
    '05:00 PM',
    '06:00 PM',
    '07:00 PM',
    '08:00 PM',
    '09:00 PM'
  ];

  const handleNext = () => {
    const dataToValidate = {
      eventDate,
      eventTime,
      eventType,
      eventLocation,
      specialInstructions
    };

    const validation = validateEventDetails(dataToValidate, {
      minAdvanceBookingHours: getInt('BUSINESS.MIN_ADVANCE_BOOKING_HOURS', 24),
      maxAdvanceBookingDays: getInt('BUSINESS.MAX_ADVANCE_BOOKING_DAYS', 90),
    });

    if (!validation.isValid) {
      setErrors(validation.errors);
      return;
    }

    // Clear errors and update parent
    setErrors({});
    onUpdate(dataToValidate);
    onNext();
  };

  return (
    <div className="event-details-form">
      <h2 className="text-2xl font-bold mb-6">Event Details</h2>

      <div className="space-y-4">
        {/* Event Date */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Event Date <span className="text-red-500">*</span>
          </label>
          <input
            type="date"
            value={eventDate}
            onChange={(e) => setEventDate(e.target.value)}
            min={getMinDate()}
            max={getMaxDate()}
            className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-red-500 focus:border-transparent ${
              errors.eventDate ? 'border-red-500' : 'border-gray-300'
            }`}
          />
          {errors.eventDate && (
            <p className="mt-1 text-sm text-red-600">{errors.eventDate}</p>
          )}
          <p className="mt-1 text-xs text-gray-500">
            Event must be scheduled at least 24 hours in advance
          </p>
        </div>

        {/* Event Time */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Event Time <span className="text-red-500">*</span>
          </label>
          <select
            value={eventTime}
            onChange={(e) => setEventTime(e.target.value)}
            className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-red-500 focus:border-transparent ${
              errors.eventTime ? 'border-red-500' : 'border-gray-300'
            }`}
          >
            <option value="">Select event time</option>
            {timeSlots.map((time) => (
              <option key={time} value={time}>
                {time}
              </option>
            ))}
          </select>
          {errors.eventTime && (
            <p className="mt-1 text-sm text-red-600">{errors.eventTime}</p>
          )}
        </div>

        {/* Event Type */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Event Type <span className="text-red-500">*</span>
          </label>
          <select
            value={eventType}
            onChange={(e) => setEventType(e.target.value)}
            className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-red-500 focus:border-transparent ${
              errors.eventType ? 'border-red-500' : 'border-gray-300'
            }`}
          >
            <option value="">Select event type</option>
            {eventTypes.map((type) => (
              <option key={type} value={type}>
                {type}
              </option>
            ))}
          </select>
          {errors.eventType && (
            <p className="mt-1 text-sm text-red-600">{errors.eventType}</p>
          )}
        </div>

        {/* Event Location */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Event Location
          </label>
          <EventLocationMapPicker
            value={eventLocation}
            onChange={(locationData) => {
              // Store the full location object or just the address string
              setEventLocation(locationData?.address || locationData);
            }}
            error={errors.eventLocation}
          />
        </div>

        {/* Special Instructions */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Special Instructions (Optional)
          </label>
          <textarea
            value={specialInstructions}
            onChange={(e) => setSpecialInstructions(e.target.value)}
            placeholder="Any dietary requirements, allergies, or special requests..."
            rows={4}
            maxLength={1000}
            className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-red-500 focus:border-transparent ${
              errors.specialInstructions ? 'border-red-500' : 'border-gray-300'
            }`}
          />
          {errors.specialInstructions && (
            <p className="mt-1 text-sm text-red-600">{errors.specialInstructions}</p>
          )}
          <p className="mt-1 text-xs text-gray-500">
            {specialInstructions.length}/1000 characters
          </p>
        </div>
      </div>

      {/* Navigation Buttons */}
      <div className="flex justify-between mt-8">
        <button
          onClick={onBack}
          className="px-6 py-3 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors"
        >
          Back
        </button>
        <button
          onClick={handleNext}
          className="px-6 py-3 bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors"
        >
          Continue
        </button>
      </div>
    </div>
  );
};

export default EventDetailsForm;
