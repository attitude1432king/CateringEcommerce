/*
========================================
File: src/components/owner/dashboard/Support.jsx
Support & Help Center - Live API Data
========================================
*/
import { useState, useEffect, useCallback } from 'react';
import { ownerApiService } from '../../../services/ownerApi';

// ===================================
// Status & Priority Helpers
// ===================================
const statusConfig = {
    Open: { bg: 'bg-yellow-100 text-yellow-800 border-yellow-200', label: 'Open' },
    InProgress: { bg: 'bg-blue-100 text-blue-800 border-blue-200', label: 'In Progress' },
    Resolved: { bg: 'bg-green-100 text-green-800 border-green-200', label: 'Resolved' },
    Closed: { bg: 'bg-neutral-100 text-neutral-800 border-neutral-200', label: 'Closed' },
};

const priorityConfig = {
    Low: { color: 'text-green-600', icon: '●' },
    Medium: { color: 'text-orange-500', icon: '●' },
    High: { color: 'text-red-500', icon: '●' },
    Urgent: { color: 'text-red-700', icon: '◉' },
};

const categories = [
    'Payment Issues',
    'Orders & Bookings',
    'Account & Settings',
    'Technical Issue',
    'Other'
];

const formatDate = (dateStr) => {
    if (!dateStr) return '';
    const d = new Date(dateStr);
    return d.toLocaleDateString('en-IN', { year: 'numeric', month: 'short', day: 'numeric' });
};

