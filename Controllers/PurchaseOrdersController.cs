using InventoryManagementSystemAPI.DTOs.PurchaseOrder;
using InventoryManagementSystemAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly IPurchaseOrderService _service;

        public PurchaseOrdersController(IPurchaseOrderService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var purchaseOrders = await _service.GetAllAsync();

            return Ok(purchaseOrders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var purchaseOrder = await _service.GetByIdAsync(id);

            return Ok(purchaseOrder);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePurchaseOrderDto dto)
        {
            var purchaseOrder = await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = purchaseOrder.Id },
                purchaseOrder);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdatePurchaseOrderDto dto)
        {
            var purchaseOrder = await _service.UpdateAsync(id, dto);

            return Ok(purchaseOrder);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var purchaseOrder = await _service.GetByIdAsync(id);

            await _service.DeleteAsync(purchaseOrder.Id);

            return NoContent();
        }
    }
}
