using InventoryManagementSystemAPI.DTOs.PurchaseOrderItem;
using InventoryManagementSystemAPI.Models;

namespace InventoryManagementSystemAPI.Mappers
{
    public static class PurchaseOrderItemMapper
    {
        public static PurchaseOrderItemDto ToDto(this PurchaseOrderItem purchaseOrderItem) 
        {
            return new PurchaseOrderItemDto 
            {
                Id = purchaseOrderItem.Id,
                Quantity = purchaseOrderItem.Quantity,
                Cost = purchaseOrderItem.Cost,
                PurchaseOrderId = purchaseOrderItem.PurchaseOrderId,
                ProductId = purchaseOrderItem.ProductId
            };
        }

        public static PurchaseOrderItem ToEntity(this CreatePurchaseOrderItemDto dto) 
        {
            return new PurchaseOrderItem 
            {
                Quantity = dto.Quantity,
                Cost = dto.Cost,
                PurchaseOrderId = dto.PurchaseOrderId,
                ProductId = dto.ProductId
            };
        }

        public static void UpdateEntity(this UpdatePurchaseOrderItemDto dto, PurchaseOrderItem purchaseOrderItem) 
        {
            purchaseOrderItem.Quantity = dto.Quantity;
            purchaseOrderItem.Cost = dto.Cost;
            purchaseOrderItem.PurchaseOrderId = dto.PurchaseOrderId;
            purchaseOrderItem.ProductId = dto.ProductId;
        }

        public static List<PurchaseOrderItemDto> ToDtoList(this IEnumerable<PurchaseOrderItem> purchaseOrderItems) 
        {
            return purchaseOrderItems
                .Select(poi => poi.ToDto())
                .ToList();
        }
    }
}
