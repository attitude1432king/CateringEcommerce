using CateringEcommerce.BAL.BAL.AuthLogic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CateringEcommerce.API.Controllers
{
    [Route("api/Auth")]
    [ApiController]
    public class AuthController : Controller
    {

        private readonly SmsService _smsService;

        public AuthController(SmsService smsService)
        {
            _smsService = smsService;
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return BadRequest("Phone number is required.");

            var otp = new Random().Next(100000, 999999).ToString();
            await _smsService.SendOtpAsync(phoneNumber, otp);

            // Store OTP securely (e.g., in a database or cache) for later verification
            // Ensure OTP expires after a certain period

            return Ok("OTP sent successfully.");
        }

        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] OtpVerificationRequest request)
        {
            // Retrieve the stored OTP for the phone number
            // Validate the OTP and its expiration

            if (IsValidOtp(request.PhoneNumber, request.Otp))
            {
                return Ok("OTP verified successfully.");
            }
            else
            {
                return Unauthorized("Invalid or expired OTP.");
            }
        }

        private bool IsValidOtp(string phoneNumber, string otp)
        {
            // Implement OTP validation logic
            return true;
        }
    }

    public class OtpVerificationRequest
    {
        public string PhoneNumber { get; set; }
        public string Otp { get; set; }
    }
}
