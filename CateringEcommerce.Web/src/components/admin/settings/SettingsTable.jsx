import { useState } from 'react';
import { Edit2, History, Eye, EyeOff, ChevronUp, ChevronDown } from 'lucide-react';

/**
 * Settings Table Component
 * Displays settings in a sortable table with filtering
 */
const SettingsTable = ({ settings, onEdit, onViewHistory, loading }) => {
    const [sortBy, setSortBy] = useState('displayOrder');
    const [sortOrder, setSortOrder] = useState('asc');
    const [showSensitive, setShowSensitive] = useState({});

    const handleSort = (field) => {
        if (sortBy === field) {
            setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc');
        } else {
            setSortBy(field);
            setSortOrder('asc');
        }
    };

    const toggleSensitive = (settingId) => {
        setShowSensitive((prev) => ({
            ...prev,
            [settingId]: !prev[settingId],
        }));
    };

    const sortedSettings = [...settings].sort((a, b) => {
        const aVal = a[sortBy];
        const bVal = b[sortBy];

        if (sortOrder === 'asc') {
            return aVal > bVal ? 1 : -1;
        } else {
            return aVal < bVal ? 1 : -1;
        }
    });

    const renderValue = (setting) => {
        if (setting.isSensitive && !showSensitive[setting.settingId]) {
            return (
                <span className="text-gray-400 italic">
                    ••••••••
                </span>
            );
        }

        if (setting.valueType === 'BOOLEAN') {
            return (
                <span
                    className={`px-2 py-1 rounded-full text-xs font-medium ${
                        setting.settingValue.toLowerCase() === 'true'
                            ? 'bg-green-100 text-green-800'
                            : 'bg-red-100 text-red-800'
                    }`}
                >
                    {setting.settingValue.toLowerCase() === 'true' ? 'Enabled' : 'Disabled'}
                </span>
            );
        }

        if (setting.valueType === 'JSON') {
            return (
                <code className="text-xs bg-gray-100 px-2 py-1 rounded">
                    {setting.settingValue.substring(0, 50)}
                    {setting.settingValue.length > 50 ? '...' : ''}
                </code>
            );
        }

        return (
            <span className="text-sm text-gray-900 break-all">
                {setting.settingValue || '-'}
            </span>
        );
    };

    const SortIcon = ({ field }) => {
        if (sortBy !== field) return null;
        return sortOrder === 'asc' ? (
            <ChevronUp className="w-4 h-4" />
        ) : (
            <ChevronDown className="w-4 h-4" />
        );
    };

    if (loading) {
        return (
            <div className="flex items-center justify-center h-64">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
            </div>
        );
    }

    if (settings.length === 0) {
        return (
            <div className="text-center py-12">
                <p className="text-gray-500">No settings found</p>
            </div>
        );
    }

    return (
        <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                    <tr>
                        <th
                            scope="col"
                            className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100"
                            onClick={() => handleSort('displayName')}
                        >
                            <div className="flex items-center gap-2">
                                Setting Name
                                <SortIcon field="displayName" />
                            </div>
                        </th>
                        <th
                            scope="col"
                            className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100"
                            onClick={() => handleSort('settingKey')}
                        >
                            <div className="flex items-center gap-2">
                                Key
                                <SortIcon field="settingKey" />
                            </div>
                        </th>
                        <th
                            scope="col"
                            className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                        >
                            Value
                        </th>
                        <th
                            scope="col"
                            className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100"
                            onClick={() => handleSort('category')}
                        >
                            <div className="flex items-center gap-2">
                                Category
                                <SortIcon field="category" />
                            </div>
                        </th>
                        <th
                            scope="col"
                            className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100"
                            onClick={() => handleSort('isActive')}
                        >
                            <div className="flex items-center gap-2">
                                Status
                                <SortIcon field="isActive" />
                            </div>
                        </th>
                        <th
                            scope="col"
                            className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider"
                        >
                            Actions
                        </th>
                    </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                    {sortedSettings.map((setting) => (
                        <tr key={setting.settingId} className="hover:bg-gray-50">
                            <td className="px-6 py-4 whitespace-nowrap">
                                <div className="flex flex-col">
                                    <span className="text-sm font-medium text-gray-900">
                                        {setting.displayName}
                                    </span>
                                    {setting.description && (
                                        <span className="text-xs text-gray-500 mt-1">
                                            {setting.description}
                                        </span>
                                    )}
                                </div>
                            </td>
                            <td className="px-6 py-4 whitespace-nowrap">
                                <code className="text-xs text-gray-600 bg-gray-100 px-2 py-1 rounded">
                                    {setting.settingKey}
                                </code>
                            </td>
                            <td className="px-6 py-4">
                                <div className="flex items-center gap-2">
                                    {renderValue(setting)}
                                    {setting.isSensitive && (
                                        <button
                                            onClick={() => toggleSensitive(setting.settingId)}
                                            className="text-gray-400 hover:text-gray-600"
                                            title={showSensitive[setting.settingId] ? 'Hide' : 'Show'}
                                        >
                                            {showSensitive[setting.settingId] ? (
                                                <EyeOff className="w-4 h-4" />
                                            ) : (
                                                <Eye className="w-4 h-4" />
                                            )}
                                        </button>
                                    )}
                                </div>
                            </td>
                            <td className="px-6 py-4 whitespace-nowrap">
                                <span className="px-2 py-1 text-xs font-medium bg-gray-100 text-gray-800 rounded">
                                    {setting.category}
                                </span>
                            </td>
                            <td className="px-6 py-4 whitespace-nowrap">
                                <span
                                    className={`px-2 py-1 text-xs font-medium rounded ${
                                        setting.isActive
                                            ? 'bg-green-100 text-green-800'
                                            : 'bg-gray-100 text-gray-800'
                                    }`}
                                >
                                    {setting.isActive ? 'Active' : 'Inactive'}
                                </span>
                            </td>
                            <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                <div className="flex items-center justify-end gap-2">
                                    {!setting.isReadOnly && (
                                        <button
                                            onClick={() => onEdit(setting)}
                                            className="text-blue-600 hover:text-blue-900"
                                            title="Edit Setting"
                                        >
                                            <Edit2 className="w-4 h-4" />
                                        </button>
                                    )}
                                    <button
                                        onClick={() => onViewHistory(setting)}
                                        className="text-gray-600 hover:text-gray-900"
                                        title="View History"
                                    >
                                        <History className="w-4 h-4" />
                                    </button>
                                </div>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
};

export default SettingsTable;
