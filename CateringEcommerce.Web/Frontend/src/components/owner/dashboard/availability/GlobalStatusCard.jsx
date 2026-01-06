/*
========================================
File: src/components/owner/dashboard/availability/GlobalStatusCard.jsx
========================================
The "Master Switch" for the catering business.
*/
import React from 'react';
import ToggleSwitch from '../../../common/ToggleSwitch';

export default function GlobalStatusCard({ status, onStatusChange }) {
    const isOpen = status === 'OPEN';

    return (
        <div className="bg-white rounded-xl shadow-sm border border-neutral-200 overflow-hidden">
            <div className="p-6 flex flex-col md:flex-row justify-between items-center gap-4">
                <div className="flex items-start gap-4">
                    <div className={`p-3 rounded-full ${isOpen ? 'bg-green-100 text-green-600' : 'bg-red-100 text-red-600'}`}>
                        {isOpen ? (
                            <svg xmlns="http://www.w3.org/2000/svg" className="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
                        ) : (
                            <svg xmlns="http://www.w3.org/2000/svg" className="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636" /></svg>
                        )}
                    </div>
                    <div>
                        <h2 className="text-lg font-bold text-neutral-800">Global Catering Status</h2>
                        <p className="text-sm text-neutral-500 mt-1">
                            {isOpen
                                ? "Your business is currently visible and accepting bookings based on your calendar."
                                : "Your business is temporarily closed. Customers cannot place new orders."}
                        </p>
                    </div>
                </div>

                <div className="flex items-center gap-3 bg-neutral-50 px-4 py-3 rounded-lg border border-neutral-100">
                    <span className={`text-sm font-bold ${isOpen ? 'text-green-600' : 'text-neutral-500'}`}>OPEN</span>
                    <ToggleSwitch
                        label=""
                        enabled={!isOpen} // Invert logic for "Closed" toggle visual if preferred, or standard
                        setEnabled={(val) => onStatusChange(val ? 'CLOSED' : 'OPEN')}
                    />
                    <span className={`text-sm font-bold ${!isOpen ? 'text-red-600' : 'text-neutral-500'}`}>CLOSED</span>
                </div>
            </div>
        </div>
    );
}
