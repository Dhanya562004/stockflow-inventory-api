using FluentValidation;
using InventoryManagementSystemAPI.DTOs.Category;

namespace InventoryManagementSystemAPI.Validators.Category
{
    public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
    {
        public CreateCategoryDtoValidator() 
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        } 
    }
}
