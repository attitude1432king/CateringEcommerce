/*
========================================
File: src/components/owner/dashboard/Discounts.jsx (NEW FILE)
========================================
*/
import React from 'react';
import DiscountsView from './discounts/DiscountsView';

export default function Discounts() {
    return (
        <div className="p-4 sm:p-6 lg:p-8">
            <h1 className="text-3xl font-bold text-neutral-800 mb-6">Discounts</h1>
            <DiscountsView />
        </div>
    );
}