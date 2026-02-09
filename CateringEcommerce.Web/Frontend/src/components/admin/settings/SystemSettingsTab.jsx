import { useState, useEffect } from 'react';
import { Search, Filter, RefreshCw } from 'lucide-react';
import SettingsTable from './SettingsTable';
import SettingEditModal from './SettingEditModal';
import SettingHistoryDrawer from './SettingHistoryDrawer';
import { systemSettingsApi } from '../../../services/settingsApi';
import { toast } from 'react-hot-toast';

/**
 * System Settings Tab Component
 * Displays and manages system settings by category
 */
const SystemSettingsTab = () => {
    const [settings, setSettings] = useState([]);
    const [loading, setLoading] = useState(false);
    const [selectedSetting, setSelectedSetting] = useState(null);
    const [showEditModal, setShowEditModal] = useState(false);
    const [showHistoryDrawer, setShowHistoryDrawer] = useState(false);
    const [saving, setSaving] = useState(false);

    // Filters
    const [filters, setFilters] = useState({
        category: '',
        searchTerm: '',
        isActive: null,
        pageNumber: 1,
        pageSize: 50,
        sortBy: 'DisplayOrder',
        sortOrder: 'ASC',
    });

    const [categories, setCategories] = useState([
        'All',
        'SYSTEM',
        'EMAIL',
        'PAYMENT',
        'BUSINESS',
        'NOTIFICATION',
    ]);

    useEffect(() => {
        fetchSettings();
    }, [filters]);

    const fetchSettings = async () => {
        setLoading(true);
        try {
            const apiFilters = {
                ...filters,
                category: filters.category === 'All' || filters.category === '' ? null : filters.category,
            };

            const response = await systemSettingsApi.getSettings(apiFilters);

            if (response.result) {
                setSettings(response.data.settings || []);
            } else {
                toast.error('Failed to load settings');
            }
        } catch (error) {
            console.error('Error fetching settings:', error);
            toast.error('Failed to load settings');
        } finally {
            setLoading(false);
        }
    };

    const handleEditSetting = (setting) => {
        setSelectedSetting(setting);
        setShowEditModal(true);
    };

    const handleSaveSetting = async (settingId, settingValue, changeReason) => {
        setSaving(true);
        try {
            const response = await systemSettingsApi.updateSetting(settingId, settingValue, changeReason);

            if (response.result) {
                toast.success('Setting updated successfully');
                setShowEditModal(false);
                fetchSettings();
            } else {
                toast.error(response.message || 'Failed to update setting');
            }
        } catch (error) {
            console.error('Error updating setting:', error);
            toast.error('Failed to update setting');
        } finally {
            setSaving(false);
        }
    };

    const handleViewHistory = (setting) => {
        setSelectedSetting(setting);
        setShowHistoryDrawer(true);
    };

    const handleCategoryChange = (category) => {
        setFilters((prev) => ({ ...prev, category, pageNumber: 1 }));
    };

    const handleSearchChange = (searchTerm) => {
        setFilters((prev) => ({ ...prev, searchTerm, pageNumber: 1 }));
    };

    const handleRefresh = () => {
        fetchSettings();
        toast.success('Settings refreshed');
    };

    return (
        <div className="p-6">
            {/* Header Actions */}
            <div className="mb-6">
                <div className="flex items-center justify-between gap-4">
                    {/* Search */}
                    <div className="relative flex-1 max-w-md">
                        <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
                        <input
                            type="text"
                            value={filters.searchTerm}
                            onChange={(e) => handleSearchChange(e.target.value)}
                            placeholder="Search settings..."
                            className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                        />
                    </div>

                    {/* Actions */}
                    <div className="flex items-center gap-3">
                        <button
                            onClick={handleRefresh}
                            className="px-4 py-2 bg-white border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors flex items-center gap-2"
                            disabled={loading}
                        >
                            <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
                            <span>Refresh</span>
                        </button>
                    </div>
                </div>

                {/* Category Filters */}
                <div className="flex items-center gap-2 mt-4">
                    <Filter className="w-4 h-4 text-gray-600" />
                    <span className="text-sm font-medium text-gray-700">Category:</span>
                    <div className="flex flex-wrap gap-2">
                        {categories.map((category) => (
                            <button
                                key={category}
                                onClick={() => handleCategoryChange(category === 'All' ? '' : category)}
                                className={`px-3 py-1 rounded-full text-sm font-medium transition-colors ${
                                    (category === 'All' && filters.category === '') ||
                                    filters.category === category
                                        ? 'bg-blue-600 text-white'
                                        : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                                }`}
                            >
                                {category}
                            </button>
                        ))}
                    </div>
                </div>
            </div>

            {/* Settings Table */}
            <SettingsTable
                settings={settings}
                onEdit={handleEditSetting}
                onViewHistory={handleViewHistory}
                loading={loading}
            />

            {/* Edit Modal */}
            <SettingEditModal
                setting={selectedSetting}
                isOpen={showEditModal}
                onClose={() => setShowEditModal(false)}
                onSave={handleSaveSetting}
                saving={saving}
            />

            {/* History Drawer */}
            <SettingHistoryDrawer
                setting={selectedSetting}
                isOpen={showHistoryDrawer}
                onClose={() => setShowHistoryDrawer(false)}
            />
        </div>
    );
};

export default SystemSettingsTab;
