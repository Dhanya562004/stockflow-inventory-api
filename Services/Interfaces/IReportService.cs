using InventoryManagementSystemAPI.DTOs.Report;

namespace InventoryManagementSystemAPI.Services.Interfaces
{
    public interface IReportService
    {
        Task<List<LowStockReportDto>> GetLowStockReportAsync(int threshold);
    }
}
