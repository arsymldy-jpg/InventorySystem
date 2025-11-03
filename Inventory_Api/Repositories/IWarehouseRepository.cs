using Inventory_Api.Models.Entities;

namespace Inventory_Api.Repositories
{
    public interface IWarehouseRepository : IRepository<Warehouse>
    {
        Task<Warehouse?> GetByCodeAsync(string code);
        Task<IEnumerable<Warehouse>> GetActiveWarehousesAsync();
        Task<bool> HasActiveStocksAsync(int warehouseId);
    }
}