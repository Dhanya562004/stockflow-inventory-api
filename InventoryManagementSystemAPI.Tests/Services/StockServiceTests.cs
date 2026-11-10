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

        // ==========================================
        // Warehouse-Specific Stock Adjustment Tests
        // ==========================================

        [Fact]
        public async Task AdjustStockAsync_WithLocation_WhenAdjustmentStaysPositive_UpdatesWarehouseAndProductQuantities()
        {
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Warehouse Widget",
                SKU = "WH-WDG-01",
                QuantityInStock = 25
            };
            var stockLevel = new StockLevel
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                LocationId = "WH-MAIN",
                Quantity = 15
            };

            var fakeRepo = new FakeStockRepository(new[] { product }, new[] { stockLevel });
            var service = new StockService(fakeRepo);

            var result = await service.AdjustStockAsync(productId, "WH-MAIN", 10);

            Assert.NotNull(result);
            Assert.Equal("WH-MAIN", result.LocationId);
            Assert.Equal(25, result.NewQuantity);
            Assert.Equal(15, result.PreviousQuantity);
            Assert.Equal(10, result.Delta);
            Assert.Equal(35, product.QuantityInStock); // 25 + 10
            Assert.Equal(25, stockLevel.Quantity);      // 15 + 10
        }

        [Fact]
        public async Task AdjustStockAsync_WithLocation_WhenStockLevelDoesNotExistYet_CreatesStockLevelAndUpdatesProduct()
        {
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Warehouse Widget",
                SKU = "WH-WDG-01",
                QuantityInStock = 10
            };

            var fakeRepo = new FakeStockRepository(new[] { product });
            fakeRepo.AddLocation("WH-SECONDARY");
            var service = new StockService(fakeRepo);

            var result = await service.AdjustStockAsync(productId, "WH-SECONDARY", 7);

            Assert.NotNull(result);
            Assert.Equal("WH-SECONDARY", result.LocationId);
            Assert.Equal(7, result.NewQuantity);
            Assert.Equal(0, result.PreviousQuantity);
            Assert.Equal(7, result.Delta);
            Assert.Equal(17, product.QuantityInStock); // 10 + 7

            var createdStockLevel = await fakeRepo.GetStockLevelAsync(productId, "WH-SECONDARY");
            Assert.NotNull(createdStockLevel);
            Assert.Equal(7, createdStockLevel.Quantity);
        }

        [Fact]
        public async Task AdjustStockAsync_WithLocation_WhenStockLevelGoesNegative_ThrowsInsufficientStockException()
        {
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Warehouse Widget",
                SKU = "WH-WDG-01",
                QuantityInStock = 20
            };
            var stockLevel = new StockLevel
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                LocationId = "WH-MAIN",
                Quantity = 5
            };

            var fakeRepo = new FakeStockRepository(new[] { product }, new[] { stockLevel });
            var service = new StockService(fakeRepo);

            var ex = await Assert.ThrowsAsync<InsufficientStockException>(() =>
                service.AdjustStockAsync(productId, "WH-MAIN", -6));

            Assert.Contains("Stock adjustment rejected for location 'WH-MAIN'", ex.Message);
            Assert.Contains("resulting quantity cannot be negative", ex.Message);
            Assert.Equal(20, product.QuantityInStock); // Product unchanged
            Assert.Equal(5, stockLevel.Quantity);      // StockLevel unchanged
        }

        [Fact]
        public async Task AdjustStockAsync_WithLocation_WhenStockLevelDoesNotExistAndDeltaNegative_ThrowsInsufficientStockException()
        {
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Warehouse Widget",
                SKU = "WH-WDG-01",
                QuantityInStock = 20
            };

            var fakeRepo = new FakeStockRepository(new[] { product });
            fakeRepo.AddLocation("WH-MAIN");
            var service = new StockService(fakeRepo);

            var ex = await Assert.ThrowsAsync<InsufficientStockException>(() =>
                service.AdjustStockAsync(productId, "WH-MAIN", -3));

            Assert.Contains("resulting quantity cannot be negative", ex.Message);
            Assert.Equal(20, product.QuantityInStock);
        }

        [Fact]
        public async Task AdjustStockAsync_WithLocation_WhenTotalProductStockWouldGoNegative_ThrowsInsufficientStockException()
        {
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Warehouse Widget",
                SKU = "WH-WDG-01",
                QuantityInStock = 2 // Catalog has 2, but warehouse has 5
            };
            var stockLevel = new StockLevel
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                LocationId = "WH-MAIN",
                Quantity = 5
            };

            var fakeRepo = new FakeStockRepository(new[] { product }, new[] { stockLevel });
            var service = new StockService(fakeRepo);

            var ex = await Assert.ThrowsAsync<InsufficientStockException>(() =>
                service.AdjustStockAsync(productId, "WH-MAIN", -4));

            Assert.Contains("resulting catalog quantity cannot be negative", ex.Message);
            Assert.Equal(2, product.QuantityInStock);
            Assert.Equal(5, stockLevel.Quantity);
        }

        [Fact]
        public async Task AdjustStockAsync_WithLocation_WhenLocationDoesNotExist_ThrowsNotFoundException()
        {
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Warehouse Widget",
                SKU = "WH-WDG-01",
                QuantityInStock = 10
            };

            var fakeRepo = new FakeStockRepository(new[] { product });
            var service = new StockService(fakeRepo);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                service.AdjustStockAsync(productId, "UNKNOWN-WAREHOUSE", 5));

            Assert.Contains("Warehouse location 'UNKNOWN-WAREHOUSE' was not found", ex.Message);
        }

        [Fact]
        public async Task AdjustStockAsync_WithLocation_WhenProductDoesNotExist_ThrowsNotFoundException()
        {
            var fakeRepo = new FakeStockRepository();
            var service = new StockService(fakeRepo);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                service.AdjustStockAsync(Guid.NewGuid(), "WH-MAIN", 5));

            Assert.Contains("was not found", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task AdjustStockAsync_WithLocation_WhenLocationIdNullOrWhitespace_ThrowsArgumentException(string? invalidLocation)
        {
            var fakeRepo = new FakeStockRepository();
            var service = new StockService(fakeRepo);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.AdjustStockAsync(Guid.NewGuid(), invalidLocation!, 5));
        }

        [Fact]
        public async Task AdjustStockAsync_WithLocation_WhenTransientConcurrencyConflict_RetriesAndSucceeds()
        {
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Warehouse Widget",
                SKU = "WH-WDG-01",
                QuantityInStock = 20
            };
            var stockLevel = new StockLevel
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                LocationId = "WH-MAIN",
                Quantity = 10
            };

            var fakeRepo = new FakeStockRepository(new[] { product }, new[] { stockLevel })
            {
                ConcurrencyExceptionsToThrow = 2 // Throws twice, succeeds on attempt 3
            };
            var service = new StockService(fakeRepo);

            var result = await service.AdjustStockAsync(productId, "WH-MAIN", 5);

            Assert.NotNull(result);
            Assert.Equal(15, result.NewQuantity);
            Assert.Equal(10, result.PreviousQuantity);
            Assert.Equal(25, product.QuantityInStock);
            Assert.Equal(15, stockLevel.Quantity);
            Assert.Equal(2, fakeRepo.ConcurrencyExceptionsThrown);
        }

        [Fact]
        public async Task AdjustStockAsync_WithLocation_WhenExceedsRetryLimit_ThrowsDbUpdateConcurrencyException()
        {
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Warehouse Widget",
                SKU = "WH-WDG-01",
                QuantityInStock = 20
            };
            var stockLevel = new StockLevel
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                LocationId = "WH-MAIN",
                Quantity = 10
            };

            var fakeRepo = new FakeStockRepository(new[] { product }, new[] { stockLevel })
            {
                ConcurrencyExceptionsToThrow = 5
            };
            var service = new StockService(fakeRepo);

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
                service.AdjustStockAsync(productId, "WH-MAIN", 5));
        }

        [Fact]
        public async Task AdjustStockAsync_WithLocation_MultipleWarehouses_MaintainsIndependentWarehouseQuantities()
        {
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Multi-Warehouse Widget",
                SKU = "MW-WDG-01",
                QuantityInStock = 50
            };
            var whMain = new StockLevel
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                LocationId = "WH-MAIN",
                Quantity = 30
            };
            var whSecondary = new StockLevel
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                LocationId = "WH-SECONDARY",
                Quantity = 20
            };

            var fakeRepo = new FakeStockRepository(new[] { product }, new[] { whMain, whSecondary });
            var service = new StockService(fakeRepo);

            // 1. Adjust WH-MAIN +10 -> WH-MAIN: 40, Product: 60, WH-SECONDARY: 20
            var res1 = await service.AdjustStockAsync(productId, "WH-MAIN", 10);
            Assert.Equal(40, res1.NewQuantity);
            Assert.Equal(30, res1.PreviousQuantity);
            Assert.Equal(40, whMain.Quantity);
            Assert.Equal(20, whSecondary.Quantity);
            Assert.Equal(60, product.QuantityInStock);

            // 2. Adjust WH-SECONDARY -5 -> WH-SECONDARY: 15, Product: 55, WH-MAIN: 40
            var res2 = await service.AdjustStockAsync(productId, "WH-SECONDARY", -5);
            Assert.Equal(15, res2.NewQuantity);
            Assert.Equal(20, res2.PreviousQuantity);
            Assert.Equal(15, whSecondary.Quantity);
            Assert.Equal(40, whMain.Quantity);
            Assert.Equal(55, product.QuantityInStock);
        }

        [Fact]
        public async Task AdjustStockAsync_WhenDeltaIsZero_ThrowsArgumentException()
        {
            var fakeRepo = new FakeStockRepository();
            var service = new StockService(fakeRepo);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.AdjustStockAsync(Guid.NewGuid(), 0));

            Assert.Contains("Adjustment delta cannot be zero", ex.Message);
        }

        [Fact]
        public async Task AdjustStockAsync_WithLocation_WhenDeltaIsZero_ThrowsArgumentException()
        {
            var fakeRepo = new FakeStockRepository();
            var service = new StockService(fakeRepo);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.AdjustStockAsync(Guid.NewGuid(), "WH-MAIN", 0));

            Assert.Contains("Adjustment delta cannot be zero", ex.Message);
        }

        [Fact]
        public async Task AdjustStockAsync_WhenExceedsRetryLimit_ExhaustsExactThreeRetriesAndThrows()
        {
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Widget",
                SKU = "WDG-01",
                QuantityInStock = 50
            };
            var fakeRepo = new FakeStockRepository(new[] { product })
            {
                ConcurrencyExceptionsToThrow = 10 // Guaranteed exhaustion
            };
            var service = new StockService(fakeRepo);

            var ex = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
                service.AdjustStockAsync(productId, -5));

            Assert.NotNull(ex);
            Assert.Equal(3, fakeRepo.UpdateCallsCount);
        }
    }
}
