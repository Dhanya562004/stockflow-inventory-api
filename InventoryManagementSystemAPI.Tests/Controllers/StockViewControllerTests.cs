using InventoryManagementSystemAPI.Controllers;
using InventoryManagementSystemAPI.DTOs.Stock;
using InventoryManagementSystemAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace InventoryManagementSystemAPI.Tests.Controllers
{
    public class StockViewControllerTests
    {
        [Fact]
        public void Index_ReturnsViewResultWithCorrectViewPath()
        {
            var fakeService = new StubStockService();
            var controller = new StockController(fakeService);

            var result = controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Stock/Index.cshtml", viewResult.ViewName);
        }

        [Fact]
        public void StockView_FormEndpointUrlMatchesApiRoute()
        {
            // Traverse upwards from BaseDirectory to find Views/Stock/Index.cshtml and wwwroot/js/stock.js
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            string? foundViewPath = null;
            string? foundJsPath = null;

            while (dir != null)
            {
                var candidateView = Path.Combine(dir.FullName, "Views", "Stock", "Index.cshtml");
                var candidateJs = Path.Combine(dir.FullName, "wwwroot", "js", "stock.js");

                if (File.Exists(candidateView) && File.Exists(candidateJs))
                {
                    foundViewPath = candidateView;
                    foundJsPath = candidateJs;
                    break;
                }
                dir = dir.Parent;
            }

            Assert.NotNull(foundViewPath);
            Assert.NotNull(foundJsPath);

            var viewContent = File.ReadAllText(foundViewPath);
            Assert.Contains("/js/stock.js", viewContent);
            Assert.Contains("id=\"stockAdjustmentForm\"", viewContent);
            Assert.Contains("id=\"productTable\"", viewContent);
            Assert.Contains("id=\"emptyState\"", viewContent);

            var jsContent = File.ReadAllText(foundJsPath);
            // Assert that the plain JavaScript fetch calls the exact API endpoint /api/stock/adjust
            Assert.Contains("fetch('/api/stock/adjust'", jsContent);
            Assert.Contains("method: 'POST'", jsContent);
            // Assert that textContent is used rather than innerHTML
            Assert.Contains("textContent", jsContent);
            Assert.DoesNotContain("innerHTML", jsContent);
        }

        private class StubStockService : IStockService
        {
            public Task<StockAdjustmentResultDto> AdjustStockAsync(Guid productId, int delta)
            {
                return Task.FromResult(new StockAdjustmentResultDto());
            }

            public Task<StockAdjustmentResultDto> AdjustStockAsync(Guid productId, string locationId, int delta)
            {
                return Task.FromResult(new StockAdjustmentResultDto { LocationId = locationId });
            }
        }
    }
}
