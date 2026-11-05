using InventoryManagementSystemAPI.Data;
using InventoryManagementSystemAPI.Models;
using InventoryManagementSystemAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystemAPI.Repositories.Implementations
{
    public class PurchaseOrderRepository : IPurchaseOrderRepository
    {
        private readonly AppDbContext _context;

        public PurchaseOrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PurchaseOrder> AddAsync(PurchaseOrder purchaseOrder)
        {
            await _context.PurchaseOrders.AddAsync(purchaseOrder);

            await _context.SaveChangesAsync();

            return purchaseOrder;
        }

        public async Task DeleteAsync(PurchaseOrder purchaseOrder)
        {
            _context.PurchaseOrders.Remove(purchaseOrder);

            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByOrderNumberAsync(string orderNumber, Guid? excludeId = null)
        {
            return await _context.PurchaseOrders.AnyAsync(po => po.OrderNumber == orderNumber && 
                (excludeId == null || po.Id != excludeId));
        }

        public async Task<List<PurchaseOrder>> GetAllAsync()
        {
            return await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.PurchaseOrderItems)
                    .ThenInclude(poi => poi.Product)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<PurchaseOrder?> GetByIdAsync(Guid id)
        {
            return await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.PurchaseOrderItems)
                    .ThenInclude(poi => poi.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(po => po.Id == id);
        }

        public async Task<PurchaseOrder> UpdateAsync(PurchaseOrder purchaseOrder)
        {
            _context.PurchaseOrders.Update(purchaseOrder);

            await _context.SaveChangesAsync();

            return purchaseOrder;
        }
    }
}
