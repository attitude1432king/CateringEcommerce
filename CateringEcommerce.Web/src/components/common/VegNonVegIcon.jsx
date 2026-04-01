/**
 * VegNonVegIcon - Standard Indian food veg/non-veg indicator
 *
 * Follows Indian food app standards (Swiggy, Zomato style)
 * - Green border with green dot = Vegetarian
 * - Red/Brown border with red dot = Non-Vegetarian
 *
 * Props:
 *   placeholder — renders a full thumbnail placeholder with food icon + veg indicator
 *                 used when no image/video is available for a food item card
 */
import React from 'react';

export default function VegNonVegIcon({ isVeg, size = 'md', showLabel = false, placeholder = false, className = '' }) {
    const sizes = {
        sm: 'w-4 h-4',
        md: 'w-5 h-5',
        lg: 'w-6 h-6',
        xl: 'w-8 h-8'
    };

    const dotSizes = {
        sm: 'w-1.5 h-1.5',
        md: 'w-2 h-2',
        lg: 'w-2.5 h-2.5',
        xl: 'w-3 h-3'
    };

    const labelSizes = {
        sm: 'text-[10px]',
        md: 'text-xs',
        lg: 'text-sm',
        xl: 'text-base'
    };

    // Thumbnail placeholder mode — fills the parent container
    if (placeholder) {
        const vegColor = isVeg
            ? { bg: 'from-green-50 to-emerald-100', circle: 'bg-green-100', border: 'border-green-400', dot: 'bg-green-500', text: 'text-green-600' }
            : { bg: 'from-red-50 to-rose-100', circle: 'bg-red-100', border: 'border-red-500', dot: 'bg-red-600', text: 'text-red-700' };

        return (
            <div className={`w-full h-full flex flex-col items-center justify-center gap-3 bg-gradient-to-br ${vegColor.bg} ${className}`}>
                {/* Food plate icon */}
                <div className={`w-16 h-16 rounded-full ${vegColor.circle} flex items-center justify-center shadow-sm`}>
                    <svg className={`w-9 h-9 ${vegColor.text} opacity-70`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
                            d="M3 13.5A9 9 0 0 1 12 4.5M3 13.5A9 9 0 0 0 12 22.5M3 13.5H21M21 13.5A9 9 0 0 0 12 4.5M21 13.5A9 9 0 0 1 12 22.5M12 4.5V3M8 4.5C8 6 9 7 9 8.5M16 4.5C16 6 15 7 15 8.5" />
                    </svg>
                </div>
                {/* Veg/Non-veg indicator */}
                <div className="flex items-center gap-1.5">
                    <div
                        className={`w-5 h-5 border-2 ${vegColor.border} rounded-sm flex items-center justify-center p-0.5`}
                        title={isVeg ? 'Vegetarian' : 'Non-Vegetarian'}
                    >
                        <div className={`w-2 h-2 ${vegColor.dot} rounded-full`} />
                    </div>
                    <span className={`text-xs font-semibold ${vegColor.text}`}>
                        {isVeg ? 'VEG' : 'NON-VEG'}
                    </span>
                </div>
            </div>
        );
    }

    return (
        <div className={`inline-flex items-center gap-1.5 ${className}`}>
            <div
                className={`${sizes[size]} border-2 ${
                    isVeg ? 'border-green-600' : 'border-red-700'
                } rounded-sm flex items-center justify-center p-0.5`}
                title={isVeg ? 'Vegetarian' : 'Non-Vegetarian'}
            >
                <div
                    className={`${dotSizes[size]} ${
                        isVeg ? 'bg-green-600' : 'bg-red-700'
                    } rounded-full`}
                ></div>
            </div>
            {showLabel && (
                <span
                    className={`font-semibold ${labelSizes[size]} ${
                        isVeg ? 'text-green-700' : 'text-red-800'
                    }`}
                >
                    {isVeg ? 'VEG' : 'NON-VEG'}
                </span>
            )}
        </div>
    );
}

/**
 * Badge style variant (rounded pill with background)
 */
export function VegNonVegBadge({ isVeg, size = 'md', className = '' }) {
    const sizes = {
        sm: 'text-[10px] px-2 py-0.5',
        md: 'text-xs px-3 py-1',
        lg: 'text-sm px-4 py-1.5',
        xl: 'text-base px-5 py-2'
    };

    const dotSizes = {
        sm: 'w-1.5 h-1.5',
        md: 'w-2 h-2',
        lg: 'w-2.5 h-2.5',
        xl: 'w-3 h-3'
    };

    return (
        <span
            className={`inline-flex items-center gap-1.5 ${sizes[size]} font-bold rounded-full shadow-lg ${
                isVeg
                    ? 'bg-green-600 text-white'
                    : 'bg-red-700 text-white'
            } ${className}`}
        >
            <span className={`${dotSizes[size]} bg-white rounded-full`}></span>
            {isVeg ? 'VEG' : 'NON-VEG'}
        </span>
    );
}
