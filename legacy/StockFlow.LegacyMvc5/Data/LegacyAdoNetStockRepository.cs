using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using StockFlow.LegacyMvc5.Models;

namespace StockFlow.LegacyMvc5.Data
{
    /// <summary>
    /// Demonstrates authentic legacy ADO.NET data access on .NET Framework 4.8.
    /// Uses synchronous connection pooling, explicit transactions, and manual SQL parameter binding.
    /// </summary>
    public class LegacyAdoNetStockRepository : ILegacyStockRepository
    {
        private readonly string _connectionString;

        public LegacyAdoNetStockRepository()
        {
            var connSetting = ConfigurationManager.ConnectionStrings["LegacyInventoryConnection"];
            _connectionString = connSetting != null ? connSetting.ConnectionString : "Server=localhost;Database=LegacyInventoryDB;Trusted_Connection=True;";
        }

        public LegacyAdoNetStockRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public LegacyProduct GetById(Guid productId)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("SELECT Id, Name, SKU, QuantityInStock, ReorderLevel, RowVersion FROM Products WHERE Id = @Id", conn))
            {
                cmd.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = productId;
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var product = new LegacyProduct
                        {
                            Id = reader.GetGuid(0),
                            Name = reader.GetString(1),
                            SKU = reader.GetString(2),
                            QuantityInStock = reader.GetInt32(3),
                            ReorderLevel = reader.GetInt32(4)
                        };

                        if (!reader.IsDBNull(5))
                        {
                            product.RowVersion = (byte[])reader[5];
                        }

                        return product;
                    }
                }
            }

            return null;
        }

        public IEnumerable<LegacyProduct> GetAll()
        {
            var list = new List<LegacyProduct>();

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("SELECT Id, Name, SKU, QuantityInStock, ReorderLevel, RowVersion FROM Products ORDER BY Name ASC", conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var product = new LegacyProduct
                        {
                            Id = reader.GetGuid(0),
                            Name = reader.GetString(1),
                            SKU = reader.GetString(2),
                            QuantityInStock = reader.GetInt32(3),
                            ReorderLevel = reader.GetInt32(4)
                        };

                        if (!reader.IsDBNull(5))
                        {
                            product.RowVersion = (byte[])reader[5];
                        }

                        list.Add(product);
                    }
                }
            }

            return list;
        }

        public bool AdjustStock(Guid productId, int delta, out int newQuantity, out string errorMessage)
        {
            newQuantity = 0;
            errorMessage = string.Empty;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        // 1. Read current stock with UPDLOCK to prevent race conditions in legacy pipeline
                        int currentStock;
                        using (var readCmd = new SqlCommand("SELECT QuantityInStock FROM Products WITH (UPDLOCK) WHERE Id = @Id", conn, tx))
                        {
                            readCmd.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = productId;
                            var scalar = readCmd.ExecuteScalar();
                            if (scalar == null || scalar == DBNull.Value)
                            {
                                errorMessage = "product not found";
                                tx.Rollback();
                                return false;
                            }
                            currentStock = Convert.ToInt32(scalar);
                        }

                        // 2. Validate non-negative
                        if (currentStock + delta < 0)
                        {
                            errorMessage = "stock would go negative";
                            newQuantity = currentStock;
                            tx.Rollback();
                            return false;
                        }

                        newQuantity = currentStock + delta;

                        // 3. Update stock
                        using (var updateCmd = new SqlCommand("UPDATE Products SET QuantityInStock = @NewQty WHERE Id = @Id", conn, tx))
                        {
                            updateCmd.Parameters.Add("@NewQty", SqlDbType.Int).Value = newQuantity;
                            updateCmd.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = productId;
                            updateCmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        errorMessage = ex.Message;
                        return false;
                    }
                }
            }
        }
    }
}
