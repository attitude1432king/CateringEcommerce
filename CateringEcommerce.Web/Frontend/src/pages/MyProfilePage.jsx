/*
========================================
File: src/components/MyProfilePage.jsx (NEW FILE)
========================================
The main page container for the CLIENT's profile.
Fixed spacing, layout, and back button styling.
*/
import React, { useState } from 'react';
import UserProfileSettings from '../components/user/UserProfileSettings';
import { useNavigate } from 'react-router-dom';

// Placeholder components (kept for completeness)
const Activity = () => <div className="p-6 bg-white rounded-lg shadow-md min-h-[400px]">Activity Content Coming Soon...</div>;
const Reviews = () => <div className="p-6 bg-white rounded-lg shadow-md min-h-[400px]">Reviews Content Coming Soon...</div>;
const PaymentMethods = () => <div className="p-6 bg-white rounded-lg shadow-md min-h-[400px]">Payment Methods Content Coming Soon...</div>;
const ManageCards = () => <div className="p-6 bg-white rounded-lg shadow-md min-h-[400px]">Manage Cards Content Coming Soon...</div>;
const MyAddresses = () => <div className="p-6 bg-white rounded-lg shadow-md min-h-[400px]">My Addresses Content Coming Soon...</div>;
const BookingHistory = () => <div className="p-6 bg-white rounded-lg shadow-md min-h-[400px]">Booking History Content Coming Soon...</div>;

export default function MyProfilePage() {
    const [activeTab, setActiveTab] = useState('profile');
    const navigate = useNavigate();

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
        <div className="flex flex-col min-h-screen bg-neutral-50">
            {/* Header is handled by App.jsx layout, but included here conceptually if standalone */}

            <main className="flex-grow container mx-auto px-4 sm:px-6 lg:px-8 py-10">
                {/* Modern Back Button */}
                <div className="mb-8">
                    <button
                        onClick={() => navigate('/')} 
                        className="inline-flex items-center gap-2 px-4 py-2 bg-white border border-neutral-200 rounded-full text-sm font-medium text-neutral-600 hover:bg-neutral-50 hover:text-rose-600 transition-colors shadow-sm"
                    >
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 19l-7-7m0 0l7-7m-7 7h18" />
                        </svg>
                        Back to Home
                    </button>
                </div>

                <div className="flex flex-col lg:flex-row gap-8 align-start">
                    {/* Sidebar */}
                    <aside className="w-full lg:w-64 flex-shrink-0">
                        <div className="bg-white rounded-xl shadow-sm border border-neutral-100 overflow-hidden sticky top-24">
                            <div className="p-4 border-b border-neutral-100 bg-neutral-50">
                                <h2 className="text-lg font-bold text-neutral-800">Settings</h2>
                            </div>
                            <nav className="p-2 space-y-1">
                                {Object.keys(tabs).map((tabKey) => (
                                    <button
                                        key={tabKey}
                                        onClick={() => setActiveTab(tabKey)}
                                        className={`w-full text-left px-4 py-3 text-sm font-medium rounded-lg transition-all duration-200 flex items-center justify-between ${activeTab === tabKey
                                                ? 'bg-rose-50 text-rose-700 shadow-sm'
                                                : 'text-neutral-600 hover:bg-neutral-50 hover:text-neutral-900'
                                            }`}
                                    >
                                        {tabs[tabKey].label}
                                        {activeTab === tabKey && (
                                            <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                                                <path fillRule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clipRule="evenodd" />
                                            </svg>
                                        )}
                                    </button>
                                ))}
                            </nav>
                        </div>
                    </aside>

                    {/* Main Content Area */}
                    <section className="flex-1 min-w-0">
                        {/* Added min-height to prevent footer overlap on short content */}
                        <div className="min-h-[600px]">
                            {renderContent()}
                        </div>
                    </section>
                </div>
            </main>

            {/* Footer is handled by App.jsx layout */}
        </div>
    );
}