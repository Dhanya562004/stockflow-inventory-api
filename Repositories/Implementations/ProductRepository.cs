using InventoryManagementSystemAPI.Data;
using InventoryManagementSystemAPI.Models;
using InventoryManagementSystemAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystemAPI.Repositories.Implementations
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context) 
        {
            _context = context;
        }

        public async Task<Product> AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);

            await _context.SaveChangesAsync();

            return product;
        }

        public async Task DeleteAsync(Product product)
        {
            _context.Products.Remove(product);

            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByBarcodeAsync(string barcode, Guid? excludeId = null)
        {
            return await _context.Products.AnyAsync(p => p.Barcode == barcode
                && (excludeId == null || p.Id != excludeId));
        }

        public async Task<bool> ExistsBySkuAsync(string sku, Guid? excludeId = null)
        {
            return await _context.Products.AnyAsync(p => p.SKU == sku
                && (excludeId == null || p.Id != excludeId));
        }

        public async Task<List<Product>> GetAllAsync() 
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> UpdateAsync(Product product)
        {
            _context.Products.Update(product);

            await _context.SaveChangesAsync();

            return product;
        }
    }
}
