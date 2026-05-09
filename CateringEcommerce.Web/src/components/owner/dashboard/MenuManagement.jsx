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

export default function MenuManagement() {
    const [activeTab, setActiveTab] = useState(TABS.FOOD_ITEMS);

    return (
        <div>
            <div className="flex justify-end mb-6">
                <div className="portal-tabs">
                    <button
                        onClick={() => setActiveTab(TABS.PACKAGES)}
                        className={activeTab === TABS.PACKAGES ? 'is-active' : ''}
                    >
                        Packages
                    </button>
                    <button
                        onClick={() => setActiveTab(TABS.FOOD_ITEMS)}
                        className={activeTab === TABS.FOOD_ITEMS ? 'is-active' : ''}
                    >
                        Food Items
                    </button>
                </div>
            </div>
            <div>
                {activeTab === TABS.PACKAGES && <PackagesView />}
                {activeTab === TABS.FOOD_ITEMS && <FoodItemsView />}
            </div>
        </div>
    );
}