/*
========================================
File: src/components/owner/dashboard/menu/FoodItemGrid.jsx (NEW FILE)
========================================
Handles the filtering, searching, pagination, and display of food item cards.
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
        <div className="bg-white p-6 rounded-xl shadow-sm">

            {/* ----------------------------- FILTER BAR ----------------------------- */}
            <div className="flex flex-col md:flex-row justify-between items-center mb-6 gap-4">

                {/* Toggles */}
                <div className="flex items-center gap-4">
                    <ToggleSwitch
                        label="Package Item?"
                        enabled={filters.isPackageItem}
                        setEnabled={(v) => handleFilterChange("isPackageItem", v)}
                    />

                    <ToggleSwitch
                        label="Sample Taste?"
                        enabled={filters.isSampleTaste}
                        setEnabled={(v) => handleFilterChange("isSampleTaste", v)}
                    />
                </div>

                {/* Add Button */}
                <button
                    onClick={onAddItem}
                    className="bg-blue-600 text-white px-4 py-2 rounded-lg font-semibold hover:bg-blue-700 flex items-center gap-2 w-full md:w-auto"
                >
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                        <path d="M10 3a1 1 0 011 1v5h5a1 1 0 110 2h-5v5a1 1 0 11-2 0v-5H4a1 1 0 110-2h5V4a1 1 0 011-1z" />
                    </svg>
                    Add Food Item
                </button>
            </div>

            {/* ----------------------------- FILTER INPUTS ----------------------------- */}
            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4 mb-6 pb-6 border-b">

                {/* Search */}
                <input
                    type="text"
                    name="name"
                    placeholder="Search by name..."
                    value={filters.name}
                    onChange={(e) => handleFilterChange("name", e.target.value)}
                    autoComplete="off"
                    className="w-full px-3 py-2 border border-neutral-300 rounded-md"
                />

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
                    className="w-full px-3 py-2 border border-neutral-300 rounded-md"
                >
                    <option value="">Any Status</option>
                    <option value="true">Active</option>
                    <option value="false">Inactive</option>
                </select>
            </div>

            {/* ----------------------------- GRID ----------------------------- */}
            {items.length > 0 ? (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                    {items.map(item => (
                        <FoodItemCard
                            key={item.id}
                            item={item}
                            onEdit={() => onEditItem(item)}
                            onDelete={() => onDeleteItem(item)}
                        />
                    ))}
                </div>
            ) : (
                <div className="text-center py-16">
                    <h3 className="text-xl font-semibold text-neutral-700">No Food Items Found</h3>
                    <p className="text-neutral-500">Try adjusting your filters.</p>
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