const formatDateTime = (dateStr) => {
    if (!dateStr) return '';
    const d = new Date(dateStr);
    return d.toLocaleDateString('en-IN', { year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' });
};

// ===================================
// Stat Card
// ===================================
const StatCard = ({ label, value, color }) => {
    const colorMap = {
        yellow: 'bg-yellow-50 text-yellow-700 border-yellow-200',
        blue: 'bg-blue-50 text-blue-700 border-blue-200',
        green: 'bg-green-50 text-green-700 border-green-200',
        neutral: 'bg-neutral-50 text-neutral-700 border-neutral-200',
    };
    return (
        <div className={`rounded-xl border p-4 text-center ${colorMap[color] || colorMap.neutral}`}>
            <p className="text-3xl font-bold">{value}</p>
            <p className="text-xs font-medium mt-1">{label}</p>
        </div>
    );
};

// ===================================
// Ticket Card
// ===================================
const TicketCard = ({ ticket, onView }) => {
    const sc = statusConfig[ticket.status] || statusConfig.Open;
    const pc = priorityConfig[ticket.priority] || priorityConfig.Medium;

    return (
        <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 p-6 hover:shadow-md transition-shadow">
            <div className="flex items-start justify-between mb-3">
                <div className="flex items-center gap-3 flex-wrap">
                    <h3 className="text-sm font-bold text-neutral-500">{ticket.ticketNumber}</h3>
                    <span className={`px-3 py-1 rounded-full text-xs font-semibold border ${sc.bg}`}>
                        {sc.label}
                    </span>
                    <span className={`flex items-center gap-1 text-xs font-semibold ${pc.color}`}>
                        {pc.icon} {ticket.priority}
                    </span>
                </div>
                {ticket.messageCount > 0 && (
                    <span className="flex items-center gap-1 text-xs text-neutral-500 flex-shrink-0">
                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
                        </svg>
                        {ticket.messageCount}
                    </span>
                )}
            </div>

            <p className="text-xs font-medium mb-2" style={{ color: 'var(--color-primary)' }}>{ticket.category}</p>
            <h4 className="font-semibold text-neutral-900 mb-2">{ticket.subject}</h4>
            <p className="text-neutral-600 text-sm mb-4 line-clamp-2">{ticket.description}</p>

            <div className="flex items-center justify-between text-sm">
                <span className="text-neutral-500">{formatDate(ticket.createdDate)}</span>
                <button
                    onClick={() => onView(ticket.ticketId)}
                    className="font-semibold"
                    style={{ color: 'var(--color-primary)' }}
                >
                    View Details →
                </button>
            </div>
        </div>
    );
};

// ===================================
// Create Ticket Modal
// ===================================
const CreateTicketModal = ({ isOpen, onClose, onSubmit, submitting }) => {
    const [form, setForm] = useState({
        subject: '',
        description: '',
        category: '',
        priority: 'Medium',
    });

    if (!isOpen) return null;

    const handleSubmit = (e) => {
        e.preventDefault();
        if (!form.subject.trim() || !form.description.trim() || !form.category) return;
        onSubmit(form);
    };

    return (
        <div className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4">
            <div className="bg-white rounded-2xl shadow-xl max-w-lg w-full max-h-[90vh] overflow-y-auto">
                <div className="p-6 border-b border-neutral-200">
                    <div className="flex items-center justify-between">
                        <h2 className="text-xl font-bold text-neutral-900">Create Support Ticket</h2>
                        <button onClick={onClose} className="text-neutral-400 hover:text-neutral-600">
                            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                            </svg>
                        </button>
                    </div>
                </div>

                <form onSubmit={handleSubmit} className="p-6 space-y-4">
                    {/* Category */}
                    <div>
                        <label className="block text-sm font-semibold text-neutral-700 mb-1">Category *</label>
                        <select
                            value={form.category}
                            onChange={(e) => setForm(f => ({ ...f, category: e.target.value }))}
                            className="w-full px-4 py-2.5 border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-orange-400"
                            required
                        >
                            <option value="">Select category...</option>
                            {categories.map(c => (
                                <option key={c} value={c}>{c}</option>
                            ))}
                        </select>
                    </div>

                    {/* Priority */}
                    <div>
                        <label className="block text-sm font-semibold text-neutral-700 mb-1">Priority</label>
                        <div className="flex gap-2">
                            {['Low', 'Medium', 'High', 'Urgent'].map(p => (
                                <button
                                    key={p}
                                    type="button"
                                    onClick={() => setForm(f => ({ ...f, priority: p }))}
                                    className={`flex-1 py-2 rounded-xl text-sm font-semibold transition-all border ${
                                        form.priority === p
                                            ? 'text-white border-transparent'
                                            : 'bg-white text-neutral-600 border-neutral-200 hover:bg-neutral-50'
                                    }`}
                                    style={form.priority === p ? { background: 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)' } : undefined}
                                >
                                    {p}
                                </button>
                            ))}
                        </div>
                    </div>

                    {/* Subject */}
                    <div>
                        <label className="block text-sm font-semibold text-neutral-700 mb-1">Subject *</label>
                        <input
                            type="text"
                            value={form.subject}
                            onChange={(e) => setForm(f => ({ ...f, subject: e.target.value }))}
                            placeholder="Brief description of your issue"
                            className="w-full px-4 py-2.5 border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-orange-400"
                            maxLength={200}
                            required
                        />
                    </div>

                    {/* Description */}
                    <div>
                        <label className="block text-sm font-semibold text-neutral-700 mb-1">Description *</label>
                        <textarea
                            value={form.description}
                            onChange={(e) => setForm(f => ({ ...f, description: e.target.value }))}
                            placeholder="Provide details about your issue..."
                            className="w-full px-4 py-3 border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-orange-400 resize-none"
                            rows={5}
                            maxLength={2000}
                            required
                        />
                        <p className="text-xs text-neutral-400 mt-1">{form.description.length}/2000</p>
                    </div>

                    {/* Actions */}
                    <div className="flex gap-3 pt-2">
                        <button
                            type="submit"
                            disabled={submitting || !form.subject.trim() || !form.description.trim() || !form.category}
                            className="flex-1 text-white py-3 rounded-xl font-semibold transition-all disabled:opacity-50 disabled:cursor-not-allowed shadow-lg hover:shadow-xl"
                            style={{ background: 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)' }}
                        >
                            {submitting ? 'Creating...' : 'Create Ticket'}
                        </button>
                        <button
                            type="button"
                            onClick={onClose}
                            disabled={submitting}
                            className="px-6 py-3 bg-neutral-200 hover:bg-neutral-300 text-neutral-700 rounded-xl font-semibold transition-colors"
                        >
                            Cancel
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};

// ===================================
// Ticket Detail View
// ===================================
const TicketDetailView = ({ ticketId, onBack }) => {
    const [detail, setDetail] = useState(null);
    const [loading, setLoading] = useState(true);
    const [messageText, setMessageText] = useState('');
    const [sending, setSending] = useState(false);

    const fetchDetail = useCallback(async () => {
        setLoading(true);
        try {
            const response = await ownerApiService.getSupportTicketDetail(ticketId);
            if (response?.result && response.data) {
                setDetail(response.data);
            }
        } catch (err) {
            console.error('Error fetching ticket detail:', err);
        } finally {
            setLoading(false);
        }
    }, [ticketId]);

    useEffect(() => {
        fetchDetail();
    }, [fetchDetail]);

    const handleSendMessage = async () => {
        if (!messageText.trim() || sending) return;
        setSending(true);
        try {
            const response = await ownerApiService.sendTicketMessage(ticketId, messageText);
            if (response?.result && response.data) {
                setDetail(prev => ({
                    ...prev,
                    messages: [...(prev.messages || []), response.data]
                }));
                setMessageText('');
            } else {
                alert(response?.message || 'Failed to send message.');
            }
        } catch (err) {
            console.error('Error sending message:', err);
            alert('Failed to send message.');
        } finally {
            setSending(false);
        }
    };

    const BackButton = () => (
        <button onClick={onBack} className="flex items-center gap-2 font-semibold" style={{ color: 'var(--color-primary)' }}>
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
            </svg>
            Back to Tickets
        </button>
    );

    if (loading) {
        return (
            <div className="space-y-4">
                <BackButton />
                <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 p-8 animate-pulse">
                    <div className="h-6 bg-neutral-200 rounded w-48 mb-4" />
                    <div className="h-4 bg-neutral-200 rounded w-full mb-2" />
                    <div className="h-4 bg-neutral-200 rounded w-3/4" />
                </div>
            </div>
        );
    }

    if (!detail) {
        return (
            <div className="space-y-4">
                <BackButton />
                <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 p-12 text-center">
                    <p className="text-neutral-600">Ticket not found.</p>
                </div>
            </div>
        );
    }

    const sc = statusConfig[detail.status] || statusConfig.Open;
    const pc = priorityConfig[detail.priority] || priorityConfig.Medium;
    const isClosed = detail.status === 'Closed' || detail.status === 'Resolved';

    return (
        <div className="space-y-4">
            <BackButton />

            {/* Ticket Header */}
            <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 p-6">
                <div className="flex flex-wrap items-center gap-3 mb-4">
                    <h2 className="text-lg font-bold text-neutral-500">{detail.ticketNumber}</h2>
                    <span className={`px-3 py-1 rounded-full text-xs font-semibold border ${sc.bg}`}>{sc.label}</span>
                    <span className={`flex items-center gap-1 text-xs font-semibold ${pc.color}`}>{pc.icon} {detail.priority}</span>
                    <span className="text-xs text-neutral-400 ml-auto">{formatDateTime(detail.createdDate)}</span>
                </div>
                <span className="inline-block px-3 py-1 rounded-lg text-xs font-semibold mb-3" style={{ background: 'rgba(255,107,53,0.08)', color: 'var(--color-primary)' }}>{detail.category}</span>
                <h3 className="text-xl font-bold text-neutral-900 mb-2">{detail.subject}</h3>
                <p className="text-neutral-700 leading-relaxed">{detail.description}</p>

                {detail.resolutionNotes && (
                    <div className="mt-4 p-4 bg-green-50 rounded-xl border-l-4 border-green-500">
                        <p className="text-sm font-semibold text-green-800 mb-1">Resolution</p>
                        <p className="text-sm text-green-700">{detail.resolutionNotes}</p>
                        {detail.resolvedDate && (
                            <p className="text-xs text-green-500 mt-1">Resolved on {formatDate(detail.resolvedDate)}</p>
                        )}
                    </div>
                )}
            </div>

            {/* Messages */}
            <div className="bg-white rounded-2xl shadow-sm border border-neutral-200">
                <div className="p-4 border-b border-neutral-200">
                    <h3 className="font-bold text-neutral-900">Conversation ({detail.messages?.length || 0})</h3>
                </div>

                <div className="p-4 space-y-4 max-h-[400px] overflow-y-auto">
                    {detail.messages && detail.messages.length > 0 ? (
                        detail.messages.map((msg) => (
                            <div
                                key={msg.messageId}
                                className={`flex ${msg.senderType === 'Owner' ? 'justify-end' : 'justify-start'}`}
                            >
                                <div
                                    className={`max-w-[80%] rounded-2xl p-4 ${msg.senderType === 'Owner' ? 'text-white' : 'bg-neutral-100 text-neutral-900'}`}
                                    style={msg.senderType === 'Owner' ? { background: 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)' } : undefined}
                                >
                                    <p className={`text-xs font-semibold mb-1 ${
                                        msg.senderType === 'Owner' ? 'text-white/70' : 'text-neutral-500'
                                    }`}>
                                        {msg.senderType === 'Owner' ? 'You' : 'Support Team'}
                                    </p>
                                    <p className="text-sm leading-relaxed whitespace-pre-wrap">{msg.messageText}</p>
                                    <p className={`text-xs mt-2 ${
                                        msg.senderType === 'Owner' ? 'text-white/70' : 'text-neutral-400'
                                    }`}>
                                        {formatDateTime(msg.createdDate)}
                                    </p>
                                </div>
                            </div>
                        ))
                    ) : (
                        <p className="text-center text-sm text-neutral-400 py-6">No messages yet. Start the conversation below.</p>
                    )}
                </div>

                {/* Message Input */}
                {!isClosed ? (
                    <div className="p-4 border-t border-neutral-200">
                        <div className="flex gap-3">
                            <textarea
                                value={messageText}
                                onChange={(e) => setMessageText(e.target.value)}
                                placeholder="Type your message..."
                                className="flex-1 px-4 py-3 border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-orange-400 resize-none"
                                rows={2}
                                disabled={sending}
                                onKeyDown={(e) => {
                                    if (e.key === 'Enter' && !e.shiftKey) {
                                        e.preventDefault();
                                        handleSendMessage();
                                    }
                                }}
                            />
                            <button
                                onClick={handleSendMessage}
                                disabled={sending || !messageText.trim()}
                                className="px-5 text-white rounded-xl font-semibold transition-all disabled:opacity-50 disabled:cursor-not-allowed self-end h-[42px]"
                                style={{ background: 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)' }}
                            >
                                {sending ? (
                                    <svg className="w-5 h-5 animate-spin" fill="none" viewBox="0 0 24 24">
                                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                                    </svg>
                                ) : (
                                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 19l9 2-9-18-9 18 9-2zm0 0v-8" />
                                    </svg>
                                )}
                            </button>
                        </div>
                    </div>
                ) : (
                    <div className="p-4 border-t border-neutral-200 text-center">
                        <p className="text-sm text-neutral-500">This ticket is {detail.status.toLowerCase()}. No further messages can be sent.</p>
                    </div>
                )}
            </div>
        </div>
    );
};

// ===================================
// FAQ Item
// ===================================
const FAQItem = ({ question, answer }) => {
    const [isOpen, setIsOpen] = useState(false);

    return (
        <div className="bg-white rounded-xl border border-neutral-200 overflow-hidden">
            <button
                onClick={() => setIsOpen(!isOpen)}
                className="w-full flex items-center justify-between p-5 text-left hover:bg-neutral-50 transition-colors"
            >
                <span className="font-semibold text-neutral-900 pr-4">{question}</span>
                <svg
                    className={`w-5 h-5 text-neutral-500 flex-shrink-0 transition-transform ${isOpen ? 'rotate-180' : ''}`}
                    fill="none" stroke="currentColor" viewBox="0 0 24 24"
                >
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                </svg>
            </button>
            {isOpen && (
                <div className="px-5 pb-5 text-neutral-600 leading-relaxed border-t border-neutral-100 pt-4">
                    {answer}
                </div>
            )}
        </div>
    );
};

// ===================================
// Main Component
// ===================================
export default function Support() {
    const [activeTab, setActiveTab] = useState('tickets');
    const [showNewTicketModal, setShowNewTicketModal] = useState(false);
    const [selectedTicketId, setSelectedTicketId] = useState(null);

    // Tickets state
    const [tickets, setTickets] = useState([]);
    const [stats, setStats] = useState(null);
    const [loading, setLoading] = useState(true);
    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [totalCount, setTotalCount] = useState(0);
    const [statusFilter, setStatusFilter] = useState(null);
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState(null);

    const pageSize = 10;

    const fetchTickets = useCallback(async () => {
        setLoading(true);
        setError(null);
        try {
            const filters = {};
            if (statusFilter) filters.status = statusFilter;

            const response = await ownerApiService.getSupportTickets(page, pageSize, filters);
            if (response?.result && response.data) {
                setTickets(response.data.tickets || []);
                setTotalPages(response.data.totalPages || 1);
                setTotalCount(response.data.totalCount || 0);
            } else {
                setTickets([]);
                setTotalCount(0);
            }
        } catch (err) {
            console.error('Error fetching tickets:', err);
            setError('Failed to load support tickets.');
            setTickets([]);
        } finally {
            setLoading(false);
        }
    }, [page, statusFilter]);

    const fetchStats = useCallback(async () => {
        try {
            const response = await ownerApiService.getSupportTicketStats();
            if (response?.result && response.data) {
                setStats(response.data);
            }
        } catch (err) {
            console.error('Error fetching ticket stats:', err);
        }
    }, []);

    useEffect(() => {
        fetchTickets();
    }, [fetchTickets]);

    useEffect(() => {
        fetchStats();
    }, [fetchStats]);

    useEffect(() => {
        setPage(1);
    }, [statusFilter]);

    const handleCreateTicket = async (form) => {
        setSubmitting(true);
        try {
            const response = await ownerApiService.createSupportTicket(form);
            if (response?.result) {
                setShowNewTicketModal(false);
                fetchTickets();
                fetchStats();
            } else {
                alert(response?.message || 'Failed to create ticket.');
            }
        } catch (err) {
            console.error('Error creating ticket:', err);
            alert('Failed to create ticket. Please try again.');
        } finally {
            setSubmitting(false);
        }
    };

    const handleViewTicket = (ticketId) => {
        setSelectedTicketId(ticketId);
    };

    const handleBackToList = () => {
        setSelectedTicketId(null);
        fetchTickets();
    };

    const faqs = [
        {
            question: 'How do I receive payments?',
            answer: 'Payments are automatically processed after successful order completion. The funds are transferred to your registered bank account. You can track your earnings in the "Earnings & Payments" section.'
        },
        {
            question: 'What is the commission structure?',
            answer: 'We charge a commission on each completed order. This commission covers platform maintenance, payment processing, customer support, and marketing services that help you get more orders.'
        },
        {
            question: 'How do I handle customer cancellations?',
            answer: 'Cancellation policies depend on timing: Orders cancelled 48+ hours before the event receive a full refund. Orders cancelled 24-48 hours before receive 50% refund. Orders cancelled within 24 hours are non-refundable, and you receive full payment.'
        },
        {
            question: 'Can I customize my menu and pricing?',
            answer: 'Yes! You have full control over your menu items, packages, and pricing. Go to "Menu Management" to add, edit, or remove items. You can also create seasonal specials and limited-time offers.'
        },
        {
            question: 'How do I manage availability?',
            answer: 'Click the "Availability" button in the top header to set your calendar. You can mark specific dates as unavailable, set booking limits, and configure your operating hours.'
        },
        {
            question: 'What if I need to cancel a confirmed booking?',
            answer: 'Partner-initiated cancellations should be avoided whenever possible as they affect your rating. If you absolutely must cancel, contact support immediately. Repeated cancellations may result in account suspension.'
        }
    ];

    // Ticket detail view
    if (selectedTicketId) {
        return (
            <TicketDetailView ticketId={selectedTicketId} onBack={handleBackToList} />
        );
    }

    return (
        <div className="space-y-6">
                {/* Header */}
                <div className="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-4">
                    <div>
                        <h1 className="text-3xl font-bold text-neutral-900">Support & Help Center</h1>
                        <p className="text-neutral-600 mt-1">We're here to help you succeed</p>
                    </div>
                    <button
                        onClick={() => setShowNewTicketModal(true)}
                        className="flex items-center gap-2 text-white px-6 py-3 rounded-xl font-semibold shadow-lg hover:shadow-xl transition-all"
                        style={{ background: 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)' }}
                    >
                        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                        </svg>
                        Create Support Ticket
                    </button>
                </div>

                {/* Stats Cards */}
                {stats && (
                    <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                        <StatCard label="Open" value={stats.openTickets} color="yellow" />
                        <StatCard label="In Progress" value={stats.inProgressTickets} color="blue" />
                        <StatCard label="Resolved" value={stats.resolvedTickets} color="green" />
                        <StatCard label="Total Tickets" value={stats.totalTickets} color="neutral" />
                    </div>
                )}

                {/* Quick Contact */}
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                    <div className="bg-white rounded-2xl shadow-sm border border-neutral-100 p-5 flex items-center gap-4">
                        <div className="p-3 rounded-xl flex-shrink-0" style={{ background: 'rgba(255,107,53,0.1)', color: 'var(--color-primary)' }}>
                            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                            </svg>
                        </div>
                        <div>
                            <h3 className="font-bold text-neutral-900 text-sm">Email Support</h3>
                            <a href="mailto:support@enyvora.com" className="text-sm font-medium hover:underline" style={{ color: 'var(--color-primary)' }}>support@enyvora.com</a>
                        </div>
                    </div>
                    <div className="bg-white rounded-2xl shadow-sm border border-neutral-100 p-5 flex items-center gap-4">
                        <div className="p-3 bg-green-100 rounded-xl text-green-600 flex-shrink-0">
                            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z" />
                            </svg>
                        </div>
                        <div>
                            <h3 className="font-bold text-neutral-900 text-sm">Phone Support</h3>
                            <a href="tel:+911234567890" className="text-green-600 text-sm font-medium hover:underline">+91-1234567890</a>
                            <p className="text-xs text-neutral-400">9 AM - 6 PM</p>
                        </div>
                    </div>
                    <div className="bg-white rounded-2xl shadow-sm border border-neutral-100 p-5 flex items-center gap-4">
                        <div className="p-3 bg-neutral-100 rounded-xl text-neutral-500 flex-shrink-0">
                            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
                            </svg>
                        </div>
                        <div>
                            <h3 className="font-bold text-neutral-900 text-sm">Live Chat</h3>
                            <p className="text-neutral-500 text-sm font-medium">Coming Soon</p>
                        </div>
                    </div>
                </div>

                {/* Tabs */}
                <div className="flex gap-3 border-b border-neutral-200">
                    <button
                        onClick={() => setActiveTab('tickets')}
                        className={`px-4 py-3 font-semibold text-sm transition-all border-b-2 ${
                            activeTab === 'tickets'
                                ? 'border-transparent'
                                : 'border-transparent text-neutral-600 hover:text-neutral-900'
                        }`}
                        style={activeTab === 'tickets' ? { color: 'var(--color-primary)', borderBottomColor: 'var(--color-primary)' } : undefined}
                    >
                        My Tickets {stats ? `(${stats.totalTickets})` : ''}
                    </button>
                    <button
                        onClick={() => setActiveTab('faqs')}
                        className={`px-4 py-3 font-semibold text-sm transition-all border-b-2 ${
                            activeTab === 'faqs'
                                ? 'border-transparent'
                                : 'border-transparent text-neutral-600 hover:text-neutral-900'
                        }`}
                        style={activeTab === 'faqs' ? { color: 'var(--color-primary)', borderBottomColor: 'var(--color-primary)' } : undefined}
                    >
                        FAQs
                    </button>
                </div>

                {/* Tickets Tab */}
                {activeTab === 'tickets' && (
                    <>
                        {/* Status Filters */}
                        <div className="flex flex-wrap gap-2">
                            {[null, 'Open', 'InProgress', 'Resolved', 'Closed'].map((s) => (
                                <button
                                    key={s || 'all'}
                                    onClick={() => setStatusFilter(s)}
                                    className={`px-4 py-2 rounded-xl text-sm font-semibold transition-all ${
                                        statusFilter === s
                                            ? 'text-white shadow-md'
                                            : 'bg-white text-neutral-600 hover:bg-neutral-50 border border-neutral-200'
                                    }`}
                                    style={statusFilter === s ? { background: 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)' } : undefined}
                                >
                                    {s ? (statusConfig[s]?.label || s) : 'All'}
                                </button>
                            ))}
                        </div>

                        {/* Error */}
                        {error && (
                            <div className="bg-red-50 border border-red-200 rounded-xl p-4 flex items-center gap-3">
                                <svg className="w-5 h-5 text-red-500 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                                </svg>
                                <p className="text-sm text-red-700">{error}</p>
                                <button onClick={fetchTickets} className="ml-auto text-sm font-semibold text-red-600 hover:text-red-800">Retry</button>
                            </div>
                        )}

                        {/* Loading */}
                        {loading ? (
                            <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
                                {[1, 2, 3, 4].map(i => (
                                    <div key={i} className="bg-white rounded-2xl shadow-sm border border-neutral-200 p-6 animate-pulse">
                                        <div className="flex items-center gap-3 mb-3">
                                            <div className="h-5 bg-neutral-200 rounded w-20" />
                                            <div className="h-5 bg-neutral-200 rounded-full w-16" />
                                        </div>
                                        <div className="h-5 bg-neutral-200 rounded w-3/4 mb-2" />
                                        <div className="h-4 bg-neutral-200 rounded w-full mb-2" />
                                        <div className="h-4 bg-neutral-200 rounded w-1/2" />
                                    </div>
                                ))}
                            </div>
                        ) : tickets.length > 0 ? (
                            <>
                                <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
                                    {tickets.map((ticket) => (
                                        <TicketCard key={ticket.ticketId} ticket={ticket} onView={handleViewTicket} />
                                    ))}
                                </div>

                                {/* Pagination */}
                                {totalPages > 1 && (
                                    <div className="flex items-center justify-between bg-white rounded-2xl shadow-sm border border-neutral-200 p-4">
                                        <p className="text-sm text-neutral-600">
                                            Showing {(page - 1) * pageSize + 1}-{Math.min(page * pageSize, totalCount)} of {totalCount}
                                        </p>
                                        <div className="flex gap-2">
                                            <button
                                                onClick={() => setPage(p => Math.max(1, p - 1))}
                                                disabled={page === 1}
                                                className="px-4 py-2 bg-white border border-neutral-200 rounded-xl text-sm font-semibold text-neutral-700 hover:bg-neutral-50 disabled:opacity-50"
                                            >
                                                Previous
                                            </button>
                                            <button
                                                onClick={() => setPage(p => Math.min(totalPages, p + 1))}
                                                disabled={page === totalPages}
                                                className="px-4 py-2 bg-white border border-neutral-200 rounded-xl text-sm font-semibold text-neutral-700 hover:bg-neutral-50 disabled:opacity-50"
                                            >
                                                Next
                                            </button>
                                        </div>
                                    </div>
                                )}
                            </>
                        ) : (
                            <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 p-12">
                                <div className="text-center">
                                    <svg className="w-20 h-20 mx-auto mb-4 text-neutral-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                                    </svg>
                                    <h3 className="text-xl font-semibold text-neutral-900 mb-2">No Support Tickets</h3>
                                    <p className="text-neutral-600 mb-4">
                                        {statusFilter
                                            ? `No ${statusConfig[statusFilter]?.label || statusFilter} tickets found.`
                                            : "You haven't created any support tickets yet."}
                                    </p>
                                    <button
                                        onClick={() => setShowNewTicketModal(true)}
                                        className="text-white px-6 py-2.5 rounded-xl font-semibold transition-all shadow-lg hover:shadow-xl"
                                        style={{ background: 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)' }}
                                    >
                                        Create Your First Ticket
                                    </button>
                                </div>
                            </div>
                        )}
                    </>
                )}

                {/* FAQs Tab */}
                {activeTab === 'faqs' && (
                    <div className="space-y-3">
                        {faqs.map((faq, index) => (
                            <FAQItem key={index} question={faq.question} answer={faq.answer} />
                        ))}
                    </div>
                )}

                {/* Resources Banner */}
                <div className="rounded-2xl shadow-lg p-6 text-white" style={{ background: 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)' }}>
                    <div className="flex items-start justify-between">
                        <div>
                            <h2 className="text-xl font-bold mb-2">Additional Resources</h2>
                            <p className="text-white/80 mb-4">Learn more about using the platform</p>
                            <div className="flex flex-wrap gap-3">
                                <button className="bg-white/20 hover:bg-white/30 px-4 py-2 rounded-lg font-semibold transition-colors">
                                    Partner Guide
                                </button>
                                <button className="bg-white/20 hover:bg-white/30 px-4 py-2 rounded-lg font-semibold transition-colors">
                                    Video Tutorials
                                </button>
                                <button className="bg-white/20 hover:bg-white/30 px-4 py-2 rounded-lg font-semibold transition-colors">
                                    Best Practices
                                </button>
                            </div>
                        </div>
                        <div className="hidden sm:block">
                            <svg className="w-24 h-24 opacity-20" fill="currentColor" viewBox="0 0 24 24">
                                <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-6h2v6zm0-8h-2V7h2v2z" />
                            </svg>
                        </div>
                    </div>
                </div>

            {/* Create Ticket Modal */}
            <CreateTicketModal
                isOpen={showNewTicketModal}
                onClose={() => setShowNewTicketModal(false)}
                onSubmit={handleCreateTicket}
                submitting={submitting}
            />
        </div>
    );
}
