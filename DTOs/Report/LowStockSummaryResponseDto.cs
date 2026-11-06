using System.Text.Json.Serialization;

namespace InventoryManagementSystemAPI.DTOs.Report
{
    public class LowStockSummaryAnalysisDto
    {
        [JsonPropertyName("summary")]
        public string Summary { get; set; } = string.Empty;

        [JsonPropertyName("items")]
        public List<string> Items { get; set; } = new();

        [JsonPropertyName("recommendedAction")]
        public string RecommendedAction { get; set; } = string.Empty;
    }

    public class LowStockSummaryResponseDto
    {
        public int Threshold { get; set; }
        public int ItemCount { get; set; }
        public string Summary { get; set; } = string.Empty;
        public LowStockSummaryAnalysisDto Analysis { get; set; } = new();
    }
}
