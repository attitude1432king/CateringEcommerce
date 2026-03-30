import React from 'react';
import { Link } from 'react-router-dom';

const TIPS = [
    {
        icon: '📸',
        title: 'Profile Optimization',
        tips: [
            'Upload high-quality photos of your best dishes and event setups.',
            'Write a clear, compelling business description highlighting your specialties.',
            'Keep your menu updated with accurate descriptions and pricing.',
            'Respond to customer inquiries quickly to improve your response rate score.',
        ],
    },
    {
        icon: '💲',
        title: 'Pricing Strategy',
        tips: [
            'Offer tiered packages (Basic, Standard, Premium) to attract a wider range of budgets.',
            'Include sample tasting options to build trust with first-time customers.',
            'Review competitor pricing periodically and stay competitive.',
            'Consider seasonal discounts during off-peak months to maintain steady bookings.',
        ],
    },
    {
        icon: '⭐',
        title: 'Customer Reviews',
        tips: [
            'Follow up with customers after events to encourage them to leave reviews.',
            'Respond to all reviews — positive and negative — in a professional tone.',
            "Address issues raised in negative reviews and show how you've improved.",
            'Showcase positive testimonials in your profile description.',
        ],
    },
    {
        icon: '🎉',
        title: 'Seasonal Promotions',
        tips: [
            'Create special packages for wedding season (November–February in India).',
            'Offer festive menus for Diwali, Christmas, New Year events.',
            'Promote corporate lunch packages during business conference seasons.',
            'Use your dashboard banners feature to highlight limited-time offers.',
        ],
    },
];

export default function GrowthResources() {
    return (
        <div className="min-h-screen bg-white">
            {/* Breadcrumb */}
            <div className="bg-gray-50 border-b">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-3 text-sm text-gray-500">
                    <Link to="/" className="hover:text-gray-700">Home</Link>
                    <span className="mx-2">/</span>
                    <span className="text-gray-700 font-medium">Growth Resources</span>
                </div>
            </div>

            {/* Hero */}
            <div className="bg-gradient-to-br from-yellow-50 to-white py-16">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
                    <h1 className="text-4xl font-bold text-gray-900">Growth Resources for Partners</h1>
                    <p className="mt-4 text-lg text-gray-600 max-w-2xl mx-auto">
                        Practical tips to grow your bookings, improve your ratings, and build a stronger catering brand on ENYVORA.
                    </p>
                </div>
            </div>

            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                    {TIPS.map((section, i) => (
                        <div key={i} className="bg-white border border-gray-100 rounded-2xl p-8 shadow-sm">
                            <div className="text-3xl mb-3">{section.icon}</div>
                            <h3 className="text-lg font-bold text-gray-900 mb-4">{section.title}</h3>
                            <ul className="space-y-3">
                                {section.tips.map((tip, j) => (
                                    <li key={j} className="flex gap-3 text-sm text-gray-600 leading-relaxed">
                                        <span className="text-yellow-500 font-bold shrink-0 mt-0.5">→</span>
                                        {tip}
                                    </li>
                                ))}
                            </ul>
                        </div>
                    ))}
                </div>

                <div className="mt-12 bg-yellow-50 rounded-2xl p-10 text-center">
                    <h3 className="text-xl font-bold text-gray-900 mb-2">Ready to grow?</h3>
                    <p className="text-gray-600 mb-6">Apply these tips in your partner dashboard and start seeing results.</p>
                    <Link to="/partner-login" className="inline-block px-8 py-3 bg-yellow-500 text-white font-semibold rounded-xl hover:bg-yellow-600 transition-colors">
                        Go to Dashboard
                    </Link>
                </div>
            </div>
        </div>
    );
}
