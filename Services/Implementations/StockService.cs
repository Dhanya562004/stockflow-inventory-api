using InventoryManagementSystemAPI.DTOs.Stock;
using InventoryManagementSystemAPI.Exceptions;
using InventoryManagementSystemAPI.Repositories.Interfaces;
using InventoryManagementSystemAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystemAPI.Services.Implementations
{
    public class StockService : IStockService
    {
        private readonly IStockRepository _stockRepository;
        private const int MaxRetries = 3;

        public StockService(IStockRepository stockRepository)
        {
            _stockRepository = stockRepository;
        }

        public async Task<StockAdjustmentResultDto> AdjustStockAsync(Guid productId, int delta)
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    StockAdjustmentResultDto? result = null;

                    await _stockRepository.ExecuteInTransactionAsync(async () =>
                    {
                        var product = await _stockRepository.GetByIdAsync(productId);
                        if (product == null)
                        {
                            throw new NotFoundException($"Product with ID '{productId}' was not found.");
                        }

                        var currentQty = product.QuantityInStock;
                        var newQty = currentQty + delta;

                        if (newQty < 0)
                        {
                            throw new InsufficientStockException(
                                $"Stock adjustment rejected: resulting quantity cannot be negative. Current stock is {currentQty}, adjustment is {delta}, resulting quantity would be {newQty}.");
                        }

                        product.QuantityInStock = newQty;
                        await _stockRepository.UpdateAsync(product);

                        result = new StockAdjustmentResultDto
                        {
                            ProductId = product.Id,
                            NewQuantity = newQty,
                            PreviousQuantity = currentQty,
                            Delta = delta,
                            Message = "Stock adjusted successfully."
                        };
                    });

                    return result!;
                }
                catch (DbUpdateConcurrencyException) when (attempt < MaxRetries)
                {
                    // Exponential backoff with jitter to reduce contention across concurrent updates
                    await Task.Delay(Random.Shared.Next(25, 75 * attempt));
                }
            }

            throw new DbUpdateConcurrencyException($"Concurrency conflict could not be resolved after {MaxRetries} attempts.");
        }
    }
}
