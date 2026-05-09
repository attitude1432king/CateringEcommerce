import React from 'react';
import { Link } from 'react-router-dom';

const features = [
    {
        title: 'Flexible Bulk Orders',
        description: 'Scale from 50 to 5,000 guests with ease. Our partners handle large volumes without compromising quality.',
        icon: (
            <svg className="w-7 h-7 text-catering-primary" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z" />
            </svg>
        ),
    },
    {
        title: 'GST Invoicing',
        description: 'Receive proper GST-compliant invoices for every order — hassle-free expense filing for your finance team.',
        icon: (
            <svg className="w-7 h-7 text-catering-primary" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
        ),
    },
    {
        title: 'Dedicated Account Manager',
        description: 'A single point of contact for planning, coordination, and on-day support — so you can focus on your event.',
        icon: (
            <svg className="w-7 h-7 text-catering-primary" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            </svg>
        ),
    },
    {
        title: 'Sample Tasting',
        description: 'Arrange a pre-event tasting session with your chosen caterer before locking in the final menu.',
        icon: (
            <svg className="w-7 h-7 text-catering-primary" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
        ),
    },
    {
        title: 'Multi-City Coordination',
        description: 'Running events across multiple cities? We coordinate with partners in each location under one booking.',
        icon: (
            <svg className="w-7 h-7 text-catering-primary" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
            </svg>
        ),
    },
    {
        title: 'Live Event Supervisors',
        description: 'On-site supervisors ensure the catering runs smoothly — from setup to cleanup — at every corporate event.',
        icon: (
            <svg className="w-7 h-7 text-catering-primary" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4" />
            </svg>
        ),
    },
];

const useCases = [
    'Annual Company Dinners',
    'Team Offsites & Retreats',
    'Product Launch Events',
    'Conference & Seminars',
    'Client Entertainment',
    'Office Celebrations',
    'Award Ceremonies',
    'Training Day Meals',
];

export default function CorporatePage() {
    return (
        <div className="min-h-screen bg-white">
            {/* Breadcrumb */}
            <div className="bg-neutral-50 border-b">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-3 text-sm text-neutral-500">
                    <Link to="/" className="hover:text-neutral-700">Home</Link>
                    <span className="mx-2">/</span>
                    <span className="text-neutral-700 font-medium">Corporate Catering</span>
                </div>
            </div>

            {/* Hero */}
            <div className="bg-gradient-to-br from-blue-50 to-white py-16">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                    <div className="max-w-3xl">
                        <span className="inline-block text-sm font-semibold text-catering-primary bg-orange-50 border border-orange-200 px-3 py-1 rounded-full mb-4">
                            Corporate Services
                        </span>
                        <h1 className="text-4xl md:text-5xl font-bold text-neutral-900 leading-tight">
                            Premium Catering for <span className="text-catering-primary">Corporate Events</span>
                        </h1>
                        <p className="mt-4 text-lg text-neutral-600 leading-relaxed">
                            Impress clients, reward your team, and run seamless events with verified catering partners who understand corporate standards — quality, punctuality, and presentation.
                        </p>
                        <div className="mt-8 flex flex-col sm:flex-row gap-4">
                            <Link
                                to="/caterings"
                                className="inline-flex items-center justify-center gap-2 btn-primary px-8 py-3 text-base font-semibold"
                            >
                                Find Corporate Caterers
                                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 8l4 4m0 0l-4 4m4-4H3" />
                                </svg>
                            </Link>
                            <Link
                                to="/contact-us"
                                className="inline-flex items-center justify-center gap-2 px-8 py-3 text-base font-semibold border border-neutral-300 rounded-lg text-neutral-700 hover:bg-neutral-50 transition-colors"
                            >
                                Talk to Us
                            </Link>
                        </div>
                    </div>
                </div>
            </div>

            {/* Features */}
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
                <div className="text-center mb-12">
                    <h2 className="text-3xl font-bold text-neutral-900">Everything Your Corporate Event Needs</h2>
                    <p className="mt-3 text-neutral-500 max-w-xl mx-auto">
                        Purpose-built features for businesses and event organizers.
                    </p>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                    {features.map((feature) => (
                        <div key={feature.title} className="bg-white border border-gray-100 rounded-2xl p-6 shadow-sm">
                            <div className="w-12 h-12 bg-orange-50 rounded-xl flex items-center justify-center mb-4">
                                {feature.icon}
                            </div>
                            <h3 className="text-lg font-semibold text-neutral-900 mb-2">{feature.title}</h3>
                            <p className="text-sm text-neutral-500 leading-relaxed">{feature.description}</p>
                        </div>
                    ))}
                </div>
            </div>

            {/* Use Cases */}
            <div className="bg-neutral-50 py-16">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                    <div className="text-center mb-10">
                        <h2 className="text-3xl font-bold text-neutral-900">Trusted for All Corporate Occasions</h2>
                    </div>
                    <div className="flex flex-wrap justify-center gap-3">
                        {useCases.map((uc) => (
                            <span
                                key={uc}
                                className="px-4 py-2 bg-white border border-neutral-200 rounded-full text-sm font-medium text-neutral-700 shadow-sm"
                            >
                                {uc}
                            </span>
                        ))}
                    </div>
                </div>
            </div>

            {/* CTA Banner */}
            <div className="bg-gradient-to-r from-catering-primary to-orange-500 py-16">
                <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 text-center text-white">
                    <h2 className="text-3xl font-bold mb-4">Plan Your Next Corporate Event</h2>
                    <p className="text-orange-100 mb-8 text-lg">
                        Browse caterers, request quotes, and book with confidence — all in one place.
                    </p>
                    <Link
                        to="/caterings"
                        className="inline-flex items-center justify-center gap-2 bg-white text-catering-primary font-semibold px-8 py-3 rounded-xl hover:bg-primary/5 transition-colors"
                    >
                        Browse Caterers
                        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 8l4 4m0 0l-4 4m4-4H3" />
                        </svg>
                    </Link>
                </div>
            </div>
        </div>
    );
}
