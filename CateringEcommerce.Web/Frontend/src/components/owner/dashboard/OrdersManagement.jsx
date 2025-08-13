/*
========================================
File: src/components/owner/dashboard/OrdersManagement.jsx (NEW FILE)
========================================
*/
import React, { useState } from 'react';

const tabs = ["New Orders", "Accepted", "In Progress", "Completed", "Cancelled"];

export default function OrdersManagement() {
    const [activeTab, setActiveTab] = useState(tabs[0]);

    return (
        <div className="animate-fade-in space-y-6">
            <h1 className="text-3xl font-bold text-neutral-800">Orders Management</h1>
            <div className="border-b border-neutral-200">
                <nav className="-mb-px flex space-x-6">
                    {tabs.map(tab => (
                        <button key={tab} onClick={() => setActiveTab(tab)} className={`whitespace-nowrap py-3 px-1 border-b-2 font-medium text-sm ${activeTab === tab ? 'border-rose-600 text-rose-600' : 'border-transparent text-neutral-500 hover:text-neutral-700 hover:border-neutral-300'
                            }`}>
                            {tab}
                        </button>
                    ))}
                </nav>
            </div>
            <div className="bg-white p-6 rounded-lg shadow-md">
                <h3 className="font-semibold mb-4">{activeTab}</h3>
                {/* Placeholder for order cards/table */}
                <p className="text-neutral-500">A list of orders will be displayed here based on the selected tab.</p>
            </div>
        </div>
    );
}