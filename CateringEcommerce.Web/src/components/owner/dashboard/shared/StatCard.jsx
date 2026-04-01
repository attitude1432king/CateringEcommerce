/**
 * Modern Stat Card Component - Shopify/Stripe Style
 * Displays key metrics with icons, trends, and gradients
 */
import React from 'react';

export default function StatCard({
    icon,
    title,
    value,
    change,
    changeType = 'neutral', // 'positive', 'negative', 'neutral'
    subtitle,
    trend,
    onClick,
    loading = false,
    gradient = 'indigo' // 'indigo', 'purple', 'green', 'amber', 'red', 'blue'
}) {
    const gradients = {
        indigo: 'from-indigo-500 to-purple-600',
        purple: 'from-purple-500 to-pink-600',
        green: 'from-green-500 to-emerald-600',
        amber: 'from-amber-500 to-orange-600',
        red: 'from-red-500 to-rose-600',
        blue: 'from-blue-500 to-cyan-600',
    };

    const changeColors = {
        positive: 'text-green-600 bg-green-50',
        negative: 'text-red-600 bg-red-50',
        neutral: 'text-neutral-600 bg-neutral-50',
    };

    if (loading) {
        return (
            <div className="bg-white rounded-2xl p-6 shadow-sm border border-neutral-100 animate-pulse">
                <div className="flex items-center justify-between mb-4">
                    <div className="w-12 h-12 rounded-xl bg-neutral-200"></div>
                    <div className="h-6 w-16 bg-neutral-200 rounded"></div>
                </div>
                <div className="h-8 w-24 bg-neutral-200 rounded mb-2"></div>
                <div className="h-4 w-32 bg-neutral-200 rounded"></div>
            </div>
        );
    }

    return (
        <div
            className={`bg-white rounded-2xl p-6 shadow-sm border border-neutral-100 hover:shadow-md transition-all duration-300 ${
                onClick ? 'cursor-pointer hover:scale-[1.02]' : ''
            }`}
            onClick={onClick}
        >
            {/* Icon and Change */}
            <div className="flex items-center justify-between mb-4">
                <div className={`w-12 h-12 rounded-xl bg-gradient-to-br ${gradients[gradient]} flex items-center justify-center text-white text-2xl shadow-lg`}>
                    {icon}
                </div>
                {change !== undefined && (
                    <span className={`px-2.5 py-1 rounded-full text-xs font-semibold flex items-center gap-1 ${changeColors[changeType]}`}>
                        {changeType === 'positive' && (
                            <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
                                <path fillRule="evenodd" d="M5.293 9.707a1 1 0 010-1.414l4-4a1 1 0 011.414 0l4 4a1 1 0 01-1.414 1.414L11 7.414V15a1 1 0 11-2 0V7.414L6.707 9.707a1 1 0 01-1.414 0z" clipRule="evenodd" />
                            </svg>
                        )}
                        {changeType === 'negative' && (
                            <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
                                <path fillRule="evenodd" d="M14.707 10.293a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 111.414-1.414L9 12.586V5a1 1 0 012 0v7.586l2.293-2.293a1 1 0 011.414 0z" clipRule="evenodd" />
                            </svg>
                        )}
                        {change}
                    </span>
                )}
            </div>

            {/* Value */}
            <h3 className="text-3xl font-bold text-neutral-900 mb-1 tracking-tight">
                {value}
            </h3>

            {/* Title */}
            <p className="text-sm font-medium text-neutral-600">
                {title}
            </p>

            {/* Subtitle/Trend */}
            {(subtitle || trend) && (
                <div className="mt-3 pt-3 border-t border-neutral-100">
                    {subtitle && (
                        <p className="text-xs text-neutral-500">{subtitle}</p>
                    )}
                    {trend && (
                        <div className="mt-2 flex items-center gap-1">
                            <div className="flex-1 h-1 bg-neutral-100 rounded-full overflow-hidden">
                                <div
                                    className={`h-full bg-gradient-to-r ${gradients[gradient]} rounded-full transition-all duration-500`}
                                    style={{ width: `${Math.min(trend, 100)}%` }}
                                ></div>
                            </div>
                            <span className="text-xs font-medium text-neutral-600">{trend}%</span>
                        </div>
                    )}
                </div>
            )}
        </div>
    );
}
