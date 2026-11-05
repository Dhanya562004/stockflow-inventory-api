namespace InventoryManagementSystemAPI.Models
{
    public class PurchaseOrderItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int Quantity { get; set; } = 0;
        public decimal Cost { get; set; } = 0;

        // Relationships
        public Guid PurchaseOrderId { get; set; }
        public PurchaseOrder? PurchaseOrder { get; set; }
        public Guid ProductId {  get; set; }
        public Product? Product { get; set; }
    }
}
