using Inventory_Api.Models.DTOs;

namespace Inventory_Api.Services
{
    public interface IReportService
    {
        Task<InventorySummaryDto> GetInventorySummaryAsync();
        Task<IEnumerable<StockMovementReportDto>> GetStockMovementReportAsync(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<WarehouseStockSummaryDto>> GetWarehouseStockSummaryAsync();
        Task<IEnumerable<ProductStockHistoryDto>> GetProductStockHistoryAsync(int productId, DateTime fromDate, DateTime toDate);
        Task<BrandComparisonReportDto> GetBrandComparisonReportAsync();
    }
}