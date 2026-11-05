using InventoryManagementSystemAPI.DTOs.Supplier;
using InventoryManagementSystemAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierService _service;

        public SuppliersController(ISupplierService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var suppliers = await _service.GetAllAsync();

            return Ok(suppliers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var supplier = await _service.GetByIdAsync(id);

            return Ok(supplier);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSupplierDto dto)
        {
            var supplier = await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = supplier.Id },
                supplier);   
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateSupplierDto dto)
        {
            var supplier = await _service.UpdateAsync(id, dto);

            return Ok(supplier);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var supplier = await _service.GetByIdAsync(id);

            await _service.DeleteAsync(supplier.Id);

            return NoContent();
        }
    }
}
