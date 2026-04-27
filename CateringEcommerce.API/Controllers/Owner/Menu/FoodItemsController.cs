using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.Owner.Menu;
using CateringEcommerce.BAL.Common;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CateringEcommerce.API.Controllers.Owner.Menu
{
    [ApiController]
    [Route("api/Owner/Menu/FoodItem")]
    [OwnerAuthorize]
    public class FoodItemsController : ControllerBase
    {
        private readonly ILogger<FoodItemsController> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly IFileStorageService _fileStorageService;
        private readonly IFoodItems _foodItemsRepository;
        private readonly IOwnerRepository _ownerRepository;
        private readonly IMediaRepository _mediaRepository;

        public FoodItemsController(
            IFileStorageService fileStorageService,
            ILogger<FoodItemsController> logger,
            ICurrentUserService currentUser,
            IFoodItems foodItemsRepository,
            IOwnerRepository ownerRepository,
            IMediaRepository mediaRepository)
        {
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _foodItemsRepository = foodItemsRepository ?? throw new ArgumentNullException(nameof(foodItemsRepository));
            _ownerRepository = ownerRepository ?? throw new ArgumentNullException(nameof(ownerRepository));
            _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
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
                var foodItems = _foodItemsRepository;
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
                var foodItems = _foodItemsRepository;
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
        [Consumes("multipart/form-data")]
        [RequestFormLimits(MultipartBodyLengthLimit = 50 * 1024 * 1024)]
        [RequestSizeLimit(50 * 1024 * 1024)]
        public async Task<IActionResult> AddFoodItem([FromForm] FoodItemDto foodItems)
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

                if (foodItems.FoodItemMediaFiles != null && foodItems.FoodItemMediaFiles.Count > 1)
                {
                    return ApiResponseHelper.Failure("Only one image or video is allowed per food item.", "warning");
                }
                var objFoodItem = _foodItemsRepository;

                bool nameExists = await objFoodItem.IsFoodItemNameExists(ownerPKID, foodItems.Name);
                if (nameExists)
                {
                    return ApiResponseHelper.Failure("Food Item is already exists.", "warning");
                }

                long foodItemID = await objFoodItem.AddFoodItem(ownerPKID, foodItems);

                if (foodItemID <= 0)
                {
                    return BadRequest(new { message = "Failed to create food item." });
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".mp4" };
                if (foodItems.FoodItemMediaFiles != null)
                {
                    foreach (var file in foodItems.FoodItemMediaFiles)
                    {
                        if (file == null || file.Length == 0)
                        {
                            _logger.LogWarning("Skipping empty media file for food item ID: {0}", foodItemID);
                            continue;
                        }
                        var validation = FileValidationHelper.ValidateFile(file, allowedExtensions, 10 * 1024 * 1024);
                        if (!validation.IsValid)
                        {
                            _logger.LogWarning("Skipping invalid file {Name}: {Error}", file.FileName, validation.ErrorMessage);
                            continue;
                        }
                        var safeFilename = FileValidationHelper.GenerateSafeFilename(file.FileName);
                        var path = await _fileStorageService.SaveRoleBaseFormFileAsync(file, ownerPKID, Role.Owner.GetDisplayName(), DocumentType.Food.GetDisplayName(), false, safeFilename);
                        await _ownerRepository.SaveFilePath(path, ownerPKID, file.FileName, DocumentType.Food, foodItemID);
                    }
                }
                _logger.LogInformation("Food Item added with ID: {0}", foodItemID);

                return ApiResponseHelper.Success(null, $"{foodItems.Name} added successfully!");
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
                var ownerRepo = _ownerRepository;
                var listCuisineType = await ownerRepo.GetCateringMasterType(CateringMaster.CuisineType);
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
        [Consumes("multipart/form-data")]
        [RequestFormLimits(MultipartBodyLengthLimit = 50 * 1024 * 1024)]
        [RequestSizeLimit(50 * 1024 * 1024)]
        public async Task<IActionResult> UpdateFoodItem([FromForm] FoodItemDto foodItems)
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

                if (foodItems.FoodItemMediaFiles != null && foodItems.FoodItemMediaFiles.Count > 1)
                {
                    return ApiResponseHelper.Failure("Only one image or video is allowed per food item.", "warning");
                }
                var objFoodItem = _foodItemsRepository;

                bool nameExists = await objFoodItem.IsFoodItemNameExists(ownerPKID, foodItems.Name, foodItems.Id);
                if (nameExists)
                {
                    return ApiResponseHelper.Failure("Food Item is already exists. Please use a different name.", "warning");
                }

                bool isValidId = await objFoodItem.IsValidFoodItemID(ownerPKID, foodItems.Id.Value);
                if (!isValidId)
                {
                    return ApiResponseHelper.Failure("Invalid Food Item ID.", "warning");
                }

                await objFoodItem.UpdateFoodItem(ownerPKID, foodItems);

                if (foodItems?.ExistingFoodItemMediaPaths != null)
                {
                    List<MediaFileModel> currentMediaPathsInDb = await _mediaRepository.GetMediaFiles(ownerPKID, DocumentType.Food, foodItems.Id ?? 0);
                    var filesToDelete = currentMediaPathsInDb
                        .Where(dbPath => !foodItems.ExistingFoodItemMediaPaths
                            .Contains(dbPath.FilePath, StringComparer.OrdinalIgnoreCase))
                        .ToList();

                    foreach (var pathToDelete in filesToDelete)
                    {
                        _fileStorageService.DeleteFilePath(pathToDelete.FilePath);
                        await _ownerRepository.DeleteDocumentFile(pathToDelete.Id);
                    }
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".mp4" };
                if (foodItems.FoodItemMediaFiles != null)
                {
                    foreach (var file in foodItems.FoodItemMediaFiles)
                    {
                        if (file == null || file.Length == 0)
                        {
                            _logger.LogWarning("Skipping empty media file for food item ID: {0}", foodItems.Id);
                            continue;
                        }
                        var validation = FileValidationHelper.ValidateFile(file, allowedExtensions, 10 * 1024 * 1024);
                        if (!validation.IsValid)
                        {
                            _logger.LogWarning("Skipping invalid file {Name}: {Error}", file.FileName, validation.ErrorMessage);
                            continue;
                        }
                        var safeFilename = FileValidationHelper.GenerateSafeFilename(file.FileName);
                        var path = await _fileStorageService.SaveRoleBaseFormFileAsync(file, ownerPKID, Role.Owner.GetDisplayName(), DocumentType.Food.GetDisplayName(), false, safeFilename);
                        await _ownerRepository.SaveFilePath(path, ownerPKID, file.FileName, DocumentType.Food, foodItems.Id ?? 0);
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
                var foodItems = _foodItemsRepository;

                bool isNotValidId = await foodItems.IsValidFoodItemID(ownerPKID, foodItemId);
                if (isNotValidId)
                {
                    return ApiResponseHelper.Failure("Invalid Food Item ID.", "warning");
                }
                _logger.LogInformation("Delteing Food Item MediaFiles");
                List<MediaFileModel> currentMediaPathsInDb = await _mediaRepository.GetMediaFiles(ownerPKID, DocumentType.Food, foodItemId);

                foreach (var pathToDelete in currentMediaPathsInDb)
                {
                    await _ownerRepository.SoftDeleteDocumentFile(pathToDelete.Id);
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

                var foodItems = _foodItemsRepository;
                var listFoodItem = await foodItems.GetFoodItemsLookup(ownerPKID);

                var lookup = (listFoodItem ?? new List<FoodItemDto>())
                    .Select(p => new
                    {
                        Id = p.Id,
                        Name = p.Name
                    })
                    .ToList();

                _logger.LogInformation("Fetched {Count} package lookups.", lookup.Count);

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
