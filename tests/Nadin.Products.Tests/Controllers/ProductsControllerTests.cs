using Application.DTOs;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Nadin.Products.Controllers;
using Nadin.Products.Responses;
using Nadin.Products.Tests.Helpers;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;

namespace Nadin.Products.Tests.Controllers
{
    [TestFixture]
    public class ProductsControllerTests
    {
        private Mock<IProductRepository> _mockRepo;
        private Mock<IMapper> _mockMapper;
        private ProductsController _controller;
        private List<Product> _products;
        private List<ProductDTO> _productDTOs;

        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<IProductRepository>();
            _mockMapper = new Mock<IMapper>();
            _controller = new ProductsController(_mockRepo.Object, _mockMapper.Object);

            // Setup test data
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

            // Setup mock repository
            _mockRepo.Setup(repo => repo.GetAllAsync(It.IsAny<Expression<Func<Product, bool>>>())).ReturnsAsync(_products);

            // Setup mock mapper
            _mockMapper.Setup(mapper => mapper.Map<List<ProductDTO>>(It.IsAny<IEnumerable<Product>>()))
                       .Returns(_productDTOs);
        }

        [Test]
        public async Task GetAllProducts_ReturnsAllProducts_WhenCalled()
        {
            var result = await _controller.GetAllProducts();

            Assert.That(result, Is.InstanceOf<ActionResult<APIResponse>>());
            var apiResponse = result.Value;
            Assert.That(apiResponse.IsSuccess, Is.True);
            Assert.That((List<ProductDTO>)apiResponse.Result, Has.Count.EqualTo(_productDTOs.Count));
            Assert.That(apiResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(apiResponse.ErrorMessages, Is.Null);
        }

        [Test]
        public async Task GetProduct_ReturnsProduct_WhenProductExists()
        {
            var productId = 1;
            var product = _products.First(p => p.ID == productId);
            var productDTO = _productDTOs.First(p => p.ID == productId);
            _mockRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<Product, bool>>>(), true)).ReturnsAsync(product);
            _mockMapper.Setup(mapper => mapper.Map<ProductDTO>(It.IsAny<Product>())).Returns(productDTO);

            var result = await _controller.GetProduct(productId);

            Assert.That(result, Is.InstanceOf<ActionResult<APIResponse>>());
            var apiResponse = result.Value;
            Assert.That(apiResponse.IsSuccess, Is.True);
            Assert.That(apiResponse.Result, Is.EqualTo(productDTO));
            Assert.That(apiResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(apiResponse.ErrorMessages, Is.Null);
        }

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
                IsAvailable = createDTO.IsAvailable
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

            var result = await _controller.CreateProduct(createDTO);

            var apiResponse = result.Value;
            Assert.That(apiResponse, Is.InstanceOf<APIResponse>());
            Assert.That(apiResponse.IsSuccess, Is.True);
            Assert.That(apiResponse.Result, Is.EqualTo(createdProductDTO));
            Assert.That(apiResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(apiResponse.ErrorMessages, Is.Null);
        }

        [Test]
        public async Task DeleteProduct_ReturnsNoContent_WhenProductIsDeleted()
        {
            var productId = 1;
            var product = _products.First(p => p.ID == productId);
            _mockRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<Product, bool>>>(), true)).ReturnsAsync(product);
            _mockRepo.Setup(repo => repo.RemoveAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

            var result = await _controller.DeleteProduct(productId);

            var apiResponse = result.Value;
            Assert.That(apiResponse, Is.InstanceOf<APIResponse>());
            Assert.That(apiResponse.IsSuccess, Is.True);
            Assert.That(apiResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(apiResponse.ErrorMessages, Is.Null);
        }

        [Test]
        public async Task UpdateProduct_ReturnsNoContent_WhenProductIsUpdated()
        {
            var updateDTO = TestData.GenerateProductUpdateDTO();
            var updatedProduct = new Product
            {
                ID = updateDTO.ID,
                Name = updateDTO.Name,
                ProduceDate = DateOnly.FromDateTime(DateTime.Now),
                ManufactureEmail = updateDTO.ManufactureEmail,
                ManufacturePhone = updateDTO.ManufacturePhone,
                IsAvailable = updateDTO.IsAvailable
            };
            var updatedProductDTO = new ProductDTO
            {
                ID = updatedProduct.ID,
                Name = updatedProduct.Name,
                ProduceDate = updatedProduct.ProduceDate,
                ManufactureEmail = updatedProduct.ManufactureEmail,
                ManufacturePhone = updatedProduct.ManufacturePhone,
                IsAvailable = updatedProduct.IsAvailable
            };
            _mockRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<Product, bool>>>(), false)).ReturnsAsync(updatedProduct);
            _mockRepo.Setup(repo => repo.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
            _mockMapper.Setup(mapper => mapper.Map<Product>(It.IsAny<ProductUpdateDTO>())).Returns(updatedProduct);
            _mockMapper.Setup(mapper => mapper.Map<ProductDTO>(It.IsAny<Product>())).Returns(updatedProductDTO);

            var result = await _controller.UpdateProduct(updateDTO);

            var apiResponse = result.Value;
            Assert.That(apiResponse, Is.InstanceOf<APIResponse>());
            Assert.That(apiResponse.Result, Is.EqualTo(updatedProductDTO));
            Assert.That(apiResponse.IsSuccess, Is.True);
            Assert.That(apiResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        }

    }
}
