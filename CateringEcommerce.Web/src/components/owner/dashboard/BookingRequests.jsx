import { useState, useEffect, useCallback } from 'react';
import {
    Calendar, TrendingUp, BarChart2, Clock, CheckCircle, XCircle,
    Search, User, Users, Home, MapPin, Tag, Truck, FlaskConical,
    ClipboardList, Check, X, Eye,
} from 'lucide-react';
import { ownerApiService } from '../../../services/ownerApi';
import { useConfirmation } from '../../../contexts/ConfirmationContext';
import { Skeleton } from '../../../design-system/components';
import OwnerOrderDetailDrawer from './OwnerOrderDetailDrawer';

// ── Status pill helper ────────────────────────────────────────
const statusPillClass = (s) => ({
    Pending: 's-pending', Confirmed: 's-confirmed', Cancelled: 's-cancelled',
    Completed: 's-completed', InProgress: 's-inprogress',
}[s] || 's-pending');

const sampleStatusPillClass = (s) => ({
    SAMPLE_REQUESTED: 's-pending', SAMPLE_ACCEPTED: 's-confirmed',
    SAMPLE_REJECTED: 's-cancelled', SAMPLE_PREPARING: 's-inprogress',
    READY_FOR_PICKUP: 's-active', IN_TRANSIT: 's-inprogress', DELIVERED: 's-completed',
}[s] || 's-pending');

const sampleStatusLabel = {
    SAMPLE_REQUESTED: 'Requested', SAMPLE_ACCEPTED: 'Accepted', SAMPLE_REJECTED: 'Rejected',
    SAMPLE_PREPARING: 'Preparing', READY_FOR_PICKUP: 'Ready for Pickup',
    IN_TRANSIT: 'In Transit', DELIVERED: 'Delivered',
};

// ── Stat card using portal .stat classes ─────────────────────
const StatCard = ({ label, value, icon: Icon, iconBgStyle }) => (
    <div className="stat">
        <div className="stat__ic" style={iconBgStyle}>
            <Icon size={18} strokeWidth={1.75} />
        </div>
        <div className="stat__l">{label}</div>
        <div className="stat__v">{value ?? '—'}</div>
    </div>
);

// ── Reject Modal ──────────────────────────────────────────────
const RejectModal = ({ isOpen, onClose, onConfirm, isProcessing, title = 'Reject Request', placeholder }) => {
    const [reason, setReason] = useState('');
    useEffect(() => { if (isOpen) setReason(''); }, [isOpen]);
    if (!isOpen) return null;
    return (
        <div className="fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center z-50 p-4">
            <div className="panel w-full max-w-md">
                <h2 className="text-xl font-bold text-neutral-900 mb-2">{title}</h2>
                <p className="text-neutral-600 text-sm mb-4">Please provide a reason for rejecting this request:</p>
                <textarea
                    value={reason}
                    onChange={(e) => setReason(e.target.value)}
                    placeholder={placeholder || 'e.g., Fully booked on this date, Not in service area, etc.'}
                    rows={3}
                    className="w-full px-4 py-2 border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-red-400 focus:border-transparent mb-4 text-sm resize-none"
                />
                <div className="flex gap-3">
                    <button
                        onClick={onClose}
                        disabled={isProcessing}
                        className="flex-1 px-4 py-2.5 border-2 border-neutral-200 text-neutral-700 rounded-xl font-semibold text-sm hover:bg-neutral-50 disabled:opacity-50"
                    >
                        Cancel
                    </button>
                    <button
                        onClick={() => onConfirm(reason)}
                        disabled={isProcessing || !reason.trim()}
                        className="flex-1 px-4 py-2.5 bg-red-600 text-white rounded-xl font-semibold text-sm hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                        {isProcessing ? 'Rejecting...' : 'Confirm Reject'}
                    </button>
                </div>
            </div>
        </div>
    );
};

