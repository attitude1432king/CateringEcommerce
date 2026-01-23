import { usePermissions } from '../../../contexts/PermissionContext';

/**
 * PermissionGuard Component
 *
 * Renders children only if user has required permission(s).
 * This is the core component for implementing permission-based UI rendering.
 *
 * @param {string} permission - Single permission required
 * @param {string[]} anyOf - Render if user has ANY of these permissions
 * @param {string[]} allOf - Render if user has ALL of these permissions
 * @param {ReactNode} fallback - What to render if permission denied
 * @param {ReactNode} children - Content to render if permission granted
 *
 * @example
 * // Single permission check
 * <PermissionGuard permission="CATERING_VERIFY">
 *   <ApproveButton />
 * </PermissionGuard>
 *
 * @example
 * // ANY of multiple permissions
 * <PermissionGuard anyOf={['USER_BLOCK', 'USER_DELETE']}>
 *   <UserActionsMenu />
 * </PermissionGuard>
 *
 * @example
 * // ALL of multiple permissions
 * <PermissionGuard allOf={['EARNINGS_VIEW', 'PAYOUT_APPROVE']}>
 *   <PayoutApprovalSection />
 * </PermissionGuard>
 *
 * @example
 * // With fallback content
 * <PermissionGuard
 *   permission="EARNINGS_VIEW"
 *   fallback={<div className="text-gray-500">No access to earnings</div>}
 * >
 *   <EarningsChart />
 * </PermissionGuard>
 */
export const PermissionGuard = ({
  permission,
  anyOf,
  allOf,
  fallback = null,
  children
}) => {
  const { hasPermission, hasAnyPermission, hasAllPermissions, loading } = usePermissions();

  // Don't render anything while loading
  if (loading) {
    return null;
  }

  let hasAccess = false;

  // Check permission based on provided props
  if (permission) {
    hasAccess = hasPermission(permission);
  } else if (anyOf && Array.isArray(anyOf)) {
    hasAccess = hasAnyPermission(anyOf);
  } else if (allOf && Array.isArray(allOf)) {
    hasAccess = hasAllPermissions(allOf);
  } else {
    // If no permission specified, deny access by default
    console.warn('PermissionGuard: No permission criteria specified');
    hasAccess = false;
  }

  return hasAccess ? <>{children}</> : fallback;
};

export default PermissionGuard;
