using InventoryManagementSystemAPI.Models;
using InventoryManagementSystemAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystemAPI.Tests.Fakes
{
    public class FakeStockRepository : IStockRepository
    {
        private readonly Dictionary<Guid, Product> _products = new();
        public int ConcurrencyExceptionsToThrow { get; set; } = 0;
        public int ConcurrencyExceptionsThrown { get; private set; } = 0;
        public int UpdateCallsCount { get; private set; } = 0;

        public FakeStockRepository(IEnumerable<Product>? products = null)
        {
            if (products != null)
            {
                foreach (var product in products)
                {
                    _products[product.Id] = product;
                }
            }
        }

        public Task<Product?> GetByIdAsync(Guid productId)
        {
            _products.TryGetValue(productId, out var product);
            return Task.FromResult(product);
        }

        public Task UpdateAsync(Product product)
        {
            UpdateCallsCount++;
            if (ConcurrencyExceptionsThrown < ConcurrencyExceptionsToThrow)
            {
                ConcurrencyExceptionsThrown++;
                throw new DbUpdateConcurrencyException("Simulated concurrency conflict.");
            }

            _products[product.Id] = product;
            return Task.CompletedTask;
        }

        public async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            var snapshot = _products.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.QuantityInStock);

            try
            {
                await action();
            }
            catch
            {
                foreach (var (id, qty) in snapshot)
                {
                    if (_products.TryGetValue(id, out var p))
                    {
                        p.QuantityInStock = qty;
                    }
                }
                throw;
            }
        }
    }
}
