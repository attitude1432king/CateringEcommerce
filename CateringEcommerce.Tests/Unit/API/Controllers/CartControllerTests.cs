using CateringEcommerce.API.Controllers.User;
using CateringEcommerce.Domain.Interfaces.User;
using CateringEcommerce.Domain.Models.User;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Security.Claims;

namespace CateringEcommerce.Tests.Unit.API.Controllers
{
    /// <summary>
    /// Unit tests for CartController.
    /// CartController reads userId directly from HttpContext.User Claims,
    /// so tests use ControllerContext with a mock ClaimsPrincipal.
    /// </summary>
    public class CartControllerTests
    {
        private readonly Mock<ICartRepository> _cartRepo = new();

        private CartController CreateSut(long userId = 42L)
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "test");
            var principal = new ClaimsPrincipal(identity);

            var controller = new CartController(_cartRepo.Object, NullLogger<CartController>.Instance);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
            return controller;
        }

        // ── GET /api/User/Cart ────────────────────────────────────────────────

        [Fact]
        public async Task GetCart_AuthenticatedUser_Returns200WithData()
        {
            // Arrange
            var cart = new CartResponseDto { CateringId = 1, CateringName = "Test Caterers" };
            _cartRepo.Setup(r => r.GetUserCartAsync(42L)).ReturnsAsync(cart);
            var controller = CreateSut();

            // Act
            var result = await controller.GetCart();

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var body = ok.Value!;
            body.GetType().GetProperty("result")!.GetValue(body).Should().Be(true);
        }

        [Fact]
        public async Task GetCart_CartIsEmpty_Returns200WithNullData()
        {
            // Arrange — service returns null (empty cart)
            _cartRepo.Setup(r => r.GetUserCartAsync(42L)).ReturnsAsync((CartResponseDto?)null);
            var controller = CreateSut();

            // Act
            var result = await controller.GetCart();

            // Assert — returns result=true, data=null (not a 404)
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var body = ok.Value!;
            body.GetType().GetProperty("result")!.GetValue(body).Should().Be(true);
            body.GetType().GetProperty("data")!.GetValue(body).Should().BeNull();
        }

        // ── POST /api/User/Cart ───────────────────────────────────────────────

        [Fact]
        public async Task AddToCart_ValidDto_Returns200WithResultTrue()
        {
            // Arrange
            var cartDto = new AddToCartDto { CateringId = 1, PackageId = 10, GuestCount = 50 };
            var updatedCart = new CartResponseDto { CateringId = 1 };
            _cartRepo.Setup(r => r.AddOrUpdateCartAsync(42L, cartDto)).ReturnsAsync(100L);
            _cartRepo.Setup(r => r.GetUserCartAsync(42L)).ReturnsAsync(updatedCart);
            var controller = CreateSut();

            // Act
            var result = await controller.AddToCart(cartDto);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var body = ok.Value!;
            body.GetType().GetProperty("result")!.GetValue(body).Should().Be(true);
            _cartRepo.Verify(r => r.AddOrUpdateCartAsync(42L, cartDto), Times.Once);
        }

        // ── DELETE /api/User/Cart ─────────────────────────────────────────────

        [Fact]
        public async Task ClearCart_Success_Returns200WithResultTrue()
        {
            // Arrange
            _cartRepo.Setup(r => r.ClearCartAsync(42L)).ReturnsAsync(true);
            var controller = CreateSut();

            // Act
            var result = await controller.ClearCart();

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var body = ok.Value!;
            body.GetType().GetProperty("result")!.GetValue(body).Should().Be(true);
        }

        // ── GET /api/User/Cart/HasCart ────────────────────────────────────────

        [Fact]
        public async Task HasCart_UserHasActiveCart_Returns200WithHasCartTrue()
        {
            // Arrange
            _cartRepo.Setup(r => r.HasActiveCartAsync(42L)).ReturnsAsync(true);
            var controller = CreateSut();

            // Act
            var result = await controller.HasCart();

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var body = ok.Value!;
            body.GetType().GetProperty("result")!.GetValue(body).Should().Be(true);
            body.GetType().GetProperty("hasCart")!.GetValue(body).Should().Be(true);
        }

        [Fact]
        public async Task HasCart_UserHasNoCart_Returns200WithHasCartFalse()
        {
            // Arrange
            _cartRepo.Setup(r => r.HasActiveCartAsync(42L)).ReturnsAsync(false);
            var controller = CreateSut();

            // Act
            var result = await controller.HasCart();

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var body = ok.Value!;
            body.GetType().GetProperty("hasCart")!.GetValue(body).Should().Be(false);
        }

        // ── DELETE /api/User/Cart/RemoveItem/{foodId} ─────────────────────────

        [Fact]
        public async Task RemoveAdditionalItem_ItemNotFound_Returns404()
        {
            // Arrange — repo returns false (item doesn't exist)
            _cartRepo.Setup(r => r.RemoveAdditionalItemAsync(42L, 99L)).ReturnsAsync(false);
            var controller = CreateSut();

            // Act
            var result = await controller.RemoveAdditionalItem(99L);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
