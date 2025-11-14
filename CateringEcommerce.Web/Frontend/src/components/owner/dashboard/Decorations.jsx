/*
========================================
File: src/components/owner/dashboard/Decorations.jsx (NEW FILE)
========================================
*/
import React from 'react';
import DecorationsView from '../../owner/dashboard/decorations/DecorationsView';

export default function Decorations() {
    return (
        <div className="p-4 sm:p-6 lg:p-8">
            <h1 className="text-3xl font-bold text-neutral-800 mb-6">Decorations</h1>
            <DecorationsView />
        </div>
    );
}