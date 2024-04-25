using Application.DTOs.Auth;
using Infrastructure.Identity;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;

namespace Infrastructure.Tests.Services
{
    [TestFixture]
    public class UserLoginServiceTests
    {
        private Mock<SignInManager<ApplicationUser>> _mockSignInManager;
        private UserLoginService _userLoginService;
        private LoginDTO _loginDto;

        [SetUp]
        public void Setup()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
                new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null).Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
                null, null, null, null);

            _userLoginService = new UserLoginService(_mockSignInManager.Object);

            _loginDto = new LoginDTO
            {
                UserName = "testUser",
                Password = "testPassword",
                RememberMe = false
            };
        }

        [Test]
        public async Task LoginAsync_ValidCredentials_ShouldReturnSuccess()
        {
            _mockSignInManager.Setup(x => x.PasswordSignInAsync(_loginDto.UserName, _loginDto.Password, _loginDto.RememberMe, false))
                .ReturnsAsync(SignInResult.Success);

            var result = await _userLoginService.LoginAsync(_loginDto);

            Assert.That(result, Is.EqualTo(SignInResult.Success));
        }

        [Test]
        public async Task LoginAsync_InvalidCredentials_ShouldReturnFailed()
        {
            _mockSignInManager.Setup(x => x.PasswordSignInAsync(_loginDto.UserName, _loginDto.Password, _loginDto.RememberMe, false))
                .ReturnsAsync(SignInResult.Failed);

            var result = await _userLoginService.LoginAsync(_loginDto);

            Assert.That(result, Is.EqualTo(SignInResult.Failed));
        }
    }
}
