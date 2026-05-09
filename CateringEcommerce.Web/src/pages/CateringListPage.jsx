import React, { useState, useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { SlidersHorizontal, ArrowLeft, ArrowRight } from 'lucide-react';
import { searchCaterings } from '../services/homeApi';
import { isSuccessResponse, extractData, extractPagination } from '../utils/responseHelpers';
import { useToast } from '../contexts/ToastContext';
import { SkeletonCard, EmptyState } from '../design-system/components';
import CatererCard from '../components/user/common/CateringCard';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '');

export default function CateringListPage() {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();
    const { showToast } = useToast();

    const [caterings,  setCaterings]  = useState([]);
    const [isLoading,  setIsLoading]  = useState(true);
    const [pagination, setPagination] = useState({ totalCount: 0, pageNumber: 1, pageSize: 20, totalPages: 0 });

    const city        = searchParams.get('city') || '';
    const keyword     = searchParams.get('keyword') || '';
    const cuisineTypes = searchParams.get('cuisineTypes') || '';
    const serviceTypes = searchParams.get('serviceTypes') || '';
    const eventTypes  = searchParams.get('eventTypes') || '';
    const minRating   = searchParams.get('minRating') || '';
    const onlineOnly  = searchParams.get('onlineOnly') === 'true';
    const pageNumber  = parseInt(searchParams.get('page') || '1', 10);

    useEffect(() => { fetchCaterings(); }, [city, keyword, cuisineTypes, serviceTypes, eventTypes, minRating, onlineOnly, pageNumber]);

    const fetchCaterings = async () => {
        try {
            setIsLoading(true);
            const response = await searchCaterings({ city, keyword, cuisineTypes, serviceTypes, eventTypes, minRating: minRating ? parseFloat(minRating) : undefined, onlineOnly: onlineOnly || undefined, verifiedOnly: true, pageNumber, pageSize: 20 });
            if (isSuccessResponse(response)) {
                setCaterings(extractData(response) || []);
                setPagination(extractPagination(response));
            } else {
                showToast('Failed to fetch catering services', 'error');
                setCaterings([]);
            }
        } catch (err) {
            console.error('Error fetching caterings:', err);
            showToast('An error occurred while searching', 'error');
            setCaterings([]);
        } finally {
            setIsLoading(false);
        }
    };

    const handlePageChange = (newPage) => {
        const params = new URLSearchParams(searchParams);
        params.set('page', newPage.toString());
        navigate(`/caterings?${params.toString()}`);
        window.scrollTo({ top: 0, behavior: 'smooth' });
    };

    /* Normalize API data to CatererCard props */
    const normalizeCatering = (c) => ({
        ...c,
        logoUrl:  c.logoUrl ? `${API_BASE_URL}${c.logoUrl}` : c.logoUrl,
        status:   c.isOnline ? 'OPEN' : 'CLOSED',
        location: [c.area, c.city].filter(Boolean).join(', '),
        distance: c.deliveryRadiusKm,
    });

    return (
        <div className="min-h-screen bg-neutral-50">
            {/* Page header */}
            <div className="bg-white border-b border-neutral-100 sticky top-[var(--header-height,64px)] z-20">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
                    <div className="flex items-center justify-between gap-4">
                        <div>
                            <h1 className="text-xl font-bold text-neutral-900">
                                {city ? `Catering Services in ${city}` : 'Browse Caterers'}
                            </h1>
                            {keyword && (
                                <p className="text-sm text-neutral-500 mt-0.5">
                                    Results for <span className="font-semibold text-primary">"{keyword}"</span>
                                </p>
                            )}
                            {!isLoading && (
                                <p className="text-xs text-neutral-400 mt-0.5">
                                    {pagination.totalCount} {pagination.totalCount === 1 ? 'result' : 'results'} found
                                </p>
                            )}
                        </div>
                        <div className="flex items-center gap-2 text-sm text-neutral-500">
                            <SlidersHorizontal size={16} />
                            <span className="hidden sm:inline">Filters</span>
                        </div>
                    </div>
                </div>
            </div>

            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                {isLoading ? (
                    <div className="caterer-grid">
                        {Array.from({ length: 9 }).map((_, i) => (
                            <motion.div key={i} initial={{ opacity: 0 }} animate={{ opacity: 1 }} transition={{ delay: i * 0.04 }}>
                                <SkeletonCard />
                            </motion.div>
                        ))}
                    </div>
                ) : caterings.length > 0 ? (
                    <>
                        <div className="caterer-grid">
                            {caterings.map((catering, index) => (
                                <motion.div
                                    key={catering.id}
                                    initial={{ opacity: 0, y: 20 }}
                                    animate={{ opacity: 1, y: 0 }}
                                    transition={{ duration: 0.3, delay: index * 0.04 }}
                                >
                                    <CatererCard catering={normalizeCatering(catering)} />
                                </motion.div>
                            ))}
                        </div>

                        {/* Pagination */}
                        {pagination.totalPages > 1 && (
                            <div className="flex justify-center items-center gap-2 mt-10">
                                <button
                                    onClick={() => handlePageChange(pagination.pageNumber - 1)}
                                    disabled={pagination.pageNumber === 1}
                                    className="icon-btn disabled:opacity-40 disabled:cursor-not-allowed"
                                    aria-label="Previous page"
                                >
                                    <ArrowLeft size={16} />
                                </button>

                                <div className="flex gap-1">
                                    {Array.from({ length: pagination.totalPages }, (_, i) => i + 1)
                                        .filter(p => p === 1 || p === pagination.totalPages || Math.abs(p - pagination.pageNumber) <= 2)
                                        .map((page, idx, arr) => {
                                            const showEllipsis = idx > 0 && arr[idx - 1] !== page - 1;
                                            return (
                                                <React.Fragment key={page}>
                                                    {showEllipsis && <span className="flex items-center px-1 text-neutral-400">…</span>}
                                                    <button
                                                        onClick={() => handlePageChange(page)}
                                                        className={`w-10 h-10 rounded-xl text-sm font-semibold transition-all ${
                                                            page === pagination.pageNumber
                                                                ? 'text-white shadow-md'
                                                                : 'bg-white border border-neutral-200 text-neutral-700 hover:border-primary/40 hover:text-primary'
                                                        }`}
                                                        style={page === pagination.pageNumber ? { background: 'var(--gradient-catering)' } : {}}
                                                    >
                                                        {page}
                                                    </button>
                                                </React.Fragment>
                                            );
                                        })}
                                </div>

                                <button
                                    onClick={() => handlePageChange(pagination.pageNumber + 1)}
                                    disabled={pagination.pageNumber === pagination.totalPages}
                                    className="icon-btn disabled:opacity-40 disabled:cursor-not-allowed"
                                    aria-label="Next page"
                                >
                                    <ArrowRight size={16} />
                                </button>
                            </div>
                        )}
                    </>
                ) : (
                    <EmptyState
                        illustration="🔍"
                        title="No caterers found"
                        description="Try adjusting your search filters or exploring different locations"
                        ctaLabel="Back to Home"
                        ctaOnClick={() => navigate('/')}
                    />
                )}
            </div>
        </div>
    );
}
