using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.User;
using CateringEcommerce.Domain.Interfaces.User;
using CateringEcommerce.Domain.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CateringEcommerce.API.Controllers.User
{
    [ApiController]
    [Route("api/User/Home")]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string _connStr;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connStr = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
        }

        /// <summary>
        /// Gets verified catering businesses.
        /// If cityId is less than or equal to 0, returns all verified catering businesses.
        /// Otherwise, returns catering businesses for the specified city.
        /// </summary>
        /// <param name="cityName">The city Name to filter by (optional). Use 0 or negative to get all.</param>
        /// <returns>List of verified catering businesses</returns>
        [AllowAnonymous]
        [HttpGet("CateringList")]
        public async Task<IActionResult> GetVerifiedCateringListAsync([FromQuery] string cityName)
        {
            try
            {
                HomeService  homeService = new HomeService(_connStr);
                _logger.LogInformation("Request received to fetch verified catering list. City Name: {0}", cityName);

                var cateringList = await homeService.GetVerifiedCateringListAsync(cityName);

                _logger.LogInformation("Successfully retrieved {Count} catering businesses.", cateringList.Count);

                return Ok(new
                {
                    success = true,
                    message = string.IsNullOrEmpty(cityName)
                        ? "All verified catering businesses retrieved successfully." 
                        : $"Verified catering businesses for {cityName} city retrieved successfully.",
                    data = cateringList,
                    count = cateringList.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred. City Name: {cityName}", cityName);
                return StatusCode(500, ApiResponseHelper.Failure("An unexpected error occurred."));
            }
        }
    }
}