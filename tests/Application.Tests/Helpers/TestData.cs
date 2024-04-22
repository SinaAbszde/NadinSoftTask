using Application.DTOs;
using Domain.Models;

namespace Application.Tests.Helpers
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

        public static ProductDTO GenerateProductDTO()
        {
            var productDTO = new ProductDTO
            {
                ID = 1,
                Name = "Test Product",
                ManufacturePhone = "1234567890",
                ManufactureEmail = "test@example.com",
                ProduceDate = DateOnly.FromDateTime(DateTime.Now),
                IsAvailable = true
            };

            return productDTO;
        }

        public static ProductCreateDTO GenerateProducCreatetDTO()
        {
            var productCreateDTO = new ProductCreateDTO
            {
                Name = "Test Product",
                ManufacturePhone = "1234567890",
                ManufactureEmail = "test@example.com",
                IsAvailable = true
            };

            return productCreateDTO;
        }

        public static ProductUpdateDTO GenerateProductUpdateDTO()
        {
            var productUpdateDTO = new ProductUpdateDTO
            {
                ID = 1,
                Name = "Test Product",
                ManufacturePhone = "1234567890",
                ManufactureEmail = "test@example.com",
                IsAvailable = true
            };

            return productUpdateDTO;
        }
    }
}
