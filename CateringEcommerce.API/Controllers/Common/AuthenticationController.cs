using CateringEcommerce.BAL.Common;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Interfaces.User;
using CateringEcommerce.Domain.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.API.Controllers.Common
{
    [Authorize]
    [ApiController]
    [Route("api/Common/Auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly IUserRepository _userRepository;
        private readonly IProfileSetting _profileSetting;
        private const string EmailType = "email";
        private const string PhoneType = "phone";
        private const string CateringNumberType = "cateringNumber";

        public AuthenticationController(
            IEmailService emailService,
            ISmsService smsService,
            IUserRepository userRepository,
            IProfileSetting profileSetting)
        {
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _profileSetting = profileSetting ?? throw new ArgumentNullException(nameof(profileSetting));
        }

        [AllowAnonymous]
        [HttpPost("send-otp")]
        [EnableRateLimiting("otp_send")]
        public async Task<IActionResult> SendOtp([FromBody] VerificationRequest request)
        {
            try
            {
                if (request.Type == EmailType && (string.IsNullOrEmpty(request.Value) || !System.Text.RegularExpressions.Regex.IsMatch(request.Value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")))
                {
                    return BadRequest(new { result = false, message = "Invalid email format." });
                }
                else if (request.Type == PhoneType && (string.IsNullOrEmpty(request.Value) || !System.Text.RegularExpressions.Regex.IsMatch(request.Value, @"^\+?[1-9]\d{1,14}$")))
                {
                    return BadRequest(new { result = false, message = "Invalid phone number format." });
                }

                var otp = Utils.GenerateOtp();
                if (request.Type == EmailType && !_userRepository.IsExistEmail(request.Value, request.Role))
                {
                    _emailService.StoreOtp(request.Value, otp);
                    //await _emailService.SendOtpAsync(request.Value, otp);
                }
                else if ((request.Type == PhoneType || request.Type == CateringNumberType) && !_userRepository.IsExistRoleBaseNumber(request.Value, request.Type, request.Role))
                {
                    _emailService.StoreOtp(request.Value, otp);
                    //_smsService.SendOtp(request.Value);
                }
                else
                {
                    return BadRequest(new { result = false, role = request.Role, message = $"{(request.Type == EmailType ? EmailType : PhoneType + " number")} is already exists." });
                }

                return Ok(new { result = true, role = request.Role, message = $"OTP sent to {(request.Type == EmailType ? EmailType : PhoneType)}." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] SendOtp failed: {ex.Message}");
                return StatusCode(500, new { result = false, message = "An error occurred while sending OTP. Please try again later." });
            }
        }

        [AllowAnonymous]
        [HttpPost("verify-otp")]
        [EnableRateLimiting("otp_verify")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerificationRequest request)
        {
            try
            {
                bool isValid = false;
                Dictionary<string, string> userData = new Dictionary<string, string>();

                if (request.Type == EmailType && string.IsNullOrEmpty(request.Value))
                {
                    return BadRequest(new { result = false, message = "Email value cannot be null or empty." });
                }

                if (string.IsNullOrEmpty(request.Otp))
                {
                    return BadRequest(new { result = false, message = "OTP value cannot be null or empty." });
                }

                if (request.Type == EmailType)
                {
                    isValid = _emailService.VerifyOtp(request.Value, request.Otp);
                    if (isValid)
                    {
                        userData.Add("email", request.Value);
                    }
                }
                else if (request.Type == PhoneType || request.Type == CateringNumberType)
                {
                    isValid = _smsService.VerifyOtp(request.Value, request.Otp);
                    if (isValid)
                    {
                        userData.Add("phone", request.Value);
                    }
                }

                if (!isValid)
                {
                    return BadRequest(new { result = false, message = "Invalid or expired OTP." });
                }

                if (request.pkID > 0)
                {
                    await _profileSetting.UpdateUserDetails(request.pkID, userData);
                }

                return Ok(new { result = true, message = "OTP verified successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] VerifyOtp failed: {ex.Message}");
                return StatusCode(500, new { result = false, message = "An error occurred while verifying OTP. Please try again later." });
            }
        }
    }

    public class VerificationRequest
    {
        [Required(ErrorMessage = "Type is required")]
        [RegularExpression("^(email|phone|cateringNumber)$", ErrorMessage = "Type must be 'email', 'phone', or 'cateringNumber'")]
        public string? Type { get; set; }

        [Required(ErrorMessage = "Value is required")]
        [StringLength(200, ErrorMessage = "Value cannot exceed 200 characters")]
        public string? Value { get; set; }

        [Required(ErrorMessage = "OTP is required")]
        [StringLength(10, MinimumLength = 4, ErrorMessage = "OTP must be between 4 and 10 characters")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "OTP must contain only digits")]
        public string? Otp { get; set; }

        [Range(0, long.MaxValue, ErrorMessage = "Invalid user ID")]
        public long? pkID { get; set; }

        [StringLength(50, ErrorMessage = "Role cannot exceed 50 characters")]
        public string? Role { get; set; }
    }
}
