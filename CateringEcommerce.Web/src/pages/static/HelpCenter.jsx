import React, { useState } from 'react';
import { Link } from 'react-router-dom';

const FAQS = [
    { q: 'How do I book a caterer?', a: 'Browse caterers on our platform, select one you like, choose a package, and proceed to checkout. You\'ll need to be logged in to complete a booking. After booking, you\'ll receive a confirmation with event details.' },
    { q: 'What payment methods are accepted?', a: 'We accept all major payment methods including credit/debit cards, UPI, net banking, and wallets via our secure Razorpay payment gateway. All transactions are encrypted and safe.' },
    { q: 'What is the cancellation policy?', a: 'Cancellation refunds depend on how far in advance you cancel. Cancellations more than 14 days before the event receive a full refund. 7-14 days: 75% refund. 3-7 days: 50% refund. Less than 3 days: no refund. Check your order details for exact terms.' },
    { q: 'How do I contact my caterer?', a: 'Once your booking is confirmed, you can view the caterer\'s contact details in your order detail page under My Orders. You can also send messages through the platform\'s order communication feature.' },
    { q: 'How long do refunds take?', a: 'Approved refunds are processed within 5-7 business days back to your original payment method. You\'ll receive an email confirmation once the refund is initiated.' },
    { q: 'My account is locked or I can\'t log in — what should I do?', a: 'If you\'re having trouble logging in, try requesting a new OTP. If your account is locked, contact our support team at support@enyvora.com with your registered phone number and we\'ll assist you within 24 hours.' },
];

export default function HelpCenter() {
    const [open, setOpen] = useState(null);

    return (
        <div className="min-h-screen bg-white">
            {/* Breadcrumb */}
            <div className="bg-neutral-50 border-b">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-3 text-sm text-neutral-500">
                    <Link to="/" className="hover:text-neutral-700">Home</Link>
                    <span className="mx-2">/</span>
                    <span className="text-neutral-700 font-medium">Help Center</span>
                </div>
            </div>

            {/* Hero */}
            <div className="bg-gradient-to-br from-teal-50 to-white py-16">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
                    <h1 className="text-4xl font-bold text-neutral-900">Help Center</h1>
                    <p className="mt-4 text-lg text-neutral-600 max-w-2xl mx-auto">
                        Answers to the most common questions about booking, payments, cancellations, and accounts.
                    </p>
                </div>
            </div>

            <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
                <div className="space-y-3 mb-12">
                    {FAQS.map((faq, i) => (
                        <div key={i} className="border border-gray-100 rounded-xl overflow-hidden">
                            <button
                                onClick={() => setOpen(open === i ? null : i)}
                                className="w-full flex items-center justify-between px-5 py-4 text-left hover:bg-neutral-50 transition-colors"
                            >
                                <span className="font-medium text-neutral-900">{faq.q}</span>
                                <svg className={`w-4 h-4 text-neutral-400 transition-transform shrink-0 ml-4 ${open === i ? 'rotate-180' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                                </svg>
                            </button>
                            {open === i && (
                                <div className="px-5 pb-4 text-neutral-600 text-sm leading-relaxed border-t border-gray-50">{faq.a}</div>
                            )}
                        </div>
                    ))}
                </div>

                <div className="bg-teal-50 rounded-2xl p-8 text-center">
                    <h3 className="text-lg font-bold text-neutral-900 mb-2">Didn't find your answer?</h3>
                    <p className="text-neutral-600 mb-6">Our support team is happy to help with any other questions.</p>
                    <Link to="/contact-us" className="inline-block px-6 py-3 bg-teal-600 text-white font-medium rounded-lg hover:bg-teal-700 transition-colors">
                        Contact Support
                    </Link>
                </div>
            </div>
        </div>
    );
}
