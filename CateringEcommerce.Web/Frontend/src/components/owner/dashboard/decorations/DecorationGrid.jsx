/*
========================================
File: src/components/owner/dashboard/decorations/DecorationGrid.jsx (FINAL)
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
        <div className="bg-white p-6 rounded-xl shadow-sm">

            {/* TITLE + BUTTON */}
            <div className="flex flex-col md:flex-row justify-between items-center mb-6">
                <h3 className="text-xl font-bold text-neutral-800">Decorations</h3>
                <button
                    onClick={onAddItem}
                    className="bg-blue-600 text-white px-4 py-2 rounded-lg font-semibold hover:bg-blue-700 flex items-center gap-2 w-full md:w-auto mt-4 md:mt-0"
                >
                    Add Decoration
                </button>
            </div>

            {/* FILTERS */}
            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4 mb-6 pb-6 border-b">

                {/* Search */}
                <input
                    type="text"
                    placeholder="Search by name..."
                    value={filters.name}
                    autoComplete="off"
                    onChange={(e) => handleFilterChange('name', e.target.value)}
                    className="w-full px-3 py-2 border border-neutral-300 rounded-md shadow-sm"
                />

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
                    className="w-full px-3 py-2 border border-neutral-300 rounded-md shadow-sm"
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

            {/* GRID */}
            {items.length > 0 ? (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                    {items.map(item => (
                        <DecorationCard
                            key={item.id}
                            item={item}
                            onEdit={() => onEditItem(item)}
                            onDelete={() => onDeleteItem(item)}
                            onStatusChange={() => onStatusChange(item, value)}
                        />
                    ))}
                </div>
            ) : (
                <div className="text-center py-16">
                    <h3 className="text-xl font-semibold text-neutral-700">No Decorations Found</h3>
                    <p className="text-neutral-500">Try adjusting your filters.</p>
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
