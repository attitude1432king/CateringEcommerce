using System.Globalization;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CateringEcommerce.API.Controllers.User
{
    [ApiController]
    [Route("api/catering")]
    public class CateringController : ControllerBase
    {
        private readonly ILogger<CateringController> _logger;
        private readonly CateringAvailabilityService _availabilityService;

        public CateringController(
            ILogger<CateringController> logger,
            CateringAvailabilityService availabilityService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _availabilityService = availabilityService ?? throw new ArgumentNullException(nameof(availabilityService));
        }

        [AllowAnonymous]
        [HttpGet("{cateringId:long}/availability")]
        public async Task<IActionResult> GetAvailabilityAsync(long cateringId, [FromQuery] string date)
        {
            if (cateringId <= 0)
            {
                return BadRequest(new { message = "Invalid catering id." });
            }

            if (string.IsNullOrWhiteSpace(date) ||
                !DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var selectedDate))
            {
                return BadRequest(new { message = "Date must be provided in YYYY-MM-DD format." });
            }

            selectedDate = selectedDate.Date;
            var minSelectableDate = DateTime.Today.AddDays(_availabilityService.GetMinimumAdvanceBookingDays());
            if (selectedDate < minSelectableDate)
            {
                return BadRequest(new
                {
                    message = $"Date must be on or after {minSelectableDate:yyyy-MM-dd}."
                });
            }

            try
            {
                var availability = await _availabilityService.GetAvailabilityAsync(cateringId, selectedDate);
                if (availability == null)
                {
                    return NotFound(new { message = "Catering not found." });
                }

                return Ok(new
                {
                    success = true,
                    message = availability.Message,
                    data = availability
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch catering availability for cateringId={CateringId} date={Date}", cateringId, date);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        [AllowAnonymous]
        [HttpGet("{cateringId:long}/availability/calendar")]
        public async Task<IActionResult> GetAvailabilityCalendarAsync(long cateringId, [FromQuery] int year, [FromQuery] int month)
        {
            if (cateringId <= 0)
            {
                return BadRequest(new { message = "Invalid catering id." });
            }

            if (year < 2000 || month < 1 || month > 12)
            {
                return BadRequest(new { message = "Year and month are required." });
            }

            try
            {
                var blockedDates = await _availabilityService.GetBlockedDatesAsync(cateringId, year, month);
                if (blockedDates == null)
                {
                    return NotFound(new { message = "Catering not found." });
                }

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        blockedDates = blockedDates.Select(dateValue => dateValue.ToString("yyyy-MM-dd")).ToList()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch blocked dates for cateringId={CateringId} year={Year} month={Month}", cateringId, year, month);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }
    }
}
