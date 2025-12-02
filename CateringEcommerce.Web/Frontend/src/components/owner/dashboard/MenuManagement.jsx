/*
========================================
File: src/components/owner/dashboard/MenuManagement.jsx (REVISED)
========================================
*/
import React, { useState } from 'react';
import PackagesView from './menu/PackagesView';
import FoodItemsView from './menu/FoodItemsView';

const TABS = {
    PACKAGES: 'Packages',
    FOOD_ITEMS: 'Food Items',
};

// YELLOW BOX: Updated TabButton component for a modern, pill-style design
const TabButton = ({ label, isActive, onClick }) => (
    <button
        onClick={onClick}
        className={`px-4 py-2 text-sm font-semibold rounded-md transition-colors duration-200 ${isActive
                ? 'bg-rose-600 text-white shadow-md'
                : 'text-neutral-600 hover:bg-neutral-100'
            }`}
    >
        {label}
    </button>
);

export default function MenuManagement() {
    const [activeTab, setActiveTab] = useState(TABS.FOOD_ITEMS); // Default to food items

    return (
        <div className="p-4 sm:p-6 lg:p-8">
            <div className="flex flex-col md:flex-row justify-between md:items-center mb-6 border-b pb-4 gap-4">
                <div>
                    <h1 className="text-3xl font-bold text-neutral-800">Menu Management</h1>
                    {/* GREEN BOX: Added dynamic subtitle */}
                    <h2 className="text-lg text-neutral-500 mt-1">
                        {activeTab === TABS.PACKAGES ? "Manage Your Packages" : "Manage Your Menu Items"}
                    </h2>
                </div>
                <div className="flex items-center gap-2 p-1 bg-neutral-100 rounded-lg">
                    <TabButton
                        label={TABS.PACKAGES}
                        isActive={activeTab === TABS.PACKAGES}
                        onClick={() => setActiveTab(TABS.PACKAGES)}
                    />
                    <TabButton
                        label={TABS.FOOD_ITEMS}
                        isActive={activeTab === TABS.FOOD_ITEMS}
                        onClick={() => setActiveTab(TABS.FOOD_ITEMS)}
                    />
                </div>
            </div>

            <div>
                {activeTab === TABS.PACKAGES && <PackagesView />}
                {activeTab === TABS.FOOD_ITEMS && <FoodItemsView />}
            </div>
        </div>
    );
}