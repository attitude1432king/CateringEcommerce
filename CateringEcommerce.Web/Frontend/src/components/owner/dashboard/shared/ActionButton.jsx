/**
 * Modern Action Button Component - Multi-Variant System
 * Professional button system for partner dashboard
 */
import React from 'react';

export default function ActionButton({
    children,
    variant = 'primary', // 'primary', 'secondary', 'success', 'danger', 'ghost', 'outline'
    size = 'md', // 'sm', 'md', 'lg', 'xl'
    icon,
    iconPosition = 'left', // 'left', 'right'
    disabled = false,
    loading = false,
    fullWidth = false,
    onClick,
    type = 'button',
    className = ''
}) {
    const baseClasses = 'inline-flex items-center justify-center font-semibold rounded-xl transition-all duration-200 focus:outline-none focus:ring-4 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed';

    const variants = {
        primary: 'bg-gradient-to-r from-indigo-600 to-purple-600 text-white hover:shadow-lg hover:scale-105 focus:ring-indigo-300',
        secondary: 'bg-white text-neutral-700 border-2 border-neutral-200 hover:border-neutral-300 hover:bg-neutral-50 focus:ring-neutral-200',
        success: 'bg-gradient-to-r from-green-600 to-emerald-600 text-white hover:shadow-lg hover:scale-105 focus:ring-green-300',
        danger: 'bg-gradient-to-r from-red-600 to-rose-600 text-white hover:shadow-lg hover:scale-105 focus:ring-red-300',
        ghost: 'bg-transparent text-neutral-700 hover:bg-neutral-100 focus:ring-neutral-200',
        outline: 'bg-transparent border-2 border-indigo-600 text-indigo-600 hover:bg-indigo-50 focus:ring-indigo-300',
    };

    const sizes = {
        sm: 'px-3 py-1.5 text-sm gap-1.5',
        md: 'px-4 py-2.5 text-sm gap-2',
        lg: 'px-6 py-3 text-base gap-2',
        xl: 'px-8 py-4 text-lg gap-3',
    };

    const classes = `
        ${baseClasses}
        ${variants[variant]}
        ${sizes[size]}
        ${fullWidth ? 'w-full' : ''}
        ${className}
    `.trim().replace(/\s+/g, ' ');

    return (
        <button
            type={type}
            onClick={onClick}
            disabled={disabled || loading}
            className={classes}
        >
            {loading && (
                <svg className="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
            )}
            {!loading && icon && iconPosition === 'left' && (
                <span className="flex-shrink-0">{icon}</span>
            )}
            <span>{children}</span>
            {!loading && icon && iconPosition === 'right' && (
                <span className="flex-shrink-0">{icon}</span>
            )}
        </button>
    );
}

/**
 * Icon Button - Compact button with just an icon
 */
export function IconButton({
    icon,
    variant = 'ghost',
    size = 'md',
    disabled = false,
    onClick,
    title,
    className = ''
}) {
    const baseClasses = 'inline-flex items-center justify-center rounded-xl transition-all duration-200 focus:outline-none focus:ring-4 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed';

    const variants = {
        primary: 'bg-gradient-to-r from-indigo-600 to-purple-600 text-white hover:shadow-lg hover:scale-105 focus:ring-indigo-300',
        ghost: 'bg-transparent text-neutral-600 hover:bg-neutral-100 focus:ring-neutral-200',
        danger: 'bg-transparent text-red-600 hover:bg-red-50 focus:ring-red-200',
    };

    const sizes = {
        sm: 'p-1.5',
        md: 'p-2',
        lg: 'p-3',
    };

    const classes = `
        ${baseClasses}
        ${variants[variant]}
        ${sizes[size]}
        ${className}
    `.trim().replace(/\s+/g, ' ');

    return (
        <button
            type="button"
            onClick={onClick}
            disabled={disabled}
            title={title}
            className={classes}
        >
            {icon}
        </button>
    );
}
