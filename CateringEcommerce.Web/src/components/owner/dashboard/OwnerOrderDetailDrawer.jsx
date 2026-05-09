import React, { useEffect, useMemo, useState } from 'react';
import { X } from 'lucide-react';
import { ownerApiService } from '../../../services/ownerApi';
import { Skeleton } from '../../../design-system/components';

const statusPillClass = (s) => ({
    Pending: 's-pending', Confirmed: 's-confirmed', Cancelled: 's-cancelled',
    Completed: 's-completed', InProgress: 's-inprogress', 'In-Progress': 's-inprogress',
}[s] || 's-pending');

const paymentPillClass = (s) => ({
    Paid: 's-completed', Pending: 's-pending', Partial: 's-inprogress', Failed: 's-cancelled',
}[s] || 's-pending');

function formatDate(value, includeTime = false) {
    if (!value) return '-';
    const options = includeTime
        ? { day: 'numeric', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' }
        : { day: 'numeric', month: 'short', year: 'numeric' };
    return new Date(value).toLocaleDateString('en-IN', options);
}

function formatCurrency(value) {
    return `Rs ${Number(value || 0).toLocaleString('en-IN')}`;
}

function parsePackageSelections(value) {
    if (!value) return null;
    try {
        return typeof value === 'string' ? JSON.parse(value) : value;
    } catch {
        return null;
    }
}

function normalizeSelectedGroups(packageSelections) {
    const selections = Array.isArray(packageSelections?.selections) ? packageSelections.selections : [];
    return selections
        .map((group) => {
            const items = Array.isArray(group?.selectedItems) ? group.selectedItems : [];
            const labels = items.map((item) => item?.foodName || item?.name || item?.itemName).filter(Boolean);
            if (labels.length === 0) return null;
            return { label: group?.categoryName || group?.selectionName || 'Selected Items', items: labels };
        })
        .filter(Boolean);
}

function normalizeSampleGroups(packageSelections) {
    const sampleSelections = Array.isArray(packageSelections?.sampleTasteSelections) ? packageSelections.sampleTasteSelections : [];
    return sampleSelections
        .map((group) => {
            const items = Array.isArray(group?.selectedItems) ? group.selectedItems : [];
            const labels = items.map((item) => item?.name || item?.foodName || item?.itemName).filter(Boolean);
            if (labels.length === 0) return null;
            return { label: group?.categoryName || 'Sample Taste', items: labels };
        })
        .filter(Boolean);
}

function SectionCard({ title, children, tone = 'default' }) {
    const toneClass = tone === 'warning'
        ? 'bg-amber-50 border-amber-200'
        : tone === 'highlight'
            ? 'bg-orange-50 border-orange-100'
            : 'bg-white border-neutral-200';
    return (
        <div className={`rounded-2xl border p-4 ${toneClass}`}>
            <p className="text-xs font-semibold uppercase tracking-wider text-neutral-500 mb-3">{title}</p>
            {children}
        </div>
    );
}

function KeyValueGrid({ items }) {
    return (
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
            {items.map((item) => (
                <div key={item.label} className="rounded-xl bg-neutral-50 p-3">
                    <p className="text-xs text-neutral-500 mb-1">{item.label}</p>
                    <p className="text-sm font-semibold text-neutral-900 break-words">{item.value || '-'}</p>
                </div>
            ))}
        </div>
    );
}

export default function OwnerOrderDetailDrawer({ orderId, isOpen, onClose }) {
    const [detail, setDetail] = useState(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);

    useEffect(() => {
        if (!isOpen || !orderId) return;
        let ignore = false;
        const loadDetails = async () => {
            setLoading(true);
            setError(null);
            setDetail(null);
            try {
                const response = await ownerApiService.getOrderDetails(orderId);
                if (!ignore) setDetail(response?.data ?? response);
            } catch {
                if (!ignore) { setError('Failed to load order details.'); setDetail(null); }
            } finally {
                if (!ignore) setLoading(false);
            }
        };
        loadDetails();
        return () => { ignore = true; };
    }, [isOpen, orderId]);

    const items = useMemo(() => detail?.items ?? [], [detail]);

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 z-50">
            <div className="absolute inset-0 bg-black/50" onClick={onClose} />
            <aside className="absolute inset-y-0 right-0 w-full max-w-3xl bg-white shadow-2xl overflow-y-auto">
                {/* Sticky header */}
                <div className="sticky top-0 z-10 bg-white border-b border-neutral-100 px-6 py-4 flex items-center justify-between">
                    <div>
                        <h2 className="text-xl font-bold text-neutral-900">Order Details</h2>
                        {detail?.orderNumber && (
                            <p className="text-sm text-neutral-500 mt-0.5">#{detail.orderNumber}</p>
                        )}
                    </div>
                    <button onClick={onClose} className="icon-btn" aria-label="Close order details">
                        <X size={18} strokeWidth={2} />
                    </button>
                </div>

                <div className="p-6 space-y-5">
                    {loading && (
                        <div className="space-y-4">
                            {[...Array(5)].map((_, i) => <Skeleton key={i} className="h-20 w-full rounded-2xl" />)}
                        </div>
                    )}

                    {error && (
                        <div className="rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                            {error}
                        </div>
                    )}

                    {!loading && !error && detail && (
                        <>
                            <SectionCard title="Overview" tone="highlight">
                                <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
                                    <div>
                                        <p className="text-2xl font-bold text-neutral-900">#{detail.orderNumber}</p>
                                        <p className="text-sm text-neutral-500 mt-1">
                                            Placed on {formatDate(detail.orderDate, true)}
                                        </p>
                                    </div>
                                    <div className="flex flex-wrap gap-2">
                                        <span className={`status-pill ${statusPillClass(detail.orderStatus)}`}>
                                            <span className="dot" />
                                            {detail.orderStatus || 'Pending'}
                                        </span>
                                        <span className={`status-pill ${paymentPillClass(detail.paymentStatus)}`}>
                                            <span className="dot" />
                                            {detail.paymentStatus || 'Pending'}
                                        </span>
                                    </div>
                                </div>
                            </SectionCard>

                            <SectionCard title="Customer">
                                <KeyValueGrid items={[
                                    { label: 'Customer Name', value: detail.customerName },
                                    { label: 'Customer Phone', value: detail.customerPhone },
                                    { label: 'Customer Email', value: detail.customerEmail || '-' },
                                ]} />
                            </SectionCard>

                            <SectionCard title="Event">
                                <KeyValueGrid items={[
                                    { label: 'Event Date', value: formatDate(detail.eventDate) },
                                    { label: 'Event Time', value: detail.eventTime || '-' },
                                    { label: 'Event Type', value: detail.eventType || '-' },
                                    { label: 'Guest Count', value: detail.guestCount ? `${detail.guestCount} people` : '-' },
                                    { label: 'Event Location', value: detail.eventLocation || '-' },
                                    { label: 'Venue / Address', value: detail.deliveryAddress || detail.venueAddress || '-' },
                                ]} />
                            </SectionCard>

                            <SectionCard title="Contact">
                                <KeyValueGrid items={[
                                    { label: 'Contact Person', value: detail.contactPerson || '-' },
                                    { label: 'Contact Phone', value: detail.contactPhone || '-' },
                                    { label: 'Contact Email', value: detail.contactEmail || '-' },
                                ]} />
                            </SectionCard>

                            {(detail.specialInstructions || items.some((item) => parsePackageSelections(item.packageSelections))) && (
                                <SectionCard title="Requirements" tone="warning">
                                    <div className="space-y-4">
                                        {detail.specialInstructions && (
                                            <div>
                                                <p className="text-sm font-semibold text-amber-900 mb-1">Special Instructions</p>
                                                <p className="text-sm text-amber-800 whitespace-pre-wrap">{detail.specialInstructions}</p>
                                            </div>
                                        )}
                                        {items.map((item) => {
                                            const packageSelections = parsePackageSelections(item.packageSelections);
                                            const selectedGroups = normalizeSelectedGroups(packageSelections);
                                            const sampleGroups = normalizeSampleGroups(packageSelections);
                                            const sampleMeta = packageSelections?.sampleTasteMeta || null;
                                            if (selectedGroups.length === 0 && sampleGroups.length === 0 && !sampleMeta) return null;
                                            return (
                                                <div key={`requirement-${item.orderItemId}`} className="rounded-xl border border-amber-200 bg-white/70 p-4">
                                                    <p className="text-sm font-semibold text-neutral-900 mb-3">{item.menuItemName}</p>
                                                    {selectedGroups.length > 0 && (
                                                        <div className="space-y-2 mb-3">
                                                            <p className="text-xs font-semibold uppercase tracking-wider text-neutral-500">Selected Package Items</p>
                                                            {selectedGroups.map((group) => (
                                                                <div key={`${item.orderItemId}-${group.label}`}>
                                                                    <p className="text-xs font-medium text-neutral-700 mb-1">{group.label}</p>
                                                                    <div className="flex flex-wrap gap-2">
                                                                        {group.items.map((name) => (
                                                                            <span key={`${group.label}-${name}`}
                                                                                className="px-2 py-1 rounded-lg text-xs font-medium border"
                                                                                style={{ background: 'rgba(255,107,53,0.06)', borderColor: 'rgba(255,107,53,0.2)', color: 'var(--color-primary)' }}>
                                                                                {name}
                                                                            </span>
                                                                        ))}
                                                                    </div>
                                                                </div>
                                                            ))}
                                                        </div>
                                                    )}
                                                    {sampleGroups.length > 0 && (
                                                        <div className="space-y-2 mb-3">
                                                            <p className="text-xs font-semibold uppercase tracking-wider text-neutral-500">Sample Taste Selections</p>
                                                            {sampleGroups.map((group) => (
                                                                <div key={`${item.orderItemId}-sample-${group.label}`}>
                                                                    <p className="text-xs font-medium text-neutral-700 mb-1">{group.label}</p>
                                                                    <div className="flex flex-wrap gap-2">
                                                                        {group.items.map((name) => (
                                                                            <span key={`${group.label}-${name}`} className="px-2 py-1 rounded-lg bg-teal-50 text-teal-700 text-xs font-medium border border-teal-100">
                                                                                {name}
                                                                            </span>
                                                                        ))}
                                                                    </div>
                                                                </div>
                                                            ))}
                                                        </div>
                                                    )}
                                                    {sampleMeta && (
                                                        <div className="text-xs text-neutral-600 space-y-1">
                                                            {sampleMeta.status && <p>Sample status: {sampleMeta.status}</p>}
                                                            {sampleMeta.rejectionReason && <p>Sample note: {sampleMeta.rejectionReason}</p>}
                                                        </div>
                                                    )}
                                                </div>
                                            );
                                        })}
                                    </div>
                                </SectionCard>
                            )}

                            <SectionCard title="Ordered Items">
                                {items.length > 0 ? (
                                    <div className="space-y-3">
                                        {items.map((item) => (
                                            <div key={item.orderItemId} className="rounded-xl border border-neutral-200 bg-neutral-50 p-4">
                                                <div className="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-3">
                                                    <div>
                                                        <div className="flex items-center gap-2 flex-wrap">
                                                            <p className="text-sm font-semibold text-neutral-900">{item.menuItemName}</p>
                                                            <span className="px-2 py-0.5 rounded-full bg-neutral-200 text-neutral-600 text-xs font-medium">
                                                                {item.itemType || item.category || 'Item'}
                                                            </span>
                                                        </div>
                                                        <p className="text-xs text-neutral-500 mt-1">
                                                            {item.category || item.itemType || 'Item'} · Qty: {item.quantity}
                                                        </p>
                                                    </div>
                                                    <div className="text-left sm:text-right">
                                                        <p className="text-sm text-neutral-500">Unit: {formatCurrency(item.unitPrice)}</p>
                                                        <p className="text-sm font-bold text-neutral-900">Total: {formatCurrency(item.totalPrice)}</p>
                                                    </div>
                                                </div>
                                            </div>
                                        ))}
                                    </div>
                                ) : (
                                    <p className="text-sm text-neutral-500">No order items were found for this booking.</p>
                                )}
                            </SectionCard>

                            <SectionCard title="Payment Summary" tone="highlight">
                                <div className="space-y-2">
                                    <div className="flex justify-between text-sm text-neutral-700">
                                        <span>Base Amount</span>
                                        <span>{formatCurrency(detail.subTotal)}</span>
                                    </div>
                                    <div className="flex justify-between text-sm text-neutral-700">
                                        <span>Tax</span>
                                        <span>{formatCurrency(detail.taxAmount)}</span>
                                    </div>
                                    <div className="flex justify-between text-sm text-neutral-700">
                                        <span>Discount</span>
                                        <span className="text-green-700">- {formatCurrency(detail.discountAmount)}</span>
                                    </div>
                                    <div className="flex justify-between text-sm text-neutral-700">
                                        <span>Delivery</span>
                                        <span>{formatCurrency(detail.deliveryCharges)}</span>
                                    </div>
                                    <div className="pt-2 border-t border-neutral-200 flex justify-between font-bold text-neutral-900">
                                        <span>Total</span>
                                        <span className="amount">{formatCurrency(detail.totalAmount)}</span>
                                    </div>
                                    <div className="flex justify-between text-sm text-green-700">
                                        <span>Paid</span>
                                        <span className="font-semibold">{formatCurrency(detail.paidAmount)}</span>
                                    </div>
                                    <div className="flex justify-between text-sm text-orange-700">
                                        <span>Balance</span>
                                        <span className="font-semibold">{formatCurrency(detail.balanceAmount)}</span>
                                    </div>
                                    {detail.paymentSplitEnabled && (
                                        <div className="mt-4 rounded-xl bg-white/80 border border-neutral-100 p-4 space-y-2">
                                            <p className="text-xs font-semibold uppercase tracking-wider text-neutral-500">Split Payment</p>
                                            {[
                                                { label: 'Payment Method', value: detail.paymentMethod },
                                                { label: 'Pre-booking Amount', value: detail.preBookingAmount != null ? formatCurrency(detail.preBookingAmount) : null },
                                                { label: 'Pre-booking Status', value: detail.preBookingStatus },
                                                { label: 'Post-event Amount', value: detail.postEventAmount != null ? formatCurrency(detail.postEventAmount) : null },
                                                { label: 'Post-event Status', value: detail.postEventStatus },
                                            ].map(row => (
                                                <div key={row.label} className="flex justify-between text-sm text-neutral-700">
                                                    <span>{row.label}</span>
                                                    <span>{row.value || '-'}</span>
                                                </div>
                                            ))}
                                        </div>
                                    )}
                                </div>
                            </SectionCard>

                            <SectionCard title="Status Timeline">
                                {detail.statusHistory?.length > 0 ? (
                                    <div className="relative pl-4 space-y-4">
                                        <div className="absolute left-1.5 top-2 bottom-2 w-0.5 bg-neutral-200" />
                                        {detail.statusHistory.map((entry, index) => (
                                            <div key={`${entry.status}-${entry.changedDate}-${index}`} className="relative flex gap-3">
                                                <div className="w-3 h-3 rounded-full border-2 border-white shrink-0 mt-0.5 -ml-1.5"
                                                    style={{ background: 'var(--color-primary)', boxShadow: '0 0 0 2px rgba(255,107,53,0.25)' }} />
                                                <div>
                                                    <p className="text-sm font-semibold text-neutral-900">{entry.status}</p>
                                                    <p className="text-xs text-neutral-500">{formatDate(entry.changedDate, true)}</p>
                                                    {entry.comments && <p className="text-xs text-neutral-600 mt-1">{entry.comments}</p>}
                                                </div>
                                            </div>
                                        ))}
                                    </div>
                                ) : (
                                    <p className="text-sm text-neutral-500">No status history is available for this order yet.</p>
                                )}
                            </SectionCard>
                        </>
                    )}
                </div>
            </aside>
        </div>
    );
}
