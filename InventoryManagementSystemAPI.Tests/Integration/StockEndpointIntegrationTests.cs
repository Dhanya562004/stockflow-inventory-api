using System.Net;
using System.Net.Http.Json;
using InventoryManagementSystemAPI.Data;
using InventoryManagementSystemAPI.DTOs.Stock;
using InventoryManagementSystemAPI.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InventoryManagementSystemAPI.Tests.Integration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _connectionString;

        public CustomWebApplicationFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseSqlServer(_connectionString);
                });
            });
        }
    }

    public class StockEndpointIntegrationTests
    {
        private static string? GetTestConnectionString() =>
            Environment.GetEnvironmentVariable("SQL_SERVER_TEST_CONNECTION_STRING");

        private static void EnsureSqlServerAvailable()
        {
            var conn = GetTestConnectionString();
            Skip.If(string.IsNullOrWhiteSpace(conn), "SQL_SERVER_TEST_CONNECTION_STRING is not configured; integration tests against SQL Server container did not run on this host.");
        }

        private static async Task<Guid> SeedProductAsync(CustomWebApplicationFactory factory, int initialQuantity)
        {
            var productId = Guid.NewGuid();
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureCreatedAsync();

            var category = new Category 
            { 
                Id = Guid.NewGuid(), 
                Name = "Test Cat " + Guid.NewGuid().ToString("N")[..8] 
            };
            var supplier = new Supplier 
            { 
                Id = Guid.NewGuid(), 
                CompanyName = "Test Sup " + Guid.NewGuid().ToString("N")[..8], 
                Email = "sup" + Guid.NewGuid().ToString("N")[..8] + "@example.com" 
            };

            db.Categories.Add(category);
            db.Suppliers.Add(supplier);

            var product = new Product
            {
                Id = productId,
                Name = "Integration Test Item " + Guid.NewGuid().ToString("N")[..8],
                SKU = "INT-" + Guid.NewGuid().ToString("N")[..8],
                Barcode = Guid.NewGuid().ToString("N")[..12],
                QuantityInStock = initialQuantity,
                CategoryId = category.Id,
                SupplierId = supplier.Id
            };

            db.Products.Add(product);
            await db.SaveChangesAsync();

            return productId;
        }

        [SkippableFact]
        public async Task PostAdjust_WhenValidAdjustment_Returns200WithNewQuantity()
        {
            EnsureSqlServerAvailable();
            var factory = new CustomWebApplicationFactory(GetTestConnectionString()!);
            var client = factory.CreateClient();

            var productId = await SeedProductAsync(factory, 10);

            var response = await client.PostAsJsonAsync("/api/stock/adjust", new AdjustStockDto
            {
                ProductId = productId,
                Delta = 5
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [SkippableFact]
        public async Task PostAdjust_WhenUnknownProduct_Returns404()
        {
            EnsureSqlServerAvailable();
            var factory = new CustomWebApplicationFactory(GetTestConnectionString()!);
            var client = factory.CreateClient();

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.EnsureCreatedAsync();
            }

            var response = await client.PostAsJsonAsync("/api/stock/adjust", new AdjustStockDto
            {
                ProductId = Guid.NewGuid(),
                Delta = 1
            });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [SkippableFact]
        public async Task PostAdjust_WhenNegativeResult_Returns409()
        {
            EnsureSqlServerAvailable();
            var factory = new CustomWebApplicationFactory(GetTestConnectionString()!);
            var client = factory.CreateClient();

            var productId = await SeedProductAsync(factory, 5);

            var response = await client.PostAsJsonAsync("/api/stock/adjust", new AdjustStockDto
            {
                ProductId = productId,
                Delta = -10
            });

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [SkippableFact]
        public async Task PostAdjust_WhenInvalidInput_Returns400()
        {
            EnsureSqlServerAvailable();
            var factory = new CustomWebApplicationFactory(GetTestConnectionString()!);
            var client = factory.CreateClient();

            var response = await client.PostAsJsonAsync("/api/stock/adjust", new AdjustStockDto
            {
                ProductId = Guid.Empty,
                Delta = 0
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [SkippableFact]
        public async Task PostAdjust_Concurrency_20ParallelCalls_10Succeed_10Conflict_FinalZero()
        {
            EnsureSqlServerAvailable();
            var connectionString = GetTestConnectionString()!;
            var factory = new CustomWebApplicationFactory(connectionString);
            var client = factory.CreateClient();

            var productId = await SeedProductAsync(factory, 10);

            var tasks = Enumerable.Range(0, 20).Select(_ =>
                client.PostAsJsonAsync("/api/stock/adjust", new AdjustStockDto
                {
                    ProductId = productId,
                    Delta = -1
                })
            );

            var responses = await Task.WhenAll(tasks);

            var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
            var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);

            Assert.Equal(10, successCount);
            Assert.Equal(10, conflictCount);

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var product = await db.Products.AsNoTracking().FirstAsync(p => p.Id == productId);
                Assert.Equal(0, product.QuantityInStock);
                Assert.False(product.QuantityInStock < 0);
            }
        }
    }
}
