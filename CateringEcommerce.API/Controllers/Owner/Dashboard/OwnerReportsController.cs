using CateringEcommerce.API.Helpers;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;
using CateringEcommerce.API.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CateringEcommerce.API.Controllers.Owner.Dashboard
{
    /// <summary>
    /// Owner Reports Controller
    /// Provides comprehensive reporting and analytics for partner owners
    /// </summary>
    [OwnerAuthorize]
    [ApiController]
    [Route("api/Owner/[controller]")]
    public class OwnerReportsController : ControllerBase
    {
        private readonly ILogger<OwnerReportsController> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly IOwnerReportsRepository _reportsRepository;

        public OwnerReportsController(
            ILogger<OwnerReportsController> logger,
            ICurrentUserService currentUser,
            IOwnerReportsRepository reportsRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _reportsRepository = reportsRepository ?? throw new ArgumentNullException(nameof(reportsRepository));
        }

        /// <summary>
        /// Generate sales report
        /// </summary>
        /// <param name="filter">Report filter parameters</param>
        /// <returns>Sales report</returns>
        [HttpPost("sales")]
        public async Task<IActionResult> GenerateSalesReport([FromBody] ReportFilterDto filter)
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Owner not authenticated.");
                }

                _logger.LogInformation($"Generating sales report for owner {ownerId}");

                var report = await _reportsRepository.GenerateSalesReport(ownerId, filter);

                return ApiResponseHelper.Success(report, "Sales report generated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating sales report");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while generating sales report."));
            }
        }

        /// <summary>
        /// Generate revenue report
        /// </summary>
        /// <param name="filter">Report filter parameters</param>
        /// <returns>Revenue report</returns>
        [HttpPost("revenue")]
        public async Task<IActionResult> GenerateRevenueReport([FromBody] ReportFilterDto filter)
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Owner not authenticated.");
                }

                _logger.LogInformation($"Generating revenue report for owner {ownerId}");

                var report = await _reportsRepository.GenerateRevenueReport(ownerId, filter);

                return ApiResponseHelper.Success(report, "Revenue report generated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating revenue report");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while generating revenue report."));
            }
        }

        /// <summary>
        /// Generate customer report
        /// </summary>
        /// <param name="filter">Report filter parameters</param>
        /// <returns>Customer report</returns>
        [HttpPost("customers")]
        public async Task<IActionResult> GenerateCustomerReport([FromBody] ReportFilterDto filter)
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Owner not authenticated.");
                }

                _logger.LogInformation($"Generating customer report for owner {ownerId}");

                var report = await _reportsRepository.GenerateCustomerReport(ownerId, filter);

                return ApiResponseHelper.Success(report, "Customer report generated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating customer report");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while generating customer report."));
            }
        }

        /// <summary>
        /// Generate menu performance report
        /// </summary>
        /// <param name="filter">Report filter parameters</param>
        /// <returns>Menu performance report</returns>
        [HttpPost("menu-performance")]
        public async Task<IActionResult> GenerateMenuPerformanceReport([FromBody] ReportFilterDto filter)
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Owner not authenticated.");
                }

                _logger.LogInformation($"Generating menu performance report for owner {ownerId}");

                var report = await _reportsRepository.GenerateMenuPerformanceReport(ownerId, filter);

                return ApiResponseHelper.Success(report, "Menu performance report generated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating menu performance report");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while generating menu performance report."));
            }
        }

        /// <summary>
        /// Generate financial report
        /// </summary>
        /// <param name="filter">Report filter parameters</param>
        /// <returns>Financial report</returns>
        [HttpPost("financial")]
        public async Task<IActionResult> GenerateFinancialReport([FromBody] ReportFilterDto filter)
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Owner not authenticated.");
                }

                _logger.LogInformation($"Generating financial report for owner {ownerId}");

                var report = await _reportsRepository.GenerateFinancialReport(ownerId, filter);

                return ApiResponseHelper.Success(report, "Financial report generated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating financial report");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while generating financial report."));
            }
        }

        /// <summary>
        /// Export report to CSV or PDF
        /// </summary>
        /// <param name="type">Report type (sales, revenue, customer, menu)</param>
        /// <param name="format">Export format (csv or pdf)</param>
        /// <param name="filter">Report filter parameters</param>
        /// <returns>File download</returns>
        [HttpPost("export")]
        public async Task<IActionResult> ExportReport(
            [FromQuery] string type,
            [FromQuery] string format = "csv",
            [FromBody] ReportFilterDto filter = null)
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Owner not authenticated.");
                }

                _logger.LogInformation($"Exporting {type} report for owner {ownerId} in {format} format");

                filter ??= new ReportFilterDto();

                byte[] fileContent;
                string contentType;
                string fileName;

                if (format.ToLower() == "pdf")
                {
                    fileContent = await _reportsRepository.ExportReportToPDF(ownerId, type, filter);
                    contentType = "application/pdf";
                    fileName = $"{type}_report_{DateTime.Now:yyyyMMdd}.pdf";
                }
                else
                {
                    fileContent = await _reportsRepository.ExportReportToCSV(ownerId, type, filter);
                    contentType = "text/csv";
                    fileName = $"{type}_report_{DateTime.Now:yyyyMMdd}.csv";
                }

                return File(fileContent, contentType, fileName);
            }
            catch (NotImplementedException ex)
            {
                _logger.LogWarning($"Export format not implemented: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while exporting report."));
            }
        }
    }
}