// ── Booking Card ──────────────────────────────────────────────
const BookingCard = ({ booking, onAccept, onReject, onViewDetails, isProcessing }) => {
    const fmt = (d) => d ? new Date(d).toLocaleDateString('en-IN', { day: 'numeric', month: 'short', year: 'numeric' }) : '-';
    const fmtDt = (d) => d ? new Date(d).toLocaleDateString('en-IN', { day: 'numeric', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' }) : '-';
    return (
        <div className="panel" style={{ marginBottom: 0 }}>
            <div className="flex items-start justify-between mb-4">
                <div className="flex-1">
                    <div className="flex items-center gap-3 mb-2 flex-wrap">
                        <h3 className="text-lg font-bold text-neutral-900">#{booking.orderNumber}</h3>
                        <span className={`status-pill ${statusPillClass(booking.orderStatus)}`}>
                            <span className="dot" />{booking.orderStatus}
                        </span>
                        {booking.daysUntilEvent <= 3 && booking.daysUntilEvent >= 0 && (
                            <span className="status-pill s-cancelled" style={{ fontSize: 10 }}>URGENT</span>
                        )}
                    </div>
                    <p className="text-neutral-600 flex items-center gap-2 text-sm">
                        <User size={14} strokeWidth={1.75} className="text-neutral-400" />
                        {booking.customerName}
                        {booking.customerPhone && (
                            <span className="text-neutral-400 text-xs">({booking.customerPhone})</span>
                        )}
                    </p>
                </div>
                <div className="text-right">
                    <p className="text-xl font-bold text-neutral-900 amount">₹{(booking.totalAmount || 0).toLocaleString('en-IN')}</p>
                    <p className="text-xs text-neutral-500">{booking.paymentStatus || 'Pending'}</p>
                </div>
            </div>

            <div className="grid grid-cols-2 gap-4 mb-4 p-4 bg-neutral-50 rounded-xl">
                <div className="flex items-start gap-2">
                    <Calendar size={16} className="mt-0.5 shrink-0" style={{ color: 'var(--color-primary)' }} />
                    <div>
                        <p className="text-xs text-neutral-500 font-medium">Event Date</p>
                        <p className="text-sm font-semibold text-neutral-900">{fmt(booking.eventDate)}</p>
                    </div>
                </div>
                <div className="flex items-start gap-2">
                    <Clock size={16} className="mt-0.5 shrink-0" style={{ color: 'var(--color-primary)' }} />
                    <div>
                        <p className="text-xs text-neutral-500 font-medium">Days Until Event</p>
                        <p className={`text-sm font-semibold ${booking.daysUntilEvent <= 3 ? 'text-red-600' : 'text-neutral-900'}`}>
                            {booking.daysUntilEvent > 0 ? `${booking.daysUntilEvent} days` : booking.daysUntilEvent === 0 ? 'Today' : 'Past'}
                        </p>
                    </div>
                </div>
                <div className="flex items-start gap-2">
                    <Users size={16} className="mt-0.5 shrink-0" style={{ color: 'var(--color-primary)' }} />
                    <div>
                        <p className="text-xs text-neutral-500 font-medium">Guests</p>
                        <p className="text-sm font-semibold text-neutral-900">{booking.guestCount} people</p>
                    </div>
                </div>
                <div className="flex items-start gap-2">
                    <Home size={16} className="mt-0.5 shrink-0" style={{ color: 'var(--color-primary)' }} />
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
                            className="flex-1 min-w-[120px] flex items-center justify-center gap-2 bg-green-600 hover:bg-green-700 text-white px-4 py-2.5 rounded-xl font-semibold text-sm transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            <Check size={16} strokeWidth={2.5} /> Accept
                        </button>
                        <button
                            onClick={() => onReject(booking.orderId)}
                            disabled={isProcessing}
                            className="flex-1 min-w-[120px] flex items-center justify-center gap-2 bg-red-600 hover:bg-red-700 text-white px-4 py-2.5 rounded-xl font-semibold text-sm transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            <X size={16} strokeWidth={2.5} /> Reject
                        </button>
                    </>
                )}
                <button
                    onClick={() => onViewDetails(booking.orderId)}
                    className="flex-1 min-w-[120px] flex items-center justify-center gap-2 bg-white hover:bg-neutral-50 text-neutral-700 border-2 border-neutral-200 px-4 py-2.5 rounded-xl font-semibold text-sm transition-colors"
                >
                    <Eye size={16} strokeWidth={1.75} /> View Details
                </button>
            </div>

            <div className="mt-3 pt-3 border-t border-neutral-100">
                <p className="text-xs text-neutral-500">Ordered on {fmtDt(booking.orderDate)}</p>
            </div>
        </div>
    );
};

