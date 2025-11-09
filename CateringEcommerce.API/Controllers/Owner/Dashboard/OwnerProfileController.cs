using CateringEcommerce.BAL.Base.Owner;
using CateringEcommerce.BAL.Base.Owner.Dashboard;
using CateringEcommerce.BAL.Common;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CateringEcommerce.Domain.Models.APIModels.Owner;
using Microsoft.OpenApi.Extensions;
using CateringEcommerce.API.Helpers;

namespace CateringEcommerce.API.Controllers.Owner.Dashboard
{
    [ApiController]
    [Route("api/Owner/Profile")]
    [Authorize(Roles = "Owner")]
    public class OwnerProfileController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<RegistrationController> _logger;
        private readonly string _connStr;
        private readonly ICurrentUserService _currentUser;

        public OwnerProfileController(IFileStorageService fileStorageService, ILogger<RegistrationController> logger, IConfiguration configuration, ICurrentUserService currentUser)
        {
            _fileStorageService = fileStorageService;
            _logger = logger;
            _connStr = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
            _currentUser = currentUser;
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
                OwnerModel ownerModel = new OwnerModel();
                OwnerProfile ownerProfile = new OwnerProfile(_connStr);
                // Placeholder for fetching data from the database
                ownerModel = await ownerProfile.GetOwnerDetails(ownerPKID);

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
                OwnerProfile ownerProfile = new OwnerProfile(_connStr);
                OwnerRegister ownerRegister = new OwnerRegister(_connStr);
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
                    string oldLogoPath = ownerProfile.GetLogoPath(ownerPkid);
                    _fileStorageService.DeleteFilePath(oldLogoPath);
                    ownerRegister.UpdateLogoPath(ownerPkid, newLogoPath);
                }
                // ... Database logic to update business settings and newLogoPath ...
                if(businessDto != null)
                    await ownerProfile.UpdateOwnerBusiness(ownerPkid, businessDto);

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
                OwnerProfile ownerProfile = new OwnerProfile(_connStr);
                // ... Database logic to update address settings ...
                if(addressDto != null)
                    await ownerProfile.UpdateCateringAddress(ownerPkid, addressDto);
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
                OwnerRepository ownerRepository = new OwnerRepository(_connStr);
                MediaRepository mediaRepository = new MediaRepository(_connStr);
                OwnerProfile ownerProfile = new OwnerProfile(_connStr);
                if (servicesDto?.ExistingMediaPaths != null)
                {
                    List<MediaFileModel> currentMediaPathsInDb = await mediaRepository.GetMediaFiles(ownerPkid, DocumentType.Kitchen);
                    var filesToDelete = currentMediaPathsInDb
                        .Where(dbPath => !servicesDto.ExistingMediaPaths
                            .Contains(dbPath.FilePath, StringComparer.OrdinalIgnoreCase)) // optional case-insensitive compare
                        .ToList();

                    // Delete the identified files from storage and the database.
                    foreach (var pathToDelete in filesToDelete)
                    {
                        _fileStorageService.DeleteFilePath(pathToDelete.FilePath);
                        await ownerRepository.DeleteDocumentFile(pathToDelete.Id);
                    }

                }

                if (servicesDto?.NewKitchenMediaFiles != null)
                {
                    foreach (var file in servicesDto.NewKitchenMediaFiles)
                    {
                        var path = await _fileStorageService.SaveFileAsync(file.Base64, ownerPkid, DocumentType.Kitchen.GetDisplayName(), false, file.Name);
                        await ownerRepository.SaveFilePath(path, ownerPkid, file.Name, DocumentType.Kitchen);
                    }
                }

                // ... Database logic to update services
                if (servicesDto != null)
                    await ownerProfile.UpdateCateringServices(ownerPkid, servicesDto);
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
                OwnerProfile ownerProfile = new OwnerProfile(_connStr);

                // ... Database logic to update legal & payment settings ...
                if(legalDto != null)
                    await ownerProfile.UpdateLegalAndBankDetails(ownerPkid, legalDto);
                return Ok(new { message = "Legal & Payment details updated successfully." });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
