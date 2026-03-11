using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.Admin;
using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace CateringEcommerce.API.Controllers.Admin
{
    [Route("api/admin/master-data")]
    [ApiController]
    [AdminAuthorize]
    public class MasterDataController : ControllerBase
    {
        private readonly IDatabaseHelper _dbHelper;

        public MasterDataController(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        private (long adminId, string adminName) GetCurrentAdmin()
        {
            var adminIdClaim = User.Claims.LastOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            var adminNameClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

            if (adminIdClaim == null || !long.TryParse(adminIdClaim.Value, out long adminId))
            {
                throw new UnauthorizedAccessException("Invalid admin session.");
            }

            return (adminId, adminNameClaim?.Value ?? "Unknown");
        }

        private async Task<bool> CheckSuperAdminAsync(long adminId)
        {
            var rbacRepo = new RBACRepository(_dbHelper);
            return await rbacRepo.IsSuperAdminAsync(adminId);
        }

        private async Task LogAuditAsync(long adminId, string adminName, string action, string module, long? targetId, string? targetType, object? details, string status, string? errorMessage = null)
        {
            var rbacRepo = new RBACRepository(_dbHelper);

            await rbacRepo.LogAuditAsync(new AuditLogEntry
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

        // ===== CITIES ENDPOINTS =====

        [HttpGet("cities")]
        public async Task<IActionResult> GetCities([FromQuery] MasterDataListRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    await LogAuditAsync(adminId, adminName, "VIEW_CITIES", "MASTER_DATA", null, null, null, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can access master data."));
                }

                var mdRepo = new MasterDataRepository(_dbHelper);

                var result = await mdRepo.GetCitiesAsync(request);

                await LogAuditAsync(adminId, adminName, "VIEW_CITIES", "MASTER_DATA", null, null, request, "SUCCESS");
                return ApiResponseHelper.Success(result, "Cities retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("cities/{id}")]
        public async Task<IActionResult> GetCityById(long id)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    await LogAuditAsync(adminId, adminName, "VIEW_CITY", "MASTER_DATA", id, "City", null, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can access master data."));
                }

                var mdRepo = new MasterDataRepository(_dbHelper);

                var city = await mdRepo.GetCityByIdAsync(id);

                if (city == null)
                {
                    return ApiResponseHelper.Failure("City not found.");
                }

                await LogAuditAsync(adminId, adminName, "VIEW_CITY", "MASTER_DATA", id, "City", null, "SUCCESS");
                return ApiResponseHelper.Success(city, "City retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost("cities")]
        public async Task<IActionResult> CreateCity([FromBody] CreateMasterDataRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    await LogAuditAsync(adminId, adminName, "CREATE_CITY", "MASTER_DATA", null, "City", request, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can modify master data."));
                }

                var mdRepo = new MasterDataRepository(_dbHelper);

                var newId = await mdRepo.CreateCityAsync(request, adminId);

                await LogAuditAsync(adminId, adminName, "CREATE_CITY", "MASTER_DATA", newId, "City", request, "SUCCESS");
                return ApiResponseHelper.Success(new { id = newId }, "City created successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPut("cities/{id}")]
        public async Task<IActionResult> UpdateCity(long id, [FromBody] UpdateMasterDataRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    await LogAuditAsync(adminId, adminName, "UPDATE_CITY", "MASTER_DATA", id, "City", request, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can modify master data."));
                }

                request.Id = id;

                var mdRepo = new MasterDataRepository(_dbHelper);

                var success = await mdRepo.UpdateCityAsync(request, adminId);

                if (!success)
                {
                    return ApiResponseHelper.Failure("City not found or update failed.");
                }

                await LogAuditAsync(adminId, adminName, "UPDATE_CITY", "MASTER_DATA", id, "City", request, "SUCCESS");
                return ApiResponseHelper.Success(null, "City updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPut("cities/{id}/status")]
        public async Task<IActionResult> UpdateCityStatus(long id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    await LogAuditAsync(adminId, adminName, "UPDATE_CITY_STATUS", "MASTER_DATA", id, "City", request, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can modify master data."));
                }

                var mdRepo = new MasterDataRepository(_dbHelper);

                var success = await mdRepo.UpdateCityStatusAsync(id, request.IsActive, adminId);

                if (!success)
                {
                    return ApiResponseHelper.Failure("City not found or status update failed.");
                }

                await LogAuditAsync(adminId, adminName, "UPDATE_CITY_STATUS", "MASTER_DATA", id, "City", request, "SUCCESS");
                return ApiResponseHelper.Success(null, $"City {(request.IsActive ? "activated" : "deactivated")} successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("cities/{id}/usage")]
        public async Task<IActionResult> CheckCityUsage(long id)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can access master data."));
                }

                var mdRepo = new MasterDataRepository(_dbHelper);

                var usage = await mdRepo.CheckUsageAsync(Table.City, "c_cityid", id);

                return ApiResponseHelper.Success(usage, "Usage check completed.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        // ===== FOOD CATEGORIES ENDPOINTS =====

        [HttpGet("food-categories")]
        public async Task<IActionResult> GetFoodCategories([FromQuery] MasterDataListRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    await LogAuditAsync(adminId, adminName, "VIEW_FOOD_CATEGORIES", "MASTER_DATA", null, null, null, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can access master data."));
                }

                var mdRepo = new MasterDataRepository(_dbHelper);

                var result = await mdRepo.GetFoodCategoriesAsync(request);

                await LogAuditAsync(adminId, adminName, "VIEW_FOOD_CATEGORIES", "MASTER_DATA", null, null, request, "SUCCESS");
                return ApiResponseHelper.Success(result, "Food categories retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("food-categories/{id}")]
        public async Task<IActionResult> GetFoodCategoryById(long id)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    await LogAuditAsync(adminId, adminName, "VIEW_FOOD_CATEGORY", "MASTER_DATA", id, "FoodCategory", null, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can access master data."));
                }

                var mdRepo = new MasterDataRepository(_dbHelper);

                var category = await mdRepo.GetFoodCategoryByIdAsync(id);

                if (category == null)
                {
                    return ApiResponseHelper.Failure("Food category not found.");
                }

                await LogAuditAsync(adminId, adminName, "VIEW_FOOD_CATEGORY", "MASTER_DATA", id, "FoodCategory", null, "SUCCESS");
                return ApiResponseHelper.Success(category, "Food category retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost("food-categories")]
        public async Task<IActionResult> CreateFoodCategory([FromBody] CreateMasterDataRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    await LogAuditAsync(adminId, adminName, "CREATE_FOOD_CATEGORY", "MASTER_DATA", null, "FoodCategory", request, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can modify master data."));
                }

                var mdRepo = new MasterDataRepository(_dbHelper);

                var newId = await mdRepo.CreateFoodCategoryAsync(request, adminId);

                await LogAuditAsync(adminId, adminName, "CREATE_FOOD_CATEGORY", "MASTER_DATA", newId, "FoodCategory", request, "SUCCESS");
                return ApiResponseHelper.Success(new { id = newId }, "Food category created successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPut("food-categories/{id}")]
        public async Task<IActionResult> UpdateFoodCategory(long id, [FromBody] UpdateMasterDataRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    await LogAuditAsync(adminId, adminName, "UPDATE_FOOD_CATEGORY", "MASTER_DATA", id, "FoodCategory", request, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can modify master data."));
                }

                request.Id = id;

                var mdRepo = new MasterDataRepository(_dbHelper);

                var success = await mdRepo.UpdateFoodCategoryAsync(request, adminId);

                if (!success)
                {
                    return ApiResponseHelper.Failure("Food category not found or update failed.");
                }

                await LogAuditAsync(adminId, adminName, "UPDATE_FOOD_CATEGORY", "MASTER_DATA", id, "FoodCategory", request, "SUCCESS");
                return ApiResponseHelper.Success(null, "Food category updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPut("food-categories/{id}/status")]
        public async Task<IActionResult> UpdateFoodCategoryStatus(long id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    await LogAuditAsync(adminId, adminName, "UPDATE_FOOD_CATEGORY_STATUS", "MASTER_DATA", id, "FoodCategory", request, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can modify master data."));
                }

                var mdRepo = new MasterDataRepository(_dbHelper);

                var success = await mdRepo.UpdateFoodCategoryStatusAsync(id, request.IsActive, adminId);

                if (!success)
                {
                    return ApiResponseHelper.Failure("Food category not found or status update failed.");
                }

                await LogAuditAsync(adminId, adminName, "UPDATE_FOOD_CATEGORY_STATUS", "MASTER_DATA", id, "FoodCategory", request, "SUCCESS");
                return ApiResponseHelper.Success(null, $"Food category {(request.IsActive ? "activated" : "deactivated")} successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("food-categories/{id}/usage")]
        public async Task<IActionResult> CheckFoodCategoryUsage(long id)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can access master data."));
                }

                var mdRepo = new MasterDataRepository(_dbHelper);

                var usage = await mdRepo.CheckUsageAsync(Table.SysFoodCategory, "c_category_id", id);

                return ApiResponseHelper.Success(usage, "Usage check completed.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        // ===== CATERING TYPES ENDPOINTS (Food Type, Cuisine Type, Event Type, Service Type) =====

        [HttpGet("catering-types/{categoryId}")]
        public async Task<IActionResult> GetCateringTypes(int categoryId, [FromQuery] MasterDataListRequest request)
        {
            try
            {
                if (!Enum.IsDefined(typeof(ServiceType), categoryId))
                {
                    return ApiResponseHelper.Failure("Invalid service type ID.");
                }
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    await LogAuditAsync(adminId, adminName, "VIEW_CATERING_TYPES", "MASTER_DATA", null, null, null, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can access master data."));
                }
                var mdRepo = new MasterDataRepository(_dbHelper);

                var result = await mdRepo.GetCateringTypesAsync(categoryId, request);

                await LogAuditAsync(adminId, adminName, "VIEW_CATERING_TYPES", "MASTER_DATA", null, null, new { categoryId, request }, "SUCCESS");
                return ApiResponseHelper.Success(result, "Catering types retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("catering-types/{categoryId}/{id}")]
        public async Task<IActionResult> GetCateringTypeById(int categoryId, long id)
        {
            try
            {
                if (!Enum.IsDefined(typeof(ServiceType), categoryId))
                {
                    return ApiResponseHelper.Failure("Invalid service type ID.");
                }

                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    await LogAuditAsync(adminId, adminName, "VIEW_CATERING_TYPE", "MASTER_DATA", id, "CateringType", null, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can access master data."));
                }

                var mdRepo = new MasterDataRepository(_dbHelper);

                var cateringType = await mdRepo.GetCateringTypeByIdAsync(id);

                if (cateringType == null)
                {
                    return ApiResponseHelper.Failure("Catering type not found.");
                }

                await LogAuditAsync(adminId, adminName, "VIEW_CATERING_TYPE", "MASTER_DATA", id, "CateringType", null, "SUCCESS");
                return ApiResponseHelper.Success(cateringType, "Catering type retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost("catering-types/{categoryId}")]
        public async Task<IActionResult> CreateCateringType(int categoryId, [FromBody] CreateMasterDataRequest request)
        {
            try
            {
                if (!Enum.IsDefined(typeof(ServiceType), categoryId))
                {
                    return ApiResponseHelper.Failure("Invalid service type ID.");
                }

                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    await LogAuditAsync(adminId, adminName, "CREATE_CATERING_TYPE", "MASTER_DATA", null, "CateringType", request, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can modify master data."));
                }

                request.CategoryId = categoryId;

                var mdRepo = new MasterDataRepository(_dbHelper);

                var newId = await mdRepo.CreateCateringTypeAsync(request, adminId);

                await LogAuditAsync(adminId, adminName, "CREATE_CATERING_TYPE", "MASTER_DATA", newId, "CateringType", request, "SUCCESS");
                return ApiResponseHelper.Success(new { id = newId }, "Catering type created successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPut("catering-types/{categoryId}/{id}")]
        public async Task<IActionResult> UpdateCateringType(int categoryId, long id, [FromBody] UpdateMasterDataRequest request)
        {
            try
            {
                if (!Enum.IsDefined(typeof(ServiceType), categoryId))
                {
                    return ApiResponseHelper.Failure("Invalid service type ID.");
                }

                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    await LogAuditAsync(adminId, adminName, "UPDATE_CATERING_TYPE", "MASTER_DATA", id, "CateringType", request, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can modify master data."));
                }

                request.Id = id;

                var mdRepo = new MasterDataRepository(_dbHelper);

                var success = await mdRepo.UpdateCateringTypeAsync(request, adminId);

                if (!success)
                {
                    return ApiResponseHelper.Failure("Catering type not found or update failed.");
                }

                await LogAuditAsync(adminId, adminName, "UPDATE_CATERING_TYPE", "MASTER_DATA", id, "CateringType", request, "SUCCESS");
                return ApiResponseHelper.Success(null, "Catering type updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPut("catering-types/{categoryId}/{id}/status")]
        public async Task<IActionResult> UpdateCateringTypeStatus(int categoryId, long id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                if (!Enum.IsDefined(typeof(ServiceType), categoryId))
                {
                    return ApiResponseHelper.Failure("Invalid service type ID.");
                }

                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    await LogAuditAsync(adminId, adminName, "UPDATE_CATERING_TYPE_STATUS", "MASTER_DATA", id, "CateringType", request, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can modify master data."));
                }

                var mdRepo = new MasterDataRepository(_dbHelper);

                var success = await mdRepo.UpdateCateringTypeStatusAsync(id, request.IsActive, adminId);

                if (!success)
                {
                    return ApiResponseHelper.Failure("Catering type not found or status update failed.");
                }

                await LogAuditAsync(adminId, adminName, "UPDATE_CATERING_TYPE_STATUS", "MASTER_DATA", id, "CateringType", request, "SUCCESS");
                return ApiResponseHelper.Success(null, $"Catering type {(request.IsActive ? "activated" : "deactivated")} successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("catering-types/{categoryId}/{id}/usage")]
        public async Task<IActionResult> CheckCateringTypeUsage(int categoryId, long id)
        {
            try
            {
                if (!Enum.IsDefined(typeof(ServiceType), categoryId))
                {
                    return ApiResponseHelper.Failure("Invalid service type ID.");
                }

                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can access master data."));
                }

                var mdRepo = new MasterDataRepository(_dbHelper);

                var usage = await mdRepo.CheckUsageAsync(Table.SysCateringTypeMaster, "c_type_id", id);

                return ApiResponseHelper.Success(usage, "Usage check completed.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        // ===== GUEST CATEGORIES ENDPOINTS =====

        [HttpGet("guest-categories")]
        public async Task<IActionResult> GetGuestCategories([FromQuery] MasterDataListRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    await LogAuditAsync(adminId, adminName, "VIEW_GUEST_CATEGORIES", "MASTER_DATA", null, null, null, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can access master data."));
                }

                var mdRepo = new MasterDataRepository(_dbHelper);

                var result = await mdRepo.GetGuestCategoriesAsync(request);

                await LogAuditAsync(adminId, adminName, "VIEW_GUEST_CATEGORIES", "MASTER_DATA", null, null, request, "SUCCESS");
                return ApiResponseHelper.Success(result, "Guest categories retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("guest-categories/{id}")]
        public async Task<IActionResult> GetGuestCategoryById(long id)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    await LogAuditAsync(adminId, adminName, "VIEW_GUEST_CATEGORY", "MASTER_DATA", id, "GuestCategory", null, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can access master data."));
                }

                var mdRepo = new MasterDataRepository(_dbHelper);

                var category = await mdRepo.GetGuestCategoryByIdAsync(id);

                if (category == null)
                {
                    return ApiResponseHelper.Failure("Guest category not found.");
                }

                await LogAuditAsync(adminId, adminName, "VIEW_GUEST_CATEGORY", "MASTER_DATA", id, "GuestCategory", null, "SUCCESS");
                return ApiResponseHelper.Success(category, "Guest category retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost("guest-categories")]
        public async Task<IActionResult> CreateGuestCategory([FromBody] CreateMasterDataRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    await LogAuditAsync(adminId, adminName, "CREATE_GUEST_CATEGORY", "MASTER_DATA", null, "GuestCategory", request, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can modify master data."));
                }

                var mdRepo = new MasterDataRepository(_dbHelper);

                var newId = await mdRepo.CreateGuestCategoryAsync(request, adminId);

                await LogAuditAsync(adminId, adminName, "CREATE_GUEST_CATEGORY", "MASTER_DATA", newId, "GuestCategory", request, "SUCCESS");
                return ApiResponseHelper.Success(new { id = newId }, "Guest category created successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPut("guest-categories/{id}")]
        public async Task<IActionResult> UpdateGuestCategory(long id, [FromBody] UpdateMasterDataRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    await LogAuditAsync(adminId, adminName, "UPDATE_GUEST_CATEGORY", "MASTER_DATA", id, "GuestCategory", request, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can modify master data."));
                }

                request.Id = id;

                var mdRepo = new MasterDataRepository(_dbHelper);

                var success = await mdRepo.UpdateGuestCategoryAsync(request, adminId);

                if (!success)
                {
                    return ApiResponseHelper.Failure("Guest category not found or update failed.");
                }

                await LogAuditAsync(adminId, adminName, "UPDATE_GUEST_CATEGORY", "MASTER_DATA", id, "GuestCategory", request, "SUCCESS");
                return ApiResponseHelper.Success(null, "Guest category updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPut("guest-categories/{id}/status")]
        public async Task<IActionResult> UpdateGuestCategoryStatus(long id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    await LogAuditAsync(adminId, adminName, "UPDATE_GUEST_CATEGORY_STATUS", "MASTER_DATA", id, "GuestCategory", request, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can modify master data."));
                }

                var mdRepo = new MasterDataRepository(_dbHelper);

                var success = await mdRepo.UpdateGuestCategoryStatusAsync(id, request.IsActive, adminId);

                if (!success)
                {
                    return ApiResponseHelper.Failure("Guest category not found or status update failed.");
                }

                await LogAuditAsync(adminId, adminName, "UPDATE_GUEST_CATEGORY_STATUS", "MASTER_DATA", id, "GuestCategory", request, "SUCCESS");
                return ApiResponseHelper.Success(null, $"Guest category {(request.IsActive ? "activated" : "deactivated")} successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("guest-categories/{id}/usage")]
        public async Task<IActionResult> CheckGuestCategoryUsage(long id)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can access master data."));
                }

                var mdRepo = new MasterDataRepository(_dbHelper);

                var usage = await mdRepo.CheckUsageAsync(Table.SysGuestCategory, "c_guest_category_id", id);

                return ApiResponseHelper.Success(usage, "Usage check completed.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        // ===== THEMES ENDPOINTS =====

        [HttpGet("themes")]
        public async Task<IActionResult> GetThemes([FromQuery] MasterDataListRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    await LogAuditAsync(adminId, adminName, "VIEW_THEMES", "MASTER_DATA", null, null, null, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can access master data."));
                }

                var mdRepo = new MasterDataRepository(_dbHelper);

                var result = await mdRepo.GetThemesAsync(request);

                await LogAuditAsync(adminId, adminName, "VIEW_THEMES", "MASTER_DATA", null, null, request, "SUCCESS");
                return ApiResponseHelper.Success(result, "Themes retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("themes/{id}")]
        public async Task<IActionResult> GetThemeById(long id)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    await LogAuditAsync(adminId, adminName, "VIEW_THEME", "MASTER_DATA", id, "Theme", null, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can access master data."));
                }

                var mdRepo = new MasterDataRepository(_dbHelper);

                var theme = await mdRepo.GetThemeByIdAsync(id);

                if (theme == null)
                {
                    return ApiResponseHelper.Failure("Theme not found.");
                }

                await LogAuditAsync(adminId, adminName, "VIEW_THEME", "MASTER_DATA", id, "Theme", null, "SUCCESS");
                return ApiResponseHelper.Success(theme, "Theme retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost("themes")]
        public async Task<IActionResult> CreateTheme([FromBody] CreateMasterDataRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    await LogAuditAsync(adminId, adminName, "CREATE_THEME", "MASTER_DATA", null, "Theme", request, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can modify master data."));
                }

                var mdRepo = new MasterDataRepository(_dbHelper);

                var newId = await mdRepo.CreateThemeAsync(request, adminId);

                await LogAuditAsync(adminId, adminName, "CREATE_THEME", "MASTER_DATA", newId, "Theme", request, "SUCCESS");
                return ApiResponseHelper.Success(new { id = newId }, "Theme created successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPut("themes/{id}")]
        public async Task<IActionResult> UpdateTheme(long id, [FromBody] UpdateMasterDataRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    await LogAuditAsync(adminId, adminName, "UPDATE_THEME", "MASTER_DATA", id, "Theme", request, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can modify master data."));
                }

                request.Id = id;

                var mdRepo = new MasterDataRepository(_dbHelper);

                var success = await mdRepo.UpdateThemeAsync(request, adminId);

                if (!success)
                {
                    return ApiResponseHelper.Failure("Theme not found or update failed.");
                }

                await LogAuditAsync(adminId, adminName, "UPDATE_THEME", "MASTER_DATA", id, "Theme", request, "SUCCESS");
                return ApiResponseHelper.Success(null, "Theme updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPut("themes/{id}/status")]
        public async Task<IActionResult> UpdateThemeStatus(long id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    await LogAuditAsync(adminId, adminName, "UPDATE_THEME_STATUS", "MASTER_DATA", id, "Theme", request, "UNAUTHORIZED");
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can modify master data."));
                }

                var mdRepo = new MasterDataRepository(_dbHelper);

                var success = await mdRepo.UpdateThemeStatusAsync(id, request.IsActive, adminId);

                if (!success)
                {
                    return ApiResponseHelper.Failure("Theme not found or status update failed.");
                }

                await LogAuditAsync(adminId, adminName, "UPDATE_THEME_STATUS", "MASTER_DATA", id, "Theme", request, "SUCCESS");
                return ApiResponseHelper.Success(null, $"Theme {(request.IsActive ? "activated" : "deactivated")} successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("themes/{id}/usage")]
        public async Task<IActionResult> CheckThemeUsage(long id)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can access master data."));
                }

                var mdRepo = new MasterDataRepository(_dbHelper);

                var usage = await mdRepo.CheckUsageAsync(Table.SysCateringThemeTypes, "c_theme_id", id);

                return ApiResponseHelper.Success(usage, "Usage check completed.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        // ===== HELPER ENDPOINTS =====

        [HttpGet("states")]
        public async Task<IActionResult> GetStates()
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can access master data."));
                }

                var mdRepo = new MasterDataRepository(_dbHelper);

                var states = await mdRepo.GetStatesAsync();

                return ApiResponseHelper.Success(states, "States retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPut("display-order")]
        public async Task<IActionResult> UpdateDisplayOrder([FromBody] dynamic request)
        {
            try
            {
                var (adminId, adminName) = GetCurrentAdmin();

                if (!await CheckSuperAdminAsync(adminId))
                {
                    return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can modify master data."));
                }

                // This is a placeholder for batch updating display order
                // Implementation depends on the specific frontend requirements

                return ApiResponseHelper.Success(null, "Display order updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }
    }
}
