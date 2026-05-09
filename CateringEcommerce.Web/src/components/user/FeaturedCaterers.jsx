import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { Star, CheckCircle, ArrowRight } from 'lucide-react';
import { SkeletonCard } from '../../design-system/components';
import { SectionHeader } from '../../design-system/components';
import { getFeaturedCaterers } from '../../services/homeApi';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '');

export default function FeaturedCaterers() {
    const [featuredList, setFeaturedList] = useState([]);
    const [loading, setLoading]           = useState(true);

    useEffect(() => {
        const load = async () => {
            try {
                const response = await getFeaturedCaterers();
                if (response.success && response.data) setFeaturedList(response.data);
            } catch (err) {
                console.error('Error loading featured caterers:', err);
            } finally {
                setLoading(false);
            }
        };
        load();
    }, []);

    if (!loading && featuredList.length === 0) return null;

    return (
        <section className="py-24 bg-white relative overflow-hidden">
            <div className="absolute top-0 right-0 w-96 h-96 rounded-full blur-3xl pointer-events-none" style={{ background: 'rgba(255,182,39,0.04)' }} />
            <div className="absolute bottom-0 left-0 w-96 h-96 rounded-full blur-3xl pointer-events-none" style={{ background: 'rgba(255,107,53,0.04)' }} />

            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative z-10">
                <SectionHeader
                    eyebrow="Trending Now"
                    title={<>Featured <span className="t-gradient">Premium Caterers</span></>}
                    subtitle="Discover our handpicked selection of top-rated catering services"
                />

                <div className="caterer-grid mt-14">
                    {loading ? (
                        Array.from({ length: 3 }).map((_, i) => <SkeletonCard key={i} />)
                    ) : (
                        featuredList.map((caterer, index) => (
                            <motion.div
                                key={caterer.id}
                                initial={{ opacity: 0, y: 40 }}
                                whileInView={{ opacity: 1, y: 0 }}
                                viewport={{ once: true }}
                                transition={{ duration: 0.45, delay: index * 0.08 }}
                                className="cat-card"
                            >
                                <div className="cat-card__img">
                                    <img src={`${API_BASE_URL}${caterer.image}`} alt={caterer.name} loading="lazy" decoding="async" />

                                    {caterer.featured && (
                                        <div className="cat-card__featured">
                                            <Star size={10} fill="currentColor" /> FEATURED
                                        </div>
                                    )}

                                    {caterer.verified && (
                                        <div className="absolute top-3.5 right-3.5 z-10 w-8 h-8 bg-success rounded-full flex items-center justify-center shadow">
                                            <CheckCircle size={16} className="text-white" fill="white" strokeWidth={0} />
                                        </div>
                                    )}
                                </div>

                                <div className="p-5 flex flex-col flex-1">
                                    <h3 className="font-bold text-neutral-900 text-lg mb-1 hover:text-primary transition-colors">{caterer.name}</h3>
                                    <div className="flex items-center gap-2 text-sm text-neutral-500 mb-3">
                                        <span>{caterer.cuisine}</span>
                                        <span className="text-neutral-300">•</span>
                                        <span className="text-xs bg-neutral-100 px-2 py-0.5 rounded-full">Min {caterer.minOrder} guests</span>
                                    </div>

                                    <div className="flex items-center gap-2 mb-3">
                                        <div className="cat-card__score">
                                            <Star size={12} fill="white" strokeWidth={0} />{caterer.rating}
                                        </div>
                                        <span className="text-xs text-neutral-500">{caterer.reviews} reviews</span>
                                    </div>

                                    {caterer.specialties?.length > 0 && (
                                        <div className="flex flex-wrap gap-1.5 mb-4">
                                            {caterer.specialties.map((s, i) => (
                                                <span key={i} className="px-2.5 py-1 text-[11px] font-semibold rounded-full" style={{ background: 'rgba(255,107,53,0.08)', color: 'var(--color-primary)' }}>
                                                    {s}
                                                </span>
                                            ))}
                                        </div>
                                    )}

                                    <a
                                        href={`/caterings/${caterer.id}`}
                                        className="mt-auto flex items-center justify-center gap-2 w-full py-3 rounded-xl text-white font-bold text-sm transition-all hover:scale-[1.02]"
                                        style={{ background: 'var(--gradient-catering)' }}
                                    >
                                        View Details <ArrowRight size={15} />
                                    </a>
                                </div>
                            </motion.div>
                        ))
                    )}
                </div>

                <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    transition={{ duration: 0.5, delay: 0.5 }}
                    className="text-center mt-12"
                >
                    <a
                        href="/caterings"
                        className="inline-flex items-center gap-2 px-8 py-4 rounded-xl border-2 border-accent text-primary font-bold hover:bg-accent hover:text-white transition-all duration-300 group"
                    >
                        Explore All Caterers <ArrowRight size={18} className="group-hover:translate-x-0.5 transition-transform" />
                    </a>
                </motion.div>
            </div>
        </section>
    );
}
