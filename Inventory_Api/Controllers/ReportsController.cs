using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Inventory_Api.Services;
using Inventory_Api.Models.DTOs;

namespace Inventory_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ReportView")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("inventory-summary")]
        public async Task<ActionResult<InventorySummaryDto>> GetInventorySummary()
        {
            var summary = await _reportService.GetInventorySummaryAsync();
            return Ok(summary);
        }

        [HttpGet("stock-movement")]
        public async Task<ActionResult<IEnumerable<StockMovementReportDto>>> GetStockMovementReport(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            var report = await _reportService.GetStockMovementReportAsync(fromDate, toDate);
            return Ok(report);
        }

        [HttpGet("warehouse-summary")]
        public async Task<ActionResult<IEnumerable<WarehouseStockSummaryDto>>> GetWarehouseStockSummary()
        {
            var summary = await _reportService.GetWarehouseStockSummaryAsync();
            return Ok(summary);
        }

        [HttpGet("product-stock-history/{productId}")]
        public async Task<ActionResult<IEnumerable<ProductStockHistoryDto>>> GetProductStockHistory(
            int productId,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            var history = await _reportService.GetProductStockHistoryAsync(productId, fromDate, toDate);
            return Ok(history);
        }

        [HttpGet("brand-comparison")]
        public async Task<ActionResult<BrandComparisonReportDto>> GetBrandComparisonReport()
        {
            var report = await _reportService.GetBrandComparisonReportAsync();
            return Ok(report);
        }
    }
}