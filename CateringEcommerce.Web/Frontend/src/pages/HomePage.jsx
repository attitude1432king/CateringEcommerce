/*
========================================
File: src/components/HomePage.jsx (NEW FILE - Extracted from App.jsx)
========================================
The original homepage content is now in its own component.
*/
// File: src/pages/HomePage.jsx (UPDATED)
import React from 'react';
import HeroBanner from '../components/user/HeroBanner';
import RecommendationsStrip from '../components/user/RecommendationsStrip';
import CategoryTiles from '../components/user/CategoryTiles';
import CatererGrid from '../components/user/common/CateringGrid';
import PromotionsSection from '../components/user/PromotionsSection';
import Testimonials from '../components/user/Testimonials';

export default function HomePage() {
    return (
        <main className="min-h-screen bg-white">
            {/* Hero Section - Full width premium experience */}
            <HeroBanner onSearch={(v) => console.log('search', v)} />

            {/* Service Categories - Premium cards section */}
            <CategoryTiles />

            {/* Featured Caterers Section */}
            <section className="py-16 md:py-24 px-4 sm:px-6 lg:px-8 bg-white">
                <div className="max-w-7xl mx-auto">
                    <div className="flex flex-col md:flex-row items-center justify-between mb-12">
                        <div>
                            <h2 className="section-title text-neutral-900 mb-4">
                                Trending Caterers Near You
                            </h2>
                            <p className="text-lg text-neutral-600">
                                Top-rated catering partners ready to bring your events to life
                            </p>
                        </div>
                        <a href="/browse" className="btn-secondary mt-6 md:mt-0">
                            View All
                        </a>
                    </div>
                    <CatererGrid />
                </div>
            </section>

            {/* Recommendations/Popular filters section */}
            <section className="py-16 md:py-20 px-4 sm:px-6 lg:px-8 bg-gradient-to-b from-catering-light to-white">
                <div className="max-w-7xl mx-auto">
                    <h2 className="section-title text-neutral-900 mb-6 text-center">
                        Popular Searches
                    </h2>
                    <RecommendationsStrip />
                </div>
            </section>

            {/* Promotions Section */}
            <PromotionsSection />

            {/* Testimonials Section */}
            <section className="py-16 md:py-24 px-4 sm:px-6 lg:px-8 bg-white border-t border-neutral-100">
                <div className="max-w-7xl mx-auto">
                    <div className="text-center mb-12">
                        <h2 className="section-title text-neutral-900 mb-4">
                            What Our Customers Say
                        </h2>
                        <p className="text-lg text-neutral-600">
                            Join thousands of satisfied event organizers
                        </p>
                    </div>
                    <Testimonials />
                </div>
            </section>

            {/* Bottom CTA Section */}
            <section className="py-20 px-4 sm:px-6 lg:px-8 bg-gradient-catering text-white">
                <div className="max-w-4xl mx-auto text-center">
                    <h2 className="text-4xl md:text-5xl font-extrabold mb-6">
                        Ready to Plan Your Perfect Event?
                    </h2>
                    <p className="text-xl text-white/90 mb-10">
                        Get connected with the best caterers in your area. Start your search today.
                    </p>
                    <button
                        onClick={() => window.scrollTo({ top: 0, behavior: 'smooth' })}
                        className="inline-flex items-center bg-white text-catering-primary px-8 py-4 rounded-xl font-semibold shadow-lg hover:shadow-card-hover transform hover:scale-105 transition-all duration-200"
                    >
                        <svg className="w-6 h-6 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
                        </svg>
                        Find Caterers Now
                    </button>
                </div>
            </section>
        </main>
    );
}