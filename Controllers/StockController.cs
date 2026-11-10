using InventoryManagementSystemAPI.DTOs.Stock;
using InventoryManagementSystemAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : Controller
    {
        private readonly IStockService _stockService;

        public StockController(IStockService stockService)
        {
            _stockService = stockService;
        }

        [HttpGet("/stock")]
        public IActionResult Index()
        {
            return View("~/Views/Stock/Index.cshtml");
        }

        [HttpPost("adjust")]
        public async Task<IActionResult> AdjustStock([FromBody] AdjustStockDto dto)
        {
            StockAdjustmentResultDto result;
            if (string.IsNullOrWhiteSpace(dto.LocationId))
            {
                result = await _stockService.AdjustStockAsync(dto.ProductId, dto.Delta);
            }
            else
            {
                result = await _stockService.AdjustStockAsync(dto.ProductId, dto.LocationId.Trim(), dto.Delta);
            }

            return Ok(new
            {
                productId = result.ProductId,
                locationId = result.LocationId,
                quantity = result.NewQuantity,
                previousQuantity = result.PreviousQuantity,
                delta = result.Delta,
                message = result.Message
            });
        }

        [HttpPost("locations/adjust")]
        public async Task<IActionResult> AdjustWarehouseStock([FromBody] AdjustWarehouseStockDto dto)
        {
            var result = await _stockService.AdjustStockAsync(dto.ProductId, dto.LocationId.Trim(), dto.Delta);

            return Ok(new
            {
                productId = result.ProductId,
                locationId = result.LocationId,
                quantity = result.NewQuantity,
                previousQuantity = result.PreviousQuantity,
                delta = result.Delta,
                message = result.Message
            });
        }
    }
}
