using CateringEcommerce.API.Controllers.Owner.Menu;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.Owner;
using CateringEcommerce.BAL.Base.Owner.Menu;
using CateringEcommerce.BAL.Common;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;
using Newtonsoft.Json;

namespace CateringEcommerce.API.Controllers.Owner
{
    [ApiController]
    [Route("api/Owner/Decorations")]
    [Authorize(Roles = "Owner")]
    public class DecorationsController : ControllerBase
    {
        private readonly string _connStr;
        private readonly ILogger<PackagesController> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly IFileStorageService _fileStorageService;

        public DecorationsController(IFileStorageService fileStorageService, ILogger<PackagesController> logger, IConfiguration configuration, ICurrentUserService currentUser)
        {
            _fileStorageService = fileStorageService;
            _logger = logger;
            _connStr = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
            _currentUser = currentUser;
        }

        [HttpGet("Count")]
        public async Task<IActionResult> GetDecorationsCount(string filterJson)
        {
            try
            {
                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }
                _logger.LogInformation("Fetching decorations setup count.");
                Decorations decorations = new Decorations(_connStr);
                var decorationsCount = await decorations.GetDecorationsCount(ownerPKID, filterJson);
                _logger.LogInformation("Fetched {Count} decorations setup.", decorationsCount);
                return Ok(decorationsCount);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while fetching decorations setup count.");
                return StatusCode(500, "An error occurred while fetching decorations setup count error: " + ex.Message);
            }
        }

