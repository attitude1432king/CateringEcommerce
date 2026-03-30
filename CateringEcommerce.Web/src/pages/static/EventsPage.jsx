import React from 'react';
import { Link } from 'react-router-dom';

const eventTypes = [
    {
        title: 'Weddings',
        description: 'Make your special day unforgettable with elegant multi-course menus, live counters, and dedicated service staff.',
        icon: (
            <svg className="w-7 h-7 text-catering-primary" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
            </svg>
        ),
    },
    {
        title: 'Birthday Parties',
        description: 'Customised menus for milestone birthdays — from intimate gatherings to lavish celebrations.',
        icon: (
            <svg className="w-7 h-7 text-catering-primary" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 15.546c-.523 0-1.046.151-1.5.454a2.704 2.704 0 01-3 0 2.704 2.704 0 00-3 0 2.704 2.704 0 01-3 0 2.704 2.704 0 00-3 0 2.704 2.704 0 01-3 0 2.701 2.701 0 00-1.5-.454M9 6v2m3-2v2m3-2v2M9 3h.01M12 3h.01M15 3h.01M21 21v-7a2 2 0 00-2-2H5a2 2 0 00-2 2v7h18z" />
            </svg>
        ),
    },
    {
        title: 'Corporate Events',
        description: 'Professional catering for conferences, team outings, product launches, and annual dinners.',
        icon: (
            <svg className="w-7 h-7 text-catering-primary" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
            </svg>
        ),
    },
    {
        title: 'Engagements & Receptions',
        description: 'Celebrate love with curated menus that impress guests and set the perfect tone for the occasion.',
        icon: (
            <svg className="w-7 h-7 text-catering-primary" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z" />
            </svg>
        ),
    },
    {
        title: 'Baby Showers & Naming Ceremonies',
        description: 'Warm, joyful setups with thoughtfully designed menus for your precious milestones.',
        icon: (
            <svg className="w-7 h-7 text-catering-primary" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
        ),
    },
    {
        title: 'Festivals & Cultural Events',
        description: 'Authentic regional cuisine and large-scale setups for community festivals and cultural gatherings.',
        icon: (
            <svg className="w-7 h-7 text-catering-primary" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3.055 11H5a2 2 0 012 2v1a2 2 0 002 2 2 2 0 012 2v2.945M8 3.935V5.5A2.5 2.5 0 0010.5 8h.5a2 2 0 012 2 2 2 0 104 0 2 2 0 012-2h1.064M15 20.488V18a2 2 0 012-2h3.064M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
        ),
    },
];

const highlights = [
    { label: '500+', description: 'Verified Catering Partners' },
    { label: '50,000+', description: 'Events Successfully Served' },
    { label: '4.8★', description: 'Average Customer Rating' },
    { label: '100+', description: 'Cities Covered' },
];

export default function EventsPage() {
    return (
        <div className="min-h-screen bg-white">
            {/* Breadcrumb */}
            <div className="bg-gray-50 border-b">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-3 text-sm text-gray-500">
                    <Link to="/" className="hover:text-gray-700">Home</Link>
                    <span className="mx-2">/</span>
                    <span className="text-gray-700 font-medium">Events</span>
                </div>
            </div>

            {/* Hero */}
            <div className="bg-gradient-to-br from-orange-50 to-white py-16">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
                    <h1 className="text-4xl md:text-5xl font-bold text-gray-900">
                        Catering for Every <span className="text-catering-primary">Occasion</span>
                    </h1>
                    <p className="mt-4 text-lg text-gray-600 max-w-2xl mx-auto">
                        From intimate family gatherings to grand weddings — ENYVORA connects you with the perfect catering partner for your event.
                    </p>
                    <div className="mt-8 flex flex-col sm:flex-row gap-4 justify-center">
                        <Link
                            to="/caterings"
                            className="inline-flex items-center justify-center gap-2 btn-primary px-8 py-3 text-base font-semibold"
                        >
                            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                            </svg>
                            Browse Caterers
                        </Link>
                        <a
                            href="#event-types"
                            className="inline-flex items-center justify-center gap-2 px-8 py-3 text-base font-semibold border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 transition-colors"
                        >
                            Explore Event Types
                        </a>
                    </div>
                </div>
            </div>

            {/* Stats */}
            <div className="border-y border-gray-100 bg-white">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
                    <div className="grid grid-cols-2 md:grid-cols-4 gap-8 text-center">
                        {highlights.map((h) => (
                            <div key={h.label}>
                                <div className="text-3xl font-bold text-catering-primary">{h.label}</div>
                                <div className="mt-1 text-sm text-gray-500">{h.description}</div>
                            </div>
                        ))}
                    </div>
                </div>
            </div>

            {/* Event Types */}
            <div id="event-types" className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
                <div className="text-center mb-12">
                    <h2 className="text-3xl font-bold text-gray-900">What Kind of Event Are You Planning?</h2>
                    <p className="mt-3 text-gray-500 max-w-xl mx-auto">
                        We cater to all types of events. Pick your occasion and find the right partner.
                    </p>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                    {eventTypes.map((event) => (
                        <Link
                            key={event.title}
                            to="/caterings"
                            className="group bg-white border border-gray-100 rounded-2xl p-6 shadow-sm hover:shadow-md hover:border-catering-primary/30 transition-all"
                        >
                            <div className="w-12 h-12 bg-orange-50 rounded-xl flex items-center justify-center mb-4 group-hover:bg-orange-100 transition-colors">
                                {event.icon}
                            </div>
                            <h3 className="text-lg font-semibold text-gray-900 mb-2">{event.title}</h3>
                            <p className="text-sm text-gray-500 leading-relaxed">{event.description}</p>
                            <div className="mt-4 text-catering-primary text-sm font-medium flex items-center gap-1 group-hover:gap-2 transition-all">
                                Find caterers
                                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                                </svg>
                            </div>
                        </Link>
                    ))}
                </div>
            </div>

            {/* CTA Banner */}
            <div className="bg-gradient-to-r from-catering-primary to-orange-500 py-16">
                <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 text-center text-white">
                    <h2 className="text-3xl font-bold mb-4">Ready to Plan Your Event?</h2>
                    <p className="text-orange-100 mb-8 text-lg">
                        Browse verified catering partners, compare packages, and book in minutes.
                    </p>
                    <Link
                        to="/caterings"
                        className="inline-flex items-center justify-center gap-2 bg-white text-catering-primary font-semibold px-8 py-3 rounded-xl hover:bg-orange-50 transition-colors"
                    >
                        Get Started
                        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 8l4 4m0 0l-4 4m4-4H3" />
                        </svg>
                    </Link>
                </div>
            </div>
        </div>
    );
}
