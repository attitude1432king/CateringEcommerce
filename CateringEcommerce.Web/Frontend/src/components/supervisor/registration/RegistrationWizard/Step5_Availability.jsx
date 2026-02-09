/**
 * Step 5: Availability (Optional)
 * Set preferred working days - can be updated later
 */

import { useState } from 'react';
import PropTypes from 'prop-types';
import { Calendar, Clock } from 'lucide-react';

const Step5_Availability = ({ data, onNext, onBack }) => {
  const [availableDays, setAvailableDays] = useState(data?.availableDays || []);
  const [skipAvailability, setSkipAvailability] = useState(false);

  const daysOfWeek = [
    { value: 'MONDAY', label: 'Monday' },
    { value: 'TUESDAY', label: 'Tuesday' },
    { value: 'WEDNESDAY', label: 'Wednesday' },
    { value: 'THURSDAY', label: 'Thursday' },
    { value: 'FRIDAY', label: 'Friday' },
    { value: 'SATURDAY', label: 'Saturday' },
    { value: 'SUNDAY', label: 'Sunday' },
  ];

  const toggleDay = (day) => {
    if (availableDays.includes(day)) {
      setAvailableDays(availableDays.filter(d => d !== day));
    } else {
      setAvailableDays([...availableDays, day]);
    }
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    onNext({ availableDays: skipAvailability ? [] : availableDays });
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div className="text-center mb-6">
        <h2 className="text-2xl font-bold text-gray-900">Availability</h2>
        <p className="text-sm text-gray-600 mt-2">
          Select your preferred working days (optional)
        </p>
      </div>

      <div className="space-y-4">
        {/* Skip Availability Option */}
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
          <div className="flex items-start">
            <div className="flex items-center h-5">
              <input
                type="checkbox"
                checked={skipAvailability}
                onChange={(e) => setSkipped(e.target.checked)}
                className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
              />
            </div>
            <div className="ml-3">
              <label className="text-sm font-medium text-blue-900">
                Skip for now
              </label>
              <p className="text-xs text-blue-700 mt-1">
                You can set your availability later from your dashboard
              </p>
            </div>
          </div>
        </div>

        {!skipAvailability && (
          <>
            {/* Days Selection */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-3">
                <Calendar className="inline w-4 h-4 mr-1" />
                Select Available Days
              </label>
              <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
                {daysOfWeek.map((day) => (
                  <button
                    key={day.value}
                    type="button"
                    onClick={() => toggleDay(day.value)}
                    className={`
                      px-4 py-3 rounded-lg border-2 text-sm font-medium transition-colors
                      ${
                        availableDays.includes(day.value)
                          ? 'border-blue-500 bg-blue-50 text-blue-700'
                          : 'border-gray-300 bg-white text-gray-700 hover:border-gray-400'
                      }
                    `}
                  >
                    {day.label}
                  </button>
                ))}
              </div>
            </div>

            {/* Info */}
            <div className="flex items-start gap-2 text-sm text-gray-600">
              <Clock className="w-4 h-4 mt-0.5 flex-shrink-0" />
              <p>
                This helps us assign events that match your schedule. You can update your availability anytime.
              </p>
            </div>
          </>
        )}
      </div>

      {/* Actions */}
      <div className="flex gap-3 pt-4">
        <button
          type="button"
          onClick={onBack}
          className="flex-1 px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
        >
          Back
        </button>
        <button
          type="submit"
          className="flex-1 px-4 py-2 border border-transparent rounded-lg text-sm font-medium text-white bg-blue-600 hover:bg-blue-700"
        >
          Next Step
        </button>
      </div>
    </form>
  );
};

Step5_Availability.propTypes = {
  data: PropTypes.object,
  onNext: PropTypes.func.isRequired,
  onBack: PropTypes.func.isRequired,
};

export default Step5_Availability;
