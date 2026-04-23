using CateringEcommerce.API.Attributes;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Interfaces.Notification;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.APIModels.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.IO;
using System.Text.Json;

namespace CateringEcommerce.API.Controllers.Owner
{
    [ApiController]
    [Route("api/Auth/Owner")]
    public class RegistrationController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<RegistrationController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly string _customKey;
        private readonly IOwnerRepository _ownerRepository;
        private readonly IOwnerRegister _ownerRegister;
        private readonly INotificationHelper _notificationHelper;
        private readonly IAdminNotificationRepository _adminNotificationRepository;

        public RegistrationController(
            IFileStorageService fileStorageService,
            ILogger<RegistrationController> logger,
            IWebHostEnvironment env,
            IOptions<EncryptionSettings> setting,
            IOwnerRepository ownerRepository,
            IOwnerRegister ownerRegister,
            INotificationHelper notificationHelper,
            IAdminNotificationRepository adminNotificationRepository)
        {
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _customKey = setting?.Value?.CustomKey ?? throw new ArgumentNullException(nameof(setting));
            _ownerRepository = ownerRepository ?? throw new ArgumentNullException(nameof(ownerRepository));
            _ownerRegister = ownerRegister ?? throw new ArgumentNullException(nameof(ownerRegister));
            _notificationHelper = notificationHelper ?? throw new ArgumentNullException(nameof(notificationHelper));
            _adminNotificationRepository = adminNotificationRepository ?? throw new ArgumentNullException(nameof(adminNotificationRepository));
        }

