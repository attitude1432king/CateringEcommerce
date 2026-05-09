import React from 'react';
import { motion } from 'framer-motion';
import { ArrowRight } from 'lucide-react';

const categories = [
    {
        id: 1,
        title: 'Wedding Catering',
        description: 'Elegant multi-course menus for your special day',
        icon: '💒',
        offer: 'UP TO 20% OFF',
        image: 'https://images.unsplash.com/photo-1511795409834-ef04bbd61622?q=80&w=600&auto=format&fit=crop',
        link: '/caterings?category=wedding',
        modifier: 'cat-tile--from-rose',
    },
    {
        id: 2,
        title: 'Corporate Events',
        description: 'Professional catering for business functions',
        icon: '🏢',
        offer: 'FREE CONSULTATION',
        image: 'Corporate.jpg',
        link: '/caterings?category=corporate',
        modifier: 'cat-tile--from-blue',
    },
    {
        id: 3,
        title: 'Party & Celebrations',
        description: 'Bulk orders for birthdays, anniversaries & gatherings',
        icon: '🎉',
        offer: 'BULK DISCOUNTS',
        image: 'Party.png',
        link: '/caterings?category=party',
        modifier: '',
    },
    {
        id: 4,
        title: 'Decorations',
        description: 'Themed decorations to light up your events',
        icon: '🎈',
        offer: 'NEW ARRIVAL',
        image: 'Decorations.png',
        link: '/caterings?category=decorations',
        modifier: 'cat-tile--from-purple',
    },
];

export default function CategoryTiles({ categories: propCategories = [] }) {
    const tiles = propCategories.length ? propCategories : categories;

    return (
        <section className="cats">
            <div className="cats__head">
                <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    transition={{ duration: 0.5 }}
                >
                    <div className="cats__eyebrow">
                        🎯 Our Services
                    </div>
                    <h2 className="cats__title">
                        Tailored for Every{' '}
                        <span className="t-gradient">Occasion</span>
                    </h2>
                    <p className="cats__sub">
                        Choose from our specialized catering services designed to make your event unforgettable
                    </p>
                </motion.div>
            </div>

            <div className="cats__grid" style={{ padding: '0 24px', maxWidth: 'var(--container-max)', margin: '0 auto' }}>
                {tiles.map((tile, index) => (
                    <motion.a
                        key={tile.id}
                        href={tile.link}
                        initial={{ opacity: 0, y: 32 }}
                        whileInView={{ opacity: 1, y: 0 }}
                        viewport={{ once: true }}
                        transition={{ duration: 0.45, delay: index * 0.08 }}
                        className={`cat-tile ${tile.modifier || ''}`}
                    >
                        {/* Top image half */}
                        <div className="cat-tile__img">
                            <img src={tile.image} alt={tile.title} loading="lazy" decoding="async" />
                        </div>
                        <div className="cat-tile__veil" />

                        {/* Offer pill */}
                        {tile.offer && (
                            <div className="cat-tile__offer">{tile.offer}</div>
                        )}

                        {/* Icon badge at the boundary */}
                        <div className="cat-tile__icon">{tile.icon}</div>

                        {/* Bottom body half */}
                        <div className="cat-tile__body">
                            <h3>{tile.title}</h3>
                            <p>{tile.description}</p>
                            <span className="cat-tile__cta">
                                Explore <ArrowRight size={13} />
                            </span>
                        </div>
                    </motion.a>
                ))}
            </div>

            <motion.div
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ duration: 0.5, delay: 0.4 }}
                className="text-center mt-12"
            >
                <a
                    href="/caterings"
                    className="inline-flex items-center gap-2 px-6 py-3 rounded-xl border-2 border-accent text-primary font-semibold text-sm hover:bg-accent hover:text-white transition-all duration-300"
                >
                    View All Services <ArrowRight size={16} />
                </a>
            </motion.div>
        </section>
    );
}
