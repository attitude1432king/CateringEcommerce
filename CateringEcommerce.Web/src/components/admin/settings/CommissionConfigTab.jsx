import { useState, useEffect } from 'react';
import { Plus, Edit2, Trash2, Filter, RefreshCw } from 'lucide-react';
import CommissionConfigForm from './CommissionConfigForm';
import { commissionConfigApi } from '../../../services/settingsApi';
import { toast } from 'react-hot-toast';

/**
 * Commission Config Tab Component
 * Manages commission configurations
 */
const CommissionConfigTab = () => {
    const [configs, setConfigs] = useState([]);
    const [loading, setLoading] = useState(false);
    const [selectedConfig, setSelectedConfig] = useState(null);
    const [showForm, setShowForm] = useState(false);
    const [formMode, setFormMode] = useState('create');
    const [saving, setSaving] = useState(false);

    // Filters
    const [filters, setFilters] = useState({
        configType: '',
        isActive: null,
        pageNumber: 1,
        pageSize: 50,
        sortBy: 'CreatedDate',
        sortOrder: 'DESC',
    });

    useEffect(() => {
        fetchConfigs();
    }, [filters]);

    const fetchConfigs = async () => {
        setLoading(true);
        try {
            const apiFilters = {
                ...filters,
                configType: filters.configType === '' ? null : filters.configType,
            };

            const response = await commissionConfigApi.getCommissionConfigs(apiFilters);

            if (response.result) {
                setConfigs(response.data.configs || []);
            } else {
                toast.error('Failed to load commission configurations');
            }
        } catch (error) {
            console.error('Error fetching configs:', error);
            toast.error('Failed to load commission configurations');
        } finally {
            setLoading(false);
        }
    };

    const handleCreate = () => {
        setSelectedConfig(null);
        setFormMode('create');
        setShowForm(true);
    };

    const handleEdit = (config) => {
        setSelectedConfig(config);
        setFormMode('edit');
        setShowForm(true);
    };

    const handleSave = async (configId, data) => {
        setSaving(true);
        try {
            let response;
            if (formMode === 'create') {
                response = await commissionConfigApi.createCommissionConfig(data);
            } else {
                response = await commissionConfigApi.updateCommissionConfig(configId, data);
            }

            if (response.result) {
                toast.success(
                    formMode === 'create'
                        ? 'Commission config created successfully'
                        : 'Commission config updated successfully'
                );
                setShowForm(false);
                fetchConfigs();
            } else {
                toast.error(response.message || 'Failed to save commission config');
            }
        } catch (error) {
            console.error('Error saving config:', error);
            toast.error('Failed to save commission config');
        } finally {
            setSaving(false);
        }
    };

    const handleDelete = async (configId) => {
        if (!confirm('Are you sure you want to delete this commission configuration?')) {
            return;
        }

        try {
            const response = await commissionConfigApi.deleteCommissionConfig(configId);

            if (response.result) {
                toast.success('Commission config deleted successfully');
                fetchConfigs();
            } else {
                toast.error(response.message || 'Failed to delete commission config');
            }
        } catch (error) {
            console.error('Error deleting config:', error);
            toast.error('Failed to delete commission config');
        }
    };

    const handleTypeFilter = (type) => {
        setFilters((prev) => ({ ...prev, configType: type, pageNumber: 1 }));
    };

    const handleRefresh = () => {
        fetchConfigs();
        toast.success('Commission configs refreshed');
    };

    const formatDate = (dateString) => {
        const date = new Date(dateString);
        return date.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
    };

    const formatCurrency = (value) => {
        return new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: 'USD',
        }).format(value);
    };

    if (loading) {
        return (
            <div className="flex items-center justify-center h-64">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
            </div>
        );
    }

    return (
        <div className="p-6">
            {/* Header Actions */}
            <div className="mb-6">
                <div className="flex items-center justify-between gap-4">
                    {/* Type Filters */}
                    <div className="flex items-center gap-2">
                        <Filter className="w-4 h-4 text-gray-600" />
                        <span className="text-sm font-medium text-gray-700">Type:</span>
                        <div className="flex gap-2">
                            {['All', 'GLOBAL', 'CATERING_SPECIFIC', 'TIERED'].map((type) => (
                                <button
                                    key={type}
                                    onClick={() => handleTypeFilter(type === 'All' ? '' : type)}
                                    className={`px-3 py-1 rounded-full text-sm font-medium transition-colors ${
                                        (type === 'All' && filters.configType === '') ||
                                        filters.configType === type
                                            ? 'bg-blue-600 text-white'
                                            : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                                    }`}
                                >
                                    {type === 'All' ? 'All' : type.replace('_', ' ')}
                                </button>
                            ))}
                        </div>
                    </div>

                    {/* Actions */}
                    <div className="flex items-center gap-3">
                        <button
                            onClick={handleRefresh}
                            className="px-4 py-2 bg-white border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors flex items-center gap-2"
                        >
                            <RefreshCw className="w-4 h-4" />
                            <span>Refresh</span>
                        </button>
                        <button
                            onClick={handleCreate}
                            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors flex items-center gap-2"
                        >
                            <Plus className="w-4 h-4" />
                            <span>Create Config</span>
                        </button>
                    </div>
                </div>
            </div>

            {/* Configs Table */}
            {configs.length === 0 ? (
                <div className="text-center py-12 bg-gray-50 rounded-lg">
                    <p className="text-gray-500">No commission configurations found</p>
                    <button
                        onClick={handleCreate}
                        className="mt-4 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
                    >
                        Create First Config
                    </button>
                </div>
            ) : (
                <div className="overflow-x-auto">
                    <table className="min-w-full divide-y divide-gray-200">
                        <thead className="bg-gray-50">
                            <tr>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Config Name
                                </th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Type
                                </th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Commission
                                </th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Fixed Fee
                                </th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Order Range
                                </th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Effective Period
                                </th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Status
                                </th>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Actions
                                </th>
                            </tr>
                        </thead>
                        <tbody className="bg-white divide-y divide-gray-200">
                            {configs.map((config) => (
                                <tr key={config.configId} className="hover:bg-gray-50">
                                    <td className="px-6 py-4 whitespace-nowrap">
                                        <div className="flex flex-col">
                                            <span className="text-sm font-medium text-gray-900">
                                                {config.configName}
                                            </span>
                                            {config.businessName && (
                                                <span className="text-xs text-gray-500">
                                                    {config.businessName}
                                                </span>
                                            )}
                                        </div>
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap">
                                        <span className="px-2 py-1 text-xs font-medium bg-gray-100 text-gray-800 rounded">
                                            {config.configType.replace('_', ' ')}
                                        </span>
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap">
                                        <span className="text-sm text-gray-900 font-medium">
                                            {config.commissionRate}%
                                        </span>
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap">
                                        <span className="text-sm text-gray-900">
                                            {formatCurrency(config.fixedFee)}
                                        </span>
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap">
                                        {config.minOrderValue || config.maxOrderValue ? (
                                            <span className="text-sm text-gray-700">
                                                {config.minOrderValue ? formatCurrency(config.minOrderValue) : '-'}
                                                {' - '}
                                                {config.maxOrderValue ? formatCurrency(config.maxOrderValue) : '∞'}
                                            </span>
                                        ) : (
                                            <span className="text-sm text-gray-400">-</span>
                                        )}
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap">
                                        <div className="text-sm text-gray-700">
                                            <div>{formatDate(config.effectiveFrom)}</div>
                                            <div className="text-xs text-gray-500">
                                                to {config.effectiveTo ? formatDate(config.effectiveTo) : 'Ongoing'}
                                            </div>
                                        </div>
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap">
                                        <span
                                            className={`px-2 py-1 text-xs font-medium rounded ${
                                                config.isActive
                                                    ? 'bg-green-100 text-green-800'
                                                    : 'bg-gray-100 text-gray-800'
                                            }`}
                                        >
                                            {config.isActive ? 'Active' : 'Inactive'}
                                        </span>
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                        <div className="flex items-center justify-end gap-2">
                                            <button
                                                onClick={() => handleEdit(config)}
                                                className="text-blue-600 hover:text-blue-900"
                                                title="Edit Config"
                                            >
                                                <Edit2 className="w-4 h-4" />
                                            </button>
                                            <button
                                                onClick={() => handleDelete(config.configId)}
                                                className="text-red-600 hover:text-red-900"
                                                title="Delete Config"
                                            >
                                                <Trash2 className="w-4 h-4" />
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}

            {/* Form Modal */}
            <CommissionConfigForm
                config={selectedConfig}
                isOpen={showForm}
                onClose={() => setShowForm(false)}
                onSave={formMode === 'create' ? handleSave : (_, data) => handleSave(selectedConfig.configId, data)}
                saving={saving}
                mode={formMode}
            />
        </div>
    );
};

export default CommissionConfigTab;
