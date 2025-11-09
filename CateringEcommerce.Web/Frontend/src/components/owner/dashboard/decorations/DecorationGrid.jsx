
/*
========================================
File: src/components/owner/dashboard/decorations/DecorationGrid.jsx (REVISED)
========================================
*/
import React, { useState, useMemo } from 'react';
import DecorationCard from './DecorationCard';
import Pagination from '../../../common/Pagination';
import MultiSelectDropdown from '../../../common/MultiSelectDropdown';

export default function DecorationGrid({ items, themes, packages, onEditItem, onDeleteItem, onAddItem }) {
    const [filters, setFilters] = useState({
        name: '',
        themeIds: [],
        status: '',
        packageIds: [] // New filter state
    });
    const [currentPage, setCurrentPage] = useState(1);
    const [itemsPerPage, setItemsPerPage] = useState(10);

    const handleFilterChange = (name, value) => {
        setCurrentPage(1);
        setFilters(prev => ({ ...prev, [name]: value }));
    };

    const filteredItems = useMemo(() => {
        return items.filter(item => {
            const hasLinkedPackage = item.linkedPackages && item.linkedPackages.length > 0;
            const packageFilterMatch = filters.packageIds.length === 0 ||
                filters.packageIds.some(pid => hasLinkedPackage && item.linkedPackages.some(lp => lp.id === pid));
            const themeFilterMatch = filters.themeIds.length === 0 ||
                filters.themeIds.some(pid => item.themeId === pid);

            return (
                (filters.name === '' || item.name.toLowerCase().includes(filters.name.toLowerCase())) &&
                (filters.status === '' || item.status.toString() === filters.status) &&
                packageFilterMatch &&
                themeFilterMatch
            );
        });
    }, [items, filters]);

    const paginatedItems = useMemo(() => {
        const startIndex = (currentPage - 1) * itemsPerPage;
        return filteredItems.slice(startIndex, startIndex + itemsPerPage);
    }, [filteredItems, currentPage, itemsPerPage]);

    // Options for the new filter
    const packageOptions = packages.map(p => ({ id: p.id, name: p.name }));
    const themeOptions = themes.map(t => ({ id: t.themeId, name: t.themeName }));


    return (
        <div className="bg-white p-6 rounded-xl shadow-sm">
            <div className="flex flex-col md:flex-row justify-between items-center mb-6">
                <h3 className="text-xl font-bold text-neutral-800">Your Decoration Setups</h3>
                <button onClick={onAddItem} className="bg-blue-600 text-white px-4 py-2 rounded-lg font-semibold hover:bg-blue-700 flex items-center gap-2 w-full md:w-auto mt-4 md:mt-0">
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor"><path fillRule="evenodd" d="M10 3a1 1 0 011 1v5h5a1 1 0 110 2h-5v5a1 1 0 11-2 0v-5H4a1 1 0 110-2h5V4a1 1 0 011-1z" clipRule="evenodd" /></svg>
                    Add New Setup
                </button>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 lg:grid-cols-4 gap-4 mb-6 pb-6 border-b">
                <input
                    type="text"
                    name="name"
                    placeholder="Search by name..."
                    value={filters.name}
                    onChange={(e) => handleFilterChange('name', e.target.value)}
                    className="w-full px-3 py-2 border border-neutral-300 rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-rose-500 focus:border-rose-500"
                />

                <MultiSelectDropdown
                    placeholder="All Themes"
                    options={themeOptions}
                    selectedIds={filters.themeIds}
                    onChange={(ids) => handleFilterChange('themeIds', ids)}
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
                    placeholder="All Packages"
                    options={packageOptions}
                    selectedIds={filters.packageIds}
                    onChange={(ids) => handleFilterChange('packageIds', ids)}
                />
            </div>

            {paginatedItems.length > 0 ? (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                    {paginatedItems.map(item => (
                        <DecorationCard
                            key={item.id}
                            item={item}
                            onEdit={() => onEditItem(item)}
                            onDelete={() => onDeleteItem(item)}
                        />
                    ))}
                </div>
            ) : (
                <div className="text-center py-16">
                    <h3 className="text-xl font-semibold text-neutral-700">No Decoration Setups Found</h3>
                    <p className="text-neutral-500 mt-2">Try adjusting your filters or adding a new setup.</p>
                </div>
            )}

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