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
            var result = await _stockService.AdjustStockAsync(dto.ProductId, dto.Delta);

            return Ok(new
            {
                productId = result.ProductId,
                quantity = result.NewQuantity,
                previousQuantity = result.PreviousQuantity,
                delta = result.Delta,
                message = result.Message
            });
        }
    }
}
