/*
========================================
File: src/pages/HomePage.jsx - Premium Luxury Redesign
========================================
*/
import React from 'react';
import { useNavigate } from 'react-router-dom';
import HeroBanner from '../components/user/HeroBanner';
import AnimatedStats from '../components/user/AnimatedStats';
import CategoryTiles from '../components/user/CategoryTiles';
import FeaturedCaterers from '../components/user/FeaturedCaterers';
import HowItWorksSection from '../components/user/HowItWorksSection';
import Testimonials from '../components/user/Testimonials';
import { useDefaultCity } from '../hooks/useDefaultCity';

export default function HomePage() {
    const cityData = useDefaultCity();
    const navigate = useNavigate();
    console.log(cityData);

    /**
     * Handle search from HeroBanner
     * Navigates to catering list page with search parameters
     */
    const handleSearch = (searchData) => {
        console.log('Search triggered:', searchData);

        // Extract city from location string (assuming format like "Mumbai, Maharashtra, India")
        let city = searchData.location || '';
        if (city && city.includes(',')) {
            city = city.split(',')[0].trim();
        }

        // Build search params
        const params = new URLSearchParams();

        if (city) {
            params.append('city', city);
        }

        if (searchData.cateringSearch) {
            params.append('keyword', searchData.cateringSearch);
        }

        // Navigate to catering list page with search params
        navigate(`/caterings?${params.toString()}`);
    };

    return (
        <main className="min-h-screen bg-white">
            {/* Hero Section - Full width premium experience with video background */}
            <HeroBanner onSearch={handleSearch} />

            {/* Animated Stats Section - Trust indicators */}
            <AnimatedStats />

            {/* Service Categories - Premium luxury cards */}
            <CategoryTiles />

            {/* How It Works Section - Premium step-by-step guide */}
            <HowItWorksSection />

            {/* Featured Caterers Section - Premium caterer showcase */}
            <FeaturedCaterers />

            {/* Testimonials Section - Premium customer reviews */}
            <Testimonials />
        </main>
    );
}
