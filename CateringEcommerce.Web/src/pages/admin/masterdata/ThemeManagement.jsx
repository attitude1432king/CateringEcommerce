import React, { useState, useEffect } from 'react';
import { Plus, Palette } from 'lucide-react';
import { toast } from 'react-hot-toast';
import AdminLayout from '../../../components/admin/layout/AdminLayout';
import MasterDataGrid from '../../../components/admin/masterdata/MasterDataGrid';
import MasterDataForm from '../../../components/admin/masterdata/MasterDataForm';
import { masterDataApi } from '../../../services/masterDataApi';

const ThemeManagement = () => {
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

    useEffect(() => {
        fetchData();
    }, [filters]);

    const fetchData = async () => {
        setLoading(true);
        try {
            const result = await masterDataApi.themes.getAll(filters);
            if (result.result && result.data) {
                setData(result.data.items || []);
                setPagination({
                    totalCount: result.data.totalCount || 0,
                    totalPages: result.data.totalPages || 0,
                    pageNumber: result.data.pageNumber || 1,
                    pageSize: result.data.pageSize || 50
                });
            } else {
                toast.error(result.message || 'Failed to load themes');
            }
        } catch (error) {
            console.error('Error fetching themes:', error);
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
        try {
            if (item.isActive) {
                const usageResult = await masterDataApi.themes.checkUsage(item.id);
                if (usageResult.success && usageResult.data && !usageResult.data.canDeactivate) {
                    toast.error(`Cannot deactivate: ${usageResult.data.message}`);
                    return;
                }
            }

            const result = await masterDataApi.themes.updateStatus(item.id, !item.isActive);
            if (result.result) {
                toast.success(`Theme ${!item.isActive ? 'activated' : 'deactivated'} successfully`);
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
                displayOrder: parseInt(formData.displayOrder) || 0
            };

            let result;
            if (formMode === 'create') {
                result = await masterDataApi.themes.create(payload);
            } else {
                result = await masterDataApi.themes.update(selectedItem.id, {
                    id: selectedItem.id,
                    ...payload
                });
            }

            if (result.result) {
                toast.success(`Theme ${formMode === 'create' ? 'created' : 'updated'} successfully`);
                setShowForm(false);
                fetchData();
            } else {
                toast.error(result.message || `Failed to ${formMode} theme`);
            }
        } catch (error) {
            console.error(`Error ${formMode}ing theme:`, error);
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
        { key: 'displayOrder', label: 'Order', type: 'number' },
        { key: 'name', label: 'Theme Name' },
        { key: 'isActive', label: 'Status', type: 'boolean' },
        { key: 'createdDate', label: 'Created Date', type: 'date' },
        { key: 'modifiedDate', label: 'Modified Date', type: 'date' },
    ];

    const formFields = [
        {
            name: 'name',
            label: 'Theme Name',
            type: 'text',
            required: true,
            placeholder: 'e.g., Traditional, Modern, Vintage, Rustic',
            validate: (value) => {
                if (!value || value.trim().length < 2) {
                    return 'Theme name must be at least 2 characters';
                }
                return null;
            }
        },
        {
            name: 'displayOrder',
            label: 'Display Order',
            type: 'number',
            required: true,
            defaultValue: 0,
            hint: 'Lower numbers appear first'
        }
    ];

    return (
        <AdminLayout>
            <nav className="text-xs text-gray-400 mb-4 flex items-center gap-1.5">
                <span>Dashboard</span><span>/</span>
                <span>Master Data</span><span>/</span>
                <span className="text-gray-700 font-medium">Themes</span>
            </nav>

            <div className="mb-6 flex items-center justify-between">
                <div>
                    <h2 className="text-2xl font-bold text-gray-900">Theme Management</h2>
                    <p className="text-gray-600 mt-1">Manage decoration and event themes</p>
                </div>
                <button
                    onClick={handleCreate}
                    className="flex items-center gap-2 px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 transition-colors"
                >
                    <Plus className="w-5 h-5" />
                    Add Theme
                </button>
            </div>

            <MasterDataGrid
                title="Themes"
                icon={Palette}
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
                title={formMode === 'create' ? 'Create New Theme' : 'Edit Theme'}
                initialData={selectedItem}
                fields={formFields}
                loading={formLoading}
            />
        </AdminLayout>
    );
};

export default ThemeManagement;
