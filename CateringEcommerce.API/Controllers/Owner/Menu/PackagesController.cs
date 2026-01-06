using CateringEcommerce.BAL.Base.Owner.Menu;
using CateringEcommerce.Domain.Interfaces.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CateringEcommerce.Domain.Models.Owner;
using CateringEcommerce.API.Helpers;

namespace CateringEcommerce.API.Controllers.Owner.Menu
{

    [ApiController]
    [Route("api/Owner/Menu/Packages")]
    [Authorize(Roles = "Owner")]
    public class PackagesController : ControllerBase
    {
        private readonly string _connStr;
        private readonly ILogger<PackagesController> _logger;
        private readonly ICurrentUserService _currentUser;

        public PackagesController(ILogger<PackagesController> logger, IConfiguration configuration, ICurrentUserService currentUser)
        {
            _logger = logger;
            _connStr = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
            _currentUser = currentUser;
        }

        [HttpGet("GetFoodCategory")]
        public async Task<IActionResult> GetFoodCategory()
        {
            try
            {
                _logger.LogInformation("Fetching food categories.");
                Packages packages = new Packages(_connStr);
                List<FoodCategoryDto> listFoodCategory = await packages.GetCategories();
                _logger.LogInformation("Fetched {Count} food categories.", listFoodCategory?.Count ?? 0);
                return Ok(listFoodCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while fetching food category.");
                return StatusCode(500, "An error occurred while fetching food category.");
            }
        }

        [HttpGet("Count")]
        public async Task<IActionResult> GetCount(string searchPackage = "")
        {
            try
            {
                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }
                _logger.LogInformation("Get packages counts");
                Packages packages = new Packages(_connStr);
                Int32 packageCount = await packages.GetPackageCount(ownerPKID, searchPackage);
                _logger.LogInformation("Fetched {Count} packages.", packageCount);
                return Ok(packageCount);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while fatching packages counts");
                throw new Exception(ex.Message);
            }
        }

        [HttpGet("Data")]
        public async Task<IActionResult> GetPackages(int page = 1, int pageSize = 10, string searchPackage = "")
        {
            try
            {
                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;


                _logger.LogInformation("Fetching packages.");
                Packages packages = new Packages(_connStr);
                var listPackages = await packages.GetPackages(ownerPKID, page, pageSize, searchPackage);
                _logger.LogInformation("Fetched {Count} packages.", listPackages?.Count ?? 0);
                return Ok(listPackages ?? new List<PackageDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching packages.");
                throw new Exception(ex.Message);
            }
        }

        [HttpPost("AddPackage")]
        public async Task<IActionResult> AddPackage([FromBody] PackageDto packageDto)
        {
            try
            {
                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }
                _logger.LogInformation("Adding new package for owner PKID: {OwnerPKID}", ownerPKID);
                Packages packages = new Packages(_connStr);

                // Check the Package Name same exists or not
                if (packages.PackageExistOrNot(ownerPKID, packageDto.Name))
                {
                    return ApiResponseHelper.Failure("Package name is already exists.", "warning");
                }

                Int64 packageId = await packages.AddPackage(ownerPKID, packageDto);
                _logger.LogInformation("Package added with ID: {PackageID}", packageId);
                foreach (var item in packageDto.Items)
                {
                    await packages.AddPackageItems(packageId, item);
                    _logger.LogInformation("Added item to package ID {PackageID}: CategoryID {CategoryID}, Quantity {Quantity}", packageId, item.CategoryId, item.Quantity);
                }
                return ApiResponseHelper.Success(null,$"{packageDto.Name} created successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding package.");
                throw new Exception(ex.Message);
            }
        }

        [HttpPost("UpdatePackage")]
        public async Task<IActionResult> UpdatePackage([FromBody] PackageDto packageDto)
        {
            try
            {
                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }
                _logger.LogInformation("Updating existing package for owner PKID: {OwnerPKID}", ownerPKID);
                Packages packages = new Packages(_connStr);

                bool isValidPackageID = await packages.IsValidPackageID(ownerPKID, packageDto.PackageId);
                if(!isValidPackageID)
                {
                    return ApiResponseHelper.Failure("Invalid Package ID or access denied.");
                }

                await packages.UpdatePackage(ownerPKID, packageDto);
                _logger.LogInformation("Package updated with ID: {PackageID}", packageDto.PackageId);

                var currentPackageItemList = await packages.GetPackageItems(packageDto.PackageId);
                var deletePackageItemList = currentPackageItemList
                        .Where(currentItem => !packageDto.Items
                            .Where(i => i.PackageItemId != 0) // skip unsaved items
                            .Any(newItem => newItem.PackageItemId == currentItem.PackageItemId))
                        .ToList();

                foreach (var item in packageDto.Items)
                {
                    if (item.PackageItemId > 0)
                    {
                        await packages.UpdatePackageItems(packageDto.PackageId, item);
                        _logger.LogInformation("Added item to package ID {PackageID}: CategoryID {CategoryID}, Quantity {Quantity}", packageDto.PackageId, item.CategoryId, item.Quantity);
                    }
                    else
                    {
                        await packages.AddPackageItems(packageDto.PackageId, item);
                        _logger.LogInformation("Added item to package ID {PackageID}: CategoryID {CategoryID}, Quantity {Quantity}", packageDto.PackageId, item.CategoryId, item.Quantity);
                    }
                }

                foreach (var item in deletePackageItemList)
                {
                    await packages.DeletePackageItems(packageDto.PackageId, item.PackageItemId);
                }

                return Ok(new { result = true, message = $"{packageDto.Name} updated successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating package.");
                throw new Exception(ex.Message);
            }
        }

        [HttpPost("DeletePackage")]
        public async Task<IActionResult> DeletePackage([FromBody] long packageId)
        {
            try
            {
                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }
                Packages packages = new Packages(_connStr);

                bool isValidPackageID = await packages.IsValidPackageID(ownerPKID, packageId);
                if (!isValidPackageID)
                {
                    return ApiResponseHelper.Failure("Invalid Package ID or access denied.");
                }

                _logger.LogInformation("Delteing Package");
                await packages.SoftDeletePackage(packageId);

                _logger.LogInformation("Deleted package by ID; {0}", packageId);
                return Ok(new { message = "Package deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting packages.");
                throw new Exception(ex.Message);
            }
        }

        [HttpGet("Lookup")]
        public async Task<IActionResult> GetPackageLookup()
        {
            try
            {
                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }

                _logger.LogInformation("Fetching package lookup.");

                Packages packages = new Packages(_connStr);
                var listPackages = await packages.GetPackagesLookup(ownerPKID);

                // Safely handle null or empty list
                var lookup = (listPackages ?? new List<PackageDto>())
                    .Select(p => new
                    {
                        Id = p.PackageId,   // Rename for frontend
                        Name = p.Name
                    })
                    .ToList();

                _logger.LogInformation("Fetched {Count} package lookups.", lookup.Count);

                // Always return an array (even if empty)
                return Ok(lookup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching package lookup.");
                throw new Exception(ex.Message);
            }
        }
    }
}
