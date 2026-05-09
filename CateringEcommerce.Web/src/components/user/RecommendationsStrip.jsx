import React from 'react';

export default function RecommendationsStrip({ items = [] }) {
    const data = items.length ? items : [
        { id: 1, name: 'Royal Banquet', price: '₹450/plate', img: 'https://placehold.co/400x300' },
        { id: 2, name: 'Green Feast', price: '₹250/plate', img: 'https://placehold.co/400x300' },
        { id: 3, name: 'Banquet Pro', price: '₹180/plate', img: 'https://placehold.co/400x300' }
    ];

    return (
        <div className="flex gap-4 overflow-x-auto snap-x py-2" role="list">
            {data.map(it => (
                <article key={it.id} role="listitem" className="snap-child min-w-[260px] bg-white rounded-xl shadow p-3 hover:shadow-lg transition">
                    <img src={it.img} loading="lazy" decoding="async" alt={it.name} className="w-full h-36 object-cover rounded-md" />
                    <div className="mt-2">
                        <div className="font-semibold truncate">{it.name}</div>
                        <div className="text-sm text-neutral-500">{it.price}</div>
                    </div>
                </article>
            ))}
        </div>
    );
}