import React from 'react';
import { Link } from 'react-router-dom';
import { Star, MapPin, ArrowRight, CheckCircle } from 'lucide-react';
import { TrustBadge, getTrustLevel } from '../../common/badges';

export default function CatererCard({ catering }) {
    const trustLevel = getTrustLevel(
        catering.totalOrders || 0,
        catering.averageRating || 0,
        catering.isKYCVerified !== false
    );

    const isOpen = catering.status === 'OPEN';

    return (
        <Link to={`/caterings/${catering.id}`} className="cat-card group">
            {/* Image */}
            <div className="cat-card__img">
                <img
                    src={catering.logoUrl}
                    alt={catering.cateringName}
                    loading="lazy"
                    decoding="async"
                />

                {/* Status pill top-left */}
                <span
                    className={`absolute top-3 left-3 z-10 text-[11px] font-bold px-2.5 py-1 rounded-full border ${
                        isOpen
                            ? 'bg-success/90 text-white border-success/20'
                            : 'bg-neutral-600/90 text-white border-neutral-500/20'
                    }`}
                >
                    {catering.status}
                </span>

                {/* Verified check top-right */}
                {catering.isKYCVerified !== false && (
                    <div className="cat-card__verified">
                        <CheckCircle size={14} className="text-white" fill="white" strokeWidth={0} />
                    </div>
                )}

                {/* Offer pill */}
                {catering.offer && (
                    <div className="cat-card__featured">{catering.offer}</div>
                )}
            </div>

            {/* Body */}
            <div className="p-5 flex flex-col flex-1">
                <div className="flex items-start justify-between gap-2 mb-2">
                    <h3 className="font-bold text-neutral-900 text-base leading-tight group-hover:text-primary transition-colors truncate">
                        {catering.cateringName}
                    </h3>
                </div>

                <p className="text-sm text-neutral-500 truncate mb-3">
                    {(catering.cuisineTypes || []).join(', ')}
                </p>

                {/* Rating + min guests */}
                <div className="flex items-center gap-2 mb-3">
                    <div className="cat-card__score">
                        <Star size={11} fill="white" strokeWidth={0} />
                        {catering.averageRating || '—'}
                    </div>
                    {catering.totalOrders > 0 && (
                        <span className="text-xs text-neutral-400">{catering.totalOrders} orders</span>
                    )}
                    {catering.minGuests && (
                        <span className="text-xs bg-neutral-100 text-neutral-600 px-2 py-0.5 rounded-full ml-auto">
                            {catering.minGuests}+ guests
                        </span>
                    )}
                </div>

                {/* Trust badge */}
                {trustLevel && (
                    <div className="mb-3">
                        <TrustBadge level={trustLevel} orderCount={catering.totalOrders} rating={catering.averageRating} size="sm" inline />
                    </div>
                )}

                {/* Location */}
                <div className="flex items-center gap-1 text-xs text-neutral-400 mb-4">
                    <MapPin size={11} />
                    <span className="truncate">{catering.location}</span>
                    {catering.distance && <span className="shrink-0">• {catering.distance} km</span>}
                </div>

                {/* CTA */}
                <div
                    className="mt-auto flex items-center justify-center gap-2 w-full py-3 rounded-xl text-white font-bold text-sm transition-all duration-200 group-hover:scale-[1.02]"
                    style={{ background: 'var(--gradient-catering)' }}
                >
                    View Details <ArrowRight size={14} />
                </div>
            </div>
        </Link>
    );
}
