using InventoryManagementSystemAPI.DTOs.PurchaseOrderItem;
using InventoryManagementSystemAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
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
            var items = await _service.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            return Ok(item);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> Create(CreatePurchaseOrderItemDto dto)
        {
            var item = await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = item.Id },
                item);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdatePurchaseOrderItemDto dto)
        {
            var item = await _service.UpdateAsync(id, dto);
            return Ok(item);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}