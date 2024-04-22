using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Tests.Helpers;
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
            var product = TestData.GenerateProduct();

            await _repository.CreateAsync(product);
            var result = await _dbContext.Products.FirstOrDefaultAsync(p => p.ID == product.ID);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ProduceDate, Is.EqualTo(DateOnly.FromDateTime(DateTime.Now)));
            Assert.That(result.IsAvailable, Is.True);
        }

        [Test]
        public async Task UpdateAsync_UpdatesProduct()
        {
            var product = TestData.GenerateProduct();
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

        [Test]
        public async Task GetAllAsync_WithoutFilter_ReturnsAllItems()
        {
            var product1 = TestData.GenerateProduct();
            var product2 = TestData.GenerateProduct();
            await _dbContext.Products.AddRangeAsync(product1, product2);
            await _dbContext.SaveChangesAsync();

            var items = await _repository.GetAllAsync();

            Assert.That(items, Is.Not.Null);
            Assert.That(items.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetAsync_WithFilter_ReturnsSingleItem()
        {
            var product = TestData.GenerateProduct();
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            var retrievedItem = await _repository.GetAsync(p => p.ManufactureEmail == product.ManufactureEmail);

            Assert.That(retrievedItem, Is.Not.Null);
            Assert.That(retrievedItem.ManufactureEmail, Is.EqualTo(product.ManufactureEmail));
        }

        [Test]
        public async Task RemoveAsync_RemovesItem()
        {
            var product = TestData.GenerateProduct();
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            await _repository.RemoveAsync(product);
            var retrievedItem = await _repository.GetAsync(p => p.ManufactureEmail == product.ManufactureEmail);

            Assert.That(retrievedItem, Is.Null);
        }
    }
}
