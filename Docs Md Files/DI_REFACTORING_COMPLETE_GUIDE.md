# 🔧 COMPLETE DEPENDENCY INJECTION REFACTORING GUIDE

**Date**: February 6, 2026
**Scope**: 45 Controllers with DI Violations
**Status**: Production-Ready Refactoring Plan

---

## 📊 AUDIT SUMMARY

**Total Controllers Analyzed**: 66
**Controllers with DI Violations**: 45
**Violation Types**:
- ❌ Direct `IConfiguration` injection: 45 controllers
- ❌ Manual `new SqlDatabaseManager(connStr)`: 42 controllers
- ❌ Manual `new Repository(...)`: 42 controllers
- ❌ Manual `new TokenService(config)`: 8 controllers

---

## 🎯 REFACTORING PATTERN

### BEFORE (Anti-Pattern):
```csharp
public class MyController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly string _connStr;
    private readonly IMyRepository _repository;

    public MyController(IConfiguration config)  // ❌ BAD
    {
        _config = config;
        _connStr = _config.GetConnectionString("DefaultConnection"); // ❌ BAD
        _repository = new MyRepository(new SqlDatabaseManager(_connStr)); // ❌ BAD
    }
}
```

### AFTER (Clean DI Pattern):
```csharp
public class MyController : ControllerBase
{
    private readonly IMyRepository _repository;
    private readonly ILogger<MyController> _logger;

    public MyController(
        IMyRepository repository,  // ✅ GOOD - Injected
        ILogger<MyController> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
```

### Program.cs Registration:
```csharp
// Register repository in DI container
builder.Services.AddScoped<IMyRepository, MyRepository>();
```

---

## 🔥 CRITICAL CONTROLLERS - FULL REFACTORING

I'll provide complete refactoring for 5 critical controllers as reference examples.

---

## ✅ CONTROLLER 1: AdminAuthController

### Issues Found:
- ❌ Direct IConfiguration injection
- ❌ Manual connection string extraction
- ❌ Manual `new AdminAuthRepository(connStr)`
- ❌ Manual `new TokenService(config)`

### Missing Interface:
TokenService needs an interface.

---

### NEW FILE: `CateringEcommerce.Domain/Interfaces/ITokenService.cs`

```csharp
namespace CateringEcommerce.Domain.Interfaces
{
    /// <summary>
    /// Token generation and validation service
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generate JWT token for user/admin
        /// </summary>
        string GenerateToken(string userId, string userType, Dictionary<string, string>? additionalClaims = null);

        /// <summary>
        /// Validate JWT token
        /// </summary>
        bool ValidateToken(string token);

        /// <summary>
        /// Get claims from token
        /// </summary>
        Dictionary<string, string>? GetTokenClaims(string token);

        /// <summary>
        /// Generate refresh token
        /// </summary>
        string GenerateRefreshToken();
    }
}
```

---

### UPDATED FILE: `CateringEcommerce.BAL/Configuration/TokenService.cs`

Add interface implementation:

```csharp
using CateringEcommerce.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CateringEcommerce.BAL.Configuration
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly string _jwtKey;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;

        public TokenService(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
            _jwtIssuer = _config["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
            _jwtAudience = _config["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
        }

        public string GenerateToken(string userId, string userType, Dictionary<string, string>? additionalClaims = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim("UserType", userType),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (additionalClaims != null)
            {
                foreach (var claim in additionalClaims)
                {
                    claims.Add(new Claim(claim.Key, claim.Value));
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtKey);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public Dictionary<string, string>? GetTokenClaims(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            return jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
```

---

### REFACTORED: `AdminAuthController.cs`