        [AllowAnonymous]
        [RequestSizeLimit(50 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 50 * 1024 * 1024)]
        [HttpPost("Register")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Register(
            [FromForm] string JsonData,
            [FromForm] IFormFile? CateringLogo,
            [FromForm] IFormFile? FssaiCertificate,
            [FromForm] IFormFile? GstCertificate,
            [FromForm] IFormFile? PanCard,
            [FromForm] IFormFile? Signature,
            [FromForm] IFormFile? ChequeCopy)
        {
            OwnerRegistrationDto registrationData;
            try
            {
                registrationData = JsonSerializer.Deserialize<OwnerRegistrationDto>(JsonData ?? "{}",
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                return BadRequest(new { message = "Invalid registration data." });
            }

            if (registrationData == null)
                return BadRequest(new { message = "Invalid registration data." });

            TryValidateModel(registrationData);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation("Received registration request for Catering Partner: {CateringName}", registrationData.CateringName);

                if (string.IsNullOrEmpty(registrationData.Email) || string.IsNullOrEmpty(registrationData.Mobile))
                    return BadRequest(new { message = "Email and Phone number are required." });

                if (_ownerRepository.IsEmailExist(registrationData.Email) || _ownerRepository.IsOwnerPhoneExist(registrationData.Mobile))
                    return BadRequest(new { message = "Email or Phone number already exists. Please use a different one." });

                _logger.LogInformation("Email and phone number are valid and not already registered.");

                Dictionary<string, object> dicData = AddedRegistionDataToDictionary(registrationData);

                Int64 ownerPkid = _ownerRegister.CreateOwnerAccount(dicData);
                if (ownerPkid <= 0)
                {
                    _logger.LogError("Failed to create owner account in the database.");
                    return ApiResponseHelper.Failure("An error occurred while creating the partner account. Please try again.");
                }
                _logger.LogInformation("Owner account created successfully with PKID: {OwnerPkid}", ownerPkid);

                var logoExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var certExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                var sigExtensions = new[] { ".png" };

                // Logo
                string logoPath = string.Empty;
                if (CateringLogo != null && CateringLogo.Length > 0)
                {
                    var v = FileValidationHelper.ValidateFile(CateringLogo, logoExtensions, 5 * 1024 * 1024);
                    if (!v.IsValid) return ApiResponseHelper.Failure(v.ErrorMessage, "warning");
                    var safeFilename = FileValidationHelper.GenerateSafeFilename(CateringLogo.FileName);
                    logoPath = await _fileStorageService.SaveFormFileAsync(CateringLogo, ownerPkid, DocumentType.Logo.GetDisplayName(), false, safeFilename);
                }

                // FSSAI Certificate
                string fssaiPath = string.Empty;
                if (FssaiCertificate != null && FssaiCertificate.Length > 0)
                {
                    var v = FileValidationHelper.ValidateFile(FssaiCertificate, certExtensions, 10 * 1024 * 1024);
                    if (!v.IsValid) return ApiResponseHelper.Failure(v.ErrorMessage, "warning");
                    var safeFilename = FileValidationHelper.GenerateSafeFilename(FssaiCertificate.FileName);
                    fssaiPath = await _fileStorageService.SaveFormFileAsync(FssaiCertificate, ownerPkid, CertificateType.FSSAI.GetDisplayName(), true, safeFilename);
                }

                // GST Certificate
                string gstPath = string.Empty;
                if (GstCertificate != null && GstCertificate.Length > 0)
                {
                    var v = FileValidationHelper.ValidateFile(GstCertificate, certExtensions, 10 * 1024 * 1024);
                    if (!v.IsValid) return ApiResponseHelper.Failure(v.ErrorMessage, "warning");
                    var safeFilename = FileValidationHelper.GenerateSafeFilename(GstCertificate.FileName);
                    gstPath = await _fileStorageService.SaveFormFileAsync(GstCertificate, ownerPkid, CertificateType.GST.GetDisplayName(), true, safeFilename);
                }

                // PAN Card
                string panPath = string.Empty;
                if (PanCard != null && PanCard.Length > 0)
                {
                    var v = FileValidationHelper.ValidateFile(PanCard, certExtensions, 10 * 1024 * 1024);
                    if (!v.IsValid) return ApiResponseHelper.Failure(v.ErrorMessage, "warning");
                    var safeFilename = FileValidationHelper.GenerateSafeFilename(PanCard.FileName);
                    panPath = await _fileStorageService.SaveFormFileAsync(PanCard, ownerPkid, CertificateType.PAN.GetDisplayName(), true, safeFilename);
                }

                // Signature — save file first, then read bytes from disk for PDF embedding
                string signaturePath = string.Empty;
                string signatureBase64 = string.Empty;
                if (Signature != null && Signature.Length > 0)
                {
                    var v = FileValidationHelper.ValidateFile(Signature, sigExtensions, 2 * 1024 * 1024);
                    if (!v.IsValid) return ApiResponseHelper.Failure(v.ErrorMessage, "warning");
                    signaturePath = await _fileStorageService.SaveFormFileAsync(Signature, ownerPkid, CertificateType.Signature.GetDisplayName(), true, $"signature_{ownerPkid}.png");
                    if (!string.IsNullOrEmpty(signaturePath))
                    {
                        var physicalPath = Path.Combine(_env.WebRootPath, signaturePath.TrimStart('/'));
                        if (System.IO.File.Exists(physicalPath))
                        {
                            var sigBytes = await System.IO.File.ReadAllBytesAsync(physicalPath);
                            signatureBase64 = $"data:image/png;base64,{Convert.ToBase64String(sigBytes)}";
                        }
                    }
                }

                // Cheque Copy
                string chequePath = string.Empty;
                if (ChequeCopy != null && ChequeCopy.Length > 0)
                {
                    var v = FileValidationHelper.ValidateFile(ChequeCopy, certExtensions, 10 * 1024 * 1024);
                    if (!v.IsValid) return ApiResponseHelper.Failure(v.ErrorMessage, "warning");
                    var safeFilename = FileValidationHelper.GenerateSafeFilename(ChequeCopy.FileName);
                    chequePath = await _fileStorageService.SaveFormFileAsync(ChequeCopy, ownerPkid, CertificateType.PAN.GetDisplayName(), true, safeFilename);
                }

                if (!string.IsNullOrEmpty(gstPath))
                    dicData.Add("GstCertificatePath", gstPath);
                if (!string.IsNullOrEmpty(panPath))
                    dicData.Add("PanCertificatePath", panPath);
                if (!string.IsNullOrEmpty(fssaiPath))
                    dicData.Add("FssaiCertificatePath", fssaiPath);
                if (!string.IsNullOrEmpty(signaturePath))
                    dicData.Add("SignaturePath", signaturePath);
                if (!string.IsNullOrEmpty(chequePath))
                    dicData.Add("ChequePath", chequePath);

                #region Register the owner catering other details
                if (!string.IsNullOrEmpty(logoPath))
                    _ownerRegister.UpdateLogoPath(ownerPkid, logoPath);
                _ownerRegister.RegisterAddress(ownerPkid, dicData);
                _ownerRegister.RegisterServices(ownerPkid, dicData);
                _ownerRegister.RegisterLegalDocuments(ownerPkid, dicData);
                _ownerRegister.RegisterBankDetails(ownerPkid, dicData);

                string agreementText = GetDefaultAgreementText();
                dicData.Add("AgreementText", agreementText);
                dicData.Add("SignatureBase64", signatureBase64);
                dicData.Add("IpAddress", GetClientIpAddress());
                dicData.Add("UserAgent", Request.Headers["User-Agent"].ToString());

                _ownerRegister.RegisterAgreement(ownerPkid, dicData, _env.WebRootPath);

                _logger.LogInformation("Logo saved at: {LogoPath}", logoPath);
                _logger.LogInformation("FSSAI saved at: {FssaiPath}", fssaiPath);
                _logger.LogInformation("Signature saved at: {SignaturePath}", signaturePath);
                #endregion

                string encOwnerId = CryptoHelper.Encrypt(ownerPkid.ToString(), _customKey);

                // Send notifications for partner registration
                try
                {
                    await _notificationHelper.SendPartnerNotificationAsync(
                        "PARTNER_REGISTRATION_ACK",
                        registrationData.OwnerName,
                        registrationData.Email,
                        registrationData.Mobile,
                        new Dictionary<string, object>
                        {
                            { "owner_name", registrationData.OwnerName },
                            { "catering_name", registrationData.CateringName },
                            { "registration_date", DateTime.Now.ToString("dd MMM yyyy") },
                            { "partner_support_email", "partner-support@enyvora.com" },
                            { "partner_support_phone", "+91-1234567890" },
                            { "terms_url", "https://enyvora.com/partner-terms" }
                        }
                    );
                    _logger.LogInformation("Partner registration acknowledgement sent to {OwnerName}. OwnerId: {OwnerId}",
                        registrationData.OwnerName, ownerPkid);

                    _adminNotificationRepository.CreateNotification(
                        "NEW_PARTNER_REGISTRATION",
                        "New Partner Registration Pending Review",
                        $"New catering partner '{registrationData.CateringName}' has registered and is pending approval. Contact: {registrationData.OwnerName} ({registrationData.Mobile})",
                        ownerPkid,
                        "OWNER",
                        $"/admin/partner-requests/{ownerPkid}",
                        null
                    );
                    _logger.LogInformation("Admin notification created for new partner registration. OwnerId: {OwnerId}", ownerPkid);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send notifications for partner registration. OwnerId: {OwnerId}", ownerPkid);
                }

                return ApiResponseHelper.Success(encOwnerId, "Catering partner registered successfully. Your application is under review.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during owner registration.");
                return StatusCode(500, "An internal server error occurred. Please try again.");
            }
        }

        [AllowAnonymous]
        [HttpGet("GetPartnerAgreement")]
        public IActionResult GetPartnerAgreement()
        {
            try
            {
                // Default agreement text - can be updated to fetch from database or file
                string agreementText = @"PARTNER AGREEMENT

                    This Partner Agreement (""Agreement"") is entered into between ENYVORA (""Company"") and the Partner (""You"" or ""Partner"").

                    1. PARTNERSHIP TERMS
                    By signing this agreement, the Partner agrees to list their catering services on the ENYVORA platform and comply with all terms and conditions outlined herein.

                    2. SERVICES
                    The Partner agrees to:
                    - Provide accurate business information including business name, contact details, and address
                    - Maintain valid FSSAI, GST, PAN, and other required licenses at all times
                    - Upload authentic photographs of kitchen facilities, food items, and event setups
                    - Honor all orders received through the platform within agreed delivery timelines
                    - Maintain quality standards as per food safety regulations
                    - Respond to customer inquiries and complaints in a timely manner

                    3. QUALITY STANDARDS
                    - All food items must be prepared in hygienic conditions
                    - FSSAI license must be valid and displayed at business premises
                    - Quality of food and service must match the descriptions provided on platform
                    - Partner must inform customers of any allergens or dietary restrictions

                    4. PRICING AND PAYMENTS
                    - Partner has the right to set their own pricing for services
                    - Company will process payments on behalf of customers
                    - Payouts will be made weekly to the Partner's registered bank account
                    - Platform commission rates will be communicated separately and may be revised with prior notice
                    - All taxes are the responsibility of the Partner

                    5. CANCELLATIONS AND REFUNDS
                    - Partner must honor confirmed bookings unless circumstances are beyond control
                    - Any cancellations must be communicated immediately to customers and the Company
                    - Refund policies must comply with platform guidelines

                    6. INTELLECTUAL PROPERTY
                    - Partner grants Company the right to use business name, logo, and photographs for marketing
                    - All content uploaded by Partner must be owned or properly licensed
                    - Partner must not use Company's intellectual property without authorization

                    7. DATA PRIVACY
                    - Partner agrees to comply with data protection laws
                    - Customer information must be kept confidential and used only for order fulfillment
                    - Partner will not share customer data with third parties

                    8. TERMINATION
                    - Either party may terminate this agreement with 30 days written notice
                    - Company reserves the right to suspend or terminate accounts for violation of terms
                    - Upon termination, all pending orders must be fulfilled

                    9. LIABILITY
                    - Partner is solely responsible for food quality, safety, and customer satisfaction
                    - Company is not liable for any issues arising from Partner's services
                    - Partner must maintain appropriate insurance coverage

                    10. ACCEPTANCE
                    By signing below, the Partner acknowledges that they have read, understood, and agree to be bound by all terms and conditions of this Agreement.

                    Date: {CurrentDate}
                    Platform: Feasto Partners
                    Version: 1.0";

                agreementText = agreementText.Replace("{CurrentDate}", DateTime.Now.ToString("MMMM dd, yyyy"));

                return Ok(new
                {
                    result = true,
                    agreementText = agreementText,
                    message = "Agreement text retrieved successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching partner agreement.");
                return StatusCode(500, new { result = false, message = "An error occurred while fetching the agreement." });
            }
        }

        [AllowAnonymous]
        [HttpGet("Service/{TypeId}")]
        public async Task<IActionResult> GetServiceTypeDetails(int TypeId)
        {
            if (!Enum.IsDefined(typeof(ServiceType), TypeId))
            {
                return ApiResponseHelper.Failure("Invalid service type ID.");
            }

            var details = await _ownerRegister.GetServiceDetailsByTypeId(TypeId);

            return Ok(details);
        }

        [AllowAnonymous]
        [HttpPost("UploadMedia")]
        [EnableRateLimiting("file_upload")]
        [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> UploadKitchenMediaFile(string ownerId, [FromForm] IFormFile cateringMedia)
        {
            const long MaxFileSize = 10 * 1024 * 1024;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".mp4" };
            var validationResult = FileValidationHelper.ValidateFile(
                cateringMedia,
                allowedExtensions,
                MaxFileSize
            );

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("File upload validation failed: {Error}. Filename: {Filename}",
                    validationResult.ErrorMessage, cateringMedia?.FileName);
                return ApiResponseHelper.Failure(validationResult.ErrorMessage, type: "warning");
            }

            long ownerPkid;
            try
            {
                ownerPkid = CryptoHelper.DecryptAndConvert<long>(ownerId, _customKey);
            }
            catch
            {
                return ApiResponseHelper.Failure("Invalid owner identifier.");
            }

            if (!await _ownerRepository.IsOwnerExistAsync(ownerPkid))
                return ApiResponseHelper.Failure("Partner does not exist.");

            try
            {
                var safeFilename = FileValidationHelper.GenerateSafeFilename(cateringMedia.FileName);

                string savedPath = await _fileStorageService.SaveFormFileAsync(
                    cateringMedia,
                    ownerPkid,
                    DocumentType.Kitchen.GetDisplayName(),
                    isSecure: false,
                    safeFilename
                );

                if (string.IsNullOrWhiteSpace(savedPath))
                    return ApiResponseHelper.Failure("File upload failed.");

                await _ownerRepository.SaveFilePath(
                    savedPath,
                    ownerPkid,
                    cateringMedia.FileName,
                    DocumentType.Kitchen
                );

                return ApiResponseHelper.Success(
                    new { filePath = savedPath },
                    "Kitchen media uploaded successfully."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Kitchen media upload failed. OwnerId: {OwnerId}",
                    ownerPkid);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponseHelper.Failure("An error occurred while uploading the file.")
                );
            }
        }

