using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Inventory_Api.Services;
using Inventory_Api.Models.DTOs;

namespace Inventory_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WarehousesController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;

        public WarehousesController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WarehouseDto>>> GetWarehouses()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var warehouses = await _warehouseService.GetAccessibleWarehousesAsync(currentUserId);
                return Ok(warehouses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"خطا در دریافت لیست انبارها: {ex.Message}");
            }
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<WarehouseDto>>> GetActiveWarehouses()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var warehouses = await _warehouseService.GetAccessibleActiveWarehousesAsync(currentUserId);
                return Ok(warehouses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"خطا در دریافت لیست انبارهای فعال: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WarehouseDto>> GetWarehouse(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var warehouse = await _warehouseService.GetAccessibleWarehouseByIdAsync(id, currentUserId);
                if (warehouse == null)
                {
                    return NotFound();
                }
                return warehouse;
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid("شما دسترسی به این انبار را ندارید");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"خطا در دریافت اطلاعات انبار: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<WarehouseDto>> CreateWarehouse(CreateWarehouseDto createWarehouseDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var warehouse = await _warehouseService.CreateWarehouseAsync(createWarehouseDto, currentUserId);
                return CreatedAtAction(nameof(GetWarehouse), new { id = warehouse.Id }, warehouse);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateWarehouse(int id, UpdateWarehouseDto updateWarehouseDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var warehouse = await _warehouseService.UpdateWarehouseAsync(id, updateWarehouseDto, currentUserId);

                if (warehouse == null)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteWarehouse(int id)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _warehouseService.DeleteWarehouseAsync(id, currentUserId);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }
    }
}