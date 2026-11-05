using InventoryManagementSystemAPI.Models;

namespace InventoryManagementSystemAPI.Repositories.Interfaces
{
    public interface IPurchaseOrderItemRepository
    {
        Task<List<PurchaseOrderItem>> GetAllAsync();

        Task<PurchaseOrderItem?> GetByIdAsync(Guid id);

        Task<PurchaseOrderItem> AddAsync(PurchaseOrderItem purchaseOrderItem);

        Task<PurchaseOrderItem> UpdateAsync(PurchaseOrderItem purchaseOrderItem);

        Task DeleteAsync(PurchaseOrderItem purchaseOrderItem);
    }
}