// ── Sample Request Card ───────────────────────────────────────
const SampleRequestCard = ({ request, onAccept, onReject, isProcessing }) => {
    const fmt = (d) => d ? new Date(d).toLocaleDateString('en-IN', { day: 'numeric', month: 'short', year: 'numeric' }) : '-';
    const isRequested = request.status === 'SAMPLE_REQUESTED';
    const sourceLabel = request.sourceType === 'event-order'
        ? `EVENT ${request.parentOrderNumber || ''}`.trim()
        : `SAMPLE #${request.sampleOrderId}`;

    return (
        <div className="panel" style={{ marginBottom: 0 }}>
            <div className="flex items-start justify-between mb-4">
                <div className="flex-1">
                    <div className="flex items-center gap-3 mb-2 flex-wrap">
                        <span className="text-xs font-bold text-neutral-400 bg-neutral-100 px-2 py-1 rounded-lg">{sourceLabel}</span>
                        <span className={`status-pill ${sampleStatusPillClass(request.status)}`}>
                            <span className="dot" />{sampleStatusLabel[request.status] || request.status}
                        </span>
                    </div>
                    <p className="text-neutral-700 font-semibold flex items-center gap-2 text-sm">
                        <User size={14} strokeWidth={1.75} className="text-neutral-400" />
                        {request.customerName}
                        {request.customerPhone && (
                            <span className="text-neutral-400 text-xs font-normal">({request.customerPhone})</span>
                        )}
                    </p>
                </div>
                <div className="text-right">
                    <p className="text-xl font-bold text-neutral-900 amount">₹{(request.totalAmount || 0).toLocaleString('en-IN')}</p>
                    <p className="text-xs text-neutral-500">{request.paymentStatus || 'Pending'}</p>
                </div>
            </div>

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

            <div className="grid grid-cols-2 gap-4 mb-4 p-4 bg-neutral-50 rounded-xl">
                <div className="flex items-start gap-2">
                    <MapPin size={16} className="mt-0.5 shrink-0 text-teal-600" />
                    <div>
                        <p className="text-xs text-neutral-500 font-medium">{request.sourceType === 'event-order' ? 'Event Address' : 'Pickup Address'}</p>
                        <p className="text-sm font-semibold text-neutral-900">{request.pickupAddress || '-'}</p>
                    </div>
                </div>
                <div className="flex items-start gap-2">
                    <Calendar size={16} className="mt-0.5 shrink-0 text-teal-600" />
                    <div>
                        <p className="text-xs text-neutral-500 font-medium">Requested On</p>
                        <p className="text-sm font-semibold text-neutral-900">{fmt(request.requestedDate)}</p>
                    </div>
                </div>
                {(request.samplePriceTotal > 0 || request.deliveryCharge > 0) && (
                    <>
                        <div className="flex items-start gap-2">
                            <Tag size={16} className="mt-0.5 shrink-0 text-teal-600" />
                            <div>
                                <p className="text-xs text-neutral-500 font-medium">Sample Price</p>
                                <p className="text-sm font-semibold text-neutral-900">₹{(request.samplePriceTotal || 0).toLocaleString('en-IN')}</p>
                            </div>
                        </div>
                        <div className="flex items-start gap-2">
                            <Truck size={16} className="mt-0.5 shrink-0 text-teal-600" />
                            <div>
                                <p className="text-xs text-neutral-500 font-medium">Delivery Charge</p>
                                <p className="text-sm font-semibold text-neutral-900">₹{(request.deliveryCharge || 0).toLocaleString('en-IN')}</p>
                            </div>
                        </div>
                    </>
                )}
            </div>

            {request.status === 'SAMPLE_REJECTED' && request.rejectionReason && (
                <div className="mb-4 p-3 bg-red-50 border border-red-100 rounded-xl">
                    <p className="text-xs text-red-600 font-medium">Rejection Reason</p>
                    <p className="text-sm text-red-800 mt-0.5">{request.rejectionReason}</p>
                </div>
            )}

            {isRequested && (
                <div className="flex flex-wrap gap-3">
                    <button
                        onClick={() => onAccept(request)}
                        disabled={isProcessing}
                        className="flex-1 min-w-[120px] flex items-center justify-center gap-2 bg-teal-600 hover:bg-teal-700 text-white px-4 py-2.5 rounded-xl font-semibold text-sm transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                        <Check size={16} strokeWidth={2.5} /> Accept
                    </button>
                    <button
                        onClick={() => onReject(request)}
                        disabled={isProcessing}
                        className="flex-1 min-w-[120px] flex items-center justify-center gap-2 bg-red-600 hover:bg-red-700 text-white px-4 py-2.5 rounded-xl font-semibold text-sm transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                        <X size={16} strokeWidth={2.5} /> Reject
                    </button>
                </div>
            )}
        </div>
    );
};

