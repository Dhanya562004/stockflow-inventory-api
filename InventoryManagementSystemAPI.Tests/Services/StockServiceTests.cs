using InventoryManagementSystemAPI.Exceptions;
using InventoryManagementSystemAPI.Models;
using InventoryManagementSystemAPI.Services.Implementations;
using InventoryManagementSystemAPI.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryManagementSystemAPI.Tests.Services
{
    public class StockServiceTests
    {
        [Fact]
        public async Task AdjustStockAsync_WhenAdjustmentStaysPositive_UpdatesQuantityAndReturnsSuccess()
        {
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Widget",
                SKU = "WDG-01",
                QuantityInStock = 10
            };
            var fakeRepo = new FakeStockRepository(new[] { product });
            var service = new StockService(fakeRepo);

            var result = await service.AdjustStockAsync(productId, 5);

            Assert.NotNull(result);
            Assert.Equal(15, result.NewQuantity);
            Assert.Equal(10, result.PreviousQuantity);
            Assert.Equal(5, result.Delta);
            Assert.Equal(15, product.QuantityInStock);
        }

        [Fact]
        public async Task AdjustStockAsync_WhenAdjustmentGoesToExactlyZero_UpdatesQuantityToZero()
        {
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Widget",
                SKU = "WDG-01",
                QuantityInStock = 10
            };
            var fakeRepo = new FakeStockRepository(new[] { product });
            var service = new StockService(fakeRepo);

            var result = await service.AdjustStockAsync(productId, -10);

            Assert.NotNull(result);
            Assert.Equal(0, result.NewQuantity);
            Assert.Equal(10, result.PreviousQuantity);
            Assert.Equal(-10, result.Delta);
            Assert.Equal(0, product.QuantityInStock);
        }

        [Fact]
        public async Task AdjustStockAsync_WhenAdjustmentGoesNegative_ThrowsInsufficientStockException()
        {
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Widget",
                SKU = "WDG-01",
                QuantityInStock = 5
            };
            var fakeRepo = new FakeStockRepository(new[] { product });
            var service = new StockService(fakeRepo);

            var ex = await Assert.ThrowsAsync<InsufficientStockException>(() => service.AdjustStockAsync(productId, -6));

            Assert.Contains("resulting quantity cannot be negative", ex.Message);
            Assert.Equal(5, product.QuantityInStock); // Unchanged
        }

        [Fact]
        public async Task AdjustStockAsync_WhenExceedsRetryLimit_ThrowsDbUpdateConcurrencyException()
        {
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Widget",
                SKU = "WDG-01",
                QuantityInStock = 20
            };
            var fakeRepo = new FakeStockRepository(new[] { product })
            {
                ConcurrencyExceptionsToThrow = 5
            };
            var service = new StockService(fakeRepo);

            var ex = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => service.AdjustStockAsync(productId, -2));

            Assert.NotNull(ex);
            Assert.Equal(3, fakeRepo.UpdateCallsCount);
        }

        [Fact]
        public async Task AdjustStockAsync_WhenTransientConcurrencyConflict_RetriesAndSucceeds()
        {
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Widget",
                SKU = "WDG-01",
                QuantityInStock = 20
            };
            var fakeRepo = new FakeStockRepository(new[] { product })
            {
                ConcurrencyExceptionsToThrow = 2 // Throws on attempt 1 and 2, succeeds on attempt 3
            };
            var service = new StockService(fakeRepo);

            var result = await service.AdjustStockAsync(productId, -5);

            Assert.NotNull(result);
            Assert.Equal(15, result.NewQuantity);
            Assert.Equal(3, fakeRepo.UpdateCallsCount);
            Assert.Equal(2, fakeRepo.ConcurrencyExceptionsThrown);
        }

        [Fact]
        public async Task AdjustStockAsync_WhenProductDoesNotExist_ThrowsNotFoundException()
        {
            var fakeRepo = new FakeStockRepository();
            var service = new StockService(fakeRepo);

            await Assert.ThrowsAsync<NotFoundException>(() => service.AdjustStockAsync(Guid.NewGuid(), 5));
        }
    }
}
