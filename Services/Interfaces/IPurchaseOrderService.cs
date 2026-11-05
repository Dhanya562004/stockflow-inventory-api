using InventoryManagementSystemAPI.DTOs.PurchaseOrder;

namespace InventoryManagementSystemAPI.Services.Interfaces
{
    public interface IPurchaseOrderService
    {
        Task<List<PurchaseOrderDto>> GetAllAsync();

        Task<PurchaseOrderDto?> GetByIdAsync(Guid id);

        Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto dto);

        Task<PurchaseOrderDto?> UpdateAsync(Guid id, UpdatePurchaseOrderDto dto);

        Task DeleteAsync(Guid id);
    }
}
