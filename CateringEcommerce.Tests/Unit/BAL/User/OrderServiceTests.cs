using CateringEcommerce.BAL.Base.User;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.BAL.Services;
using CateringEcommerce.Domain.Interfaces.Notification;
using CateringEcommerce.Domain.Interfaces.User;
using CateringEcommerce.Domain.Models.User;
using CateringEcommerce.Tests.Helpers;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Data;

namespace CateringEcommerce.Tests.Unit.BAL.User
{
    /// <summary>
    /// Unit tests for OrderService business logic.
    /// All external dependencies are mocked — no real DB or API calls.
    /// Pattern: Arrange → Act → Assert
    /// </summary>
    public class OrderServiceTests
    {
        // ── Mocked dependencies ────────────────────────────────────────────────
        private readonly Mock<IDatabaseHelper> _dbHelper = new();
        private readonly Mock<IOrderRepository> _orderRepo = new();
        private readonly Mock<IPaymentStageRepository> _paymentStageRepo = new();
        private readonly Mock<INotificationHelper> _notificationHelper = new();
        private readonly Mock<INotificationService> _notificationService = new();
        private readonly Mock<IFileStorageService> _fileStorage = new();
        private readonly Mock<ISystemSettingsProvider> _settingsProvider = new();
        private readonly ILogger<OrderService> _logger = NullLogger<OrderService>.Instance;

        private OrderService CreateSut() => new OrderService(
            _dbHelper.Object,
            _orderRepo.Object,
            _paymentStageRepo.Object,
            _notificationHelper.Object,
            _notificationService.Object,
            _fileStorage.Object,
            _settingsProvider.Object,
            _logger
        );

        // ── Shared default mock setup ──────────────────────────────────────────