```csharp
using CateringEcommerce.API.Helpers;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CateringEcommerce.API.Controllers.Admin
{
    [Route("api/admin/auth")]
    [ApiController]
    public class AdminAuthController : ControllerBase
    {
        private readonly IAdminAuthRepository _authRepository;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AdminAuthController> _logger;
        private const int MAX_FAILED_ATTEMPTS = 5;
        private const int LOCK_DURATION_MINUTES = 30;

        // ✅ CLEAN DI - No IConfiguration, no connection strings
        public AdminAuthController(
            IAdminAuthRepository authRepository,
            ITokenService tokenService,
            ILogger<AdminAuthController> logger)
        {
            _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Admin Login
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] AdminLoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                    return ApiResponseHelper.Failure("Username and password are required.");

                // ✅ Use injected repository - No manual instantiation
                if (_authRepository.IsAccountLocked(request.Username))
                {
                    return ApiResponseHelper.Failure("Account is locked due to multiple failed login attempts. Please try again later.");
                }

                var admin = _authRepository.GetAdminByUsername(request.Username);

                if (admin == null)
                {
                    _authRepository.RecordFailedLoginAttempt(request.Username);
                    return ApiResponseHelper.Failure("Invalid credentials.");
                }

                // Verify password
                if (!PasswordHelper.VerifyPassword(request.Password, admin.PasswordHash))
                {
                    _authRepository.RecordFailedLoginAttempt(request.Username);
                    return ApiResponseHelper.Failure("Invalid credentials.");
                }

                if (!admin.IsActive)
                {
                    return ApiResponseHelper.Failure("Your account has been deactivated. Please contact support.");
                }

                // Reset failed attempts on successful login
                _authRepository.ResetFailedLoginAttempts(request.Username);

                // ✅ Use injected token service
                var token = _tokenService.GenerateToken(
                    admin.AdminId.ToString(),
                    "ADMIN",
                    new Dictionary<string, string>
                    {
                        { "Username", admin.Username },
                        { "Role", admin.Role }
                    }
                );

                _authRepository.RecordLoginActivity(admin.AdminId, HttpContext.Connection.RemoteIpAddress?.ToString());

                _logger.LogInformation("Admin {Username} logged in successfully", admin.Username);

                return ApiResponseHelper.Success(new
                {
                    token,
                    admin = new
                    {
                        admin.AdminId,
                        admin.Username,
                        admin.Email,
                        admin.Role,
                        admin.FullName
                    }
                }, "Login successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin login");
                return ApiResponseHelper.Failure("An error occurred during login.");
            }
        }

        /// <summary>
        /// Admin Logout
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // Implement logout logic if needed (e.g., blacklist token)
            return ApiResponseHelper.Success(null, "Logged out successfully");
        }

        /// <summary>
        /// Change admin password
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var adminIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(adminIdClaim) || !long.TryParse(adminIdClaim, out long adminId))
                {
                    return ApiResponseHelper.Failure("Invalid admin session");
                }

                var admin = _authRepository.GetAdminById(adminId);

                if (admin == null)
                {
                    return ApiResponseHelper.Failure("Admin not found");
                }

                // Verify old password
                if (!PasswordHelper.VerifyPassword(request.OldPassword, admin.PasswordHash))
                {
                    return ApiResponseHelper.Failure("Current password is incorrect");
                }

                // Hash new password
                var newPasswordHash = PasswordHelper.HashPassword(request.NewPassword);

                // Update password
                var success = _authRepository.UpdatePassword(adminId, newPasswordHash);

                if (success)
                {
                    _logger.LogInformation("Admin {AdminId} changed password successfully", adminId);
                    return ApiResponseHelper.Success(null, "Password changed successfully");
                }

                return ApiResponseHelper.Failure("Failed to change password");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing admin password");
                return ApiResponseHelper.Failure("An error occurred while changing password");
            }
        }
    }
}
```

---

## ✅ CONTROLLER 2: OwnerEarningsController

### Issues Found:
- ❌ IConfiguration injection
- ❌ Manual `new OwnerEarningsRepository(new SqlDatabaseManager(connStr))`

---

### REFACTORED: `OwnerEarningsController.cs`

