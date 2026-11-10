using System;

namespace InventoryManagementSystemAPI.DTOs.Stock
{
    public class AdjustWarehouseStockDto
    {
        public Guid ProductId { get; set; }
        public string LocationId { get; set; } = string.Empty;
        public int Delta { get; set; }
    }
}
