using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.User;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CateringEcommerce.API.Controllers.User
{
    [Authorize]
    [ApiController]
    [Route("api/User/[controller]")]
    public class UserAddressesController : ControllerBase
    {
        private readonly ILogger<UserAddressesController> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly UserAddressService _userAddressService;

        public UserAddressesController(
            ILogger<UserAddressesController> logger,
            ICurrentUserService currentUser,
            UserAddressService userAddressService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _userAddressService = userAddressService ?? throw new ArgumentNullException(nameof(userAddressService));
        }

        // ===================================
        // GET: api/User/UserAddresses
        // Get all saved addresses for the authenticated user
        // ===================================
        [HttpGet]
        public async Task<IActionResult> GetAddresses()
        {
            try
            {
                // Get authenticated user ID
                long userId = _currentUser.UserId;
                if (userId <= 0)
                {
                    return ApiResponseHelper.Failure("User not authenticated.");
                }

                _logger.LogInformation($"Fetching addresses for user {userId}");

                // Create service
                var addressService = _userAddressService;

                // Get addresses
                List<SavedAddressDto> addresses = await addressService.GetUserAddressesAsync(userId);

                _logger.LogInformation($"Retrieved {addresses.Count} addresses for user {userId}");

                return ApiResponseHelper.Success(addresses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user addresses");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching your addresses. Please try again."));
            }
        }

        // ===================================
        // GET: api/User/UserAddresses/{id}
        // Get a specific address by ID
        // ===================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAddressById(long id)
        {
            try
            {
                // Get authenticated user ID
                long userId = _currentUser.UserId;
                if (userId <= 0)
                {
                    return ApiResponseHelper.Failure("User not authenticated.");
                }

                _logger.LogInformation($"Fetching address {id} for user {userId}");

                // Create service
                var addressService = _userAddressService;

                // Get address
                SavedAddressDto address = await addressService.GetAddressByIdAsync(id, userId);

                return ApiResponseHelper.Success(address);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Address fetch failed: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message, "warning");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching address {id}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching the address. Please try again."));
            }
        }

        // ===================================
        // POST: api/User/UserAddresses/Create
        // Create a new saved address
        // ===================================
        [HttpPost("Create")]
        public async Task<IActionResult> CreateAddress([FromBody] CreateAddressDto addressData)
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

                // Validate address data
                if (addressData == null)
                {
                    return ApiResponseHelper.Failure("Invalid address data.");
                }

                _logger.LogInformation($"Creating address for user {userId}");

                // Create service
                var addressService = _userAddressService;

                // Create address
                SavedAddressDto address = await addressService.CreateAddressAsync(userId, addressData);

                _logger.LogInformation($"Address created successfully: {address.AddressId}");

                return ApiResponseHelper.Success(address, "Address saved successfully!");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Address creation failed: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message, "warning");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Address creation validation failed: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message, "warning");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating address");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while saving your address. Please try again."));
            }
        }

        // ===================================
        // PUT: api/User/UserAddresses/{id}
        // Update an existing address
        // ===================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAddress(long id, [FromBody] UpdateAddressDto addressData)
        {
            try
            {
                // Validate model
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate ID match
                if (id != addressData.AddressId)
                {
                    return ApiResponseHelper.Failure("Address ID mismatch.");
                }

                // Get authenticated user ID
                long userId = _currentUser.UserId;
                if (userId <= 0)
                {
                    return ApiResponseHelper.Failure("User not authenticated.");
                }

                // Validate address data
                if (addressData == null)
                {
                    return ApiResponseHelper.Failure("Invalid address data.");
                }

                _logger.LogInformation($"Updating address {id} for user {userId}");

                // Create service
                var addressService = _userAddressService;

                // Update address
                SavedAddressDto address = await addressService.UpdateAddressAsync(userId, addressData);

                _logger.LogInformation($"Address updated successfully: {address.AddressId}");

                return ApiResponseHelper.Success(address, "Address updated successfully!");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Address update failed: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message, "warning");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Address update validation failed: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message, "warning");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating address {id}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while updating your address. Please try again."));
            }
        }

        // ===================================
        // DELETE: api/User/UserAddresses/{id}
        // Delete an address (soft delete)
        // ===================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(long id)
        {
            try
            {
                // Get authenticated user ID
                long userId = _currentUser.UserId;
                if (userId <= 0)
                {
                    return ApiResponseHelper.Failure("User not authenticated.");
                }

                _logger.LogInformation($"Deleting address {id} for user {userId}");

                // Create service
                var addressService = _userAddressService;

                // Delete address
                bool deleted = await addressService.DeleteAddressAsync(id, userId);

                _logger.LogInformation($"Address deleted successfully: {id}");

                return ApiResponseHelper.Success(new { deleted = true }, "Address deleted successfully!");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Address deletion failed: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message, "warning");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Address deletion validation failed: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message, "warning");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting address {id}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while deleting your address. Please try again."));
            }
        }

        // ===================================
        // POST: api/User/UserAddresses/{id}/SetDefault
        // Set an address as default
        // ===================================
        [HttpPost("{id}/SetDefault")]
        public async Task<IActionResult> SetDefaultAddress(long id)
        {
            try
            {
                // Get authenticated user ID
                long userId = _currentUser.UserId;
                if (userId <= 0)
                {
                    return ApiResponseHelper.Failure("User not authenticated.");
                }

                _logger.LogInformation($"Setting address {id} as default for user {userId}");

                // Create service
                var addressService = _userAddressService;

                // Set default address
                bool updated = await addressService.SetDefaultAddressAsync(id, userId);

                _logger.LogInformation($"Address set as default successfully: {id}");

                return ApiResponseHelper.Success(new { updated = true }, "Default address updated successfully!");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Set default address failed: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message, "warning");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Set default address validation failed: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message, "warning");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting default address {id}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while updating your default address. Please try again."));
            }
        }
    }
}
