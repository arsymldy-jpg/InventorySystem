using Inventory_Api.Models.Entities;
using Inventory_Api.Repositories;

public interface IUserWarehouseAccessRepository : IRepository<UserWarehouseAccess>
{
    Task<UserWarehouseAccess?> GetByUserAndWarehouseAsync(int userId, int warehouseId); // همه دسترسی‌ها
    Task<UserWarehouseAccess?> GetActiveByUserAndWarehouseAsync(int userId, int warehouseId); // فقط دسترسی‌های فعال
    Task<IEnumerable<UserWarehouseAccess>> GetByUserIdAsync(int userId);
    Task<IEnumerable<UserWarehouseAccess>> GetByWarehouseIdAsync(int warehouseId);
    Task<bool> UserCanAccessWarehouseAsync(int userId, int warehouseId);
    Task<bool> UserCanModifyWarehouseAsync(int userId, int warehouseId);
    Task<bool> RemoveUserAccessAsync(int userId, int warehouseId);
}