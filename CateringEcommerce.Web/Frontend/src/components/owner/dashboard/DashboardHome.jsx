/*
========================================
File: src/components/owner/dashboard/DashboardHome.jsx
Modern Redesign - Inspired by Swiggy/Zomato Partner Dashboard
========================================
*/
import React, { useState } from 'react';

// Icons
const TrendingUpIcon = () => (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6" />
    </svg>
);

const CurrencyIcon = () => (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
    </svg>
);

const ClockIcon = () => (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
    </svg>
);

const StarIcon = () => (
    <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
        <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" />
    </svg>
);

// Modern Stat Card Component
const StatCard = ({ title, value, change, icon, iconBg, iconColor }) => (
    <div className="bg-white rounded-2xl shadow-sm border border-neutral-100 p-6 hover:shadow-md transition-shadow">
        <div className="flex items-start justify-between">
            <div className="flex-1">
                <p className="text-sm font-medium text-neutral-500 mb-1">{title}</p>
                <h3 className="text-3xl font-bold text-neutral-900 mb-2">{value}</h3>
                {change && (
                    <div className="flex items-center gap-1 text-sm">
                        <span className={`font-semibold ${change >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                            {change >= 0 ? '+' : ''}{change}%
                        </span>
                        <span className="text-neutral-500">vs last week</span>
                    </div>
                )}
            </div>
            <div className={`p-3 rounded-xl ${iconBg}`}>
                <div className={iconColor}>
                    {icon}
                </div>
            </div>
        </div>
    </div>
);

// Quick Action Button
const ActionButton = ({ icon, label, variant = 'primary', onClick }) => {
    const variants = {
        primary: 'bg-gradient-to-r from-indigo-600 to-purple-600 hover:from-indigo-700 hover:to-purple-700 text-white shadow-md',
        success: 'bg-green-600 hover:bg-green-700 text-white',
        danger: 'bg-red-600 hover:bg-red-700 text-white',
        outline: 'bg-white border-2 border-neutral-200 hover:border-neutral-300 text-neutral-700'
    };

    return (
        <button
            onClick={onClick}
            className={`flex items-center justify-center gap-2 px-6 py-3 rounded-xl font-semibold transition-all ${variants[variant]}`}
        >
            {icon}
            <span>{label}</span>
        </button>
    );
};

// Order Card Component
const OrderCard = ({ orderNumber, customerName, eventDate, status, amount }) => {
    const statusColors = {
        pending: 'bg-yellow-100 text-yellow-800',
        confirmed: 'bg-green-100 text-green-800',
        completed: 'bg-blue-100 text-blue-800',
    };

    return (
        <div className="bg-neutral-50 rounded-xl p-4 hover:bg-neutral-100 transition-colors border border-neutral-200">
            <div className="flex items-center justify-between mb-3">
                <div>
                    <p className="font-semibold text-neutral-900">Order #{orderNumber}</p>
                    <p className="text-sm text-neutral-600">{customerName}</p>
                </div>
                <span className={`px-3 py-1 rounded-full text-xs font-semibold ${statusColors[status]}`}>
                    {status.charAt(0).toUpperCase() + status.slice(1)}
                </span>
            </div>
            <div className="flex items-center justify-between text-sm">
                <span className="text-neutral-600">{eventDate}</span>
                <span className="font-bold text-neutral-900">₹{amount.toLocaleString()}</span>
            </div>
        </div>
    );
};

export default function DashboardHome() {
    const [isOnline, setIsOnline] = useState(true);

    // Mock data - replace with real API data
    const stats = {
        todayOrders: { value: '12', change: 15 },
        todayRevenue: { value: '₹45,230', change: 8 },
        pendingRequests: { value: '5', change: -10 },
        rating: { value: '4.8', change: 2 }
    };

    const recentOrders = [
        { orderNumber: '1234', customerName: 'John Doe', eventDate: 'Jan 15, 2026', status: 'confirmed', amount: 12500 },
        { orderNumber: '1235', customerName: 'Jane Smith', eventDate: 'Jan 18, 2026', status: 'pending', amount: 8750 },
        { orderNumber: '1236', customerName: 'Mike Johnson', eventDate: 'Jan 20, 2026', status: 'confirmed', amount: 15000 },
    ];

    return (
        <div className="min-h-screen bg-neutral-50">
            <div className="p-4 sm:p-6 lg:p-8 space-y-6">
                {/* Header */}
                <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
                    <div>
                        <h1 className="text-3xl font-bold text-neutral-900">Dashboard</h1>
                        <p className="text-neutral-600 mt-1">Welcome back! Here's what's happening today.</p>
                    </div>

                    {/* Online/Offline Toggle */}
                    <div className="flex items-center gap-3 bg-white px-4 py-2 rounded-xl shadow-sm border border-neutral-200">
                        <span className="text-sm font-medium text-neutral-700">Status:</span>
                        <button
                            onClick={() => setIsOnline(!isOnline)}
                            className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                                isOnline ? 'bg-green-500' : 'bg-neutral-300'
                            }`}
                        >
                            <span
                                className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                                    isOnline ? 'translate-x-6' : 'translate-x-1'
                                }`}
                            />
                        </button>
                        <span className={`text-sm font-semibold ${isOnline ? 'text-green-600' : 'text-neutral-500'}`}>
                            {isOnline ? 'Online' : 'Offline'}
                        </span>
                    </div>
                </div>

                {/* Stats Grid */}
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
                    <StatCard
                        title="Today's Orders"
                        value={stats.todayOrders.value}
                        change={stats.todayOrders.change}
                        icon={<TrendingUpIcon />}
                        iconBg="bg-blue-100"
                        iconColor="text-blue-600"
                    />
                    <StatCard
                        title="Today's Revenue"
                        value={stats.todayRevenue.value}
                        change={stats.todayRevenue.change}
                        icon={<CurrencyIcon />}
                        iconBg="bg-green-100"
                        iconColor="text-green-600"
                    />
                    <StatCard
                        title="Pending Requests"
                        value={stats.pendingRequests.value}
                        change={stats.pendingRequests.change}
                        icon={<ClockIcon />}
                        iconBg="bg-orange-100"
                        iconColor="text-orange-600"
                    />
                    <StatCard
                        title="Overall Rating"
                        value={stats.rating.value}
                        change={stats.rating.change}
                        icon={<StarIcon />}
                        iconBg="bg-yellow-100"
                        iconColor="text-yellow-600"
                    />
                </div>

                {/* Quick Actions */}
                <div className="bg-white rounded-2xl shadow-sm border border-neutral-100 p-6">
                    <h2 className="text-lg font-bold text-neutral-900 mb-4">Quick Actions</h2>
                    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                        <ActionButton
                            icon={<svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" /></svg>}
                            label="Add Menu Item"
                            variant="primary"
                        />
                        <ActionButton
                            icon={<svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" /></svg>}
                            label="View Bookings"
                            variant="outline"
                        />
                        <ActionButton
                            icon={<svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z" /></svg>}
                            label="Create Discount"
                            variant="outline"
                        />
                        <ActionButton
                            icon={<svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>}
                            label="Set Availability"
                            variant="outline"
                        />
                    </div>
                </div>

                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                    {/* Recent Orders */}
                    <div className="bg-white rounded-2xl shadow-sm border border-neutral-100 p-6">
                        <div className="flex items-center justify-between mb-4">
                            <h2 className="text-lg font-bold text-neutral-900">Recent Orders</h2>
                            <button className="text-sm font-semibold text-indigo-600 hover:text-indigo-700">
                                View All →
                            </button>
                        </div>
                        <div className="space-y-3">
                            {recentOrders.length > 0 ? (
                                recentOrders.map((order) => (
                                    <OrderCard key={order.orderNumber} {...order} />
                                ))
                            ) : (
                                <div className="text-center py-8 text-neutral-500">
                                    <p>No recent orders</p>
                                </div>
                            )}
                        </div>
                    </div>

                    {/* Upcoming Events */}
                    <div className="bg-white rounded-2xl shadow-sm border border-neutral-100 p-6">
                        <div className="flex items-center justify-between mb-4">
                            <h2 className="text-lg font-bold text-neutral-900">Upcoming Events (7 Days)</h2>
                            <button className="text-sm font-semibold text-indigo-600 hover:text-indigo-700">
                                View All →
                            </button>
                        </div>
                        <div className="space-y-4">
                            <div className="text-center py-8 text-neutral-500">
                                <svg className="w-16 h-16 mx-auto mb-3 text-neutral-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                                </svg>
                                <p>No upcoming events in the next 7 days</p>
                            </div>
                        </div>
                    </div>
                </div>

                {/* Performance Insights */}
                <div className="bg-gradient-to-br from-indigo-600 to-purple-600 rounded-2xl shadow-lg p-6 text-white">
                    <div className="flex items-start justify-between">
                        <div>
                            <h2 className="text-xl font-bold mb-2">Performance Insights</h2>
                            <p className="text-indigo-100 mb-4">Your business is growing! Keep it up!</p>
                            <ul className="space-y-2 text-sm">
                                <li className="flex items-center gap-2">
                                    <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20"><path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" /></svg>
                                    <span>15% increase in orders this week</span>
                                </li>
                                <li className="flex items-center gap-2">
                                    <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20"><path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" /></svg>
                                    <span>4.8★ rating from customers</span>
                                </li>
                                <li className="flex items-center gap-2">
                                    <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20"><path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" /></svg>
                                    <span>95% on-time delivery rate</span>
                                </li>
                            </ul>
                        </div>
                        <div className="hidden sm:block">
                            <svg className="w-24 h-24 opacity-20" fill="currentColor" viewBox="0 0 24 24">
                                <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" />
                            </svg>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}
