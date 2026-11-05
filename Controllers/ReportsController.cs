using InventoryManagementSystemAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
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
    }
}