// ── Main BookingRequests Component ────────────────────────────
export default function BookingRequests() {
    const confirm = useConfirmation();
    const [requestType, setRequestType] = useState('events');
    const [isProcessing, setIsProcessing] = useState(false);
    const [rejectModal, setRejectModal]   = useState({ open: false, id: null, payload: null });
    const [detailOrderId, setDetailOrderId] = useState(null);

    const [eventFilter, setEventFilter]   = useState('all');
    const [eventSearch, setEventSearch]   = useState('');
    const [eventPage, setEventPage]       = useState(1);
    const [bookings, setBookings]         = useState([]);
    const [eventTotalPages, setEventTotalPages] = useState(1);
    const [eventStats, setEventStats]     = useState(null);
    const [eventLoading, setEventLoading] = useState(true);
    const [eventError, setEventError]     = useState(null);

    const [sampleFilter, setSampleFilter] = useState('all');
    const [sampleSearch, setSampleSearch] = useState('');
    const [samplePage, setSamplePage]     = useState(1);
    const [samples, setSamples]           = useState([]);
    const [sampleTotalPages, setSampleTotalPages] = useState(1);
    const [sampleStats, setSampleStats]   = useState({ requested: 0, accepted: 0, rejected: 0, total: 0 });
    const [sampleLoading, setSampleLoading] = useState(false);
    const [sampleError, setSampleError]   = useState(null);

    const fetchBookings = useCallback(async () => {
        setEventLoading(true);
        setEventError(null);
        try {
            const filters = {};
            if (eventFilter === 'pending')   filters.OrderStatus = 'Pending';
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

    useEffect(() => { fetchBookings(); }, [fetchBookings]);
    useEffect(() => { fetchEventStats(); }, [fetchEventStats]);
    useEffect(() => { if (requestType === 'samples') fetchSamples(); }, [requestType, fetchSamples]);
    useEffect(() => { if (requestType === 'samples') fetchSampleStats(); }, [requestType, fetchSampleStats]);

    const handleAcceptEvent = async (orderId) => {
        const confirmed = await confirm({ type: 'info', title: 'Accept Booking', message: 'Are you sure you want to accept this booking?', confirmText: 'Accept', cancelText: 'Cancel' });
        if (!confirmed) return;
        setIsProcessing(true);
        try {
            const response = await ownerApiService.updateOrderStatus(orderId, { NewStatus: 'Confirmed', Comments: 'Booking accepted by partner' });
            if (response.result) { fetchBookings(); fetchEventStats(); }
            else alert(response.message || 'Failed to accept booking');
        } catch { alert('An error occurred while accepting the booking'); }
        finally { setIsProcessing(false); }
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
                fetchBookings(); fetchEventStats();
            } else alert(response.message || 'Failed to reject booking');
        } catch { alert('An error occurred while rejecting the booking'); }
        finally { setIsProcessing(false); }
    };

    const handleAcceptSample = async (request) => {
        const confirmed = await confirm({ type: 'info', title: 'Accept Sample Request', message: 'Are you sure you want to accept this sample request?', confirmText: 'Accept', cancelText: 'Cancel' });
        if (!confirmed) return;
        setIsProcessing(true);
        try {
            const response = await ownerApiService.actionSampleRequest(request.sampleOrderId, {
                Action: 'Accept', SourceType: request.sourceType,
                LinkedOrderId: request.linkedOrderId, LinkedOrderItemId: request.linkedOrderItemId,
            });
            if (response.result) { fetchSamples(); fetchSampleStats(); }
            else alert(response.message || 'Failed to accept sample request');
        } catch { alert('An error occurred while accepting the sample request'); }
        finally { setIsProcessing(false); }
    };

    const handleRejectSample = (request) => setRejectModal({ open: true, id: request.sampleOrderId, payload: request });

    const confirmRejectSample = async (reason) => {
        setIsProcessing(true);
        try {
            const response = await ownerApiService.actionSampleRequest(rejectModal.id, {
                Action: 'Reject', RejectionReason: reason,
                SourceType: rejectModal.payload?.sourceType,
                LinkedOrderId: rejectModal.payload?.linkedOrderId,
                LinkedOrderItemId: rejectModal.payload?.linkedOrderItemId,
            });
            if (response.result) {
                setRejectModal({ open: false, id: null, payload: null });
                fetchSamples(); fetchSampleStats();
            } else alert(response.message || 'Failed to reject sample request');
        } catch { alert('An error occurred while rejecting the sample request'); }
        finally { setIsProcessing(false); }
    };

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
        <div className="space-y-5">
            {/* ── Search + Request type tabs ── */}
            <div className="flex flex-col sm:flex-row gap-4 items-start sm:items-center justify-between">
                <div className="portal-tabs">
                    <button onClick={() => setRequestType('events')} className={requestType === 'events' ? 'is-active' : ''}>
                        <span className="flex items-center gap-2">
                            <Calendar size={13} strokeWidth={2} />
                            Event Order Requests
                            {eventCounts.pending > 0 && (
                                <span className="inline-flex items-center justify-center px-1.5 py-0.5 rounded-full font-bold bg-amber-100 text-amber-700" style={{ fontSize: 10 }}>
                                    {eventCounts.pending}
                                </span>
                            )}
                        </span>
                    </button>
                    <button onClick={() => setRequestType('samples')} className={requestType === 'samples' ? 'is-active' : ''}>
                        <span className="flex items-center gap-2">
                            <FlaskConical size={13} strokeWidth={2} />
                            Sample Taste Requests
                            {sampleStats.requested > 0 && (
                                <span className="inline-flex items-center justify-center px-1.5 py-0.5 rounded-full font-bold bg-teal-100 text-teal-700" style={{ fontSize: 10 }}>
                                    {sampleStats.requested}
                                </span>
                            )}
                        </span>
                    </button>
                </div>

                {/* Search */}
                <div className="relative w-full sm:w-80">
                    <Search size={16} className="absolute left-3.5 top-1/2 -translate-y-1/2 text-neutral-400" strokeWidth={1.75} />
                    <input
                        type="text"
                        placeholder={requestType === 'events' ? 'Search customer or order…' : 'Search customer name…'}
                        value={requestType === 'events' ? eventSearch : sampleSearch}
                        onChange={(e) => {
                            if (requestType === 'events') { setEventSearch(e.target.value); setEventPage(1); }
                            else { setSampleSearch(e.target.value); setSamplePage(1); }
                        }}
                        className="w-full pl-10 pr-4 py-2 bg-white border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-orange-400 focus:border-transparent text-sm"
                    />
                </div>
            </div>

            {/* ══ EVENT ORDERS TAB ══════════════════════════════════════ */}
            {requestType === 'events' && (
                <>
                    {/* Stats */}
                    <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-4">
                        <StatCard label="Today"      value={eventStats?.todayRequests  ?? '-'} icon={Calendar}     iconBgStyle={{ background: 'rgba(59,130,246,0.1)', color: '#3B82F6' }} />
                        <StatCard label="This Week"  value={eventStats?.weekRequests   ?? '-'} icon={TrendingUp}   iconBgStyle={{ background: 'rgba(99,102,241,0.1)', color: '#6366F1' }} />
                        <StatCard label="This Month" value={eventStats?.monthRequests  ?? '-'} icon={BarChart2}    iconBgStyle={{ background: 'rgba(168,85,247,0.1)', color: '#A855F7' }} />
                        <StatCard label="Pending"    value={eventStats?.totalPending   ?? '-'} icon={Clock}        iconBgStyle={{ background: 'rgba(245,158,11,0.1)', color: '#D97706' }} />
                        <StatCard label="Confirmed"  value={eventStats?.totalConfirmed ?? '-'} icon={CheckCircle}  iconBgStyle={{ background: 'rgba(34,197,94,0.1)',  color: '#16A34A' }} />
                        <StatCard label="Rejected"   value={eventStats?.totalRejected  ?? '-'} icon={XCircle}      iconBgStyle={{ background: 'rgba(239,68,68,0.1)',   color: '#DC2626' }} />
                    </div>

                    {/* Filter tabs */}
                    <div className="portal-tabs">
                        {[
                            { key: 'all',      label: 'All Requests' },
                            { key: 'pending',  label: 'Pending'      },
                            { key: 'accepted', label: 'Accepted'     },
                            { key: 'rejected', label: 'Rejected'     },
                        ].map(({ key, label }) => (
                            <button key={key} onClick={() => { setEventFilter(key); setEventPage(1); }}
                                className={eventFilter === key ? 'is-active' : ''}>
                                <span className="flex items-center gap-1.5">
                                    {label}
                                    {eventCounts[key] > 0 && (
                                        <span style={{ marginLeft: 2, background: 'rgba(0,0,0,0.08)', borderRadius: 999, padding: '1px 6px', fontSize: 10 }}>
                                            {eventCounts[key]}
                                        </span>
                                    )}
                                </span>
                            </button>
                        ))}
                    </div>

                    {/* Loading */}
                    {eventLoading && (
                        <div className="grid grid-cols-1 xl:grid-cols-2 gap-5">
                            {[...Array(4)].map((_, i) => <Skeleton key={i} className="h-64 w-full rounded-2xl" />)}
                        </div>
                    )}

                    {/* Error */}
                    {eventError && !eventLoading && (
                        <div className="panel" style={{ borderColor: 'rgba(239,68,68,0.3)', background: '#FEF2F2' }}>
                            <p className="text-sm font-medium text-red-700">{eventError}</p>
                            <button onClick={fetchBookings} className="mt-2 text-sm font-semibold text-red-600 underline">Try again</button>
                        </div>
                    )}

                    {/* Cards */}
                    {!eventLoading && !eventError && bookings.length > 0 && (
                        <div className="grid grid-cols-1 xl:grid-cols-2 gap-5">
                            {bookings.map((booking) => (
                                <BookingCard
                                    key={booking.orderId}
                                    booking={booking}
                                    onAccept={handleAcceptEvent}
                                    onReject={handleRejectEvent}
                                    onViewDetails={setDetailOrderId}
                                    isProcessing={isProcessing}
                                />
                            ))}
                        </div>
                    )}

                    {/* Empty */}
                    {!eventLoading && !eventError && bookings.length === 0 && (
                        <div className="panel text-center py-14">
                            <ClipboardList size={48} strokeWidth={1} className="mx-auto mb-3 text-neutral-300" />
                            <h3 className="text-lg font-semibold text-neutral-900 mb-1">No Event Booking Requests</h3>
                            <p className="text-sm text-neutral-500">
                                {eventSearch ? 'No bookings match your search criteria.'
                                    : eventFilter !== 'all' ? `No ${eventFilter} booking requests at the moment.`
                                    : 'No new booking requests at the moment.'}
                            </p>
                        </div>
                    )}

                    {/* Pagination */}
                    {!eventLoading && eventTotalPages > 1 && (
                        <div className="flex justify-center items-center gap-3 pt-2">
                            <button onClick={() => setEventPage(p => Math.max(1, p - 1))} disabled={eventPage === 1}
                                className="px-4 py-2 rounded-xl border border-neutral-200 text-sm font-medium hover:bg-neutral-50 disabled:opacity-50 disabled:cursor-not-allowed">
                                Previous
                            </button>
                            <span className="text-sm text-neutral-600">Page {eventPage} of {eventTotalPages}</span>
                            <button onClick={() => setEventPage(p => Math.min(eventTotalPages, p + 1))} disabled={eventPage === eventTotalPages}
                                className="px-4 py-2 rounded-xl border border-neutral-200 text-sm font-medium hover:bg-neutral-50 disabled:opacity-50 disabled:cursor-not-allowed">
                                Next
                            </button>
                        </div>
                    )}
                </>
            )}

            {/* ══ SAMPLE TASTE REQUESTS TAB ════════════════════════════ */}
            {requestType === 'samples' && (
                <>
                    {/* Stats */}
                    <div className="stat-grid">
                        <StatCard label="Total"     value={sampleStats.total}     icon={FlaskConical} iconBgStyle={{ background: 'rgba(20,184,166,0.1)',  color: '#0F766E' }} />
                        <StatCard label="Requested" value={sampleStats.requested} icon={Clock}        iconBgStyle={{ background: 'rgba(245,158,11,0.1)',  color: '#D97706' }} />
                        <StatCard label="Accepted"  value={sampleStats.accepted}  icon={CheckCircle}  iconBgStyle={{ background: 'rgba(34,197,94,0.1)',   color: '#16A34A' }} />
                        <StatCard label="Rejected"  value={sampleStats.rejected}  icon={XCircle}      iconBgStyle={{ background: 'rgba(239,68,68,0.1)',   color: '#DC2626' }} />
                    </div>

                    {/* Filter tabs */}
                    <div className="portal-tabs">
                        {[
                            { key: 'all',              label: 'All'       },
                            { key: 'SAMPLE_REQUESTED', label: 'Requested' },
                            { key: 'SAMPLE_ACCEPTED',  label: 'Accepted'  },
                            { key: 'SAMPLE_REJECTED',  label: 'Rejected'  },
                        ].map(({ key, label }) => (
                            <button key={key} onClick={() => { setSampleFilter(key); setSamplePage(1); }}
                                className={sampleFilter === key ? 'is-active' : ''}>
                                <span className="flex items-center gap-1.5">
                                    {label}
                                    {sampleCounts[key] > 0 && (
                                        <span style={{ marginLeft: 2, background: 'rgba(0,0,0,0.08)', borderRadius: 999, padding: '1px 6px', fontSize: 10 }}>
                                            {sampleCounts[key]}
                                        </span>
                                    )}
                                </span>
                            </button>
                        ))}
                    </div>

                    {/* Loading */}
                    {sampleLoading && (
                        <div className="grid grid-cols-1 xl:grid-cols-2 gap-5">
                            {[...Array(4)].map((_, i) => <Skeleton key={i} className="h-56 w-full rounded-2xl" />)}
                        </div>
                    )}

                    {/* Error */}
                    {sampleError && !sampleLoading && (
                        <div className="panel" style={{ borderColor: 'rgba(239,68,68,0.3)', background: '#FEF2F2' }}>
                            <p className="text-sm font-medium text-red-700">{sampleError}</p>
                            <button onClick={fetchSamples} className="mt-2 text-sm font-semibold text-red-600 underline">Try again</button>
                        </div>
                    )}

                    {/* Cards */}
                    {!sampleLoading && !sampleError && samples.length > 0 && (
                        <div className="grid grid-cols-1 xl:grid-cols-2 gap-5">
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
                        <div className="panel text-center py-14">
                            <FlaskConical size={48} strokeWidth={1} className="mx-auto mb-3 text-neutral-300" />
                            <h3 className="text-lg font-semibold text-neutral-900 mb-1">No Sample Taste Requests</h3>
                            <p className="text-sm text-neutral-500">
                                {sampleSearch ? 'No sample requests match your search.'
                                    : sampleFilter !== 'all' ? `No ${sampleFilter.replace('SAMPLE_', '').toLowerCase()} sample requests at the moment.`
                                    : 'No sample taste requests at the moment.'}
                            </p>
                        </div>
                    )}

                    {/* Pagination */}
                    {!sampleLoading && sampleTotalPages > 1 && (
                        <div className="flex justify-center items-center gap-3 pt-2">
                            <button onClick={() => setSamplePage(p => Math.max(1, p - 1))} disabled={samplePage === 1}
                                className="px-4 py-2 rounded-xl border border-neutral-200 text-sm font-medium hover:bg-neutral-50 disabled:opacity-50 disabled:cursor-not-allowed">
                                Previous
                            </button>
                            <span className="text-sm text-neutral-600">Page {samplePage} of {sampleTotalPages}</span>
                            <button onClick={() => setSamplePage(p => Math.min(sampleTotalPages, p + 1))} disabled={samplePage === sampleTotalPages}
                                className="px-4 py-2 rounded-xl border border-neutral-200 text-sm font-medium hover:bg-neutral-50 disabled:opacity-50 disabled:cursor-not-allowed">
                                Next
                            </button>
                        </div>
                    )}
                </>
            )}

            {/* Reject Modal */}
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
