namespace InventoryManagementSystemAPI.Models
{
    public class StockLevel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }
        public string LocationId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public byte[] RowVersion { get; set; } = [];
    }
}
