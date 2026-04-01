/*
========================================
File: src/components/owner/dashboard/banners/BannersGrid.jsx
Modern Redesign - ENYVORA Brand
========================================
*/
import React from 'react';
import BannerCard from './BannerCard';
import Pagination from '../../../common/Pagination';
import ToggleSwitch from '../../../common/ToggleSwitch';

export default function BannersGrid({
    items,
    filters,
    setFilters,
    totalCount,
    currentPage,
    itemsPerPage,
    setCurrentPage,
    setItemsPerPage,
    onEditItem,
    onDeleteItem,
    onAddItem,
    onStatusChange
}) {
    const handleFilterChange = (name, value) => {
        setFilters(prev => ({ ...prev, [name]: value }));
        setCurrentPage(1);
    };

    return (
        <div className="space-y-6">
            {/* Header Section */}
            <div className="bg-white rounded-2xl shadow-sm border border-neutral-100 p-6">
                <div className="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-4">
                    <div className="flex-1">
                        <h2 className="text-3xl font-bold text-neutral-900">Banner Management</h2>
                        <p className="text-neutral-600 mt-1">Create and manage promotional banners for your homepage</p>
                    </div>

                    {/* Modern Add Button */}
                    <button
                        onClick={onAddItem}
                        className="group flex items-center gap-2 bg-gradient-to-r from-indigo-600 to-purple-600 hover:from-indigo-700 hover:to-purple-700 text-white px-6 py-3 rounded-xl font-semibold shadow-lg hover:shadow-xl transition-all duration-300 transform hover:scale-105"
                    >
                        <svg className="w-5 h-5 transition-transform group-hover:rotate-90 duration-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                        </svg>
                        Add Banner
                    </button>
                </div>
            </div>

            {/* Filter Section */}
            <div className="bg-white rounded-2xl shadow-sm border border-neutral-100 p-6">
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                    {/* Search with Icon */}
                    <div className="relative">
                        <svg className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                        </svg>
                        <input
                            type="text"
                            placeholder="Search banners..."
                            value={filters.title}
                            autoComplete="off"
                            onChange={(e) => handleFilterChange('title', e.target.value)}
                            className="w-full pl-10 pr-4 py-3 border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition-all"
                        />
                    </div>

                    {/* Status Toggle */}
                    <div className="flex items-center gap-2 px-3">
                        <ToggleSwitch
                            label="Show Active Only"
                            enabled={filters.isActive}
                            setEnabled={(value) => handleFilterChange('isActive', value ? true : null)}
                        />
                    </div>
                </div>
            </div>

            {/* Grid Section with Animations */}
            {items.length > 0 ? (
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6 animate-fade-in">
                    {items.map((item, index) => (
                        <div
                            key={item.id}
                            className="transform transition-all duration-300 hover:scale-105"
                            style={{ animationDelay: `${index * 50}ms` }}
                        >
                            <BannerCard
                                item={item}
                                onEdit={() => onEditItem(item)}
                                onDelete={() => onDeleteItem(item)}
                                onStatusChange={(value) => onStatusChange(item, value)}
                            />
                        </div>
                    ))}
                </div>
            ) : (
                <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 p-12">
                    <div className="text-center">
                        <svg className="w-20 h-20 mx-auto mb-4 text-neutral-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                        </svg>
                        <h3 className="text-xl font-semibold text-neutral-900 mb-2">No Banners Found</h3>
                        <p className="text-neutral-600 mb-4">Try adjusting your filters or create your first banner.</p>
                        <button
                            onClick={onAddItem}
                            className="bg-gradient-to-r from-indigo-600 to-purple-600 hover:from-indigo-700 hover:to-purple-700 text-white px-6 py-2.5 rounded-xl font-semibold transition-all"
                        >
                            Create First Banner
                        </button>
                    </div>
                </div>
            )}

            {/* Pagination */}
            <Pagination
                currentPage={currentPage}
                totalItems={totalCount}
                itemsPerPage={itemsPerPage}
                onPageChange={setCurrentPage}
                onItemsPerPageChange={setItemsPerPage}
            />
        </div>
    );
}