        /// <summary>
        /// Sets up all repository mocks for a successful order creation flow.
        /// Individual tests override only what they need to change.
        /// </summary>
        private void SetupHappyPathMocks(long orderId = 1001L, long cateringId = 1L, long packageId = 10L)
        {
            _settingsProvider
                .Setup(s => s.GetInt("BUSINESS.MIN_ADVANCE_BOOKING_DAYS", 5))
                .Returns(5);

            _settingsProvider
                .Setup(s => s.GetInt("BUSINESS.DEFAULT_DAILY_BOOKING_CAPACITY", 1))
                .Returns(1);

            // Catering is active & verified
            _dbHelper
                .Setup(d => d.ExecuteAsync(It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(TestFixtures.ActiveCateringDataTable());

            // Package item exists  (COUNT(*) = 1)
            _dbHelper
                .Setup(d => d.ExecuteAsync(
                    It.Is<string>(q => q.Contains("c_packageid")),
                    It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(TestFixtures.CountOneDataTable());

            _orderRepo.Setup(r => r.CheckCateringAvailabilityAsync(cateringId, It.IsAny<DateTime>()))
                      .ReturnsAsync(true);
            _orderRepo.Setup(r => r.GenerateOrderNumberAsync())
                      .ReturnsAsync("ORD-20260320-001");
            _orderRepo.Setup(r => r.InsertOrderAsync(It.IsAny<long>(), It.IsAny<CreateOrderDto>(), It.IsAny<string>()))
                      .ReturnsAsync(orderId);
            _orderRepo.Setup(r => r.InsertOrderItemsAsync(orderId, It.IsAny<List<CreateOrderItemDto>>()))
                      .ReturnsAsync(true);
            _orderRepo.Setup(r => r.InsertOrderPaymentAsync(orderId, It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string?>(), It.IsAny<string?>()))
                      .ReturnsAsync(true);
            _orderRepo.Setup(r => r.InsertOrderStatusHistoryAsync(orderId, It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<long?>()))
                      .ReturnsAsync(true);
            _orderRepo.Setup(r => r.GetOrderByIdAsync(orderId, It.IsAny<long>()))
                      .ReturnsAsync(TestFixtures.BuildOrderDto(orderId, cateringId));

            _paymentStageRepo.Setup(r => r.InsertPaymentStageAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<DateTime?>()))
                             .ReturnsAsync(1L);

            _notificationHelper.Setup(n => n.SendOrderNotificationAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<string?>(), It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);
        }

        // ── CreateOrderAsync — validation tests ────────────────────────────────

        [Fact]
        public async Task CreateOrder_EventDateLessThan24Hours_ThrowsInvalidOperation()
        {
            // Arrange
            var sut = CreateSut();
            var dto = TestFixtures.BuildValidOrderDto();
            dto.EventDate = DateTime.UtcNow.AddHours(12); // Too soon

            // Act
            Func<Task> act = () => sut.CreateOrderAsync(1, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*24 hours*");
        }

        [Fact]
        public async Task CreateOrder_CateringInactive_ThrowsInvalidOperation()
        {
            // Arrange
            _dbHelper.Setup(d => d.ExecuteAsync(It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
                     .ReturnsAsync(TestFixtures.InactiveCateringDataTable());

            var sut = CreateSut();
            var dto = TestFixtures.BuildValidOrderDto();

            // Act
            Func<Task> act = () => sut.CreateOrderAsync(1, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*unavailable*");
        }

        [Fact]
        public async Task CreateOrder_CateringNotFound_ThrowsInvalidOperation()
        {
            // Arrange
            _dbHelper.Setup(d => d.ExecuteAsync(It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
                     .ReturnsAsync(TestFixtures.EmptyDataTable()); // no rows

            var sut = CreateSut();
            var dto = TestFixtures.BuildValidOrderDto();

            // Act
            Func<Task> act = () => sut.CreateOrderAsync(1, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task CreateOrder_CateringUnavailableOnDate_ThrowsInvalidOperation()
        {
            // Arrange
            _dbHelper.Setup(d => d.ExecuteAsync(It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
                     .ReturnsAsync(TestFixtures.ActiveCateringDataTable());
            _orderRepo.Setup(r => r.CheckCateringAvailabilityAsync(It.IsAny<long>(), It.IsAny<DateTime>()))
                      .ReturnsAsync(false);

            var sut = CreateSut();
            var dto = TestFixtures.BuildValidOrderDto();

            // Act
            Func<Task> act = () => sut.CreateOrderAsync(1, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*not available*");
        }

        [Fact]
        public async Task CreateOrder_InvalidCartItems_ThrowsInvalidOperation()
        {
            // Arrange — catering is active but package returns COUNT=0
            _dbHelper.SetupSequence(d => d.ExecuteAsync(It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
                     .ReturnsAsync(TestFixtures.ActiveCateringDataTable())   // IsCateringActiveAsync
                     .ReturnsAsync(TestFixtures.CountZeroDataTable());       // IsPackageValidAsync

            _orderRepo.Setup(r => r.CheckCateringAvailabilityAsync(It.IsAny<long>(), It.IsAny<DateTime>()))
                      .ReturnsAsync(true);

            var sut = CreateSut();
            var dto = TestFixtures.BuildValidOrderDto();

            // Act
            Func<Task> act = () => sut.CreateOrderAsync(1, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*no longer available*");
        }

        [Fact]
        public async Task CreateOrder_BankTransfer_NoProof_ThrowsInvalidOperation()
        {
            // Arrange
            _dbHelper.Setup(d => d.ExecuteAsync(It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
                     .ReturnsAsync(TestFixtures.ActiveCateringDataTable());
            _orderRepo.Setup(r => r.CheckCateringAvailabilityAsync(It.IsAny<long>(), It.IsAny<DateTime>()))
                      .ReturnsAsync(true);
            _dbHelper.Setup(d => d.ExecuteAsync(
                    It.Is<string>(q => q.Contains("c_packageid")), It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(TestFixtures.CountOneDataTable());

            var sut = CreateSut();
            var dto = TestFixtures.BuildBankTransferOrderDto(proof: null!);

            // Act
            Func<Task> act = () => sut.CreateOrderAsync(1, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*proof*");
        }

        // ── CreateOrderAsync — payment logic tests ─────────────────────────────

        [Fact]
        public async Task CreateOrder_FullPayment_InsertOrderPaymentCalledWithFull()
        {
            // Arrange
            SetupHappyPathMocks();
            var sut = CreateSut();
            var dto = TestFixtures.BuildValidOrderDto();

            // Act
            await sut.CreateOrderAsync(1, dto);

            // Assert
            _orderRepo.Verify(r => r.InsertOrderPaymentAsync(
                It.IsAny<long>(),
                dto.PaymentMethod,
                dto.TotalAmount,
                null,
                "Full"),   // ← stage type must be "Full"
                Times.Once);
        }

        [Fact]
        public async Task CreateOrder_SplitPayment_CreatesTwoPaymentStages()
        {
            // Arrange
            SetupHappyPathMocks();
            var sut = CreateSut();
            var dto = TestFixtures.BuildSplitPaymentOrderDto();

            // Act
            await sut.CreateOrderAsync(1, dto);

            // Assert — InsertPaymentStageAsync called exactly twice
            _paymentStageRepo.Verify(r => r.InsertPaymentStageAsync(
                It.IsAny<long>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<DateTime?>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task CreateOrder_SplitPayment_PreBookingStageIs40Percent()
        {
            // Arrange
            SetupHappyPathMocks();
            var sut = CreateSut();
            var dto = TestFixtures.BuildSplitPaymentOrderDto();
            dto.PreBookingAmount = null; // force calculation from TotalAmount
            dto.TotalAmount = 10_000m;

            decimal capturedPercentage = 0;
            decimal capturedAmount = 0;
            _paymentStageRepo
                .Setup(r => r.InsertPaymentStageAsync(
                    It.IsAny<long>(),
                    "PreBooking",
                    It.IsAny<decimal>(),
                    It.IsAny<decimal>(),
                    null))
                .Callback<long, string, decimal, decimal, DateTime?>((_, _, pct, amt, _) =>
                {
                    capturedPercentage = pct;
                    capturedAmount = amt;
                })
                .ReturnsAsync(1L);

            // Act
            await sut.CreateOrderAsync(1, dto);

            // Assert
            capturedPercentage.Should().Be(40.00m);
            capturedAmount.Should().Be(4_000m); // 40% of 10,000
        }

        [Fact]
        public async Task CreateOrder_SplitPayment_PostEventDueDateIsEventDatePlusOneDay()
        {
            // Arrange
            SetupHappyPathMocks();
            var sut = CreateSut();
            var dto = TestFixtures.BuildSplitPaymentOrderDto();
            var eventDate = DateTime.UtcNow.AddDays(10).Date;
            dto.EventDate = eventDate;

            DateTime? capturedDueDate = null;
            _paymentStageRepo
                .Setup(r => r.InsertPaymentStageAsync(
                    It.IsAny<long>(),
                    "PostEvent",
                    It.IsAny<decimal>(),
                    It.IsAny<decimal>(),
                    It.IsAny<DateTime?>()))
                .Callback<long, string, decimal, decimal, DateTime?>((_, _, _, _, due) =>
                {
                    capturedDueDate = due;
                })
                .ReturnsAsync(2L);

            // Act
            await sut.CreateOrderAsync(1, dto);

            // Assert
            capturedDueDate.Should().Be(eventDate.AddDays(1));
        }

        [Fact]
        public async Task CreateOrder_SplitPayment_UsesProvidedAmountsWhenNotNull()
        {
            // Arrange
            SetupHappyPathMocks();
            var sut = CreateSut();
            var dto = TestFixtures.BuildSplitPaymentOrderDto();
            dto.PreBookingAmount = 20_000m;  // explicit override
            dto.PostEventAmount = 30_000m;
            dto.TotalAmount = 50_000m;

            decimal capturedPreBookingAmount = 0;
            _paymentStageRepo
                .Setup(r => r.InsertPaymentStageAsync(
                    It.IsAny<long>(), "PreBooking", It.IsAny<decimal>(), It.IsAny<decimal>(), null))
                .Callback<long, string, decimal, decimal, DateTime?>((_, _, _, amt, _) =>
                    capturedPreBookingAmount = amt)
                .ReturnsAsync(1L);

            // Act
            await sut.CreateOrderAsync(1, dto);

            // Assert — uses dto.PreBookingAmount (20,000), not 40% of 50,000 (20,000 happens to be same, so use clear value)
            capturedPreBookingAmount.Should().Be(20_000m);
        }

        [Fact]
        public async Task CreateOrder_SplitPayment_InsertPaymentCalledWithPreBookingStageType()
        {
            // Arrange
            SetupHappyPathMocks();
            var sut = CreateSut();
            var dto = TestFixtures.BuildSplitPaymentOrderDto();

            // Act
            await sut.CreateOrderAsync(1, dto);

            // Assert — final payment record uses "PreBooking" stage type for split
            _orderRepo.Verify(r => r.InsertOrderPaymentAsync(
                It.IsAny<long>(),
                dto.PaymentMethod,
                dto.TotalAmount,
                null,
                "PreBooking"),
                Times.Once);
        }

        // ── CreateOrderAsync — happy path & resilience ─────────────────────────

        [Fact]
        public async Task CreateOrder_Success_ReturnsPopulatedOrderDto()
        {
            // Arrange
            SetupHappyPathMocks(orderId: 999L);
            var sut = CreateSut();
            var dto = TestFixtures.BuildValidOrderDto();

            // Act
            var result = await sut.CreateOrderAsync(42, dto);

            // Assert
            result.Should().NotBeNull();
            result.OrderId.Should().Be(999L);
            result.OrderNumber.Should().Be("ORD-20260320-001");
        }

        [Fact]
        public async Task CreateOrder_InsertOrderFails_ThrowsInvalidOperation()
        {
            // Arrange
            _dbHelper.Setup(d => d.ExecuteAsync(It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
                     .ReturnsAsync(TestFixtures.ActiveCateringDataTable());
            _dbHelper.Setup(d => d.ExecuteAsync(
                    It.Is<string>(q => q.Contains("c_packageid")), It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(TestFixtures.CountOneDataTable());
            _orderRepo.Setup(r => r.CheckCateringAvailabilityAsync(It.IsAny<long>(), It.IsAny<DateTime>()))
                      .ReturnsAsync(true);
            _orderRepo.Setup(r => r.GenerateOrderNumberAsync())
                      .ReturnsAsync("ORD-FAIL-001");
            _orderRepo.Setup(r => r.InsertOrderAsync(It.IsAny<long>(), It.IsAny<CreateOrderDto>(), It.IsAny<string>()))
                      .ReturnsAsync(0); // ← failure

            var sut = CreateSut();
            var dto = TestFixtures.BuildValidOrderDto();

            // Act
            Func<Task> act = () => sut.CreateOrderAsync(1, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Failed to create order*");
        }

        [Fact]
        public async Task CreateOrder_NotificationThrows_OrderStillReturned()
        {
            // Arrange — notification throws but order is already created
            SetupHappyPathMocks(orderId: 777L);
            _notificationHelper
                .Setup(n => n.SendOrderNotificationAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<string?>(), It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ThrowsAsync(new Exception("SMTP failure"));

            var sut = CreateSut();
            var dto = TestFixtures.BuildValidOrderDto();

            // Act — should NOT throw despite notification failure
            var result = await sut.CreateOrderAsync(1, dto);

            // Assert
            result.Should().NotBeNull();
            result.OrderId.Should().Be(777L);
        }

        [Fact]
        public async Task CreateOrder_BankTransfer_ValidProof_FileStorageCalled()
        {
            // Arrange
            _dbHelper.Setup(d => d.ExecuteAsync(It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
                     .ReturnsAsync(TestFixtures.ActiveCateringDataTable());
            _dbHelper.Setup(d => d.ExecuteAsync(
                    It.Is<string>(q => q.Contains("c_packageid")), It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(TestFixtures.CountOneDataTable());
            _orderRepo.Setup(r => r.CheckCateringAvailabilityAsync(It.IsAny<long>(), It.IsAny<DateTime>()))
                      .ReturnsAsync(true);
            _orderRepo.Setup(r => r.GenerateOrderNumberAsync()).ReturnsAsync("ORD-BT-001");
            _orderRepo.Setup(r => r.InsertOrderAsync(It.IsAny<long>(), It.IsAny<CreateOrderDto>(), It.IsAny<string>()))
                      .ReturnsAsync(1L);
            _orderRepo.Setup(r => r.InsertOrderItemsAsync(1L, It.IsAny<List<CreateOrderItemDto>>())).ReturnsAsync(true);
            _orderRepo.Setup(r => r.InsertOrderPaymentAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string?>(), It.IsAny<string?>())).ReturnsAsync(true);
            _orderRepo.Setup(r => r.InsertOrderStatusHistoryAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<long?>())).ReturnsAsync(true);
            _orderRepo.Setup(r => r.GetOrderByIdAsync(1L, It.IsAny<long>())).ReturnsAsync(TestFixtures.BuildOrderDto(1L));
            _notificationHelper.Setup(n => n.SendOrderNotificationAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<string?>(), It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);
            _fileStorage.Setup(f => f.SaveFormFileAsync(
                    It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(),
                    It.IsAny<long>(), It.IsAny<string>(), It.IsAny<bool>(),
                    It.IsAny<string>(), It.IsAny<long?>()))
                .ReturnsAsync("/uploads/payment_proof.jpg");

            var proof = TestFixtures.BuildMockJpegFile("proof.jpg", 500 * 1024);
            var sut = CreateSut();
            var dto = TestFixtures.BuildBankTransferOrderDto(proof);

            // Act
            await sut.CreateOrderAsync(1, dto);

            // Assert
            _fileStorage.Verify(f => f.SaveFormFileAsync(
                It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(),
                It.IsAny<long>(), It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<long?>()),
                Times.Once);
        }

        // ── GetUserOrdersAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task GetUserOrders_DelegatesToRepository_WithCorrectArguments()
        {
            // Arrange
            _orderRepo.Setup(r => r.GetOrdersByUserIdAsync(42L, 2, 5))
                      .ReturnsAsync(new List<OrderListItemDto>());
            var sut = CreateSut();

            // Act
            await sut.GetUserOrdersAsync(42L, pageNumber: 2, pageSize: 5);

            // Assert
            _orderRepo.Verify(r => r.GetOrdersByUserIdAsync(42L, 2, 5), Times.Once);
        }

        [Fact]
        public async Task GetUserOrders_DefaultPagination_UsesPage1Size10()
        {
            // Arrange
            _orderRepo.Setup(r => r.GetOrdersByUserIdAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>()))
                      .ReturnsAsync(new List<OrderListItemDto>());
            var sut = CreateSut();

            // Act
            await sut.GetUserOrdersAsync(1L);

            // Assert
            _orderRepo.Verify(r => r.GetOrdersByUserIdAsync(1L, 1, 10), Times.Once);
        }

        // ── CancelOrderAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task CancelOrder_DelegatesToRepository_WithUserIdAndReason()
        {
            // Arrange
            _orderRepo.Setup(r => r.GetOrderByIdAsync(55L, 99L))
                      .ReturnsAsync(TestFixtures.BuildOrderDto(55L));
            _orderRepo.Setup(r => r.CancelOrderAsync(55L, 99L, "Changed plans"))
                      .ReturnsAsync(true);
            var sut = CreateSut();

            // Act
            await sut.CancelOrderAsync(99L, 55L, "Changed plans");

            // Assert
            _orderRepo.Verify(r => r.CancelOrderAsync(55L, 99L, "Changed plans"), Times.Once);
        }
    }
}
