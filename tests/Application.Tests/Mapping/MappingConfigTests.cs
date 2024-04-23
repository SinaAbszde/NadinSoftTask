using Application.DTOs;
using Application.Mapping;
using Application.Tests.Helpers;
using AutoMapper;
using Domain.Models;
using NUnit.Framework;

namespace Application.Tests.Mapping
{
    [TestFixture]
    public class MappingConfigTests
    {
        private IMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingConfig());
            });
            _mapper = mappingConfig.CreateMapper();
        }

        [Test]
        public void ProductToProductDTO_MappingIsValid()
        {
            var product = TestData.GenerateProduct();

            var productDto = _mapper.Map<ProductDTO>(product);

            Assert.That(product.ID, Is.EqualTo(productDto.ID));
            Assert.That(product.Name, Is.EqualTo(productDto.Name));
            Assert.That(product.ProduceDate, Is.EqualTo(productDto.ProduceDate));
            Assert.That(product.ManufacturePhone, Is.EqualTo(productDto.ManufacturePhone));
            Assert.That(product.ManufactureEmail, Is.EqualTo(productDto.ManufactureEmail));
            Assert.That(product.IsAvailable, Is.EqualTo(productDto.IsAvailable));
        }

        [Test]
        public void ProductCreateDTOToProduct_MappingIsValid()
        {
            var productCreateDto = TestData.GenerateProducCreatetDTO();

            var product = _mapper.Map<Product>(productCreateDto);

            Assert.That(productCreateDto.Name, Is.EqualTo(product.Name));
            Assert.That(productCreateDto.ManufacturePhone, Is.EqualTo(product.ManufacturePhone));
            Assert.That(productCreateDto.ManufactureEmail, Is.EqualTo(product.ManufactureEmail));
            Assert.That(productCreateDto.IsAvailable, Is.EqualTo(product.IsAvailable));
        }

        [Test]
        public void ProductUpdateDTOToProduct_MappingIsValid()
        {
            var productUpdateDto = TestData.GenerateProductUpdateDTO();

            var product = _mapper.Map<Product>(productUpdateDto);

            Assert.That(productUpdateDto.ID, Is.EqualTo(product.ID));
            Assert.That(productUpdateDto.Name, Is.EqualTo(product.Name));
            Assert.That(productUpdateDto.ManufacturePhone, Is.EqualTo(product.ManufacturePhone));
            Assert.That(productUpdateDto.ManufactureEmail, Is.EqualTo(product.ManufactureEmail));
            Assert.That(productUpdateDto.IsAvailable, Is.EqualTo(product.IsAvailable));
        }
    }
}
