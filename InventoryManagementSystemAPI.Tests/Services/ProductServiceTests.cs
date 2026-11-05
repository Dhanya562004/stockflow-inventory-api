using InventoryManagementSystemAPI.DTOs.Product;
using InventoryManagementSystemAPI.Exceptions;
using InventoryManagementSystemAPI.Models;
using InventoryManagementSystemAPI.Services.Implementations;
using InventoryManagementSystemAPI.Tests.Fakes;
using Xunit;

namespace InventoryManagementSystemAPI.Tests.Services
{
    public class ProductServiceTests
    {
        [Fact]
        public async Task CreateAsync_WhenValidDto_CreatesAndReturnsProductDto()
        {
            var fakeRepo = new FakeProductRepository();
            var service = new ProductService(fakeRepo);
            var dto = new CreateProductDto
            {
                Name = "Laptop",
                Description = "High end laptop",
                SKU = "LAPTOP-01",
                Barcode = "987654321012",
                Price = 1200.00m,
                Cost = 800.00m,
                QuantityInStock = 5,
                ReorderLevel = 2,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CategoryId = Guid.NewGuid(),
                SupplierId = Guid.NewGuid()
            };

            var result = await service.CreateAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
            Assert.Equal(dto.SKU, result.SKU);
            Assert.Equal(dto.QuantityInStock, result.QuantityInStock);
            var allProducts = await fakeRepo.GetAllAsync();
            Assert.Single(allProducts);
        }

        [Fact]
        public async Task CreateAsync_WhenDuplicateSku_ThrowsDuplicateResourceException()
        {
            var existing = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Existing Product",
                SKU = "DUPE-SKU",
                Barcode = "1111111111"
            };
            var fakeRepo = new FakeProductRepository(new[] { existing });
            var service = new ProductService(fakeRepo);
            var dto = new CreateProductDto
            {
                Name = "New Product",
                SKU = "DUPE-SKU",
                Barcode = "2222222222"
            };

            var ex = await Assert.ThrowsAsync<DuplicateResourceException>(() => service.CreateAsync(dto));
            Assert.Equal("A product with this SKU already exists.", ex.Message);
        }

        [Fact]
        public async Task CreateAsync_WhenDuplicateBarcode_ThrowsDuplicateResourceException()
        {
            var existing = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Existing Product",
                SKU = "SKU-1",
                Barcode = "DUPE-BARCODE"
            };
            var fakeRepo = new FakeProductRepository(new[] { existing });
            var service = new ProductService(fakeRepo);
            var dto = new CreateProductDto
            {
                Name = "New Product",
                SKU = "SKU-2",
                Barcode = "DUPE-BARCODE"
            };

            var ex = await Assert.ThrowsAsync<DuplicateResourceException>(() => service.CreateAsync(dto));
            Assert.Equal("A product with this barcode already exists.", ex.Message);
        }

        [Fact]
        public async Task GetByIdAsync_WhenProductExists_ReturnsProductDto()
        {
            var productId = Guid.NewGuid();
            var existing = new Product
            {
                Id = productId,
                Name = "Mouse",
                SKU = "MOU-01",
                Barcode = "123456",
                Price = 25m,
                Cost = 15m,
                QuantityInStock = 50
            };
            var fakeRepo = new FakeProductRepository(new[] { existing });
            var service = new ProductService(fakeRepo);

            var result = await service.GetByIdAsync(productId);

            Assert.NotNull(result);
            Assert.Equal(productId, result.Id);
            Assert.Equal("Mouse", result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_WhenProductDoesNotExist_ThrowsNotFoundException()
        {
            var fakeRepo = new FakeProductRepository();
            var service = new ProductService(fakeRepo);

            await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task DeleteAsync_WhenProductExists_RemovesProduct()
        {
            var productId = Guid.NewGuid();
            var existing = new Product
            {
                Id = productId,
                Name = "Keyboard",
                SKU = "KEY-01",
                Barcode = "654321"
            };
            var fakeRepo = new FakeProductRepository(new[] { existing });
            var service = new ProductService(fakeRepo);

            await service.DeleteAsync(productId);

            var all = await fakeRepo.GetAllAsync();
            Assert.Empty(all);
        }

        [Fact]
        public async Task DeleteAsync_WhenProductDoesNotExist_ThrowsNotFoundException()
        {
            var fakeRepo = new FakeProductRepository();
            var service = new ProductService(fakeRepo);

            await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(Guid.NewGuid()));
        }
    }
}
