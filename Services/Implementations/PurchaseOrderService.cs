using InventoryManagementSystemAPI.DTOs.PurchaseOrder;
using InventoryManagementSystemAPI.Exceptions;
using InventoryManagementSystemAPI.Mappers;
using InventoryManagementSystemAPI.Repositories.Implementations;
using InventoryManagementSystemAPI.Repositories.Interfaces;
using InventoryManagementSystemAPI.Services.Interfaces;

namespace InventoryManagementSystemAPI.Services.Implementations
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IPurchaseOrderRepository _repository;
        private readonly ISupplierRepository _sRepository;

        public PurchaseOrderService(IPurchaseOrderRepository repository, ISupplierRepository sRepository)
        {
            _repository = repository;
            _sRepository = sRepository;
        }

        public async Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto dto)
        {
            var supplier = await _sRepository.GetByIdAsync(dto.SupplierId);

            if (supplier == null)
            {
                throw new NotFoundException("Supplier does not exist.");
            }

            if (await _repository.ExistsByOrderNumberAsync(dto.OrderNumber))
            {
                throw new DuplicateResourceException("A purchase order with this order number already exists.");
            }

            var purchaseOrder = dto.ToEntity();

            await _repository.AddAsync(purchaseOrder);

            return purchaseOrder.ToDto();
        }

        public async Task DeleteAsync(Guid id)
        {
            var purchaseOrder = await _repository.GetByIdAsync(id);

            if (purchaseOrder == null)
                throw new NotFoundException("Purchase order not found.");

            await _repository.DeleteAsync(purchaseOrder);
        }

        public async Task<List<PurchaseOrderDto>> GetAllAsync()
        {
            var purchaseOrders = await _repository.GetAllAsync();

            return purchaseOrders.ToDtoList();
        }

        public async Task<PurchaseOrderDto?> GetByIdAsync(Guid id)
        {
            var purchaseOrder = await _repository.GetByIdAsync(id);

            if (purchaseOrder == null)
                throw new NotFoundException("Purchase order not found.");

            return purchaseOrder.ToDto();
        }

        public async Task<PurchaseOrderDto?> UpdateAsync(Guid id, UpdatePurchaseOrderDto dto)
        {
            var supplier = await _sRepository.GetByIdAsync(dto.SupplierId);

            if (supplier == null)
            {
                throw new NotFoundException("Supplier does not exist.");
            }

            var purchaseOrder = await _repository.GetByIdAsync(id);

            if (purchaseOrder == null)
                throw new NotFoundException("Purchase order not found.");

            if (purchaseOrder.OrderNumber != dto.OrderNumber)
            {
                if (await _repository.ExistsByOrderNumberAsync(dto.OrderNumber, id))
                {
                    throw new DuplicateResourceException("A purchase order with this order number already exists.");
                }
            }

            dto.UpdateEntity(purchaseOrder);

            await _repository.UpdateAsync(purchaseOrder);

            return purchaseOrder.ToDto();
        }
    }
}
