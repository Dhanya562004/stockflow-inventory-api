using InventoryManagementSystemAPI.Models;

namespace InventoryManagementSystemAPI.Repositories.Interfaces
{
    public interface ISupplierRepository
    {
        Task<List<Supplier>> GetAllAsync();

        Task<Supplier?> GetByIdAsync(Guid id);

        Task<Supplier> AddAsync(Supplier supplier);

        Task<Supplier> UpdateAsync(Supplier supplier);

        Task DeleteAsync(Supplier supplier);

        Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null);
    }
}
