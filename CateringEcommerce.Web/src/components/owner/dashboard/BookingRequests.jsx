/*
========================================
File: src/components/owner/dashboard/BookingRequests.jsx
Booking Management — Two-Tab: Event Order Requests + Sample Taste Requests
========================================
*/
import { useState, useEffect, useCallback } from 'react';
import { ownerApiService } from '../../../services/ownerApi';
import { useConfirmation } from '../../../contexts/ConfirmationContext';
import OwnerOrderDetailDrawer from './OwnerOrderDetailDrawer';

// ===================================
// Shared: Stat Card
// ===================================
const StatCard = ({ label, value, icon, color }) => {
    const colorMap = {
        blue:   'bg-blue-50 text-blue-700 border-blue-200',
        green:  'bg-green-50 text-green-700 border-green-200',
        amber:  'bg-amber-50 text-amber-700 border-amber-200',
        purple: 'bg-purple-50 text-purple-700 border-purple-200',
        red:    'bg-red-50 text-red-700 border-red-200',
        indigo: 'bg-indigo-50 text-indigo-700 border-indigo-200',
        teal:   'bg-teal-50 text-teal-700 border-teal-200',
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
// Shared: Filter Button
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
// Shared: Reject Modal
// ===================================
const RejectModal = ({ isOpen, onClose, onConfirm, isProcessing, title = 'Reject Request', placeholder }) => {
    const [reason, setReason] = useState('');

    // Reset reason each time the modal opens
    useEffect(() => {
        if (isOpen) setReason('');
    }, [isOpen]);

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-2xl max-w-md w-full p-6">
                <h2 className="text-xl font-bold text-neutral-900 mb-2">{title}</h2>
                <p className="text-neutral-600 text-sm mb-4">
                    Please provide a reason for rejecting this request:
                </p>
                <textarea
                    value={reason}
                    onChange={(e) => setReason(e.target.value)}
                    placeholder={placeholder || 'e.g., Fully booked on this date, Not in service area, etc.'}
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
                        {isProcessing ? 'Rejecting...' : 'Confirm Reject'}
                    </button>
                </div>
            </div>
        </div>
    );
};

// ===================================
// Event Tab: Booking Request Card
// ===================================
const BookingCard = ({ booking, onAccept, onReject, onViewDetails, isProcessing }) => {
    const statusColors = {
        Pending:    'bg-yellow-100 text-yellow-800 border-yellow-200',
        Confirmed:  'bg-green-100 text-green-800 border-green-200',
        Cancelled:  'bg-red-100 text-red-800 border-red-200',
        Completed:  'bg-blue-100 text-blue-800 border-blue-200',
        InProgress: 'bg-purple-100 text-purple-800 border-purple-200',
    };

    const fmt = (d) => d ? new Date(d).toLocaleDateString('en-IN', { day: 'numeric', month: 'short', year: 'numeric' }) : '-';
    const fmtDt = (d) => d ? new Date(d).toLocaleDateString('en-IN', { day: 'numeric', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' }) : '-';

    return (
        <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 p-6 hover:shadow-md transition-shadow">
            <div className="flex items-start justify-between mb-4">
                <div className="flex-1">
                    <div className="flex items-center gap-3 mb-2 flex-wrap">
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
                    <p className="text-xs text-neutral-500">{booking.paymentStatus || 'Pending'}</p>
                </div>
            </div>

            <div className="grid grid-cols-2 gap-4 mb-4 p-4 bg-neutral-50 rounded-xl">
                <div className="flex items-start gap-2">
                    <svg className="w-5 h-5 text-indigo-600 mt-0.5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                    </svg>
                    <div>
                        <p className="text-xs text-neutral-500 font-medium">Event Date</p>
                        <p className="text-sm font-semibold text-neutral-900">{fmt(booking.eventDate)}</p>
                    </div>
                </div>
                <div className="flex items-start gap-2">
                    <svg className="w-5 h-5 text-indigo-600 mt-0.5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
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
                    <svg className="w-5 h-5 text-indigo-600 mt-0.5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z" />
                    </svg>
                    <div>
                        <p className="text-xs text-neutral-500 font-medium">Guests</p>
                        <p className="text-sm font-semibold text-neutral-900">{booking.guestCount} people</p>
                    </div>
                </div>
                <div className="flex items-start gap-2">
                    <svg className="w-5 h-5 text-indigo-600 mt-0.5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
                    </svg>
                    <div>
                        <p className="text-xs text-neutral-500 font-medium">Event Type</p>
                        <p className="text-sm font-semibold text-neutral-900">{booking.eventType || '-'}</p>
                    </div>
                </div>
            </div>

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

            <div className="mt-3 pt-3 border-t border-neutral-100">
                <p className="text-xs text-neutral-500">Ordered on {fmtDt(booking.orderDate)}</p>
            </div>
        </div>
    );
};

// ===================================
// Sample Tab: Sample Request Card
// ===================================
const SampleRequestCard = ({ request, onAccept, onReject, isProcessing }) => {
    const statusColors = {
        SAMPLE_REQUESTED: 'bg-teal-100 text-teal-800 border-teal-200',
        SAMPLE_ACCEPTED:  'bg-green-100 text-green-800 border-green-200',
        SAMPLE_REJECTED:  'bg-red-100 text-red-800 border-red-200',
        SAMPLE_PREPARING: 'bg-blue-100 text-blue-800 border-blue-200',
        READY_FOR_PICKUP: 'bg-indigo-100 text-indigo-800 border-indigo-200',
        IN_TRANSIT:       'bg-purple-100 text-purple-800 border-purple-200',
        DELIVERED:        'bg-emerald-100 text-emerald-800 border-emerald-200',
    };

    const statusLabel = {
        SAMPLE_REQUESTED: 'Requested',
        SAMPLE_ACCEPTED:  'Accepted',
        SAMPLE_REJECTED:  'Rejected',
        SAMPLE_PREPARING: 'Preparing',
        READY_FOR_PICKUP: 'Ready for Pickup',
        IN_TRANSIT:       'In Transit',
        DELIVERED:        'Delivered',
    };

    const fmt = (d) => d ? new Date(d).toLocaleDateString('en-IN', { day: 'numeric', month: 'short', year: 'numeric' }) : '-';

    const isRequested = request.status === 'SAMPLE_REQUESTED';
    const sourceLabel = request.sourceType === 'event-order'
        ? `EVENT ${request.parentOrderNumber || ''}`.trim()
        : `SAMPLE #${request.sampleOrderId}`;

    return (
        <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 p-6 hover:shadow-md transition-shadow">
            {/* Header */}
            <div className="flex items-start justify-between mb-4">
                <div className="flex-1">
                    <div className="flex items-center gap-3 mb-2 flex-wrap">
                        <span className="text-xs font-bold text-neutral-400 bg-neutral-100 px-2 py-1 rounded-lg">
                            {sourceLabel}
                        </span>
                        <span className={`px-3 py-1 rounded-full text-xs font-semibold border ${statusColors[request.status] || 'bg-gray-100 text-gray-800'}`}>
                            {statusLabel[request.status] || request.status}
                        </span>
                    </div>
                    <p className="text-neutral-700 font-semibold flex items-center gap-2">
                        <svg className="w-4 h-4 text-neutral-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                        </svg>
                        {request.customerName}
                        {request.customerPhone && (
                            <span className="text-neutral-400 text-sm font-normal">({request.customerPhone})</span>
                        )}
                    </p>
                </div>
                <div className="text-right">
                    <p className="text-2xl font-bold text-neutral-900">₹{(request.totalAmount || 0).toLocaleString('en-IN')}</p>
                    <p className="text-xs text-neutral-500">{request.paymentStatus || 'Pending'}</p>
                </div>
            </div>

            {/* Sample Items */}
            {request.sampleItems?.length > 0 && (
                <div className="mb-4">
                    <p className="text-xs text-neutral-500 font-medium mb-2">Sample Items</p>
                    <div className="flex flex-wrap gap-2">
                        {request.sampleItems.map((item, idx) => (
                            <span key={idx} className="px-2.5 py-1 bg-teal-50 text-teal-700 border border-teal-100 rounded-lg text-xs font-medium">
                                {item}
                            </span>
                        ))}
                    </div>
                </div>
            )}

            {/* Details */}
            <div className="grid grid-cols-2 gap-4 mb-4 p-4 bg-neutral-50 rounded-xl">
                <div className="flex items-start gap-2">
                    <svg className="w-5 h-5 text-teal-600 mt-0.5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                    </svg>
                    <div>
                        <p className="text-xs text-neutral-500 font-medium">{request.sourceType === 'event-order' ? 'Event Address' : 'Pickup Address'}</p>
                        <p className="text-sm font-semibold text-neutral-900">{request.pickupAddress || '-'}</p>
                    </div>
                </div>
                <div className="flex items-start gap-2">
                    <svg className="w-5 h-5 text-teal-600 mt-0.5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                    </svg>
                    <div>
                        <p className="text-xs text-neutral-500 font-medium">Requested On</p>
                        <p className="text-sm font-semibold text-neutral-900">{fmt(request.requestedDate)}</p>
                    </div>
                </div>
                {(request.samplePriceTotal > 0 || request.deliveryCharge > 0) && (
                    <>
                        <div className="flex items-start gap-2">
                            <svg className="w-5 h-5 text-teal-600 mt-0.5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 14l6-6m-5.5.5h.01m4.99 5h.01M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16l3.5-2 3.5 2 3.5-2 3.5 2z" />
                            </svg>
                            <div>
                                <p className="text-xs text-neutral-500 font-medium">Sample Price</p>
                                <p className="text-sm font-semibold text-neutral-900">₹{(request.samplePriceTotal || 0).toLocaleString('en-IN')}</p>
                            </div>
                        </div>
                        <div className="flex items-start gap-2">
                            <svg className="w-5 h-5 text-teal-600 mt-0.5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7h12m0 0l-4-4m4 4l-4 4m0 6H4m0 0l4 4m-4-4l4-4" />
                            </svg>
                            <div>
                                <p className="text-xs text-neutral-500 font-medium">Delivery Charge</p>
                                <p className="text-sm font-semibold text-neutral-900">₹{(request.deliveryCharge || 0).toLocaleString('en-IN')}</p>
                            </div>
                        </div>
                    </>
                )}
            </div>

            {/* Rejection reason (read-only) */}
            {request.status === 'SAMPLE_REJECTED' && request.rejectionReason && (
                <div className="mb-4 p-3 bg-red-50 border border-red-100 rounded-xl">
                    <p className="text-xs text-red-600 font-medium">Rejection Reason</p>
                    <p className="text-sm text-red-800 mt-0.5">{request.rejectionReason}</p>
                </div>
            )}

            {/* Actions */}
            {isRequested && (
                <div className="flex flex-wrap gap-3">
                    <button
                        onClick={() => onAccept(request)}
                        disabled={isProcessing}
                        className="flex-1 min-w-[120px] flex items-center justify-center gap-2 bg-teal-600 hover:bg-teal-700 text-white px-4 py-2.5 rounded-xl font-semibold transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                        </svg>
                        Accept
                    </button>
                    <button
                        onClick={() => onReject(request)}
                        disabled={isProcessing}
                        className="flex-1 min-w-[120px] flex items-center justify-center gap-2 bg-red-600 hover:bg-red-700 text-white px-4 py-2.5 rounded-xl font-semibold transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                        </svg>
                        Reject
                    </button>
                </div>
            )}
        </div>
    );
};

// ===================================
// Main BookingRequests Component
// ===================================
export default function BookingRequests() {
    const confirm = useConfirmation();
    // ── Request type tab ─────────────────────────────
    const [requestType, setRequestType] = useState('events'); // 'events' | 'samples'

    // ── Shared ───────────────────────────────────────
    const [isProcessing, setIsProcessing] = useState(false);
    const [rejectModal, setRejectModal]   = useState({ open: false, id: null, payload: null });
    const [detailOrderId, setDetailOrderId] = useState(null);

    // ── Event tab state ──────────────────────────────
    const [eventFilter, setEventFilter]   = useState('all');
    const [eventSearch, setEventSearch]   = useState('');
    const [eventPage, setEventPage]       = useState(1);
    const [bookings, setBookings]         = useState([]);
    const [eventTotalPages, setEventTotalPages] = useState(1);
    const [eventStats, setEventStats]     = useState(null);
    const [eventLoading, setEventLoading] = useState(true);
    const [eventError, setEventError]     = useState(null);

    // ── Sample tab state ─────────────────────────────
    const [sampleFilter, setSampleFilter] = useState('all');
    const [sampleSearch, setSampleSearch] = useState('');
    const [samplePage, setSamplePage]     = useState(1);
    const [samples, setSamples]           = useState([]);
    const [sampleTotalPages, setSampleTotalPages] = useState(1);
    const [sampleStats, setSampleStats]   = useState({ requested: 0, accepted: 0, rejected: 0, total: 0 });
    const [sampleLoading, setSampleLoading] = useState(false);
    const [sampleError, setSampleError]   = useState(null);

    // ─────────────────────────────────────────────────
    // Event tab: fetch bookings
    // ─────────────────────────────────────────────────
    const fetchBookings = useCallback(async () => {
        setEventLoading(true);
        setEventError(null);
        try {
            const filters = {};
            if (eventFilter === 'pending')  filters.OrderStatus = 'Pending';
            else if (eventFilter === 'accepted') filters.OrderStatus = 'Confirmed';
            else if (eventFilter === 'rejected') filters.OrderStatus = 'Cancelled';
            if (eventSearch.trim()) filters.SearchTerm = eventSearch.trim();

            const response = await ownerApiService.getOrdersList(eventPage, 20, filters);
            if (response.result && response.data) {
                setBookings(response.data.orders || []);
                setEventTotalPages(response.data.totalPages || 1);
            } else {
                setBookings([]);
                setEventError(response.message || 'Failed to load bookings');
            }
        } catch (err) {
            console.error('Error fetching bookings:', err);
            setEventError('An error occurred while loading bookings');
            setBookings([]);
        } finally {
            setEventLoading(false);
        }
    }, [eventFilter, eventSearch, eventPage]);

    const fetchEventStats = useCallback(async () => {
        try {
            const response = await ownerApiService.getBookingRequestStats();
            if (response.result && response.data) setEventStats(response.data);
        } catch (err) {
            console.error('Error fetching event stats:', err);
        }
    }, []);

    // ─────────────────────────────────────────────────
    // Sample tab: fetch samples
    // ─────────────────────────────────────────────────
    const fetchSamples = useCallback(async () => {
        setSampleLoading(true);
        setSampleError(null);
        try {
            const filters = {};
            if (sampleFilter !== 'all') filters.StatusFilter = sampleFilter;
            if (sampleSearch.trim()) filters.SearchTerm = sampleSearch.trim();

            const response = await ownerApiService.getSampleRequestsList(samplePage, 20, filters);
            if (response.result && response.data) {
                setSamples(response.data.requests || []);
                setSampleTotalPages(response.data.totalPages || 1);
            } else {
                setSamples([]);
                setSampleError(response.message || 'Failed to load sample requests');
            }
        } catch (err) {
            console.error('Error fetching sample requests:', err);
            setSampleError('An error occurred while loading sample requests');
            setSamples([]);
        } finally {
            setSampleLoading(false);
        }
    }, [sampleFilter, sampleSearch, samplePage]);

    const fetchSampleStats = useCallback(async () => {
        try {
            const [all, req, acc, rej] = await Promise.all([
                ownerApiService.getSampleRequestsList(1, 1, {}),
                ownerApiService.getSampleRequestsList(1, 1, { StatusFilter: 'SAMPLE_REQUESTED' }),
                ownerApiService.getSampleRequestsList(1, 1, { StatusFilter: 'SAMPLE_ACCEPTED' }),
                ownerApiService.getSampleRequestsList(1, 1, { StatusFilter: 'SAMPLE_REJECTED' }),
            ]);
            setSampleStats({
                total:     all?.data?.totalCount  || 0,
                requested: req?.data?.totalCount  || 0,
                accepted:  acc?.data?.totalCount  || 0,
                rejected:  rej?.data?.totalCount  || 0,
            });
        } catch (err) {
            console.error('Error fetching sample stats:', err);
        }
    }, []);

    // ─────────────────────────────────────────────────
    // Effects
    // ─────────────────────────────────────────────────
    useEffect(() => {
        fetchBookings();
    }, [fetchBookings]);

    useEffect(() => {
        fetchEventStats();
    }, [fetchEventStats]);

    useEffect(() => {
        if (requestType === 'samples') fetchSamples();
    }, [requestType, fetchSamples]);

    useEffect(() => {
        if (requestType === 'samples') fetchSampleStats();
    }, [requestType, fetchSampleStats]);

    // ─────────────────────────────────────────────────
    // Event tab: accept / reject handlers
    // ─────────────────────────────────────────────────
    const handleAcceptEvent = async (orderId) => {
        const confirmed = await confirm({
            type: 'info',
            title: 'Accept Booking',
            message: 'Are you sure you want to accept this booking?',
            confirmText: 'Accept',
            cancelText: 'Cancel',
        });
        if (!confirmed) return;
        setIsProcessing(true);
        try {
            const response = await ownerApiService.updateOrderStatus(orderId, {
                NewStatus: 'Confirmed',
                Comments: 'Booking accepted by partner',
            });
            if (response.result) { fetchBookings(); fetchEventStats(); }
            else alert(response.message || 'Failed to accept booking');
        } catch {
            alert('An error occurred while accepting the booking');
        } finally {
            setIsProcessing(false);
        }
    };

    const handleRejectEvent = (orderId) => setRejectModal({ open: true, id: orderId });

    const confirmRejectEvent = async (reason) => {
        setIsProcessing(true);
        try {
            const response = await ownerApiService.updateOrderStatus(rejectModal.id, {
                NewStatus: 'Cancelled',
                Comments: `Booking rejected by partner. Reason: ${reason}`,
            });
            if (response.result) {
                setRejectModal({ open: false, id: null });
                fetchBookings();
                fetchEventStats();
            } else alert(response.message || 'Failed to reject booking');
        } catch {
            alert('An error occurred while rejecting the booking');
        } finally {
            setIsProcessing(false);
        }
    };

    // ─────────────────────────────────────────────────
    // Sample tab: accept / reject handlers
    // ─────────────────────────────────────────────────
    const handleAcceptSample = async (request) => {
        const confirmed = await confirm({
            type: 'info',
            title: 'Accept Sample Request',
            message: 'Are you sure you want to accept this sample request?',
            confirmText: 'Accept',
            cancelText: 'Cancel',
        });
        if (!confirmed) return;
        setIsProcessing(true);
        try {
            const response = await ownerApiService.actionSampleRequest(request.sampleOrderId, {
                Action: 'Accept',
                SourceType: request.sourceType,
                LinkedOrderId: request.linkedOrderId,
                LinkedOrderItemId: request.linkedOrderItemId
            });
            if (response.result) { fetchSamples(); fetchSampleStats(); }
            else alert(response.message || 'Failed to accept sample request');
        } catch {
            alert('An error occurred while accepting the sample request');
        } finally {
            setIsProcessing(false);
        }
    };

    const handleRejectSample = (request) => setRejectModal({ open: true, id: request.sampleOrderId, payload: request });

    const confirmRejectSample = async (reason) => {
        setIsProcessing(true);
        try {
            const response = await ownerApiService.actionSampleRequest(rejectModal.id, {
                Action: 'Reject',
                RejectionReason: reason,
                SourceType: rejectModal.payload?.sourceType,
                LinkedOrderId: rejectModal.payload?.linkedOrderId,
                LinkedOrderItemId: rejectModal.payload?.linkedOrderItemId,
            });
            if (response.result) {
                setRejectModal({ open: false, id: null, payload: null });
                fetchSamples();
                fetchSampleStats();
            } else alert(response.message || 'Failed to reject sample request');
        } catch {
            alert('An error occurred while rejecting the sample request');
        } finally {
            setIsProcessing(false);
        }
    };

    const handleViewEventDetails = (orderId) => {
        setDetailOrderId(orderId);
    };

    // ─────────────────────────────────────────────────
    // Derived counts
    // ─────────────────────────────────────────────────
    const eventCounts = {
        all:      (eventStats?.totalPending || 0) + (eventStats?.totalConfirmed || 0) + (eventStats?.totalRejected || 0),
        pending:  eventStats?.totalPending  || 0,
        accepted: eventStats?.totalConfirmed || 0,
        rejected: eventStats?.totalRejected  || 0,
    };

    const sampleCounts = {
        all:              sampleStats.total,
        SAMPLE_REQUESTED: sampleStats.requested,
        SAMPLE_ACCEPTED:  sampleStats.accepted,
        SAMPLE_REJECTED:  sampleStats.rejected,
    };

    return (
        <div className="min-h-screen bg-neutral-50">
            <div className="p-4 sm:p-6 lg:p-8 space-y-6">

                {/* ── Page Header ── */}
                <div className="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-4">
                    <div>
                        <h1 className="text-3xl font-bold text-neutral-900">Booking Requests</h1>
                        <p className="text-neutral-600 mt-1">Manage incoming event and sample taste requests</p>
                    </div>

                    {/* Search Bar */}
                    <div className="w-full lg:w-96">
                        <div className="relative">
                            <input
                                type="text"
                                placeholder={requestType === 'events' ? 'Search by customer or order number...' : 'Search by customer name...'}
                                value={requestType === 'events' ? eventSearch : sampleSearch}
                                onChange={(e) => {
                                    if (requestType === 'events') { setEventSearch(e.target.value); setEventPage(1); }
                                    else { setSampleSearch(e.target.value); setSamplePage(1); }
                                }}
                                className="w-full pl-11 pr-4 py-3 bg-white border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                            />
                            <svg className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                            </svg>
                        </div>
                    </div>
                </div>

                {/* ── Request Type Tabs ── */}
                <div className="flex gap-2 p-1 bg-neutral-100 rounded-2xl w-fit">
                    <button
                        onClick={() => setRequestType('events')}
                        className={`flex items-center gap-2 px-5 py-2.5 rounded-xl font-semibold text-sm transition-all ${
                            requestType === 'events'
                                ? 'bg-white text-indigo-700 shadow-sm'
                                : 'text-neutral-600 hover:text-neutral-900'
                        }`}
                    >
                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                        </svg>
                        Event Order Requests
                        {eventCounts.pending > 0 && (
                            <span className="px-2 py-0.5 rounded-full text-xs font-bold bg-amber-100 text-amber-700">
                                {eventCounts.pending}
                            </span>
                        )}
                    </button>
                    <button
                        onClick={() => setRequestType('samples')}
                        className={`flex items-center gap-2 px-5 py-2.5 rounded-xl font-semibold text-sm transition-all ${
                            requestType === 'samples'
                                ? 'bg-white text-teal-700 shadow-sm'
                                : 'text-neutral-600 hover:text-neutral-900'
                        }`}
                    >
                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z" />
                        </svg>
                        Sample Taste Requests
                        {sampleStats.requested > 0 && (
                            <span className="px-2 py-0.5 rounded-full text-xs font-bold bg-teal-100 text-teal-700">
                                {sampleStats.requested}
                            </span>
                        )}
                    </button>
                </div>

                {/* ══════════════════════════════════════════════
                    EVENT ORDERS TAB
                ══════════════════════════════════════════════ */}
                {requestType === 'events' && (
                    <>
                        {/* Stats */}
                        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-4">
                            <StatCard label="Today"      value={eventStats?.todayRequests  ?? '-'} icon="📅" color="blue"   />
                            <StatCard label="This Week"  value={eventStats?.weekRequests   ?? '-'} icon="📊" color="indigo" />
                            <StatCard label="This Month" value={eventStats?.monthRequests  ?? '-'} icon="📈" color="purple" />
                            <StatCard label="Pending"    value={eventStats?.totalPending   ?? '-'} icon="⏳" color="amber"  />
                            <StatCard label="Confirmed"  value={eventStats?.totalConfirmed ?? '-'} icon="✅" color="green"  />
                            <StatCard label="Rejected"   value={eventStats?.totalRejected  ?? '-'} icon="❌" color="red"    />
                        </div>

                        {/* Status Filters */}
                        <div className="flex flex-wrap gap-3">
                            {[
                                { key: 'all',      label: 'All Requests' },
                                { key: 'pending',  label: 'Pending'      },
                                { key: 'accepted', label: 'Accepted'     },
                                { key: 'rejected', label: 'Rejected'     },
                            ].map(({ key, label }) => (
                                <FilterButton
                                    key={key}
                                    label={label}
                                    isActive={eventFilter === key}
                                    onClick={() => { setEventFilter(key); setEventPage(1); }}
                                    count={eventCounts[key]}
                                />
                            ))}
                        </div>

                        {/* Loading */}
                        {eventLoading && (
                            <div className="flex justify-center py-12">
                                <div className="text-center">
                                    <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-indigo-600 mx-auto" />
                                    <p className="mt-3 text-neutral-500 text-sm">Loading bookings...</p>
                                </div>
                            </div>
                        )}

                        {/* Error */}
                        {eventError && !eventLoading && (
                            <div className="bg-red-50 border border-red-200 text-red-800 px-6 py-4 rounded-xl">
                                <p className="font-medium">{eventError}</p>
                                <button onClick={fetchBookings} className="mt-2 text-sm font-semibold text-red-700 underline hover:text-red-900">
                                    Try again
                                </button>
                            </div>
                        )}

                        {/* Cards */}
                        {!eventLoading && !eventError && bookings.length > 0 && (
                            <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
                                {bookings.map((booking) => (
                                    <BookingCard
                                        key={booking.orderId}
                                        booking={booking}
                                        onAccept={handleAcceptEvent}
                                        onReject={handleRejectEvent}
                                        onViewDetails={handleViewEventDetails}
                                        isProcessing={isProcessing}
                                    />
                                ))}
                            </div>
                        )}

                        {/* Empty */}
                        {!eventLoading && !eventError && bookings.length === 0 && (
                            <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 p-12">
                                <div className="text-center">
                                    <svg className="w-20 h-20 mx-auto mb-4 text-neutral-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                                    </svg>
                                    <h3 className="text-xl font-semibold text-neutral-900 mb-2">No Event Booking Requests</h3>
                                    <p className="text-neutral-600">
                                        {eventSearch ? 'No bookings match your search criteria.'
                                            : eventFilter !== 'all' ? `No ${eventFilter} booking requests at the moment.`
                                            : 'No new booking requests at the moment.'}
                                    </p>
                                </div>
                            </div>
                        )}

                        {/* Pagination */}
                        {!eventLoading && eventTotalPages > 1 && (
                            <div className="flex justify-center gap-2 pt-4">
                                <button onClick={() => setEventPage(p => Math.max(1, p - 1))} disabled={eventPage === 1}
                                    className="px-4 py-2 rounded-xl border border-neutral-200 text-sm font-medium hover:bg-neutral-50 disabled:opacity-50 disabled:cursor-not-allowed">
                                    Previous
                                </button>
                                <span className="px-4 py-2 text-sm text-neutral-600">Page {eventPage} of {eventTotalPages}</span>
                                <button onClick={() => setEventPage(p => Math.min(eventTotalPages, p + 1))} disabled={eventPage === eventTotalPages}
                                    className="px-4 py-2 rounded-xl border border-neutral-200 text-sm font-medium hover:bg-neutral-50 disabled:opacity-50 disabled:cursor-not-allowed">
                                    Next
                                </button>
                            </div>
                        )}
                    </>
                )}

                {/* ══════════════════════════════════════════════
                    SAMPLE TASTE REQUESTS TAB
                ══════════════════════════════════════════════ */}
                {requestType === 'samples' && (
                    <>
                        {/* Stats */}
                        <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
                            <StatCard label="Total"     value={sampleStats.total}     icon="🧪" color="teal"   />
                            <StatCard label="Requested" value={sampleStats.requested} icon="⏳" color="amber"  />
                            <StatCard label="Accepted"  value={sampleStats.accepted}  icon="✅" color="green"  />
                            <StatCard label="Rejected"  value={sampleStats.rejected}  icon="❌" color="red"    />
                        </div>

                        {/* Status Filters */}
                        <div className="flex flex-wrap gap-3">
                            {[
                                { key: 'all',              label: 'All'       },
                                { key: 'SAMPLE_REQUESTED', label: 'Requested' },
                                { key: 'SAMPLE_ACCEPTED',  label: 'Accepted'  },
                                { key: 'SAMPLE_REJECTED',  label: 'Rejected'  },
                            ].map(({ key, label }) => (
                                <FilterButton
                                    key={key}
                                    label={label}
                                    isActive={sampleFilter === key}
                                    onClick={() => { setSampleFilter(key); setSamplePage(1); }}
                                    count={sampleCounts[key]}
                                />
                            ))}
                        </div>

                        {/* Loading */}
                        {sampleLoading && (
                            <div className="flex justify-center py-12">
                                <div className="text-center">
                                    <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-teal-600 mx-auto" />
                                    <p className="mt-3 text-neutral-500 text-sm">Loading sample requests...</p>
                                </div>
                            </div>
                        )}

                        {/* Error */}
                        {sampleError && !sampleLoading && (
                            <div className="bg-red-50 border border-red-200 text-red-800 px-6 py-4 rounded-xl">
                                <p className="font-medium">{sampleError}</p>
                                <button onClick={fetchSamples} className="mt-2 text-sm font-semibold text-red-700 underline hover:text-red-900">
                                    Try again
                                </button>
                            </div>
                        )}

                        {/* Cards */}
                        {!sampleLoading && !sampleError && samples.length > 0 && (
                            <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
                                {samples.map((request) => (
                                    <SampleRequestCard
                                        key={request.sampleOrderId}
                                        request={request}
                                        onAccept={handleAcceptSample}
                                        onReject={handleRejectSample}
                                        isProcessing={isProcessing}
                                    />
                                ))}
                            </div>
                        )}

                        {/* Empty */}
                        {!sampleLoading && !sampleError && samples.length === 0 && (
                            <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 p-12">
                                <div className="text-center">
                                    <svg className="w-20 h-20 mx-auto mb-4 text-neutral-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z" />
                                    </svg>
                                    <h3 className="text-xl font-semibold text-neutral-900 mb-2">No Sample Taste Requests</h3>
                                    <p className="text-neutral-600">
                                        {sampleSearch ? 'No sample requests match your search.'
                                            : sampleFilter !== 'all' ? `No ${sampleFilter.replace('SAMPLE_', '').toLowerCase()} sample requests at the moment.`
                                            : 'No sample taste requests at the moment.'}
                                    </p>
                                </div>
                            </div>
                        )}

                        {/* Pagination */}
                        {!sampleLoading && sampleTotalPages > 1 && (
                            <div className="flex justify-center gap-2 pt-4">
                                <button onClick={() => setSamplePage(p => Math.max(1, p - 1))} disabled={samplePage === 1}
                                    className="px-4 py-2 rounded-xl border border-neutral-200 text-sm font-medium hover:bg-neutral-50 disabled:opacity-50 disabled:cursor-not-allowed">
                                    Previous
                                </button>
                                <span className="px-4 py-2 text-sm text-neutral-600">Page {samplePage} of {sampleTotalPages}</span>
                                <button onClick={() => setSamplePage(p => Math.min(sampleTotalPages, p + 1))} disabled={samplePage === sampleTotalPages}
                                    className="px-4 py-2 rounded-xl border border-neutral-200 text-sm font-medium hover:bg-neutral-50 disabled:opacity-50 disabled:cursor-not-allowed">
                                    Next
                                </button>
                            </div>
                        )}
                    </>
                )}
            </div>

            {/* ── Reject Modal (shared for both tabs) ── */}
            <RejectModal
                isOpen={rejectModal.open}
                onClose={() => setRejectModal({ open: false, id: null, payload: null })}
                onConfirm={requestType === 'events' ? confirmRejectEvent : confirmRejectSample}
                isProcessing={isProcessing}
                title={requestType === 'events' ? 'Reject Booking' : 'Reject Sample Request'}
                placeholder={requestType === 'events'
                    ? 'e.g., Fully booked on this date, Not in service area, etc.'
                    : 'e.g., Cannot prepare samples this week, Items unavailable, etc.'}
            />

            <OwnerOrderDetailDrawer
                isOpen={Boolean(detailOrderId)}
                orderId={detailOrderId}
                onClose={() => setDetailOrderId(null)}
            />
        </div>
    );
}
