/*
========================================
File: src/components/owner/dashboard/Reviews.jsx (NEW FILE)
========================================
*/
import React from 'react';

export default function Reviews() {
    return (
        <div className="animate-fade-in space-y-6">
            <h1 className="text-3xl font-bold text-neutral-800">Reviews & Feedback</h1>
            <div className="text-center py-20 bg-white rounded-xl shadow-sm text-neutral-500">
                <p>You haven't received any reviews yet.</p>
            </div>
        </div>
    );
}