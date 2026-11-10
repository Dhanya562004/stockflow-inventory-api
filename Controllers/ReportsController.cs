using System.Text;
using System.Text.Json;
using InventoryManagementSystemAPI.DTOs.Report;
using InventoryManagementSystemAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILlmClient? _llmClient;
        private readonly IWebHostEnvironment? _environment;
        private static string? _cachedPromptTemplate;

        public ReportsController(
            IReportService reportService,
            ILlmClient? llmClient = null,
            IWebHostEnvironment? environment = null)
        {
            _reportService = reportService;
            _llmClient = llmClient;
            _environment = environment;
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
                    Summary = "All products currently meet or exceed the inventory threshold. No immediate reorders required.",
                    Analysis = new LowStockSummaryAnalysisDto
                    {
                        Summary = "All products currently meet or exceed the inventory threshold. No immediate reorders required.",
                        Items = new List<string>(),
                        RecommendedAction = "No reorders required."
                    }
                });
            }

            string promptTemplate;
            try
            {
                promptTemplate = LoadPromptTemplate(_environment);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Prompt template configuration error." });
            }

            var itemsBuilder = new StringBuilder();
            foreach (var item in report)
            {
                itemsBuilder.AppendLine($"- Product: {item.ProductName} (SKU: {item.SKU}), Current Stock: {item.TotalStock}, Reorder Level: {item.ReorderLevel}");
            }

            var prompt = promptTemplate
                .Replace("{{threshold}}", threshold.ToString())
                .Replace("{{itemsList}}", itemsBuilder.ToString().TrimEnd());

            string rawModelOutput;
            try
            {
                rawModelOutput = await _llmClient.GenerateSummaryAsync(prompt, cancellationToken);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "LLM summary service is currently unavailable." });
            }

            // Clean markdown code blocks if the model wrapped the JSON
            var cleanedOutput = rawModelOutput.Trim();
            if (cleanedOutput.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            {
                cleanedOutput = cleanedOutput.Substring(7);
            }
            else if (cleanedOutput.StartsWith("```"))
            {
                cleanedOutput = cleanedOutput.Substring(3);
            }

            if (cleanedOutput.EndsWith("```"))
            {
                cleanedOutput = cleanedOutput.Substring(0, cleanedOutput.Length - 3);
            }
            cleanedOutput = cleanedOutput.Trim();

            LowStockSummaryAnalysisDto? analysis;
            try
            {
                analysis = JsonSerializer.Deserialize<LowStockSummaryAnalysisDto>(cleanedOutput, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new { message = "LLM provider returned invalid JSON output." });
            }

            // Schema validation: non-empty summary, non-null items array, non-empty recommended action
            if (analysis == null
                || string.IsNullOrWhiteSpace(analysis.Summary)
                || analysis.Items == null
                || string.IsNullOrWhiteSpace(analysis.RecommendedAction))
            {
                return StatusCode(StatusCodes.Status502BadGateway, new { message = "LLM output did not adhere to required JSON schema." });
            }

            return Ok(new LowStockSummaryResponseDto
            {
                Threshold = threshold,
                ItemCount = report.Count,
                Summary = analysis.Summary,
                Analysis = analysis
            });
        }

        private static string LoadPromptTemplate(IWebHostEnvironment? env)
        {
            if (_cachedPromptTemplate != null)
            {
                return _cachedPromptTemplate;
            }

            var candidates = new List<string>
            {
                Path.Combine(AppContext.BaseDirectory, "Prompts", "low-stock-summary.v1.txt"),
                Path.Combine(Directory.GetCurrentDirectory(), "Prompts", "low-stock-summary.v1.txt")
            };

            if (env != null && !string.IsNullOrWhiteSpace(env.ContentRootPath))
            {
                candidates.Insert(0, Path.Combine(env.ContentRootPath, "Prompts", "low-stock-summary.v1.txt"));
            }

            foreach (var path in candidates)
            {
                if (System.IO.File.Exists(path))
                {
                    _cachedPromptTemplate = System.IO.File.ReadAllText(path);
                    return _cachedPromptTemplate;
                }
            }

            throw new FileNotFoundException("Versioned prompt template Prompts/low-stock-summary.v1.txt not found.");
        }
    }
}
