namespace InventoryManagementSystemAPI.DTOs.Report
{
    public class LowStockReportDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int TotalStock { get; set; }
        public int ReorderLevel { get; set; }
    }
}
