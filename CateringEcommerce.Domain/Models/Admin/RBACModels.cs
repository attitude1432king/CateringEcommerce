namespace CateringEcommerce.Domain.Models.Admin
{
    // =====================================================
    // ROLE MODELS
    // =====================================================

    public class AdminRole
    {
        public long RoleId { get; set; }
        public string RoleCode { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Color { get; set; } = "#6366f1";
        public bool IsSystemRole { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public long? UpdatedBy { get; set; }

        // Navigation properties
        public List<string> Permissions { get; set; } = new();
    }

    public class CreateRoleRequest
    {
        public string RoleCode { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Color { get; set; } = "#6366f1";
        public List<string> PermissionCodes { get; set; } = new();
    }

    public class UpdateRoleRequest
    {
        public long RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Color { get; set; } = "#6366f1";
        public List<string> PermissionCodes { get; set; } = new();
    }

    public class RoleListResponse
    {
        public List<RoleItem> Roles { get; set; } = new();
    }

    public class RoleItem
    {
        public long RoleId { get; set; }
        public string RoleCode { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Color { get; set; } = "#6366f1";
        public bool IsSystemRole { get; set; }
        public bool IsActive { get; set; }
        public int PermissionCount { get; set; }
        public int AdminCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class RoleDetailResponse
    {
        public long RoleId { get; set; }
        public string RoleCode { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Color { get; set; } = "#6366f1";
        public bool IsSystemRole { get; set; }
        public bool IsActive { get; set; }
        public List<PermissionItem> Permissions { get; set; } = new();
        public List<AdminUserItem> AdminUsers { get; set; } = new();
        public DateTime CreatedDate { get; set; }
    }

    // =====================================================
    // PERMISSION MODELS
    // =====================================================

    public class AdminPermission
    {
        public long PermissionId { get; set; }
        public string PermissionCode { get; set; } = string.Empty;
        public string PermissionName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Module { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; }
    }

    public class PermissionItem
    {
        public long PermissionId { get; set; }
        public string PermissionCode { get; set; } = string.Empty;
        public string PermissionName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Module { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class PermissionListResponse
    {
        public List<PermissionGroup> Groups { get; set; } = new();
    }

    public class PermissionGroup
    {
        public string Module { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public List<PermissionItem> Permissions { get; set; } = new();
    }

    // =====================================================
    // ADMIN USER MODELS (Extended)
    // =====================================================

    public class AdminUserWithRoles
    {
        public long AdminId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
        public List<string> RoleCodes { get; set; } = new();
        public List<string> RoleNames { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }

    public class AdminUserItem
    {
        public long AdminId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class AssignRoleRequest
    {
        public long AdminId { get; set; }
        public List<long> RoleIds { get; set; } = new();
    }

    // =====================================================
    // PERMISSION CONTEXT MODELS (for JWT and Frontend)
    // =====================================================

    public class AdminPermissionsResponse
    {
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
        public bool IsSuperAdmin { get; set; }
    }

    // =====================================================
    // AUDIT LOG MODELS
    // =====================================================

    public class AuditLogEntry
    {
        public long AuditId { get; set; }
        public long AdminId { get; set; }
        public string AdminName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public long? TargetId { get; set; }
        public string? TargetType { get; set; }
        public string? Details { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = "SUCCESS";
        public string? ErrorMessage { get; set; }
    }

    public class AuditLogListRequest
    {
        public long? AdminId { get; set; }
        public string? Action { get; set; }
        public string? Module { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class AuditLogListResponse
    {
        public List<AuditLogEntry> Logs { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    // =====================================================
    // HELPER/UTILITY MODELS
    // =====================================================

    public class PermissionCheckRequest
    {
        public List<string> Permissions { get; set; } = new();
        public bool RequireAll { get; set; } = false;  // true = AND, false = OR
    }

    public class PermissionCheckResponse
    {
        public bool HasPermission { get; set; }
        public List<string> GrantedPermissions { get; set; } = new();
        public List<string> DeniedPermissions { get; set; } = new();
    }
}
