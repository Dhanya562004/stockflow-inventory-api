using InventoryManagementSystemAPI.DTOs.Stock;

namespace InventoryManagementSystemAPI.Services.Interfaces
{
    public interface IStockService
    {
        Task<StockAdjustmentResultDto> AdjustStockAsync(Guid productId, int delta);
    }
}
