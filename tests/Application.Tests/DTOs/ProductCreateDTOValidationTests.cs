using Application.DTOs;
using Application.Tests.Helpers;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;

namespace Application.Tests
{
    [TestFixture]
    public class ProductCreateDTOValidationTests
    {
        private ProductCreateDTO _productCreateDto;
        private ValidationContext _validationContext;
        private List<ValidationResult> _validationResults;

        [SetUp]
        public void SetUp()
        {
            _productCreateDto = TestData.GenerateProducCreatetDTO();

            _validationContext = new ValidationContext(_productCreateDto, serviceProvider: null, items: null);
            _validationResults = new List<ValidationResult>();
        }

        [Test]
        public void ValidateProductCreateDTO_AllFieldsAreValid_ShouldPass()
        {
            var isValid = Validator.TryValidateObject(_productCreateDto, _validationContext, _validationResults, true);

            Assert.That(isValid);
        }

        [Test]
        public void ValidateProductCreateDTO_ManufacturePhoneExceedsMaxLength_ShouldFail()
        {
            _productCreateDto.ManufacturePhone = "123456789012";

            var isValid = Validator.TryValidateObject(_productCreateDto, _validationContext, _validationResults, true);

            Assert.That(isValid);
        }

        [Test]
        public void ValidateProductCreateDTO_ManufactureEmailIsNotValid_ShouldFail()
        {
            _productCreateDto.ManufactureEmail = "invalid-email";

            var isValid = Validator.TryValidateObject(_productCreateDto, _validationContext, _validationResults, true);

            Assert.That(isValid);
        }
    }
}
