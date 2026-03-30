import React, { useState } from 'react';
import { Link } from 'react-router-dom';

const FAQS = [
    { q: 'How do I get my profile approved?', a: 'After completing the 5-step registration, our team reviews your profile within 2-3 business days. You\'ll receive a notification once approved or if additional documents are needed.' },
    { q: 'When do I receive payouts?', a: 'Payouts are processed within 3-5 business days after each event is marked complete. You can track all payouts in your dashboard under Earnings.' },
    { q: 'Can I update my menu and pricing?', a: 'Yes. You can update your menu, packages, and pricing at any time from the dashboard. Note that existing confirmed bookings will not be affected by price changes.' },
    { q: 'What happens if a customer cancels?', a: 'Cancellations are handled per our platform policy. Depending on when the cancellation occurs, you may receive a partial or full payment as compensation. See the cancellation policy in your partner agreement.' },
];

export default function PartnerSupport() {
    const [open, setOpen] = useState(null);

    return (
        <div className="min-h-screen bg-white">
            {/* Breadcrumb */}
            <div className="bg-gray-50 border-b">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-3 text-sm text-gray-500">
                    <Link to="/" className="hover:text-gray-700">Home</Link>
                    <span className="mx-2">/</span>
                    <span className="text-gray-700 font-medium">Partner Support</span>
                </div>
            </div>

            {/* Hero */}
            <div className="bg-gradient-to-br from-green-50 to-white py-16">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
                    <h1 className="text-4xl font-bold text-gray-900">Partner Support</h1>
                    <p className="mt-4 text-lg text-gray-600 max-w-2xl mx-auto">
                        We're here to help you succeed. Find answers below or reach out to our partner support team.
                    </p>
                </div>
            </div>

            <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
                <h2 className="text-xl font-bold text-gray-900 mb-6">Frequently Asked Questions</h2>
                <div className="space-y-3 mb-12">
                    {FAQS.map((faq, i) => (
                        <div key={i} className="border border-gray-100 rounded-xl overflow-hidden">
                            <button
                                onClick={() => setOpen(open === i ? null : i)}
                                className="w-full flex items-center justify-between px-5 py-4 text-left hover:bg-gray-50 transition-colors"
                            >
                                <span className="font-medium text-gray-900">{faq.q}</span>
                                <svg className={`w-4 h-4 text-gray-400 transition-transform shrink-0 ml-4 ${open === i ? 'rotate-180' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                                </svg>
                            </button>
                            {open === i && (
                                <div className="px-5 pb-4 text-gray-600 text-sm leading-relaxed border-t border-gray-50">{faq.a}</div>
                            )}
                        </div>
                    ))}
                </div>

                <div className="bg-green-50 rounded-2xl p-8 text-center">
                    <h3 className="text-lg font-bold text-gray-900 mb-2">Still need help?</h3>
                    <p className="text-gray-600 mb-6">Our partner support team is available Monday–Saturday, 9 AM to 7 PM.</p>
                    <div className="flex flex-col sm:flex-row items-center justify-center gap-4">
                        <a href="mailto:partners@enyvora.com" className="px-6 py-2.5 bg-green-600 text-white font-medium rounded-lg hover:bg-green-700 transition-colors">
                            Email Support
                        </a>
                        <Link to="/contact-us" className="px-6 py-2.5 border border-gray-300 text-gray-700 font-medium rounded-lg hover:border-green-400 hover:text-green-600 transition-colors">
                            Contact Us
                        </Link>
                    </div>
                </div>
            </div>
        </div>
    );
}
