using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.Owner;
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
    [Route("api/Owner/Banners")]
    [OwnerAuthorize]
    public class BannersController : ControllerBase
    {
        private readonly ILogger<BannersController> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly IFileStorageService _fileStorageService;
        private readonly IBannerService _bannerService;
        private readonly IOwnerRepository _ownerRepository;

        public BannersController(
            IFileStorageService fileStorageService,
            ILogger<BannersController> logger,
            ICurrentUserService currentUser,
            IBannerService bannerService,
            IOwnerRepository ownerRepository)
        {
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _bannerService = bannerService ?? throw new ArgumentNullException(nameof(bannerService));
            _ownerRepository = ownerRepository ?? throw new ArgumentNullException(nameof(ownerRepository));
        }

        [HttpGet("Count")]
        public async Task<IActionResult> GetBannersCount(string filterJson)
        {
            try
            {
                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }

                var filter = JsonConvert.DeserializeObject<BannerFilter>(filterJson ?? "{}");
                _logger.LogInformation("Fetching banners count for owner: {OwnerPKID}", ownerPKID);

                var bannerService = _bannerService;
                var count = await bannerService.GetBannersCount(ownerPKID, filter);

                _logger.LogInformation("Fetched {Count} banners.", count);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching banners count.");
                return StatusCode(500, "An error occurred while fetching banners count.");
            }
        }

        [HttpGet("Data")]
        public async Task<IActionResult> GetBannersList(int page, int pageSize, string filterJson)
        {
            try
            {
                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }

                var filter = JsonConvert.DeserializeObject<BannerFilter>(filterJson ?? "{}");
                _logger.LogInformation("Fetching banners list for owner: {OwnerPKID}", ownerPKID);

                var bannerService = _bannerService;
                var banners = await bannerService.GetBanners(ownerPKID, page, pageSize, filter);

                _logger.LogInformation("Fetched {Count} banners.", banners?.Count ?? 0);
                return Ok(banners);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching banners list.");
                return StatusCode(500, "An error occurred while fetching banners list.");
            }
        }

        [HttpPost("Create")]
        [Consumes("multipart/form-data")]
        [RequestFormLimits(MultipartBodyLengthLimit = 5 * 1024 * 1024)]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> CreateBanner([FromForm] BannerDto banner, [FromForm] IFormFile? BannerImage)
        {
            try
            {
                var ownerPKID = _currentUser.UserId;

                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }

                if (banner == null || string.IsNullOrEmpty(banner.Title))
                {
                    return ApiResponseHelper.Failure("Invalid banner data.", "warning");
                }

                var bannerService = _bannerService;

                bool titleExists = await bannerService.IsBannerTitleExists(ownerPKID, banner.Title);
                if (titleExists)
                {
                    return ApiResponseHelper.Failure("Banner title already exists.", "warning");
                }

                long bannerId = await bannerService.AddBanner(ownerPKID, banner);

                if (bannerId <= 0)
                {
                    return ApiResponseHelper.Failure("Failed to create banner.");
                }

                // Upload banner image
                if (BannerImage != null && BannerImage.Length > 0)
                {
                    var validation = FileValidationHelper.ValidateFile(BannerImage, new[] { ".jpg", ".jpeg", ".png", ".webp" }, 5 * 1024 * 1024);
                    if (!validation.IsValid)
                        return ApiResponseHelper.Failure(validation.ErrorMessage, "warning");

                    var safeFilename = FileValidationHelper.GenerateSafeFilename(BannerImage.FileName);
                    var imagePath = await _fileStorageService.SaveFormFileAsync(
                        BannerImage,
                        ownerPKID,
                        DocumentType.Banner.GetDisplayName(),
                        false,
                        safeFilename);

                    banner.ImagePath = imagePath;

                    await _ownerRepository.SaveFilePath(
                        imagePath,
                        ownerPKID,
                        BannerImage.FileName,
                        DocumentType.Banner,
                        bannerId);
                }

                _logger.LogInformation("Banner created with ID: {BannerId}", bannerId);
                return ApiResponseHelper.Success(null, $"{banner.Title} created successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating banner.");
                return StatusCode(500, "An error occurred while creating banner.");
            }
        }

        [HttpPost("Update")]
        [Consumes("multipart/form-data")]
        [RequestFormLimits(MultipartBodyLengthLimit = 5 * 1024 * 1024)]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UpdateBanner([FromForm] BannerDto banner, [FromForm] IFormFile? BannerImage)
        {
            try
            {
                var ownerPKID = _currentUser.UserId;

                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }

                if (banner == null || banner.Id <= 0 || string.IsNullOrEmpty(banner.Title))
                {
                    return ApiResponseHelper.Failure("Invalid banner data.", "warning");
                }

                var bannerService = _bannerService;

                bool titleExists = await bannerService.IsBannerTitleExists(ownerPKID, banner.Title, banner.Id);
                if (titleExists)
                {
                    return ApiResponseHelper.Failure("Banner title already exists.", "warning");
                }

                bool updated = await bannerService.UpdateBanner(ownerPKID, banner);
                if (!updated)
                {
                    return ApiResponseHelper.Failure("Failed to update banner.");
                }

                // Upload new image if provided
                if (BannerImage != null && BannerImage.Length > 0)
                {
                    var validation = FileValidationHelper.ValidateFile(BannerImage, new[] { ".jpg", ".jpeg", ".png", ".webp" }, 5 * 1024 * 1024);
                    if (!validation.IsValid)
                        return ApiResponseHelper.Failure(validation.ErrorMessage, "warning");

                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(banner.ImagePath))
                    {
                        _fileStorageService.DeleteFilePath(banner.ImagePath);
                    }

                    var safeFilename = FileValidationHelper.GenerateSafeFilename(BannerImage.FileName);
                    var imagePath = await _fileStorageService.SaveFormFileAsync(
                        BannerImage,
                        ownerPKID,
                        DocumentType.Banner.GetDisplayName(),
                        false,
                        safeFilename);

                    banner.ImagePath = imagePath;

                    await _ownerRepository.UpdateDocumentFilePath(
                        banner.Id.Value,
                        DocumentType.Banner,
                        imagePath);
                }

                _logger.LogInformation("Banner updated with ID: {BannerId}", banner.Id);
                return ApiResponseHelper.Success(null, $"{banner.Title} updated successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating banner.");
                return StatusCode(500, "An error occurred while updating banner.");
            }
        }

        [HttpPost("Delete")]
        public async Task<IActionResult> DeleteBanner([FromBody] long bannerId)
        {
            try
            {
                var ownerPKID = _currentUser.UserId;

                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }

                var bannerService = _bannerService;
                bool deleted = await bannerService.DeleteBanner(ownerPKID, bannerId);

                if (!deleted)
                {
                    return ApiResponseHelper.Failure("Failed to delete banner.");
                }

                await _ownerRepository.SoftDeleteByReferenceID(bannerId, DocumentType.Banner);

                _logger.LogInformation("Banner deleted with ID: {BannerId}", bannerId);
                return ApiResponseHelper.Success(null, "Banner deleted successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting banner.");
                return StatusCode(500, "An error occurred while deleting banner.");
            }
        }

        [HttpPost("UpdateStatus")]
        public async Task<IActionResult> UpdateBannerStatus([FromQuery] long bannerId, [FromQuery] bool isActive)
        {
            try
            {
                var ownerPKID = _currentUser.UserId;

                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }

                var bannerService = _bannerService;
                bool updated = await bannerService.UpdateBannerStatus(ownerPKID, bannerId, isActive);

                if (!updated)
                {
                    return ApiResponseHelper.Failure("Failed to update banner status.");
                }

                _logger.LogInformation("Banner status updated for ID: {BannerId}", bannerId);
                return ApiResponseHelper.Success(null, "Banner status updated successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating banner status.");
                return StatusCode(500, "An error occurred while updating banner status.");
            }
        }
    }
}
