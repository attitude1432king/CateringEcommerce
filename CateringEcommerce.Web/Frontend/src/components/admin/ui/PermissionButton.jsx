import { usePermissions } from '../../../contexts/PermissionContext';
import Button from './Button';

/**
 * PermissionButton Component
 *
 * A button that automatically disables or hides based on user permissions.
 *
 * @param {string} permission - Single permission required
 * @param {string[]} anyOf - Render if user has ANY of these permissions
 * @param {string[]} allOf - Render if user has ALL of these permissions
 * @param {boolean} hideWhenDenied - Hide button instead of disabling (default: false)
 * @param {boolean} showTooltip - Show tooltip explaining why disabled (default: true)
 * @param {string} deniedMessage - Custom message when permission denied
 *
 * @example
 * // Disabled with tooltip when no permission
 * <PermissionButton permission="CATERING_VERIFY" onClick={handleApprove}>
 *   Approve
 * </PermissionButton>
 *
 * @example
 * // Hide completely when no permission
 * <PermissionButton
 *   permission="CATERING_DELETE"
 *   hideWhenDenied={true}
 *   onClick={handleDelete}
 * >
 *   Delete
 * </PermissionButton>
 *
 * @example
 * // Custom denied message
 * <PermissionButton
 *   permission="EARNINGS_VIEW"
 *   deniedMessage="Finance access required"
 *   onClick={handleExport}
 * >
 *   Export Report
 * </PermissionButton>
 */
export const PermissionButton = ({
    permission,
    anyOf,
    allOf,
    requiredPermissions,
    requireSuperAdmin = false,
    hideWhenDenied = false,
    showTooltip = true,
    deniedMessage,
    children,
    ...buttonProps
}) => {
    const { hasPermission, hasAnyPermission, hasAllPermissions, isSuperAdmin } = usePermissions();

    let hasAccess = false;
    let permissionName = '';

    // Check permissions
    if (requireSuperAdmin) {
        hasAccess = isSuperAdmin;
        permissionName = 'SUPER_ADMIN';
    } else if (requiredPermissions && Array.isArray(requiredPermissions)) {
        hasAccess = hasAllPermissions(requiredPermissions);
        permissionName = requiredPermissions.join(' and ');
    } else if (permission) {
        hasAccess = hasPermission(permission);
        permissionName = permission;
    } else if (anyOf && Array.isArray(anyOf)) {
        hasAccess = hasAnyPermission(anyOf);
        permissionName = anyOf.join(' or ');
    } else if (allOf && Array.isArray(allOf)) {
        hasAccess = hasAllPermissions(allOf);
        permissionName = allOf.join(' and ');
    }

    // Hide button if permission denied and hideWhenDenied is true
    if (!hasAccess && hideWhenDenied) {
        return null;
    }

    // Disabled button with tooltip
    if (!hasAccess) {
        const tooltipMessage = deniedMessage || `Requires permission: ${permissionName}`;

        return (
            <div className="relative group inline-block">
                <Button
                    {...buttonProps}
                    disabled={true}
                    className={`opacity-50 cursor-not-allowed ${buttonProps.className || ''}`}
                >
                    {children}
                </Button>

                {showTooltip && (
                    <div className="absolute bottom-full left-1/2 transform -translate-x-1/2 mb-2 px-3 py-2 bg-gray-900 text-white text-xs rounded-lg opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none whitespace-nowrap z-50">
                        {tooltipMessage}
                        <div className="absolute top-full left-1/2 transform -translate-x-1/2 -mt-1 border-4 border-transparent border-t-gray-900"></div>
                    </div>
                )}
            </div>
        );
    }

    // Render normal button when permission granted
    return <Button {...buttonProps}>{children}</Button>;
};

export default PermissionButton;
