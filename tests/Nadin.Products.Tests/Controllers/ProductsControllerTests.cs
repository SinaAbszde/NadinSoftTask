using Application.DTOs;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Models;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Nadin.Products.Controllers;
using Nadin.Products.Responses;
using Nadin.Products.Tests.Helpers;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;

namespace Nadin.Products.Tests.Controllers
{
    [TestFixture]
    public class ProductsControllerTests
    {
        private Mock<IProductRepository> _mockRepo;
        private Mock<IMapper> _mockMapper;
        private Mock<UserManager<ApplicationUser>> _mockUserManager;
        private ProductsController _controller;
        private List<Product> _products;
        private List<ProductDTO> _productDTOs;
        private ApplicationUser _testUser;

        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<IProductRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

            _controller = new ProductsController(_mockUserManager.Object, _mockRepo.Object, _mockMapper.Object);

            _testUser = new ApplicationUser { UserName = "testuser", Id = "userid" };
            _mockUserManager.Setup(um => um.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(_testUser);

            _products = new List<Product> { TestData.GenerateProduct(), TestData.GenerateProduct() };

            _productDTOs = _products.Select(p => new ProductDTO
            {
                ID = p.ID,
                Name = p.Name,
                ProduceDate = p.ProduceDate,
                ManufacturePhone = p.ManufacturePhone,
                ManufactureEmail = p.ManufactureEmail,
                IsAvailable = p.IsAvailable
            }).ToList();

            _mockRepo.Setup(repo => repo.GetAllAsync(It.IsAny<Expression<Func<Product, bool>>>())).ReturnsAsync(_products);

            _mockMapper.Setup(mapper => mapper.Map<List<ProductDTO>>(It.IsAny<IEnumerable<Product>>()))
                       .Returns(_productDTOs);
        }

        #region GetAllProducts
        [Test]
        public async Task GetAllProducts_ReturnsAllProducts_WhenCalledWithValidUser()
        {
            var result = await _controller.GetAllProducts("testuser");

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult.Value, Is.InstanceOf<APIResponse>());
            var apiResponse = okResult.Value as APIResponse;
            Assert.That(apiResponse.IsSuccess, Is.True);
            Assert.That(apiResponse.Result, Is.InstanceOf<List<ProductDTO>>());
            Assert.That(((List<ProductDTO>)apiResponse.Result).Count, Is.EqualTo(_productDTOs.Count));
            Assert.That(apiResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(apiResponse.ErrorMessages, Is.Null);
        }

