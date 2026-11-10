using InventoryManagementSystemAPI.Models;

namespace InventoryManagementSystemAPI.Repositories.Interfaces
{
    public interface IStockRepository
    {
        Task<Product?> GetByIdAsync(Guid productId);
        Task UpdateAsync(Product product);
        Task<StockLevel?> GetStockLevelAsync(Guid productId, string locationId);
        Task UpdateStockLevelAsync(StockLevel stockLevel);
        Task AddStockLevelAsync(StockLevel stockLevel);
        Task<bool> LocationExistsAsync(string locationId);
        Task ExecuteInTransactionAsync(Func<Task> action);
    }
}
