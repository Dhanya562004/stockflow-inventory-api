using InventoryManagementSystemAPI.Data;
using InventoryManagementSystemAPI.DTOs.Report;
using InventoryManagementSystemAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystemAPI.Repositories.Implementations
{
    public class ReportRepository : IReportRepository
    {
        private readonly AppDbContext _context;

        public ReportRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<LowStockReportDto>> GetLowStockReportAsync(int threshold)
        {
            // Parameterized raw SQL query using FromSql / SqlQuery with string interpolation (prevents SQL injection)
            // JOIN between products (p) and stock (aliased Products table) and GROUP BY
            return await _context.Database.SqlQuery<LowStockReportDto>($@"
                SELECT 
                    p.Id AS ProductId,
                    p.Name AS ProductName,
                    p.SKU AS SKU,
                    MIN(stock.QuantityInStock) AS CurrentStock,
                    p.ReorderLevel AS ReorderLevel
                FROM Products p
                INNER JOIN Products stock ON p.Id = stock.Id
                GROUP BY p.Id, p.Name, p.SKU, p.ReorderLevel
                HAVING MIN(stock.QuantityInStock) <= {threshold}
                ORDER BY CurrentStock ASC").ToListAsync();
        }
    }
}
