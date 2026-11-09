using System;

namespace StockFlow.LegacyMvc5.Models
{
    public class LegacyStockAdjustmentResponse
    {
        public Guid ProductId { get; set; }
        public int NewQuantity { get; set; }
        public int PreviousQuantity { get; set; }
        public int Delta { get; set; }
        public string Message { get; set; }
    }
}
