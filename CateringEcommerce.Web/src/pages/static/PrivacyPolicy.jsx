import React from 'react';
import { Link } from 'react-router-dom';

const SECTIONS = [
    { title: '1. Information We Collect', content: 'We collect information you provide directly, such as your name, phone number, email address, and event details when you register or make a booking. We also collect usage data including pages visited, features used, device information, and IP address to improve our services.' },
    { title: '2. How We Use Your Information', content: 'We use your information to process bookings and payments, send booking confirmations and updates, provide customer support, personalize your experience on the Platform, send promotional communications (with your consent), and improve our products and services. We do not sell your personal data to third parties.' },
    { title: '3. Sharing of Information', content: 'We share your information with catering partners only as necessary to fulfill your booking (e.g., event date, guest count, delivery address). We share data with payment processors (Razorpay) to process transactions. We may disclose information if required by law or to protect the rights and safety of ENYVORA and its users.' },
    { title: '4. Cookies', content: 'We use cookies and similar tracking technologies to maintain session state, remember your preferences, analyze platform usage, and serve relevant content. You can control cookie settings through your browser preferences. Note that disabling certain cookies may affect Platform functionality.' },
    { title: '5. Data Security', content: 'We implement industry-standard security measures including HTTPS encryption, secure httpOnly cookies for authentication tokens, and rate limiting to protect against unauthorized access. However, no method of transmission over the Internet is 100% secure, and we cannot guarantee absolute security.' },
    { title: '6. Your Rights', content: 'You have the right to access the personal data we hold about you, request correction of inaccurate data, request deletion of your account and associated data, opt out of marketing communications at any time, and file a complaint with relevant data protection authorities. To exercise these rights, contact us at privacy@enyvora.com.' },
    { title: '7. Contact', content: 'If you have questions or concerns about this Privacy Policy or our data practices, please contact our Privacy Team at privacy@enyvora.com. We will respond to your inquiry within 30 days.' },
];

export default function PrivacyPolicy() {
    return (
        <div className="min-h-screen bg-white">
            {/* Breadcrumb */}
            <div className="bg-gray-50 border-b">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-3 text-sm text-gray-500">
                    <Link to="/" className="hover:text-gray-700">Home</Link>
                    <span className="mx-2">/</span>
                    <span className="text-gray-700 font-medium">Privacy Policy</span>
                </div>
            </div>

            {/* Header */}
            <div className="bg-gray-50 border-b py-12">
                <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8">
                    <h1 className="text-3xl font-bold text-gray-900">Privacy Policy</h1>
                    <p className="mt-2 text-sm text-gray-500">Last updated: March 2026</p>
                </div>
            </div>

            <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
                <p className="text-gray-600 text-sm leading-relaxed mb-10">
                    ENYVORA ("we", "our", "us") is committed to protecting your privacy. This Privacy Policy explains how we collect, use, and safeguard your information when you use our Platform. By using ENYVORA, you agree to the practices described in this policy.
                </p>

                <div className="space-y-8">
                    {SECTIONS.map((section, i) => (
                        <div key={i}>
                            <h2 className="text-lg font-bold text-gray-900 mb-3">{section.title}</h2>
                            <p className="text-gray-600 leading-relaxed text-sm">{section.content}</p>
                        </div>
                    ))}
                </div>

                <div className="border-t border-gray-100 pt-8 mt-10 text-sm text-gray-500">
                    <p>For privacy concerns, <Link to="/contact-us" className="text-indigo-600 hover:underline">contact us</Link> or email <a href="mailto:privacy@enyvora.com" className="text-indigo-600 hover:underline">privacy@enyvora.com</a>.</p>
                </div>
            </div>
        </div>
    );
}
