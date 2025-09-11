/*
========================================
File: src/components/MyProfilePage.jsx (NEW FILE)
========================================
This is the main container for the user profile section.
*/
import React, { useState } from 'react';
import UserProfileSettings from '../components/user/UserProfileSettings';

// ... (Placeholder components remain the same)
const Activity = () => <div className="p-6 bg-white rounded-lg shadow-md">Activity Content Coming Soon...</div>;
const Reviews = () => <div className="p-6 bg-white rounded-lg shadow-md">Reviews Content Coming Soon...</div>;
const PaymentMethods = () => <div className="p-6 bg-white rounded-lg shadow-md">Payment Methods Content Coming Soon...</div>;
const ManageCards = () => <div className="p-6 bg-white rounded-lg shadow-md">Manage Cards Content Coming Soon...</div>;
const MyAddresses = () => <div className="p-6 bg-white rounded-lg shadow-md">My Addresses Content Coming Soon...</div>;
const BookingHistory = () => <div className="p-6 bg-white rounded-lg shadow-md">Booking History Content Coming Soon...</div>;


export default function MyProfilePage({ navigateTo }) {
    const [activeTab, setActiveTab] = useState('profile');

    const tabs = {
        profile: { label: 'Profile', component: <UserProfileSettings /> },
        activity: { label: 'Activity', component: <Activity /> },
        reviews: { label: 'Reviews', component: <Reviews /> },
        payments: { label: 'Payment Methods', component: <PaymentMethods /> },
        cards: { label: 'Manage Cards', component: <ManageCards /> },
        addresses: { label: 'My Addresses', component: <MyAddresses /> },
        history: { label: 'Booking History', component: <BookingHistory /> },
    };

    const renderContent = () => {
        return tabs[activeTab].component;
    };

    return (
        <div className="bg-amber-50 min-h-screen">
            <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-8">
                <button onClick={() => navigateTo('home')} className="text-sm text-rose-600 hover:underline mb-6">
                    &larr; Back to Home
                </button>
                <div className="flex flex-col md:flex-row gap-8">
                    <aside className="md:w-1/4">
                        <div className="p-4 bg-white rounded-lg shadow-md">
                            <h2 className="text-lg font-semibold text-neutral-800 mb-4">Settings</h2>
                            <nav className="space-y-1">
                                {Object.keys(tabs).map((tabKey) => (
                                    <button
                                        key={tabKey}
                                        onClick={() => setActiveTab(tabKey)}
                                        className={`w-full text-left px-3 py-2 text-sm rounded-md transition-colors ${activeTab === tabKey
                                                ? 'bg-rose-100 text-rose-700 font-semibold'
                                                : 'text-neutral-600 hover:bg-amber-100 hover:text-neutral-800'
                                            }`}
                                    >
                                        {tabs[tabKey].label}
                                    </button>
                                ))}
                            </nav>
                        </div>
                    </aside>
                    <main className="flex-1">
                        {renderContent()}
                    </main>
                </div>
            </div>
        </div>
    );
}