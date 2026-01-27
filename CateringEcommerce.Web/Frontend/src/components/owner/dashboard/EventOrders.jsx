/*
========================================
File: src/components/owner/dashboard/EventOrders.jsx
Modern Redesign - Event Order Management with Timeline
========================================
*/
import React, { useState } from 'react';

// Status Badge Component
const StatusBadge = ({ status }) => {
    const statusConfig = {
        upcoming: { bg: 'bg-blue-100', text: 'text-blue-800', border: 'border-blue-200', label: 'Upcoming' },
        'in-progress': { bg: 'bg-purple-100', text: 'text-purple-800', border: 'border-purple-200', label: 'In Progress' },
        completed: { bg: 'bg-green-100', text: 'text-green-800', border: 'border-green-200', label: 'Completed' },
        cancelled: { bg: 'bg-red-100', text: 'text-red-800', border: 'border-red-200', label: 'Cancelled' },
    };

    const config = statusConfig[status];
    return (
        <span className={`px-3 py-1 rounded-full text-xs font-semibold border ${config.bg} ${config.text} ${config.border}`}>
            {config.label}
        </span>
    );
};

// Event Order Card Component
const EventOrderCard = ({ order, onViewDetails, onManage }) => {
    const getProgressPercentage = () => {
        switch (order.status) {
            case 'upcoming': return 25;
            case 'in-progress': return 75;
            case 'completed': return 100;
            default: return 0;
        }
    };

    const progress = getProgressPercentage();

    return (
        <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 hover:shadow-md transition-shadow">
            {/* Header */}
            <div className="p-6 border-b border-neutral-100">
                <div className="flex items-start justify-between mb-3">
                    <div className="flex-1">
                        <div className="flex items-center gap-3 mb-2">
                            <h3 className="text-xl font-bold text-neutral-900">#{order.id}</h3>
                            <StatusBadge status={order.status} />
                        </div>
                        <p className="text-neutral-600 flex items-center gap-2 text-sm">
                            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                            </svg>
                            {order.customerName}
                        </p>
                    </div>
                    <div className="text-right">
                        <p className="text-2xl font-bold text-neutral-900">₹{order.totalAmount.toLocaleString()}</p>
                        <p className="text-xs text-neutral-500">Total Amount</p>
                    </div>
                </div>

                {/* Progress Bar */}
                {order.status !== 'cancelled' && (
                    <div className="mt-4">
                        <div className="flex items-center justify-between mb-2">
                            <span className="text-xs font-semibold text-neutral-600">Order Progress</span>
                            <span className="text-xs font-bold text-indigo-600">{progress}%</span>
                        </div>
                        <div className="w-full bg-neutral-200 rounded-full h-2">
                            <div
                                className="bg-gradient-to-r from-indigo-600 to-purple-600 h-2 rounded-full transition-all duration-300"
                                style={{ width: `${progress}%` }}
                            />
                        </div>
                    </div>
                )}
            </div>

            {/* Event Details */}
            <div className="p-6">
                <div className="grid grid-cols-2 gap-4 mb-4">
                    <div className="flex items-start gap-3">
                        <div className="p-2 bg-indigo-100 rounded-lg">
                            <svg className="w-5 h-5 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                            </svg>
                        </div>
                        <div>
                            <p className="text-xs text-neutral-500 font-medium mb-1">Event Date</p>
                            <p className="text-sm font-bold text-neutral-900">{order.eventDate}</p>
                        </div>
                    </div>

                    <div className="flex items-start gap-3">
                        <div className="p-2 bg-purple-100 rounded-lg">
                            <svg className="w-5 h-5 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                            </svg>
                        </div>
                        <div>
                            <p className="text-xs text-neutral-500 font-medium mb-1">Event Time</p>
                            <p className="text-sm font-bold text-neutral-900">{order.eventTime}</p>
                        </div>
                    </div>

                    <div className="flex items-start gap-3">
                        <div className="p-2 bg-green-100 rounded-lg">
                            <svg className="w-5 h-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                            </svg>
                        </div>
                        <div>
                            <p className="text-xs text-neutral-500 font-medium mb-1">Guests</p>
                            <p className="text-sm font-bold text-neutral-900">{order.guests} people</p>
                        </div>
                    </div>

                    <div className="flex items-start gap-3">
                        <div className="p-2 bg-orange-100 rounded-lg">
                            <svg className="w-5 h-5 text-orange-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                            </svg>
                        </div>
                        <div>
                            <p className="text-xs text-neutral-500 font-medium mb-1">Venue</p>
                            <p className="text-sm font-bold text-neutral-900">{order.venue}</p>
                        </div>
                    </div>
                </div>

                {/* Event Type */}
                <div className="mb-4 p-3 bg-neutral-50 rounded-xl">
                    <p className="text-xs text-neutral-500 font-medium mb-1">Event Type</p>
                    <p className="text-sm font-bold text-neutral-900">{order.eventType}</p>
                </div>

                {/* Menu Items */}
                <div className="mb-4">
                    <p className="text-xs text-neutral-500 font-semibold mb-2">Menu Items</p>
                    <div className="flex flex-wrap gap-2">
                        {order.menuItems.map((item, index) => (
                            <span key={index} className="px-3 py-1 bg-indigo-50 text-indigo-700 rounded-lg text-xs font-medium">
                                {item}
                            </span>
                        ))}
                    </div>
                </div>

                {/* Action Buttons */}
                <div className="flex gap-3">
                    <button
                        onClick={() => onViewDetails(order.id)}
                        className="flex-1 flex items-center justify-center gap-2 bg-gradient-to-r from-indigo-600 to-purple-600 hover:from-indigo-700 hover:to-purple-700 text-white px-4 py-3 rounded-xl font-semibold transition-all"
                    >
                        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                        </svg>
                        View Details
                    </button>
                    {order.status !== 'completed' && order.status !== 'cancelled' && (
                        <button
                            onClick={() => onManage(order.id)}
                            className="flex-1 flex items-center justify-center gap-2 bg-white hover:bg-neutral-50 text-neutral-700 border-2 border-neutral-200 px-4 py-3 rounded-xl font-semibold transition-all"
                        >
                            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                            </svg>
                            Manage
                        </button>
                    )}
                </div>
            </div>

            {/* Footer */}
            <div className="px-6 py-3 bg-neutral-50 rounded-b-2xl border-t border-neutral-100">
                <p className="text-xs text-neutral-500">
                    Confirmed on {order.confirmedDate}
                </p>
            </div>
        </div>
    );
};

