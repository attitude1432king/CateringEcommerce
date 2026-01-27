/*
========================================
File: src/components/owner/dashboard/BookingRequests.jsx
Modern Redesign - Booking Management System
========================================
*/
import React, { useState } from 'react';

// Filter Button Component
const FilterButton = ({ label, isActive, onClick, count }) => (
    <button
        onClick={onClick}
        className={`px-4 py-2 rounded-xl font-semibold text-sm transition-all ${
            isActive
                ? 'bg-indigo-600 text-white shadow-md'
                : 'bg-white text-neutral-600 hover:bg-neutral-50 border border-neutral-200'
        }`}
    >
        {label}
        {count > 0 && (
            <span className={`ml-2 px-2 py-0.5 rounded-full text-xs font-bold ${
                isActive ? 'bg-white/20' : 'bg-neutral-100'
            }`}>
                {count}
            </span>
        )}
    </button>
);

// Booking Request Card Component
const BookingCard = ({ booking, onAccept, onReject, onViewDetails }) => {
    const statusColors = {
        pending: 'bg-yellow-100 text-yellow-800 border-yellow-200',
        accepted: 'bg-green-100 text-green-800 border-green-200',
        rejected: 'bg-red-100 text-red-800 border-red-200',
    };

    return (
        <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 p-6 hover:shadow-md transition-shadow">
            {/* Header */}
            <div className="flex items-start justify-between mb-4">
                <div className="flex-1">
                    <div className="flex items-center gap-3 mb-2">
                        <h3 className="text-lg font-bold text-neutral-900">#{booking.id}</h3>
                        <span className={`px-3 py-1 rounded-full text-xs font-semibold border ${statusColors[booking.status]}`}>
                            {booking.status.charAt(0).toUpperCase() + booking.status.slice(1)}
                        </span>
                    </div>
                    <p className="text-neutral-600 flex items-center gap-2">
                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                        </svg>
                        {booking.customerName}
                    </p>
                </div>
                <div className="text-right">
                    <p className="text-2xl font-bold text-neutral-900">₹{booking.amount.toLocaleString()}</p>
                    <p className="text-xs text-neutral-500">Estimated</p>
                </div>
            </div>

            {/* Details Grid */}
            <div className="grid grid-cols-2 gap-4 mb-4 p-4 bg-neutral-50 rounded-xl">
                <div className="flex items-start gap-2">
                    <svg className="w-5 h-5 text-indigo-600 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                    </svg>
                    <div>
                        <p className="text-xs text-neutral-500 font-medium">Event Date</p>
                        <p className="text-sm font-semibold text-neutral-900">{booking.eventDate}</p>
                    </div>
                </div>
                <div className="flex items-start gap-2">
                    <svg className="w-5 h-5 text-indigo-600 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                    <div>
                        <p className="text-xs text-neutral-500 font-medium">Event Time</p>
                        <p className="text-sm font-semibold text-neutral-900">{booking.eventTime}</p>
                    </div>
                </div>
                <div className="flex items-start gap-2">
                    <svg className="w-5 h-5 text-indigo-600 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                    </svg>
                    <div>
                        <p className="text-xs text-neutral-500 font-medium">Guests</p>
                        <p className="text-sm font-semibold text-neutral-900">{booking.guests} people</p>
                    </div>
                </div>
                <div className="flex items-start gap-2">
                    <svg className="w-5 h-5 text-indigo-600 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
                    </svg>
                    <div>
                        <p className="text-xs text-neutral-500 font-medium">Event Type</p>
                        <p className="text-sm font-semibold text-neutral-900">{booking.eventType}</p>
                    </div>
                </div>
            </div>

            {/* Message */}
            {booking.message && (
                <div className="mb-4 p-3 bg-blue-50 rounded-xl border border-blue-100">
                    <p className="text-xs font-semibold text-blue-900 mb-1">Customer Message:</p>
                    <p className="text-sm text-blue-800">{booking.message}</p>
                </div>
            )}

            {/* Actions */}
            <div className="flex flex-wrap gap-3">
                {booking.status === 'pending' && (
                    <>
                        <button
                            onClick={() => onAccept(booking.id)}
                            className="flex-1 min-w-[120px] flex items-center justify-center gap-2 bg-green-600 hover:bg-green-700 text-white px-4 py-2.5 rounded-xl font-semibold transition-colors"
                        >
                            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                            </svg>
                            Accept
                        </button>
                        <button
                            onClick={() => onReject(booking.id)}
                            className="flex-1 min-w-[120px] flex items-center justify-center gap-2 bg-red-600 hover:bg-red-700 text-white px-4 py-2.5 rounded-xl font-semibold transition-colors"
                        >
                            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                            </svg>
                            Reject
                        </button>
                    </>
                )}
                <button
                    onClick={() => onViewDetails(booking.id)}
                    className="flex-1 min-w-[120px] flex items-center justify-center gap-2 bg-white hover:bg-neutral-50 text-neutral-700 border-2 border-neutral-200 px-4 py-2.5 rounded-xl font-semibold transition-colors"
                >
                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                    </svg>
                    View Details
                </button>
            </div>

            {/* Timestamp */}
            <div className="mt-3 pt-3 border-t border-neutral-100">
                <p className="text-xs text-neutral-500">
                    Requested on {booking.requestedDate}
                </p>
            </div>
        </div>
    );
};

