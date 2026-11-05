namespace InventoryManagementSystemAPI.Models
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string Barcode {  get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public int QuantityInStock { get; set; }
        public int ReorderLevel { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Relationships
        public Guid CategoryId { get; set; } 
        public Category? Category { get; set; }
        public Guid SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
        public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = [];
    }
}
