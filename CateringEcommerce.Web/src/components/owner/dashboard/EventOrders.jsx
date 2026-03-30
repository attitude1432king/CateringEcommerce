/*
========================================
File: src/components/owner/dashboard/EventOrders.jsx
Event Order Management — Live API Integration
========================================
*/
import React, { useState, useEffect, useCallback, useRef } from 'react';
import { ownerApiService } from '../../../services/ownerApi';

// ─── Helpers ────────────────────────────────────────────────────────────────

const STATUS_MAP = {
    Confirmed:       { uiKey: 'upcoming',     label: 'Upcoming',    bg: 'bg-blue-100',   text: 'text-blue-800',   border: 'border-blue-200' },
    'In-Progress':   { uiKey: 'in-progress',  label: 'In Progress', bg: 'bg-purple-100', text: 'text-purple-800', border: 'border-purple-200' },
    Completed:       { uiKey: 'completed',    label: 'Completed',   bg: 'bg-green-100',  text: 'text-green-800',  border: 'border-green-200' },
    Cancelled:       { uiKey: 'cancelled',    label: 'Cancelled',   bg: 'bg-red-100',    text: 'text-red-800',    border: 'border-red-200' },
    Pending:         { uiKey: 'pending',      label: 'Pending',     bg: 'bg-yellow-100', text: 'text-yellow-800', border: 'border-yellow-200' },
};

const PAYMENT_STATUS_MAP = {
    Paid:       { bg: 'bg-green-100',  text: 'text-green-700' },
    Pending:    { bg: 'bg-yellow-100', text: 'text-yellow-700' },
    Partial:    { bg: 'bg-orange-100', text: 'text-orange-700' },
    Failed:     { bg: 'bg-red-100',    text: 'text-red-700' },
};

const TAB_STATUS_FILTER = {
    all:          null,
    upcoming:     'Confirmed',
    'in-progress':'In-Progress',
    completed:    'Completed',
};

function formatDate(dateStr) {
    if (!dateStr) return '—';
    return new Date(dateStr).toLocaleDateString('en-IN', { day: 'numeric', month: 'short', year: 'numeric' });
}

function getProgress(status) {
    switch (status) {
        case 'Confirmed':    return 25;
        case 'In-Progress':  return 75;
        case 'Completed':    return 100;
        default:             return 0;
    }
}

function parsePackageSelections(value) {
    if (!value) return null;
    try {
        return typeof value === 'string' ? JSON.parse(value) : value;
    } catch {
        return null;
    }
}

// ─── Sub-components ──────────────────────────────────────────────────────────

const StatusBadge = ({ status }) => {
    const cfg = STATUS_MAP[status] ?? STATUS_MAP.Pending;
    return (
        <span className={`px-3 py-1 rounded-full text-xs font-semibold border ${cfg.bg} ${cfg.text} ${cfg.border}`}>
            {cfg.label}
        </span>
    );
};

