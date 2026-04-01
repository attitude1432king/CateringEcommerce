import { useState, useEffect } from 'react';
import { X, Search } from 'lucide-react';
import { partnerApprovalApi, ApprovalStatus, PriorityStatus } from '../../../services/partnerApprovalApi';

/**
 * Partner Filters Component (UPDATED - Enum-based)
 *
 * Advanced filtering panel for partner requests
 * Works with NEW PartnerApprovalController backend
 */
const PartnerFilters = ({ filters, onFilterChange, onClose }) => {
    const [localFilters, setLocalFilters] = useState(filters);
    const [statusOptions, setStatusOptions] = useState([]);
    const [priorityOptions, setPriorityOptions] = useState([]);
    const [loading, setLoading] = useState(true);

    // Fetch enum options from API
    useEffect(() => {
        const fetchEnumOptions = async () => {
            setLoading(true);
            try {
                const [statusesResult, prioritiesResult] = await Promise.all([
                    partnerApprovalApi.getApprovalStatuses(),
                    partnerApprovalApi.getPriorities()
                ]);

                if (statusesResult.success) {
                    setStatusOptions(statusesResult.data || []);
                }

                if (prioritiesResult.success) {
                    setPriorityOptions(prioritiesResult.data || []);
                }
            } catch (error) {
                console.error('Error fetching enum options:', error);
            } finally {
                setLoading(false);
            }
        };

        fetchEnumOptions();
    }, []);

    // Predefined city list (can be fetched from API later)
    const cities = [
        { id: 1401, name: 'Mumbai' },
        { id: 3201, name: 'Delhi' },
        { id: 1101, name: 'Bengaluru' },
        { id: 2401, name: 'Hyderabad' },
        { id: 2301, name: 'Chennai' },
        { id: 2801, name: 'Kolkata' },
        { id: 1402, name: 'Pune' },
        { id: 702, name: 'Ahmedabad' }
    ];

    const handleApply = () => {
        onFilterChange(localFilters);
        onClose();
    };

    const handleReset = () => {
        const resetFilters = {
            approvalStatusId: null,
            priorityId: null,
            cityId: null,
            fromDate: null,
            toDate: null,
            searchTerm: '',
            pageNumber: 1,
            pageSize: 20,
            sortBy: 'c_createddate',
            sortOrder: 'DESC'
        };
        setLocalFilters(resetFilters);
        onFilterChange(resetFilters);
    };

    return (
        <div className="bg-white border border-gray-200 rounded-lg p-6">
            <div className="flex items-center justify-between mb-6">
                <h3 className="text-lg font-semibold text-gray-900">Advanced Filters</h3>
                <button
                    onClick={onClose}
                    className="p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-lg transition-colors"
                >
                    <X className="w-5 h-5" />
                </button>
            </div>

            {loading ? (
                <div className="flex items-center justify-center py-8">
                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
                </div>
            ) : (
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                    {/* Status Filter (Enum-based) */}
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                            Approval Status
                        </label>
                        <select
                            value={localFilters.approvalStatusId || ''}
                            onChange={(e) => setLocalFilters({
                                ...localFilters,
                                approvalStatusId: e.target.value ? parseInt(e.target.value) : null
                            })}
                            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        >
                            <option value="">All Statuses</option>
                            {statusOptions.map(status => (
                                <option key={status.id} value={status.id}>
                                    {status.name}
                                </option>
                            ))}
                        </select>
                    </div>

                    {/* Priority Filter (Enum-based) */}
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                            Priority
                        </label>
                        <select
                            value={localFilters.priorityId || ''}
                            onChange={(e) => setLocalFilters({
                                ...localFilters,
                                priorityId: e.target.value ? parseInt(e.target.value) : null
                            })}
                            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        >
                            <option value="">All Priorities</option>
                            {priorityOptions.map(priority => (
                                <option key={priority.id} value={priority.id}>
                                    {priority.name}
                                </option>
                            ))}
                        </select>
                    </div>

                    {/* City Filter */}
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                            City
                        </label>
                        <select
                            value={localFilters.cityId || ''}
                            onChange={(e) => setLocalFilters({
                                ...localFilters,
                                cityId: e.target.value ? parseInt(e.target.value) : null
                            })}
                            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        >
                            <option value="">All Cities</option>
                            {cities.map(city => (
                                <option key={city.id} value={city.id}>
                                    {city.name}
                                </option>
                            ))}
                        </select>
                    </div>

                    {/* From Date */}
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                            From Date
                        </label>
                        <input
                            type="date"
                            value={localFilters.fromDate || ''}
                            onChange={(e) => setLocalFilters({ ...localFilters, fromDate: e.target.value || null })}
                            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        />
                    </div>

                    {/* To Date */}
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                            To Date
                        </label>
                        <input
                            type="date"
                            value={localFilters.toDate || ''}
                            onChange={(e) => setLocalFilters({ ...localFilters, toDate: e.target.value || null })}
                            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        />
                    </div>

                    {/* Sort By */}
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                            Sort By
                        </label>
                        <select
                            value={localFilters.sortBy || 'c_createddate'}
                            onChange={(e) => setLocalFilters({ ...localFilters, sortBy: e.target.value })}
                            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        >
                            <option value="c_createddate">Registration Date</option>
                            <option value="co.c_catering_name">Business Name</option>
                            <option value="co.c_owner_name">Owner Name</option>
                            <option value="c.c_cityname">City</option>
                            <option value="co.c_priority">Priority</option>
                        </select>
                    </div>
                </div>
            )}

            {/* Action Buttons */}
            <div className="flex items-center justify-end space-x-3 mt-6 pt-6 border-t border-gray-200">
                <button
                    onClick={handleReset}
                    className="px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-100 transition-colors"
                >
                    Reset All
                </button>
                <button
                    onClick={handleApply}
                    disabled={loading}
                    className="px-6 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors disabled:opacity-50"
                >
                    Apply Filters
                </button>
            </div>
        </div>
    );
};

export default PartnerFilters;
