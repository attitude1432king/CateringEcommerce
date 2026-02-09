import { Navigate } from 'react-router-dom';
import { usePermissions } from '../../contexts/PermissionContext';

/**
 * ProtectedRoute - Wraps routes that require specific permissions
 *
 * Usage:
 * <ProtectedRoute module="MASTER_DATA" action="VIEW">
 *   <MasterDataPage />
 * </ProtectedRoute>
 *
 * OR (legacy):
 * <ProtectedRoute permission="USER_VIEW">
 *   <UserPage />
 * </ProtectedRoute>
 */
const ProtectedRoute = ({ children, module, action, permission, fallback = '/admin/dashboard' }) => {
  const { hasPermission, loading } = usePermissions();

  // Show loading state while checking permissions
  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="text-center">
          <div className="w-16 h-16 border-4 border-indigo-600 border-t-transparent rounded-full animate-spin mx-auto mb-4"></div>
          <p className="text-gray-600">Checking permissions...</p>
        </div>
      </div>
    );
  }

  // Check permissions
  let allowed = false;

  if (module && action) {
    // Module + Action format
    allowed = hasPermission(module, action);
  } else if (permission) {
    // Legacy permission string format
    allowed = hasPermission(permission);
  } else {
    console.error('ProtectedRoute: Either (module + action) or permission must be provided');
    return <Navigate to={fallback} replace />;
  }

  // Redirect if not allowed
  if (!allowed) {
    return <Navigate to={fallback} replace />;
  }

  return children;
};

export default ProtectedRoute;
