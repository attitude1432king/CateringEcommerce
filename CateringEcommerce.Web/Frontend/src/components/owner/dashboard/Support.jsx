/*
========================================
File: src/components/owner/dashboard/Support.jsx
Modern Redesign - Support & Help Center
========================================
*/
import React, { useState } from 'react';

// Ticket Card Component
const TicketCard = ({ ticket }) => {
    const statusColors = {
        open: 'bg-yellow-100 text-yellow-800 border-yellow-200',
        'in-progress': 'bg-blue-100 text-blue-800 border-blue-200',
        resolved: 'bg-green-100 text-green-800 border-green-200',
        closed: 'bg-neutral-100 text-neutral-800 border-neutral-200',
    };

    const priorityColors = {
        high: 'text-red-600',
        medium: 'text-orange-600',
        low: 'text-green-600',
    };

    return (
        <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 p-6 hover:shadow-md transition-shadow">
            <div className="flex items-start justify-between mb-4">
                <div className="flex-1">
                    <div className="flex items-center gap-3 mb-2">
                        <h3 className="text-lg font-bold text-neutral-900">#{ticket.id}</h3>
                        <span className={`px-3 py-1 rounded-full text-xs font-semibold border ${statusColors[ticket.status]}`}>
                            {ticket.status.charAt(0).toUpperCase() + ticket.status.slice(1).replace('-', ' ')}
                        </span>
                        <span className={`flex items-center gap-1 text-xs font-semibold ${priorityColors[ticket.priority]}`}>
                            <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                                <path fillRule="evenodd" d="M3 6a3 3 0 013-3h10a1 1 0 01.8 1.6L14.25 8l2.55 3.4A1 1 0 0116 13H6a1 1 0 00-1 1v3a1 1 0 11-2 0V6z" clipRule="evenodd" />
                            </svg>
                            {ticket.priority.charAt(0).toUpperCase() + ticket.priority.slice(1)}
                        </span>
                    </div>
                    <p className="text-neutral-600 text-sm mb-2">{ticket.category}</p>
                </div>
            </div>

            <h4 className="font-semibold text-neutral-900 mb-2">{ticket.subject}</h4>
            <p className="text-neutral-600 text-sm mb-4 line-clamp-2">{ticket.description}</p>

            <div className="flex items-center justify-between text-sm">
                <span className="text-neutral-500">Created: {ticket.createdDate}</span>
                <button className="text-indigo-600 hover:text-indigo-700 font-semibold">
                    View Details →
                </button>
            </div>
        </div>
    );
};

// FAQ Item Component
const FAQItem = ({ question, answer }) => {
    const [isOpen, setIsOpen] = useState(false);

    return (
        <div className="bg-white rounded-xl border border-neutral-200 overflow-hidden">
            <button
                onClick={() => setIsOpen(!isOpen)}
                className="w-full flex items-center justify-between p-6 text-left hover:bg-neutral-50 transition-colors"
            >
                <span className="font-semibold text-neutral-900 pr-4">{question}</span>
                <svg
                    className={`w-5 h-5 text-neutral-500 flex-shrink-0 transition-transform ${isOpen ? 'rotate-180' : ''}`}
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                >
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                </svg>
            </button>
            {isOpen && (
                <div className="px-6 pb-6 text-neutral-600 leading-relaxed border-t border-neutral-100 pt-4">
                    {answer}
                </div>
            )}
        </div>
    );
};

// Quick Action Card
const QuickActionCard = ({ icon, title, description, buttonText, onClick }) => (
    <div className="bg-white rounded-2xl shadow-sm border border-neutral-100 p-6 hover:shadow-md transition-shadow">
        <div className="flex flex-col items-center text-center">
            <div className="p-4 bg-indigo-100 rounded-xl mb-4">
                <div className="text-indigo-600">
                    {icon}
                </div>
            </div>
            <h3 className="text-lg font-bold text-neutral-900 mb-2">{title}</h3>
            <p className="text-sm text-neutral-600 mb-4">{description}</p>
            <button
                onClick={onClick}
                className="w-full bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2.5 rounded-xl font-semibold transition-colors"
            >
                {buttonText}
            </button>
        </div>
    </div>
);

