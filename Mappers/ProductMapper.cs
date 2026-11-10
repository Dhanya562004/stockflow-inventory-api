using InventoryManagementSystemAPI.DTOs.Product;
using InventoryManagementSystemAPI.Models;

namespace InventoryManagementSystemAPI.Mappers
{
    public static class ProductMapper
    {
        public static ProductDto ToDto(this Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                SKU = product.SKU,
                Barcode = product.Barcode,
                Price = product.Price,
                Cost = product.Cost,
                QuantityInStock = product.QuantityInStock,
                ReorderLevel = product.ReorderLevel,
                IsActive = product.IsActive,
                CategoryId = product.CategoryId,
                SupplierId = product.SupplierId
            };
        }

        public static Product ToEntity(this CreateProductDto dto)
        {
            return new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                SKU = dto.SKU,
                Barcode = dto.Barcode,
                Price = dto.Price,
                Cost = dto.Cost,
                QuantityInStock = dto.QuantityInStock,
                ReorderLevel = dto.ReorderLevel,
                CategoryId = dto.CategoryId,
                SupplierId = dto.SupplierId
            };
        }

        public static void UpdateEntity(this UpdateProductDto dto, Product product)
        {
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.SKU = dto.SKU;
            product.Barcode = dto.Barcode;
            product.Price = dto.Price;
            product.Cost = dto.Cost;
            product.QuantityInStock = dto.QuantityInStock;
            product.ReorderLevel = dto.ReorderLevel;
            product.IsActive = dto.IsActive;
            product.CategoryId = dto.CategoryId;
            product.SupplierId = dto.SupplierId;
        }

        public static List<ProductDto> ToDtoList(this IEnumerable<Product> products)
        {
            return products
                .Select(p => p.ToDto())
                .ToList();
        }
    }
}
