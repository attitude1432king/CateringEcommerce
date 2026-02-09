import { useState, useEffect } from 'react';
import { X, Save, Eye, Send, Code } from 'lucide-react';

/**
 * Email Template Editor Component
 * Editor for email templates with preview and test send
 */
const EmailTemplateEditor = ({ template, isOpen, onClose, onSave, onPreview, onTestSend, saving }) => {
    const [formData, setFormData] = useState({
        subject: '',
        body: '',
        changeReason: '',
    });
    const [errors, setErrors] = useState({});
    const [showPreview, setShowPreview] = useState(false);
    const [variables, setVariables] = useState([]);

    useEffect(() => {
        if (template) {
            setFormData({
                subject: template.subject || '',
                body: template.body || '',
                changeReason: '',
            });
            setErrors({});
            // Extract variables from template
            extractVariables(template.subject, template.body);
        }
    }, [template]);

    const extractVariables = (subject, body) => {
        const text = `${subject} ${body}`;
        const regex = /\{\{\s*(\w+)\s*\}\}/g;
        const matches = [...text.matchAll(regex)];
        const uniqueVars = [...new Set(matches.map((m) => m[1]))];
        setVariables(uniqueVars);
    };

    const validateForm = () => {
        const newErrors = {};

        if (!formData.subject.trim()) {
            newErrors.subject = 'Subject is required';
        }

        if (!formData.body.trim()) {
            newErrors.body = 'Body is required';
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

        await onSave(template.templateId, formData.subject, formData.body, formData.changeReason);
    };

    const handleChange = (field, value) => {
        setFormData((prev) => ({ ...prev, [field]: value }));
        if (errors[field]) {
            setErrors((prev) => ({ ...prev, [field]: '' }));
        }
        if (field === 'subject' || field === 'body') {
            extractVariables(
                field === 'subject' ? value : formData.subject,
                field === 'body' ? value : formData.body
            );
        }
    };

    const insertVariable = (variable) => {
        const textarea = document.getElementById('template-body');
        const start = textarea.selectionStart;
        const end = textarea.selectionEnd;
        const text = formData.body;
        const before = text.substring(0, start);
        const after = text.substring(end);
        const newText = `${before}{{ ${variable} }}${after}`;
        handleChange('body', newText);
        setTimeout(() => {
            textarea.focus();
            textarea.setSelectionRange(start + variable.length + 6, start + variable.length + 6);
        }, 0);
    };

    const handlePreview = () => {
        if (onPreview) {
            onPreview(template.templateId, formData.subject, formData.body);
        }
        setShowPreview(true);
    };

    if (!isOpen || !template) return null;

    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4">
            <div className="bg-white rounded-lg shadow-xl max-w-5xl w-full max-h-[90vh] overflow-y-auto">
                {/* Header */}
                <div className="flex items-center justify-between p-6 border-b border-gray-200">
                    <div>
                        <h2 className="text-xl font-bold text-gray-900">Edit Email Template</h2>
                        <p className="text-sm text-gray-600 mt-1">
                            {template.templateName} ({template.templateCode})
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
                <form onSubmit={handleSubmit} className="p-6">
                    <div className="grid grid-cols-3 gap-6">
                        {/* Left Column - Editor */}
                        <div className="col-span-2 space-y-6">
                            {/* Template Info */}
                            <div className="bg-gray-50 rounded-lg p-4">
                                <div className="grid grid-cols-3 gap-4 text-sm">
                                    <div>
                                        <span className="text-gray-600">Category:</span>
                                        <span className="ml-2 font-medium">{template.category}</span>
                                    </div>
                                    <div>
                                        <span className="text-gray-600">Version:</span>
                                        <span className="ml-2 font-medium">{template.version}</span>
                                    </div>
                                    <div>
                                        <span className="text-gray-600">Status:</span>
                                        <span
                                            className={`ml-2 px-2 py-1 text-xs font-medium rounded ${
                                                template.isActive
                                                    ? 'bg-green-100 text-green-800'
                                                    : 'bg-gray-100 text-gray-800'
                                            }`}
                                        >
                                            {template.isActive ? 'Active' : 'Inactive'}
                                        </span>
                                    </div>
                                </div>
                            </div>

                            {/* Subject */}
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-2">
                                    Subject *
                                </label>
                                <input
                                    type="text"
                                    value={formData.subject}
                                    onChange={(e) => handleChange('subject', e.target.value)}
                                    className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                                        errors.subject ? 'border-red-500' : 'border-gray-300'
                                    }`}
                                    placeholder="Email subject line"
                                    disabled={saving}
                                />
                                {errors.subject && (
                                    <p className="text-sm text-red-600 mt-1">{errors.subject}</p>
                                )}
                            </div>

                            {/* Body */}
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-2">
                                    Body *
                                </label>
                                <textarea
                                    id="template-body"
                                    value={formData.body}
                                    onChange={(e) => handleChange('body', e.target.value)}
                                    rows={16}
                                    className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 font-mono text-sm ${
                                        errors.body ? 'border-red-500' : 'border-gray-300'
                                    }`}
                                    placeholder="Email body content..."
                                    disabled={saving}
                                />
                                {errors.body && (
                                    <p className="text-sm text-red-600 mt-1">{errors.body}</p>
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
                                    rows={2}
                                    className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                                        errors.changeReason ? 'border-red-500' : 'border-gray-300'
                                    }`}
                                    placeholder="Explain why this change is being made..."
                                    disabled={saving}
                                />
                                {errors.changeReason && (
                                    <p className="text-sm text-red-600 mt-1">{errors.changeReason}</p>
                                )}
                            </div>
                        </div>

                        {/* Right Column - Variables and Actions */}
                        <div className="space-y-6">
                            {/* Available Variables */}
                            <div className="bg-gray-50 rounded-lg p-4">
                                <div className="flex items-center gap-2 mb-3">
                                    <Code className="w-4 h-4 text-gray-600" />
                                    <h3 className="text-sm font-medium text-gray-900">
                                        Available Variables
                                    </h3>
                                </div>
                                {variables.length > 0 ? (
                                    <div className="space-y-2">
                                        {variables.map((variable) => (
                                            <button
                                                key={variable}
                                                type="button"
                                                onClick={() => insertVariable(variable)}
                                                className="w-full text-left px-3 py-2 bg-white border border-gray-200 rounded hover:bg-gray-50 transition-colors"
                                            >
                                                <code className="text-xs text-blue-600">
                                                    {'{{ ' + variable + ' }}'}
                                                </code>
                                            </button>
                                        ))}
                                    </div>
                                ) : (
                                    <p className="text-sm text-gray-500">
                                        No variables detected in template
                                    </p>
                                )}
                            </div>

                            {/* Quick Actions */}
                            <div className="space-y-2">
                                <button
                                    type="button"
                                    onClick={handlePreview}
                                    className="w-full px-4 py-2 bg-white border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors flex items-center justify-center gap-2"
                                    disabled={saving}
                                >
                                    <Eye className="w-4 h-4" />
                                    <span>Preview</span>
                                </button>
                                <button
                                    type="button"
                                    onClick={() => onTestSend && onTestSend(template.templateId)}
                                    className="w-full px-4 py-2 bg-white border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors flex items-center justify-center gap-2"
                                    disabled={saving}
                                >
                                    <Send className="w-4 h-4" />
                                    <span>Send Test</span>
                                </button>
                            </div>

                            {/* Template Info */}
                            {template.description && (
                                <div className="bg-blue-50 rounded-lg p-4">
                                    <h4 className="text-sm font-medium text-blue-900 mb-2">
                                        Description
                                    </h4>
                                    <p className="text-sm text-blue-800">{template.description}</p>
                                </div>
                            )}
                        </div>
                    </div>

                    {/* Actions */}
                    <div className="flex items-center justify-end gap-3 pt-6 mt-6 border-t border-gray-200">
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

export default EmailTemplateEditor;
