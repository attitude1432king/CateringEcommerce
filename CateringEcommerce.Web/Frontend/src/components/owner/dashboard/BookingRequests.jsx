/*
========================================
File: src/components/owner/dashboard/BookingRequests.jsx
Booking Management System - Live API Integration
Supports both Event Orders & Sample Taste requests
========================================
*/
import { useState, useEffect, useCallback } from 'react';
import { ownerApiService } from '../../../services/ownerApi';

// ===================================
// Stat Card Component
// ===================================
const StatCard = ({ label, value, icon, color }) => {
    const colorMap = {
        blue: 'bg-blue-50 text-blue-700 border-blue-200',
        green: 'bg-green-50 text-green-700 border-green-200',
        amber: 'bg-amber-50 text-amber-700 border-amber-200',
        purple: 'bg-purple-50 text-purple-700 border-purple-200',
        red: 'bg-red-50 text-red-700 border-red-200',
        indigo: 'bg-indigo-50 text-indigo-700 border-indigo-200',
    };

    return (
        <div className={`rounded-xl border p-4 ${colorMap[color] || colorMap.blue}`}>
            <div className="flex items-center justify-between">
                <div>
                    <p className="text-xs font-medium opacity-75">{label}</p>
                    <p className="text-2xl font-bold mt-1">{value}</p>
                </div>
                <span className="text-2xl">{icon}</span>
            </div>
        </div>
    );
};

// ===================================
// Filter Button Component
// ===================================
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

// ===================================
// Booking Request Card Component
// ===================================
const BookingCard = ({ booking, onAccept, onReject, onViewDetails, isProcessing }) => {
    const statusColors = {
        Pending: 'bg-yellow-100 text-yellow-800 border-yellow-200',
        Confirmed: 'bg-green-100 text-green-800 border-green-200',
        Cancelled: 'bg-red-100 text-red-800 border-red-200',
        Completed: 'bg-blue-100 text-blue-800 border-blue-200',
        InProgress: 'bg-purple-100 text-purple-800 border-purple-200',
    };

    const formatDate = (dateStr) => {
        if (!dateStr) return '-';
        return new Date(dateStr).toLocaleDateString('en-IN', {
            day: 'numeric', month: 'short', year: 'numeric'
        });
    };

    const formatDateTime = (dateStr) => {
        if (!dateStr) return '-';
        return new Date(dateStr).toLocaleDateString('en-IN', {
            day: 'numeric', month: 'short', year: 'numeric',
            hour: '2-digit', minute: '2-digit'
        });
    };

    return (
        <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 p-6 hover:shadow-md transition-shadow">
            {/* Header */}
            <div className="flex items-start justify-between mb-4">
                <div className="flex-1">
                    <div className="flex items-center gap-3 mb-2">
                        <h3 className="text-lg font-bold text-neutral-900">#{booking.orderNumber}</h3>
                        <span className={`px-3 py-1 rounded-full text-xs font-semibold border ${statusColors[booking.orderStatus] || 'bg-gray-100 text-gray-800'}`}>
                            {booking.orderStatus}
                        </span>
                        {booking.daysUntilEvent <= 3 && booking.daysUntilEvent >= 0 && (
                            <span className="px-2 py-0.5 rounded-full text-xs font-bold bg-red-100 text-red-700 border border-red-200">
                                Urgent
                            </span>
                        )}
                    </div>
                    <p className="text-neutral-600 flex items-center gap-2">
                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                        </svg>
                        {booking.customerName}
                        {booking.customerPhone && (
                            <span className="text-neutral-400 text-sm">({booking.customerPhone})</span>
                        )}
                    </p>
                </div>
                <div className="text-right">
                    <p className="text-2xl font-bold text-neutral-900">₹{(booking.totalAmount || 0).toLocaleString('en-IN')}</p>
                    <p className="text-xs text-neutral-500">
                        {booking.paymentStatus || 'Pending'}
                    </p>
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
                        <p className="text-sm font-semibold text-neutral-900">{formatDate(booking.eventDate)}</p>
                    </div>
                </div>
                <div className="flex items-start gap-2">
                    <svg className="w-5 h-5 text-indigo-600 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                    <div>
                        <p className="text-xs text-neutral-500 font-medium">Days Until Event</p>
                        <p className={`text-sm font-semibold ${booking.daysUntilEvent <= 3 ? 'text-red-600' : 'text-neutral-900'}`}>
                            {booking.daysUntilEvent > 0 ? `${booking.daysUntilEvent} days` : booking.daysUntilEvent === 0 ? 'Today' : 'Past'}
                        </p>
                    </div>
                </div>
                <div className="flex items-start gap-2">
                    <svg className="w-5 h-5 text-indigo-600 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                    </svg>
                    <div>
                        <p className="text-xs text-neutral-500 font-medium">Guests</p>
                        <p className="text-sm font-semibold text-neutral-900">{booking.guestCount} people</p>
                    </div>
                </div>
                <div className="flex items-start gap-2">
                    <svg className="w-5 h-5 text-indigo-600 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
                    </svg>
                    <div>
                        <p className="text-xs text-neutral-500 font-medium">Event Type</p>
                        <p className="text-sm font-semibold text-neutral-900">{booking.eventType || '-'}</p>
                    </div>
                </div>
            </div>

            {/* Actions */}
            <div className="flex flex-wrap gap-3">
                {booking.orderStatus === 'Pending' && (
                    <>
                        <button
                            onClick={() => onAccept(booking.orderId)}
                            disabled={isProcessing}
                            className="flex-1 min-w-[120px] flex items-center justify-center gap-2 bg-green-600 hover:bg-green-700 text-white px-4 py-2.5 rounded-xl font-semibold transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                            </svg>
                            Accept
                        </button>
                        <button
                            onClick={() => onReject(booking.orderId)}
                            disabled={isProcessing}
                            className="flex-1 min-w-[120px] flex items-center justify-center gap-2 bg-red-600 hover:bg-red-700 text-white px-4 py-2.5 rounded-xl font-semibold transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                            </svg>
                            Reject
                        </button>
                    </>
                )}
                <button
                    onClick={() => onViewDetails(booking.orderId)}
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
                    Ordered on {formatDateTime(booking.orderDate)}
                </p>
            </div>
        </div>
    );
};