        [Test]
        public async Task GetAllProducts_ReturnsBadRequest_WhenCalledWithInvalidUser()
        {
            _mockUserManager.Setup(um => um.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);

            var result = await _controller.GetAllProducts("invaliduser");

            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result.Result as BadRequestObjectResult;
            var apiResponse = badRequestResult.Value as APIResponse;
            Assert.That(apiResponse.IsSuccess, Is.False);
            Assert.That(apiResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(apiResponse.ErrorMessages, Contains.Item("Invalid Username!"));
        }
        #endregion

        #region GetProduct
        [Test]
        public async Task GetProduct_ReturnsBadRequest_WhenIdIsZero()
        {
            var result = await _controller.GetProduct(0);

            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.That(badRequestResult.Value, Is.InstanceOf<APIResponse>());
            var apiResponse = badRequestResult.Value as APIResponse;
            Assert.That(apiResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task GetProduct_ReturnsNotFound_WhenProductDoesNotExist()
        {
            var productId = 999;
            _mockRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<bool>())).ReturnsAsync((Product)null);

            var result = await _controller.GetProduct(productId);

            Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.That(notFoundResult.Value, Is.InstanceOf<APIResponse>());
            var apiResponse = notFoundResult.Value as APIResponse;
            Assert.That(apiResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task GetProduct_ReturnsProduct_WhenProductExists()
        {
            var productId = 1; // Use an ID that exists in your test data
            var product = _products.First(p => p.ID == productId);
            var productDTO = _productDTOs.First(p => p.ID == productId);
            _mockRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<bool>())).ReturnsAsync(product);
            _mockMapper.Setup(mapper => mapper.Map<ProductDTO>(It.IsAny<Product>())).Returns(productDTO);

            var result = await _controller.GetProduct(productId);

            Assert.That(result.Value, Is.InstanceOf<APIResponse>());
            var apiResponse = result.Value as APIResponse;
            Assert.That(apiResponse.IsSuccess, Is.True);
            Assert.That(apiResponse.Result, Is.EqualTo(productDTO));
            Assert.That(apiResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(apiResponse.ErrorMessages, Is.Null);
        }

        [Test]
        public async Task GetProduct_ReturnsInternalServerError_WhenExceptionIsThrown()
        {
            var productId = 1;
            _mockRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<bool>())).ThrowsAsync(new Exception("Test exception"));

            var result = await _controller.GetProduct(productId);

            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            var internalServerErrorResult = result.Result as ObjectResult;
            Assert.That(internalServerErrorResult.StatusCode, Is.EqualTo(500));
            var apiResponse = internalServerErrorResult.Value as APIResponse;
            Assert.That(apiResponse.IsSuccess, Is.False);
            Assert.That(apiResponse.ErrorMessages, Contains.Item("Test exception"));
        }
        #endregion

