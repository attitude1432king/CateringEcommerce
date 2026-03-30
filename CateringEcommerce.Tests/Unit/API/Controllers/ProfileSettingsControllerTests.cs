using CateringEcommerce.API.Controllers.User;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.User;
using CateringEcommerce.Domain.Models.User;
using CateringEcommerce.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CateringEcommerce.Tests.Unit.API.Controllers
{
    /// <summary>
    /// Unit tests for ProfileSettingsController.
    /// </summary>
    public class ProfileSettingsControllerTests
    {
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly Mock<IFileStorageService> _fileStorage = new();
        private readonly Mock<IProfileSetting> _profileSetting = new();
        private readonly Mock<IUserRepository> _userRepository = new();

        private ProfileSettingsController CreateSut(long userId = 42L)
        {
            _currentUser.Setup(u => u.UserId).Returns(userId);
            return new ProfileSettingsController(
                _currentUser.Object,
                _fileStorage.Object,
                _profileSetting.Object,
                _userRepository.Object);
        }

        // ── GET GetUserProfile ────────────────────────────────────────────────

        [Fact]
        public void GetUserProfile_AuthenticatedUser_Returns200WithProfile()
        {
            // Arrange
            var profile = new UserModel { PkID = 42, Phone = "9876543210" };
            _userRepository.Setup(r => r.GetUserDetails(42L)).Returns(profile);
            var controller = CreateSut();

            // Act
            var result = controller.GetUserProfile();

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().Be(profile);
        }

        [Fact]
        public void GetUserProfile_RepositoryThrows_Returns500()
        {
            // Arrange
            _userRepository.Setup(r => r.GetUserDetails(It.IsAny<long>()))
                           .Throws(new Exception("DB error"));
            var controller = CreateSut();

            // Act
            var result = controller.GetUserProfile();

            // Assert
            var status = result.Should().BeOfType<ObjectResult>().Subject;
            status.StatusCode.Should().Be(500);
        }

        // ── POST UpdateProfile ────────────────────────────────────────────────

        [Fact]
        public async Task UpdateProfile_UserIdZero_ReturnsBadRequestOrFailure()
        {
            // Arrange — userId = 0 means invalid/unauthenticated
            var controller = CreateSut(userId: 0L);
            var request = new UserModel { StateID = 1, CityID = 2, Phone = "9876543210" };

            // Act
            var result = await controller.UpdateProfileDetails(request);

            // Assert — controller returns ApiResponseHelper.Failure → OkObjectResult with result=false
            var obj = result.Should().BeAssignableTo<ObjectResult>().Subject;
            var body = obj.Value!;
            body.GetType().GetProperty("result")!.GetValue(body).Should().Be(false);
        }

        [Fact]
        public async Task UpdateProfile_NoValidFields_ReturnsBadRequest()
        {
            // Arrange — empty UserModel has no valid update fields (StateID=0, CityID=0, Description=null)
            var controller = CreateSut();
            var request = new UserModel { Phone = "9876543210" }; // all update fields default = no data

            // Act
            var result = await controller.UpdateProfileDetails(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateProfile_ValidState_Returns200WithMessage()
        {
            // Arrange
            var updatedProfile = new UserModel { PkID = 42, StateID = 5, Phone = "9876543210" };
            _profileSetting.Setup(p => p.UpdateUserDetails(42L, It.IsAny<Dictionary<string, string>>()))
                           .Returns(Task.CompletedTask);
            _userRepository.Setup(r => r.GetUserDetails(42L)).Returns(updatedProfile);
            var controller = CreateSut();
            var request = new UserModel { StateID = 5, Phone = "9876543210" };

            // Act
            var result = await controller.UpdateProfileDetails(request);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var body = ok.Value!;
            body.GetType().GetProperty("message")!.GetValue(body)!.ToString()
                .Should().Contain("updated");
        }

        // ── POST UploadProfilePhoto ───────────────────────────────────────────

        [Fact]
        public async Task UploadProfilePhoto_UserIdZero_ReturnsFailure()
        {
            // Arrange
            var controller = CreateSut(userId: 0L);
            var file = TestFixtures.BuildMockJpegFile();

            // Act
            var result = await controller.UploadProfilePhoto(file);

            // Assert
            var obj = result.Should().BeAssignableTo<ObjectResult>().Subject;
            var body = obj.Value!;
            body.GetType().GetProperty("result")!.GetValue(body).Should().Be(false);
        }

        [Fact]
        public async Task UploadProfilePhoto_NullFile_ReturnsBadRequest()
        {
            // Arrange
            var controller = CreateSut();

            // Act
            var result = await controller.UploadProfilePhoto(null!);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();

            // FileStorage should NOT be called
            _fileStorage.Verify(
                s => s.SaveRoleBaseFormFileAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(),
                    It.IsAny<long>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }
    }
}
