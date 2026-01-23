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
  const [permissions, setPermissions] = useState([]);
  const [roles, setRoles] = useState([]);
  const [loading, setLoading] = useState(true);

  // Fetch permissions from API or decode from JWT
  useEffect(() => {
    if (admin) {
      fetchPermissions();
    } else {
      setPermissions([]);
      setRoles([]);
      setLoading(false);
    }
  }, [admin]);

  const fetchPermissions = async () => {
    try {
      const token = getToken();

      // TODO: Replace with actual API endpoint when backend is ready
      // For now, mock based on admin role
      const mockPermissions = getMockPermissions(admin.role);

      setPermissions(mockPermissions.permissions);
      setRoles(mockPermissions.roles);

      /*
      // Actual API call (uncomment when backend is ready):
      const response = await fetch('http://localhost:5000/api/admin/auth/permissions', {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      const result = await response.json();

      if (result.result && result.data) {
        setPermissions(result.data.permissions || []);
        setRoles(result.data.roles || []);
      }
      */
    } catch (error) {
      console.error('Failed to fetch permissions:', error);
    } finally {
      setLoading(false);
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
   * @param {string} permission - Permission code to check
   * @returns {boolean}
   */
  const hasPermission = (permission) => {
    // Super admin has all permissions
    if (roles.includes('SUPER_ADMIN') || permissions.includes('*')) {
      return true;
    }

    // Check if permission exists in user's permission list
    return permissions.includes(permission);
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
    hasPermission,
    hasAnyPermission,
    hasAllPermissions,
    hasRole,
    getResourcePermissions,
    refreshPermissions: fetchPermissions,
    isSuperAdmin: roles.includes('SUPER_ADMIN') || permissions.includes('*')
  };

  return (
    <PermissionContext.Provider value={value}>
      {children}
    </PermissionContext.Provider>
  );
};
