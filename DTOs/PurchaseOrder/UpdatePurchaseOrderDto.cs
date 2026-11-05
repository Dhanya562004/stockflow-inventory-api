using InventoryManagementSystemAPI.Enums;

namespace InventoryManagementSystemAPI.DTOs.PurchaseOrder
{
    public class UpdatePurchaseOrderDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public Status Status { get; set; } = Status.PENDING;
        public decimal TotalAmount { get; set; }
        public Guid SupplierId { get; set; }
    }
}
