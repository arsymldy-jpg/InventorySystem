using Inventory_Api.Models.Entities;
using Inventory_Api.Models.DTOs;

namespace Inventory_Api.Repositories
{
    public interface IStockRepository : IRepository<Stock>
    {
        Task<Stock?> GetByProductAndWarehouseAsync(int productId, int warehouseId);
        Task<IEnumerable<Stock>> GetByProductIdAsync(int productId);
        Task<IEnumerable<Stock>> GetStocksByWarehouseAsync(int warehouseId);
        Task<IEnumerable<Product>> GetLowStockProductsAsync();
        Task<IEnumerable<BrandStockSummaryDto>> GetBrandStockSummaryAsync();
        void Update(Stock entity);
        Task SaveChangesAsync();
    }
}