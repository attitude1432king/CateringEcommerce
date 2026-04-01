/**
 * ProgressCircle Component
 * Circular progress indicator with percentage
 */

import PropTypes from 'prop-types';

const ProgressCircle = ({
  percentage,
  size = 120,
  strokeWidth = 8,
  color = '#3b82f6',
  backgroundColor = '#e5e7eb',
  showPercentage = true,
  className = '',
}) => {
  const radius = (size - strokeWidth) / 2;
  const circumference = radius * 2 * Math.PI;
  const offset = circumference - (percentage / 100) * circumference;

  return (
    <div className={`inline-flex items-center justify-center ${className}`}>
      <svg width={size} height={size} className="transform -rotate-90">
        {/* Background Circle */}
        <circle
          cx={size / 2}
          cy={size / 2}
          r={radius}
          stroke={backgroundColor}
          strokeWidth={strokeWidth}
          fill="none"
        />

        {/* Progress Circle */}
        <circle
          cx={size / 2}
          cy={size / 2}
          r={radius}
          stroke={color}
          strokeWidth={strokeWidth}
          fill="none"
          strokeDasharray={circumference}
          strokeDashoffset={offset}
          strokeLinecap="round"
          className="transition-all duration-500 ease-out"
        />

        {/* Percentage Text */}
        {showPercentage && (
          <text
            x="50%"
            y="50%"
            dominantBaseline="middle"
            textAnchor="middle"
            className="transform rotate-90"
            style={{ transformOrigin: 'center' }}
          >
            <tspan
              className="text-2xl font-bold"
              fill="#1f2937"
            >
              {Math.round(percentage)}%
            </tspan>
          </text>
        )}
      </svg>
    </div>
  );
};

ProgressCircle.propTypes = {
  percentage: PropTypes.number.isRequired,
  size: PropTypes.number,
  strokeWidth: PropTypes.number,
  color: PropTypes.string,
  backgroundColor: PropTypes.string,
  showPercentage: PropTypes.bool,
  className: PropTypes.string,
};

export default ProgressCircle;
