using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Common.Admin;
using Microsoft.AspNetCore.Mvc;

namespace CateringEcommerce.API.Controllers.Admin
{
    [Route("api/admin/dashboard")]
    [ApiController]
    [AdminAuthorize]
    public class AdminDashboardController : ControllerBase
    {
        private readonly string _connStr;

        public AdminDashboardController(IConfiguration config)
        {
            _connStr = config.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
        }

        /// <summary>
        /// Get complete dashboard metrics including stats, charts, and recent data
        /// </summary>
        [HttpGet("metrics")]
        public IActionResult GetDashboardMetrics()
        {
            try
            {
                var repository = new AdminDashboardRepository(_connStr);
                var metrics = repository.GetDashboardMetrics();
                return ApiResponseHelper.Success(metrics, "Dashboard metrics retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }
    }
}
