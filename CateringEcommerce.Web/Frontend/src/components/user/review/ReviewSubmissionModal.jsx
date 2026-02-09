import React, { useState } from 'react';
import StarRating from '../../common/StarRating';
import { submitReview } from '../../../services/reviewApi';

const ReviewSubmissionModal = ({ isOpen, onClose, order, onReviewSubmitted }) => {
  const [formData, setFormData] = useState({
    orderId: order?.orderId || 0,
    cateringId: order?.cateringId || 0,
    overallRating: 0,
    foodQualityRating: 0,
    hygieneRating: 0,
    staffBehaviorRating: 0,
    decorationRating: 0,
    punctualityRating: 0,
    reviewTitle: '',
    reviewComment: '',
    reviewImageUrls: []
  });

  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState(null);

  const handleRatingChange = (field, value) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);

    // Validation
    if (formData.overallRating === 0) {
      setError('Please provide an overall rating');
      return;
    }

    if (!formData.reviewComment.trim()) {
      setError('Please write a review comment');
      return;
    }

    setIsSubmitting(true);

    try {
      const response = await submitReview(formData);

      if (response.result) {
        // Success!
        if (onReviewSubmitted) {
          onReviewSubmitted(response.data);
        }
        onClose();
      } else {
        setError(response.message || 'Failed to submit review');
      }
    } catch (err) {
      console.error('Error submitting review:', err);
      setError(err.message || 'An error occurred while submitting your review');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4 overflow-y-auto">
      <div className="bg-white rounded-lg max-w-2xl w-full p-6 my-8">
        {/* Header */}
        <div className="flex justify-between items-center mb-6">
          <div>
            <h2 className="text-2xl font-bold text-gray-900">Write a Review</h2>
            <p className="text-gray-600 mt-1">{order?.cateringName}</p>
          </div>
          <button
            onClick={onClose}
            disabled={isSubmitting}
            className="text-gray-400 hover:text-gray-600 transition-colors"
          >
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        {/* Error Message */}
        {error && (
          <div className="mb-4 p-3 bg-red-50 border border-red-200 text-red-700 rounded-lg">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Overall Rating - Required */}
          <div className="bg-yellow-50 p-4 rounded-lg border border-yellow-200">
            <label className="block text-sm font-medium text-gray-900 mb-2">
              Overall Rating <span className="text-red-500">*</span>
            </label>
            <StarRating
              rating={formData.overallRating}
              onRatingChange={(value) => handleRatingChange('overallRating', value)}
              size="xl"
              showValue
            />
          </div>

          {/* Detailed Ratings */}
          <div className="space-y-4">
            <h3 className="font-semibold text-gray-900">Rate Specific Aspects (Optional)</h3>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {/* Food Quality */}
              <div>
                <StarRating
                  label="Food Quality"
                  rating={formData.foodQualityRating}
                  onRatingChange={(value) => handleRatingChange('foodQualityRating', value)}
                  size="md"
                  showValue
                />
              </div>

              {/* Hygiene */}
              <div>
                <StarRating
                  label="Hygiene"
                  rating={formData.hygieneRating}
                  onRatingChange={(value) => handleRatingChange('hygieneRating', value)}
                  size="md"
                  showValue
                />
              </div>

              {/* Staff Behavior */}
              <div>
                <StarRating
                  label="Staff Behavior"
                  rating={formData.staffBehaviorRating}
                  onRatingChange={(value) => handleRatingChange('staffBehaviorRating', value)}
                  size="md"
                  showValue
                />
              </div>

              {/* Decoration */}
              <div>
                <StarRating
                  label="Decoration"
                  rating={formData.decorationRating}
                  onRatingChange={(value) => handleRatingChange('decorationRating', value)}
                  size="md"
                  showValue
                />
              </div>

              {/* Punctuality */}
              <div className="md:col-span-2">
                <StarRating
                  label="Punctuality"
                  rating={formData.punctualityRating}
                  onRatingChange={(value) => handleRatingChange('punctualityRating', value)}
                  size="md"
                  showValue
                />
              </div>
            </div>
          </div>

          {/* Review Title */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Review Title (Optional)
            </label>
            <input
              type="text"
              name="reviewTitle"
              value={formData.reviewTitle}
              onChange={handleInputChange}
              placeholder="e.g., Amazing food and service!"
              maxLength="200"
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-red-500 focus:border-transparent"
            />
          </div>

          {/* Review Comment */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Your Review <span className="text-red-500">*</span>
            </label>
            <textarea
              name="reviewComment"
              value={formData.reviewComment}
              onChange={handleInputChange}
              placeholder="Share your experience with this catering service..."
              rows="6"
              maxLength="2000"
              required
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-red-500 focus:border-transparent resize-none"
            />
            <p className="text-sm text-gray-500 mt-1">
              {formData.reviewComment.length}/2000 characters
            </p>
          </div>

          {/* Photo Upload - Optional for future enhancement */}
          <div className="bg-gray-50 p-4 rounded-lg">
            <p className="text-sm text-gray-600">
              📷 Photo uploads coming soon! For now, you can describe your experience in detail above.
            </p>
          </div>

          {/* Action Buttons */}
          <div className="flex gap-3 pt-4">
            <button
              type="button"
              onClick={onClose}
              disabled={isSubmitting}
              className="flex-1 px-6 py-3 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors disabled:opacity-50 font-medium"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="flex-1 px-6 py-3 bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors disabled:opacity-50 font-medium"
            >
              {isSubmitting ? 'Submitting...' : 'Submit Review'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default ReviewSubmissionModal;
