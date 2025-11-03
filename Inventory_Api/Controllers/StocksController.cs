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
    public class StocksController : ControllerBase
    {
        private readonly IStockService _stockService;

        public StocksController(IStockService stockService)
        {
            _stockService = stockService;
        }

        [HttpGet]
        [Authorize(Policy = "ViewOnly")]
        public async Task<ActionResult<IEnumerable<StockDto>>> GetStocks()
        {
            var stocks = await _stockService.GetAllStocksAsync();
            return Ok(stocks);
        }

        [HttpGet("product/{productId}")]
        [Authorize(Policy = "ViewOnly")]
        public async Task<ActionResult<IEnumerable<StockDto>>> GetStocksByProduct(int productId)
        {
            var stocks = await _stockService.GetStocksByProductAsync(productId);
            return Ok(stocks);
        }

        [HttpGet("warehouse/{warehouseId}")]
        public async Task<ActionResult<IEnumerable<StockDto>>> GetStocksByWarehouse(int warehouseId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var hasAccess = await _stockService.CanUserAccessWarehouseAsync(currentUserId, warehouseId);

                if (!hasAccess)
                {
                    return Forbid("You don't have access to this warehouse");
                }

                var stocks = await _stockService.GetStocksByWarehouseAsync(warehouseId);
                return Ok(stocks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"خطا در دریافت موجودی انبار: {ex.Message}");
            }
        }

        [HttpGet("product/{productId}/warehouse/{warehouseId}")]
        [Authorize(Policy = "ViewOnly")]
        public async Task<ActionResult<StockDto>> GetStockByProductAndWarehouse(int productId, int warehouseId)
        {
            var stock = await _stockService.GetStockByProductAndWarehouseAsync(productId, warehouseId);
            if (stock == null)
            {
                return NotFound();
            }
            return stock;
        }

        [HttpPost("adjust")]
        [Authorize(Policy = "StockManagement")]
        public async Task<ActionResult<StockDto>> AdjustStock(AdjustStockDto adjustStockDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // Check if user has modification access to this warehouse
                var hasModifyAccess = await _stockService.CanUserModifyWarehouseAsync(currentUserId, adjustStockDto.WarehouseId);
                if (!hasModifyAccess)
                {
                    return Forbid("You don't have modification access to this warehouse");
                }

                var stock = await _stockService.AdjustStockAsync(adjustStockDto, currentUserId);
                return Ok(stock);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("transfer")]
        [Authorize(Policy = "StockManagement")]
        public async Task<ActionResult> TransferStock(TransferStockDto transferStockDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // Check if user has modification access to both warehouses
                var hasSourceAccess = await _stockService.CanUserModifyWarehouseAsync(currentUserId, transferStockDto.SourceWarehouseId);
                var hasDestinationAccess = await _stockService.CanUserModifyWarehouseAsync(currentUserId, transferStockDto.DestinationWarehouseId);

                if (!hasSourceAccess || !hasDestinationAccess)
                {
                    return Forbid("You don't have modification access to one or both warehouses");
                }

                await _stockService.TransferStockAsync(transferStockDto, currentUserId);
                return Ok("Stock transferred successfully");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("reports/low-stock")]
        [Authorize(Policy = "ReportView")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetLowStockProducts()
        {
            var products = await _stockService.GetLowStockProductsAsync();
            return Ok(products);
        }

        [HttpGet("reports/brand-summary")]
        [Authorize(Policy = "ReportView")]
        public async Task<ActionResult<IEnumerable<BrandStockSummaryDto>>> GetBrandStockSummary()
        {
            var summary = await _stockService.GetBrandStockSummaryAsync();
            return Ok(summary);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }
    }
}