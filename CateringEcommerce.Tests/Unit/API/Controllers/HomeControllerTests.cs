using CateringEcommerce.API.Controllers.User;
using CateringEcommerce.Domain.Interfaces.User;
using CateringEcommerce.Domain.Models.User;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CateringEcommerce.Tests.Unit.API.Controllers
{
    /// <summary>
    /// Unit tests for HomeController.
    /// Verifies HTTP response shapes and delegation to IHomeService
    /// without making any real DB or HTTP calls.
    /// </summary>
    public class HomeControllerTests
    {
        private readonly Mock<IHomeService> _homeService = new();
        private readonly Mock<IConfiguration> _config = new();

        private HomeController CreateSut()
        {
            // GetConnectionString("DefaultConnection") is an extension that calls
            // configuration.GetSection("ConnectionStrings")["DefaultConnection"]
            var connSection = new Mock<IConfigurationSection>();
            connSection.Setup(s => s["DefaultConnection"]).Returns("Server=test;Database=test");
            _config.Setup(c => c.GetSection("ConnectionStrings")).Returns(connSection.Object);

            return new HomeController(
                NullLogger<HomeController>.Instance,
                _homeService.Object,
                _config.Object);
        }

        // ── GET /api/User/Home/CateringList ────────────────────────────────────

        [Fact]
        public async Task GetVerifiedCateringList_WithCity_Returns200WithSuccessTrue()
        {
            // Arrange
            var list = new List<CateringBusinessListDto>
            {
                new CateringBusinessListDto { Id = 1, CateringName = "Test Caterers" }
            };
            _homeService.Setup(s => s.GetVerifiedCateringListAsync("Mumbai")).ReturnsAsync(list);
            var controller = CreateSut();

            // Act
            var result = await controller.GetVerifiedCateringListAsync("Mumbai");

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var body = ok.Value!;
            body.GetType().GetProperty("success")!.GetValue(body).Should().Be(true);
        }

        [Fact]
        public async Task GetVerifiedCateringList_ServiceThrows_Returns500()
        {
            // Arrange
            _homeService.Setup(s => s.GetVerifiedCateringListAsync(It.IsAny<string>()))
                        .ThrowsAsync(new Exception("DB down"));
            var controller = CreateSut();

            // Act
            var result = await controller.GetVerifiedCateringListAsync("Mumbai");

            // Assert
            var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(500);
        }

        // ── GET /api/User/Home/FeaturedCaterers ───────────────────────────────

        [Fact]
        public async Task GetFeaturedCaterers_Returns200WithSuccessTrue()
        {
            // Arrange
            _homeService.Setup(s => s.GetFeaturedCaterersAsync())
                        .ReturnsAsync(new List<FeaturedCatererDto>());
            var controller = CreateSut();

            // Act
            var result = await controller.GetFeaturedCaterersAsync();

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var body = ok.Value!;
            body.GetType().GetProperty("success")!.GetValue(body).Should().Be(true);
        }

        // ── GET /api/User/Home/Catering/{id}/Detail ───────────────────────────

        [Fact]
        public async Task GetCateringDetail_WhenFound_Returns200()
        {
            // Arrange
            var detail = new CateringDetailDto { CateringId = 7 };
            _homeService.Setup(s => s.GetCateringDetailForBrowsingAsync(7L)).ReturnsAsync(detail);
            var controller = CreateSut();

            // Act
            var result = await controller.GetCateringDetailAsync(7L);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var body = ok.Value!;
            body.GetType().GetProperty("success")!.GetValue(body).Should().Be(true);
        }

        [Fact]
        public async Task GetCateringDetail_WhenNotFound_Returns404()
        {
            // Arrange
            _homeService.Setup(s => s.GetCateringDetailForBrowsingAsync(It.IsAny<long>()))
                        .ReturnsAsync((CateringDetailDto?)null);
            var controller = CreateSut();

            // Act
            var result = await controller.GetCateringDetailAsync(999L);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        // ── GET /api/User/Home/Catering/{id}/Packages ─────────────────────────

        [Fact]
        public async Task GetCateringPackages_ValidId_Returns200WithList()
        {
            // Arrange
            var packages = new List<CateringPackageDto>
            {
                new CateringPackageDto { PackageId = 10, Name = "Premium" }
            };
            _homeService.Setup(s => s.GetCateringPackagesAsync(1L)).ReturnsAsync(packages);
            var controller = CreateSut();

            // Act
            var result = await controller.GetCateringPackagesAsync(1L);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var body = ok.Value!;
            body.GetType().GetProperty("success")!.GetValue(body).Should().Be(true);
            ((int)body.GetType().GetProperty("count")!.GetValue(body)!).Should().Be(1);
        }

        [Fact]
        public async Task GetCateringPackages_ServiceThrowsArgumentException_Returns400()
        {
            // Arrange
            _homeService.Setup(s => s.GetCateringPackagesAsync(It.IsAny<long>()))
                        .ThrowsAsync(new ArgumentException("Invalid catering ID."));
            var controller = CreateSut();

            // Act
            var result = await controller.GetCateringPackagesAsync(0L);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        // ── GET /api/User/Home/Search ──────────────────────────────────────────

        [Fact]
        public async Task SearchCaterings_WithKeyword_Returns200WithPagination()
        {
            // Arrange
            var searchResult = new CateringSearchResultDto
            {
                TotalCount = 5,
                PageNumber = 1,
                PageSize = 20,
                Results = new List<CateringBusinessListDto>()
            };
            _homeService.Setup(s => s.SearchCateringsAsync(It.IsAny<CateringSearchFilterDto>()))
                        .ReturnsAsync(searchResult);
            var controller = CreateSut();

            // Act
            var result = await controller.SearchCateringsAsync(keyword: "biryani");

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var body = ok.Value!;
            body.GetType().GetProperty("success")!.GetValue(body).Should().Be(true);
        }
    }
}
