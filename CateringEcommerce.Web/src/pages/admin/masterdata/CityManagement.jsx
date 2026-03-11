import React, { useState, useEffect } from 'react';
import { Plus, MapPin } from 'lucide-react';
import { toast } from 'react-hot-toast';
import MasterDataGrid from '../../../components/admin/masterdata/MasterDataGrid';
import MasterDataForm from '../../../components/admin/masterdata/MasterDataForm';
import { masterDataApi } from '../../../services/masterDataApi';

const CityManagement = () => {
    const [data, setData] = useState([]);
    const [states, setStates] = useState([]);
    const [loading, setLoading] = useState(true);
    const [showForm, setShowForm] = useState(false);
    const [formMode, setFormMode] = useState('create');
    const [selectedItem, setSelectedItem] = useState(null);
    const [formLoading, setFormLoading] = useState(false);

    const [filters, setFilters] = useState({
        searchTerm: '',
        isActive: null,
        stateId: null,
        pageNumber: 1,
        pageSize: 20,
        sortBy: 'DisplayOrder',
        sortOrder: 'ASC'
    });

    const [pagination, setPagination] = useState({
        totalCount: 0,
        totalPages: 0,
        pageNumber: 1,
        pageSize: 20
    });

    useEffect(() => {
        fetchData();
    }, [filters]);

    useEffect(() => {
        fetchStates();
    }, []);

    const fetchData = async () => {
        setLoading(true);
        try {
            const result = await masterDataApi.cities.getAll(filters);
            if (result.result && result.data) {
                setData(result.data.items || []);
                setPagination({
                    totalCount: result.data.totalCount || 0,
                    totalPages: result.data.totalPages || 0,
                    pageNumber: result.data.pageNumber || 1,
                    pageSize: result.data.pageSize || 50
                });
            } else {
                toast.error(result.message || 'Failed to load cities');
            }
        } catch (error) {
            console.error('Error fetching cities:', error);
            toast.error('Network error. Please try again.');
        } finally {
            setLoading(false);
        }
    };

    const fetchStates = async () => {
        try {
            const result = await masterDataApi.getStates();
            if (result.result && result.data) {
                setStates(result.data);
            }
        } catch (error) {
            console.error('Error fetching states:', error);
        }
    };

    const handleCreate = () => {
        setFormMode('create');
        setSelectedItem(null);
        setShowForm(true);
    };

    const handleEdit = (item) => {
        setFormMode('edit');
        setSelectedItem(item);
        setShowForm(true);
    };

    const handleToggleStatus = async (item) => {
        try {
            // Check usage before deactivating
            if (item.isActive) {
                const usageResult = await masterDataApi.cities.checkUsage(item.id);
                if (usageResult.success && usageResult.data && !usageResult.data.canDeactivate) {
                    toast.error(`Cannot deactivate: ${usageResult.data.message}`);
                    return;
                }
            }

            const result = await masterDataApi.cities.updateStatus(item.id, !item.isActive);
            if (result.result) {
                toast.success(`City ${!item.isActive ? 'activated' : 'deactivated'} successfully`);
                fetchData();
            } else {
                toast.error(result.message || 'Failed to update status');
            }
        } catch (error) {
            console.error('Error updating status:', error);
            toast.error('Network error. Please try again.');
        }
    };

    const handleFormSubmit = async (formData) => {
        setFormLoading(true);
        try {
            const payload = {
                name: formData.name,
                stateId: parseInt(formData.stateId),
            };

            let result;
            if (formMode === 'create') {
                result = await masterDataApi.cities.create(payload);
            } else {
                result = await masterDataApi.cities.update(selectedItem.id, {
                    id: selectedItem.id,
                    ...payload
                });
            }

            if (result.result) {
                toast.success(`City ${formMode === 'create' ? 'created' : 'updated'} successfully`);
                setShowForm(false);
                fetchData();
            } else {
                toast.error(result.message || `Failed to ${formMode} city`);
            }
        } catch (error) {
            console.error(`Error ${formMode}ing city:`, error);
            toast.error('Network error. Please try again.');
        } finally {
            setFormLoading(false);
        }
    };

    const handleSearch = (searchTerm) => {
        setFilters(prev => ({ ...prev, searchTerm, pageNumber: 1 }));
    };

    const handleFilterChange = (isActive) => {
        setFilters(prev => ({ ...prev, isActive, pageNumber: 1 }));
    };

    const handleStateFilter = (e) => {
        const stateId = e.target.value ? parseInt(e.target.value) : null;
        setFilters(prev => ({ ...prev, stateId, pageNumber: 1 }));
    };

    const handlePageChange = (pageNumber) => {
        setFilters(prev => ({ ...prev, pageNumber }));
    };

    const columns = [
        { key: 'name', label: 'City Name' },
        { key: 'stateName', label: 'State' },
        { key: 'isActive', label: 'Status', type: 'boolean' },
        { key: 'createdDate', label: 'Created Date', type: 'date' },
        { key: 'modifiedDate', label: 'Modified Date', type: 'date' },
    ];

    const formFields = [
        {
            name: 'name',
            label: 'City Name',
            type: 'text',
            required: true,
            placeholder: 'Enter city name',
            validate: (value) => {
                if (!value || value.trim().length < 2) {
                    return 'City name must be at least 2 characters';
                }
                return null;
            }
        },
        {
            name: 'stateId',
            label: 'State',
            type: 'select',
            required: true,
            options: states.map(state => ({
                value: state.stateId,
                label: state.stateName
            }))
        }
    ];

    const extraFilters = (
        <select
            value={filters.stateId || ''}
            onChange={handleStateFilter}
            className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
        >
            <option value="">All States</option>
            {states.map(state => (
                <option key={state.stateId} value={state.stateId}>
                    {state.stateName}
                </option>
            ))}
        </select>
    );

    return (
        <div>
            {/* Header with Create Button */}
            <div className="mb-6 flex items-center justify-between">
                <div>
                    <h2 className="text-2xl font-bold text-gray-900">City Management</h2>
                    <p className="text-gray-600 mt-1">Manage cities and their state associations</p>
                </div>
                <button
                    onClick={handleCreate}
                    className="flex items-center gap-2 px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 transition-colors"
                >
                    <Plus className="w-5 h-5" />
                    Add City
                </button>
            </div>

            {/* Grid */}
            <MasterDataGrid
                title="Cities"
                icon={MapPin}
                data={data}
                loading={loading}
                pagination={pagination}
                searchTerm={filters.searchTerm}
                activeFilter={filters.isActive}
                onSearch={handleSearch}
                onFilterChange={handleFilterChange}
                onPageChange={handlePageChange}
                onEdit={handleEdit}
                onToggleStatus={handleToggleStatus}
                columns={columns}
                extraFilters={extraFilters}
            />

            {/* Form Modal */}
            <MasterDataForm
                isOpen={showForm}
                onClose={() => setShowForm(false)}
                onSubmit={handleFormSubmit}
                mode={formMode}
                title={formMode === 'create' ? 'Create New City' : 'Edit City'}
                initialData={selectedItem}
                fields={formFields}
                loading={formLoading}
            />
        </div>
    );
};

export default CityManagement;
