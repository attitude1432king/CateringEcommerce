import { useState } from 'react';
import { Settings, DollarSign, Mail, Download, Upload } from 'lucide-react';
import AdminLayout from '../../components/admin/layout/AdminLayout';
import { ProtectedRoute } from '../../components/admin/auth/ProtectedRoute';
import SystemSettingsTab from '../../components/admin/settings/SystemSettingsTab';
import CommissionConfigTab from '../../components/admin/settings/CommissionConfigTab';
import EmailTemplatesTab from '../../components/admin/settings/EmailTemplatesTab';

/**
 * Admin Settings Management Page
 *
 * Features:
 * - Tab-based navigation for different settings sections
 * - System Settings: Manage platform configuration
 * - Commission Config: Manage commission rates and rules
 * - Email Templates: Manage email templates
 * - Export/Import functionality
 * - Permission-based access control
 */
const AdminSettings = () => {
    const [activeTab, setActiveTab] = useState('system');

    const tabs = [
        {
            id: 'system',
            label: 'System Settings',
            icon: Settings,
            component: SystemSettingsTab,
            permission: 'SYSTEM_CONFIG',
        },
        {
            id: 'commission',
            label: 'Commission Config',
            icon: DollarSign,
            component: CommissionConfigTab,
            permission: 'SYSTEM_CONFIG',
        },
        {
            id: 'email',
            label: 'Email Templates',
            icon: Mail,
            component: EmailTemplatesTab,
            permission: 'SYSTEM_CONFIG',
        },
    ];

    const activeTabData = tabs.find((tab) => tab.id === activeTab);
    const ActiveTabComponent = activeTabData?.component;

    return (
        <ProtectedRoute permission="SYSTEM_CONFIG">
            <AdminLayout>
                <div className="p-6 space-y-6">
                    {/* Header */}
                    <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                        <div className="flex items-center justify-between">
                            <div>
                                <h1 className="text-2xl font-bold text-gray-900">Settings</h1>
                                <p className="text-sm text-gray-600 mt-1">
                                    Manage system configuration, commission rates, and email templates
                                </p>
                            </div>
                            <div className="flex gap-3">
                                <button
                                    className="px-4 py-2 bg-white border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors flex items-center gap-2"
                                    title="Export Settings"
                                >
                                    <Download className="w-4 h-4" />
                                    <span>Export</span>
                                </button>
                                <button
                                    className="px-4 py-2 bg-white border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors flex items-center gap-2"
                                    title="Import Settings"
                                >
                                    <Upload className="w-4 h-4" />
                                    <span>Import</span>
                                </button>
                            </div>
                        </div>

                        {/* Tab Navigation */}
                        <div className="flex gap-2 mt-6 border-b border-gray-200">
                            {tabs.map((tab) => {
                                const Icon = tab.icon;
                                return (
                                    <button
                                        key={tab.id}
                                        onClick={() => setActiveTab(tab.id)}
                                        className={`
                                            flex items-center gap-2 px-4 py-3 font-medium transition-colors relative
                                            ${
                                                activeTab === tab.id
                                                    ? 'text-blue-600 border-b-2 border-blue-600'
                                                    : 'text-gray-600 hover:text-gray-900'
                                            }
                                        `}
                                    >
                                        <Icon className="w-5 h-5" />
                                        <span>{tab.label}</span>
                                    </button>
                                );
                            })}
                        </div>
                    </div>

                    {/* Active Tab Content */}
                    <div className="bg-white rounded-lg shadow-sm border border-gray-200">
                        {ActiveTabComponent && <ActiveTabComponent />}
                    </div>
                </div>
            </AdminLayout>
        </ProtectedRoute>
    );
};

export default AdminSettings;
