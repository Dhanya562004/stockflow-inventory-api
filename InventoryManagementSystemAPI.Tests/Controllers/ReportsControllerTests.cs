using InventoryManagementSystemAPI.Controllers;
using InventoryManagementSystemAPI.DTOs.Report;
using InventoryManagementSystemAPI.Models;
using InventoryManagementSystemAPI.Services.Implementations;
using InventoryManagementSystemAPI.Tests.Fakes;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace InventoryManagementSystemAPI.Tests.Controllers
{
    public class ReportsControllerTests
    {
        [Fact]
        public async Task GetLowStock_WhenCalledWithThreshold_ReturnsOnlyProductsAtOrBelowThreshold()
        {
            // Arrange products with various stock levels: 3, 10, 15, 50
            var prod1 = new Product { Id = Guid.NewGuid(), Name = "Low Item 1", SKU = "LOW-01", QuantityInStock = 3, ReorderLevel = 5 };
            var prod2 = new Product { Id = Guid.NewGuid(), Name = "Threshold Item", SKU = "MID-01", QuantityInStock = 10, ReorderLevel = 10 };
            var prod3 = new Product { Id = Guid.NewGuid(), Name = "High Item 1", SKU = "HIGH-01", QuantityInStock = 15, ReorderLevel = 10 };
            var prod4 = new Product { Id = Guid.NewGuid(), Name = "High Item 2", SKU = "HIGH-02", QuantityInStock = 50, ReorderLevel = 20 };

            var fakeRepo = new FakeReportRepository(new[] { prod1, prod2, prod3, prod4 });
            var service = new ReportService(fakeRepo);
            var controller = new ReportsController(service);

            // Act: request threshold of 10
            var actionResult = await controller.GetLowStock(threshold: 10);

            // Assert: OkObjectResult with only products with QuantityInStock <= 10
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var report = Assert.IsAssignableFrom<IEnumerable<LowStockReportDto>>(okResult.Value);
            var reportList = report.ToList();

            Assert.Equal(2, reportList.Count);
            Assert.Contains(reportList, item => item.ProductId == prod1.Id && item.CurrentStock == 3);
            Assert.Contains(reportList, item => item.ProductId == prod2.Id && item.CurrentStock == 10);
            Assert.DoesNotContain(reportList, item => item.ProductId == prod3.Id);
            Assert.DoesNotContain(reportList, item => item.ProductId == prod4.Id);
        }

        [Fact]
        public async Task GetLowStock_WhenThresholdNegative_ReturnsBadRequest()
        {
            var fakeRepo = new FakeReportRepository();
            var service = new ReportService(fakeRepo);
            var controller = new ReportsController(service);

            var actionResult = await controller.GetLowStock(threshold: -1);

            Assert.IsType<BadRequestObjectResult>(actionResult);
        }
    }
}
