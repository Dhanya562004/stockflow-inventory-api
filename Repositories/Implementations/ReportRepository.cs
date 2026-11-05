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
            // Parameterized raw SQL query joining Products and StockLevels, grouped by product and filtered by SUM(Quantity) <= @threshold
            return await _context.Database.SqlQuery<LowStockReportDto>($@"
                SELECT 
                    p.Id AS ProductId,
                    p.Name AS ProductName,
                    p.SKU AS SKU,
                    CAST(COALESCE(SUM(sl.Quantity), 0) AS int) AS TotalStock,
                    p.ReorderLevel AS ReorderLevel
                FROM Products p
                INNER JOIN StockLevels sl ON p.Id = sl.ProductId
                GROUP BY p.Id, p.Name, p.SKU, p.ReorderLevel
                HAVING SUM(sl.Quantity) <= {threshold}
                ORDER BY TotalStock ASC").ToListAsync();
        }
    }
}
