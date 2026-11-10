using InventoryManagementSystemAPI.Models;
using InventoryManagementSystemAPI.Services.Implementations;
using InventoryManagementSystemAPI.Tests.Fakes;
using Xunit;

namespace InventoryManagementSystemAPI.Tests.Services
{
    public class ReportServiceTests
    {
        [Fact]
        public async Task GetLowStockReportAsync_WhenStockEqualsThreshold_IncludesItemInReport()
        {
            // Boundary condition: p.QuantityInStock == threshold must be included (<= operator)
            var threshold = 10;
            var boundaryProduct = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Boundary Item",
                SKU = "BND-01",
                QuantityInStock = 10, // Exactly equals threshold
                ReorderLevel = 15
            };
            var aboveProduct = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Above Threshold Item",
                SKU = "ABV-01",
                QuantityInStock = 11, // Exceeds threshold by 1
                ReorderLevel = 15
            };

            var fakeRepo = new FakeReportRepository(new[] { boundaryProduct, aboveProduct });
            var service = new ReportService(fakeRepo);

            var report = await service.GetLowStockReportAsync(threshold);

            Assert.NotNull(report);
            Assert.Single(report);
            Assert.Equal("Boundary Item", report[0].ProductName);
            Assert.Equal(10, report[0].TotalStock);
        }

        [Fact]
        public async Task GetLowStockReportAsync_WhenStockExceedsThresholdByOne_ExcludesItemFromReport()
        {
            var threshold = 5;
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Stocked Item",
                SKU = "STK-01",
                QuantityInStock = 6, // 5 + 1
                ReorderLevel = 10
            };

            var fakeRepo = new FakeReportRepository(new[] { product });
            var service = new ReportService(fakeRepo);

            var report = await service.GetLowStockReportAsync(threshold);

            Assert.NotNull(report);
            Assert.Empty(report);
        }

        [Fact]
        public async Task GetLowStockReportAsync_WhenThresholdIsNegative_ThrowsArgumentException()
        {
            var fakeRepo = new FakeReportRepository();
            var service = new ReportService(fakeRepo);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GetLowStockReportAsync(-1));

            Assert.Contains("Threshold must be greater than or equal to zero", ex.Message);
        }

        [Fact]
        public async Task GetLowStockReportAsync_WhenThresholdIsZero_ReturnsOnlyZeroStockItems()
        {
            var zeroStockProduct = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Depleted Item",
                SKU = "DEP-01",
                QuantityInStock = 0,
                ReorderLevel = 10
            };
            var positiveStockProduct = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Normal Item",
                SKU = "NRM-01",
                QuantityInStock = 1,
                ReorderLevel = 10
            };

            var fakeRepo = new FakeReportRepository(new[] { zeroStockProduct, positiveStockProduct });
            var service = new ReportService(fakeRepo);

            var report = await service.GetLowStockReportAsync(0);

            Assert.NotNull(report);
            Assert.Single(report);
            Assert.Equal("Depleted Item", report[0].ProductName);
            Assert.Equal(0, report[0].TotalStock);
        }
    }
}
