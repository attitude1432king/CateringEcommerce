using CateringEcommerce.API.Controllers.Owner.Menu;
using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.Owner;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Ocsp;

namespace CateringEcommerce.API.Controllers.Owner
{
    [Route("api/Owner/Availability")]
    [ApiController]
    [OwnerAuthorize]
    public class AvailabilityController : ControllerBase
    {
        private readonly ILogger<AvailabilityController> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly IAvailabilityRepository _availabilityRepository;

        public AvailabilityController(
            ILogger<AvailabilityController> logger,
            ICurrentUserService currentUser,
            IAvailabilityRepository availabilityRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _availabilityRepository = availabilityRepository ?? throw new ArgumentNullException(nameof(availabilityRepository));
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
                var data = await _availabilityRepository.GetAvailabilityForPageAsync(ownerId, year, month);
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
                await _availabilityRepository.UpsertGlobalAsync(ownerId, status);
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
                await _availabilityRepository.UpsertDateAsync(ownerId, req.Date, req.Status, req.Note);
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
