using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Common.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.AspNetCore.Mvc;

namespace CateringEcommerce.API.Controllers.Admin
{
    [Route("api/admin/earnings")]
    [ApiController]
    [AdminAuthorize]
    public class AdminEarningsController : ControllerBase
    {
        private readonly string _connStr;

        public AdminEarningsController(IConfiguration config)
        {
            _connStr = config.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
        }

        /// <summary>
        /// Get earnings summary
        /// </summary>
        [HttpGet("summary")]
        public IActionResult GetEarningsSummary()
        {
            try
            {
                var repository = new AdminEarningsRepository(_connStr);
                var summary = repository.GetEarningsSummary();
                return ApiResponseHelper.Success(summary, "Earnings summary retrieved successfully.");
            }
            catch (Exception ex)
            {
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
                var repository = new AdminEarningsRepository(_connStr);
                var earnings = repository.GetEarningsByDate(request);
                return ApiResponseHelper.Success(earnings, "Earnings by date retrieved successfully.");
            }
            catch (Exception ex)
            {
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
                var repository = new AdminEarningsRepository(_connStr);
                var result = repository.GetEarningsByCatering(request);
                return ApiResponseHelper.Success(result, "Earnings by catering retrieved successfully.");
            }
            catch (Exception ex)
            {
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

                var repository = new AdminEarningsRepository(_connStr);
                var report = repository.GetMonthlyReport(year);
                return ApiResponseHelper.Success(report, "Monthly report retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }
    }
}
