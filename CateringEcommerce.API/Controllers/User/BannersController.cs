using CateringEcommerce.BAL.Base.Owner;
using Microsoft.AspNetCore.Mvc;

namespace CateringEcommerce.API.Controllers.User
{
    [ApiController]
    [Route("api/User/Banners")]
    public class UserBannersController : ControllerBase
    {
        private readonly string _connStr;
        private readonly ILogger<UserBannersController> _logger;

        public UserBannersController(
            ILogger<UserBannersController> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _connStr = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
        }

        [HttpGet("Active")]
        public async Task<IActionResult> GetActiveBanners()
        {
            try
            {
                _logger.LogInformation("Fetching active banners for homepage.");

                var bannerService = new BannerService(_connStr);
                var banners = await bannerService.GetActiveBannersForHomepage();

                _logger.LogInformation("Fetched {Count} active banners.", banners?.Count ?? 0);
                return Ok(banners);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching active banners.");
                return StatusCode(500, "An error occurred while fetching banners.");
            }
        }

        [HttpPost("TrackView")]
        public async Task<IActionResult> TrackBannerView([FromBody] long bannerId)
        {
            try
            {
                var bannerService = new BannerService(_connStr);
                await bannerService.IncrementViewCount(bannerId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while tracking banner view.");
                return StatusCode(500, "An error occurred.");
            }
        }

        [HttpPost("TrackClick")]
        public async Task<IActionResult> TrackBannerClick([FromBody] long bannerId)
        {
            try
            {
                var bannerService = new BannerService(_connStr);
                await bannerService.IncrementClickCount(bannerId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while tracking banner click.");
                return StatusCode(500, "An error occurred.");
            }
        }
    }
}