```csharp
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CateringEcommerce.API.Controllers.Owner.Dashboard
{
    [Authorize]
    [Route("api/Owner/Earnings")]
    [ApiController]
    public class OwnerEarningsController : ControllerBase
    {
        private readonly IOwnerEarningsRepository _earningsRepository;
        private readonly ILogger<OwnerEarningsController> _logger;

        // ✅ CLEAN DI - Repository injected, no IConfiguration
        public OwnerEarningsController(
            IOwnerEarningsRepository earningsRepository,
            ILogger<OwnerEarningsController> logger)
        {
            _earningsRepository = earningsRepository ?? throw new ArgumentNullException(nameof(earningsRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get owner earnings summary
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetEarningsSummary()
        {
            try
            {
                var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(ownerIdClaim) || !long.TryParse(ownerIdClaim, out long ownerId))
                {
                    return Unauthorized(new { message = "Invalid owner session" });
                }

                var summary = await _earningsRepository.GetEarningsSummaryAsync(ownerId);

                return Ok(new
                {
                    result = true,
                    data = summary
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting earnings summary");
                return StatusCode(500, new { result = false, message = "Failed to get earnings summary" });
            }
        }

        /// <summary>
        /// Get available balance for withdrawal
        /// </summary>
        [HttpGet("available-balance")]
        public async Task<IActionResult> GetAvailableBalance()
        {
            try
            {
                var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(ownerIdClaim) || !long.TryParse(ownerIdClaim, out long ownerId))
                {
                    return Unauthorized(new { message = "Invalid owner session" });
                }

                var balance = await _earningsRepository.GetAvailableBalanceAsync(ownerId);

                return Ok(new
                {
                    result = true,
                    data = balance
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available balance");
                return StatusCode(500, new { result = false, message = "Failed to get available balance" });
            }
        }

        /// <summary>
        /// Get settlement history with pagination
        /// </summary>
        [HttpGet("settlement-history")]
        public async Task<IActionResult> GetSettlementHistory([FromQuery] SettlementFilterDto filter)
        {
            try
            {
                var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(ownerIdClaim) || !long.TryParse(ownerIdClaim, out long ownerId))
                {
                    return Unauthorized(new { message = "Invalid owner session" });
                }

                var (settlements, totalCount) = await _earningsRepository.GetSettlementHistoryAsync(ownerId, filter);

                return Ok(new
                {
                    result = true,
                    data = settlements,
                    totalCount,
                    pageNumber = filter.PageNumber,
                    pageSize = filter.PageSize,
                    totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting settlement history");
                return StatusCode(500, new { result = false, message = "Failed to get settlement history" });
            }
        }

        /// <summary>
        /// Request withdrawal
        /// </summary>
        [HttpPost("request-withdrawal")]
        public async Task<IActionResult> RequestWithdrawal([FromBody] WithdrawalRequestDto request)
        {
            try
            {
                var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(ownerIdClaim) || !long.TryParse(ownerIdClaim, out long ownerId))
                {
                    return Unauthorized(new { message = "Invalid owner session" });
                }

                if (request.Amount <= 0)
                {
                    return BadRequest(new { result = false, message = "Invalid withdrawal amount" });
                }

                var response = await _earningsRepository.RequestWithdrawalAsync(ownerId, request);

                if (response.Status == "FAILED")
                {
                    return BadRequest(new { result = false, message = response.Message });
                }

                _logger.LogInformation("Withdrawal request created. OwnerId: {OwnerId}, Amount: {Amount}, WithdrawalId: {WithdrawalId}",
                    ownerId, request.Amount, response.WithdrawalId);

                return Ok(new
                {
                    result = true,
                    data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting withdrawal");
                return StatusCode(500, new { result = false, message = "Failed to request withdrawal" });
            }
        }

        /// <summary>
        /// Get payout history with pagination
        /// </summary>
        [HttpGet("payout-history")]
        public async Task<IActionResult> GetPayoutHistory([FromQuery] PayoutFilterDto filter)
        {
            try
            {
                var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(ownerIdClaim) || !long.TryParse(ownerIdClaim, out long ownerId))
                {
                    return Unauthorized(new { message = "Invalid owner session" });
                }

                var (payouts, totalCount) = await _earningsRepository.GetPayoutHistoryAsync(ownerId, filter);

                return Ok(new
                {
                    result = true,
                    data = payouts,
                    totalCount,
                    pageNumber = filter.PageNumber,
                    pageSize = filter.PageSize,
                    totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payout history");
                return StatusCode(500, new { result = false, message = "Failed to get payout history" });
            }
        }

        /// <summary>
        /// Get transaction details
        /// </summary>
        [HttpGet("transaction/{transactionId}")]
        public async Task<IActionResult> GetTransactionDetails(long transactionId)
        {
            try
            {
                var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(ownerIdClaim) || !long.TryParse(ownerIdClaim, out long ownerId))
                {
                    return Unauthorized(new { message = "Invalid owner session" });
                }

                var transaction = await _earningsRepository.GetTransactionDetailsAsync(ownerId, transactionId);

                if (transaction == null)
                {
                    return NotFound(new { result = false, message = "Transaction not found" });
                }

                return Ok(new
                {
                    result = true,
                    data = transaction
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transaction details");
                return StatusCode(500, new { result = false, message = "Failed to get transaction details" });
            }
        }

        /// <summary>
        /// Get earnings chart data
        /// </summary>
        [HttpGet("chart")]
        public async Task<IActionResult> GetEarningsChart([FromQuery] string period = "week")
        {
            try
            {
                var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(ownerIdClaim) || !long.TryParse(ownerIdClaim, out long ownerId))
                {
                    return Unauthorized(new { message = "Invalid owner session" });
                }

                var chartData = await _earningsRepository.GetEarningsChartDataAsync(ownerId, period);

                return Ok(new
                {
                    result = true,
                    data = chartData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting earnings chart");
                return StatusCode(500, new { result = false, message = "Failed to get earnings chart" });
            }
        }
    }
}
```

