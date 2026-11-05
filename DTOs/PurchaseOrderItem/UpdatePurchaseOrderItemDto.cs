namespace InventoryManagementSystemAPI.DTOs.PurchaseOrderItem
{
    public class UpdatePurchaseOrderItemDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int Quantity { get; set; } = 0;
        public decimal Cost { get; set; } = 0;
        public Guid PurchaseOrderId { get; set; }
        public Guid ProductId { get; set; }
    }
}
