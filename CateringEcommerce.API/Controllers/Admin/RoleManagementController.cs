using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.Admin;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace CateringEcommerce.API.Controllers.Admin
{
    [Route("api/admin/roles")]
    [ApiController]
    [AdminAuthorize]
    public class RoleManagementController : ControllerBase
    {
        private readonly IRBACRepository _iRBACRepository;

        public RoleManagementController(IRBACRepository rbacRepository)
        {
            _iRBACRepository = rbacRepository;
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
            return await _iRBACRepository.AdminHasPermissionAsync(adminId, permissionCode) ||
                   await _iRBACRepository.IsSuperAdminAsync(adminId);
        }

        private async Task LogAuditAsync(long adminId, string adminName, string action, string module, long? targetId, string? targetType, object? details, string status, string? errorMessage = null)
        {
            await _iRBACRepository.LogAuditAsync(new AuditLogEntry
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
        /// Get all roles (for dropdown binding - accessible to all admins)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                var roles = await _iRBACRepository.GetAllRolesAsync();

                // Filter Super Admin role for non-super-admins
                var isSuperAdmin = await _iRBACRepository.IsSuperAdminAsync(adminId);
                if (!isSuperAdmin)
                {
                    roles = roles.Where(r => r.RoleCode != "SUPER_ADMIN").ToList();
                }

                return ApiResponseHelper.Success(roles, "Roles retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get role details by ID with permissions
        /// Permission Required: ROLE_VIEW or Super Admin
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(long id)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                // Check permission
                if (!await CheckPermissionAsync(adminId, "ROLE_VIEW"))
                {
                    await LogAuditAsync(adminId, adminName, "VIEW_ROLE", "SYSTEM", id, "Role", null, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("You do not have permission to view roles."));
                }

                var role = await _iRBACRepository.GetRoleByIdAsync(id);

                if (role == null)
                {
                    return ApiResponseHelper.Failure("Role not found.");
                }

                await LogAuditAsync(adminId, adminName, "VIEW_ROLE", "SYSTEM", id, "Role", null, "SUCCESS");
                return ApiResponseHelper.Success(role, "Role details retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create new role
        /// Permission Required: ROLE_CREATE or Super Admin
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                // Check permission
                if (!await CheckPermissionAsync(adminId, "ROLE_CREATE"))
                {
                    await LogAuditAsync(adminId, adminName, "CREATE_ROLE", "SYSTEM", null, "Role", request, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("You do not have permission to create roles."));
                }

                // Check if role code already exists
                if (await _iRBACRepository.RoleCodeExistsAsync(request.RoleCode))
                {
                    return ApiResponseHelper.Failure("Role code already exists.");
                }

                var roleId = await _iRBACRepository.CreateRoleAsync(request, adminId);

                await LogAuditAsync(adminId, adminName, "CREATE_ROLE", "SYSTEM", roleId, "Role", request, "SUCCESS");
                return ApiResponseHelper.Success(new { RoleId = roleId }, "Role created successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update role
        /// Permission Required: ROLE_EDIT or Super Admin
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(long id, [FromBody] UpdateRoleRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                // Check permission
                if (!await CheckPermissionAsync(adminId, "ROLE_EDIT"))
                {
                    await LogAuditAsync(adminId, adminName, "UPDATE_ROLE", "SYSTEM", id, "Role", request, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("You do not have permission to edit roles."));
                }

                // Check if role exists and is not a system role
                var existingRole = await _iRBACRepository.GetRoleByIdAsync(id);
                if (existingRole == null)
                {
                    return ApiResponseHelper.Failure("Role not found.");
                }

                if (existingRole.IsSystemRole)
                {
                    return ApiResponseHelper.Failure("Cannot modify system roles.");
                }

                request.RoleId = id;
                var success = await _iRBACRepository.UpdateRoleAsync(request, adminId);

                if (!success)
                {
                    return ApiResponseHelper.Failure("Failed to update role.");
                }

                await LogAuditAsync(adminId, adminName, "UPDATE_ROLE", "SYSTEM", id, "Role", request, "SUCCESS");
                return ApiResponseHelper.Success(null, "Role updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete role (soft delete)
        /// Permission Required: ROLE_DELETE or Super Admin
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(long id)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                // Check permission
                if (!await CheckPermissionAsync(adminId, "ROLE_DELETE"))
                {
                    await LogAuditAsync(adminId, adminName, "DELETE_ROLE", "SYSTEM", id, "Role", null, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("You do not have permission to delete roles."));
                }

                // Check if role exists and is not a system role
                var existingRole = await _iRBACRepository.GetRoleByIdAsync(id);
                if (existingRole == null)
                {
                    return ApiResponseHelper.Failure("Role not found.");
                }

                if (existingRole.IsSystemRole)
                {
                    return ApiResponseHelper.Failure("Cannot delete system roles.");
                }

                var success = await _iRBACRepository.DeleteRoleAsync(id, adminId);

                if (!success)
                {
                    return ApiResponseHelper.Failure("Failed to delete role.");
                }

                await LogAuditAsync(adminId, adminName, "DELETE_ROLE", "SYSTEM", id, "Role", null, "SUCCESS");
                return ApiResponseHelper.Success(null, "Role deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get all permissions grouped by module (accessible to all admins)
        /// </summary>
        [HttpGet("~/api/admin/permissions")]
        public async Task<IActionResult> GetAllPermissions()
        {
            try
            {
                var permissions = await _iRBACRepository.GetAllPermissionsAsync();

                return ApiResponseHelper.Success(permissions, "Permissions retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }
    }
}
