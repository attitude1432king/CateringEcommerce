/**
 * VegNonVegIcon - Standard Indian food veg/non-veg indicator
 *
 * Follows Indian food app standards (Swiggy, Zomato style)
 * - Green border with green dot = Vegetarian
 * - Red/Brown border with red dot = Non-Vegetarian
 */
import React from 'react';

export default function VegNonVegIcon({ isVeg, size = 'md', showLabel = false, className = '' }) {
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
