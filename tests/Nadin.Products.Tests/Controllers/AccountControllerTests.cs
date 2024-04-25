using Application.DTOs.Auth;
using Application.Interfaces.Auth;
using Domain.Models;
using Infrastructure.Identity;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Nadin.Products.Controllers;
using Nadin.Products.Responses;
using NUnit.Framework;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Nadin.Products.Tests.Controllers
{
    [TestFixture]
    public class AccountControllerTests
    {
        private Mock<IUserRegistrationService> _mockRegistrationService;
        private Mock<IUserLoginService> _mockLoginService;
        private Mock<IUserLogoutService> _mockLogoutService;
        private Mock<UserManager<ApplicationUser>> _mockUserManager;
        private Mock<IOptions<JwtSettings>> _mockJwtSettings;
        private AccountController _controller;

        [SetUp]
        public void Setup()
        {
            _mockRegistrationService = new Mock<IUserRegistrationService>();
            _mockLoginService = new Mock<IUserLoginService>();
            _mockLogoutService = new Mock<IUserLogoutService>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            _mockJwtSettings = new Mock<IOptions<JwtSettings>>();
            _mockJwtSettings.Setup(j => j.Value).Returns(new JwtSettings
            {
                Secret = "SimpleSecretKeyForDevelopment4321!",
                Issuer = "https://localhost:44394",
                Audience = "https://localhost:44394",
                ExpirationInMinutes = 30
            });

            var jwtService = new JwtService(_mockJwtSettings.Object);

            _controller = new AccountController(
                _mockRegistrationService.Object,
                _mockLoginService.Object,
                _mockLogoutService.Object,
                _mockUserManager.Object,
                jwtService); // Mocked JwtSettings and Used the actual JwtService
        }

        #region Register
        [Test]
        public async Task Register_ReturnsOk_WhenRegistrationIsSuccessful()
        {
            var model = new RegisterDTO { UserName = "Username", Email = "user@example.com", FullName = "Full Name", Password = "12345678" };
            _mockRegistrationService.Setup(s => s.RegisterUserAsync(model)).ReturnsAsync(IdentityResult.Success);

            var result = await _controller.Register(model);

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            var apiResponse = okResult.Value as APIResponse;
            Assert.That(apiResponse.IsSuccess);
            Assert.That(apiResponse.Result, Is.EqualTo("Registered successfully."));
        }

        [Test]
        public async Task Register_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            var model = new RegisterDTO { Email = "user@example.com", FullName = "Full Name", Password = "12345678" };
            _controller.ModelState.AddModelError("UserName", "Username is required");

            var result = await _controller.Register(model);

            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result.Result as BadRequestObjectResult;
            var apiResponse = badRequestResult.Value as APIResponse;
            Assert.That(apiResponse.IsSuccess, Is.False);
            Assert.That(apiResponse.ErrorMessages, Contains.Item("Username is required"));
        }

        [Test]
        public async Task Register_ReturnsBadRequest_WhenRegistrationFails()
        {
            var model = new RegisterDTO { UserName = "User Name", Email = "user.com", FullName = "Full Name", Password = "12" };
            _mockRegistrationService.Setup(s => s.RegisterUserAsync(model))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Registration failed" }));

            var result = await _controller.Register(model);

            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result.Result as BadRequestObjectResult;
            var apiResponse = badRequestResult.Value as APIResponse;
            Assert.That(apiResponse.IsSuccess, Is.False);
            Assert.That(apiResponse.ErrorMessages, Contains.Item("Registration failed"));
        }

        [Test]
        public async Task Register_ReturnsInternalServerError_WhenExceptionIsThrown()
        {
            var model = new RegisterDTO { UserName = "Username", Email = "user@example.com", FullName = "Full Name", Password = "12345678" };
            _mockRegistrationService.Setup(s => s.RegisterUserAsync(model))
                .ThrowsAsync(new Exception("Internal server error"));

            var result = await _controller.Register(model);

            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            var internalServerErrorResult = result.Result as ObjectResult;
            var apiResponse = internalServerErrorResult.Value as APIResponse;
            Assert.That(apiResponse.IsSuccess, Is.False);
            Assert.That(internalServerErrorResult.StatusCode, Is.EqualTo(500));
            Assert.That(apiResponse.ErrorMessages, Contains.Item("Internal server error"));
        }
        #endregion

        #region Login
        [Test]
        public async Task Login_ReturnsOk_WhenLoginIsSuccessful()
        {
            var model = new LoginDTO { UserName = "Username", Password = "12345678", RememberMe = true };
            _mockLoginService.Setup(s => s.LoginAsync(model))
                .ReturnsAsync(SignInResult.Success);
            _mockUserManager.Setup(um => um.FindByNameAsync(model.UserName))
                .ReturnsAsync(new ApplicationUser { UserName = "Username", Email = "user@example.com", FullName = "Full Name" });

            var result = await _controller.Login(model);

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            var apiResponse = okResult.Value as APIResponse;
            Assert.That(apiResponse.IsSuccess, Is.True);
            var resultData = apiResponse.Result;
            var tokenProperty = resultData.GetType().GetProperty("Token");
            Assert.That(tokenProperty, Is.Not.Null, "Token property not found.");
            var tokenValue = tokenProperty.GetValue(resultData);
            Assert.That(tokenValue, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public async Task Login_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            var model = new LoginDTO { UserName = "Username", RememberMe = true };
            _controller.ModelState.AddModelError("Password", "Password is required");

            var result = await _controller.Login(model);

            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result.Result as BadRequestObjectResult;
            var apiResponse = badRequestResult.Value as APIResponse;
            Assert.That(apiResponse.IsSuccess, Is.False);
            Assert.That(apiResponse.ErrorMessages, Contains.Item("Password is required"));
        }

        [Test]
        public async Task Login_ReturnsBadRequest_WhenLoginFails()
        {
            var model = new LoginDTO { UserName = "Username", Password = "12345678", RememberMe = true };
            _mockLoginService.Setup(s => s.LoginAsync(model))
                .ReturnsAsync(SignInResult.Failed);

            var result = await _controller.Login(model);

            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result.Result as BadRequestObjectResult;
            var apiResponse = badRequestResult.Value as APIResponse;
            Assert.That(apiResponse.IsSuccess, Is.False);
            Assert.That(apiResponse.ErrorMessages, Contains.Item("There was an error while trying to Login."));
        }

        [Test]
        public async Task Login_ReturnsInternalServerError_WhenExceptionIsThrown()
        {
            var model = new LoginDTO { UserName = "Username", Password = "12345678", RememberMe = true };
            _mockLoginService.Setup(s => s.LoginAsync(model))
                .ThrowsAsync(new Exception("Internal server error"));

            var result = await _controller.Login(model);

            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            var internalServerErrorResult = result.Result as ObjectResult;
            var apiResponse = internalServerErrorResult.Value as APIResponse;
            Assert.That(apiResponse.IsSuccess, Is.False);
            Assert.That(internalServerErrorResult.StatusCode, Is.EqualTo(500));
            Assert.That(apiResponse.ErrorMessages, Contains.Item("Internal server error"));
        }
        #endregion

        #region Logout
        [Test]
        public async Task Logout_ReturnsOk_WhenLogoutIsSuccessful()
        {
            _mockLogoutService.Setup(s => s.LogoutAsync()).Returns(Task.CompletedTask);

            var result = await _controller.Logout();

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            var apiResponse = okResult.Value as APIResponse;
            Assert.That(apiResponse.IsSuccess, Is.True);
            Assert.That(apiResponse.Result, Is.EqualTo("Logged out successfully."));
        }

        [Test]
        public async Task Logout_ReturnsInternalServerError_WhenExceptionIsThrown()
        {
            _mockLogoutService.Setup(s => s.LogoutAsync()).ThrowsAsync(new Exception("Internal server error"));

            var result = await _controller.Logout();

            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            var internalServerErrorResult = result.Result as ObjectResult;
            var apiResponse = internalServerErrorResult.Value as APIResponse;
            Assert.That(apiResponse.IsSuccess, Is.False);
            Assert.That(internalServerErrorResult.StatusCode, Is.EqualTo(500));
            Assert.That(apiResponse.ErrorMessages, Contains.Item("Internal server error"));
        }
        #endregion
    }
}
