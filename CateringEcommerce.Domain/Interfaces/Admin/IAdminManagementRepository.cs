using CateringEcommerce.Domain.Models.Admin;

namespace CateringEcommerce.Domain.Interfaces.Admin
{
    public interface IAdminManagementRepository
    {
        // ==========================================
        // LIST & SEARCH
        // ==========================================

        /// <summary>
        /// Get all admins with pagination, search, and filters
        /// </summary>
        Task<AdminListResponse> GetAllAdminsAsync(AdminListRequest request);

        /// <summary>
        /// Get admin details by ID with role and permission information
        /// </summary>
        Task<AdminDetailResponse?> GetAdminByIdAsync(long adminId);

        // ==========================================
        // CRUD OPERATIONS
        // ==========================================

        /// <summary>
        /// Create new admin user with role assignment
        /// </summary>
        /// <returns>New admin ID</returns>
        Task<long> CreateAdminAsync(CreateAdminRequest request, long createdBy);

        /// <summary>
        /// Update admin information (excluding password and role)
        /// </summary>
        Task<bool> UpdateAdminAsync(UpdateAdminRequest request, long updatedBy);

        /// <summary>
        /// Soft delete admin user
        /// </summary>
        Task<bool> DeleteAdminAsync(long adminId, long deletedBy);

        // ==========================================
        // STATUS MANAGEMENT
        // ==========================================

        /// <summary>
        /// Activate or deactivate admin account
        /// </summary>
        Task<bool> UpdateAdminStatusAsync(long adminId, bool isActive, long updatedBy);

        /// <summary>
        /// Assign a role to an admin user (one-to-one)
        /// </summary>
        Task<bool> AssignRoleToAdminAsync(long adminId, long roleId, long assignedBy);

        // ==========================================
        // PASSWORD MANAGEMENT
        // ==========================================

        /// <summary>
        /// Reset admin password (admin provides hash, not plain text)
        /// </summary>
        Task<bool> ResetAdminPasswordAsync(long adminId, string newPasswordHash, bool forceReset, long resetBy);

        /// <summary>
        /// Force admin to reset password on next login
        /// </summary>
        Task<bool> ForcePasswordResetAsync(long adminId, long updatedBy);

        // ==========================================
        // VALIDATION
        // ==========================================

        /// <summary>
        /// Check if username already exists (excluding specific admin ID if provided)
        /// </summary>
        Task<bool> UsernameExistsAsync(string username, long? excludeAdminId = null);

        /// <summary>
        /// Check if email already exists (excluding specific admin ID if provided)
        /// </summary>
        Task<bool> EmailExistsAsync(string email, long? excludeAdminId = null);

        // ==========================================
        // SUPER ADMIN PROTECTION
        // ==========================================

        /// <summary>
        /// Get count of active Super Admin users
        /// </summary>
        Task<int> GetActiveSuperAdminCountAsync();

        /// <summary>
        /// Check if admin has Super Admin role
        /// </summary>
        Task<bool> IsSuperAdminAsync(long adminId);

        /// <summary>
        /// Check if admin can be deactivated (prevents deactivating last Super Admin)
        /// </summary>
        Task<bool> CanDeactivateAdminAsync(long adminId);

        Task<bool> RemoveAdminLoginAccessAsync(long adminId);
    }
}
