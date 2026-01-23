using CateringEcommerce.API.Controllers.Owner.Menu;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.Owner;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Ocsp;

namespace CateringEcommerce.API.Controllers.Owner
{
    [Route("api/Owner/Availability")]
    [ApiController]
    [Authorize(Roles = "Owner")]
    public class AvailabilityController : ControllerBase
    {
        private readonly string _connStr;
        private readonly ILogger<AvailabilityController> _logger;
        private readonly ICurrentUserService _currentUser;

        public AvailabilityController(ILogger<AvailabilityController> logger, IConfiguration configuration, ICurrentUserService currentUser)
        {
            _logger = logger;
            _connStr = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
            _currentUser = currentUser;
        }

        [HttpGet("GetAvailability")]
        public async Task<IActionResult> GetAvailabilityPageData([FromQuery] int year, [FromQuery] int month) 
        {
            var ownerId = _currentUser.UserId;
            if (ownerId <= 0)
                return ApiResponseHelper.Failure("Access denied.");

            _logger.LogInformation(
                "GetAvailabilityPageData started | OwnerId={OwnerId}",
                ownerId);

            try
            {
                AvailabilityRepository _services = new AvailabilityRepository(_connStr);
                var data = await _services.GetAvailabilityForPageAsync(ownerId, year, month);
                return ApiResponseHelper.Success(data, "Availability data loaded");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "GetAvailabilityPageData failed | OwnerId={OwnerId}",
                    ownerId);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.");
            }
        }


        [HttpPost("UpdateStatus")]
        public async Task<IActionResult> UpdateGlobalAvailability([FromBody] int status)
        {
            var ownerId = _currentUser.UserId;
            if (ownerId <= 0)
                return ApiResponseHelper.Failure("Access denied.");

            _logger.LogInformation(
                "UpdateGlobalAvailability started | OwnerId={OwnerId}",
                ownerId);
            try
            {
                AvailabilityRepository availability = new AvailabilityRepository(_connStr);
                await availability.UpsertGlobalAsync(ownerId, status);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "UpdateGlobalAvailability failed | OwnerId={OwnerId}",
                    ownerId);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.");
            }
        }

        [HttpPost("UpdateDateStatus")]
        public async Task<IActionResult> UpdateDateAvailability([FromBody] DateAvailabilityDTO req)
        {
            var ownerId = _currentUser.UserId;
            if (ownerId <= 0)
                return ApiResponseHelper.Failure("Access denied.");

            _logger.LogInformation(
                "UpdateDateAvailability started | OwnerId={OwnerId}",
                ownerId);
            try
            {
                AvailabilityRepository availability = new AvailabilityRepository(_connStr);
                await availability.UpsertDateAsync(ownerId, req.Date, req.Status, req.Note);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "UpdateDateAvailability failed | OwnerId={OwnerId}",
                    ownerId);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.");
            }
        }
    }
}
