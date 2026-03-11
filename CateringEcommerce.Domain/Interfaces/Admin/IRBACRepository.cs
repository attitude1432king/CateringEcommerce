using CateringEcommerce.Domain.Models.Admin;

namespace CateringEcommerce.Domain.Interfaces.Admin
{
    /// <summary>
    /// Repository for Role-Based Access Control operations
    /// </summary>
    public interface IRBACRepository
    {
        // =====================================================
        // ROLE MANAGEMENT
        // =====================================================

        /// <summary>
        /// Get all roles
        /// </summary>
        Task<List<RoleItem>> GetAllRolesAsync();

        /// <summary>
        /// Get role details by ID
        /// </summary>
        Task<RoleDetailResponse?> GetRoleByIdAsync(long roleId);

        /// <summary>
        /// Get role details by code
        /// </summary>
        Task<AdminRole?> GetRoleByCodeAsync(string roleCode);

        /// <summary>
        /// Create a new role
        /// </summary>
        Task<long> CreateRoleAsync(CreateRoleRequest request, long createdBy);

        /// <summary>
        /// Update an existing role
        /// </summary>
        Task<bool> UpdateRoleAsync(UpdateRoleRequest request, long updatedBy);

        /// <summary>
        /// Delete a role (soft delete by setting IsActive = false)
        /// </summary>
        Task<bool> DeleteRoleAsync(long roleId, long deletedBy);

        /// <summary>
        /// Check if role code already exists
        /// </summary>
        Task<bool> RoleCodeExistsAsync(string roleCode, long? excludeRoleId = null);

        // =====================================================
        // PERMISSION MANAGEMENT
        // =====================================================

        /// <summary>
        /// Get all permissions grouped by module
        /// </summary>
        Task<PermissionListResponse> GetAllPermissionsAsync();

        /// <summary>
        /// Get permissions for a specific role
        /// </summary>
        Task<List<string>> GetRolePermissionsAsync(long roleId);

        /// <summary>
        /// Assign permissions to a role
        /// </summary>
        Task<bool> AssignPermissionsToRoleAsync(long roleId, List<string> permissionCodes, long assignedBy);

        /// <summary>
        /// Get all permission codes for an admin user (across all their roles)
        /// </summary>
        Task<List<string>> GetAdminPermissionsAsync(long adminId);

        // =====================================================
        // USER-ROLE MANAGEMENT
        // =====================================================

        /// <summary>
        /// Get all roles assigned to an admin user
        /// </summary>
        Task<List<string>> GetAdminRolesAsync(long adminId);

        /// <summary>
        /// Assign roles to an admin user
        /// </summary>
        Task<bool> AssignRolesToAdminAsync(long adminId, List<long> roleIds, long assignedBy);

        /// <summary>
        /// Remove a specific role from an admin user
        /// </summary>
        Task<bool> RemoveRoleFromAdminAsync(long adminId, long roleId);

        /// <summary>
        /// Check if admin has a specific permission
        /// </summary>
        Task<bool> AdminHasPermissionAsync(long adminId, string permissionCode);

        /// <summary>
        /// Check if admin has any of the specified permissions
        /// </summary>
        Task<bool> AdminHasAnyPermissionAsync(long adminId, List<string> permissionCodes);

        /// <summary>
        /// Check if admin has all of the specified permissions
        /// </summary>
        Task<bool> AdminHasAllPermissionsAsync(long adminId, List<string> permissionCodes);

        /// <summary>
        /// Get complete permission context for an admin (roles + permissions)
        /// </summary>
        Task<AdminPermissionsResponse> GetAdminPermissionContextAsync(long adminId);

        /// <summary>
        /// Check if admin has Super Admin role
        /// </summary>
        Task<bool> IsSuperAdminAsync(long adminId);

        // =====================================================
        // AUDIT LOGGING
        // =====================================================

        /// <summary>
        /// Log an audit entry
        /// </summary>
        Task LogAuditAsync(AuditLogEntry entry);

        /// <summary>
        /// Get audit logs with filtering
        /// </summary>
        Task<AuditLogListResponse> GetAuditLogsAsync(AuditLogListRequest request);

        // =====================================================
        // ADMIN ROLE INITIALIZATION & DIAGNOSTICS
        // =====================================================

        /// <summary>
        /// Ensures an admin has at least one active role assigned
        /// Auto-assigns SUPER_ADMIN (for first admin) or CATERING_ADMIN (for others) if missing
        /// </summary>
        Task<bool> EnsureAdminHasRoleAsync(long adminId, long? assignedBy = null);

        /// <summary>
        /// Gets admin roles with automatic fallback to assign role if missing
        /// SAFE version that prevents empty role lists
        /// </summary>
        Task<List<string>> GetAdminRolesWithFallbackAsync(long adminId);

        /// <summary>
        /// Gets admin permissions with automatic fallback to assign role if missing
        /// SAFE version that prevents empty permission lists
        /// </summary>
        Task<List<string>> GetAdminPermissionsWithFallbackAsync(long adminId);

        /// <summary>
        /// Diagnoses admin role assignment issues and returns detailed diagnostic info
        /// Used for troubleshooting missing role/permission data
        /// </summary>
        Task<AdminRoleDiagnostics> DiagnoseAdminRolesAsync(long adminId);
    }
}