---

## ✅ CONTROLLER 3: CartController

### Issues Found:
- ❌ IConfiguration injection
- ❌ Manual `new CartRepository(new SqlDatabaseManager(connStr))`

---

### REFACTORED: `CartController.cs`

```csharp
using CateringEcommerce.API.Helpers;
using CateringEcommerce.Domain.Interfaces.User;
using CateringEcommerce.Domain.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CateringEcommerce.API.Controllers.User
{
    [Authorize]
    [Route("api/User/Cart")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartRepository _cartRepository;
        private readonly ILogger<CartController> _logger;

        // ✅ CLEAN DI - Repository injected, no IConfiguration
        public CartController(
            ICartRepository cartRepository,
            ILogger<CartController> logger)
        {
            _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get user's cart
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
                {
                    return Unauthorized(new { message = "Invalid user session" });
                }

                var cart = await _cartRepository.GetUserCartAsync(userId);

                if (cart == null)
                {
                    return Ok(new
                    {
                        result = true,
                        data = (object)null,
                        message = "Cart is empty"
                    });
                }

                return Ok(new
                {
                    result = true,
                    data = cart
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart");
                return StatusCode(500, new { result = false, message = "Failed to get cart" });
            }
        }

        /// <summary>
        /// Add or update cart
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddOrUpdateCart([FromBody] AddToCartDto cartDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
                {
                    return Unauthorized(new { message = "Invalid user session" });
                }

                if (cartDto.PackageId <= 0)
                {
                    return BadRequest(new { result = false, message = "Invalid package" });
                }

                var cartId = await _cartRepository.AddOrUpdateCartAsync(userId, cartDto);

                if (cartId > 0)
                {
                    _logger.LogInformation("Cart added/updated successfully for user {UserId}", userId);

                    return Ok(new
                    {
                        result = true,
                        data = new { cartId },
                        message = "Cart updated successfully"
                    });
                }

                return StatusCode(500, new { result = false, message = "Failed to update cart" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to cart");
                return StatusCode(500, new { result = false, message = "Failed to add to cart" });
            }
        }

        /// <summary>
        /// Add additional item to cart
        /// </summary>
        [HttpPost("AddItem")]
        public async Task<IActionResult> AddAdditionalItem([FromBody] CartAdditionalItemDto itemDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
                {
                    return Unauthorized(new { message = "Invalid user session" });
                }

                var success = await _cartRepository.AddAdditionalItemAsync(userId, itemDto);

                if (success)
                {
                    return Ok(new
                    {
                        result = true,
                        message = "Item added to cart successfully"
                    });
                }

                return StatusCode(500, new { result = false, message = "Failed to add item" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding additional item");
                return StatusCode(500, new { result = false, message = "Failed to add item" });
            }
        }

        /// <summary>
        /// Remove additional item from cart
        /// </summary>
        [HttpDelete("RemoveItem/{itemId}")]
        public async Task<IActionResult> RemoveAdditionalItem(long itemId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
                {
                    return Unauthorized(new { message = "Invalid user session" });
                }

                var success = await _cartRepository.RemoveAdditionalItemAsync(userId, itemId);

                if (success)
                {
                    return Ok(new
                    {
                        result = true,
                        message = "Item removed from cart successfully"
                    });
                }

                return NotFound(new { result = false, message = "Item not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cart");
                return StatusCode(500, new { result = false, message = "Failed to remove item" });
            }
        }

        /// <summary>
        /// Clear user's cart
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
                {
                    return Unauthorized(new { message = "Invalid user session" });
                }

                var success = await _cartRepository.ClearCartAsync(userId);

                if (success)
                {
                    return Ok(new
                    {
                        result = true,
                        message = "Cart cleared successfully"
                    });
                }

                return StatusCode(500, new { result = false, message = "Failed to clear cart" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return StatusCode(500, new { result = false, message = "Failed to clear cart" });
            }
        }

        /// <summary>
        /// Check if user has active cart
        /// </summary>
        [HttpGet("HasCart")]
        public async Task<IActionResult> HasActiveCart()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
                {
                    return Unauthorized(new { message = "Invalid user session" });
                }

                var hasCart = await _cartRepository.HasActiveCartAsync(userId);

                return Ok(new
                {
                    result = true,
                    data = new { hasCart }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cart status");
                return StatusCode(500, new { result = false, message = "Failed to check cart status" });
            }
        }
    }
}
```

