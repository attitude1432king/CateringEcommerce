import React from 'react';
import { Link } from 'react-router-dom';

const SECTIONS = [
    { title: '1. Agreement to Terms', content: 'By accessing or using the ENYVORA platform ("Platform"), you agree to be bound by these Terms and Conditions ("Terms"). If you do not agree with any part of these terms, you may not access the Platform. These Terms apply to all visitors, users, and others who access or use the Platform.' },
    { title: '2. Use of the Platform', content: 'You may use the Platform only for lawful purposes and in accordance with these Terms. You agree not to use the Platform in any way that violates applicable laws or regulations, to transmit harmful or offensive content, to impersonate any person or entity, or to interfere with or disrupt the Platform or its servers.' },
    { title: '3. User Accounts', content: 'When you create an account, you must provide accurate and complete information. You are responsible for maintaining the confidentiality of your account credentials and for all activities that occur under your account. You must notify us immediately upon becoming aware of any breach of security or unauthorized use of your account.' },
    { title: '4. Bookings & Payments', content: 'All bookings are subject to availability and confirmation by the catering partner. Prices displayed are inclusive of applicable taxes unless stated otherwise. Payment is required at the time of booking. ENYVORA uses Razorpay for payment processing, and all transactions are subject to Razorpay\'s terms of service.' },
    { title: '5. Cancellations & Refunds', content: 'Cancellation and refund policies vary based on timing. Cancellations more than 14 days before the event are eligible for a full refund. Between 7-14 days: 75% refund. Between 3-7 days: 50% refund. Less than 3 days: no refund. Refunds are processed within 5-7 business days to the original payment method.' },
    { title: '6. Limitation of Liability', content: 'ENYVORA acts as a marketplace connecting customers with catering businesses. We are not responsible for the quality, safety, or accuracy of services provided by catering partners. To the maximum extent permitted by law, ENYVORA shall not be liable for any indirect, incidental, special, or consequential damages arising from your use of the Platform.' },
    { title: '7. Privacy', content: 'Your use of the Platform is also governed by our Privacy Policy, which is incorporated into these Terms by reference. Please review our Privacy Policy to understand our practices regarding the collection and use of your personal information.' },
    { title: '8. Changes to Terms', content: 'We reserve the right to modify these Terms at any time. We will notify you of significant changes by email or by posting a notice on the Platform. Your continued use of the Platform after changes are posted constitutes your acceptance of the revised Terms.' },
];

export default function TermsConditions() {
    return (
        <div className="min-h-screen bg-white">
            {/* Breadcrumb */}
            <div className="bg-neutral-50 border-b">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-3 text-sm text-neutral-500">
                    <Link to="/" className="hover:text-neutral-700">Home</Link>
                    <span className="mx-2">/</span>
                    <span className="text-neutral-700 font-medium">Terms & Conditions</span>
                </div>
            </div>

            {/* Hero */}
            <div className="bg-neutral-50 border-b py-12">
                <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8">
                    <h1 className="text-3xl font-bold text-neutral-900">Terms & Conditions</h1>
                    <p className="mt-2 text-sm text-neutral-500">Last updated: March 2026</p>
                </div>
            </div>

            <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
                <div className="prose prose-gray max-w-none">
                    {SECTIONS.map((section, i) => (
                        <div key={i} className="mb-8">
                            <h2 className="text-lg font-bold text-neutral-900 mb-3">{section.title}</h2>
                            <p className="text-neutral-600 leading-relaxed text-sm">{section.content}</p>
                        </div>
                    ))}
                </div>

                <div className="border-t border-gray-100 pt-8 mt-8 text-sm text-neutral-500">
                    <p>If you have questions about these Terms, please <Link to="/contact-us" className="text-indigo-600 hover:underline">contact us</Link>.</p>
                </div>
            </div>
        </div>
    );
}
