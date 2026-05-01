using CateringEcommerce.API.Controllers.Owner.Menu;
using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.Owner;
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

namespace CateringEcommerce.API.Controllers.Owner
{
    [ApiController]
    [Route("api/Owner/Decorations")]
    [OwnerAuthorize]
    public class DecorationsController : ControllerBase
    {
        private readonly ILogger<DecorationsController> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly IFileStorageService _fileStorageService;
        private readonly IDecorations _decorationsRepository;
        private readonly IOwnerRepository _ownerRepository;
        private readonly IMediaRepository _mediaRepository;

        public DecorationsController(
            IFileStorageService fileStorageService,
            ILogger<DecorationsController> logger,
            ICurrentUserService currentUser,
            IDecorations decorationsRepository,
            IOwnerRepository ownerRepository,
            IMediaRepository mediaRepository)
        {
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _decorationsRepository = decorationsRepository ?? throw new ArgumentNullException(nameof(decorationsRepository));
            _ownerRepository = ownerRepository ?? throw new ArgumentNullException(nameof(ownerRepository));
            _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
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
                var decorations = _decorationsRepository;
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
                var decorations = _decorationsRepository;
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
        [Consumes("multipart/form-data")]
        [RequestFormLimits(MultipartBodyLengthLimit = 50 * 1024 * 1024)]
        [RequestSizeLimit(50 * 1024 * 1024)]
        public async Task<IActionResult> AddDeorationsSetup([FromForm] DecorationsDto decoration)
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
                var decorationsObj = _decorationsRepository;

                bool nameExists = await decorationsObj.IsDecorationNameExistsAsync(ownerPKID, decoration.Name);

                if (nameExists)
                    return ApiResponseHelper.Failure("Decoration name already exists.", "warning");

                long decoratinosID = await decorationsObj.AddDecoration(ownerPKID, decoration);

                if (decoratinosID <= 0)
                {
                    return ApiResponseHelper.Failure("Failed to create decoration setup.");
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".mp4" };
                if (decoration.DecorationsMediaFiles != null)
                {
                    foreach (var file in decoration.DecorationsMediaFiles)
                    {
                        if (file == null || file.Length == 0)
                        {
                            _logger.LogWarning("Skipping empty media file for decoration item ID: {0}", decoratinosID);
                            continue;
                        }
                        var validation = FileValidationHelper.ValidateFile(file, allowedExtensions, 10 * 1024 * 1024);
                        if (!validation.IsValid)
                        {
                            _logger.LogWarning("Skipping invalid file {Name}: {Error}", file.FileName, validation.ErrorMessage);
                            continue;
                        }
                        var safeFilename = FileValidationHelper.GenerateSafeFilename(file.FileName);
                        var path = await _fileStorageService.SaveRoleBaseFormFileAsync(file, ownerPKID, Role.Owner.GetDisplayName(), DocumentType.EventSetup.GetDisplayName(), false, safeFilename);
                        await _ownerRepository.SaveFilePath(path, ownerPKID, file.FileName, DocumentType.EventSetup, decoratinosID);
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
                var decorations = _decorationsRepository;
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
        [Consumes("multipart/form-data")]
        [RequestFormLimits(MultipartBodyLengthLimit = 50 * 1024 * 1024)]
        [RequestSizeLimit(50 * 1024 * 1024)]
        public async Task<IActionResult> UpdateDecoratoinsSetup([FromForm] DecorationsDto decoration)
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
                var decorationObj = _decorationsRepository;

                bool nameExists = await decorationObj.IsDecorationNameExistsAsync(ownerPKID, decoration.Name, decoration.Id);

                if (nameExists)
                    return ApiResponseHelper.Failure("Decoration name already exists. Please use a different name.", "warning");

                await decorationObj.UpdateDecoration(ownerPKID, decoration);

                if (decoration?.ExistingDecorationsMediaPaths != null)
                {
                    List<MediaFileModel> currentMediaPathsInDb = await _mediaRepository.GetMediaFiles(ownerPKID, DocumentType.EventSetup, decoration.Id ?? 0);
                    var filesToDelete = currentMediaPathsInDb
                        .Where(dbPath => !decoration.ExistingDecorationsMediaPaths
                            .Contains(dbPath.FilePath, StringComparer.OrdinalIgnoreCase))
                        .ToList();

                    foreach (var pathToDelete in filesToDelete)
                    {
                        _fileStorageService.DeleteFilePath(pathToDelete.FilePath);
                        await _ownerRepository.DeleteDocumentFile(pathToDelete.Id);
                    }
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".mp4" };
                if (decoration?.DecorationsMediaFiles != null)
                {
                    foreach (var file in decoration.DecorationsMediaFiles)
                    {
                        if (file == null || file.Length == 0)
                        {
                            _logger.LogWarning("Skipping empty media file for decoration setup ID: {0}", decoration.Id);
                            continue;
                        }
                        var validation = FileValidationHelper.ValidateFile(file, allowedExtensions, 10 * 1024 * 1024);
                        if (!validation.IsValid)
                        {
                            _logger.LogWarning("Skipping invalid file {Name}: {Error}", file.FileName, validation.ErrorMessage);
                            continue;
                        }
                        var safeFilename = FileValidationHelper.GenerateSafeFilename(file.FileName);
                        var path = await _fileStorageService.SaveRoleBaseFormFileAsync(file, ownerPKID, Role.Owner.GetDisplayName(), DocumentType.EventSetup.GetDisplayName(), false, safeFilename);
                        await _ownerRepository.SaveFilePath(path, ownerPKID, file.FileName, DocumentType.EventSetup, decoration.Id ?? 0);
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

                var decorations = _decorationsRepository;

                if (decorationId <= 0 || !decorations.IsValidDecorationID(ownerPKID, decorationId))
                {
                    return ApiResponseHelper.Failure("Invalid Decoration ID or access denied.");
                }

                _logger.LogInformation("Delteing Food Item MediaFiles");
                List<MediaFileModel> currentMediaPathsInDb = await _mediaRepository.GetMediaFiles(ownerPKID, DocumentType.EventSetup, decorationId);

                foreach (var pathToDelete in currentMediaPathsInDb)
                {
                    await _ownerRepository.SoftDeleteDocumentFile(pathToDelete.Id);
                }
                _logger.LogInformation("Deleting Decoration setup.");
                await decorations.SoftDeleteDecoration(ownerPKID, decorationId);

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

                var decorations = _decorationsRepository;

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
