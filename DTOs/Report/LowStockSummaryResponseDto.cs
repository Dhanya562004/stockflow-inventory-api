namespace InventoryManagementSystemAPI.DTOs.Report
{
    public class LowStockSummaryResponseDto
    {
        public int Threshold { get; set; }
        public int ItemCount { get; set; }
        public string Summary { get; set; } = string.Empty;
    }
}
