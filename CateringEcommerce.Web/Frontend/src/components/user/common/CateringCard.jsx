/*
========================================
File: src/components/common/CateringCard.jsx (NEW FILE)
========================================
A card component to display catering service details in a grid.
*/
import React from 'react';
import { Link } from 'react-router-dom';

export default function CateringCard({ catering }) {
    return (
        <Link
            to={`/caterings/${catering.id}`}
            className="block bg-white rounded-2xl overflow-hidden shadow-lg hover:shadow-xl transition-all duration-300 transform hover:-translate-y-1"
        >
            <div className="relative aspect-[4/3]">
                <img
                    src={catering.coverImage}
                    alt={catering.name}
                    className="w-full h-full object-cover"
                />
                {catering.isOpen ? (
                    <span className="absolute top-3 left-3 bg-green-600 text-white text-xs font-bold px-2 py-1 rounded-full">
                        OPEN
                    </span>
                ) : (
                    <span className="absolute top-3 left-3 bg-neutral-500 text-white text-xs font-bold px-2 py-1 rounded-full">
                        CLOSED
                    </span>
                )}
                {catering.offer && (
                    <span className="absolute bottom-3 left-3 bg-blue-600 text-white text-xs font-bold px-2 py-1 rounded-md shadow-sm">
                        {catering.offer}
                    </span>
                )}
            </div>

            <div className="p-4">
                <div className="flex justify-between items-start mb-1">
                    <h3 className="text-lg font-bold text-neutral-900 truncate pr-2">{catering.name}</h3>
                    <div className="flex items-center bg-green-700 text-white text-xs px-1.5 py-0.5 rounded">
                        <span className="font-bold mr-1">{catering.rating}</span>
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-3 w-3 fill-current" viewBox="0 0 20 20"><path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" /></svg>
                    </div>
                </div>

                <p className="text-sm text-neutral-500 truncate mb-3">{catering.cuisines.join(', ')}</p>

                <div className="flex items-center justify-between text-xs text-neutral-500 border-t pt-3">
                    <span className="flex items-center">
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-3.5 w-3.5 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" /><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" /></svg>
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