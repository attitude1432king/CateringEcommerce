import React, { useState } from 'react';
import PropTypes from 'prop-types';
import { X, Shield, AlertCircle, Info } from 'lucide-react';
import { adminManagementApi } from '../../../services/adminApi';
import { toast } from 'react-hot-toast';
import RoleBadge from '../user-management/RoleBadge';
import { useConfirmation } from '../../../contexts/ConfirmationContext'; // P2 FIX: Replace window.confirm

/**
 * RoleAssignmentModal Component
 * Modal for assigning/changing roles for existing admin users
 *
 * Props:
 * - admin: Admin user object with current role
 * - roles: Array of available roles
 * - onSuccess: Callback when role is successfully assigned
 * - onCancel: Callback when modal is closed
 */
const RoleAssignmentModal = ({ admin, roles, onSuccess, onCancel }) => {
    const confirm = useConfirmation(); // P2 FIX: Use confirmation hook
    const [selectedRoleId, setSelectedRoleId] = useState(admin.role?.roleId || '');
    const [loading, setLoading] = useState(false);

    const handleAssignRole = async () => {
        if (!selectedRoleId) {
            toast.error('Please select a role');
            return;
        }

        if (selectedRoleId === admin.role?.roleId) {
            toast.error('This user already has this role');
            return;
        }

        const selectedRole = roles.find(r => r.roleId === parseInt(selectedRoleId));
        if (!selectedRole) {
            toast.error('Invalid role selected');
            return;
        }

        // P2 FIX: Replace window.confirm with confirmation context
        const isSuperAdmin = selectedRole.roleCode === 'SUPER_ADMIN';
        const confirmed = await confirm({
            title: isSuperAdmin ? '⚠️ Assign Super Admin Role' : 'Change Role',
            message: isSuperAdmin
                ? `WARNING: You are about to assign the SUPER ADMIN role to ${admin.fullName}. This will give them full system access. Are you absolutely sure?`
                : `Are you sure you want to change ${admin.fullName}'s role to ${selectedRole.roleName}?`,
            type: isSuperAdmin ? 'warning' : 'info',
            confirmText: isSuperAdmin ? 'Yes, Assign Super Admin' : 'Change Role',
            cancelText: 'Cancel'
        });

        if (!confirmed) {
            return;
        }

        setLoading(true);

        try {
            const response = await adminManagementApi.assignRole(admin.adminId, {
                roleId: parseInt(selectedRoleId)
            });

            if (response.result) {
                toast.success(`Role assigned successfully to ${admin.fullName}`);
                onSuccess();
            } else {
                toast.error(response.message || 'Failed to assign role');
            }
        } catch (error) {
            console.error('Error assigning role:', error);

            // Handle specific error cases
            if (error.response?.status === 403) {
                toast.error('You do not have permission to assign this role');
            } else {
                toast.error('Network error. Please try again.');
            }
        } finally {
            setLoading(false);
        }
    };

    const selectedRole = roles.find(r => r.roleId === parseInt(selectedRoleId));
    const currentRole = admin.role;

    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full">
                {/* Header */}
                <div className="bg-gradient-to-r from-blue-600 to-blue-700 px-6 py-4 flex items-center justify-between rounded-t-lg">
                    <div className="flex items-center gap-3">
                        <Shield className="w-6 h-6 text-white" />
                        <h2 className="text-xl font-bold text-white">
                            Assign Role
                        </h2>
                    </div>
                    <button
                        onClick={onCancel}
                        className="text-white hover:text-gray-200 transition-colors"
                    >
                        <X className="w-6 h-6" />
                    </button>
                </div>

                {/* Content */}
                <div className="p-6 space-y-6">
                    {/* Admin Info */}
                    <div className="bg-gray-50 p-4 rounded-lg border border-gray-200">
                        <div className="flex items-center justify-between">
                            <div>
                                <div className="text-sm text-gray-600">Assigning role to:</div>
                                <div className="text-lg font-semibold text-gray-900 mt-1">
                                    {admin.fullName}
                                </div>
                                <div className="text-sm text-gray-500">{admin.email}</div>
                            </div>
                            <div>
                                <div className="text-sm text-gray-600 mb-1">Current Role:</div>
                                {currentRole ? (
                                    <RoleBadge
                                        roleName={currentRole.roleName}
                                        roleColor={currentRole.color}
                                    />
                                ) : (
                                    <span className="text-sm text-gray-400">No role assigned</span>
                                )}
                            </div>
                        </div>
                    </div>

                    {/* Role Selection */}
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                            Select New Role <span className="text-red-500">*</span>
                        </label>
                        <select
                            value={selectedRoleId}
                            onChange={(e) => setSelectedRoleId(e.target.value)}
                            className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                        >
                            <option value="">-- Select a role --</option>
                            {roles.map(role => (
                                <option
                                    key={role.roleId}
                                    value={role.roleId}
                                    disabled={role.roleId === currentRole?.roleId}
                                >
                                    {role.roleName} {role.roleId === currentRole?.roleId ? '(Current)' : ''}
                                </option>
                            ))}
                        </select>
                    </div>

                    {/* Selected Role Preview */}
                    {selectedRole && selectedRole.roleId !== currentRole?.roleId && (
                        <div className="bg-blue-50 p-4 rounded-lg border border-blue-200">
                            <div className="flex items-start gap-3">
                                <Info className="w-5 h-5 text-blue-600 flex-shrink-0 mt-0.5" />
                                <div className="flex-1">
                                    <h4 className="text-sm font-semibold text-blue-900 mb-2">
                                        New Role Details
                                    </h4>
                                    <div className="space-y-1">
                                        <div className="flex items-center gap-2">
                                            <span className="text-sm text-blue-700">Role:</span>
                                            <RoleBadge
                                                roleName={selectedRole.roleName}
                                                roleColor={selectedRole.color}
                                            />
                                        </div>
                                        {selectedRole.description && (
                                            <p className="text-sm text-blue-700">
                                                {selectedRole.description}
                                            </p>
                                        )}
                                    </div>
                                </div>
                            </div>
                        </div>
                    )}

                    {/* Super Admin Warning */}
                    {selectedRole?.roleCode === 'SUPER_ADMIN' && (
                        <div className="bg-red-50 p-4 rounded-lg border border-red-200">
                            <div className="flex items-start gap-3">
                                <AlertCircle className="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" />
                                <div>
                                    <h4 className="text-sm font-semibold text-red-900 mb-1">
                                        ⚠️ Critical: Super Admin Role
                                    </h4>
                                    <p className="text-sm text-red-700">
                                        This role grants <strong>full unrestricted access</strong> to the entire system.
                                        Only assign this role to highly trusted administrators.
                                    </p>
                                </div>
                            </div>
                        </div>
                    )}

                    {/* Permission Note */}
                    <div className="bg-gray-50 p-3 rounded-lg border border-gray-200">
                        <p className="text-xs text-gray-600">
                            <strong>Note:</strong> Only Super Admins can assign the Super Admin role to others.
                            The user will immediately receive the new role's permissions.
                        </p>
                    </div>
                </div>

                {/* Footer Actions */}
                <div className="bg-gray-50 px-6 py-4 flex justify-end gap-3 rounded-b-lg border-t border-gray-200">
                    <button
                        type="button"
                        onClick={onCancel}
                        className="px-6 py-2 text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
                        disabled={loading}
                    >
                        Cancel
                    </button>
                    <button
                        type="button"
                        onClick={handleAssignRole}
                        className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                        disabled={loading || !selectedRoleId || selectedRoleId === currentRole?.roleId?.toString()}
                    >
                        {loading ? (
                            <span className="flex items-center gap-2">
                                <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                                Assigning...
                            </span>
                        ) : (
                            'Assign Role'
                        )}
                    </button>
                </div>
            </div>
        </div>
    );
};

RoleAssignmentModal.propTypes = {
    admin: PropTypes.object.isRequired,
    roles: PropTypes.array.isRequired,
    onSuccess: PropTypes.func.isRequired,
    onCancel: PropTypes.func.isRequired,
};

export default RoleAssignmentModal;
