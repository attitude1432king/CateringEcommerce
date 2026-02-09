using CateringEcommerce.API.Controllers.Owner.Menu;
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
    [Authorize(Roles = "Owner")]
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
        public async Task<IActionResult> AddStaffAsync([FromBody] StaffDto staff)
        {
            try
            {
                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }
                var staffService = _staffRepository;
                bool numberExists = await staffService.IsStaffNumberExistsAsync(ownerPKID, staff.Contact);
                if (numberExists)
                {
                    return ApiResponseHelper.Failure("Staff contact number already exists.", "warning");
                }
                long? staffPKID = await staffService.AddStaffAsync(ownerPKID, staff);
                if (staffPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Failed to add staff.");
                }

                if(staff.Profile != null || staff.IdentityDocument != null || staff.ResumeDocument != null)
                {
                    Dictionary<string, string> dicPath = new Dictionary<string, string>();
                    if (staff.Profile != null)
                    {
                        var profilePath = await _fileStorageService.SaveFileAsync(staff.Profile.Base64, ownerPKID, DocumentType.Staff.GetDisplayName(), true, staff.Profile.Name, staffPKID);
                        dicPath.Add("ProfilePath", profilePath);
                    }
                    if (staff.IdentityDocument != null)
                    {
                        var identityPath = await _fileStorageService.SaveFileAsync(staff.IdentityDocument.Base64, ownerPKID, DocumentType.Staff.GetDisplayName(), true, staff.IdentityDocument.Name, staffPKID);
                        dicPath.Add("IdentityDocumentPath", identityPath);
                    }
                    if (staff.ResumeDocument != null)
                    {
                        var resumePath = await _fileStorageService.SaveFileAsync(staff.ResumeDocument.Base64, ownerPKID, DocumentType.Staff.GetDisplayName(), true, staff.ResumeDocument.Name, staffPKID);
                        dicPath.Add("ResumeDocumentPath", resumePath);
                    }
                    await staffService.UpdateStaffDocumentPath(ownerPKID, staffPKID, dicPath);
                }

                return ApiResponseHelper.Success(null, $"{staff.Name} added successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding staff.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("Update")]
        public async Task<IActionResult> UpdateStaffAsync([FromBody] StaffDto staff)
        {
            try
            {
                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }

                if (staff == null || staff.ID <= 0 || string.IsNullOrEmpty(staff.Contact))
                {
                    return ApiResponseHelper.Failure("Invalid staff ID.");
                }
                var staffService = _staffRepository;
                bool numberExists = await staffService.IsStaffNumberExistsAsync(ownerPKID, staff.Contact, staff.ID);
                if (numberExists)
                {
                    return ApiResponseHelper.Failure("Staff contact number already exists.", "warning");
                }
                await staffService.UpdateStaffAsync(ownerPKID, staff);

                if (staff.FilesToDelete != null) 
                { 
                    foreach (var filePath in staff.FilesToDelete)
                    {
                        bool canDelete = await staffService.TryClearStaffFilePathAsync(ownerPKID, staff.ID, filePath);
                        if (canDelete)
                        {
                            _fileStorageService.DeleteFilePath(filePath);
                        }
                    }
                }

                if (staff.Profile != null || staff.IdentityDocument != null || staff.ResumeDocument != null)
                {
                    Dictionary<string, string> dicPath = new Dictionary<string, string>();
                    if (staff.Profile != null)
                    {
                        var profilePath = await _fileStorageService.SaveFileAsync(staff.Profile.Base64, ownerPKID, DocumentType.Staff.GetDisplayName(), true, staff.Profile.Name, staff.ID);
                        dicPath.Add("ProfilePath", profilePath);
                    }
                    if (staff.IdentityDocument != null)
                    {
                        var identityPath = await _fileStorageService.SaveFileAsync(staff.IdentityDocument.Base64, ownerPKID, DocumentType.Staff.GetDisplayName(), true, staff.IdentityDocument.Name, staff.ID);
                        dicPath.Add("IdentityDocumentPath", identityPath);
                    }
                    if (staff.ResumeDocument != null)
                    {
                        var resumePath = await _fileStorageService.SaveFileAsync(staff.ResumeDocument.Base64, ownerPKID, DocumentType.Staff.GetDisplayName(), true, staff.ResumeDocument.Name, staff.ID);
                        dicPath.Add("ResumeDocumentPath", resumePath);
                    }
                    await staffService.UpdateStaffDocumentPath(ownerPKID, staff.ID, dicPath);
                }

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
