import React, { useState } from 'react';

/**
 * Star Rating Component
 * Can be used in read-only mode (display) or interactive mode (selection)
 */
const StarRating = ({
  rating = 0,
  onRatingChange,
  size = 'md',
  readonly = false,
  showValue = false,
  maxRating = 5,
  label = ''
}) => {
  const [hoverRating, setHoverRating] = useState(0);

  const sizes = {
    sm: 'w-4 h-4',
    md: 'w-6 h-6',
    lg: 'w-8 h-8',
    xl: 'w-10 h-10'
  };

  const handleClick = (value) => {
    if (!readonly && onRatingChange) {
      onRatingChange(value);
    }
  };

  const handleMouseEnter = (value) => {
    if (!readonly) {
      setHoverRating(value);
    }
  };

  const handleMouseLeave = () => {
    if (!readonly) {
      setHoverRating(0);
    }
  };

  const getStarColor = (starIndex) => {
    const currentRating = hoverRating || rating;
    if (starIndex <= currentRating) {
      return 'text-yellow-400';
    }
    return 'text-gray-300';
  };

  const stars = Array.from({ length: maxRating }, (_, index) => index + 1);

  return (
    <div className="flex items-center gap-2">
      {label && (
        <span className="text-sm font-medium text-neutral-700 mr-1">{label}</span>
      )}
      <div className="flex items-center gap-1">
        {stars.map((star) => (
          <button
            key={star}
            type="button"
            onClick={() => handleClick(star)}
            onMouseEnter={() => handleMouseEnter(star)}
            onMouseLeave={handleMouseLeave}
            disabled={readonly}
            className={`
              ${readonly ? 'cursor-default' : 'cursor-pointer hover:scale-110'}
              transition-transform duration-150
              ${sizes[size]}
              focus:outline-none
            `}
            aria-label={`${star} star${star !== 1 ? 's' : ''}`}
          >
            <svg
              className={`${sizes[size]} ${getStarColor(star)} transition-colors duration-150`}
              fill="currentColor"
              viewBox="0 0 20 20"
              xmlns="http://www.w3.org/2000/svg"
            >
              <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
            </svg>
          </button>
        ))}
      </div>
      {showValue && (
        <span className="text-sm font-medium text-neutral-700 ml-1">
          {(hoverRating || rating).toFixed(1)}
        </span>
      )}
    </div>
  );
};

export default StarRating;
