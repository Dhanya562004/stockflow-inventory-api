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
            // Traverse upwards from BaseDirectory to find Views/Stock/Index.cshtml
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            string? foundPath = null;

            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "Views", "Stock", "Index.cshtml");
                if (File.Exists(candidate))
                {
                    foundPath = candidate;
                    break;
                }
                dir = dir.Parent;
            }

            Assert.NotNull(foundPath);
            var content = File.ReadAllText(foundPath);

            // Assert that the plain JavaScript fetch calls the exact API endpoint /api/stock/adjust
            Assert.Contains("fetch('/api/stock/adjust'", content);
            Assert.Contains("method: 'POST'", content);
            // Assert that textContent is used rather than innerHTML
            Assert.Contains("textContent", content);
            Assert.DoesNotContain("innerHTML", content);
        }

        private class StubStockService : IStockService
        {
            public Task<StockAdjustmentResultDto> AdjustStockAsync(Guid productId, int delta)
            {
                return Task.FromResult(new StockAdjustmentResultDto());
            }
        }
    }
}
