import React from 'react';


export default function CategoryTiles({ categories = [] }) {
    const serviceCategories = categories.length ? categories : [
        {
            id: 1,
            title: 'Wedding Catering',
            description: 'Elegant multi-course menus for your special day',
            icon: '💍',
            offer: 'UP TO 20% OFF',
            image: 'https://images.unsplash.com/photo-1555939594-58d7cb561a1b?w=400&h=300&fit=crop',
            link: '/services/wedding'
        },
        {
            id: 2,
            title: 'Corporate Events',
            description: 'Professional catering for business functions',
            icon: '🏢',
            offer: 'FREE CONSULTATION',
            image: 'https://images.unsplash.com/photo-1552664730-d307ca884978?w=400&h=300&fit=crop',
            link: '/services/corporate'
        },
        {
            id: 3,
            title: 'Party & Celebrations',
            description: 'Bulk orders for birthdays, anniversaries & gatherings',
            icon: '🎉',
            offer: 'BULK DISCOUNTS',
            image: 'https://images.unsplash.com/photo-1504674900152-b8b6fb4c4973?w=400&h=300&fit=crop',
            link: '/services/parties'
        }
    ];

    return (
        <section className="py-16 md:py-24 px-4 sm:px-6 lg:px-8 bg-gradient-to-b from-white to-catering-light">
            <div className="max-w-7xl mx-auto">
                {/* Section header */}
                <div className="text-center mb-16">
                    <h2 className="section-title text-neutral-900 mb-4">
                        Services Tailored for Every Occasion
                    </h2>
                    <p className="text-lg text-neutral-600 max-w-2xl mx-auto">
                        Choose from our specialized catering services designed to make your event unforgettable.
                    </p>
                </div>

                {/* Service cards grid */}
                <div
                    className="grid grid-cols-1 md:grid-cols-3 gap-6 md:gap-8"
                    role="list"
                >
                    {serviceCategories.map((service) => (
                        <a
                            key={service.id}
                            href={service.link}
                            role="listitem"
                            className="card-premium group flex flex-col overflow-hidden"
                        >
                            {/* Image container */}
                            <div className="relative overflow-hidden h-48 md:h-56 mb-6 rounded-xl">
                                <img
                                    src={service.image}
                                    alt={service.title}
                                    loading="lazy"
                                    decoding="async"
                                    className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-500"
                                />
                                
                                {/* Overlay gradient */}
                                <div className="absolute inset-0 bg-gradient-to-t from-black/30 to-transparent" />

                                {/* Offer badge */}
                                <div className="absolute top-4 right-4 bg-gradient-catering text-white px-4 py-2 rounded-full text-sm font-semibold shadow-lg">
                                    {service.offer}
                                </div>
                            </div>

                            {/* Content */}
                            <div className="flex flex-col flex-grow">
                                <div className="flex items-start justify-between mb-3">
                                    <h3 className="text-xl md:text-2xl font-bold text-neutral-900 group-hover:text-catering-primary transition-colors">
                                        {service.title}
                                    </h3>
                                    <span className="text-3xl ml-2">{service.icon}</span>
                                </div>

                                <p className="text-neutral-600 text-sm md:text-base mb-6 flex-grow">
                                    {service.description}
                                </p>

                                {/* CTA */}
                                <div className="flex items-center gap-2 text-catering-primary font-semibold group-hover:gap-4 transition-all">
                                    <span>Browse Options</span>
                                    <svg className="w-5 h-5 transform group-hover:translate-x-1 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 7l5 5m0 0l-5 5m5-5H6" />
                                    </svg>
                                </div>
                            </div>
                        </a>
                    ))}
                </div>

                {/* Bottom CTA */}
                <div className="mt-16 text-center">
                    <p className="text-neutral-600 mb-6">
                        Can't find what you're looking for? We offer customized catering solutions for any type of event.
                    </p>
                    <button className="btn-secondary px-8 py-3 md:py-4">
                        Explore All Services
                    </button>
                </div>
            </div>
        </section>
    );
}
