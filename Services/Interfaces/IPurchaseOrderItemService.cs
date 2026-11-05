using InventoryManagementSystemAPI.DTOs.PurchaseOrderItem;

namespace InventoryManagementSystemAPI.Services.Interfaces
{
    public interface IPurchaseOrderItemService
    {
        Task<List<PurchaseOrderItemDto>> GetAllAsync();
        Task<PurchaseOrderItemDto?> GetByIdAsync(Guid id);
        Task<PurchaseOrderItemDto> CreateAsync(CreatePurchaseOrderItemDto dto);
        Task<PurchaseOrderItemDto?> UpdateAsync(Guid id, UpdatePurchaseOrderItemDto dto);
        Task DeleteAsync(Guid id);
    }
}
