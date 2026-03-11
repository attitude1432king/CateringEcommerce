import { useState, useEffect, useRef, useCallback } from 'react';
import { X, Save, Eye, Send, Code, ChevronDown, Plus } from 'lucide-react';
import { emailTemplateApi } from '../../../services/settingsApi';
import { toast } from 'react-hot-toast';

const CHANNELS = ['EMAIL', 'SMS', 'INAPP'];
const CATEGORIES = ['USER', 'OWNER', 'ADMIN', 'SUPERVISOR'];
const LANGUAGES = [
    { value: 'en', label: 'English' },
    { value: 'hi', label: 'Hindi' },
    { value: 'mr', label: 'Marathi' },
];

const TEMPLATE_VARIABLES = [
    { key: 'UserName', label: 'User Name' },
    { key: 'OrderId', label: 'Order ID' },
    { key: 'CompanyName', label: 'Company Name' },
    { key: 'EventDate', label: 'Event Date' },
    { key: 'CustomerName', label: 'Customer Name' },
    { key: 'PartnerName', label: 'Partner Name' },
    { key: 'Amount', label: 'Amount' },
    { key: 'SupportEmail', label: 'Support Email' },
    { key: 'AppName', label: 'App Name' },
    { key: 'OTPCode', label: 'OTP Code' },
    { key: 'ResetLink', label: 'Reset Link' },
    { key: 'EventName', label: 'Event Name' },
    { key: 'VenueName', label: 'Venue Name' },
    { key: 'GuestCount', label: 'Guest Count' },
    { key: 'BookingDate', label: 'Booking Date' },
    { key: 'InvoiceNumber', label: 'Invoice Number' },
    { key: 'PaymentStatus', label: 'Payment Status' },
    { key: 'TrackingLink', label: 'Tracking Link' },
    { key: 'SupervisorName', label: 'Supervisor Name' },
    { key: 'SupervisorEmail', label: 'Supervisor Email' },
    { key: 'SupervisorPhone', label: 'Supervisor Phone' },
    { key: 'SupervisorStatus', label: 'Supervisor Status' },
    { key: 'EventLocation', label: 'Event Location' },
    { key: 'ClientName', label: 'Client Name' },
    { key: 'MonitoringStartTime', label: 'Monitoring Start Time' },
    { key: 'MonitoringEndTime', label: 'Monitoring End Time' },
];

