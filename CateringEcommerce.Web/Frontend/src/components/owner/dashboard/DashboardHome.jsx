/*
========================================
File: src/components/owner/dashboard/DashboardHome.jsx (NEW FILE)
========================================
*/
import React from 'react';

const QuickActionButton = ({ children }) => (
    <button className="w-full bg-white text-rose-600 border border-rose-600 px-4 py-2 rounded-md font-medium hover:bg-rose-50 transition-colors">
        {children}
    </button>
);

export default function DashboardHome() {
    return (
        <div className="animate-fade-in p-4 sm:p-6 lg:p-8">
            <div>
                <h1 className="text-3xl font-bold text-neutral-800">Dashboard</h1>
                <p className="text-neutral-500 mt-1">Today's overview of your catering business.</p>
            </div>
            <div className="bg-white p-6 rounded-xl shadow-sm">
                <h3 className="font-semibold text-neutral-800 mb-4">Quick Actions</h3>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <button className="w-full bg-green-600 text-white px-4 py-3 rounded-md font-medium hover:bg-green-700">
                        Open for New Bookings
                    </button>
                    <button className="w-full bg-red-600 text-white px-4 py-3 rounded-md font-medium hover:bg-red-700">
                        Close Bookings
                    </button>
                </div>
            </div>
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                <div className="bg-white p-6 rounded-xl shadow-sm">
                    <h3 className="font-semibold text-neutral-800 mb-4">Upcoming Confirmed Orders</h3>
                    <div className="text-center py-8 text-neutral-500">
                        <p>No upcoming events in the next 7 days.</p>
                    </div>
                </div>
                <div className="bg-white p-6 rounded-xl shadow-sm">
                    <h3 className="font-semibold text-neutral-800 mb-4">Tastings Scheduled</h3>
                    <div className="text-center py-8 text-neutral-500">
                        <p>No tastings scheduled.</p>
                    </div>
                </div>
            </div>
        </div>
    );
}
