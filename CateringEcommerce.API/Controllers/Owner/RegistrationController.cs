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

namespace CateringEcommerce.API.Controllers.Owner
{
    [Authorize]
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
                Int64 ownerPkid = 0;

                if (string.IsNullOrEmpty(registrationData.Email) || string.IsNullOrEmpty(registrationData.Mobile))
                {
                    return BadRequest(new { message = "Email and Phone number are required." });
                }

                // Here you would typically check if the email or phone number already exists in your database
                if (_ownerRepository.IsEmailExist(registrationData.Email) || _ownerRepository.IsOwnerPhoneExist(registrationData.Mobile))
                {
                    return BadRequest(new { message = "Email or Phone number already exists. Please use a different one." });
                }
                _logger.LogInformation("Email and phone number are valid and not already registered.");


                _logger.LogInformation("Registration data converted to dictionary for database insertion.");
                dicData = AddedRegistionDataToDictionary(registrationData);

                // 1. Create a new owner in the database and get the PKID
                ownerPkid = _ownerRegister.CreateOwnerAccount(dicData);
                if (ownerPkid <= 0)
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

                // Signature
                var signatureBase64 = registrationData.Signature ?? string.Empty;
                var signaturePath = await _fileStorageService.SaveFileAsync(signatureBase64, ownerPkid, CertificateType.Signature.GetDisplayName(), isSecure: true, $"signature_{ownerPkid}.png");

                if (!string.IsNullOrEmpty(gstPath))
                    dicData.Add("GstCertificatePath", gstPath);
                if (!string.IsNullOrEmpty(panPath))
                    dicData.Add("PanCertificatePath", panPath);
                if (!string.IsNullOrEmpty(fssaiPath))
                    dicData.Add("FssaiCertificatePath", fssaiPath);
                if (!string.IsNullOrEmpty(signaturePath))
                    dicData.Add("SignaturePath", signaturePath);

                //3. Create a method to save everything to the database

                #region Register the owner catering other details
                if (!string.IsNullOrEmpty(logoPath) && registrationData.CateringLogo != null)
                    _ownerRegister.UpdateLogoPath(ownerPkid, logoPath);
                _ownerRegister.RegisterAddress(ownerPkid, dicData);
                _ownerRegister.RegisterServices(ownerPkid, dicData);
                _ownerRegister.RegisterLegalDocuments(ownerPkid, dicData);
                _ownerRegister.RegisterBankDetails(ownerPkid, dicData);

                // Fetch agreement text and add additional data for PDF generation
                string agreementText = GetDefaultAgreementText();
                dicData.Add("AgreementText", agreementText);
                dicData.Add("SignatureBase64", registrationData.Signature ?? string.Empty);
                dicData.Add("IpAddress", GetClientIpAddress());
                dicData.Add("UserAgent", Request.Headers["User-Agent"].ToString());

                _ownerRegister.RegisterAgreement(ownerPkid, dicData, _env.WebRootPath);

                _logger.LogInformation("Logo saved at: {LogoPath}", logoPath);
                _logger.LogInformation("FSSAI saved at: {FssaiPath}", fssaiPath);
                _logger.LogInformation("Signature saved at: {SignaturePath}", signaturePath);

                string encOwnerId = CryptoHelper.Encrypt(ownerPkid.ToString(), _customKey);

                // Send notifications for partner registration
                try
                {
                    // 1. Send acknowledgement to partner (Email + SMS)
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

                    // 2. Send notification to admin (In-App)
                    _adminNotificationRepository.CreateNotification(
                        "NEW_PARTNER_REGISTRATION",
                        "New Partner Registration Pending Review",
                        $"New catering partner '{registrationData.CateringName}' has registered and is pending approval. Contact: {registrationData.OwnerName} ({registrationData.Mobile})",
                        ownerPkid,
                        "OWNER",
                        $"/admin/partner-requests/{ownerPkid}",  // link to partner approval page
                        null   // adminId null = broadcast to all admins
                    );
                    _logger.LogInformation("Admin notification created for new partner registration. OwnerId: {OwnerId}", ownerPkid);
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the registration
                    _logger.LogError(ex, "Failed to send notifications for partner registration. OwnerId: {OwnerId}", ownerPkid);
                }

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

            //Your logic to fetch and return service type details goes here.
            var details = await _ownerRegister.GetServiceDetailsByTypeId(TypeId);

            return Ok(details);
        }

        [AllowAnonymous]
        [HttpPost("UploadMedia")]
        [EnableRateLimiting("file_upload")]
        [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)] // 10 MB
        [RequestSizeLimit(10 * 1024 * 1024)]                             // 10 MB
        public async Task<IActionResult> UploadKitchenMediaFile(string ownerId, [FromForm] IFormFile cateringMedia)
        {
            const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

            // ---------------------------
            // 1. SECURITY: Validate file with signature checking
            // ---------------------------
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

            // ---------------------------
            // 2. Validate owner
            // ---------------------------
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

            // ---------------------------
            // 3. SECURITY: Sanitize filename and save file
            // ---------------------------
            try
            {
                // Generate safe filename to prevent path traversal attacks
                var safeFilename = FileValidationHelper.GenerateSafeFilename(cateringMedia.FileName);

                string savedPath = await _fileStorageService.SaveFormFileAsync(
                    cateringMedia,
                    ownerPkid,
                    DocumentType.Kitchen.GetDisplayName(),
                    isSecure: false,
                    safeFilename  // Use sanitized filename
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



        /// <summary>
        /// Get client IP address from request
        /// </summary>
        /// <returns></returns>
        private string GetClientIpAddress()
        {
            try
            {
                // Try to get IP from X-Forwarded-For header (for proxies/load balancers)
                string ipAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault();

                if (string.IsNullOrEmpty(ipAddress))
                {
                    // Fall back to RemoteIpAddress
                    ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                }

                return ipAddress ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Get default agreement text
        /// </summary>
        /// <returns></returns>
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
                { "ChequePath", registrationData.ChequePath ?? string.Empty }, // Fix for CS8604
                { "UpiId", registrationData.UpiId ?? string.Empty }, // Fix for CS8604
                { "AgreementAccepted", registrationData.AgreementAccepted }
            };
            return dicData;
        }
    }
}
