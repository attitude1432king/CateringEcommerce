/**
 * Step 5: Availability (REDESIGNED)
 * Set preferred working days - can be updated later
 */

import { useState } from 'react';
import PropTypes from 'prop-types';
import { Calendar, Clock, ArrowRight, ArrowLeft, Info } from 'lucide-react';

const Step5_Availability = ({ data, onNext, onBack }) => {
    const [availableDays, setAvailableDays] = useState(data?.availableDays || []);
    const [skipAvailability, setSkipAvailability] = useState(false);

    const daysOfWeek = [
        { value: 'MONDAY', label: 'Monday', short: 'Mon' },
        { value: 'TUESDAY', label: 'Tuesday', short: 'Tue' },
        { value: 'WEDNESDAY', label: 'Wednesday', short: 'Wed' },
        { value: 'THURSDAY', label: 'Thursday', short: 'Thu' },
        { value: 'FRIDAY', label: 'Friday', short: 'Fri' },
        { value: 'SATURDAY', label: 'Saturday', short: 'Sat' },
        { value: 'SUNDAY', label: 'Sunday', short: 'Sun' },
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
            {/* Section Header */}
            <div className="flex items-center gap-3 mb-6">
                <div className="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center">
                    <Calendar className="w-5 h-5 text-green-600" />
                </div>
                <div>
                    <h2 className="text-xl font-bold text-neutral-800">Availability</h2>
                    <p className="text-sm text-neutral-500">Select your preferred working days (optional)</p>
                </div>
            </div>

            <div className="space-y-5">
                {/* Skip Option */}
                <div className="bg-blue-50 border-l-4 border-blue-400 rounded-lg p-4">
                    <label className="flex items-center gap-3 cursor-pointer">
                        <input
                            type="checkbox"
                            checked={skipAvailability}
                            onChange={(e) => setSkipAvailability(e.target.checked)}
                            className="h-5 w-5 rounded border-2 border-blue-300 text-blue-600 focus:ring-blue-500"
                        />
                        <div>
                            <span className="text-sm font-semibold text-blue-900">Skip for now</span>
                            <p className="text-xs text-blue-700 mt-0.5">You can set your availability later from your dashboard</p>
                        </div>
                    </label>
                </div>

                {!skipAvailability && (
                    <>
                        {/* Days Selection */}
                        <div>
                            <label className="block text-sm font-semibold text-neutral-800 mb-3 flex items-center gap-2">
                                <Calendar className="w-4 h-4 text-neutral-500" />
                                Select Available Days
                            </label>
                            <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-3">
                                {daysOfWeek.map((day) => (
                                    <button
                                        key={day.value}
                                        type="button"
                                        onClick={() => toggleDay(day.value)}
                                        className={`px-4 py-3.5 rounded-xl border-2 text-sm font-semibold transition-all duration-200 ${
                                            availableDays.includes(day.value)
                                                ? 'border-rose-400 bg-rose-50 text-rose-700 shadow-sm'
                                                : 'border-neutral-200 bg-white text-neutral-600 hover:border-neutral-300 hover:bg-neutral-50'
                                        }`}
                                    >
                                        <span className="hidden sm:inline">{day.label}</span>
                                        <span className="sm:hidden">{day.short}</span>
                                    </button>
                                ))}
                            </div>
                        </div>

                        {/* Info */}
                        <div className="flex items-start gap-3 bg-neutral-50 rounded-xl p-4 border-2 border-neutral-100">
                            <Info className="w-5 h-5 text-neutral-500 mt-0.5 flex-shrink-0" />
                            <p className="text-sm text-neutral-600">
                                This helps us assign events that match your schedule. You can update your availability anytime from your profile.
                            </p>
                        </div>
                    </>
                )}
            </div>

            {/* Actions */}
            <div className="flex gap-4 pt-4">
                <button type="button" onClick={onBack} className="flex-1 px-6 py-3.5 bg-white text-neutral-700 rounded-xl font-semibold border-2 border-neutral-200 hover:border-neutral-300 hover:bg-neutral-50 transition-all duration-200 flex items-center justify-center gap-2">
                    <ArrowLeft className="w-4 h-4" /> Back
                </button>
                <button type="submit" className="flex-1 px-6 py-3.5 bg-gradient-to-r from-rose-600 to-rose-500 text-white rounded-xl font-semibold shadow-lg hover:shadow-xl transition-all duration-200 flex items-center justify-center gap-2">
                    Next Step <ArrowRight className="w-4 h-4" />
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
