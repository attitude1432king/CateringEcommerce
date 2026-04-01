using CateringEcommerce.Domain.Interfaces.User;
using CateringEcommerce.Domain.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CateringEcommerce.API.Controllers.User
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class ContactController : ControllerBase
    {
        private readonly IContactRepository _contact;

        public ContactController(IContactRepository contact)
        {
            _contact = contact;
        }

        /// <summary>POST /api/contact — Save a user contact message.</summary>
        [HttpPost]
        public IActionResult Submit([FromBody] ContactMessageRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { result = false, message = "Please fill in all required fields correctly." });

            string? ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            bool saved = _contact.SaveMessage(request, ip);

            if (!saved)
                return StatusCode(500, new { result = false, message = "Failed to save your message. Please try again." });

            return Ok(new { result = true, message = "Your message has been received. We will get back to you shortly." });
        }
    }
}
