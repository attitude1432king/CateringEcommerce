import React from 'react';
import { motion } from 'framer-motion';

export default function CategoryTiles({ categories = [] }) {
    const serviceCategories = categories.length ? categories : [
        {
            id: 1,
            title: 'Wedding Catering',
            description: 'Elegant multi-course menus for your special day',
            icon: '💒',
            offer: 'UP TO 20% OFF',
            image: 'https://images.unsplash.com/photo-1511795409834-ef04bbd61622?q=80&w=600&auto=format&fit=crop',
            link: '/services/wedding',
            gradient: 'from-rose-600 to-pink-500',
            bgGradient: 'from-rose-50/80 to-pink-50/60'
        },
        {
            id: 2,
            title: 'Corporate Events',
            description: 'Professional catering for business functions',
            icon: '🏢',
            offer: 'FREE CONSULTATION',
            image: 'Corporate.jpg',
            link: '/services/corporate',
            gradient: 'from-blue-600 to-indigo-600',
            bgGradient: 'from-blue-50/80 to-indigo-50/60'
        },
        {
            id: 3,
            title: 'Party & Celebrations',
            description: 'Bulk orders for birthdays, anniversaries & gatherings',
            icon: '🎉',
            offer: 'BULK DISCOUNTS',
            image: 'Party.png',
            link: '/services/parties',
            gradient: 'from-catering-primary to-catering-accent',
            bgGradient: 'from-catering-light/80 to-orange-50/60'
        },
        {
            id: 4,
            title: 'Decorations',
            description: 'Themed decorations to light up your events',
            icon: '🎈',
            offer: 'NEW ARRIVAL',
            image: 'Decorations.png',
            link: '/services/decorations',
            gradient: 'from-purple-600 to-fuchsia-600',
            bgGradient: 'from-purple-50/80 to-fuchsia-50/60'
        }
    ];

    return (
        <section className="py-20 px-4 sm:px-6 lg:px-8 bg-gradient-to-b from-white to-gray-50 relative overflow-hidden">
            {/* Decorative background elements */}
            <div className="absolute top-0 left-0 w-96 h-96 bg-catering-accent/5 rounded-full blur-3xl -translate-x-1/2 -translate-y-1/2"></div>
            <div className="absolute bottom-0 right-0 w-96 h-96 bg-catering-primary/5 rounded-full blur-3xl translate-x-1/2 translate-y-1/2"></div>

            <div className="max-w-7xl mx-auto relative z-10">
                {/* Section header */}
                <motion.div
                    initial={{ opacity: 0, y: 30 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    transition={{ duration: 0.6 }}
                    className="text-center mb-16"
                >
                    <div className="inline-flex items-center gap-2 px-4 py-2 bg-gradient-to-r from-catering-accent/10 to-catering-primary/10 rounded-full mb-6">
                        <span className="text-catering-accent text-xl">🎯</span>
                        <span className="text-catering-primary font-semibold text-sm uppercase tracking-wider">
                            Our Services
                        </span>
                    </div>

                    <h2 className="text-4xl md:text-5xl font-bold text-gray-900 mb-4">
                        Tailored for Every{' '}
                        <span className="bg-gradient-to-r from-catering-primary to-catering-accent bg-clip-text text-transparent">
                            Occasion
                        </span>
                    </h2>
                    <p className="text-lg text-gray-600 max-w-2xl mx-auto leading-relaxed">
                        Choose from our specialized catering services designed to make your event unforgettable
                    </p>
                </motion.div>

                {/* Service cards grid */}
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
                    {serviceCategories.map((service, index) => (
                        <motion.a
                            key={service.id}
                            href={service.link}
                            initial={{ opacity: 0, y: 50 }}
                            whileInView={{ opacity: 1, y: 0 }}
                            viewport={{ once: true }}
                            transition={{ duration: 0.5, delay: index * 0.1 }}
                            className="group relative flex flex-col h-full overflow-hidden rounded-3xl shadow-lg hover:shadow-2xl transition-all duration-500 border border-white/50 bg-white transform hover:-translate-y-2"
                        >
                            {/* Premium gold border on hover */}
                            <div className="absolute inset-0 rounded-3xl border-2 border-catering-accent/0 group-hover:border-catering-accent/50 transition-all duration-500 z-30 pointer-events-none"></div>

                            {/* Image container with overlay */}
                            <div className="relative h-52 w-full overflow-hidden">
                                {/* Gradient overlay */}
                                <div className="absolute inset-0 bg-gradient-to-t from-black/70 via-black/30 to-transparent z-10 transition-opacity duration-500 group-hover:from-black/60" />

                                {/* Premium shine effect */}
                                <div className="absolute inset-0 bg-gradient-to-tr from-transparent via-white/0 to-white/0 group-hover:via-white/20 transition-all duration-700 z-20"></div>

                                <img
                                    src={service.image}
                                    alt={service.title}
                                    loading="lazy"
                                    className="w-full h-full object-cover transition-transform duration-700 group-hover:scale-110"
                                />

                                {/* Offer Badge with premium styling */}
                                {service.offer && (
                                    <div className={`absolute top-4 right-4 z-20 bg-gradient-to-r ${service.gradient} text-white text-[10px] font-bold px-3 py-1.5 rounded-full shadow-lg uppercase tracking-wider backdrop-blur-sm border border-white/20`}>
                                        {service.offer}
                                    </div>
                                )}

                                {/* Icon with elegant backdrop */}
                                <div className="absolute bottom-4 left-4 z-20">
                                    <div className="w-14 h-14 bg-white/20 backdrop-blur-md rounded-2xl flex items-center justify-center border border-white/30 group-hover:scale-110 transition-transform shadow-xl">
                                        <span className="text-3xl filter drop-shadow-lg">{service.icon}</span>
                                    </div>
                                </div>
                            </div>

                            {/* Content with premium gradient background */}
                            <div className={`flex flex-col flex-grow p-6 bg-gradient-to-br ${service.bgGradient} backdrop-blur-sm relative`}>
                                {/* Subtle pattern overlay */}
                                <div className="absolute inset-0 bg-white/40"></div>

                                <div className="relative z-10">
                                    <h3 className="text-xl font-bold mb-2 text-gray-900 group-hover:text-catering-primary transition-colors">
                                        {service.title}
                                    </h3>
                                    <p className="text-sm text-gray-700 leading-relaxed mb-4">
                                        {service.description}
                                    </p>

                                    {/* Premium CTA with arrow */}
                                    <div className="flex items-center gap-2 text-sm font-semibold text-catering-primary group-hover:gap-3 transition-all">
                                        <span>Explore</span>
                                        <svg
                                            className="w-4 h-4 group-hover:translate-x-1 transition-transform"
                                            fill="none"
                                            stroke="currentColor"
                                            viewBox="0 0 24 24"
                                        >
                                            <path
                                                strokeLinecap="round"
                                                strokeLinejoin="round"
                                                strokeWidth={2}
                                                d="M13 7l5 5m0 0l-5 5m5-5H6"
                                            />
                                        </svg>
                                    </div>
                                </div>

                                {/* Decorative gold accent line */}
                                <div className="absolute bottom-0 left-0 right-0 h-1 bg-gradient-to-r from-transparent via-catering-accent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-500"></div>
                            </div>
                        </motion.a>
                    ))}
                </div>

                {/* View all services CTA */}
                <motion.div
                    initial={{ opacity: 0, y: 30 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    transition={{ duration: 0.6, delay: 0.6 }}
                    className="text-center mt-12"
                >
                    <a
                        href="/caterings"
                        className="inline-flex items-center gap-2 px-6 py-3 border-2 border-catering-accent text-catering-primary font-semibold rounded-xl hover:bg-catering-accent hover:text-white transition-all duration-300 shadow-md hover:shadow-xl group"
                    >
                        <span>View All Services</span>
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
