using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.Owner.Menu;
using CateringEcommerce.BAL.Common;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;
using Newtonsoft.Json;

namespace CateringEcommerce.API.Controllers.Owner.Menu
{
    [ApiController]
    [Route("api/Owner/Menu/FoodItem")]
    [Authorize(Roles = "Owner")]
    public class FoodItemsController : ControllerBase
    {
        private readonly string _connStr;
        private readonly ILogger<PackagesController> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly IFileStorageService _fileStorageService;

        public FoodItemsController(IFileStorageService fileStorageService, ILogger<PackagesController> logger, IConfiguration configuration, ICurrentUserService currentUser)
        {
            _fileStorageService = fileStorageService;
            _logger = logger;
            _connStr = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
            _currentUser = currentUser;
        }

        [HttpGet("Count")]
        public async Task<IActionResult> GetFoodItemCount(string filterJson)
        {
            try
            {
                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }
                var filter = JsonConvert.DeserializeObject<FoodItemFilter>(filterJson ?? "{}");
                _logger.LogInformation("Fetching food items count.");
                FoodItems foodItems = new FoodItems(_connStr);
                var foodItemsCount = await foodItems.GetFoodItemsCount(ownerPKID, filter);
                _logger.LogInformation("Fetched {Count} food items.", foodItemsCount);
                return Ok(foodItemsCount);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while fetching food category.");
                return StatusCode(500, "An error occurred while fetching food category.");
            }
        }