        [HttpGet("Data")]
        public async Task<IActionResult> GetDecorationsList(int page, int pageSize, string filterJson)
        {
            try
            {
                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }
                _logger.LogInformation("Fetching decorations setup.");
                Decorations decorations = new Decorations(_connStr);
                var listDecorations = await decorations.GetDecorations(ownerPKID, page, pageSize, filterJson);
                _logger.LogInformation("Fetched {Count} decorations setup.", listDecorations?.Count ?? 0);
                return Ok(listDecorations);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while fetching decorations setup.");
                return StatusCode(500, "An error occurred while fetching decorations setup error: " + ex.Message);
            }
        }

        [HttpPost("Create")]
        public async Task<IActionResult> AddDeorationsSetup([FromBody] DecorationsDto decoration)
        {
            try
            {
                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }

                if (decoration == null || string.IsNullOrEmpty(decoration.Name) || decoration.ThemeId <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid decoration setup data.", "warning");
                }
                Decorations decorationsObj = new Decorations(_connStr);
                OwnerRepository ownerRepository = new OwnerRepository(_connStr);

                bool nameExists = await decorationsObj.IsDecorationNameExistsAsync(ownerPKID, decoration.Name);

                if (nameExists)
                    return ApiResponseHelper.Failure("Decoration name already exists.", "warning");

                long decoratinosID = await decorationsObj.AddDecoration(ownerPKID, decoration);

                if (decoratinosID <= 0)
                {
                    return ApiResponseHelper.Failure("Failed to create decoration setup.");
                }

                if (decoration.DecorationsMediaFiles != null)
                {
                    foreach (var file in decoration.DecorationsMediaFiles)
                    {
                        if (file == null || string.IsNullOrEmpty(file.Base64) || string.IsNullOrEmpty(file.Name))
                        {
                            _logger.LogWarning("Skipping invalid media file for decoration item ID: {0}", decoratinosID);
                            continue;
                        }
                        var path = await _fileStorageService.SaveFileAsync(file.Base64, ownerPKID, DocumentType.EventSetup.GetDisplayName(), false, file.Name);
                        await ownerRepository.SaveFilePath(path, ownerPKID, file.Name, DocumentType.EventSetup, decoratinosID);
                    }
                }
                _logger.LogInformation("Decoration setup added with ID: {0}", decoratinosID);

                return ApiResponseHelper.Success(null,$"{decoration.Name} added successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding decoration setup.");
                throw new Exception(ex.Message);
            }
        }

        [HttpGet]
        [Route("ThemeType")]
        public async Task<IActionResult> GetThemeType()
        {
            try
            {
                _logger.LogInformation("Fetching Theme Type.");
                Decorations decorations = new Decorations(_connStr);
                var listDecorations = await decorations.GetDecorationThemes();
              
                _logger.LogInformation("Fetched {Count} Theme Type.", listDecorations?.Count ?? 0);
                return Ok(listDecorations);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while fetching Theme type: " + ex.Message);
                return StatusCode(500, "An error occurred while fetching Theme type.");
            }
        }

        [HttpPost("Udpate")]
        public async Task<IActionResult> UpdateDecoratoinsSetup([FromBody] DecorationsDto decoration)
        {
            try
            {
                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }

                if (decoration == null || string.IsNullOrEmpty(decoration.Name) || decoration.ThemeId <= 0 || decoration.Id <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid decoration setup data", "warning");
                }
                Decorations decorationObj = new Decorations(_connStr);
                OwnerRepository ownerRepository = new OwnerRepository(_connStr);
                MediaRepository mediaRepository = new MediaRepository(_connStr);

                bool nameExists = await decorationObj.IsDecorationNameExistsAsync(ownerPKID, decoration.Name, decoration.Id);

                if (nameExists)
                    return ApiResponseHelper.Failure("Decoration name already exists. Please use a different name.", "warning");

                await decorationObj.UpdateDecoration(ownerPKID, decoration);

                if (decoration?.ExistingDecorationsMediaPaths != null)
                {
                    List<MediaFileModel> currentMediaPathsInDb = await mediaRepository.GetMediaFiles(ownerPKID, DocumentType.EventSetup, decoration.Id ?? 0);
                    var filesToDelete = currentMediaPathsInDb
                        .Where(dbPath => !decoration.ExistingDecorationsMediaPaths
                            .Contains(dbPath.FilePath, StringComparer.OrdinalIgnoreCase)) // optional case-insensitive compare
                        .ToList();

                    // Delete the identified files from storage and the database.
                    foreach (var pathToDelete in filesToDelete)
                    {
                        _fileStorageService.DeleteFilePath(pathToDelete.FilePath);
                        await ownerRepository.DeleteDocumentFile(pathToDelete.Id);
                    }

                }

                if (decoration?.DecorationsMediaFiles != null)
                {
                    foreach (var file in decoration.DecorationsMediaFiles)
                    {
                        if (file == null || string.IsNullOrEmpty(file.Base64) || string.IsNullOrEmpty(file.Name))
                        {
                            _logger.LogWarning("Skipping invalid media file for decoration setup ID: {0}", decoration.Id);
                            continue;
                        }
                        var path = await _fileStorageService.SaveFileAsync(file.Base64, ownerPKID, DocumentType.EventSetup.GetDisplayName(), false, file.Name);
                        await ownerRepository.SaveFilePath(path, ownerPKID, file.Name, DocumentType.EventSetup, decoration.Id ?? 0);
                    }
                }

                _logger.LogInformation("Decoration setup updated with ID: {0}", decoration.Id);

                return ApiResponseHelper.Success(null, $"{decoration.Name} updated successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating decoration setup.");
                throw new Exception(ex.Message);
            }
        }

        [HttpPost("Delete")]
        public async Task<IActionResult> DeleteDecorationSetup([FromBody] long decorationId)
        {
            try
            {
                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }

                Decorations decorations = new Decorations(_connStr);
                MediaRepository mediaRepository = new MediaRepository(_connStr);
                OwnerRepository ownerRepository = new OwnerRepository(_connStr);

                if (decorationId <= 0 || !decorations.IsValidDecorationID(ownerPKID, decorationId))
                {
                    return ApiResponseHelper.Failure("Invalid Decoration ID or access denied.");
                }

                _logger.LogInformation("Delteing Food Item MediaFiles");
                List<MediaFileModel> currentMediaPathsInDb = await mediaRepository.GetMediaFiles(ownerPKID, DocumentType.EventSetup, decorationId);

                // Delete the identified files from storage and the database.
                foreach (var pathToDelete in currentMediaPathsInDb)
                {
                    _fileStorageService.DeleteFilePath(pathToDelete.FilePath);
                    await ownerRepository.DeleteDocumentFile(pathToDelete.Id);
                }
                _logger.LogInformation("Deleting Decoration setup.");
                await decorations.DeleteDecoration(ownerPKID, decorationId);

                _logger.LogInformation("Deleted decoration setup by ID: {0}", decorationId);
                return ApiResponseHelper.Success(null, "Deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting decoration setup.");
                throw new Exception(ex.Message);
            }
        }

        [HttpPost("UpdateStatus")]
        public async Task<IActionResult> UpdateDecorationStatus([FromQuery] long decorationId, bool status)
        {
            try
            {
                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }

                Decorations decorations = new Decorations(_connStr);

                if (decorationId <= 0 || !decorations.IsValidDecorationID(ownerPKID, decorationId))
                {
                    return ApiResponseHelper.Failure("Invalid Decoration ID or access denied.");
                }

                await decorations.UpdateDecorationStatus(ownerPKID, decorationId, status);

                _logger.LogInformation("Update decoration setup status by ID: {0}", decorationId);

                 return ApiResponseHelper.Success(null,"Decoration status updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while update decoration setup status.");
                throw new Exception(ex.Message);
            }
        }
    }
}
