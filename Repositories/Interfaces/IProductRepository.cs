using InventoryManagementSystemAPI.Models;

namespace InventoryManagementSystemAPI.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(Guid id);
        Task<Product> AddAsync(Product product);
        Task<Product> UpdateAsync(Product product);
        Task DeleteAsync(Product product);
        Task<bool> ExistsBySkuAsync(string sku, Guid? excludeId = null);
        Task<bool> ExistsByBarcodeAsync(string barcode, Guid? excludeId = null);
    }
}
