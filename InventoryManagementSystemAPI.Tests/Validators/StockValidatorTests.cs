using FluentValidation.TestHelper;
using InventoryManagementSystemAPI.DTOs.Stock;
using InventoryManagementSystemAPI.Validators.Stock;
using Xunit;

namespace InventoryManagementSystemAPI.Tests.Validators
{
    public class StockValidatorTests
    {
        private readonly AdjustStockDtoValidator _validator = new();

        [Fact]
        public void AdjustStockDto_WhenValid_PassesValidation()
        {
            var dto = new AdjustStockDto
            {
                ProductId = Guid.NewGuid(),
                Delta = 5
            };

            var result = _validator.TestValidate(dto);

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

            var result = _validator.TestValidate(dto);

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

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Delta);
        }
    }
}
