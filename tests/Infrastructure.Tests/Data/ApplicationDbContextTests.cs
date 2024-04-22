using Domain.Models;
using Infrastructure.Data;
using Infrastructure.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Infrastructure.Tests.Data
{
    [TestFixture]
    public class ApplicationDbContextTests
    {
        private ApplicationDbContext _db;
        private Product _product;

        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite($"DataSource=:memory:;Cache=Shared") // Had to use Sqlite because the in-memory database didn't enforce unique constraints
                .Options;

            _db = new ApplicationDbContext(options);
            _db.Database.OpenConnection();
            _db.Database.EnsureCreated();

            _product = TestData.GenerateProduct();

            _db.Products.Add(_product);
            await _db.SaveChangesAsync();
        }

        [Test]
        public async Task UniqueIndexOnManufactureEmailAndProduceDate_ShouldPreventDuplicates()
        {
            var duplicateProduct = new Product
            {
                Name = "Duplicate Test Product",
                ManufacturePhone = "0987654321",
                ManufactureEmail = _product.ManufactureEmail,
                ProduceDate = _product.ProduceDate,
                IsAvailable = false
            };

            _db.Products.Add(duplicateProduct);

            Assert.ThrowsAsync<DbUpdateException>(async () => await _db.SaveChangesAsync());
        }

        [TearDown]
        public async Task TearDown()
        {
            await _db.Database.CloseConnectionAsync();
            _db.Dispose();
        }
    }
}
