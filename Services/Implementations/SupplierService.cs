using InventoryManagementSystemAPI.DTOs.Supplier;
using InventoryManagementSystemAPI.Exceptions;
using InventoryManagementSystemAPI.Mappers;
using InventoryManagementSystemAPI.Repositories.Interfaces;
using InventoryManagementSystemAPI.Services.Interfaces;

namespace InventoryManagementSystemAPI.Services.Implementations
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _repository;

        public SupplierService(ISupplierRepository repository)
        {
            _repository = repository;
        }

        public async Task<SupplierDto> CreateAsync(CreateSupplierDto dto)
        {
            if (await _repository.ExistsByEmailAsync(dto.Email))
            {
                throw new DuplicateResourceException("A supplier with this email already exists.");
            }

            var supplier = dto.ToEntity();

            await _repository.AddAsync(supplier);

            return supplier.ToDto();
        }

        public async Task DeleteAsync(Guid id)
        {
            var supplier = await _repository.GetByIdAsync(id);

            if (supplier == null)
                throw new NotFoundException("Supplier not found.");

            await _repository.DeleteAsync(supplier);
        }

        public async Task<List<SupplierDto>> GetAllAsync()
        {
            var suppliers = await _repository.GetAllAsync();

            return suppliers.ToDtoList();
        }

        public async Task<SupplierDto?> GetByIdAsync(Guid id)
        {
            var supplier = await _repository.GetByIdAsync(id);

            if (supplier == null)
                throw new NotFoundException("Supplier not found.");

            return supplier.ToDto();
        }

        public async Task<SupplierDto?> UpdateAsync(Guid id, UpdateSupplierDto dto)
        {
            var supplier = await _repository.GetByIdAsync(id);

            if (supplier == null)
                throw new NotFoundException("Supplier not found.");

            if (supplier.Email != dto.Email)
            {
                if (await _repository.ExistsByEmailAsync(dto.Email, id))
                {
                    throw new DuplicateResourceException("A supplier with this email already exists.");
                }
            }

            dto.UpdateEntity(supplier);

            await _repository.UpdateAsync(supplier);

            return supplier.ToDto();
        }
    }
}
