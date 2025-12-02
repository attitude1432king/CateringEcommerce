using CateringEcommerce.API.Attributes;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.Owner;
using CateringEcommerce.BAL.Common;
using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Models.APIModels.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Extensions;
using System.IO;

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
        private readonly string _customKey;
        // In a real app, you would inject your database service here
        // private readonly IOwnerRepository _ownerRepository;

        public RegistrationController(IFileStorageService fileStorageService, ILogger<RegistrationController> logger, IConfiguration configuration, IOptions<EncryptionSettings> setting)
        {
            _fileStorageService = fileStorageService;
            _logger = logger;
            _configuration = configuration;
            _connStr = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
            _customKey = setting.Value.CustomKey;
        }

        [AllowAnonymous]
        [RequestSizeLimit(long.MaxValue)]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
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


                _logger.LogInformation("Registration data converted to dictionary for database insertion.");
                dicData = AddedRegistionDataToDictionary(registrationData);

                // 1. Create a new owner in the database and get the PKID
                ownerPkid = ownerRegister.CreateOwnerAccount(dicData);
                if(ownerPkid <= 0)
                {
                    _logger.LogError("Failed to create owner account in the database.");
                    return ApiResponseHelper.Failure("An error occurred while creating the parnter account. Please try again.");
                }
                _logger.LogInformation("Owner account created successfully with PKID: {OwnerPkid}", ownerPkid);

                // 2. Save uploaded files and get their paths
                var logoPath = await _fileStorageService.SaveFileAsync(registrationData.CateringLogo ?? string.Empty, ownerPkid, DocumentType.Logo.GetDisplayName(), isSecure: false);

                // FSSAI Certificate
                var fssaiBase64 = registrationData.FssaiCertificate?.Base64 ?? string.Empty;
                var fssaiName = registrationData.FssaiCertificate?.Name ?? string.Empty;
                var fssaiPath = await _fileStorageService.SaveFileAsync(fssaiBase64, ownerPkid, CertificateType.FSSAI.GetDisplayName(), isSecure: true, fssaiName);

                // GST Certificate
                var gstBase64 = registrationData.GstCertificate?.Base64 ?? string.Empty;
                var gstName = registrationData.GstCertificate?.Name ?? string.Empty;
                var gstPath = await _fileStorageService.SaveFileAsync(gstBase64, ownerPkid, CertificateType.GST.GetDisplayName(), isSecure: true, gstName);

                // PAN Card
                var panBase64 = registrationData.PanCard?.Base64 ?? string.Empty;
                var panName = registrationData.PanCard?.Name ?? string.Empty;
                var panPath = await _fileStorageService.SaveFileAsync(panBase64, ownerPkid, CertificateType.PAN.GetDisplayName(), isSecure: true, panName);
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
                    
                string encOwnerId = CryptoHelper.Encrypt(ownerPkid.ToString(), _customKey);

                #endregion
                // 6. Return a success response
                return ApiResponseHelper.Success(encOwnerId, "Catering partner registered successfully. Your application is under review.");
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

        [AllowAnonymous]
        [RequestSizeLimit(long.MaxValue)]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        [HttpPost("UploadMedia")]
        public async Task<IActionResult> UploadKitchenMediaFile (string ownerId, [FromForm] List<IFormFile> CateringMedia)
        {
            if (CateringMedia == null || CateringMedia.Count == 0)
            {
                return BadRequest(new { message = "Invalid file data." });
            }
            try
            {
                OwnerRepository ownerRepository = new OwnerRepository(_connStr);
                long ownerPkid = CryptoHelper.DecryptAndConvert<long>(ownerId, _customKey);
                bool isOwnerExist = await ownerRepository.IsOwnerExistAsync(ownerPkid);
                if (!isOwnerExist)
                {
                    return ApiResponseHelper.Failure("Partner does not exist.");
                }
                var filePaths = new List<string>();
                if (CateringMedia != null && CateringMedia.Any())
                {
                    foreach (var file in CateringMedia)
                    {
                        var filePath = await _fileStorageService.SaveFormFileAsync(file, ownerPkid, DocumentType.Kitchen.GetDisplayName(), isSecure: false, file.FileName);
                        await ownerRepository.SaveFilePath(filePath, ownerPkid, file.FileName, DocumentType.Kitchen);
                    }
                }
                return ApiResponseHelper.Success(null, "Partner medias uploads successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading kitchen media file.");
                return StatusCode(500, "An error occurred while uploading the file.");
            }
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