        private string GetClientIpAddress()
        {
            try
            {
                string ipAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (string.IsNullOrEmpty(ipAddress))
                    ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                return ipAddress ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private string GetDefaultAgreementText()
        {
            return @"PARTNER AGREEMENT

This Partner Agreement (""Agreement"") is entered into between ENYVORA (""Company"") and the Partner (""You"" or ""Partner"").

1. PARTNERSHIP TERMS
By signing this agreement, the Partner agrees to list their catering services on the ENYVORA platform and comply with all terms and conditions outlined herein.

2. SERVICES
The Partner agrees to:
- Provide accurate business information including business name, contact details, and address
- Maintain valid FSSAI, GST, PAN, and other required licenses at all times
- Upload authentic photographs of kitchen facilities, food items, and event setups
- Honor all orders received through the platform within agreed delivery timelines
- Maintain quality standards as per food safety regulations
- Respond to customer inquiries and complaints in a timely manner

3. QUALITY STANDARDS
- All food items must be prepared in hygienic conditions
- FSSAI license must be valid and displayed at business premises
- Quality of food and service must match the descriptions provided on platform
- Partner must inform customers of any allergens or dietary restrictions

4. PRICING AND PAYMENTS
- Partner has the right to set their own pricing for services
- Company will process payments on behalf of customers
- Payouts will be made weekly to the Partner's registered bank account
- Platform commission rates will be communicated separately and may be revised with prior notice
- All taxes are the responsibility of the Partner

5. CANCELLATIONS AND REFUNDS
- Partner must honor confirmed bookings unless circumstances are beyond control
- Any cancellations must be communicated immediately to customers and the Company
- Refund policies must comply with platform guidelines

6. INTELLECTUAL PROPERTY
- Partner grants Company the right to use business name, logo, and photographs for marketing
- All content uploaded by Partner must be owned or properly licensed
- Partner must not use Company's intellectual property without authorization

7. DATA PRIVACY
- Partner agrees to comply with data protection laws
- Customer information must be kept confidential and used only for order fulfillment
- Partner will not share customer data with third parties

8. TERMINATION
- Either party may terminate this agreement with 30 days written notice
- Company reserves the right to suspend or terminate accounts for violation of terms
- Upon termination, all pending orders must be fulfilled

9. LIABILITY
- Partner is solely responsible for food quality, safety, and customer satisfaction
- Company is not liable for any issues arising from Partner's services
- Partner must maintain appropriate insurance coverage

10. ACCEPTANCE
By signing below, the Partner acknowledges that they have read, understood, and agree to be bound by all terms and conditions of this Agreement.

Date: " + DateTime.Now.ToString("MMMM dd, yyyy") + @"
Platform: ENYVORA Partners
Version: 1.0";
        }

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
                { "StateID", registrationData.StateID},
                { "CityID", registrationData.CityID},
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
                { "UpiId", registrationData.UpiId ?? string.Empty },
                { "AgreementAccepted", registrationData.AgreementAccepted }
            };
            return dicData;
        }
    }
}