        #region CreateProduct
        [Test]
        public async Task CreateProduct_ReturnsCreatedProduct_WhenProductIsCreated()
        {
            var createDTO = TestData.GenerateProducCreatetDTO();
            var createdProduct = new Product
            {
                ID = 3,
                Name = createDTO.Name,
                ProduceDate = DateOnly.FromDateTime(DateTime.Now),
                ManufacturePhone = createDTO.ManufacturePhone,
                ManufactureEmail = createDTO.ManufactureEmail,
                IsAvailable = createDTO.IsAvailable,
                UserId = _testUser.Id
            };
            var createdProductDTO = new ProductDTO
            {
                ID = createdProduct.ID,
                ProduceDate = createdProduct.ProduceDate,
                Name = createdProduct.Name,
                ManufactureEmail = createdProduct.ManufactureEmail,
                ManufacturePhone = createdProduct.ManufacturePhone,
                IsAvailable = createdProduct.IsAvailable
            };

            _mockMapper.Setup(mapper => mapper.Map<Product>(It.IsAny<ProductCreateDTO>())).Returns(createdProduct);
            _mockMapper.Setup(mapper => mapper.Map<ProductDTO>(It.IsAny<Product>())).Returns(createdProductDTO);
            _mockRepo.Setup(repo => repo.CreateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(_testUser.Id);

            var result = await _controller.CreateProduct(createDTO);

            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            var objectResult = result.Result as ObjectResult;
            Assert.That(objectResult, Is.Not.Null);
            Assert.That(objectResult.StatusCode, Is.EqualTo((int)HttpStatusCode.Created));
            var apiResponse = objectResult.Value as APIResponse;
            Assert.That(apiResponse, Is.Not.Null);
            Assert.That(apiResponse.IsSuccess, Is.True);
            Assert.That(apiResponse.Result, Is.EqualTo(createdProductDTO));
            Assert.That(apiResponse.ErrorMessages, Is.Null);
        }

        [Test]
        public async Task CreateProduct_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("Name", "The Name field is required.");

            var result = await _controller.CreateProduct(new ProductCreateDTO());

            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result.Result as BadRequestObjectResult;
            var apiResponse = badRequestResult.Value as APIResponse;
            Assert.That(apiResponse.IsSuccess, Is.False);
            Assert.That(apiResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(apiResponse.ErrorMessages, Contains.Item("The Name field is required."));
        }
        #endregion

        #region DeleteProduct
        [Test]
        public async Task DeleteProduct_ReturnsNoContent_WhenProductIsDeletedAndUserIsOwner()
        {
            // Arrange
            var productId = 1;
            var product = _products.First(p => p.ID == productId);
            product.UserId = _testUser.Id;
            _mockRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<bool>())).ReturnsAsync(product);
            _mockRepo.Setup(repo => repo.RemoveAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(_testUser.Id);

            // Act
            var actionResult = await _controller.DeleteProduct(productId);

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<APIResponse>>());
            var result = actionResult.Result as ObjectResult;
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo((int)HttpStatusCode.NoContent));
            var apiResponse = result.Value as APIResponse;
            Assert.That(apiResponse, Is.Not.Null);
            Assert.That(apiResponse.IsSuccess, Is.True);
            Assert.That(apiResponse.ErrorMessages, Is.Null);
        }

        [Test]
        public async Task DeleteProduct_ReturnsForbidden_WhenUserIsNotOwner()
        {
            var productId = 1;
            var product = _products.First(p => p.ID == productId);
            product.UserId = "some-other-user-id";
            _mockRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<bool>())).ReturnsAsync(product);
            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(_testUser.Id);

            var actionResult = await _controller.DeleteProduct(productId);

            Assert.That(actionResult.Result, Is.InstanceOf<ObjectResult>());
            var objectResult = actionResult.Result as ObjectResult;
            Assert.That(objectResult.StatusCode, Is.EqualTo((int)HttpStatusCode.Forbidden));
            var apiResponse = objectResult.Value as APIResponse;
            Assert.That(apiResponse.IsSuccess, Is.False);
            Assert.That(apiResponse.ErrorMessages, Contains.Item("You can't delete Products created by others!"));
        }

        [Test]
        public async Task DeleteProduct_ReturnsBadRequest_WhenIdIsZero()
        {
            var actionResult = await _controller.DeleteProduct(0);

            Assert.That(actionResult.Result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = actionResult.Result as BadRequestObjectResult;
            var apiResponse = badRequestResult.Value as APIResponse;
            Assert.That(apiResponse.IsSuccess, Is.False);
            Assert.That(apiResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task DeleteProduct_ReturnsNotFound_WhenProductDoesNotExist()
        {
            var productId = 999;
            _mockRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<bool>())).ReturnsAsync((Product)null);

            var result = await _controller.DeleteProduct(productId);

            Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.That(notFoundResult.Value, Is.InstanceOf<APIResponse>());
            var apiResponse = notFoundResult.Value as APIResponse;
            Assert.That(apiResponse.IsSuccess, Is.False);
            Assert.That(apiResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task DeleteProduct_ReturnsInternalServerError_WhenExceptionIsThrown()
        {
            var productId = 1;
            _mockRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<bool>())).ThrowsAsync(new Exception("Test exception"));

            var actionResult = await _controller.DeleteProduct(productId);

            Assert.That(actionResult.Result, Is.InstanceOf<ObjectResult>());
            var internalServerErrorResult = actionResult.Result as ObjectResult;
            var apiResponse = internalServerErrorResult.Value as APIResponse;
            Assert.That(apiResponse.IsSuccess, Is.False);
            Assert.That(apiResponse.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(apiResponse.ErrorMessages, Contains.Item("Test exception"));
        }
        #endregion

        #region UpdateProduct
        [Test]
        public async Task UpdateProduct_ReturnsNoContent_WhenProductIsUpdatedAndUserIsOwner()
        {
            // Arrange
            var updateDTO = TestData.GenerateProductUpdateDTO();
            var product = _products.First(p => p.ID == updateDTO.ID);
            product.UserId = _testUser.Id;
            _mockRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<Product, bool>>>(), false)).ReturnsAsync(product);
            _mockRepo.Setup(repo => repo.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
            _mockMapper.Setup(mapper => mapper.Map<Product>(It.IsAny<ProductUpdateDTO>())).Returns(product);
            _mockMapper.Setup(mapper => mapper.Map<ProductDTO>(It.IsAny<Product>())).Returns(new ProductDTO());
            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(_testUser.Id);

            // Act
            var actionResult = await _controller.UpdateProduct(updateDTO);

            // Assert
            Assert.That(actionResult.Result, Is.InstanceOf<ObjectResult>());
            var objectResult = actionResult.Result as ObjectResult;
            Assert.That(objectResult.StatusCode, Is.EqualTo((int)HttpStatusCode.NoContent));
            var apiResponse = objectResult.Value as APIResponse;
            Assert.That(apiResponse.IsSuccess, Is.True);
            Assert.That(apiResponse.ErrorMessages, Is.Null);
        }

        [Test]
        public async Task UpdateProduct_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Name", "The Name field is required.");
            var updateDTO = new ProductUpdateDTO();

            // Act
            var actionResult = await _controller.UpdateProduct(updateDTO);

            // Assert
            Assert.That(actionResult.Result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = actionResult.Result as BadRequestObjectResult;
            var apiResponse = badRequestResult.Value as APIResponse;
            Assert.That(apiResponse.IsSuccess, Is.False);
            Assert.That(apiResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(apiResponse.ErrorMessages, Contains.Item("The Name field is required."));
        }

        [Test]
        public async Task UpdateProduct_ReturnsNotFound_WhenProductDoesNotExist()
        {
            // Arrange
            var updateDTO = TestData.GenerateProductUpdateDTO();
            _mockRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<Product, bool>>>(), false)).ReturnsAsync((Product)null);

            // Act
            var actionResult = await _controller.UpdateProduct(updateDTO);

            // Assert
            Assert.That(actionResult.Result, Is.InstanceOf<NotFoundObjectResult>());
            var notFoundResult = actionResult.Result as NotFoundObjectResult;
            var apiResponse = notFoundResult.Value as APIResponse;
            Assert.That(apiResponse.IsSuccess, Is.False);
            Assert.That(apiResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task UpdateProduct_ReturnsForbidden_WhenUserIsNotOwner()
        {
            // Arrange
            var updateDTO = TestData.GenerateProductUpdateDTO();
            var product = _products.First(p => p.ID == updateDTO.ID);
            product.UserId = "some-other-user-id";
            _mockRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<Product, bool>>>(), false)).ReturnsAsync(product);
            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(_testUser.Id);

            // Act
            var actionResult = await _controller.UpdateProduct(updateDTO);

            // Assert
            Assert.That(actionResult.Result, Is.InstanceOf<ObjectResult>());
            var objectResult = actionResult.Result as ObjectResult;
            Assert.That(objectResult.StatusCode, Is.EqualTo((int)HttpStatusCode.Forbidden));
            var apiResponse = objectResult.Value as APIResponse;
            Assert.That(apiResponse.IsSuccess, Is.False);
            Assert.That(apiResponse.ErrorMessages, Contains.Item("You can't update Products created by others!"));
        }

        [Test]
        public async Task UpdateProduct_ReturnsInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange
            var updateDTO = TestData.GenerateProductUpdateDTO();
            _mockRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<Product, bool>>>(), false)).ThrowsAsync(new Exception("Test exception"));

            // Act
            var actionResult = await _controller.UpdateProduct(updateDTO);

            // Assert
            Assert.That(actionResult.Result, Is.InstanceOf<ObjectResult>());
            var internalServerErrorResult = actionResult.Result as ObjectResult;
            Assert.That(internalServerErrorResult.StatusCode, Is.EqualTo((int)HttpStatusCode.InternalServerError));
            var apiResponse = internalServerErrorResult.Value as APIResponse;
            Assert.That(apiResponse.IsSuccess, Is.False);
            Assert.That(apiResponse.ErrorMessages, Contains.Item("Test exception"));
        }

        #endregion
    }
}