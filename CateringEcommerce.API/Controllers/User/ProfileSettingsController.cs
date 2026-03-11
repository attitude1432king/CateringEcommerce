using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Interfaces.User;
using CateringEcommerce.Domain.Models.User;
using CateringEcommerce.API.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CateringEcommerce.API.Controllers.User
{
    [UserAuthorize]
    [ApiController]
    [Route("api/User/ProfileSettings")]
    public class ProfileSettingsController : ControllerBase
    {
        private readonly ICurrentUserService _currentUser;
        private readonly IFileStorageService _fileStorageService;
        private readonly IProfileSetting _profileSetting;
        private readonly IUserRepository _userRepository;

        public ProfileSettingsController(
            ICurrentUserService currentUser,
            IFileStorageService fileStorageService,
            IProfileSetting profileSetting,
            IUserRepository userRepository)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
            _profileSetting = profileSetting ?? throw new ArgumentNullException(nameof(profileSetting));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        [HttpGet("GetUserProfile")]
        public IActionResult GetUserProfile()
        {
            try
            {
                var userIdClaim = _currentUser.UserId;
                UserModel userProfile = _userRepository.GetUserDetails(userIdClaim);
                return Ok(userProfile);
            }
            catch (Exception)
            {
                // Log the exception or handle it appropriately
                return StatusCode(500, "An error occurred while fetching location data.");
            }
        }

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

                await _profileSetting.UpdateUserDetails(userPKID, userData);

                return Ok(new { message = "Profile updated successfully.", user = _userRepository.GetUserDetails(userPKID) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating profile.", error = ex.Message });
            }
        }

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

                // Delete old profile photo if it exists
                var oldPhotoPath = _profileSetting.GetUserProfilePicture(userPKID);
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
                 await _profileSetting.UpdateUserDetails(userPKID, userData);

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