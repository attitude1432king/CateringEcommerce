import { Navigate } from 'react-router-dom';
import { usePermissions } from '../../../contexts/PermissionContext';
import LoadingSkeleton from '../ui/LoadingSkeleton';

/**
 * ProtectedRoute Component
 *
 * Wraps a route component and only renders it if user has required permission(s).
 * Redirects to 403 page if access is denied.
 *
 * @param {string} permission - Single permission required
 * @param {string[]} anyOf - Allow if user has ANY of these permissions
 * @param {string[]} allOf - Allow if user has ALL of these permissions
 * @param {ReactNode} children - Route component to render
 *
 * @example
 * // In Router.jsx
 * <Route
 *   path="/admin/caterings"
 *   element={
 *     <ProtectedRoute permission="CATERING_VIEW">
 *       <AdminCaterings />
 *     </ProtectedRoute>
 *   }
 * />
 *
 * @example
 * // Require ANY of multiple permissions
 * <Route
 *   path="/admin/finance"
 *   element={
 *     <ProtectedRoute anyOf={['EARNINGS_VIEW', 'PAYOUT_APPROVE']}>
 *       <AdminFinance />
 *     </ProtectedRoute>
 *   }
 * />
 */
export const ProtectedRoute = ({
    permission,
    anyOf,
    allOf,
    requiredPermissions,
    requireSuperAdmin = false,
    children
}) => {
    const { hasPermission, hasAnyPermission, hasAllPermissions, isSuperAdmin, loading } = usePermissions();

    // Show loading state while checking permissions
    if (loading) {
        return (
            <div className="min-h-screen bg-gray-50 flex items-center justify-center">
                <LoadingSkeleton type="page" />
            </div>
        );
    }

    let hasAccess = false;

    // Check permission based on provided props
    if (requireSuperAdmin) {
        hasAccess = isSuperAdmin;
    } else if (requiredPermissions && Array.isArray(requiredPermissions)) {
        hasAccess = hasAllPermissions(requiredPermissions);
    } else if (permission) {
        hasAccess = hasPermission(permission);
    } else if (anyOf && Array.isArray(anyOf)) {
        hasAccess = hasAnyPermission(anyOf);
    } else if (allOf && Array.isArray(allOf)) {
        hasAccess = hasAllPermissions(allOf);
    } else {
        // If no permission specified, deny access by default
        console.warn('ProtectedRoute: No permission criteria specified');
        hasAccess = false;
    }

    return hasAccess ? children : <Navigate to="/admin/403" replace />;
};

export default ProtectedRoute;
