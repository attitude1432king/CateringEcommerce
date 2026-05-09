import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { Star, Quote, MapPin } from 'lucide-react';
import { SectionHeader, SkeletonCard } from '../../design-system/components';
import { getTestimonials } from '../../services/homeApi';

export default function Testimonials() {
    const [testimonials, setTestimonials] = useState([]);
    const [loading, setLoading]           = useState(true);

    useEffect(() => {
        const load = async () => {
            try {
                const response = await getTestimonials();
                if (response.success && response.data) setTestimonials(response.data);
            } catch (err) {
                console.error('Error loading testimonials:', err);
            } finally {
                setLoading(false);
            }
        };
        load();
    }, []);

    if (!loading && testimonials.length === 0) return null;

    return (
        <section className="py-24 relative overflow-hidden" style={{ background: 'linear-gradient(135deg, #f9fafb, #fff, rgba(255,107,53,0.03))' }}>
            <div className="absolute top-20 right-0 w-96 h-96 rounded-full blur-3xl pointer-events-none" style={{ background: 'rgba(255,182,39,0.05)' }} />
            <div className="absolute bottom-20 left-0 w-96 h-96 rounded-full blur-3xl pointer-events-none" style={{ background: 'rgba(255,107,53,0.05)' }} />

            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative z-10">
                <SectionHeader
                    eyebrow="Testimonials"
                    title={<>What Our <span className="t-gradient">Clients Say</span></>}
                    subtitle="Real experiences from our valued customers who trusted us with their special moments"
                />

                {loading ? (
                    <div className="caterer-grid mt-14">
                        {Array.from({ length: 3 }).map((_, i) => <SkeletonCard key={i} />)}
                    </div>
                ) : (
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8 mt-14">
                        {testimonials.map((t, index) => (
                            <motion.div
                                key={t.id}
                                initial={{ opacity: 0, y: 40 }}
                                whileInView={{ opacity: 1, y: 0 }}
                                viewport={{ once: true }}
                                transition={{ duration: 0.45, delay: index * 0.08 }}
                                className="group relative bg-white rounded-3xl p-8 shadow-card hover:shadow-card-hover transition-all duration-500 border border-neutral-100 hover:border-accent/30 hover:-translate-y-2"
                            >
                                {/* Quote icon */}
                                <Quote
                                    size={64}
                                    className="absolute top-6 right-6 transition-colors duration-300"
                                    style={{ color: 'rgba(255,107,53,0.06)' }}
                                />

                                {/* Stars */}
                                <div className="flex gap-1 mb-4">
                                    {Array.from({ length: t.rating || 5 }).map((_, i) => (
                                        <Star key={i} size={16} className="text-accent" fill="currentColor" />
                                    ))}
                                </div>

                                <p className="text-neutral-700 leading-relaxed mb-6 relative z-10 text-sm">
                                    "{t.text}"
                                </p>

                                <div className="flex items-center gap-4 pt-5 border-t border-neutral-100">
                                    <img
                                        src={t.image}
                                        alt={t.author}
                                        className="w-12 h-12 rounded-full object-cover border-2 shadow-sm"
                                        style={{ borderColor: 'rgba(255,182,39,0.3)' }}
                                        loading="lazy"
                                    />
                                    <div className="flex-1 min-w-0">
                                        <div className="font-bold text-neutral-900 truncate">{t.author}</div>
                                        <div className="text-sm text-neutral-500 truncate">{t.role}</div>
                                        <div className="flex items-center gap-1.5 mt-0.5 text-xs text-neutral-400">
                                            <MapPin size={11} />
                                            <span>{t.location}</span>
                                            {t.event && <><span>•</span><span>{t.event}</span></>}
                                        </div>
                                    </div>
                                </div>

                                {/* Hover accent line */}
                                <div
                                    className="absolute bottom-0 left-0 right-0 h-1 rounded-b-3xl opacity-0 group-hover:opacity-100 transition-opacity duration-500"
                                    style={{ background: 'var(--gradient-catering)' }}
                                />
                            </motion.div>
                        ))}
                    </div>
                )}

                {/* Bottom stats strip */}
                <motion.div
                    initial={{ opacity: 0, y: 24 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    transition={{ duration: 0.5, delay: 0.5 }}
                    className="mt-16 grid grid-cols-2 md:grid-cols-4 gap-6"
                >
                    {[
                        { value: '4.9',  label: 'Average Rating', icon: '⭐' },
                        { value: '10k+', label: 'Happy Clients',  icon: '😊' },
                        { value: '98%',  label: 'Satisfaction',   icon: '👍' },
                        { value: '5k+',  label: 'Events Catered', icon: '🎉' },
                    ].map((stat, i) => (
                        <div key={i} className="text-center p-6 bg-white rounded-2xl border border-neutral-100 shadow-card">
                            <div className="text-3xl mb-2">{stat.icon}</div>
                            <div className="text-3xl font-extrabold mb-1 t-gradient">{stat.value}</div>
                            <div className="text-sm text-neutral-500">{stat.label}</div>
                        </div>
                    ))}
                </motion.div>
            </div>
        </section>
    );
}
