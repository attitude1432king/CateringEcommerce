using CateringEcommerce.BAL.Base.User.Profile;
using CateringEcommerce.BAL.Common;
using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using static System.Net.WebRequestMethods;

namespace CateringEcommerce.API.Controllers.User
{
    [ApiController]
    [Route("api/User/ProfileSettings")]
    public class ProfileSettingsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _connStr;

        // Constructor updated to initialize all required fields
        public ProfileSettingsController(IConfiguration configuration, IOptions<EmailSettings> emailSettings, ISmsService smsService)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _connStr = _configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
        }

        [HttpGet("GetUserProfile")]
        public IActionResult GetUserProfile([FromQuery] long userPKID)
        {
            try
            {
                UserRepository userRepository = new UserRepository(_connStr);
                UserModel userProfile = userRepository.GetUserDetails(userPKID);
                return Ok(userProfile);
            }
            catch (Exception)
            {
                // Log the exception or handle it appropriately
                return StatusCode(500, "An error occurred while fetching location data.");
            }
        }


        [HttpPost("UpdateProfile/{userPKID}")]
        public IActionResult UpdateProfileDetails(long userPKID, [FromBody] UserModel request)
        {
            try
            {
                ProfileSetting profileSetting = new ProfileSetting(_connStr);
                UserRepository userRepository = new UserRepository(_connStr);
                Dictionary<string, string> userData = new Dictionary<string, string>();
                if(request.StateID > 0)
                    userData.Add("stateID",request.StateID.ToString());
                if(request.CityID > 0)
                    userData.Add("cityID", request.CityID.ToString());
                if(request.Description != null)
                    userData.Add("description", request.Description);
                if(request.ProfilePhoto != null)
                    userData.Add("pictureUrl", request.ProfilePhoto);

                if(userData.Count == 0)
                {
                    return BadRequest(new { message = "No valid data provided for update." });
                }
                else
                {
                    profileSetting.UpdateUserDetails(userPKID, userData);
                }

                return Ok(new { message = "Profile updated successfully.", user = userRepository.GetUserDetails(userPKID) });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}