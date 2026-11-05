using InventoryManagementSystemAPI.DTOs.PurchaseOrder;
using InventoryManagementSystemAPI.Models;

namespace InventoryManagementSystemAPI.Mappers
{
    public static class PurchaseOrderMapper
    {
        public static PurchaseOrderDto ToDto(this PurchaseOrder purchaseOrder) 
        {
            return new PurchaseOrderDto
            {
                Id = purchaseOrder.Id,
                OrderNumber = purchaseOrder.OrderNumber,
                OrderDate = purchaseOrder.OrderDate,
                Status = purchaseOrder.Status,
                TotalAmount = purchaseOrder.TotalAmount,
                SupplierId = purchaseOrder.SupplierId
            };
        }

        public static PurchaseOrder ToEntity(this CreatePurchaseOrderDto dto) 
        {
            return new PurchaseOrder 
            {
                OrderNumber = dto.OrderNumber,
                OrderDate = dto.OrderDate,
                Status = dto.Status,
                TotalAmount = dto.TotalAmount,
                SupplierId = dto.SupplierId
            };
        }

        public static void UpdateEntity(this UpdatePurchaseOrderDto dto, PurchaseOrder purchaseOrder) 
        {
            purchaseOrder.OrderNumber = dto.OrderNumber;
            purchaseOrder.OrderDate = dto.OrderDate;
            purchaseOrder.Status = dto.Status;
            purchaseOrder.TotalAmount = dto.TotalAmount;
            purchaseOrder.SupplierId = dto.SupplierId;
        }

        public static List<PurchaseOrderDto> ToDtoList(this IEnumerable<PurchaseOrder> purchaseOrders) 
        {
            return purchaseOrders
                .Select(po => po.ToDto())
                .ToList();
        }
    }
}
