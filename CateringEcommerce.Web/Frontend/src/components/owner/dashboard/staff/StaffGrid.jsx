/*
========================================
File: src/components/owner/dashboard/staff/StaffGrid.jsx (NEW FILE)
========================================
Handles the filtering, pagination, and display of staff members in a table/grid.
*/
import React from 'react';
import Pagination from '../../../common/Pagination';
import { useConfirmation } from '../../../../contexts/ConfirmationContext';
import ToggleSwitch from '../../../common/ToggleSwitch';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

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
        <div className="bg-white p-6 rounded-xl shadow-sm">
            <div className="flex flex-col md:flex-row justify-between items-center mb-6">
                <h3 className="text-xl font-bold text-neutral-800">Your Staff Members</h3>
                <button onClick={onAddItem} className="bg-blue-600 text-white px-4 py-2 rounded-lg font-semibold hover:bg-blue-700 flex items-center gap-2 w-full md:w-auto mt-4 md:mt-0">
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor"><path fillRule="evenodd" d="M10 3a1 1 0 011 1v5h5a1 1 0 110 2h-5v5a1 1 0 11-2 0v-5H4a1 1 0 110-2h5V4a1 1 0 011-1z" clipRule="evenodd" /></svg>
                    Add New Staff
                </button>
            </div>

            {/* Filters */}
            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4 mb-6 pb-6 border-b">
                <input
                    type="text"
                    name="name"
                    placeholder="Search by name..."
                    value={filters.name}
                    autoComplete="off"
                    onChange={(e) => handleFilterChange('name', e.target.value)}
                    className="w-full px-3 py-2 border border-neutral-300 rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-rose-500 focus:border-rose-500"
                />
                <input
                    type="text"
                    name="role"
                    placeholder="Search by role..."
                    value={filters.role}
                    autoComplete="off"
                    onChange={(e) => handleFilterChange('role', e.target.value)}
                    className="w-full px-3 py-2 border border-neutral-300 rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-rose-500 focus:border-rose-500"
                />
                <select
                    name="status"
                    value={filters.status}
                    onChange={(e) => handleFilterChange('status', e.target.value)}
                    className="w-full px-3 py-2 border border-neutral-300 rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-rose-500 focus:border-rose-500"
                >
                    <option value="">Any Status</option>
                    <option value="true">Available</option>
                    <option value="false">Unavailable</option>
                </select>
            </div>

            {/* Staff Table */}
            <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-neutral-200">
                    <thead className="bg-neutral-50">
                        <tr>
                            <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-neutral-500 uppercase tracking-wider">Name</th>
                            <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-neutral-500 uppercase tracking-wider">Role/Post</th>
                            <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-neutral-500 uppercase tracking-wider">Expertise</th>
                            <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-neutral-500 uppercase tracking-wider">Salary</th>
                            <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-neutral-500 uppercase tracking-wider">Availability</th>
                            <th scope="col" className="relative px-6 py-3"><span className="sr-only">Actions</span></th>
                        </tr>
                    </thead>
                    <tbody className="bg-white divide-y divide-neutral-200">
                        {items.map(item => (
                            <tr key={item.id}>
                                <td className="px-6 py-4 whitespace-nowrap">
                                    <div className="flex items-center">
                                        <div className="flex-shrink-0 h-10 w-10">
                                            <img className="h-10 w-10 rounded-full object-cover" src={item.photo && item.photo[0] ? (item.photo[0].path ? `${API_BASE_URL}${item.photo[0].path}` : item.photo[0].preview) : `https://ui-avatars.com/api/?name=${item.name.replace(' ', '+')}&background=e0e7ff&color=4338ca`} alt="" />
                                        </div>
                                        <div className="ml-4">
                                            <div className="text-sm font-medium text-neutral-900">{item.name}</div>
                                        </div>
                                    </div>
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap text-sm text-neutral-700">
                                    {item.otherRole
                                        ? `${item.role}(${item.otherRole})`
                                        : item.role}
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap text-sm text-neutral-700">{item.expertise}</td>
                                <td className="px-6 py-4 whitespace-nowrap text-sm text-neutral-700">
                                    ₹{item.salaryAmount} <span className="text-xs text-neutral-500">/{item.salaryType === 'Monthly' ? 'mo' : 'day'}</span>
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap">
                                    <ToggleSwitch enabled={item.availability} setEnabled={(value) => onStatusChange(item, value)} label="" />
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                    <div className="flex items-center justify-end gap-2">

                                        {/* Edit Button */}
                                        <button
                                            onClick={() => onEditItem(item)}
                                            title="Edit"
                                            className="p-2 rounded-md bg-gray-100 text-gray-700 hover:bg-blue-100 hover:text-blue-600 transition border border-gray-200"
                                        >
                                            <svg
                                                xmlns="http://www.w3.org/2000/svg"
                                                className="h-5 w-5"
                                                fill="none"
                                                viewBox="0 0 24 24"
                                                stroke="currentColor"
                                                strokeWidth={1.8}
                                            >
                                                <path
                                                    strokeLinecap="round"
                                                    strokeLinejoin="round"
                                                    d="M11 5h2m7.207 1.793l-2-2a1 1 0 00-1.414 0l-10 10-1 4 4-1 10-10a1 1 0 000-1.414z"
                                                />
                                            </svg>
                                        </button>

                                        {/* Delete Button */}
                                        <button
                                            onClick={() => handleDelete(item)}
                                            title="Delete"
                                            className="p-2 rounded-md bg-gray-100 text-gray-700 hover:bg-red-100 hover:text-red-600 transition border border-gray-200"
                                        >
                                            <svg
                                                xmlns="http://www.w3.org/2000/svg"
                                                className="h-5 w-5"
                                                fill="none"
                                                viewBox="0 0 24 24"
                                                stroke="currentColor"
                                                strokeWidth={1.8}
                                            >
                                                <path
                                                    strokeLinecap="round"
                                                    strokeLinejoin="round"
                                                    d="M6 7h12M9 7v10m6-10v10M4 7h16l-1 13H5L4 7z"
                                                />
                                            </svg>
                                        </button>

                                    </div>
                                </td>



                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>

            {items.length === 0 && (
                <div className="text-center py-16">
                    <h3 className="text-xl font-semibold text-neutral-700">No Staff Members Found</h3>
                    <p className="text-neutral-500 mt-2">Try adjusting your filters or adding a new staff member.</p>
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