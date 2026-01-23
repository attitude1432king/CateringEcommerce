import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { getTestimonials } from '../../services/homeApi';

export default function Testimonials() {
    const [testimonials, setTestimonials] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        const loadTestimonials = async () => {
            try {
                setLoading(true);
                const response = await getTestimonials();
                if (response.success && response.data) {
                    setTestimonials(response.data);
                }
            } catch (err) {
                console.error('Error loading testimonials:', err);
                setError(err.message);
            } finally {
                setLoading(false);
            }
        };

        loadTestimonials();
    }, []);

    if (loading) {
        return (
            <section className="py-24 bg-gradient-to-br from-gray-50 via-white to-catering-light/20">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
                    <div className="text-lg text-gray-600">Loading testimonials...</div>
                </div>
            </section>
        );
    }

    if (error) {
        return (
            <section className="py-24 bg-gradient-to-br from-gray-50 via-white to-catering-light/20">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
                    <div className="text-lg text-red-600">Error: {error}</div>
                </div>
            </section>
        );
    }

    if (testimonials.length === 0) {
        return null; // Don't show the section if there are no testimonials
    }

    return (
        <section className="py-24 bg-gradient-to-br from-gray-50 via-white to-catering-light/20 relative overflow-hidden">
            {/* Decorative elements */}
            <div className="absolute top-20 right-0 w-96 h-96 bg-catering-accent/5 rounded-full blur-3xl"></div>
            <div className="absolute bottom-20 left-0 w-96 h-96 bg-catering-primary/5 rounded-full blur-3xl"></div>

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
                        <span className="text-catering-accent text-xl">💬</span>
                        <span className="text-catering-primary font-semibold text-sm uppercase tracking-wider">
                            Testimonials
                        </span>
                    </div>

                    <h2 className="text-4xl md:text-5xl font-bold text-gray-900 mb-6">
                        What Our{' '}
                        <span className="bg-gradient-to-r from-catering-primary to-catering-accent bg-clip-text text-transparent">
                            Clients Say
                        </span>
                    </h2>

                    <p className="text-lg text-gray-600 max-w-2xl mx-auto leading-relaxed">
                        Real experiences from our valued customers who trusted us with their special moments
                    </p>

                    {/* Decorative line */}
                    <div className="mt-8 flex items-center justify-center gap-2">
                        <div className="w-12 h-0.5 bg-gradient-to-r from-transparent to-catering-accent"></div>
                        <div className="flex gap-1">
                            {[...Array(5)].map((_, i) => (
                                <svg key={i} className="w-4 h-4 text-catering-accent" fill="currentColor" viewBox="0 0 20 20">
                                    <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                                </svg>
                            ))}
                        </div>
                        <div className="w-12 h-0.5 bg-gradient-to-l from-transparent to-catering-accent"></div>
                    </div>
                </motion.div>

                {/* Testimonials Grid */}
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
                    {testimonials.map((testimonial, index) => (
                        <motion.div
                            key={testimonial.id}
                            initial={{ opacity: 0, y: 50 }}
                            whileInView={{ opacity: 1, y: 0 }}
                            viewport={{ once: true }}
                            transition={{ duration: 0.5, delay: index * 0.1 }}
                            className="group relative bg-white rounded-3xl p-8 shadow-lg hover:shadow-2xl transition-all duration-500 border border-gray-100 hover:border-catering-accent/30 hover:-translate-y-2"
                        >
                            {/* Quote icon */}
                            <div className="absolute top-6 right-6 text-catering-accent/10 group-hover:text-catering-accent/20 transition-colors">
                                <svg className="w-16 h-16" fill="currentColor" viewBox="0 0 24 24">
                                    <path d="M14.017 21v-7.391c0-5.704 3.731-9.57 8.983-10.609l.995 2.151c-2.432.917-3.995 3.638-3.995 5.849h4v10h-9.983zm-14.017 0v-7.391c0-5.704 3.748-9.57 9-10.609l.996 2.151c-2.433.917-3.996 3.638-3.996 5.849h3.983v10h-9.983z" />
                                </svg>
                            </div>

                            {/* Rating stars */}
                            <div className="flex gap-1 mb-4">
                                {[...Array(testimonial.rating)].map((_, i) => (
                                    <svg key={i} className="w-5 h-5 text-catering-accent" fill="currentColor" viewBox="0 0 20 20">
                                        <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                                    </svg>
                                ))}
                            </div>

                            {/* Testimonial text */}
                            <p className="text-gray-700 leading-relaxed mb-6 relative z-10">
                                "{testimonial.text}"
                            </p>

                            {/* Author info */}
                            <div className="flex items-center gap-4 pt-6 border-t border-gray-100">
                                <img
                                    src={testimonial.image}
                                    alt={testimonial.author}
                                    className="w-14 h-14 rounded-full border-2 border-catering-accent/30 shadow-md"
                                />
                                <div className="flex-1">
                                    <div className="font-bold text-gray-900 mb-1">
                                        {testimonial.author}
                                    </div>
                                    <div className="text-sm text-gray-600 mb-1">
                                        {testimonial.role}
                                    </div>
                                    <div className="flex items-center gap-2 text-xs text-gray-500">
                                        <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
                                            <path fillRule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clipRule="evenodd" />
                                        </svg>
                                        <span>{testimonial.location}</span>
                                        <span className="text-gray-400">•</span>
                                        <span>{testimonial.event}</span>
                                    </div>
                                </div>
                            </div>

                            {/* Gold accent line on hover */}
                            <div className="absolute bottom-0 left-0 right-0 h-1 bg-gradient-to-r from-transparent via-catering-accent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-500 rounded-b-3xl"></div>
                        </motion.div>
                    ))}
                </div>

                {/* Stats */}
                <motion.div
                    initial={{ opacity: 0, y: 30 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    transition={{ duration: 0.6, delay: 0.8 }}
                    className="mt-16 grid grid-cols-2 md:grid-cols-4 gap-6"
                >
                    {[
                        { value: '4.9', label: 'Average Rating', icon: '⭐' },
                        { value: '10k+', label: 'Happy Clients', icon: '😊' },
                        { value: '98%', label: 'Satisfaction', icon: '👍' },
                        { value: '5k+', label: 'Events Catered', icon: '🎉' }
                    ].map((stat, index) => (
                        <div
                            key={index}
                            className="text-center p-6 bg-white rounded-2xl border border-gray-100 shadow-sm"
                        >
                            <div className="text-3xl mb-2">{stat.icon}</div>
                            <div className="text-3xl font-bold bg-gradient-to-r from-catering-primary to-catering-accent bg-clip-text text-transparent mb-1">
                                {stat.value}
                            </div>
                            <div className="text-sm text-gray-600">{stat.label}</div>
                        </div>
                    ))}
                </motion.div>
            </div>
        </section>
    );
}
