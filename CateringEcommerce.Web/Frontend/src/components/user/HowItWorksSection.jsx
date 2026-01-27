import React from 'react';
import { motion } from 'framer-motion';

export default function HowItWorksSection() {
    const steps = [
        {
            id: 1,
            icon: '🔍',
            title: 'Search & Discover',
            description: 'Browse through our curated selection of premium caterers in your city',
        },
        {
            id: 2,
            icon: '📋',
            title: 'Compare & Select',
            description: 'Review menus, pricing, and customer reviews to find your perfect match',
        },
        {
            id: 3,
            icon: '📅',
            title: 'Book & Customize',
            description: 'Reserve your date and customize the menu to your preferences',
        },
        {
            id: 4,
            icon: '🎉',
            title: 'Enjoy Your Event',
            description: 'Sit back and relax while we deliver an unforgettable culinary experience',
        },
    ];

    return (
        <section className="py-24 bg-gradient-to-b from-white via-catering-light/30 to-white relative overflow-hidden">
            {/* Decorative elements */}
            <div className="absolute top-20 left-10 w-64 h-64 bg-catering-accent/5 rounded-full blur-3xl"></div>
            <div className="absolute bottom-20 right-10 w-72 h-72 bg-catering-primary/5 rounded-full blur-3xl"></div>

            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative z-10">
                {/* Section Header */}
                <motion.div
                    initial={{ opacity: 0, y: 30 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    transition={{ duration: 0.6 }}
                    className="text-center mb-20"
                >
                    <div className="inline-flex items-center gap-2 px-4 py-2 bg-gradient-to-r from-catering-accent/10 to-catering-primary/10 rounded-full mb-6">
                        <span className="text-catering-accent text-xl">✨</span>
                        <span className="text-catering-primary font-semibold text-sm uppercase tracking-wider">
                            Simple & Seamless
                        </span>
                    </div>

                    <h2 className="text-4xl md:text-5xl font-bold text-gray-900 mb-6">
                        How ENYVORA <span className="bg-gradient-to-r from-catering-primary to-catering-accent bg-clip-text text-transparent">Works</span>
                    </h2>

                    <p className="text-lg text-gray-600 max-w-2xl mx-auto leading-relaxed">
                        Your journey to exceptional catering in four elegant steps
                    </p>

                    {/* Decorative line */}
                    <div className="mt-8 flex items-center justify-center gap-2">
                        <div className="w-12 h-0.5 bg-gradient-to-r from-transparent to-catering-accent"></div>
                        <div className="w-2 h-2 bg-catering-accent rounded-full"></div>
                        <div className="w-24 h-0.5 bg-catering-accent"></div>
                        <div className="w-2 h-2 bg-catering-accent rounded-full"></div>
                        <div className="w-12 h-0.5 bg-gradient-to-l from-transparent to-catering-accent"></div>
                    </div>
                </motion.div>

                {/* Steps Grid */}
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8 relative">
                    {/* Connecting line (desktop only) */}
                    <div className="hidden lg:block absolute top-24 left-0 right-0 h-0.5 bg-gradient-to-r from-catering-accent/20 via-catering-primary/40 to-catering-accent/20"></div>

                    {steps.map((step, index) => (
                        <motion.div
                            key={step.id}
                            initial={{ opacity: 0, y: 50 }}
                            whileInView={{ opacity: 1, y: 0 }}
                            viewport={{ once: true }}
                            transition={{ duration: 0.5, delay: index * 0.15 }}
                            className="relative group"
                        >
                            {/* Step Card */}
                            <div className="bg-white rounded-2xl p-8 shadow-lg hover:shadow-2xl transition-all duration-300 border border-gray-100 hover:border-catering-accent/30 group-hover:-translate-y-2 relative z-10">
                                {/* Step number badge */}
                                <div className="absolute -top-4 -right-4 w-12 h-12 bg-gradient-to-br from-catering-primary to-catering-accent rounded-full flex items-center justify-center text-white font-bold text-lg shadow-xl group-hover:scale-110 transition-transform">
                                    {step.id}
                                </div>

                                {/* Icon container */}
                                <div className="w-20 h-20 bg-gradient-to-br from-catering-light to-white rounded-2xl flex items-center justify-center text-5xl mb-6 mx-auto group-hover:scale-110 transition-transform shadow-inner border border-catering-accent/20">
                                    {step.icon}
                                </div>

                                {/* Content */}
                                <h3 className="text-xl font-bold text-gray-900 mb-3 text-center">
                                    {step.title}
                                </h3>

                                <p className="text-gray-600 text-center leading-relaxed text-sm">
                                    {step.description}
                                </p>

                                {/* Decorative bottom accent */}
                                <div className="mt-6 h-1 w-16 bg-gradient-to-r from-catering-accent to-catering-primary rounded-full mx-auto group-hover:w-full transition-all duration-500"></div>
                            </div>

                            {/* Arrow connector (desktop only) */}
                            {index < steps.length - 1 && (
                                <div className="hidden lg:block absolute top-24 -right-4 z-20">
                                    <svg
                                        className="w-8 h-8 text-catering-accent opacity-60"
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
                            )}
                        </motion.div>
                    ))}
                </div>

                {/* Call to Action */}
                <motion.div
                    initial={{ opacity: 0, y: 30 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    transition={{ duration: 0.6, delay: 0.8 }}
                    className="text-center mt-16"
                >
                    <a
                        href="#search"
                        className="inline-flex items-center gap-3 px-8 py-4 bg-gradient-to-r from-catering-primary via-catering-secondary to-catering-accent text-white font-bold rounded-xl shadow-xl hover:shadow-2xl hover:shadow-catering-accent/30 transform hover:scale-105 transition-all duration-300 group"
                    >
                        <span>Get Started Now</span>
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
                                d="M13 7l5 5m0 0l-5 5m5-5H6"
                            />
                        </svg>
                    </a>
                </motion.div>
            </div>
        </section>
    );
}
