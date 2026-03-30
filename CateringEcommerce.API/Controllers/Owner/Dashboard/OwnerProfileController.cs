using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.APIModels.Owner;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CateringEcommerce.API.Controllers.Owner.Dashboard
{
    [ApiController]
    [Route("api/Owner/Profile")]
    [Authorize(Roles = "Owner")]
    public class OwnerProfileController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<OwnerProfileController> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly IOwnerProfile _ownerProfile;
        private readonly IOwnerRegister _ownerRegister;
        private readonly IOwnerRepository _ownerRepository;
        private readonly IMediaRepository _mediaRepository;

        public OwnerProfileController(
            IFileStorageService fileStorageService,
            ILogger<OwnerProfileController> logger,
            ICurrentUserService currentUser,
            IOwnerProfile ownerProfile,
            IOwnerRegister ownerRegister,
            IOwnerRepository ownerRepository,
            IMediaRepository mediaRepository)
        {
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _ownerProfile = ownerProfile ?? throw new ArgumentNullException(nameof(ownerProfile));
            _ownerRegister = ownerRegister ?? throw new ArgumentNullException(nameof(ownerRegister));
            _ownerRepository = ownerRepository ?? throw new ArgumentNullException(nameof(ownerRepository));
            _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
        }

        [HttpGet("GetProfileDetails")]
        public async Task<IActionResult> GetPartnerDetails()
        {
            var ownerPKID = _currentUser.UserId;
            if (ownerPKID <= 0)
            {
                return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
            }
            try
            {
                // Fetch owner details using injected repository
                OwnerModel ownerModel = await _ownerProfile.GetOwnerDetails(ownerPKID);

                return Ok(new { message = "Dashboard data endpoint is under construction.", formData = ownerModel });
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        [HttpPost("UpdateBusiness")]
        [Consumes("multipart/form-data")]
        [RequestFormLimits(MultipartBodyLengthLimit = 5 * 1024 * 1024)]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UpdateBusiness([FromForm] BusinessSettingsDto businessDto, [FromForm] IFormFile? NewLogoFile)
        {
            var ownerPkid = _currentUser.UserId;
            if (ownerPkid <= 0)
            {
                return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
            }
            try
            {
                string newLogoPath = string.Empty;
                if (NewLogoFile != null && NewLogoFile.Length > 0)
                {
                    var validation = FileValidationHelper.ValidateFile(NewLogoFile, new[] { ".jpg", ".jpeg", ".png" }, 5 * 1024 * 1024);
                    if (!validation.IsValid)
                        return ApiResponseHelper.Failure(validation.ErrorMessage, "warning");

                    var safeFilename = FileValidationHelper.GenerateSafeFilename(NewLogoFile.FileName);
                    newLogoPath = await _fileStorageService.SaveFormFileAsync(
                        NewLogoFile,
                        ownerPkid,
                        "Logo",
                        false,
                        safeFilename
                    );
                }

                //Remove old logo file and Update the logo path if a new logo was uploaded
                if (!string.IsNullOrEmpty(newLogoPath))
                {
                    string oldLogoPath = _ownerProfile.GetLogoPath(ownerPkid);
                    _fileStorageService.DeleteFilePath(oldLogoPath);
                    _ownerRegister.UpdateLogoPath(ownerPkid, newLogoPath);
                }

                if(businessDto != null)
                    await _ownerProfile.UpdateOwnerBusiness(ownerPkid, businessDto);

                return Ok(new { message = "Business details updated successfully." });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        [HttpPost("UpdateAddress")]
        public async Task<IActionResult> UpdateAddress([FromBody] AddressSettingsDto addressDto)
        {
            var ownerPkid = _currentUser.UserId;
            if (ownerPkid <= 0) return Unauthorized();

            try
            {
                if(addressDto != null)
                    await _ownerProfile.UpdateCateringAddress(ownerPkid, addressDto);
                return Ok(new { message = "Address details updated successfully." });

            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }


        [HttpPost("UpdateServices")]
        [Consumes("multipart/form-data")]
        [RequestFormLimits(MultipartBodyLengthLimit = 50 * 1024 * 1024)]
        [RequestSizeLimit(50 * 1024 * 1024)]
        public async Task<IActionResult> UpdateServices([FromForm] ServicesSettingsDto servicesDto, [FromForm] List<IFormFile>? NewKitchenMediaFiles)
        {
            var ownerPkid = _currentUser.UserId;
            if (ownerPkid <= 0) return Unauthorized();
            try
            {
                if (servicesDto?.ExistingMediaPaths != null)
                {
                    List<MediaFileModel> currentMediaPathsInDb = await _mediaRepository.GetMediaFiles(ownerPkid, DocumentType.Kitchen);
                    var filesToDelete = currentMediaPathsInDb
                        .Where(dbPath => !servicesDto.ExistingMediaPaths
                            .Contains(dbPath.FilePath, StringComparer.OrdinalIgnoreCase))
                        .ToList();

                    foreach (var pathToDelete in filesToDelete)
                    {
                        _fileStorageService.DeleteFilePath(pathToDelete.FilePath);
                        await _ownerRepository.DeleteDocumentFile(pathToDelete.Id);
                    }
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".mp4" };
                if (NewKitchenMediaFiles != null)
                {
                    foreach (var file in NewKitchenMediaFiles)
                    {
                        if (file == null || file.Length == 0)
                            continue;

                        var validation = FileValidationHelper.ValidateFile(file, allowedExtensions, 10 * 1024 * 1024);
                        if (!validation.IsValid)
                        {
                            _logger.LogWarning("Skipping invalid kitchen media file {Name}: {Error}", file.FileName, validation.ErrorMessage);
                            continue;
                        }

                        var safeFilename = FileValidationHelper.GenerateSafeFilename(file.FileName);
                        var path = await _fileStorageService.SaveFormFileAsync(file, ownerPkid, DocumentType.Kitchen.GetDisplayName(), false, safeFilename);
                        await _ownerRepository.SaveFilePath(path, ownerPkid, file.FileName, DocumentType.Kitchen);
                    }
                }

                if (servicesDto != null)
                    await _ownerProfile.UpdateCateringServices(ownerPkid, servicesDto);
                return Ok(new { message = "Service details updated successfully." });

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost("UpdateLegal")]
        public async Task<IActionResult> UpdateLegal([FromBody] LegalPaymentSettingsDto legalDto)
        {
            var ownerPkid = _currentUser.UserId;
            if (ownerPkid <= 0) return Unauthorized();
            try
            {
                if(legalDto != null)
                    await _ownerProfile.UpdateLegalAndBankDetails(ownerPkid, legalDto);
                return Ok(new { message = "Legal & Payment details updated successfully." });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
