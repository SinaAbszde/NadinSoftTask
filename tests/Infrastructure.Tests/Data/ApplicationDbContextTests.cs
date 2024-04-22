using Domain.Models;
using Infrastructure.Data;
using Infrastructure.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Infrastructure.Tests
{
    [TestFixture]
    public class ApplicationDbContextTests
    {
        private ApplicationDbContext _db;
        private Product _product;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);

            _product = TestData.GenerateProduct();

            _db.Products.Add(_product);
            _db.SaveChanges();
        }

        [Test]
        public async Task UniqueIndexOnManufactureEmailAndProduceDate_ShouldPreventDuplicates() // This test isn't going as expected. I'll get back to it later
        {
            var product = new Product
            {
                Name = "Test Product",
                ManufacturePhone = "1234567890",
                ManufactureEmail = _product.ManufactureEmail,
                ProduceDate = _product.ProduceDate,
                IsAvailable = true
            };

            _db.Products.Add(product);

            Assert.ThrowsAsync<DbUpdateException>(async () => await _db.SaveChangesAsync());
        }

        [TearDown]
        public void TearDown()
        {
            _db.Dispose();
        }
    }
}
