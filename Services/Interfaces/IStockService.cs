using InventoryManagementSystemAPI.DTOs.Stock;

namespace InventoryManagementSystemAPI.Services.Interfaces
{
    public interface IStockService
    {
        Task<StockAdjustmentResultDto> AdjustStockAsync(Guid productId, int delta);
        Task<StockAdjustmentResultDto> AdjustStockAsync(Guid productId, string locationId, int delta);
    }
}
