using Application.DTOs.Auth;
using Infrastructure.Identity;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;

namespace Infrastructure.Tests.Services
{
    [TestFixture]
    public class UserRegistrationServiceTests
    {
        private Mock<UserManager<ApplicationUser>> _mockUserManager;
        private UserRegistrationService _userRegistrationService;
        private RegisterDTO _registerDto;

        [SetUp]
        public void Setup()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object,
                null, null, null, null, null, null, null, null);

            _userRegistrationService = new UserRegistrationService(_mockUserManager.Object);

            _registerDto = new RegisterDTO
            {
                UserName = "newUser",
                Email = "newUser@example.com",
                Password = "TestPassword123!",
                FullName = "New User"
            };
        }

        [Test]
        public async Task RegisterUserAsync_WithNonExistingUser_ShouldReturnSuccess()
        {
            _mockUserManager.Setup(x => x.FindByNameAsync(_registerDto.UserName))
                .ReturnsAsync((ApplicationUser)null);
            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), _registerDto.Password))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _userRegistrationService.RegisterUserAsync(_registerDto);

            Assert.That(result, Is.EqualTo(IdentityResult.Success));
        }

        [Test]
        public async Task RegisterUserAsync_WithExistingUser_ShouldReturnFailed()
        {
            _mockUserManager.Setup(x => x.FindByNameAsync(_registerDto.UserName))
                .ReturnsAsync(new ApplicationUser());
            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), _registerDto.Password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Test error" }));

            var result = await _userRegistrationService.RegisterUserAsync(_registerDto);

            Assert.That(result, Is.Not.EqualTo(IdentityResult.Success));
            Assert.That(result.Errors, Has.Some.Matches<IdentityError>(e => e.Description.Contains("Username 'newUser' is already taken.")));
        }
    }
}
