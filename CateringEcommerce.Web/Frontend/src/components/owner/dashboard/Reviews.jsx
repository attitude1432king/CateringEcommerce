/*
========================================
File: src/components/owner/dashboard/Reviews.jsx
Reviews & Feedback Management - Live API Data
========================================
*/
import { useState, useEffect, useCallback } from 'react';
import { ownerApiService } from '../../../services/ownerApi';

// Star Rating Component
const StarRating = ({ rating, size = 'default' }) => {
    const sizeClasses = {
        small: 'w-4 h-4',
        default: 'w-5 h-5',
        large: 'w-6 h-6'
    };

    return (
        <div className="flex items-center gap-1">
            {[1, 2, 3, 4, 5].map((star) => (
                <svg
                    key={star}
                    className={`${sizeClasses[size]} ${star <= Math.round(rating) ? 'text-yellow-400' : 'text-neutral-300'}`}
                    fill="currentColor"
                    viewBox="0 0 20 20"
                >
                    <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                </svg>
            ))}
        </div>
    );
};

// Review Card Component
const ReviewCard = ({ review, onReply, submittingReplyId }) => {
    const [showReplyBox, setShowReplyBox] = useState(false);
    const [replyText, setReplyText] = useState('');

    const handleSubmitReply = () => {
        if (!replyText.trim()) return;
        onReply(review.reviewId, replyText);
        setReplyText('');
        setShowReplyBox(false);
    };

    const isSubmitting = submittingReplyId === review.reviewId;

    const formatDate = (dateStr) => {
        if (!dateStr) return '';
        const date = new Date(dateStr);
        return date.toLocaleDateString('en-IN', { year: 'numeric', month: 'short', day: 'numeric' });
    };

    return (
        <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 p-6 hover:shadow-md transition-shadow">
            {/* Header */}
            <div className="flex items-start justify-between mb-4">
                <div className="flex items-start gap-4 flex-1">
                    <div className="w-12 h-12 rounded-full border-2 border-neutral-200 bg-indigo-100 flex items-center justify-center text-indigo-700 font-bold text-lg">
                        {review.customerName?.charAt(0)?.toUpperCase() || 'C'}
                    </div>
                    <div className="flex-1">
                        <h3 className="font-bold text-neutral-900">{review.customerName}</h3>
                        <div className="flex items-center gap-2 mt-1">
                            <StarRating rating={review.overallRating} size="small" />
                            <span className="text-sm font-semibold text-neutral-700">{Number(review.overallRating).toFixed(1)}</span>
                            <span className="text-sm text-neutral-500">• {formatDate(review.reviewDate)}</span>
                        </div>
                        {review.orderNumber && (
                            <span className="text-xs text-neutral-400">Order #{review.orderNumber}</span>
                        )}
                    </div>
                </div>
                <div className="flex flex-col items-end gap-1">
                    {review.eventType && (
                        <span className="px-3 py-1 bg-indigo-50 text-indigo-700 rounded-lg text-xs font-semibold">
                            {review.eventType}
                        </span>
                    )}
                    {review.isVerified && (
                        <span className="px-2 py-0.5 bg-green-50 text-green-700 rounded text-xs font-medium flex items-center gap-1">
                            <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20"><path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" /></svg>
                            Verified
                        </span>
                    )}
                </div>
            </div>

            {/* Review Content */}
            {review.reviewTitle && (
                <h4 className="font-semibold text-neutral-800 mb-1">{review.reviewTitle}</h4>
            )}
            {review.reviewComment && (
                <p className="text-neutral-700 leading-relaxed mb-4">{review.reviewComment}</p>
            )}

            {/* Rating Breakdown */}
            <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-3 p-4 bg-neutral-50 rounded-xl mb-4">
                {review.foodQualityRating != null && (
                    <div className="text-center">
                        <p className="text-xs text-neutral-500 font-medium mb-1">Food Quality</p>
                        <div className="flex items-center justify-center gap-1">
                            <span className="text-sm font-bold text-neutral-900">{Number(review.foodQualityRating).toFixed(1)}</span>
                            <svg className="w-4 h-4 text-yellow-400" fill="currentColor" viewBox="0 0 20 20">
                                <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                            </svg>
                        </div>
                    </div>
                )}
                {review.hygieneRating != null && (
                    <div className="text-center">
                        <p className="text-xs text-neutral-500 font-medium mb-1">Hygiene</p>
                        <div className="flex items-center justify-center gap-1">
                            <span className="text-sm font-bold text-neutral-900">{Number(review.hygieneRating).toFixed(1)}</span>
                            <svg className="w-4 h-4 text-yellow-400" fill="currentColor" viewBox="0 0 20 20">
                                <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                            </svg>
                        </div>
                    </div>
                )}
                {review.staffBehaviorRating != null && (
                    <div className="text-center">
                        <p className="text-xs text-neutral-500 font-medium mb-1">Staff</p>
                        <div className="flex items-center justify-center gap-1">
                            <span className="text-sm font-bold text-neutral-900">{Number(review.staffBehaviorRating).toFixed(1)}</span>
                            <svg className="w-4 h-4 text-yellow-400" fill="currentColor" viewBox="0 0 20 20">
                                <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                            </svg>
                        </div>
                    </div>
                )}
                {review.decorationRating != null && (
                    <div className="text-center">
                        <p className="text-xs text-neutral-500 font-medium mb-1">Decoration</p>
                        <div className="flex items-center justify-center gap-1">
                            <span className="text-sm font-bold text-neutral-900">{Number(review.decorationRating).toFixed(1)}</span>
                            <svg className="w-4 h-4 text-yellow-400" fill="currentColor" viewBox="0 0 20 20">
                                <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                            </svg>
                        </div>
                    </div>
                )}
                {review.punctualityRating != null && (
                    <div className="text-center">
                        <p className="text-xs text-neutral-500 font-medium mb-1">Punctuality</p>
                        <div className="flex items-center justify-center gap-1">
                            <span className="text-sm font-bold text-neutral-900">{Number(review.punctualityRating).toFixed(1)}</span>
                            <svg className="w-4 h-4 text-yellow-400" fill="currentColor" viewBox="0 0 20 20">
                                <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                            </svg>
                        </div>
                    </div>
                )}
            </div>

            {/* Owner Reply (if exists) */}
            {review.ownerReply ? (
                <div className="ml-8 p-4 bg-indigo-50 rounded-xl border-l-4 border-indigo-600">
                    <div className="flex items-center gap-2 mb-2">
                        <svg className="w-5 h-5 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 10h10a8 8 0 018 8v2M3 10l6 6m-6-6l6-6" />
                        </svg>
                        <span className="text-sm font-semibold text-indigo-900">Your Reply</span>
                        {review.ownerReplyDate && (
                            <span className="text-xs text-indigo-500">• {formatDate(review.ownerReplyDate)}</span>
                        )}
                    </div>
                    <p className="text-sm text-indigo-800">{review.ownerReply}</p>
                </div>
            ) : (
                <div className="flex gap-3">
                    {!showReplyBox ? (
                        <button
                            onClick={() => setShowReplyBox(true)}
                            className="flex items-center gap-2 px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl font-semibold transition-colors"
                        >
                            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 10h10a8 8 0 018 8v2M3 10l6 6m-6-6l6-6" />
                            </svg>
                            Reply to Review
                        </button>
                    ) : (
                        <div className="flex-1 space-y-3">
                            <textarea
                                value={replyText}
                                onChange={(e) => setReplyText(e.target.value)}
                                placeholder="Write your response..."
                                className="w-full px-4 py-3 border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500 resize-none"
                                rows={3}
                                disabled={isSubmitting}
                            />
                            <div className="flex gap-2">
                                <button
                                    onClick={handleSubmitReply}
                                    disabled={isSubmitting || !replyText.trim()}
                                    className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl font-semibold transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                                >
                                    {isSubmitting ? 'Sending...' : 'Send Reply'}
                                </button>
                                <button
                                    onClick={() => {
                                        setShowReplyBox(false);
                                        setReplyText('');
                                    }}
                                    disabled={isSubmitting}
                                    className="px-4 py-2 bg-neutral-200 hover:bg-neutral-300 text-neutral-700 rounded-xl font-semibold transition-colors"
                                >
                                    Cancel
                                </button>
                            </div>
                        </div>
                    )}
                </div>
            )}
        </div>
    );
};

