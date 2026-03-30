import { useState, useEffect } from 'react';
import { fetchApi } from '../../services/apiUtils';

function formatDiscount(offer) {
    if (offer.discountType === 'Percentage') {
        return `${offer.discountValue}% OFF`;
    }
    return `₹${offer.discountValue.toLocaleString('en-IN')} OFF`;
}

function formatDate(dateStr) {
    if (!dateStr) return null;
    const d = new Date(dateStr);
    return d.toLocaleDateString('en-IN', { day: 'numeric', month: 'short', year: 'numeric' });
}

function OfferCardSkeleton() {
    return (
        <div className="bg-white p-4 rounded-lg shadow-sm flex gap-3 items-center animate-pulse">
            <div className="w-20 h-20 rounded-md bg-gray-200 flex-shrink-0" />
            <div className="flex-1 space-y-2">
                <div className="h-4 bg-gray-200 rounded w-3/4" />
                <div className="h-3 bg-gray-100 rounded w-full" />
                <div className="h-3 bg-gray-100 rounded w-1/2" />
            </div>
        </div>
    );
}

export default function OffersSection() {
    const [offers, setOffers] = useState([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        fetchApi('/User/Coupons/Featured')
            .then(data => {
                if (data?.success && Array.isArray(data.data)) {
                    setOffers(data.data);
                }
            })
            .catch(() => {})
            .finally(() => setLoading(false));
    }, []);

    return (
        <section id="packages">
            <h2 className="text-lg font-semibold mb-3">Latest Offers</h2>

            {loading ? (
                <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
                    <OfferCardSkeleton />
                    <OfferCardSkeleton />
                    <OfferCardSkeleton />
                </div>
            ) : offers.length === 0 ? (
                <p className="text-sm text-gray-400 py-4">No active offers right now. Check back soon!</p>
            ) : (
                <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
                    {offers.map(o => (
                        <div key={o.discountId} className="bg-white p-4 rounded-lg shadow-sm flex gap-3 items-center hover:shadow-md transition">
                            <div className="w-20 h-20 rounded-md bg-gradient-to-tr from-indigo-500 to-cyan-400 flex flex-col items-center justify-center text-white font-bold flex-shrink-0 text-center px-1">
                                <span className="text-sm leading-tight">{formatDiscount(o)}</span>
                            </div>
                            <div className="min-w-0">
                                <div className="font-semibold text-sm truncate">{o.name}</div>
                                {o.description && (
                                    <div className="text-xs text-gray-500 mt-0.5 line-clamp-2">{o.description}</div>
                                )}
                                <div className="flex items-center gap-2 mt-1.5 flex-wrap">
                                    <span className="inline-block bg-indigo-50 text-indigo-700 text-xs font-mono font-semibold px-2 py-0.5 rounded border border-indigo-200">
                                        {o.couponCode}
                                    </span>
                                    {o.validTo && (
                                        <span className="text-xs text-gray-400">Valid till {formatDate(o.validTo)}</span>
                                    )}
                                </div>
                                {o.minOrderValue > 0 && (
                                    <div className="text-xs text-gray-400 mt-0.5">
                                        Min. order ₹{o.minOrderValue.toLocaleString('en-IN')}
                                    </div>
                                )}
                            </div>
                        </div>
                    ))}
                </div>
            )}
        </section>
    );
}
