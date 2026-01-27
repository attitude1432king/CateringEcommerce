import React from 'react';

const offers = [
    { id: 1, title: 'Flat 10% off on Weddings', desc: 'Applicable for bookings above ₹1,00,000' },
    { id: 2, title: 'Bulk Order Discount', desc: 'Contact for corporate bulk menus & pricing' },
    { id: 3, title: 'Free Decoration on Selected Packages', desc: 'Choose Gold or Premium plans' }
];

export default function OffersSection() {
    return (
        <section id="packages">
            <h2 className="text-lg font-semibold mb-3">Latest Offers</h2>
            <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
                {offers.map(o => (
                    <div key={o.id} className="bg-white p-4 rounded-lg shadow-sm flex gap-3 items-center hover:shadow-md transition">
                        <div className="w-20 h-20 rounded-md bg-gradient-to-tr from-indigo-500 to-cyan-400 flex items-center justify-center text-white font-bold">
                            {o.title.split(' ')[1] || 'Hot'}
                        </div>
                        <div>
                            <div className="font-semibold">{o.title}</div>
                            <div className="text-sm text-gray-500">{o.desc}</div>
                        </div>
                    </div>
                ))}
            </div>
        </section>
    );
}
