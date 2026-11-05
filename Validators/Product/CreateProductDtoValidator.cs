using FluentValidation;
using InventoryManagementSystemAPI.DTOs.Product;

namespace InventoryManagementSystemAPI.Validators.Product
{
    public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductDtoValidator() 
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
            RuleFor(x => x.SKU).NotEmpty().MaximumLength(16);
            RuleFor(x => x.Barcode).NotEmpty().MaximumLength(14);
            RuleFor(x => x.Price).GreaterThan(0);
            RuleFor(x => x.Cost).GreaterThan(0);
            RuleFor(x => x.QuantityInStock).GreaterThanOrEqualTo(0);
            RuleFor(x => x.ReorderLevel).GreaterThanOrEqualTo(0);
            RuleFor(x => x.IsActive).NotNull();
            RuleFor(x => x.CreatedDate).NotEmpty();
            RuleFor(x => x.CategoryId).NotEmpty();
            RuleFor(x => x.SupplierId).NotEmpty();
        }
    }
}