// Category Rating Bar for stats overview
const CategoryRatingBar = ({ label, value }) => {
    if (value == null) return null;
    const numValue = Number(value);
    const percentage = (numValue / 5) * 100;

    return (
        <div className="flex items-center gap-3">
            <span className="text-sm text-neutral-600 min-w-[100px]">{label}</span>
            <div className="flex-1 bg-neutral-200 rounded-full h-2">
                <div
                    className="bg-gradient-to-r from-indigo-500 to-purple-500 h-2 rounded-full transition-all duration-300"
                    style={{ width: `${percentage}%` }}
                />
            </div>
            <span className="text-sm font-semibold text-neutral-900 min-w-[35px] text-right">{numValue.toFixed(1)}</span>
        </div>
    );
};

export default function Reviews() {
    const [filterRating, setFilterRating] = useState('all');
    const [filterReplyStatus, setFilterReplyStatus] = useState(null); // null = all, true = replied, false = unreplied
    const [reviews, setReviews] = useState([]);
    const [stats, setStats] = useState(null);
    const [loading, setLoading] = useState(true);
    const [statsLoading, setStatsLoading] = useState(true);
    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [totalCount, setTotalCount] = useState(0);
    const [submittingReplyId, setSubmittingReplyId] = useState(null);
    const [error, setError] = useState(null);

    const pageSize = 10;

    const fetchReviews = useCallback(async () => {
        setLoading(true);
        setError(null);
        try {
            const filters = {};
            if (filterRating !== 'all') {
                filters.rating = parseInt(filterRating);
            }
            if (filterReplyStatus !== null) {
                filters.hasReply = filterReplyStatus;
            }

            const response = await ownerApiService.getReviews(page, pageSize, filters);
            if (response?.result && response.data) {
                setReviews(response.data.reviews || []);
                setTotalPages(response.data.totalPages || 1);
                setTotalCount(response.data.totalCount || 0);
            } else {
                setReviews([]);
                setTotalCount(0);
            }
        } catch (err) {
            console.error('Error fetching reviews:', err);
            setError('Failed to load reviews.');
            setReviews([]);
        } finally {
            setLoading(false);
        }
    }, [page, filterRating, filterReplyStatus]);

    const fetchStats = useCallback(async () => {
        setStatsLoading(true);
        try {
            const response = await ownerApiService.getReviewStats();
            if (response?.result && response.data) {
                setStats(response.data);
            }
        } catch (err) {
            console.error('Error fetching review stats:', err);
        } finally {
            setStatsLoading(false);
        }
    }, []);

    useEffect(() => {
        fetchReviews();
    }, [fetchReviews]);

    useEffect(() => {
        fetchStats();
    }, [fetchStats]);

    // Reset to page 1 when filters change
    useEffect(() => {
        setPage(1);
    }, [filterRating, filterReplyStatus]);

    const handleReply = async (reviewId, replyText) => {
        setSubmittingReplyId(reviewId);
        try {
            const response = await ownerApiService.submitReviewReply(reviewId, replyText);
            if (response?.result) {
                // Update the review in local state
                setReviews(prev =>
                    prev.map(r =>
                        r.reviewId === reviewId
                            ? { ...r, ownerReply: replyText, ownerReplyDate: new Date().toISOString() }
                            : r
                    )
                );
                // Refresh stats (unreplied count changed)
                fetchStats();
            } else {
                alert(response?.message || 'Failed to submit reply.');
            }
        } catch (err) {
            console.error('Error submitting reply:', err);
            alert('Failed to submit reply. Please try again.');
        } finally {
            setSubmittingReplyId(null);
        }
    };

    const ratingDistribution = stats
        ? [
            { rating: 5, count: stats.fiveStarCount },
            { rating: 4, count: stats.fourStarCount },
            { rating: 3, count: stats.threeStarCount },
            { rating: 2, count: stats.twoStarCount },
            { rating: 1, count: stats.oneStarCount },
        ].map(item => ({
            ...item,
            percentage: stats.totalReviews > 0 ? (item.count / stats.totalReviews) * 100 : 0
        }))
        : [];

    return (
        <div className="min-h-screen bg-neutral-50">
            <div className="p-4 sm:p-6 lg:p-8 space-y-6">
                {/* Header */}
                <div>
                    <h1 className="text-3xl font-bold text-neutral-900">Reviews & Feedback</h1>
                    <p className="text-neutral-600 mt-1">Manage customer reviews and respond to feedback</p>
                </div>

                {/* Rating Overview */}
                {statsLoading ? (
                    <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                        <div className="bg-white rounded-2xl shadow-sm border border-neutral-100 p-6 animate-pulse">
                            <div className="h-6 bg-neutral-200 rounded w-32 mb-4" />
                            <div className="h-16 bg-neutral-200 rounded w-20 mx-auto mb-2" />
                            <div className="h-4 bg-neutral-200 rounded w-24 mx-auto" />
                        </div>
                        <div className="lg:col-span-2 bg-white rounded-2xl shadow-sm border border-neutral-100 p-6 animate-pulse">
                            <div className="h-6 bg-neutral-200 rounded w-40 mb-4" />
                            <div className="space-y-3">
                                {[1, 2, 3, 4, 5].map(i => (
                                    <div key={i} className="h-4 bg-neutral-200 rounded" />
                                ))}
                            </div>
                        </div>
                    </div>
                ) : stats && (
                    <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                        {/* Overall Rating */}
                        <div className="bg-white rounded-2xl shadow-sm border border-neutral-100 p-6">
                            <h2 className="text-lg font-bold text-neutral-900 mb-4">Overall Rating</h2>
                            <div className="text-center">
                                <div className="text-6xl font-bold text-neutral-900 mb-2">
                                    {Number(stats.averageRating).toFixed(1)}
                                </div>
                                <StarRating rating={Math.round(Number(stats.averageRating))} size="large" />
                                <p className="text-sm text-neutral-500 mt-2">Based on {stats.totalReviews} reviews</p>
                                {stats.unrepliedCount > 0 && (
                                    <p className="text-xs text-orange-600 mt-1 font-medium">
                                        {stats.unrepliedCount} unreplied
                                    </p>
                                )}
                            </div>

                            {/* Category Averages */}
                            <div className="mt-6 space-y-2">
                                <CategoryRatingBar label="Food Quality" value={stats.avgFoodQuality} />
                                <CategoryRatingBar label="Hygiene" value={stats.avgHygiene} />
                                <CategoryRatingBar label="Staff" value={stats.avgStaffBehavior} />
                                <CategoryRatingBar label="Punctuality" value={stats.avgPunctuality} />
                            </div>
                        </div>

                        {/* Rating Distribution */}
                        <div className="lg:col-span-2 bg-white rounded-2xl shadow-sm border border-neutral-100 p-6">
                            <h2 className="text-lg font-bold text-neutral-900 mb-4">Rating Distribution</h2>
                            <div className="space-y-3">
                                {ratingDistribution.map(({ rating, count, percentage }) => (
                                    <div key={rating} className="flex items-center gap-4">
                                        <div className="flex items-center gap-1 min-w-[60px]">
                                            <span className="text-sm font-semibold text-neutral-900">{rating}</span>
                                            <svg className="w-4 h-4 text-yellow-400" fill="currentColor" viewBox="0 0 20 20">
                                                <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                                            </svg>
                                        </div>
                                        <div className="flex-1 bg-neutral-200 rounded-full h-3">
                                            <div
                                                className="bg-gradient-to-r from-indigo-600 to-purple-600 h-3 rounded-full transition-all duration-300"
                                                style={{ width: `${percentage}%` }}
                                            />
                                        </div>
                                        <span className="text-sm font-semibold text-neutral-900 min-w-[40px]">{count}</span>
                                    </div>
                                ))}
                            </div>

                            {/* Quick Stats Cards */}
                            <div className="grid grid-cols-3 gap-4 mt-6">
                                <div className="text-center p-3 bg-green-50 rounded-xl">
                                    <p className="text-2xl font-bold text-green-700">{stats.fiveStarCount + stats.fourStarCount}</p>
                                    <p className="text-xs text-green-600 font-medium">Positive (4-5)</p>
                                </div>
                                <div className="text-center p-3 bg-yellow-50 rounded-xl">
                                    <p className="text-2xl font-bold text-yellow-700">{stats.threeStarCount}</p>
                                    <p className="text-xs text-yellow-600 font-medium">Neutral (3)</p>
                                </div>
                                <div className="text-center p-3 bg-red-50 rounded-xl">
                                    <p className="text-2xl font-bold text-red-700">{stats.twoStarCount + stats.oneStarCount}</p>
                                    <p className="text-xs text-red-600 font-medium">Negative (1-2)</p>
                                </div>
                            </div>
                        </div>
                    </div>
                )}

                {/* Filter Buttons */}
                <div className="flex flex-wrap gap-3">
                    <button
                        onClick={() => setFilterRating('all')}
                        className={`px-4 py-2 rounded-xl font-semibold text-sm transition-all ${
                            filterRating === 'all'
                                ? 'bg-indigo-600 text-white shadow-md'
                                : 'bg-white text-neutral-600 hover:bg-neutral-50 border border-neutral-200'
                        }`}
                    >
                        All Reviews {stats ? `(${stats.totalReviews})` : ''}
                    </button>
                    {[5, 4, 3, 2, 1].map((rating) => {
                        const countMap = stats ? {
                            5: stats.fiveStarCount,
                            4: stats.fourStarCount,
                            3: stats.threeStarCount,
                            2: stats.twoStarCount,
                            1: stats.oneStarCount,
                        } : {};
                        return (
                            <button
                                key={rating}
                                onClick={() => setFilterRating(rating.toString())}
                                className={`px-4 py-2 rounded-xl font-semibold text-sm transition-all ${
                                    filterRating === rating.toString()
                                        ? 'bg-indigo-600 text-white shadow-md'
                                        : 'bg-white text-neutral-600 hover:bg-neutral-50 border border-neutral-200'
                                }`}
                            >
                                {rating} ★ {stats ? `(${countMap[rating] || 0})` : ''}
                            </button>
                        );
                    })}

                    <div className="border-l border-neutral-300 mx-1" />

                    <button
                        onClick={() => setFilterReplyStatus(filterReplyStatus === false ? null : false)}
                        className={`px-4 py-2 rounded-xl font-semibold text-sm transition-all ${
                            filterReplyStatus === false
                                ? 'bg-orange-500 text-white shadow-md'
                                : 'bg-white text-neutral-600 hover:bg-neutral-50 border border-neutral-200'
                        }`}
                    >
                        Unreplied {stats ? `(${stats.unrepliedCount})` : ''}
                    </button>
                    <button
                        onClick={() => setFilterReplyStatus(filterReplyStatus === true ? null : true)}
                        className={`px-4 py-2 rounded-xl font-semibold text-sm transition-all ${
                            filterReplyStatus === true
                                ? 'bg-green-600 text-white shadow-md'
                                : 'bg-white text-neutral-600 hover:bg-neutral-50 border border-neutral-200'
                        }`}
                    >
                        Replied
                    </button>
                </div>

                {/* Error State */}
                {error && (
                    <div className="bg-red-50 border border-red-200 rounded-xl p-4 flex items-center gap-3">
                        <svg className="w-5 h-5 text-red-500 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                        </svg>
                        <p className="text-sm text-red-700">{error}</p>
                        <button
                            onClick={fetchReviews}
                            className="ml-auto text-sm font-semibold text-red-600 hover:text-red-800"
                        >
                            Retry
                        </button>
                    </div>
                )}

                {/* Loading State */}
                {loading ? (
                    <div className="space-y-6">
                        {[1, 2, 3].map(i => (
                            <div key={i} className="bg-white rounded-2xl shadow-sm border border-neutral-200 p-6 animate-pulse">
                                <div className="flex items-start gap-4 mb-4">
                                    <div className="w-12 h-12 rounded-full bg-neutral-200" />
                                    <div className="flex-1">
                                        <div className="h-5 bg-neutral-200 rounded w-32 mb-2" />
                                        <div className="h-4 bg-neutral-200 rounded w-48" />
                                    </div>
                                </div>
                                <div className="h-4 bg-neutral-200 rounded w-full mb-2" />
                                <div className="h-4 bg-neutral-200 rounded w-3/4" />
                            </div>
                        ))}
                    </div>
                ) : reviews.length > 0 ? (
                    <>
                        {/* Reviews List */}
                        <div className="space-y-6">
                            {reviews.map((review) => (
                                <ReviewCard
                                    key={review.reviewId}
                                    review={review}
                                    onReply={handleReply}
                                    submittingReplyId={submittingReplyId}
                                />
                            ))}
                        </div>

                        {/* Pagination */}
                        {totalPages > 1 && (
                            <div className="flex items-center justify-between bg-white rounded-2xl shadow-sm border border-neutral-200 p-4">
                                <p className="text-sm text-neutral-600">
                                    Showing {(page - 1) * pageSize + 1}-{Math.min(page * pageSize, totalCount)} of {totalCount} reviews
                                </p>
                                <div className="flex gap-2">
                                    <button
                                        onClick={() => setPage(p => Math.max(1, p - 1))}
                                        disabled={page === 1}
                                        className="px-4 py-2 bg-white border border-neutral-200 rounded-xl text-sm font-semibold text-neutral-700 hover:bg-neutral-50 disabled:opacity-50 disabled:cursor-not-allowed"
                                    >
                                        Previous
                                    </button>
                                    <div className="flex items-center gap-1">
                                        {Array.from({ length: Math.min(totalPages, 5) }, (_, i) => {
                                            let pageNum;
                                            if (totalPages <= 5) {
                                                pageNum = i + 1;
                                            } else if (page <= 3) {
                                                pageNum = i + 1;
                                            } else if (page >= totalPages - 2) {
                                                pageNum = totalPages - 4 + i;
                                            } else {
                                                pageNum = page - 2 + i;
                                            }
                                            return (
                                                <button
                                                    key={pageNum}
                                                    onClick={() => setPage(pageNum)}
                                                    className={`w-10 h-10 rounded-xl text-sm font-semibold transition-all ${
                                                        page === pageNum
                                                            ? 'bg-indigo-600 text-white shadow-md'
                                                            : 'bg-white text-neutral-600 hover:bg-neutral-50 border border-neutral-200'
                                                    }`}
                                                >
                                                    {pageNum}
                                                </button>
                                            );
                                        })}
                                    </div>
                                    <button
                                        onClick={() => setPage(p => Math.min(totalPages, p + 1))}
                                        disabled={page === totalPages}
                                        className="px-4 py-2 bg-white border border-neutral-200 rounded-xl text-sm font-semibold text-neutral-700 hover:bg-neutral-50 disabled:opacity-50 disabled:cursor-not-allowed"
                                    >
                                        Next
                                    </button>
                                </div>
                            </div>
                        )}
                    </>
                ) : (
                    <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 p-12">
                        <div className="text-center">
                            <svg className="w-20 h-20 mx-auto mb-4 text-neutral-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11.049 2.927c.3-.921 1.603-.921 1.902 0l1.519 4.674a1 1 0 00.95.69h4.915c.969 0 1.371 1.24.588 1.81l-3.976 2.888a1 1 0 00-.363 1.118l1.518 4.674c.3.922-.755 1.688-1.538 1.118l-3.976-2.888a1 1 0 00-1.176 0l-3.976 2.888c-.783.57-1.838-.197-1.538-1.118l1.518-4.674a1 1 0 00-.363-1.118l-3.976-2.888c-.784-.57-.38-1.81.588-1.81h4.914a1 1 0 00.951-.69l1.519-4.674z" />
                            </svg>
                            <h3 className="text-xl font-semibold text-neutral-900 mb-2">No Reviews</h3>
                            <p className="text-neutral-600">
                                {filterRating !== 'all' || filterReplyStatus !== null
                                    ? 'No reviews match the selected filters.'
                                    : "You haven't received any reviews yet."}
                            </p>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
}
