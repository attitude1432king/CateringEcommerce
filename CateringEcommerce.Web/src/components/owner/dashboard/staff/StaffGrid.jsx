/*
========================================
File: src/components/owner/dashboard/staff/StaffGrid.jsx
Modern Redesign - ENYVORA Brand
========================================
*/
import React from 'react';
import Pagination from '../../../common/Pagination';
import { useConfirmation } from '../../../../contexts/ConfirmationContext';
import ToggleSwitch from '../../../common/ToggleSwitch';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '');

// Modern Staff Card Component
const StaffCard = ({ item, onEdit, onDelete, onStatusChange }) => (
    <div className="group bg-white rounded-2xl shadow-sm border border-neutral-100 overflow-hidden transition-all duration-300 hover:shadow-xl hover:border-indigo-200">
        <div className="p-6">
            {/* Header with Avatar */}
            <div className="flex items-start gap-4 mb-4">
                <div className="w-16 h-16 rounded-full bg-gradient-to-br from-indigo-600 to-purple-600 flex items-center justify-center text-white text-2xl font-bold flex-shrink-0">
                    {item.name.charAt(0).toUpperCase()}
                </div>
                <div className="flex-1 min-w-0">
                    <h3 className="text-lg font-bold text-neutral-900 group-hover:text-indigo-600 transition-colors truncate">{item.name}</h3>
                    <p className="text-sm text-neutral-600 mt-1">{item.role}</p>
                    <div className="flex items-center gap-2 mt-2">
                        <span className={`px-2 py-1 rounded-lg text-xs font-semibold ${
                            item.isAvailable
                                ? 'bg-green-100 text-green-800'
                                : 'bg-neutral-100 text-neutral-600'
                        }`}>
                            {item.isAvailable ? 'Available' : 'Unavailable'}
                        </span>
                    </div>
                </div>
            </div>

            {/* Info Grid */}
            <div className="space-y-3 mb-4">
                <div className="flex items-center gap-2 text-sm">
                    <svg className="w-4 h-4 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 13.255A23.931 23.931 0 0112 15c-3.183 0-6.22-.62-9-1.745M16 6V4a2 2 0 00-2-2h-4a2 2 0 00-2 2v2m4 6h.01M5 20h14a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                    </svg>
                    <span className="text-neutral-600">Expertise:</span>
                    <span className="font-semibold text-neutral-900">{item.expertiseName || 'N/A'}</span>
                </div>
                <div className="flex items-center justify-between pt-3 border-t border-neutral-100">
                    <span className="text-xs text-neutral-500">Monthly Salary</span>
                    <span className="text-xl font-bold bg-gradient-to-r from-indigo-600 to-purple-600 bg-clip-text text-transparent">
                        ₹{item.salary?.toLocaleString() || '0'}
                    </span>
                </div>
            </div>

            {/* Availability Toggle */}
            <div className="flex items-center justify-between pt-3 border-t border-neutral-100 mb-3">
                <span className="text-sm font-medium text-neutral-700">Availability Status</span>
                <ToggleSwitch
                    enabled={item.isAvailable}
                    setEnabled={() => onStatusChange(item)}
                />
            </div>
        </div>

        {/* Action Buttons */}
        <div className="bg-gradient-to-r from-neutral-50 to-indigo-50 px-6 py-4 flex gap-3 border-t border-neutral-100">
            <button
                onClick={() => onEdit(item)}
                className="flex-1 flex items-center justify-center gap-1 px-4 py-2 bg-white hover:bg-indigo-50 text-indigo-600 rounded-lg font-semibold transition-all shadow-sm hover:shadow"
            >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                </svg>
                Edit
            </button>
            <button
                onClick={() => onDelete(item)}
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

export default function StaffGrid({
    items,
    totalCount,
    filters,
    setFilters,
    currentPage,
    setCurrentPage,
    itemsPerPage,
    setItemsPerPage,
    onEditItem,
    onDeleteItem,
    onAddItem,
    onStatusChange
}) {

    const confirm = useConfirmation();

    const handleFilterChange = (name, value) => {
        setCurrentPage(1);
        setFilters(prev => ({ ...prev, [name]: value }));
    };

    const handleDelete = (item) => {
        confirm({ type: 'delete', title: 'Delete Staff Member', message: `Are you sure you want to delete "${item.name}"?` })
            .then(isConfirmed => {
                if (isConfirmed) onDeleteItem(item);
            });
    };

    return (
        <div className="space-y-6">
            {/* Header Section */}
            <div className="bg-white rounded-2xl shadow-sm border border-neutral-100 p-6">
                <div className="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-4">
                    <div className="flex-1">
                        <h2 className="text-3xl font-bold text-neutral-900">Staff Members</h2>
                    </div>

                    {/* Modern Add Button */}
                    <button
                        onClick={onAddItem}
                        className="group flex items-center gap-2 bg-gradient-to-r from-indigo-600 to-purple-600 hover:from-indigo-700 hover:to-purple-700 text-white px-6 py-3 rounded-xl font-semibold shadow-lg hover:shadow-xl transition-all duration-300 transform hover:scale-105"
                    >
                        <svg className="w-5 h-5 transition-transform group-hover:rotate-90 duration-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                        </svg>
                        Add Staff Member
                    </button>
                </div>
            </div>

            {/* Filter Section */}
            <div className="bg-white rounded-2xl shadow-sm border border-neutral-100 p-6">
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                    {/* Search Name */}
                    <div className="relative">
                        <svg className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                        </svg>
                        <input
                            type="text"
                            name="name"
                            placeholder="Search by name..."
                            value={filters.name}
                            autoComplete="off"
                            onChange={(e) => handleFilterChange('name', e.target.value)}
                            className="w-full pl-10 pr-4 py-3 border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition-all"
                        />
                    </div>

                    {/* Search Role */}
                    <input
                        type="text"
                        name="role"
                        placeholder="Search by role..."
                        value={filters.role}
                        autoComplete="off"
                        onChange={(e) => handleFilterChange('role', e.target.value)}
                        className="w-full px-4 py-3 border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition-all"
                    />

                    {/* Status Filter */}
                    <select
                        name="status"
                        value={filters.status}
                        onChange={(e) => handleFilterChange('status', e.target.value)}
                        className="w-full px-4 py-3 border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition-all"
                    >
                        <option value="">Any Status</option>
                        <option value="true">Available</option>
                        <option value="false">Unavailable</option>
                    </select>
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
                            <StaffCard
                                item={item}
                                onEdit={onEditItem}
                                onDelete={handleDelete}
                                onStatusChange={onStatusChange}
                            />
                        </div>
                    ))}
                </div>
            ) : (
                <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 p-12">
                    <div className="text-center">
                        <svg className="w-20 h-20 mx-auto mb-4 text-neutral-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                        </svg>
                        <h3 className="text-xl font-semibold text-neutral-900 mb-2">No Staff Members Found</h3>
                        <p className="text-neutral-600 mb-4">Try adjusting your filters or add your first team member.</p>
                        <button
                            onClick={onAddItem}
                            className="bg-gradient-to-r from-indigo-600 to-purple-600 hover:from-indigo-700 hover:to-purple-700 text-white px-6 py-2.5 rounded-xl font-semibold transition-all"
                        >
                            Add First Staff Member
                        </button>
                    </div>
                </div>
            )}

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