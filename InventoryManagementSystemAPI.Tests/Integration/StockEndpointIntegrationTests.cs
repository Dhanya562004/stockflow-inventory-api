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
        private const string DockerSkipReason = "Docker daemon is not running on this host; integration test against SQL Server container did not run.";

        [Fact(Skip = DockerSkipReason)]
        public async Task PostAdjust_WhenValidAdjustment_Returns200WithNewQuantity()
        {
            var factory = new CustomWebApplicationFactory("Server=localhost;Database=TestDB;Integrated Security=true;TrustServerCertificate=True;");
            var client = factory.CreateClient();

            var response = await client.PostAsJsonAsync("/api/stock/adjust", new AdjustStockDto
            {
                ProductId = Guid.NewGuid(),
                Delta = 5
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact(Skip = DockerSkipReason)]
        public async Task PostAdjust_WhenUnknownProduct_Returns404()
        {
            var factory = new CustomWebApplicationFactory("Server=localhost;Database=TestDB;Integrated Security=true;TrustServerCertificate=True;");
            var client = factory.CreateClient();

            var response = await client.PostAsJsonAsync("/api/stock/adjust", new AdjustStockDto
            {
                ProductId = Guid.NewGuid(),
                Delta = 1
            });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(Skip = DockerSkipReason)]
        public async Task PostAdjust_WhenNegativeResult_Returns409()
        {
            var factory = new CustomWebApplicationFactory("Server=localhost;Database=TestDB;Integrated Security=true;TrustServerCertificate=True;");
            var client = factory.CreateClient();

            var response = await client.PostAsJsonAsync("/api/stock/adjust", new AdjustStockDto
            {
                ProductId = Guid.NewGuid(),
                Delta = -999
            });

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact(Skip = DockerSkipReason)]
        public async Task PostAdjust_WhenInvalidInput_Returns400()
        {
            var factory = new CustomWebApplicationFactory("Server=localhost;Database=TestDB;Integrated Security=true;TrustServerCertificate=True;");
            var client = factory.CreateClient();

            var response = await client.PostAsJsonAsync("/api/stock/adjust", new AdjustStockDto
            {
                ProductId = Guid.Empty,
                Delta = 0
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact(Skip = DockerSkipReason)]
        public async Task PostAdjust_Concurrency_20ParallelCalls_10Succeed_10Conflict_FinalZero()
        {
            var connectionString = Environment.GetEnvironmentVariable("SQL_SERVER_TEST_CONNECTION_STRING")
                ?? "Server=localhost;Database=InventoryIntegrationTestDB;User Id=sa;Password=Password123!;TrustServerCertificate=True;";

            var factory = new CustomWebApplicationFactory(connectionString);
            var client = factory.CreateClient();

            var productId = Guid.NewGuid();
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.EnsureCreatedAsync();

                var product = new Product
                {
                    Id = productId,
                    Name = "Concurrency Test Item",
                    SKU = "CONC-01",
                    Barcode = "999999999",
                    QuantityInStock = 10
                };
                db.Products.Add(product);
                await db.SaveChangesAsync();
            }

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
