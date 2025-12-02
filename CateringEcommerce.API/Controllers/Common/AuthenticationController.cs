using CateringEcommerce.BAL.Base.User.Profile;
using CateringEcommerce.BAL.Common;
using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CateringEcommerce.API.Controllers.Common
{
    [Authorize]
    [ApiController]
    [Route("api/Common/Auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly string _connStr;
        private readonly EmailService _emailService;
        private readonly ISmsService _smsService;
        private const string EmailType = "email";
        private const string PhoneType = "phone";
        private const string CateringNumberType = "cateringNumber";

        // Constructor updated to initialize all required fields
        public AuthenticationController(IConfiguration configuration, IOptions<EmailSettings> emailSettings, ISmsService smsService)
        {
            _connStr = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
            _emailService = new EmailService(emailSettings ?? throw new ArgumentNullException(nameof(emailSettings)));
            _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
        }

        [AllowAnonymous]
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] VerificationRequest request)
        {
            try
            {
                if (request.Type == EmailType && string.IsNullOrEmpty(request.Value) && !System.Text.RegularExpressions.Regex.IsMatch(request.Value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    return BadRequest(new { result = false, message = "Invalid email format." });
                }
                else if (request.Type == PhoneType && string.IsNullOrEmpty(request.Value) && !System.Text.RegularExpressions.Regex.IsMatch(request.Value, @"^\+?[1-9]\d{1,14}$"))
                {
                    return BadRequest(new { result = false, message = "Invalid phone number format." });
                }

                UserRepository userRepository = new UserRepository(_connStr);

                var otp = Utils.GenerateOtp();
                if (request.Type == EmailType && !userRepository.IsExistEmail(request.Value, request.Role))
                {
                    _emailService.StoreOtp(request.Value, otp);
                    await _emailService.SendOtpAsync(request.Value, otp);
                }
                else if ((request.Type == PhoneType || request.Type == CateringNumberType) && !userRepository.IsExistRoleBaseNumber(request.Value, request.Type, request.Role))
                {
                    _emailService.StoreOtp(request.Value, otp);
                    _smsService.SendOtp(request.Value);
                }
                else
                {
                    return BadRequest(new { result = false, role = request.Role, message = $"{(request.Type == EmailType ? EmailType : PhoneType + " number")} is already exists." });
                }

                return Ok(new { result = true, role = request.Role, message = $"OTP sent to {(request.Type == EmailType ? EmailType : PhoneType)}." });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] VerificationRequest request)
        {
            try
            {
                bool isValid = false;
                ProfileSetting profileSetting = new ProfileSetting(_connStr);
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
                    userData.Add("email", request.Value);
                }
                else if (request.Type == PhoneType || request.Type == CateringNumberType)
                {
                    isValid = _smsService.VerifyOtp(request.Value, request.Otp);
                    userData.Add("phone", request.Value);
                }

                if (!isValid)
                {
                    return BadRequest(new { result = false, message = "Invalid or expired OTP." });
                }

                if (request.pkID > 0)
                {
                    profileSetting.UpdateUserDetails(request.pkID, userData);
                }

                return Ok(new { result = true, otp = request.Otp, message = "OTP verified successfully." });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }

    public class VerificationRequest
    {
        public string? Type { get; set; }
        public string? Value { get; set; }
        public string? Otp { get; set; }
        public long? pkID { get; set; }
        public string? Role { get; set; }

    }
}
