import { useState, useEffect } from 'react';
import { Search, Filter, RefreshCw, Edit2, Mail } from 'lucide-react';
import EmailTemplateEditor from './EmailTemplateEditor';
import { emailTemplateApi } from '../../../services/settingsApi';
import { toast } from 'react-hot-toast';

/**
 * Email Templates Tab Component
 * View and edit email templates
 */
const EmailTemplatesTab = () => {
    const [templates, setTemplates] = useState([]);
    const [loading, setLoading] = useState(false);
    const [selectedTemplate, setSelectedTemplate] = useState(null);
    const [showEditor, setShowEditor] = useState(false);
    const [saving, setSaving] = useState(false);

    // Filters
    const [filters, setFilters] = useState({
        category: '',
        searchTerm: '',
        isActive: null,
        pageNumber: 1,
        pageSize: 50,
        sortBy: 'TemplateName',
        sortOrder: 'ASC',
    });

    const categories = ['All', 'USER', 'OWNER', 'ADMIN'];

    useEffect(() => {
        fetchTemplates();
    }, [filters]);

    const fetchTemplates = async () => {
        setLoading(true);
        try {
            const apiFilters = {
                ...filters,
                category: filters.category === 'All' || filters.category === '' ? null : filters.category,
            };

            const response = await emailTemplateApi.getEmailTemplates(apiFilters);

            if (response.result) {
                setTemplates(response.data.templates || []);
            } else {
                toast.error('Failed to load email templates');
            }
        } catch (error) {
            console.error('Error fetching templates:', error);
            toast.error('Failed to load email templates');
        } finally {
            setLoading(false);
        }
    };

    const handleEdit = (template) => {
        setSelectedTemplate(template);
        setShowEditor(true);
    };

    const handleSave = async (templateId, subject, body, changeReason) => {
        setSaving(true);
        try {
            const response = await emailTemplateApi.updateEmailTemplate(
                templateId,
                subject,
                body,
                changeReason
            );

            if (response.result) {
                toast.success('Email template updated successfully');
                setShowEditor(false);
                fetchTemplates();
            } else {
                toast.error(response.message || 'Failed to update email template');
            }
        } catch (error) {
            console.error('Error updating template:', error);
            toast.error('Failed to update email template');
        } finally {
            setSaving(false);
        }
    };

    const handlePreview = async (templateId, subject, body) => {
        try {
            const response = await emailTemplateApi.previewTemplate(
                templateId,
                null,
                subject,
                body,
                {}
            );

            if (response.result) {
                toast.success('Preview generated');
                // You could show preview in a modal here
                console.log('Preview:', response.data);
            } else {
                toast.error('Failed to generate preview');
            }
        } catch (error) {
            console.error('Error generating preview:', error);
            toast.error('Failed to generate preview');
        }
    };

    const handleTestSend = async (templateId) => {
        const email = prompt('Enter email address to send test:');
        if (!email) return;

        try {
            const response = await emailTemplateApi.sendTestEmail(templateId, email);

            if (response.result) {
                toast.success(`Test email sent to ${email}`);
            } else {
                toast.error(response.message || 'Failed to send test email');
            }
        } catch (error) {
            console.error('Error sending test email:', error);
            toast.error('Failed to send test email');
        }
    };

    const handleCategoryChange = (category) => {
        setFilters((prev) => ({ ...prev, category: category === 'All' ? '' : category, pageNumber: 1 }));
    };

    const handleSearchChange = (searchTerm) => {
        setFilters((prev) => ({ ...prev, searchTerm, pageNumber: 1 }));
    };

    const handleRefresh = () => {
        fetchTemplates();
        toast.success('Email templates refreshed');
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
                    {/* Search */}
                    <div className="relative flex-1 max-w-md">
                        <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
                        <input
                            type="text"
                            value={filters.searchTerm}
                            onChange={(e) => handleSearchChange(e.target.value)}
                            placeholder="Search templates..."
                            className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                        />
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
                                onClick={() => handleCategoryChange(category)}
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

            {/* Templates Grid */}
            {templates.length === 0 ? (
                <div className="text-center py-12 bg-gray-50 rounded-lg">
                    <Mail className="w-16 h-16 text-gray-300 mx-auto mb-4" />
                    <p className="text-gray-500">No email templates found</p>
                </div>
            ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                    {templates.map((template) => (
                        <div
                            key={template.templateId}
                            className="bg-white border border-gray-200 rounded-lg p-6 hover:shadow-lg transition-shadow"
                        >
                            {/* Template Header */}
                            <div className="flex items-start justify-between mb-4">
                                <div className="flex-1">
                                    <h3 className="text-lg font-semibold text-gray-900 mb-1">
                                        {template.templateName}
                                    </h3>
                                    <code className="text-xs text-gray-500 bg-gray-100 px-2 py-1 rounded">
                                        {template.templateCode}
                                    </code>
                                </div>
                                <span
                                    className={`px-2 py-1 text-xs font-medium rounded ${
                                        template.isActive
                                            ? 'bg-green-100 text-green-800'
                                            : 'bg-gray-100 text-gray-800'
                                    }`}
                                >
                                    {template.isActive ? 'Active' : 'Inactive'}
                                </span>
                            </div>

                            {/* Template Info */}
                            <div className="space-y-3 mb-4">
                                <div>
                                    <p className="text-xs text-gray-500 mb-1">Category</p>
                                    <span className="px-2 py-1 text-xs font-medium bg-blue-100 text-blue-800 rounded">
                                        {template.category}
                                    </span>
                                </div>
                                <div>
                                    <p className="text-xs text-gray-500 mb-1">Subject</p>
                                    <p className="text-sm text-gray-900 line-clamp-2">
                                        {template.subject}
                                    </p>
                                </div>
                                {template.description && (
                                    <div>
                                        <p className="text-xs text-gray-500 mb-1">Description</p>
                                        <p className="text-sm text-gray-700 line-clamp-2">
                                            {template.description}
                                        </p>
                                    </div>
                                )}
                            </div>

                            {/* Template Metadata */}
                            <div className="border-t border-gray-200 pt-3 mb-4">
                                <div className="flex items-center justify-between text-xs text-gray-500">
                                    <span>Version {template.version}</span>
                                    {template.modifiedDate && (
                                        <span title={formatDate(template.modifiedDate)}>
                                            Updated {new Date(template.modifiedDate).toLocaleDateString()}
                                        </span>
                                    )}
                                </div>
                                {template.modifiedByName && (
                                    <p className="text-xs text-gray-500 mt-1">
                                        by {template.modifiedByName}
                                    </p>
                                )}
                            </div>

                            {/* Actions */}
                            <div className="flex items-center gap-2">
                                <button
                                    onClick={() => handleEdit(template)}
                                    className="flex-1 px-3 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors flex items-center justify-center gap-2 text-sm"
                                >
                                    <Edit2 className="w-4 h-4" />
                                    <span>Edit</span>
                                </button>
                                <button
                                    onClick={() => handleTestSend(template.templateId)}
                                    className="px-3 py-2 bg-white border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors text-sm"
                                    title="Send Test Email"
                                >
                                    <Mail className="w-4 h-4" />
                                </button>
                            </div>
                        </div>
                    ))}
                </div>
            )}

            {/* Template Editor */}
            <EmailTemplateEditor
                template={selectedTemplate}
                isOpen={showEditor}
                onClose={() => setShowEditor(false)}
                onSave={handleSave}
                onPreview={handlePreview}
                onTestSend={handleTestSend}
                saving={saving}
            />
        </div>
    );
};

export default EmailTemplatesTab;
