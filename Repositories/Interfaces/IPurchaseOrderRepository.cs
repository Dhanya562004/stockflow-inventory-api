using InventoryManagementSystemAPI.Models;

namespace InventoryManagementSystemAPI.Repositories.Interfaces
{
    public interface IPurchaseOrderRepository
    {
        Task<List<PurchaseOrder>> GetAllAsync();

        Task<PurchaseOrder?> GetByIdAsync(Guid id);

        Task<PurchaseOrder> AddAsync(PurchaseOrder purchaseOrder);

        Task<PurchaseOrder> UpdateAsync(PurchaseOrder purchaseOrder);

        Task DeleteAsync(PurchaseOrder purchaseOrder);

        Task<bool> ExistsByOrderNumberAsync(string orderNumber, Guid? excludeId = null);
    }
}
