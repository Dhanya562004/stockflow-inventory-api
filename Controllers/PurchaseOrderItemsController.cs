using InventoryManagementSystemAPI.DTOs.PurchaseOrderItem;
using InventoryManagementSystemAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PurchaseOrderItemsController : ControllerBase
    {
        private readonly IPurchaseOrderItemService _service;

        public PurchaseOrderItemsController(IPurchaseOrderItemService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var purchaseOrderItems = await _service.GetAllAsync();

            return Ok(purchaseOrderItems);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var purchaseOrderItem = await _service.GetByIdAsync(id);

            return Ok(purchaseOrderItem);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePurchaseOrderItemDto dto)
        {
            var purchaseOrderItem = await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = purchaseOrderItem.Id },
                purchaseOrderItem);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdatePurchaseOrderItemDto dto)
        {
            var purchaseOrderItem = await _service.UpdateAsync(id, dto);

            return Ok(purchaseOrderItem);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var purchaseOrderItem = await _service.GetByIdAsync(id);

            await _service.DeleteAsync(purchaseOrderItem.Id);

            return NoContent();
        }
    }
}
