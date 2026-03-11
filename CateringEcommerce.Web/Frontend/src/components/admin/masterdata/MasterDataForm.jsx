import React, { useState, useEffect, useRef } from 'react';
import { X, Loader2, AlertCircle } from 'lucide-react';
import { toast } from 'react-hot-toast';

const MasterDataForm = ({
    isOpen,
    onClose,
    onSubmit,
    mode = 'create', // 'create' | 'edit'
    title,
    initialData = {},
    fields = [],
    loading = false,
}) => {
    const [formData, setFormData] = useState({});
    const [errors, setErrors] = useState({});
    const initialFormDataRef = useRef({});

    useEffect(() => {
        if (isOpen) {
            if (mode === 'edit' && initialData) {
                setFormData(initialData);
                // Store initial data for comparison
                initialFormDataRef.current = { ...initialData };
            } else {
                const defaultValues = {};
                fields.forEach(field => {
                    defaultValues[field.name] = field.defaultValue || '';
                });
                setFormData(defaultValues);
                initialFormDataRef.current = {};
            }
            setErrors({});
        }
    }, [isOpen, mode, initialData, fields]);

    const handleChange = (e) => {
        const { name, value, type, checked } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: type === 'checkbox' ? checked : value
        }));
        // Clear error for this field
        if (errors[name]) {
            setErrors(prev => ({ ...prev, [name]: null }));
        }
    };

    const validateForm = () => {
        const newErrors = {};
        fields.forEach(field => {
            if (field.required && !formData[field.name]) {
                newErrors[field.name] = `${field.label} is required`;
            }
            if (field.validate) {
                const error = field.validate(formData[field.name], formData);
                if (error) {
                    newErrors[field.name] = error;
                }
            }
        });
        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const hasChanges = () => {
        if (mode !== 'edit') return true;

        // Compare current formData with initial data
        const fieldsToCheck = fields.map(f => f.name);

        for (const fieldName of fieldsToCheck) {
            const currentValue = formData[fieldName];
            const initialValue = initialFormDataRef.current[fieldName];

            // Handle different types of comparisons
            if (currentValue !== initialValue) {
                // For strings, trim and compare
                if (typeof currentValue === 'string' && typeof initialValue === 'string') {
                    if (currentValue.trim() !== initialValue.trim()) {
                        return true;
                    }
                } else {
                    return true;
                }
            }
        }

        return false;
    };

    const handleSubmit = (e) => {
        e.preventDefault();

        if (!validateForm()) {
            return;
        }

        // Check if there are any changes in edit mode
        if (mode === 'edit' && !hasChanges()) {
            toast.error('No changes detected. Please modify at least one field before saving.', {
                duration: 4000,
                icon: '⚠️',
            });
            return;
        }

        onSubmit(formData);
    };

    const renderField = (field) => {
        const commonClasses = "w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent";
        const errorClasses = errors[field.name] ? "border-red-500" : "border-gray-300";

        switch (field.type) {
            case 'text':
            case 'number':
                return (
                    <input
                        type={field.type}
                        name={field.name}
                        value={formData[field.name] || ''}
                        onChange={handleChange}
                        placeholder={field.placeholder}
                        disabled={field.disabled || loading}
                        autoComplete="off"
                        className={`${commonClasses} ${errorClasses}`}
                    />
                );

            case 'textarea':
                return (
                    <textarea
                        name={field.name}
                        value={formData[field.name] || ''}
                        onChange={handleChange}
                        placeholder={field.placeholder}
                        rows={field.rows || 3}
                        disabled={field.disabled || loading}
                        className={`${commonClasses} ${errorClasses}`}
                    />
                );

            case 'select':
                return (
                    <select
                        name={field.name}
                        value={formData[field.name] || ''}
                        onChange={handleChange}
                        disabled={field.disabled || loading}
                        className={`${commonClasses} ${errorClasses}`}
                    >
                        <option value="">Select {field.label}</option>
                        {field.options?.map(option => (
                            <option key={option.value} value={option.value}>
                                {option.label}
                            </option>
                        ))}
                    </select>
                );

            case 'checkbox':
                return (
                    <div className="flex items-center">
                        <input
                            type="checkbox"
                            name={field.name}
                            checked={formData[field.name] || false}
                            onChange={handleChange}
                            disabled={field.disabled || loading}
                            className="w-4 h-4 text-purple-600 border-gray-300 rounded focus:ring-purple-500"
                        />
                        <label className="ml-2 text-sm text-gray-700">{field.checkboxLabel || field.label}</label>
                    </div>
                );

            default:
                return null;
        }
    };

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 z-50 overflow-y-auto">
            <div className="flex items-center justify-center min-h-screen px-4 pt-4 pb-20 text-center sm:block sm:p-0">
                {/* Backdrop */}
                <div
                    className="fixed inset-0 transition-opacity bg-gray-500 bg-opacity-75"
                    onClick={onClose}
                />

                {/* Modal */}
                <div className="inline-block w-full max-w-lg my-8 overflow-hidden text-left align-middle transition-all transform bg-white rounded-lg shadow-xl">
                    {/* Header */}
                    <div className="flex items-center justify-between px-6 py-4 border-b">
                        <h3 className="text-lg font-semibold text-gray-900">{title}</h3>
                        <button
                            onClick={onClose}
                            disabled={loading}
                            className="p-1 text-gray-400 hover:text-gray-600 transition-colors"
                        >
                            <X className="w-5 h-5" />
                        </button>
                    </div>

                    {/* Form */}
                    <form onSubmit={handleSubmit}>
                        <div className="px-6 py-4 space-y-4">
                            {fields.map((field) => (
                                <div key={field.name}>
                                    {field.type !== 'checkbox' && (
                                        <label className="block text-sm font-medium text-gray-700 mb-1">
                                            {field.label}
                                            {field.required && <span className="text-red-500 ml-1">*</span>}
                                        </label>
                                    )}
                                    {renderField(field)}
                                    {field.hint && (
                                        <p className="mt-1 text-xs text-gray-500">{field.hint}</p>
                                    )}
                                    {errors[field.name] && (
                                        <p className="mt-1 text-xs text-red-500">{errors[field.name]}</p>
                                    )}
                                </div>
                            ))}
                        </div>

                        {/* Footer */}
                        <div className="px-6 py-4 border-t">
                            {/* No Changes Warning */}
                            {mode === 'edit' && !hasChanges() && (
                                <div className="mb-3 flex items-start gap-2 p-3 bg-yellow-50 border border-yellow-200 rounded-lg">
                                    <AlertCircle className="w-5 h-5 text-yellow-600 flex-shrink-0 mt-0.5" />
                                    <div>
                                        <p className="text-sm font-medium text-yellow-800">No changes detected</p>
                                        <p className="text-xs text-yellow-700 mt-0.5">
                                            Modify at least one field before saving.
                                        </p>
                                    </div>
                                </div>
                            )}

                            <div className="flex items-center justify-end gap-3">
                                <button
                                    type="button"
                                    onClick={onClose}
                                    disabled={loading}
                                    className="px-4 py-2 text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors disabled:opacity-50"
                                >
                                    Cancel
                                </button>
                                <button
                                    type="submit"
                                    disabled={loading}
                                    className="px-4 py-2 text-white bg-purple-600 rounded-lg hover:bg-purple-700 transition-colors disabled:opacity-50 flex items-center gap-2"
                                >
                                    {loading && <Loader2 className="w-4 h-4 animate-spin" />}
                                    {mode === 'edit' ? 'Update' : 'Create'}
                                </button>
                            </div>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    );
};

export default MasterDataForm;
