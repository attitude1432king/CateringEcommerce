using CateringEcommerce.API.Controllers.Owner.Menu;
using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CateringEcommerce.API.Controllers.Owner
{
    [ApiController]
    [Route("api/Owner/Staff")]
    [OwnerAuthorize]
    public class StaffController : ControllerBase
    {
        private readonly ILogger<StaffController> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly IFileStorageService _fileStorageService;
        private readonly IStaff _staffRepository;

        public StaffController(
            IFileStorageService fileStorageService,
            ILogger<StaffController> logger,
            ICurrentUserService currentUser,
            IStaff staffRepository)
        {
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _staffRepository = staffRepository ?? throw new ArgumentNullException(nameof(staffRepository));
        }

        /// <summary>
        /// Get staff count with optional filtering (SECURITY: Uses strongly-typed model to prevent SQL injection)
        /// </summary>
        [HttpGet("Count")]
        public async Task<IActionResult> GetStaffCount([FromQuery] StaffFilterRequest filter)
        {
            try
            {
                // Validate model
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }

                // Use strongly-typed filter instead of unsafe JSON string
                // Serialize filter to JSON for interface compatibility
                string filterJson = JsonSerializer.Serialize(filter);
                var staffCount = await _staffRepository.GetStaffCountAsync(ownerPKID, filterJson);
                _logger.LogInformation("Fetching staff count for owner {OwnerId}: {Count}", ownerPKID, staffCount);
                return Ok(staffCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting staff count for owner {OwnerId}", _currentUser.UserId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while retrieving staff count."));
            }
        }

        /// <summary>
        /// Get paginated staff list with filtering (SECURITY: Uses strongly-typed model to prevent SQL injection)
        /// </summary>
        [HttpGet("Data")]
        public async Task<IActionResult> GetStaffListAsync([FromQuery] StaffFilterRequest filter)
        {
            try
            {
                // Validate model
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }

                // Use strongly-typed filter with built-in pagination
                // Serialize filter to JSON for interface compatibility
                string filterJson = JsonSerializer.Serialize(filter);
                var staffList = await _staffRepository.GetStaffListAsync(ownerPKID, filter.PageNumber, filter.PageSize, filterJson);
                _logger.LogInformation("Retrieved {Count} staff members for owner {OwnerId}",
                    staffList?.Count ?? 0, ownerPKID);
                return Ok(staffList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting staff list for owner {OwnerId}", _currentUser.UserId);
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while retrieving staff list."));
            }
        }

        [HttpPost("Create")]
        [Consumes("multipart/form-data")]
        [RequestFormLimits(MultipartBodyLengthLimit = 25 * 1024 * 1024)]
        [RequestSizeLimit(25 * 1024 * 1024)]
        public async Task<IActionResult> AddStaffAsync(
            [FromForm] string JsonData,
            [FromForm] IFormFile? Profile,
            [FromForm] IFormFile? IdentityDocument,
            [FromForm] IFormFile? ResumeDocument)
        {
            try
            {
                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0)
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");

                StaffDto staff;
                try { staff = JsonSerializer.Deserialize<StaffDto>(JsonData ?? "{}"); }
                catch { return ApiResponseHelper.Failure("Invalid staff data.", "warning"); }

                if (staff == null || string.IsNullOrEmpty(staff.Contact))
                    return ApiResponseHelper.Failure("Invalid staff data.", "warning");

                var staffService = _staffRepository;
                bool numberExists = await staffService.IsStaffNumberExistsAsync(ownerPKID, staff.Contact);
                if (numberExists)
                    return ApiResponseHelper.Failure("Staff contact number already exists.", "warning");

                long? staffPKID = await staffService.AddStaffAsync(ownerPKID, staff);
                if (staffPKID <= 0)
                    return ApiResponseHelper.Failure("Failed to add staff.");

                var profileExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var docExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                Dictionary<string, string> dicPath = new Dictionary<string, string>();

                if (Profile != null && Profile.Length > 0)
                {
                    var v = FileValidationHelper.ValidateFile(Profile, profileExtensions, 5 * 1024 * 1024);
                    if (!v.IsValid) return ApiResponseHelper.Failure(v.ErrorMessage, "warning");
                    var safeFilename = FileValidationHelper.GenerateSafeFilename(Profile.FileName);
                    var profilePath = await _fileStorageService.SaveFormFileAsync(Profile, ownerPKID, DocumentType.Staff.GetDisplayName(), true, safeFilename, staffPKID);
                    dicPath.Add("ProfilePath", profilePath);
                }

                if (IdentityDocument != null && IdentityDocument.Length > 0)
                {
                    var v = FileValidationHelper.ValidateFile(IdentityDocument, docExtensions, 10 * 1024 * 1024);
                    if (!v.IsValid) return ApiResponseHelper.Failure(v.ErrorMessage, "warning");
                    var safeFilename = FileValidationHelper.GenerateSafeFilename(IdentityDocument.FileName);
                    var identityPath = await _fileStorageService.SaveFormFileAsync(IdentityDocument, ownerPKID, DocumentType.Staff.GetDisplayName(), true, safeFilename, staffPKID);
                    dicPath.Add("IdentityDocumentPath", identityPath);
                }

                if (ResumeDocument != null && ResumeDocument.Length > 0)
                {
                    var v = FileValidationHelper.ValidateFile(ResumeDocument, docExtensions, 10 * 1024 * 1024);
                    if (!v.IsValid) return ApiResponseHelper.Failure(v.ErrorMessage, "warning");
                    var safeFilename = FileValidationHelper.GenerateSafeFilename(ResumeDocument.FileName);
                    var resumePath = await _fileStorageService.SaveFormFileAsync(ResumeDocument, ownerPKID, DocumentType.Staff.GetDisplayName(), true, safeFilename, staffPKID);
                    dicPath.Add("ResumeDocumentPath", resumePath);
                }

                if (dicPath.Count > 0)
                    await staffService.UpdateStaffDocumentPath(ownerPKID, staffPKID, dicPath);

                return ApiResponseHelper.Success(null, $"{staff.Name} added successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding staff.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("Update")]
        [Consumes("multipart/form-data")]
        [RequestFormLimits(MultipartBodyLengthLimit = 25 * 1024 * 1024)]
        [RequestSizeLimit(25 * 1024 * 1024)]
        public async Task<IActionResult> UpdateStaffAsync(
            [FromForm] string JsonData,
            [FromForm] IFormFile? Profile,
            [FromForm] IFormFile? IdentityDocument,
            [FromForm] IFormFile? ResumeDocument)
        {
            try
            {
                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0)
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");

                StaffDto staff;
                try { staff = JsonSerializer.Deserialize<StaffDto>(JsonData ?? "{}"); }
                catch { return ApiResponseHelper.Failure("Invalid staff data.", "warning"); }

                if (staff == null || staff.ID <= 0 || string.IsNullOrEmpty(staff.Contact))
                    return ApiResponseHelper.Failure("Invalid staff ID.");

                var staffService = _staffRepository;
                bool numberExists = await staffService.IsStaffNumberExistsAsync(ownerPKID, staff.Contact, staff.ID);
                if (numberExists)
                    return ApiResponseHelper.Failure("Staff contact number already exists.", "warning");

                await staffService.UpdateStaffAsync(ownerPKID, staff);

                if (staff.FilesToDelete != null)
                {
                    foreach (var filePath in staff.FilesToDelete)
                    {
                        bool canDelete = await staffService.TryClearStaffFilePathAsync(ownerPKID, staff.ID, filePath);
                        if (canDelete)
                            _fileStorageService.DeleteFilePath(filePath);
                    }
                }

                var profileExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var docExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                Dictionary<string, string> dicPath = new Dictionary<string, string>();

                if (Profile != null && Profile.Length > 0)
                {
                    var v = FileValidationHelper.ValidateFile(Profile, profileExtensions, 5 * 1024 * 1024);
                    if (!v.IsValid) return ApiResponseHelper.Failure(v.ErrorMessage, "warning");
                    var safeFilename = FileValidationHelper.GenerateSafeFilename(Profile.FileName);
                    var profilePath = await _fileStorageService.SaveFormFileAsync(Profile, ownerPKID, DocumentType.Staff.GetDisplayName(), true, safeFilename, staff.ID);
                    dicPath.Add("ProfilePath", profilePath);
                }

                if (IdentityDocument != null && IdentityDocument.Length > 0)
                {
                    var v = FileValidationHelper.ValidateFile(IdentityDocument, docExtensions, 10 * 1024 * 1024);
                    if (!v.IsValid) return ApiResponseHelper.Failure(v.ErrorMessage, "warning");
                    var safeFilename = FileValidationHelper.GenerateSafeFilename(IdentityDocument.FileName);
                    var identityPath = await _fileStorageService.SaveFormFileAsync(IdentityDocument, ownerPKID, DocumentType.Staff.GetDisplayName(), true, safeFilename, staff.ID);
                    dicPath.Add("IdentityDocumentPath", identityPath);
                }

                if (ResumeDocument != null && ResumeDocument.Length > 0)
                {
                    var v = FileValidationHelper.ValidateFile(ResumeDocument, docExtensions, 10 * 1024 * 1024);
                    if (!v.IsValid) return ApiResponseHelper.Failure(v.ErrorMessage, "warning");
                    var safeFilename = FileValidationHelper.GenerateSafeFilename(ResumeDocument.FileName);
                    var resumePath = await _fileStorageService.SaveFormFileAsync(ResumeDocument, ownerPKID, DocumentType.Staff.GetDisplayName(), true, safeFilename, staff.ID);
                    dicPath.Add("ResumeDocumentPath", resumePath);
                }

                if (dicPath.Count > 0)
                    await staffService.UpdateStaffDocumentPath(ownerPKID, staff.ID, dicPath);

                return ApiResponseHelper.Success(null, $"{staff.Name} updated successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating staff.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("Delete")]
        public async Task<IActionResult> DeleteStaffAsync([FromBody] long staffId)
        {
            try
            {
                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0 || staffId <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }
                var staffService = _staffRepository;

                bool isValidStaff = await staffService.IsValidStaffId(ownerPKID, staffId);

                if(!isValidStaff)
                    return ApiResponseHelper.Failure("Invalid Staff ID or access denied.");

                var filePaths = await staffService.GetAllStaffFilePathsAsync(ownerPKID, staffId);
                foreach (var path in filePaths)
                {
                    _fileStorageService.DeleteFilePath(path);
                }

                int result = await staffService.SoftDeleteStaffAsync(ownerPKID, staffId);
                if (result <= 0)
                {
                    return ApiResponseHelper.Failure("Failed to delete staff.");
                }
                return ApiResponseHelper.Success(null, "Staff deleted successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting staff.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("UpdateStatus")]
        public async Task<IActionResult> UpdateStaffStatus([FromQuery] long staffId, bool status)
        {
            try
            {
                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }

                var staff = _staffRepository;
                bool isValid = await staff.IsValidStaffId(ownerPKID, staffId);
                if (staffId <= 0 || !isValid)
                {
                    return ApiResponseHelper.Failure("Invalid Staff ID or access denied.");
                }

                await staff.UpdateStaffStatus(ownerPKID, staffId, status);

                _logger.LogInformation("Update staff available status by ID: {0}", staffId);

                return ApiResponseHelper.Success(null, "Staff status updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while update staff status.");
                throw new Exception(ex.Message);
            }
        }

    }
}
