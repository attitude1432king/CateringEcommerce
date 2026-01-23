using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.Owner;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CateringEcommerce.API.Controllers.Owner
{
    [Authorize]
    [ApiController]
    [Route("api/Owner/[controller]")]
    public class OrderModificationsController : ControllerBase
    {
        private readonly ILogger<OrderModificationsController> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly IConfiguration _configuration;
        private readonly string _connStr;

        public OrderModificationsController(
            ILogger<OrderModificationsController> logger,
            ICurrentUserService currentUser,
            IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _connStr = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
        }

        // ===================================
        // POST: api/Owner/OrderModifications/Create
        // Create a new modification request (Owner)
        // ===================================
        [HttpPost("Create")]
        public async Task<IActionResult> CreateModification([FromBody] CreateOrderModificationDto modificationData)
        {
            try
            {
                // Validate model
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Get authenticated owner ID
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Owner not authenticated.");
                }

                // Validate modification data
                if (modificationData == null)
                {
                    return ApiResponseHelper.Failure("Invalid modification data.");
                }

                // Set the requested by to current owner
                modificationData.RequestedBy = ownerId;

                _logger.LogInformation($"Creating order modification for OrderId: {modificationData.OrderId} by Owner: {ownerId}");

                // Create service
                OrderModificationService modificationService = new OrderModificationService(_connStr);

                // Create modification
                OrderModificationDto modification = await modificationService.CreateModificationAsync(modificationData);

                _logger.LogInformation($"Order modification created successfully: {modification.ModificationId}");

                return ApiResponseHelper.Success(modification, "Modification request sent to customer for approval!");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Modification creation failed: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message, "warning");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Modification validation failed: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message, "warning");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating modification");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while creating the modification. Please try again."));
            }
        }

        // ===================================
        // GET: api/Owner/OrderModifications/{orderId}
        // Get all modifications for an order
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

                _logger.LogInformation($"Fetching modifications for OrderId: {orderId}");

                // Create service
                OrderModificationService modificationService = new OrderModificationService(_connStr);

                // Get modifications
                OrderModificationsSummaryDto summary = await modificationService.GetOrderModificationsAsync(orderId);

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
    }
}
