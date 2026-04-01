import React, { useState, useEffect } from 'react';
import { Plus } from 'lucide-react';
import { toast } from 'react-hot-toast';
import AdminLayout from '../../../components/admin/layout/AdminLayout';
import MasterDataGrid from '../../../components/admin/masterdata/MasterDataGrid';
import MasterDataForm from '../../../components/admin/masterdata/MasterDataForm';
import { masterDataApi } from '../../../services/masterDataApi';

const CateringTypeManagement = ({ categoryId, categoryName, icon: Icon }) => {
    const [data, setData] = useState([]);
    const [loading, setLoading] = useState(true);
    const [showForm, setShowForm] = useState(false);
    const [formMode, setFormMode] = useState('create');
    const [selectedItem, setSelectedItem] = useState(null);
    const [formLoading, setFormLoading] = useState(false);

    const [filters, setFilters] = useState({
        searchTerm: '',
        isActive: null,
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

    // Get the appropriate API based on categoryId
    const getApi = () => {
        switch (categoryId) {
            case 1: return masterDataApi.foodTypes;
            case 2: return masterDataApi.cuisineTypes;
            case 3: return masterDataApi.eventTypes;
            case 4: return masterDataApi.serviceTypes;
            default: return null;
        }
    };

    const api = getApi();

    useEffect(() => {
        if (api) {
            fetchData();
        }
    }, [filters, categoryId]);

    const fetchData = async () => {
        if (!api) return;

        setLoading(true);
        try {
            const result = await api.getAll(filters);
            if (result.result && result.data) {
                setData(result.data.items || []);
                setPagination({
                    totalCount: result.data.totalCount || 0,
                    totalPages: result.data.totalPages || 0,
                    pageNumber: result.data.pageNumber || 1,
                    pageSize: result.data.pageSize || 50
                });
            } else {
                toast.error(result.message || `Failed to load ${categoryName.toLowerCase()}`);
            }
        } catch (error) {
            console.error(`Error fetching ${categoryName}:`, error);
            toast.error('Network error. Please try again.');
        } finally {
            setLoading(false);
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
        if (!api) return;

        try {
            if (item.isActive) {
                const usageResult = await api.checkUsage(item.id);
                if (usageResult.success && usageResult.data && !usageResult.data.canDeactivate) {
                    toast.error(`Cannot deactivate: ${usageResult.data.message}`);
                    return;
                }
            }

            const result = await api.updateStatus(item.id, !item.isActive);
            if (result.result) {
                toast.success(`${categoryName.slice(0, -1)} ${!item.isActive ? 'activated' : 'deactivated'} successfully`);
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
        if (!api) return;

        setFormLoading(true);
        try {
            const payload = {
                name: formData.name,
                categoryId: categoryId,
                description: formData.description || null
            };

            let result;
            if (formMode === 'create') {
                result = await api.create(payload);
            } else {
                result = await api.update(selectedItem.id, {
                    id: selectedItem.id,
                    ...payload
                });
            }

            if (result.result) {
                toast.success(`${categoryName.slice(0, -1)} ${formMode === 'create' ? 'created' : 'updated'} successfully`);
                setShowForm(false);
                fetchData();
            } else {
                toast.error(result.message || `Failed to ${formMode} ${categoryName.toLowerCase()}`);
            }
        } catch (error) {
            console.error(`Error ${formMode}ing ${categoryName}:`, error);
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

    const handlePageChange = (pageNumber) => {
        setFilters(prev => ({ ...prev, pageNumber }));
    };

    const columns = [
        { key: 'name', label: 'Type Name' },
        { key: 'description', label: 'Description', type: 'textarea' },
        { key: 'isActive', label: 'Status', type: 'boolean' },
        { key: 'createdDate', label: 'Created Date', type: 'date' },
        { key: 'modifiedDate', label: 'Modified Date', type: 'date' },
    ];

    const formFields = [
        {
            name: 'name',
            label: 'Type Name',
            type: 'text',
            required: true,
            placeholder: `Enter ${categoryName.toLowerCase().slice(0, -1)} name`,
            validate: (value) => {
                if (!value || value.trim().length < 2) {
                    return 'Type name must be at least 2 characters';
                }
                return null;
            }
        },
        {
            name: 'description',
            label: 'Description',
            type: 'textarea',
            required: false,
            placeholder: 'Enter a description for this category',
            validate: (value) => {
                if (value.length > 500) {
                    return 'Category Descripton must be 500 characters or less.';
                }
                return null;
            }
        }
    ];

    return (
        <AdminLayout>
            <nav className="text-xs text-gray-400 mb-4 flex items-center gap-1.5">
                <span>Dashboard</span><span>/</span>
                <span>Master Data</span><span>/</span>
                <span className="text-gray-700 font-medium">{categoryName}</span>
            </nav>

            <div className="mb-6 flex items-center justify-between">
                <div>
                    <h2 className="text-2xl font-bold text-gray-900">{categoryName} Management</h2>
                    <p className="text-gray-600 mt-1">Manage {categoryName.toLowerCase()} for catering services</p>
                </div>
                <button
                    onClick={handleCreate}
                    className="flex items-center gap-2 px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 transition-colors"
                >
                    <Plus className="w-5 h-5" />
                    Add {categoryName.slice(0, -1)}
                </button>
            </div>

            <MasterDataGrid
                title={categoryName}
                icon={Icon}
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
            />

            <MasterDataForm
                isOpen={showForm}
                onClose={() => setShowForm(false)}
                onSubmit={handleFormSubmit}
                mode={formMode}
                title={formMode === 'create' ? `Create New ${categoryName.slice(0, -1)}` : `Edit ${categoryName.slice(0, -1)}`}
                initialData={selectedItem}
                fields={formFields}
                loading={formLoading}
            />
        </AdminLayout>
    );
};

export default CateringTypeManagement;
