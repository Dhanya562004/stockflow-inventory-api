namespace InventoryManagementSystemAPI.DTOs.Stock
{
    public class StockAdjustmentResultDto
    {
        public Guid ProductId { get; set; }
        public string? LocationId { get; set; }
        public int NewQuantity { get; set; }
        public int PreviousQuantity { get; set; }
        public int Delta { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
