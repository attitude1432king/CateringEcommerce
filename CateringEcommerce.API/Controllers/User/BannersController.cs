using CateringEcommerce.Domain.Interfaces.Owner;
using Microsoft.AspNetCore.Mvc;

namespace CateringEcommerce.API.Controllers.User
{
    [ApiController]
    [Route("api/User/Banners")]
    public class UserBannersController : ControllerBase
    {
        private readonly ILogger<UserBannersController> _logger;
        private readonly IBannerService _bannerService;

        public UserBannersController(
            ILogger<UserBannersController> logger,
            IBannerService bannerService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _bannerService = bannerService ?? throw new ArgumentNullException(nameof(bannerService));
        }

        [HttpGet("Active")]
        public async Task<IActionResult> GetActiveBanners()
        {
            try
            {
                _logger.LogInformation("Fetching active banners for homepage.");

                var banners = await _bannerService.GetActiveBannersForHomepage();

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
                await _bannerService.IncrementViewCount(bannerId);
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
                await _bannerService.IncrementClickCount(bannerId);
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
