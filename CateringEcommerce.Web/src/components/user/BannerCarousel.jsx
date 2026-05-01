/*
========================================
File: src/components/user/BannerCarousel.jsx
Modern banner carousel for homepage
========================================
*/
import React, { useState, useEffect } from 'react';
import { homeApiService } from '../../services/homeApi';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '');

export default function BannerCarousel() {
    const [banners, setBanners] = useState([]);
    const [currentIndex, setCurrentIndex] = useState(0);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        fetchBanners();
    }, []);

    const fetchBanners = async () => {
        try {
            const data = await homeApiService.getActiveBanners();
            setBanners(data || []);
        } catch (error) {
            console.error('Error fetching banners:', error);
        } finally {
            setIsLoading(false);
        }
    };

    // Auto-advance carousel
    useEffect(() => {
        if (banners.length <= 1) return;

        const timer = setInterval(() => {
            setCurrentIndex((prevIndex) => (prevIndex + 1) % banners.length);
        }, 5000); // Change slide every 5 seconds

        return () => clearInterval(timer);
    }, [banners.length]);

    const handleBannerClick = async (banner) => {
        if (banner.linkUrl) {
            // Track click
            try {
                await homeApiService.trackBannerClick(banner.id);
            } catch (error) {
                console.error('Error tracking banner click:', error);
            }
            window.open(banner.linkUrl, '_blank');
        }
    };

    const goToSlide = (index) => {
        setCurrentIndex(index);
    };

    const goToPrevious = () => {
        setCurrentIndex((prevIndex) =>
            prevIndex === 0 ? banners.length - 1 : prevIndex - 1
        );
    };

    const goToNext = () => {
        setCurrentIndex((prevIndex) =>
            (prevIndex + 1) % banners.length
        );
    };

    if (isLoading) {
        return (
            <div className="w-full h-96 bg-gradient-to-r from-neutral-100 to-neutral-200 animate-pulse rounded-2xl"></div>
        );
    }

    if (banners.length === 0) {
        return null; // Don't show anything if no banners
    }

    return (
        <div className="relative w-full h-96 md:h-[500px] overflow-hidden rounded-2xl shadow-2xl group">
            {/* Banner Images */}
            {banners.map((banner, index) => (
                <div
                    key={banner.id}
                    className={`absolute inset-0 transition-opacity duration-700 ${
                        index === currentIndex ? 'opacity-100' : 'opacity-0'
                    }`}
                    onClick={() => handleBannerClick(banner)}
                    style={{ cursor: banner.linkUrl ? 'pointer' : 'default' }}
                >
                    <img
                        src={`${API_BASE_URL}${banner.imagePath}`}
                        alt={banner.title}
                        className="w-full h-full object-cover"
                    />

                    {/* Gradient Overlay */}
                    <div className="absolute inset-0 bg-gradient-to-t from-black/60 via-black/20 to-transparent"></div>

                    {/* Banner Content */}
                    <div className="absolute bottom-0 left-0 right-0 p-8 md:p-12 text-white">
                        <h2 className="text-3xl md:text-5xl font-bold mb-3 drop-shadow-lg">
                            {banner.title}
                        </h2>
                        {banner.description && (
                            <p className="text-lg md:text-xl mb-4 max-w-2xl drop-shadow-md">
                                {banner.description}
                            </p>
                        )}
                        {banner.linkUrl && (
                            <button className="inline-flex items-center gap-2 bg-white text-neutral-900 px-6 py-3 rounded-xl font-semibold hover:bg-neutral-100 transition-all shadow-lg">
                                Learn More
                                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 7l5 5m0 0l-5 5m5-5H6" />
                                </svg>
                            </button>
                        )}
                    </div>
                </div>
            ))}

            {/* Navigation Arrows - Only show if more than 1 banner */}
            {banners.length > 1 && (
                <>
                    <button
                        onClick={goToPrevious}
                        className="absolute left-4 top-1/2 -translate-y-1/2 bg-white/90 hover:bg-white text-neutral-900 p-3 rounded-full shadow-lg opacity-0 group-hover:opacity-100 transition-all"
                    >
                        <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                        </svg>
                    </button>
                    <button
                        onClick={goToNext}
                        className="absolute right-4 top-1/2 -translate-y-1/2 bg-white/90 hover:bg-white text-neutral-900 p-3 rounded-full shadow-lg opacity-0 group-hover:opacity-100 transition-all"
                    >
                        <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                        </svg>
                    </button>
                </>
            )}

            {/* Dot Indicators - Only show if more than 1 banner */}
            {banners.length > 1 && (
                <div className="absolute bottom-4 left-1/2 -translate-x-1/2 flex gap-2">
                    {banners.map((_, index) => (
                        <button
                            key={index}
                            onClick={() => goToSlide(index)}
                            className={`w-3 h-3 rounded-full transition-all ${
                                index === currentIndex
                                    ? 'bg-white w-8'
                                    : 'bg-white/50 hover:bg-white/75'
                            }`}
                            aria-label={`Go to slide ${index + 1}`}
                        />
                    ))}
                </div>
            )}
        </div>
    );
}