// ===================================
// Reject Reason Modal
// ===================================
const RejectModal = ({ isOpen, onClose, onConfirm, isProcessing }) => {
    const [reason, setReason] = useState('');

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-2xl max-w-md w-full p-6">
                <h2 className="text-xl font-bold text-neutral-900 mb-2">Reject Booking</h2>
                <p className="text-neutral-600 text-sm mb-4">
                    Please provide a reason for rejecting this booking request:
                </p>
                <textarea
                    value={reason}
                    onChange={(e) => setReason(e.target.value)}
                    placeholder="e.g., Fully booked on this date, Not in service area, etc."
                    rows={3}
                    className="w-full px-4 py-2 border border-neutral-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-transparent mb-4"
                />
                <div className="flex gap-3">
                    <button
                        onClick={onClose}
                        disabled={isProcessing}
                        className="flex-1 px-4 py-2.5 border-2 border-neutral-200 text-neutral-700 rounded-xl font-semibold hover:bg-neutral-50 disabled:opacity-50"
                    >
                        Cancel
                    </button>
                    <button
                        onClick={() => onConfirm(reason)}
                        disabled={isProcessing || !reason.trim()}
                        className="flex-1 px-4 py-2.5 bg-red-600 text-white rounded-xl font-semibold hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                        {isProcessing ? 'Rejecting...' : 'Reject Booking'}
                    </button>
                </div>
            </div>
        </div>
    );
};

