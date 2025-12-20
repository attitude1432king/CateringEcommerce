/*
========================================
File: src/components/HomePage.jsx (NEW FILE - Extracted from App.jsx)
========================================
The original homepage content is now in its own component.
*/
import React from 'react';
import HeroSection from '../components/user/HeroSection';
import CuisineBrowseSection from '../components/user/CuisineBrowseSection';
import FeaturedCaterersSection from '../components/user/FeaturedCaterersSection';
import HowItWorksSection from '../components/user/HowItWorksSection';
import PromotionsSection from '../components/user/PromotionsSection';

export default function HomePage() {
    // Mock Data
    const cuisinesData = [
        { id: 1, name: "Indian Delights", image: "https://placehold.co/300x200/FFEDD5/BE185D?text=Indian+Feast" },
        { id: 2, name: "Italian Classics", image: "https://placehold.co/300x200/FEF3C7/BE185D?text=Italian+Pasta" },
        { id: 3, name: "Continental Spread", image: "https://placehold.co/300x200/FFF7ED/BE185D?text=Continental+Buffet" },
        { id: 4, name: "Party Platters", image: "https://placehold.co/300x200/FEF9C3/BE185D?text=Party+Snacks" },
        { id: 5, name: "Asian Fusion", image: "https://placehold.co/300x200/FFF1F2/BE185D?text=Asian+Noodles" },
        { id: 6, name: "BBQ & Grill", image: "https://placehold.co/300x200/FCE7F3/BE185D?text=Sizzling+BBQ" },
    ];

    const featuredCaterersData = [
        { id: 1, name: "Gourmet Gatherings", image: "https://placehold.co/400x250/FEF3C7/86198F?text=Gourmet+Events", cuisines: "Italian, Continental", rating: 4.8, reviews: 120 },
        { id: 2, name: "Spice Route Catering", image: "https://placehold.co/400x250/FFF7ED/86198F?text=Exotic+Spices", cuisines: "Indian, Asian", rating: 4.9, reviews: 250 },
        { id: 3, name: "The Eventful Plate", image: "https://placehold.co/400x250/FFEDD5/86198F?text=Memorable+Plates", cuisines: "Party Platters, BBQ", rating: 4.7, reviews: 95 },
    ];

    const howItWorksStepsData = [
        { id: 1, title: "Discover Caterers", description: "Find top-rated caterers near you for any occasion.", icon: "🔍" },
        { id: 2, title: "Customize Menu", description: "Browse menus and tailor your order to perfection.", icon: "📝" },
        { id: 3, title: "Easy Ordering", description: "Place your order online in just a few clicks.", icon: "💳" },
        { id: 4, title: "Enjoy Your Feast", description: "Relax while delicious food is prepared and delivered.", icon: "🎉" },
    ];

    return (
        <main>
            <HeroSection />
            <CuisineBrowseSection cuisines={cuisinesData} />
            <FeaturedCaterersSection caterers={featuredCaterersData} />
            <HowItWorksSection steps={howItWorksStepsData} />
            <PromotionsSection />
        </main>
    );
}