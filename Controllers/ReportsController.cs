using System.Text;
using InventoryManagementSystemAPI.DTOs.Report;
using InventoryManagementSystemAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILlmClient? _llmClient;

        public ReportsController(IReportService reportService, ILlmClient? llmClient = null)
        {
            _reportService = reportService;
            _llmClient = llmClient;
        }

        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStock([FromQuery] int threshold = 10)
        {
            if (threshold < 0)
            {
                return BadRequest(new { message = "Threshold must be greater than or equal to 0." });
            }

            var report = await _reportService.GetLowStockReportAsync(threshold);
            return Ok(report);
        }

        [HttpGet("low-stock/summary")]
        public async Task<IActionResult> GetLowStockSummary([FromQuery] int threshold = 10, CancellationToken cancellationToken = default)
        {
            if (threshold < 0)
            {
                return BadRequest(new { message = "Threshold must be greater than or equal to 0." });
            }

            if (_llmClient == null)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "LLM summary service is currently unavailable." });
            }

            var report = (await _reportService.GetLowStockReportAsync(threshold)).ToList();

            if (!report.Any())
            {
                return Ok(new LowStockSummaryResponseDto
                {
                    Threshold = threshold,
                    ItemCount = 0,
                    Summary = "All products currently meet or exceed the inventory threshold. No immediate reorders required."
                });
            }

            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine($"The following {report.Count} product(s) have stock at or below the reorder threshold of {threshold}:");
            foreach (var item in report)
            {
                promptBuilder.AppendLine($"- Product: {item.ProductName} (SKU: {item.SKU}), Current Stock: {item.TotalStock}, Reorder Level: {item.ReorderLevel}");
            }
            promptBuilder.AppendLine("Please provide a concise, prioritized reorder recommendation for warehouse inventory management.");

            try
            {
                var summary = await _llmClient.GenerateSummaryAsync(promptBuilder.ToString(), cancellationToken);
                return Ok(new LowStockSummaryResponseDto
                {
                    Threshold = threshold,
                    ItemCount = report.Count,
                    Summary = summary
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "LLM summary service is currently unavailable." });
            }
        }
    }
}
