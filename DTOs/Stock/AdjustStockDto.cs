namespace InventoryManagementSystemAPI.DTOs.Stock
{
    public class AdjustStockDto
    {
        public Guid ProductId { get; set; }
        public string? LocationId { get; set; }
        public int Delta { get; set; }
    }
}
