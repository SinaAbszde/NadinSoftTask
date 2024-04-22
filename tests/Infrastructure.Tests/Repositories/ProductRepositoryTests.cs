using Domain.Models;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Infrastructure.Tests.Repositories
{
    [TestFixture]
    public class ProductRepositoryTests
    {
        private ApplicationDbContext _dbContext;
        private ProductRepository _repository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique name for each test run
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _repository = new ProductRepository(_dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task CreateAsync_AddsProductAndSetsProduceDate()
        {
            var product = new Product
            {
                Name = "Test Product",
                ManufacturePhone = "1234567890",
                ManufactureEmail = "test@example.com",
                IsAvailable = true
            };

            await _repository.CreateAsync(product);
            var result = await _dbContext.Products.FirstOrDefaultAsync(p => p.ID == product.ID);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ProduceDate, Is.EqualTo(DateOnly.FromDateTime(DateTime.Now)));
            Assert.That(result.IsAvailable, Is.True);
        }

        [Test]
        public async Task UpdateAsync_UpdatesProduct()
        {
            var product = new Product
            {
                ID = 1,
                Name = "Test Product",
                ManufacturePhone = "1234567890",
                ManufactureEmail = "test@example.com",
                IsAvailable = true
            };
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            product.Name = "Updated Product";
            product.IsAvailable = false;
            await _repository.UpdateAsync(product);
            var result = await _dbContext.Products.FirstOrDefaultAsync(p => p.ID == product.ID);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Updated Product"));
            Assert.That(result.IsAvailable, Is.False);
        }
    }
}
