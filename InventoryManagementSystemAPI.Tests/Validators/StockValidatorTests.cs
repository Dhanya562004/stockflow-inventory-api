using FluentValidation.TestHelper;
using InventoryManagementSystemAPI.DTOs.Stock;
using InventoryManagementSystemAPI.Validators.Stock;
using Xunit;

namespace InventoryManagementSystemAPI.Tests.Validators
{
    public class StockValidatorTests
    {
        private readonly AdjustStockDtoValidator _adjustStockValidator = new();
        private readonly AdjustWarehouseStockDtoValidator _adjustWarehouseValidator = new();

        [Fact]
        public void AdjustStockDto_WhenValid_PassesValidation()
        {
            var dto = new AdjustStockDto
            {
                ProductId = Guid.NewGuid(),
                Delta = 5
            };

            var result = _adjustStockValidator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void AdjustStockDto_WhenProductIdEmpty_FailsValidation()
        {
            var dto = new AdjustStockDto
            {
                ProductId = Guid.Empty,
                Delta = 5
            };

            var result = _adjustStockValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ProductId);
        }

        [Fact]
        public void AdjustStockDto_WhenDeltaZero_FailsValidation()
        {
            var dto = new AdjustStockDto
            {
                ProductId = Guid.NewGuid(),
                Delta = 0
            };

            var result = _adjustStockValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Delta);
        }

        [Fact]
        public void AdjustStockDto_WhenLocationIdSpecifiedAndValid_PassesValidation()
        {
            var dto = new AdjustStockDto
            {
                ProductId = Guid.NewGuid(),
                LocationId = "WH-MAIN",
                Delta = 5
            };

            var result = _adjustStockValidator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void AdjustStockDto_WhenLocationIdContainsInvalidChars_FailsValidation()
        {
            var dto = new AdjustStockDto
            {
                ProductId = Guid.NewGuid(),
                LocationId = "WH MAIN#1!",
                Delta = 5
            };

            var result = _adjustStockValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.LocationId);
        }

        [Fact]
        public void AdjustStockDto_WhenLocationIdExceedsMaxLength_FailsValidation()
        {
            var dto = new AdjustStockDto
            {
                ProductId = Guid.NewGuid(),
                LocationId = new string('A', 51),
                Delta = 5
            };

            var result = _adjustStockValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.LocationId);
        }

        [Fact]
        public void AdjustStockDto_WhenLocationIdEmptyString_FailsValidation()
        {
            var dto = new AdjustStockDto
            {
                ProductId = Guid.NewGuid(),
                LocationId = "",
                Delta = 5
            };

            var result = _adjustStockValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.LocationId);
        }

        [Fact]
        public void AdjustWarehouseStockDto_WhenValid_PassesValidation()
        {
            var dto = new AdjustWarehouseStockDto
            {
                ProductId = Guid.NewGuid(),
                LocationId = "WH-NORTH-01",
                Delta = 10
            };

            var result = _adjustWarehouseValidator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void AdjustWarehouseStockDto_WhenProductIdEmpty_FailsValidation()
        {
            var dto = new AdjustWarehouseStockDto
            {
                ProductId = Guid.Empty,
                LocationId = "WH-MAIN",
                Delta = 10
            };

            var result = _adjustWarehouseValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ProductId);
        }

        [Fact]
        public void AdjustWarehouseStockDto_WhenLocationIdEmpty_FailsValidation()
        {
            var dto = new AdjustWarehouseStockDto
            {
                ProductId = Guid.NewGuid(),
                LocationId = "",
                Delta = 10
            };

            var result = _adjustWarehouseValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.LocationId);
        }

        [Fact]
        public void AdjustWarehouseStockDto_WhenLocationIdHasInvalidChars_FailsValidation()
        {
            var dto = new AdjustWarehouseStockDto
            {
                ProductId = Guid.NewGuid(),
                LocationId = "WH MAIN*",
                Delta = 10
            };

            var result = _adjustWarehouseValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.LocationId);
        }

        [Fact]
        public void AdjustWarehouseStockDto_WhenLocationIdExceeds50Chars_FailsValidation()
        {
            var dto = new AdjustWarehouseStockDto
            {
                ProductId = Guid.NewGuid(),
                LocationId = new string('W', 51),
                Delta = 10
            };

            var result = _adjustWarehouseValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.LocationId);
        }

        [Fact]
        public void AdjustWarehouseStockDto_WhenDeltaZero_FailsValidation()
        {
            var dto = new AdjustWarehouseStockDto
            {
                ProductId = Guid.NewGuid(),
                LocationId = "WH-MAIN",
                Delta = 0
            };

            var result = _adjustWarehouseValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Delta);
        }
    }
}
