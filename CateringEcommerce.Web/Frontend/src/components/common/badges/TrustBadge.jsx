import React from 'react';
import { ShieldCheck, Award, Crown, Star } from 'lucide-react';

/**
 * TrustBadge Component
 *
 * Displays catering partner trust indicators on catering cards
 * Levels:
 * - Verified Partner: KYC complete
 * - Trusted Partner: 20+ successful orders
 * - Premium Partner: 50+ orders + 4.5★ rating
 */

const TrustBadge = ({
  level, // 'verified', 'trusted', 'premium'
  orderCount = 0,
  rating = 0,
  showTooltip = true,
  size = 'md', // 'sm', 'md', 'lg'
  inline = false // Display inline vs block
}) => {
  // Badge configuration
  const badges = {
    verified: {
      icon: ShieldCheck,
      label: 'Verified Partner',
      bg: 'bg-blue-100',
      text: 'text-blue-700',
      border: 'border-blue-300',
      tooltip: 'Catering partner has completed KYC verification and background check',
      iconColor: 'text-blue-600'
    },
    trusted: {
      icon: Award,
      label: 'Trusted Partner',
      bg: 'bg-green-100',
      text: 'text-green-700',
      border: 'border-green-300',
      tooltip: 'Established catering partner with 20+ successful events delivered',
      iconColor: 'text-green-600'
    },
    premium: {
      icon: Crown,
      label: 'Premium Partner',
      bg: 'bg-gradient-to-r from-amber-100 to-yellow-100',
      text: 'text-amber-800',
      border: 'border-amber-400',
      tooltip: 'Top-tier catering partner • 50+ events • 4.5★+ rating • Premium service',
      iconColor: 'text-amber-600'
    }
  };

  // Size configuration
  const sizes = {
    sm: {
      badge: 'px-2 py-1 text-xs',
      icon: 'w-3 h-3',
      gap: 'gap-1'
    },
    md: {
      badge: 'px-3 py-1.5 text-sm',
      icon: 'w-4 h-4',
      gap: 'gap-1.5'
    },
    lg: {
      badge: 'px-4 py-2 text-base',
      icon: 'w-5 h-5',
      gap: 'gap-2'
    }
  };

  if (!level || !badges[level]) {
    return null;
  }

  const badge = badges[level];
  const sizeConfig = sizes[size];
  const Icon = badge.icon;

  return (
    <div className={`relative ${inline ? 'inline-block' : 'block'} group`}>
      <div
        className={`
          ${badge.bg} ${badge.text} ${badge.border}
          ${sizeConfig.badge} ${sizeConfig.gap}
          border-2 rounded-full font-semibold
          flex items-center w-fit
          shadow-sm hover:shadow-md transition-all duration-200
        `}
      >
        <Icon className={`${sizeConfig.icon} ${badge.iconColor}`} />
        <span>{badge.label}</span>

        {/* Additional indicators for premium */}
        {level === 'premium' && (
          <div className="flex items-center ml-1">
            <Star className={`${sizeConfig.icon} text-amber-500 fill-amber-500`} />
          </div>
        )}
      </div>

      {/* Tooltip */}
      {showTooltip && (
        <div className="absolute left-0 top-full mt-2 w-64 bg-gray-900 text-white text-xs rounded-lg py-2 px-3 opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200 z-50 shadow-lg">
          <p className="mb-2">{badge.tooltip}</p>

          {/* Additional stats */}
          {(level === 'trusted' || level === 'premium') && orderCount > 0 && (
            <div className="text-gray-300 text-xs mt-1">
              <p>✓ {orderCount}+ successful events</p>
            </div>
          )}

          {level === 'premium' && rating > 0 && (
            <div className="text-gray-300 text-xs">
              <p>⭐ {rating} average rating</p>
            </div>
          )}

          {/* Arrow */}
          <div className="absolute bottom-full left-4 mb-[-1px]">
            <div className="border-4 border-transparent border-b-gray-900"></div>
          </div>
        </div>
      )}
    </div>
  );
};

/**
 * Determine trust level based on catering partner/owner stats
 */
export const getPartnerTrustLevel = (orderCount, rating, isKYCVerified) => {
  if (!isKYCVerified) {
    return null; // No badge if partner not verified
  }

  if (orderCount >= 50 && rating >= 4.5) {
    return 'premium';
  } else if (orderCount >= 20) {
    return 'trusted';
  }

  return 'verified';
};

// Legacy alias for backward compatibility
export const getTrustLevel = getPartnerTrustLevel;

export default TrustBadge;
