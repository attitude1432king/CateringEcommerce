/*
========================================
File: src/components/owner/dashboard/menu/FoodItemGrid.jsx
Modern Redesign - ENYVORA Brand
========================================
*/
import React from 'react';
import FoodItemCard from './FoodItemCard';
import Pagination from '../../../common/Pagination';
import MultiSelectDropdown from '../../../common/MultiSelectDropdown';
import ToggleSwitch from '../../../common/ToggleSwitch';

export default function FoodItemGrid({
    items,
    totalCount,
    filters,
    setFilters,
    currentPage,
    setCurrentPage,
    itemsPerPage,
    setItemsPerPage,
    categories,
    cuisines,
    onEditItem,
    onDeleteItem,
    onAddItem
}) {

    const handleFilterChange = (name, value) => {
        setFilters(prev => ({ ...prev, [name]: value }));
        setCurrentPage(1);
    };

    const categoryOptions = categories.map(c => ({ id: c.categoryId, name: c.name }));
    const cuisineOptions = cuisines.map(c => ({ id: c.typeId, name: c.typeName }));

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
                <div>
                    <h1 className="text-3xl font-bold text-neutral-900">Food Items</h1>
                </div>

                {/* Right Side Actions */}
                <div className="flex flex-wrap items-center gap-4">
                    {/* Toggles */}
                    <ToggleSwitch
                        label="Package Items"
                        enabled={filters.isPackageItem}
                        setEnabled={(v) => handleFilterChange("isPackageItem", v)}
                    />
                    <ToggleSwitch
                        label="Sample Taste"
                        enabled={filters.isSampleTaste}
                        setEnabled={(v) => handleFilterChange("isSampleTaste", v)}
                    />

                    {/* Add Button */}
                    <button
                        onClick={onAddItem}
                        className="flex items-center gap-2 text-white px-6 py-3 rounded-xl font-semibold shadow-lg hover:shadow-xl transition-all duration-200"
                        style={{ background: 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)' }}
                    >
                        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                        </svg>
                        Add Food Item
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
                            name="name"
                            placeholder="Search by name..."
                            value={filters.name}
                            onChange={(e) => handleFilterChange("name", e.target.value)}
                            autoComplete="off"
                            className="w-full pl-10 pr-4 py-3 border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-orange-400 focus:border-transparent transition-all"
                        />
                    </div>

                    {/* Category Multi-select */}
                    <MultiSelectDropdown
                        placeholder="All Categories"
                        options={categoryOptions}
                        selectedIds={filters.categoryIds}
                        onChange={(ids) => handleFilterChange("categoryIds", ids)}
                    />

                    {/* Cuisine Multi-select */}
                    <MultiSelectDropdown
                        placeholder="All Cuisines"
                        options={cuisineOptions}
                        selectedIds={filters.cuisineIds}
                        onChange={(ids) => handleFilterChange("cuisineIds", ids)}
                    />

                    {/* Status */}
                    <select
                        name="status"
                        value={filters.status}
                        onChange={(e) => handleFilterChange("status", e.target.value)}
                        className="w-full px-4 py-3 border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-orange-400 focus:border-transparent transition-all"
                    >
                        <option value="">Any Status</option>
                        <option value="true">Active</option>
                        <option value="false">Inactive</option>
                    </select>
                </div>
            </div>

            {/* Grid Section with Modern Cards */}
            {items.length > 0 ? (
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 animate-fade-in">
                    {items.map((item, index) => (
                        <div
                            key={item.id}
                            className="transform transition-all duration-300 hover:scale-105"
                            style={{ animationDelay: `${index * 50}ms` }}
                        >
                            <FoodItemCard
                                item={item}
                                onEdit={() => onEditItem(item)}
                                onDelete={() => onDeleteItem(item)}
                            />
                        </div>
                    ))}
                </div>
            ) : (
                <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 p-12">
                    <div className="text-center">
                        <svg className="w-20 h-20 mx-auto mb-4 text-neutral-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                        </svg>
                        <h3 className="text-xl font-semibold text-neutral-900 mb-2">No Food Items Found</h3>
                        <p className="text-neutral-600 mb-4">Try adjusting your filters or add your first item.</p>
                        <button
                            onClick={onAddItem}
                            className="text-white px-6 py-2.5 rounded-xl font-semibold transition-all"
                            style={{ background: 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)' }}
                        >
                            Add First Item
                        </button>
                    </div>
                </div>
            )}

            {/* ----------------------------- PAGINATION ----------------------------- */}
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
