import React from 'react';
import { Link } from 'react-router-dom';

export default function AboutUs() {
    return (
        <div className="min-h-screen bg-white">
            {/* Breadcrumb */}
            <div className="bg-neutral-50 border-b">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-3 text-sm text-neutral-500">
                    <Link to="/" className="hover:text-neutral-700">Home</Link>
                    <span className="mx-2">/</span>
                    <span className="text-neutral-700 font-medium">About Us</span>
                </div>
            </div>

            {/* Hero */}
            <div className="bg-gradient-to-br from-indigo-50 to-white py-16">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
                    <h1 className="text-4xl font-bold text-neutral-900">About ENYVORA</h1>
                    <p className="mt-4 text-lg text-neutral-600 max-w-2xl mx-auto">
                        We connect exceptional catering businesses with people who want to make their events unforgettable.
                    </p>
                </div>
            </div>

            {/* Mission */}
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
                <div className="max-w-3xl mx-auto text-center mb-16">
                    <h2 className="text-2xl font-bold text-neutral-900 mb-4">Our Mission</h2>
                    <p className="text-neutral-600 leading-relaxed">
                        ENYVORA was founded with a simple belief — every celebration deserves extraordinary food. We built a platform that makes it effortless to discover, compare, and book premium catering services for any occasion, from intimate dinners to grand weddings.
                    </p>
                </div>

                {/* Feature cards */}
                <div className="grid grid-cols-1 md:grid-cols-3 gap-8 mb-16">
                    <div className="bg-white border border-gray-100 rounded-2xl p-8 shadow-sm text-center">
                        <div className="w-12 h-12 bg-indigo-100 rounded-full flex items-center justify-center mx-auto mb-4">
                            <svg className="w-6 h-6 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4M7.835 4.697a3.42 3.42 0 001.946-.806 3.42 3.42 0 014.438 0 3.42 3.42 0 001.946.806 3.42 3.42 0 013.138 3.138 3.42 3.42 0 00.806 1.946 3.42 3.42 0 010 4.438 3.42 3.42 0 00-.806 1.946 3.42 3.42 0 01-3.138 3.138 3.42 3.42 0 00-1.946.806 3.42 3.42 0 01-4.438 0 3.42 3.42 0 00-1.946-.806 3.42 3.42 0 01-3.138-3.138 3.42 3.42 0 00-.806-1.946 3.42 3.42 0 010-4.438 3.42 3.42 0 00.806-1.946 3.42 3.42 0 013.138-3.138z" />
                            </svg>
                        </div>
                        <h3 className="text-lg font-semibold text-neutral-900 mb-2">Premium Quality</h3>
                        <p className="text-neutral-500 text-sm">Every caterer on our platform is vetted and approved, ensuring top-quality food and service for your events.</p>
                    </div>
                    <div className="bg-white border border-gray-100 rounded-2xl p-8 shadow-sm text-center">
                        <div className="w-12 h-12 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
                            <svg className="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z" />
                            </svg>
                        </div>
                        <h3 className="text-lg font-semibold text-neutral-900 mb-2">Trusted Partners</h3>
                        <p className="text-neutral-500 text-sm">We work with certified catering businesses across the country, each committed to excellence and reliability.</p>
                    </div>
                    <div className="bg-white border border-gray-100 rounded-2xl p-8 shadow-sm text-center">
                        <div className="w-12 h-12 bg-orange-100 rounded-full flex items-center justify-center mx-auto mb-4">
                            <svg className="w-6 h-6 text-primary" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
                            </svg>
                        </div>
                        <h3 className="text-lg font-semibold text-neutral-900 mb-2">Seamless Experience</h3>
                        <p className="text-neutral-500 text-sm">From browsing to booking, our platform makes every step smooth — so you can focus on your event, not logistics.</p>
                    </div>
                </div>

                {/* Stats */}
                <div className="bg-gray-900 rounded-2xl p-10 text-center">
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
                        <div>
                            <p className="text-4xl font-bold text-white">1,000+</p>
                            <p className="text-neutral-400 mt-1">Verified Caterers</p>
                        </div>
                        <div>
                            <p className="text-4xl font-bold text-white">50,000+</p>
                            <p className="text-neutral-400 mt-1">Events Catered</p>
                        </div>
                        <div>
                            <p className="text-4xl font-bold text-white">4.8★</p>
                            <p className="text-neutral-400 mt-1">Average Rating</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}
