using FluentValidation;
using InventoryManagementSystemAPI.DTOs.Stock;

namespace InventoryManagementSystemAPI.Validators.Stock
{
    public class AdjustStockDtoValidator : AbstractValidator<AdjustStockDto>
    {
        public AdjustStockDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("ProductId is required.");

            RuleFor(x => x.Delta)
                .NotEqual(0)
                .WithMessage("Delta must not be zero.");
        }
    }
}
