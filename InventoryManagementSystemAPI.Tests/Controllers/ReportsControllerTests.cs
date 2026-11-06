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
            var prod1 = new Product { Id = Guid.NewGuid(), Name = "Low Item 1", SKU = "LOW-01", QuantityInStock = 3, ReorderLevel = 5 };
            var prod2 = new Product { Id = Guid.NewGuid(), Name = "Threshold Item", SKU = "MID-01", QuantityInStock = 10, ReorderLevel = 10 };
            var prod3 = new Product { Id = Guid.NewGuid(), Name = "High Item 1", SKU = "HIGH-01", QuantityInStock = 15, ReorderLevel = 10 };
            var prod4 = new Product { Id = Guid.NewGuid(), Name = "High Item 2", SKU = "HIGH-02", QuantityInStock = 50, ReorderLevel = 20 };

            var fakeRepo = new FakeReportRepository(new[] { prod1, prod2, prod3, prod4 });
            var service = new ReportService(fakeRepo);
            var controller = new ReportsController(service);

            var actionResult = await controller.GetLowStock(threshold: 10);

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
        public async Task GetLowStockSummary_WhenCalledWithValidSchema_Returns200WithStructuredAnalysis()
        {
            var prod = new Product { Id = Guid.NewGuid(), Name = "Low Item", SKU = "LOW-99", QuantityInStock = 2, ReorderLevel = 10 };
            var fakeRepo = new FakeReportRepository(new[] { prod });
            var service = new ReportService(fakeRepo);

            var validJson = "{\"summary\": \"Critical shortage for LOW-99.\", \"items\": [\"LOW-99\"], \"recommendedAction\": \"Order 50 units immediately.\" }";
            var fakeLlm = new FakeLlmClient(validJson);
            var controller = new ReportsController(service, fakeLlm);

            var actionResult = await controller.GetLowStockSummary(threshold: 5);

            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var response = Assert.IsType<LowStockSummaryResponseDto>(okResult.Value);
            Assert.Equal(5, response.Threshold);
            Assert.Equal(1, response.ItemCount);
            Assert.Equal("Critical shortage for LOW-99.", response.Summary);
            Assert.Equal("Order 50 units immediately.", response.Analysis.RecommendedAction);
            Assert.Contains("LOW-99", response.Analysis.Items);
        }

        [Fact]
        public async Task GetLowStockSummary_WhenLlmReturnsInvalidJson_Returns502BadGateway()
        {
            var prod = new Product { Id = Guid.NewGuid(), Name = "Low Item", SKU = "LOW-99", QuantityInStock = 2, ReorderLevel = 10 };
            var fakeRepo = new FakeReportRepository(new[] { prod });
            var service = new ReportService(fakeRepo);

            // Plain text output without JSON structure
            var fakeLlm = new FakeLlmClient("Here is a summary: LOW-99 is low in stock.");
            var controller = new ReportsController(service, fakeLlm);

            var actionResult = await controller.GetLowStockSummary(threshold: 5);

            var statusResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status502BadGateway, statusResult.StatusCode);
        }

        [Fact]
        public async Task GetLowStockSummary_WhenLlmOutputViolatesSchema_Returns502BadGateway()
        {
            var prod = new Product { Id = Guid.NewGuid(), Name = "Low Item", SKU = "LOW-99", QuantityInStock = 2, ReorderLevel = 10 };
            var fakeRepo = new FakeReportRepository(new[] { prod });
            var service = new ReportService(fakeRepo);

            // Missing required recommendedAction
            var missingFieldJson = "{\"summary\": \"Low stock summary.\", \"items\": [\"LOW-99\"] }";
            var fakeLlm = new FakeLlmClient(missingFieldJson);
            var controller = new ReportsController(service, fakeLlm);

            var actionResult = await controller.GetLowStockSummary(threshold: 5);

            var statusResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status502BadGateway, statusResult.StatusCode);
        }

        [Fact]
        public async Task GetLowStockSummary_WhenLlmThrowsException_Returns503ServiceUnavailable()
        {
            var prod = new Product { Id = Guid.NewGuid(), Name = "Low Item", SKU = "LOW-99", QuantityInStock = 2, ReorderLevel = 10 };
            var fakeRepo = new FakeReportRepository(new[] { prod });
            var service = new ReportService(fakeRepo);
            var fakeLlm = new FakeLlmClient(new HttpRequestException("External API network failure"));
            var controller = new ReportsController(service, fakeLlm);

            var actionResult = await controller.GetLowStockSummary(threshold: 5);

            var statusResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status503ServiceUnavailable, statusResult.StatusCode);
        }

        [Fact]
        public async Task GetLowStockSummary_WhenThresholdNegative_ReturnsBadRequest()
        {
            var fakeRepo = new FakeReportRepository();
            var service = new ReportService(fakeRepo);
            var fakeLlm = new FakeLlmClient("{}");
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
