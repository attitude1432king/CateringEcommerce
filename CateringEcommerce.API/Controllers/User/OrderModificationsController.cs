using CateringEcommerce.API.Helpers;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CateringEcommerce.API.Controllers.User
{
    [Authorize]
    [ApiController]
    [Route("api/User/[controller]")]
    public class OrderModificationsController : ControllerBase
    {
        private readonly ILogger<OrderModificationsController> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly IOrderModificationService _modificationService;

        public OrderModificationsController(
            ILogger<OrderModificationsController> logger,
            ICurrentUserService currentUser,
            IOrderModificationService modificationService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _modificationService = modificationService ?? throw new ArgumentNullException(nameof(modificationService));
        }

        // ===================================
        // GET: api/User/OrderModifications/{orderId}
        // Get all modifications for an order (User view)
        // ===================================
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderModifications(long orderId)
        {
            try
            {
                // Get authenticated user ID
                long userId = _currentUser.UserId;
                if (userId <= 0)
                {
                    return ApiResponseHelper.Failure("User not authenticated.");
                }

                if (orderId <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid order ID.");
                }

                _logger.LogInformation($"User {userId} fetching modifications for OrderId: {orderId}");

                // Get modifications
                OrderModificationsSummaryDto summary = await _modificationService.GetOrderModificationsAsync(orderId);

                return ApiResponseHelper.Success(summary);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Get modifications validation failed: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message, "warning");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching modifications for OrderId: {orderId}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching modifications. Please try again."));
            }
        }

        // ===================================
        // POST: api/User/OrderModifications/{id}/Approve
        // Approve a modification request (User)
        // ===================================
        [HttpPost("{id}/Approve")]
        public async Task<IActionResult> ApproveModification(long id, [FromBody] ApprovalNotesDto? approvalNotes)
        {
            try
            {
                // Get authenticated user ID
                long userId = _currentUser.UserId;
                if (userId <= 0)
                {
                    return ApiResponseHelper.Failure("User not authenticated.");
                }

                if (id <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid modification ID.");
                }

                _logger.LogInformation($"User {userId} approving modification {id}");

                // Create approval data
                ApproveOrderModificationDto approvalData = new ApproveOrderModificationDto
                {
                    ModificationId = id,
                    UserId = userId,
                    ApprovalNotes = approvalNotes?.Notes
                };

                // Approve modification
                OrderModificationDto modification = await _modificationService.ApproveModificationAsync(approvalData);

                _logger.LogInformation($"Modification {id} approved successfully");

                return ApiResponseHelper.Success(modification, "Modification approved! The additional amount will be added to your final payment.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Modification approval failed: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message, "warning");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Modification approval validation failed: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message, "warning");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error approving modification {id}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while approving the modification. Please try again."));
            }
        }

        // ===================================
        // POST: api/User/OrderModifications/{id}/Reject
        // Reject a modification request (User)
        // ===================================
        [HttpPost("{id}/Reject")]
        public async Task<IActionResult> RejectModification(long id, [FromBody] RejectOrderModificationDto rejectionData)
        {
            try
            {
                // Validate model
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Get authenticated user ID
                long userId = _currentUser.UserId;
                if (userId <= 0)
                {
                    return ApiResponseHelper.Failure("User not authenticated.");
                }

                if (id <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid modification ID.");
                }

                if (rejectionData == null)
                {
                    return ApiResponseHelper.Failure("Invalid rejection data.");
                }

                // Ensure ID matches
                if (rejectionData.ModificationId != id)
                {
                    return ApiResponseHelper.Failure("Modification ID mismatch.");
                }

                // Set user ID
                rejectionData.UserId = userId;

                _logger.LogInformation($"User {userId} rejecting modification {id}");

                // Reject modification
                OrderModificationDto modification = await _modificationService.RejectModificationAsync(rejectionData);

                _logger.LogInformation($"Modification {id} rejected successfully");

                return ApiResponseHelper.Success(modification, "Modification rejected successfully.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Modification rejection failed: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message, "warning");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Modification rejection validation failed: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message, "warning");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error rejecting modification {id}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while rejecting the modification. Please try again."));
            }
        }
    }

    // ===================================
    // APPROVAL NOTES DTO
    // ===================================
    public class ApprovalNotesDto
    {
        public string? Notes { get; set; }
    }
}
