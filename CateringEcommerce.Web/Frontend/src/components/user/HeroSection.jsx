
/*
========================================
File: src/components/HeroSection.jsx
========================================
*/
import React from 'react';

export default function HeroSection() {
    return (
        <div className="hero-bg text-white relative">
            <div className="absolute inset-0 bg-black opacity-40"></div>
            <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-24 md:py-32 lg:py-48 text-center relative z-10">
                <h1 className="text-4xl sm:text-5xl md:text-6xl font-extrabold tracking-tight">
                    Order Catering for <span className="text-amber-300">Any Occasion</span>
                </h1>
                <p className="mt-6 max-w-xl mx-auto text-lg sm:text-xl text-neutral-200">
                    Discover top local caterers, explore diverse menus, and book effortlessly for your next event.
                </p>
                <div className="mt-10 max-w-2xl mx-auto">
                    <form className="sm:flex">
                        <input
                            type="text"
                            placeholder="Enter delivery address or event location"
                            className="w-full px-5 py-3 placeholder-neutral-500 focus:ring-rose-500 focus:border-rose-500 sm:max-w-xs border-neutral-300 rounded-md text-neutral-900"
                        />
                        <button
                            type="submit"
                            className="mt-3 w-full sm:mt-0 sm:ml-3 sm:w-auto sm:flex-shrink-0 bg-rose-600 hover:bg-rose-700 border border-transparent rounded-md py-3 px-5 flex items-center justify-center text-base font-medium text-white focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-rose-500"
                        >
                            Find Caterers
                        </button>
                    </form>
                </div>
            </div>
        </div>
    );
}
