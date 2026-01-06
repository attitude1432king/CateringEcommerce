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
import { useDefaultCity } from '../hooks/useDefaultCity';

export default function HomePage() {
    const cityData = useDefaultCity();
    console.log(cityData);
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
                    <CatererGrid/>
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
        </main>
    );
}