        [HttpGet("Data")]
        public async Task<IActionResult> GetFoodItemList(int page, int pageSize, string filterJson)
        {
            try
            {
                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }
                var filter = JsonConvert.DeserializeObject<FoodItemFilter>(filterJson ?? "{}");
                _logger.LogInformation("Fetching food items.");
                FoodItems foodItems = new FoodItems(_connStr);
                var listFoodItems = await foodItems.GetFoodItems(ownerPKID, page, pageSize, filter);
                _logger.LogInformation("Fetched {Count} food categories.", listFoodItems?.Count ?? 0);
                return Ok(listFoodItems);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while fetching food category.");
                return StatusCode(500, "An error occurred while fetching food category.");
            }
        }

        [HttpPost("Create")]
        public async Task<IActionResult> AddFoodItem([FromBody] FoodItemDto foodItems)
        {
            try
            {
                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }

                if(foodItems == null || string.IsNullOrEmpty(foodItems.Name) || foodItems.CategoryId <= 0 || foodItems.Price <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid food item data.", "warning");
                }
                FoodItems objFoodItem = new FoodItems(_connStr);
                OwnerRepository ownerRepository = new OwnerRepository(_connStr);

                bool nameExists = await objFoodItem.IsFoodItemNameExists(ownerPKID, foodItems.Name);
                // Check the Food Item Name exists or not
                if (nameExists)
                {
                    return ApiResponseHelper.Failure("Food Item is already exists.", "warning");
                }

                long foodItemID = await objFoodItem.AddFoodItem(ownerPKID, foodItems);

                if (foodItemID <= 0)
                {
                    return BadRequest(new { message = "Failed to create food item." });
                }

                if (foodItems.FoodItemMediaFiles != null)
                {
                    foreach (var file in foodItems.FoodItemMediaFiles)
                    {
                        if(file == null || string.IsNullOrEmpty(file.Base64) || string.IsNullOrEmpty(file.Name))
                        {
                            _logger.LogWarning("Skipping invalid media file for food item ID: {0}", foodItemID);
                            continue;
                        }
                        var path = await _fileStorageService.SaveFileAsync(file.Base64, ownerPKID, DocumentType.Food.GetDisplayName(), false, file.Name);
                        await ownerRepository.SaveFilePath(path, ownerPKID, file.Name, DocumentType.Food, foodItemID);
                    }
                }
                _logger.LogInformation("Food Item added with ID: {0}", foodItemID);
                
                return Ok(new { message = $"{foodItems.Name} added successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding food item.");
                throw new Exception(ex.Message);
            }
        }

        [HttpGet]
        [Route("GetCuisineType")]
        public async Task<IActionResult> GetCuisineType()
        {
            try
            {
                _logger.LogInformation("Fetching Cuisine Type.");
                OwnerRepository ownerRepo = new OwnerRepository(_connStr);
                var listCuisineType = await ownerRepo.GetCateringMasterType(CateringMaster.CuisineType);
                // Filter to include only TypeId and TypeName
                var filteredList = listCuisineType?
                    .Select(x => new
                    {
                        x.TypeId,
                        x.TypeName
                    })
                    .ToList();
                _logger.LogInformation("Fetched {Count} Cuisine Type.", filteredList?.Count ?? 0);
                return Ok(filteredList);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while fetching cuisine type: " + ex.Message);
                return StatusCode(500, "An error occurred while fetching cuisine type.");
            }
        }

        [HttpPost("Udpate")]
        public async Task<IActionResult> UpdateFoodItem([FromBody] FoodItemDto foodItems)
        {
            try
            {
                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }

                if (foodItems == null || string.IsNullOrEmpty(foodItems.Name) || foodItems.CategoryId <= 0 || foodItems.Price <= 0 || foodItems.Id <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid food item data.", "warning");
                }
                FoodItems objFoodItem = new FoodItems(_connStr);
                OwnerRepository ownerRepository = new OwnerRepository(_connStr);
                MediaRepository mediaRepository = new MediaRepository(_connStr);

                bool nameExists = await objFoodItem.IsFoodItemNameExists(ownerPKID, foodItems.Name, foodItems.Id);
                if (nameExists)
                {
                    return ApiResponseHelper.Failure("Food Item is already exists. Please use a different name.", "warning");
                }

                bool isNotValidId = await objFoodItem.IsValidFoodItemID(ownerPKID, foodItems.Id.Value);
                if (isNotValidId)
                {
                    return ApiResponseHelper.Failure("Invalid Food Item ID.", "warning");
                }

                await objFoodItem.UpdateFoodItem(ownerPKID, foodItems);

                if (foodItems?.ExistingFoodItemMediaPaths != null)
                {
                    List<MediaFileModel> currentMediaPathsInDb = await mediaRepository.GetMediaFiles(ownerPKID, DocumentType.Food, foodItems.Id ?? 0);
                    var filesToDelete = currentMediaPathsInDb
                        .Where(dbPath => !foodItems.ExistingFoodItemMediaPaths
                            .Contains(dbPath.FilePath, StringComparer.OrdinalIgnoreCase)) // optional case-insensitive compare
                        .ToList();

                    // Delete the identified files from storage and the database.
                    foreach (var pathToDelete in filesToDelete)
                    {
                        _fileStorageService.DeleteFilePath(pathToDelete.FilePath);
                        await ownerRepository.DeleteDocumentFile(pathToDelete.Id);
                    }

                }

                if (foodItems.FoodItemMediaFiles != null)
                {
                    foreach (var file in foodItems.FoodItemMediaFiles)
                    {
                        if (file == null || string.IsNullOrEmpty(file.Base64) || string.IsNullOrEmpty(file.Name))
                        {
                            _logger.LogWarning("Skipping invalid media file for food item ID: {0}", foodItems.Id);
                            continue;
                        }
                        var path = await _fileStorageService.SaveFileAsync(file.Base64, ownerPKID, DocumentType.Food.GetDisplayName(), false, file.Name);
                        await ownerRepository.SaveFilePath(path, ownerPKID, file.Name, DocumentType.Food, foodItems.Id ?? 0);
                    }
                }

                _logger.LogInformation("Food Item updated with ID: {0}", foodItems.Id);

                return ApiResponseHelper.Success(null, $"{foodItems.Name} updated successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating food item.");
                throw new Exception(ex.Message);
            }
        }

        [HttpPost("Delete")]
        public async Task<IActionResult> DeleteFoodItem([FromBody] long foodItemId)
        {
            try
            {
                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }
                FoodItems foodItems = new FoodItems(_connStr);
                MediaRepository mediaRepository = new MediaRepository(_connStr);
                OwnerRepository ownerRepository = new OwnerRepository(_connStr);

                bool isNotValidId = await foodItems.IsValidFoodItemID(ownerPKID, foodItemId);
                if (isNotValidId)
                {
                    return ApiResponseHelper.Failure("Invalid Food Item ID.", "warning");
                }
                _logger.LogInformation("Delteing Food Item MediaFiles");
                List<MediaFileModel> currentMediaPathsInDb = await mediaRepository.GetMediaFiles(ownerPKID, DocumentType.Food, foodItemId);

                // Delete the identified files from storage and the database.
                foreach (var pathToDelete in currentMediaPathsInDb)
                {
                    await ownerRepository.SoftDeleteDocumentFile(pathToDelete.Id);
                }
                _logger.LogInformation("Deleting FoodItems.");
                await foodItems.SoftDeleteFoodItem(ownerPKID, foodItemId);

                _logger.LogInformation("Deleted food item by ID; {0}", foodItemId);
                return Ok(new { message = "Deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting food item.");
                throw new Exception(ex.Message);
            }
        }

        [HttpGet("Lookup")]
        public async Task<IActionResult> GetFoodItemLookup()
        {
            try
            {
                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }

                _logger.LogInformation("Fetching food item lookup.");

                FoodItems foodItems = new FoodItems(_connStr);
                var listFoodItem = await foodItems.GetFoodItemsLookup(ownerPKID);

                // Safely handle null or empty list
                var lookup = (listFoodItem ?? new List<FoodItemDto>())
                    .Select(p => new
                    {
                        Id = p.Id,   // Rename for frontend
                        Name = p.Name
                    })
                    .ToList();

                _logger.LogInformation("Fetched {Count} package lookups.", lookup.Count);

                // Always return an array (even if empty)
                return Ok(lookup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching food item lookup.");
                throw new Exception(ex.Message);
            }
        }
    }
}
