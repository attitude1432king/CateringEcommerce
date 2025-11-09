/*
========================================
File: src/components/owner/dashboard/menu/FoodItemGrid.jsx (NEW FILE)
========================================
Handles the filtering, searching, pagination, and display of food item cards.
*/
import React, { useState, useMemo } from 'react';
import FoodItemCard from './FoodItemCard';
import Pagination from '../../../common/Pagination'; // A new reusable component
import MultiSelectDropdown from '../../../common/MultiSelectDropdown';
import ToggleSwitch from '../../../common/ToggleSwitch'; // Import new component

export default function FoodItemGrid({ items, categories, cuisines, onEditItem, onDeleteItem, onAddItem }) {
    const [filters, setFilters] = useState({
        name: '',
        categoryIds: [], // Changed to array for multi-select
        status: '',
        isPackageItem: false, // Changed to boolean for toggle
        cuisineIds: [] // Changed to array for multi-select
    });
    const [currentPage, setCurrentPage] = useState(1);
    const [itemsPerPage, setItemsPerPage] = useState(10);

    const handleFilterChange = (name, value) => {
        setCurrentPage(1); // Reset to first page on filter change
        setFilters(prev => ({
            ...prev,
            [name]: value
        }));
    };

    const filteredItems = useMemo(() => {
        return items.filter(item => {
            return (
                (filters.name === '' || item.name.toLowerCase().includes(filters.name.toLowerCase())) &&
                (filters.categoryIds.length === 0 || filters.categoryIds.includes(item.categoryId)) &&
                (filters.status === '' || item.status.toString() === filters.status) &&
                (!filters.isPackageItem || item.isPackageItem === true) &&
                (filters.cuisineIds.length === 0 || filters.cuisineIds.includes(item.cuisineId))
            );
        });
    }, [items, filters]);

    const paginatedItems = useMemo(() => {
        const startIndex = (currentPage - 1) * itemsPerPage;
        return filteredItems.slice(startIndex, startIndex + itemsPerPage);
    }, [filteredItems, currentPage, itemsPerPage]);

    const categoryOptions = categories.map(c => ({ id: c.categoryId, name: c.name }));
    const cuisineOptions = cuisines.map(c => ({ id: c.typeId, name: c.typeName }));

    return (
        <div className="bg-white p-6 rounded-xl shadow-sm">
            <div className="flex flex-col md:flex-row justify-between items-center mb-6">
                <h3 className="text-xl font-bold text-neutral-800">Menu Items</h3>
                <button onClick={onAddItem} className="bg-blue-600 text-white px-4 py-2 rounded-lg font-semibold hover:bg-blue-700 flex items-center gap-2 w-full md:w-auto mt-4 md:mt-0">
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor"><path d="M17.414 2.586a2 2 0 00-2.828 0L7 10.172V13h2.828l7.586-7.586a2 2 0 000-2.828z" /><path fillRule="evenodd" d="M2 6a2 2 0 012-2h4a1 1 0 010 2H4v10h10v-4a1 1 0 112 0v4a2 2 0 01-2 2H4a2 2 0 01-2-2V6z" clipRule="evenodd" /></svg>
                    Add Food Item
                </button>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4 mb-6 pb-6 border-b">
                <input
                    type="text"
                    name="name"
                    placeholder="Search by name..."
                    value={filters.name}
                    onChange={(e) => handleFilterChange('name', e.target.value)}
                    autoComplete="off"
                    className="w-full px-3 py-2 border border-neutral-300 rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-rose-500 focus:border-rose-500"
                />

                <MultiSelectDropdown
                    placeholder="All Categories"
                    options={categoryOptions}
                    selectedIds={filters.categoryIds}
                    onChange={(ids) => handleFilterChange('categoryIds', ids)}
                />

                <select
                    name="status"
                    value={filters.status}
                    onChange={(e) => handleFilterChange('status', e.target.value)}
                    className="w-full px-3 py-2 border border-neutral-300 rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-rose-500 focus:border-rose-500"
                >
                    <option value="">Any Status</option>
                    <option value="true">Active</option>
                    <option value="false">Inactive</option>
                </select>

                <MultiSelectDropdown
                    placeholder="All Cuisines"
                    options={cuisineOptions}
                    selectedIds={filters.cuisineIds}
                    onChange={(ids) => handleFilterChange('cuisineIds', ids)}
                />

                <div className="flex items-center justify-center md:justify-start">
                    <ToggleSwitch
                        label="Package Item?"
                        enabled={filters.isPackageItem}
                        setEnabled={(value) => handleFilterChange('isPackageItem', value)}
                    />
                </div>
            </div>

            {/* Grid of Food Item Cards */}
            {paginatedItems.length > 0 ? (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                    {paginatedItems.map(item => (
                        <FoodItemCard
                            key={item.id}
                            item={item}
                            onEdit={() => onEditItem(item)}
                            onDelete={() => handleDelete(item)}
                        />
                    ))}
                </div>
            ) : (
                <div className="text-center py-16">
                    <h3 className="text-xl font-semibold text-neutral-700">No Food Items Found</h3>
                    <p className="text-neutral-500 mt-2">Try adjusting your filters or adding a new item.</p>
                </div>
            )}


            {/* Pagination Controls */}
            <Pagination
                currentPage={currentPage}
                totalItems={filteredItems.length}
                itemsPerPage={itemsPerPage}
                onPageChange={setCurrentPage}
                onItemsPerPageChange={setItemsPerPage}
            />
        </div>
    );
}
