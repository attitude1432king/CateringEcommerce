using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CateringEcommerce.API.Controllers.Admin
{
    [Route("api/admin/search")]
    [ApiController]
    [AdminAuthorize]
    public class AdminSearchController : ControllerBase
    {
        private readonly IAdminSearchRepository _searchRepo;

        public AdminSearchController(IAdminSearchRepository searchRepo)
        {
            _searchRepo = searchRepo ?? throw new ArgumentNullException(nameof(searchRepo));
        }

        /// <summary>
        /// Global admin search across all permitted modules.
        /// Permission filtering is handled inside the repository via RBAC.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GlobalSearch([FromQuery] string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
                {
                    return Ok(ApiResponseHelper.Success(
                        new GlobalSearchResponse { Query = q?.Trim() ?? string.Empty },
                        "Query too short. Minimum 2 characters required."));
                }

                var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (adminIdClaim == null || !long.TryParse(adminIdClaim.Value, out long adminId))
                {
                    return Unauthorized(ApiResponseHelper.Failure("Invalid admin session."));
                }

                var request = new GlobalSearchRequest
                {
                    Query = q.Trim(),
                    MaxResultsPerModule = 5
                };

                var result = await _searchRepo.GlobalSearchAsync(request, adminId);
                return Ok(ApiResponseHelper.Success(result, "Search completed."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }
    }
}
