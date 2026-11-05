using InventoryManagementSystemAPI.Enums;

namespace InventoryManagementSystemAPI.Models
{
    public class PurchaseOrder
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public Status Status { get; set; } = Status.PENDING;
        public decimal TotalAmount { get; set; }

        // Relationships
        public Guid SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
        public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = [];
    }
}
