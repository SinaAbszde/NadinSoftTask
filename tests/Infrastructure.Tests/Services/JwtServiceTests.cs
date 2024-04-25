using Domain.Models;
using Infrastructure.Identity;
using Infrastructure.Services;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System.IdentityModel.Tokens.Jwt;

namespace Infrastructure.Tests.Services
{
    [TestFixture]
    public class JwtServiceTests
    {
        private JwtService _jwtService;
        private Mock<IOptions<JwtSettings>> _mockJwtSettings;
        private ApplicationUser _testUser;

        [SetUp]
        public void Setup()
        {
            // Mocking JwtSettings
            _mockJwtSettings = new Mock<IOptions<JwtSettings>>();
            _mockJwtSettings.Setup(j => j.Value).Returns(new JwtSettings
            {
                Secret = "SimpleSecretKeyForDevelopment4321!",
                Issuer = "https://localhost:44394",
                Audience = "https://localhost:44394",
                ExpirationInMinutes = 30
            });

            // Initialize JwtService with mocked settings
            _jwtService = new JwtService(_mockJwtSettings.Object);

            // Setup a test user
            _testUser = new ApplicationUser
            {
                UserName = "TestUser",
                Id = "TestId"
            };
        }

        [Test]
        public void GenerateToken_ShouldReturnValidToken()
        {
            // Act
            var token = _jwtService.GenerateToken(_testUser);

            // Assert
            Assert.That(token, Is.Not.Null.Or.Empty);
            Assert.That(() =>
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
                return jsonToken != null && jsonToken.ValidTo > DateTime.UtcNow;
            }, Is.True.After(200, 10000), "The token should be valid and not expired.");
        }
    }
}
