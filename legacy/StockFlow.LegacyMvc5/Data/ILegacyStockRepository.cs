using System;
using System.Collections.Generic;
using StockFlow.LegacyMvc5.Models;

namespace StockFlow.LegacyMvc5.Data
{
    public interface ILegacyStockRepository
    {
        LegacyProduct GetById(Guid productId);
        IEnumerable<LegacyProduct> GetAll();
        bool AdjustStock(Guid productId, int delta, out int newQuantity, out string errorMessage);
    }
}
