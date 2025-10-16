/*
========================================
File: src/components/owner/dashboard/EventOrders.jsx (NEW FILE)
========================================
*/
import React from 'react';

export default function EventOrders() {
    return (
        <div className="animate-fade-in space-y-6">
            <h1 className="text-3xl font-bold text-neutral-800">Event Orders</h1>
            <div className="text-center py-20 bg-white rounded-xl shadow-sm text-neutral-500">
                <p>No confirmed events to display.</p>
            </div>
        </div>
    );
}