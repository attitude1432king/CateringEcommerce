/*
========================================
File: src/components/owner/dashboard/banners/BannerCard.jsx
Modern Banner Card Component
========================================
*/
import React from 'react';
import ToggleSwitch from '../../../common/ToggleSwitch';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '');

export default function BannerCard({ item, onEdit, onDelete, onStatusChange }) {
    const imageUrl = item.imagePath ? `${API_BASE_URL}${item.imagePath}` : null;

    // Format dates
    const formatDate = (dateString) => {
        if (!dateString) return 'Not set';
        return new Date(dateString).toLocaleDateString('en-IN', {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        });
    };

    return (
        <div className="group bg-white rounded-2xl shadow-sm border border-neutral-100 overflow-hidden transition-all duration-300 hover:shadow-xl hover:border-indigo-200">
            {/* Banner Image Preview */}
            <div className="relative h-48 bg-gradient-to-br from-neutral-100 to-neutral-200 overflow-hidden">
                {imageUrl ? (
                    <img
                        src={imageUrl}
                        alt={item.title}
                        className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-300"
                    />
                ) : (
                    <div className="flex items-center justify-center h-full">
                        <svg className="w-16 h-16 text-neutral-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                        </svg>
                    </div>
                )}

                {/* Status Badge */}
                <div className="absolute top-3 right-3">
                    <span className={`px-3 py-1 rounded-full text-xs font-bold ${
                        item.isActive
                            ? 'bg-green-100 text-green-800 border border-green-200'
                            : 'bg-neutral-100 text-neutral-600 border border-neutral-200'
                    }`}>
                        {item.isActive ? 'Active' : 'Inactive'}
                    </span>
                </div>

                {/* Display Order Badge */}
                <div className="absolute top-3 left-3">
                    <span className="px-3 py-1 rounded-full text-xs font-bold bg-indigo-600 text-white">
                        Order: {item.displayOrder}
                    </span>
                </div>
            </div>

            <div className="p-6">
                {/* Title */}
                <h3 className="text-lg font-bold text-neutral-900 group-hover:text-indigo-600 transition-colors mb-2 line-clamp-1">
                    {item.title}
                </h3>

                {/* Description */}
                {item.description && (
                    <p className="text-sm text-neutral-600 mb-4 line-clamp-2">
                        {item.description}
                    </p>
                )}

                {/* Link URL */}
                {item.linkUrl && (
                    <div className="mb-3 flex items-center gap-2 text-xs text-indigo-600">
                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13.828 10.172a4 4 0 00-5.656 0l-4 4a4 4 0 105.656 5.656l1.102-1.101m-.758-4.899a4 4 0 005.656 0l4-4a4 4 0 00-5.656-5.656l-1.1 1.1" />
                        </svg>
                        <span className="truncate">{item.linkUrl}</span>
                    </div>
                )}

                {/* Date Range */}
                <div className="space-y-2 text-xs text-neutral-600 mb-4 border-t border-neutral-100 pt-3">
                    <div className="flex items-center justify-between">
                        <span>Start Date:</span>
                        <span className="font-semibold">{formatDate(item.startDate)}</span>
                    </div>
                    <div className="flex items-center justify-between">
                        <span>End Date:</span>
                        <span className="font-semibold">{formatDate(item.endDate)}</span>
                    </div>
                </div>

                {/* Stats */}
                <div className="grid grid-cols-2 gap-3 mb-4">
                    <div className="bg-indigo-50 rounded-lg p-3 text-center">
                        <div className="text-2xl font-bold text-indigo-600">{item.viewCount || 0}</div>
                        <div className="text-xs text-neutral-600">Views</div>
                    </div>
                    <div className="bg-purple-50 rounded-lg p-3 text-center">
                        <div className="text-2xl font-bold text-purple-600">{item.clickCount || 0}</div>
                        <div className="text-xs text-neutral-600">Clicks</div>
                    </div>
                </div>

                {/* Toggle Switch */}
                <div className="flex items-center justify-between py-3 border-t border-neutral-100">
                    <span className="text-sm font-medium text-neutral-700">Active Status</span>
                    <ToggleSwitch
                        enabled={item.isActive}
                        setEnabled={() => onStatusChange(!item.isActive)}
                    />
                </div>
            </div>

            {/* Action Buttons */}
            <div className="bg-gradient-to-r from-neutral-50 to-indigo-50 px-6 py-4 flex gap-3 border-t border-neutral-100">
                <button
                    onClick={onEdit}
                    className="flex-1 flex items-center justify-center gap-1 px-4 py-2 bg-white hover:bg-indigo-50 text-indigo-600 rounded-lg font-semibold transition-all shadow-sm hover:shadow"
                >
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                    </svg>
                    Edit
                </button>
                <button
                    onClick={onDelete}
                    className="flex-1 flex items-center justify-center gap-1 px-4 py-2 bg-white hover:bg-red-50 text-red-600 rounded-lg font-semibold transition-all shadow-sm hover:shadow"
                >
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                    </svg>
                    Delete
                </button>
            </div>
        </div>
    );
}
