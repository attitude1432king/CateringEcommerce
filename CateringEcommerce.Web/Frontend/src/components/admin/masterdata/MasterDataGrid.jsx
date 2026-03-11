import React from 'react';
import { Search, Filter, Edit, Power, ChevronLeft, ChevronRight, Loader2 } from 'lucide-react';

const MasterDataGrid = ({
    title,
    icon: Icon,
    data = [],
    loading = false,
    pagination = { pageNumber: 1, pageSize: 20, totalCount: 0, totalPages: 0 },
    searchTerm = '',
    activeFilter = null,
    onSearch,
    onFilterChange,
    onPageChange,
    onEdit,
    onToggleStatus,
    columns = [],
    extraFilters = null,
    showActions = true,
}) => {
    const handleSearchChange = (e) => {
        const value = e.target.value;
        if (onSearch) {
            onSearch(value);
        }
    };

    const handleFilterChange = (value) => {
        if (onFilterChange) {
            onFilterChange(value === 'all' ? null : value === 'active');
        }
    };

    const handlePrevPage = () => {
        if (pagination.pageNumber > 1 && onPageChange) {
            onPageChange(pagination.pageNumber - 1);
        }
    };

    const handleNextPage = () => {
        if (pagination.pageNumber < pagination.totalPages && onPageChange) {
            onPageChange(pagination.pageNumber + 1);
        }
    };

    const renderCellValue = (row, column) => {
        const value = row[column.key];

        if (column.render) {
            return column.render(row);
        }

        if (column.type === 'boolean') {
            return (
                <span className={`px-2 py-1 text-xs font-medium rounded-full ${value ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-700'}`}>
                    {value ? 'Active' : 'Inactive'}
                </span>
            );
        }

        if (column.type === 'date') {
            return value ? new Date(value).toLocaleDateString() : '-';
        }

        if (column.type === 'number') {
            return value || 0;
        }

        return value || '-';
    };

    return (
        <div className="bg-white rounded-lg shadow">
            {/* Header */}
            <div className="p-6 border-b">
                <div className="flex items-center gap-3 mb-4">
                    {Icon && <Icon className="w-6 h-6 text-purple-600" />}
                    <h2 className="text-xl font-bold text-gray-800">{title}</h2>
                </div>

                {/* Search and Filters */}
                <div className="flex flex-col md:flex-row gap-4">
                    {/* Search Bar */}
                    <div className="flex-1 relative">
                        <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
                        <input
                            type="text"
                            placeholder="Search..."
                            value={searchTerm}
                            onChange={handleSearchChange}
                            className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                        />
                    </div>

                    {/* Status Filter */}
                    <div className="flex items-center gap-2">
                        <Filter className="w-5 h-5 text-gray-400" />
                        <select
                            value={activeFilter === null ? 'all' : activeFilter ? 'active' : 'inactive'}
                            onChange={(e) => handleFilterChange(e.target.value)}
                            className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                        >
                            <option value="all">All Status</option>
                            <option value="active">Active Only</option>
                            <option value="inactive">Inactive Only</option>
                        </select>
                    </div>

                    {/* Extra Filters */}
                    {extraFilters && <div className="flex items-center gap-2">{extraFilters}</div>}
                </div>
            </div>

            {/* Table */}
            <div className="overflow-x-auto">
                <table className="w-full">
                    <thead className="bg-gray-50 border-b">
                        <tr>
                            {columns.map((column) => (
                                <th
                                    key={column.key}
                                    className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                                >
                                    {column.label}
                                </th>
                            ))}
                            {showActions && (
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Actions
                                </th>
                            )}
                        </tr>
                    </thead>
                    <tbody className="bg-white divide-y divide-gray-200">
                        {loading ? (
                            <tr>
                                <td colSpan={columns.length + (showActions ? 1 : 0)} className="px-6 py-12">
                                    <div className="flex flex-col items-center justify-center text-gray-400">
                                        <Loader2 className="w-8 h-8 animate-spin mb-2" />
                                        <p>Loading data...</p>
                                    </div>
                                </td>
                            </tr>
                        ) : data.length === 0 ? (
                            <tr>
                                <td colSpan={columns.length + (showActions ? 1 : 0)} className="px-6 py-12">
                                    <div className="flex flex-col items-center justify-center text-gray-400">
                                        <p className="text-lg font-medium">No data found</p>
                                        <p className="text-sm mt-1">Try adjusting your search or filters</p>
                                    </div>
                                </td>
                            </tr>
                        ) : (
                            data.map((row, index) => (
                                <tr key={row.id || index} className="hover:bg-gray-50 transition-colors">
                                    {columns.map((column) => (
                                        <td key={column.key} className="px-6 py-4 text-sm text-gray-900 whitespace-nowrap">
                                            {renderCellValue(row, column)}
                                        </td>
                                    ))}
                                    {showActions && (
                                        <td className="px-6 py-4 text-sm whitespace-nowrap">
                                            <div className="flex items-center gap-2">
                                                {onEdit && (
                                                    <button
                                                        onClick={() => onEdit(row)}
                                                        className="inline-flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
                                                        title="Edit"
                                                    >
                                                        <Edit className="w-3.5 h-3.5" />
                                                        <span>Edit</span>
                                                    </button>
                                                )}
                                                {onToggleStatus && (
                                                    <button
                                                        onClick={() => onToggleStatus(row)}
                                                        className={`inline-flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium rounded-lg transition-colors ${row.isActive
                                                                ? 'text-white bg-red-600 hover:bg-red-700'
                                                                : 'text-white bg-green-600 hover:bg-green-700'
                                                            }`}
                                                        title={row.isActive ? 'Deactivate' : 'Activate'}
                                                    >
                                                        <Power className="w-3.5 h-3.5" />
                                                        <span>{row.isActive ? 'Deactivate' : 'Activate'}</span>
                                                    </button>
                                                )}
                                            </div>
                                        </td>
                                    )}
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </div>

            {/* Pagination */}
            {!loading && data.length > 0 && (
                <div className="px-6 py-4 border-t flex items-center justify-between">
                    <div className="text-sm text-gray-600">
                        Showing {((pagination.pageNumber - 1) * pagination.pageSize) + 1} to{' '}
                        {Math.min(pagination.pageNumber * pagination.pageSize, pagination.totalCount)} of{' '}
                        {pagination.totalCount} results
                    </div>
                    <div className="flex items-center gap-2">
                        <button
                            onClick={handlePrevPage}
                            disabled={pagination.pageNumber === 1}
                            className="px-3 py-2 border rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50 transition-colors"
                        >
                            <ChevronLeft className="w-4 h-4" />
                        </button>
                        <span className="px-4 py-2 text-sm text-gray-600">
                            Page {pagination.pageNumber} of {pagination.totalPages}
                        </span>
                        <button
                            onClick={handleNextPage}
                            disabled={pagination.pageNumber >= pagination.totalPages}
                            className="px-3 py-2 border rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50 transition-colors"
                        >
                            <ChevronRight className="w-4 h-4" />
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
};

export default MasterDataGrid;
