import React from 'react';


export default function CategoryTiles({ categories = [] }) {
    const serviceCategories = categories.length ? categories : [
        {
            id: 1,
            title: 'Wedding Catering',
            description: 'Elegant multi-course menus for your special day',
            icon: '💍',
            offer: 'UP TO 20% OFF',
            image: 'https://images.unsplash.com/photo-1511795409834-ef04bbd61622?q=80&w=600&auto=format&fit=crop', // Placeholder image
            link: '/services/wedding',
            color: 'from-pink-50 to-rose-100', // Custom gradient for each card
            textColor: 'text-rose-700',
            offerColor: 'bg-rose-600'
        },
        {
            id: 2,
            title: 'Corporate Events',
            description: 'Professional catering for business functions',
            icon: '🏢',
            offer: 'FREE CONSULTATION',
            image: 'Corporate.jpg',
            link: '/services/corporate',
            color: 'from-blue-50 to-indigo-100',
            textColor: 'text-indigo-700',
            offerColor: 'bg-indigo-600'
        },
        {
            id: 3,
            title: 'Party & Celebrations',
            description: 'Bulk orders for birthdays, anniversaries & gatherings',
            icon: '🎉',
            offer: 'BULK DISCOUNTS',
            image: 'Party.png', // Placeholder image
            link: '/services/parties',
            color: 'from-amber-50 to-orange-100',
            textColor: 'text-orange-700',
            offerColor: 'bg-orange-600'
        },
        {
            id: 4,
            title: 'Decorations',
            description: 'Themed decorations to light up your events',
            icon: '🎈',
            offer: 'NEW ARRIVAL',
            image: 'Decorations.png', // Placeholder image
            link: '/services/decorations',
            color: 'from-purple-50 to-fuchsia-100',
            textColor: 'text-fuchsia-700',
            offerColor: 'bg-fuchsia-600'
        }
    ];

    return (
        <section className="py-12 px-4 sm:px-6 lg:px-8 bg-white">
            <div className="max-w-7xl mx-auto">
                {/* Section header */}
                <div className="text-center mb-10">
                    <h2 className="text-3xl font-bold text-gray-900 mb-3 tracking-tight">
                        Services Tailored for Every Occasion
                    </h2>
                    <p className="text-lg text-gray-500 max-w-2xl mx-auto">
                        Choose from our specialized catering services designed to make your event unforgettable.
                    </p>
                </div>

                {/* Service cards grid - 4 in a row on large screens */}
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
                    {serviceCategories.map((service) => (
                        <a
                            key={service.id}
                            href={service.link}
                            className="group relative flex flex-col h-full overflow-hidden rounded-2xl shadow-sm hover:shadow-xl transition-all duration-300 border border-gray-100 bg-white transform hover:-translate-y-1"
                        >
                            {/* Image container */}
                            <div className="relative h-48 w-full overflow-hidden">
                                <div className={`absolute inset-0 bg-gradient-to-t from-black/60 to-transparent z-10 transition-opacity duration-300 opacity-80 group-hover:opacity-60`} />
                                <img
                                    src={service.image}
                                    alt={service.title}
                                    loading="lazy"
                                    className="w-full h-full object-cover transition-transform duration-700 group-hover:scale-110"
                                />
                                {/* Offer Badge */}
                                {service.offer && (
                                    <div className={`absolute top-3 right-3 z-20 ${service.offerColor} text-white text-[10px] font-bold px-2 py-1 rounded shadow-sm uppercase tracking-wide`}>
                                        {service.offer}
                                    </div>
                                )}
                                {/* Icon Overlay */}
                                <div className="absolute bottom-3 left-4 z-20 flex items-center gap-2">
                                    <span className="text-3xl filter drop-shadow-md">{service.icon}</span>
                                </div>
                            </div>

                            {/* Content */}
                            <div className={`flex flex-col flex-grow p-5 bg-gradient-to-b ${service.color}`}>
                                <h3 className={`text-lg font-bold mb-2 ${service.textColor} group-hover:underline decoration-2 underline-offset-2`}>
                                    {service.title}
                                </h3>
                                <p className="text-sm text-gray-600 leading-relaxed">
                                    {service.description}
                                </p>

                                {/* Arrow indicator */}
                                <div className="mt-auto pt-4 flex justify-end">
                                    <div className={`p-2 rounded-full bg-white/50 group-hover:bg-white text-gray-400 group-hover:${service.textColor} transition-colors shadow-sm`}>
                                        <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                            <path fillRule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clipRule="evenodd" />
                                        </svg>
                                    </div>
                                </div>
                            </div>
                        </a>
                    ))}
                </div>
            </div>
        </section>
    );
}
