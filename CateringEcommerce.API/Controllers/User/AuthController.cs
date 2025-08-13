using CateringEcommerce.BAL.Base.User.AuthLogic;
using CateringEcommerce.BAL.Common;
using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Models;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;
using System.Security.Claims;

namespace CateringEcommerce.API.Controllers.User
{
    [Route("api/User/Auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly ISmsService _smsService;
        private readonly IConfiguration _config;
        private readonly TokenService _tokenService;
        private readonly string _connStr;

        public AuthController(ISmsService smsService, IConfiguration config)
        {
            _smsService = smsService;
            _config = config ?? throw new ArgumentNullException(nameof(config)); // Ensure config is not null
            _connStr = _config.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection string is not configured."); // Ensure connection string is not null
            _tokenService = new TokenService(config);
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] ActionRequest request)
        {

            try
            {
                Role role  = request.IsPartnerLogin ? Role.Owner : Role.User; // Determine role based on IsPartnerLogin
                if (string.IsNullOrEmpty(request.PhoneNumber) && !System.Text.RegularExpressions.Regex.IsMatch(request.PhoneNumber, @"^\+91[6-9]\d{9}$"))
                    return BadRequest(new { message = "The phone number you entered is not valid. Use + followed by the 10-digit number." });

                UserRepository authentication = new UserRepository(_connStr);
                string mgs = string.Empty;

                if (!string.IsNullOrEmpty(request.CurrentAction) && request.CurrentAction == "login")
                {
                    if (authentication.IsExistNumber(request.PhoneNumber, role.GetDisplayName()))
                    {
                        mgs = "OTP sent successfully for login.";
                    }
                    else
                    {
                        string shortMsg = role == Role.User ? "sign up" : "register";
                        return BadRequest(new { message = $"Phone number does not exist. Please {shortMsg} first." });
                    }
                }
                else
                {
                    if (authentication.IsExistNumber(request.PhoneNumber, role.GetDisplayName()))
                    {
                        return BadRequest(new { message = "Phone number already exists. Please login instead." });
                    }
                    else
                    {
                        mgs = "OTP sent successfully for signup.";
                    }
                }
                _smsService.SendOtp(request.PhoneNumber);

                return Ok(new { message = mgs });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message); // Internal Server Error
            }
        }

        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] OtpVerificationRequest request)
        {
            // Retrieve the stored OTP for the phone number
            // Validate the OTP and its expiration
            string msg = string.Empty;
            Role roleName = request.IsPartnerLogin ? Role.Owner : Role.User; // Determine role based on IsPartnerLogin
            try
            {
                if (_smsService.VerifyOtp(request.PhoneNumber, request.Otp))
                {
                    Authentication authenticationDB = new Authentication(_connStr);

                    if (!string.IsNullOrEmpty(request.PhoneNumber) && !string.IsNullOrEmpty(request.Name) && request.CurrentAction == "signup" && !request.IsPartnerLogin)
                    {
                        authenticationDB.CreateUserAccount(request.Name, request.PhoneNumber);
                        msg = "Create & verify account successfully.";
                    }
                    else if (!string.IsNullOrEmpty(request.PhoneNumber) && request.CurrentAction == "login")
                    {
                        msg = "OTP verified successfully.";
                    }
                    else
                    {
                        return BadRequest(new { message = "Phone number or name is missing." });
                    }
                    string newToken = _tokenService.GenerateToken(request.Name, roleName.GetDisplayName());
                    return Ok(new { message = msg, token = newToken, user = authenticationDB.GetUserData(request.PhoneNumber), role = roleName });
                }
                else
                {
                    return Ok(new { message = "Invalid or expired OTP." });
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message); // Internal Server Error
            }
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] string token)
        {
            try
            {
                UserRepository userRepository = new UserRepository(_connStr);
                Authentication authentication = new Authentication(_connStr);
                var payload = await GoogleJsonWebSignature.ValidateAsync(token);

                var email = payload.Email;
                var name = payload.Name;
                var picture = payload.Picture;

                // Check if user exists or insert new (use your DB logic here)
                if (!userRepository.IsExistEmail(email))
                {
                    Dictionary<string, string> keyValuePairs = new Dictionary<string, string>
                    {
                        { "isGoogleAuthention", "true" },
                        { "googleId", payload.Subject },
                        { "isVerified", "true" },
                        { "pictureUrl", picture }
                    };
                    int inserted = authentication.CreateUserAccount(name: name, dicData: keyValuePairs);

                    // Fix: Check if the insertion was successful based on the returned integer value
                    if (inserted <= 0)
                    {
                        return BadRequest(new { status = "error", message = "Failed to create user account." });
                    }
                }

                return Ok(new { status = "success", email, name, picture });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("final-verify")]
        public async Task<IActionResult> FinalVerification([FromQuery] long userPKID)
        {
            try
            {
                if (userPKID == 0)
                    return BadRequest(new { message = "Invalid user ID." });
                // Check OTP and email verification success here
                UserRepository _userService = new UserRepository(_connStr);
                UserModel user = _userService.GetUserDetails(userPKID);
                if (user == null)
                    return Unauthorized();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userPKID.ToString()), // Fix: Use userPKID.ToString() instead of user.Id.ToString()
                    new Claim(ClaimTypes.MobilePhone, user.Phone),
                };

                var identity = new ClaimsIdentity(claims, "CateringCookieAuth");
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("CateringCookieAuth", principal, new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                });

                return Ok(new { message = "Login complete" });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }

    }

    public class OtpVerificationRequest
    {
        public string? CurrentAction { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Name { get; set; }
        public string? Otp { get; set; }
        public bool IsPartnerLogin { get; set; }
    }

    public class ActionRequest
    {
        public string? CurrentAction { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsPartnerLogin { get; set; }
    }

}
