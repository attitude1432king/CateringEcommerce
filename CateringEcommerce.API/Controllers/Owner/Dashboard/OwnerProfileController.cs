using CateringEcommerce.API.Attributes;
using CateringEcommerce.BAL.Base.Owner;
using CateringEcommerce.BAL.Base.Owner.Dashboard;
using CateringEcommerce.BAL.Common;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Models;
using CateringEcommerce.Domain.Models.APIModels.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static CateringEcommerce.Domain.Models.APIModels.Owner.UpdateOwnerProfileDto;

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
        // In a real app, you would inject your database service here
        // private readonly IOwnerRepository _ownerRepository;

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
                return BadRequest(new { message = "Invalid owner PKID." });
            }
            try
            {
                OwnerModel ownerModel = new OwnerModel();
                OwnerProfile ownerProfile = new OwnerProfile(_connStr);
                // Placeholder for fetching data from the database
                ownerModel = ownerProfile.GetOwnerDetails(ownerPKID);

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
            if (ownerPkid <= 0) return Unauthorized();
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

                // Update the logo path if a new logo was uploaded
                if (!string.IsNullOrEmpty(newLogoPath))
                {
                    ownerRegister.UpdateLogoPath(ownerPkid, newLogoPath);
                }
                // ... Database logic to update business settings and newLogoPath ...
                if(businessDto != null)
                    ownerProfile.UpdateOwnerBusiness(ownerPkid, businessDto);

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
                    ownerProfile.UpdateCateringAddress(ownerPkid, addressDto);
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
                OwnerProfile ownerProfile = new OwnerProfile(_connStr);
                if (servicesDto?.NewKitchenMediaFiles != null)
                {
                    foreach (var file in servicesDto.NewKitchenMediaFiles)
                    {
                        var path = await _fileStorageService.SaveFileAsync(file.Base64, ownerPkid, "Kitchen", false, file.Name);
                        await ownerRepository.SaveFilePath(path, ownerPkid, file.Name, DocumentType.Kitchen);
                    }
                }

                // ... Database logic to update services
                if(servicesDto != null)
                    ownerProfile.UpdateCateringServices(ownerPkid, servicesDto);
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
                    ownerProfile.UpdateLegalAndBankDetails(ownerPkid, legalDto);
                return Ok(new { message = "Legal & Payment details updated successfully." });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
