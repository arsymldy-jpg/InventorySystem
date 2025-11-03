using Microsoft.AspNetCore.Authorization; // برای ویژگی Authorize
using Microsoft.AspNetCore.Mvc;
using Inventory_Api.Models.DTOs;
using Inventory_Api.Services;
// دیگر نیازی به: using Inventory_Api.Helpers; نیست

namespace Inventory_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CostCentersController : ControllerBase
    {
        private readonly ICostCenterService _costCenterService;

        public CostCentersController(ICostCenterService costCenterService)
        {
            _costCenterService = costCenterService;
        }

        // GET: api/CostCenters
        [HttpGet]
        [Authorize(Policy = "ViewOnly")] // استفاده از سیاست تعریف شده
        public async Task<ActionResult<IEnumerable<CostCenterDto>>> GetCostCenters()
        {
            var costCenters = await _costCenterService.GetAllCostCentersAsync();
            return Ok(costCenters);
        }

        // GET: api/CostCenters/active
        [HttpGet("active")]
        [Authorize(Policy = "ViewOnly")] // استفاده از سیاست تعریف شده
        public async Task<ActionResult<IEnumerable<CostCenterDto>>> GetActiveCostCenters()
        {
            var costCenters = await _costCenterService.GetActiveCostCentersAsync();
            return Ok(costCenters);
        }

        // GET: api/CostCenters/5
        [HttpGet("{id}")]
        [Authorize(Policy = "ViewOnly")] // استفاده از سیاست تعریف شده
        public async Task<ActionResult<CostCenterDto>> GetCostCenter(int id)
        {
            var costCenter = await _costCenterService.GetCostCenterByIdAsync(id);
            if (costCenter == null)
            {
                return NotFound();
            }
            return Ok(costCenter);
        }

        // POST: api/CostCenters
        [HttpPost]
        [Authorize(Policy = "UserManagement")] // استفاده از سیاست تعریف شده (Admin, SeniorUser, SeniorWarehouseManager)
        public async Task<ActionResult<CostCenterDto>> CreateCostCenter(CreateCostCenterDto createDto)
        {
            var costCenter = await _costCenterService.CreateCostCenterAsync(createDto);
            return CreatedAtAction(nameof(GetCostCenter), new { id = costCenter.Id }, costCenter);
        }

        // PUT: api/CostCenters/5
        [HttpPut("{id}")]
        [Authorize(Policy = "UserManagement")] // استفاده از سیاست تعریف شده (Admin, SeniorUser, SeniorWarehouseManager)
        public async Task<IActionResult> UpdateCostCenter(int id, UpdateCostCenterDto updateDto)
        {
            if (id != updateDto.Id)
            {
                return BadRequest();
            }

            var updatedCostCenter = await _costCenterService.UpdateCostCenterAsync(updateDto);
            if (updatedCostCenter == null)
            {
                return NotFound();
            }
            return Ok(updatedCostCenter);
        }

        // DELETE: api/CostCenters/5 (غیرفعال کردن)
        [HttpDelete("{id}")]
        [Authorize(Policy = "UserManagement")] // استفاده از سیاست تعریف شده (Admin, SeniorUser, SeniorWarehouseManager)
        public async Task<IActionResult> DeleteCostCenter(int id)
        {
            var result = await _costCenterService.DeleteCostCenterAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}