// ===================================
// Main BookingRequests Component
// ===================================
export default function BookingRequests() {
    const [activeFilter, setActiveFilter] = useState('all');
    const [searchQuery, setSearchQuery] = useState('');
    const [bookings, setBookings] = useState([]);
    const [stats, setStats] = useState(null);
    const [isLoading, setIsLoading] = useState(true);
    const [isProcessing, setIsProcessing] = useState(false);
    const [error, setError] = useState(null);
    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [rejectModal, setRejectModal] = useState({ open: false, orderId: null });

    // Fetch booking requests from API
    const fetchBookings = useCallback(async () => {
        setIsLoading(true);
        setError(null);
        try {
            const filters = {};
            if (activeFilter === 'pending') filters.OrderStatus = 'Pending';
            else if (activeFilter === 'accepted') filters.OrderStatus = 'Confirmed';
            else if (activeFilter === 'rejected') filters.OrderStatus = 'Cancelled';
            if (searchQuery.trim()) filters.SearchTerm = searchQuery.trim();

            const response = await ownerApiService.getOrdersList(page, 20, filters);

            if (response.result && response.data) {
                setBookings(response.data.orders || []);
                setTotalPages(response.data.totalPages || 1);
            } else {
                setBookings([]);
                setError(response.message || 'Failed to load bookings');
            }
        } catch (err) {
            console.error('Error fetching bookings:', err);
            setError('An error occurred while loading bookings');
            setBookings([]);
        } finally {
            setIsLoading(false);
        }
    }, [activeFilter, searchQuery, page]);

    // Fetch stats
    const fetchStats = useCallback(async () => {
        try {
            const response = await ownerApiService.getBookingRequestStats();
            if (response.result && response.data) {
                setStats(response.data);
            }
        } catch (err) {
            console.error('Error fetching stats:', err);
        }
    }, []);

    useEffect(() => {
        fetchBookings();
    }, [fetchBookings]);

    useEffect(() => {
        fetchStats();
    }, [fetchStats]);

    // Accept booking (Pending → Confirmed)
    const handleAccept = async (orderId) => {
        if (!confirm('Are you sure you want to accept this booking?')) return;

        setIsProcessing(true);
        try {
            const response = await ownerApiService.updateOrderStatus(orderId, {
                NewStatus: 'Confirmed',
                Comments: 'Booking accepted by partner'
            });

            if (response.result) {
                fetchBookings();
                fetchStats();
            } else {
                alert(response.message || 'Failed to accept booking');
            }
        } catch (err) {
            console.error('Error accepting booking:', err);
            alert('An error occurred while accepting the booking');
        } finally {
            setIsProcessing(false);
        }
    };

    // Reject booking (Pending → Cancelled)
    const handleReject = (orderId) => {
        setRejectModal({ open: true, orderId });
    };

    const confirmReject = async (reason) => {
        setIsProcessing(true);
        try {
            const response = await ownerApiService.updateOrderStatus(rejectModal.orderId, {
                NewStatus: 'Cancelled',
                Comments: `Booking rejected by partner. Reason: ${reason}`
            });

            if (response.result) {
                setRejectModal({ open: false, orderId: null });
                fetchBookings();
                fetchStats();
            } else {
                alert(response.message || 'Failed to reject booking');
            }
        } catch (err) {
            console.error('Error rejecting booking:', err);
            alert('An error occurred while rejecting the booking');
        } finally {
            setIsProcessing(false);
        }
    };

    const handleViewDetails = (orderId) => {
        // Navigate to order detail — can be customized based on routing setup
        console.log('View details for order:', orderId);
    };

    // Calculate filter counts from stats
    const counts = {
        all: (stats?.totalPending || 0) + (stats?.totalConfirmed || 0) + (stats?.totalRejected || 0),
        pending: stats?.totalPending || 0,
        accepted: stats?.totalConfirmed || 0,
        rejected: stats?.totalRejected || 0,
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
                                placeholder="Search by customer or order number..."
                                value={searchQuery}
                                onChange={(e) => {
                                    setSearchQuery(e.target.value);
                                    setPage(1);
                                }}
                                className="w-full pl-11 pr-4 py-3 bg-white border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                            />
                            <svg className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                            </svg>
                        </div>
                    </div>
                </div>

                {/* Stats Cards — Today / This Week / This Month */}
                <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-4">
                    <StatCard label="Today" value={stats?.todayRequests ?? '-'} icon="📅" color="blue" />
                    <StatCard label="This Week" value={stats?.weekRequests ?? '-'} icon="📊" color="indigo" />
                    <StatCard label="This Month" value={stats?.monthRequests ?? '-'} icon="📈" color="purple" />
                    <StatCard label="Pending" value={stats?.totalPending ?? '-'} icon="⏳" color="amber" />
                    <StatCard label="Confirmed" value={stats?.totalConfirmed ?? '-'} icon="✅" color="green" />
                    <StatCard label="Rejected" value={stats?.totalRejected ?? '-'} icon="❌" color="red" />
                </div>

                {/* Filters */}
                <div className="flex flex-wrap gap-3">
                    <FilterButton
                        label="All Requests"
                        isActive={activeFilter === 'all'}
                        onClick={() => { setActiveFilter('all'); setPage(1); }}
                        count={counts.all}
                    />
                    <FilterButton
                        label="Pending"
                        isActive={activeFilter === 'pending'}
                        onClick={() => { setActiveFilter('pending'); setPage(1); }}
                        count={counts.pending}
                    />
                    <FilterButton
                        label="Accepted"
                        isActive={activeFilter === 'accepted'}
                        onClick={() => { setActiveFilter('accepted'); setPage(1); }}
                        count={counts.accepted}
                    />
                    <FilterButton
                        label="Rejected"
                        isActive={activeFilter === 'rejected'}
                        onClick={() => { setActiveFilter('rejected'); setPage(1); }}
                        count={counts.rejected}
                    />
                </div>

                {/* Loading State */}
                {isLoading && (
                    <div className="flex justify-center py-12">
                        <div className="text-center">
                            <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-indigo-600 mx-auto"></div>
                            <p className="mt-3 text-neutral-500 text-sm">Loading bookings...</p>
                        </div>
                    </div>
                )}

                {/* Error State */}
                {error && !isLoading && (
                    <div className="bg-red-50 border border-red-200 text-red-800 px-6 py-4 rounded-xl">
                        <p className="font-medium">{error}</p>
                        <button
                            onClick={fetchBookings}
                            className="mt-2 text-sm font-semibold text-red-700 underline hover:text-red-900"
                        >
                            Try again
                        </button>
                    </div>
                )}

                {/* Bookings Grid */}
                {!isLoading && !error && bookings.length > 0 && (
                    <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
                        {bookings.map((booking) => (
                            <BookingCard
                                key={booking.orderId}
                                booking={booking}
                                onAccept={handleAccept}
                                onReject={handleReject}
                                onViewDetails={handleViewDetails}
                                isProcessing={isProcessing}
                            />
                        ))}
                    </div>
                )}

                {/* Empty State */}
                {!isLoading && !error && bookings.length === 0 && (
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

                {/* Pagination */}
                {!isLoading && totalPages > 1 && (
                    <div className="flex justify-center gap-2 pt-4">
                        <button
                            onClick={() => setPage(p => Math.max(1, p - 1))}
                            disabled={page === 1}
                            className="px-4 py-2 rounded-xl border border-neutral-200 text-sm font-medium hover:bg-neutral-50 disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            Previous
                        </button>
                        <span className="px-4 py-2 text-sm text-neutral-600">
                            Page {page} of {totalPages}
                        </span>
                        <button
                            onClick={() => setPage(p => Math.min(totalPages, p + 1))}
                            disabled={page === totalPages}
                            className="px-4 py-2 rounded-xl border border-neutral-200 text-sm font-medium hover:bg-neutral-50 disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            Next
                        </button>
                    </div>
                )}
            </div>

            {/* Reject Reason Modal */}
            <RejectModal
                isOpen={rejectModal.open}
                onClose={() => setRejectModal({ open: false, orderId: null })}
                onConfirm={confirmReject}
                isProcessing={isProcessing}
            />
        </div>
    );
}
