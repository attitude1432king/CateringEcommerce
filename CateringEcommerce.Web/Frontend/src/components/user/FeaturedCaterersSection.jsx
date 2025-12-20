
/*
========================================
File: src/components/FeaturedCaterersSection.jsx
========================================
*/
import React from 'react';

export default function FeaturedCaterersSection({ caterers }) {
    return (
        <section className="py-12 md:py-16 bg-amber-50">
            <div className="container mx-auto px-4 sm:px-6 lg:px-8">
                <h2 className="text-3xl font-bold text-neutral-800 text-center mb-10">Featured Caterers</h2>
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
                    {caterers.map(caterer => (
                        <div key={caterer.id} className="bg-white rounded-lg shadow-lg overflow-hidden transform hover:scale-105 transition-transform duration-300">
                            <img src={caterer.image} alt={caterer.name} className="w-full h-56 object-cover" />
                            <div className="p-6">
                                <h3 className="text-xl font-semibold text-neutral-800 mb-1">{caterer.name}</h3>
                                <p className="text-sm text-neutral-500 mb-2">{caterer.cuisines}</p>
                                <div className="flex items-center mb-4">
                                    <span className="text-yellow-500">{'★'.repeat(Math.floor(caterer.rating))}{'☆'.repeat(5 - Math.floor(caterer.rating))}</span>
                                    <span className="ml-2 text-sm text-neutral-600">{caterer.rating} ({caterer.reviews} reviews)</span>
                                </div>
                                <a href="#" className="inline-block w-full text-center bg-rose-600 text-white px-6 py-2 rounded-md text-sm font-medium hover:bg-rose-700 transition-colors">
                                    View Menu
                                </a>
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </section>
    );
}