const EmailTemplateEditor = ({ template, isOpen, onClose, onSaved, saving, setSaving, mode = 'edit' }) => {
    const isAddMode = mode === 'add';

    const [formData, setFormData] = useState({
        templateCode: '',
        templateName: '',
        description: '',
        language: 'en',
        channel: 'EMAIL',
        category: '',
        subject: '',
        body: '',
        isActive: true,
        changeReason: '',
    });
    const [errors, setErrors] = useState({});
    const [variableDropdownOpen, setVariableDropdownOpen] = useState(false);
    const [variableTarget, setVariableTarget] = useState(null);

    const subjectRef = useRef(null);
    const bodyRef = useRef(null);
    const subjectCursorRef = useRef(0);
    const bodyCursorRef = useRef(0);
    const variableDropdownRef = useRef(null);

    useEffect(() => {
        if (isAddMode) {
            setFormData({
                templateCode: '',
                templateName: '',
                description: '',
                language: 'en',
                channel: 'EMAIL',
                category: '',
                subject: '',
                body: '',
                isActive: true,
                changeReason: '',
            });
            setErrors({});
        } else if (template) {
            setFormData({
                templateCode: template.templateCode || '',
                templateName: template.templateName || '',
                description: template.description || '',
                language: template.language || 'en',
                channel: template.channel || 'EMAIL',
                category: template.category || '',
                subject: template.subject || '',
                body: template.body || '',
                isActive: template.isActive ?? true,
                changeReason: '',
            });
            setErrors({});
        }
    }, [template, isAddMode, isOpen]);

    useEffect(() => {
        const handleClickOutside = (e) => {
            if (variableDropdownRef.current && !variableDropdownRef.current.contains(e.target)) {
                setVariableDropdownOpen(false);
            }
        };
        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, []);

    const trackCursor = useCallback((field) => {
        if (field === 'subject' && subjectRef.current) {
            subjectCursorRef.current = subjectRef.current.selectionStart;
        } else if (field === 'body' && bodyRef.current) {
            bodyCursorRef.current = bodyRef.current.selectionStart;
        }
    }, []);

    const validateForm = () => {
        const newErrors = {};

        if (isAddMode && !formData.templateCode.trim()) {
            newErrors.templateCode = 'Template code is required';
        }

        if (!formData.templateName.trim()) {
            newErrors.templateName = 'Template name is required';
        }

        if (!formData.channel) {
            newErrors.channel = 'Channel is required';
        }

        if (!formData.category) {
            newErrors.category = 'Category is required';
        }

        if (formData.channel === 'EMAIL' && !formData.subject.trim()) {
            newErrors.subject = 'Subject is required for EMAIL channel';
        }

        if (!formData.body.trim()) {
            newErrors.body = 'Body is required';
        }

        if (!isAddMode && !formData.changeReason.trim()) {
            newErrors.changeReason = 'Change reason is required';
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!validateForm()) return;

        setSaving(true);
        try {
            if (isAddMode) {
                const response = await emailTemplateApi.createEmailTemplate({
                    templateCode: formData.templateCode.trim(),
                    templateName: formData.templateName.trim(),
                    description: formData.description.trim() || null,
                    language: formData.language,
                    channel: formData.channel,
                    category: formData.category,
                    subject: formData.subject.trim() || null,
                    body: formData.body.trim(),
                    isActive: formData.isActive,
                });
                if (response.result) {
                    toast.success('Email template created successfully');
                    onSaved();
                } else {
                    toast.error(response.message || 'Failed to create email template');
                }
            } else {
                const response = await emailTemplateApi.updateEmailTemplate(template.templateId, {
                    templateName: formData.templateName.trim(),
                    description: formData.description.trim() || null,
                    category: formData.category,
                    subject: formData.subject.trim() || null,
                    body: formData.body.trim(),
                    isActive: formData.isActive,
                    changeReason: formData.changeReason.trim(),
                });
                if (response.result) {
                    toast.success('Email template updated successfully');
                    onSaved();
                } else {
                    toast.error(response.message || 'Failed to update email template');
                }
            }
        } catch (error) {
            toast.error(error.message || `Failed to ${isAddMode ? 'create' : 'update'} email template`);
        } finally {
            setSaving(false);
        }
    };

    const handleChange = (field, value) => {
        setFormData((prev) => ({ ...prev, [field]: value }));
        if (errors[field]) {
            setErrors((prev) => ({ ...prev, [field]: '' }));
        }
    };

    const openVariableSelector = (targetField) => {
        trackCursor(targetField);
        setVariableTarget(targetField);
        setVariableDropdownOpen(true);
    };

    const insertVariable = (variableKey) => {
        const token = `@${variableKey}`;
        const field = variableTarget;

        if (field === 'subject') {
            const pos = subjectCursorRef.current;
            const text = formData.subject;
            const newText = text.substring(0, pos) + token + text.substring(pos);
            handleChange('subject', newText);
            setTimeout(() => {
                if (subjectRef.current) {
                    const newPos = pos + token.length;
                    subjectRef.current.focus();
                    subjectRef.current.setSelectionRange(newPos, newPos);
                    subjectCursorRef.current = newPos;
                }
            }, 0);
        } else if (field === 'body') {
            const pos = bodyCursorRef.current;
            const text = formData.body;
            const newText = text.substring(0, pos) + token + text.substring(pos);
            handleChange('body', newText);
            setTimeout(() => {
                if (bodyRef.current) {
                    const newPos = pos + token.length;
                    bodyRef.current.focus();
                    bodyRef.current.setSelectionRange(newPos, newPos);
                    bodyCursorRef.current = newPos;
                }
            }, 0);
        }

        setVariableDropdownOpen(false);
    };

    const handlePreview = async () => {
        try {
            const response = await emailTemplateApi.previewTemplate(
                isAddMode ? null : template?.templateId,
                null,
                formData.subject,
                formData.body,
                {}
            );
            if (response.result) {
                toast.success('Preview generated - check console');
            } else {
                toast.error('Failed to generate preview');
            }
        } catch {
            toast.error('Failed to generate preview');
        }
    };

    const handleTestSend = async () => {
        if (isAddMode || !template?.templateId) return;
        const email = prompt('Enter email address to send test:');
        if (!email) return;
        try {
            const response = await emailTemplateApi.sendTestEmail(template.templateId, email);
            if (response.result) {
                toast.success(`Test email sent to ${email}`);
            } else {
                toast.error(response.message || 'Failed to send test email');
            }
        } catch {
            toast.error('Failed to send test email');
        }
    };

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4">
            <div className="bg-white rounded-lg shadow-xl max-w-5xl w-full max-h-[90vh] overflow-y-auto">
                {/* Header */}
                <div className="flex items-center justify-between p-6 border-b border-gray-200">
                    <div>
                        <h2 className="text-xl font-bold text-gray-900">
                            {isAddMode ? 'Add Email Template' : 'Edit Email Template'}
                        </h2>
                        {!isAddMode && template && (
                            <p className="text-sm text-gray-600 mt-1">
                                {template.templateName} ({template.templateCode})
                            </p>
                        )}
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
                        {/* Left Column - Main Fields */}
                        <div className="col-span-2 space-y-5">
                            {/* Row 1: Template Code + Template Name */}
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">
                                        Template Code *
                                    </label>
                                    <input
                                        type="text"
                                        value={formData.templateCode}
                                        onChange={(e) => handleChange('templateCode', e.target.value.toUpperCase().replace(/[^A-Z0-9_]/g, ''))}
                                        className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${errors.templateCode ? 'border-red-500' : 'border-gray-300'
                                            } ${!isAddMode ? 'bg-gray-100 cursor-not-allowed' : ''}`}
                                        placeholder="e.g. USER_WELCOME_EMAIL"
                                        disabled={saving || !isAddMode}
                                        readOnly={!isAddMode}
                                    />
                                    {errors.templateCode && (
                                        <p className="text-xs text-red-600 mt-1">{errors.templateCode}</p>
                                    )}
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">
                                        Template Name *
                                    </label>
                                    <input
                                        type="text"
                                        value={formData.templateName}
                                        onChange={(e) => handleChange('templateName', e.target.value)}
                                        className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${errors.templateName ? 'border-red-500' : 'border-gray-300'
                                            }`}
                                        placeholder="Welcome Email"
                                        disabled={saving}
                                    />
                                    {errors.templateName && (
                                        <p className="text-xs text-red-600 mt-1">{errors.templateName}</p>
                                    )}
                                </div>
                            </div>

                            {/* Row 2: Channel, Category, Language */}
                            <div className="grid grid-cols-3 gap-4">
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">
                                        Channel *
                                    </label>
                                    <select
                                        value={formData.channel}
                                        onChange={(e) => handleChange('channel', e.target.value)}
                                        className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${errors.channel ? 'border-red-500' : 'border-gray-300'
                                            } ${!isAddMode ? 'bg-gray-100 cursor-not-allowed' : ''}`}
                                        disabled={saving || !isAddMode}
                                    >
                                        {CHANNELS.map((ch) => (
                                            <option key={ch} value={ch}>{ch}</option>
                                        ))}
                                    </select>
                                    {errors.channel && (
                                        <p className="text-xs text-red-600 mt-1">{errors.channel}</p>
                                    )}
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">
                                        Category *
                                    </label>
                                    <select
                                        value={formData.category}
                                        onChange={(e) => handleChange('category', e.target.value)}
                                        className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${errors.category ? 'border-red-500' : 'border-gray-300'
                                            }`}
                                        disabled={saving}
                                    >
                                        <option value="">Select Category</option>
                                        {CATEGORIES.map((cat) => (
                                            <option key={cat} value={cat}>{cat}</option>
                                        ))}
                                    </select>
                                    {errors.category && (
                                        <p className="text-xs text-red-600 mt-1">{errors.category}</p>
                                    )}
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">
                                        Language
                                    </label>
                                    <select
                                        value={formData.language}
                                        onChange={(e) => handleChange('language', e.target.value)}
                                        className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 border-gray-300 ${!isAddMode ? 'bg-gray-100 cursor-not-allowed' : ''
                                            }`}
                                        disabled={saving || !isAddMode}
                                    >
                                        {LANGUAGES.map((lang) => (
                                            <option key={lang.value} value={lang.value}>{lang.label}</option>
                                        ))}
                                    </select>
                                </div>
                            </div>

                            {/* Description */}
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">
                                    Description
                                </label>
                                <input
                                    type="text"
                                    value={formData.description}
                                    onChange={(e) => handleChange('description', e.target.value)}
                                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                                    placeholder="Brief description of this template"
                                    disabled={saving}
                                />
                            </div>

                            {/* Subject */}
                            <div>
                                <div className="flex items-center justify-between mb-1">
                                    <label className="block text-sm font-medium text-gray-700">
                                        Subject {formData.channel === 'EMAIL' ? '*' : ''}
                                    </label>
                                    <button
                                        type="button"
                                        onClick={() => openVariableSelector('subject')}
                                        className="text-xs text-blue-600 hover:text-blue-800 flex items-center gap-1"
                                        disabled={saving}
                                    >
                                        <Code className="w-3 h-3" />
                                        Insert @Variable
                                    </button>
                                </div>
                                <input
                                    ref={subjectRef}
                                    type="text"
                                    value={formData.subject}
                                    onChange={(e) => handleChange('subject', e.target.value)}
                                    onSelect={() => trackCursor('subject')}
                                    onClick={() => trackCursor('subject')}
                                    onKeyUp={() => trackCursor('subject')}
                                    className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${errors.subject ? 'border-red-500' : 'border-gray-300'
                                        }`}
                                    placeholder="Email subject line"
                                    disabled={saving}
                                />
                                {errors.subject && (
                                    <p className="text-xs text-red-600 mt-1">{errors.subject}</p>
                                )}
                            </div>

                            {/* Body */}
                            <div>
                                <div className="flex items-center justify-between mb-1">
                                    <label className="block text-sm font-medium text-gray-700">
                                        Body *
                                    </label>
                                    <button
                                        type="button"
                                        onClick={() => openVariableSelector('body')}
                                        className="text-xs text-blue-600 hover:text-blue-800 flex items-center gap-1"
                                        disabled={saving}
                                    >
                                        <Code className="w-3 h-3" />
                                        Insert @Variable
                                    </button>
                                </div>
                                <textarea
                                    ref={bodyRef}
                                    value={formData.body}
                                    onChange={(e) => handleChange('body', e.target.value)}
                                    onSelect={() => trackCursor('body')}
                                    onClick={() => trackCursor('body')}
                                    onKeyUp={() => trackCursor('body')}
                                    rows={14}
                                    className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 font-mono text-sm ${errors.body ? 'border-red-500' : 'border-gray-300'
                                        }`}
                                    placeholder="Email body content..."
                                    disabled={saving}
                                />
                                {errors.body && (
                                    <p className="text-xs text-red-600 mt-1">{errors.body}</p>
                                )}
                            </div>

                            {/* Status Toggle */}
                            <div className="flex items-center gap-3">
                                <label className="text-sm font-medium text-gray-700">Status:</label>
                                <button
                                    type="button"
                                    onClick={() => handleChange('isActive', !formData.isActive)}
                                    className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${formData.isActive ? 'bg-green-500' : 'bg-gray-300'
                                        }`}
                                    disabled={saving}
                                >
                                    <span
                                        className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${formData.isActive ? 'translate-x-6' : 'translate-x-1'
                                            }`}
                                    />
                                </button>
                                <span className={`text-sm font-medium ${formData.isActive ? 'text-green-700' : 'text-gray-500'}`}>
                                    {formData.isActive ? 'Active' : 'Inactive'}
                                </span>
                            </div>

                            {/* Change Reason (Edit only) */}
                            {!isAddMode && (
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">
                                        Change Reason *
                                    </label>
                                    <textarea
                                        value={formData.changeReason}
                                        onChange={(e) => handleChange('changeReason', e.target.value)}
                                        rows={2}
                                        className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${errors.changeReason ? 'border-red-500' : 'border-gray-300'
                                            }`}
                                        placeholder="Explain why this change is being made..."
                                        disabled={saving}
                                    />
                                    {errors.changeReason && (
                                        <p className="text-xs text-red-600 mt-1">{errors.changeReason}</p>
                                    )}
                                </div>
                            )}
                        </div>

                        {/* Right Column - Variables & Metadata */}
                        <div className="space-y-5">
                            {/* Variable Selector */}
                            <div className="bg-gray-50 rounded-lg p-4" ref={variableDropdownRef}>
                                <div className="flex items-center gap-2 mb-3">
                                    <Code className="w-4 h-4 text-gray-600" />
                                    <h3 className="text-sm font-medium text-gray-900">
                                        Template Variables
                                    </h3>
                                </div>
                                <p className="text-xs text-gray-500 mb-3">
                                    Click a variable to insert at cursor position in Subject or Body field.
                                </p>
                                <div className="relative mb-3">
                                    <button
                                        type="button"
                                        onClick={() => {
                                            if (!variableTarget) {
                                                setVariableTarget('body');
                                            }
                                            setVariableDropdownOpen(!variableDropdownOpen);
                                        }}
                                        className="w-full px-3 py-2 bg-white border border-gray-300 rounded-lg text-sm text-left flex items-center justify-between hover:bg-gray-50"
                                        disabled={saving}
                                    >
                                        <span className="text-gray-700">Select Variable</span>
                                        <ChevronDown className="w-4 h-4 text-gray-400" />
                                    </button>
                                    {variableDropdownOpen && (
                                        <div className="absolute z-10 mt-1 w-full bg-white border border-gray-200 rounded-lg shadow-lg max-h-60 overflow-y-auto">
                                            {TEMPLATE_VARIABLES.map((v) => (
                                                <button
                                                    key={v.key}
                                                    type="button"
                                                    onClick={() => insertVariable(v.key)}
                                                    className="w-full text-left px-3 py-2 hover:bg-blue-50 flex items-center justify-between text-sm border-b border-gray-50 last:border-b-0"
                                                >
                                                    <span className="text-blue-600 font-mono">@{v.key}</span>
                                                    <span className="text-gray-400 text-xs">{v.label}</span>
                                                </button>
                                            ))}
                                        </div>
                                    )}
                                </div>
                                <p className="text-xs text-gray-400">
                                    Target: <span className="font-medium text-gray-600">{variableTarget === 'subject' ? 'Subject' : 'Body'}</span>
                                </p>
                            </div>

                            {/* Quick Actions (Edit mode only) */}
                            {!isAddMode && (
                                <div className="space-y-2">
                                    <button
                                        type="button"
                                        onClick={handlePreview}
                                        className="w-full px-4 py-2 bg-white border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors flex items-center justify-center gap-2 text-sm"
                                        disabled={saving}
                                    >
                                        <Eye className="w-4 h-4" />
                                        <span>Preview</span>
                                    </button>
                                    <button
                                        type="button"
                                        onClick={handleTestSend}
                                        className="w-full px-4 py-2 bg-white border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors flex items-center justify-center gap-2 text-sm"
                                        disabled={saving}
                                    >
                                        <Send className="w-4 h-4" />
                                        <span>Send Test</span>
                                    </button>
                                </div>
                            )}

                            {/* Read-Only Metadata (Edit mode only) */}
                            {!isAddMode && template && (
                                <div className="bg-gray-50 rounded-lg p-4 space-y-3">
                                    <h3 className="text-sm font-medium text-gray-900 mb-2">Template Info</h3>
                                    <div className="text-xs space-y-2">
                                        <div className="flex justify-between">
                                            <span className="text-gray-500">Version</span>
                                            <span className="font-medium text-gray-900">{template.version}</span>
                                        </div>
                                        <div className="flex justify-between">
                                            <span className="text-gray-500">Channel</span>
                                            <span className="font-medium text-gray-900">{template.channel}</span>
                                        </div>
                                        <div className="flex justify-between">
                                            <span className="text-gray-500">Language</span>
                                            <span className="font-medium text-gray-900">{template.language}</span>
                                        </div>
                                        <div className="flex justify-between">
                                            <span className="text-gray-500">Usage Count</span>
                                            <span className="font-medium text-gray-900">{template.usageCount ?? 0}</span>
                                        </div>
                                        <div className="flex justify-between">
                                            <span className="text-gray-500">Created</span>
                                            <span className="font-medium text-gray-900">
                                                {template.createdDate ? new Date(template.createdDate).toLocaleDateString() : '-'}
                                            </span>
                                        </div>
                                        {template.modifiedDate && (
                                            <div className="flex justify-between">
                                                <span className="text-gray-500">Modified</span>
                                                <span className="font-medium text-gray-900">
                                                    {new Date(template.modifiedDate).toLocaleDateString()}
                                                </span>
                                            </div>
                                        )}
                                        {template.modifiedByName && (
                                            <div className="flex justify-between">
                                                <span className="text-gray-500">Modified By</span>
                                                <span className="font-medium text-gray-900">{template.modifiedByName}</span>
                                            </div>
                                        )}
                                    </div>
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
                                    <span>{isAddMode ? 'Creating...' : 'Saving...'}</span>
                                </>
                            ) : (
                                <>
                                    {isAddMode ? <Plus className="w-4 h-4" /> : <Save className="w-4 h-4" />}
                                    <span>{isAddMode ? 'Create Template' : 'Save Changes'}</span>
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
