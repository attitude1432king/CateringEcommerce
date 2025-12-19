import React from 'react';

export default function Testimonials({ reviews = [] }) {
    const data = reviews.length ? reviews : [
        { id: 1, text: 'Feasto made our wedding easy — great food and professional team.', author: 'Priya & Rohit' },
        { id: 2, text: 'Corporate lunch for 200 — punctual & delicious.', author: 'Amit Shah' }
    ];

    return (
        <section aria-labelledby="testimonials-heading">
            <h2 id="testimonials-heading" className="text-xl font-semibold mb-4">What customers say</h2>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                {data.map(r => (
                    <blockquote key={r.id} className="bg-white p-4 rounded-lg shadow-sm">
                        <p className="text-gray-700">{r.text}</p>
                        <footer className="mt-3 text-sm text-gray-600">— {r.author}</footer>
                    </blockquote>
                ))}
            </div>
        </section>
    );
}