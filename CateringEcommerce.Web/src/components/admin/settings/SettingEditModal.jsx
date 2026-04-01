import { useState, useEffect } from 'react';
import { X, Save, AlertCircle } from 'lucide-react';

/**
 * Setting Edit Modal Component
 * Modal for editing setting values with validation
 */
const SettingEditModal = ({ setting, isOpen, onClose, onSave, saving }) => {
    const [formData, setFormData] = useState({
        settingValue: '',
        changeReason: '',
    });
    const [errors, setErrors] = useState({});

    useEffect(() => {
        if (setting) {
            setFormData({
                settingValue: setting.settingValue || '',
                changeReason: '',
            });
            setErrors({});
        }
    }, [setting]);

    const validateForm = () => {
        const newErrors = {};

        if (!formData.settingValue.trim()) {
            newErrors.settingValue = 'Setting value is required';
        }

        // Validate based on valueType
        if (setting.valueType === 'NUMBER') {
            if (isNaN(formData.settingValue)) {
                newErrors.settingValue = 'Value must be a number';
            }
        } else if (setting.valueType === 'BOOLEAN') {
            const lowerValue = formData.settingValue.toLowerCase();
            if (lowerValue !== 'true' && lowerValue !== 'false') {
                newErrors.settingValue = 'Value must be "true" or "false"';
            }
        } else if (setting.valueType === 'JSON') {
            try {
                JSON.parse(formData.settingValue);
            } catch (e) {
                newErrors.settingValue = 'Invalid JSON format';
            }
        }

        // Validate against regex if provided
        if (setting.validationRegex) {
            try {
                const regex = new RegExp(setting.validationRegex);
                if (!regex.test(formData.settingValue)) {
                    newErrors.settingValue = 'Value does not match required format';
                }
            } catch (e) {
                console.error('Invalid regex:', e);
            }
        }

        if (!formData.changeReason.trim()) {
            newErrors.changeReason = 'Change reason is required';
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (!validateForm()) {
            return;
        }

        await onSave(setting.settingId, formData.settingValue, formData.changeReason);
    };

    const handleChange = (field, value) => {
        setFormData((prev) => ({ ...prev, [field]: value }));
        if (errors[field]) {
            setErrors((prev) => ({ ...prev, [field]: '' }));
        }
    };

    if (!isOpen || !setting) return null;

    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4">
            <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full max-h-[90vh] overflow-y-auto">
                {/* Header */}
                <div className="flex items-center justify-between p-6 border-b border-gray-200">
                    <div>
                        <h2 className="text-xl font-bold text-gray-900">Edit Setting</h2>
                        <p className="text-sm text-gray-600 mt-1">{setting.displayName}</p>
                    </div>
                    <button
                        onClick={onClose}
                        className="text-gray-400 hover:text-gray-600"
                        disabled={saving}
                    >
                        <X className="w-6 h-6" />
                    </button>
                </div>

                {/* Form */}
                <form onSubmit={handleSubmit} className="p-6 space-y-6">
                    {/* Setting Info */}
                    <div className="bg-gray-50 rounded-lg p-4 space-y-2">
                        <div className="grid grid-cols-2 gap-4 text-sm">
                            <div>
                                <span className="text-gray-600">Key:</span>
                                <code className="ml-2 text-xs bg-white px-2 py-1 rounded border border-gray-200">
                                    {setting.settingKey}
                                </code>
                            </div>
                            <div>
                                <span className="text-gray-600">Category:</span>
                                <span className="ml-2 font-medium">{setting.category}</span>
                            </div>
                            <div>
                                <span className="text-gray-600">Type:</span>
                                <span className="ml-2 font-medium">{setting.valueType}</span>
                            </div>
                            <div>
                                <span className="text-gray-600">Default:</span>
                                <span className="ml-2 text-gray-700">{setting.defaultValue || '-'}</span>
                            </div>
                        </div>
                        {setting.description && (
                            <div className="pt-2 border-t border-gray-200">
                                <p className="text-sm text-gray-600">{setting.description}</p>
                            </div>
                        )}
                    </div>

                    {/* Setting Value */}
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                            Setting Value *
                        </label>
                        {setting.valueType === 'BOOLEAN' ? (
                            <select
                                value={formData.settingValue}
                                onChange={(e) => handleChange('settingValue', e.target.value)}
                                className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                                    errors.settingValue ? 'border-red-500' : 'border-gray-300'
                                }`}
                                disabled={saving}
                            >
                                <option value="true">Enabled (true)</option>
                                <option value="false">Disabled (false)</option>
                            </select>
                        ) : setting.valueType === 'JSON' ? (
                            <textarea
                                value={formData.settingValue}
                                onChange={(e) => handleChange('settingValue', e.target.value)}
                                rows={6}
                                className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 font-mono text-sm ${
                                    errors.settingValue ? 'border-red-500' : 'border-gray-300'
                                }`}
                                placeholder='{"key": "value"}'
                                disabled={saving}
                            />
                        ) : (
                            <input
                                type="text"
                                value={formData.settingValue}
                                onChange={(e) => handleChange('settingValue', e.target.value)}
                                className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                                    errors.settingValue ? 'border-red-500' : 'border-gray-300'
                                }`}
                                placeholder={`Enter ${setting.valueType.toLowerCase()} value`}
                                disabled={saving}
                            />
                        )}
                        {errors.settingValue && (
                            <p className="text-sm text-red-600 mt-1 flex items-center gap-1">
                                <AlertCircle className="w-4 h-4" />
                                {errors.settingValue}
                            </p>
                        )}
                    </div>

                    {/* Change Reason */}
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                            Change Reason *
                        </label>
                        <textarea
                            value={formData.changeReason}
                            onChange={(e) => handleChange('changeReason', e.target.value)}
                            rows={3}
                            className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                                errors.changeReason ? 'border-red-500' : 'border-gray-300'
                            }`}
                            placeholder="Explain why this change is being made..."
                            disabled={saving}
                        />
                        {errors.changeReason && (
                            <p className="text-sm text-red-600 mt-1 flex items-center gap-1">
                                <AlertCircle className="w-4 h-4" />
                                {errors.changeReason}
                            </p>
                        )}
                    </div>

                    {/* Warning for sensitive settings */}
                    {setting.isSensitive && (
                        <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
                            <div className="flex items-start gap-2">
                                <AlertCircle className="w-5 h-5 text-yellow-600 flex-shrink-0 mt-0.5" />
                                <div>
                                    <p className="text-sm font-medium text-yellow-800">
                                        Sensitive Setting
                                    </p>
                                    <p className="text-sm text-yellow-700 mt-1">
                                        This is a sensitive setting. Changes will be logged and may affect system functionality.
                                    </p>
                                </div>
                            </div>
                        </div>
                    )}

                    {/* Actions */}
                    <div className="flex items-center justify-end gap-3 pt-4 border-t border-gray-200">
                        <button
                            type="button"
                            onClick={onClose}
                            className="px-4 py-2 text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
                            disabled={saving}
                        >
                            Cancel
                        </button>
                        <button
                            type="submit"
                            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors flex items-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
                            disabled={saving}
                        >
                            {saving ? (
                                <>
                                    <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                                    <span>Saving...</span>
                                </>
                            ) : (
                                <>
                                    <Save className="w-4 h-4" />
                                    <span>Save Changes</span>
                                </>
                            )}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default SettingEditModal;
