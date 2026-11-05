using InventoryManagementSystemAPI.Data;
using InventoryManagementSystemAPI.Models;
using InventoryManagementSystemAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystemAPI.Repositories.Implementations
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly AppDbContext _context;

        public SupplierRepository(AppDbContext context) 
        {
            _context = context;
        }

        public async Task<Supplier> AddAsync(Supplier supplier)
        {
            await _context.Suppliers.AddAsync(supplier);

            await _context.SaveChangesAsync();

            return supplier;
        }

        public async Task DeleteAsync(Supplier supplier)
        {
            _context.Suppliers.Remove(supplier);

            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null)
        {
            return await _context.Suppliers.AnyAsync(s => s.Email == email && (excludeId == null || s.Id != excludeId));
        }

        public async Task<List<Supplier>> GetAllAsync()
        {
            return await _context.Suppliers
                .Include(s => s.PurchaseOrders)
                .Include(s => s.Products)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Supplier?> GetByIdAsync(Guid id)
        {
            return await _context.Suppliers
                .Include(s => s.PurchaseOrders)
                .Include(s => s.Products)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Supplier> UpdateAsync(Supplier supplier)
        {
            _context.Suppliers.Update(supplier);

            await _context.SaveChangesAsync();

            return supplier;
        }
    }
}
