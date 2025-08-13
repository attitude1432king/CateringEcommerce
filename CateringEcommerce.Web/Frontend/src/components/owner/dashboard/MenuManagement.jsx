/*
========================================
File: src/components/owner/dashboard/MenuManagement.jsx (NEW FILE)
========================================
*/
import React from 'react';

export default function MenuManagement() {
    return (
        <div className="animate-fade-in space-y-6">
            <div className="flex items-center justify-between">
                <h1 className="text-3xl font-bold text-neutral-800">Menu Management</h1>
                <button className="bg-rose-600 text-white px-4 py-2 rounded-md font-medium hover:bg-rose-700">Add New Item</button>
            </div>
            <div className="bg-white p-6 rounded-lg shadow-md">
                <h3 className="font-semibold mb-4">Menu Categories</h3>
                {/* Placeholder for categories */}
                <p className="text-neutral-500">A list of your menu categories will be displayed here.</p>
            </div>
            <div className="bg-white p-6 rounded-lg shadow-md">
                <h3 className="font-semibold mb-4">Menu Items</h3>
                {/* Placeholder for items table */}
                <p className="text-neutral-500">A table of your menu items will be displayed here.</p>
            </div>
        </div>
    );
}
