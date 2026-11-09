using System;
using System.ComponentModel.DataAnnotations;

namespace StockFlow.LegacyMvc5.Models
{
    public class LegacyStockAdjustmentRequest
    {
        [Required(ErrorMessage = "Product ID is required.")]
        public Guid ProductId { get; set; }

        [Required(ErrorMessage = "Adjustment delta is required.")]
        [Range(-10000, 10000, ErrorMessage = "Adjustment delta must be between -10,000 and 10,000.")]
        public int Delta { get; set; }
    }
}
