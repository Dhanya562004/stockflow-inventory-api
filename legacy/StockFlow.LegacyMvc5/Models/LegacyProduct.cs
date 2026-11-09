using System;

namespace StockFlow.LegacyMvc5.Models
{
    public class LegacyProduct
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public int QuantityInStock { get; set; }
        public int ReorderLevel { get; set; }
        public byte[] RowVersion { get; set; }

        public LegacyProduct()
        {
            Id = Guid.NewGuid();
            Name = string.Empty;
            SKU = string.Empty;
            RowVersion = new byte[8];
        }
    }
}
