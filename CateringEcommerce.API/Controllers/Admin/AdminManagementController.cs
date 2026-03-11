using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace CateringEcommerce.API.Controllers.Admin
{
    [Route("api/admin/admins")]
    [ApiController]
    [AdminAuthorize]
    public class AdminManagementController : ControllerBase
    {
        private readonly IAdminManagementRepository _adminRepo;
        private readonly IRBACRepository _rbacRepo;

        public AdminManagementController(
            IAdminManagementRepository adminRepo,
            IRBACRepository rbacRepo)
        {
            _adminRepo = adminRepo ?? throw new ArgumentNullException(nameof(adminRepo));
            _rbacRepo = rbacRepo ?? throw new ArgumentNullException(nameof(rbacRepo));
        }

        private (long adminId, string adminName) GetCurrentAdmin()
        {
            var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            var adminNameClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);

            if (adminIdClaim == null || !long.TryParse(adminIdClaim.Value, out long adminId))
            {
                throw new UnauthorizedAccessException("Invalid admin session.");
            }

            return (adminId, adminNameClaim?.Value ?? "Unknown");
        }

        private async Task<bool> CheckPermissionAsync(long adminId, string permissionCode)
        {
            return await _rbacRepo.AdminHasPermissionAsync(adminId, permissionCode) ||
                   await _rbacRepo.IsSuperAdminAsync(adminId);
        }

        private async Task LogAuditAsync(long adminId, string adminName, string action, string module, long? targetId, string? targetType, object? details, string status, string? errorMessage = null)
        {
            await _rbacRepo.LogAuditAsync(new AuditLogEntry
            {
                AdminId = adminId,
                AdminName = adminName,
                Action = action,
                Module = module,
                TargetId = targetId,
                TargetType = targetType,
                Details = details != null ? JsonSerializer.Serialize(details) : null,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                Status = status,
                ErrorMessage = errorMessage
            });
        }

        /// <summary>
        /// Get all admin users with pagination, search, and filters
        /// Permission Required: ADMIN_VIEW or Super Admin
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllAdmins([FromQuery] AdminListRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                // Check permission
                if (!await CheckPermissionAsync(adminId, "ADMIN_VIEW"))
                {
                    await LogAuditAsync(adminId, adminName, "VIEW_ADMINS", "ADMIN", null, null, null, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("You do not have permission to view admin users."));
                }

                var result = await _adminRepo.GetAllAdminsAsync(request);

                await LogAuditAsync(adminId, adminName, "VIEW_ADMINS", "ADMIN", null, null, new { request.PageNumber, request.PageSize, request.SearchTerm }, "SUCCESS");
                return ApiResponseHelper.Success(result, "Admin users retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get admin user details by ID
        /// Permission Required: ADMIN_VIEW or Super Admin
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAdminById(long id)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                // Check permission
                if (!await CheckPermissionAsync(adminId, "ADMIN_VIEW"))
                {
                    await LogAuditAsync(adminId, adminName, "VIEW_ADMIN", "ADMIN", id, "AdminUser", null, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("You do not have permission to view admin users."));
                }

                var admin = await _adminRepo.GetAdminByIdAsync(id);

                if (admin == null)
                {
                    return ApiResponseHelper.Failure("Admin user not found.");
                }

                await LogAuditAsync(adminId, adminName, "VIEW_ADMIN", "ADMIN", id, "AdminUser", null, "SUCCESS");
                return ApiResponseHelper.Success(admin, "Admin user details retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create new admin user
        /// Permission Required: ADMIN_CREATE or Super Admin
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                // Check permission
                if (!await CheckPermissionAsync(adminId, "ADMIN_CREATE"))
                {
                    await LogAuditAsync(adminId, adminName, "CREATE_ADMIN", "ADMIN", null, "AdminUser", request, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("You do not have permission to create admin users."));
                }

                // Check if username or email already exists
                if (await _adminRepo.UsernameExistsAsync(request.Username))
                {
                    return ApiResponseHelper.Failure("Username already exists.");
                }

                if (await _adminRepo.EmailExistsAsync(request.Email))
                {
                    return ApiResponseHelper.Failure("Email already exists.");
                }

                // Prevent non-super-admins from creating Super Admin users
                var roleDetail = await _rbacRepo.GetRoleByIdAsync(request.RoleId);
                if (roleDetail != null && roleDetail.RoleCode == "SUPER_ADMIN")
                {
                    var isSuperAdmin = await _rbacRepo.IsSuperAdminAsync(adminId);
                    if (!isSuperAdmin)
                    {
                        await LogAuditAsync(adminId, adminName, "CREATE_ADMIN", "ADMIN", null, "AdminUser", request, "FORBIDDEN", "Attempted to create Super Admin without permission");
                        return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can create Super Admin users."));
                    }
                }

                request.Password = HashHelper.HashPassword(request.Password); // Hash password before storing
                var newAdminId = await _adminRepo.CreateAdminAsync(request, adminId);

                await LogAuditAsync(adminId, adminName, "CREATE_ADMIN", "ADMIN", newAdminId, "AdminUser", new { request.Username, request.Email, request.RoleId }, "SUCCESS");
                return ApiResponseHelper.Success(new { AdminId = newAdminId }, "Admin user created successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update admin user information
        /// Permission Required: ADMIN_EDIT or Super Admin
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAdmin(long id, [FromBody] UpdateAdminRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                // Check permission
                if (!await CheckPermissionAsync(adminId, "ADMIN_EDIT"))
                {
                    await LogAuditAsync(adminId, adminName, "UPDATE_ADMIN", "ADMIN", id, "AdminUser", request, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("You do not have permission to edit admin users."));
                }

                // Check if admin exists
                var existingAdmin = await _adminRepo.GetAdminByIdAsync(id);
                if (existingAdmin == null)
                {
                    return ApiResponseHelper.Failure("Admin user not found.");
                }

                // Check if email already exists (excluding current admin)
                if (await _adminRepo.EmailExistsAsync(request.Email, id))
                {
                    return ApiResponseHelper.Failure("Email already exists.");
                }

                request.AdminId = id;
                var success = await _adminRepo.UpdateAdminAsync(request, adminId);

                if (!success)
                {
                    return ApiResponseHelper.Failure("Failed to update admin user.");
                }

                await LogAuditAsync(adminId, adminName, "UPDATE_ADMIN", "ADMIN", id, "AdminUser", request, "SUCCESS");
                return ApiResponseHelper.Success(null, "Admin user updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update admin user status (Activate/Deactivate)
        /// Permission Required: Super Admin Only
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateAdminStatus(long id, [FromBody] UpdateAdminStatusRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                // Only Super Admin can change admin status
                if (!await _rbacRepo.IsSuperAdminAsync(adminId))
                {
                    await LogAuditAsync(adminId, adminName, "UPDATE_ADMIN_STATUS", "ADMIN", id, "AdminUser", request, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can change admin status."));
                }

                // Prevent self-deactivation
                if (id == adminId && !request.IsActive)
                {
                    return ApiResponseHelper.Failure("You cannot deactivate your own account.");
                }

                // Check if admin can be deactivated (prevent last Super Admin deactivation)
                if (!request.IsActive && !await _adminRepo.CanDeactivateAdminAsync(id))
                {
                    await LogAuditAsync(adminId, adminName, "UPDATE_ADMIN_STATUS", "ADMIN", id, "AdminUser", request, "FORBIDDEN", "Attempted to deactivate last Super Admin");
                    return ApiResponseHelper.Failure("Cannot deactivate the last active Super Admin.");
                }

                var success = await _adminRepo.UpdateAdminStatusAsync(id, request.IsActive, adminId);

                if (!success)
                {
                    return ApiResponseHelper.Failure("Failed to update admin status.");
                }

                await LogAuditAsync(adminId, adminName, "UPDATE_ADMIN_STATUS", "ADMIN", id, "AdminUser", request, "SUCCESS");
                return ApiResponseHelper.Success(null, $"Admin user {(request.IsActive ? "activated" : "deactivated")} successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Assign role to admin user
        /// Permission Required: ADMIN_ASSIGN_ROLE or Super Admin
        /// </summary>
        [HttpPut("{id}/role")]
        public async Task<IActionResult> AssignRole(long id, [FromBody] AssignRoleRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                // Check permission
                if (!await CheckPermissionAsync(adminId, "ADMIN_ASSIGN_ROLE"))
                {
                    await LogAuditAsync(adminId, adminName, "ASSIGN_ROLE", "ADMIN", id, "AdminUser", request, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("You do not have permission to assign roles."));
                }

                // Prevent non-super-admins from assigning Super Admin role
                var roleDetail = await _rbacRepo.GetRoleByIdAsync(request.RoleId);
                if (roleDetail != null && roleDetail.RoleCode == "SUPER_ADMIN")
                {
                    var isSuperAdmin = await _rbacRepo.IsSuperAdminAsync(adminId);
                    if (!isSuperAdmin)
                    {
                        await LogAuditAsync(adminId, adminName, "ASSIGN_ROLE", "ADMIN", id, "AdminUser", request, "FORBIDDEN", "Attempted to assign Super Admin role without permission");
                        return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can assign the Super Admin role."));
                    }
                }

                request.AdminId = id;
                var success = await _adminRepo.AssignRoleToAdminAsync(id, request.RoleId, adminId);

                if (!success)
                {
                    return ApiResponseHelper.Failure("Failed to assign role.");
                }

                await LogAuditAsync(adminId, adminName, "ASSIGN_ROLE", "ADMIN", id, "AdminUser", request, "SUCCESS");
                return ApiResponseHelper.Success(null, "Role assigned successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Reset admin password
        /// Permission Required: Super Admin Only
        /// </summary>
        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(long id, [FromBody] ResetAdminPasswordRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                // Only Super Admin can reset passwords
                if (!await _rbacRepo.IsSuperAdminAsync(adminId))
                {
                    await LogAuditAsync(adminId, adminName, "RESET_ADMIN_PASSWORD", "ADMIN", id, "AdminUser", null, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can reset admin passwords."));
                }

                request.AdminId = id;
                var success = await _adminRepo.ResetAdminPasswordAsync(id, request.NewPasswordHash, request.ForcePasswordReset, adminId);

                if (!success)
                {
                    return ApiResponseHelper.Failure("Failed to reset password.");
                }

                await LogAuditAsync(adminId, adminName, "RESET_ADMIN_PASSWORD", "ADMIN", id, "AdminUser", new { request.ForcePasswordReset }, "SUCCESS");
                return ApiResponseHelper.Success(null, "Admin password reset successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete (soft delete) admin user
        /// Permission Required: ADMIN_DELETE or Super Admin
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdmin(long id)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                // Check permission
                if (!await CheckPermissionAsync(adminId, "ADMIN_DELETE"))
                {
                    await LogAuditAsync(adminId, adminName, "DELETE_ADMIN", "ADMIN", id, "AdminUser", null, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("You do not have permission to delete admin users."));
                }

                // Prevent self-deletion
                if (id == adminId)
                {
                    return ApiResponseHelper.Failure("You cannot delete your own account.");
                }

                // Check if admin can be deleted (prevent last Super Admin deletion)
                if (!await _adminRepo.CanDeactivateAdminAsync(id))
                {
                    await LogAuditAsync(adminId, adminName, "DELETE_ADMIN", "ADMIN", id, "AdminUser", null, "FORBIDDEN", "Attempted to delete last Super Admin");
                    return ApiResponseHelper.Failure("Cannot delete the last active Super Admin.");
                }

                // Delete Admin Users 
                await _adminRepo.RemoveAdminLoginAccessAsync(id); // Remove admin user associations with admin

                var success = await _adminRepo.DeleteAdminAsync(id, adminId);

                if (!success)
                {
                    return ApiResponseHelper.Failure("Failed to delete admin user.");
                }

                await LogAuditAsync(adminId, adminName, "DELETE_ADMIN", "ADMIN", id, "AdminUser", null, "SUCCESS");
                return ApiResponseHelper.Success(null, "Admin user deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Check if username exists
        /// </summary>
        [HttpGet("check-username")]
        public async Task<IActionResult> CheckUsername([FromQuery] string username, [FromQuery] long? excludeAdminId = null)
        {
            try
            {
                var exists = await _adminRepo.UsernameExistsAsync(username, excludeAdminId);
                return ApiResponseHelper.Success(new { Exists = exists }, null);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Check if email exists
        /// </summary>
        [HttpGet("check-email")]
        public async Task<IActionResult> CheckEmail([FromQuery] string email, [FromQuery] long? excludeAdminId = null)
        {
            try
            {
                var exists = await _adminRepo.EmailExistsAsync(email, excludeAdminId);
                return ApiResponseHelper.Success(new { Exists = exists }, null);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }
    }
}
