import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { getFeaturedCaterers } from '../../services/homeApi';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:44368';

export default function FeaturedCaterers() {
    const [featuredList, setFeaturedList] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        const loadFeaturedCaterers = async () => {
            try {
                setLoading(true);
                const response = await getFeaturedCaterers();
                if (response.success && response.data) {
                    setFeaturedList(response.data);
                }
            } catch (err) {
                console.error('Error loading featured caterers:', err);
                setError(err.message);
            } finally {
                setLoading(false);
            }
        };

        loadFeaturedCaterers();
    }, []);

    if (loading) {
        return (
            <section className="py-24 bg-white">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
                    <div className="text-lg text-gray-600">Loading featured caterers...</div>
                </div>
            </section>
        );
    }

    if (error) {
        return (
            <section className="py-24 bg-white">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
                    <div className="text-lg text-red-600">Error: {error}</div>
                </div>
            </section>
        );
    }

    if (featuredList.length === 0) {
        return null; // Don't show the section if there are no featured caterers
    }

    return (
        <section className="py-24 bg-white relative overflow-hidden">
            {/* Decorative elements */}
            <div className="absolute top-0 right-0 w-96 h-96 bg-catering-accent/5 rounded-full blur-3xl"></div>
            <div className="absolute bottom-0 left-0 w-96 h-96 bg-catering-primary/5 rounded-full blur-3xl"></div>

            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative z-10">
                {/* Section Header */}
                <motion.div
                    initial={{ opacity: 0, y: 30 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    transition={{ duration: 0.6 }}
                    className="text-center mb-16"
                >
                    <div className="inline-flex items-center gap-2 px-4 py-2 bg-gradient-to-r from-catering-accent/10 to-catering-primary/10 rounded-full mb-6">
                        <span className="text-catering-accent text-xl">👑</span>
                        <span className="text-catering-primary font-semibold text-sm uppercase tracking-wider">
                            Trending Now
                        </span>
                    </div>

                    <h2 className="text-4xl md:text-5xl font-bold text-gray-900 mb-6">
                        Featured{' '}
                        <span className="bg-gradient-to-r from-catering-primary to-catering-accent bg-clip-text text-transparent">
                            Premium Caterers
                        </span>
                    </h2>

                    <p className="text-lg text-gray-600 max-w-2xl mx-auto leading-relaxed">
                        Discover our handpicked selection of top-rated catering services
                    </p>
                </motion.div>

                {/* Caterers Grid */}
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
                    {featuredList.map((caterer, index) => (
                        <motion.div
                            key={caterer.id}
                            initial={{ opacity: 0, y: 50 }}
                            whileInView={{ opacity: 1, y: 0 }}
                            viewport={{ once: true }}
                            transition={{ duration: 0.5, delay: index * 0.1 }}
                            className="group relative bg-white rounded-3xl overflow-hidden shadow-lg hover:shadow-2xl transition-all duration-500 border border-gray-100 hover:border-catering-accent/30 hover:-translate-y-2"
                        >
                            {/* Featured badge */}
                            {caterer.featured && (
                                <div className="absolute top-4 left-4 z-20 flex items-center gap-1 px-3 py-1.5 bg-gradient-to-r from-catering-accent to-catering-secondary rounded-full text-white text-xs font-bold shadow-lg">
                                    <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
                                        <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                                    </svg>
                                    FEATURED
                                </div>
                            )}

                            {/* Verified badge */}
                            {caterer.verified && (
                                <div className="absolute top-4 right-4 z-20 w-10 h-10 bg-green-500 rounded-full flex items-center justify-center shadow-lg">
                                    <svg className="w-6 h-6 text-white" fill="currentColor" viewBox="0 0 20 20">
                                        <path fillRule="evenodd" d="M6.267 3.455a3.066 3.066 0 001.745-.723 3.066 3.066 0 013.976 0 3.066 3.066 0 001.745.723 3.066 3.066 0 012.812 2.812c.051.643.304 1.254.723 1.745a3.066 3.066 0 010 3.976 3.066 3.066 0 00-.723 1.745 3.066 3.066 0 01-2.812 2.812 3.066 3.066 0 00-1.745.723 3.066 3.066 0 01-3.976 0 3.066 3.066 0 00-1.745-.723 3.066 3.066 0 01-2.812-2.812 3.066 3.066 0 00-.723-1.745 3.066 3.066 0 010-3.976 3.066 3.066 0 00.723-1.745 3.066 3.066 0 012.812-2.812zm7.44 5.252a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                                    </svg>
                                </div>
                            )}

                            {/* Image */}
                            <div className="relative h-56 overflow-hidden">
                                <div className="absolute inset-0 bg-gradient-to-t from-black/60 via-transparent to-transparent z-10"></div>
                                <img
                                    src={API_BASE_URL+caterer.image}
                                    alt={caterer.name}
                                    className="w-full h-full object-cover transition-transform duration-700 group-hover:scale-110"
                                />
                            </div>

                            {/* Content */}
                            <div className="p-6">
                                {/* Name and Rating */}
                                <div className="mb-4">
                                    <h3 className="text-xl font-bold text-gray-900 mb-2 group-hover:text-catering-primary transition-colors">
                                        {caterer.name}
                                    </h3>
                                    <div className="flex items-center gap-2 text-sm text-gray-600">
                                        <span className="font-medium">{caterer.cuisine}</span>
                                        <span className="text-gray-400">•</span>
                                        <span className="text-xs bg-gray-100 px-2 py-1 rounded-full">
                                            Min ₹{caterer.minOrder} guests
                                        </span>
                                    </div>
                                </div>

                                {/* Rating */}
                                <div className="flex items-center gap-3 mb-4">
                                    <div className="flex items-center gap-1 bg-gradient-to-r from-catering-primary to-catering-accent px-3 py-1.5 rounded-full">
                                        <svg className="w-4 h-4 text-white" fill="currentColor" viewBox="0 0 20 20">
                                            <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                                        </svg>
                                        <span className="text-white font-bold text-sm">{caterer.rating}</span>
                                    </div>
                                    <span className="text-sm text-gray-500">
                                        {caterer.reviews} reviews
                                    </span>
                                </div>

                                {/* Specialties */}
                                <div className="flex flex-wrap gap-2 mb-4">
                                    {caterer.specialties.map((specialty, idx) => (
                                        <span
                                            key={idx}
                                            className="px-3 py-1 bg-gradient-to-r from-catering-light to-orange-50 text-catering-primary text-xs font-semibold rounded-full border border-catering-accent/20"
                                        >
                                            {specialty}
                                        </span>
                                    ))}
                                </div>

                                {/* CTA Button */}
                                <a
                                    href={`/caterings/${caterer.id}`}
                                    className="block w-full py-3 px-4 bg-gradient-to-r from-catering-primary to-catering-accent text-white font-bold text-center rounded-xl shadow-md hover:shadow-xl hover:scale-105 transition-all duration-300 group/btn"
                                >
                                    <span className="flex items-center justify-center gap-2">
                                        View Details
                                        <svg className="w-4 h-4 group-hover/btn:translate-x-1 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                                        </svg>
                                    </span>
                                </a>
                            </div>

                            {/* Gold accent line */}
                            <div className="absolute bottom-0 left-0 right-0 h-1 bg-gradient-to-r from-transparent via-catering-accent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-500"></div>
                        </motion.div>
                    ))}
                </div>

                {/* View All CTA */}
                <motion.div
                    initial={{ opacity: 0, y: 30 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    transition={{ duration: 0.6, delay: 0.8 }}
                    className="text-center mt-12"
                >
                    <a
                        href="/caterings"
                        className="inline-flex items-center gap-3 px-8 py-4 bg-white border-2 border-catering-accent text-catering-primary font-bold rounded-xl shadow-md hover:bg-catering-accent hover:text-white transition-all duration-300 group"
                    >
                        <span>Explore All Caterers</span>
                        <svg
                            className="w-5 h-5 group-hover:translate-x-1 transition-transform"
                            fill="none"
                            stroke="currentColor"
                            viewBox="0 0 24 24"
                        >
                            <path
                                strokeLinecap="round"
                                strokeLinejoin="round"
                                strokeWidth={2}
                                d="M17 8l4 4m0 0l-4 4m4-4H3"
                            />
                        </svg>
                    </a>
                </motion.div>
            </div>
        </section>
    );
}