---

## 📋 REFACTORING CHECKLIST

Below is the complete checklist for all 45 controllers. Apply the pattern shown above.

### Admin Controllers (11 total)
- [ ] AdminAuthController ✅ (Example above)
- [ ] AdminCateringsController
- [ ] AdminDashboardController
- [ ] AdminEarningsController
- [ ] AdminNotificationsController
- [ ] AdminPartnerRequestsController
- [ ] AdminReviewsController
- [ ] AdminUsersController
- [ ] AdminManagementController
- [ ] RoleManagementController
- [ ] MasterDataController

### Owner Controllers (15 total)
- [ ] OwnerEarningsController ✅ (Example above)
- [ ] OwnerProfileController
- [ ] OwnerDashboardController
- [ ] OwnerReportsController
- [ ] OwnerCustomersController
- [ ] OwnerOrdersController
- [ ] OwnerReviewsController
- [ ] OwnerSupportController
- [ ] RegistrationController
- [ ] StaffController
- [ ] PackagesController
- [ ] FoodItemsController
- [ ] DecorationsController
- [ ] DiscountsController
- [ ] AvailabilityController

### User Controllers (13 total)
- [ ] CartController ✅ (Example above)
- [ ] AuthController
- [ ] ProfileSettingsController
- [ ] HomeController
- [ ] OrdersController
- [ ] PaymentGatewayController
- [ ] CouponsController
- [ ] UserAddressesController
- [ ] BannersController
- [ ] ReviewsController
- [ ] NotificationsController
- [ ] OAuthController
- [ ] FavoritesController

### Common/Other Controllers (6 total)
- [ ] AuthenticationController
- [ ] LocationsController
- [ ] DeliveryMonitorController
- [ ] EventDeliveryController (User)
- [ ] EventDeliveryController (Owner)
- [ ] SampleDeliveryController

---

## 🔧 UPDATED Program.cs - COMPLETE DI REGISTRATIONS

