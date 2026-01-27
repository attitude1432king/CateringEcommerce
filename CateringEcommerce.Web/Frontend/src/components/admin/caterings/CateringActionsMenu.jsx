import { useState } from 'react';
import { Eye, CheckCircle, Ban, Edit, Trash2, MoreVertical } from 'lucide-react';
import { PermissionGuard } from '../auth/PermissionGuard';
import { usePermissions } from '../../../contexts/PermissionContext';

/**
 * Permission-Aware Action Menu for Catering Table Rows
 *
 * This component demonstrates how to conditionally render action buttons
 * based on user permissions. Actions only appear if the user has the required permission.
 */
const CateringActionsMenu = ({ catering, onAction }) => {
  const [isOpen, setIsOpen] = useState(false);
  const { hasPermission, hasAnyPermission } = usePermissions();

  // Don't show menu if user has no action permissions at all
  const hasAnyActionPermission = hasAnyPermission([
    'CATERING_VERIFY',
    'CATERING_BLOCK',
    'CATERING_EDIT',
    'CATERING_DELETE'
  ]);

  const handleAction = (actionType) => {
    onAction(catering, actionType);
    setIsOpen(false);
  };

  return (
    <div className="relative">
      {/* View Details Button - Always Available */}
      <button
        onClick={() => handleAction('view')}
        className="p-1.5 text-gray-600 hover:text-indigo-600 hover:bg-indigo-50 rounded transition-colors"
        title="View Details"
      >
        <Eye className="w-4 h-4" />
      </button>

      {/* Actions Dropdown - Only if user has any action permissions */}
      {hasAnyActionPermission && (
        <>
          <button
            onClick={() => setIsOpen(!isOpen)}
            className="p-1.5 text-gray-600 hover:text-gray-900 hover:bg-gray-100 rounded transition-colors"
            title="More Actions"
          >
            <MoreVertical className="w-4 h-4" />
          </button>

          {/* Dropdown Menu */}
          {isOpen && (
            <>
              {/* Backdrop */}
              <div
                className="fixed inset-0 z-10"
                onClick={() => setIsOpen(false)}
              ></div>

              {/* Menu */}
              <div className="absolute right-0 mt-2 w-48 bg-white rounded-lg shadow-lg border border-gray-200 py-1 z-20">
                {/* Verify Action - Only for Pending caterings with CATERING_VERIFY permission */}
                <PermissionGuard permission="CATERING_VERIFY">
                  {catering.status === 'Pending' && (
                    <button
                      onClick={() => handleAction('verify')}
                      className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-green-50 hover:text-green-700 flex items-center space-x-2"
                    >
                      <CheckCircle className="w-4 h-4" />
                      <span>Verify & Approve</span>
                    </button>
                  )}
                </PermissionGuard>

                {/* Block/Unblock Action */}
                <PermissionGuard permission="CATERING_BLOCK">
                  <button
                    onClick={() => handleAction(catering.isBlocked ? 'unblock' : 'block')}
                    className={`w-full text-left px-4 py-2 text-sm flex items-center space-x-2 ${
                      catering.isBlocked
                        ? 'text-green-700 hover:bg-green-50'
                        : 'text-red-700 hover:bg-red-50'
                    }`}
                  >
                    <Ban className="w-4 h-4" />
                    <span>{catering.isBlocked ? 'Unblock' : 'Block'}</span>
                  </button>
                </PermissionGuard>

                {/* Edit Action */}
                <PermissionGuard permission="CATERING_EDIT">
                  <button
                    onClick={() => handleAction('edit')}
                    className="w-full text-left px-4 py-2 text-sm text-blue-700 hover:bg-blue-50 flex items-center space-x-2"
                  >
                    <Edit className="w-4 h-4" />
                    <span>Edit Details</span>
                  </button>
                </PermissionGuard>

                {/* Divider - Only show if delete permission exists */}
                <PermissionGuard permission="CATERING_DELETE">
                  <div className="border-t border-gray-200 my-1"></div>
                </PermissionGuard>

                {/* Delete Action */}
                <PermissionGuard permission="CATERING_DELETE">
                  <button
                    onClick={() => handleAction('delete')}
                    className="w-full text-left px-4 py-2 text-sm text-red-700 hover:bg-red-50 flex items-center space-x-2"
                  >
                    <Trash2 className="w-4 h-4" />
                    <span>Delete</span>
                  </button>
                </PermissionGuard>
              </div>
            </>
          )}
        </>
      )}
    </div>
  );
};

export default CateringActionsMenu;
