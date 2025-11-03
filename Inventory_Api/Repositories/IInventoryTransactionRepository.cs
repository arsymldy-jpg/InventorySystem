using Inventory_Api.Models.Entities;

namespace Inventory_Api.Repositories
{
    public interface IInventoryTransactionRepository : IRepository<InventoryTransaction>
    {
        Task<IEnumerable<InventoryTransaction>> GetTransactionsByProductAsync(int productId);
        Task<IEnumerable<InventoryTransaction>> GetTransactionsByWarehouseAsync(int warehouseId);
        // اضافه کردن متدهای مورد نیاز به Interface
        void Update(InventoryTransaction entity);
        Task SaveChangesAsync();
    }
}