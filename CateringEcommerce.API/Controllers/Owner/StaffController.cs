using CateringEcommerce.API.Controllers.Owner.Menu;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.Owner;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;

namespace CateringEcommerce.API.Controllers.Owner
{
    [ApiController]
    [Route("api/Owner/Staff")]
    [Authorize(Roles = "Owner")]
    public class StaffController : ControllerBase
    {
        private readonly string _connStr;
        private readonly ILogger<PackagesController> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly IFileStorageService _fileStorageService;

        public StaffController(IFileStorageService fileStorageService, ILogger<PackagesController> logger, IConfiguration configuration, ICurrentUserService currentUser)
        {
            _fileStorageService = fileStorageService;
            _logger = logger;
            _connStr = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
            _currentUser = currentUser;
        }

        [HttpGet("Count")]
        public async Task<IActionResult> GetStaffCount(string filterJson)
        {
            try
            {
                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }
                var staffService = new Staff(_connStr);
                var staffCount = await staffService.GetStaffCountAsync(ownerPKID, filterJson);
                _logger.LogInformation("Fetching the staff count: " + staffCount);
                return Ok(staffCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting staff count.");
                throw new Exception(ex.Message);
            }
        }

        [HttpGet("Data")]
        public async Task<IActionResult> GetStaffListAsync(int page, int pageSize, string filterJson)
        {
            try
            {
                var ownerPKID = _currentUser.UserId;
                if (ownerPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid owner PKID or access denied.");
                }
                var staffService = new Staff(_connStr);
                var staffList = await staffService.GetStaffListAsync(ownerPKID, page, pageSize, filterJson);
                return Ok(staffList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting staff list.");
                return StatusCode(500, "Internal server error");
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
                var staffService = new Staff(_connStr);
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
                var staffService = new Staff(_connStr);
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
                var staffService = new Staff(_connStr);
                
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

                Staff staff = new Staff(_connStr);
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
