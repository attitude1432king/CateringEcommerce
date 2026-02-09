import { createContext, useContext, useState, useEffect } from 'react';
import { useAdminAuth } from './AdminAuthContext';

const PermissionContext = createContext(null);

export const usePermissions = () => {
  const context = useContext(PermissionContext);
  if (!context) {
    throw new Error('usePermissions must be used within PermissionProvider');
  }
  return context;
};

export const PermissionProvider = ({ children }) => {
  const { admin, getToken } = useAdminAuth();
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
      const token = getToken();
      const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:44368';

      // Call the actual backend API
      const response = await fetch(`${API_BASE_URL}/api/admin/auth/permissions`, {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const result = await response.json();

      if (result.result && result.data) {
        setPermissions(result.data.permissions || []);
        setRoles(result.data.roles || []);
      } else {
        // Fallback to mock permissions if API fails
        console.warn('API returned unexpected format, using mock permissions');
        const mockPermissions = getMockPermissions(admin.role);
        setPermissions(mockPermissions.permissions);
        setRoles(mockPermissions.roles);
      }
    } catch (error) {
      console.error('Failed to fetch permissions:', error);

      // Fallback to mock permissions on error
      try {
        const mockPermissions = getMockPermissions(admin.role);
        setPermissions(mockPermissions.permissions);
        setRoles(mockPermissions.roles);
      } catch (mockError) {
        console.error('Mock permissions also failed:', mockError);
        setPermissions([]);
        setRoles([]);
      }
    } finally {
      setLoading(false);
      setIsLoadingPermissions(false);
    }
  };

  // Mock permission mapping based on role (remove when backend is ready)
  const getMockPermissions = (role) => {
    const permissionMap = {
      'Super Admin': {
        roles: ['SUPER_ADMIN'],
        permissions: ['*'] // All permissions
      },
      'Catering Manager': {
        roles: ['CATERING_ADMIN'],
        permissions: [
          'CATERING_VIEW',
          'CATERING_VERIFY',
          'CATERING_BLOCK',
          'CATERING_EDIT'
        ]
      },
      'User Manager': {
        roles: ['USER_ADMIN'],
        permissions: [
          'USER_VIEW',
          'USER_BLOCK',
          'USER_EDIT',
          'REVIEW_VIEW',
          'REVIEW_MODERATE',
          'REVIEW_DELETE'
        ]
      },
      'Finance Manager': {
        roles: ['FINANCE_ADMIN'],
        permissions: [
          'EARNINGS_VIEW',
          'EARNINGS_EXPORT',
          'PAYOUT_APPROVE'
        ]
      },
      'Marketing Manager': {
        roles: ['MARKETING_ADMIN'],
        permissions: [
          'DISCOUNT_VIEW',
          'DISCOUNT_CREATE',
          'DISCOUNT_EDIT',
          'BANNER_MANAGE'
        ]
      }
    };

    return permissionMap[role] || { roles: [], permissions: [] };
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
