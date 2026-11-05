using InventoryManagementSystemAPI.DTOs.Category;
using InventoryManagementSystemAPI.Models;

namespace InventoryManagementSystemAPI.Mappers
{
    public static class CategoryMapper
    {
        public static CategoryDto ToDto(this Category category) 
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }

        public static Category ToEntity(this CreateCategoryDto dto) 
        {
            return new Category
            {
                Name = dto.Name,
                Description = dto.Description
            };
        }

        public static void UpdateEntity(this UpdateCategoryDto dto, Category category) 
        {
            category.Name = dto.Name;
            category.Description = dto.Description;
        }

        public static List<CategoryDto> ToDtoList(this IEnumerable<Category> categories) 
        {
            return categories
                .Select(c => c.ToDto())
                .ToList();
        }
    }
}
