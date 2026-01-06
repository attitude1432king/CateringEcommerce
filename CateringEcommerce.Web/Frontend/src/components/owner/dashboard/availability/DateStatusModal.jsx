
/*
========================================
File: src/components/owner/dashboard/availability/DateStatusModal.jsx
========================================
Modal to set status for a specific date.
*/
import React, { useState, useEffect } from 'react';

export default function DateStatusModal({ isOpen, onClose, dateData, onSave }) {
    const [status, setStatus] = useState('OPEN');
    const [note, setNote] = useState('');

    useEffect(() => {
        if (isOpen && dateData) {
            setStatus(dateData.status || 'OPEN');
            setNote(dateData.note || '');
        }
    }, [isOpen, dateData]);

    const handleSubmit = (e) => {
        e.preventDefault();
        onSave(status, note);
    };

    if (!isOpen) return null;

    // Helper to format date nicely
    const formattedDate = dateData ? new Date(dateData.date).toLocaleDateString('en-US', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }) : '';

    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex justify-center items-center z-50 p-4">
            <div className="bg-white rounded-xl shadow-lg w-full max-w-md animate-fade-in">
                <div className="p-6 border-b">
                    <h3 className="text-xl font-bold text-neutral-800">Set Availability</h3>
                    <p className="text-sm text-neutral-500 mt-1">{formattedDate}</p>
                </div>

                <form onSubmit={handleSubmit} className="p-6 space-y-6">
                    <div>
                        <label className="block text-sm font-medium text-neutral-700 mb-3">Status</label>
                        <div className="space-y-3">
                            <label className={`flex items-center p-3 border rounded-lg cursor-pointer transition-colors ${status === 'OPEN' ? 'bg-green-50 border-green-500 ring-1 ring-green-500' : 'hover:bg-neutral-50'}`}>
                                <input type="radio" name="status" value="OPEN" checked={status === 'OPEN'} onChange={(e) => setStatus(e.target.value)} className="text-green-600 focus:ring-green-500 h-4 w-4" />
                                <div className="ml-3">
                                    <span className="block text-sm font-medium text-neutral-900">Open</span>
                                    <span className="block text-xs text-neutral-500">Accepting new bookings</span>
                                </div>
                            </label>

                            <label className={`flex items-center p-3 border rounded-lg cursor-pointer transition-colors ${status === 'FULLY_BOOKED' ? 'bg-orange-50 border-orange-500 ring-1 ring-orange-500' : 'hover:bg-neutral-50'}`}>
                                <input type="radio" name="status" value="FULLY_BOOKED" checked={status === 'FULLY_BOOKED'} onChange={(e) => setStatus(e.target.value)} className="text-orange-600 focus:ring-orange-500 h-4 w-4" />
                                <div className="ml-3">
                                    <span className="block text-sm font-medium text-neutral-900">Fully Booked</span>
                                    <span className="block text-xs text-neutral-500">No new orders, existing ones visible</span>
                                </div>
                            </label>

                            <label className={`flex items-center p-3 border rounded-lg cursor-pointer transition-colors ${status === 'CLOSED' ? 'bg-red-50 border-red-500 ring-1 ring-red-500' : 'hover:bg-neutral-50'}`}>
                                <input type="radio" name="status" value="CLOSED" checked={status === 'CLOSED'} onChange={(e) => setStatus(e.target.value)} className="text-red-600 focus:ring-red-500 h-4 w-4" />
                                <div className="ml-3">
                                    <span className="block text-sm font-medium text-neutral-900">Closed</span>
                                    <span className="block text-xs text-neutral-500">Not accepting any orders</span>
                                </div>
                            </label>
                        </div>
                    </div>

                    <div>
                        <label className="block text-sm font-medium text-neutral-700 mb-1">Note (Optional)</label>
                        <textarea
                            value={note}
                            onChange={(e) => setNote(e.target.value)}
                            rows="2"
                            placeholder="e.g., Family Function, renovation, etc."
                            className="w-full p-2 border border-neutral-300 rounded-md text-sm focus:ring-rose-500 focus:border-rose-500"
                        ></textarea>
                    </div>

                    <div className="flex justify-end gap-3 pt-2">
                        <button type="button" onClick={onClose} className="px-4 py-2 text-sm font-medium text-neutral-700 bg-white border border-neutral-300 rounded-md hover:bg-neutral-50">Cancel</button>
                        <button type="submit" className="px-4 py-2 text-sm font-medium text-white bg-rose-600 rounded-md hover:bg-rose-700">Save Status</button>
                    </div>
                </form>
            </div>
        </div>
    );
}