/*
========================================
File: src/components/owner/dashboard/Reviews.jsx
Modern Redesign - Reviews & Feedback Management
========================================
*/
import React, { useState } from 'react';

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
                    className={`${sizeClasses[size]} ${star <= rating ? 'text-yellow-400' : 'text-neutral-300'}`}
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
const ReviewCard = ({ review, onReply }) => {
    const [showReplyBox, setShowReplyBox] = useState(false);
    const [replyText, setReplyText] = useState('');

    const handleSubmitReply = () => {
        onReply(review.id, replyText);
        setReplyText('');
        setShowReplyBox(false);
    };

    return (
        <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 p-6 hover:shadow-md transition-shadow">
            {/* Header */}
            <div className="flex items-start justify-between mb-4">
                <div className="flex items-start gap-4 flex-1">
                    <img
                        src={review.customerAvatar}
                        alt={review.customerName}
                        className="w-12 h-12 rounded-full border-2 border-neutral-200"
                    />
                    <div className="flex-1">
                        <h3 className="font-bold text-neutral-900">{review.customerName}</h3>
                        <div className="flex items-center gap-2 mt-1">
                            <StarRating rating={review.rating} size="small" />
                            <span className="text-sm text-neutral-500">• {review.date}</span>
                        </div>
                    </div>
                </div>
                <div className="text-right">
                    <span className="px-3 py-1 bg-indigo-50 text-indigo-700 rounded-lg text-xs font-semibold">
                        {review.orderType}
                    </span>
                </div>
            </div>

            {/* Review Content */}
            <p className="text-neutral-700 leading-relaxed mb-4">{review.comment}</p>

            {/* Review Images (if any) */}
            {review.images && review.images.length > 0 && (
                <div className="flex gap-2 mb-4">
                    {review.images.map((image, index) => (
                        <img
                            key={index}
                            src={image}
                            alt={`Review ${index + 1}`}
                            className="w-20 h-20 rounded-lg object-cover border border-neutral-200"
                        />
                    ))}
                </div>
            )}

            {/* Rating Breakdown */}
            <div className="grid grid-cols-3 gap-4 p-4 bg-neutral-50 rounded-xl mb-4">
                <div className="text-center">
                    <p className="text-xs text-neutral-500 font-medium mb-1">Food Quality</p>
                    <div className="flex items-center justify-center gap-1">
                        <span className="text-sm font-bold text-neutral-900">{review.ratings.food}</span>
                        <svg className="w-4 h-4 text-yellow-400" fill="currentColor" viewBox="0 0 20 20">
                            <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                        </svg>
                    </div>
                </div>
                <div className="text-center">
                    <p className="text-xs text-neutral-500 font-medium mb-1">Service</p>
                    <div className="flex items-center justify-center gap-1">
                        <span className="text-sm font-bold text-neutral-900">{review.ratings.service}</span>
                        <svg className="w-4 h-4 text-yellow-400" fill="currentColor" viewBox="0 0 20 20">
                            <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                        </svg>
                    </div>
                </div>
                <div className="text-center">
                    <p className="text-xs text-neutral-500 font-medium mb-1">Presentation</p>
                    <div className="flex items-center justify-center gap-1">
                        <span className="text-sm font-bold text-neutral-900">{review.ratings.presentation}</span>
                        <svg className="w-4 h-4 text-yellow-400" fill="currentColor" viewBox="0 0 20 20">
                            <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                        </svg>
                    </div>
                </div>
            </div>

            {/* Owner Reply (if exists) */}
            {review.ownerReply ? (
                <div className="ml-8 p-4 bg-indigo-50 rounded-xl border-l-4 border-indigo-600">
                    <div className="flex items-center gap-2 mb-2">
                        <svg className="w-5 h-5 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 10h10a8 8 0 018 8v2M3 10l6 6m-6-6l6-6" />
                        </svg>
                        <span className="text-sm font-semibold text-indigo-900">Your Reply</span>
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
                            />
                            <div className="flex gap-2">
                                <button
                                    onClick={handleSubmitReply}
                                    className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl font-semibold transition-colors"
                                >
                                    Send Reply
                                </button>
                                <button
                                    onClick={() => {
                                        setShowReplyBox(false);
                                        setReplyText('');
                                    }}
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

export default function Reviews() {
    const [filterRating, setFilterRating] = useState('all');

    // Mock data - replace with real API data
    const reviews = [
        {
            id: 1,
            customerName: 'Priya Sharma',
            customerAvatar: 'https://ui-avatars.com/api/?name=Priya+Sharma&background=random',
            rating: 5,
            ratings: { food: 5, service: 5, presentation: 5 },
            date: 'Jan 10, 2026',
            orderType: 'Wedding',
            comment: 'Absolutely wonderful experience! The food was delicious and beautifully presented. The staff was professional and courteous. Highly recommend for any event!',
            images: [],
            ownerReply: 'Thank you so much for your kind words! We are thrilled you enjoyed our service. Hope to serve you again!'
        },
        {
            id: 2,
            customerName: 'Rahul Verma',
            customerAvatar: 'https://ui-avatars.com/api/?name=Rahul+Verma&background=random',
            rating: 4,
            ratings: { food: 4, service: 5, presentation: 4 },
            date: 'Jan 8, 2026',
            orderType: 'Corporate Event',
            comment: 'Great service and tasty food! The presentation was good. Only minor issue was timing, but overall very satisfied with the experience.',
            images: [],
            ownerReply: null
        },
        {
            id: 3,
            customerName: 'Anjali Gupta',
            customerAvatar: 'https://ui-avatars.com/api/?name=Anjali+Gupta&background=random',
            rating: 5,
            ratings: { food: 5, service: 5, presentation: 5 },
            date: 'Jan 5, 2026',
            orderType: 'Birthday Party',
            comment: 'Perfect for my son\'s birthday party! Kids loved the food, and parents appreciated the quality. Professional team!',
            images: [],
            ownerReply: 'Thank you! So glad the kids enjoyed it. Happy birthday to your son!'
        },
    ];

    const handleReply = (reviewId, replyText) => {
        console.log('Reply to review:', reviewId, replyText);
        // Implement reply logic here
    };

    const averageRating = (reviews.reduce((sum, r) => sum + r.rating, 0) / reviews.length).toFixed(1);
    const ratingDistribution = [5, 4, 3, 2, 1].map(rating => ({
        rating,
        count: reviews.filter(r => r.rating === rating).length,
        percentage: (reviews.filter(r => r.rating === rating).length / reviews.length) * 100
    }));

    const filteredReviews = filterRating === 'all'
        ? reviews
        : reviews.filter(r => r.rating === parseInt(filterRating));

    return (
        <div className="min-h-screen bg-neutral-50">
            <div className="p-4 sm:p-6 lg:p-8 space-y-6">
                {/* Header */}
                <div>
                    <h1 className="text-3xl font-bold text-neutral-900">Reviews & Feedback</h1>
                    <p className="text-neutral-600 mt-1">Manage customer reviews and respond to feedback</p>
                </div>

                {/* Rating Overview */}
                <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                    {/* Overall Rating */}
                    <div className="bg-white rounded-2xl shadow-sm border border-neutral-100 p-6">
                        <h2 className="text-lg font-bold text-neutral-900 mb-4">Overall Rating</h2>
                        <div className="text-center">
                            <div className="text-6xl font-bold text-neutral-900 mb-2">{averageRating}</div>
                            <StarRating rating={Math.round(parseFloat(averageRating))} size="large" />
                            <p className="text-sm text-neutral-500 mt-2">Based on {reviews.length} reviews</p>
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
                    </div>
                </div>

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
                        All Reviews ({reviews.length})
                    </button>
                    {[5, 4, 3, 2, 1].map((rating) => (
                        <button
                            key={rating}
                            onClick={() => setFilterRating(rating.toString())}
                            className={`px-4 py-2 rounded-xl font-semibold text-sm transition-all ${
                                filterRating === rating.toString()
                                    ? 'bg-indigo-600 text-white shadow-md'
                                    : 'bg-white text-neutral-600 hover:bg-neutral-50 border border-neutral-200'
                            }`}
                        >
                            {rating} ★ ({reviews.filter(r => r.rating === rating).length})
                        </button>
                    ))}
                </div>

                {/* Reviews List */}
                {filteredReviews.length > 0 ? (
                    <div className="space-y-6">
                        {filteredReviews.map((review) => (
                            <ReviewCard key={review.id} review={review} onReply={handleReply} />
                        ))}
                    </div>
                ) : (
                    <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 p-12">
                        <div className="text-center">
                            <svg className="w-20 h-20 mx-auto mb-4 text-neutral-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11.049 2.927c.3-.921 1.603-.921 1.902 0l1.519 4.674a1 1 0 00.95.69h4.915c.969 0 1.371 1.24.588 1.81l-3.976 2.888a1 1 0 00-.363 1.118l1.518 4.674c.3.922-.755 1.688-1.538 1.118l-3.976-2.888a1 1 0 00-1.176 0l-3.976 2.888c-.783.57-1.838-.197-1.538-1.118l1.518-4.674a1 1 0 00-.363-1.118l-3.976-2.888c-.784-.57-.38-1.81.588-1.81h4.914a1 1 0 00.951-.69l1.519-4.674z" />
                            </svg>
                            <h3 className="text-xl font-semibold text-neutral-900 mb-2">No Reviews</h3>
                            <p className="text-neutral-600">
                                {filterRating !== 'all'
                                    ? `No ${filterRating}-star reviews yet.`
                                    : "You haven't received any reviews yet."}
                            </p>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
}