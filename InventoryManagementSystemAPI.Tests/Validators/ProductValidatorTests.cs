using FluentValidation.TestHelper;
using InventoryManagementSystemAPI.DTOs.Product;
using InventoryManagementSystemAPI.Validators.Product;
using Xunit;

namespace InventoryManagementSystemAPI.Tests.Validators
{
    public class ProductValidatorTests
    {
        private readonly CreateProductDtoValidator _createValidator = new();
        private readonly UpdateProductDtoValidator _updateValidator = new();

        [Fact]
        public void CreateProductDto_WhenValid_PassesValidation()
        {
            var dto = new CreateProductDto
            {
                Name = "Test Product",
                Description = "A valid test product description",
                SKU = "TEST-SKU-001",
                Barcode = "123456789012",
                Price = 19.99m,
                Cost = 9.99m,
                QuantityInStock = 10,
                ReorderLevel = 5,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CategoryId = Guid.NewGuid(),
                SupplierId = Guid.NewGuid()
            };

            var result = _createValidator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void CreateProductDto_WhenSkuEmpty_FailsValidation(string? sku)
        {
            var dto = new CreateProductDto { SKU = sku! };

            var result = _createValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.SKU);
        }

        [Fact]
        public void CreateProductDto_WhenSkuExceeds16Characters_FailsValidation()
        {
            var dto = new CreateProductDto { SKU = "12345678901234567" }; // 17 chars

            var result = _createValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.SKU);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5.50)]
        public void CreateProductDto_WhenPriceZeroOrNegative_FailsValidation(decimal price)
        {
            var dto = new CreateProductDto { Price = price };

            var result = _createValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Price);
        }

        [Fact]
        public void CreateProductDto_WhenPriceGreaterThanZero_PassesPriceRule()
        {
            var dto = new CreateProductDto { Price = 0.01m };

            var result = _createValidator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Price);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1.00)]
        public void CreateProductDto_WhenCostZeroOrNegative_FailsValidation(decimal cost)
        {
            var dto = new CreateProductDto { Cost = cost };

            var result = _createValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Cost);
        }

        [Fact]
        public void CreateProductDto_WhenQuantityInStockNegative_FailsValidation()
        {
            var dto = new CreateProductDto { QuantityInStock = -1 };

            var result = _createValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.QuantityInStock);
        }

        [Fact]
        public void CreateProductDto_WhenQuantityInStockZeroOrPositive_PassesValidation()
        {
            var dto = new CreateProductDto { QuantityInStock = 0 };

            var result = _createValidator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.QuantityInStock);
        }

        [Fact]
        public void CreateProductDto_WhenReorderLevelNegative_FailsValidation()
        {
            var dto = new CreateProductDto { ReorderLevel = -1 };

            var result = _createValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ReorderLevel);
        }

        [Fact]
        public void UpdateProductDto_WhenIdEmpty_FailsValidation()
        {
            var dto = new UpdateProductDto { Id = Guid.Empty };

            var result = _updateValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public void UpdateProductDto_WhenPriceNegative_FailsValidation()
        {
            var dto = new UpdateProductDto { Price = -10.0m };

            var result = _updateValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Price);
        }
    }
}
