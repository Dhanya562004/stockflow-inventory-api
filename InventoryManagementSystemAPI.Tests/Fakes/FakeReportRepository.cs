using InventoryManagementSystemAPI.DTOs.Report;
using InventoryManagementSystemAPI.Models;
using InventoryManagementSystemAPI.Repositories.Interfaces;

namespace InventoryManagementSystemAPI.Tests.Fakes
{
    public class FakeReportRepository : IReportRepository
    {
        private readonly List<Product> _products;

        public FakeReportRepository(IEnumerable<Product>? products = null)
        {
            _products = products != null ? products.ToList() : new List<Product>();
        }

        public Task<List<LowStockReportDto>> GetLowStockReportAsync(int threshold)
        {
            var lowStockItems = _products
                .Where(p => p.QuantityInStock <= threshold)
                .OrderBy(p => p.QuantityInStock)
                .Select(p => new LowStockReportDto
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    SKU = p.SKU,
                    CurrentStock = p.QuantityInStock,
                    ReorderLevel = p.ReorderLevel
                })
                .ToList();

            return Task.FromResult(lowStockItems);
        }
    }
}
