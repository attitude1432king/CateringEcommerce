using CateringEcommerce.API.Attributes;
using CateringEcommerce.BAL.Base.Owner;
using CateringEcommerce.BAL.Common;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Models.APIModels.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;

namespace CateringEcommerce.API.Controllers.Owner
{
    [Authorize]
    [ApiController]
    [Route("api/Auth/Owner")]
    public class RegistrationController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<RegistrationController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connStr;
        // In a real app, you would inject your database service here
        // private readonly IOwnerRepository _ownerRepository;

        public RegistrationController(IFileStorageService fileStorageService, ILogger<RegistrationController> logger, IConfiguration configuration)
        {
            _fileStorageService = fileStorageService;
            _logger = logger;
            _configuration = configuration;
            _connStr = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
        }

        [AllowAnonymous]
        [ValidateModel]
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] OwnerRegistrationDto registrationData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                _logger.LogInformation("Received registration request for Catering Partner: {CateringName}", registrationData.CateringName);
                Dictionary<string, object> dicData = new Dictionary<string, object>();
                OwnerRepository ownerRepository = new OwnerRepository(_connStr);
                OwnerRegister ownerRegister = new OwnerRegister(_connStr);
                Int64 ownerPkid = 0;

                if (string.IsNullOrEmpty(registrationData.Email) || string.IsNullOrEmpty(registrationData.Mobile))
                {
                    return BadRequest(new { message = "Email and Phone number are required." });
                }

                // Here you would typically check if the email or phone number already exists in your database
                if(ownerRepository.IsEmailExist(registrationData.Email) || ownerRepository.IsOwnerPhoneExist(registrationData.Mobile))
                {
                    return BadRequest(new {message = "Email or Phone number already exists. Please use a different one." });
                }
                _logger.LogInformation("Email and phone number are valid and not already registered."); 

                dicData = AddedRegistionDataToDictionary(registrationData);

                _logger.LogInformation("Registration data converted to dictionary for database insertion.");

                // 1. Create a new owner in the database and get the PKID
                ownerPkid = ownerRegister.CreateOwnerAccount(dicData);

                // 2. Save uploaded files and get their paths
                if (registrationData.CateringMedia != null)
                {
                    foreach (var mediaItem in registrationData.CateringMedia)
                    {
                        var path = await _fileStorageService.SaveFileAsync(mediaItem.Base64Data, ownerPkid, DocumentType.Kitchen.GetDisplayName(), isSecure: false, mediaItem.FileName);
                        await ownerRepository.SaveFilePath(path, ownerPkid, mediaItem.FileName, DocumentType.Kitchen);
                    }
                }

                var logoPath = await _fileStorageService.SaveFileAsync(registrationData.CateringLogo, ownerPkid, DocumentType.Logo.GetDisplayName(), isSecure: false);
                var fssaiPath = await _fileStorageService.SaveFileAsync(registrationData.FssaiCertificate.Base64, ownerPkid, CertificateType.FSSAI.GetDisplayName(), isSecure: true, registrationData.FssaiCertificate.Name);
                var gstPath = await _fileStorageService.SaveFileAsync(registrationData.GstCertificate.Base64, ownerPkid, CertificateType.GST.GetDisplayName(), isSecure: true, registrationData.GstCertificate.Name);
                var panPath = await _fileStorageService.SaveFileAsync(registrationData.PanCard.Base64, ownerPkid, CertificateType.PAN.GetDisplayName(), isSecure: true, registrationData.PanCard.Name);
                if (!string.IsNullOrEmpty(gstPath))
                    dicData.Add("GstCertificatePath", gstPath);
                if (!string.IsNullOrEmpty(panPath))
                    dicData.Add("PanCertificatePath", panPath);
                if (!string.IsNullOrEmpty(fssaiPath))
                    dicData.Add("FssaiCertificatePath", fssaiPath);

                //3. Create a method to save everything to the database

                #region Register the owner catering other details
                if (!string.IsNullOrEmpty(logoPath) && registrationData.CateringLogo != null)
                    ownerRegister.UpdateLogoPath(ownerPkid, logoPath);
                ownerRegister.RegisterAddress(ownerPkid, dicData);
                ownerRegister.RegisterServices(ownerPkid, dicData);
                ownerRegister.RegisterLegalDocuments(ownerPkid, dicData);
                ownerRegister.RegisterBankDetails(ownerPkid, dicData);

                _logger.LogInformation("Logo saved at: {LogoPath}", logoPath);
                _logger.LogInformation("FSSAI saved at: {FssaiPath}", fssaiPath);

                #endregion
                // 6. Return a success response
                return Ok(new { message = "Catering partner registered successfully. Your application is under review." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during owner registration.");
                return StatusCode(500, "An internal server error occurred. Please try again.");
            }
        }

        [AllowAnonymous]
        [HttpGet("Service/{TypeId}")]
        public async Task<IActionResult> GetServiceTypeDetails(int TypeId)
        {
            if (!Enum.IsDefined(typeof(ServiceType), TypeId))
            {
                return BadRequest(new { message = "Invalid service type ID." });
            }
            OwnerRegister ownerRegister = new OwnerRegister(_connStr);

            //Your logic to fetch and return service type details goes here.
            var details = await ownerRegister.GetServiceDetailsByTypeId(TypeId);
                
            return Ok(details);
        }

        /// <summary>
        /// Addded registration data to dictionary for database insertion.
        /// </summary>
        /// <param name="registrationData"></param>
        /// <returns></returns>
        private Dictionary<string, object> AddedRegistionDataToDictionary(OwnerRegistrationDto registrationData)
        {
            Dictionary<string, object> dicData = new Dictionary<string, object>
            {
                { "CateringName", registrationData.CateringName },
                { "OwnerName", registrationData.OwnerName },
                { "Mobile", registrationData.Mobile },
                { "CateringNumber", registrationData.CateringNumber },
                { "StdNumber", registrationData.StdNumber ?? string.Empty },
                { "Email", registrationData.Email },
                { "IsSameContact", registrationData.CateringNumberSameAsMobile },
                { "SupportContact", registrationData.SupportContact ?? string.Empty },
                { "AlternateEmail", registrationData.AlternateEmail ?? string.Empty },
                { "WhatsappNumber", registrationData.WhatsappNumber ?? string.Empty },
                { "ShopNo", registrationData.ShopNo },
                { "Street", registrationData.Floor ?? string.Empty },
                { "Area", registrationData.Landmark ?? string.Empty },
                { "State", registrationData.State ?? string.Empty },
                { "City", registrationData.City ?? string.Empty },
                { "Pincode", registrationData.Pincode ?? string.Empty },
                { "Latitude", registrationData.Latitude ?? string.Empty },
                { "Longitude", registrationData.Longitude ?? string.Empty },
                { "MapUrl", registrationData.MapUrl ?? string.Empty},
                { "Cuisines", registrationData.CuisineIds ?? string.Empty },
                { "ServiceTypes", registrationData.ServiceTypeIds ?? string.Empty },
                { "FoodTypes", registrationData.FoodTypeIds ?? string.Empty },
                { "EventTypes", registrationData.EventTypeIds ?? string.Empty },
                { "MinOrderValue", registrationData.MinOrderValue.ToString() },
                { "FssaiNumber", registrationData.FssaiNumber ?? string.Empty },
                { "FssaiExpiryDate", registrationData.FssaiExpiry ?? string.Empty },
                { "GstNumber", registrationData.GstNumber ?? string.Empty },
                { "IsGstApplicable", registrationData.IsGstApplicable},
                { "PanHolderName", registrationData.PanHolderName ?? string.Empty },
                { "PanNumber", registrationData.PanNumber ?? string.Empty },
                { "BankAccountName", registrationData.BankAccountName },
                { "BankAccountNumber", registrationData.BankAccountNumber },
                { "IfscCode", registrationData.IfscCode },
                { "ChequePath", registrationData.ChequePath ?? string.Empty }, // Fix for CS8604
                { "UpiId", registrationData.UpiId ?? string.Empty } // Fix for CS8604
            };
            return dicData;
        }
    }
}
