import React, { useRef, useEffect, useState } from 'react';

export default function HeroBanner({ onSearch }) {
    const locationInputRef = useRef(null);
    const [locationTerm, setLocationTerm] = useState('');
    const [cateringSearch, setCateringSearch] = useState('');

    useEffect(() => {
        if (window.google && window.google.maps && window.google.maps.places && locationInputRef.current) {
            try {
                const ac = new window.google.maps.places.Autocomplete(locationInputRef.current, { types: ['geocode'] });
                ac.setFields(['formatted_address', 'geometry', 'place_id']);
                ac.addListener('place_changed', () => {
                    const place = ac.getPlace();
                    if (onSearch) onSearch({ address: place.formatted_address, cateringSearch, place });
                });
            } catch (e) { /* ignore missing API */ }
        }
    }, [onSearch, cateringSearch]);

    const handleSubmit = (e) => {
        e.preventDefault();
        if (onSearch) {
            onSearch({
                location: locationTerm,
                cateringSearch,
            });
        }
    };

    return (
        <section className="relative w-full h-screen min-h-[500px] md:min-h-[600px] lg:min-h-[700px] flex items-center justify-center overflow-hidden">
            {/* Background Video */}
            <video
                autoPlay
                muted
                loop
                playsInline
                className="absolute inset-0 w-full h-full object-cover"
                aria-hidden="true"
            >
                <source src="/catering-hero.mp4" type="video/mp4" />
            </video>

            {/* Dark Overlay for Text Readability */}
            <div className="absolute inset-0 bg-black/45" aria-hidden="true" />

            {/* Content Container */}
            <div className="relative z-10 w-full max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 text-center">

                {/* Main Headline */}
                <h1 className="text-4xl md:text-5xl lg:text-6xl font-bold text-white mb-6 leading-tight drop-shadow-lg">
                    Catering for Every Occasion
                </h1>

                {/* Description */}
                <p className="text-base md:text-lg text-white/90 mb-10 max-w-3xl mx-auto leading-relaxed drop-shadow">
                    Weddings, Corporate Events, and Parties. Book verified caterers instantly.
                </p>

                {/* Search Form */}
                <form onSubmit={handleSubmit} className="mb-16">
                    {/* Form Container - Single Row on Desktop, Stack on Mobile */}
                    <div className="flex flex-col sm:flex-row gap-3 max-w-3xl mx-auto">
                        {/* Location Input */}
                        <input
                            ref={locationInputRef}
                            type="text"
                            placeholder="Enter your city"
                            value={locationTerm}
                            onChange={(e) => setLocationTerm(e.target.value)}
                            className="flex-1 h-12 px-4 rounded-lg border border-neutral-300 bg-white placeholder-neutral-500 text-neutral-900 text-sm md:text-base focus:outline-none focus:ring-2 focus:ring-[#FF6B35] focus:border-transparent transition-all"
                            aria-label="Location"
                        />

                        {/* Catering Search Input */}
                        <input
                            type="text"
                            placeholder="Search catering services"
                            value={cateringSearch}
                            onChange={(e) => setCateringSearch(e.target.value)}
                            className="flex-1 h-12 px-4 rounded-lg border border-neutral-300 bg-white placeholder-neutral-500 text-neutral-900 text-sm md:text-base focus:outline-none focus:ring-2 focus:ring-[#FF6B35] focus:border-transparent transition-all"
                            aria-label="Catering search"
                        />

                        {/* Primary CTA Button */}
                        <button
                            type="submit"
                            className="h-12 px-6 md:px-8 bg-gradient-to-r from-[#FF6B35] via-[#FF8C42] to-[#FFB627] text-white font-semibold rounded-lg shadow-lg hover:shadow-[0_12px_28px_rgba(0,0,0,0.12)] transform hover:scale-105 transition-all duration-200 flex items-center justify-center gap-2 whitespace-nowrap"
                        >
                            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                            </svg>
                            <span className="hidden sm:inline">Find Caterers</span>
                            <span className="sm:hidden">Search</span>
                        </button>
                    </div>
                </form>

                {/* Trust Indicators */}
                {/*<div className="flex flex-col sm:flex-row items-center justify-center gap-6 md:gap-10 pt-8">*/}
                {/*    <div className="flex items-center gap-2">*/}
                {/*        <svg className="w-5 h-5 text-white" fill="currentColor" viewBox="0 0 20 20">*/}
                {/*            <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />*/}
                {/*        </svg>*/}
                {/*        <span className="text-sm md:text-base font-medium text-white">500+ Caterers</span>*/}
                {/*    </div>*/}
                {/*    <div className="flex items-center gap-2">*/}
                {/*        <svg className="w-5 h-5 text-white" fill="currentColor" viewBox="0 0 20 20">*/}
                {/*            <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />*/}
                {/*        </svg>*/}
                {/*        <span className="text-sm md:text-base font-medium text-white">4.8/5 Rating</span>*/}
                {/*    </div>*/}
                {/*    <div className="flex items-center gap-2">*/}
                {/*        <svg className="w-5 h-5 text-white" fill="currentColor" viewBox="0 0 20 20">*/}
                {/*            <path fillRule="evenodd" d="M6.267 3.455a3.066 3.066 0 001.745-2.723V2H7a1 1 0 000 2v.252a3.06 3.06 0 002.68 3.056l.996.167.213.662a4 4 0 01-.956 4.29.5.5 0 00.146.668l1.416.7a.5.5 0 00.668-.147 5.5 5.5 0 00-1.313-5.884 1 1 0 00-.926-.176zM12 15a1 1 0 100 2h.01a1 1 0 100-2H12z" clipRule="evenodd" />*/}
                {/*        </svg>*/}
                {/*        <span className="text-sm md:text-base font-medium text-white">Verified</span>*/}
                {/*    </div>*/}
                {/*</div>*/}
            </div>
        </section>
    );
}