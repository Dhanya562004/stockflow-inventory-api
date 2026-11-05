using InventoryManagementSystemAPI.DTOs.Report;
using InventoryManagementSystemAPI.Repositories.Interfaces;
using InventoryManagementSystemAPI.Services.Interfaces;

namespace InventoryManagementSystemAPI.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<List<LowStockReportDto>> GetLowStockReportAsync(int threshold)
        {
            if (threshold < 0)
            {
                throw new ArgumentException("Threshold must be greater than or equal to zero.", nameof(threshold));
            }

            return await _reportRepository.GetLowStockReportAsync(threshold);
        }
    }
}
