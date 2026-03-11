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
        public async Task<IActionResult> UpdateBusiness([FromBody] BusinessSettingsDto businessDto)
        {
            var ownerPkid = _currentUser.UserId;
            if (ownerPkid <= 0)
            {
                return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
            }
            try
            {
                string newLogoPath = string.Empty;
                if (businessDto?.NewLogoFile != null)
                {
                    newLogoPath = await _fileStorageService.SaveFileAsync(
                        businessDto.NewLogoFile.Base64,
                        ownerPkid,
                        "Logo",
                        false,
                        businessDto.NewLogoFile.Name
                    );
                }

                //Remove old logo file and Update the logo path if a new logo was uploaded
                if (!string.IsNullOrEmpty(newLogoPath))
                {
                    string oldLogoPath = _ownerProfile.GetLogoPath(ownerPkid);
                    _fileStorageService.DeleteFilePath(oldLogoPath);
                    _ownerRegister.UpdateLogoPath(ownerPkid, newLogoPath);
                }
                // ... Database logic to update business settings and newLogoPath ...
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
                // ... Database logic to update address settings ...
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
        public async Task<IActionResult> UpdateServices([FromBody] ServicesSettingsDto servicesDto)
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
                            .Contains(dbPath.FilePath, StringComparer.OrdinalIgnoreCase)) // optional case-insensitive compare
                        .ToList();

                    // Delete the identified files from storage and the database.
                    foreach (var pathToDelete in filesToDelete)
                    {
                        _fileStorageService.DeleteFilePath(pathToDelete.FilePath);
                        await _ownerRepository.DeleteDocumentFile(pathToDelete.Id);
                    }

                }

                if (servicesDto?.NewKitchenMediaFiles != null)
                {
                    foreach (var file in servicesDto.NewKitchenMediaFiles)
                    {
                        var path = await _fileStorageService.SaveFileAsync(file.Base64, ownerPkid, DocumentType.Kitchen.GetDisplayName(), false, file.Name);
                        await _ownerRepository.SaveFilePath(path, ownerPkid, file.Name, DocumentType.Kitchen);
                    }
                }

                // ... Database logic to update services
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
                // ... Database logic to update legal & payment settings ...
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
