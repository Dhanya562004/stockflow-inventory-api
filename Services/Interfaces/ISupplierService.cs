using InventoryManagementSystemAPI.DTOs.Supplier;

namespace InventoryManagementSystemAPI.Services.Interfaces
{
    public interface ISupplierService
    {
        Task<List<SupplierDto>> GetAllAsync();

        Task<SupplierDto?> GetByIdAsync(Guid id);

        Task<SupplierDto> CreateAsync(CreateSupplierDto dto);

        Task<SupplierDto?> UpdateAsync(Guid id, UpdateSupplierDto dto);

        Task DeleteAsync(Guid id);
    }
}
