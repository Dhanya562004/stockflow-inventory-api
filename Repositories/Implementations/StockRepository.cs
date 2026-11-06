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
                await trackedEntry.ReloadAsync();
                return trackedEntry.Entity;
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

        public async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                await action();
                await transaction.CommitAsync();
            });
        }
    }
}
