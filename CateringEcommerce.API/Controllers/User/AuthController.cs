using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.User.AuthLogic;
using CateringEcommerce.BAL.Common;
using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Interfaces.User;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace CateringEcommerce.API.Controllers.User
{
    [Authorize]
    [Route("api/User/Auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly ISmsService _smsService;
        private readonly ITokenService _tokenService;
        private readonly IUserRepository _userRepository;
        private readonly IAuthentication _authentication;
        private readonly IOwnerRepository _ownerRepository;
        private readonly ISystemSettingsProvider _settings;

        public AuthController(ISmsService smsService, IOwnerRepository ownerRepository, ITokenService tokenService, IUserRepository userRepository, IAuthentication authentication, ISystemSettingsProvider settings)
        {
            _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
            _ownerRepository = ownerRepository ?? throw new ArgumentNullException(nameof(ownerRepository));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _userRepository = userRepository;
            _authentication = authentication;
            _settings = settings;
        }

        [AllowAnonymous]
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] ActionRequest request)
        {
            try
            {
                Role role = request.IsPartnerLogin ? Role.Owner : Role.User; // Determine role based on IsPartnerLogin
                
                // Validate phone number format
                if (string.IsNullOrEmpty(request.PhoneNumber) || !System.Text.RegularExpressions.Regex.IsMatch(request.PhoneNumber, @"^[6-9]\d{9}$"))
                    return BadRequest(new { result = false, message = "The phone number you entered is not valid. Use + followed by the 10-digit number." });
                
                string mgs = string.Empty;

                if (!string.IsNullOrEmpty(request.CurrentAction) && request.CurrentAction == "login")
                {
                    // LOGIN FLOW
                    if (_userRepository.IsExistNumber(request.PhoneNumber, role.GetDisplayName()))
                    {
                        // For Partner/Owner login - Check approval status
                        if (role == Role.Owner)
                        {
                            var (exists, approvalStatus) = _userRepository.CheckOwnerWithApprovalStatus(request.PhoneNumber);
                            
                            if (!exists || approvalStatus == null)
                            {
                                return ApiResponseHelper.Failure("Partner registration not found. Please register first.");
                            }

                            // Check approval status and return appropriate message
                            string approvalMessage = GetApprovalStatusMessage(approvalStatus.Value);
                            
                            if (!string.IsNullOrEmpty(approvalMessage))
                            {
                                return ApiResponseHelper.Failure(approvalMessage);
                            }
                            
                            // If approved, proceed with OTP
                            mgs = "OTP sent successfully for login.";
                        }
                        else
                        {
                            // User login - no approval status check needed
                            mgs = "OTP sent successfully for login.";
                        }
                    }
                    else
                    {
                        string shortMsg = role == Role.User ? "sign up" : "register";
                        return ApiResponseHelper.Failure($"Phone number does not exist. Please {shortMsg} first.");
                    }
                }
                else
                {
                    // SIGNUP FLOW
                    if (_userRepository.IsExistNumber(request.PhoneNumber, role.GetDisplayName()))
                    {
                        // For Partner signup - Check if already registered
                        if (role == Role.Owner)
                        {
                            var (exists, approvalStatus) = _userRepository.CheckOwnerWithApprovalStatus(request.PhoneNumber);
                            
                            if (exists)
                            {
                                return ApiResponseHelper.Failure("This phone number is already registered. Please login instead.");
                            }
                        }
                        else
                        {
                            return ApiResponseHelper.Failure("Phone number already exists. Please login instead.");
                        }
                    }
                    else
                    {
                        mgs = role == Role.User ? "OTP sent successfully for signup." : "OTP sent successfully for partner registration.";
                    }
                }

                // Send OTP via SMS service
                _smsService.SendOtp(request.PhoneNumber);
                return ApiResponseHelper.Success(null, mgs);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message); // Internal Server Error
            }
        }

        /// <summary>
        /// Get status message based on approval status enum value
        /// Returns empty string if approved, otherwise returns status message
        /// </summary>
        private string GetApprovalStatusMessage(int approvalStatusValue)
        {
            return approvalStatusValue switch
            {
                (int)CateringEcommerce.Domain.Enums.Admin.ApprovalStatus.Approved => "", // Empty means approved, no message
                (int)CateringEcommerce.Domain.Enums.Admin.ApprovalStatus.Pending => "Your registration is pending approval. Please wait for admin approval.",
                (int)CateringEcommerce.Domain.Enums.Admin.ApprovalStatus.UnderReview => "Your registration is under review. Our team will review and get back to you soon.",
                (int)CateringEcommerce.Domain.Enums.Admin.ApprovalStatus.Info_Requested => "We need more information to process your registration. Please check your email for details.",
                (int)CateringEcommerce.Domain.Enums.Admin.ApprovalStatus.Rejected => "Your registration has been rejected. Please contact support for more information.",
                _ => "Your registration status is unknown. Please contact support."
            };
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
                if (_smsService.VerifyOtp(request.PhoneNumber ?? string.Empty, request.Otp ?? string.Empty))
                {
                    if (!string.IsNullOrEmpty(request.PhoneNumber) && !string.IsNullOrEmpty(request.Name) && request.CurrentAction == "signup" && !request.IsPartnerLogin)
                    {
                        _authentication.CreateUserAccount(request.Name, request.PhoneNumber);
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
                        ? (object)_ownerRepository.GetOwnerDetails(request.PhoneNumber)
                        : (object)_authentication.GetUserData(request.PhoneNumber);

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
                    string newToken = _tokenService.GenerateToken(pkId ?? "", roleName, additionalClaims);

                    // Set token in httpOnly cookie — not exposed to JavaScript
                    var expireMinutes = _settings.GetInt("JWT.EXPIRE_MINUTES", 1440);
                    Response.Cookies.Append("authToken", newToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Lax,
                        Path = "/",
                        Expires = DateTimeOffset.UtcNow.AddMinutes(expireMinutes)
                    });

                    return Ok(new { result = true, message = msg, user = loginUserDetails, role = roleName });
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
                var payload = await GoogleJsonWebSignature.ValidateAsync(token);

                var email = payload.Email;
                var name = payload.Name;
                var picture = payload.Picture;

                // Check if user exists or insert new (use your DB logic here)
                if (!_userRepository.IsExistEmail(email))
                {
                    Dictionary<string, string> keyValuePairs = new Dictionary<string, string>
                    {
                        { "isGoogleAuthention", "true" },
                        { "googleId", payload.Subject },
                        { "isVerified", "true" },
                        { "pictureUrl", picture }
                    };
                    int inserted = _authentication.CreateUserAccount(name: name, dicData: keyValuePairs);

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
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Append("authToken", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            });
            return Ok(new { result = true, message = "Logged out successfully." });
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var user = HttpContext.User;
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = user.FindFirst(ClaimTypes.Role)?.Value;
            var phone = user.FindFirst("PhoneNumber")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { result = false, message = "Invalid session." });

            return Ok(new { result = true, userId, role, phoneNumber = phone });
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
