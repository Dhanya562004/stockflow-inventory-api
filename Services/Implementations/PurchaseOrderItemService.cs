using InventoryManagementSystemAPI.DTOs.PurchaseOrderItem;
using InventoryManagementSystemAPI.Exceptions;
using InventoryManagementSystemAPI.Mappers;
using InventoryManagementSystemAPI.Repositories.Interfaces;
using InventoryManagementSystemAPI.Services.Interfaces;

namespace InventoryManagementSystemAPI.Services.Implementations
{
    public class PurchaseOrderItemService : IPurchaseOrderItemService
    {
        private readonly IPurchaseOrderItemRepository _repository;
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly IProductRepository _productRepository;

        public PurchaseOrderItemService (IPurchaseOrderItemRepository repository, IPurchaseOrderRepository purchaseOrderRepository, IProductRepository productRepository )
        {
            _repository = repository;
            _purchaseOrderRepository = purchaseOrderRepository;
            _productRepository = productRepository;
        }

        public async Task<PurchaseOrderItemDto> CreateAsync(CreatePurchaseOrderItemDto dto)
        {
            var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(dto.PurchaseOrderId);

            if (purchaseOrder == null) 
            {
                throw new NotFoundException("Purchase order does not exist.");
            }

            var product = await _productRepository.GetByIdAsync(dto.ProductId);

            if (product == null) 
            {
                throw new NotFoundException("Product does not exist.");
            }

            var purchaseOrderItem = dto.ToEntity();

            await _repository.AddAsync(purchaseOrderItem);

            return purchaseOrderItem.ToDto();
        }

        public async Task DeleteAsync(Guid id)
        {
            var purchaseOrderItem = await _repository.GetByIdAsync(id);

            if (purchaseOrderItem == null)
                throw new NotFoundException("Purchase order item not found.");

            await _repository.DeleteAsync(purchaseOrderItem);
        }

        public async Task<List<PurchaseOrderItemDto>> GetAllAsync()
        {
            var purchaseOrderItems = await _repository.GetAllAsync();

            return purchaseOrderItems.ToDtoList();
        }

        public async Task<PurchaseOrderItemDto?> GetByIdAsync(Guid id)
        {
            var purchaseOrderItem = await _repository.GetByIdAsync(id);

            if (purchaseOrderItem == null)
                throw new NotFoundException("Purchase order item not found.");

            return purchaseOrderItem.ToDto();
        }

        public async Task<PurchaseOrderItemDto?> UpdateAsync(Guid id, UpdatePurchaseOrderItemDto dto)
        {

            var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(dto.PurchaseOrderId);

            if (purchaseOrder == null)
            {
                throw new NotFoundException("Purchase order does not exist.");
            }

            var product = await _productRepository.GetByIdAsync(dto.ProductId);

            if (product == null)
            {
                throw new NotFoundException("Product does not exist.");
            }

            var purchaseOrderItem = await _repository.GetByIdAsync(id);

            if (purchaseOrderItem == null)
                throw new NotFoundException("Purchase order item not found.");

            dto.UpdateEntity(purchaseOrderItem);

            await _repository.UpdateAsync(purchaseOrderItem);

            return purchaseOrderItem.ToDto();
        }
    }
}
