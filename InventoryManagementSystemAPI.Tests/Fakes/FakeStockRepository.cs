using InventoryManagementSystemAPI.Models;
using InventoryManagementSystemAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystemAPI.Tests.Fakes
{
    public class FakeStockRepository : IStockRepository
    {
        private readonly Dictionary<Guid, Product> _products = new();
        private readonly Dictionary<(Guid ProductId, string LocationId), StockLevel> _stockLevels = new();
        private readonly HashSet<string> _knownLocations = new(StringComparer.OrdinalIgnoreCase)
        {
            "WH-MAIN",
            "WH-SECONDARY"
        };

        public int ConcurrencyExceptionsToThrow { get; set; } = 0;
        public int ConcurrencyExceptionsThrown { get; private set; } = 0;
        public int UpdateCallsCount { get; private set; } = 0;

        public FakeStockRepository(IEnumerable<Product>? products = null, IEnumerable<StockLevel>? stockLevels = null)
        {
            if (products != null)
            {
                foreach (var product in products)
                {
                    _products[product.Id] = product;
                }
            }

            if (stockLevels != null)
            {
                foreach (var sl in stockLevels)
                {
                    _stockLevels[(sl.ProductId, sl.LocationId)] = sl;
                    _knownLocations.Add(sl.LocationId);
                }
            }
        }

        public void AddLocation(string locationId)
        {
            if (!string.IsNullOrWhiteSpace(locationId))
            {
                _knownLocations.Add(locationId);
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

        public Task<StockLevel?> GetStockLevelAsync(Guid productId, string locationId)
        {
            _stockLevels.TryGetValue((productId, locationId), out var stockLevel);
            return Task.FromResult(stockLevel);
        }

        public Task UpdateStockLevelAsync(StockLevel stockLevel)
        {
            UpdateCallsCount++;
            if (ConcurrencyExceptionsThrown < ConcurrencyExceptionsToThrow)
            {
                ConcurrencyExceptionsThrown++;
                throw new DbUpdateConcurrencyException("Simulated concurrency conflict on StockLevel.");
            }

            _stockLevels[(stockLevel.ProductId, stockLevel.LocationId)] = stockLevel;
            return Task.CompletedTask;
        }

        public Task AddStockLevelAsync(StockLevel stockLevel)
        {
            UpdateCallsCount++;
            if (ConcurrencyExceptionsThrown < ConcurrencyExceptionsToThrow)
            {
                ConcurrencyExceptionsThrown++;
                throw new DbUpdateConcurrencyException("Simulated concurrency conflict on StockLevel insert.");
            }

            var key = (stockLevel.ProductId, stockLevel.LocationId);
            if (_stockLevels.ContainsKey(key))
            {
                throw new DbUpdateException("Duplicate key IX_StockLevels_ProductId_LocationId_Unique");
            }

            _stockLevels[key] = stockLevel;
            _knownLocations.Add(stockLevel.LocationId);
            return Task.CompletedTask;
        }

        public Task<bool> LocationExistsAsync(string locationId)
        {
            if (string.IsNullOrWhiteSpace(locationId))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(_knownLocations.Contains(locationId));
        }

        public async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            var productSnapshot = _products.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.QuantityInStock);

            var stockLevelSnapshot = _stockLevels.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Quantity);

            try
            {
                await action();
            }
            catch
            {
                // Revert product stock
                foreach (var (id, qty) in productSnapshot)
                {
                    if (_products.TryGetValue(id, out var p))
                    {
                        p.QuantityInStock = qty;
                    }
                }

                // Remove any stock levels added during failed transaction
                var newlyAddedKeys = _stockLevels.Keys.Except(stockLevelSnapshot.Keys).ToList();
                foreach (var key in newlyAddedKeys)
                {
                    _stockLevels.Remove(key);
                }

                // Revert stock level quantities
                foreach (var (key, qty) in stockLevelSnapshot)
                {
                    if (_stockLevels.TryGetValue(key, out var sl))
                    {
                        sl.Quantity = qty;
                    }
                }

                throw;
            }
        }
    }
}
