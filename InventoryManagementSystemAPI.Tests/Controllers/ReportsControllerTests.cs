using InventoryManagementSystemAPI.Controllers;
using InventoryManagementSystemAPI.DTOs.Report;
using InventoryManagementSystemAPI.Models;
using InventoryManagementSystemAPI.Services.Implementations;
using InventoryManagementSystemAPI.Tests.Fakes;
using Microsoft.AspNetCore.Http;
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
            Assert.Contains(reportList, item => item.ProductId == prod1.Id && item.TotalStock == 3);
            Assert.Contains(reportList, item => item.ProductId == prod2.Id && item.TotalStock == 10);
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

        [Fact]
        public async Task GetLowStockSummary_WhenCalledWithThreshold_ReturnsLlmSummary()
        {
            // Arrange
            var prod = new Product { Id = Guid.NewGuid(), Name = "Low Item", SKU = "LOW-99", QuantityInStock = 2, ReorderLevel = 10 };
            var fakeRepo = new FakeReportRepository(new[] { prod });
            var service = new ReportService(fakeRepo);
            var fakeLlm = new FakeLlmClient("Reorder recommendation: purchase 20 units of LOW-99 immediately.");
            var controller = new ReportsController(service, fakeLlm);

            // Act
            var actionResult = await controller.GetLowStockSummary(threshold: 5);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var response = Assert.IsType<LowStockSummaryResponseDto>(okResult.Value);
            Assert.Equal(5, response.Threshold);
            Assert.Equal(1, response.ItemCount);
            Assert.Equal("Reorder recommendation: purchase 20 units of LOW-99 immediately.", response.Summary);
        }

        [Fact]
        public async Task GetLowStockSummary_WhenLlmThrowsException_Returns503ServiceUnavailable()
        {
            // Arrange
            var prod = new Product { Id = Guid.NewGuid(), Name = "Low Item", SKU = "LOW-99", QuantityInStock = 2, ReorderLevel = 10 };
            var fakeRepo = new FakeReportRepository(new[] { prod });
            var service = new ReportService(fakeRepo);
            var fakeLlm = new FakeLlmClient(new HttpRequestException("External API network failure"));
            var controller = new ReportsController(service, fakeLlm);

            // Act
            var actionResult = await controller.GetLowStockSummary(threshold: 5);

            // Assert: 503 Service Unavailable
            var statusResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status503ServiceUnavailable, statusResult.StatusCode);
        }

        [Fact]
        public async Task GetLowStockSummary_WhenThresholdNegative_ReturnsBadRequest()
        {
            var fakeRepo = new FakeReportRepository();
            var service = new ReportService(fakeRepo);
            var fakeLlm = new FakeLlmClient("summary");
            var controller = new ReportsController(service, fakeLlm);

            var actionResult = await controller.GetLowStockSummary(threshold: -5);

            Assert.IsType<BadRequestObjectResult>(actionResult);
        }

        [Fact]
        public async Task GetLowStockSummary_WhenNoItemsLowStock_ReturnsFriendlyMessageWithoutCallingLlm()
        {
            var fakeRepo = new FakeReportRepository(); // empty
            var service = new ReportService(fakeRepo);
            var fakeLlm = new FakeLlmClient("Should not be called");
            var controller = new ReportsController(service, fakeLlm);

            var actionResult = await controller.GetLowStockSummary(threshold: 5);

            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var response = Assert.IsType<LowStockSummaryResponseDto>(okResult.Value);
            Assert.Equal(0, response.ItemCount);
            Assert.Contains("No immediate reorders required", response.Summary);
        }
    }
}
