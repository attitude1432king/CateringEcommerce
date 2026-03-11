import { createContext, useContext, useState, useEffect } from 'react';
import { useAdminAuth } from './AdminAuthContext';
import { apiCall } from '../services/apiUtils'; // P3 FIX: Use consolidated apiUtils
const PermissionContext = createContext(null);

export const usePermissions = () => {
    const context = useContext(PermissionContext);
    if (!context) {
        throw new Error('usePermissions must be used within PermissionProvider');
    }
    return context;
};

export const PermissionProvider = ({ children }) => {
    const { admin } = useAdminAuth();
    const [permissions, setPermissions] = useState([]); // Array of { module, actions } or simple strings
    const [roles, setRoles] = useState([]);
    const [loading, setLoading] = useState(true);
    const [isLoadingPermissions, setIsLoadingPermissions] = useState(true);

    // Fetch permissions from API or decode from JWT
    useEffect(() => {
        if (admin) {
            fetchPermissions();
        } else {
            setPermissions([]);
            setRoles([]);
            setLoading(false);
            setIsLoadingPermissions(false);
        }
    }, [admin]);

    const fetchPermissions = async () => {
        try {
            const result = await apiCall('/admin/auth/permissions');

            if (result.result && result.data) {
                setPermissions(result.data.permissions || []);
                setRoles(result.data.roles || []);
            } else {
                // P1 SECURITY FIX: Fail closed - grant no permissions on API failure
                // Never trust client-side role from localStorage
                console.error('Permission API returned unexpected format - denying all permissions');
                setPermissions([]);
                setRoles([]);
            }
        } catch (error) {
            console.error('Failed to fetch permissions:', error);

            // P1 SECURITY FIX: Fail closed - grant no permissions on error
            // Mock permissions based on localStorage role is a security vulnerability
            setPermissions([]);
            setRoles([]);
        } finally {
            setLoading(false);
            setIsLoadingPermissions(false);
        }
    };

    /**
     * Check if user has a specific permission
     * Supports two formats:
     * 1. hasPermission(module, action) - e.g., hasPermission("MASTER_DATA", "VIEW")
     * 2. hasPermission(permission) - e.g., hasPermission("USER_VIEW") (legacy)
     * @param {string} moduleOrPermission - Module name or full permission string
     * @param {string} [action] - Optional action (VIEW, ADD, EDIT, DELETE, etc.)
     * @returns {boolean}
     */
    const hasPermission = (moduleOrPermission, action = null) => {
        // Super admin has all permissions
        if (roles.includes('SUPER_ADMIN') || permissions.includes('*')) {
            return true;
        }

        // If action is provided, check module-action format
        if (action) {
            // Check if permissions array contains objects with module/actions
            const modulePermission = permissions.find(
                p => typeof p === 'object' && p.module === moduleOrPermission
            );

            if (modulePermission && modulePermission.actions) {
                return modulePermission.actions.includes(action);
            }

            // Fallback: check for legacy format like "MODULE_ACTION"
            const legacyFormat = `${moduleOrPermission}_${action}`;
            return permissions.includes(legacyFormat);
        }

        // Legacy format: simple permission string
        return permissions.includes(moduleOrPermission);
    };

    /**
     * Check if user has ANY of the provided permissions
     * @param {string[]} permissionList - Array of permissions
     * @returns {boolean}
     */
    const hasAnyPermission = (permissionList) => {
        if (roles.includes('SUPER_ADMIN') || permissions.includes('*')) {
            return true;
        }
        return permissionList.some(p => permissions.includes(p));
    };

    /**
     * Check if user has ALL of the provided permissions
     * @param {string[]} permissionList - Array of permissions
     * @returns {boolean}
     */
    const hasAllPermissions = (permissionList) => {
        if (roles.includes('SUPER_ADMIN') || permissions.includes('*')) {
            return true;
        }
        return permissionList.every(p => permissions.includes(p));
    };

    /**
     * Check if user has a specific role
     * @param {string} role - Role code to check
     * @returns {boolean}
     */
    const hasRole = (role) => {
        return roles.includes(role);
    };

    /**
     * Get filtered permissions for a specific resource
     * @param {string} resource - Resource prefix (e.g., 'CATERING', 'USER')
     * @returns {string[]}
     */
    const getResourcePermissions = (resource) => {
        if (permissions.includes('*')) {
            // Return mock full permissions for the resource
            return [`${resource}_VIEW`, `${resource}_EDIT`, `${resource}_DELETE`];
        }
        return permissions.filter(p => p.startsWith(resource + '_'));
    };

    const value = {
        permissions,
        roles,
        loading,
        isLoadingPermissions,
        hasPermission,
        hasAnyPermission,
        hasAllPermissions,
        hasRole,
        getResourcePermissions,
        refreshPermissions: fetchPermissions,
        isSuperAdmin: roles.includes('SUPER_ADMIN') || permissions.includes('*')
    };

    // Show loading spinner while permissions are being fetched
    if (loading && admin) {
        return (
            <div className="min-h-screen flex items-center justify-center bg-gray-50">
                <div className="text-center">
                    <div className="w-16 h-16 border-4 border-indigo-600 border-t-transparent rounded-full animate-spin mx-auto mb-4"></div>
                    <p className="text-gray-600 font-medium">Loading permissions...</p>
                </div>
            </div>
        );
    }

    return (
        <PermissionContext.Provider value={value}>
            {children}
        </PermissionContext.Provider>
    );
};
