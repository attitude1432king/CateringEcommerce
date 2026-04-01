import { useState, useEffect } from 'react';
import { X, Clock, User, FileText } from 'lucide-react';
import { systemSettingsApi } from '../../../services/settingsApi';
import { toast } from 'react-hot-toast';

/**
 * Setting History Drawer Component
 * Displays change history for a setting
 */
const SettingHistoryDrawer = ({ setting, isOpen, onClose }) => {
    const [history, setHistory] = useState([]);
    const [loading, setLoading] = useState(false);
    const [pagination, setPagination] = useState({
        currentPage: 1,
        pageSize: 20,
        totalCount: 0,
    });

    useEffect(() => {
        if (isOpen && setting) {
            fetchHistory();
        }
    }, [isOpen, setting]);

    const fetchHistory = async () => {
        if (!setting) return;

        setLoading(true);
        try {
            const response = await systemSettingsApi.getSettingHistory(
                setting.settingId,
                pagination.currentPage,
                pagination.pageSize
            );

            if (response.result) {
                setHistory(response.data.history || []);
                setPagination((prev) => ({
                    ...prev,
                    totalCount: response.data.totalCount || 0,
                }));
            } else {
                toast.error('Failed to load setting history');
            }
        } catch (error) {
            console.error('Error fetching history:', error);
            toast.error('Failed to load setting history');
        } finally {
            setLoading(false);
        }
    };

    const formatDate = (dateString) => {
        const date = new Date(dateString);
        return date.toLocaleString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
        });
    };

    if (!isOpen) return null;

    return (
        <>
            {/* Backdrop */}
            <div
                className="fixed inset-0 bg-black bg-opacity-50 z-40"
                onClick={onClose}
            />

            {/* Drawer */}
            <div className="fixed right-0 top-0 h-full w-full max-w-2xl bg-white shadow-xl z-50 flex flex-col">
                {/* Header */}
                <div className="flex items-center justify-between p-6 border-b border-gray-200">
                    <div>
                        <h2 className="text-xl font-bold text-gray-900">Change History</h2>
                        {setting && (
                            <p className="text-sm text-gray-600 mt-1">{setting.displayName}</p>
                        )}
                    </div>
                    <button
                        onClick={onClose}
                        className="text-gray-400 hover:text-gray-600"
                    >
                        <X className="w-6 h-6" />
                    </button>
                </div>

                {/* Content */}
                <div className="flex-1 overflow-y-auto p-6">
                    {loading ? (
                        <div className="flex items-center justify-center h-64">
                            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
                        </div>
                    ) : history.length === 0 ? (
                        <div className="text-center py-12">
                            <Clock className="w-16 h-16 text-gray-300 mx-auto mb-4" />
                            <p className="text-gray-500">No change history available</p>
                        </div>
                    ) : (
                        <div className="space-y-4">
                            {history.map((item, index) => (
                                <div
                                    key={item.historyId}
                                    className="bg-white border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow"
                                >
                                    {/* Header */}
                                    <div className="flex items-start justify-between mb-3">
                                        <div className="flex items-center gap-2">
                                            <User className="w-4 h-4 text-gray-400" />
                                            <span className="font-medium text-gray-900">
                                                {item.changedByName}
                                            </span>
                                        </div>
                                        <div className="flex items-center gap-2 text-sm text-gray-500">
                                            <Clock className="w-4 h-4" />
                                            {formatDate(item.changeDate)}
                                        </div>
                                    </div>

                                    {/* Changes */}
                                    <div className="space-y-2 mb-3">
                                        <div className="bg-red-50 border border-red-200 rounded p-3">
                                            <p className="text-xs font-medium text-red-700 mb-1">
                                                Old Value
                                            </p>
                                            <code className="text-sm text-red-900 break-all">
                                                {item.oldValue || '(empty)'}
                                            </code>
                                        </div>
                                        <div className="bg-green-50 border border-green-200 rounded p-3">
                                            <p className="text-xs font-medium text-green-700 mb-1">
                                                New Value
                                            </p>
                                            <code className="text-sm text-green-900 break-all">
                                                {item.newValue || '(empty)'}
                                            </code>
                                        </div>
                                    </div>

                                    {/* Change Reason */}
                                    {item.changeReason && (
                                        <div className="bg-gray-50 rounded p-3">
                                            <div className="flex items-center gap-2 mb-1">
                                                <FileText className="w-4 h-4 text-gray-400" />
                                                <p className="text-xs font-medium text-gray-700">
                                                    Change Reason
                                                </p>
                                            </div>
                                            <p className="text-sm text-gray-700">{item.changeReason}</p>
                                        </div>
                                    )}

                                    {/* IP Address */}
                                    {item.ipAddress && (
                                        <div className="text-xs text-gray-500 mt-2">
                                            IP: {item.ipAddress}
                                        </div>
                                    )}
                                </div>
                            ))}
                        </div>
                    )}

                    {/* Pagination Info */}
                    {!loading && history.length > 0 && (
                        <div className="mt-6 text-center text-sm text-gray-600">
                            Showing {history.length} of {pagination.totalCount} changes
                        </div>
                    )}
                </div>

                {/* Footer */}
                <div className="border-t border-gray-200 p-4">
                    <button
                        onClick={onClose}
                        className="w-full px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors"
                    >
                        Close
                    </button>
                </div>
            </div>
        </>
    );
};

export default SettingHistoryDrawer;
