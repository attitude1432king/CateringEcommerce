import React, { useState, useEffect, useCallback, useRef } from 'react';
import {
    Search, User, Calendar, Clock, Users, MapPin, Settings,
    Eye, ChevronLeft, ChevronRight, AlertCircle, X,
} from 'lucide-react';
import { ownerApiService } from '../../../services/ownerApi';
import { Skeleton } from '../../../design-system/components';
import OwnerOrderDetailDrawer from './OwnerOrderDetailDrawer';

// ── Helpers ───────────────────────────────────────────────────

const statusPillClass = (s) => ({
    Confirmed: 's-confirmed', 'In-Progress': 's-inprogress',
    Completed: 's-completed', Cancelled: 's-cancelled', Pending: 's-pending',
}[s] || 's-pending');

const statusLabel = (s) => ({
    Confirmed: 'Upcoming', 'In-Progress': 'In Progress',
    Completed: 'Completed', Cancelled: 'Cancelled', Pending: 'Pending',
}[s] || s);

const paymentPillClass = (s) => ({
    Paid: 's-completed', Pending: 's-pending', Partial: 's-inprogress', Failed: 's-cancelled',
}[s] || 's-pending');

const TAB_STATUS_FILTER = {
    all: null, upcoming: 'Confirmed', 'in-progress': 'In-Progress', completed: 'Completed',
};

function formatDate(dateStr) {
    if (!dateStr) return '—';
    return new Date(dateStr).toLocaleDateString('en-IN', { day: 'numeric', month: 'short', year: 'numeric' });
}

function getProgress(status) {
    return ({ Confirmed: 25, 'In-Progress': 75, Completed: 100 })[status] || 0;
}

function parsePackageSelections(value) {
    if (!value) return null;
    try { return typeof value === 'string' ? JSON.parse(value) : value; }
    catch { return null; }
}

// ── Manage / Status Update Modal ──────────────────────────────

const VALID_NEXT_STATUSES = ['Confirmed', 'In-Progress', 'Completed', 'Cancelled'];

