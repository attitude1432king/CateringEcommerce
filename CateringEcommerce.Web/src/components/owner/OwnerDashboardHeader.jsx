/*
========================================
File: src/components/owner/OwnerDashboardHeader.jsx (REVISED)
========================================
The top header for the owner dashboard, containing the Availability toggle.
Fixed vertical alignment of logo and title.
*/
import React, { useState } from 'react';
import AvailabilityManagement from './dashboard/availability/AvailabilityManagement'; // Import your component
import { useAuth } from '../../contexts/AuthContext';
import OwnerNotifications from './OwnerNotifications';

// Availability Icon
const AvailabilityIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
        <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm1-12a1 1 0 10-2 0v4a1 1 0 00.293.707l2.828 2.829a1 1 0 101.415-1.415L11 9.586V6z" clipRule="evenodd" />
    </svg>
);

export default function OwnerDashboardHeader() {
    const { user } = useAuth();
    const [isAvailabilityOpen, setIsAvailabilityOpen] = useState(false);

    return (
        <header className="bg-white shadow-sm border-b border-gray-200 h-16 flex items-center justify-between py-10 px-6 z-20">
            {/* Left Section: Logo & Page Title aligned */}
            <div className="flex items-center gap-4">
                {/* Optional: Add Logo if not in sidebar, or just keep title aligned */}
                {/*<h2 className="text-xl font-bold text-gray-800 leading-none">Dashboard</h2>*/}
            </div>

            <div className="flex items-center gap-4">
                {/* Availability Trigger Button */}
                <button
                    onClick={() => setIsAvailabilityOpen(true)}
                    className="flex items-center gap-2 px-3 py-2 bg-neutral-100 hover:bg-neutral-200 text-neutral-700 rounded-lg text-sm font-medium transition-colors"
                    title="Manage Availability"
                >
                    <AvailabilityIcon />
                    <span>Availability</span>
                </button>

                {/* Notification Bell */}
                <OwnerNotifications />

                {/* User Info / Profile Link could go here */}
                <div className="flex items-center gap-2">
                    <img
                        src={user?.logoUrl || `https://ui-avatars.com/api/?name=${user?.name || 'User'}&background=random`}
                        alt="Profile"
                        className="h-8 w-8 rounded-full border border-gray-200"
                    />
                    <span className="text-sm font-medium text-gray-700 hidden md:block">{user?.name}</span>
                </div>
            </div>

            {/* Availability Modal */}
            {isAvailabilityOpen && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
                    <div className="bg-white rounded-2xl shadow-2xl w-full max-w-4xl max-h-[90vh] overflow-y-auto relative">
                        <button
                            onClick={() => setIsAvailabilityOpen(false)}
                            className="absolute top-4 right-4 p-2 text-gray-400 hover:text-gray-600 rounded-full hover:bg-gray-100 transition-colors z-10"
                        >
                            <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                            </svg>
                        </button>

                        {/* Render the AvailabilityManagement component inside the modal */}
                        <AvailabilityManagement />
                    </div>
                </div>
            )}
        </header>
    );
}