using System.Security.Claims;
using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.AspNetCore.Mvc;

namespace CateringEcommerce.API.Controllers.Admin
{
    [Route("api/logs")]
    [ApiController]
    [AdminAuthorize]
    public class LogsController : ControllerBase
    {
        private readonly IErrorLogRepository _errorLogRepository;
        private readonly IRBACRepository _rbacRepository;

        public LogsController(
            IErrorLogRepository errorLogRepository,
            IRBACRepository rbacRepository)
        {
            _errorLogRepository = errorLogRepository ?? throw new ArgumentNullException(nameof(errorLogRepository));
            _rbacRepository = rbacRepository ?? throw new ArgumentNullException(nameof(rbacRepository));
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs([FromQuery] ErrorLogListRequest request)
        {
            var adminId = GetCurrentAdminId();
            if (!await _rbacRepository.IsSuperAdminAsync(adminId))
            {
                return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can view system logs."));
            }

            var result = await _errorLogRepository.GetLogsAsync(request);
            return ApiResponseHelper.Success(result, "Logs retrieved successfully.");
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetLogById(long id)
        {
            var adminId = GetCurrentAdminId();
            if (!await _rbacRepository.IsSuperAdminAsync(adminId))
            {
                return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can view system logs."));
            }

            var log = await _errorLogRepository.GetByIdAsync(id);
            if (log == null)
            {
                return NotFound(new
                {
                    result = false,
                    success = false,
                    message = "Log entry not found."
                });
            }

            return ApiResponseHelper.Success(log, "Log details retrieved successfully.");
        }

        [HttpDelete]
        public async Task<IActionResult> Cleanup([FromQuery] DateTime beforeDate)
        {
            var adminId = GetCurrentAdminId();
            if (!await _rbacRepository.IsSuperAdminAsync(adminId))
            {
                return StatusCode(403, ApiResponseHelper.Failure("Only Super Admins can delete system logs."));
            }

            if (beforeDate == default)
            {
                return BadRequest(new
                {
                    result = false,
                    success = false,
                    message = "beforeDate is required."
                });
            }

            var deletedCount = await _errorLogRepository.DeleteBeforeAsync(beforeDate);
            return ApiResponseHelper.Success(new { deletedCount }, "Log cleanup completed successfully.");
        }

        private long GetCurrentAdminId()
        {
            var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (adminIdClaim == null || !long.TryParse(adminIdClaim.Value, out var adminId))
            {
                throw new UnauthorizedAccessException("Invalid admin session.");
            }

            return adminId;
        }
    }
}
