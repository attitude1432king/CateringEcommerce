import { useState, useEffect } from 'react';
import { X, Save, AlertCircle } from 'lucide-react';

/**
 * Commission Config Form Component
 * Form for creating/editing commission configurations
 */
const CommissionConfigForm = ({ config, isOpen, onClose, onSave, saving, mode = 'create' }) => {
    const [formData, setFormData] = useState({
        configName: '',
        configType: 'GLOBAL',
        cateringOwnerId: null,
        commissionRate: 0,
        fixedFee: 0,
        minOrderValue: null,
        maxOrderValue: null,
        isActive: true,
        effectiveFrom: '',
        effectiveTo: null,
    });
    const [errors, setErrors] = useState({});

    useEffect(() => {
        if (config && mode === 'edit') {
            setFormData({
                configName: config.configName || '',
                configType: config.configType || 'GLOBAL',
                cateringOwnerId: config.cateringOwnerId || null,
                commissionRate: config.commissionRate || 0,
                fixedFee: config.fixedFee || 0,
                minOrderValue: config.minOrderValue || null,
                maxOrderValue: config.maxOrderValue || null,
                isActive: config.isActive !== undefined ? config.isActive : true,
                effectiveFrom: config.effectiveFrom ? config.effectiveFrom.split('T')[0] : '',
                effectiveTo: config.effectiveTo ? config.effectiveTo.split('T')[0] : null,
            });
            setErrors({});
        } else if (mode === 'create') {
            // Reset for create mode
            setFormData({
                configName: '',
                configType: 'GLOBAL',
                cateringOwnerId: null,
                commissionRate: 0,
                fixedFee: 0,
                minOrderValue: null,
                maxOrderValue: null,
                isActive: true,
                effectiveFrom: new Date().toISOString().split('T')[0],
                effectiveTo: null,
            });
            setErrors({});
        }
    }, [config, mode, isOpen]);

    const validateForm = () => {
        const newErrors = {};

        if (!formData.configName.trim()) {
            newErrors.configName = 'Configuration name is required';
        }

        if (!formData.configType) {
            newErrors.configType = 'Configuration type is required';
        }

        if (formData.configType === 'CATERING_SPECIFIC' && !formData.cateringOwnerId) {
            newErrors.cateringOwnerId = 'Catering owner is required for specific configurations';
        }

        if (formData.commissionRate < 0 || formData.commissionRate > 100) {
            newErrors.commissionRate = 'Commission rate must be between 0 and 100';
        }

        if (formData.fixedFee < 0) {
            newErrors.fixedFee = 'Fixed fee cannot be negative';
        }

        if (formData.configType === 'TIERED') {
            if (!formData.minOrderValue && !formData.maxOrderValue) {
                newErrors.minOrderValue = 'At least one order value threshold is required for tiered configs';
            }
            if (formData.minOrderValue && formData.maxOrderValue && formData.minOrderValue >= formData.maxOrderValue) {
                newErrors.maxOrderValue = 'Max order value must be greater than min order value';
            }
        }

        if (!formData.effectiveFrom) {
            newErrors.effectiveFrom = 'Effective from date is required';
        }

        if (formData.effectiveTo && formData.effectiveFrom && formData.effectiveTo <= formData.effectiveFrom) {
            newErrors.effectiveTo = 'Effective to date must be after effective from date';
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (!validateForm()) {
            return;
        }

        const payload = {
            ...formData,
            minOrderValue: formData.minOrderValue || null,
            maxOrderValue: formData.maxOrderValue || null,
            effectiveTo: formData.effectiveTo || null,
            commissionRate: parseFloat(formData.commissionRate),
            fixedFee: parseFloat(formData.fixedFee),
        };

        if (mode === 'edit') {
            await onSave(config.configId, payload);
        } else {
            await onSave(payload);
        }
    };

    const handleChange = (field, value) => {
        setFormData((prev) => ({ ...prev, [field]: value }));
        if (errors[field]) {
            setErrors((prev) => ({ ...prev, [field]: '' }));
        }
    };

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4">
            <div className="bg-white rounded-lg shadow-xl max-w-3xl w-full max-h-[90vh] overflow-y-auto">
                {/* Header */}
                <div className="flex items-center justify-between p-6 border-b border-gray-200">
                    <div>
                        <h2 className="text-xl font-bold text-gray-900">
                            {mode === 'create' ? 'Create Commission Configuration' : 'Edit Commission Configuration'}
                        </h2>
                        <p className="text-sm text-gray-600 mt-1">
                            Configure commission rates and rules
                        </p>
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
                    {/* Config Name */}
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                            Configuration Name *
                        </label>
                        <input
                            type="text"
                            value={formData.configName}
                            onChange={(e) => handleChange('configName', e.target.value)}
                            className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                                errors.configName ? 'border-red-500' : 'border-gray-300'
                            }`}
                            placeholder="e.g., Standard Commission, Premium Partner Rate"
                            disabled={saving}
                        />
                        {errors.configName && (
                            <p className="text-sm text-red-600 mt-1">{errors.configName}</p>
                        )}
                    </div>

                    {/* Config Type */}
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                            Configuration Type *
                        </label>
                        <select
                            value={formData.configType}
                            onChange={(e) => handleChange('configType', e.target.value)}
                            className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                                errors.configType ? 'border-red-500' : 'border-gray-300'
                            }`}
                            disabled={saving || mode === 'edit'}
                        >
                            <option value="GLOBAL">Global (applies to all partners)</option>
                            <option value="CATERING_SPECIFIC">Catering Specific (applies to one partner)</option>
                            <option value="TIERED">Tiered (based on order value)</option>
                        </select>
                        {errors.configType && (
                            <p className="text-sm text-red-600 mt-1">{errors.configType}</p>
                        )}
                    </div>

                    {/* Catering Owner ID (only for CATERING_SPECIFIC) */}
                    {formData.configType === 'CATERING_SPECIFIC' && (
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">
                                Catering Owner ID *
                            </label>
                            <input
                                type="number"
                                value={formData.cateringOwnerId || ''}
                                onChange={(e) => handleChange('cateringOwnerId', e.target.value ? parseInt(e.target.value) : null)}
                                className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                                    errors.cateringOwnerId ? 'border-red-500' : 'border-gray-300'
                                }`}
                                placeholder="Enter catering owner ID"
                                disabled={saving}
                            />
                            {errors.cateringOwnerId && (
                                <p className="text-sm text-red-600 mt-1">{errors.cateringOwnerId}</p>
                            )}
                        </div>
                    )}

                    {/* Commission Rate and Fixed Fee */}
                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">
                                Commission Rate (%) *
                            </label>
                            <input
                                type="number"
                                step="0.01"
                                min="0"
                                max="100"
                                value={formData.commissionRate}
                                onChange={(e) => handleChange('commissionRate', e.target.value)}
                                className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                                    errors.commissionRate ? 'border-red-500' : 'border-gray-300'
                                }`}
                                placeholder="0.00"
                                disabled={saving}
                            />
                            {errors.commissionRate && (
                                <p className="text-sm text-red-600 mt-1">{errors.commissionRate}</p>
                            )}
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">
                                Fixed Fee *
                            </label>
                            <input
                                type="number"
                                step="0.01"
                                min="0"
                                value={formData.fixedFee}
                                onChange={(e) => handleChange('fixedFee', e.target.value)}
                                className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                                    errors.fixedFee ? 'border-red-500' : 'border-gray-300'
                                }`}
                                placeholder="0.00"
                                disabled={saving}
                            />
                            {errors.fixedFee && (
                                <p className="text-sm text-red-600 mt-1">{errors.fixedFee}</p>
                            )}
                        </div>
                    </div>

                    {/* Order Value Thresholds (only for TIERED) */}
                    {formData.configType === 'TIERED' && (
                        <div className="grid grid-cols-2 gap-4">
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-2">
                                    Min Order Value
                                </label>
                                <input
                                    type="number"
                                    step="0.01"
                                    min="0"
                                    value={formData.minOrderValue || ''}
                                    onChange={(e) => handleChange('minOrderValue', e.target.value ? parseFloat(e.target.value) : null)}
                                    className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                                        errors.minOrderValue ? 'border-red-500' : 'border-gray-300'
                                    }`}
                                    placeholder="0.00"
                                    disabled={saving}
                                />
                                {errors.minOrderValue && (
                                    <p className="text-sm text-red-600 mt-1">{errors.minOrderValue}</p>
                                )}
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-2">
                                    Max Order Value
                                </label>
                                <input
                                    type="number"
                                    step="0.01"
                                    min="0"
                                    value={formData.maxOrderValue || ''}
                                    onChange={(e) => handleChange('maxOrderValue', e.target.value ? parseFloat(e.target.value) : null)}
                                    className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                                        errors.maxOrderValue ? 'border-red-500' : 'border-gray-300'
                                    }`}
                                    placeholder="999999.99"
                                    disabled={saving}
                                />
                                {errors.maxOrderValue && (
                                    <p className="text-sm text-red-600 mt-1">{errors.maxOrderValue}</p>
                                )}
                            </div>
                        </div>
                    )}

                    {/* Effective Dates */}
                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">
                                Effective From *
                            </label>
                            <input
                                type="date"
                                value={formData.effectiveFrom}
                                onChange={(e) => handleChange('effectiveFrom', e.target.value)}
                                className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                                    errors.effectiveFrom ? 'border-red-500' : 'border-gray-300'
                                }`}
                                disabled={saving}
                            />
                            {errors.effectiveFrom && (
                                <p className="text-sm text-red-600 mt-1">{errors.effectiveFrom}</p>
                            )}
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">
                                Effective To (Optional)
                            </label>
                            <input
                                type="date"
                                value={formData.effectiveTo || ''}
                                onChange={(e) => handleChange('effectiveTo', e.target.value || null)}
                                className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                                    errors.effectiveTo ? 'border-red-500' : 'border-gray-300'
                                }`}
                                disabled={saving}
                            />
                            {errors.effectiveTo && (
                                <p className="text-sm text-red-600 mt-1">{errors.effectiveTo}</p>
                            )}
                        </div>
                    </div>

                    {/* Is Active */}
                    <div className="flex items-center gap-2">
                        <input
                            type="checkbox"
                            id="isActive"
                            checked={formData.isActive}
                            onChange={(e) => handleChange('isActive', e.target.checked)}
                            className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                            disabled={saving}
                        />
                        <label htmlFor="isActive" className="text-sm font-medium text-gray-700">
                            Active
                        </label>
                    </div>

                    {/* Info Note */}
                    <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                        <div className="flex items-start gap-2">
                            <AlertCircle className="w-5 h-5 text-blue-600 flex-shrink-0 mt-0.5" />
                            <div>
                                <p className="text-sm font-medium text-blue-800">Commission Calculation</p>
                                <p className="text-sm text-blue-700 mt-1">
                                    Total Commission = (Order Value × Commission Rate%) + Fixed Fee
                                </p>
                            </div>
                        </div>
                    </div>

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
                                    <span>{mode === 'create' ? 'Create' : 'Update'}</span>
                                </>
                            )}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default CommissionConfigForm;
