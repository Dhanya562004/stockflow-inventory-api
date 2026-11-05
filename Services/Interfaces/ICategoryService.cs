using InventoryManagementSystemAPI.DTOs.Category;

namespace InventoryManagementSystemAPI.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllAsync();

        Task<CategoryDto?> GetByIdAsync(Guid id);

        Task<CategoryDto> CreateAsync(CreateCategoryDto dto);

        Task<CategoryDto?> UpdateAsync(Guid id, UpdateCategoryDto dto);

        Task DeleteAsync(Guid id);
    }
}
