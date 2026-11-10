using FluentValidation;
using InventoryManagementSystemAPI.DTOs.PurchaseOrder;

namespace InventoryManagementSystemAPI.Validators.PurchaseOrder
{
    public class CreatePurchaseOrderDtoValidator : AbstractValidator<CreatePurchaseOrderDto>
    {
        public CreatePurchaseOrderDtoValidator()
        {
            RuleFor(x => x.OrderNumber).NotEmpty().MaximumLength(20);
            RuleFor(x => x.OrderDate).NotEmpty();
            RuleFor(x => x.Status).IsInEnum();
            RuleFor(x => x.TotalAmount).GreaterThan(0).Must(x => decimal.Round(x, 2) == x);
            RuleFor(x => x.SupplierId).NotEmpty();
        }
    }
}
