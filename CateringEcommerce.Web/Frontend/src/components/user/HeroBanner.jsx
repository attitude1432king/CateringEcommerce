import React, { useRef, useEffect, useState } from 'react';
import { motion } from 'framer-motion';

export default function HeroBanner({ onSearch }) {
    const locationInputRef = useRef(null);
    const [locationTerm, setLocationTerm] = useState('');
    const [cateringSearch, setCateringSearch] = useState('');
    const [showSuggestions, setShowSuggestions] = useState(false);
    const suggestionsRef = useRef(null);

    // Popular search suggestions for live preview
    const popularSearches = [
        { term: 'Wedding Catering', icon: '💒', description: 'Elegant wedding services' },
        { term: 'Corporate Events', icon: '🏢', description: 'Professional business catering' },
        { term: 'Birthday Parties', icon: '🎂', description: 'Celebrate with delicious food' },
        { term: 'Indian Cuisine', icon: '🍛', description: 'Authentic Indian flavors' },
        { term: 'BBQ & Grill', icon: '🍖', description: 'Grilled specialties' },
        { term: 'Vegan Options', icon: '🥗', description: 'Plant-based catering' },
    ];

    // Filter suggestions based on search input
    const filteredSuggestions = cateringSearch
        ? popularSearches.filter(item =>
            item.term.toLowerCase().includes(cateringSearch.toLowerCase())
        )
        : popularSearches;

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

    // Click outside to close suggestions
    useEffect(() => {
        const handleClickOutside = (event) => {
            if (suggestionsRef.current && !suggestionsRef.current.contains(event.target)) {
                setShowSuggestions(false);
            }
        };

        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, []);

    const handleSubmit = (e) => {
        e.preventDefault();
        if (onSearch) {
            onSearch({
                location: locationTerm,
                cateringSearch,
            });
        }
        setShowSuggestions(false);
    };

    const handleSuggestionClick = (suggestion) => {
        setCateringSearch(suggestion.term);
        setShowSuggestions(false);
        if (onSearch) {
            onSearch({
                location: locationTerm,
                cateringSearch: suggestion.term,
            });
        }
    };

    return (
        <section className="relative w-full h-screen min-h-[500px] md:min-h-[700px] lg:min-h-[800px] flex items-center justify-center overflow-hidden">
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

            {/* Premium gradient overlay for elegance */}
            <div className="absolute inset-0 bg-gradient-to-br from-black/60 via-black/50 to-gray-900/60" aria-hidden="true" />

            {/* Subtle gold accent glow */}
            <div className="absolute inset-0 bg-gradient-to-t from-catering-accent/10 via-transparent to-transparent" aria-hidden="true" />

            {/* Content Container */}
            <div className="relative z-10 w-full max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
                {/* Luxury badge */}
                <motion.div
                    initial={{ opacity: 0, y: -20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.8 }}
                    className="inline-flex items-center gap-2 px-4 py-2 bg-white/10 backdrop-blur-md border border-catering-accent/30 rounded-full mb-8"
                >
                    <span className="text-catering-accent text-lg">✨</span>
                    <span className="text-white text-sm font-medium tracking-wider">PREMIUM CATERING SERVICES</span>
                </motion.div>

                {/* Main Headline with premium typography */}
                <motion.h1
                    initial={{ opacity: 0, y: 30 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.8, delay: 0.2 }}
                    className="text-5xl md:text-6xl lg:text-7xl font-bold mb-6 leading-tight"
                >
                    <span className="block text-white drop-shadow-2xl">Exquisite Catering</span>
                    <span className="block bg-gradient-to-r from-catering-accent via-yellow-300 to-catering-secondary bg-clip-text text-transparent drop-shadow-lg">
                        for Every Occasion
                    </span>
                </motion.h1>

                {/* Elegant Description */}
                <motion.p
                    initial={{ opacity: 0, y: 30 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.8, delay: 0.4 }}
                    className="text-lg md:text-xl text-white/95 mb-12 max-w-3xl mx-auto leading-relaxed font-light tracking-wide"
                >
                    Elevate your weddings, corporate events, and celebrations with
                    <span className="text-catering-accent font-medium"> verified premium caterers</span>
                </motion.p>

                {/* Premium Search Form */}
                <motion.form
                    initial={{ opacity: 0, y: 30 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.8, delay: 0.6 }}
                    onSubmit={handleSubmit}
                    className="mb-16 relative z-50"
                >
                    <div className="relative max-w-4xl mx-auto">
                        {/* Form Container with luxury styling */}
                        <div className="flex flex-col sm:flex-row gap-3 bg-white/95 backdrop-blur-xl p-3 rounded-2xl shadow-2xl border border-white/20 relative z-50">
                            {/* Location Input with icon */}
                            <div className="flex-1 relative">
                                <div className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400">
                                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                                    </svg>
                                </div>
                                <input
                                    ref={locationInputRef}
                                    type="text"
                                    placeholder="Enter your city"
                                    value={locationTerm}
                                    onChange={(e) => setLocationTerm(e.target.value)}
                                    className="w-full h-14 pl-12 pr-4 rounded-xl border-2 border-transparent bg-gray-50 placeholder-gray-500 text-gray-900 text-base focus:outline-none focus:border-catering-accent focus:bg-white transition-all"
                                    aria-label="Location"
                                />
                            </div>

                            {/* Catering Search Input with icon and suggestions */}
                            <div className="flex-1 relative z-[100]" ref={suggestionsRef}>
                                <div className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none z-10">
                                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                                    </svg>
                                </div>
                                <input
                                    type="text"
                                    placeholder="Search catering services"
                                    value={cateringSearch}
                                    onChange={(e) => {
                                        setCateringSearch(e.target.value);
                                        setShowSuggestions(true);
                                    }}
                                    onFocus={() => setShowSuggestions(true)}
                                    className="w-full h-14 pl-12 pr-4 rounded-xl border-2 border-transparent bg-gray-50 placeholder-gray-500 text-gray-900 text-base focus:outline-none focus:border-catering-accent focus:bg-white transition-all relative"
                                    aria-label="Catering search"
                                />

                                {/* Live Search Suggestions Dropdown */}
                                {showSuggestions && filteredSuggestions.length > 0 && (
                                    <motion.div
                                        initial={{ opacity: 0, y: -10 }}
                                        animate={{ opacity: 1, y: 0 }}
                                        className="absolute top-full left-0 right-0 mt-2 bg-white rounded-xl shadow-2xl border border-gray-200 overflow-hidden max-h-80 overflow-y-auto z-[9999]"
                                    >
                                        <div className="p-2">
                                            <div className="text-xs font-semibold text-gray-400 uppercase tracking-wider px-3 py-2">
                                                Popular Searches
                                            </div>
                                            {filteredSuggestions.map((suggestion, index) => (
                                                <button
                                                    key={index}
                                                    type="button"
                                                    onClick={() => handleSuggestionClick(suggestion)}
                                                    className="w-full flex items-center gap-3 px-3 py-3 hover:bg-gradient-to-r hover:from-catering-light hover:to-white rounded-lg transition-all group text-left"
                                                >
                                                    <span className="text-2xl flex-shrink-0">{suggestion.icon}</span>
                                                    <div className="flex-1 min-w-0">
                                                        <div className="text-sm font-semibold text-gray-900 group-hover:text-catering-primary">
                                                            {suggestion.term}
                                                        </div>
                                                        <div className="text-xs text-gray-500">
                                                            {suggestion.description}
                                                        </div>
                                                    </div>
                                                    <svg className="w-4 h-4 text-gray-400 group-hover:text-catering-accent flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                                                    </svg>
                                                </button>
                                            ))}
                                        </div>
                                    </motion.div>
                                )}
                            </div>

                            {/* Premium CTA Button */}
                            <button
                                type="submit"
                                className="h-14 px-8 md:px-10 bg-gradient-to-r from-catering-primary via-catering-secondary to-catering-accent text-white font-bold rounded-xl shadow-xl hover:shadow-2xl hover:shadow-catering-accent/30 transform hover:scale-105 transition-all duration-300 flex items-center justify-center gap-3 whitespace-nowrap relative overflow-hidden group"
                            >
                                {/* Shine effect */}
                                <span className="absolute inset-0 bg-gradient-to-r from-transparent via-white/20 to-transparent translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700"></span>

                                <svg className="w-5 h-5 relative z-10" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                                </svg>
                                <span className="hidden sm:inline relative z-10">Find Caterers</span>
                                <span className="sm:hidden relative z-10">Search</span>
                            </button>
                        </div>

                        {/* Decorative gold line below search */}
                        <div className="absolute -bottom-6 left-1/2 -translate-x-1/2 w-32 h-0.5 bg-gradient-to-r from-transparent via-catering-accent to-transparent"></div>
                    </div>
                </motion.form>

                {/* Trust Indicators with luxury styling */}
                <motion.div
                    initial={{ opacity: 0, y: 30 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.8, delay: 0.8 }}
                    className="flex flex-col sm:flex-row items-center justify-center gap-8 md:gap-12"
                >
                    <div className="flex items-center gap-3 group">
                        <div className="w-12 h-12 bg-white/10 backdrop-blur-md rounded-full flex items-center justify-center border border-catering-accent/30 group-hover:border-catering-accent transition-all">
                            <svg className="w-6 h-6 text-catering-accent" fill="currentColor" viewBox="0 0 20 20">
                                <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
                            </svg>
                        </div>
                        <div className="text-left">
                            <div className="text-xl font-bold text-white">500+</div>
                            <div className="text-sm text-white/80">Verified Caterers</div>
                        </div>
                    </div>

                    <div className="flex items-center gap-3 group">
                        <div className="w-12 h-12 bg-white/10 backdrop-blur-md rounded-full flex items-center justify-center border border-catering-accent/30 group-hover:border-catering-accent transition-all">
                            <svg className="w-6 h-6 text-catering-accent" fill="currentColor" viewBox="0 0 20 20">
                                <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                            </svg>
                        </div>
                        <div className="text-left">
                            <div className="text-xl font-bold text-white">4.8/5</div>
                            <div className="text-sm text-white/80">Average Rating</div>
                        </div>
                    </div>

                    <div className="flex items-center gap-3 group">
                        <div className="w-12 h-12 bg-white/10 backdrop-blur-md rounded-full flex items-center justify-center border border-catering-accent/30 group-hover:border-catering-accent transition-all">
                            <svg className="w-6 h-6 text-catering-accent" fill="currentColor" viewBox="0 0 20 20">
                                <path d="M2 10.5a1.5 1.5 0 113 0v6a1.5 1.5 0 01-3 0v-6zM6 10.333v5.43a2 2 0 001.106 1.79l.05.025A4 4 0 008.943 18h5.416a2 2 0 001.962-1.608l1.2-6A2 2 0 0015.56 8H12V4a2 2 0 00-2-2 1 1 0 00-1 1v.667a4 4 0 01-.8 2.4L6.8 7.933a4 4 0 00-.8 2.4z" />
                            </svg>
                        </div>
                        <div className="text-left">
                            <div className="text-xl font-bold text-white">50k+</div>
                            <div className="text-sm text-white/80">Happy Clients</div>
                        </div>
                    </div>
                </motion.div>
            </div>
        </section>
    );
}
