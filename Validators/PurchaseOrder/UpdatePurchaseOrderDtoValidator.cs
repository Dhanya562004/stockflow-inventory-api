using FluentValidation;
using InventoryManagementSystemAPI.DTOs.PurchaseOrder;

namespace InventoryManagementSystemAPI.Validators.PurchaseOrder
{
    public class UpdatePurchaseOrderDtoValidator : AbstractValidator<UpdatePurchaseOrderDto>
    {
        public UpdatePurchaseOrderDtoValidator() 
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.OrderNumber).NotEmpty().MaximumLength(20);
            RuleFor(x => x.OrderDate).NotEmpty();
            RuleFor(x => x.Status).IsInEnum();
            RuleFor(x => x.TotalAmount).GreaterThan(0).Must(x => decimal.Round(x, 2) == x);
            RuleFor(x => x.SupplierId).NotEmpty();
        }
    }
}
