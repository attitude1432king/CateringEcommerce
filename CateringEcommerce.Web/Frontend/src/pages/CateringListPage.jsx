/*
========================================
File: src/pages/CateringListPage.jsx
========================================
Displays the grid of catering services with search functionality.
*/
import React, { useState, useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { searchCaterings } from '../services/homeApi';
import { isSuccessResponse, extractData, extractPagination } from '../utils/responseHelpers';
import { useToast } from '../contexts/ToastContext';
import Loader from '../components/common/Loader';

export default function CateringListPage() {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();
    const { showToast } = useToast();

    const [caterings, setCaterings] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [pagination, setPagination] = useState({
        totalCount: 0,
        pageNumber: 1,
        pageSize: 20,
        totalPages: 0
    });

    // Extract search params from URL
    const city = searchParams.get('city') || '';
    const keyword = searchParams.get('keyword') || '';
    const cuisineTypes = searchParams.get('cuisineTypes') || '';
    const serviceTypes = searchParams.get('serviceTypes') || '';
    const eventTypes = searchParams.get('eventTypes') || '';
    const minRating = searchParams.get('minRating') || '';
    const onlineOnly = searchParams.get('onlineOnly') === 'true';
    const pageNumber = parseInt(searchParams.get('page') || '1', 10);

    useEffect(() => {
        fetchCaterings();
    }, [city, keyword, cuisineTypes, serviceTypes, eventTypes, minRating, onlineOnly, pageNumber]);

    const fetchCaterings = async () => {
        try {
            setIsLoading(true);

            const searchFilters = {
                city,
                keyword,
                cuisineTypes,
                serviceTypes,
                eventTypes,
                minRating: minRating ? parseFloat(minRating) : undefined,
                onlineOnly: onlineOnly || undefined,
                verifiedOnly: true,
                pageNumber,
                pageSize: 20
            };

            console.log('Searching with filters:', searchFilters);

            const response = await searchCaterings(searchFilters);

            if (isSuccessResponse(response)) {
                const data = extractData(response);
                const paginationData = extractPagination(response);

                setCaterings(data || []);
                setPagination(paginationData);

                console.log('Search results:', data);
            } else {
                showToast('Failed to fetch catering services', 'error');
                setCaterings([]);
            }
        } catch (error) {
            console.error('Error fetching caterings:', error);
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
    };

    if (isLoading) {
        return (
            <div className="h-screen flex justify-center items-center">
                <Loader />
            </div>
        );
    }

    return (
        <div className="container mx-auto px-4 py-8 min-h-screen">
            {/* Search Summary Header */}
            <div className="mb-8">
                <h1 className="text-2xl md:text-3xl font-bold text-neutral-800 mb-2">
                    {city ? `Catering Services in ${city}` : 'Catering Services'}
                </h1>
                {keyword && (
                    <p className="text-neutral-600 text-sm">
                        Showing results for "<span className="font-semibold text-catering-primary">{keyword}</span>"
                    </p>
                )}
                <p className="text-neutral-500 text-sm mt-1">
                    {pagination.totalCount} {pagination.totalCount === 1 ? 'result' : 'results'} found
                </p>
            </div>

            {/* Results Grid */}
            {caterings.length > 0 ? (
                <>
                    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 mb-8">
                        {caterings.map(catering => (
                            <div
                                key={catering.id}
                                onClick={() => navigate(`/caterings/${catering.id}`)}
                                className="bg-white rounded-2xl shadow-lg hover:shadow-2xl transition-all duration-300 overflow-hidden cursor-pointer group"
                            >
                                {/* Image */}
                                <div className="relative h-48 bg-gradient-to-br from-orange-100 to-rose-100">
                                    {catering.logoUrl ? (
                                        <img
                                            src={`${import.meta.env.VITE_API_BASE_URL}${catering.logoUrl}`}
                                            alt={catering.cateringName}
                                            className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
                                        />
                                    ) : (
                                        <div className="w-full h-full flex items-center justify-center">
                                            <span className="text-6xl">🍽️</span>
                                        </div>
                                    )}
                                    {catering.isOnline && (
                                        <div className="absolute top-3 right-3 bg-green-500 text-white text-xs px-2 py-1 rounded-full font-semibold">
                                            Available
                                        </div>
                                    )}
                                </div>

                                {/* Content */}
                                <div className="p-5">
                                    <h3 className="text-lg font-bold text-neutral-900 mb-2 line-clamp-1">
                                        {catering.cateringName}
                                    </h3>

                                    <div className="flex items-center gap-2 mb-2">
                                        <div className="flex items-center gap-1">
                                            <span className="text-yellow-500">⭐</span>
                                            <span className="text-sm font-semibold text-neutral-700">
                                                {catering.averageRating.toFixed(1)}
                                            </span>
                                        </div>
                                        <span className="text-neutral-400">•</span>
                                        <span className="text-sm text-neutral-600">
                                            {catering.totalReviews} reviews
                                        </span>
                                    </div>

                                    <div className="flex items-center gap-2 text-sm text-neutral-600 mb-3">
                                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                                        </svg>
                                        <span>{catering.area}, {catering.city}</span>
                                    </div>

                                    <div className="flex items-center justify-between pt-3 border-t border-neutral-100">
                                        <div className="text-sm">
                                            <span className="text-neutral-500">Min Order: </span>
                                            <span className="font-bold text-catering-primary">₹{catering.minOrderValue}</span>
                                        </div>
                                        <div className="text-xs text-neutral-500">
                                            {catering.deliveryRadiusKm} km
                                        </div>
                                    </div>
                                </div>
                            </div>
                        ))}
                    </div>

                    {/* Pagination */}
                    {pagination.totalPages > 1 && (
                        <div className="flex justify-center items-center gap-2 mt-8">
                            <button
                                onClick={() => handlePageChange(pagination.pageNumber - 1)}
                                disabled={pagination.pageNumber === 1}
                                className="px-4 py-2 bg-white border border-neutral-300 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-neutral-50 transition-colors"
                            >
                                Previous
                            </button>

                            <div className="flex gap-2">
                                {Array.from({ length: pagination.totalPages }, (_, i) => i + 1)
                                    .filter(page =>
                                        page === 1 ||
                                        page === pagination.totalPages ||
                                        Math.abs(page - pagination.pageNumber) <= 2
                                    )
                                    .map((page, idx, arr) => {
                                        if (idx > 0 && arr[idx - 1] !== page - 1) {
                                            return (
                                                <React.Fragment key={`ellipsis-${page}`}>
                                                    <span className="px-2">...</span>
                                                    <button
                                                        onClick={() => handlePageChange(page)}
                                                        className={`px-4 py-2 rounded-lg transition-colors ${
                                                            page === pagination.pageNumber
                                                                ? 'bg-catering-primary text-white'
                                                                : 'bg-white border border-neutral-300 hover:bg-neutral-50'
                                                        }`}
                                                    >
                                                        {page}
                                                    </button>
                                                </React.Fragment>
                                            );
                                        }
                                        return (
                                            <button
                                                key={page}
                                                onClick={() => handlePageChange(page)}
                                                className={`px-4 py-2 rounded-lg transition-colors ${
                                                    page === pagination.pageNumber
                                                        ? 'bg-catering-primary text-white'
                                                        : 'bg-white border border-neutral-300 hover:bg-neutral-50'
                                                }`}
                                            >
                                                {page}
                                            </button>
                                        );
                                    })}
                            </div>

                            <button
                                onClick={() => handlePageChange(pagination.pageNumber + 1)}
                                disabled={pagination.pageNumber === pagination.totalPages}
                                className="px-4 py-2 bg-white border border-neutral-300 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-neutral-50 transition-colors"
                            >
                                Next
                            </button>
                        </div>
                    )}
                </>
            ) : (
                <div className="text-center py-16">
                    <div className="text-6xl mb-4">🔍</div>
                    <h2 className="text-2xl font-bold text-neutral-800 mb-2">No results found</h2>
                    <p className="text-neutral-600 mb-6">
                        Try adjusting your search filters or exploring different locations
                    </p>
                    <button
                        onClick={() => navigate('/')}
                        className="px-6 py-3 bg-catering-primary text-white rounded-lg hover:bg-catering-secondary transition-colors"
                    >
                        Back to Home
                    </button>
                </div>
            )}
        </div>
    );
}
