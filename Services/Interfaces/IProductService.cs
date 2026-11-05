using InventoryManagementSystemAPI.DTOs.Product;

namespace InventoryManagementSystemAPI.Services.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAllAsync();

        Task<ProductDto?> GetByIdAsync(Guid id);

        Task<ProductDto> CreateAsync(CreateProductDto dto);

        Task<ProductDto?> UpdateAsync(Guid id, UpdateProductDto dto);

        Task DeleteAsync(Guid id);
    }
}
