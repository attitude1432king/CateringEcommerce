/*
========================================
File: src/pages/CateringListPage.jsx (NEW FILE)
========================================
Displays the grid of catering services.
*/
import React, { useState, useEffect } from 'react';
import CateringCard from '../components/user/common/CateringCard';
import Loader from '../components/common/Loader';
// import { apiService } from '../services/api';

// Mock Data
const mockCaterings = [
    { id: 1, name: "The Royal Feast", coverImage: "https://placehold.co/600x400/ffedd5/9a3412?text=Royal+Feast", cuisines: ["North Indian", "Chinese"], rating: 4.5, location: "Adajan", distance: 2.5, priceRange: "₹300-800", isOpen: true, offer: "20% OFF" },
    { id: 2, name: "Spice Route", coverImage: "https://placehold.co/600x400/ecfccb/3f6212?text=Spice+Route", cuisines: ["South Indian", "Biryani"], rating: 4.2, location: "Vesu", distance: 5.0, priceRange: "₹200-600", isOpen: true, offer: "" },
    { id: 3, name: "Green Leaf Catering", coverImage: "https://placehold.co/600x400/d1fae5/065f46?text=Green+Leaf", cuisines: ["Gujarati", "Rajasthani"], rating: 4.8, location: "City Light", distance: 1.2, priceRange: "₹400-900", isOpen: false, offer: "Free Dessert" },
    { id: 4, name: "Urban Platter", coverImage: "https://placehold.co/600x400/e0e7ff/3730a3?text=Urban+Platter", cuisines: ["Continental", "Italian"], rating: 4.0, location: "Piplod", distance: 8.5, priceRange: "₹500-1200", isOpen: true, offer: "" },
];

export default function CateringListPage() {
    const [caterings, setCaterings] = useState([]);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        // Simulate API call
        setTimeout(() => {
            setCaterings(mockCaterings);
            setIsLoading(false);
        }, 800);
    }, []);

    if (isLoading) return <div className="h-screen flex justify-center items-center"><Loader /></div>;

    return (
        <div className="container mx-auto px-4 py-8">
            <h1 className="text-2xl md:text-3xl font-bold text-neutral-800 mb-6">Catering Services Near You</h1>

            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                {caterings.map(catering => (
                    <CateringCard key={catering.id} catering={catering} />
                ))}
            </div>
        </div>
    );
}
