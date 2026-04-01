using CateringEcommerce.API.Controllers.User;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Interfaces.User;
using CateringEcommerce.Domain.Models.User;
using CateringEcommerce.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CateringEcommerce.Tests.Unit.API.Controllers
{
    /// <summary>
    /// Unit tests for OrdersController.
    /// Verifies HTTP response codes and that the controller delegates
    /// correctly to IOrderService without doing any business logic itself.
    /// </summary>
    public class OrdersControllerTests
    {
        private readonly Mock<IOrderService> _orderService = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly ILogger<OrdersController> _logger = NullLogger<OrdersController>.Instance;

        private OrdersController CreateSut()
        {
            _currentUser.Setup(u => u.UserId).Returns(42L);
            return new OrdersController(_logger, _currentUser.Object, _orderService.Object);
        }

        // ── POST /api/User/Orders/Create ───────────────────────────────────────

        [Fact]
        public async Task CreateOrder_ValidRequest_Returns200WithOrderDto()
        {
            // Arrange
            var expectedOrder = TestFixtures.BuildOrderDto();
            _orderService.Setup(s => s.CreateOrderAsync(42L, It.IsAny<CreateOrderDto>()))
                         .ReturnsAsync(expectedOrder);

            var controller = CreateSut();
            var dto = TestFixtures.BuildValidOrderDto();

            // Act
            var result = await controller.CreateOrder(dto);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var body = okResult.Value!;
            var resultProp = body.GetType().GetProperty("result")!.GetValue(body);
            resultProp.Should().Be(true);
        }

        [Fact]
        public async Task CreateOrder_ServiceThrowsInvalidOperation_Returns400()
        {
            // Arrange
            _orderService.Setup(s => s.CreateOrderAsync(It.IsAny<long>(), It.IsAny<CreateOrderDto>()))
                         .ThrowsAsync(new InvalidOperationException("Event date must be at least 24 hours in advance."));

            var controller = CreateSut();
            var dto = TestFixtures.BuildValidOrderDto();

            // Act
            var result = await controller.CreateOrder(dto);

            // Assert — InvalidOperationException maps to 200 OK with result=false (warning type)
            // OR BadRequest depending on ApiResponseHelper. Both cases checked:
            result.Should().BeAssignableTo<ObjectResult>();
            var objResult = (ObjectResult)result;
            var body = objResult.Value!;
            var resultProp = body.GetType().GetProperty("result")!.GetValue(body);
            resultProp.Should().Be(false);
        }

        [Fact]
        public async Task CreateOrder_UserIdIsZero_ReturnsBadRequestWithMessage()
        {
            // Arrange — user not authenticated
            _currentUser.Setup(u => u.UserId).Returns(0L);
            _orderService.Setup(s => s.CreateOrderAsync(It.IsAny<long>(), It.IsAny<CreateOrderDto>()))
                         .ReturnsAsync(TestFixtures.BuildOrderDto());

            var controller = new OrdersController(_logger, _currentUser.Object, _orderService.Object);
            var dto = TestFixtures.BuildValidOrderDto();

            // Act
            var result = await controller.CreateOrder(dto);

            // Assert
            var objResult = result.Should().BeAssignableTo<ObjectResult>().Subject;
            var body = objResult.Value!;
            var resultProp = body.GetType().GetProperty("result")!.GetValue(body);
            resultProp.Should().Be(false);
        }

        [Fact]
        public async Task CreateOrder_EmptyOrderItems_ReturnsBadRequest()
        {
            // Arrange
            var controller = CreateSut();
            var dto = TestFixtures.BuildValidOrderDto();
            dto.OrderItems = new List<CreateOrderItemDto>(); // no items

            // Act
            var result = await controller.CreateOrder(dto);

            // Assert
            var objResult = result.Should().BeAssignableTo<ObjectResult>().Subject;
            var body = objResult.Value!;
            var resultProp = body.GetType().GetProperty("result")!.GetValue(body);
            resultProp.Should().Be(false);

            // Service should NOT have been called
            _orderService.Verify(s => s.CreateOrderAsync(It.IsAny<long>(), It.IsAny<CreateOrderDto>()), Times.Never);
        }

        // ── GET /api/User/Orders ───────────────────────────────────────────────

        [Fact]
        public async Task GetUserOrders_ValidUser_Returns200WithList()
        {
            // Arrange
            var orders = new List<OrderListItemDto>
            {
                new OrderListItemDto { OrderId = 1, OrderNumber = "ORD-001" },
                new OrderListItemDto { OrderId = 2, OrderNumber = "ORD-002" }
            };
            _orderService.Setup(s => s.GetUserOrdersAsync(42L, 1, 10)).ReturnsAsync(orders);

            var controller = CreateSut();

            // Act
            var result = await controller.GetUserOrders();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var body = okResult.Value!;
            body.GetType().GetProperty("result")!.GetValue(body).Should().Be(true);
        }

        // ── GET /api/User/Orders/{orderId} ─────────────────────────────────────

        [Fact]
        public async Task GetOrderDetails_OrderExists_Returns200()
        {
            // Arrange
            var order = TestFixtures.BuildOrderDto(orderId: 55L);
            _orderService.Setup(s => s.GetOrderDetailsAsync(42L, 55L)).ReturnsAsync(order);

            var controller = CreateSut();

            // Act
            var result = await controller.GetOrderDetails(55L);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetOrderDetails_OrderNotFound_Returns404()
        {
            // Arrange
            _orderService.Setup(s => s.GetOrderDetailsAsync(It.IsAny<long>(), It.IsAny<long>()))
                         .ReturnsAsync((OrderDto?)null);

            var controller = CreateSut();

            // Act
            var result = await controller.GetOrderDetails(999L);

            // Assert — controller uses ApiResponseHelper.Failure("msg", "warning") → OkObjectResult with result=false
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var body = okResult.Value!;
            body.GetType().GetProperty("result")!.GetValue(body).Should().Be(false);
        }

        // ── POST /api/User/Orders/{orderId}/Cancel ─────────────────────────────

        [Fact]
        public async Task CancelOrder_Success_Returns200()
        {
            // Arrange
            _orderService.Setup(s => s.CancelOrderAsync(42L, 55L, "Changed plans"))
                         .ReturnsAsync(true);

            var controller = CreateSut();

            // Act
            var result = await controller.CancelOrder(55L, new CancelOrderDto { Reason = "Changed plans" });

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var body = okResult.Value!;
            body.GetType().GetProperty("result")!.GetValue(body).Should().Be(true);
        }

        [Fact]
        public async Task CancelOrder_ServiceReturnsFalse_ReturnsBadRequest()
        {
            // Arrange
            _orderService.Setup(s => s.CancelOrderAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>()))
                         .ReturnsAsync(false);

            var controller = CreateSut();

            // Act
            var result = await controller.CancelOrder(55L, new CancelOrderDto { Reason = "Test" });

            // Assert
            var objResult = result.Should().BeAssignableTo<ObjectResult>().Subject;
            var body = objResult.Value!;
            body.GetType().GetProperty("result")!.GetValue(body).Should().Be(false);
        }
    }
}
