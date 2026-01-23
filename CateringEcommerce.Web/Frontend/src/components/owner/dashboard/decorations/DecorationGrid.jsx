/*
========================================
File: src/components/owner/dashboard/decorations/DecorationGrid.jsx
Modern Redesign - ENYVORA Brand
========================================
*/
import React from 'react';
import DecorationCard from './DecorationCard';
import Pagination from '../../../common/Pagination';
import MultiSelectDropdown from '../../../common/MultiSelectDropdown';

export default function DecorationGrid({
    items,
    filters,
    setFilters,
    themes,
    packages,
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

    // Dropdown options
    const themeOptions = themes.map(t => ({ id: t.themeId, name: t.themeName }));
    const packageOptions = packages.map(p => ({ id: p.id, name: p.name }));

    return (
        <div className="space-y-6">
            {/* Header Section */}
            <div className="bg-white rounded-2xl shadow-sm border border-neutral-100 p-6">
                <div className="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-4">
                    <div className="flex-1">
                        <h2 className="text-3xl font-bold text-neutral-900">Decorations</h2>
                    </div>

                    {/* Modern Add Button */}
                    <button
                        onClick={onAddItem}
                        className="group flex items-center gap-2 bg-gradient-to-r from-indigo-600 to-purple-600 hover:from-indigo-700 hover:to-purple-700 text-white px-6 py-3 rounded-xl font-semibold shadow-lg hover:shadow-xl transition-all duration-300 transform hover:scale-105"
                    >
                        <svg className="w-5 h-5 transition-transform group-hover:rotate-90 duration-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                        </svg>
                        Add Decoration
                    </button>
                </div>
            </div>

            {/* Filter Section */}
            <div className="bg-white rounded-2xl shadow-sm border border-neutral-100 p-6">
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                    {/* Search with Icon */}
                    <div className="relative">
                        <svg className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                        </svg>
                        <input
                            type="text"
                            placeholder="Search decorations..."
                            value={filters.name}
                            autoComplete="off"
                            onChange={(e) => handleFilterChange('name', e.target.value)}
                            className="w-full pl-10 pr-4 py-3 border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition-all"
                        />
                    </div>

                    {/* Themes */}
                    <MultiSelectDropdown
                        placeholder="All Themes"
                        options={themeOptions}
                        selectedIds={filters.themeIds}
                        onChange={(ids) => handleFilterChange('themeIds', ids)}
                    />

                    {/* Status */}
                    <select
                        value={filters.status}
                        onChange={(e) => handleFilterChange('status', e.target.value)}
                        className="w-full px-4 py-3 border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition-all"
                    >
                        <option value="">Any Status</option>
                        <option value="true">Active</option>
                        <option value="false">Inactive</option>
                    </select>

                    {/* Package filter */}
                    <MultiSelectDropdown
                        placeholder="All Packages"
                        options={packageOptions}
                        selectedIds={filters.packageIds}
                        onChange={(ids) => handleFilterChange('packageIds', ids)}
                    />
                </div>
            </div>

            {/* Grid Section with Animations */}
            {items.length > 0 ? (
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 animate-fade-in">
                    {items.map((item, index) => (
                        <div
                            key={item.id}
                            className="transform transition-all duration-300 hover:scale-105"
                            style={{ animationDelay: `${index * 50}ms` }}
                        >
                            <DecorationCard
                                item={item}
                                onEdit={() => onEditItem(item)}
                                onDelete={() => onDeleteItem(item)}
                                onStatusChange={() => onStatusChange(item, value)}
                            />
                        </div>
                    ))}
                </div>
            ) : (
                <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 p-12">
                    <div className="text-center">
                        <svg className="w-20 h-20 mx-auto mb-4 text-neutral-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z" />
                        </svg>
                        <h3 className="text-xl font-semibold text-neutral-900 mb-2">No Decorations Found</h3>
                        <p className="text-neutral-600 mb-4">Try adjusting your filters or add your first decoration.</p>
                        <button
                            onClick={onAddItem}
                            className="bg-gradient-to-r from-indigo-600 to-purple-600 hover:from-indigo-700 hover:to-purple-700 text-white px-6 py-2.5 rounded-xl font-semibold transition-all"
                        >
                            Add First Decoration
                        </button>
                    </div>
                </div>
            )}

            {/* PAGINATION */}
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
