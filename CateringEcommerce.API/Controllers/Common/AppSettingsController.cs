using CateringEcommerce.API.Filters;
using CateringEcommerce.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CateringEcommerce.API.Controllers.Common
{
    [ApiController]
    [Route("api/app-settings")]
    public class AppSettingsController : ControllerBase
    {
        private readonly ISystemSettingsProvider _settingsProvider;
        private readonly IPublicStatsRepository _publicStats;

        public AppSettingsController(
            ISystemSettingsProvider settingsProvider,
            IPublicStatsRepository publicStats)
        {
            _settingsProvider = settingsProvider;
            _publicStats = publicStats;
        }

        [HttpGet]
        public IActionResult GetPublicSettings()
        {
            try
            {
                var settings = _settingsProvider.GetPublicSettings();
                return Ok(new { result = true, data = settings });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { result = false, message = "Failed to load settings" });
            }
        }

        /// <summary>
        /// Public endpoint — returns live platform stats for the Partner Login page.
        /// Results are cached server-side for 1 hour (no auth required).
        /// </summary>
        [HttpGet("partner-stats")]
        public async Task<IActionResult> GetPartnerStats()
        {
            try
            {
                var stats = await _publicStats.GetPartnerStatsAsync();
                return Ok(new { result = true, data = stats });
            }
            catch (Exception)
            {
                return StatusCode(500, new { result = false, message = "Failed to load partner stats" });
            }
        }

        [HttpPost("refresh")]
        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        public async Task<IActionResult> RefreshSettings()
        {
            try
            {
                await _settingsProvider.RefreshAsync();
                return Ok(new { result = true, message = "Settings refreshed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { result = false, message = "Failed to refresh settings" });
            }
        }
    }
}
