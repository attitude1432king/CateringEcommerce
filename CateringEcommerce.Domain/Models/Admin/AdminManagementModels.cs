namespace CateringEcommerce.Domain.Models.Admin
{
    #region Admin List & Search

    public class AdminListRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; } // Search by name, email, or username
        public long? RoleId { get; set; } // Filter by role
        public bool? IsActive { get; set; } // Filter by status (null = all, true = active, false = inactive)
        public string? SortBy { get; set; } = "CreatedDate"; // CreatedDate, FullName, LastLogin, etc.
        public string? SortOrder { get; set; } = "DESC"; // ASC or DESC
    }

    public class AdminListResponse
    {
        public List<AdminListItem> Admins { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class AdminListItem
    {
        public long AdminId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Mobile { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedDate { get; set; }

        // Role information
        public long RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string RoleCode { get; set; } = string.Empty;
        public string RoleColor { get; set; } = "#6366f1";
    }

    #endregion

    #region Admin Detail

    public class AdminDetailResponse
    {
        public long AdminId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Mobile { get; set; }
        public string? ProfilePhoto { get; set; }
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockedUntil { get; set; }
        public int FailedLoginAttempts { get; set; }
        public bool ForcePasswordReset { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedDate { get; set; }
        public long? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime? LastModified { get; set; }
        public long? ModifiedBy { get; set; }
        public string? ModifiedByName { get; set; }

        // Role information
        public AdminRoleInfo Role { get; set; } = new();

        // Permissions
        public List<string> Permissions { get; set; } = new();
    }

    public class AdminRoleInfo
    {
        public long RoleId { get; set; }
        public string RoleCode { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string RoleDescription { get; set; } = string.Empty;
        public string RoleColor { get; set; } = "#6366f1";
        public bool IsSystemRole { get; set; }
    }

    #endregion

    #region Create Admin

    public class CreateAdminRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Mobile { get; set; }
        public string PasswordHash { get; set; } = string.Empty; // Client sends pre-hashed password
        public long RoleId { get; set; }
        public bool IsActive { get; set; } = true;
        public bool ForcePasswordReset { get; set; } = true; // Default to force password reset
    }

    #endregion

    #region Update Admin

    public class UpdateAdminRequest
    {
        public long AdminId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Mobile { get; set; }
        public string? ProfilePhoto { get; set; }
        // Note: Password, Role, and Status are updated via separate endpoints
    }

    #endregion

    #region Status Management

    public class UpdateAdminStatusRequest
    {
        public long AdminId { get; set; }
        public bool IsActive { get; set; }
    }

    public class AssignRoleRequest
    {
        public long AdminId { get; set; }
        public long RoleId { get; set; }
    }

    #endregion

    #region Password Management

    public class ResetAdminPasswordRequest
    {
        public long AdminId { get; set; }
        public string NewPasswordHash { get; set; } = string.Empty; // Pre-hashed by client or generated
        public bool ForcePasswordReset { get; set; } = true; // Force user to change on next login
    }

    #endregion

    #region Validation Responses

    public class UsernameExistsResponse
    {
        public bool Exists { get; set; }
    }

    public class EmailExistsResponse
    {
        public bool Exists { get; set; }
    }

    #endregion

    #region Super Admin Protection

    public class SuperAdminCheckResponse
    {
        public bool IsSuperAdmin { get; set; }
        public bool CanDeactivate { get; set; }
        public int ActiveSuperAdminCount { get; set; }
    }

    #endregion
}
