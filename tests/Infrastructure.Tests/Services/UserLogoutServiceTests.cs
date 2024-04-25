using Infrastructure.Identity;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;

namespace Infrastructure.Tests.Services
{
    [TestFixture]
    public class UserLogoutServiceTests
    {
        private Mock<SignInManager<ApplicationUser>> _mockSignInManager;
        private UserLogoutService _userLogoutService;

        [SetUp]
        public void Setup()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
                new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null).Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
                null, null, null, null);

            _userLogoutService = new UserLogoutService(_mockSignInManager.Object);
        }

        [Test]
        public async Task LogoutAsync_ShouldInvokeSignOut()
        {
            _mockSignInManager.Setup(x => x.SignOutAsync()).Returns(Task.CompletedTask).Verifiable();

            await _userLogoutService.LogoutAsync();

            _mockSignInManager.Verify(x => x.SignOutAsync(), Times.Once);
        }
    }
}
