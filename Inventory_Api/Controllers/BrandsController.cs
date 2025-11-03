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
    public class BrandsController : ControllerBase
    {
        private readonly IBrandService _brandService;

        public BrandsController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        [HttpGet]
        [Authorize(Policy = "ViewOnly")]
        public async Task<ActionResult<IEnumerable<BrandDto>>> GetBrands()
        {
            var brands = await _brandService.GetAllBrandsAsync();
            return Ok(brands);
        }

        [HttpGet("management")]
        [Authorize(Policy = "ProductManagement")]
        public async Task<ActionResult<IEnumerable<BrandDto>>> GetBrandsForManagement()
        {
            var brands = await _brandService.GetAllBrandsForManagementAsync();
            return Ok(brands);
        }


        [HttpGet("{id}")]
        [Authorize(Policy = "ViewOnly")]
        public async Task<ActionResult<BrandDto>> GetBrand(int id)
        {
            var brand = await _brandService.GetBrandByIdAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            return Ok(brand);
        }

        [HttpPost]
        [Authorize(Policy = "ProductManagement")]
        public async Task<ActionResult<BrandDto>> CreateBrand(CreateBrandDto createBrandDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var brand = await _brandService.CreateBrandAsync(createBrandDto, currentUserId);
                return CreatedAtAction(nameof(GetBrand), new { id = brand.Id }, brand);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "ProductManagement")]
        public async Task<IActionResult> UpdateBrand(int id, UpdateBrandDto updateBrandDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var brand = await _brandService.UpdateBrandAsync(id, updateBrandDto, currentUserId);

                if (brand == null)
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
        [Authorize(Policy = "ProductManagement")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _brandService.DeleteBrandAsync(id, currentUserId);

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