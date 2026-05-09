/*
========================================
File: src/components/owner/dashboard/availability/DateStatusModal.jsx
Modern Redesign - ENYVORA Brand
========================================
Modal to set status for a specific date.
*/
import React, { useState, useEffect } from 'react';
import { AvailabilityStatus } from '../../../../utils/staticData';


export default function DateStatusModal({ isOpen, onClose, dateData, onSave }) {
    const [status, setStatus] = useState(AvailabilityStatus.OPEN);
    const [note, setNote] = useState('');

    useEffect(() => {
        if (isOpen && dateData) {
            setStatus(dateData.status || AvailabilityStatus.OPEN);
            setNote(dateData.note || '');
        }
    }, [isOpen, dateData]);

    const handleSubmit = (e) => {
        e.preventDefault();
        onSave(status, note);
    };

    if (!isOpen) return null;

    // Helper to format date nicely
    const formattedDate = dateData ? new Date(dateData.date).toLocaleDateString('en-US', {
        weekday: 'long',
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    }) : '';

    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex justify-center items-center z-50 p-4 animate-backdrop-fade">
            <div className="bg-white rounded-2xl shadow-2xl w-full max-w-md transform transition-all animate-modal-slide-up">
                {/* Header */}
                <div className="relative text-white px-6 py-5 rounded-t-2xl" style={{ background: 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)' }}>
                    <div className="flex items-center gap-3">
                        <div className="p-2 bg-white bg-opacity-20 rounded-lg">
                            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                            </svg>
                        </div>
                        <div>
                            <h3 className="text-xl font-bold">Set Date Availability</h3>
                            <p className="text-sm text-white/80 mt-0.5">{formattedDate}</p>
                        </div>
                    </div>

                    <button
                        onClick={onClose}
                        className="absolute top-4 right-4 text-white hover:bg-white hover:bg-opacity-20 rounded-lg p-2 transition-all"
                    >
                        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                        </svg>
                    </button>
                </div>

                {/* Form */}
                <form onSubmit={handleSubmit} className="p-6 space-y-6">
                    {/* Status Selection */}
                    <div>
                        <label className="block text-sm font-bold text-neutral-900 mb-3">
                            Select Status
                        </label>
                        <div className="space-y-3">
                            {/* Open */}
                            <label className={`flex items-start p-4 border-2 rounded-xl cursor-pointer transition-all transform hover:scale-105 hover:shadow-lg ${
                                status === AvailabilityStatus.OPEN
                                    ? 'bg-green-50 border-green-500 shadow-md animate-option-select'
                                    : 'border-neutral-200 hover:border-green-300 hover:bg-green-50'
                            }`}>
                                <input
                                    type="radio"
                                    name="status"
                                    value={AvailabilityStatus.OPEN}
                                    checked={status === AvailabilityStatus.OPEN}
                                    onChange={(e) => setStatus(parseInt(e.target.value))}
                                    className="mt-1 text-green-600 focus:ring-green-500 w-4 h-4"
                                />
                                <div className="ml-3 flex-1">
                                    <div className="flex items-center gap-2 mb-1">
                                        <svg className="w-5 h-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                                        </svg>
                                        <span className="font-bold text-neutral-900">Open for Bookings</span>
                                    </div>
                                    <span className="block text-xs text-neutral-600">
                                        Accepting new catering orders for this date
                                    </span>
                                </div>
                            </label>

                            {/* Fully Booked */}
                            <label className={`flex items-start p-4 border-2 rounded-xl cursor-pointer transition-all transform hover:scale-105 hover:shadow-lg ${
                                status === AvailabilityStatus.FULLY_BOOKED
                                    ? 'bg-orange-50 border-orange-500 shadow-md animate-option-select'
                                    : 'border-neutral-200 hover:border-orange-300 hover:bg-orange-50'
                            }`}>
                                <input
                                    type="radio"
                                    name="status"
                                    value={AvailabilityStatus.FULLY_BOOKED}
                                    checked={status === AvailabilityStatus.FULLY_BOOKED}
                                    onChange={(e) => setStatus(parseInt(e.target.value))}
                                    className="mt-1 text-orange-600 focus:ring-orange-500 w-4 h-4"
                                />
                                <div className="ml-3 flex-1">
                                    <div className="flex items-center gap-2 mb-1">
                                        <svg className="w-5 h-5 text-orange-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                                        </svg>
                                        <span className="font-bold text-neutral-900">Fully Booked</span>
                                    </div>
                                    <span className="block text-xs text-neutral-600">
                                        No new orders accepted, capacity reached
                                    </span>
                                </div>
                            </label>

                            {/* Closed */}
                            <label className={`flex items-start p-4 border-2 rounded-xl cursor-pointer transition-all ${
                                status === AvailabilityStatus.CLOSED
                                    ? 'bg-red-50 border-red-500 shadow-md'
                                    : 'border-neutral-200 hover:border-red-300 hover:bg-red-50'
                            }`}>
                                <input
                                    type="radio"
                                    name="status"
                                    value={AvailabilityStatus.CLOSED}
                                    checked={status === AvailabilityStatus.CLOSED}
                                    onChange={(e) => setStatus(parseInt(e.target.value))}
                                    className="mt-1 text-red-600 focus:ring-red-500 w-4 h-4"
                                />
                                <div className="ml-3 flex-1">
                                    <div className="flex items-center gap-2 mb-1">
                                        <svg className="w-5 h-5 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
                                        </svg>
                                        <span className="font-bold text-neutral-900">Closed</span>
                                    </div>
                                    <span className="block text-xs text-neutral-600">
                                        Not accepting any orders for this date
                                    </span>
                                </div>
                            </label>
                        </div>
                    </div>

                    {/* Note Field */}
                    <div>
                        <label className="block text-sm font-bold text-neutral-900 mb-2">
                            Add Note (Optional)
                        </label>
                        <div className="relative">
                            <textarea
                                value={note}
                                onChange={(e) => setNote(e.target.value)}
                                rows="3"
                                placeholder="e.g., Family Function, Maintenance, Staff Training..."
                                className="w-full px-4 py-3 border-2 border-neutral-200 rounded-xl text-sm focus:ring-2 focus:ring-orange-400 focus:border-orange-400 transition-all resize-none"
                            ></textarea>
                            <div className="absolute bottom-2 right-2 text-xs text-neutral-400">
                                {note.length}/100
                            </div>
                        </div>
                    </div>

                    {/* Action Buttons */}
                    <div className="flex gap-3 pt-2">
                        <button
                            type="button"
                            onClick={onClose}
                            className="flex-1 px-6 py-3 text-sm font-semibold text-neutral-700 bg-white border-2 border-neutral-300 rounded-xl hover:bg-neutral-50 transition-all"
                        >
                            Cancel
                        </button>
                        <button
                            type="submit"
                            className="flex-1 px-6 py-3 text-sm font-semibold text-white rounded-xl shadow-lg hover:shadow-xl transition-all"
                            style={{ background: 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)' }}
                        >
                            Save Status
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}
