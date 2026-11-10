using InventoryManagementSystemAPI.DTOs.Stock;
using InventoryManagementSystemAPI.Exceptions;
using InventoryManagementSystemAPI.Models;
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
            if (delta == 0)
            {
                throw new ArgumentException("Adjustment delta cannot be zero.", nameof(delta));
            }

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

        public async Task<StockAdjustmentResultDto> AdjustStockAsync(Guid productId, string locationId, int delta)
        {
            if (string.IsNullOrWhiteSpace(locationId))
            {
                throw new ArgumentException("Warehouse location ID must be provided.", nameof(locationId));
            }

            locationId = locationId.Trim();

            if (delta == 0)
            {
                throw new ArgumentException("Adjustment delta cannot be zero.", nameof(delta));
            }

            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    StockAdjustmentResultDto? result = null;

                    await _stockRepository.ExecuteInTransactionAsync(async () =>
                    {
                        var locationExists = await _stockRepository.LocationExistsAsync(locationId);
                        if (!locationExists)
                        {
                            throw new NotFoundException($"Warehouse location '{locationId}' was not found.");
                        }

                        var product = await _stockRepository.GetByIdAsync(productId);
                        if (product == null)
                        {
                            throw new NotFoundException($"Product with ID '{productId}' was not found.");
                        }

                        var stockLevel = await _stockRepository.GetStockLevelAsync(productId, locationId);

                        int previousLocationQty;
                        int newLocationQty;

                        if (stockLevel == null)
                        {
                            previousLocationQty = 0;
                            newLocationQty = delta;

                            if (newLocationQty < 0)
                            {
                                throw new InsufficientStockException(
                                    $"Stock adjustment rejected for location '{locationId}': resulting quantity cannot be negative. Current stock is 0, adjustment is {delta}, resulting quantity would be {newLocationQty}.");
                            }

                            var newTotalQty = product.QuantityInStock + delta;
                            if (newTotalQty < 0)
                            {
                                throw new InsufficientStockException(
                                    $"Stock adjustment rejected: resulting catalog quantity cannot be negative. Current stock is {product.QuantityInStock}, adjustment is {delta}, resulting quantity would be {newTotalQty}.");
                            }

                            stockLevel = new StockLevel
                            {
                                Id = Guid.NewGuid(),
                                ProductId = productId,
                                LocationId = locationId,
                                Quantity = newLocationQty
                            };

                            product.QuantityInStock = newTotalQty;

                            await _stockRepository.AddStockLevelAsync(stockLevel);
                            await _stockRepository.UpdateAsync(product);
                        }
                        else
                        {
                            previousLocationQty = stockLevel.Quantity;
                            newLocationQty = previousLocationQty + delta;

                            if (newLocationQty < 0)
                            {
                                throw new InsufficientStockException(
                                    $"Stock adjustment rejected for location '{locationId}': resulting quantity cannot be negative. Current stock is {previousLocationQty}, adjustment is {delta}, resulting quantity would be {newLocationQty}.");
                            }

                            var newTotalQty = product.QuantityInStock + delta;
                            if (newTotalQty < 0)
                            {
                                throw new InsufficientStockException(
                                    $"Stock adjustment rejected: resulting catalog quantity cannot be negative. Current stock is {product.QuantityInStock}, adjustment is {delta}, resulting quantity would be {newTotalQty}.");
                            }

                            stockLevel.Quantity = newLocationQty;
                            product.QuantityInStock = newTotalQty;

                            await _stockRepository.UpdateStockLevelAsync(stockLevel);
                            await _stockRepository.UpdateAsync(product);
                        }

                        result = new StockAdjustmentResultDto
                        {
                            ProductId = product.Id,
                            LocationId = locationId,
                            NewQuantity = newLocationQty,
                            PreviousQuantity = previousLocationQty,
                            Delta = delta,
                            Message = $"Stock adjusted successfully for location '{locationId}'."
                        };
                    });

                    return result!;
                }
                catch (Exception ex) when ((ex is DbUpdateConcurrencyException || ex is DbUpdateException) && attempt < MaxRetries)
                {
                    // Exponential backoff with jitter to reduce contention across concurrent updates
                    await Task.Delay(Random.Shared.Next(25, 75 * attempt));
                }
            }

            throw new DbUpdateConcurrencyException($"Concurrency conflict could not be resolved after {MaxRetries} attempts.");
        }
    }
}
