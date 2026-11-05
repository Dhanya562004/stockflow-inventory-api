using InventoryManagementSystemAPI.Models;
using InventoryManagementSystemAPI.Repositories.Interfaces;

namespace InventoryManagementSystemAPI.Tests.Fakes
{
    public class FakeProductRepository : IProductRepository
    {
        private readonly List<Product> _products = new();

        public FakeProductRepository(IEnumerable<Product>? initialProducts = null)
        {
            if (initialProducts != null)
            {
                _products.AddRange(initialProducts);
            }
        }

        public Task<Product> AddAsync(Product product)
        {
            _products.Add(product);
            return Task.FromResult(product);
        }

        public Task DeleteAsync(Product product)
        {
            _products.Remove(product);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsByBarcodeAsync(string barcode, Guid? excludeId = null)
        {
            var exists = _products.Any(p => p.Barcode == barcode && (excludeId == null || p.Id != excludeId));
            return Task.FromResult(exists);
        }

        public Task<bool> ExistsBySkuAsync(string sku, Guid? excludeId = null)
        {
            var exists = _products.Any(p => p.SKU == sku && (excludeId == null || p.Id != excludeId));
            return Task.FromResult(exists);
        }

        public Task<List<Product>> GetAllAsync()
        {
            return Task.FromResult(_products.ToList());
        }

        public Task<Product?> GetByIdAsync(Guid id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            return Task.FromResult(product);
        }

        public Task<Product> UpdateAsync(Product product)
        {
            var index = _products.FindIndex(p => p.Id == product.Id);
            if (index >= 0)
            {
                _products[index] = product;
            }
            return Task.FromResult(product);
        }
    }
}

