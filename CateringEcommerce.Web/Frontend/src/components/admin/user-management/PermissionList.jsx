import React from 'react';
import PropTypes from 'prop-types';
import { Check } from 'lucide-react';

/**
 * PermissionList Component
 * Displays permissions grouped by module with checkmarks (read-only)
 */
const PermissionList = ({ permissions, className = '' }) => {
  // Group permissions by module
  const groupedPermissions = React.useMemo(() => {
    const grouped = {};

    Object.entries(permissions).forEach(([module, perms]) => {
      grouped[module] = perms;
    });

    return grouped;
  }, [permissions]);

  if (!permissions || Object.keys(permissions).length === 0) {
    return (
      <div className="text-sm text-gray-500 italic">
        No permissions assigned to this role.
      </div>
    );
  }

  return (
    <div className={`space-y-4 ${className}`}>
      {Object.entries(groupedPermissions).map(([module, perms]) => (
        <div key={module} className="border-l-4 border-blue-500 pl-4">
          <h4 className="text-sm font-semibold text-gray-700 mb-2">
            {module}
          </h4>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-2">
            {perms.map((perm) => (
              <div key={perm.permissionCode} className="flex items-start space-x-2">
                <Check className="w-4 h-4 text-green-600 mt-0.5 flex-shrink-0" />
                <div>
                  <div className="text-sm font-medium text-gray-800">
                    {perm.permissionName}
                  </div>
                  {perm.description && (
                    <div className="text-xs text-gray-500">
                      {perm.description}
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      ))}
    </div>
  );
};

PermissionList.propTypes = {
  permissions: PropTypes.object.isRequired, // Object with module as keys and array of permissions as values
  className: PropTypes.string,
};

export default PermissionList;