export default function BookingRequests() {
    const [activeFilter, setActiveFilter] = useState('all');
    const [searchQuery, setSearchQuery] = useState('');

    // Mock data - replace with real API data
    const bookings = [
        {
            id: 'BK1001',
            customerName: 'Priya Sharma',
            eventDate: 'Jan 25, 2026',
            eventTime: '6:00 PM - 10:00 PM',
            guests: 150,
            eventType: 'Wedding',
            amount: 75000,
            status: 'pending',
            message: 'Looking for a traditional Indian wedding menu with vegetarian options.',
            requestedDate: 'Jan 12, 2026 at 3:45 PM'
        },
        {
            id: 'BK1002',
            customerName: 'Rahul Verma',
            eventDate: 'Jan 28, 2026',
            eventTime: '7:00 PM - 11:00 PM',
            guests: 100,
            eventType: 'Corporate Event',
            amount: 45000,
            status: 'pending',
            message: 'Need both veg and non-veg options for corporate dinner.',
            requestedDate: 'Jan 12, 2026 at 2:30 PM'
        },
        {
            id: 'BK1003',
            customerName: 'Anjali Gupta',
            eventDate: 'Jan 22, 2026',
            eventTime: '12:00 PM - 4:00 PM',
            guests: 75,
            eventType: 'Birthday Party',
            amount: 30000,
            status: 'accepted',
            message: 'Kids birthday party, need variety of snacks and desserts.',
            requestedDate: 'Jan 11, 2026 at 11:20 AM'
        },
    ];

    const handleAccept = (bookingId) => {
        console.log('Accept booking:', bookingId);
        // Implement accept logic
    };

    const handleReject = (bookingId) => {
        console.log('Reject booking:', bookingId);
        // Implement reject logic
    };

    const handleViewDetails = (bookingId) => {
        console.log('View details:', bookingId);
        // Implement view details logic
    };

    const filteredBookings = bookings.filter(booking => {
        if (activeFilter !== 'all' && booking.status !== activeFilter) return false;
        if (searchQuery && !booking.customerName.toLowerCase().includes(searchQuery.toLowerCase()) &&
            !booking.id.toLowerCase().includes(searchQuery.toLowerCase())) return false;
        return true;
    });

    const counts = {
        all: bookings.length,
        pending: bookings.filter(b => b.status === 'pending').length,
        accepted: bookings.filter(b => b.status === 'accepted').length,
        rejected: bookings.filter(b => b.status === 'rejected').length,
    };

    return (
        <div className="min-h-screen bg-neutral-50">
            <div className="p-4 sm:p-6 lg:p-8 space-y-6">
                {/* Header */}
                <div className="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-4">
                    <div>
                        <h1 className="text-3xl font-bold text-neutral-900">Booking Requests</h1>
                        <p className="text-neutral-600 mt-1">Manage your incoming booking requests</p>
                    </div>

                    {/* Search Bar */}
                    <div className="w-full lg:w-96">
                        <div className="relative">
                            <input
                                type="text"
                                placeholder="Search by customer or booking ID..."
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

                {/* Filters */}
                <div className="flex flex-wrap gap-3">
                    <FilterButton
                        label="All Requests"
                        isActive={activeFilter === 'all'}
                        onClick={() => setActiveFilter('all')}
                        count={counts.all}
                    />
                    <FilterButton
                        label="Pending"
                        isActive={activeFilter === 'pending'}
                        onClick={() => setActiveFilter('pending')}
                        count={counts.pending}
                    />
                    <FilterButton
                        label="Accepted"
                        isActive={activeFilter === 'accepted'}
                        onClick={() => setActiveFilter('accepted')}
                        count={counts.accepted}
                    />
                    <FilterButton
                        label="Rejected"
                        isActive={activeFilter === 'rejected'}
                        onClick={() => setActiveFilter('rejected')}
                        count={counts.rejected}
                    />
                </div>

                {/* Bookings Grid */}
                {filteredBookings.length > 0 ? (
                    <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
                        {filteredBookings.map((booking) => (
                            <BookingCard
                                key={booking.id}
                                booking={booking}
                                onAccept={handleAccept}
                                onReject={handleReject}
                                onViewDetails={handleViewDetails}
                            />
                        ))}
                    </div>
                ) : (
                    <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 p-12">
                        <div className="text-center">
                            <svg className="w-20 h-20 mx-auto mb-4 text-neutral-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                            </svg>
                            <h3 className="text-xl font-semibold text-neutral-900 mb-2">No Booking Requests</h3>
                            <p className="text-neutral-600">
                                {searchQuery
                                    ? 'No bookings match your search criteria.'
                                    : activeFilter !== 'all'
                                    ? `No ${activeFilter} booking requests at the moment.`
                                    : 'No new booking requests at the moment.'}
                            </p>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
}