using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.Admin;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.AspNetCore.Mvc;

namespace CateringEcommerce.API.Controllers.Admin
{
    [Route("api/admin/earnings")]
    [ApiController]
    [AdminAuthorize]
    public class AdminEarningsController : ControllerBase
    {
        private readonly IAdminEarningsRepository _earningsRepository;
        private readonly ILogger<AdminEarningsController> _logger;

        public AdminEarningsController(
            IAdminEarningsRepository earningsRepository,
            ILogger<AdminEarningsController> logger)
        {
            _earningsRepository = earningsRepository ?? throw new ArgumentNullException(nameof(earningsRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get earnings summary
        /// </summary>
        [HttpGet("summary")]
        public IActionResult GetEarningsSummary()
        {
            try
            {
                var summary = _earningsRepository.GetEarningsSummary();
                return ApiResponseHelper.Success(summary, "Earnings summary retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get earnings data");
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get earnings by date range (grouped by day/week/month/year)
        /// </summary>
        [HttpGet("by-date")]
        public IActionResult GetEarningsByDate([FromQuery] AdminEarningsByDateRequest request)
        {
            try
            {
                var earnings = _earningsRepository.GetEarningsByDate(request);
                return ApiResponseHelper.Success(earnings, "Earnings by date retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get earnings data");
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get earnings by catering with pagination
        /// </summary>
        [HttpGet("by-catering")]
        public IActionResult GetEarningsByCatering([FromQuery] AdminEarningsByCateringRequest request)
        {
            try
            {
                var result = _earningsRepository.GetEarningsByCatering(request);
                return ApiResponseHelper.Success(result, "Earnings by catering retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get earnings data");
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get monthly report for a specific year
        /// </summary>
        [HttpGet("monthly-report")]
        public IActionResult GetMonthlyReport([FromQuery] int year)
        {
            try
            {
                if (year == 0)
                    year = DateTime.Now.Year;

                var report = _earningsRepository.GetMonthlyReport(year);
                return ApiResponseHelper.Success(report, "Monthly report retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get earnings data");
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }
    }
}
