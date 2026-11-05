using InventoryManagementSystemAPI.Data;
using InventoryManagementSystemAPI.Models;
using InventoryManagementSystemAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystemAPI.Repositories.Implementations
{
    public class PurchaseOrderItemRepository : IPurchaseOrderItemRepository
    {
        private readonly AppDbContext _context;

        public PurchaseOrderItemRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PurchaseOrderItem> AddAsync(PurchaseOrderItem purchaseOrderItem)
        {
            await _context.PurchaseOrderItems.AddAsync(purchaseOrderItem);

            await _context.SaveChangesAsync();

            return purchaseOrderItem;
        }

        public async Task DeleteAsync(PurchaseOrderItem purchaseOrderItem)
        {
            _context.PurchaseOrderItems.Remove(purchaseOrderItem);

            await _context.SaveChangesAsync();
        }

        public async Task<List<PurchaseOrderItem>> GetAllAsync()
        {
            return await _context.PurchaseOrderItems
                .Include(poi => poi.PurchaseOrder)
                .Include(poi => poi.Product)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<PurchaseOrderItem?> GetByIdAsync(Guid id)
        {
            return await _context.PurchaseOrderItems
                .Include(poi => poi.PurchaseOrder)
                .Include(poi => poi.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(poi => poi.Id == id);
        }

        public async Task<PurchaseOrderItem> UpdateAsync(PurchaseOrderItem purchaseOrderItem)
        {
            _context.PurchaseOrderItems.Update(purchaseOrderItem);

            await _context.SaveChangesAsync();

            return purchaseOrderItem;
        }
    }
}
