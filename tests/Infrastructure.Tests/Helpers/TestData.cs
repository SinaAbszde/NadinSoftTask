using Domain.Models;

namespace Infrastructure.Tests.Helpers
{
    public class TestData
    {
        private static int _productCounter = 1;

        public static Product GenerateProduct()
        {
            var product = new Product
            {
                Name = $"Test Product {_productCounter}",
                ManufacturePhone = "1234567890",
                ManufactureEmail = $"test{_productCounter}@example.com",
                ProduceDate = DateOnly.FromDateTime(DateTime.Now.AddDays(_productCounter)),
                IsAvailable = true
            };

            _productCounter++;
            return product;
        }
    }
}