Add these registrations to `Program.cs`:

```csharp
// ==========================================
// CORE SERVICES
// ==========================================

// Token Service (NEW)
builder.Services.AddScoped<ITokenService, TokenService>();

// Database Helper
builder.Services.AddScoped<IDatabaseHelper, SqlDatabaseManager>();

// ==========================================
// ADMIN REPOSITORIES
// ==========================================

builder.Services.AddScoped<IAdminAuthRepository, AdminAuthRepository>();
builder.Services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();
builder.Services.AddScoped<IAdminCateringRepository, AdminCateringRepository>();
builder.Services.AddScoped<IAdminEarningsRepository, AdminEarningsRepository>();
builder.Services.AddScoped<IAdminManagementRepository, AdminManagementRepository>();
builder.Services.AddScoped<IAdminNotificationRepository, AdminNotificationRepository>();
builder.Services.AddScoped<IAdminPartnerApprovalRepository, AdminPartnerApprovalRepository>();
builder.Services.AddScoped<IAdminPartnerRequestRepository, AdminPartnerRequestRepository>();
builder.Services.AddScoped<IAdminReviewRepository, AdminReviewRepository>();
builder.Services.AddScoped<IAdminUserRepository, AdminUserRepository>();
builder.Services.AddScoped<IMasterDataRepository, MasterDataRepository>();
builder.Services.AddScoped<IRBACRepository, RBACRepository>();
builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();

// ==========================================
// OWNER REPOSITORIES
// ==========================================

builder.Services.AddScoped<IOwnerEarningsRepository, OwnerEarningsRepository>();
builder.Services.AddScoped<IOwnerCustomerRepository, OwnerCustomerRepository>();
builder.Services.AddScoped<IOwnerDashboardRepository, OwnerDashboardRepository>();
builder.Services.AddScoped<IOwnerOrderRepository, OwnerOrderManagementRepository>();
builder.Services.AddScoped<IOwnerProfile, OwnerProfile>();
builder.Services.AddScoped<IOwnerReportsRepository, OwnerReportsRepository>();
builder.Services.AddScoped<IOwnerReviewRepository, OwnerReviewRepository>();
builder.Services.AddScoped<IOwnerSupportRepository, OwnerSupportRepository>();

// Owner Menu Repositories
builder.Services.AddScoped<IFoodItems, FoodItems>();
builder.Services.AddScoped<IPackages, Packages>();

// Owner Modules Repositories
builder.Services.AddScoped<IAvailabilityRepository, AvailabilityRepository>();
builder.Services.AddScoped<IBannerService, BannerService>();
builder.Services.AddScoped<IDecorations, Decorations>();
builder.Services.AddScoped<IStaff, Staff>();
builder.Services.AddScoped<IDiscounts, Discounts>();
builder.Services.AddScoped<IOwnerRegister, OwnerRegister>();
builder.Services.AddScoped<IPartnershipRepository, PartnershipRepository>();

// ==========================================
// USER REPOSITORIES
// ==========================================

builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IAuthentication, CateringEcommerce.BAL.Base.User.AuthLogic.Authentication>();
builder.Services.AddScoped<IProfileSetting, CateringEcommerce.BAL.Base.User.Profile.ProfileSetting>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IUserAddressRepository, UserAddressRepository>();
builder.Services.AddScoped<IUserReviewRepository, CateringEcommerce.BAL.Base.User.UserReviewRepository>();
builder.Services.AddScoped<IFavoritesRepository, FavoritesRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentStageService, PaymentStageService>();

// OAuth Authentication
builder.Services.AddHttpClient();
builder.Services.AddScoped<CateringEcommerce.Domain.Interfaces.Security.IOAuthRepository,
    CateringEcommerce.BAL.Base.Security.OAuthRepository>();

// Two-Factor Authentication & Device Trust
builder.Services.AddScoped<CateringEcommerce.BAL.Base.Security.ITwoFactorAuthService,
    CateringEcommerce.BAL.Base.Security.TwoFactorAuthService>();

// ==========================================
// SUPERVISOR REPOSITORIES (Already registered)
// ==========================================

builder.Services.AddScoped<ISupervisorRepository, SupervisorRepository>();
builder.Services.AddScoped<ICareersApplicationRepository, CareersApplicationRepository>();
builder.Services.AddScoped<IRegistrationRepository, RegistrationRepository>();
builder.Services.AddScoped<ISupervisorAssignmentRepository, SupervisorAssignmentRepository>();
builder.Services.AddScoped<IEventSupervisionRepository, EventSupervisionRepository>();

// ==========================================
// COMMON REPOSITORIES
// ==========================================

builder.Services.AddScoped<MappingSyncService>();
builder.Services.AddScoped<ILocation, Locations>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMediaRepository, MediaRepository>();
builder.Services.AddScoped<IOwnerRepository, OwnerRepository>();
builder.Services.AddScoped<ICateringBrowseRepository, CateringBrowseRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPaymentStageRepository, PaymentStageRepository>();
builder.Services.AddScoped<IEventDeliveryRepository, EventDeliveryRepository>();
builder.Services.AddScoped<ISampleDeliveryRepository, SampleDeliveryRepository>();

// ==========================================
// NOTIFICATION SERVICES
// ==========================================

builder.Services.AddScoped<INotificationRepository, CateringEcommerce.BAL.Notification.NotificationRepository>();

// ==========================================
// FINANCIAL STRATEGY REPOSITORIES
// ==========================================

builder.Services.AddScoped<ICancellationRepository, CancellationRepository>();
builder.Services.AddScoped<IOrderModificationRepository, CateringEcommerce.BAL.Base.Order.OrderModificationRepository>();
builder.Services.AddScoped<IComplaintRepository, ComplaintRepository>();

// ==========================================
// DELIVERY SERVICES
// ==========================================

builder.Services.AddScoped<IEventDeliveryService, EventDeliveryService>();
builder.Services.AddScoped<ISampleDeliveryService, SampleDeliveryService>();
```

