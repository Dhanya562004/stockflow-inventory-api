using InventoryManagementSystemAPI.Models;

namespace InventoryManagementSystemAPI.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllAsync();

        Task<Category?> GetByIdAsync(Guid id);

        Task<Category> AddAsync(Category category);

        Task<Category> UpdateAsync(Category category);

        Task DeleteAsync(Category category);

        Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null);
    }
}
