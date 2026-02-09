using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.User.AuthLogic;
using CateringEcommerce.BAL.Common;
using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Common;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CateringEcommerce.API.Controllers.User
{
    [Authorize]
    [Route("api/User/Auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly ISmsService _smsService;
        private readonly ITokenService _tokenService;
        private readonly IDatabaseHelper _dbHelper;

        public AuthController(ISmsService smsService, IConfiguration config, IDatabaseHelper dbHelper, ITokenService tokenService)
        {
            _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        [AllowAnonymous]
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] ActionRequest request)
        {
            try
            {
                Role role  = request.IsPartnerLogin ? Role.Owner : Role.User; // Determine role based on IsPartnerLogin
                if (string.IsNullOrEmpty(request.PhoneNumber) && !System.Text.RegularExpressions.Regex.IsMatch(request.PhoneNumber, @"^\+91[6-9]\d{9}$"))
                    return BadRequest(new { result = false, message = "The phone number you entered is not valid. Use + followed by the 10-digit number." });
                UserRepository authentication = new UserRepository(_dbHelper);
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
                        return ApiResponseHelper.Failure($"Phone number does not exist. Please {shortMsg} first.");
                    }
                }
                else
                {
                    if (authentication.IsExistNumber(request.PhoneNumber, role.GetDisplayName()))
                    {
                        return ApiResponseHelper.Failure("Phone number already exists. Please login instead.");
                    }
                    else
                    {
                        mgs = "OTP sent successfully for signup.";
                    }
                }
                _smsService.SendOtp(request.PhoneNumber);
                return ApiResponseHelper.Success(null, mgs);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message); // Internal Server Error
            }
        }

        [AllowAnonymous]
        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] OtpVerificationRequest request)
        {
            // Retrieve the stored OTP for the phone number
            // Validate the OTP and its expiration
            string msg = string.Empty;
            string roleName = request.IsPartnerLogin ? Role.Owner.GetDisplayName() : Role.User.GetDisplayName(); // Determine role based on IsPartnerLogin
            try
            {
                // Subscription is pending to the SMS service and verify the OTP
                //if (_smsService.VerifyOtp(request.PhoneNumber, request.Otp))
                if (true)
                {
                    Authentication authenticationDB = new Authentication(_dbHelper);
                    OwnerRepository ownerRepository = new OwnerRepository(_dbHelper);
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
                        return BadRequest(new { result = false, message = "Phone number or name is missing." });
                    }

                    // With the following explicit type handling:
                    object loginUserDetails = request.IsPartnerLogin
                        ? (object)ownerRepository.GetOwnerDetails(request.PhoneNumber)
                        : (object)authenticationDB.GetUserData(request.PhoneNumber);

                    // Fix: Use reflection or dynamic to access PkID property
                    string pkId = null;
                    if (loginUserDetails != null)
                    {
                        var pkIdProp = loginUserDetails.GetType().GetProperty("PkID");
                        if (pkIdProp != null)
                        {
                            pkId = pkIdProp.GetValue(loginUserDetails)?.ToString();
                        }
                    }

                    // Generate token with additional claims
                    var additionalClaims = new Dictionary<string, string>
                    {
                        { "UserId", pkId ?? "0" },
                        { "PhoneNumber", request.PhoneNumber ?? "" }
                    };
                    string newToken = _tokenService.GenerateToken(request.Name ?? "", roleName, additionalClaims);
                    return Ok(new { result = true, message = msg, token = newToken, user = loginUserDetails, role = roleName });
                }
                else
                {
                    return Ok(new { result = false, message = "Invalid or expired OTP." });
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
                UserRepository userRepository = new UserRepository(_dbHelper);
                Authentication authentication = new Authentication(_dbHelper);
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

        [Authorize]
        [HttpPost("final-verify")]
        public async Task<IActionResult> FinalVerification([FromBody] FinalRequest finalRequest)
        {
            try
            {
                if (finalRequest.UserPkId == 0)
                    return BadRequest(new { message = "Invalid user ID." });
                // Check OTP and email verification success here
                var claims = new List<Claim>();

                if (!string.IsNullOrWhiteSpace(finalRequest.Role))
                {
                    claims.Add(new Claim(ClaimTypes.Role, finalRequest.Role));
                }
                if (finalRequest.UserPkId > 0)
                {
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, finalRequest.UserPkId.ToString()));
                }


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

    // Change the access modifier from 'private' to 'internal' or remove it entirely for nested classes in a namespace scope

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

    public class FinalRequest
    {
        public Int64 UserPkId { get; set; }
        public string? Role { get; set; }
    }

}
