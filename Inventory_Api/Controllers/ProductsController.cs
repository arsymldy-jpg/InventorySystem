using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Inventory_Api.Services;
using Inventory_Api.Models.DTOs;
using Inventory_Api.Models.Enums;

namespace Inventory_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        [Authorize(Policy = "ViewOnly")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "ViewOnly")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return product;
        }

        [HttpGet("search/{searchTerm}")]
        [Authorize(Policy = "ViewOnly")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> SearchProducts(string searchTerm)
        {
            var products = await _productService.SearchProductsAsync(searchTerm);
            return Ok(products);
        }

        [HttpGet("brand/{brandId}")]
        [Authorize(Policy = "ViewOnly")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByBrand(int brandId)
        {
            var products = await _productService.GetProductsByBrandAsync(brandId);
            return Ok(products);
        }

        [HttpPost]
        [Authorize(Policy = "ProductManagement")]
        public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto createProductDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var product = await _productService.CreateProductAsync(createProductDto, currentUserId);
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "ProductManagement")]
        public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto updateProductDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var product = await _productService.UpdateProductAsync(id, updateProductDto, currentUserId);

                if (product == null)
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
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _productService.DeleteProductAsync(id, currentUserId);

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