const ManageModal = ({ order, onClose, onSuccess }) => {
    const [newStatus, setNewStatus] = useState(order.orderStatus || 'Confirmed');
    const [comments, setComments] = useState('');
    const [saving, setSaving] = useState(false);
    const [error, setError] = useState(null);

    const handleSubmit = async (e) => {
        e.preventDefault();
        setSaving(true);
        setError(null);
        try {
            await ownerApiService.updateOrderStatus(order.orderId, { NewStatus: newStatus, Comments: comments });
            onSuccess();
        } catch {
            setError('Failed to update status. Please try again.');
        } finally {
            setSaving(false);
        }
    };

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm" onClick={onClose}>
            <div className="panel w-full max-w-md" onClick={e => e.stopPropagation()}>
                <div className="flex items-start justify-between mb-4">
                    <div>
                        <h2 className="text-lg font-bold text-neutral-900">Manage Order</h2>
                        <p className="text-xs text-neutral-500 mt-0.5">#{order.orderNumber} · {order.customerName}</p>
                    </div>
                    <button onClick={onClose} className="icon-btn"><X size={18} /></button>
                </div>

                <form onSubmit={handleSubmit} className="space-y-4">
                    <div>
                        <label className="block text-sm font-semibold text-neutral-700 mb-1.5">Update Status</label>
                        <select
                            value={newStatus}
                            onChange={e => setNewStatus(e.target.value)}
                            className="w-full border border-neutral-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-orange-400"
                        >
                            {VALID_NEXT_STATUSES.map(s => <option key={s} value={s}>{s}</option>)}
                        </select>
                    </div>

                    <div>
                        <label className="block text-sm font-semibold text-neutral-700 mb-1.5">
                            Comments <span className="font-normal text-neutral-400">(optional)</span>
                        </label>
                        <textarea
                            value={comments}
                            onChange={e => setComments(e.target.value)}
                            rows={3}
                            placeholder="Add a note about this status change..."
                            className="w-full border border-neutral-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-orange-400 resize-none"
                        />
                    </div>

                    {error && <p className="text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{error}</p>}

                    <div className="flex gap-3 pt-1">
                        <button type="button" onClick={onClose}
                            className="flex-1 py-2.5 rounded-xl border-2 border-neutral-200 text-neutral-700 font-semibold text-sm hover:bg-neutral-50 transition-colors">
                            Cancel
                        </button>
                        <button type="submit" disabled={saving}
                            className="flex-1 py-2.5 rounded-xl text-white font-semibold text-sm transition-all disabled:opacity-60"
                            style={{ background: 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)' }}>
                            {saving ? 'Saving…' : 'Update Status'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};

// ── Event Order Card ──────────────────────────────────────────

const EventOrderCard = ({ order, onViewDetails, onManage }) => {
    const progress = getProgress(order.orderStatus);
    const isCancelled = order.orderStatus === 'Cancelled';
    const isUrgent = order.daysUntilEvent >= 0 && order.daysUntilEvent <= 3 && order.orderStatus === 'Confirmed';

    return (
        <div className="panel" style={{ marginBottom: 0, padding: 0, overflow: 'hidden', display: 'flex', flexDirection: 'column' }}>
            {/* Header */}
            <div className="p-5 border-b border-neutral-100">
                <div className="flex items-start justify-between mb-3">
                    <div className="flex-1 min-w-0 pr-4">
                        <div className="flex items-center flex-wrap gap-2 mb-2">
                            <h3 className="text-lg font-bold text-neutral-900">#{order.orderNumber}</h3>
                            <span className={`status-pill ${statusPillClass(order.orderStatus)}`}>
                                <span className="dot" />{statusLabel(order.orderStatus)}
                            </span>
                            {isUrgent && (
                                <span className="status-pill s-cancelled animate-pulse" style={{ fontSize: 10 }}>URGENT</span>
                            )}
                        </div>
                        <p className="text-neutral-600 flex items-center gap-2 text-sm truncate">
                            <User size={13} strokeWidth={1.75} className="shrink-0 text-neutral-400" />
                            {order.customerName}
                        </p>
                    </div>
                    <div className="text-right shrink-0">
                        <p className="text-xl font-bold text-neutral-900 amount">₹{order.totalAmount?.toLocaleString('en-IN')}</p>
                        <p className="text-xs text-neutral-500">Total Amount</p>
                    </div>
                </div>

                {/* Progress Bar */}
                {!isCancelled && (
                    <div className="mt-3">
                        <div className="flex items-center justify-between mb-1.5">
                            <span className="text-xs font-semibold text-neutral-500">Order Progress</span>
                            <span className="text-xs font-bold" style={{ color: 'var(--color-primary)' }}>{progress}%</span>
                        </div>
                        <div className="w-full bg-neutral-100 rounded-full h-1.5">
                            <div className="h-1.5 rounded-full transition-all duration-500"
                                style={{ width: `${progress}%`, background: 'linear-gradient(90deg, #FF6B35, #FFB627)' }} />
                        </div>
                    </div>
                )}
            </div>

            {/* Event Details */}
            <div className="p-5 flex-1 flex flex-col">
                <div className="grid grid-cols-2 gap-4 mb-4">
                    <div className="flex items-start gap-3">
                        <div className="p-2 rounded-lg shrink-0" style={{ background: 'rgba(255,107,53,0.08)' }}>
                            <Calendar size={16} strokeWidth={1.75} style={{ color: 'var(--color-primary)' }} />
                        </div>
                        <div className="min-w-0">
                            <p className="text-xs text-neutral-500 font-medium mb-0.5">Event Date</p>
                            <p className="text-sm font-bold text-neutral-900">{formatDate(order.eventDate)}</p>
                        </div>
                    </div>

                    <div className="flex items-start gap-3">
                        <div className="p-2 rounded-lg shrink-0" style={{ background: 'rgba(255,182,39,0.1)' }}>
                            <Clock size={16} strokeWidth={1.75} style={{ color: 'var(--color-accent)' }} />
                        </div>
                        <div className="min-w-0">
                            <p className="text-xs text-neutral-500 font-medium mb-0.5">Event Time</p>
                            <p className="text-sm font-bold text-neutral-900">{order.eventTime || '—'}</p>
                        </div>
                    </div>

                    <div className="flex items-start gap-3">
                        <div className="p-2 rounded-lg shrink-0 bg-green-50">
                            <Users size={16} strokeWidth={1.75} className="text-green-600" />
                        </div>
                        <div className="min-w-0">
                            <p className="text-xs text-neutral-500 font-medium mb-0.5">Guests</p>
                            <p className="text-sm font-bold text-neutral-900">{order.guestCount} people</p>
                        </div>
                    </div>

                    <div className="flex items-start gap-3">
                        <div className="p-2 rounded-lg shrink-0 bg-neutral-100">
                            <MapPin size={16} strokeWidth={1.75} className="text-neutral-500" />
                        </div>
                        <div className="min-w-0">
                            <p className="text-xs text-neutral-500 font-medium mb-0.5">Venue</p>
                            <p className="text-sm font-bold text-neutral-900 truncate">{order.venueAddress || '—'}</p>
                        </div>
                    </div>
                </div>

                <div className="mb-4 p-3 bg-neutral-50 rounded-xl">
                    <p className="text-xs text-neutral-500 font-medium mb-0.5">Event Type</p>
                    <p className="text-sm font-bold text-neutral-900">{order.eventType || '—'}</p>
                </div>

                {order.menuItems?.length > 0 && (
                    <div className="mb-4">
                        <p className="text-xs text-neutral-500 font-semibold mb-2">Menu Items</p>
                        <div className="flex flex-wrap gap-2">
                            {order.menuItems.map((item, i) => (
                                <span key={i} className="px-2.5 py-1 rounded-lg text-xs font-medium border"
                                    style={{ background: 'rgba(255,107,53,0.06)', borderColor: 'rgba(255,107,53,0.15)', color: 'var(--color-primary)' }}>
                                    {item}
                                </span>
                            ))}
                        </div>
                    </div>
                )}

                {/* Action Buttons */}
                <div className="flex gap-3 mt-auto">
                    <button
                        onClick={() => onViewDetails(order.orderId)}
                        className="flex-1 flex items-center justify-center gap-2 text-white px-4 py-2.5 rounded-xl font-semibold text-sm transition-all"
                        style={{ background: 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)' }}
                    >
                        <Eye size={15} strokeWidth={1.75} /> View Details
                    </button>
                    {!isCancelled && order.orderStatus !== 'Completed' && (
                        <button
                            onClick={() => onManage(order)}
                            className="flex-1 flex items-center justify-center gap-2 bg-white hover:bg-neutral-50 text-neutral-700 border-2 border-neutral-200 px-4 py-2.5 rounded-xl font-semibold text-sm transition-all"
                        >
                            <Settings size={15} strokeWidth={1.75} /> Manage
                        </button>
                    )}
                </div>
            </div>

            {/* Footer */}
            <div className="px-5 py-3 bg-neutral-50 border-t border-neutral-100 flex items-center justify-between">
                <p className="text-xs text-neutral-500">Confirmed on {formatDate(order.orderDate)}</p>
                <div className="flex items-center gap-2">
                    <span className={`status-pill ${paymentPillClass(order.paymentStatus)}`}>
                        <span className="dot" />{order.paymentStatus || 'Pending'}
                    </span>
                    {order.balanceAmount > 0 && (
                        <span className="text-xs text-orange-600 font-medium amount">
                            ₹{order.balanceAmount?.toLocaleString('en-IN')} due
                        </span>
                    )}
                </div>
            </div>
        </div>
    );
};

// ── Main Component ────────────────────────────────────────────

const PAGE_SIZE = 10;

export default function EventOrders() {
    const [orders, setOrders] = useState([]);
    const [totalCount, setTotalCount] = useState(0);
    const [totalPages, setTotalPages] = useState(1);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [page, setPage] = useState(1);
    const [activeTab, setActiveTab] = useState('all');
    const [searchQuery, setSearchQuery] = useState('');
    const [stats, setStats] = useState(null);
    const [detailOrderId, setDetailOrderId] = useState(null);
    const [manageOrder, setManageOrder] = useState(null);

    const searchTimer = useRef(null);
    const [debouncedSearch, setDebouncedSearch] = useState('');
    useEffect(() => {
        clearTimeout(searchTimer.current);
        searchTimer.current = setTimeout(() => setDebouncedSearch(searchQuery), 400);
        return () => clearTimeout(searchTimer.current);
    }, [searchQuery]);

    useEffect(() => { setPage(1); }, [activeTab, debouncedSearch]);

    useEffect(() => {
        ownerApiService.getOrderStats()
            .then(res => setStats(res?.data ?? res))
            .catch(() => {});
    }, []);

    const buildFilter = useCallback(() => {
        const f = { SearchTerm: debouncedSearch || undefined };
        if (TAB_STATUS_FILTER[activeTab]) {
            f.OrderStatus = TAB_STATUS_FILTER[activeTab];
        } else {
            f.ExcludeStatuses = ['Pending'];
        }
        return f;
    }, [activeTab, debouncedSearch]);

    const fetchOrders = useCallback(async () => {
        setLoading(true);
        setError(null);
        try {
            const res = await ownerApiService.getOrdersList(page, PAGE_SIZE, buildFilter());
            const data = res?.data ?? res;
            setOrders(data?.orders ?? []);
            setTotalCount(data?.totalCount ?? 0);
            setTotalPages(data?.totalPages ?? 1);
        } catch {
            setError('Failed to load orders. Please try again.');
        } finally {
            setLoading(false);
        }
    }, [page, buildFilter]);

    useEffect(() => { fetchOrders(); }, [fetchOrders]);

    const tabs = [
        { id: 'all',         label: 'All Orders',  count: stats ? (stats.confirmedOrders + stats.completedOrders + stats.cancelledOrders) : null },
        { id: 'upcoming',    label: 'Upcoming',    count: stats?.confirmedOrders ?? null },
        { id: 'in-progress', label: 'In Progress', count: null },
        { id: 'completed',   label: 'Completed',   count: stats?.completedOrders ?? null },
    ];

    return (
        <div className="space-y-5">
            {/* Header + Search */}
            <div className="flex flex-col sm:flex-row gap-4 items-start sm:items-center justify-between">
                <div>
                    <p className="text-sm text-neutral-500 mt-0.5">
                        Track and manage your confirmed event bookings
                        {totalCount > 0 && (
                            <span className="ml-2 font-semibold" style={{ color: 'var(--color-primary)' }}>
                                ({totalCount} orders)
                            </span>
                        )}
                    </p>
                </div>
                <div className="relative w-full sm:w-80">
                    <Search size={16} className="absolute left-3.5 top-1/2 -translate-y-1/2 text-neutral-400" strokeWidth={1.75} />
                    <input
                        type="text"
                        placeholder="Search by customer or order number…"
                        value={searchQuery}
                        onChange={e => setSearchQuery(e.target.value)}
                        className="w-full pl-10 pr-4 py-2 bg-white border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-orange-400 focus:border-transparent text-sm"
                    />
                </div>
            </div>

            {/* Tabs */}
            <div className="portal-tabs">
                {tabs.map(tab => (
                    <button key={tab.id} onClick={() => setActiveTab(tab.id)}
                        className={activeTab === tab.id ? 'is-active' : ''}>
                        <span className="flex items-center gap-1.5">
                            {tab.label}
                            {tab.count !== null && (
                                <span style={{ background: 'rgba(0,0,0,0.08)', borderRadius: 999, padding: '1px 6px', fontSize: 10, fontWeight: 700 }}>
                                    {tab.count}
                                </span>
                            )}
                        </span>
                    </button>
                ))}
            </div>

            {/* Error */}
            {error && (
                <div className="panel flex items-center gap-3" style={{ borderColor: 'rgba(239,68,68,0.3)', background: '#FEF2F2' }}>
                    <AlertCircle size={18} className="text-red-500 shrink-0" />
                    <span className="text-sm font-medium text-red-700">{error}</span>
                    <button onClick={fetchOrders} className="ml-auto text-xs font-semibold text-red-600 underline">Retry</button>
                </div>
            )}

            {/* Orders Grid */}
            {loading ? (
                <div className="grid grid-cols-1 xl:grid-cols-2 gap-5">
                    {[...Array(4)].map((_, i) => <Skeleton key={i} className="h-72 w-full rounded-2xl" />)}
                </div>
            ) : orders.length > 0 ? (
                <div className="grid grid-cols-1 xl:grid-cols-2 gap-5">
                    {orders.map(order => (
                        <EventOrderCard
                            key={order.orderId}
                            order={order}
                            onViewDetails={setDetailOrderId}
                            onManage={setManageOrder}
                        />
                    ))}
                </div>
            ) : !error ? (
                <div className="panel text-center py-14">
                    <Calendar size={48} strokeWidth={1} className="mx-auto mb-3 text-neutral-300" />
                    <h3 className="text-lg font-semibold text-neutral-900 mb-1">No Event Orders</h3>
                    <p className="text-sm text-neutral-500">
                        {debouncedSearch
                            ? 'No orders match your search criteria.'
                            : activeTab !== 'all'
                            ? `No ${tabs.find(t => t.id === activeTab)?.label.toLowerCase()} orders at the moment.`
                            : 'No confirmed events to display.'}
                    </p>
                </div>
            ) : null}

            {/* Pagination */}
            {!loading && totalPages > 1 && (
                <div className="flex items-center justify-between panel" style={{ padding: '10px 16px' }}>
                    <p className="text-sm text-neutral-500">
                        Page <span className="font-semibold text-neutral-900">{page}</span> of{' '}
                        <span className="font-semibold text-neutral-900">{totalPages}</span>
                        <span className="text-neutral-400 ml-2">({totalCount} total)</span>
                    </p>
                    <div className="flex gap-2">
                        <button disabled={page <= 1} onClick={() => setPage(p => p - 1)}
                            className="flex items-center gap-1 px-3 py-1.5 rounded-lg border border-neutral-200 text-sm font-medium text-neutral-700 hover:bg-neutral-50 disabled:opacity-40 disabled:cursor-not-allowed transition-colors">
                            <ChevronLeft size={15} /> Previous
                        </button>
                        <button disabled={page >= totalPages} onClick={() => setPage(p => p + 1)}
                            className="flex items-center gap-1 px-3 py-1.5 rounded-lg border border-neutral-200 text-sm font-medium text-neutral-700 hover:bg-neutral-50 disabled:opacity-40 disabled:cursor-not-allowed transition-colors">
                            Next <ChevronRight size={15} />
                        </button>
                    </div>
                </div>
            )}

            {/* Modals */}
            <OwnerOrderDetailDrawer
                isOpen={Boolean(detailOrderId)}
                orderId={detailOrderId}
                onClose={() => setDetailOrderId(null)}
            />
            {manageOrder && (
                <ManageModal order={manageOrder} onClose={() => setManageOrder(null)} onSuccess={() => { setManageOrder(null); fetchOrders(); }} />
            )}
        </div>
    );
}
