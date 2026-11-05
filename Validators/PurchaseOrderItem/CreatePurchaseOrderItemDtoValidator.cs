using FluentValidation;
using InventoryManagementSystemAPI.DTOs.PurchaseOrderItem;

namespace InventoryManagementSystemAPI.Validators.PurchaseOrderItem
{
    public class CreatePurchaseOrderItemDtoValidator : AbstractValidator<CreatePurchaseOrderItemDto>
    {
        public CreatePurchaseOrderItemDtoValidator() 
        {
            RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Cost).GreaterThan(0).Must(x => decimal.Round(x, 2) == x);
            RuleFor(x => x.PurchaseOrderId).NotEmpty();
            RuleFor(x => x.ProductId).NotEmpty();
        }
    }
}
