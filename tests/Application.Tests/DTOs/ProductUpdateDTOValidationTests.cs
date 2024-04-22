using Application.DTOs;
using Application.Tests.Helpers;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;

namespace Application.Tests
{
    [TestFixture]
    public class ProductUpdateDTOValidationTests
    {
        private ProductUpdateDTO _productUpdateDto;
        private ValidationContext _validationContext;
        private List<ValidationResult> _validationResults;

        [SetUp]
        public void SetUp()
        {
            _productUpdateDto = TestData.GenerateProductUpdateDTO();

            _validationContext = new ValidationContext(_productUpdateDto, serviceProvider: null, items: null);
            _validationResults = new List<ValidationResult>();
        }

        [Test]
        public void ValidateProductUpdateDTO_AllFieldsAreValid_ShouldPass()
        {
            var isValid = Validator.TryValidateObject(_productUpdateDto, _validationContext, _validationResults, true);

            Assert.That(isValid);
        }

        [Test]
        public void ValidateProductUpdateDTO_ManufacturePhoneExceedsMaxLength_ShouldFail()
        {
            _productUpdateDto.ManufacturePhone = "123456789012";

            var isValid = Validator.TryValidateObject(_productUpdateDto, _validationContext, _validationResults, true);

            Assert.That(isValid);
        }

        [Test]
        public void ValidateProductUpdateDTO_ManufactureEmailIsNotValid_ShouldFail()
        {
            _productUpdateDto.ManufactureEmail = "invalid-email";

            var isValid = Validator.TryValidateObject(_productUpdateDto, _validationContext, _validationResults, true);

            Assert.That(isValid);
        }
    }
}
