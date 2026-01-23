/**
 * Status Badge Component - Professional Status Indicators
 * Used across booking, orders, and other status displays
 */
import React from 'react';

export default function StatusBadge({
    status,
    children,
    variant, // Override auto-detection
    size = 'md', // 'sm', 'md', 'lg'
    icon,
    pulse = false,
    className = ''
}) {
    // Auto-detect variant based on common status keywords
    const autoVariant = variant || detectVariant(status || children);

    const variants = {
        success: 'bg-green-100 text-green-800 border-green-200',
        warning: 'bg-amber-100 text-amber-800 border-amber-200',
        danger: 'bg-red-100 text-red-800 border-red-200',
        info: 'bg-blue-100 text-blue-800 border-blue-200',
        neutral: 'bg-neutral-100 text-neutral-800 border-neutral-200',
        purple: 'bg-purple-100 text-purple-800 border-purple-200',
    };

    const sizes = {
        sm: 'px-2 py-0.5 text-xs',
        md: 'px-2.5 py-1 text-xs',
        lg: 'px-3 py-1.5 text-sm',
    };

    const icons = {
        success: (
            <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
            </svg>
        ),
        warning: (
            <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
            </svg>
        ),
        danger: (
            <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
            </svg>
        ),
    };

    const baseClasses = 'inline-flex items-center font-semibold rounded-full border transition-colors';

    const classes = `
        ${baseClasses}
        ${variants[autoVariant]}
        ${sizes[size]}
        ${className}
    `.trim().replace(/\s+/g, ' ');

    return (
        <span className={classes}>
            {pulse && (
                <span className="relative flex h-2 w-2 mr-1.5">
                    <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-current opacity-75"></span>
                    <span className="relative inline-flex rounded-full h-2 w-2 bg-current"></span>
                </span>
            )}
            {icon || icons[autoVariant]}
            {(icon || icons[autoVariant]) && <span className="ml-1.5">{children || status}</span>}
            {!icon && !icons[autoVariant] && (children || status)}
        </span>
    );
}

/**
 * Auto-detect variant based on status text
 */
function detectVariant(text) {
    if (!text) return 'neutral';

    const lower = String(text).toLowerCase();

    // Success states
    if (lower.match(/confirm|complet|success|active|approv|paid|deliver|accept/)) {
        return 'success';
    }

    // Warning states
    if (lower.match(/pending|progress|processing|review|wait|schedul/)) {
        return 'warning';
    }

    // Danger states
    if (lower.match(/cancel|reject|fail|error|decline|expir|overdue/)) {
        return 'danger';
    }

    // Info states
    if (lower.match(/new|info|draft/)) {
        return 'info';
    }

    // Purple states
    if (lower.match(/premium|vip|special/)) {
        return 'purple';
    }

    return 'neutral';
}

/**
 * Booking Status Badge - Specialized for bookings
 */
export function BookingStatusBadge({ status }) {
    const statusConfig = {
        'Pending': { pulse: true },
        'Confirmed': { icon: '✓' },
        'Completed': { icon: '✓' },
        'Cancelled': { icon: '✕' },
        'Rejected': { icon: '✕' },
    };

    const config = statusConfig[status] || {};

    return <StatusBadge status={status} {...config} />;
}
