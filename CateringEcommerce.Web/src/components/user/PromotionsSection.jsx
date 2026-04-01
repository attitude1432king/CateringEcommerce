
/*
========================================
File: src/components/PromotionsSection.jsx
========================================
*/
import React from 'react';

export default function PromotionsSection() {
    return (
        <section className="py-12 md:py-16 bg-rose-50">
            <div className="container mx-auto px-4 sm:px-6 lg:px-8">
                <h2 className="text-3xl font-bold text-rose-700 text-center mb-10">Special Offers & Promotions</h2>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                    <a href="#" className="block rounded-lg overflow-hidden shadow-lg hover:shadow-xl transition-shadow">
                        <img src="https://placehold.co/600x300/DB2777/FFFFFF?text=Weekend+Party+Special+-+20%25+OFF" alt="Promotion 1" className="w-full h-64 object-cover" />
                    </a>
                    <a href="#" className="block rounded-lg overflow-hidden shadow-lg hover:shadow-xl transition-shadow">
                        <img src="https://placehold.co/600x300/FDF2F8/DB2777?text=Corporate+Lunch+Deals" alt="Promotion 2" className="w-full h-64 object-cover" />
                    </a>
                </div>
            </div>
        </section>
    );
}

