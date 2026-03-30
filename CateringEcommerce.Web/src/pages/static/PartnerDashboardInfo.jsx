import React from 'react';
import { Link } from 'react-router-dom';

export default function PartnerDashboardInfo() {
    return (
        <div className="min-h-screen bg-white">
            {/* Breadcrumb */}
            <div className="bg-gray-50 border-b">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-3 text-sm text-gray-500">
                    <Link to="/" className="hover:text-gray-700">Home</Link>
                    <span className="mx-2">/</span>
                    <span className="text-gray-700 font-medium">Partner Dashboard</span>
                </div>
            </div>

            {/* Hero */}
            <div className="bg-gradient-to-br from-blue-50 to-white py-16">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
                    <h1 className="text-4xl font-bold text-gray-900">Your Partner Dashboard</h1>
                    <p className="mt-4 text-lg text-gray-600 max-w-2xl mx-auto">
                        Everything you need to manage your catering business — orders, earnings, menu, and more — in one place.
                    </p>
                    <Link to="/partner-login" className="mt-8 inline-block px-8 py-4 bg-blue-600 text-white font-semibold rounded-xl hover:bg-blue-700 transition-colors text-lg">
                        Go to Dashboard
                    </Link>
                </div>
            </div>

            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
                <h2 className="text-2xl font-bold text-gray-900 text-center mb-10">Dashboard Features</h2>
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8 mb-16">
                    {[
                        { icon: '📋', title: 'Manage Orders', desc: 'View, accept, and track all booking requests. Stay on top of upcoming events and client requirements.' },
                        { icon: '💹', title: 'Track Earnings', desc: 'Real-time revenue reports, commission breakdowns, and payout history — transparent and easy to understand.' },
                        { icon: '🍽️', title: 'Handle Your Menu', desc: 'Add, edit, and update your food items, packages, and pricing at any time.' },
                        { icon: '📅', title: 'Manage Availability', desc: 'Set your calendar, block dates, and control how far in advance customers can book.' },
                        { icon: '👥', title: 'Staff & Supervisors', desc: 'Assign supervisors to events, manage your team, and track on-ground operations.' },
                        { icon: '⭐', title: 'Reviews & Ratings', desc: 'See customer feedback, respond to reviews, and build your reputation on the platform.' },
                    ].map((f, i) => (
                        <div key={i} className="bg-white border border-gray-100 rounded-2xl p-6 shadow-sm hover:shadow-md transition-shadow">
                            <div className="text-3xl mb-3">{f.icon}</div>
                            <h3 className="font-semibold text-gray-900 mb-2">{f.title}</h3>
                            <p className="text-gray-500 text-sm leading-relaxed">{f.desc}</p>
                        </div>
                    ))}
                </div>

                <div className="bg-blue-50 rounded-2xl p-10 text-center">
                    <h3 className="text-xl font-bold text-gray-900 mb-2">Already a partner?</h3>
                    <p className="text-gray-600 mb-6">Log in to your partner account to access your full dashboard.</p>
                    <Link to="/partner-login" className="inline-block px-8 py-3 bg-blue-600 text-white font-semibold rounded-xl hover:bg-blue-700 transition-colors">
                        Partner Login
                    </Link>
                </div>
            </div>
        </div>
    );
}
