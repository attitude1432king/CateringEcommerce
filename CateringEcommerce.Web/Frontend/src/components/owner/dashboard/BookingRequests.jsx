/*
========================================
File: src/components/owner/dashboard/BookingRequests.jsx (NEW FILE)
========================================
*/
import React from 'react';

export default function BookingRequests() {
    return (
        <div className="animate-fade-in space-y-6">
            <h1 className="text-3xl font-bold text-neutral-800">Booking Requests</h1>
            <div className="text-center py-20 bg-white rounded-xl shadow-sm text-neutral-500">
                <p>No new booking requests at the moment.</p>
            </div>
        </div>
    );
}