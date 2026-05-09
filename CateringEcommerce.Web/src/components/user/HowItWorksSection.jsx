import React from 'react';
import { motion } from 'framer-motion';
import { Search, BookOpen, CalendarCheck, PartyPopper, ArrowRight } from 'lucide-react';
import { SectionHeader } from '../../design-system/components';

const steps = [
    { id: 1, Icon: Search,       title: 'Search & Discover',  description: 'Browse through our curated selection of premium caterers in your city'                         },
    { id: 2, Icon: BookOpen,     title: 'Compare & Select',   description: 'Review menus, pricing, and customer reviews to find your perfect match'                         },
    { id: 3, Icon: CalendarCheck,title: 'Book & Customize',   description: 'Reserve your date and customize the menu to your preferences'                                   },
    { id: 4, Icon: PartyPopper,  title: 'Enjoy Your Event',   description: 'Sit back and relax while we deliver an unforgettable culinary experience'                       },
];

export default function HowItWorksSection() {
    return (
        <section id="how-it-works" className="py-24 relative overflow-hidden" style={{ background: 'linear-gradient(180deg, #fff, var(--color-light, #fff8f3), #fff)' }}>
            {/* Soft bg blobs */}
            <div className="absolute top-20 left-10 w-64 h-64 rounded-full blur-3xl pointer-events-none" style={{ background: 'rgba(255,182,39,0.05)' }} />
            <div className="absolute bottom-20 right-10 w-72 h-72 rounded-full blur-3xl pointer-events-none" style={{ background: 'rgba(255,107,53,0.05)' }} />

            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative z-10">
                <SectionHeader
                    eyebrow="Simple & Seamless"
                    title={<>How ENYVORA <span className="t-gradient">Works</span></>}
                    subtitle="Your journey to exceptional catering in four elegant steps"
                />

                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8 relative mt-16">
                    {/* Connector line (desktop) */}
                    <div className="hidden lg:block absolute top-10 left-0 right-0 h-px" style={{ background: 'linear-gradient(90deg, rgba(255,182,39,0.15), rgba(255,107,53,0.35), rgba(255,182,39,0.15))' }} />

                    {steps.map((step, index) => (
                        <motion.div
                            key={step.id}
                            initial={{ opacity: 0, y: 40 }}
                            whileInView={{ opacity: 1, y: 0 }}
                            viewport={{ once: true }}
                            transition={{ duration: 0.45, delay: index * 0.12 }}
                            className="relative group"
                        >
                            <div className="bg-white rounded-2xl p-8 shadow-card hover:shadow-card-hover transition-all duration-300 border border-neutral-100 hover:border-accent/30 group-hover:-translate-y-2 relative z-10">
                                {/* Step number badge */}
                                <div
                                    className="absolute -top-4 -right-4 w-10 h-10 rounded-full flex items-center justify-center text-white font-bold text-sm shadow-lg group-hover:scale-110 transition-transform"
                                    style={{ background: 'var(--gradient-catering)' }}
                                >
                                    {step.id}
                                </div>

                                {/* Icon */}
                                <div
                                    className="w-16 h-16 rounded-2xl flex items-center justify-center mx-auto mb-5 group-hover:scale-110 transition-transform"
                                    style={{ background: 'rgba(255,107,53,0.08)', color: 'var(--color-primary)' }}
                                >
                                    <step.Icon size={28} strokeWidth={1.75} />
                                </div>

                                <h3 className="text-lg font-bold text-neutral-900 mb-3 text-center">{step.title}</h3>
                                <p className="text-neutral-600 text-center text-sm leading-relaxed">{step.description}</p>

                                <div
                                    className="mt-6 h-1 w-12 rounded-full mx-auto group-hover:w-full transition-all duration-500"
                                    style={{ background: 'var(--gradient-catering)' }}
                                />
                            </div>

                            {/* Arrow connector (desktop) */}
                            {index < steps.length - 1 && (
                                <div className="hidden lg:flex absolute top-10 -right-5 z-20 items-center justify-center">
                                    <ArrowRight size={20} className="text-accent opacity-60" />
                                </div>
                            )}
                        </motion.div>
                    ))}
                </div>

                <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    transition={{ duration: 0.5, delay: 0.6 }}
                    className="text-center mt-14"
                >
                    <a
                        href="/caterings"
                        className="inline-flex items-center gap-2 px-8 py-4 rounded-xl text-white font-bold transition-all duration-300 hover:scale-105"
                        style={{ background: 'var(--gradient-catering)', boxShadow: 'var(--shadow-cta)' }}
                    >
                        Get Started Now <ArrowRight size={18} />
                    </a>
                </motion.div>
            </div>
        </section>
    );
}