const PaymentBadge = ({ status }) => {
    const cfg = PAYMENT_STATUS_MAP[status] ?? PAYMENT_STATUS_MAP.Pending;
    return (
        <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${cfg.bg} ${cfg.text}`}>
            {status || 'Pending'}
        </span>
    );
};

// ─── Skeleton Card ───────────────────────────────────────────────────────────

const SkeletonCard = () => (
    <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 overflow-hidden animate-pulse">
        <div className="p-6 border-b border-neutral-100 space-y-3">
            <div className="flex justify-between">
                <div className="h-5 w-32 bg-neutral-200 rounded" />
                <div className="h-7 w-24 bg-neutral-200 rounded" />
            </div>
            <div className="h-3 w-48 bg-neutral-100 rounded" />
            <div className="h-2 w-full bg-neutral-100 rounded-full mt-4" />
        </div>
        <div className="p-6 space-y-4">
            <div className="grid grid-cols-2 gap-4">
                {[...Array(4)].map((_, i) => (
                    <div key={i} className="flex gap-3">
                        <div className="w-9 h-9 bg-neutral-100 rounded-lg shrink-0" />
                        <div className="space-y-1.5 flex-1">
                            <div className="h-2.5 w-16 bg-neutral-100 rounded" />
                            <div className="h-3.5 w-24 bg-neutral-200 rounded" />
                        </div>
                    </div>
                ))}
            </div>
            <div className="h-9 w-full bg-neutral-100 rounded-xl" />
            <div className="flex gap-2 flex-wrap">
                {[...Array(3)].map((_, i) => <div key={i} className="h-6 w-20 bg-neutral-100 rounded-lg" />)}
            </div>
            <div className="flex gap-3">
                <div className="flex-1 h-11 bg-neutral-200 rounded-xl" />
                <div className="flex-1 h-11 bg-neutral-100 rounded-xl" />
            </div>
        </div>
    </div>
);

// ─── Order Detail Modal ───────────────────────────────────────────────────────

const DetailModal = ({ orderId, onClose }) => {
    const [detail, setDetail] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        ownerApiService.getOrderDetails(orderId)
            .then(res => setDetail(res?.data ?? res))
            .catch(() => setError('Failed to load order details.'))
            .finally(() => setLoading(false));
    }, [orderId]);

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm" onClick={onClose}>
            <div
                className="bg-white rounded-2xl shadow-2xl w-full max-w-2xl max-h-[90vh] overflow-y-auto"
                onClick={e => e.stopPropagation()}
            >
                {/* Header */}
                <div className="sticky top-0 bg-white border-b border-neutral-100 px-6 py-4 flex items-center justify-between rounded-t-2xl z-10">
                    <h2 className="text-lg font-bold text-neutral-900">Order Details</h2>
                    <button onClick={onClose} className="p-2 hover:bg-neutral-100 rounded-lg transition-colors">
                        <svg className="w-5 h-5 text-neutral-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                        </svg>
                    </button>
                </div>

                <div className="p-6 space-y-6">
                    {loading && (
                        <div className="space-y-3">
                            {[...Array(4)].map((_, i) => <div key={i} className="h-4 bg-neutral-100 rounded animate-pulse" />)}
                        </div>
                    )}

                    {error && <p className="text-red-600 text-sm">{error}</p>}

                    {detail && (
                        <>
                            {/* Order + Status */}
                            <div className="flex items-center justify-between">
                                <div>
                                    <p className="text-2xl font-bold text-neutral-900">#{detail.orderNumber}</p>
                                    <p className="text-xs text-neutral-500 mt-0.5">Placed on {formatDate(detail.orderDate)}</p>
                                </div>
                                <div className="flex flex-col items-end gap-1.5">
                                    <StatusBadge status={detail.orderStatus} />
                                    <PaymentBadge status={detail.paymentStatus} />
                                </div>
                            </div>

                            {/* Customer */}
                            <div className="bg-neutral-50 rounded-xl p-4 space-y-1.5">
                                <p className="text-xs font-semibold text-neutral-500 uppercase tracking-wider">Customer</p>
                                <p className="font-bold text-neutral-900">{detail.customerName}</p>
                                <p className="text-sm text-neutral-600">{detail.customerPhone}</p>
                                {detail.customerEmail && <p className="text-sm text-neutral-600">{detail.customerEmail}</p>}
                            </div>

                            {/* Event Info */}
                            <div className="grid grid-cols-2 gap-3">
                                {[
                                    { label: 'Event Date', value: formatDate(detail.eventDate) },
                                    { label: 'Event Time', value: detail.eventTime || '—' },
                                    { label: 'Event Type', value: detail.eventType || '—' },
                                    { label: 'Guests', value: `${detail.guestCount} people` },
                                ].map(item => (
                                    <div key={item.label} className="bg-neutral-50 rounded-xl p-3">
                                        <p className="text-xs text-neutral-500 mb-0.5">{item.label}</p>
                                        <p className="font-semibold text-neutral-900 text-sm">{item.value}</p>
                                    </div>
                                ))}
                            </div>

                            {/* Venue */}
                            {detail.venueAddress && (
                                <div className="bg-neutral-50 rounded-xl p-4">
                                    <p className="text-xs font-semibold text-neutral-500 uppercase tracking-wider mb-1">Venue</p>
                                    <p className="text-sm text-neutral-900">{detail.venueAddress}</p>
                                </div>
                            )}

                            {/* Special Instructions */}
                            {detail.specialInstructions && (
                                <div className="bg-amber-50 border border-amber-200 rounded-xl p-4">
                                    <p className="text-xs font-semibold text-amber-700 uppercase tracking-wider mb-1">Special Instructions</p>
                                    <p className="text-sm text-amber-900">{detail.specialInstructions}</p>
                                </div>
                            )}

                            {/* Menu Items */}
                            {detail.items?.length > 0 && (
                                <div>
                                    <p className="text-xs font-semibold text-neutral-500 uppercase tracking-wider mb-3">Menu Items</p>
                                    <div className="divide-y divide-neutral-100 border border-neutral-200 rounded-xl overflow-hidden">
                                        {detail.items.map((item, i) => (
                                            <div key={i} className="flex items-center justify-between px-4 py-3 bg-white">
                                                <div>
                                                    <p className="text-sm font-semibold text-neutral-900">{item.menuItemName}</p>
                                                    <p className="text-xs text-neutral-500">{item.category} · Qty: {item.quantity}</p>
                                                    {item.specialRequest && <p className="text-xs text-amber-600 mt-0.5">{item.specialRequest}</p>}
                                                </div>
                                                <p className="text-sm font-bold text-neutral-900">₹{item.totalPrice?.toLocaleString('en-IN')}</p>
                                            </div>
                                        ))}
                                    </div>
                                </div>
                            )}

                            {detail.items?.some(item => (parsePackageSelections(item.packageSelections)?.sampleTasteSelections || []).length > 0) && (
                                <div>
                                    <p className="text-xs font-semibold text-neutral-500 uppercase tracking-wider mb-3">Sample Taste Selections</p>
                                    <div className="space-y-3">
                                        {detail.items.map((item) => {
                                            const packageSelections = parsePackageSelections(item.packageSelections);
                                            const sampleSelections = packageSelections?.sampleTasteSelections || [];

                                            if (sampleSelections.length === 0) {
                                                return null;
                                            }

                                            return (
                                                <div key={`sample-${item.orderItemId}`} className="bg-teal-50 border border-teal-100 rounded-xl p-4">
                                                    <p className="text-sm font-semibold text-teal-900 mb-2">{item.menuItemName}</p>
                                                    <div className="space-y-2">
                                                        {sampleSelections.map((category) => (
                                                            <div key={`${item.orderItemId}-${category.categoryId || category.categoryName}`}>
                                                                <p className="text-xs font-medium text-teal-700">{category.categoryName}</p>
                                                                <div className="flex flex-wrap gap-2 mt-1">
                                                                    {(category.selectedItems || []).map((sampleItem) => (
                                                                        <span key={sampleItem.foodItemId || sampleItem.name} className="px-2 py-1 bg-white border border-teal-200 rounded-lg text-xs text-teal-800">
                                                                            {sampleItem.name}
                                                                        </span>
                                                                    ))}
                                                                </div>
                                                            </div>
                                                        ))}
                                                    </div>
                                                </div>
                                            );
                                        })}
                                    </div>
                                </div>
                            )}

                            {/* Financial Summary */}
                            <div className="bg-gradient-to-br from-indigo-50 to-purple-50 rounded-xl p-4 space-y-2">
                                <p className="text-xs font-semibold text-neutral-500 uppercase tracking-wider mb-3">Financial Summary</p>
                                {[
                                    { label: 'Sub Total',    value: detail.subTotal },
                                    { label: 'Tax',          value: detail.taxAmount },
                                    { label: 'Discount',     value: -detail.discountAmount },
                                    { label: 'Delivery',     value: detail.deliveryCharges },
                                ].map(row => row.value !== 0 && (
                                    <div key={row.label} className="flex justify-between text-sm text-neutral-700">
                                        <span>{row.label}</span>
                                        <span className={row.value < 0 ? 'text-green-600' : ''}>
                                            {row.value < 0 ? '-' : ''}₹{Math.abs(row.value)?.toLocaleString('en-IN')}
                                        </span>
                                    </div>
                                ))}
                                <div className="pt-2 border-t border-indigo-200 flex justify-between font-bold text-neutral-900">
                                    <span>Total</span>
                                    <span>₹{detail.totalAmount?.toLocaleString('en-IN')}</span>
                                </div>
                                <div className="flex justify-between text-sm">
                                    <span className="text-green-700">Paid</span>
                                    <span className="text-green-700 font-semibold">₹{detail.paidAmount?.toLocaleString('en-IN')}</span>
                                </div>
                                {detail.balanceAmount > 0 && (
                                    <div className="flex justify-between text-sm">
                                        <span className="text-orange-700">Balance Due</span>
                                        <span className="text-orange-700 font-semibold">₹{detail.balanceAmount?.toLocaleString('en-IN')}</span>
                                    </div>
                                )}
                            </div>

                            {/* Status History */}
                            {detail.statusHistory?.length > 0 && (
                                <div>
                                    <p className="text-xs font-semibold text-neutral-500 uppercase tracking-wider mb-3">Status Timeline</p>
                                    <div className="relative pl-4 space-y-4">
                                        <div className="absolute left-1.5 top-2 bottom-2 w-0.5 bg-neutral-200" />
                                        {detail.statusHistory.map((h, i) => (
                                            <div key={i} className="relative flex gap-3">
                                                <div className="w-3 h-3 rounded-full bg-indigo-600 border-2 border-white ring-2 ring-indigo-200 shrink-0 mt-0.5 -ml-1.5" />
                                                <div>
                                                    <p className="text-sm font-semibold text-neutral-900">{h.status}</p>
                                                    <p className="text-xs text-neutral-500">{formatDate(h.changedDate)}</p>
                                                    {h.comments && <p className="text-xs text-neutral-600 mt-0.5">{h.comments}</p>}
                                                </div>
                                            </div>
                                        ))}
                                    </div>
                                </div>
                            )}
                        </>
                    )}
                </div>
            </div>
        </div>
    );
};

// ─── Manage / Status Update Modal ────────────────────────────────────────────

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
            <div
                className="bg-white rounded-2xl shadow-2xl w-full max-w-md"
                onClick={e => e.stopPropagation()}
            >
                <div className="px-6 py-4 border-b border-neutral-100 flex items-center justify-between">
                    <div>
                        <h2 className="text-lg font-bold text-neutral-900">Manage Order</h2>
                        <p className="text-xs text-neutral-500">#{order.orderNumber} · {order.customerName}</p>
                    </div>
                    <button onClick={onClose} className="p-2 hover:bg-neutral-100 rounded-lg transition-colors">
                        <svg className="w-5 h-5 text-neutral-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                        </svg>
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="p-6 space-y-4">
                    <div>
                        <label className="block text-sm font-semibold text-neutral-700 mb-1.5">Update Status</label>
                        <select
                            value={newStatus}
                            onChange={e => setNewStatus(e.target.value)}
                            className="w-full border border-neutral-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        >
                            {VALID_NEXT_STATUSES.map(s => (
                                <option key={s} value={s}>{s}</option>
                            ))}
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
                            className="w-full border border-neutral-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 resize-none"
                        />
                    </div>

                    {error && <p className="text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{error}</p>}

                    <div className="flex gap-3 pt-2">
                        <button
                            type="button"
                            onClick={onClose}
                            className="flex-1 py-2.5 rounded-xl border-2 border-neutral-200 text-neutral-700 font-semibold text-sm hover:bg-neutral-50 transition-colors"
                        >
                            Cancel
                        </button>
                        <button
                            type="submit"
                            disabled={saving}
                            className="flex-1 py-2.5 rounded-xl bg-gradient-to-r from-indigo-600 to-purple-600 hover:from-indigo-700 hover:to-purple-700 text-white font-semibold text-sm transition-all disabled:opacity-60"
                        >
                            {saving ? 'Saving…' : 'Update Status'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};

// ─── Event Order Card ─────────────────────────────────────────────────────────

const EventOrderCard = ({ order, onViewDetails, onManage }) => {
    const progress = getProgress(order.orderStatus);
    const isCancelled = order.orderStatus === 'Cancelled';
    const isUrgent = order.daysUntilEvent >= 0 && order.daysUntilEvent <= 3 && order.orderStatus === 'Confirmed';

    return (
        <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 hover:shadow-md transition-shadow flex flex-col">
            {/* Header */}
            <div className="p-6 border-b border-neutral-100">
                <div className="flex items-start justify-between mb-3">
                    <div className="flex-1 min-w-0 pr-4">
                        <div className="flex items-center flex-wrap gap-2 mb-2">
                            <h3 className="text-xl font-bold text-neutral-900">#{order.orderNumber}</h3>
                            <StatusBadge status={order.orderStatus} />
                            {isUrgent && (
                                <span className="px-2 py-0.5 rounded-full text-xs font-bold bg-red-100 text-red-700 border border-red-200 animate-pulse">
                                    Urgent
                                </span>
                            )}
                        </div>
                        <p className="text-neutral-600 flex items-center gap-2 text-sm truncate">
                            <svg className="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                            </svg>
                            {order.customerName}
                        </p>
                    </div>
                    <div className="text-right shrink-0">
                        <p className="text-2xl font-bold text-neutral-900">₹{order.totalAmount?.toLocaleString('en-IN')}</p>
                        <p className="text-xs text-neutral-500">Total Amount</p>
                    </div>
                </div>

                {/* Progress Bar */}
                {!isCancelled && (
                    <div className="mt-4">
                        <div className="flex items-center justify-between mb-1.5">
                            <span className="text-xs font-semibold text-neutral-600">Order Progress</span>
                            <span className="text-xs font-bold text-indigo-600">{progress}%</span>
                        </div>
                        <div className="w-full bg-neutral-200 rounded-full h-2">
                            <div
                                className="bg-gradient-to-r from-indigo-600 to-purple-600 h-2 rounded-full transition-all duration-500"
                                style={{ width: `${progress}%` }}
                            />
                        </div>
                    </div>
                )}
            </div>

            {/* Event Details */}
            <div className="p-6 flex-1 flex flex-col">
                <div className="grid grid-cols-2 gap-4 mb-4">
                    {/* Event Date */}
                    <div className="flex items-start gap-3">
                        <div className="p-2 bg-indigo-100 rounded-lg shrink-0">
                            <svg className="w-5 h-5 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                            </svg>
                        </div>
                        <div className="min-w-0">
                            <p className="text-xs text-neutral-500 font-medium mb-0.5">Event Date</p>
                            <p className="text-sm font-bold text-neutral-900">{formatDate(order.eventDate)}</p>
                        </div>
                    </div>

                    {/* Event Time */}
                    <div className="flex items-start gap-3">
                        <div className="p-2 bg-purple-100 rounded-lg shrink-0">
                            <svg className="w-5 h-5 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                            </svg>
                        </div>
                        <div className="min-w-0">
                            <p className="text-xs text-neutral-500 font-medium mb-0.5">Event Time</p>
                            <p className="text-sm font-bold text-neutral-900">{order.eventTime || '—'}</p>
                        </div>
                    </div>

                    {/* Guests */}
                    <div className="flex items-start gap-3">
                        <div className="p-2 bg-green-100 rounded-lg shrink-0">
                            <svg className="w-5 h-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z" />
                            </svg>
                        </div>
                        <div className="min-w-0">
                            <p className="text-xs text-neutral-500 font-medium mb-0.5">Guests</p>
                            <p className="text-sm font-bold text-neutral-900">{order.guestCount} people</p>
                        </div>
                    </div>

                    {/* Venue */}
                    <div className="flex items-start gap-3">
                        <div className="p-2 bg-orange-100 rounded-lg shrink-0">
                            <svg className="w-5 h-5 text-orange-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                            </svg>
                        </div>
                        <div className="min-w-0">
                            <p className="text-xs text-neutral-500 font-medium mb-0.5">Venue</p>
                            <p className="text-sm font-bold text-neutral-900 truncate">{order.venueAddress || '—'}</p>
                        </div>
                    </div>
                </div>

                {/* Event Type */}
                <div className="mb-4 p-3 bg-neutral-50 rounded-xl">
                    <p className="text-xs text-neutral-500 font-medium mb-0.5">Event Type</p>
                    <p className="text-sm font-bold text-neutral-900">{order.eventType || '—'}</p>
                </div>

                {/* Menu Items */}
                {order.menuItems?.length > 0 && (
                    <div className="mb-4">
                        <p className="text-xs text-neutral-500 font-semibold mb-2">Menu Items</p>
                        <div className="flex flex-wrap gap-2">
                            {order.menuItems.map((item, i) => (
                                <span key={i} className="px-3 py-1 bg-indigo-50 text-indigo-700 rounded-lg text-xs font-medium">
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
                        className="flex-1 flex items-center justify-center gap-2 bg-gradient-to-r from-indigo-600 to-purple-600 hover:from-indigo-700 hover:to-purple-700 text-white px-4 py-3 rounded-xl font-semibold text-sm transition-all"
                    >
                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                        </svg>
                        View Details
                    </button>
                    {!isCancelled && order.orderStatus !== 'Completed' && (
                        <button
                            onClick={() => onManage(order)}
                            className="flex-1 flex items-center justify-center gap-2 bg-white hover:bg-neutral-50 text-neutral-700 border-2 border-neutral-200 px-4 py-3 rounded-xl font-semibold text-sm transition-all"
                        >
                            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                            </svg>
                            Manage
                        </button>
                    )}
                </div>
            </div>

            {/* Footer */}
            <div className="px-6 py-3 bg-neutral-50 rounded-b-2xl border-t border-neutral-100 flex items-center justify-between">
                <p className="text-xs text-neutral-500">Confirmed on {formatDate(order.orderDate)}</p>
                <div className="flex items-center gap-2">
                    <PaymentBadge status={order.paymentStatus} />
                    {order.balanceAmount > 0 && (
                        <span className="text-xs text-orange-600 font-medium">
                            ₹{order.balanceAmount?.toLocaleString('en-IN')} due
                        </span>
                    )}
                </div>
            </div>
        </div>
    );
};

// ─── Main Component ───────────────────────────────────────────────────────────

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

    // Debounce search
    const searchTimer = useRef(null);
    const [debouncedSearch, setDebouncedSearch] = useState('');
    useEffect(() => {
        clearTimeout(searchTimer.current);
        searchTimer.current = setTimeout(() => setDebouncedSearch(searchQuery), 400);
        return () => clearTimeout(searchTimer.current);
    }, [searchQuery]);

    // Reset page on tab/search change
    useEffect(() => { setPage(1); }, [activeTab, debouncedSearch]);

    // Load stats once on mount
    useEffect(() => {
        ownerApiService.getOrderStats()
            .then(res => setStats(res?.data ?? res))
            .catch(() => { /* stats are supplementary */ });
    }, []);

    const buildFilter = useCallback(() => {
        const f = { SearchTerm: debouncedSearch || undefined };
        if (TAB_STATUS_FILTER[activeTab]) {
            f.OrderStatus = TAB_STATUS_FILTER[activeTab];
        } else {
            // "all" tab: exclude Pending (those belong in Booking Requests)
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
        { id: 'all',          label: 'All Orders',  count: stats ? (stats.confirmedOrders + stats.completedOrders + stats.cancelledOrders) : null },
        { id: 'upcoming',     label: 'Upcoming',    count: stats?.confirmedOrders ?? null },
        { id: 'in-progress',  label: 'In Progress', count: null },
        { id: 'completed',    label: 'Completed',   count: stats?.completedOrders ?? null },
    ];

    const handleManageSuccess = () => {
        setManageOrder(null);
        fetchOrders();
    };

    return (
        <div className="min-h-screen bg-neutral-50">
            <div className="p-4 sm:p-6 lg:p-8 space-y-6">

                {/* Header */}
                <div className="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-4">
                    <div>
                        <h1 className="text-3xl font-bold text-neutral-900">Event Orders</h1>
                        <p className="text-neutral-600 mt-1">
                            Track and manage your confirmed event bookings
                            {totalCount > 0 && <span className="ml-2 text-indigo-600 font-semibold">({totalCount} orders)</span>}
                        </p>
                    </div>

                    {/* Search */}
                    <div className="w-full lg:w-96">
                        <div className="relative">
                            <input
                                type="text"
                                placeholder="Search by customer or order number…"
                                value={searchQuery}
                                onChange={e => setSearchQuery(e.target.value)}
                                className="w-full pl-11 pr-4 py-3 bg-white border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent text-sm"
                            />
                            <svg className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                            </svg>
                        </div>
                    </div>
                </div>

                {/* Tabs */}
                <div className="flex flex-wrap gap-3">
                    {tabs.map(tab => (
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
                            {tab.count !== null && (
                                <span className={`ml-2 px-2 py-0.5 rounded-full text-xs font-bold ${
                                    activeTab === tab.id ? 'bg-white/20 text-white' : 'bg-neutral-100 text-neutral-600'
                                }`}>
                                    {tab.count}
                                </span>
                            )}
                        </button>
                    ))}
                </div>

                {/* Error */}
                {error && (
                    <div className="flex items-center gap-3 bg-red-50 border border-red-200 text-red-700 rounded-xl px-4 py-3">
                        <svg className="w-5 h-5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                        </svg>
                        <span className="text-sm font-medium">{error}</span>
                        <button onClick={fetchOrders} className="ml-auto text-xs underline font-semibold">Retry</button>
                    </div>
                )}

                {/* Orders Grid */}
                {loading ? (
                    <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
                        <SkeletonCard /><SkeletonCard /><SkeletonCard /><SkeletonCard />
                    </div>
                ) : orders.length > 0 ? (
                    <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
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
                    <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 p-12">
                        <div className="text-center">
                            <svg className="w-20 h-20 mx-auto mb-4 text-neutral-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                            </svg>
                            <h3 className="text-xl font-semibold text-neutral-900 mb-2">No Event Orders</h3>
                            <p className="text-neutral-600">
                                {debouncedSearch
                                    ? 'No orders match your search criteria.'
                                    : activeTab !== 'all'
                                    ? `No ${tabs.find(t => t.id === activeTab)?.label.toLowerCase()} orders at the moment.`
                                    : 'No confirmed events to display.'}
                            </p>
                        </div>
                    </div>
                ) : null}

                {/* Pagination */}
                {!loading && totalPages > 1 && (
                    <div className="flex items-center justify-between bg-white border border-neutral-200 rounded-xl px-4 py-3">
                        <p className="text-sm text-neutral-600">
                            Page <span className="font-semibold text-neutral-900">{page}</span> of{' '}
                            <span className="font-semibold text-neutral-900">{totalPages}</span>
                            <span className="text-neutral-400 ml-2">({totalCount} total)</span>
                        </p>
                        <div className="flex gap-2">
                            <button
                                disabled={page <= 1}
                                onClick={() => setPage(p => p - 1)}
                                className="flex items-center gap-1.5 px-3 py-2 rounded-lg border border-neutral-200 text-sm font-medium text-neutral-700 hover:bg-neutral-50 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
                            >
                                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                                </svg>
                                Previous
                            </button>
                            <button
                                disabled={page >= totalPages}
                                onClick={() => setPage(p => p + 1)}
                                className="flex items-center gap-1.5 px-3 py-2 rounded-lg border border-neutral-200 text-sm font-medium text-neutral-700 hover:bg-neutral-50 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
                            >
                                Next
                                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                                </svg>
                            </button>
                        </div>
                    </div>
                )}
            </div>
                
            {/* Modals */}
            {detailOrderId && (
                <DetailModal orderId={detailOrderId} onClose={() => setDetailOrderId(null)} />
            )}
            {manageOrder && (
                <ManageModal order={manageOrder} onClose={() => setManageOrder(null)} onSuccess={handleManageSuccess} />
            )}
        </div>
    );
}
