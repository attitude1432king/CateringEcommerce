using CateringEcommerce.API.Controllers.Admin;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Security.Claims;

namespace CateringEcommerce.Tests.Unit.API.Controllers
{
    /// <summary>
    /// Unit tests for AdminAuthController.
    /// Verifies login credential validation, account lockout, and admin session endpoints.
    /// </summary>
    public class AdminAuthControllerTests
    {
        private readonly Mock<IAdminAuthRepository> _adminAuthRepo = new();
        private readonly Mock<IRBACRepository> _rbacRepo = new();
        private readonly Mock<ITokenService> _tokenService = new();

        private AdminAuthController CreateSut(long? adminIdClaim = null)
        {
            var controller = new AdminAuthController(
                _adminAuthRepo.Object,
                _rbacRepo.Object,
                _tokenService.Object,
                NullLogger<AdminAuthController>.Instance);

            // Set up DefaultHttpContext (required for Response.Cookies.Append in Login)
            var httpContext = new DefaultHttpContext();

            if (adminIdClaim.HasValue)
            {
                var claims = new[] { new Claim(ClaimTypes.NameIdentifier, adminIdClaim.Value.ToString()) };
                var identity = new ClaimsIdentity(claims, "test");
                httpContext.User = new ClaimsPrincipal(identity);
            }

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

        // ── POST /api/admin/auth/login ─────────────────────────────────────

        [Fact]
        public void Login_AccountLocked_ReturnsFailureWithResultFalse()
        {
            // Arrange
            _adminAuthRepo.Setup(r => r.IsAccountLocked("admin")).Returns(true);
            var controller = CreateSut();
            var request = new AdminLoginRequest { Username = "admin", Password = "any" };

            // Act
            var result = controller.Login(request);

            // Assert
            var obj = result.Should().BeAssignableTo<ObjectResult>().Subject;
            var body = obj.Value!;
            body.GetType().GetProperty("result")!.GetValue(body).Should().Be(false);
        }

        [Fact]
        public void Login_AdminNotFound_ReturnsFailureAndIncrementsAttempts()
        {
            // Arrange
            _adminAuthRepo.Setup(r => r.IsAccountLocked(It.IsAny<string>())).Returns(false);
            _adminAuthRepo.Setup(r => r.GetAdminByUsername("unknown")).Returns((AdminModel?)null);
            var controller = CreateSut();
            var request = new AdminLoginRequest { Username = "unknown", Password = "any" };

            // Act
            var result = controller.Login(request);

            // Assert — result=false
            var obj = result.Should().BeAssignableTo<ObjectResult>().Subject;
            var body = obj.Value!;
            body.GetType().GetProperty("result")!.GetValue(body).Should().Be(false);

            // Verify failed attempt was recorded
            _adminAuthRepo.Verify(r => r.IncrementFailedLoginAttempts("unknown"), Times.Once);
        }

        [Fact]
        public void Login_ValidCredentials_Returns200WithAdminData()
        {
            // Arrange — use real HashHelper.HashPassword so VerifyPassword succeeds
            const string TestPassword = "Admin@Test!1";
            var admin = new AdminModel
            {
                AdminId = 1,
                Username = "admin",
                Email = "admin@test.com",
                FullName = "Test Admin",
                Role = "SuperAdmin",
                PasswordHash = HashHelper.HashPassword(TestPassword),
                FailedLoginAttempts = 0
            };

            _adminAuthRepo.Setup(r => r.IsAccountLocked("admin")).Returns(false);
            _adminAuthRepo.Setup(r => r.GetAdminByUsername("admin")).Returns(admin);
            _adminAuthRepo.Setup(r => r.IsTemporaryPassword(1)).Returns(false);
            _tokenService.Setup(t => t.GenerateToken(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>()))
                .Returns("test-jwt-token");

            var controller = CreateSut();
            var request = new AdminLoginRequest { Username = "admin", Password = TestPassword };

            // Act
            var result = controller.Login(request);

            // Assert
            var ok = result.Should().BeAssignableTo<ObjectResult>().Subject;
            var body = ok.Value!;
            body.GetType().GetProperty("result")!.GetValue(body).Should().Be(true);

            // Verify session cleanup happened
            _adminAuthRepo.Verify(r => r.ResetFailedLoginAttempts(1), Times.Once);
            _adminAuthRepo.Verify(r => r.UpdateLastLogin(1), Times.Once);
        }

        [Fact]
        public void Login_WrongPassword_ReturnsFailureAndIncrementsAttempts()
        {
            // Arrange
            var admin = new AdminModel
            {
                AdminId = 1,
                Username = "admin",
                PasswordHash = HashHelper.HashPassword("RealPass@1!"),
                FailedLoginAttempts = 0
            };
            var adminAfterFail = new AdminModel { FailedLoginAttempts = 1 };

            _adminAuthRepo.Setup(r => r.IsAccountLocked("admin")).Returns(false);
            _adminAuthRepo.Setup(r => r.GetAdminByUsername("admin"))
                .Returns(admin);
            // Returns updated admin (with 1 failed attempt) after incrementing
            _adminAuthRepo.SetupSequence(r => r.GetAdminByUsername("admin"))
                .Returns(admin)
                .Returns(adminAfterFail);

            var controller = CreateSut();
            var request = new AdminLoginRequest { Username = "admin", Password = "WrongPass@1!" };

            // Act
            var result = controller.Login(request);

            // Assert
            var obj = result.Should().BeAssignableTo<ObjectResult>().Subject;
            var body = obj.Value!;
            body.GetType().GetProperty("result")!.GetValue(body).Should().Be(false);
            _adminAuthRepo.Verify(r => r.IncrementFailedLoginAttempts("admin"), Times.Once);
        }

        // ── GET /api/admin/auth/me ─────────────────────────────────────────

        [Fact]
        public void GetCurrentAdmin_ValidClaim_Returns200WithAdminData()
        {
            // Arrange
            var admin = new AdminModel { AdminId = 5, Username = "admin5" };
            _adminAuthRepo.Setup(r => r.GetAdminById(5L)).Returns(admin);
            var controller = CreateSut(adminIdClaim: 5L);

            // Act
            var result = controller.GetCurrentAdmin();

            // Assert
            var obj = result.Should().BeAssignableTo<ObjectResult>().Subject;
            var body = obj.Value!;
            body.GetType().GetProperty("result")!.GetValue(body).Should().Be(true);
        }

        [Fact]
        public void GetCurrentAdmin_AdminNotFoundInDb_ReturnsFailure()
        {
            // Arrange
            _adminAuthRepo.Setup(r => r.GetAdminById(It.IsAny<long>())).Returns((AdminModel?)null);
            var controller = CreateSut(adminIdClaim: 999L);

            // Act
            var result = controller.GetCurrentAdmin();

            // Assert
            var obj = result.Should().BeAssignableTo<ObjectResult>().Subject;
            var body = obj.Value!;
            body.GetType().GetProperty("result")!.GetValue(body).Should().Be(false);
        }
    }
}
