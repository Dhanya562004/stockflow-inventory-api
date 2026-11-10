using InventoryManagementSystemAPI.Data;
using InventoryManagementSystemAPI.Models;
using InventoryManagementSystemAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystemAPI.Repositories.Implementations
{
    public class StockRepository : IStockRepository
    {
        private readonly AppDbContext _context;

        public StockRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetByIdAsync(Guid productId)
        {
            var trackedEntry = _context.ChangeTracker
                .Entries<Product>()
                .FirstOrDefault(e => e.Entity.Id == productId);

            if (trackedEntry != null)
            {
                if (trackedEntry.State == EntityState.Added)
                {
                    trackedEntry.State = EntityState.Detached;
                }
                else
                {
                    await trackedEntry.ReloadAsync();
                    return trackedEntry.Entity;
                }
            }

            return await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
        }

        public async Task UpdateAsync(Product product)
        {
            var entry = _context.Entry(product);
            if (entry.State == EntityState.Detached)
            {
                _context.Products.Attach(product);
                entry.State = EntityState.Modified;
            }
            entry.Property(p => p.RowVersion).IsModified = false;
            await _context.SaveChangesAsync();
        }

        public async Task<StockLevel?> GetStockLevelAsync(Guid productId, string locationId)
        {
            var trackedEntry = _context.ChangeTracker
                .Entries<StockLevel>()
                .FirstOrDefault(e => e.Entity.ProductId == productId && e.Entity.LocationId == locationId);

            if (trackedEntry != null)
            {
                if (trackedEntry.State == EntityState.Added)
                {
                    trackedEntry.State = EntityState.Detached;
                }
                else
                {
                    await trackedEntry.ReloadAsync();
                    return trackedEntry.Entity;
                }
            }

            return await _context.StockLevels
                .FirstOrDefaultAsync(s => s.ProductId == productId && s.LocationId == locationId);
        }

        public async Task UpdateStockLevelAsync(StockLevel stockLevel)
        {
            var entry = _context.Entry(stockLevel);
            if (entry.State == EntityState.Detached)
            {
                _context.StockLevels.Attach(stockLevel);
                entry.State = EntityState.Modified;
            }
            entry.Property(s => s.RowVersion).IsModified = false;
            await _context.SaveChangesAsync();
        }

        public async Task AddStockLevelAsync(StockLevel stockLevel)
        {
            await _context.StockLevels.AddAsync(stockLevel);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> LocationExistsAsync(string locationId)
        {
            if (string.IsNullOrWhiteSpace(locationId))
                return false;

            return await _context.StockLevels.AnyAsync(s => s.LocationId == locationId);
        }

        public async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    await action();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    _context.ChangeTracker.Clear();
                    throw;
                }
            });
        }
    }
}