export default function Support() {
    const [activeTab, setActiveTab] = useState('tickets');
    const [showNewTicketModal, setShowNewTicketModal] = useState(false);

    // Mock data - replace with real API data
    const tickets = [
        {
            id: 'TKT1001',
            subject: 'Payment not received for Order #2001',
            description: 'I completed the event on Jan 10 but haven\'t received payment yet. Can you please check?',
            category: 'Payment Issues',
            status: 'in-progress',
            priority: 'high',
            createdDate: 'Jan 11, 2026'
        },
        {
            id: 'TKT1002',
            subject: 'How to update menu pricing?',
            description: 'I want to update the prices for my menu items. Where can I do this in the dashboard?',
            category: 'Account & Settings',
            status: 'resolved',
            priority: 'medium',
            createdDate: 'Jan 9, 2026'
        },
        {
            id: 'TKT1003',
            subject: 'Customer cancelled last minute',
            description: 'A customer cancelled 2 hours before the event. What is the cancellation policy?',
            category: 'Orders & Bookings',
            status: 'closed',
            priority: 'low',
            createdDate: 'Jan 5, 2026'
        },
    ];

    const faqs = [
        {
            question: 'How do I receive payments?',
            answer: 'Payments are automatically processed after successful order completion. The funds are transferred to your registered bank account every week on Friday. You can track your earnings in the "Earnings & Payments" section.'
        },
        {
            question: 'What is the commission structure?',
            answer: 'We charge a 10% commission on each completed order. This commission covers platform maintenance, payment processing, customer support, and marketing services that help you get more orders.'
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
            answer: 'Click the "Availability" button in the top header to set your calendar. You can mark specific dates as unavailable, set booking limits, and configure your operating hours. This ensures you only receive bookings when you\'re available.'
        },
        {
            question: 'What if I need to cancel a confirmed booking?',
            answer: 'Partner-initiated cancellations should be avoided whenever possible as they affect your rating. If you absolutely must cancel, contact support immediately at support@enyvora.com or call +91-1234567890. Repeated cancellations may result in account suspension.'
        }
    ];

    const contactOptions = [
        {
            icon: <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" /></svg>,
            title: 'Email Support',
            description: 'Get help via email within 24 hours',
            buttonText: 'support@enyvora.com',
            onClick: () => window.location.href = 'mailto:support@enyvora.com'
        },
        {
            icon: <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z" /></svg>,
            title: 'Phone Support',
            description: 'Talk to us directly (9 AM - 6 PM)',
            buttonText: '+91-1234567890',
            onClick: () => window.location.href = 'tel:+911234567890'
        },
        {
            icon: <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" /></svg>,
            title: 'Live Chat',
            description: 'Chat with our support team',
            buttonText: 'Start Chat',
            onClick: () => console.log('Start chat')
        },
    ];

    return (
        <div className="min-h-screen bg-neutral-50">
            <div className="p-4 sm:p-6 lg:p-8 space-y-6">
                {/* Header */}
                <div className="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-4">
                    <div>
                        <h1 className="text-3xl font-bold text-neutral-900">Support & Help Center</h1>
                        <p className="text-neutral-600 mt-1">We're here to help you succeed</p>
                    </div>

                    <button
                        onClick={() => setShowNewTicketModal(true)}
                        className="flex items-center gap-2 bg-indigo-600 hover:bg-indigo-700 text-white px-6 py-3 rounded-xl font-semibold transition-colors shadow-md"
                    >
                        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                        </svg>
                        Create Support Ticket
                    </button>
                </div>

                {/* Quick Contact Options */}
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                    {contactOptions.map((option, index) => (
                        <QuickActionCard key={index} {...option} />
                    ))}
                </div>

                {/* Tabs */}
                <div className="flex gap-3 border-b border-neutral-200">
                    <button
                        onClick={() => setActiveTab('tickets')}
                        className={`px-4 py-3 font-semibold text-sm transition-all border-b-2 ${
                            activeTab === 'tickets'
                                ? 'border-indigo-600 text-indigo-600'
                                : 'border-transparent text-neutral-600 hover:text-neutral-900'
                        }`}
                    >
                        My Tickets ({tickets.length})
                    </button>
                    <button
                        onClick={() => setActiveTab('faqs')}
                        className={`px-4 py-3 font-semibold text-sm transition-all border-b-2 ${
                            activeTab === 'faqs'
                                ? 'border-indigo-600 text-indigo-600'
                                : 'border-transparent text-neutral-600 hover:text-neutral-900'
                        }`}
                    >
                        FAQs
                    </button>
                </div>

                {/* Content */}
                {activeTab === 'tickets' && (
                    <div>
                        {tickets.length > 0 ? (
                            <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
                                {tickets.map((ticket) => (
                                    <TicketCard key={ticket.id} ticket={ticket} />
                                ))}
                            </div>
                        ) : (
                            <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 p-12">
                                <div className="text-center">
                                    <svg className="w-20 h-20 mx-auto mb-4 text-neutral-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                                    </svg>
                                    <h3 className="text-xl font-semibold text-neutral-900 mb-2">No Support Tickets</h3>
                                    <p className="text-neutral-600 mb-4">You haven't created any support tickets yet.</p>
                                    <button
                                        onClick={() => setShowNewTicketModal(true)}
                                        className="bg-indigo-600 hover:bg-indigo-700 text-white px-6 py-2.5 rounded-xl font-semibold transition-colors"
                                    >
                                        Create Your First Ticket
                                    </button>
                                </div>
                            </div>
                        )}
                    </div>
                )}

                {activeTab === 'faqs' && (
                    <div className="space-y-4">
                        {faqs.map((faq, index) => (
                            <FAQItem key={index} question={faq.question} answer={faq.answer} />
                        ))}
                    </div>
                )}

                {/* Help Resources */}
                <div className="bg-gradient-to-br from-indigo-600 to-purple-600 rounded-2xl shadow-lg p-6 text-white">
                    <div className="flex items-start justify-between">
                        <div>
                            <h2 className="text-xl font-bold mb-2">Additional Resources</h2>
                            <p className="text-indigo-100 mb-4">Learn more about using the platform</p>
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
            </div>
        </div>
    );
}