export default function EventOrders() {
    const [activeTab, setActiveTab] = useState('all');
    const [searchQuery, setSearchQuery] = useState('');

    // Mock data - replace with real API data
    const orders = [
        {
            id: 'ORD2001',
            customerName: 'Amit & Priya Wedding',
            eventDate: 'Jan 25, 2026',
            eventTime: '6:00 PM - 11:00 PM',
            guests: 200,
            venue: 'Grand Ballroom, Hotel Taj',
            eventType: 'Wedding Reception',
            totalAmount: 150000,
            status: 'upcoming',
            menuItems: ['Paneer Tikka', 'Biryani', 'Dal Makhani', 'Gulab Jamun'],
            confirmedDate: 'Jan 10, 2026'
        },
        {
            id: 'ORD2002',
            customerName: 'TechCorp Annual Meet',
            eventDate: 'Jan 16, 2026',
            eventTime: '12:00 PM - 3:00 PM',
            guests: 150,
            venue: 'Conference Hall, IT Park',
            eventType: 'Corporate Event',
            totalAmount: 75000,
            status: 'in-progress',
            menuItems: ['Mix Veg', 'Chicken Curry', 'Rice', 'Salad'],
            confirmedDate: 'Jan 5, 2026'
        },
        {
            id: 'ORD2003',
            customerName: 'Rohan Birthday Party',
            eventDate: 'Jan 10, 2026',
            eventTime: '4:00 PM - 8:00 PM',
            guests: 50,
            venue: 'Home Party, Whitefield',
            eventType: 'Birthday Party',
            totalAmount: 25000,
            status: 'completed',
            menuItems: ['Pizza', 'Pasta', 'Sandwiches', 'Cake'],
            confirmedDate: 'Dec 28, 2025'
        },
    ];

    const handleViewDetails = (orderId) => {
        console.log('View order details:', orderId);
    };

    const handleManage = (orderId) => {
        console.log('Manage order:', orderId);
    };

    const tabs = [
        { id: 'all', label: 'All Orders', count: orders.length },
        { id: 'upcoming', label: 'Upcoming', count: orders.filter(o => o.status === 'upcoming').length },
        { id: 'in-progress', label: 'In Progress', count: orders.filter(o => o.status === 'in-progress').length },
        { id: 'completed', label: 'Completed', count: orders.filter(o => o.status === 'completed').length },
    ];

    const filteredOrders = orders.filter(order => {
        if (activeTab !== 'all' && order.status !== activeTab) return false;
        if (searchQuery && !order.customerName.toLowerCase().includes(searchQuery.toLowerCase()) &&
            !order.id.toLowerCase().includes(searchQuery.toLowerCase())) return false;
        return true;
    });

    return (
        <div className="min-h-screen bg-neutral-50">
            <div className="p-4 sm:p-6 lg:p-8 space-y-6">
                {/* Header */}
                <div className="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-4">
                    <div>
                        <h1 className="text-3xl font-bold text-neutral-900">Event Orders</h1>
                        <p className="text-neutral-600 mt-1">Track and manage your confirmed event bookings</p>
                    </div>

                    {/* Search Bar */}
                    <div className="w-full lg:w-96">
                        <div className="relative">
                            <input
                                type="text"
                                placeholder="Search by customer or order ID..."
                                value={searchQuery}
                                onChange={(e) => setSearchQuery(e.target.value)}
                                className="w-full pl-11 pr-4 py-3 bg-white border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                            />
                            <svg className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                            </svg>
                        </div>
                    </div>
                </div>

                {/* Tabs */}
                <div className="flex flex-wrap gap-3">
                    {tabs.map((tab) => (
                        <button
                            key={tab.id}
                            onClick={() => setActiveTab(tab.id)}
                            className={`px-4 py-2 rounded-xl font-semibold text-sm transition-all ${
                                activeTab === tab.id
                                    ? 'bg-indigo-600 text-white shadow-md'
                                    : 'bg-white text-neutral-600 hover:bg-neutral-50 border border-neutral-200'
                            }`}
                        >
                            {tab.label}
                            <span className={`ml-2 px-2 py-0.5 rounded-full text-xs font-bold ${
                                activeTab === tab.id ? 'bg-white/20' : 'bg-neutral-100'
                            }`}>
                                {tab.count}
                            </span>
                        </button>
                    ))}
                </div>

                {/* Orders Grid */}
                {filteredOrders.length > 0 ? (
                    <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
                        {filteredOrders.map((order) => (
                            <EventOrderCard
                                key={order.id}
                                order={order}
                                onViewDetails={handleViewDetails}
                                onManage={handleManage}
                            />
                        ))}
                    </div>
                ) : (
                    <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 p-12">
                        <div className="text-center">
                            <svg className="w-20 h-20 mx-auto mb-4 text-neutral-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                            </svg>
                            <h3 className="text-xl font-semibold text-neutral-900 mb-2">No Event Orders</h3>
                            <p className="text-neutral-600">
                                {searchQuery
                                    ? 'No orders match your search criteria.'
                                    : activeTab !== 'all'
                                    ? `No ${activeTab} orders at the moment.`
                                    : 'No confirmed events to display.'}
                            </p>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
}