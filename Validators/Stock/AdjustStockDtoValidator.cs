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

            RuleFor(x => x.LocationId)
                .NotEmpty()
                .WithMessage("LocationId must not be empty when specified.")
                .MaximumLength(50)
                .WithMessage("LocationId must not exceed 50 characters.")
                .Matches(@"^[a-zA-Z0-9_\-]+$")
                .WithMessage("LocationId can only contain letters, numbers, underscores, and hyphens.")
                .When(x => x.LocationId != null);
        }
    }
}
