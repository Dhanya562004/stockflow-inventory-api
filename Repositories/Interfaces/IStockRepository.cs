using InventoryManagementSystemAPI.Models;

namespace InventoryManagementSystemAPI.Repositories.Interfaces
{
    public interface IStockRepository
    {
        Task<Product?> GetByIdAsync(Guid productId);
        Task UpdateAsync(Product product);
        Task ExecuteInTransactionAsync(Func<Task> action);
    }
}
