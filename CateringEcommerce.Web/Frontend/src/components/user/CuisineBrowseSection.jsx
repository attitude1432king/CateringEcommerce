
/*
========================================
File: src/components/CuisineBrowseSection.jsx
========================================
*/
import React from 'react';

export default function CuisineBrowseSection({ cuisines }) {
    return (
        <section className="py-12 md:py-16 bg-white">
            <div className="container mx-auto px-4 sm:px-6 lg:px-8">
                <h2 className="text-3xl font-bold text-neutral-800 text-center mb-10">Browse by Cuisine</h2>
                <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-4 md:gap-6">
                    {cuisines.map(cuisine => (
                        <a key={cuisine.id} href="#" className="group block text-center p-2 rounded-lg hover:bg-amber-100 transition-colors">
                            <img src={cuisine.image} alt={cuisine.name} className="w-full h-32 sm:h-40 object-cover rounded-md mb-3 group-hover:opacity-90 transition-opacity" />
                            <h3 className="text-md font-semibold text-neutral-700 group-hover:text-rose-600">{cuisine.name}</h3>
                        </a>
                    ))}
                </div>
            </div>
        </section>
    );
}
