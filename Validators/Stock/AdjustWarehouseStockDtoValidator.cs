using FluentValidation;
using InventoryManagementSystemAPI.DTOs.Stock;

namespace InventoryManagementSystemAPI.Validators.Stock
{
    public class AdjustWarehouseStockDtoValidator : AbstractValidator<AdjustWarehouseStockDto>
    {
        public AdjustWarehouseStockDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("ProductId is required.");

            RuleFor(x => x.LocationId)
                .NotEmpty()
                .WithMessage("LocationId is required.")
                .MaximumLength(50)
                .WithMessage("LocationId must not exceed 50 characters.")
                .Matches(@"^[a-zA-Z0-9_\-]+$")
                .WithMessage("LocationId can only contain letters, numbers, underscores, and hyphens.");

            RuleFor(x => x.Delta)
                .NotEqual(0)
                .WithMessage("Delta must not be zero.");
        }
    }
}
