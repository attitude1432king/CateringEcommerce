/*
========================================
File: src/components/owner/dashboard/HistoryPage.jsx (NEW FILE)
========================================
*/
import React from 'react';

export default function HistoryPage() {
    return (
        <div className="animate-fade-in space-y-6">
            <h1 className="text-3xl font-bold text-neutral-800">History & Reports</h1>
            <div className="bg-white p-6 rounded-lg shadow-md">
                <h3 className="font-semibold mb-4">Earning History</h3>
                {/* Placeholder for earnings chart */}
                <div className="h-64 bg-neutral-100 flex items-center justify-center text-neutral-500 rounded-md">Earnings Chart Placeholder</div>
            </div>
            <div className="bg-white p-6 rounded-lg shadow-md">
                <h3 className="font-semibold mb-4">Most Ordered Items</h3>
                {/* Placeholder for items list */}
                <p className="text-neutral-500">A list of your most popular items will be displayed here.</p>
            </div>
        </div>
    );
}