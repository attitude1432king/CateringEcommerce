using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.User.Profile;
using CateringEcommerce.BAL.Common;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;

namespace CateringEcommerce.API.Controllers.User
{
    [ApiController]
    [Route("api/User/ProfileSettings")]
    public class ProfileSettingsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _connStr;
        private readonly ICurrentUserService _currentUser;
        private readonly IFileStorageService _fileStorageService;

        // Constructor updated to initialize all required fields
        public ProfileSettingsController(IConfiguration configuration, ICurrentUserService currentUser, IFileStorageService fileStorageService)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _connStr = _configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
            _currentUser = currentUser;
            _fileStorageService = fileStorageService;
        }

        [Authorize]
        [HttpGet("GetUserProfile")]
        public IActionResult GetUserProfile()
        {
            try
            {
                var userIdClaim = _currentUser.UserId;
                UserRepository userRepository = new UserRepository(_connStr);
                UserModel userProfile = userRepository.GetUserDetails(userIdClaim);
                return Ok(userProfile);
            }
            catch (Exception)
            {
                // Log the exception or handle it appropriately
                return StatusCode(500, "An error occurred while fetching location data.");
            }
        }

        [Authorize]
        [HttpPost("UpdateProfile")]
        public async Task<IActionResult> UpdateProfileDetails([FromBody] UserModel request)
        {
            try
            {
                var userPKID = _currentUser.UserId;
                if (userPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid user.");
                }
                ProfileSetting profileSetting = new ProfileSetting(_connStr);
                UserRepository userRepository = new UserRepository(_connStr);
                Dictionary<string, string> userData = new Dictionary<string, string>();
                
                if(request.StateID > 0)
                    userData.Add("stateID", request.StateID.ToString());
                if(request.CityID > 0)
                    userData.Add("cityID", request.CityID.ToString());
                if(request.Description != null)
                    userData.Add("description", request.Description);

                if(userData.Count == 0)
                {
                    return BadRequest(new { message = "No valid data provided for update." });
                }

                await profileSetting.UpdateUserDetails(userPKID, userData);

                return Ok(new { message = "Profile updated successfully.", user = userRepository.GetUserDetails(userPKID) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating profile.", error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("UploadProfilePhoto")]
        public async Task<IActionResult> UploadProfilePhoto([FromBody] string profilePhoto)
        {
            try
            {
                var userPKID = _currentUser.UserId;
                if (userPKID <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid user.");
                }

                if (string.IsNullOrEmpty(profilePhoto))
                {
                    return BadRequest(new { message = "Profile photo is required." });
                }

                ProfileSetting profileSetting = new ProfileSetting(_connStr);

                // Delete old profile photo if it exists
                var oldPhotoPath = profileSetting.GetUserProfilePicture(userPKID);
                if (!string.IsNullOrEmpty(oldPhotoPath))
                {
                    _fileStorageService.DeleteFilePath(oldPhotoPath);
                }

                // Save new profile photo
                var profilePath = await _fileStorageService.SaveUserFileAsync(
                    profilePhoto,
                    userPKID,
                    DocumentType.UserProfilePhoto.GetDisplayName()
                );

                // Update picture URL in database
                var userData = new Dictionary<string, string>
                {
                    { "pictureUrl", profilePath }
                };
                 await profileSetting.UpdateUserDetails(userPKID, userData);

                return Ok(new
                {
                    message = "Profile photo uploaded successfully.",
                    photoUrl = profilePath,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error uploading profile photo.", error = ex.Message });
            }
        }

    }
}