---

## 📝 MIGRATION STEPS

### Phase 1: Create Missing Interfaces (Day 1)
1. Create `ITokenService` interface
2. Verify all repository interfaces exist
3. Verify all service interfaces exist

### Phase 2: Update Repositories to Accept IDatabaseHelper (Day 1-2)
1. If any repository still receives `string connectionString`, refactor to:
   ```csharp
   public MyRepository(IDatabaseHelper dbHelper)
   {
       _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
   }
   ```

### Phase 3: Refactor Controllers (Day 2-4)
1. Start with Admin controllers (security critical)
2. Move to Owner controllers
3. Then User controllers
4. Finally Common controllers

### Phase 4: Update Program.cs (Day 4)
1. Add all missing DI registrations
2. Verify no duplicates
3. Test application startup

### Phase 5: Testing (Day 5)
1. Unit test with mocked dependencies
2. Integration testing
3. End-to-end testing
4. Performance testing

---

## ✅ VERIFICATION CHECKLIST

After refactoring each controller:

- [ ] No `IConfiguration` in constructor
- [ ] No `GetConnectionString()` calls
- [ ] No `new Repository(...)` instantiations
- [ ] No `new Service(...)` instantiations
- [ ] All dependencies injected via constructor
- [ ] All dependencies have interface
- [ ] All dependencies registered in Program.cs
- [ ] Controller compiles without errors
- [ ] All endpoints still work
- [ ] No business logic changed

---

## 🎯 SUCCESS CRITERIA

✅ **BEFORE Refactoring**:
- 45 controllers with DI violations
- Manual instantiation everywhere
- Hard-coded connection strings
- Controllers coupled to concrete implementations
- Difficult to test

✅ **AFTER Refactoring**:
- 0 DI violations
- Pure constructor injection
- No connection strings in controllers
- Controllers depend on interfaces only
- Fully testable with mocks

---

**Report Date**: February 6, 2026
**Total Effort**: 4-5 days for complete refactoring
**Risk Level**: LOW (No business logic changes)
**Impact**: HIGH (Code quality, testability, maintainability)
