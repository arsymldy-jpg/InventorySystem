using Inventory_Api.Models.DTOs;

namespace Inventory_Api.Services
{
    public interface IStockService
    {
        Task<StockDto?> GetStockByProductAndWarehouseAsync(int productId, int warehouseId);
        Task<IEnumerable<StockDto>> GetAllStocksAsync();
        Task<IEnumerable<StockDto>> GetStocksByProductAsync(int productId);
        Task<IEnumerable<StockDto>> GetStocksByWarehouseAsync(int warehouseId);
        Task<StockDto> AdjustStockAsync(AdjustStockDto adjustStockDto, int currentUserId);
        Task TransferStockAsync(TransferStockDto transferStockDto, int currentUserId);
        Task<IEnumerable<ProductDto>> GetLowStockProductsAsync();
        Task<IEnumerable<BrandStockSummaryDto>> GetBrandStockSummaryAsync();
        Task<bool> CanUserAccessWarehouseAsync(int userId, int warehouseId);
        Task<bool> CanUserModifyWarehouseAsync(int userId, int warehouseId);
        //Task<bool> CanUserAccessWarehouseAsync(int userId, int warehouseId);
    }
}