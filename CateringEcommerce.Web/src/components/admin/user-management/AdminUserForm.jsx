import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import { X, AlertCircle, Info, Copy, CheckCircle, KeyRound } from 'lucide-react';
import { adminManagementApi, roleManagementApi } from '../../../services/adminApi';
import { toast } from 'react-hot-toast';
import RoleBadge from './RoleBadge';
import PermissionList from './PermissionList';

/**
 * AdminUserForm Component
 * Modal form for creating and editing admin users
 *
 * Props:
 * - mode: 'create' or 'edit'
 * - adminData: Admin object when editing (null when creating)
 * - roles: Array of available roles
 * - onSuccess: Callback when form submission succeeds
 * - onCancel: Callback when form is cancelled
 */
const AdminUserForm = ({ mode, adminData, roles, onSuccess, onCancel }) => {
    const [loading, setLoading] = useState(false);
    const [selectedRole, setSelectedRole] = useState(null);
    const [rolePermissions, setRolePermissions] = useState({});
    // Holds the one-time temp password shown after account creation
    const [tempPasswordToShow, setTempPasswordToShow] = useState(null);
    const [copied, setCopied] = useState(false);

    // Form state — no password fields; server auto-generates a temporary password
    const [formData, setFormData] = useState({
        username: '',
        email: '',
        fullName: '',
        mobile: '',
        roleId: '',
        isActive: true,
    });

    // Validation errors
    const [errors, setErrors] = useState({});

    // Initialize form data when editing
    useEffect(() => {
        if (mode === 'edit' && adminData) {
            setFormData({
                username: adminData.username || '',
                email: adminData.email || '',
                fullName: adminData.fullName || '',
                mobile: adminData.mobile || '',
                roleId: adminData.role?.roleId || '',
                isActive: adminData.isActive !== undefined ? adminData.isActive : true,
            });

            if (adminData.role) {
                fetchRolePermissions(adminData.role.roleId);
            }
        }
    }, [mode, adminData]);

    // Fetch permissions when role is selected
    const fetchRolePermissions = async (roleId) => {
        if (!roleId) {
            setRolePermissions({});
            setSelectedRole(null);
            return;
        }

        try {
            const response = await roleManagementApi.getRoleById(roleId);
            if (response.result && response.data) {
                setSelectedRole(response.data);

                // Group permissions by module
                const permissionsByModule = {};
                response.data.permissions.forEach(perm => {
                    if (!permissionsByModule[perm.module]) {
                        permissionsByModule[perm.module] = [];
                    }
                    permissionsByModule[perm.module].push(perm);
                });

                setRolePermissions(permissionsByModule);
            }
        } catch (error) {
            console.error('Error fetching role permissions:', error);
        }
    };

    const handleInputChange = (e) => {
        const { name, value, type, checked } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: type === 'checkbox' ? checked : value
        }));

        // Clear error for this field
        if (errors[name]) {
            setErrors(prev => ({ ...prev, [name]: '' }));
        }

        // Fetch role permissions when role changes
        if (name === 'roleId') {
            fetchRolePermissions(value);
        }
    };

    const validateForm = () => {
        const newErrors = {};

        // Username validation (only for create mode)
        if (mode === 'create' && !formData.username.trim()) {
            newErrors.username = 'Username is required';
        } else if (mode === 'create' && formData.username.length < 3) {
            newErrors.username = 'Username must be at least 3 characters';
        }

        // Email validation
        if (!formData.email.trim()) {
            newErrors.email = 'Email is required';
        } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
            newErrors.email = 'Invalid email format';
        }

        // Full name validation
        if (!formData.fullName.trim()) {
            newErrors.fullName = 'Full name is required';
        }

        // Mobile validation (optional)
        if (formData.mobile && !/^[0-9]{10}$/.test(formData.mobile.replace(/\D/g, ''))) {
            newErrors.mobile = 'Invalid mobile number (must be 10 digits)';
        }

        // Role validation
        if (!formData.roleId) {
            newErrors.roleId = 'Please select a role';
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (!validateForm()) {
            toast.error('Please fix the validation errors');
            return;
        }

        setLoading(true);

        try {
            let response;

            if (mode === 'create') {
                // Server auto-generates a secure temporary password — no password sent from client
                response = await adminManagementApi.createAdmin({
                    username: formData.username,
                    email: formData.email,
                    fullName: formData.fullName,
                    mobile: formData.mobile || null,
                    roleId: parseInt(formData.roleId),
                    isActive: formData.isActive,
                });
            } else {
                response = await adminManagementApi.updateAdmin(adminData.adminId, {
                    email: formData.email,
                    fullName: formData.fullName,
                    mobile: formData.mobile || null,
                    profilePhoto: null // Can be extended to support profile photo upload
                });
            }

            if (response.result) {
                if (mode === 'create' && response.data?.temporaryPassword) {
                    // Show the one-time temp password dialog before calling onSuccess
                    setTempPasswordToShow(response.data.temporaryPassword);
                } else {
                    toast.success('Admin user updated successfully');
                    onSuccess();
                }
            } else {
                toast.error(response.message || 'Operation failed');
            }
        } catch (error) {
            console.error('Error submitting form:', error);
            toast.error('Network error. Please try again.');
        } finally {
            setLoading(false);
        }
    };

    const generateUsername = () => {
        if (formData.fullName) {
            const username = formData.fullName.toLowerCase()
                .replace(/\s+/g, '.')
                .replace(/[^a-z0-9.]/g, '');
            setFormData(prev => ({ ...prev, username }));
        }
    };

    const handleCopyPassword = async () => {
        if (!tempPasswordToShow) return;
        try {
            await navigator.clipboard.writeText(tempPasswordToShow);
            setCopied(true);
            setTimeout(() => setCopied(false), 2000);
        } catch {
            toast.error('Unable to copy — please copy manually.');
        }
    };

    const handleTempPasswordAcknowledge = () => {
        setTempPasswordToShow(null);
        toast.success('Admin user created successfully');
        onSuccess();
    };

    // One-time temp password dialog shown after admin creation
    if (tempPasswordToShow) {
        return (
            <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
                <div className="bg-white rounded-xl shadow-2xl max-w-md w-full p-6">
                    <div className="flex items-center gap-3 mb-4">
                        <div className="bg-indigo-100 rounded-full p-2">
                            <KeyRound className="w-6 h-6 text-indigo-600" />
                        </div>
                        <div>
                            <h3 className="text-lg font-bold text-gray-900">Admin Account Created</h3>
                            <p className="text-sm text-gray-500">Share this temporary password with the new admin</p>
                        </div>
                    </div>

                    <div className="bg-amber-50 border border-amber-200 rounded-lg p-4 mb-4">
                        <p className="text-xs font-semibold text-amber-800 mb-2 uppercase tracking-wide">
                            Temporary Password — Shown Once
                        </p>
                        <div className="flex items-center gap-2">
                            <code className="flex-1 text-lg font-mono font-bold text-gray-900 bg-white border border-gray-200 rounded px-3 py-2 select-all break-all">
                                {tempPasswordToShow}
                            </code>
                            <button
                                type="button"
                                onClick={handleCopyPassword}
                                className="flex-shrink-0 p-2 rounded-lg bg-indigo-600 hover:bg-indigo-700 text-white transition-colors"
                                title="Copy to clipboard"
                            >
                                {copied ? <CheckCircle className="w-5 h-5" /> : <Copy className="w-5 h-5" />}
                            </button>
                        </div>
                    </div>

                    <div className="bg-red-50 border border-red-200 rounded-lg p-3 mb-5 text-sm text-red-800">
                        <strong>This password will not be shown again.</strong> The admin will be forced to
                        change it on their first login.
                    </div>

                    <button
                        type="button"
                        onClick={handleTempPasswordAcknowledge}
                        className="w-full py-2.5 bg-indigo-600 hover:bg-indigo-700 text-white font-semibold rounded-lg transition-colors"
                    >
                        I have copied the password — Done
                    </button>
                </div>
            </div>
        );
    }

    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-lg shadow-xl max-w-4xl w-full max-h-[90vh] overflow-y-auto">
                {/* Header */}
                <div className="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between">
                    <h2 className="text-xl font-bold text-gray-900">
                        {mode === 'create' ? 'Create New Admin User' : 'Edit Admin User'}
                    </h2>
                    <button
                        onClick={onCancel}
                        className="text-gray-400 hover:text-gray-600 transition-colors"
                    >
                        <X className="w-6 h-6" />
                    </button>
                </div>

                {/* Form */}
                <form onSubmit={handleSubmit} className="p-6 space-y-6">
                    {/* Basic Information Section */}
                    <div>
                        <h3 className="text-lg font-semibold text-gray-900 mb-4 border-b pb-2">
                            Basic Information
                        </h3>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            {/* Full Name */}
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">
                                    Full Name <span className="text-red-500">*</span>
                                </label>
                                <input
                                    type="text"
                                    name="fullName"
                                    value={formData.fullName}
                                    onChange={handleInputChange}
                                    className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${
                                        errors.fullName ? 'border-red-500' : 'border-gray-300'
                                    }`}
                                    placeholder="John Doe"
                                />
                                {errors.fullName && (
                                    <p className="text-red-500 text-xs mt-1 flex items-center gap-1">
                                        <AlertCircle className="w-3 h-3" />
                                        {errors.fullName}
                                    </p>
                                )}
                            </div>

                            {/* Email */}
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">
                                    Email <span className="text-red-500">*</span>
                                </label>
                                <input
                                    type="email"
                                    name="email"
                                    value={formData.email}
                                    onChange={handleInputChange}
                                    className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${
                                        errors.email ? 'border-red-500' : 'border-gray-300'
                                    }`}
                                    placeholder="john.doe@example.com"
                                />
                                {errors.email && (
                                    <p className="text-red-500 text-xs mt-1 flex items-center gap-1">
                                        <AlertCircle className="w-3 h-3" />
                                        {errors.email}
                                    </p>
                                )}
                            </div>

                            {/* Username (Create mode only) */}
                            {mode === 'create' && (
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">
                                        Username <span className="text-red-500">*</span>
                                    </label>
                                    <div className="flex gap-2">
                                        <input
                                            type="text"
                                            name="username"
                                            value={formData.username}
                                            onChange={handleInputChange}
                                            className={`flex-1 px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${
                                                errors.username ? 'border-red-500' : 'border-gray-300'
                                            }`}
                                            placeholder="john.doe"
                                        />
                                        <button
                                            type="button"
                                            onClick={generateUsername}
                                            className="px-3 py-2 text-sm bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors"
                                        >
                                            Auto-generate
                                        </button>
                                    </div>
                                    {errors.username && (
                                        <p className="text-red-500 text-xs mt-1 flex items-center gap-1">
                                            <AlertCircle className="w-3 h-3" />
                                            {errors.username}
                                        </p>
                                    )}
                                </div>
                            )}

                            {/* Mobile */}
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">
                                    Mobile Number
                                </label>
                                <input
                                    type="tel"
                                    name="mobile"
                                    value={formData.mobile}
                                    onChange={handleInputChange}
                                    className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${
                                        errors.mobile ? 'border-red-500' : 'border-gray-300'
                                    }`}
                                    placeholder="+1 (555) 123-4567"
                                />
                                {errors.mobile && (
                                    <p className="text-red-500 text-xs mt-1 flex items-center gap-1">
                                        <AlertCircle className="w-3 h-3" />
                                        {errors.mobile}
                                    </p>
                                )}
                            </div>
                        </div>
                    </div>

                    {/* Credentials notice (Create mode only) */}
                    {mode === 'create' && (
                        <div className="flex items-start gap-2 p-3 bg-indigo-50 border border-indigo-200 rounded-lg text-indigo-800 text-sm">
                            <Info className="w-4 h-4 mt-0.5 flex-shrink-0" />
                            <span>
                                A secure <strong>temporary password</strong> will be automatically generated and
                                shown to you after creation. The new admin must change it on their first login.
                            </span>
                        </div>
                    )}

                    {/* Role & Access Section */}
                    <div>
                        <h3 className="text-lg font-semibold text-gray-900 mb-4 border-b pb-2">
                            Role & Access
                        </h3>
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">
                                Admin Role <span className="text-red-500">*</span>
                            </label>
                            <select
                                name="roleId"
                                value={formData.roleId}
                                onChange={handleInputChange}
                                disabled={mode === 'edit'} // Cannot change role in edit mode
                                className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${
                                    errors.roleId ? 'border-red-500' : 'border-gray-300'
                                } ${mode === 'edit' ? 'bg-gray-100 cursor-not-allowed' : ''}`}
                            >
                                <option value="">Select a role...</option>
                                {roles.map(role => (
                                    <option key={role.roleId} value={role.roleId}>
                                        {role.roleName}
                                    </option>
                                ))}
                            </select>
                            {errors.roleId && (
                                <p className="text-red-500 text-xs mt-1 flex items-center gap-1">
                                    <AlertCircle className="w-3 h-3" />
                                    {errors.roleId}
                                </p>
                            )}
                            {mode === 'edit' && (
                                <p className="text-xs text-gray-500 mt-1 flex items-center gap-1">
                                    <Info className="w-3 h-3" />
                                    Role cannot be changed in edit mode. Use "Assign Role" action instead.
                                </p>
                            )}
                        </div>

                        {/* Display role permissions */}
                        {selectedRole && Object.keys(rolePermissions).length > 0 && (
                            <div className="mt-4 p-4 bg-gray-50 rounded-lg border border-gray-200">
                                <h4 className="text-sm font-semibold text-gray-700 mb-3 flex items-center gap-2">
                                    <RoleBadge roleName={selectedRole.roleName} roleColor={selectedRole.color} />
                                    Permissions
                                </h4>
                                <PermissionList permissions={rolePermissions} />
                            </div>
                        )}
                    </div>

                    {/* Status & Control Section */}
                    <div>
                        <h3 className="text-lg font-semibold text-gray-900 mb-4 border-b pb-2">
                            Status & Control
                        </h3>
                        <div>
                            <label className="flex items-center gap-2">
                                <input
                                    type="checkbox"
                                    name="isActive"
                                    checked={formData.isActive}
                                    onChange={handleInputChange}
                                />
                                <div>
                                    <span className="text-sm font-medium text-gray-700">
                                        Account Active
                                    </span>
                                    <p className="text-xs text-gray-500">
                                        Inactive accounts cannot login to the admin panel
                                    </p>
                                </div>
                            </label>
                        </div>
                    </div>

                    {/* Form Actions */}
                    <div className="flex justify-end gap-3 pt-4 border-t border-gray-200">
                        <button
                            type="button"
                            onClick={onCancel}
                            className="px-6 py-2 text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
                            disabled={loading}
                        >
                            Cancel
                        </button>
                        <button
                            type="submit"
                            className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                            disabled={loading}
                        >
                            {loading ? (
                                <span className="flex items-center gap-2">
                                    <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                                    Saving...
                                </span>
                            ) : (
                                mode === 'create' ? 'Create Admin User' : 'Save Changes'
                            )}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};

AdminUserForm.propTypes = {
    mode: PropTypes.oneOf(['create', 'edit']).isRequired,
    adminData: PropTypes.object,
    roles: PropTypes.array.isRequired,
    onSuccess: PropTypes.func.isRequired,
    onCancel: PropTypes.func.isRequired,
};

export default AdminUserForm;
