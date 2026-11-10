using System;
using System.Data.SqlClient;
using StockFlow.LegacyMvc5.Data;
using Xunit;

namespace StockFlow.LegacyMvc5.Tests.Data
{
    public class LegacyAdoNetStockRepositoryRealSqlTests
    {
        [SkippableFact]
        public void AdjustStock_AgainstRealSqlServer_ExecutesAdoNetTransactionSuccessfully()
        {
            var connectionString = Environment.GetEnvironmentVariable("SQL_SERVER_TEST_CONNECTION_STRING");
            Skip.If(string.IsNullOrWhiteSpace(connectionString),
                "SQL_SERVER_TEST_CONNECTION_STRING is unset; skipping real SQL Server ADO.NET integration test.");

            var testProductId = Guid.NewGuid();
            var initialQuantity = 25;
            var delta = 10;
            var sku = "LEGACY-" + Guid.NewGuid().ToString("N").Substring(0, 8);
            var barcode = "BAR-" + Guid.NewGuid().ToString("N").Substring(0, 8);

            // Ensure test table and row exist in the test database
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Ensure Categories and Suppliers exist for foreign keys
                Guid categoryId = Guid.Empty;
                using (var catCmd = new SqlCommand("SELECT TOP 1 Id FROM Categories", conn))
                {
                    var val = catCmd.ExecuteScalar();
                    if (val != null && val != DBNull.Value) categoryId = (Guid)val;
                }
                if (categoryId == Guid.Empty)
                {
                    categoryId = Guid.NewGuid();
                    using (var insCat = new SqlCommand("INSERT INTO Categories (Id, Name) VALUES (@Id, 'Legacy Cat')", conn))
                    {
                        insCat.Parameters.AddWithValue("@Id", categoryId);
                        insCat.ExecuteNonQuery();
                    }
                }

                Guid supplierId = Guid.Empty;
                using (var supCmd = new SqlCommand("SELECT TOP 1 Id FROM Suppliers", conn))
                {
                    var val = supCmd.ExecuteScalar();
                    if (val != null && val != DBNull.Value) supplierId = (Guid)val;
                }
                if (supplierId == Guid.Empty)
                {
                    supplierId = Guid.NewGuid();
                    using (var insSup = new SqlCommand("INSERT INTO Suppliers (Id, CompanyName, Email) VALUES (@Id, 'Legacy Sup', 'legacysup@test.com')", conn))
                    {
                        insSup.Parameters.AddWithValue("@Id", supplierId);
                        insSup.ExecuteNonQuery();
                    }
                }

                // Insert a temporary test product
                using (var insertCmd = new SqlCommand(
                    @"INSERT INTO Products 
                      (Id, Name, Description, SKU, Barcode, Price, Cost, QuantityInStock, ReorderLevel, IsActive, CreatedDate, CategoryId, SupplierId) 
                      VALUES 
                      (@Id, @Name, @Desc, @SKU, @Barcode, @Price, @Cost, @Qty, @Reorder, @IsActive, @CreatedDate, @CategoryId, @SupplierId)", conn))
                {
                    insertCmd.Parameters.AddWithValue("@Id", testProductId);
                    insertCmd.Parameters.AddWithValue("@Name", "Legacy ADO.NET Test Item");
                    insertCmd.Parameters.AddWithValue("@Desc", "Test Description");
                    insertCmd.Parameters.AddWithValue("@SKU", sku);
                    insertCmd.Parameters.AddWithValue("@Barcode", barcode);
                    insertCmd.Parameters.AddWithValue("@Price", 19.99m);
                    insertCmd.Parameters.AddWithValue("@Cost", 9.99m);
                    insertCmd.Parameters.AddWithValue("@Qty", initialQuantity);
                    insertCmd.Parameters.AddWithValue("@Reorder", 5);
                    insertCmd.Parameters.AddWithValue("@IsActive", true);
                    insertCmd.Parameters.AddWithValue("@CreatedDate", DateTime.UtcNow);
                    insertCmd.Parameters.AddWithValue("@CategoryId", categoryId);
                    insertCmd.Parameters.AddWithValue("@SupplierId", supplierId);
                    insertCmd.ExecuteNonQuery();
                }
            }

            try
            {
                var repo = new LegacyAdoNetStockRepository(connectionString);

                // Act: Adjust stock by delta (+10)
                int newQuantity;
                string errorMessage;
                var success = repo.AdjustStock(testProductId, delta, out newQuantity, out errorMessage);

                // Assert
                Assert.True(success, "Expected AdjustStock to succeed, error: " + errorMessage);
                Assert.Equal(35, newQuantity);

                // Verify via GetById
                var fetched = repo.GetById(testProductId);
                Assert.NotNull(fetched);
                Assert.Equal(35, fetched.QuantityInStock);
            }
            finally
            {
                // Clean up the temporary row
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cleanupCmd = new SqlCommand("DELETE FROM Products WHERE Id = @Id", conn))
                    {
                        cleanupCmd.Parameters.AddWithValue("@Id", testProductId);
                        cleanupCmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
