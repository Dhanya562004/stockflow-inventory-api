using InventoryManagementSystemAPI.DTOs.Report;

namespace InventoryManagementSystemAPI.Repositories.Interfaces
{
    public interface IReportRepository
    {
        Task<List<LowStockReportDto>> GetLowStockReportAsync(int threshold);
    }
}
