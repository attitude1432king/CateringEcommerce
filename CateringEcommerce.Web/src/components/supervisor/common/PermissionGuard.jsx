/**
 * PermissionGuard Component
 * Conditionally render children based on supervisor permissions
 */

import PropTypes from 'prop-types';
import { Lock } from 'lucide-react';
import { hasPermission as checkPermission } from '../../../utils/supervisor/helpers';

const PermissionGuard = ({
  supervisor,
  permission,
  children,
  fallback = null,
  showLocked = false,
  lockedMessage,
}) => {
  const hasAccess = checkPermission(supervisor, permission);

  if (hasAccess) {
    return <>{children}</>;
  }

  if (showLocked) {
    return (
      <div className="relative">
        {/* Disabled/Locked UI */}
        <div className="opacity-50 pointer-events-none">
          {children}
        </div>

        {/* Lock Overlay */}
        <div className="absolute inset-0 flex items-center justify-center bg-gray-900/10 rounded">
          <div className="bg-white rounded-lg shadow-lg p-4 flex items-center gap-3 max-w-sm">
            <Lock className="w-5 h-5 text-gray-500 flex-shrink-0" />
            <div>
              <p className="text-sm font-medium text-gray-900">
                Permission Required
              </p>
              {lockedMessage && (
                <p className="text-xs text-gray-600 mt-1">
                  {lockedMessage}
                </p>
              )}
            </div>
          </div>
        </div>
      </div>
    );
  }

  return fallback;
};

PermissionGuard.propTypes = {
  supervisor: PropTypes.object.isRequired,
  permission: PropTypes.string.isRequired,
  children: PropTypes.node.isRequired,
  fallback: PropTypes.node,
  showLocked: PropTypes.bool,
  lockedMessage: PropTypes.string,
};

export default PermissionGuard;
