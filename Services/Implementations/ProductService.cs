using InventoryManagementSystemAPI.DTOs.Product;
using InventoryManagementSystemAPI.Exceptions;
using InventoryManagementSystemAPI.Mappers;
using InventoryManagementSystemAPI.Repositories.Implementations;
using InventoryManagementSystemAPI.Repositories.Interfaces;
using InventoryManagementSystemAPI.Services.Interfaces;

namespace InventoryManagementSystemAPI.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository) 
        {
            _repository = repository;
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto dto)
        {
            if (await _repository.ExistsBySkuAsync(dto.SKU))
            {
                throw new DuplicateResourceException("A product with this SKU already exists.");
            }

            if (await _repository.ExistsByBarcodeAsync(dto.Barcode))
            {
                throw new DuplicateResourceException("A product with this barcode already exists.");
            }

            var product = dto.ToEntity();

            await _repository.AddAsync(product);

            return product.ToDto();
        }

        public async Task DeleteAsync(Guid id)
        {
            var product = await _repository.GetByIdAsync(id);

            if (product == null)
                throw new NotFoundException("Product not found.");

            await _repository.DeleteAsync(product);
        }

        public async Task<List<ProductDto>> GetAllAsync()
        {
            var products = await _repository.GetAllAsync();

            return products.ToDtoList();
        }

        public async Task<ProductDto?> GetByIdAsync(Guid id)
        {
            var product = await _repository.GetByIdAsync(id);

            if (product == null)
                throw new NotFoundException("Product not found.");

            return product.ToDto();
        }

        public async Task<ProductDto?> UpdateAsync(Guid id, UpdateProductDto dto)
        {
            var product = await _repository.GetByIdAsync(id);

            if (product == null)
                throw new NotFoundException("Product not found.");

            if (product.SKU != dto.SKU)
            {
                if (await _repository.ExistsBySkuAsync(dto.SKU, id))
                {
                    throw new DuplicateResourceException("A product with this SKU already exists.");
                }
            }
            if (product.Barcode != dto.Barcode)
            {
                if (await _repository.ExistsByBarcodeAsync(dto.Barcode, id))
                {
                    throw new DuplicateResourceException("A product with this barcode already exists.");
                }
            }

            dto.UpdateEntity(product);

            await _repository.UpdateAsync(product);

            return product.ToDto();

        }
    }
}
