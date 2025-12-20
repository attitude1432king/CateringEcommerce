/*
========================================
File: src/components/common/CateringCard.jsx (NEW FILE)
========================================
A card component to display catering service details in a grid.
*/
// File: src/components/user/CatererCard.jsx
import React from 'react';
import { Link } from 'react-router-dom';


export default function CatererCard({ catering }) {
    return (
        <Link
            to={`/caterings/${catering.id}`}
            className="block bg-white rounded-2xl overflow-hidden shadow
                 hover:shadow-xl transition-transform hover:-translate-y-1"
        >
            {/* IMAGE */}
            <div className="relative aspect-[3/2] bg-gray-100">
                <img
                    src={catering.coverImage}
                    alt={catering.name}
                    className="w-full h-full object-cover"
                    loading="lazy"
                />

                <span
                    className={`absolute top-3 left-3 text-xs font-bold px-2 py-1 rounded-full ${catering.isOpen
                            ? 'bg-green-600 text-white'
                            : 'bg-neutral-500 text-white'
                        }`}
                >
                    {catering.isOpen ? 'OPEN' : 'CLOSED'}
                </span>

                {catering.offer && (
                    <span className="absolute bottom-3 left-3 bg-indigo-600 text-white text-xs font-bold px-2 py-1 rounded-md">
                        {catering.offer}
                    </span>
                )}
            </div>

            {/* CONTENT */}
            <div className="p-4">
                <div className="flex justify-between items-start mb-1 gap-2">
                    <h3 className="text-lg font-semibold text-neutral-900 truncate">
                        {catering.name}
                    </h3>
                    <div className="bg-amber-100 text-amber-800 px-2 py-0.5 rounded text-xs font-semibold">
                        {catering.rating} ★
                    </div>
                </div>

                <p className="text-sm text-neutral-500 truncate mb-3">
                    {(catering.cuisines || []).join(', ')}
                </p>

                <div className="flex items-center justify-between text-xs text-neutral-500 border-t pt-3">
                    <span className="truncate">
                        {catering.location} • {catering.distance} km
                    </span>
                    <span className="font-medium text-neutral-700">
                        {catering.priceRange}
                    </span>
                </div>
            </div>
        </Link>
    );
}
