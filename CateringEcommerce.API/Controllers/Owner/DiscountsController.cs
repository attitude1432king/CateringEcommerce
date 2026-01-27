using CateringEcommerce.API.Attributes;
using CateringEcommerce.API.Controllers.Owner.Menu;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.Owner;
using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CateringEcommerce.API.Controllers.Owner
{
    [Route("api/Owner/Discounts")]
    [ApiController]
    [Authorize(Roles = "Owner")]
    public class DiscountsController : ControllerBase
    {
        private readonly string _connStr;
        private readonly ILogger<PackagesController> _logger;
        private readonly ICurrentUserService _currentUser;

        public DiscountsController(ILogger<PackagesController> logger, IConfiguration configuration, ICurrentUserService currentUser)
        {
            _logger = logger;
            _connStr = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
            _currentUser = currentUser;
        }

        [HttpGet("Count")]
        public async Task<IActionResult> GetDiscountCount([FromQuery] string? filterJson)
        {
            var ownerId = _currentUser.UserId;
            if (ownerId <= 0)
                return ApiResponseHelper.Failure("Access denied.");

            _logger.LogInformation(
                "GetDiscountCount started | OwnerId={OwnerId}",
                ownerId);

            try
            {
                var discountService = new Discounts(_connStr);
                var count = await discountService.GetDiscountsCountAsync(ownerId, filterJson);

                _logger.LogInformation(
                    "GetDiscountCount completed | OwnerId={OwnerId} | Count={Count}",
                    ownerId, count);

                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "GetDiscountCount failed | OwnerId={OwnerId}",
                    ownerId);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.");
            }
        }


        [HttpGet("Data")]
        public async Task<IActionResult> GetDiscountListAsync([FromQuery] int page, [FromQuery] int pageSize, [FromQuery] string? filterJson)
        {
            var ownerId = _currentUser.UserId;
            if (ownerId <= 0)
                return ApiResponseHelper.Failure("Access denied.");

            if (page <= 0 || pageSize <= 0)
                return ApiResponseHelper.Failure("Invalid paging parameters.");

            _logger.LogInformation(
                "GetDiscountList started | OwnerId={OwnerId} | Page={Page} | PageSize={PageSize}",
                ownerId, page, pageSize);

            try
            {
                var discountService = new Discounts(_connStr);
                var discounts = await discountService.GetDiscountListAsync(
                    ownerId, page, pageSize, filterJson);

                _logger.LogInformation(
                    "GetDiscountList completed | OwnerId={OwnerId} | Count={Count}",
                    ownerId, discounts?.Count ?? 0);

                return Ok(discounts);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "GetDiscountList failed | OwnerId={OwnerId} | Page={Page}",
                    ownerId, page);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.");
            }
        }


        [ValidateModel]
        [HttpPost("Create")]
        public async Task<IActionResult> CreateDiscountAsync([FromBody] DiscountDto discountDto)
        {
            if (discountDto == null)
                return ApiResponseHelper.Failure("Invalid request payload.");

            var ownerId = _currentUser.UserId;
            if (ownerId <= 0)
                return ApiResponseHelper.Failure("Access denied.");

            _logger.LogInformation(
                "CreateDiscount started | OwnerId={OwnerId} | DiscountName={DiscountName}",
                ownerId, discountDto.Name);

            try
            {
                var discounts = new Discounts(_connStr);

                // 1️⃣ Validate unique discount name
                if (await discounts.IsDiscountNameExists(ownerId, discountDto.Name))
                {
                    return ApiResponseHelper.Failure(
                        "Discount name already exists. Please choose a different name.",
                        "warning");
                }

                // 2. Validate item price is low then original price
                if (discountDto.Mode == DiscountMode.Flat.GetHashCode())
                {
                    bool isValidNotItem = await CheckPriceHigherThanSelectedItems(discountDto, ownerId);
                    if (isValidNotItem)
                    {
                        return ApiResponseHelper.Failure("selected items price cannot exceed the original item price.", "warning");
                    }
                }

                // 3. Create discount
                var discountId = await discounts.AddDiscountAsync(ownerId, discountDto);
                if (discountId <= 0)
                    return ApiResponseHelper.Failure("Failed to create discount.");

                // 4. Sync mappings (if any)
                await SyncDiscountMappingsAsync(discountDto, discountId);

                _logger.LogInformation(
                    "CreateDiscount completed | DiscountId={DiscountId} | OwnerId={OwnerId}",
                    discountId, ownerId);

                return ApiResponseHelper.Success(
                    discountId,
                    $"{discountDto.Name} discount added successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "CreateDiscount failed | OwnerId={OwnerId} | DiscountName={DiscountName}",
                    ownerId, discountDto.Name);

                return ApiResponseHelper.Failure("An unexpected error occurred.");
            }
        }


        [ValidateModel]
        [HttpPost("Update")]
        public async Task<IActionResult> UpdateDiscountAsync([FromBody] DiscountDto discountDto)
        {
            if (discountDto == null)
                return ApiResponseHelper.Failure("Invalid request payload.");

            var ownerId = _currentUser.UserId;
            if (ownerId <= 0)
                return ApiResponseHelper.Failure("Access denied.");

            if (!discountDto.ID.HasValue || discountDto.ID <= 0)
                return ApiResponseHelper.Failure("Invalid discount ID.");

            _logger.LogInformation(
                "UpdateDiscount started | DiscountId={DiscountId} | OwnerId={OwnerId}",
                discountDto.ID, ownerId);

            try
            {
                var discounts = new Discounts(_connStr);

                // 1️. Validate discount ownership
                if (!await discounts.IsValidDiscountId(ownerId, discountDto.ID.Value))
                    return ApiResponseHelper.Failure("Discount not found or access denied.");

                // 2️. Validate unique name
                if (await discounts.IsDiscountNameExists(ownerId, discountDto.Name, discountDto.ID))
                    return ApiResponseHelper.Failure(
                        "Discount name already exists. Please choose a different name.",
                        "warning");


                // 3. Validate item price is low then original price
                if (discountDto.Mode == DiscountMode.Flat.GetHashCode())
                {
                    bool isValidNotItem = await CheckPriceHigherThanSelectedItems(discountDto, ownerId);
                    if (isValidNotItem)
                    {
                        return ApiResponseHelper.Failure("selected items price cannot exceed the original item price.", "warning");
                    }
                }

                // 4. Update discount
                var updateResult = await discounts.UpdateDiscountAsync(ownerId, discountDto);

                // 5. Sync mappings (only if needed)
                await SyncDiscountMappingsAsync(discountDto, discountDto.ID.Value);

                _logger.LogInformation(
                    "UpdateDiscount completed | DiscountId={DiscountId}",
                    discountDto.ID);

                return ApiResponseHelper.Success(
                    updateResult,
                    $"{discountDto.Name} discount updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "UpdateDiscount failed | DiscountId={DiscountId} | OwnerId={OwnerId}",
                    discountDto.ID, ownerId);

                return ApiResponseHelper.Failure("An unexpected error occurred.");
            }
        }

        [HttpDelete("{discountId:long}")]
        public async Task<IActionResult> DeleteDiscountAsync(long discountId)
        {
            var ownerId = _currentUser.UserId;
            if (ownerId <= 0)
                return ApiResponseHelper.Failure("Access denied.");

            if (discountId <= 0)
                return ApiResponseHelper.Failure("Invalid discount ID.");

            _logger.LogInformation(
                "DeleteDiscount started | OwnerId={OwnerId} | DiscountId={DiscountId}",
                ownerId, discountId);

            try
            {
                var discountService = new Discounts(_connStr);

                // 1️⃣ Validate discount ownership
                if (!await discountService.IsValidDiscountId(ownerId, discountId))
                    return ApiResponseHelper.Failure("Discount not found or access denied.");

                // 2️⃣ Delete discount (soft delete recommended)
                var isDeleted = await discountService.SoftDeleteDiscountAsync(ownerId, discountId);
                if (!isDeleted)
                    return ApiResponseHelper.Failure("Failed to delete discount.");

                // 3️⃣ Optional: deactivate mappings
                await DeactivateDiscountMappingsAsync(discountId);

                _logger.LogInformation(
                    "DeleteDiscount completed | OwnerId={OwnerId} | DiscountId={DiscountId}",
                    ownerId, discountId);

                return ApiResponseHelper.Success(
                    discountId,
                    "Discount deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "DeleteDiscount failed | OwnerId={OwnerId} | DiscountId={DiscountId}",
                    ownerId, discountId);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.");
            }
        }


        private async Task SyncDiscountMappingsAsync(DiscountDto discountDto, long discountId)
        {
            if (discountDto.SelectedItems == null || !discountDto.SelectedItems.Any())
                return;

            var mappingService = new MappingSyncService(_connStr);

            switch ((DiscountType)discountDto.Type)
            {
                case DiscountType.Item:
                    await mappingService.SyncAsync(
                        Table.SysCateringDiscountItemMapping,
                        "c_discountid",
                        "c_foodid",
                        discountId,
                        discountDto.SelectedItems);
                    break;

                case DiscountType.Package:
                    await mappingService.SyncAsync(
                        Table.SysCateringDiscountPackageMapping,
                        "c_discountid",
                        "c_packageid",
                        discountId,
                        discountDto.SelectedItems);
                    break;
            }
        }

        private async Task DeactivateDiscountMappingsAsync(long discountId)
        {
            var mappingService = new MappingSyncService(_connStr);

            await mappingService.DeactivateByParentIdAsync(
                Table.SysCateringDiscountItemMapping,
                "c_discountid",
                discountId);

            await mappingService.DeactivateByParentIdAsync(
                Table.SysCateringDiscountPackageMapping,
                "c_discountid",
                discountId);

        }

        private async Task<bool> CheckPriceHigherThanSelectedItems(DiscountDto discountDto, long ownerPKID)
        {
            bool result = false;
            Discounts discounts = new Discounts(_connStr);
            switch ((DiscountType)discountDto.Type)
            {
                case DiscountType.Item:
                   result = await discounts.IsHigherThanSelectedItemPrice(
                        Table.SysFoodItems,
                        "c_foodid",
                        ownerPKID,
                        discountDto.Value,
                        discountDto.SelectedItems);
                    break;

                case DiscountType.Package:
                   result = await discounts.IsHigherThanSelectedItemPrice(
                        Table.SysMenuPackage,
                        "c_packageid",
                        ownerPKID,
                        discountDto.Value,
                        discountDto.SelectedItems);
                    break;
            }

            return result;
        }

    }
}
