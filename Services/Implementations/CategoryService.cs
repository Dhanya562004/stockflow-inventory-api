using InventoryManagementSystemAPI.DTOs.Category;
using InventoryManagementSystemAPI.Exceptions;
using InventoryManagementSystemAPI.Mappers;
using InventoryManagementSystemAPI.Repositories.Interfaces;
using InventoryManagementSystemAPI.Services.Interfaces;

namespace InventoryManagementSystemAPI.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;

        public CategoryService(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
        {
            if (await _repository.ExistsByNameAsync(dto.Name))
            {
                throw new DuplicateResourceException("A category with this name already exists.");
            }

            var category = dto.ToEntity();

            await _repository.AddAsync(category);

            return category.ToDto();
        }

        public async Task DeleteAsync(Guid id)
        {
            var category = await _repository.GetByIdAsync(id);

            if (category == null)
                throw new NotFoundException("Category not found.");

            await _repository.DeleteAsync(category);
        }

        public async Task<List<CategoryDto>> GetAllAsync()
        {
            var categories = await _repository.GetAllAsync();

            return categories.ToDtoList();
        }

        public async Task<CategoryDto?> GetByIdAsync(Guid id)
        {
            var category = await _repository.GetByIdAsync(id);

            if (category == null)
                throw new NotFoundException("Category not found.");

            return category.ToDto();
        }

        public async Task<CategoryDto?> UpdateAsync(Guid id, UpdateCategoryDto dto)
        {
            var category = await _repository.GetByIdAsync(id);

            if (category == null)
                throw new NotFoundException("Category not found.");

            if (category.Name != dto.Name)
            {
                if (await _repository.ExistsByNameAsync(dto.Name, id))
                {
                    throw new DuplicateResourceException("A category with this name already exists.");
                }
            }

            dto.UpdateEntity(category);

            await _repository.UpdateAsync(category);

            return category.ToDto();
        }
    }
}
